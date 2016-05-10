using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SuplexApp
{
	public partial class FileExportDlg : Window
	{
		private bool _shuttingDown = false;
		private bool _result = false;
		SaveFileDialog _saveFileDialog = new SaveFileDialog();
		OpenFileDialog _openFileDialog = new OpenFileDialog();
		private const string __publicPrivateKeyPairFilter = "Public/Private Key Pair File|*.keypair|All Files|*.*";

		private enum FilterOption
		{
			Suplex,
			PublicPrivateKeyPair,
			PublicOnlyKey
		}

		public FileExportDlg()
		{
			InitializeComponent();

			Application.Current.MainWindow.Closing +=
				new System.ComponentModel.CancelEventHandler( this.MainDlg_Closing );
		}

		#region Props
		public string FileName { get { return txtSaveFile.Text; } }
		public bool SignFile { get { return chkSignFile.IsChecked == true; } }
		public string KeysFileName
		{
			get { return txtKeysFile.Text; }
			private set { txtKeysFile.Text = value; }
		}
		public string KeysContainerName
		{
			get { return txtKeysContainerName.Text; }
			private set { txtKeysContainerName.Text = value; }
		}
		public bool HasKeysFileName { get { return !string.IsNullOrEmpty( txtKeysFile.Text ); } }
		public bool HasKeysContainerName { get { return !string.IsNullOrEmpty( txtKeysContainerName.Text ); } }
		public bool IsForExport
		{
			get { return grpExportOptions.Visibility == Visibility.Visible; }
			set { grpExportOptions.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
		}
		public bool ExportValidation { get { return chkExportValidation.IsChecked == true || chkExportAll.IsChecked == true; } }
		public bool ExportSecurity { get { return chkExportSecurity.IsChecked == true || chkExportAll.IsChecked == true; } }

		private string PublicPrivateFileName { get { return txtPubPrivKeys.Text; } }
		private string PublicOnlyFileName { get { return txtPubOnlyKey.Text; } }
		private string PublicPrivateContainerName { get { return txtKeyPairContainerName.Text; } }
		#endregion

		private void cmdBrowseSaveFile_Click(object sender, RoutedEventArgs e)
		{
			this.BrowseSaveFileAs( txtSaveFile, FilterOption.Suplex );
		}

		private void cmdBrowseKeysFile_Click(object sender, RoutedEventArgs e)
		{
			_openFileDialog.Filter = __publicPrivateKeyPairFilter;
			if( _openFileDialog.ShowDialog( this ) == true )
			{
				txtKeysFile.Text = _openFileDialog.FileName;
			}
		}

		private void cmdBrowsePubPriv_Click(object sender, RoutedEventArgs e)
		{
			this.BrowseSaveFileAs( txtPubPrivKeys, FilterOption.PublicPrivateKeyPair );
		}

		private void cmdBrowsePubOnly_Click(object sender, RoutedEventArgs e)
		{
			this.BrowseSaveFileAs( txtPubOnlyKey, FilterOption.PublicOnlyKey );
		}

		private void cmdGenerateKeys_Click(object sender, RoutedEventArgs e)
		{
			if( !string.IsNullOrEmpty( this.PublicPrivateContainerName ) )
			{
				CspParameters cspParams = new CspParameters();
				cspParams.KeyContainerName = this.PublicPrivateContainerName;
				RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider( cspParams );

				if( !string.IsNullOrEmpty( this.PublicPrivateFileName ) )
				{
					using( StreamWriter sw = new StreamWriter( this.PublicPrivateFileName ) )
					{
						sw.Write( rsaKey.ToXmlString( true ) );
					}

					if( !this.HasKeysContainerName )
					{
						this.KeysContainerName = this.PublicPrivateContainerName;
					}

					if( !this.HasKeysFileName )
					{
						this.KeysFileName = this.PublicPrivateFileName;
					}
				}

				if( !string.IsNullOrEmpty( this.PublicOnlyFileName ) )
				{
					using( StreamWriter sw = new StreamWriter( this.PublicOnlyFileName ) )
					{
						sw.Write( rsaKey.ToXmlString( false ) );
					}
				}
			}
		}

		private bool BrowseSaveFileAs(TextBox textBox, FilterOption filterOption)
		{
			bool ok = false;

			switch( filterOption )
			{
				case FilterOption.Suplex:
				{
					_saveFileDialog.Filter = "Suplex File|*.splx|Suplex XML File|*.xml|All Files|*.*";
					break;
				}
				case FilterOption.PublicPrivateKeyPair:
				{
					_saveFileDialog.Filter = __publicPrivateKeyPairFilter;
					break;
				}
				case FilterOption.PublicOnlyKey:
				{
					_saveFileDialog.Filter = "Public Key Only File|*.pubkey|All Files|*.*";
					break;
				}
			}

			_saveFileDialog.FileName = string.Empty;
			if( _saveFileDialog.ShowDialog( this ) == true )
			{
				textBox.Text = _saveFileDialog.FileName;

				ok = true;
			}

			return ok;
		}

		public new bool? ShowDialog()
		{
			_result = false;
			base.ShowDialog();
			return _result;
		}

		private void cmdSave_Click(object sender, RoutedEventArgs e)
		{
			_result = true;
			this.Close();
		}

		private void cmdCancel_Click(object sender, RoutedEventArgs e)
		{
			_result = false;
			this.Close();
		}

		private void Something_CheckChanged(object sender, RoutedEventArgs e)
		{
			this.Something_Changed( null, null );
		}

		private void Something_Changed(object sender, TextChangedEventArgs e)
		{
			cmdSave.IsEnabled = txtSaveFile.Text.Length > 0;
			if( cmdSave.IsEnabled && chkSignFile.IsChecked == true )
			{
				cmdSave.IsEnabled = txtKeysFile.Text.Length > 0 && txtKeysContainerName.Text.Length > 0;
			}
		}


		//when using a single instance of a Window, the instance is unusable after calling .Close()
		//the MainDlg_Closing and Window_Closing methods below are a work-around for this
		private void MainDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_shuttingDown = true;
			this.Close();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if( !_shuttingDown )
			{
				typeof( Window ).GetField( "_isClosing", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( this, false );
				e.Cancel = true;
				this.Hide();
			}
		}
	}
}