using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using sf = Suplex.Forms;
using sg = Suplex.General;
using Suplex.General;
using ss = Suplex.Security;

namespace Suplex.Forms.ObjectModel.Api
{
	public partial class SuplexDataAccessLayer
	{
		public User GetUserById(string id)
		{
			DataSet ds = _da.GetDataSet( "splx.splx_dal_sel_userbyid",
				new sSortedList( "@SPLX_USER_ID", id ) );

			if( ds.Tables[0].Rows.Count > 0 )
			{
				UserFactory factory = new UserFactory();
				return factory.CreateObject( ds.Tables[0].Rows[0] );
			}
			else
			{
				throw new RowNotInTableException( string.Format( "Unable to fetch User '{0}' from the data store.", id ) );
			}
		}

		public MembershipList<Group> GetUserGroupMemberOf(string userId)
		{
			MembershipList<Group> list = new MembershipList<Group>();
			GroupFactory factory = new GroupFactory();

			DataSet groupMembs =
				_da.GetDataSet( "splx.splx_api_sel_groupmembbyuser", new sSortedList( "@SPLX_USER_ID", userId ) );
			_da.NameTablesFromCompositeSelect( ref groupMembs );

			list.MemberList.LoadSuplexObjectTable( groupMembs.Tables["GroupMembership"], factory, null, null);
			list.NonMemberList.LoadSuplexObjectTable( groupMembs.Tables["GroupNonMembership"], factory, null, null );

			return list;
		}

        public List<User> GetUserList()
        {
            List<User> users = new List<User>();
            DataSet ds = _da.GetDataSet( "splx.splx_api_sel_users", null );
            UserFactory userFactory = new UserFactory();
            users.LoadSuplexObjectTable( ds.Tables[0], userFactory, null, null );
            return users;
        }

        #region upsert
        public User UpsertUser(User user, List<Group> addedGroupMembership, List<Group> removedGroupMembership)
		{
			SortedList inparms = this.GetUserInputParms( user );
			SortedList outparms = this.GetUserOutputParms( user );
			SqlParameter id = (SqlParameter)outparms["@SPLX_USER_ID"];
			SqlParameter name = (SqlParameter)outparms["@USER_NAME"];

			_da.OpenConnection();

			_da.ExecuteSP( "splx.splx_api_upsert_user", inparms, ref outparms, false );
			user.Id = id.Value.ToString();
			user.Name = name.Value.ToString();

			this.IterateUserAddedGroupMembership( user.Id, addedGroupMembership );
			this.IterateUserRemovedGroupMembership( user.Id, removedGroupMembership );

			_da.CloseConnection();

			user.IsDirty = false;

			return user;
		}

		public void UpsertUserForImport(User user, List<Group> addedGroupMembership, ref SqlTransaction tr)
		{
			SortedList inparms = this.GetUserInputParms( user );
			SortedList outparms = this.GetUserOutputParms( user );
			SqlParameter id = (SqlParameter)outparms["@SPLX_USER_ID"];
			SqlParameter name = (SqlParameter)outparms["@USER_NAME"];

			_da.ExecuteSP( "splx.splx_api_upsert_user", inparms, ref outparms, false, tr );
			user.Id = id.Value.ToString();
			user.Name = name.Value.ToString();

			if( addedGroupMembership != null )
			{
				foreach( Group group in addedGroupMembership )
				{
					try
					{
						SortedList parms = new sSortedList( "@SPLX_USER_ID", user.Id );
						parms.Add( "@SPLX_GROUP_ID", group.Id );
						parms.Add( "@CURR_USER_ID", Guid.Empty );
						_da.ExecuteSP( "splx.splx_api_ins_groupmemb", parms, false, tr );
					}
					catch( SqlException ex )
					{
						if( !(ex.Number == 2601) ) //2601 is UniqueIndex violation (throw away the dups)
						{
							throw ex;
						}
					}
				}
			}
		}

		private void IterateUserAddedGroupMembership(string userId, List<Group> addedGroupMembership)
		{
			if( addedGroupMembership != null )
			{
				foreach( Group group in addedGroupMembership )
				{
					try
					{
						SortedList parms = new sSortedList( "@SPLX_USER_ID", userId );
						parms.Add( "@SPLX_GROUP_ID", group.Id );
						parms.Add( "@CURR_USER_ID", Guid.Empty );
						_da.ExecuteSP( "splx.splx_api_ins_groupmemb", parms, false );
					}
					catch( SqlException ex )
					{
						if( !(ex.Number == 2601) ) //2601 is UniqueIndex violation (throw away the dups)
						{
							throw ex;
						}
					}
				}
			}
		}

		private void IterateUserRemovedGroupMembership(string userId, List<Group> removedGroupMembership)
		{
			if( removedGroupMembership != null )
			{
				foreach( Group group in removedGroupMembership )
				{
					try
					{
						SortedList parms = new sSortedList( "@SPLX_USER_ID", userId );
						parms.Add( "@SPLX_GROUP_ID", group.Id );
						parms.Add( "@CURR_USER_ID", Guid.Empty );
						_da.ExecuteSP( "splx.splx_api_del_groupmemb", parms, false );
					}
					catch( SqlException ex )
					{
						if( !(ex.Number == 2601) ) //2601 is UniqueIndex violation (throw away the dups)
						{
							throw ex;
						}
					}
				}
			}
		}

		private SortedList GetUserInputParms(User user)
		{
			sSortedList s = new sSortedList();	//"@USER_NAME", user.Name
			s.Add( "@USER_DESC", user.Description );
			s.Add( "@USER_LOCAL", user.IsLocal );
			s.Add( "@USER_ENABLED", user.IsEnabled );

			return s;
		}

		private SortedList GetUserOutputParms(User user)
		{
			SqlParameter id = new SqlParameter( "@SPLX_USER_ID", SqlDbType.UniqueIdentifier );
			id.Value = new Guid( user.Id );
			id.Direction = ParameterDirection.InputOutput;
			SortedList s = new sSortedList( "@SPLX_USER_ID", id );

			SqlParameter name = new SqlParameter( "@USER_NAME", SqlDbType.VarChar );
			name.Value = user.Name;
			name.Direction = ParameterDirection.InputOutput;
			s.Add( "@USER_NAME", name );

			return s;
		}
		#endregion

		#region delete
		public void DeleteUserById(string id)
		{
			_da.ExecuteSP( "splx.splx_api_del_user", new sSortedList( "@SPLX_USER_ID", id ) );
		}
		#endregion
	}

	public class UserFactory : ISuplexObjectFactory<User>
	{
		public ISuplexObject CreateSuplexObjectBase(DataRow r)
		{
			return this.CreateObject( r );
		}

		public User CreateObject(DataRow r)
		{
			User u = new User();
			this.CreateObject( r, ref u );
			return u;
		}

		public void CreateObject(DataRow r, ref User u)
		{
			u.Id = r["SPLX_USER_ID"].ToString();
			u.Name = (string)r["USER_NAME"];
			u.Description = (string)r["USER_DESC"];
			u.IsLocal = (bool)r["USER_LOCAL"];
			u.IsEnabled = (bool)r["USER_ENABLED"];
			u.LastLogon = r.IsDBNullOrValue<DateTime>( "USER_LAST_LOGIN", DateTime.MinValue );
			u.IsDirty = false;
		}
	}
}