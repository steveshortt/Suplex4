using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

namespace Suplex.WinForms.Specialized
{
	/// <summary>
	/// Implements NavigateComplete2 method and manages the temp file.
	/// </summary>
	[ToolboxItem( true )]
	[ToolboxBitmap( typeof( Suplex.WinForms.sButton ), "Resources.WebHostHelper.gif" )]
	public class WebHostHelper : System.ComponentModel.Component
	{
		private WebBrowser _webBrowser = null;
		private bool _deleteTempFile = true;
		private StreamWriter _reportWriter = null;
		private string _outFile = null;
		private object _missing = Type.Missing;


		private System.Windows.Forms.Timer deleteTimer;
		private System.ComponentModel.IContainer components;

		public WebHostHelper(System.ComponentModel.IContainer container)
		{
			///
			/// Required for Windows.Forms Class Composition Designer support
			///
			container.Add( this );
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		public WebHostHelper()
		{
			///
			/// Required for Windows.Forms Class Composition Designer support
			///
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if( disposing )
			{
				if( components != null )
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.deleteTimer = new System.Windows.Forms.Timer( this.components );
			// 
			// deleteTimer
			// 
			this.deleteTimer.Tick += new System.EventHandler( this.deleteTimer_Tick );

		}
		#endregion


		#region public accessors
		public WebBrowser WebBrowser
		{
			get
			{
				return _webBrowser;
			}
			set
			{
				_webBrowser = value;

				if( value != null )
				{
					_webBrowser.Navigated += new WebBrowserNavigatedEventHandler( this.WebBrowser_Navigated );
				}
			}
		}

		[Browsable( false )]
		public string TempFileName
		{
			get
			{
				return _outFile;
			}
		}

		public void Navigate(StringBuilder data, bool deleteTempFile)
		{
			this.Navigate( data.ToString(), deleteTempFile );
		}

		public void Navigate(string data, bool deleteTempFile)
		{
			_deleteTempFile = deleteTempFile;
			_outFile = Path.GetTempFileName();
			File.Move( _outFile, _outFile += ".html" );
			_reportWriter = new StreamWriter( _outFile );
			_reportWriter.Write( data );
			_reportWriter.Close();

			this.navigate( _outFile );
		}

		public void Navigate(string url)
		{
			_deleteTempFile = false;
			this.navigate( url );
		}
		#endregion

		private void navigate(string url)
		{
			_webBrowser.Navigate( url );
		}

		//NavigateComplete2 sometimes fires while _outFile is still in use, 
		//so the timer holds the file_delete code and just keeps trying until successful.
		private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			deleteTimer.Enabled = _deleteTempFile;
		}

		private void deleteTimer_Tick(object sender, System.EventArgs e)
		{
			while( deleteTimer.Enabled )
			{
				try
				{
					File.Delete( _outFile );
					deleteTimer.Enabled = false;
				}
				catch { }
			}
		}
	}
}