using System;
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
	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	[ToolboxItem( true ), ToolboxBitmap( typeof( Button ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.Click | ControlEvents.TextChanged )]
	public class sButton : System.Windows.Forms.Button, IValidationControl
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationAccessor _va = null;


		public sButton() : base()
		{
			this.FlatStyle = FlatStyle.System;	//for XP style compliance

			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationAccessor( this, TypeCode.String );
		}

		protected override void InitLayout()
		{
			_sa.EnsureDefaultState();
			if( !this.DesignMode )
			{
				base.Visible = this.InitVisible;
				base.Enabled = this.InitEnabled;
			}

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
			ValidationResult vr = new ValidationResult( this.UniqueName );
			if( this.Enabled )
			{
				vr = _va.ProcessEvent( this.Text, ControlEvents.Validating, processFillMaps );
			}
			return vr;
		}

		protected override void OnValidating(CancelEventArgs e)
		{
			e.Cancel = !this.ProcessValidate( true ).Success;

			base.OnValidating( e );
		}

		protected override void OnClick(EventArgs e)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Clicked.", false );

			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				_va.ProcessEvent( this.Text, ControlEvents.Click, true );
				base.OnClick( e );
			}
		}

		protected override void OnTextChanged(System.EventArgs e)
		{
			_va.ProcessEvent( this.Text, ControlEvents.TextChanged, true );

			base.OnTextChanged( e );
		}

		protected override void OnEnter(EventArgs e)
		{
			_va.ProcessEvent( this.Text, ControlEvents.Enter, true );
			base.OnEnter( e );
		}

		protected override void OnLeave(EventArgs e)
		{
			_va.ProcessEvent( this.Text, ControlEvents.Leave, true );
			base.OnLeave( e );
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				_va.ProcessEvent( this.Text, ControlEvents.EnabledChanged, true );
				//_sa.AuditAccess( this, AceType.UI, UIRight.Enabled, _auditEventHandler );

				base.OnEnabledChanged( e );
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
			{
				_va.ProcessEvent( this.Text, ControlEvents.VisibleChanged, true );
				//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Visible, _auditEventHandler );

				base.OnVisibleChanged( e );
			}
		}
		#endregion


		#region Security Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Security management properties and tools." ), Category( "Suplex" )]
		public SecurityAccessor Security
		{
			get { return _sa; }
		}

		public void ApplySecurity()
		{
			if( !this.DesignMode )
			{
				if( !_sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					base.Enabled = false;
				}
				if( !_sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					base.Visible = false;
				}
			}
		}

		[DefaultValue( false )]
		new public bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					base.Visible = value;
				}
			}
		}
		[DefaultValue( false )]
		public bool InitVisible { get; set; }

		[DefaultValue( false )]
		new public bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					base.Enabled = value;
				}
			}
		}
		[DefaultValue( false )]
		public bool InitEnabled { get; set; }

		/*
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if( _OperateRight )
			{
				base.OnMouseDown( e );
			}
		}
		*/
		#endregion
	}//class
}	//namespace