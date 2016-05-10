using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Suplex.WinForms.Specialized
{
	/// <summary>
	/// Summary description for HorizontalLine.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(Suplex.WinForms.sButton), "Resources.HorizontalLine.gif")]
	public class HorizontalLine : PictureBox
	{
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(HorizontalLine));
			// 
			// HorizontalLine
			// 
			this.Image = ((System.Drawing.Image)(resources.GetObject("$this.Image")));
			this.Size = new System.Drawing.Size(200, 1);
			this.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;

		}
	
		public HorizontalLine()
		{
			InitializeComponent();
		}
	}
}
