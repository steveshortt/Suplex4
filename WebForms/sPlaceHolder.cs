using System;
using System.Reflection;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Collections;

using mUtilities.Data;
using mUtilities.Forms;
using mUtilities.Security;
using mUtilities.Security.Standard;

namespace System.Web.UI.WebControls
{
	/// <summary>
	/// Provides secuirty, extended validation properties, and auto-validation.
	/// </summary>
	[ToolboxItem(true),
	   System.Drawing.ToolboxBitmap( typeof( System.Web.UI.WebControls.PlaceHolder ) )]
	public class mPlaceHolder : System.Web.UI.WebControls.PlaceHolder, IValidationControl, ISecureControl, ISecureContainer, IWebExtra
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

		private DataAccessLayer				_dal						= new DataAccessLayer();
		private DataAccessor				_da							= null;

		private User						_UserContext				= User.Unknown;
		private DefaultSecurityStates		_DefaultSecurityState		= DefaultSecurityStates.Locked;
		private ISecurityDescriptor			_SecurityDescriptor			= new SecurityDescriptorBase();
		private SecureControlCollection		_SecureControls				= new SecureControlCollection();
		private SecurityResultCollection	_sr					= null;
		private AuditWriter					_auditWriter				= null;
		private AuditEventHandler			_sa.AuditEventHandler			= null;
		private RightRoleCollection			_RightRoles					= new RightRoleCollection();
		private AuditType					_auditType					= AuditType.ControlDetail;
		private AceType[]					_nativeAceTypes				= new AceType[1] { AceType.UI };


		private object _tagObject = null;
		private string _tag = null;
		private bool _allowExtendedViewState = false;


		private bool _includeInHierarchy = false;


		//this is a subset of ControlEvents:
		// - every overridden event should have an entry here
		// - every entry here MUST exist in ControlEvents
		public enum BindableEvents
		{
			Initialize,
			Validating,
			EnabledChanged,
			VisibleChanged,
			Click,
			TextChanged
		}


		#region Events
		public event ValidateCompletedEventHandler ValidateCompleted;
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



		public mPlaceHolder() : base()
		{
			_sa.AuditEventHandler = new AuditEventHandler( this.OnAudited );

			_sr = this.SecurityDescriptor.SecurityResults;
			_sr.AddAceType( AceType.UI, true, false, false ); //manually creates space for UI security info
		}

		protected override void OnInit(EventArgs e)
		{
			//this.AuditAccess();

			base.OnInit( e );
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

				//this.AuditAccess();
			}

			base.OnLoad (e);
		}

		protected override void OnPreRender(EventArgs e)
		{
			if( !this.DesignMode )
			{
				if( !_sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					base.Visible = false;
				}
			}

			base.OnPreRender( e );
		}

		protected override object SaveViewState()
		{
			if( _allowExtendedViewState )
			{
				object baseState = base.SaveViewState();

				object[] s = new object[9];

				s[0] = baseState;
				s[1] = _uniqueName;
				s[2] = _typeCode;
				s[3] = _Validated;
				s[4] = _ProcessFillMaps;
				s[5] = _AllowUndeclaredControls;
				s[6] = _DefaultSecurityState;
				s[7] = _auditType;
				s[8] = _tag;

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
						_DefaultSecurityState = (DefaultSecurityStates)s[6];

					if( s[7] != null )
						_auditType = (AuditType)s[7];

					if( s[8] != null )
						_tag = (string)s[8];
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


		public string ToolTip
		{
			get
			{
				return null;
			}
			set
			{
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
			if ( !_Validated && this.ValidationRules.Count > 0 )
			{
				DataComparer.Result result =
					this.ValidationRules.Process( null, _typeCode, _dal.Application, ControlEvents.Validating );

				if( result.Success ) //everything passed
				{
					OnValidateCompleted( this, new ValidationArgs() );
					OnValidated();
					e.Cancel = false;

					_Validated = true;
				}
				else //something failed
				{
					OnValidateCompleted( this, new ValidationArgs(true, result.Message) );
					e.Cancel = true;
				}
			}
		}


		/// <summary>
		/// Fires on Validation success. Processes FillMaps.
		/// </summary>
		private void OnValidated()
		{
			if ( _ProcessFillMaps )
			{
				this.FillMaps.Process( _dal.Application, ControlEvents.Validating );
			}

			_ProcessFillMaps = false;
		}


		//protected override void OnClick(EventArgs e)
		//{
		//    SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, "Clicked." );

		//    if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
		//    {
		//        ProcessEvent( ControlEvents.Click );
		//        base.OnClick( e );
		//    }
		//}


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
					this.ValidationRules.Process( null, _typeCode, _dal.Application, EventBinding );

				if( result.Success ) //everything passed
				{
					OnValidateCompleted( this, new ValidationArgs() );
				}
				else //something failed
				{
					OnValidateCompleted( this, new ValidationArgs(true, result.Message) );
				}
			}

			if( this.EventBindings.FillMapEvents.Contains( EventBinding ) )
			{
				this.FillMaps.Process( _dal.Application, EventBinding );
			}
		}



		#region Security Implementation

		[Browsable(false)]
		public User UserContext
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
		public AceType[] NativeAceTypes
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
				this.ApplySecurity( AceType.Native, null, null );
			}
		}


		public virtual void ResolveRightRoles()
		{
			SecurityBuilder.ResolveRightRoles( this, this.SecureControls );
		}


		public void ApplySecurity(AceType AceType)
		{
			this.ApplySecurity( AceType, null, null );
		}


		public void ApplySecurity(AceType AceType, ArrayList DaclParameters, ArrayList SaclParameters)
		{
			switch( AceType )
			{
				case AceType.UI:
				case AceType.Native:
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
			_auditType = AuditType.ControlDetail;
			if( !_sr[AceType.UI, UIRight.Operate].AccessAllowed || !_sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				_auditType = AuditType.Warning;
			}

			SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Operate, _sa.AuditEventHandler );
			SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Enabled, _sa.AuditEventHandler );
			SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Visible, _sa.AuditEventHandler );
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


		//[DefaultValue(false)]
		//public override bool Enabled
		//{
		//    get
		//    {
		//        return base.Enabled;
		//    }
		//    set
		//    {
		//        if ( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
		//        {
		//            if( base.Enabled != value )
		//            {
		//                base.Enabled = value;
		//                OnEnabledChanged();
		//            }
		//        }
		//    }
		//}


		#endregion

		protected override void AddedControl(Control control, int index)
		{
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				if( control is IValidationControl )
				{
					if( !_includeInHierarchy && ( this.Parent is IValidationControl ) )
					{
						((IValidationControl)this.Parent).ValidationControls.Add( ( (IValidationControl)control ).UniqueName, (IValidationControl)control );
					}
					else
					{
						this.ValidationControls.Add( ( (IValidationControl)control ).UniqueName, (IValidationControl)control );
					}
				}

				if( control is ISecureControl )
				{
					if( !_includeInHierarchy && ( this.Parent is ISecureControl ) )
					{
						( (ISecureControl)this.Parent ).SecureControls.Add( ( (ISecureControl)control ).UniqueName, (ISecureControl)control );
					}
					else
					{
						this.SecureControls.Add( ( (ISecureControl)control ).UniqueName, (ISecureControl)control );
					}
				}

				base.AddedControl( control, index );
			}
		}


		protected override void RemovedControl(Control control)
		{
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				if( control is IValidationControl )
				{
					this.ValidationControls.Remove( ( (IValidationControl)control ).UniqueName );
				}

				if( control is ISecureControl )
				{
					this.SecureControls.Remove( ( (ISecureControl)control ).UniqueName );
				}

				base.RemovedControl( control );
			}
		}


		public bool IncludeInHierarchy
		{
			get { return _includeInHierarchy; }
			set { _includeInHierarchy = value; }
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
}