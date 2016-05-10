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
		public event System.EventHandler<AsyncCallCompletedEventArgs<ValidationRule>> GetValidationRuleByIdAsyncCompleted;
		public event System.EventHandler<AsyncCompletedEventArgs> UpsertValidationRuleAsyncCompleted;
		public event System.EventHandler<AsyncCompletedEventArgs> DeleteValidationRuleByIdAsyncCompleted;

		#region select
		public ValidationRule GetValidationRuleById(string id, bool shallow)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/vr/{1}/?shallow={2}", this.BaseUrl, id, shallow ) );
				return this.WebRequestSync<ValidationRule>( url );
			}
			else if( this.IsDatabaseConnection )
			{
				if( shallow )
				{
					return _splxDal.GetValidationRuleByIdShallow( id );
				}
				else
				{
					return _splxDal.GetValidationRuleByIdDeep( id );
				}
			}
			else
			{
				return null;
			}
		}

		public void GetValidationRuleByIdAsync(string id, bool shallow, object state)
		{
			Uri url = new Uri( string.Format( "{0}/vr/{1}/?shallow={2}", this.BaseUrl, id, shallow ) );
			RequestData<ValidationRule> rd = new RequestData<ValidationRule>( url, state );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.GetValidationRuleById_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( GetValidationRuleById_RunWorkerCompleted );
			w.RunWorkerAsync( rd );
		}

		void GetValidationRuleById_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData<ValidationRule> rd = e.Argument as RequestData<ValidationRule>;
			rd.Result = this.WebRequestSync<ValidationRule>( rd.Url );
			e.Result = rd;
		}
		void GetValidationRuleById_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.GetValidationRuleByIdAsyncCompleted != null )
			{
				RequestData<ValidationRule> rd = (RequestData<ValidationRule>)e.Result;
				this.GetValidationRuleByIdAsyncCompleted( this,
					new AsyncCallCompletedEventArgs<ValidationRule>( rd.Result, rd.State ) );
			}
		}
		#endregion

		#region upsert
		public ValidationRule UpsertValidationRule(ValidationRule vr)
		{
			vr.ResolveParents();

			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/vr/", this.BaseUrl ) );
				byte[] data = this.SerializeObject<ValidationRule>( vr );
				return this.WebRequestSync<ValidationRule>( url, HttpMethod.Post, data );
			}
			else if( this.IsDatabaseConnection )
			{
				return _splxDal.UpsertValidationRule( vr );
			}
			else
			{
				return null;
			}
		}

		public void UpsertValidationRuleAsync(ValidationRule vr, object state)
		{
			vr.ResolveParents();

			Uri url = new Uri( string.Format( "{0}/vr/", this.BaseUrl ) );
			byte[] data = this.SerializeObject<ValidationRule>( vr );
			RequestData rd = new RequestData( url, state, data );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.UpsertValidationRule_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( UpsertValidationRule_RunWorkerCompleted );
			w.RunWorkerAsync( rd );
		}

		void UpsertValidationRule_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData rd = e.Argument as RequestData;
			this.WebRequestSync( rd.Url, HttpMethod.Post, rd.Data );
			e.Result = rd;
		}

		void UpsertValidationRule_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.UpsertValidationRuleAsyncCompleted != null )
			{
				RequestData rd = (RequestData)e.Result;
				this.UpsertValidationRuleAsyncCompleted( this,
					new AsyncCompletedEventArgs( null, false, rd.State ) );
			}
		}
		#endregion

		#region delete
		public void DeleteValidationRuleById(Guid id)
		{
			if( this.IsRestConnection )
			{
				Uri url = new Uri( string.Format( "{0}/vr/{1}/", this.BaseUrl, id ) );
				this.WebRequestSync( url, HttpMethod.Delete, null );

			}
			else if( this.IsDatabaseConnection )
			{
				_splxDal.DeleteLogicRuleById( id.ToString() );
			}
		}

		public void DeleteValidationRuleByIdAsync(Guid id, object state)
		{
			Uri url = new Uri( string.Format( "{0}/vr/{1}/", this.BaseUrl, id ) );
			this.WebRequestSync( url, HttpMethod.Delete, null );

			BackgroundWorker w = new BackgroundWorker();
			w.DoWork += new DoWorkEventHandler( this.DeleteValidationRuleById_Worker );
			w.RunWorkerCompleted += new RunWorkerCompletedEventHandler( DeleteValidationRuleById_RunWorkerCompleted );
			w.RunWorkerAsync();
		}

		void DeleteValidationRuleById_Worker(object sender, DoWorkEventArgs e)
		{
			RequestData rd = e.Argument as RequestData;
			this.WebRequestSync( rd.Url, HttpMethod.Delete, null );
			e.Result = rd;
		}

		void DeleteValidationRuleById_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if( this.DeleteValidationRuleByIdAsyncCompleted != null )
			{
				RequestData rd = (RequestData)e.Result;
				this.DeleteValidationRuleByIdAsyncCompleted( this,
					new AsyncCompletedEventArgs( null, false, rd.State ) );
			}
		}
		#endregion
	}
}