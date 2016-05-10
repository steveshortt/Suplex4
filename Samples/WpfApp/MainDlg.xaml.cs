using System.Data;
using System.Windows;

using Suplex.Forms;
using Suplex.Forms.ObjectModel.Api;
using Suplex.Wpf;

namespace WpfApp
{
	public partial class MainDlg : Window
	{
		private SuplexStore _splxStore = null;
		private SuplexApiClient _apiClient = null;
		private DataSet _securityCache = null;
		private DataSet _validationCache = null;
		private SecurityLoadParameters _securityLoadParameters = null;
		private bool _isInit = true;

		public MainDlg()
		{
			InitializeComponent();
		}

		private void cmdLoadSuplex_Click(object sender, RoutedEventArgs e)
		{
			this.SetupSuplex_Api();
		}

		private void SetupSuplex_Api()
		{
			grpFoo.Validation.AutoValidateContainer = true;
			//grpFoo.Validation.ValidationSummaryControl = validationSummary1;

			string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

			_securityLoadParameters = new SecurityLoadParameters()
			{
				ExternalGroupInfo = new ExternalGroupInfo( null, true, "Everyone,Power Users" ),
				User = new Suplex.Security.Standard.User( userName, string.Empty )
			};
			_securityLoadParameters.User.Id = "c9e5d922-1a88-4e7b-a9a5-6aaef395cbd0";

			_apiClient = new SuplexApiClient( "http://localhost:10712/SuplexApi.svc", WebMessageFormatType.Json );
			grdTop.Security.Clear( true );
			_securityCache = grdTop.Security.Load( _apiClient, _securityLoadParameters );
			//_validationCache = grpFoo.Validation.Load( _splxStore );

			//base.DumpDiagInfo();

			SplxDiagnosticInfoDlg diagDlg = new SplxDiagnosticInfoDlg();
			diagDlg.SetControl( grdTop );
		}

		private void SetupSuplex_File()
		{
			grpFoo.Validation.AutoValidateContainer = true;
			//grpFoo.Validation.ValidationSummaryControl = validationSummary1;

			string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

			_securityLoadParameters = new SecurityLoadParameters()
			{
				ExternalGroupInfo = new ExternalGroupInfo( null, true, "Everyone,Power Users" ),
				User = new Suplex.Security.Standard.User( userName, string.Empty )
			};

			_splxStore = SuplexApiClient.LoadSuplexFile( "MainDlg.splx" );

			_securityCache = grpFoo.Security.Load( _splxStore, _securityLoadParameters );
			_validationCache = grpFoo.Validation.Load( _splxStore );

			//base.DumpDiagInfo();
		}
	}
}