using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;


namespace Suplex.Forms.ObjectModel.Api
{
	public partial class SuplexApiClient
	{
		public event System.EventHandler<AsyncCallCompletedEventArgs<FillMap>> GetFillMapByIdAsyncCompleted;
		public event System.EventHandler<AsyncCompletedEventArgs> UpsertFillMapAsyncCompleted;
		public event System.EventHandler<AsyncCompletedEventArgs> DeleteFillMapByIdAsyncCompleted;

		#region select
		public FillMap GetFillMapById(string id)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/fm/{1}", this.BaseUrl, id ) );
				return this.WebRequestSync<FillMap>( url );
			}
			else
			{
				return _splxDal.GetFillMapById( id );
			}
		}

		public void GetFillMapByIdAsync(string id, object state)
		{
			Uri url = new Uri( string.Format( "{0}/fm/{1}", this.BaseUrl, id ) );
			RequestData<FillMap> rd = new RequestData<FillMap>( url, state );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.GetFillMapById_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( GetFillMapById_RunWorkerCompleted );
			w.RunWorkerAsync( rd );
		}

		void GetFillMapById_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData<FillMap> rd = e.Argument as RequestData<FillMap>;
			rd.Result = this.WebRequestSync<FillMap>( rd.Url );
			e.Result = rd;
		}
		void GetFillMapById_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.GetFillMapByIdAsyncCompleted != null )
			{
				RequestData<FillMap> rd = (RequestData<FillMap>)e.Result;
				this.GetFillMapByIdAsyncCompleted( this,
					new AsyncCallCompletedEventArgs<FillMap>( rd.Result, rd.State ) );
			}
		}
		#endregion

		#region upsert
		public FillMap UpsertFillMap(FillMap fm)
		{
			fm.ResolveParent();

			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/fm/", this.BaseUrl ) );
				byte[] data = this.SerializeObject<FillMap>( fm );
				return this.WebRequestSync<FillMap>( url, HttpMethod.Post, data );
			}
			else if( this.IsDatabaseConnection )
			{
				return _splxDal.UpsertFillMap( fm );
			}
			else
			{
				return null;
			}
		}

		public void UpsertFillMapAsync(FillMap fm, object state)
		{
			fm.ResolveParent();

			Uri url = new Uri( string.Format( "{0}/fm/", this.BaseUrl ) );
			byte[] data = this.SerializeObject<FillMap>( fm );
			RequestData rd = new RequestData( url, state, data );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.UpsertFillMap_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( UpsertFillMap_RunWorkerCompleted );
			w.RunWorkerAsync( rd );
		}

		void UpsertFillMap_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData rd = e.Argument as RequestData;
			this.WebRequestSync( rd.Url, HttpMethod.Post, rd.Data );
			e.Result = rd;
		}

		void UpsertFillMap_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.UpsertFillMapAsyncCompleted != null )
			{
				RequestData rd = (RequestData)e.Result;
				this.UpsertFillMapAsyncCompleted( this,
					new AsyncCompletedEventArgs( null, false, rd.State ) );
			}
		}
		#endregion

		#region delete
		public void DeleteFillMapById(long id)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/fm/{1}/", this.BaseUrl, id ) );
				this.WebRequestSync( url, HttpMethod.Delete, null );
			}
			else if( this.IsDatabaseConnection )
			{
				_splxDal.DeleteFillMapById( id.ToString() );
			}
		}

		public void DeleteFillMapByIdAsync(Guid id, object state)
		{
			Uri url = new Uri( string.Format( "{0}/fm/{1}/", this.BaseUrl, id ) );
			this.WebRequestSync( url, HttpMethod.Delete, null );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.DeleteFillMapById_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( DeleteFillMapById_RunWorkerCompleted );
			w.RunWorkerAsync();
		}

		void DeleteFillMapById_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData rd = e.Argument as RequestData;
			this.WebRequestSync( rd.Url, HttpMethod.Delete, null );
			e.Result = rd;
		}

		void DeleteFillMapById_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.DeleteFillMapByIdAsyncCompleted != null )
			{
				RequestData rd = (RequestData)e.Result;
				this.DeleteFillMapByIdAsyncCompleted( this,
					new AsyncCompletedEventArgs( null, false, rd.State ) );
			}
		}
		#endregion
	}
}