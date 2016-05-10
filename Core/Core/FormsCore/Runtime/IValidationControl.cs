using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;

using WinForms = System.Windows.Forms;
using WebForms = System.Web.UI;
using Wpf = System.Windows;


using Suplex.Data;
using Suplex.Security;
using api = Suplex.Forms.ObjectModel.Api;

namespace Suplex.Forms
{
	public interface IValidationControl : ISecureControl
	{
		IValidationAccessor Validation { get; }

		//NOTE: I ran into a naming conflict with WinForms.ContainerControls in 2.0, 
		//	where .NET added a Validate(bool) method overload, so I renamed to ProcessValidate.
		ValidationResult ProcessValidate(bool processFillMaps);
	}

	public interface IValidationContainer : IValidationControl, ISecureContainer
	{
		void AddChild(IValidationControl control);
	}

	public interface IValidationSummaryControl
	{
		void SetError(IValidationControl control, string errorMessage);
		bool Visible { get; set; }
	}

	public interface IValidationTextBox : IValidationControl
	{
		string FormatString { get; set; }
	}

	public interface IValidationAccessor
	{
		//events
		event ValidateCompletedEventHandler ValidateCompleted;


		//standard props
		ValidationRuleCollection ValidationRules { get; }
		FillMapCollection FillMaps { get; }
		ControlEventBindings EventBindings { get; }
		TypeCode DataType { get; set; }
		string DataTypeErrorMessage { get; set; }
		string ToolTip { get; set; }

		//methods
		DataSet Load(string filePath);
		DataSet Load(System.IO.TextReader reader);
		DataSet Load(api.SuplexStore splxStore);
		DataSet Load(DataAccessLayer dal);
		void Load(DataSet validationCache);
		void ResolveInternalReferences();
		ValidationResult ProcessEvent(string defaultCompareValue, ControlEvents eventBinding, bool processFillMaps);

		void SetValidationResultHandlerDelegate(ValidationResultHandlerMethod method);
		//void InvokeValidationResultHandler(ValidationResult vr);


		//the following props are for Containers only
		bool AllowDynamicControls { get; set; }
		bool AutoValidateContainer { get; set; }
		bool AutoValidateDisplayMessages { get; set; }
		List<ValidationResult> Results { get; }
		IValidationSummaryControl ValidationSummaryControl { get; set; }
	}


	public class ValidationAccessor : IValidationAccessor
	{
		private IValidationControl _owner = null;
		private ValidationRuleBuilder _validationRuleBuilder = new ValidationRuleBuilder();

		private bool _isContainerControl = false;

		private ValidationRuleCollection _rules = null;
		private FillMapCollection _fillMaps = null;
		private ControlEventBindings _eventBindings = null;
		private TypeCode _dataType = TypeCode.String;
		private string _dataTypeErrMsg = string.Empty;
		private string _toolTip = string.Empty;

		private ValidationResultHandlerMethod _vrHandler = null;


		#region Events
		public event ValidateCompletedEventHandler ValidateCompleted;

		protected void OnValidateCompleted(object sender, ValidationArgs e)
		{
			if( ValidateCompleted != null )
			{
				ValidateCompleted( sender, e );
			}
		}
		#endregion


		#region ctors
		internal ValidationAccessor() { }

		public ValidationAccessor(IValidationControl owner, TypeCode dataType)
		{
			_owner = owner;
			this.ControlType = EnumUtil.GetControlType( _owner );
			_isContainerControl = _owner is IValidationContainer;
			_dataType = dataType;

			_rules = new ValidationRuleCollection();
			_fillMaps = new FillMapCollection();
			_eventBindings = new ControlEventBindings();
		}
		#endregion


		#region standard properties
		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual ValidationRuleCollection ValidationRules
		{
			get { return _rules; }
		}

		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual FillMapCollection FillMaps
		{
			get { return _fillMaps; }
		}

		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual ControlEventBindings EventBindings
		{
			get { return _eventBindings; }
		}

		[DefaultValue( TypeCode.String ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual TypeCode DataType
		{
			get { return _dataType; }
			set { _dataType = value; }
		}

		[DefaultValue( "" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual string DataTypeErrorMessage
		{
			get { return _dataTypeErrMsg; }
			set { _dataTypeErrMsg = value; }
		}

		[DefaultValue( "" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual string ToolTip
		{
			get { return _toolTip; }
			set
			{
				_toolTip = value;

				switch( this.ControlType )
				{
					case ControlType.WinForms:
					{
						WinForms.ToolTip t = new WinForms.ToolTip();
						t.SetToolTip( (WinForms.Control)_owner, _toolTip );
						break;
					}
					case ControlType.WebForms:
					{
						( (WebForms.WebControls.WebControl)_owner ).ToolTip = _toolTip;
						break;
					}
					case ControlType.Wpf:
					{
						if( _owner is Wpf.FrameworkElement )
						{
							( (Wpf.FrameworkElement)_owner ).ToolTip = _toolTip;
						}
						else if( _owner is Wpf.FrameworkContentElement )
						{
							( (Wpf.FrameworkContentElement)_owner ).ToolTip = _toolTip;
						}
						break;
					}
				}
			}
		}

		//internal ValidationResultHandlerMethod ValidationResultHandler { get { return _vrHandler; } }
		//internal bool HasValidationResultHandler { get { return _vrHandler != null; } }

		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IValidationControl Owner { get { return _owner; } }

		[Browsable( false )]
		public bool IsContainerControl { get { return ( _isContainerControl ); } }


		internal ControlType ControlType { get; set; }
		#endregion


		#region methods
		#region Load
		public virtual DataSet Load(string filePath)
		{
			DataSet validationCache = _validationRuleBuilder.CreateCache( filePath );
			this.Load( validationCache );
			return validationCache;
		}

		public virtual DataSet Load(System.IO.TextReader reader)
		{
			DataSet validationCache = _validationRuleBuilder.CreateCache( reader );
			this.Load( validationCache );
			return validationCache;
		}

		public virtual DataSet Load(api.SuplexStore splxStore)
		{
			DataSet validationCache = _validationRuleBuilder.CreateCache( splxStore );
			this.Load( validationCache );
			return validationCache;
		}

		public virtual DataSet Load(DataAccessLayer dal)
		{
			DataSet validationCache = _validationRuleBuilder.CreateCache( dal.Platform, _owner.UniqueName );
			this.Load( validationCache );
			return validationCache;
		}

		public virtual void Load(DataSet validationCache)
		{
			_validationRuleBuilder.LoadRulesFromCache( _owner, validationCache );
		}
		#endregion

		public virtual void ResolveInternalReferences()
		{
			_validationRuleBuilder.ResolveInternalReferences( _owner );
		}

		//string GetDefaultCompareValue(); //was interface declaration
		//public virtual ValidationResult Validate(bool processFillMaps)
		//{
		//    return this.ProcessEvent( _owner.GetDefaultCompareValue(), ControlEvents.Validating, processFillMaps );
		//}

		public virtual ValidationResult ProcessEvent(string defaultCompareValue, ControlEvents eventBinding, bool processFillMaps)
		{
			//NOTE: optimistic result initialization here: if no rules to process, don't throw an error
			//	12/26/2008, modified ValidationResult to be pessimistic, this is one of a few that needs optimistic init
			ValidationResult vr = new ValidationResult( _owner.UniqueName, true, string.Empty );
			bool ok = true;

			if( _eventBindings.ValidationEvents.Contains( eventBinding ) )
			{
				//thsi step will re-initialize the ValidationResult, so re-assign need to _owner.UniqueName after
				vr = _rules.Process( defaultCompareValue, _dataType,
					_owner.Validation.DataTypeErrorMessage, _owner.DataAccessLayer.Application, eventBinding );
				vr.UniqueName = _owner.UniqueName;

				if( eventBinding == ControlEvents.Validating )
				{
					this.OnValidateCompleted( _owner, new ValidationArgs( _owner, vr ) );

					if( !vr.Success )
					{
						ok = false;	//don't process FillMaps when Validating event fails
					}
				}

				if( _vrHandler != null )
				{
					_vrHandler.Invoke( _owner, vr );
				}
			}


			if( ok && processFillMaps && _eventBindings.FillMapEvents.Contains( eventBinding ) )
			{
				this.FillMaps.Process( _owner.DataAccessLayer.Application, eventBinding );
			}

			if( this.IsContainerControl && eventBinding == ControlEvents.Validating )
			{
				( (ValidationContainerAccessor)this ).ProcessChildValidation( processFillMaps );
			}

			return vr;
		}

		public virtual void SetValidationResultHandlerDelegate(ValidationResultHandlerMethod method)
		{
			_vrHandler = method;
		}

		internal virtual ValidationResultHandlerMethod CurrentValidationResultHandlerMethod
		{
			get { return _vrHandler; }
		}

		//public void InvokeValidationResultHandler(ValidationResult vr)
		//{
		//    if( _vrHandler != null )
		//    {
		//        _vrHandler.Invoke( _owner, vr );
		//    }
		//}
		#endregion


		#region IValidationAccessor Members (not implemented)
		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual bool AllowDynamicControls
		{
			get
			{
				return false;
			}
			set
			{
				//throw new Exception( "The method or operation is not implemented." );
			}
		}

		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual bool AutoValidateContainer
		{
			get
			{
				return false;
			}
			set
			{
				//throw new Exception( "The method or operation is not implemented." );
			}
		}

		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual bool AutoValidateDisplayMessages
		{
			get
			{
				return false;
			}
			set
			{
				//throw new Exception( "The method or operation is not implemented." );
			}
		}

		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual List<ValidationResult> Results
		{
			//get { throw new Exception( "The method or operation is not implemented." ); }
			get { return null; }
		}

		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual IValidationSummaryControl ValidationSummaryControl
		{
			get
			{
				//throw new Exception( "The method or operation is not implemented." );
				return null;
			}
			set
			{
				//throw new Exception( "The method or operation is not implemented." );
			}
		}
		#endregion
	}//class

	public class ValidationContainerAccessor : ValidationAccessor
	{
		private bool _allowDynamicControls = false;
		private bool _autoValidateContainer = false;
		private bool _autoValidateDisplayMessages = true;
		private bool _childValidationSuccess = false;
		private List<ValidationResult> _validationResults = null;
		private IValidationSummaryControl _validationSummaryControl = null;
		private bool _hasValidationSummaryControl = false;
		private WinForms.ErrorProvider _errorProvider = null;

		private ValidationResultHandlerMethod _vrHandler = null;

		private EnumUtil _enumUtil = new EnumUtil();


		public ValidationContainerAccessor(IValidationContainer owner, TypeCode dataType)
			: base( owner, dataType )
		{
			_validationResults = new List<ValidationResult>();

			if( this.ControlType == ControlType.WinForms )
			{
				_errorProvider = new WinForms.ErrorProvider();
				if( this.Owner is WinForms.ContainerControl )
				{
					_errorProvider.ContainerControl = (WinForms.ContainerControl)this.Owner;
				}
			}
		}


		[DefaultValue( TypeCode.Empty ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public override TypeCode DataType
		{
			get { return base.DataType; }
			set { base.DataType = value; }
		}


		#region ValidationContainer Extensions
		#region Events
		public event System.EventHandler AutoValidating;
		public event System.EventHandler AutoValidateCompleted;

		internal protected void OnAutoValidating(object sender, EventArgs e)
		{
			if( AutoValidating != null )
			{
				AutoValidating( sender, e );
			}
		}

		internal protected void OnAutoValidateCompleted(object sender, EventArgs e)
		{
			if( AutoValidateCompleted != null )
			{
				AutoValidateCompleted( sender, e );
			}
		}
		#endregion

		[DefaultValue( false )]
		public override bool AllowDynamicControls
		{
			get { return _allowDynamicControls; }
			set { _allowDynamicControls = value; }
		}

		[DefaultValue( false )]
		public override bool AutoValidateContainer
		{
			get { return _autoValidateContainer; }
			set { _autoValidateContainer = value; }
		}

		[DefaultValue( true )]
		public override bool AutoValidateDisplayMessages
		{
			get { return _autoValidateDisplayMessages; }
			set { _autoValidateDisplayMessages = value; }
		}

		[Browsable( false )]
		public override List<ValidationResult> Results
		{
			get { return _validationResults; }
		}

		[DefaultValue( null )]
		public override IValidationSummaryControl ValidationSummaryControl
		{
			get { return _validationSummaryControl; }
			set
			{
				_validationSummaryControl = value;
				_hasValidationSummaryControl = value != null;
			}
		}

		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual bool ChildValidationSuccess { get { return _childValidationSuccess; } }

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual WinForms.ErrorProvider ErrorProvider
		{
			get { return _errorProvider; }
			set
			{
				if( !( this.ControlType == ControlType.WinForms ) || !( this.Owner is WinForms.ContainerControl ) )
				{
					throw new
						NotSupportedException( "ErrorProvider is only supported for Windows-Forms ContainerControls." );
				}

				_errorProvider = value;
				if( _errorProvider != null )
				{
					_errorProvider.ContainerControl = (WinForms.ContainerControl)this.Owner;
				}
			}
		}
		#endregion

		public virtual void ProcessChildValidation(bool processFillMaps)
		{
			_childValidationSuccess = true;

			if( _autoValidateContainer )
			{
				this.OnAutoValidating( this, EventArgs.Empty );

				_validationResults.Clear();

				this.ValidateChildrenRecursive( this.Owner, processFillMaps );

				if( _hasValidationSummaryControl )
				{
					_validationSummaryControl.Visible = !_childValidationSuccess;
				}

				this.OnAutoValidateCompleted( this, new EventArgs() );
			}
		}

		private void ValidateChildrenRecursive(object parent, bool processFillMaps)
		{
			IEnumerator children = _enumUtil.GetChildren( parent ).GetEnumerator();
			while( children.MoveNext() )
			{
				if( children.Current is IValidationControl )
				{
					ValidationResult vr =
						( (IValidationControl)children.Current ).ProcessValidate( processFillMaps );

					if( vr != null )
					{
						_validationResults.Add( vr );
					}
				}

				this.ValidateChildrenRecursive( children.Current, processFillMaps );
			}
		}

		internal override ValidationResultHandlerMethod CurrentValidationResultHandlerMethod
		{
			get
			{
				return base.CurrentValidationResultHandlerMethod == null
					? this.DefaultValidationResultHandler : base.CurrentValidationResultHandlerMethod;
			}
		}

		public virtual void DefaultValidationResultHandler(IValidationControl control, ValidationResult vr)
		{
			if( !vr.Success )
			{
				_childValidationSuccess = false;
			}

			IValidationControl errorControl = vr.ErrorControl != null ? vr.ErrorControl : control;

			if( this.ControlType == ControlType.WinForms && _autoValidateDisplayMessages && _errorProvider != null )
			{
				_errorProvider.SetError( (WinForms.Control)errorControl, vr.Message );
			}

			if( _hasValidationSummaryControl )
			{
				_validationSummaryControl.SetError( errorControl, vr.Message );
			}
		}
	}
}



/*
 * deprecated
 * 
public interface _IValidationControl //: ImControl
{
	event ValidateCompletedEventHandler ValidateCompleted;

	string UniqueName { get; set; }

	TypeCode DataType { get; set; }

	ValidationRuleCollection ValidationRules { get; set; }

	FillMapCollection FillMaps { get; set; }

	ValidationControlCollection ValidationControls { get; set; }

	Suplex.Data.DataAccessLayer DataAccessLayer { get; set; }

	//Suplex.Data.DataAccessor DataAccessor { get; set; }

	ControlEventBindings EventBindings { get; set; }

	bool AllowUndeclaredControls { get; set; }

	string ToolTip { get; set; }


	void LoadRules();

	void LoadRules(string filePath);

	void ResolveInternalReferences();

	//void ValidateData(object sender, System.ComponentModel.CancelEventArgs e);
	void Validate(bool ProcessFillMaps);

}//interface
 */
