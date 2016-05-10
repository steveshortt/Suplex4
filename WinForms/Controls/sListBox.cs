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
	[ToolboxItem( true ), ToolboxBitmap( typeof( ListBox ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.SelectedIndexChanged )]
	public class sListBox : System.Windows.Forms.ListBox, IValidationControl
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationAccessor _va = null;

		private int _lastSelectedIndex = -1;


		public sListBox() : base()
		{
			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationAccessor( this, TypeCode.String );
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
				vr = _va.ProcessEvent( this.GetCompareValue(), ControlEvents.Validating, processFillMaps );
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
				_va.ProcessEvent( this.GetCompareValue(), ControlEvents.Click, true );
				base.OnClick( e );
			}
		}

		protected override void OnTextChanged(System.EventArgs e)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "TextChanged.", false );

			_va.ProcessEvent( this.Text, ControlEvents.TextChanged, true );

			base.OnTextChanged( e );
		}

		protected override void OnEnter(EventArgs e)
		{
			_va.ProcessEvent( this.GetCompareValue(), ControlEvents.Enter, true );
			base.OnEnter( e );
		}

		protected override void OnLeave(EventArgs e)
		{
			_va.ProcessEvent( this.GetCompareValue(), ControlEvents.Leave, true );
			base.OnLeave( e );
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				_va.ProcessEvent( this.GetCompareValue(), ControlEvents.EnabledChanged, true );
				//_sa.AuditAccess( this, AceType.UI, UIRight.Enabled, _auditEventHandler );

				base.OnEnabledChanged( e );
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
			{
				_va.ProcessEvent( this.GetCompareValue(), ControlEvents.VisibleChanged, true );
				//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Visible, _auditEventHandler );

				base.OnVisibleChanged( e );
			}
		}

		protected override void OnDataSourceChanged(EventArgs e)
		{
			_lastSelectedIndex = -1;

			base.OnDataSourceChanged( e );
		}

		/// <summary>
		/// Resets flags indicating control should be validated and FillMaps should processed.
		/// </summary>
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "SelectedIndexChanged.", false );

			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				if( this.SelectedIndex != _lastSelectedIndex )
				{
					_lastSelectedIndex = this.SelectedIndex;

					_va.ProcessEvent( this.GetCompareValue(), ControlEvents.SelectedIndexChanged, true );
				}

				base.OnSelectedIndexChanged( e );
			}
			else
			{
				this.SelectedIndex = _lastSelectedIndex;
			}
		}

		private string GetCompareValue()
		{
			return this.SelectedValue != null ? this.SelectedValue.ToString() : this.Text;
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

		protected override void OnDoubleClick(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				base.OnDoubleClick( e );
			}
		}
		#endregion
	}	//class
}	//namespace