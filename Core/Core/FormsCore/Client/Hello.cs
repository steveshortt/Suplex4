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
		#region hello
		public event System.EventHandler<AsyncCallCompletedEventArgs<string>> HelloAsyncCompleted;

		public string Hello()
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/hello", this.BaseUrl ) );
				return this.WebRequestSync<string>( url );
			}
			else
			{
				return "something";
			}
		}

		public void HelloAsync(object state)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/hello", this.BaseUrl ) );
				RequestData<string> rd = new RequestData<string>( url, state );

				BackgroundWorker w = new BackgroundWorker();
				w.DoWork += new DoWorkEventHandler( this.Hello_Worker );
				w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( Hello_RunWorkerCompleted );
				w.RunWorkerAsync( rd );
			}
			else
			{ }
		}

		void Hello_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData<string> rd = e.Argument as RequestData<string>;
			rd.Result = this.WebRequestSync<string>( rd.Url );
			e.Result = rd;
		}
		void Hello_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.HelloAsyncCompleted != null )
			{
				RequestData<string> rd = (RequestData<string>)e.Result;
				this.HelloAsyncCompleted( this,
					new AsyncCallCompletedEventArgs<string>( rd.Result, rd.State ) );
			}
		}
		#endregion

		#region WhoAmI
		public event System.EventHandler<AsyncCallCompletedEventArgs<WhoAmIRecord>> WhoAmIAsyncCompleted;

		public WhoAmIRecord WhoAmI()
		{
			Uri url = new Uri( string.Format( "{0}/hello/?whoami", this.BaseUrl ) );
			return this.WebRequestSync<WhoAmIRecord>( url );
		}

		public void WhoAmIAsync(object state)
		{
			Uri url = new Uri( string.Format( "{0}/hello/?whoami", this.BaseUrl ) );
			RequestData<WhoAmIRecord> rd = new RequestData<WhoAmIRecord>( url, state );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.WhoAmI_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( WhoAmI_RunWorkerCompleted );
			w.RunWorkerAsync( rd );
		}

		void WhoAmI_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData<WhoAmIRecord> rd = e.Argument as RequestData<WhoAmIRecord>;
			rd.Result = this.WebRequestSync<WhoAmIRecord>( rd.Url );
			e.Result = rd;
		}
		void WhoAmI_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.WhoAmIAsyncCompleted != null )
			{
				RequestData<WhoAmIRecord> rd = (RequestData<WhoAmIRecord>)e.Result;
				this.WhoAmIAsyncCompleted( this,
					new AsyncCallCompletedEventArgs<WhoAmIRecord>( rd.Result, rd.State ) );
			}
		}
		#endregion
	}
}