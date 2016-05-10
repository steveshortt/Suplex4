using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;
using Suplex.General;
using Suplex.Security.Standard;

namespace Suplex.WinForms
{
	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	[ToolboxItem( true ), ToolboxBitmap( typeof( TabControl ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.SelectedIndexChanged )]
	public class sTabControl : System.Windows.Forms.TabControl, IValidationContainer
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationContainerAccessor _va = null;

		private sTabControl.sTabPageCollection _innerTabPages;
		private int _lastSelectedIndex = -1;


		public sTabControl() : base()
		{
			_innerTabPages = new sTabPageCollection( this );

			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationContainerAccessor( this, TypeCode.Empty );
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

		[Browsable( false ), Category( "Suplex" ),
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
			if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				vr = _va.ProcessEvent( null, ControlEvents.Validating, processFillMaps );
			}
			return vr;
		}

		protected override void OnEnter(EventArgs e)
		{
			_va.ProcessEvent( null, ControlEvents.Enter, true );
			base.OnEnter( e );
		}

		protected override void OnLeave(EventArgs e)
		{
			_va.ProcessEvent( null, ControlEvents.Leave, true );
			base.OnLeave( e );
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				_va.ProcessEvent( null, ControlEvents.EnabledChanged, true );
				//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Enabled, _auditEventHandler );

				base.OnEnabledChanged( e );
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
			{
				_va.ProcessEvent( null, ControlEvents.VisibleChanged, true );
				//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Visible, _auditEventHandler );

				base.OnVisibleChanged( e );
			}
		}

		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			if( !this.DesignMode )
			{
				_sa.AuditAction( AuditType.ControlDetail, null, "SelectedIndexChanged.", false );

				if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
				{
					if( this.SelectedIndex != _lastSelectedIndex )
					{
						_lastSelectedIndex = this.SelectedIndex;

						_va.ProcessEvent( null, ControlEvents.SelectedIndexChanged, true );
					}

					base.OnSelectedIndexChanged( e );
				}
				else
				{
					this.SelectedIndex = _lastSelectedIndex;
				}
			}
		}

		public virtual void AddChild(IValidationControl control)
		{
			throw new NotImplementedException();
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

		public virtual IEnumerable GetChildren()
		{
			return (ICollection)this.Controls;
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
				if( !this.DesignMode && _sr[AceType.UI, UIRight.Visible].AccessAllowed )
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
				if( !this.DesignMode && _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					base.Enabled = value;
				}
			}
		}

		protected override void OnClick(EventArgs e)
		{
			if( !this.DesignMode && _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				base.OnClick( e );
			}
		}
		#endregion


		#region sTabPageCollection Overrides
		[Description( "Gets the collection of sTabPages in this sTabControl." )]
		new public sTabControl.sTabPageCollection TabPages
		{
			get { return _innerTabPages; }
		}

		public class sTabPageCollection : TabControl.TabPageCollection
		{
			public sTabPageCollection( sTabControl owner ) : base( (TabControl)owner ){}

			new public sTabPage this[int index]
			{
				get
				{
					return (sTabPage)base[index];
				}
				set
				{
					base[index] = (TabPage)value;
				}
			}


			public void Add(sTabPage value)
			{
				base.Add( (TabPage)value );
			}


			public void AddRange(sTabPage[] pages)
			{
				base.AddRange( (TabPage[])pages );
			}


			public bool Contains(sTabPage page)
			{
				return base.Contains( (TabPage)page );
			}


			public int IndexOf(sTabPage page)
			{
				return base.IndexOf( (TabPage)page );
			}


			public void Remove(sTabPage value)
			{
				base.Remove( (TabPage)value );
			}
		}
		#endregion
	}	//class
}	//namespace