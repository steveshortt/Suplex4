using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using Microsoft.Win32;

using Suplex.Data;
using Suplex.Forms;
using Suplex.General;
using Suplex.Security;
using Suplex.Security.Standard;
using Suplex.WinForms.Specialized;
using System.IO;


namespace Suplex.WinForms
{
	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(System.Windows.Forms.Form))]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.Closing )]
	public class sForm : System.Windows.Forms.Form, IValidationContainer
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationContainerAccessor _va = null;

		#region Diag Vars
		private sDiagInfo					_diagInfoForm;
		private sDiagInfoCtrl				_diagInfoCtrl;
		private int							_diagClickCount			= 0;
		private Timer						_diagTimer;
		private IContainer					_diagComponents;
		#endregion


		public sForm() : base()
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

		protected override void OnLoad(EventArgs e)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Loaded.", false );

			base.OnLoad( e );
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

		protected override void OnClosing(CancelEventArgs e)
		{
			//switching to _ValidationErrors.Count > 0, 06272005
//			_ValidationFailed = false;	// 3/2/2004

			this.ProcessValidate( true );

			//if( this.AutoValidateContainer )			{			}

//			e.Cancel = _ValidationFailed;	//switching to _ValidationErrors.Count > 0, 06272005
			//e.Cancel = _ValidationErrors.Count > 0;

			e.Cancel = !_va.ChildValidationSuccess;

			if( _va.ChildValidationSuccess )
			{
				_va.ProcessEvent( null, ControlEvents.Closing, true );
			}

			base.OnClosing( e );
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

		private void ApplySecurity_Old()
		{
			//base.Enabled = _sr[AceType.UI, UIRight.Enabled].AccessAllowed;
					
			//if( this.Created )
			//{
			//    base.Visible = _sr[AceType.UI, UIRight.Visible].AccessAllowed;
			//}

			//_auditType = AuditTypes.ControlDetail;
			//if( !_sr[AceType.UI, UIRight.Operate].AccessAllowed || !_sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			//{
			//    _auditType = AuditTypes.Warning;
			//}

			//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Operate, _auditEventHandler );
			//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Enabled, _auditEventHandler );
			//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Visible, _auditEventHandler );
		}


		protected override void OnClick(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				base.OnClick( e );
			}
		}


		[DefaultValue(false)]
		new public bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				if ( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					base.Visible = value;
				}
			}
		}


		[DefaultValue(false)]
		new public bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				if ( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					base.Enabled = value;
				}
			}
		}


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
			this.Controls.Add( _diagInfoCtrl );

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
			_diagInfoForm = new sDiagInfo();

			SecureControlUtils scu = new SecureControlUtils();
			ValidationControlUtils vcu = new ValidationControlUtils();

			DiagInfoStreams s = scu.DumpSecurity( this, true, false );
			DiagInfoStreams v = vcu.DumpValidation( this, true, false );

			_diagInfoForm.SecurityText = scu.DumpHierarchy( this, false );
			_diagInfoForm.SecurityText += s.Text;
			_diagInfoForm.SecurityHtml = s.Html.ToString();

			_diagInfoForm.ValidationText = vcu.DumpHierarchy( this, false );
			_diagInfoForm.ValidationText += v.Text;
			_diagInfoForm.ValidationHtml = v.Html.ToString();

			_diagInfoForm.Show();
		}
		#endregion


		#region position handlers

		public void LoadSizePosition( string regKey )
		{
			RegistryKey key = Registry.CurrentUser.OpenSubKey( regKey );

			if( !(key == null) )
			{
				string formDesc = key.GetValue( "FormDescriptor", string.Empty ).ToString();

				if( formDesc.Length > 0 )
				{
					string[] fd = formDesc.Split( '.' );
					this.Top = Int32.Parse( fd[0] );
					this.Left = Int32.Parse( fd[1] );
					this.Height = Int32.Parse( fd[2] );
					this.Width = Int32.Parse( fd[3] );

					bool ok = false;
					int n = 0;
					while( !ok && n<Screen.AllScreens.Length )
					{
						ok = this.DesktopBounds.IntersectsWith( Screen.AllScreens[n].Bounds );
						n++;
					}

					if( !ok )
					{
						this.Top = this.Left = 40;
					}

				}

				key.Close();
			}
		}


		public void SaveSizePosition( string regKey )
		{
			RegistryKey key = Registry.CurrentUser.CreateSubKey( regKey );

			key.SetValue( "FormDescriptor",
				string.Format( "{0}.{1}.{2}.{3}", this.Top, this.Left, this.Height, this.Width ) );
		}


		#endregion
	}	//class
}	//namespace