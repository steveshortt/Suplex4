using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.IO;


namespace Suplex.Forms.ObjectModel.Api
{
	public partial class SuplexApiClient : INotifyPropertyChanged
	{
		public event System.EventHandler<AsyncCallCompletedEventArgs<DataSet>> GetSecurityAsyncCompleted;
		public event System.EventHandler<AsyncCallCompletedEventArgs<SuplexStore>> GetSecurityStoreAsyncCompleted;

		public DataSet GetSecurity(string uniqueName, Security.Standard.User user, ExternalGroupInfo egi)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/uie/security/str/{1}", this.BaseUrl, uniqueName ) );
				string sec = this.WebRequestSync<string>( url );

				byte[] secBytes = ASCIIEncoding.UTF8.GetBytes( sec );
				MemoryStream ms = new MemoryStream( secBytes );
				DataSet ds = new DataSet();
				ds.ReadXml( ms );

				return ds;
			}
			else
			{
				return _splxDal.GetSecurityCache( uniqueName, user, egi );
			}
		}

		public void GetSecurityAsync(string uniqueName, object state)
		{
			Uri url = new Uri( string.Format( "{0}/uie/security/{1}", this.BaseUrl, uniqueName ) );
			RequestData<DataSet> rd = new RequestData<DataSet>( url, state );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.GetSecurity_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( GetSecurity_RunWorkerCompleted );
			w.RunWorkerAsync( rd );
		}

		void GetSecurity_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData<DataSet> rd = e.Argument as RequestData<DataSet>;
			rd.Result = this.WebRequestSync<DataSet>( rd.Url );
			e.Result = rd;
		}
		void GetSecurity_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.GetSecurityAsyncCompleted != null )
			{
				RequestData<DataSet> rd = (RequestData<DataSet>)e.Result;
				this.GetSecurityAsyncCompleted( this,
					new AsyncCallCompletedEventArgs<DataSet>( rd.Result, rd.State ) );
			}
		}


		public SuplexStore GetSecurityStore(string uniqueName, Security.Standard.User user, ExternalGroupInfo egi)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/uie/security/store/{1}", this.BaseUrl, uniqueName ) );
				return this.WebRequestSync<SuplexStore>( url );
			}
			else
			{
				return _splxDal.GetSecurityStore( uniqueName, user, egi );
			}
		}

		public void GetSecurityStoreAsync(string id, object state)
		{
			Uri url = new Uri( string.Format( "{0}/uie/security/store/{1}", this.BaseUrl, id ) );
			RequestData<SuplexStore> rd = new RequestData<SuplexStore>( url, state );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.GetSecurityStore_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( GetSecurityStore_RunWorkerCompleted );
			w.RunWorkerAsync( rd );
		}

		void GetSecurityStore_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData<SuplexStore> rd = e.Argument as RequestData<SuplexStore>;
			rd.Result = this.WebRequestSync<SuplexStore>( rd.Url );
			e.Result = rd;
		}
		void GetSecurityStore_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.GetSecurityStoreAsyncCompleted != null )
			{
				RequestData<SuplexStore> rd = (RequestData<SuplexStore>)e.Result;
				this.GetSecurityStoreAsyncCompleted( this,
					new AsyncCallCompletedEventArgs<SuplexStore>( rd.Result, rd.State ) );
			}
		}
	}
}