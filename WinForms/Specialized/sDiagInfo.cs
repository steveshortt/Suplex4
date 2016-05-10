using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Suplex.WinForms.Specialized
{
	/// <summary>
	/// Summary description for sDiagInfo.
	/// </summary>
	public class sDiagInfo : System.Windows.Forms.Form
	{
		private string _vHtml			= string.Empty;
		private string _sHtml			= string.Empty;
		private TabPage _activeTabPage	= null;

		private System.Windows.Forms.Button cmdClipboard;
		private System.Windows.Forms.Button cmdClose;
		private System.Windows.Forms.TabControl tcDiagInfo;
		private System.Windows.Forms.TabPage tpSecurity;
		private System.Windows.Forms.TabPage tpValidation;
		private System.Windows.Forms.TabControl tcSecurity;
		private System.Windows.Forms.TabPage tpSHTML;
		private System.Windows.Forms.TabPage tpSText;
		private System.Windows.Forms.TabControl tcValidation;
		private System.Windows.Forms.TextBox txtValidation;
		private Suplex.WinForms.Specialized.WebHostHelper whhValidation;
		private Suplex.WinForms.Specialized.WebHostHelper whhSecurity;
		private System.Windows.Forms.TextBox txtSecurity;
		private System.Windows.Forms.Button cmdSave;
		private System.Windows.Forms.TabPage tpVHTML;
		private System.Windows.Forms.TabPage tpVText;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.WebBrowser wbSecurity;
		private System.Windows.Forms.WebBrowser wbValidation;
		private System.ComponentModel.IContainer components;

		public sDiagInfo()
		{
			InitializeComponent();	// Required for Windows Form Designer support

			_activeTabPage = tpSHTML;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( sDiagInfo ) );
			this.cmdClipboard = new System.Windows.Forms.Button();
			this.cmdClose = new System.Windows.Forms.Button();
			this.tcDiagInfo = new System.Windows.Forms.TabControl();
			this.tpSecurity = new System.Windows.Forms.TabPage();
			this.tcSecurity = new System.Windows.Forms.TabControl();
			this.tpSHTML = new System.Windows.Forms.TabPage();
			this.tpSText = new System.Windows.Forms.TabPage();
			this.txtSecurity = new System.Windows.Forms.TextBox();
			this.tpValidation = new System.Windows.Forms.TabPage();
			this.tcValidation = new System.Windows.Forms.TabControl();
			this.tpVHTML = new System.Windows.Forms.TabPage();
			this.tpVText = new System.Windows.Forms.TabPage();
			this.txtValidation = new System.Windows.Forms.TextBox();
			this.cmdSave = new System.Windows.Forms.Button();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.wbSecurity = new System.Windows.Forms.WebBrowser();
			this.wbValidation = new System.Windows.Forms.WebBrowser();
			this.whhValidation = new Suplex.WinForms.Specialized.WebHostHelper( this.components );
			this.whhSecurity = new Suplex.WinForms.Specialized.WebHostHelper( this.components );
			this.tcDiagInfo.SuspendLayout();
			this.tpSecurity.SuspendLayout();
			this.tcSecurity.SuspendLayout();
			this.tpSHTML.SuspendLayout();
			this.tpSText.SuspendLayout();
			this.tpValidation.SuspendLayout();
			this.tcValidation.SuspendLayout();
			this.tpVHTML.SuspendLayout();
			this.tpVText.SuspendLayout();
			this.SuspendLayout();
			// 
			// cmdClipboard
			// 
			this.cmdClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdClipboard.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdClipboard.Location = new System.Drawing.Point( 514, 676 );
			this.cmdClipboard.Name = "cmdClipboard";
			this.cmdClipboard.Size = new System.Drawing.Size( 104, 23 );
			this.cmdClipboard.TabIndex = 2;
			this.cmdClipboard.Text = "Cop&y to Clipboard";
			this.cmdClipboard.Click += new System.EventHandler( this.cmdClipboard_Click );
			// 
			// cmdClose
			// 
			this.cmdClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdClose.Location = new System.Drawing.Point( 624, 676 );
			this.cmdClose.Name = "cmdClose";
			this.cmdClose.Size = new System.Drawing.Size( 104, 23 );
			this.cmdClose.TabIndex = 3;
			this.cmdClose.Text = "&Close";
			this.cmdClose.Click += new System.EventHandler( this.cmdClose_Click );
			// 
			// tcDiagInfo
			// 
			this.tcDiagInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tcDiagInfo.Controls.Add( this.tpSecurity );
			this.tcDiagInfo.Controls.Add( this.tpValidation );
			this.tcDiagInfo.Location = new System.Drawing.Point( 0, 0 );
			this.tcDiagInfo.Name = "tcDiagInfo";
			this.tcDiagInfo.SelectedIndex = 0;
			this.tcDiagInfo.Size = new System.Drawing.Size( 732, 672 );
			this.tcDiagInfo.TabIndex = 0;
			this.tcDiagInfo.SelectedIndexChanged += new System.EventHandler( this.tcDiagInfo_SelectedIndexChanged );
			// 
			// tpSecurity
			// 
			this.tpSecurity.Controls.Add( this.tcSecurity );
			this.tpSecurity.Location = new System.Drawing.Point( 4, 22 );
			this.tpSecurity.Name = "tpSecurity";
			this.tpSecurity.Size = new System.Drawing.Size( 724, 646 );
			this.tpSecurity.TabIndex = 0;
			this.tpSecurity.Text = "Security";
			// 
			// tcSecurity
			// 
			this.tcSecurity.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tcSecurity.Controls.Add( this.tpSHTML );
			this.tcSecurity.Controls.Add( this.tpSText );
			this.tcSecurity.Location = new System.Drawing.Point( 8, 8 );
			this.tcSecurity.Name = "tcSecurity";
			this.tcSecurity.SelectedIndex = 0;
			this.tcSecurity.Size = new System.Drawing.Size( 708, 632 );
			this.tcSecurity.TabIndex = 0;
			this.tcSecurity.SelectedIndexChanged += new System.EventHandler( this.tcSecurity_SelectedIndexChanged );
			// 
			// tpSHTML
			// 
			this.tpSHTML.Controls.Add( this.wbSecurity );
			this.tpSHTML.Location = new System.Drawing.Point( 4, 22 );
			this.tpSHTML.Name = "tpSHTML";
			this.tpSHTML.Size = new System.Drawing.Size( 700, 606 );
			this.tpSHTML.TabIndex = 0;
			this.tpSHTML.Text = "HTML";
			// 
			// tpSText
			// 
			this.tpSText.Controls.Add( this.txtSecurity );
			this.tpSText.Location = new System.Drawing.Point( 4, 22 );
			this.tpSText.Name = "tpSText";
			this.tpSText.Size = new System.Drawing.Size( 700, 606 );
			this.tpSText.TabIndex = 1;
			this.tpSText.Text = "Text";
			// 
			// txtSecurity
			// 
			this.txtSecurity.BackColor = System.Drawing.SystemColors.Window;
			this.txtSecurity.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtSecurity.Font = new System.Drawing.Font( "Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)) );
			this.txtSecurity.Location = new System.Drawing.Point( 0, 0 );
			this.txtSecurity.MaxLength = 0;
			this.txtSecurity.Multiline = true;
			this.txtSecurity.Name = "txtSecurity";
			this.txtSecurity.ReadOnly = true;
			this.txtSecurity.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtSecurity.Size = new System.Drawing.Size( 700, 606 );
			this.txtSecurity.TabIndex = 1;
			// 
			// tpValidation
			// 
			this.tpValidation.Controls.Add( this.tcValidation );
			this.tpValidation.Location = new System.Drawing.Point( 4, 22 );
			this.tpValidation.Name = "tpValidation";
			this.tpValidation.Size = new System.Drawing.Size( 724, 646 );
			this.tpValidation.TabIndex = 1;
			this.tpValidation.Text = "Validation";
			// 
			// tcValidation
			// 
			this.tcValidation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tcValidation.Controls.Add( this.tpVHTML );
			this.tcValidation.Controls.Add( this.tpVText );
			this.tcValidation.Location = new System.Drawing.Point( 8, 8 );
			this.tcValidation.Multiline = true;
			this.tcValidation.Name = "tcValidation";
			this.tcValidation.SelectedIndex = 0;
			this.tcValidation.Size = new System.Drawing.Size( 708, 632 );
			this.tcValidation.TabIndex = 1;
			this.tcValidation.SelectedIndexChanged += new System.EventHandler( this.tcValidation_SelectedIndexChanged );
			// 
			// tpVHTML
			// 
			this.tpVHTML.Controls.Add( this.wbValidation );
			this.tpVHTML.Location = new System.Drawing.Point( 4, 22 );
			this.tpVHTML.Name = "tpVHTML";
			this.tpVHTML.Size = new System.Drawing.Size( 700, 606 );
			this.tpVHTML.TabIndex = 0;
			this.tpVHTML.Text = "HTML";
			// 
			// tpVText
			// 
			this.tpVText.Controls.Add( this.txtValidation );
			this.tpVText.Location = new System.Drawing.Point( 4, 22 );
			this.tpVText.Name = "tpVText";
			this.tpVText.Size = new System.Drawing.Size( 700, 606 );
			this.tpVText.TabIndex = 1;
			this.tpVText.Text = "Text";
			// 
			// txtValidation
			// 
			this.txtValidation.BackColor = System.Drawing.SystemColors.Window;
			this.txtValidation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtValidation.Font = new System.Drawing.Font( "Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)) );
			this.txtValidation.Location = new System.Drawing.Point( 0, 0 );
			this.txtValidation.MaxLength = 0;
			this.txtValidation.Multiline = true;
			this.txtValidation.Name = "txtValidation";
			this.txtValidation.ReadOnly = true;
			this.txtValidation.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtValidation.Size = new System.Drawing.Size( 700, 606 );
			this.txtValidation.TabIndex = 1;
			// 
			// cmdSave
			// 
			this.cmdSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdSave.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdSave.Location = new System.Drawing.Point( 404, 676 );
			this.cmdSave.Name = "cmdSave";
			this.cmdSave.Size = new System.Drawing.Size( 104, 23 );
			this.cmdSave.TabIndex = 1;
			this.cmdSave.Text = "&Save";
			this.cmdSave.Click += new System.EventHandler( this.cmdSave_Click );
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.Filter = "Html Files|*.html;*.htm|Text Files|*.txt|All Files|*.*";
			// 
			// wbSecurity
			// 
			this.wbSecurity.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wbSecurity.Location = new System.Drawing.Point( 0, 0 );
			this.wbSecurity.MinimumSize = new System.Drawing.Size( 20, 20 );
			this.wbSecurity.Name = "wbSecurity";
			this.wbSecurity.Size = new System.Drawing.Size( 700, 606 );
			this.wbSecurity.TabIndex = 0;
			// 
			// wbValidation
			// 
			this.wbValidation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wbValidation.Location = new System.Drawing.Point( 0, 0 );
			this.wbValidation.MinimumSize = new System.Drawing.Size( 20, 20 );
			this.wbValidation.Name = "wbValidation";
			this.wbValidation.Size = new System.Drawing.Size( 700, 606 );
			this.wbValidation.TabIndex = 0;
			// 
			// whhValidation
			// 
			this.whhValidation.WebBrowser = this.wbValidation;
			// 
			// whhSecurity
			// 
			this.whhSecurity.WebBrowser = this.wbSecurity;
			// 
			// sDiagInfo
			// 
			this.AcceptButton = this.cmdClose;
			this.AutoScaleBaseSize = new System.Drawing.Size( 5, 13 );
			this.CancelButton = this.cmdClose;
			this.ClientSize = new System.Drawing.Size( 732, 707 );
			this.Controls.Add( this.cmdSave );
			this.Controls.Add( this.tcDiagInfo );
			this.Controls.Add( this.cmdClose );
			this.Controls.Add( this.cmdClipboard );
			this.Icon = ((System.Drawing.Icon)(resources.GetObject( "$this.Icon" )));
			this.Name = "sDiagInfo";
			this.Text = "Diagnostic Information";
			this.tcDiagInfo.ResumeLayout( false );
			this.tpSecurity.ResumeLayout( false );
			this.tcSecurity.ResumeLayout( false );
			this.tpSHTML.ResumeLayout( false );
			this.tpSText.ResumeLayout( false );
			this.tpSText.PerformLayout();
			this.tpValidation.ResumeLayout( false );
			this.tcValidation.ResumeLayout( false );
			this.tpVHTML.ResumeLayout( false );
			this.tpVText.ResumeLayout( false );
			this.tpVText.PerformLayout();
			this.ResumeLayout( false );

		}
		#endregion


		private void cmdClose_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}


		private void cmdClipboard_Click(object sender, System.EventArgs e)
		{
			Clipboard.SetDataObject( GetText(), true );
			MessageBox.Show( this, "Text copied to clipboard.",
				"Copy complete", MessageBoxButtons.OK, MessageBoxIcon.Information );
		}


		private void cmdSave_Click(object sender, System.EventArgs e)
		{
			string text = GetText();
			saveFileDialog.FileName = string.Empty;
			DialogResult r = saveFileDialog.ShowDialog( this );
			if( r == DialogResult.OK )
			{
				StreamWriter w = new StreamWriter( saveFileDialog.FileName );	//System.Text.UnicodeEncoding.Unicode;
				w.Write( text );
				w.Close();
			}
		}


		private string GetText()
		{
			string text = string.Empty;

			switch( _activeTabPage.Name )
			{
				case "tpSHTML":
				{
					text = _sHtml;
					saveFileDialog.FilterIndex = 1;
					break;
				}
				case "tpSText":
				{
					text = txtSecurity.Text;
					saveFileDialog.FilterIndex = 2;
					break;
				}
				case "tpVHTML":
				{
					text = _vHtml;
					saveFileDialog.FilterIndex = 1;
					break;
				}
				case "tpVText":
				{
					text = txtValidation.Text;
					saveFileDialog.FilterIndex = 2;
					break;
				}
			}

			return text;
		}


		private void tcDiagInfo_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			_activeTabPage =
				tcDiagInfo.SelectedTab == tpSecurity ?
				tcSecurity.SelectedTab : tcValidation.SelectedTab;
		}
		private void tcSecurity_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			_activeTabPage = tcSecurity.SelectedTab;
		}
		private void tcValidation_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			_activeTabPage = tcValidation.SelectedTab;
		}


		public string ValidationText
		{
			get
			{
				return txtValidation.Text;
			}
			set
			{
				txtValidation.Text = value;
				txtValidation.SelectionLength = 0;
			}
		}


		public string ValidationHtml
		{
			set
			{
				_vHtml = value;
				whhValidation.Navigate( value.ToString(), true );
			}
		}

	
		public string SecurityText
		{
			get
			{
				return txtSecurity.Text;
			}
			set
			{
				txtSecurity.Text = value;
				txtSecurity.SelectionLength = 0;
			}
		}


		public string SecurityHtml
		{
			set
			{
				_sHtml = value;
				whhSecurity.Navigate( value.ToString(), true );
			}
		}
	}
}
