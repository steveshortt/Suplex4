using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace Suplex.Forms.ObjectModel.Api
{
	public partial class SuplexApiClient
	{
		#region select
		public event System.EventHandler<AsyncCallCompletedEventArgs<User>> GetUserByIdAsyncCompleted;

		public User GetUserById(string id)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/user/{1}/", this.BaseUrl, id ) );
				return this.WebRequestSync<User>( url );
			}
			else
			{
				return _splxDal.GetUserById( id );
			}
		}

		public void GetUserByIdAsync(string id, object state)
		{
			Uri url = new Uri( string.Format( "{0}/user/{1}/", this.BaseUrl, id ) );
			RequestData<User> rd = new RequestData<User>( url, state );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.GetUserById_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( GetUserById_RunWorkerCompleted );
			w.RunWorkerAsync( rd );
		}

		void GetUserById_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData<User> rd = e.Argument as RequestData<User>;
			rd.Result = this.WebRequestSync<User>( rd.Url );
			e.Result = rd;
		}
		void GetUserById_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.GetUserByIdAsyncCompleted != null )
			{
				RequestData<User> rd = (RequestData<User>)e.Result;
				this.GetUserByIdAsyncCompleted( this,
					new AsyncCallCompletedEventArgs<User>( rd.Result, rd.State ) );
			}
		}

		public MembershipList<Group> GetUserGroupMemberOf(string userId)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/user/{1}/memberof/", this.BaseUrl, userId ) );
				return this.WebRequestSync<MembershipList<Group>>( url );
			}
			else if( this.IsDatabaseConnection )
			{
				return _splxDal.GetUserGroupMemberOf( userId );
			}
			else
			{
				return null;
			}
		}
		#endregion

		#region upsert
		public User UpsertUser(User user, List<Group> addedGroupMembership, List<Group> removedGroupMembership)
		{
			if( this.IsRestConnection )
			{
				UserData userData = new UserData();
				userData.User = user;
				userData.AddedGroupMembership = this.JoinMembership( addedGroupMembership );
				userData.RemovedGroupMembership = this.JoinMembership( removedGroupMembership );

				Uri url = new Uri( string.Format( "{0}/user/", this.BaseUrl ) );
				byte[] data = this.SerializeObject<UserData>( userData );
				return this.WebRequestSync<User>( url, HttpMethod.Post, data );
			}
			else if( this.IsDatabaseConnection )
			{
				return _splxDal.UpsertUser( user, addedGroupMembership, removedGroupMembership );
			}
			else
			{
				return null;
			}
		}
		#endregion

		#region delete
		public void DeleteUserById(string id)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/user/{1}/", this.BaseUrl, id ) );
				this.WebRequestSync( url, HttpMethod.Delete, null );
			}
			else if( this.IsDatabaseConnection )
			{
				_splxDal.DeleteUserById( id );
			}
		}
		#endregion
	}
}