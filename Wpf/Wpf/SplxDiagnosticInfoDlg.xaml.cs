using System.Windows;
using Suplex.Forms;
using System.IO;

namespace Suplex.Wpf
{
	/// <summary>
	/// Interaction logic for SplxDiagnosticInfoDlg.xaml
	/// </summary>
	public partial class SplxDiagnosticInfoDlg : Window
	{
		public SplxDiagnosticInfoDlg()
		{
			InitializeComponent();
		}

		public void SetControl(ISecureControl control)
		{
			SecureControlUtils scu = new SecureControlUtils();
			DiagInfoStreams s = scu.DumpSecurity( control, true, false );

			securityText.Text = scu.DumpHierarchy( control, false );
			securityText.Text += s.Text;
			securityHtml.NavigateToString( s.Html.ToString() );

			if( control is IValidationControl )
			{
				ValidationControlUtils vcu = new ValidationControlUtils();
				DiagInfoStreams v = vcu.DumpValidation( (IValidationControl)control, true, false );

				validationText.Text = vcu.DumpHierarchy( (IValidationControl)control, false );
				validationText.Text += v.Text;
				validationHtml.NavigateToString( v.Html.ToString() );
			}

			this.Show();
		}

		//private void Navigate(string data)
		//{
		//    string outFile = Path.GetTempFileName();
		//    File.Move( outFile, outFile += ".html" );
		//    StreamWriter reportWriter = new StreamWriter( outFile );
		//    reportWriter.Write( data );
		//    reportWriter.Close();
		//    securityHtml.NavigateToString(
		//}
	}
}
