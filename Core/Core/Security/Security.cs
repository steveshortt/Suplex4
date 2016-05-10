using System;
using System.Diagnostics;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;

using Suplex.Data;
using Suplex.Security.Standard;


namespace Suplex.Security
{

	#region Enums

	[Flags]
	public enum UIRight
	{
		FullControl = 7,
		Operate = 4,
		Enabled = 2,
		Visible = 1
	}

	[Flags]
	public enum RecordRight
	{
		FullControl = 31,
		Delete = 16,
		Update = 8,
		Insert = 4,
		Select = 2,
		List = 1
	}

	[Flags]
	public enum FileSystemRight
	{
		FullControl = 511,
		Execute = 256,
		Delete = 128,
		Write = 64,
		Create = 32,
		Read = 16,
		List = 8,
		ChangePermissions = 4,
		ReadPermissions = 2,
		TakeOwnership = 1
	}

	[Flags]
	[RightsAccessor( (int)SynchronizationRight.OneWay )]
	public enum SynchronizationRight
	{
		TwoWay = 7,
		Upload = 5,
		Download = 3,
		OneWay = 1
	}


	public enum AceType
	{
		None = 0,
		Native = 1,
		UI = 2,
		Record = 3,
		FileSystem = 4,
		Synchronization = 5
	}


	public class AceTypeRights
	{
		private AceType _aceType = AceType.None;
		private AceType _lastAceType = AceType.None;
		private object[] _rights = null;
		private Type _right = null;
		private static Type _rightType = null;


		#region ctor, instance members
		public AceTypeRights(){}

		public AceTypeRights(string aceType)
		{
			_aceType = AceTypeRights.GetAceType( aceType );
			AceTypeRights.GetRights( _aceType, ref _right, ref _rights );
		}

		public AceTypeRights(AceType aceType)
		{
			_aceType = aceType;
			AceTypeRights.GetRights( aceType, ref _right, ref _rights );
		}

		public void Eval(string aceType)
		{
			_aceType = AceTypeRights.GetAceType( aceType );

			if( _aceType != _lastAceType )
			{
				_right = null;
				_rights = null;
				_lastAceType = _aceType;
			}

			AceTypeRights.GetRights( _aceType, ref _right, ref _rights );
		}

		public void Eval(AceType aceType)
		{
			_aceType = aceType;

			if( _aceType != _lastAceType )
			{
				_right = null;
				_rights = null;
				_lastAceType = _aceType;
			}

			AceTypeRights.GetRights( aceType, ref _right, ref _rights );
		}

		public AceType AceType
		{
			get { return _aceType; }
		}

		public Type Right
		{
			get { return _right; }
		}

		public object[] Rights
		{
			get { return _rights; }
		}
		#endregion


		#region static members
		public static AceType GetAceType(string aceType)
		{
			aceType = aceType.ToLower();

			if( aceType.EndsWith( "ace" ) )
			{
				aceType = aceType.Substring( 0, aceType.Length - 3 );
			}
			if( aceType.EndsWith( "audit" ) )
			{
				aceType = aceType.Substring( 0, aceType.Length - 5 );
			}

			return (AceType)Enum.Parse( typeof( AceType ), aceType, true ); ;
		}

		public static AceType GetAceType(Type t)
		{
			string aceType = t.ToString().Replace( "Suplex.Security.", "" ).Replace( "Right", "" );
			return Suplex.General.MiscUtils.ParseEnum<AceType>( aceType );
		}

		public static void GetRights(string aceType, ref Type right, ref object[] rights)
		{
			AceType at = GetAceType( aceType );
			GetRights( at, ref right, ref rights );
		}
		public static void GetRights(AceType aceType, ref Type right, ref object[] rights)
		{
			if( aceType == AceType.Native || aceType == AceType.None )
			{
				return;
			}



			Array a = null;

			// uses reflection:
			if( right == null )
			{
				right = Assembly.GetExecutingAssembly().GetType(
					string.Format( "Suplex.Security.{0}Right", aceType.ToString() ) );
			}
			_rightType = right;

			if( right != null )
			{
				a = Enum.GetValues( right );
				if( a != null )
				{
					rights = new object[a.Length];
					a.CopyTo( rights, 0 );
				}
			}


			/* non-reflection way...
			 * 
			switch( aceType )
			{
				case AceType.UI:
				{
					_rightType = typeof(UIRight);
					a = Enum.GetValues( _rightType );
					break;
				}
				case AceType.Record:
				{
					_rightType = typeof(RecordRight);
					a = Enum.GetValues( _rightType );
					break;
				}
				case AceType.Synchronization:
				{
					_rightType = typeof( SynchronizationRight );
					a = Enum.GetValues( _rightType );
					break;
				}
				default:
				{
					break;
				}
			}

			if( a != null )
			{
				rights = new object[a.Length];
				a.CopyTo( rights, 0 );
			}
			 * 
			 */
		}

		public static object[] GetRights(string aceType)
		{
			return AceTypeRights.GetRights( AceTypeRights.GetAceType( aceType ) );
		}

		public static object[] GetRights(AceType aceType)
		{
			object[] rights = null;
			Type right = null;

			AceTypeRights.GetRights( aceType, ref right, ref rights );

			return rights;
		}

		public static Type RightType
		{
			get { return _rightType; }
		}
		#endregion
	}


	public enum DefaultSecurityState
	{
		Locked,
		Unlocked
	}


	[Flags]
	public enum AuditType
	{
		SuccessAudit	= 1,
		FailureAudit	= 2,
		Information		= 4,
		Warning			= 8,
		Error			= 16,
		ControlDetail	= 32
	}


	public enum AuditCategory
	{
		Access,
		Action
	}

	#endregion

	public class RightsAccessorAttribute : Attribute
	{
		int _rightsMask = 0;

		public RightsAccessorAttribute() { }

		public RightsAccessorAttribute(int rightsMask)
		{
			_rightsMask = rightsMask;
		}

		public int RightsMask { get { return _rightsMask; } }
		public bool HasMask { get { return _rightsMask > 0; } }
	}

	#region AuditEvent
	public delegate void AuditEventHandler(object sender, AuditEventArgs e);

	public class AuditEventArgs : System.EventArgs
	{

		private User		_user			= User.Unknown;
		private string		_computerName	= SystemInformation.ComputerName;
		private DateTime	_date			= DateTime.Now;
		private AuditType	_auditType;
		private string		_source			= "";
		private string		_category		= "";
		private string		_description	= "";
 
		public AuditEventArgs(){}

		public AuditEventArgs( User user, AuditType auditType, string source, AuditCategory category, NameValueCollection description )
		{
			_user			= user;
			_auditType		= auditType;
			_source			= source;
			_category		= category.ToString();
			
			StringBuilder desc = new StringBuilder( description.Count );
			IDictionaryEnumerator d = ((IDictionary)description).GetEnumerator();
			while( d.MoveNext() )
			{
				desc.AppendFormat( "{0}:\t{1}\r\n", d.Key, d.Value );
			}
			_description	= desc.ToString();
		}


		public AuditEventArgs( User user, AuditType auditType, string source, AuditCategory category, StringCollection description )
		{
			_user			= user;
			_auditType		= auditType;
			_source			= source;
			_category		= category.ToString();
			
			StringBuilder desc = new StringBuilder( description.Count );
			IEnumerator d = ((IEnumerable)description).GetEnumerator();
			while( d.MoveNext() )
			{
				desc.AppendFormat( "{0}\r\n", d.Current.ToString() );
			}
			_description	= desc.ToString();
		}


		public AuditEventArgs( User user, AuditType auditType, string source, AuditCategory category, string[] description )
		{
			_user			= user;
			_auditType		= auditType;
			_source			= source;
			_category		= category.ToString();
			
			StringBuilder desc = new StringBuilder( description.Length );
			for( int n=0; n<description.Length; n++ )
			{
				desc.AppendFormat( "{0}\r\n", description[n] );
			}
			_description	= desc.ToString();
		}


		public AuditEventArgs( User user, AuditType auditType, string source, AuditCategory category, string description )
		{
			_user			= user;
			_auditType		= auditType;
			_source			= source;
			_category		= category.ToString();
			_description	= description + "\r\n";
		}


		public User User
		{
			get
			{
				return _user;
			}
		}


		public string ComputerName
		{
			get
			{
				return _computerName;
			}
		}


		public DateTime Date
		{
			get
			{
				return _date;
			}
		}


		public AuditType AuditType
		{
			get
			{
				return _auditType;
			}
		}


		public string Source
		{
			get
			{
				return _source;
			}
		}


		public string Category
		{
			get
			{
				return _category;
			}
		}


		public string Description
		{
			get
			{
				return _description;
			}
		}

	}
	#endregion

}