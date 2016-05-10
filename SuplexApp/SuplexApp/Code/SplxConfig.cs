using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Suplex.General;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace SuplexApp
{
	public class Settings
	{
		public Settings()
		{
			this.RecentFiles = new ObservableCollection<string>();
			this.RecentServiceConnections = new ObservableCollection<string>();
			this.RecentDatabaseConnections = new ObservableCollection<DatabaseConnectionData>();

			CollectionContainer rsc = new CollectionContainer() { Collection = this.RecentServiceConnections };
			CollectionContainer rdc = new CollectionContainer() { Collection = this.RecentDatabaseConnections };

			this.RecentRemoteConnections = new CompositeCollection();
			this.RecentRemoteConnections.Add( rsc );
			this.RecentRemoteConnections.Add( rdc );
		}

		public ObservableCollection<string> RecentFiles { get; set; }
		public ObservableCollection<string> RecentServiceConnections { get; set; }
		public ObservableCollection<DatabaseConnectionData> RecentDatabaseConnections { get; set; }
		//public System.Drawing.Point FileToolbar { get; set; }
		//public System.Drawing.Point DatabaseToolbar { get; set; }
		//public System.Drawing.Point ViewToolbar { get; set; }

		[XmlIgnore()]
		public CompositeCollection RecentRemoteConnections { get; private set; }

		[XmlIgnore()]
		private static string FileName { get { return string.Format( @"{0}\SuplexAdmin_Settings.txt",
			Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location ) ); } }


		public void AddRecentFile(string file)
		{
			if( !string.IsNullOrEmpty( file ) )
			{
				for( int i = this.RecentFiles.Count - 1; i >= 0; i-- )
				{
					if( this.RecentFiles[i].ToString() == file )
					{
						this.RecentFiles.Remove( file );
						break;
					}
				}
				this.RecentFiles.Insert( 0, file );
			}
		}

		public void AddRecentServiceConnection(string url)
		{
			if( !string.IsNullOrEmpty( url ) )
			{
				for( int i = this.RecentServiceConnections.Count - 1; i >= 0; i-- )
				{
					if( this.RecentServiceConnections[i].ToString() == url )
					{
						this.RecentServiceConnections.Remove( url );
						break;
					}
				}
				this.RecentServiceConnections.Insert( 0, url );
			}
		}

		public void AddRecentDatabaseConnection(string server, string db, string username)
		{
			if( !string.IsNullOrEmpty( server ) )
			{
				DatabaseConnectionData cd = new DatabaseConnectionData()
				{
					Server = server,
					Database = db
				};
				if( !string.IsNullOrEmpty( username ) )
				{
					cd.UserName = username;
				}

				this.AddRecentDatabaseConnection( cd );
			}
		}
		public void AddRecentDatabaseConnection(DatabaseConnectionData connectionData)
		{
			for( int i = this.RecentDatabaseConnections.Count - 1; i >= 0; i-- )
			{
				if( this.RecentDatabaseConnections[i].ToString() == connectionData.ToString() )
				{
					this.RecentDatabaseConnections.RemoveAt( i );
					break;
				}
			}
			this.RecentDatabaseConnections.Insert( 0, connectionData );
		}

		public void Serialize()
		{
			XmlUtils.Serialize<Settings>( this, FileName );
		}

		public static Settings Deserialize()
		{
			if( File.Exists( FileName ) )
			{
				return XmlUtils.Deserialize<Settings>( FileName );
			}
			else
			{
				return new Settings();
			}
		}
	}

	public class DatabaseConnectionData : INotifyPropertyChanged
	{
		private string _server;
		private string _database;
		private string _username;
		private string _password;

		[XmlAttribute()]
		public string Server
		{
			get { return _server; }
			set
			{
				if( value != _server )
				{
					_server = value;
					this.OnPropertyChanged( "Server" );
				}
			}
		}

		[XmlAttribute()]
		public string Database
		{
			get { return _database; }
			set
			{
				if( value != _database )
				{
					_database = value;
					this.OnPropertyChanged( "Database" );
				}
			}
		}

		[XmlAttribute()]
		public string UserName
		{
			get { return _username; }
			set
			{
				if( value != _username )
				{
					_username = value;
					this.OnPropertyChanged( "UserName" );
				}
			}
		}

		[XmlIgnore()]
		public string Password
		{
			get { return _password; }
			set
			{
				if( value != _password )
				{
					_password = value;
					this.OnPropertyChanged( "Password" );
				}
			}
		}

		[XmlIgnore()]
		public bool UseSqlCredentials { get { return !string.IsNullOrEmpty( this.UserName ); } }

		public override string ToString()
		{
			string username = string.Empty;
			if( this.UseSqlCredentials )
			{
				username = string.Format( " [{0}]", this.UserName );
			}
			return string.Format( @"{0} :: {1}{2}", this.Server, this.Database, username );
		}

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
}