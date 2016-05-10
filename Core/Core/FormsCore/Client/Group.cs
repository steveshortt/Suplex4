using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace Suplex.Forms.ObjectModel.Api
{
	public partial class SuplexApiClient
	{
		#region select
		public event EventHandler<AsyncCallCompletedEventArgs<Group>> GetGroupByIdAsyncCompleted;
		public event EventHandler<AsyncCallCompletedEventArgs<List<Group>>> GetGroupListAsyncCompleted;

		public Group GetGroupById(string id)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/group/{1}/", this.BaseUrl, id ) );
				return this.WebRequestSync<Group>( url );
			}
			else
			{
				return _splxDal.GetGroupById( id );
			}
		}

		public void GetGroupByIdAsync(string id, object state)
		{
			Uri url = new Uri( string.Format( "{0}/group/{1}/", this.BaseUrl, id ) );
			RequestData<Group> rd = new RequestData<Group>( url, state );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.GetGroupById_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( GetGroupById_RunWorkerCompleted );
			w.RunWorkerAsync( rd );
		}

		void GetGroupById_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData<Group> rd = e.Argument as RequestData<Group>;
			rd.Result = this.WebRequestSync<Group>( rd.Url );
			e.Result = rd;
		}
		void GetGroupById_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.GetGroupByIdAsyncCompleted != null )
			{
				RequestData<Group> rd = (RequestData<Group>)e.Result;
				this.GetGroupByIdAsyncCompleted( this,
					new AsyncCallCompletedEventArgs<Group>( rd.Result, rd.State ) );
			}
		}


		public List<Group> GetGroupList()
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/group/", this.BaseUrl ) );
				return this.WebRequestSync<List<Group>>( url );
			}
			else
			{
				return _splxDal.GetGroupList();
			}
		}

		public void GetGroupListAsync(object state)
		{
			Uri url = new Uri( string.Format( "{0}/group/", this.BaseUrl ) );
			RequestData<List<Group>> rd = new RequestData<List<Group>>( url, state );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.GetGroupList_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( GetGroupList_RunWorkerCompleted );
			w.RunWorkerAsync( rd );
		}

		void GetGroupList_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData<List<Group>> rd = e.Argument as RequestData<List<Group>>;
			rd.Result = this.WebRequestSync<List<Group>>( rd.Url );
			e.Result = rd;
		}
		void GetGroupList_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.GetGroupListAsyncCompleted != null )
			{
				RequestData<List<Group>> rd = (RequestData<List<Group>>)e.Result;
				this.GetGroupListAsyncCompleted( this,
					new AsyncCallCompletedEventArgs<List<Group>>( rd.Result, rd.State ) );
			}
		}


		public MembershipList<SecurityPrincipalBase> GetGroupMembers(string groupId)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/group/{1}/members/", this.BaseUrl, groupId ) );
				return this.WebRequestSync<MembershipList<SecurityPrincipalBase>>( url );
			}
			else if( this.IsDatabaseConnection )
			{
				return _splxDal.GetGroupMembers( groupId );
			}
			else
			{
				return null;
			}
		}

		public List<Group> GetGroupHierarchy(string groupId)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/group/{1}/hier/", this.BaseUrl, groupId ) );
				return this.WebRequestSync<List<Group>>( url );
			}
			else if( this.IsDatabaseConnection )
			{
				return _splxDal.GetGroupHierarchy( groupId );
			}
			else
			{
				return null;
			}
		}
		#endregion

		#region upsert
		public Group UpsertGroup(Group group, List<SecurityPrincipalBase> addedGroupMembership, List<SecurityPrincipalBase> removedGroupMembership)
		{
			if( this.IsRestConnection )
			{
				GroupData groupData = new GroupData();
				groupData.Group = group;
				groupData.AddedGroupMembership = this.JoinMembership( addedGroupMembership );
				groupData.RemovedGroupMembership = this.JoinMembership( removedGroupMembership );

				Uri url = new Uri( string.Format( "{0}/group/", this.BaseUrl ) );
				byte[] data = this.SerializeObject<GroupData>( groupData );
				return this.WebRequestSync<Group>( url, HttpMethod.Post, data );
			}
			else if( this.IsDatabaseConnection )
			{
				return _splxDal.UpsertGroup( group, addedGroupMembership, removedGroupMembership );
			}
			else
			{
				return null;
			}
		}
		#endregion

		#region delete
		public void DeleteGroupById(string id)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/group/{1}/", this.BaseUrl, id ) );
				this.WebRequestSync( url, HttpMethod.Delete, null );
			}
			else if( this.IsDatabaseConnection )
			{
				_splxDal.DeleteGroupById( id );
			}
		}
		#endregion
	}
}