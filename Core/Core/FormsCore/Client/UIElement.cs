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
		public event System.EventHandler<AsyncCallCompletedEventArgs<UIElement>> GetUIElementByIdAsyncCompleted;
		public event System.EventHandler<AsyncCompletedEventArgs> UpsertUIElementAsyncCompleted;
		public event System.EventHandler<AsyncCompletedEventArgs> DeleteUIElementByIdAsyncCompleted;

		#region select
		public UIElement GetUIElementById(string id, bool shallow)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/uie/{1}/?shallow={2}", this.BaseUrl, id, shallow ) );
				return this.WebRequestSync<UIElement>( url );
			}
			else if( this.IsDatabaseConnection )
			{
				if( shallow )
				{
					return _splxDal.GetUIElementByIdShallow( id );
				}
				else
				{
					return _splxDal.GetUIElementByIdDeep( id );
				}
			}
			else
			{
				return null;
			}
		}

		public void GetUIElementByIdAsync(string id, bool shallow, object state)
		{
			Uri url = new Uri( string.Format( "{0}/uie/{1}/?shallow={2}", this.BaseUrl, id, shallow ) );
			RequestData<UIElement> rd = new RequestData<UIElement>( url, state );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.GetUIElementById_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( GetUIElementById_RunWorkerCompleted );
			w.RunWorkerAsync( rd );
		}

		void GetUIElementById_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData<UIElement> rd = e.Argument as RequestData<UIElement>;
			rd.Result = this.WebRequestSync<UIElement>( rd.Url );
			e.Result = rd;
		}
		void GetUIElementById_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.GetUIElementByIdAsyncCompleted != null )
			{
				RequestData<UIElement> rd = (RequestData<UIElement>)e.Result;
				this.GetUIElementByIdAsyncCompleted( this,
					new AsyncCallCompletedEventArgs<UIElement>( rd.Result, rd.State ) );
			}
		}
		#endregion

		#region upsert
		public UIElement UpsertUIElement(UIElement uie)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/uie/", this.BaseUrl ) );
				byte[] data = this.SerializeObject<UIElement>( uie );
				return this.WebRequestSync<UIElement>( url, HttpMethod.Post, data );
			}
			else if( this.IsDatabaseConnection )
			{
				return _splxDal.UpsertUIElement( uie );
			}
			else
			{
				return null;
			}
		}

		public void UpsertUIElementAsync(UIElement uie, object state)
		{
			Uri url = new Uri( string.Format( "{0}/uie/", this.BaseUrl ) );
			byte[] data = this.SerializeObject<UIElement>( uie );
			RequestData rd = new RequestData( url, state, data );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.UpsertUIElement_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( UpsertUIElement_RunWorkerCompleted );
			w.RunWorkerAsync( rd );
		}

		void UpsertUIElement_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData rd = e.Argument as RequestData;
			this.WebRequestSync( rd.Url, HttpMethod.Post, rd.Data );
			e.Result = rd;
		}

		void UpsertUIElement_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.UpsertUIElementAsyncCompleted != null )
			{
				RequestData rd = (RequestData)e.Result;
				this.UpsertUIElementAsyncCompleted( this,
					new AsyncCompletedEventArgs( null, false, rd.State ) );
			}
		}
		#endregion

		#region delete
		public void DeleteUIElementById(Guid id)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/uie/{1}/", this.BaseUrl, id ) );
				this.WebRequestSync( url, HttpMethod.Delete, null );
			}
			else if( this.IsDatabaseConnection )
			{
				_splxDal.DeleteUIElementById( id.ToString() );
			}
		}

		public void DeleteUIElementByIdAsync(Guid id, object state)
		{
			Uri url = new Uri( string.Format( "{0}/uie/{1}/", this.BaseUrl, id ) );
			this.WebRequestSync( url, HttpMethod.Delete, null );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.DeleteUIElementById_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( DeleteUIElementById_RunWorkerCompleted );
			w.RunWorkerAsync();
		}

		void DeleteUIElementById_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData rd = e.Argument as RequestData;
			this.WebRequestSync( rd.Url, HttpMethod.Delete, null );
			e.Result = rd;
		}

		void DeleteUIElementById_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.DeleteUIElementByIdAsyncCompleted != null )
			{
				RequestData rd = (RequestData)e.Result;
				this.DeleteUIElementByIdAsyncCompleted( this,
					new AsyncCompletedEventArgs( null, false, rd.State ) );
			}
		}
		#endregion
	}
}