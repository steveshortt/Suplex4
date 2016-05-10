using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;
using Suplex.Security.Standard;


namespace Suplex.WinForms
{
	[ToolboxItem( false )]
	[EventBindings( EventBindingsAttribute.BaseEvents.None, ControlEvents.None, false )]
	public class sSecureManagerBase : System.Windows.Forms.PictureBox, IValidationControl, ISecurityExtender	//, IHiddenControl
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();

		/* NOTE: protected members */
		protected SecurityAccessor _sa = null;
		protected SecurityResultCollection _sr = null;
		protected ValidationAccessor _va = null;
		/* NOTE: protected members */

		private Image _image = null;
		private Size mySize = new Size( 16, 16 );

		private object _value = null;


		private sSecureManagerBase() : base() { }

		public sSecureManagerBase(Image image) : base()
		{
			this.TabStop = false;
			base.Image = _image = image;
			base.SizeMode = PictureBoxSizeMode.AutoSize;
		}

		protected override void InitLayout()
		{
			_sa.EnsureDefaultState();

			base.InitLayout();
		}


		[ParenthesizePropertyName( true ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public string UniqueName
		{
			get
			{
				return string.IsNullOrEmpty( _uniqueName ) ? base.Name : _uniqueName;
			}
			set
			{
				_uniqueName = value;
			}
		}

		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Suplex.Data.DataAccessLayer DataAccessLayer
		{
			get { return _dal; }
			set { _dal = value; }
		}


		#region Security Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Security management properties and tools." ), Category( "Suplex" )]
		public SecurityAccessor Security
		{
			get { return _sa; }
		}

		public virtual void ApplySecurity()
		{
			base.Visible = false;
		}

		[Browsable( false ), DefaultValue( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		new public bool Visible
		{
			get { return base.Visible; }
			set { base.Visible = this.DesignMode; }
		}

		[Browsable( false ), DefaultValue( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		new public bool Enabled
		{
			get { return base.Enabled; }
			set { base.Enabled = this.DesignMode; }
		}
		#endregion


		#region Validation Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Validation management properties and tools." )]
		public IValidationAccessor Validation
		{
			get { return _va; }
		}

		public virtual ValidationResult ProcessValidate(bool processFillMaps)
		{
			return _va.ProcessEvent( null, ControlEvents.Validating, processFillMaps );
		}

		protected override void OnTextChanged(System.EventArgs e)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "TextChanged.", false );

			_va.ProcessEvent( this.Value.ToString(), ControlEvents.TextChanged, true );

			base.OnTextChanged( e );
		}

		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public object Value
		{
			get { return _value; }
			set
			{
				if( _value != value )
				{
					_value = value;
					this.OnValueChanged();
				}
			}
		}

		protected virtual void OnValueChanged()
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "ValueChanged.", false );

			_va.ProcessEvent( this.Value.ToString(), ControlEvents.ValueChanged, true );
		}
		#endregion


		#region Overrides
		[Browsable( false )]
		new public Image Image
		{
			get { return base.Image; }
			set { base.Image = _image; }
		}
		[Browsable( false )]
		new public Size Size
		{
			get { return mySize; }
			set { base.Size = mySize; }
		}
		[Browsable( false )]
		[DefaultValue( PictureBoxSizeMode.AutoSize )]
		new public PictureBoxSizeMode SizeMode
		{
			get { return base.SizeMode; }
			set { base.SizeMode = PictureBoxSizeMode.AutoSize; }
		}
		[Browsable( false )]
		new public string AccessibleName
		{
			get { return base.AccessibleName; }
			set { base.AccessibleName = value; }
		}
		[Browsable( false )]
		new public string AccessibleDescription
		{
			get { return base.AccessibleDescription; }
			set { base.AccessibleDescription = value; }
		}
		[Browsable( false )]
		new public AccessibleRole AccessibleRole
		{
			get { return base.AccessibleRole; }
			set { base.AccessibleRole = value; }
		}
		[Browsable( false )]
		public override AnchorStyles Anchor
		{
			get { return base.Anchor; }
			set { base.Anchor = value; }
		}
		[Browsable( false )]
		public override Cursor Cursor
		{
			get { return base.Cursor; }
			set { base.Cursor = value; }
		}
		[Browsable( false )]
		public override ContextMenu ContextMenu
		{
			get { return base.ContextMenu; }
			set { base.ContextMenu = value; }
		}
		[Browsable( false )]
		public override Color ForeColor
		{
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}
		[Browsable( false )]
		public override Color BackColor
		{
			get { return base.BackColor; }
			set { base.BackColor = value; }
		}
		[Browsable( false )]
		public override Image BackgroundImage
		{
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = null; }
		}
		[Browsable( false )]
		new public BorderStyle BorderStyle
		{
			get { return base.BorderStyle; }
			set { base.BorderStyle = value; }
		}
		[Browsable( false )]
		public override DockStyle Dock
		{
			get { return base.Dock; }
			set { base.Dock = DockStyle.None; }
		}
		protected override void OnVisibleChanged(EventArgs e)
		{
			if( !DesignMode ) base.Visible = false;
			base.OnVisibleChanged( e );
		}
		#endregion
	}//class
}	//namespace