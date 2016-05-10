using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

using Suplex.Data;
using Suplex.General;
using splxApi = Suplex.Forms.ObjectModel.Api;


namespace Suplex.Security.Standard
{
	public interface ISecurityPrincipal
	{
		string Id { get; set; }
		string Name { get; set; }
		string Description { get; set; }
		bool IsLocal { get; set; }
		bool IsBuiltIn { get; }
		bool IsEnabled { get; set; }
		bool IsValid { get; }
		bool IsAnonymous { get; }
	}

	public abstract class SecurityPrincipal : ISecurityPrincipal
	{
		private string _id = null;
		private string _name = null;
		private string _desc = null;
		private bool _local = false;
		protected bool _builtIn = false;
		private bool _enabled = false;
		protected bool _valid = false;
		protected bool _anonymous = false;

		private DataAccessor _da	= null;


		public SecurityPrincipal(){}

		public SecurityPrincipal( DataAccessor platformDataAccessor )
		{
			_da = platformDataAccessor;
		}

		public SecurityPrincipal( string name, string description )
		{
			_name = name;
			_desc = description;
			_valid = true;
		}

		public SecurityPrincipal( string id, string name, string description )
		{
			_id = id;
			_name = name;
			_desc = description;
			_valid = true;
		}

		public SecurityPrincipal( string name, bool local, DataAccessor platformDataAccessor )
		{
			_name = name;
			_local = local;
			_da = platformDataAccessor;

			_valid = ( (SqlResult)this.ResolveByName() ).Success;
		}

		public virtual string Id
		{
			get { return _id; }
			set { _id = value; }
		}

		public Guid IdToGuid() { return Guid.Parse( _id ); }

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual string Description
		{
			get { return _desc; }
			set { _desc = value; }
		}

		public virtual bool IsLocal
		{
			get { return _local; }
			set { _local = value; }
		}

		public bool IsBuiltIn
		{
			get { return _builtIn; }
		}

		public virtual bool IsEnabled
		{
			get { return _enabled; }
			set { _enabled = value; }
		}

		public bool IsValid
		{
			get { return _valid; }
		}

		public bool IsAnonymous
		{
			get { return _anonymous; }
		}

		public DataAccessor DataAccessor
		{
			get { return _da; }
			set { _da = value; }
		}

		public byte[] RlsMask { get; set; }

		public abstract SqlResult ResolveById();

		public abstract SqlResult ResolveByName();
	}


	public class User : SecurityPrincipal
	{
		private Credentials _credentials = null;
		private User _impersonationContext = null;
		
		public static readonly User Unknown;
		
		static User() 
		{
			Unknown = new User( "Unknown", "Unknown" );
		}


		public User() : base(){}

		public User( string name, string description ) : base( name, description ){}

		public User( string id, string name, string description ) : base( id, name, description ){}

		public User( DataAccessor platformDataAccessor ) : base( platformDataAccessor ){}

		public User( string name, bool local, DataAccessor platformDataAccessor ) : base( name, local, platformDataAccessor ){}

		public User( Credentials credentials, DataAccessor platformDataAccessor )
		{
			this._credentials = credentials;
			this.Name = credentials.UserName;
			this.IsLocal = credentials.Local;
			this.DataAccessor = platformDataAccessor;

			AuthenticationResult r = this.Authenticate( true );	//sets base._valid
		}

		public bool CreateUnresolvedName { get; set; }

		public override SqlResult ResolveById()
		{
			return this.ResolveById( false, null );
		}
		public SqlResult ResolveById(bool resolveRlsMask, string externalGroupsListCsv)
		{
			SqlResult result = new SqlResult();

			if( !string.IsNullOrEmpty( this.Id ) )
			{
				SortedList inparms = new SortedList();
				inparms.Add( "@SPLX_USER_ID", this.Id );
				if( resolveRlsMask )
				{
					inparms.Add( "@RESOLVE_RLS", true );
					inparms.Add( "@EXTERNAL_GROUP_LIST", externalGroupsListCsv );
				}

				try
				{
					DataSet ds = DataAccessor.GetDataSet( "splx.splx_dal_sel_userbyid", inparms );

					if( ds.Tables[0].Rows.Count == 0 )
					{
						base._anonymous = true;
						this.Id = Guid.Empty.ToString();
						result.SetResult( false, "Could not resolve user; set as Anonymous" );
					}
					else if( ds.Tables[0].Rows.Count == 1 )
					{
						this.Id = ds.Tables[0].Rows[0]["SPLX_USER_ID"].ToString();
						this.Name = ds.Tables[0].Rows[0]["USER_NAME"].ToString();
						this.Description = ds.Tables[0].Rows[0]["USER_DESC"].ToString();
						base._builtIn = (bool)ds.Tables[0].Rows[0]["USER_BUILT_IN"];
						this.IsEnabled = (bool)ds.Tables[0].Rows[0]["USER_ENABLED"];
						this.IsLocal = (bool)ds.Tables[0].Rows[0]["USER_LOCAL"];
						if( ds.Tables[0].Rows[0]["GROUP_MEMBERSHIP_MASK"] != Convert.DBNull )
						{
							this.RlsMask = (byte[])ds.Tables[0].Rows[0]["GROUP_MEMBERSHIP_MASK"];
						}
					}
					else
					{
						result.SetResult( false, "Duplicate ID match: could not resolve user by ID." );
					}
				}
				catch( SqlException ex )
				{
					result.SetResult( false, ex.Message, ex );
				}
				catch( Exception ex )
				{
					result.SetResult( false, ex.Message );
				}
			}
			else
			{
				result.SetResult( false, "Cannot resolve User by Id if Id is empty." );
			}

			return result;
		}

		public override SqlResult ResolveByName()
		{
			return this.ResolveByName( false, null );
		}
		public SqlResult ResolveByName(bool resolveRlsMask, string externalGroupsListCsv)
		{
			SqlResult result = new SqlResult();

			if( !string.IsNullOrEmpty( this.Name ) )
			{
				SortedList inparms = new SortedList();
				inparms.Add( "@USER_NAME", this.Name );
				if( resolveRlsMask )
				{
					inparms.Add( "@RESOLVE_RLS", true );
					inparms.Add( "@EXTERNAL_GROUP_LIST", externalGroupsListCsv );
				}
				//inparms.Add( "@USER_LOCAL", this.IsLocal );

				try
				{
					DataSet ds = DataAccessor.GetDataSet( "splx.splx_dal_sel_userbyname", inparms );

					if( ds.Tables[0].Rows.Count == 0 )
					{
						if( this.CreateUnresolvedName )
						{
							splxApi.SuplexDataAccessLayer splxDal =
								new splxApi.SuplexDataAccessLayer( this.DataAccessor.ConnectionString );
							splxApi.User newUser = new splxApi.User()
							{
								Id = Guid.NewGuid().ToString(),
								Name = this.Name,
								Description = this.Name,
								IsEnabled = true,
								IsLocal = false,
								IsBuiltIn = false
							};
							newUser = splxDal.UpsertUser( newUser, null, null );

							this.Id = newUser.Id;
							this.Description = newUser.Description;
							base._builtIn = newUser.IsBuiltIn;
							this.IsEnabled = newUser.IsEnabled;
							this.IsLocal = newUser.IsLocal;

							splxDal = null;
						}
						else
						{
							base._anonymous = true;
							this.Id = Guid.Empty.ToString();
							result.SetResult( false, "Could not resolve user; set as Anonymous" );
						}
					}
					else if( ds.Tables[0].Rows.Count == 1 )
					{
						this.Id = ds.Tables[0].Rows[0]["SPLX_USER_ID"].ToString();
						//this.Name = ds.Tables[0].Rows[0]["USER_NAME"].ToString();
						this.Description = ds.Tables[0].Rows[0]["USER_DESC"].ToString();
						base._builtIn = (bool)ds.Tables[0].Rows[0]["USER_BUILT_IN"];
						this.IsEnabled = (bool)ds.Tables[0].Rows[0]["USER_ENABLED"];
						this.IsLocal = (bool)ds.Tables[0].Rows[0]["USER_LOCAL"];
						if( ds.Tables[0].Rows[0]["GROUP_MEMBERSHIP_MASK"] != Convert.DBNull )
						{
							this.RlsMask = (byte[])ds.Tables[0].Rows[0]["GROUP_MEMBERSHIP_MASK"];
						}
					}
					else
					{
						result.SetResult( false, "Duplicate Name match: could not resolve user by Name." );
					}
				}
				catch( SqlException ex )
				{
					result.SetResult( false, ex.Message, ex );
				}
				catch( Exception ex )
				{
					result.SetResult( false, ex.Message );
				}
			}
			else
			{
				result.SetResult( false, "Cannot resolve User by Name if Name is empty." );
			}

			return result;
		}

		public AuthenticationResult Authenticate(bool resolveOnSuccess)
		{
			return this.Authenticate( _credentials.Password, resolveOnSuccess );
		}

		public AuthenticationResult Authenticate(string password, bool resolveOnSuccess)
		{
			AuthenticationResult result = null;
			base._valid = false;

			if( this.Name != null && this.Name.Length > 0 )
			{
				SortedList inparms = new SortedList();
				SortedList outparms = new SortedList();

				inparms.Add( "@USER_NAME", this.Name );
				inparms.Add( "@USER_PASSWORD", string.IsNullOrEmpty( password ) ? Convert.DBNull : TextFormatter.GetBytes( password ) );
				inparms.Add( "@USER_LOCAL", this.IsLocal );

				try
				{
					DataSet ds = DataAccessor.GetDataSet( "splx.splx_dal_sel_authuser", inparms );
					DataRow r = ds.Tables[0].Rows[0];
					base._valid = (bool)r["USER_EXISTS"];

					if( base._valid )
					{
						this._credentials.Password = password;

						if( resolveOnSuccess )
						{
							this.ResolveByName();
						}
					}

					result = new AuthenticationResult(
						(bool)r["USER_EXISTS"],
						(DateTime)r["LOCKOUT_END_TIME"],
						(bool)r["USER_LOCKED"],
						r["ERROR_MESSAGE"].ToString(),
						(bool)r["ALLOW_LOGIN_RETRY"] );
				}
				catch( SqlException ex )
				{
					result = new AuthenticationResult( false, DateTime.Now, false, ex.Message, true );
				}
			}
			else
			{
				result = new AuthenticationResult( false, DateTime.Now, false,
					"Cannot authenticate user if Name is empty.", true );
			}

			return result;
		}

		public Credentials Credentials
		{
			get { return _credentials; }
			set
			{
				_credentials = value;

				if( value != null )
				{
					this.Name = value.UserName;
					this.IsLocal = value.Local;
				}
			}
		}

		public User ImpersonationContext
		{
			get { return _impersonationContext; }
			set { _impersonationContext = value; }
		}

		public string ImpersonationName
		{
			get
			{
				return _impersonationContext == null ? base.Name :
					string.Format( "{0} as [{1}]", base.Name, _impersonationContext.Name );
			}
		}
	}


	public class AuthenticationResult
	{
		private bool _usrSuccess = false;
		private DateTime _usrLockoutEndTime;
		private bool _usrIsLocked = true;
		private string _usrError = string.Empty;
		private bool _usrCanRetry = false;

		private AuthenticationResult(){}


		public AuthenticationResult(bool success, DateTime lockoutEndTime, bool isLocked, string errorMsg, bool canRetry)
		{
			_usrSuccess = success;
			_usrLockoutEndTime = lockoutEndTime;
			_usrIsLocked = isLocked;
			_usrError = errorMsg;
			_usrCanRetry = canRetry;

			if( isLocked )
			{
				_usrError = String.Format("The account is locked until {0}.", lockoutEndTime.ToString() );
			}
		}

		public bool Success
		{
			get { return _usrSuccess; }
		}

		public DateTime LockoutEndTime
		{
			get { return _usrLockoutEndTime; }
		}

		public bool Locked
		{
			get { return _usrIsLocked; }
		}

		public string ErrorMsg
		{
			get { return _usrError; }
		}

		public bool CanRetryLogin
		{
			get { return _usrCanRetry; }
		}
	}


	public class Group : SecurityPrincipal
	{
		public Group() : base(){}

		public Group( string name, string description ) : base( name, description ){}

		public Group( string id, string name, string description ) : base( id, name, description ){}

		public Group( DataAccessor platformDataAccessor ) : base( platformDataAccessor ){}

		public Group( string name, bool local, DataAccessor platformDataAccessor ) : base( name, local, platformDataAccessor ){}

		public override SqlResult ResolveById()
		{
			SqlResult result = new SqlResult();

			if( this.Name != null && this.Name.Length > 0 )
			{
				SortedList inparms = new SortedList();
				inparms.Add( "@SPLX_GROUP_ID", this.Id );

				try
				{
					DataSet ds = DataAccessor.GetDataSet( "splx.splx_dal_sel_groupbyid", inparms );

					this.Id = ds.Tables[0].Rows[0]["SPLX_GROUP_ID"].ToString();
					this.Name = ds.Tables[0].Rows[0]["GROUP_NAME"].ToString();
					this.Description = ds.Tables[0].Rows[0]["GROUP_DESC"].ToString();
					this._builtIn = (bool)ds.Tables[0].Rows[0]["GROUP_BUILTIN"];
					this.IsEnabled = (bool)ds.Tables[0].Rows[0]["GROUP_ENABLED"];
					this.IsLocal = (bool)ds.Tables[0].Rows[0]["GROUP_LOCAL"];
				}
				catch( SqlException ex )
				{
					result.SetResult( false, ex.Message, ex );
				}
				catch( Exception ex )
				{
					result.SetResult( false, ex.Message );
				}
			}
			else
			{
				result.SetResult( false, "Cannot resolve Group by Id if Id is empty." );
			}

			return result;
		}

		public override SqlResult ResolveByName()
		{
			SqlResult result = new SqlResult();

			if( this.Name != null && this.Name.Length > 0 )
			{
				SortedList inparms = new SortedList();
				inparms.Add( "@GROUP_NAME", this.Name );

				try
				{
					DataSet ds = DataAccessor.GetDataSet( "splx.splx_dal_sel_groupbyname", inparms );

					this.Id = ds.Tables[0].Rows[0]["SPLX_GROUP_ID"].ToString();
					this.Name = ds.Tables[0].Rows[0]["GROUP_NAME"].ToString();
					this.Description = ds.Tables[0].Rows[0]["GROUP_DESC"].ToString();
					this._builtIn = (bool)ds.Tables[0].Rows[0]["GROUP_BUILTIN"];
					this.IsEnabled = (bool)ds.Tables[0].Rows[0]["GROUP_ENABLED"];
					this.IsLocal = (bool)ds.Tables[0].Rows[0]["GROUP_LOCAL"];
				}
				catch( SqlException ex )
				{
					result.SetResult( false, ex.Message, ex );
				}
				catch( Exception ex )
				{
					result.SetResult( false, ex.Message );
				}
			}
			else
			{
				result.SetResult( false, "Cannot resolve Group by Name if Name is empty." );
			}

			return result;
		}

	}


	public class Credentials : ICloneable
	{
		private string	_username	= null;
		private string	_password	= null;
		private bool	_local		= false;

		public static readonly Credentials WindowsLogon;

		static Credentials()
		{
			WindowsLogon = new Credentials(
				System.Security.Principal.WindowsIdentity.GetCurrent().Name, null, false );
		}

		public Credentials() { }

		public Credentials(string username, string password)
		{
			_username = username; //CleanName( username );
			_password = password;
			_local = true;
		}
		public Credentials(string username, string password, bool local)
		{
			_username = username; //CleanName( username );
			_password = password;
			_local = local;
		}

		public string UserName
		{
			get { return _username; }
		}

		public string Password
		{
			get { return _password; }
			set { _password = value; }
		}

		public bool Local
		{
			get { return _local; }
		}

		public string GetCleanName(string name)
		{
			string clean = name;
			int i = clean.IndexOf( "\\" );
			if( i > 0 )
			{
				clean = clean.Substring( i + 1 );
			}
			return clean;
		}

		#region ICloneable Members
		public object Clone()
		{
			return new Credentials( _username, _password, _local );
		}
		#endregion

		public override string ToString()
		{
			return string.Format( "UserName: {0}, Local: {1}", _username, _local );
		}
	}
}