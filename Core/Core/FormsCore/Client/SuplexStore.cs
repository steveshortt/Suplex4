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
		#region select
		public event System.EventHandler<AsyncCallCompletedEventArgs<SuplexStore>> GetSuplexStoreByIdAsyncCompleted;

		public SuplexStore GetSuplexStore()
		{
			if( !this.IsConnected ) { return null; }

			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/store/", this.BaseUrl ) );
				return this.WebRequestSync<SuplexStore>( url );

				//return new SuplexStore();
			}
			else
			{
				return _splxDal.GetSuplexStore();
			}
		}
		public SuplexStore GetSuplexStore(bool exportValidation, bool exportSecurity)
		{
			if( !this.IsConnected ) { return null; }

			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/store/", this.BaseUrl ) );
				return this.WebRequestSync<SuplexStore>( url );
			}
			else
			{
				return _splxDal.GetSuplexStore( exportValidation, exportSecurity );
			}
		}

		public void GetSuplexStoreByIdAsync(string id, bool shallow, object state)
		{
			Uri url = new Uri( string.Format( "{0}/store", this.BaseUrl ) );
			RequestData<SuplexStore> rd = new RequestData<SuplexStore>( url, state );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.GetSuplexStoreById_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( GetSuplexStoreById_RunWorkerCompleted );
			w.RunWorkerAsync( rd );
		}

		void GetSuplexStoreById_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData<SuplexStore> rd = e.Argument as RequestData<SuplexStore>;
			rd.Result = this.WebRequestSync<SuplexStore>( rd.Url );
			e.Result = rd;
		}
		void GetSuplexStoreById_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.GetSuplexStoreByIdAsyncCompleted != null )
			{
				RequestData<SuplexStore> rd = (RequestData<SuplexStore>)e.Result;
				this.GetSuplexStoreByIdAsyncCompleted( this,
					new AsyncCallCompletedEventArgs<SuplexStore>( rd.Result, rd.State ) );
			}
		}
		#endregion

		public void UpsertWholeStore(SuplexStore importStore, bool includeValidation, bool includeSecurity)
		{
			if( !this.IsConnected ) { return; }

			if( this.IsRestConnection )
			{
				//Uri url = new Uri( string.Format( "{0}/store/", this.BaseUrl ) );
				//return this.WebRequestSync<SuplexStore>( url );
			}
			else
			{
				_splxDal.UpsertWholeStore( importStore, includeValidation, includeSecurity );
			}
		}
	}
}