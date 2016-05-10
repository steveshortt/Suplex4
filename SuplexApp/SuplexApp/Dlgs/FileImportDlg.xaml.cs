using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SuplexApp
{
	public partial class FileImportDlg : Window
	{
		private bool _shuttingDown = false;
		private bool _result = false;
		private OpenFileDialog _openFileDialog = new OpenFileDialog();
		private const string __keyFilter = "Public Key Only File|*.pubkey|Public/Private Key Pair File|*.keypair|All Files|*.*";
		private const string __suplexFileFilter = "Suplex File|*.splx|Suplex XML File|*.xml|All Files|*.*";

		public FileImportDlg()
		{
			InitializeComponent();

			Application.Current.MainWindow.Closing +=
				new System.ComponentModel.CancelEventHandler( this.MainDlg_Closing );
		}

		#region Props
		public string FileName { get { return txtOpenFile.Text; } }
		public bool VerifySignature { get { return chkVerifySignature.IsChecked == true; } }
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
		public bool IsForImport
		{
			get { return grpImportOptions.Visibility == Visibility.Visible; }
			set { grpImportOptions.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
		}
		public bool ImportValidation { get { return chkImportValidation.IsChecked == true || chkImportAll.IsChecked == true; } }
		public bool ImportSecurity { get { return chkImportSecurity.IsChecked == true || chkImportAll.IsChecked == true; } }
		#endregion

		private void cmdOpenSaveFile_Click(object sender, RoutedEventArgs e)
		{
			_openFileDialog.Filter = __suplexFileFilter;
			if( _openFileDialog.ShowDialog( this ) == true )
			{
				txtOpenFile.Text = _openFileDialog.FileName;
			}
		}

		private void cmdBrowseKeysFile_Click(object sender, RoutedEventArgs e)
		{
			_openFileDialog.Filter = __keyFilter;
			if( _openFileDialog.ShowDialog( this ) == true )
			{
				txtKeysFile.Text = _openFileDialog.FileName;
			}
		}

		public new bool? ShowDialog()
		{
			_result = false;
			base.ShowDialog();
			return _result;
		}

		private void cmdOpen_Click(object sender, RoutedEventArgs e)
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
			cmdOpen.IsEnabled = txtOpenFile.Text.Length > 0;
			if( cmdOpen.IsEnabled && chkVerifySignature.IsChecked == true )
			{
				cmdOpen.IsEnabled = txtKeysFile.Text.Length > 0 && txtKeysContainerName.Text.Length > 0;
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