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
using Suplex.WinForms.Specialized;


namespace Suplex.WinForms
{
	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(TabPage))]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.CheckStateChanged )]
	public class sTabPage : System.Windows.Forms.TabPage, IValidationContainer
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationContainerAccessor _va = null;

		private TabControl _parentTabControl = null;
		private bool _tabVisible = true;
		private int _tabIndex = -1;

		#region Diag Vars
		private sDiagInfoCtrl				_diagInfoCtrl;
		private int							_diagClickCount			= 0;
		private Timer						_diagTimer;
		private IContainer					_diagComponents;
		#endregion


		public sTabPage() : base()
		{
			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationContainerAccessor( this, TypeCode.Empty );

			this.Diag_Setup();
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


		[DefaultValue(true)]
		public bool TabVisible
		{
			get
			{
				return _tabVisible;
			}
			set
			{
				if( !this.DesignMode && _sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					if( _tabVisible != value )
					{
						if( _parentTabControl == null )
						{
							_parentTabControl = (TabControl)Parent;
						}

						_tabVisible = value;

						if( value && !_parentTabControl.TabPages.Contains( this ) )
						{
							_parentTabControl.TabPages.Insert( _tabIndex > -1 ? _tabIndex : 0, this );
						}
						else
						{
							_tabIndex = _parentTabControl.TabPages.IndexOf( this );
							_parentTabControl.TabPages.Remove( this );
						}
					}
				}
			}
		}


//        [DefaultValue(false)]
//        new public bool Visible
//        {
//            get
//            {
//                return base.Visible;
//            }
//            set
//            {
//                if ( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
//                {
//                    base.Visible = value;

////					if( value )
////					{
////						((TabControl)Parent).TabPages.Remove( this );
////					}
////					else
////					{
////						((TabControl)Parent).TabPages.Add( this );
////					}
//                }
//            }
//        }
		#endregion


		#region Diag Handlers
		private void Diag_Setup()
		{
			this._diagComponents = new System.ComponentModel.Container();

			this._diagTimer = new System.Windows.Forms.Timer( this._diagComponents );
			this._diagTimer.Enabled = false;
			this._diagTimer.Interval = 2500;
			this._diagTimer.Tick += new System.EventHandler( this.Diag_Timer_Tick );

			this._diagInfoCtrl = new sDiagInfoCtrl();
			this._diagInfoCtrl.Name = "_diagInfoCtrl";
			this._diagInfoCtrl.Location = new Point( 0, 0 );
			this._diagInfoCtrl.KeyPress += new System.Windows.Forms.KeyPressEventHandler( this.Diag_KeyPress );

			try
			{
				this.Controls.Add( _diagInfoCtrl );
			}
			catch( Exception ex )
			{
				System.Diagnostics.Debug.WriteLine( string.Format( "{0}-{1}", ex.Message, this.GetType() ) );
			}

			this.DoubleClick += new EventHandler( Diag_Form_DoubleClick );
		}

		private void Diag_Form_DoubleClick(object sender, System.EventArgs e)
		{
			_diagClickCount++;
			if( _diagClickCount > 1 )
			{
				_diagInfoCtrl.Focus();
				_diagTimer.Enabled = true;
			}
		}

		private void Diag_Timer_Tick(object sender, System.EventArgs e)
		{
			_diagClickCount = 0;
			_diagTimer.Enabled = false;
		}

		private void Diag_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if( _diagClickCount > 1 )
			{
				if( e.KeyChar == '`' )
				{
					e.Handled = true;
					DumpDiagInfo();
				}
			}
		}

		public virtual void DumpDiagInfo()
		{
			if( this.TopLevelControl is sForm )
			{
				( (sForm)this.TopLevelControl ).DumpDiagInfo();
			}
			else if( this.TopLevelControl is sUserControl )
			{
				( (sUserControl)this.TopLevelControl ).DumpDiagInfo();
			}
		}
		#endregion
	}	//class
}	//namespace