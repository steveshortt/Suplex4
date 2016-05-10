using System;
using System.IO;
using System.Text;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;


namespace Suplex.WinForms.Specialized
{
	/// <summary>
	/// Summary description for ReportHostHtml.
	/// </summary>
	public class ReportHostHtml : System.Windows.Forms.Form
	{
		private object			_reportDataSource	= null;
		private string			_title				= null;
		private string			_cssPath			= null;
		private StreamWriter	_report				= null;
		private StringWriter	_reportData			= null;

		private System.Windows.Forms.ToolBar toolBar;
		private System.Windows.Forms.ImageList imageList;
		private System.Windows.Forms.ToolBarButton tbbSave;
		private System.Windows.Forms.ToolBarButton tbbPrint;
		private System.Windows.Forms.ToolBarButton tbbClose;
		private Suplex.WinForms.Specialized.WebHostHelper whHelper;
		private System.Windows.Forms.SaveFileDialog saveFileDlg;
		private System.Windows.Forms.ToolBarButton tbbSep2;
		private System.Windows.Forms.ToolBarButton tbbSep1;
		private System.Windows.Forms.WebBrowser wbReportHost;
		private System.ComponentModel.IContainer components;


		public ReportHostHtml()
		{
			InitializeComponent();	// Required for Windows Form Designer support
		}

		public ReportHostHtml(object reportDataSource, string title, string cssPath)
		{
			InitializeComponent();	// Required for Windows Form Designer support

			Report( reportDataSource, title, cssPath );
		}

		public ReportHostHtml(StringBuilder reportData, string title, string cssPath)
		{
			InitializeComponent();	// Required for Windows Form Designer support

			Report( reportData, title, cssPath );
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( ReportHostHtml ) );
			this.toolBar = new System.Windows.Forms.ToolBar();
			this.tbbPrint = new System.Windows.Forms.ToolBarButton();
			this.tbbSep1 = new System.Windows.Forms.ToolBarButton();
			this.tbbSave = new System.Windows.Forms.ToolBarButton();
			this.tbbSep2 = new System.Windows.Forms.ToolBarButton();
			this.tbbClose = new System.Windows.Forms.ToolBarButton();
			this.imageList = new System.Windows.Forms.ImageList( this.components );
			this.saveFileDlg = new System.Windows.Forms.SaveFileDialog();
			this.wbReportHost = new System.Windows.Forms.WebBrowser();
			this.whHelper = new Suplex.WinForms.Specialized.WebHostHelper( this.components );
			this.SuspendLayout();
			// 
			// toolBar
			// 
			this.toolBar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBar.AutoSize = false;
			this.toolBar.Buttons.AddRange( new System.Windows.Forms.ToolBarButton[] {
            this.tbbPrint,
            this.tbbSep1,
            this.tbbSave,
            this.tbbSep2,
            this.tbbClose} );
			this.toolBar.Divider = false;
			this.toolBar.DropDownArrows = true;
			this.toolBar.ImageList = this.imageList;
			this.toolBar.Location = new System.Drawing.Point( 0, 0 );
			this.toolBar.Name = "toolBar";
			this.toolBar.ShowToolTips = true;
			this.toolBar.Size = new System.Drawing.Size( 692, 23 );
			this.toolBar.TabIndex = 3;
			this.toolBar.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
			this.toolBar.Wrappable = false;
			this.toolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler( this.toolBar_ButtonClick );
			// 
			// tbbPrint
			// 
			this.tbbPrint.ImageIndex = 0;
			this.tbbPrint.Name = "tbbPrint";
			this.tbbPrint.Tag = "tbbPrint";
			this.tbbPrint.Text = " Print Report";
			// 
			// tbbSep1
			// 
			this.tbbSep1.Name = "tbbSep1";
			this.tbbSep1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// tbbSave
			// 
			this.tbbSave.ImageIndex = 1;
			this.tbbSave.Name = "tbbSave";
			this.tbbSave.Tag = "tbbSave";
			this.tbbSave.Text = " Save Report";
			// 
			// tbbSep2
			// 
			this.tbbSep2.Name = "tbbSep2";
			this.tbbSep2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// tbbClose
			// 
			this.tbbClose.ImageIndex = 2;
			this.tbbClose.Name = "tbbClose";
			this.tbbClose.Tag = "tbbClose";
			this.tbbClose.Text = "Close Window";
			this.tbbClose.ToolTipText = "Close Window";
			// 
			// imageList
			// 
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject( "imageList.ImageStream" )));
			this.imageList.TransparentColor = System.Drawing.Color.Magenta;
			this.imageList.Images.SetKeyName( 0, "" );
			this.imageList.Images.SetKeyName( 1, "" );
			this.imageList.Images.SetKeyName( 2, "" );
			// 
			// saveFileDlg
			// 
			this.saveFileDlg.Filter = "Html Files|*.html;*.htm|All Files|*.*";
			this.saveFileDlg.Title = "Save Report";
			this.saveFileDlg.FileOk += new System.ComponentModel.CancelEventHandler( this.saveFileDlg_FileOk );
			// 
			// wbReportHost
			// 
			this.wbReportHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wbReportHost.Location = new System.Drawing.Point( 0, 23 );
			this.wbReportHost.MinimumSize = new System.Drawing.Size( 20, 20 );
			this.wbReportHost.Name = "wbReportHost";
			this.wbReportHost.Size = new System.Drawing.Size( 692, 448 );
			this.wbReportHost.TabIndex = 4;
			// 
			// whHelper
			// 
			this.whHelper.WebBrowser = this.wbReportHost;
			// 
			// ReportHostHtml
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size( 5, 13 );
			this.ClientSize = new System.Drawing.Size( 692, 471 );
			this.Controls.Add( this.wbReportHost );
			this.Controls.Add( this.toolBar );
			this.Icon = ((System.Drawing.Icon)(resources.GetObject( "$this.Icon" )));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ReportHostHtml";
			this.ResumeLayout( false );

		}
		#endregion

		public void Report(object reportDataSource, string title, string cssPath)
		{
			_reportDataSource = reportDataSource;
			_title = title;
			_cssPath = cssPath;

			this.Text = _title;

			_reportData = ReportingUtils.CreateHtml( _reportDataSource, _title, _cssPath );

			whHelper.Navigate( _reportData.ToString(), true );
		}

		public void Report(StringBuilder reportData, string title, string cssPath)
		{
			_reportData = new StringWriter( reportData );
			_title = title;
			_cssPath = cssPath;

			this.Text = _title;

			whHelper.Navigate( reportData, true );
		}

		public void SetInlineCss(string css)
		{
			ReportingUtils.InlineCss = css;
		}

		private void toolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch( e.Button.Tag.ToString() )
			{
				case "tbbPrint":
				{
					PrintReport();
					break;
				}
				case "tbbSave":
				{
					saveFileDlg.ShowDialog();
					break;
				}
				case "tbbClose":
				{
					this.Close();
					break;
				}
			}
		}

		private void saveFileDlg_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_report = new StreamWriter( saveFileDlg.FileName );
			_report.Write( _reportData.ToString() );
			_report.Close();
		}

		private void PrintReport()
		{
			//object m = Type.Missing;
			//wbReportHost.ExecWB( SHDocVw.OLECMDID.OLECMDID_PRINT, SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_PROMPTUSER, ref m, ref m );
			wbReportHost.Print();
		}
	}
}