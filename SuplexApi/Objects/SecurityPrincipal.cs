using System;
using System.Collections.Generic;
using Suplex.Forms.ObjectModel.Api;

namespace Suplex.Api
{
	public partial class SuplexApi
	{
		public User GetUserById(string id)
		{
			return _splxDal.GetUserById( id );
		}
		public MembershipList<Group> GetUserGroupMemberOf(string userId)
		{
			return _splxDal.GetUserGroupMemberOf( userId );
		}
		public User UpsertUser(UserData userData)
		{
			List<Group> addedGroupMembership =
				this.SplitSecurityPrincipalList<Group>( userData.AddedGroupMembership );
			List<Group> removedGroupMembership =
				this.SplitSecurityPrincipalList<Group>( userData.RemovedGroupMembership );

			return _splxDal.UpsertUser( userData.User, addedGroupMembership, removedGroupMembership );
		}
		public void DeleteUserById(string id)
		{
			_splxDal.DeleteUserById( id );
		}



		public Group GetGroupById(string id)
		{
			return _splxDal.GetGroupById( id );
		}
		public List<Group> GetGroupList()
		{
			return _splxDal.GetGroupList();
		}
		public MembershipList<SecurityPrincipalBase> GetGroupMembers(string groupId)
		{
			return _splxDal.GetGroupMembers( groupId );
		}
		public List<Group> GetGroupHierarchy(string groupId)
		{
			return _splxDal.GetGroupHierarchy( groupId );
		}
		public Group UpsertGroup(GroupData groupData)
		{
			List<SecurityPrincipalBase> addedGroupMembership =
				this.SplitSecurityPrincipalList<SecurityPrincipalBase>( groupData.AddedGroupMembership );
			List<SecurityPrincipalBase> removedGroupMembership =
				this.SplitSecurityPrincipalList<SecurityPrincipalBase>( groupData.RemovedGroupMembership );

			return _splxDal.UpsertGroup( groupData.Group, addedGroupMembership, removedGroupMembership );
		}
		public void DeleteGroupById(string id)
		{
			_splxDal.DeleteGroupById( id );
		}



		List<T> SplitSecurityPrincipalList<T>(string csv) where T : SecurityPrincipalBase
		{
			List<T> list = new List<T>();

			if( !string.IsNullOrWhiteSpace( csv ) )
			{
				string[] values = csv.Split( new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries );
				foreach( string value in values )
				{
					string[] s = value.Split( new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries );
					bool isUser = bool.Parse( s[1] );
					T sp = isUser ? new User() as T : new Group() as T;
					sp.Id = s[0];
					list.Add( sp );
				}
			}

			return list;
		}
	}
}