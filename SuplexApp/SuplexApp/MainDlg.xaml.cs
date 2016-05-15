using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

using api = Suplex.Forms.ObjectModel.Api;



namespace SuplexApp
{
	/// <summary>
	/// Suplex Main Window 
	/// </summary>
	public partial class MainDlg : Window
	{
		#region api/decls
		api.SuplexApiClient _apiClient = new api.SuplexApiClient();
		api.SuplexStore _splxStore = null;

		private Settings _settings = null;
		private RemoteConnectDlg _remoteConnectDlg = null;
		private FileExportDlg _fileExportDlg = null;
		private FileImportDlg _fileImportDlg = null;
		private const string _fileName = "sample.xml";
		#endregion

		public MainDlg()
		{
			InitializeComponent();

			//WpfApplication2.uieWindow w = new WpfApplication2.uieWindow();
			//w.ShowDialog();
			//Application.Current.Shutdown();

			//Application Level
			//Application.Current.ApplyTheme( ThemeManager.GetThemes()[13] );

			this.LoadSettings();

			this.FileNew();

			//if( App.StartUpDocumentIsValid )
			//{
			//	this.OpenFile( App.StartUpDocument );
			//}
			//else if( App.CommandLineArgs.Count > 0 )
			//{
			//	if( App.CommandLineArgs.Keys.Contains( "/config" ) )
			//	{
			//		//placeholder
			//		//this.OpenConfig( App.CommandLineArgs["/config"] );
			//	}
			//	else if( App.CommandLineArgs.Keys.Contains( "/dbserver" ) && App.CommandLineArgs.Keys.Contains( "/dbname" ) )
			//	{
			//		if( App.CommandLineArgs.Keys.Contains( "/dbuser" ) && App.CommandLineArgs.Keys.Contains( "/dbpswd" ) )
			//		{
			//			//placeholder
			//		}
			//	}
			//}
		}

		#region MainDlg handlers, Startup/Shutdown
		private void MainDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if( this.GlobalVerifySaveChanges() )
			{
				//_settings.FileToolbar = tsFile.Location;
				//_settings.ConnectionToolbar = tsConnection.Location;
				//_settings.ExecuteToolbar = tsExecute.Location;
				_settings.Serialize();
			}
			else
			{
				e.Cancel = true;
			}
		}

		private void ToggleView_Click(object sender, RoutedEventArgs e)
		{
			//tcMainPanel.SelectedItem = ( (Hyperlink)sender ).Name == cmdUieTab.Name ? tpUie : tpSec;
		}

		private void LoadSettings()
		{
			_settings = Settings.Deserialize();
			tbbOpenSplxFileStore.DropDownContextMenu.DataContext = _settings.RecentFiles;
			tbbRemoteConnect.DropDownContextMenu.DataContext = _settings.RecentRemoteConnections;

			//tsFile.Location = _settings.FileToolbar;
			//tsConnection.Location = _settings.ConnectionToolbar;
			//tsExecute.Location = _settings.ExecuteToolbar;
		}
		#endregion

		#region File New/Open/Save, Remote Connect/Disconnect

		#region file
		private void tbbNewSplxFileStore_Click(object sender, RoutedEventArgs e)
		{
			bool ok = this.GlobalVerifySaveChanges();
			if( ok )
			{
				this.FileNew();
			}
		}

		private void tbbOpenSplxFileStore_Click(object sender, RoutedEventArgs e)
		{
			bool ok = this.GlobalVerifySaveChanges();
			if( ok )
			{
				OpenFileDialog dlg = new OpenFileDialog();
				dlg.Filter = "Suplex Files|*.splx;*.xml|All Files|*.*";
				if( dlg.ShowDialog( this ) == true )
				{
					this.OpenFile( dlg.FileName );
				}
			}
		}

		private void mnuRecentFile_Click(object sender, RoutedEventArgs e)
		{
			bool ok = this.GlobalVerifySaveChanges();
			if( ok )
			{
				string file = ((MenuItem)e.OriginalSource).Header.ToString();
				if( File.Exists( file ) )
				{
					this.OpenFile( file );
				}
			}
		}

		private void tbbSaveSplxFileStore_Click(object sender, RoutedEventArgs e)
		{
			this.SaveFile();
		}

		private void tbbSaveSplxFileStoreSecure_Click(object sender, RoutedEventArgs e)
		{
			if( _fileExportDlg == null )
			{
				_fileExportDlg = new FileExportDlg();
				_fileExportDlg.Owner = this;
				_fileExportDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			}

			_fileExportDlg.IsForExport = false;
			if( _fileExportDlg.ShowDialog() == true )
			{
				_apiClient.SetFile( _fileExportDlg.FileName );
				if( _fileExportDlg.SignFile )
				{
					_apiClient.SetPublicPrivateKeyFile( _fileExportDlg.KeysFileName );
					_apiClient.PublicPrivateKeyContainerName = _fileExportDlg.KeysContainerName;
				}
				else
				{
					_apiClient.SetPublicPrivateKeyFile( null );
					_apiClient.PublicPrivateKeyContainerName = null;
				}
				this.SaveFile();
			}
		}

		private void tbbSaveAllSplxFileStore_Click(object sender, RoutedEventArgs e)
		{
			uieDlg.SaveIfDirty();
			spDlg.SaveIfDirty();

			if( !_apiClient.IsConnected )
			{
				this.SaveFile();
			}
		}

		private void tbbSaveAsSplxFileStore_Click(object sender, RoutedEventArgs e)
		{
			this.SaveFileAs();
		}

		private void FileNew()
		{
			if( _splxStore == null )
			{
				_splxStore = new api.SuplexStore();
			}

			_splxStore.Clear();
			_apiClient.SetFile( null );
			_splxStore.IsDirty = false;

			uieDlg.ClearContentPanel();
			spDlg.ClearContentPanel();

			this.SetMainDlgDataContext();
		}

		private void OpenFile(string fileName)
		{
			this.FileNew();

			_splxStore = _apiClient.LoadFile( fileName );

			this.SetMainDlgDataContext();

			_settings.AddRecentFile( fileName );	//re-sorts the recent file list

			//_splx.GroupMembership.Resolve();
		}

		private bool SaveFile()
		{
			bool ok = _apiClient.HasFileConnection;
			if( !ok )
			{
				ok = this.SaveFileAs();
			}
			else
			{
				_apiClient.SaveFile( _splxStore );
				ok = true;
			}

			return ok;
		}

		private bool SaveFileAs()
		{
			bool ok = false;
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Suplex File|*.splx|Suplex XML File|*.xml";
			if( dlg.ShowDialog( this ) == true )
			{
				_apiClient.SetFile( dlg.FileName );

				ok = this.SaveFile();
			}

			return ok;
		}
		#endregion

		#region remote
		private void tbbRemoteConnect_Click(object sender, RoutedEventArgs e)
		{
			bool ok = this.GlobalVerifySaveChanges();
			if( ok )
			{
				this.ShowConnectionDialog( null );
			}
		}

		private void mnuRecentConnection_Click(object sender, RoutedEventArgs e)
		{
			DatabaseConnectionData connectionData = ((MenuItem)e.OriginalSource).Header as DatabaseConnectionData;
			if( connectionData == null )
			{
				string url = ((MenuItem)e.OriginalSource).Header as string;
				this.OpenConnection( url );
			}
			else
			{
				if( connectionData.UseSqlCredentials )
				{
					this.ShowConnectionDialog( connectionData );
				}
				else
				{
					this.OpenConnection( connectionData );
				}
			}
		}

		private void ShowConnectionDialog(DatabaseConnectionData connectionData)
		{
			if( _remoteConnectDlg == null )
			{
				_remoteConnectDlg = new RemoteConnectDlg();
				_remoteConnectDlg.Owner = this;
				_remoteConnectDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			}

			bool ok = false;
			if( connectionData == null )
			{
				ok = _remoteConnectDlg.ShowDialog( string.Empty );
			}
			else
			{
				ok = _remoteConnectDlg.ShowDialog( connectionData );
			}

			if( ok )
			{
				if( _remoteConnectDlg.IsServiceConnection )
				{
					this.OpenConnection( _remoteConnectDlg.ServiceUrl );
				}
				else
				{
					this.OpenConnection( _remoteConnectDlg.ConnectionData );
				}
			}
		}

		private void OpenConnection(string url)
		{
			_apiClient.Connect( url, api.WebMessageFormatType.Json );

			_splxStore = _apiClient.GetSuplexStore();
			this.SetMainDlgDataContext();

			_settings.AddRecentServiceConnection( url );
		}

		private void OpenConnection(DatabaseConnectionData connectionData)
		{
			if( connectionData.UseSqlCredentials )
			{
				//SplxStore.Connect( connectionData.Server, connectionData.Database,
				//    connectionData.UserName, connectionData.Password, true );

				_apiClient.Connect( connectionData.Server, connectionData.Database,
					connectionData.UserName, connectionData.Password );
			}
			else
			{
				//SplxStore.Connect( connectionData.Server, connectionData.Database, null, null, true );
				_apiClient.Connect( connectionData.Server, connectionData.Database, null, null );
			}

			_splxStore = _apiClient.GetSuplexStore();
			this.SetMainDlgDataContext();

			_settings.AddRecentDatabaseConnection( connectionData );
		}

		private void tbbRemoteDisconnect_Click(object sender, RoutedEventArgs e)
		{
			_apiClient.Disconnect();
			this.FileNew();
		}

		private void tbbRemoteImport_Click(object sender, RoutedEventArgs e)
		{
			if( _fileImportDlg == null )
			{
				_fileImportDlg = new FileImportDlg();
				_fileImportDlg.Owner = this;
				_fileImportDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			}

			if( _fileImportDlg.ShowDialog() == true )
			{
				api.SuplexStore importStore = new api.SuplexStore();

				bool ok = true;
				if( _fileImportDlg.VerifySignature )
				{
					try
					{
						importStore = _apiClient.LoadFile(
							_fileImportDlg.FileName, _fileImportDlg.KeysFileName, _fileImportDlg.KeysContainerName );
					}
					catch( CryptographicException ex )
					{
						ok = false;
						MessageBox.Show( ex.Message, "Signature Error", MessageBoxButton.OK, MessageBoxImage.Error );
					}
				}
				else
				{
					importStore = _apiClient.LoadFile( _fileImportDlg.FileName );
				}

				if( ok )
				{
					_apiClient.UpsertWholeStore( importStore, _fileImportDlg.ImportValidation, _fileImportDlg.ImportSecurity );
					importStore = null;
					_splxStore = _apiClient.GetSuplexStore();
				}
			}
		}

		private void tbbRemoteRefresh_Click(object sender, RoutedEventArgs e)
		{
			_splxStore = _apiClient.GetSuplexStore();
			this.SetMainDlgDataContext();
		}

		private void tbbRemoteExport_Click(object sender, RoutedEventArgs e)
		{
			if( _fileExportDlg == null )
			{
				_fileExportDlg = new FileExportDlg();
				_fileExportDlg.Owner = this;
				_fileExportDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			}

			_fileExportDlg.IsForExport = true;

			if( _fileExportDlg.ShowDialog() == true )
			{
				api.SuplexStore exportStore =
					_apiClient.GetSuplexStore( _fileExportDlg.ExportValidation, _fileExportDlg.ExportSecurity );

				api.SuplexApiClient exportClient = new api.SuplexApiClient();
				exportClient.SetFile( _fileExportDlg.FileName );
				if( _fileExportDlg.SignFile )
				{
					exportClient.SetPublicPrivateKeyFile( _fileExportDlg.KeysFileName );
					exportClient.PublicPrivateKeyContainerName = _fileExportDlg.KeysContainerName;
				}
				else
				{
					exportClient.SetPublicPrivateKeyFile( null );
					exportClient.PublicPrivateKeyContainerName = null;
				}

				exportClient.SaveFile( exportStore );
				exportClient = null;
				exportStore = null;
			}
		}

		private void tbbCreateRsaKeys_Click(object sender, RoutedEventArgs e)
		{
			CspParameters cspParams = new CspParameters();
			cspParams.KeyContainerName = "XML_DSIG_RSA_KEY";
			RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider( cspParams );
			using( StreamWriter sw = new StreamWriter( "pub_priv.txt" ) )
			{
				sw.Write( rsaKey.ToXmlString( true ) );
			}
			using( StreamWriter sw = new StreamWriter( "pub_only.txt" ) )
			{
				sw.Write( rsaKey.ToXmlString( false ) );
			}
		}
		#endregion

		private bool StoreFileVerifySaveChanges()
		{
			bool ok = false;
			if( !_apiClient.IsConnected && _splxStore.IsDirty )
			{
				MessageBoxResult mbr =
					MessageBox.Show( string.Format( "Save changes to {0}?", _apiClient.HasFileConnection ? _apiClient.File.Name : "Untitled Document" ),
					"Save changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes );

				switch( mbr )
				{
					case MessageBoxResult.Yes:
					{
						ok = this.SaveFile();
						break;
					}
					case MessageBoxResult.No:
					{
						ok = true;
						break;
					}
					case MessageBoxResult.Cancel:
					{
						break;
					}
				}
			}
			else
			{
				//no item to verify or item is not dirty
				ok = true;
			}

			return ok;
		}

		private bool GlobalVerifySaveChanges()
		{
			bool ok = uieDlg.VerifySaveChanges();
			if( ok )
				ok = spDlg.VerifySaveChanges();
			if( ok )
				ok = this.StoreFileVerifySaveChanges();

			return ok;
		}
		#endregion

		#region MainDlg DataContext
		private void SetMainDlgDataContext()
		{
			this.DataContext = _apiClient;

			uieDlg.SplxStore = _splxStore;
			uieDlg.ApiClient = _apiClient;

			spDlg.SplxStore = _splxStore;
			spDlg.ApiClient = _apiClient;
		}
		#endregion




		//TODO: insert some logging
		private void HandleException(Exception ex)
		{
			//MessageBox.Show( this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			throw ex;
		}
	}
}