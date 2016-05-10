using System;
using System.Reflection;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Collections;

using mUtilities.Data;
using mUtilities.Forms;
using mUtilities.General;
using mUtilities.Security;
using mUtilities.Security.Standard;

namespace System.Web.UI.WebControls
{
	#region Table

	/// <summary>
	/// Provides secuirty, extended validation properties, and auto-validation.
	/// </summary>
	[ToolboxItem(true),
		System.Drawing.ToolboxBitmap(typeof(System.Web.UI.WebControls.Table))]
	public class mTable : System.Web.UI.WebControls.Table, IValidationControl, ISecureControl, ISecureContainer, IWebExtra
	{
		private string						_uniqueName					= null;
		private TypeCode					_typeCode					= TypeCode.String;
		private ValidationRuleCollection	_ValidationRules			= new ValidationRuleCollection();
		private FillMapCollection			_FillMaps					= new FillMapCollection();
		private ValidationControlCollection	_ValidationControls			= new ValidationControlCollection();
		private bool						_Validated					= false;
		private bool						_ProcessFillMaps			= true;
		private ControlEventBindings		_EventBindings				= new ControlEventBindings();
		private bool						_AllowUndeclaredControls	= false;

		private bool						_AutoValidateContainer			= true;
		private bool						_AutoValidateDisplayMessages	= true;
		private bool						_ValidationFailed				= false;
		private ValidationErrorCollection	_ValidationErrors				= new ValidationErrorCollection();
		private mValidationSummary			_ValidationSummaryControl		= null;
		
		private DataAccessLayer				_dal						= new DataAccessLayer();
		private DataAccessor				_da							= null;

		private mUtilities.Security.Standard.User	_UserContext	= mUtilities.Security.Standard.User.Unknown;

		private DefaultSecurityStates		_DefaultSecurityState	= DefaultSecurityStates.Unlocked;
		private ISecurityDescriptor			_SecurityDescriptor		= new SecurityDescriptorBase();
		private SecureControlCollection		_SecureControls			= new SecureControlCollection();
		private SecurityResultCollection	_sr				= null;
		private AuditWriter					_auditWriter			= null;
		private AuditEventHandler			_sa.AuditEventHandler		= null;
		private RightRoleCollection			_RightRoles				= new RightRoleCollection();
		private AuditType					_auditType				= AuditType.ControlDetail;
		private AceTypes[]					_nativeAceTypes			= new AceTypes[1] { AceType.UI };


		private object _tagObject = null;
		private string _tag = null;
		private bool _allowExtendedViewState = false;


		private mTableRowCollection			_rows					= null;

		//this is a subset of ControlEvents:
		//	- every overridden event should have an entry here
		//  - every entry here MUST exist in ControlEvents
		public enum BindableEvents
		{
			Initialize,
			EnabledChanged,
			VisibleChanged,
			TextChanged
		}


		#region Events
		public event ValidateCompletedEventHandler ValidateCompleted;
		public event System.EventHandler AutoValidating;
		public event System.EventHandler AutoValidateCompleted;
		public event AuditEventHandler Audited;

		protected bool OnValidateCompleted(object sender, ValidationArgs e)
		{
			if(ValidateCompleted != null)
			{
				ValidateCompleted(sender, e);
				return true;
			}
			else
			{
				return false;
			}
		}
		

		protected bool OnAutoValidating(object sender, EventArgs e)
		{
			if(AutoValidating != null)
			{
				AutoValidating(sender, e);
				return true;
			}
			else
			{
				return false;
			}
		}
				

		protected bool OnAutoValidateCompleted(object sender, EventArgs e)
		{
			if(AutoValidateCompleted != null)
			{
				AutoValidateCompleted(sender, e);
				return true;
			}
			else
			{
				return false;
			}
		}
				

		private void onAudited(object sender, AuditEventArgs e)
		{
			if( Audited != null )
			{
				Audited( sender, e );
			}
		}
		
		
		/// <summary>
		/// Writes audit, Raises the Audited event.
		/// </summary>
		protected void OnAudited(object sender, AuditEventArgs e)
		{
			if( (this.SecurityDescriptor.Sacl.AuditTypeFilter & e.AuditType) == e.AuditType )
			{
				if( this.AuditWriter != null )
				{
					this.AuditWriter.Write( e );
				}
			
				this.onAudited( sender, e );
			}
		}


		#endregion

		
		
		public mTable() : base()
		{
			_rows = new mTableRowCollection( this );

			_sa.AuditEventHandler = new AuditEventHandler(this.OnAudited);
			
			_sr = this.SecurityDescriptor.SecurityResults;
			_sr.AddAceType( AceType.UI, true, false, false );	//manually creates space for UI security info

			this.ApplySecurity( AceType.UI );

//			base.Visible = false;
//			base.Enabled = false;
		}


		protected override void OnLoad(EventArgs e)
		{
			if( _DefaultSecurityState != DefaultSecurityStates.Locked )
			{
				object[] rights = AceTypeRights.GetRights( AceType.UI );
				for( int n=0; n<rights.Length; n++ )
				{
					_sr[AceType.UI, rights[n]].AccessAllowed = true;
				}

				base.Visible = true;
				//base.Enabled = true;

				this.ApplySecurity( AceType.UI );
			}

			base.OnLoad (e);
		}


		protected override object SaveViewState()
		{
			if( _allowExtendedViewState )
			{
				object baseState = base.SaveViewState();

				object[] s = new object[12];

				s[0] = baseState;
				s[1] = _uniqueName;
				s[2] = _typeCode;
				s[3] = _Validated;
				s[4] = _ProcessFillMaps;
				s[5] = _AllowUndeclaredControls;
				s[6] = _AutoValidateContainer;
				s[7] = _AutoValidateDisplayMessages;
				s[8] = _ValidationFailed;
				s[9] = _DefaultSecurityState;
				s[10] = _auditType;
				s[11] = _tag;

				return s;
			}
			else
			{
				object baseState = base.SaveViewState();

				object[] s = new object[2];

				s[0] = baseState;
				s[1] = _tag;

				return s;
			}
		}


		/// <summary>
		/// Load State from the array of objects that was saved at SavedViewState.
		/// </summary>
		/// <param name="savedState">State information to load.</param>
		protected override void LoadViewState(object savedState)
		{
			if( _allowExtendedViewState )
			{
				if( savedState != null )
				{
					object[] s = (object[])savedState;

					if( s[0] != null )
						base.LoadViewState( s[0] );

					if( s[1] != null )
						_uniqueName = (string)s[1];

					if( s[2] != null )
						_typeCode = (TypeCode)s[2];

					if( s[3] != null )
						_Validated = (bool)s[3];

					if( s[4] != null )
						_ProcessFillMaps = (bool)s[4];

					if( s[5] != null )
						_AllowUndeclaredControls = (bool)s[5];

					if( s[6] != null )
						_AutoValidateContainer = (bool)s[6];

					if( s[7] != null )
						_AutoValidateDisplayMessages = (bool)s[7];

					if( s[8] != null )
						_ValidationFailed = (bool)s[8];

					if( s[9] != null )
						_DefaultSecurityState = (DefaultSecurityStates)s[9];

					if( s[10] != null )
						_auditType = (AuditType)s[10];

					if( s[11] != null )
						_tag = (string)s[11];
				}
			}
			else
			{
				object[] s = (object[])savedState;

				if( s[0] != null )
					base.LoadViewState( s[0] );

				if( s[1] != null )
					_tag = (string)s[1];
			}
		}


		[ParenthesizePropertyName(true)]
		public string UniqueName
		{
			get
			{
				if( _uniqueName == null || _uniqueName.Length == 0 )
				{
					_uniqueName = base.UniqueID;
				}

				return _uniqueName;
			}
			set
			{
				_uniqueName = value;
			}
		}


		public TypeCode DataType
		{
			get
			{
				return _typeCode;
			}
			set
			{
				_typeCode = value;
			}
		}


		[Browsable(false)]
		public ValidationRuleCollection ValidationRules
		{
			get
			{
				return _ValidationRules;
			}
			set
			{
				_ValidationRules = value;
			}
		}


		[Browsable(false)]
		public FillMapCollection FillMaps
		{
			get
			{
				return _FillMaps;
			}
			set
			{
				_FillMaps = value;
			}
		}


		[Browsable(false)]
		public ValidationControlCollection ValidationControls
		{
			get
			{
				return _ValidationControls;
			}
			set
			{
				_ValidationControls = value;
			}
		}


		[Browsable(false)]
		public mUtilities.Data.DataAccessLayer DataAccessLayer
		{
			get
			{
				return _dal;
			}
			set
			{
				_dal = value;
			}
		}


		[Browsable(false)]
		[Obsolete("User DataAccessLayer instead.", true)]
		[DefaultValue(null)]
		public mUtilities.Data.DataAccessor DataAccessor
		{
			get
			{
				return _da;
			}
			set
			{
				_da = value;
			}
		}


		[Browsable(false)]
		public ControlEventBindings EventBindings
		{
			get
			{
				return _EventBindings;
			}
			set
			{
				_EventBindings = value;
			}
		}

		
		public bool AllowUndeclaredControls
		{
			get
			{
				return _AllowUndeclaredControls;
			}
			set
			{
				_AllowUndeclaredControls = value;
			}
		}


		public bool AutoValidateContainer
		{
			get
			{
				return _AutoValidateContainer;
			}
			set
			{
				_AutoValidateContainer = value;
			}
		}


		public bool AutoValidateDisplayMessages
		{
			get
			{
				return _AutoValidateDisplayMessages;
			}
			set
			{
				_AutoValidateDisplayMessages = value;
			}
		}


		[Browsable(false)]
		public ValidationErrorCollection ValidationErrors
		{
			get
			{
				return _ValidationErrors;
			}
			set
			{
				_ValidationErrors = value;
			}
		}


		public mValidationSummary ValidationSummaryControl
		{
			get
			{
				return _ValidationSummaryControl;
			}
			set
			{
				_ValidationSummaryControl = value;
			}
		}


		/// <summary>
		/// Loads rules from a database using the DataAccessor for this control.
		/// Calls ResolveInternalReferences() automatically.
		/// </summary>
		public void LoadRules()
		{
			ValidationRuleBuilder.LoadRulesWeb( this );
		}


		/// <summary>
		/// Loads rules from a rules file.
		/// Calls ResolveInternalReferences() automatically.
		/// </summary>
		/// <param name="ruleFilePath">Path to the rules file.</param>
		public void LoadRules( string ruleFilePath )
		{
			ValidationRuleBuilder.LoadRulesWeb( this, ruleFilePath );
		}


		/// <summary>
		/// Recurses the ValidationControls Collection for this control and
		/// all child controls to resolve internal name references.
		/// </summary>
		public void ResolveInternalReferences()
		{
			ValidationRuleBuilder.ResolveInternalReferences( this );
		}

		
		/// <summary>
		/// Forces manual validation of control text.
		/// </summary>
		/// <param name="ProcessFillMaps">Boolean to specify whether FillMaps should be 
		/// processed on Validation success.
		/// </param>
		public virtual void Validate(bool ProcessFillMaps)
		{
			_ProcessFillMaps = ProcessFillMaps;
			_Validated = false;
			this.OnValidating( new System.ComponentModel.CancelEventArgs() );
		}


	
		/// <summary>
		/// Processes ValidationRules (which internally implements DataTypeCheck) 
		/// and reports success/failure.
		/// </summary>
		private void OnValidating( System.ComponentModel.CancelEventArgs e )
		{
			if ( !_Validated && this.ValidationRules.Count > 0 )	//&& this.Enabled 
			{
				this.ValidationErrors.Clear();

				DataComparer.Result result = 
					this.ValidationRules.Process( null, _typeCode, _dal.Application, ControlEvents.Validating );

				if( result.Success )	//everything passed
				{
					OnValidateCompleted( this, new ValidationArgs() );
					OnValidated();
					e.Cancel = false;

					_Validated = true;
				}
				else					//something failed
				{
					OnValidateCompleted( this, new ValidationArgs(true, result.Message) );
					e.Cancel = true;
				}
			}

			ValidationControl_ValidateChildren();
			e.Cancel = _ValidationFailed;

		}


		/// <summary>
		/// Fires on Validation success.  Processes FillMaps.
		/// </summary>
		private void OnValidated()
		{
			if ( _ProcessFillMaps )
			{
				this.FillMaps.Process( _dal.Application, ControlEvents.Validating );
			}

			_ProcessFillMaps = false;
		}


		private void OnEnabledChanged()
		{
			if ( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				ProcessEvent( ControlEvents.EnabledChanged );
			}
		}


		private void OnVisibleChanged()
		{
			if ( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
			{
				ProcessEvent( ControlEvents.VisibleChanged );
			}
		}

		
		private void ProcessEvent( ControlEvents EventBinding )
		{
			if( this.EventBindings.ValidationEvents.Contains( EventBinding ) )
			{
				DataComparer.Result result = 
					this.ValidationRules.Process( null, _typeCode, _dal.Application, EventBinding  );

				if( result.Success )	//everything passed
				{
					OnValidateCompleted( this, new ValidationArgs() );
				}
				else					//something failed
				{
					OnValidateCompleted( this, new ValidationArgs(true, result.Message) );
				}
			}

			if( this.EventBindings.FillMapEvents.Contains( EventBinding ) )
			{
				this.FillMaps.Process( _dal.Application, EventBinding );
			}
		}

		
		private void ValidationControl_ValidateChildren()
		{
			if( this.AutoValidateContainer )
			{
				OnAutoValidating( this, EventArgs.Empty );

				this.ValidationErrors.Clear();

				_ValidationFailed = false;

				IEnumerator vctls = this.ValidationControls.Values.GetEnumerator();
				while( vctls.MoveNext() )
				{
					((IValidationControl)vctls.Current).Validate( false );
				}

				if( _ValidationSummaryControl != null )
				{
					if( _ValidationSummaryControl.ErrorCount > 0 )
					{
						//_ValidationSummaryControl.Refresh();

						_ValidationSummaryControl.Title = "You have a validation error!";
						_ValidationSummaryControl.Visible = true;
					}
					else
					{
						_ValidationSummaryControl.Visible = false;
					}
				}

				OnAutoValidateCompleted( this, new EventArgs() );
			}
		}

		
		private void ValidationControl_ValidateCompleted(object sender, ValidationArgs e)
		{
			Control ctl = (Control)sender;
			if( e.Error )
			{
				if( this.AutoValidateDisplayMessages )
				{
					//this._err_Validation.SetError(ctl, e.Message);
				}

				if( _ValidationSummaryControl != null )
				{
					_ValidationSummaryControl.ErrorAdd( (IValidationControl)ctl, e.Message );
				}
				
				OnValidateCompleted( ctl, new ValidationArgs(true, e.Message) );
				
				_ValidationFailed = true;
			}
			else
			{
				if( this.AutoValidateDisplayMessages )
				{
					//this._err_Validation.SetError(ctl, "");
				}

				if( _ValidationSummaryControl != null )
				{
					_ValidationSummaryControl.ErrorRemove( (IValidationControl)ctl );
				}

				OnValidateCompleted( ctl, new ValidationArgs() );
			}
		}

		

		
		#region Security Implementation

		[Browsable(false)]
		public mUtilities.Security.Standard.User UserContext
		{
			get
			{
				return _UserContext;
			}
			set
			{
				_UserContext = value;
			}
		}


		public DefaultSecurityStates DefaultSecurityState
		{
			get
			{
				return _DefaultSecurityState;
			}
			set
			{
				_DefaultSecurityState = value;
			}
		}


		[Browsable(false)]
		public ISecurityDescriptor SecurityDescriptor
		{
			get
			{
				return _SecurityDescriptor;
			}
			set
			{
				_SecurityDescriptor = value;
			}
		}


		[Browsable(false)]
		public SecureControlCollection SecureControls
		{
			get
			{
				return _SecureControls;
			}
			set
			{
				_SecureControls = value;
			}
		}


		[Browsable(false)]
		public AuditWriter AuditWriter
		{
			get
			{
				return _auditWriter;
			}
			set
			{
				_auditWriter = value;
			}
		}


		[Browsable(false)]
		public RightRoleCollection RightRoles
		{
			get
			{
				return _RightRoles;
			}
			set
			{
				_RightRoles = value;
			}
		}


		[Browsable(false)]
		public AceTypes[] NativeAceTypes
		{
			get
			{
				return _nativeAceTypes;
			}
		}


		public virtual void LoadSecurity( mUtilities.Data.DataAccessor da, mUtilities.Security.Standard.User user, AuditEventHandler defaultAuditEventHandler, bool propagateAuditWriter, bool applyDefaultSecurity )
		{
			if( da == null ) da = _dal.Platform;
			SecurityBuilder.LoadRulesFromDB( this, da, user, defaultAuditEventHandler, propagateAuditWriter );

			if( applyDefaultSecurity )
			{
				this.ApplySecurity( AceTypes.Native, null, null );
			}
		}


		public virtual void ResolveRightRoles()
		{
			SecurityBuilder.ResolveRightRoles( this, this.SecureControls );
		}


		public void ApplySecurity(AceTypes AceType)
		{
			this.ApplySecurity( AceType, null, null );
		}

		
		public void ApplySecurity(AceTypes AceType, ArrayList DaclParameters, ArrayList SaclParameters)
		{
			switch( AceType )
			{
				case AceType.UI:
				case AceTypes.Native:
				{
					this.SecurityDescriptor.EvalSecurity( AceType.UI, DaclParameters, SaclParameters );
					this.AuditAccess();
					SecurityBuilder.PropagateSecurity( this );
					break;
				}
			}
		}


		public void AuditAccess()
		{
			//base.Enabled = _sr[AceType.UI, UIRight.Enabled].AccessAllowed;
			base.Visible = _sr[AceType.UI, UIRight.Visible].AccessAllowed;

			_auditType = AuditType.ControlDetail;
			if( !_sr[AceType.UI, UIRight.Operate].AccessAllowed || !_sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				_auditType = AuditType.Warning;
			}

			SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Operate, _sa.AuditEventHandler );
			SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Enabled, _sa.AuditEventHandler );
			SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Visible, _sa.AuditEventHandler );
		}


		private void PropagateSecurity(AceTypes AceType)
		{
			IEnumerator sControls = this.SecureControls.Values.GetEnumerator();
			while( sControls.MoveNext() )
			{
				ISecureControl sc = (ISecureControl)sControls.Current;
				this.SecurityDescriptor.CopyTo( sc.SecurityDescriptor, AceType, false );
				sc.ApplySecurity( AceType );
			}
		}


		[DefaultValue(false)]
		public override bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				if ( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					if( base.Visible != value )
					{
						base.Visible = value;
						OnVisibleChanged();
					}
				}
			}
		}


		#endregion



		protected override void AddedControl(Control control, int index)
		{
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				if( ReflectionUtils.ImplementsInterface( control, "mUtilities.Forms.IValidationControl" ) )
				{
					this.ValidationControls.Add( ((IValidationControl)control).UniqueName, (IValidationControl)control );

					if( this.AutoValidateContainer )
					{
						((IValidationControl)control).ValidateCompleted += 
							new ValidateCompletedEventHandler( this.ValidationControl_ValidateCompleted );
					}
				}
			
				if( ReflectionUtils.ImplementsInterface( control, "mUtilities.Forms.ISecureControl" ) )
				{
					this.SecureControls.Add( ((ISecureControl)control).UniqueName, (ISecureControl)control );
				}
			
				base.AddedControl( control, index );
			}
		}


		protected override void RemovedControl(Control control)
		{
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				if( ReflectionUtils.ImplementsInterface( control, "mUtilities.Forms.IValidationControl" ) )
				{
					this.ValidationControls.Remove( ((IValidationControl)control).UniqueName );
				}
			
				if( ReflectionUtils.ImplementsInterface( control, "mUtilities.Forms.ISecureControl" ) )
				{
					this.SecureControls.Remove( ((ISecureControl)control).UniqueName );
				}
			
				base.RemovedControl( control );
			}
		}


	
		public mTableRowCollection mRows
		{
			get
			{
				return _rows;
			}
		}


		#region IWebExtra Members
		public object TagObj
		{
			get { return _tagObject; }
			set { _tagObject = value; }
		}

		public string Tag
		{
			get { return _tag; }
			set { _tag = value; }
		}

		public bool AllowExtendedViewState
		{
			get { return _allowExtendedViewState; }
			set { _allowExtendedViewState = value; }
		}
		#endregion
	}


	#endregion


	#region TableRow

	public class mTableRow : System.Web.UI.WebControls.TableRow, IValidationControl, ISecureControl, ISecureContainer
	{
		private string						_uniqueName					= null;
		private TypeCode					_typeCode					= TypeCode.String;
		private ValidationRuleCollection	_ValidationRules			= new ValidationRuleCollection();
		private FillMapCollection			_FillMaps					= new FillMapCollection();
		private ValidationControlCollection	_ValidationControls			= new ValidationControlCollection();
		private bool						_Validated					= false;
		private bool						_ProcessFillMaps			= true;
		private ControlEventBindings		_EventBindings				= new ControlEventBindings();
		private bool						_AllowUndeclaredControls	= false;

		private bool						_AutoValidateContainer			= true;
		private bool						_AutoValidateDisplayMessages	= true;
		private bool						_ValidationFailed				= false;
		private ValidationErrorCollection	_ValidationErrors				= new ValidationErrorCollection();
		private mValidationSummary			_ValidationSummaryControl		= null;
		
		private DataAccessLayer				_dal						= new DataAccessLayer();
		private DataAccessor				_da							= null;

		private mUtilities.Security.Standard.User	_UserContext	= mUtilities.Security.Standard.User.Unknown;

		private DefaultSecurityStates		_DefaultSecurityState	= DefaultSecurityStates.Unlocked;
		private ISecurityDescriptor			_SecurityDescriptor		= new SecurityDescriptorBase();
		private SecureControlCollection		_SecureControls			= new SecureControlCollection();
		private SecurityResultCollection	_sr				= null;
		private AuditWriter					_auditWriter			= null;
		private AuditEventHandler			_sa.AuditEventHandler		= null;
		private RightRoleCollection			_RightRoles				= new RightRoleCollection();
		private AuditType					_auditType				= AuditType.ControlDetail;
		private AceTypes[]					_nativeAceTypes			= new AceTypes[1] { AceType.UI };


		private mTableCellCollection		_cells					= null;

		//this is a subset of ControlEvents:
		//	- every overridden event should have an entry here
		//  - every entry here MUST exist in ControlEvents
		public enum BindableEvents
		{
			Initialize,
			EnabledChanged,
			VisibleChanged,
			TextChanged
		}


		#region Events
		public event ValidateCompletedEventHandler ValidateCompleted;
		public event System.EventHandler AutoValidating;
		public event System.EventHandler AutoValidateCompleted;
		public event AuditEventHandler Audited;

		protected bool OnValidateCompleted(object sender, ValidationArgs e)
		{
			if(ValidateCompleted != null)
			{
				ValidateCompleted(sender, e);
				return true;
			}
			else
			{
				return false;
			}
		}
		

		protected bool OnAutoValidating(object sender, EventArgs e)
		{
			if(AutoValidating != null)
			{
				AutoValidating(sender, e);
				return true;
			}
			else
			{
				return false;
			}
		}
				

		protected bool OnAutoValidateCompleted(object sender, EventArgs e)
		{
			if(AutoValidateCompleted != null)
			{
				AutoValidateCompleted(sender, e);
				return true;
			}
			else
			{
				return false;
			}
		}
				

		private void onAudited(object sender, AuditEventArgs e)
		{
			if( Audited != null )
			{
				Audited( sender, e );
			}
		}
		
		
		/// <summary>
		/// Writes audit, Raises the Audited event.
		/// </summary>
		protected void OnAudited(object sender, AuditEventArgs e)
		{
			if( (this.SecurityDescriptor.Sacl.AuditTypeFilter & e.AuditType) == e.AuditType )
			{
				if( this.AuditWriter != null )
				{
					this.AuditWriter.Write( e );
				}
			
				this.onAudited( sender, e );
			}
		}


		#endregion

		
		
		public mTableRow() : base()
		{
			_cells = new mTableCellCollection( this );

			_sa.AuditEventHandler = new AuditEventHandler(this.OnAudited);
			
			_sr = this.SecurityDescriptor.SecurityResults;
			_sr.AddAceType( AceType.UI, true, false, false );	//manually creates space for UI security info

			this.ApplySecurity( AceType.UI );

			//			base.Visible = false;
			//			base.Enabled = false;
		}


		protected override void OnLoad(EventArgs e)
		{
			if( _DefaultSecurityState != DefaultSecurityStates.Locked )
			{
				object[] rights = AceTypeRights.GetRights( AceType.UI );
				for( int n=0; n<rights.Length; n++ )
				{
					_sr[AceType.UI, rights[n]].AccessAllowed = true;
				}

				base.Visible = true;
				//base.Enabled = true;

				this.ApplySecurity( AceType.UI );
			}

			base.OnLoad (e);
		}


		protected override object SaveViewState()
		{
			object baseState = base.SaveViewState();

			object[] s = new object[11];

			s[0]	= baseState;
			s[1]	= _uniqueName;
			s[2]	= _typeCode;
			s[3]	= _Validated;
			s[4]	= _ProcessFillMaps;
			s[5]	= _AllowUndeclaredControls;
			s[6]	= _AutoValidateContainer;
			s[7]	= _AutoValidateDisplayMessages;
			s[8]	= _ValidationFailed;
			s[9]	= _DefaultSecurityState;
			s[10]	= _auditType;

			return s;
		}


		/// <summary>
		/// Load State from the array of objects that was saved at SavedViewState.
		/// </summary>
		/// <param name="savedState">State information to load.</param>
		protected override void LoadViewState(object savedState) 
		{
			if (savedState != null)
			{
				object[] s = (object[])savedState;

				if ( s[0] != null )
					base.LoadViewState(s[0]);

				if ( s[1] != null )
					_uniqueName = (string)s[1];

				if ( s[2] != null )
					_typeCode = (TypeCode)s[2];

				if ( s[3] != null )
					_Validated = (bool)s[3];
				
				if ( s[4] != null )
					_ProcessFillMaps = (bool)s[4];
				
				if ( s[5] != null )
					_AllowUndeclaredControls = (bool)s[5];

				if ( s[6] != null )
					_AutoValidateContainer = (bool)s[6];
			
				if ( s[7] != null )
					_AutoValidateDisplayMessages = (bool)s[7];
			
				if ( s[8] != null )
					_ValidationFailed = (bool)s[8];
			
				if ( s[9] != null )
					_DefaultSecurityState = (DefaultSecurityStates)s[9];

				if ( s[10] != null )
					_auditType = (AuditType)s[10];
			}
		}


		[ParenthesizePropertyName(true)]
		public string UniqueName
		{
			get
			{
				if( _uniqueName == null || _uniqueName.Length == 0 )
				{
					if( this.Parent == null )
					{
						_uniqueName = base.UniqueID;
					}
					else
					{
						_uniqueName = string.Format( "{0}_r{1}",
							((mTable)this.Parent).UniqueName, ((mTable)this.Parent).mRows.GetRowIndex( this ) );
					}
				}

				return _uniqueName;
			}
			set
			{
				_uniqueName = value;
			}
		}


		public TypeCode DataType
		{
			get
			{
				return _typeCode;
			}
			set
			{
				_typeCode = value;
			}
		}


		[Browsable(false)]
		public ValidationRuleCollection ValidationRules
		{
			get
			{
				return _ValidationRules;
			}
			set
			{
				_ValidationRules = value;
			}
		}


		[Browsable(false)]
		public FillMapCollection FillMaps
		{
			get
			{
				return _FillMaps;
			}
			set
			{
				_FillMaps = value;
			}
		}


		[Browsable(false)]
		public ValidationControlCollection ValidationControls
		{
			get
			{
				return _ValidationControls;
			}
			set
			{
				_ValidationControls = value;
			}
		}


		[Browsable(false)]
		public mUtilities.Data.DataAccessLayer DataAccessLayer
		{
			get
			{
				return _dal;
			}
			set
			{
				_dal = value;
			}
		}


		[Browsable(false)]
		[Obsolete("User DataAccessLayer instead.", true)]
		[DefaultValue(null)]
		public mUtilities.Data.DataAccessor DataAccessor
		{
			get
			{
				return _da;
			}
			set
			{
				_da = value;
			}
		}


		[Browsable(false)]
		public ControlEventBindings EventBindings
		{
			get
			{
				return _EventBindings;
			}
			set
			{
				_EventBindings = value;
			}
		}

		
		public bool AllowUndeclaredControls
		{
			get
			{
				return _AllowUndeclaredControls;
			}
			set
			{
				_AllowUndeclaredControls = value;
			}
		}


		public bool AutoValidateContainer
		{
			get
			{
				return _AutoValidateContainer;
			}
			set
			{
				_AutoValidateContainer = value;
			}
		}


		public bool AutoValidateDisplayMessages
		{
			get
			{
				return _AutoValidateDisplayMessages;
			}
			set
			{
				_AutoValidateDisplayMessages = value;
			}
		}


		[Browsable(false)]
		public ValidationErrorCollection ValidationErrors
		{
			get
			{
				return _ValidationErrors;
			}
			set
			{
				_ValidationErrors = value;
			}
		}


		public mValidationSummary ValidationSummaryControl
		{
			get
			{
				return _ValidationSummaryControl;
			}
			set
			{
				_ValidationSummaryControl = value;
			}
		}


		/// <summary>
		/// Loads rules from a database using the DataAccessor for this control.
		/// Calls ResolveInternalReferences() automatically.
		/// </summary>
		public void LoadRules()
		{
			ValidationRuleBuilder.LoadRulesWeb( this );
		}


		/// <summary>
		/// Loads rules from a rules file.
		/// Calls ResolveInternalReferences() automatically.
		/// </summary>
		/// <param name="ruleFilePath">Path to the rules file.</param>
		public void LoadRules( string ruleFilePath )
		{
			ValidationRuleBuilder.LoadRulesWeb( this, ruleFilePath );
		}


		/// <summary>
		/// Recurses the ValidationControls Collection for this control and
		/// all child controls to resolve internal name references.
		/// </summary>
		public void ResolveInternalReferences()
		{
			ValidationRuleBuilder.ResolveInternalReferences( this );
		}

		
		/// <summary>
		/// Forces manual validation of control text.
		/// </summary>
		/// <param name="ProcessFillMaps">Boolean to specify whether FillMaps should be 
		/// processed on Validation success.
		/// </param>
		public virtual void Validate(bool ProcessFillMaps)
		{
			_ProcessFillMaps = ProcessFillMaps;
			_Validated = false;
			this.OnValidating( new System.ComponentModel.CancelEventArgs() );
		}


	
		/// <summary>
		/// Processes ValidationRules (which internally implements DataTypeCheck) 
		/// and reports success/failure.
		/// </summary>
		private void OnValidating( System.ComponentModel.CancelEventArgs e )
		{
			if ( !_Validated && this.ValidationRules.Count > 0 )	//&& this.Enabled 
			{
				this.ValidationErrors.Clear();

				DataComparer.Result result = 
					this.ValidationRules.Process( null, _typeCode, _dal.Application, ControlEvents.Validating );

				if( result.Success )	//everything passed
				{
					OnValidateCompleted( this, new ValidationArgs() );
					OnValidated();
					e.Cancel = false;

					_Validated = true;
				}
				else					//something failed
				{
					OnValidateCompleted( this, new ValidationArgs(true, result.Message) );
					e.Cancel = true;
				}
			}

			ValidationControl_ValidateChildren();
			e.Cancel = _ValidationFailed;

		}


		/// <summary>
		/// Fires on Validation success.  Processes FillMaps.
		/// </summary>
		private void OnValidated()
		{
			if ( _ProcessFillMaps )
			{
				this.FillMaps.Process( _dal.Application, ControlEvents.Validating );
			}

			_ProcessFillMaps = false;
		}


		private void OnEnabledChanged()
		{
			if ( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				ProcessEvent( ControlEvents.EnabledChanged );
			}
		}


		private void OnVisibleChanged()
		{
			if ( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
			{
				ProcessEvent( ControlEvents.VisibleChanged );
			}
		}

		
		private void ProcessEvent( ControlEvents EventBinding )
		{
			if( this.EventBindings.ValidationEvents.Contains( EventBinding ) )
			{
				DataComparer.Result result = 
					this.ValidationRules.Process( null, _typeCode, _dal.Application, EventBinding  );

				if( result.Success )	//everything passed
				{
					OnValidateCompleted( this, new ValidationArgs() );
				}
				else					//something failed
				{
					OnValidateCompleted( this, new ValidationArgs(true, result.Message) );
				}
			}

			if( this.EventBindings.FillMapEvents.Contains( EventBinding ) )
			{
				this.FillMaps.Process( _dal.Application, EventBinding );
			}
		}

		
		private void ValidationControl_ValidateChildren()
		{
			if( this.AutoValidateContainer )
			{
				OnAutoValidating( this, EventArgs.Empty );

				this.ValidationErrors.Clear();

				_ValidationFailed = false;

				IEnumerator vctls = this.ValidationControls.Values.GetEnumerator();
				while( vctls.MoveNext() )
				{
					((IValidationControl)vctls.Current).Validate( false );
				}

				if( _ValidationSummaryControl != null )
				{
					if( _ValidationSummaryControl.ErrorCount > 0 )
					{
						//_ValidationSummaryControl.Refresh();

						_ValidationSummaryControl.Title = "You have a validation error!";
						_ValidationSummaryControl.Visible = true;
					}
					else
					{
						_ValidationSummaryControl.Visible = false;
					}
				}

				OnAutoValidateCompleted( this, new EventArgs() );
			}
		}

		
		private void ValidationControl_ValidateCompleted(object sender, ValidationArgs e)
		{
			Control ctl = (Control)sender;
			if( e.Error )
			{
				if( this.AutoValidateDisplayMessages )
				{
					//this._err_Validation.SetError(ctl, e.Message);
				}

				if( _ValidationSummaryControl != null )
				{
					_ValidationSummaryControl.ErrorAdd( (IValidationControl)ctl, e.Message );
				}
				
				OnValidateCompleted( ctl, new ValidationArgs(true, e.Message) );
				
				_ValidationFailed = true;
			}
			else
			{
				if( this.AutoValidateDisplayMessages )
				{
					//this._err_Validation.SetError(ctl, "");
				}

				if( _ValidationSummaryControl != null )
				{
					_ValidationSummaryControl.ErrorRemove( (IValidationControl)ctl );
				}

				OnValidateCompleted( ctl, new ValidationArgs() );
			}
		}

		

		
		#region Security Implementation

		[Browsable(false)]
		public mUtilities.Security.Standard.User UserContext
		{
			get
			{
				return _UserContext;
			}
			set
			{
				_UserContext = value;
			}
		}


		public DefaultSecurityStates DefaultSecurityState
		{
			get
			{
				return _DefaultSecurityState;
			}
			set
			{
				_DefaultSecurityState = value;
			}
		}


		[Browsable(false)]
		public ISecurityDescriptor SecurityDescriptor
		{
			get
			{
				return _SecurityDescriptor;
			}
			set
			{
				_SecurityDescriptor = value;
			}
		}


		[Browsable(false)]
		public SecureControlCollection SecureControls
		{
			get
			{
				return _SecureControls;
			}
			set
			{
				_SecureControls = value;
			}
		}


		[Browsable(false)]
		public AuditWriter AuditWriter
		{
			get
			{
				return _auditWriter;
			}
			set
			{
				_auditWriter = value;
			}
		}


		[Browsable(false)]
		public RightRoleCollection RightRoles
		{
			get
			{
				return _RightRoles;
			}
			set
			{
				_RightRoles = value;
			}
		}


		[Browsable(false)]
		public AceTypes[] NativeAceTypes
		{
			get
			{
				return _nativeAceTypes;
			}
		}


		public virtual void LoadSecurity( mUtilities.Data.DataAccessor da, mUtilities.Security.Standard.User user, AuditEventHandler defaultAuditEventHandler, bool propagateAuditWriter, bool applyDefaultSecurity )
		{
			if( da == null ) da = _dal.Platform;
			SecurityBuilder.LoadRulesFromDB( this, da, user, defaultAuditEventHandler, propagateAuditWriter );

			if( applyDefaultSecurity )
			{
				this.ApplySecurity( AceTypes.Native, null, null );
			}
		}


		public virtual void ResolveRightRoles()
		{
			SecurityBuilder.ResolveRightRoles( this, this.SecureControls );
		}


		public void ApplySecurity(AceTypes AceType)
		{
			this.ApplySecurity( AceType, null, null );
		}

		
		public void ApplySecurity(AceTypes AceType, ArrayList DaclParameters, ArrayList SaclParameters)
		{
			switch( AceType )
			{
				case AceType.UI:
				case AceTypes.Native:
				{
					this.SecurityDescriptor.EvalSecurity( AceType.UI, DaclParameters, SaclParameters );
					this.AuditAccess();
					SecurityBuilder.PropagateSecurity( this );
					break;
				}
			}
		}


		public void AuditAccess()
		{
			//base.Enabled = _sr[AceType.UI, UIRight.Enabled].AccessAllowed;
			base.Visible = _sr[AceType.UI, UIRight.Visible].AccessAllowed;

			_auditType = AuditType.ControlDetail;
			if( !_sr[AceType.UI, UIRight.Operate].AccessAllowed || !_sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				_auditType = AuditType.Warning;
			}

			SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Operate, _sa.AuditEventHandler );
			SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Enabled, _sa.AuditEventHandler );
			SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Visible, _sa.AuditEventHandler );
		}


		private void PropagateSecurity(AceTypes AceType)
		{
			IEnumerator sControls = this.SecureControls.Values.GetEnumerator();
			while( sControls.MoveNext() )
			{
				ISecureControl sc = (ISecureControl)sControls.Current;
				this.SecurityDescriptor.CopyTo( sc.SecurityDescriptor, AceType, false );
				sc.ApplySecurity( AceType );
			}
		}


		[DefaultValue(false)]
		public override bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				if ( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					if( base.Visible != value )
					{
						base.Visible = value;
						OnVisibleChanged();
					}
				}
			}
		}


		#endregion



		protected override void AddedControl(Control control, int index)
		{
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				if( ReflectionUtils.ImplementsInterface( control, "mUtilities.Forms.IValidationControl" ) )
				{
					this.ValidationControls.Add( ((IValidationControl)control).UniqueName, (IValidationControl)control );

					if( this.AutoValidateContainer )
					{
						((IValidationControl)control).ValidateCompleted += 
							new ValidateCompletedEventHandler( this.ValidationControl_ValidateCompleted );
					}
				}
			
				if( ReflectionUtils.ImplementsInterface( control, "mUtilities.Forms.ISecureControl" ) )
				{
					this.SecureControls.Add( ((ISecureControl)control).UniqueName, (ISecureControl)control );
				}
			
				base.AddedControl( control, index );
			}
		}


		protected override void RemovedControl(Control control)
		{
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				if( ReflectionUtils.ImplementsInterface( control, "mUtilities.Forms.IValidationControl" ) )
				{
					this.ValidationControls.Remove( ((IValidationControl)control).UniqueName );
				}
			
				if( ReflectionUtils.ImplementsInterface( control, "mUtilities.Forms.ISecureControl" ) )
				{
					this.SecureControls.Remove( ((ISecureControl)control).UniqueName );
				}
			
				base.RemovedControl( control );
			}
		}



		public mTableCellCollection mCells
		{
			get
			{
				return _cells;
			}
		}
	}

	#endregion


	#region TableCell

	public class mTableCell : System.Web.UI.WebControls.TableCell, IValidationControl, ISecureControl, ISecureContainer
	{
		private string						_uniqueName					= null;
		private TypeCode					_typeCode					= TypeCode.String;
		private ValidationRuleCollection	_ValidationRules			= new ValidationRuleCollection();
		private FillMapCollection			_FillMaps					= new FillMapCollection();
		private ValidationControlCollection	_ValidationControls			= new ValidationControlCollection();
		private bool						_Validated					= false;
		private bool						_ProcessFillMaps			= true;
		private ControlEventBindings		_EventBindings				= new ControlEventBindings();
		private bool						_AllowUndeclaredControls	= false;

		private bool						_AutoValidateContainer			= true;
		private bool						_AutoValidateDisplayMessages	= true;
		private bool						_ValidationFailed				= false;
		private ValidationErrorCollection	_ValidationErrors				= new ValidationErrorCollection();
		private mValidationSummary			_ValidationSummaryControl		= null;
		
		private DataAccessLayer				_dal						= new DataAccessLayer();
		private DataAccessor				_da							= null;

		private mUtilities.Security.Standard.User	_UserContext	= mUtilities.Security.Standard.User.Unknown;

		private DefaultSecurityStates		_DefaultSecurityState	= DefaultSecurityStates.Unlocked;
		private ISecurityDescriptor			_SecurityDescriptor		= new SecurityDescriptorBase();
		private SecureControlCollection		_SecureControls			= new SecureControlCollection();
		private SecurityResultCollection	_sr				= null;
		private AuditWriter					_auditWriter			= null;
		private AuditEventHandler			_sa.AuditEventHandler		= null;
		private RightRoleCollection			_RightRoles				= new RightRoleCollection();
		private AuditType					_auditType				= AuditType.ControlDetail;
		private AceTypes[]					_nativeAceTypes			= new AceTypes[1] { AceType.UI };


		//this is a subset of ControlEvents:
		//	- every overridden event should have an entry here
		//  - every entry here MUST exist in ControlEvents
		public enum BindableEvents
		{
			Initialize,
			EnabledChanged,
			VisibleChanged,
			TextChanged
		}


		#region Events
		public event ValidateCompletedEventHandler ValidateCompleted;
		public event System.EventHandler AutoValidating;
		public event System.EventHandler AutoValidateCompleted;
		public event AuditEventHandler Audited;

		protected bool OnValidateCompleted(object sender, ValidationArgs e)
		{
			if(ValidateCompleted != null)
			{
				ValidateCompleted(sender, e);
				return true;
			}
			else
			{
				return false;
			}
		}
		

		protected bool OnAutoValidating(object sender, EventArgs e)
		{
			if(AutoValidating != null)
			{
				AutoValidating(sender, e);
				return true;
			}
			else
			{
				return false;
			}
		}
				

		protected bool OnAutoValidateCompleted(object sender, EventArgs e)
		{
			if(AutoValidateCompleted != null)
			{
				AutoValidateCompleted(sender, e);
				return true;
			}
			else
			{
				return false;
			}
		}
				

		private void onAudited(object sender, AuditEventArgs e)
		{
			if( Audited != null )
			{
				Audited( sender, e );
			}
		}
		
		
		/// <summary>
		/// Writes audit, Raises the Audited event.
		/// </summary>
		protected void OnAudited(object sender, AuditEventArgs e)
		{
			if( (this.SecurityDescriptor.Sacl.AuditTypeFilter & e.AuditType) == e.AuditType )
			{
				if( this.AuditWriter != null )
				{
					this.AuditWriter.Write( e );
				}
			
				this.onAudited( sender, e );
			}
		}


		#endregion

		
		
		public mTableCell() : base()
		{
			_sa.AuditEventHandler = new AuditEventHandler(this.OnAudited);
			
			_sr = this.SecurityDescriptor.SecurityResults;
			_sr.AddAceType( AceType.UI, true, false, false );	//manually creates space for UI security info

			this.ApplySecurity( AceType.UI );

			//			base.Visible = false;
			//			base.Enabled = false;
		}


		protected override void OnLoad(EventArgs e)
		{
			if( _DefaultSecurityState != DefaultSecurityStates.Locked )
			{
				object[] rights = AceTypeRights.GetRights( AceType.UI );
				for( int n=0; n<rights.Length; n++ )
				{
					_sr[AceType.UI, rights[n]].AccessAllowed = true;
				}

				base.Visible = true;
				//base.Enabled = true;

				this.ApplySecurity( AceType.UI );
			}

			base.OnLoad (e);
		}


		protected override object SaveViewState()
		{
			object baseState = base.SaveViewState();

			object[] s = new object[11];

			s[0]	= baseState;
			s[1]	= _uniqueName;
			s[2]	= _typeCode;
			s[3]	= _Validated;
			s[4]	= _ProcessFillMaps;
			s[5]	= _AllowUndeclaredControls;
			s[6]	= _AutoValidateContainer;
			s[7]	= _AutoValidateDisplayMessages;
			s[8]	= _ValidationFailed;
			s[9]	= _DefaultSecurityState;
			s[10]	= _auditType;

			return s;
		}


		/// <summary>
		/// Load State from the array of objects that was saved at SavedViewState.
		/// </summary>
		/// <param name="savedState">State information to load.</param>
		protected override void LoadViewState(object savedState) 
		{
			if (savedState != null)
			{
				object[] s = (object[])savedState;

				if ( s[0] != null )
					base.LoadViewState(s[0]);

				if ( s[1] != null )
					_uniqueName = (string)s[1];

				if ( s[2] != null )
					_typeCode = (TypeCode)s[2];

				if ( s[3] != null )
					_Validated = (bool)s[3];
				
				if ( s[4] != null )
					_ProcessFillMaps = (bool)s[4];
				
				if ( s[5] != null )
					_AllowUndeclaredControls = (bool)s[5];

				if ( s[6] != null )
					_AutoValidateContainer = (bool)s[6];
			
				if ( s[7] != null )
					_AutoValidateDisplayMessages = (bool)s[7];
			
				if ( s[8] != null )
					_ValidationFailed = (bool)s[8];
			
				if ( s[9] != null )
					_DefaultSecurityState = (DefaultSecurityStates)s[9];

				if ( s[10] != null )
					_auditType = (AuditType)s[10];
			}
		}


		[ParenthesizePropertyName(true)]
		public string UniqueName
		{
			get
			{
				if( _uniqueName == null || _uniqueName.Length == 0 )
				{
					if( this.Parent == null )
					{
						_uniqueName = base.UniqueID;
					}
					else
					{
						_uniqueName = string.Format( "{0}_c{1}",
							((mTableRow)this.Parent).UniqueName, ((mTableRow)this.Parent).mCells.GetCellIndex( this ) );
					}
				}

				return _uniqueName;
			}
			set
			{
				_uniqueName = value;
			}
		}


		public TypeCode DataType
		{
			get
			{
				return _typeCode;
			}
			set
			{
				_typeCode = value;
			}
		}


		[Browsable(false)]
		public ValidationRuleCollection ValidationRules
		{
			get
			{
				return _ValidationRules;
			}
			set
			{
				_ValidationRules = value;
			}
		}


		[Browsable(false)]
		public FillMapCollection FillMaps
		{
			get
			{
				return _FillMaps;
			}
			set
			{
				_FillMaps = value;
			}
		}


		[Browsable(false)]
		public ValidationControlCollection ValidationControls
		{
			get
			{
				return _ValidationControls;
			}
			set
			{
				_ValidationControls = value;
			}
		}


		[Browsable(false)]
		public mUtilities.Data.DataAccessLayer DataAccessLayer
		{
			get
			{
				return _dal;
			}
			set
			{
				_dal = value;
			}
		}


		[Browsable(false)]
		[Obsolete("User DataAccessLayer instead.", true)]
		[DefaultValue(null)]
		public mUtilities.Data.DataAccessor DataAccessor
		{
			get
			{
				return _da;
			}
			set
			{
				_da = value;
			}
		}


		[Browsable(false)]
		public ControlEventBindings EventBindings
		{
			get
			{
				return _EventBindings;
			}
			set
			{
				_EventBindings = value;
			}
		}

		
		public bool AllowUndeclaredControls
		{
			get
			{
				return _AllowUndeclaredControls;
			}
			set
			{
				_AllowUndeclaredControls = value;
			}
		}


		public bool AutoValidateContainer
		{
			get
			{
				return _AutoValidateContainer;
			}
			set
			{
				_AutoValidateContainer = value;
			}
		}


		public bool AutoValidateDisplayMessages
		{
			get
			{
				return _AutoValidateDisplayMessages;
			}
			set
			{
				_AutoValidateDisplayMessages = value;
			}
		}


		[Browsable(false)]
		public ValidationErrorCollection ValidationErrors
		{
			get
			{
				return _ValidationErrors;
			}
			set
			{
				_ValidationErrors = value;
			}
		}


		public mValidationSummary ValidationSummaryControl
		{
			get
			{
				return _ValidationSummaryControl;
			}
			set
			{
				_ValidationSummaryControl = value;
			}
		}


		/// <summary>
		/// Loads rules from a database using the DataAccessor for this control.
		/// Calls ResolveInternalReferences() automatically.
		/// </summary>
		public void LoadRules()
		{
			ValidationRuleBuilder.LoadRulesWeb( this );
		}


		/// <summary>
		/// Loads rules from a rules file.
		/// Calls ResolveInternalReferences() automatically.
		/// </summary>
		/// <param name="ruleFilePath">Path to the rules file.</param>
		public void LoadRules( string ruleFilePath )
		{
			ValidationRuleBuilder.LoadRulesWeb( this, ruleFilePath );
		}


		/// <summary>
		/// Recurses the ValidationControls Collection for this control and
		/// all child controls to resolve internal name references.
		/// </summary>
		public void ResolveInternalReferences()
		{
			ValidationRuleBuilder.ResolveInternalReferences( this );
		}

		
		/// <summary>
		/// Forces manual validation of control text.
		/// </summary>
		/// <param name="ProcessFillMaps">Boolean to specify whether FillMaps should be 
		/// processed on Validation success.
		/// </param>
		public virtual void Validate(bool ProcessFillMaps)
		{
			_ProcessFillMaps = ProcessFillMaps;
			_Validated = false;
			this.OnValidating( new System.ComponentModel.CancelEventArgs() );
		}


	
		/// <summary>
		/// Processes ValidationRules (which internally implements DataTypeCheck) 
		/// and reports success/failure.
		/// </summary>
		private void OnValidating( System.ComponentModel.CancelEventArgs e )
		{
			if ( !_Validated && this.ValidationRules.Count > 0 )	//&& this.Enabled 
			{
				this.ValidationErrors.Clear();

				DataComparer.Result result = 
					this.ValidationRules.Process( null, _typeCode, _dal.Application, ControlEvents.Validating );

				if( result.Success )	//everything passed
				{
					OnValidateCompleted( this, new ValidationArgs() );
					OnValidated();
					e.Cancel = false;

					_Validated = true;
				}
				else					//something failed
				{
					OnValidateCompleted( this, new ValidationArgs(true, result.Message) );
					e.Cancel = true;
				}
			}

			ValidationControl_ValidateChildren();
			e.Cancel = _ValidationFailed;

		}


		/// <summary>
		/// Fires on Validation success.  Processes FillMaps.
		/// </summary>
		private void OnValidated()
		{
			if ( _ProcessFillMaps )
			{
				this.FillMaps.Process( _dal.Application, ControlEvents.Validating );
			}

			_ProcessFillMaps = false;
		}


		private void OnEnabledChanged()
		{
			if ( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				ProcessEvent( ControlEvents.EnabledChanged );
			}
		}


		private void OnVisibleChanged()
		{
			if ( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
			{
				ProcessEvent( ControlEvents.VisibleChanged );
			}
		}

		
		private void ProcessEvent( ControlEvents EventBinding )
		{
			if( this.EventBindings.ValidationEvents.Contains( EventBinding ) )
			{
				DataComparer.Result result = 
					this.ValidationRules.Process( null, _typeCode, _dal.Application, EventBinding  );

				if( result.Success )	//everything passed
				{
					OnValidateCompleted( this, new ValidationArgs() );
				}
				else					//something failed
				{
					OnValidateCompleted( this, new ValidationArgs(true, result.Message) );
				}
			}

			if( this.EventBindings.FillMapEvents.Contains( EventBinding ) )
			{
				this.FillMaps.Process( _dal.Application, EventBinding );
			}
		}

		
		private void ValidationControl_ValidateChildren()
		{
			if( this.AutoValidateContainer )
			{
				OnAutoValidating( this, EventArgs.Empty );

				this.ValidationErrors.Clear();

				_ValidationFailed = false;

				IEnumerator vctls = this.ValidationControls.Values.GetEnumerator();
				while( vctls.MoveNext() )
				{
					((IValidationControl)vctls.Current).Validate( false );
				}

				if( _ValidationSummaryControl != null )
				{
					if( _ValidationSummaryControl.ErrorCount > 0 )
					{
						//_ValidationSummaryControl.Refresh();

						_ValidationSummaryControl.Title = "You have a validation error!";
						_ValidationSummaryControl.Visible = true;
					}
					else
					{
						_ValidationSummaryControl.Visible = false;
					}
				}

				OnAutoValidateCompleted( this, new EventArgs() );
			}
		}

		
		private void ValidationControl_ValidateCompleted(object sender, ValidationArgs e)
		{
			Control ctl = (Control)sender;
			if( e.Error )
			{
				if( this.AutoValidateDisplayMessages )
				{
					//this._err_Validation.SetError(ctl, e.Message);
				}

				if( _ValidationSummaryControl != null )
				{
					_ValidationSummaryControl.ErrorAdd( (IValidationControl)ctl, e.Message );
				}
				
				OnValidateCompleted( ctl, new ValidationArgs(true, e.Message) );
				
				_ValidationFailed = true;
			}
			else
			{
				if( this.AutoValidateDisplayMessages )
				{
					//this._err_Validation.SetError(ctl, "");
				}

				if( _ValidationSummaryControl != null )
				{
					_ValidationSummaryControl.ErrorRemove( (IValidationControl)ctl );
				}

				OnValidateCompleted( ctl, new ValidationArgs() );
			}
		}

		

		
		#region Security Implementation

		[Browsable(false)]
		public mUtilities.Security.Standard.User UserContext
		{
			get
			{
				return _UserContext;
			}
			set
			{
				_UserContext = value;
			}
		}


		public DefaultSecurityStates DefaultSecurityState
		{
			get
			{
				return _DefaultSecurityState;
			}
			set
			{
				_DefaultSecurityState = value;
			}
		}


		[Browsable(false)]
		public ISecurityDescriptor SecurityDescriptor
		{
			get
			{
				return _SecurityDescriptor;
			}
			set
			{
				_SecurityDescriptor = value;
			}
		}


		[Browsable(false)]
		public SecureControlCollection SecureControls
		{
			get
			{
				return _SecureControls;
			}
			set
			{
				_SecureControls = value;
			}
		}


		[Browsable(false)]
		public AuditWriter AuditWriter
		{
			get
			{
				return _auditWriter;
			}
			set
			{
				_auditWriter = value;
			}
		}


		[Browsable(false)]
		public RightRoleCollection RightRoles
		{
			get
			{
				return _RightRoles;
			}
			set
			{
				_RightRoles = value;
			}
		}


		[Browsable(false)]
		public AceTypes[] NativeAceTypes
		{
			get
			{
				return _nativeAceTypes;
			}
		}


		public virtual void LoadSecurity( mUtilities.Data.DataAccessor da, mUtilities.Security.Standard.User user, AuditEventHandler defaultAuditEventHandler, bool propagateAuditWriter, bool applyDefaultSecurity )
		{
			if( da == null ) da = _dal.Platform;
			SecurityBuilder.LoadRulesFromDB( this, da, user, defaultAuditEventHandler, propagateAuditWriter );

			if( applyDefaultSecurity )
			{
				this.ApplySecurity( AceTypes.Native, null, null );
			}
		}


		public virtual void ResolveRightRoles()
		{
			SecurityBuilder.ResolveRightRoles( this, this.SecureControls );
		}


		public void ApplySecurity(AceTypes AceType)
		{
			this.ApplySecurity( AceType, null, null );
		}

		
		public void ApplySecurity(AceTypes AceType, ArrayList DaclParameters, ArrayList SaclParameters)
		{
			switch( AceType )
			{
				case AceType.UI:
				case AceTypes.Native:
				{
					this.SecurityDescriptor.EvalSecurity( AceType.UI, DaclParameters, SaclParameters );
					this.AuditAccess();
					SecurityBuilder.PropagateSecurity( this );
					break;
				}
			}
		}


		public void AuditAccess()
		{
			//base.Enabled = _sr[AceType.UI, UIRight.Enabled].AccessAllowed;
			base.Visible = _sr[AceType.UI, UIRight.Visible].AccessAllowed;

			_auditType = AuditType.ControlDetail;
			if( !_sr[AceType.UI, UIRight.Operate].AccessAllowed || !_sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				_auditType = AuditType.Warning;
			}

			SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Operate, _sa.AuditEventHandler );
			SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Enabled, _sa.AuditEventHandler );
			SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Visible, _sa.AuditEventHandler );
		}


		private void PropagateSecurity(AceTypes AceType)
		{
			IEnumerator sControls = this.SecureControls.Values.GetEnumerator();
			while( sControls.MoveNext() )
			{
				ISecureControl sc = (ISecureControl)sControls.Current;
				this.SecurityDescriptor.CopyTo( sc.SecurityDescriptor, AceType, false );
				sc.ApplySecurity( AceType );
			}
		}


		[DefaultValue(false)]
		public override bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				if ( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					if( base.Visible != value )
					{
						base.Visible = value;
						OnVisibleChanged();
					}
				}
			}
		}


		#endregion



		protected override void AddedControl(Control control, int index)
		{
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				if( ReflectionUtils.ImplementsInterface( control, "mUtilities.Forms.IValidationControl" ) )
				{
					this.ValidationControls.Add( ((IValidationControl)control).UniqueName, (IValidationControl)control );

					if( this.AutoValidateContainer )
					{
						((IValidationControl)control).ValidateCompleted += 
							new ValidateCompletedEventHandler( this.ValidationControl_ValidateCompleted );
					}
				}
			
				if( ReflectionUtils.ImplementsInterface( control, "mUtilities.Forms.ISecureControl" ) )
				{
					this.SecureControls.Add( ((ISecureControl)control).UniqueName, (ISecureControl)control );
				}
			
				base.AddedControl( control, index );
			}
		}


		protected override void RemovedControl(Control control)
		{
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				if( ReflectionUtils.ImplementsInterface( control, "mUtilities.Forms.IValidationControl" ) )
				{
					this.ValidationControls.Remove( ((IValidationControl)control).UniqueName );
				}
			
				if( ReflectionUtils.ImplementsInterface( control, "mUtilities.Forms.ISecureControl" ) )
				{
					this.SecureControls.Remove( ((ISecureControl)control).UniqueName );
				}
			
				base.RemovedControl( control );
			}
		}


	
	}

	#endregion


	#region TableRowCollection


	public class mTableRowCollection : CollectionBase
	{
		private mTable _owner = null;
		private TableRowCollection _rows = null;

		public mTableRowCollection(mTable owner)
		{
			_owner = owner;
			_rows = ((Table)owner).Rows;
		}


		public mTableRow this[ int index ]  
		{
			get  
			{
				return( (mTableRow)_rows[index] );
			}
		}

		public int Add( mTableRow value )  
		{
			_owner.ValidationControls.Add( value );
			return( _rows.Add( value ) );
		}

		public void AddAt( int index, mTableRow value )  
		{
			_owner.ValidationControls.Add( value );
			_rows.AddAt( index, value );
		}

		public int GetRowIndex( mTableRow value )  
		{
			return( _rows.GetRowIndex( value ) );
		}

		public void Remove( mTableRow value )  
		{
			_owner.ValidationControls.Remove( value.UniqueName );
			_rows.Remove( value );
		}


		new public IEnumerator GetEnumerator()
		{
			return _rows.GetEnumerator();
		}
	}


	#endregion


	#region TableCellCollection


	public class mTableCellCollection : CollectionBase
	{
		private mTableRow _owner = null;
		private TableCellCollection _cells = null;

		public mTableCellCollection(mTableRow owner)
		{
			_owner = owner;
			_cells = ((TableRow)owner).Cells;
		}


		public mTableCell this[ int index ]  
		{
			get  
			{
				return( (mTableCell)_cells[index] );
			}
		}

		public int Add( mTableCell value )  
		{
			_owner.ValidationControls.Add( value );
			return( _cells.Add( value ) );
		}

		public void AddAt( int index, mTableCell value )  
		{
			_owner.ValidationControls.Add( value );
			_cells.AddAt( index, value );
		}

		public int GetCellIndex( mTableCell value )  
		{
			return( _cells.GetCellIndex( value ) );
		}

		public void Remove( mTableCell value )  
		{
			_owner.ValidationControls.Remove( value.UniqueName );
			_cells.Remove( value );
		}


		new public IEnumerator GetEnumerator()
		{
			return _cells.GetEnumerator();
		}
	}


	#endregion
}