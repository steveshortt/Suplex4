using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;


namespace Suplex.Data
{

	/// <summary>
	/// Prompts for database connection properties.
	/// </summary>
	public class ConnectionPropertiesDlg : System.Windows.Forms.Form
	{
		private DialogResult _dialogResult = DialogResult.None;
		private ConnectionProperties _cp = null;
		private string _connectionString = null;

		private Control _databaseServerCtl = null;

		private bool _databaseServerNameOk = false;
		private bool _userNameOk = false;
		private bool _passwordOk = false;
		private bool _databaseNameOk = false;
		//private bool _sqlDmoOk = false;

		private ToolTip t = new ToolTip();

		private System.Windows.Forms.RadioButton rdoNTSecurity;
		private System.Windows.Forms.TextBox txtUserName;
		private System.Windows.Forms.RadioButton rdoSQLSecurity;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.TextBox txtDatabaseServerName;
		private TypeAheadComboBox cmbDatabaseServerName;
		private TypeAheadComboBox cmbDatabaseName;
		private System.Windows.Forms.Label lblDatabaseServerName;
		private System.Windows.Forms.Label lblDatabaseName;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.StatusBarPanel sbPanel;
		private System.Windows.Forms.StatusBarPanel sbOk;
		private System.Windows.Forms.StatusBarPanel sbFail;
		private System.ComponentModel.IContainer components;


		#region Constructors/Destructor

		public ConnectionPropertiesDlg()
		{
			InitializeComponent(); // Required for Windows Form Designer support

			InitDatabaseServers();
		}


		public ConnectionPropertiesDlg(string databaseServerName, string databaseName)
		{
			InitializeComponent(); // Required for Windows Form Designer support

			InitDatabaseServers();

			this.DatabaseServerName = databaseServerName;
			this.DatabaseName = databaseName;
		}


		public ConnectionPropertiesDlg(string databaseServerName, string username, string password, string databaseName)
		{
			InitializeComponent(); // Required for Windows Form Designer support

			InitDatabaseServers();

			this.DatabaseServerName = databaseServerName;
			this.UserName = username;
			this.Password = password;
			this.DatabaseName = databaseName;
		}


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#endregion


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ConnectionPropertiesDlg));
			this.txtDatabaseServerName = new System.Windows.Forms.TextBox();
			this.rdoNTSecurity = new System.Windows.Forms.RadioButton();
			this.txtUserName = new System.Windows.Forms.TextBox();
			this.rdoSQLSecurity = new System.Windows.Forms.RadioButton();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.lblDatabaseServerName = new System.Windows.Forms.Label();
			this.lblDatabaseName = new System.Windows.Forms.Label();
			this.cmdOK = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.cmbDatabaseServerName = new TypeAheadComboBox();
			this.cmbDatabaseName = new TypeAheadComboBox();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.sbPanel = new System.Windows.Forms.StatusBarPanel();
			this.sbOk = new System.Windows.Forms.StatusBarPanel();
			this.sbFail = new System.Windows.Forms.StatusBarPanel();
			((System.ComponentModel.ISupportInitialize)(this.sbPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sbOk)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sbFail)).BeginInit();
			this.SuspendLayout();
			// 
			// txtDatabaseServerName
			// 
			this.txtDatabaseServerName.Location = new System.Drawing.Point(24, 32);
			this.txtDatabaseServerName.Name = "txtDatabaseServerName";
			this.txtDatabaseServerName.Size = new System.Drawing.Size(152, 20);
			this.txtDatabaseServerName.TabIndex = 1;
			this.txtDatabaseServerName.Text = "";
			this.txtDatabaseServerName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DatabaseServerCtl_KeyPress);
			this.txtDatabaseServerName.TextChanged += new System.EventHandler(this.RequiredInfo_Changed);
			this.txtDatabaseServerName.Enter += new System.EventHandler(this.TextBox_Enter);
			// 
			// rdoNTSecurity
			// 
			this.rdoNTSecurity.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.rdoNTSecurity.Location = new System.Drawing.Point(24, 64);
			this.rdoNTSecurity.Name = "rdoNTSecurity";
			this.rdoNTSecurity.Size = new System.Drawing.Size(168, 24);
			this.rdoNTSecurity.TabIndex = 3;
			this.rdoNTSecurity.Text = "&Windows Integrated Security";
			this.rdoNTSecurity.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.rdoNTSecurity_KeyPress);
			this.rdoNTSecurity.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
			// 
			// txtUserName
			// 
			this.txtUserName.Location = new System.Drawing.Point(40, 112);
			this.txtUserName.Name = "txtUserName";
			this.txtUserName.Size = new System.Drawing.Size(136, 20);
			this.txtUserName.TabIndex = 5;
			this.txtUserName.Text = "";
			this.txtUserName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtUserName_KeyPress);
			this.txtUserName.TextChanged += new System.EventHandler(this.UsernamePswd_TextChanged);
			this.txtUserName.Enter += new System.EventHandler(this.TextBox_Enter);
			// 
			// rdoSQLSecurity
			// 
			this.rdoSQLSecurity.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.rdoSQLSecurity.Location = new System.Drawing.Point(24, 88);
			this.rdoSQLSecurity.Name = "rdoSQLSecurity";
			this.rdoSQLSecurity.Size = new System.Drawing.Size(168, 24);
			this.rdoSQLSecurity.TabIndex = 4;
			this.rdoSQLSecurity.Text = "&User Name and Password:";
			this.rdoSQLSecurity.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
			// 
			// txtPassword
			// 
			this.txtPassword.Location = new System.Drawing.Point(40, 136);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '*';
			this.txtPassword.Size = new System.Drawing.Size(136, 20);
			this.txtPassword.TabIndex = 6;
			this.txtPassword.Text = "";
			this.txtPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPassword_KeyPress);
			this.txtPassword.TextChanged += new System.EventHandler(this.UsernamePswd_TextChanged);
			this.txtPassword.Enter += new System.EventHandler(this.TextBox_Enter);
			// 
			// lblDatabaseServerName
			// 
			this.lblDatabaseServerName.Location = new System.Drawing.Point(22, 16);
			this.lblDatabaseServerName.Name = "lblDatabaseServerName";
			this.lblDatabaseServerName.Size = new System.Drawing.Size(160, 16);
			this.lblDatabaseServerName.TabIndex = 0;
			this.lblDatabaseServerName.Text = "Database &Server Name:";
			// 
			// lblDatabaseName
			// 
			this.lblDatabaseName.Location = new System.Drawing.Point(22, 176);
			this.lblDatabaseName.Name = "lblDatabaseName";
			this.lblDatabaseName.Size = new System.Drawing.Size(154, 16);
			this.lblDatabaseName.TabIndex = 7;
			this.lblDatabaseName.Text = "&Database Name:";
			// 
			// cmdOK
			// 
			this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOK.Enabled = false;
			this.cmdOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdOK.Location = new System.Drawing.Point(24, 232);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.TabIndex = 9;
			this.cmdOK.Text = "&OK";
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// cmdCancel
			// 
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdCancel.Location = new System.Drawing.Point(104, 232);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.TabIndex = 10;
			this.cmdCancel.Text = "&Cancel";
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// cmbDatabaseServerName
			// 
			this.cmbDatabaseServerName.Location = new System.Drawing.Point(24, 32);
			this.cmbDatabaseServerName.Name = "cmbDatabaseServerName";
			this.cmbDatabaseServerName.Size = new System.Drawing.Size(152, 21);
			this.cmbDatabaseServerName.TabIndex = 2;
			this.cmbDatabaseServerName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DatabaseServerCtl_KeyPress);
			this.cmbDatabaseServerName.SelectedIndexChanged += new System.EventHandler(this.cmbDatabaseServerName_SelectedIndexChanged);
			// 
			// cmbDatabaseName
			// 
			this.cmbDatabaseName.Location = new System.Drawing.Point(24, 192);
			this.cmbDatabaseName.Name = "cmbDatabaseName";
			this.cmbDatabaseName.Size = new System.Drawing.Size(152, 21);
			this.cmbDatabaseName.TabIndex = 8;
			this.cmbDatabaseName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmbDatabaseName_KeyPress);
			this.cmbDatabaseName.TextChanged += new System.EventHandler(this.RequiredInfo_Changed);
			this.cmbDatabaseName.SelectedIndexChanged += new System.EventHandler(this.cmbDatabaseName_SelectedIndexChanged);
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 271);
			this.statusBar.Name = "statusBar";
			this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																						 this.sbPanel,
																						 this.sbOk,
																						 this.sbFail});
			this.statusBar.ShowPanels = true;
			this.statusBar.Size = new System.Drawing.Size(202, 22);
			this.statusBar.SizingGrip = false;
			this.statusBar.TabIndex = 11;
			this.statusBar.Text = "statusBar1";
			// 
			// sbPanel
			// 
			this.sbPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.sbPanel.Text = "Ready";
			this.sbPanel.Width = 202;
			// 
			// sbOk
			// 
			this.sbOk.Icon = ((System.Drawing.Icon)(resources.GetObject("sbOk.Icon")));
			this.sbOk.MinWidth = 0;
			this.sbOk.Width = 0;
			// 
			// sbFail
			// 
			this.sbFail.Icon = ((System.Drawing.Icon)(resources.GetObject("sbFail.Icon")));
			this.sbFail.MinWidth = 0;
			this.sbFail.Width = 0;
			// 
			// ConnectionPropertiesDlg
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(202, 293);
			this.Controls.Add(this.statusBar);
			this.Controls.Add(this.cmbDatabaseName);
			this.Controls.Add(this.cmbDatabaseServerName);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdOK);
			this.Controls.Add(this.lblDatabaseName);
			this.Controls.Add(this.lblDatabaseServerName);
			this.Controls.Add(this.txtPassword);
			this.Controls.Add(this.txtUserName);
			this.Controls.Add(this.txtDatabaseServerName);
			this.Controls.Add(this.rdoSQLSecurity);
			this.Controls.Add(this.rdoNTSecurity);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ConnectionPropertiesDlg";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Connection Properties";
			((System.ComponentModel.ISupportInitialize)(this.sbPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sbOk)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sbFail)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion


		#region Events
		public event System.EventHandler ConnectionPropertiesOk;

		protected void OnConnectionPropertiesOk(object sender, System.EventArgs e)
		{
			if(ConnectionPropertiesOk != null)
			{
				ConnectionPropertiesOk(sender, e);
			}
		}
		#endregion


		#region Public Properties/Methods

		new public DialogResult ShowDialog()
		{
			base.ShowDialog();

			return _dialogResult;
		}


		new public DialogResult ShowDialog(IWin32Window owner)
		{
			base.ShowDialog( owner );

			return _dialogResult;
		}


		public string DatabaseServerName
		{
			get
			{
				return _databaseServerCtl.Text;
			}
			set
			{
				_databaseServerCtl.Text = value;
			}
		}


		public string UserName
		{
			get
			{
				return txtUserName.Text;
			}
			set
			{
				txtUserName.Text = value;
			}
		}


		public string Password
		{
			get
			{
				return txtPassword.Text;
			}
			set
			{
				txtPassword.Text = value;
			}
		}


		public string DatabaseName
		{
			get
			{
				return cmbDatabaseName.Text;
			}
			set
			{
				cmbDatabaseName.Text = value;
			}
		}


		public string ConnectionString
		{
			get
			{
				return _connectionString;
			}
		}


		#endregion


		#region EventHandlers/Fucntions

		private void cmbDatabaseServerName_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			_databaseServerNameOk = CheckText( cmbDatabaseServerName.Text );
			
			EnumerateDatabases();
		}


		private void DatabaseServerCtl_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			_databaseServerNameOk = CheckText( this.DatabaseServerName );

			if( e.KeyChar == (char)Keys.Enter )
			{
				if( rdoNTSecurity.Checked )
				{
					cmbDatabaseName.Focus();
				}
				else if( rdoSQLSecurity.Checked )
				{
					txtUserName.Focus();
				}
				else
				{
					rdoNTSecurity.Focus();
				}

				EnumerateDatabases();

				e.Handled = true;
			}
		}


		private void rdo_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdoSQLSecurity.Checked )
			{
				_userNameOk = CheckText( txtUserName.Text );
				_passwordOk = CheckText( txtPassword.Text );

				txtUserName.Enabled = true;
				txtPassword.Enabled = true;
				txtUserName.Focus();
			}
			else
			{
				txtUserName.Enabled = false;
				txtPassword.Enabled = false;

				_userNameOk = true;
				_passwordOk = true;

				this.Refresh();
			}

			RequiredInfo_Changed( null, null );
			EnumerateDatabases();
		}


		private void rdoNTSecurity_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if( e.KeyChar == (char)Keys.Enter )
			{
				cmbDatabaseName.Focus();

				EnumerateDatabases();

				e.Handled = true;
			}
		}


		private void txtUserName_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			_userNameOk = CheckText( txtUserName.Text );

			if( e.KeyChar == (char)Keys.Enter )
			{
				if( _passwordOk )
				{
					EnumerateDatabases();
				}
				else
				{
					txtPassword.Focus();
				}

				e.Handled = true;
			}
		}


		private void txtPassword_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			_passwordOk = CheckText( txtPassword.Text );

			if( e.KeyChar == (char)Keys.Enter )
			{
				e.Handled = true;

				EnumerateDatabases();
			}
		}


		private void UsernamePswd_TextChanged(object sender, System.EventArgs e)
		{
			rdoSQLSecurity.Checked = true;
		}


		private void cmbDatabaseName_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			_databaseNameOk = CheckText( cmbDatabaseName.Text );
		
			RequiredInfo_Changed( null, null );
		}


		private void cmbDatabaseName_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			_databaseNameOk = CheckText( cmbDatabaseName.Text );

			if( e.KeyChar == (char)Keys.Enter )
			{
				e.Handled = true;
				if( cmdOK.Enabled )
				{
					cmdOK_Click( this, EventArgs.Empty );
				}
			}
		}


		private void RequiredInfo_Changed(object sender, System.EventArgs e)
		{
			if( _databaseServerNameOk && _databaseNameOk &&
				( rdoNTSecurity.Checked || rdoSQLSecurity.Checked ) )
			{
				cmdOK.Enabled = true;
			}
			else
			{
				cmdOK.Enabled = false;
			}
		}


		private void TextBox_Enter(object sender, System.EventArgs e)
		{
			((TextBox)sender).SelectAll();
		}


		private void cmdOK_Click(object sender, System.EventArgs e)
		{
			_dialogResult = DialogResult.OK;

			if( rdoNTSecurity.Checked )
			{
				_cp = new ConnectionProperties(
					this.DatabaseServerName, this.DatabaseName );

				_connectionString = _cp.ConnectionString;
			}
			else
			{
				_cp = new ConnectionProperties(
					this.DatabaseServerName, this.DatabaseName, this.UserName, this.Password );

				_connectionString = _cp.ConnectionString;
			}

			OnConnectionPropertiesOk( this, EventArgs.Empty );

			this.Close();
		}


		private void cmdCancel_Click(object sender, System.EventArgs e)
		{
			_dialogResult = DialogResult.Cancel;
			this.Close();
		}



		private void InitDatabaseServers()
		{
			bool gotList = false;

			try
			{
				gotList = EnumerateDatabaseServers();
			}
			catch(Exception ex)
			{ 
				System.Diagnostics.Debug.WriteLine(ex.Message); 
			}

			if( gotList )
			{
				cmbDatabaseServerName.Visible = true;
				txtDatabaseServerName.Visible = false;

				_databaseServerCtl = cmbDatabaseServerName;
			}
			else
			{
				cmbDatabaseServerName.Visible = false;
				txtDatabaseServerName.Visible = true;

				txtDatabaseServerName.Text = "(local)";
				_databaseServerCtl = txtDatabaseServerName;
				_databaseServerNameOk = true;
			}
		}


		/*
		 * See: http://msdn2.microsoft.com/en-us/library/system.data.sql.sqldatasourceenumerator.getdatasources.aspx
		 * SqlDataSourceEnumerator.Instance.GetDataSources() exposes a datatable with the following columns:
		 * 
		 * ServerName:		Name of the server.
		 * InstanceName:	Name of the server instance. Blank if the server is running as the default instance.
		 * IsClustered:		Indicates whether the server is part of a cluster.
		 * Version:			Version of the server (8.00.x for SQL Server 2000, and 9.00.x for SQL Server 2005).
		 * 
		 */
		private bool EnumerateDatabaseServers()
		{
			bool gotList = false;

			SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
			DataTable dataSources = instance.GetDataSources();

			if( dataSources.Rows.Count > 0 )
			{
			    foreach(DataRow r in dataSources.Rows)
			    {
					cmbDatabaseServerName.Items.Add( r["ServerName"] );
			    }

			    gotList = true;
			}

			return gotList;
		}

		[Obsolete( "Use EnumerateDatabaseServers() instead.", true )]
		private bool EnumerateDatabaseServers_DMO()
		{
			bool gotList = false;

			//SQLDMO.Application sql = new SQLDMO.ApplicationClass();
			//SQLDMO.NameList names = sql.ListAvailableSQLServers();

			//if( names.Count > 0 )
			//{
			//    for( int n=1; n<names.Count+1; n++ )
			//    {
			//        cmbDatabaseServerName.Items.Add( names.Item(n) );
			//    }

			//    gotList = true;
			//}

			//_sqlDmoOk = true;

			return gotList;
		}

		private void EnumerateDatabases()
		{
			if( _databaseServerNameOk && _userNameOk && _passwordOk )
			{

				ConnectionProperties cp = new ConnectionProperties( this.DatabaseServerName, null, this.UserName, this.Password );
				string cn = null;
				string sql = "SELECT Name FROM master.dbo.sysdatabases WHERE " +
					"DATABASEPROPERTY(name, N'IsDetached') = 0 AND has_dbaccess(name) = 1";

				cmbDatabaseName.Items.Clear();
				cmbDatabaseName.Text = "";
				cmdOK.Enabled = false;

				sbPanel.Icon = this.Icon;
				sbPanel.Text = "Enumerating databases...";
				this.Refresh();
				this.Cursor = Cursors.WaitCursor;

				cn = cp.BuildIntegratedString();
				if( rdoSQLSecurity.Checked )
				{
					cn = cp.BuildProprietaryString();
				}

				DataAccessor da = new DataAccessor( cn );
				
				try
				{
					DataSet ds = da.GetDataSet( sql );

					sbPanel.Icon = null;
				
					if( ds != null )
					{
						DataRowCollection rows = ds.Tables[0].Rows;

						for( int n=0; n<rows.Count; n++ )
						{
							cmbDatabaseName.Items.Add( rows[n]["name"].ToString() );
						}

						sbPanel.Text = "Ready";
						sbPanel.Icon = sbOk.Icon;
						cmbDatabaseName.Focus();

						t.SetToolTip( statusBar, "Ready" );
					}
					else
					{
						sbPanel.Text = "Database enumeration failed.";
						sbPanel.Icon = sbFail.Icon;

						t.SetToolTip( statusBar, "Database enumeration failed." );
					}
				}
				catch(SqlException sqlex)
				{
					sbPanel.Text = "Database enumeration failed.";
					sbPanel.Icon = sbFail.Icon;

					t.SetToolTip( statusBar, sqlex.Message );
				}

				this.Cursor = Cursors.Default;

			}
		}


		private bool CheckText(string text)
		{
			return text != null && text.Length > 0;
		}


		#endregion




		#region not in use

		/// <summary>
		/// Uses DMO to enumerate databases, not in use.
		/// </summary>
		[Obsolete( "Use EnumerateDatabases() instead.", true )]
		private void EnumerateDatabases_DMO()
		{
			//string name = null;
			//string pswd = null;

			//SQLDMO.SQLServer srv = new SQLDMO.SQLServerClass();

			//srv.LoginSecure = true;
			//if( rdoSQLSecurity.Checked )
			//{
			//    srv.LoginSecure = false;
			//    name = UserName;
			//    pswd = Password;
			//}

			//srv.Connect( this.DatabaseServerName, name, pswd );

			//for( int n=1; n<srv.Databases.Count; n++ )
			//{
			//    cmbDatabaseName.Items.Add( srv.Databases.ItemByID(n).Name );
			//}

			//cmbDatabaseName.Focus();
		}


		#endregion

	}



	internal class TypeAheadComboBox : System.Windows.Forms.ComboBox
	{
		private bool _displayActiveList = false;
		private bool _restrictListItems = false;

		public TypeAheadComboBox() : base() {}


		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if( !Char.IsControl(e.KeyChar) )
			{
				this.DroppedDown = _displayActiveList;
				string searchText = this.Text.Substring( 0, this.SelectionStart ) + e.KeyChar;
				int i = this.FindString( searchText );

				if( i > -1 )
				{
					e.Handled = true;
					this.SelectedIndex = i;
					this.SelectionStart = searchText.Length;
					this.SelectionLength = this.Text.Length - this.SelectionStart;
				}
			}

			base.OnKeyPress (e);
		}


		protected override void OnValidating(System.ComponentModel.CancelEventArgs e)
		{
			if( _restrictListItems )
			{
				e.Cancel = !this.ItemIsInList;
			}

			base.OnValidating (e);
		}




		[System.ComponentModel.DefaultValue(false)]
		public bool DisplayActiveList
		{
			get
			{
				return _displayActiveList;
			}
			set
			{
				_displayActiveList = value;
			}
		}


		[System.ComponentModel.DefaultValue(false)]
		public bool RestrictListItems
		{
			get
			{
				return _restrictListItems;
			}
			set
			{
				_restrictListItems = value;
			}
		}


		[System.ComponentModel.Browsable(false)]
		public bool ItemIsInList
		{
			get
			{
				return this.FindString( this.Text ) > -1;
			}
		}
	}



}