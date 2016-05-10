using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;


namespace Suplex.WinForms.Specialized
{
	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap( typeof( Suplex.WinForms.Specialized.sDiagInfoCtrl ), "Resources.mHiddenValue.gif" )]
	public class sDiagInfoCtrl : System.Windows.Forms.PictureBox
	{
		private Size mySize = new Size(0,0);

		public sDiagInfoCtrl() : base()
		{
			base.TabStop = false;
			base.Image = null;
			base.BackColor = SystemColors.Control;
			base.Size = mySize;
			base.SizeMode = PictureBoxSizeMode.AutoSize;
		}


		#region Overrides

		[Browsable(false)]
		new public Image Image
		{
			get
			{
				return base.Image;
			}
			set
			{
				base.Image = null;
			}
		}


		[Browsable(false)]
		new public Size Size
		{
			get
			{
				return mySize;
			}
			set
			{
				base.Size = mySize;
			}
		}


		[Browsable(false)]
		[DefaultValue(PictureBoxSizeMode.AutoSize)]
		new public PictureBoxSizeMode SizeMode
		{
			get
			{
				return base.SizeMode;
			}
			set
			{
				base.SizeMode = PictureBoxSizeMode.AutoSize;
			}
		}


		#region Misc

		[Browsable(false)]
		new public string AccessibleName
		{
			get
			{
				return base.AccessibleName;
			}
			set
			{
				base.AccessibleName = value;
			}
		}


		[Browsable(false)]
		new public string AccessibleDescription
		{
			get
			{
				return base.AccessibleDescription;
			}
			set
			{
				base.AccessibleDescription = value;
			}
		}


		[Browsable(false)]
		new public AccessibleRole AccessibleRole
		{
			get
			{
				return base.AccessibleRole;
			}
			set
			{
				base.AccessibleRole = value;
			}
		}


		[Browsable(false)]
		public override AnchorStyles Anchor
		{
			get
			{
				return base.Anchor;
			}
			set
			{
				base.Anchor = value;
			}
		}


		[Browsable(false)]
		public override Cursor Cursor
		{
			get
			{
				return base.Cursor;
			}
			set
			{
				base.Cursor = value;
			}
		}


		[Browsable(false)]
		public override ContextMenu ContextMenu
		{
			get
			{
				return base.ContextMenu;
			}
			set
			{
				base.ContextMenu = value;
			}
		}


		[Browsable(false)]
		public override Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}


		[Browsable(false)]
		public override Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = SystemColors.Control;
			}
		}


		[Browsable(false)]
		public override Image BackgroundImage
		{
			get
			{
				return base.BackgroundImage;
			}
			set
			{
				base.BackgroundImage = value;
			}
		}


		[Browsable(false)]
		new public BorderStyle BorderStyle
		{
			get
			{
				return base.BorderStyle;
			}
			set
			{
				base.BorderStyle = value;
			}
		}


		[Browsable(false)]
		public override DockStyle Dock
		{
			get
			{
				return base.Dock;
			}
			set
			{
				base.Dock = value;
			}
		}


		[Browsable(false)]
		[DefaultValue(false)]
		new public bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				base.Visible = value;
			}
		}


		[Browsable(false)]
		[DefaultValue(false)]
		new public bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				base.Enabled = value;
			}
		}

		#endregion


		#endregion
		

	}//class


}	//namespace