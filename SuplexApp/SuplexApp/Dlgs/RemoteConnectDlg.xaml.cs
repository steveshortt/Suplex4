using System;
using System.Data;
using System.Reflection;
using System.Windows;

using Suplex.Data;


namespace SuplexApp
{
	public partial class RemoteConnectDlg : Window
	{
		private bool _shuttingDown = false;
		private bool _result = false;

		public RemoteConnectDlg()
		{
			InitializeComponent();

			Application.Current.MainWindow.Closing +=
				new System.ComponentModel.CancelEventHandler( this.MainDlg_Closing );
		}

		public bool ShowDialog(DatabaseConnectionData connectionData)
		{
			_result = false;
			this.ServiceUrl = string.Empty;
			this.ConnectionData = connectionData;
			rbDatabase.IsChecked = true;
			this.ShowDialog();
			return _result;
		}

		public bool ShowDialog(string serviceUrl)
		{
			_result = false;
			this.ServiceUrl = serviceUrl;
			this.ConnectionData = null;
			rbService.IsChecked = true;
			this.ShowDialog();
			return _result;
		}

		public bool IsServiceConnection { get { return rbService.IsChecked.Value; } }

		public string ServiceUrl
		{
			get { return txtServiceUrl.Text; }
			internal set { txtServiceUrl.Text = value; }
		}

		public DatabaseConnectionData ConnectionData
		{
			get { return (DatabaseConnectionData)this.DataContext; }
			internal set { this.DataContext = value == null ? new DatabaseConnectionData() : value; }
		}

		private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			string db = string.Empty;
			if( !string.IsNullOrEmpty( cmbDatabase.Text ) && cmbDatabase.Text.Trim().Length > 0 )
			{
				db = cmbDatabase.Text;
				this.ConnectionData.Database = db;
			}

			if( this.IsServiceConnection )
			{
				cmdConnect.IsEnabled = txtServiceUrl.Text.Trim().Length > 0;
			}
			else
			{
				cmdConnect.IsEnabled = txtServer.Text.Trim().Length > 0 && !string.IsNullOrEmpty( db );
				if( cmdConnect.IsEnabled && chkUseSqlCredentials.IsChecked == true )
				{
					cmdConnect.IsEnabled = txtUserName.Text.Trim().Length > 0 && txtPassword.Password.Length > 0;
				}

				cmdBrowseDatabases.IsEnabled = txtServer.Text.Trim().Length > 0;
				if( cmdBrowseDatabases.IsEnabled && chkUseSqlCredentials.IsChecked == true )
				{
					cmdBrowseDatabases.IsEnabled = txtUserName.Text.Trim().Length > 0 && txtPassword.Password.Length > 0;
				}
			}
		}

		//apparently the PasswordBox.Password prop is not a Dependency Property, so it doesn't support databinding
		private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
		{
			this.ConnectionData.Password = txtPassword.Password;
			this.TextBox_TextChanged( null, null );
		}

		private void txtDatabase_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			this.TextBox_TextChanged( null, null );
		}

		private void txtDatabase_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			string text = null;
			string db = string.Empty;
			DataRowView selValue = cmbDatabase.SelectedValue as DataRowView;
			if( selValue != null )
			{
				text = selValue.Row["name"].ToString();
			}
			if( !string.IsNullOrEmpty( text ) && text.Trim().Length > 0 )
			{
				db = text;
				this.ConnectionData.Database = db;
			}

			cmdConnect.IsEnabled = txtServer.Text.Trim().Length > 0 && !string.IsNullOrEmpty( db );
			if( cmdConnect.IsEnabled && chkUseSqlCredentials.IsChecked == true )
			{
				cmdConnect.IsEnabled = txtUserName.Text.Trim().Length > 0 && txtPassword.Password.Length > 0;
			}
		}

		private void chkUseSqlCredentials_CheckChanged(object sender, RoutedEventArgs e)
		{
			this.TextBox_TextChanged( null, null );
		}

		private void cmdBrowseDatabases_Click(object sender, RoutedEventArgs e)
		{
			ConnectionProperties cp;

			if( chkUseSqlCredentials.IsChecked == true )
			{
				cp = new ConnectionProperties(
					txtServer.Text, string.Empty, txtUserName.Text, txtPassword.Password );
			}
			else
			{
				cp = new ConnectionProperties(
					txtServer.Text, string.Empty );
			}

			DataAccessor da = new DataAccessor( cp.ConnectionString );

			try
			{
				DataSet ds = da.GetDataSet( "select name from master..sysdatabases" );
				DataRow r = ds.Tables[0].NewRow();
				r["name"] = string.Empty;
				ds.Tables[0].Rows.Add( r );
				DataView v = ds.Tables[0].DefaultView;
				v.Sort = "name";
				cmbDatabase.DataContext = v;
				cmbDatabase.DisplayMemberPath = "name";
			}
			catch( Exception ex )
			{
				//cmbDatabases.ComboBox.DataSource = null;
				//this.HandleException( "Error listing databases from sysdatabases", ex );
			}
		}


		private void Connect_Click(object sender, RoutedEventArgs e)
		{
			_result = true;
			if( chkUseSqlCredentials.IsChecked == false )
			{
				txtUserName.Text = string.Empty;
				txtPassword.Password = string.Empty;
			}
			this.Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			_result = false;
			this.Close();
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