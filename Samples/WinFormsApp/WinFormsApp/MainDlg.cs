using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Suplex.WinForms;
using Suplex.Forms;
using om = Suplex.Forms.ObjectModel;

namespace WinFormsApp
{
	public partial class MainDlg : sForm
	{
		private om.SuplexStore _splxStore = null;
		private DataSet _securityCache = null;
		private DataSet _validationCache = null;
		private SecurityLoadParameters _securityLoadParameters = null;
		private bool _isInit = true;

		public MainDlg()
		{
			InitializeComponent();

			rmFoo.RecordMode = RecordMode.Insert;
			cmbRecordMode.DataSource = Enum.GetValues( typeof( RecordMode ) );

			this.SetupSuplex();

			_isInit = false;
		}

		private void SetupSuplex()
		{
			this.Validation.AutoValidateContainer = true;
			this.Validation.ValidationSummaryControl = validationSummary1;

			string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

			_securityLoadParameters = new SecurityLoadParameters()
			{
				ExternalGroupInfo = new ExternalGroupInfo( null, true, "Everyone,Power Users" ),
				User = new Suplex.Security.Standard.User( userName, string.Empty )
			};

			_splxStore = om.SuplexStore.LoadSuplexFile( "MainDlg.splx" );

			_securityCache = this.Security.Load( _splxStore, _securityLoadParameters );
			_validationCache = this.Validation.Load( _splxStore );

			base.DumpDiagInfo();
		}

		private void cmbRecordMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			if( !_isInit )
			{
				rmFoo.RecordMode = (RecordMode)cmbRecordMode.SelectedItem;
				this.Security.Clear( true );
				this.Security.Load( _securityCache, _securityLoadParameters );
			}
		}

		private void cmdEdit_Click(object sender, EventArgs e)
		{
			this.ProcessValidate( true );
		}
	}
}