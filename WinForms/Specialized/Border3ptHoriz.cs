using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Suplex.WinForms.Specialized
{
	/// <summary>
	/// Summary description for Border3ptHoriz.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(Suplex.WinForms.sButton), "Resources.Border3ptHoriz.gif")]
	public class Border3ptHoriz : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.Panel pnlOuter;
		private System.Windows.Forms.Panel pnlInner;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Border3ptHoriz()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			this.TabStop = false;
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pnlOuter = new System.Windows.Forms.Panel();
			this.pnlInner = new System.Windows.Forms.Panel();
			this.pnlOuter.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlOuter
			// 
			this.pnlOuter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.pnlOuter.BackColor = System.Drawing.SystemColors.Window;
			this.pnlOuter.Controls.Add(this.pnlInner);
			this.pnlOuter.Location = new System.Drawing.Point(0, 0);
			this.pnlOuter.Name = "pnlOuter";
			this.pnlOuter.Size = new System.Drawing.Size(150, 3);
			this.pnlOuter.TabIndex = 0;
			// 
			// pnlInner
			// 
			this.pnlInner.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.pnlInner.BackColor = System.Drawing.SystemColors.ControlDark;
			this.pnlInner.Location = new System.Drawing.Point(0, 1);
			this.pnlInner.Name = "pnlInner";
			this.pnlInner.Size = new System.Drawing.Size(150, 1);
			this.pnlInner.TabIndex = 0;
			// 
			// Border3ptHoriz
			// 
			this.Controls.Add(this.pnlOuter);
			//this.Name = "Border3ptHoriz";
			this.Size = new System.Drawing.Size(150, 3);
			this.pnlOuter.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
