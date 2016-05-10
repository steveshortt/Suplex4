using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;

using Suplex.Data;
using sg = Suplex.General;


namespace Suplex.Forms.ObjectModel.Api
{
	public partial class SuplexApiClient : INotifyPropertyChanged
	{
		private SuplexDataAccessLayer _splxDal = null;

		private string _baseUrl = string.Empty;
		private bool _hasRestConnection = false;

		private bool _hasDatabaseConnection = false;

		private bool _hasFileConnection = false;
		private FileInfo _fileStore = null;
		private bool _hasPublicPrivateKeyFile = false;
		private FileInfo _publicPrivateKeyFileStore = null;

		private bool _isConnected = false;
		private string _connectionPath = string.Empty;


		#region ctors
		public SuplexApiClient()
		{
			_splxDal = new SuplexDataAccessLayer();
		}
		public SuplexApiClient(string connectionString)
			: this()
		{
			this.ConnectionString = connectionString;
		}
		public SuplexApiClient(string baseUrl, WebMessageFormatType messageFormat)
			: this()
		{
			this.BaseUrl = baseUrl;
			this.MessageFormat = messageFormat;
		}
		#endregion

		#region props
		//private ApiConnectionType ConnectionType { get; set; }

		private bool IsRestConnection { get { return this.HasRestConnection; } }
		private bool IsDatabaseConnection { get { return this.HasDatabaseConnection; } }
		private bool IsFileConnection { get { return this.HasFileConnection; } }

		public bool IsConnected
		{
			get { return _isConnected; }
			internal set
			{
				if( _isConnected != value )
				{
					_isConnected = value;
					this.OnPropertyChanged( "IsConnected" );
				}
			}
		}

		private void CheckIsConnected()
		{
			this.IsConnected = this.HasRestConnection || this.HasDatabaseConnection;
		}

		public string ConnectionPath
		{
			get { return _connectionPath; }
			set
			{
				if( _connectionPath != value )
				{
					_connectionPath = value;
					this.OnPropertyChanged( "ConnectionPath" );
				}
			}
		}
		#endregion

		#region rest connection
		public void Connect(string baseUrl, WebMessageFormatType messageFormat)
		{
			this.BaseUrl = baseUrl;
			this.MessageFormat = messageFormat;
		}

		public bool IsJson { get { return this.MessageFormat == WebMessageFormatType.Json; } }
		public string ContentType { get { return this.IsJson ? "application/json" : "application/xml"; } }

		public string BaseUrl
		{
			get { return _baseUrl; }
			set
			{
				if( _baseUrl != value )
				{
					_baseUrl = value;
					//this.ConnectionType = ApiConnectionType.RestService;
					this.HasRestConnection = !string.IsNullOrWhiteSpace( value );
					this.ConnectionPath = value;
				}
			}
		}
		public WebMessageFormatType MessageFormat { get; set; }

		public bool HasRestConnection
		{
			get { return _hasRestConnection; }
			internal set
			{
				if( _hasRestConnection != value )
				{
					_hasRestConnection = value;
					if( value )
					{
						this.HasFileConnection = false;
						this.HasDatabaseConnection = false;
					}
					this.CheckIsConnected();
					this.OnPropertyChanged( "HasRestConnection" );
				}
			}
		}

		#region WebRequest helpers
		//todo: something with exceptions...
		protected void WebRequestSync(Uri url)
		{
			this.WebRequestSync<VoidObject>( url, HttpMethod.Get, null );
		}
		protected T WebRequestSync<T>(Uri url)
		{
			return this.WebRequestSync<T>( url, HttpMethod.Get, null );
		}

		protected void WebRequestSync(Uri url, string method, byte[] data)
		{
			this.WebRequestSync<VoidObject>( url, method, data );
		}
		protected T WebRequestSync<T>(Uri url, string method, byte[] data)
		{
			T result = default( T );

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create( url );
			request.ContentType = this.ContentType;
			request.Credentials = System.Net.CredentialCache.DefaultCredentials;
			request.Method = method;

			if( data != null && data.Length > 0 )
			{
				using( Stream requestStream = request.GetRequestStream() )
				{
					requestStream.Write( data, 0, data.Length );
				}
			}

			HttpWebResponse response = null;
			try
			{
				response = (HttpWebResponse)request.GetResponse();
			}
			catch( WebException wex )
			{
				throw wex.ToServiceException();
			}
			catch { throw; }

			if( typeof( T ) != typeof( VoidObject ) )
			{
				XmlObjectSerializer dcs = new DataContractJsonSerializer( typeof( T ) );
				if( response.ContentType.ToLower().Contains( "xml" ) )
				{
					dcs = new DataContractSerializer( typeof( T ) );
				}
				using( Stream rs = response.GetResponseStream() )
				{
					result = (T)dcs.ReadObject( rs );
				}
			}
			response.Close();

			return result;
		}
		//todo: something with exceptions...
		protected RequestResult<T> WebRequestSyncTimed<T>(Uri url)
		{
			RequestResult<T> rr = new RequestResult<T>();
			rr.Result = this.WebRequestSync<T>( url, HttpMethod.Get, null );
			rr.Stop();
			return rr;
		}


		[Obsolete( "Works, but not for UI databinding.  Do not use.", false )]
		internal void WebRequestAsync<T>(Uri url, object state, AsyncCallback asyncCallBackMethod)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create( url );
			request.ContentType = this.ContentType;
			request.Credentials = System.Net.CredentialCache.DefaultCredentials;

			RequestData<T> rd = new RequestData<T>( request, state );
			request.BeginGetResponse( asyncCallBackMethod, rd );
		}

		[Obsolete( "Works, but not for UI databinding.  Do not use.", false )]
		internal RequestData<T> WebRequestAsyncCallBackHelper<T>(IAsyncResult asyncResult)
		{
			RequestData<T> rd = asyncResult.AsyncState as RequestData<T>;
			rd.Result = default( T );
			HttpWebResponse response = rd.Request.EndGetResponse( asyncResult ) as HttpWebResponse;
			XmlObjectSerializer dcs = new DataContractJsonSerializer( typeof( T ) );
			if( response.ContentType.Contains( "xml" ) )
			{
				dcs = new DataContractSerializer( typeof( T ) );
			}
			using( Stream rs = response.GetResponseStream() )
			{
				rd.Result = (T)dcs.ReadObject( rs );
			}
			response.Close();

			return rd;
		}

		internal byte[] SerializeObject<T>(object data)
		{
			XmlObjectSerializer dcs = new DataContractSerializer( typeof( T ) );
			if( this.IsJson )
			{
				dcs = new DataContractJsonSerializer( typeof( T ) );
			}

			MemoryStream ms = new MemoryStream();
			dcs.WriteObject( ms, data );

			return ms.ToArray();
		}

		//protected void WebRequestAsyncCallBackHelper<T, TEventArgs>(IAsyncResult asyncResult, EventHandler<TEventArgs> eventHandler)
		//{
		//    StateWrapper<T> sw = asyncResult.AsyncState as StateWrapper<T>;
		//    sw.Result = default( T );
		//    HttpWebResponse response = sw.Request.EndGetResponse( asyncResult ) as HttpWebResponse;
		//    DataContractSerializer dcs = new DataContractSerializer( typeof( T ) );
		//    using( Stream rs = response.GetResponseStream() )
		//    {
		//        sw.Result = (T)dcs.ReadObject( rs );
		//    }
		//    response.Close();

		//    if( eventHandler != null )
		//    {
		//        eventHandler( this,
		//            new T( sw.Result, sw.State ) );
		//    }
		//}
		#endregion
		#endregion

		#region database connection
		public void Connect(string databaseServerName, string databaseName, string username, string password)
		{
			ConnectionProperties cp = null;
			if( string.IsNullOrEmpty( username ) || string.IsNullOrEmpty( password ) )
			{
				cp = new ConnectionProperties( databaseServerName, databaseName );
			}
			else
			{
				cp = new ConnectionProperties( databaseServerName, databaseName, username, password );
			}

			this.ConnectionString = cp.ConnectionString;
			this.ConnectionPath = cp.LiteDisplayString;
		}

		public void Disconnect()
		{
			this.BaseUrl = string.Empty;
			this.ConnectionString = string.Empty;
			this.ConnectionPath = string.Empty;
			this.CheckIsConnected();
		}

		public string ConnectionString
		{
			get { return _splxDal.ConnectionString; }
			internal set
			{
				_splxDal.ConnectionString = value;
				//this.ConnectionType = ApiConnectionType.Database;
				this.HasDatabaseConnection = !string.IsNullOrWhiteSpace( value );
			}
		}

		public bool HasDatabaseConnection
		{
			get { return _hasDatabaseConnection; }
			internal set
			{
				if( _hasDatabaseConnection != value )
				{
					_hasDatabaseConnection = value;
					if( value )
					{
						this.HasFileConnection = false;
						this.HasRestConnection = false;
					}
					this.CheckIsConnected();
					this.OnPropertyChanged( "HasDatabaseConnection" );
				}
			}
		}

		public SuplexDataAccessLayer SplxDal { get { return _splxDal; } }
		#endregion

		#region file connection
		public static SuplexStore LoadSuplexFile(string path)
		{
			SuplexStore store = sg.XmlUtils.Deserialize<SuplexStore>( path );

			if( store.GroupMembership != null )
			{
				store.GroupMembership.OwnerStore = store;
				store.GroupMembership.Resolve();
			}

			store.IsDirty = false;

			return store;
		}

		public FileInfo File
		{
			get { return _fileStore; }
			internal set
			{
				if( _fileStore != value )
				{
					_fileStore = value;
					this.OnPropertyChanged( "File" );
					this.HasFileConnection = value != null;
					//this.ConnectionType = ApiConnectionType.File;
					if( this.HasFileConnection )
					{
						this.ConnectionPath = value.FullName;
					}
					else
					{
						this.ConnectionPath = string.Empty;
					}
				}
			}
		}

		public bool HasFileConnection
		{
			get { return _hasFileConnection; }
			internal set
			{
				if( _hasFileConnection != value )
				{
					_hasFileConnection = value;
					if( value )
					{
						this.HasRestConnection = false;
						this.HasDatabaseConnection = false;
					}
					this.OnPropertyChanged( "HasFileConnection" );
				}
			}
		}


		public FileInfo PublicPrivateKeyFile
		{
			get { return _publicPrivateKeyFileStore; }
			internal set
			{
				if( _publicPrivateKeyFileStore != value )
				{
					_publicPrivateKeyFileStore = value;
					this.OnPropertyChanged( "PublicPrivateKeyFile" );
					this.HasPublicPrivateKeyFile = value != null;
				}
			}
		}

		public string PublicPrivateKeyContainerName { get; set; }

		public bool HasPublicPrivateKeyFile
		{
			get { return _hasPublicPrivateKeyFile; }
			internal set
			{
				if( _hasPublicPrivateKeyFile != value )
				{
					_hasPublicPrivateKeyFile = value;
					this.OnPropertyChanged( "HasPublicPrivateKeyFile" );
				}
			}
		}

		public void SetFile(string path)
		{
			if( path == null )
			{
				this.File = null;
			}
			else
			{
				this.File = new FileInfo( path );
			}
		}

		public void SetPublicPrivateKeyFile(string path)
		{
			if( path == null )
			{
				this.PublicPrivateKeyFile = null;
			}
			else
			{
				this.PublicPrivateKeyFile = new FileInfo( path );
			}
		}

		public SuplexStore LoadFile(string path)
		{
			this.SetFile( path );
			return this.LoadFile();
		}
		public SuplexStore LoadFile()
		{
			SuplexStore store = sg.XmlUtils.Deserialize<SuplexStore>( _fileStore.FullName );
			//store.File = _fileStore;

			if( store.GroupMembership != null )
			{
				store.GroupMembership.OwnerStore = store;
				store.GroupMembership.Resolve();
			}

			store.IsDirty = false;

			return store;
		}
		public SuplexStore LoadFile(string suplexFilePath, string keyFilePath, string keyContainerName)
		{
			this.SetFile( suplexFilePath );
			RSACryptoServiceProvider rsaKey = sg.XmlUtils.LoadRsaKeys( keyContainerName, keyFilePath );
			SuplexStore store = sg.XmlUtils.Deserialize<SuplexStore>( _fileStore.FullName, rsaKey );
			//store.File = _fileStore;

			if( store.GroupMembership != null )
			{
				store.GroupMembership.OwnerStore = store;
				store.GroupMembership.Resolve();
			}

			store.IsDirty = false;

			return store;
		}


		public SuplexStore LoadFromReader(TextReader reader)
		{
			//RSACryptoServiceProvider rsaKey = sg.XmlUtils.LoadRsaKeys( "XML_DSIG_RSA_KEY", "pub_only.txt" );
			//SuplexStore store = sg.XmlUtils.Deserialize<SuplexStore>( _fileStore.FullName, rsaKey );
			SuplexStore store = sg.XmlUtils.Deserialize<SuplexStore>( reader );
			//store.File = null;

			if( store.GroupMembership != null )
			{
				store.GroupMembership.OwnerStore = store;
				store.GroupMembership.Resolve();
			}

			store.IsDirty = false;

			return store;
		}

		public void SaveFile(SuplexStore splxStore, string path)
		{
			this.SetFile( path );
			this.SaveFile( splxStore );
		}
		public void SaveFile(SuplexStore splxStore)
		{
			if( this.HasPublicPrivateKeyFile )
			{
				RSACryptoServiceProvider rsaKey =
					sg.XmlUtils.LoadRsaKeys( this.PublicPrivateKeyContainerName, _publicPrivateKeyFileStore.FullName );
				sg.XmlUtils.Serialize<SuplexStore>( splxStore, _fileStore.FullName, rsaKey );
			}
			else
			{
				sg.XmlUtils.Serialize<SuplexStore>( splxStore, _fileStore.FullName );
			}

			splxStore.IsDirty = false;
		}
		#endregion

		#region INotifyPropertyChanged Members
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string propertyName)
		{
			if( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
			}
		}
		#endregion
	}

	//surely there's a better way to handle this
	class VoidObject { }

	internal class RequestData
	{
		public RequestData() { }
		public RequestData(Uri url, object state)
		{
			this.Url = url;
			this.State = state;
		}
		public RequestData(Uri url, object state, byte[] data)
		{
			this.Url = url;
			this.State = state;
			this.Data = data;
		}
		public RequestData(HttpWebRequest request, object state)
		{
			this.Request = request;
			this.State = state;
		}

		public Uri Url { get; set; }
		public HttpWebRequest Request { get; private set; }
		public Object State { get; private set; }
		public byte[] Data { get; private set; }
	}
	internal class RequestData<T> : RequestData
	{
		public RequestData(Uri url, object state)
			: base( url, state )
		{
		}
		public RequestData(Uri url, object state, byte[] data)
			: base( url, state, data )
		{
		}
		public RequestData(HttpWebRequest request, object state)
			: base( request, state )
		{
		}

		public T Result { get; internal set; }
	}

	public class RequestResult<T>
	{
		public RequestResult()
		{
			this.RequestStart = DateTime.Now;
		}

		internal void Start() { this.RequestStart = DateTime.Now; }
		internal void Stop() { this.RequestEnd = DateTime.Now; }

		public DateTime RequestStart { get; internal set; }
		public DateTime RequestEnd { get; internal set; }
		public TimeSpan RequestTime { get { return this.RequestEnd.Subtract( this.RequestStart ); } }
		public T Result { get; internal set; }
	}

	public partial class AsyncCallCompletedEventArgs<T> : System.ComponentModel.AsyncCompletedEventArgs
	{
		public AsyncCallCompletedEventArgs(T result)
			: base( null, false, null )
		{
			this.Result = result;
		}
		public AsyncCallCompletedEventArgs(T result, object userState)
			: base( null, false, userState )
		{
			this.Result = result;
		}
		public AsyncCallCompletedEventArgs(T result, object userState, Exception error, bool cancelled)
			: base( error, cancelled, userState )
		{
			this.Result = result;
		}

		public T Result { get; private set; }
	}


	//public enum ApiConnectionType { RestService, Database, File }

	public enum WebMessageFormatType
    {
        Json,
        Xml
    }

	public struct HttpMethod
	{
		public const string Create = "POST";
		public const string Update = "PUT";
		public const string Delete = "DELETE";
		public const string Select = "GET";

		public const string Post = "POST";
		public const string Put = "PUT";
		public const string Get = "GET";
		public const string Head = "HEAD";
		public const string Trace = "TRACE";
		public const string Options = "OPTIONS";
	}
}