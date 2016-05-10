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


using System.Diagnostics;



namespace Suplex.WinForms
{
	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap( typeof( Suplex.WinForms.sRecordManagerCpnt ), "Resources.mRecordManager.gif" )]
	public class sRecordManagerCpnt : System.ComponentModel.Component, IValidationControl, ISecureControl, IRecordManager, ISecurityExtender
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private AceType[] _nativeAceTypes = new AceType[1] { AceType.Record };


		private string						_ToolTip					= null;
		private TypeCode					_typeCode					= TypeCode.Object;
		private ValidationRuleCollection	_ValidationRules			= new ValidationRuleCollection();
		private FillMapCollection			_FillMaps					= new FillMapCollection();
		private ValidationControlCollection	_ValidationControls			= new ValidationControlCollection();
		private bool						_Validated					= false;
		private bool						_ProcessFillMaps			= true;
		private ControlEventBindings		_EventBindings				= new ControlEventBindings();
		private bool						_AllowUndeclaredControls	= false;


		private string						_text					= string.Empty;
		private object						_value					= null;

		private RecordMode					_recordMode				= RecordMode.None;

		private string						_hierarchyPath			= string.Empty;
		private ISecureControl _parentIm = null;
		private string						_UniqueNameDefault		= DateTime.Now.Ticks.ToString();


		//this is a subset of ControlEvents:
		//	- every overridden event should have an entry here
		//  - every entry here MUST exist in ControlEvents
		public enum BindableEvents
		{
			ListRecords,
			SelectRecord,
			InsertRecord,
			UpdateRecord,
			DeleteRecord,
			Initialize,
			Validating,
			Enter,
			Leave,
			TextChanged,
			ValueChanged
		}


		#region Events
		public event ValidateCompletedEventHandler ValidateCompleted;

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
		#endregion


		#region IRecordManager2 Events

		public event Suplex.Forms.RaiseRecordEventHandler ListRecords;
		public event Suplex.Forms.RaiseRecordEventHandler SelectRecord;
		public event Suplex.Forms.RaiseRecordEventHandler InsertRecord;
		public event Suplex.Forms.RaiseRecordEventHandler UpdateRecord;
		public event Suplex.Forms.RaiseRecordEventHandler DeleteRecord;

		public event Suplex.Forms.RecordEventHandler ListedRecords;
		public event Suplex.Forms.RecordEventHandler SelectedRecord;
		public event Suplex.Forms.RecordEventHandler InsertedRecord;
		public event Suplex.Forms.RecordEventHandler UpdatedRecord;
		public event Suplex.Forms.RecordEventHandler DeletedRecord;


		protected virtual void OnListRecords(RaiseRecordEventArgs e)
		{
			if( ListRecords != null )
			{
				ListRecords( this, e );
			}
		}


		protected virtual void OnSelectRecord(RaiseRecordEventArgs e)
		{
			if( SelectRecord != null )
			{
				SelectRecord( this, e );
			}
		}


		protected virtual void OnInsertRecord(RaiseRecordEventArgs e)
		{
			if( InsertRecord != null )
			{
				InsertRecord( this, e );
			}
		}


		protected virtual void OnUpdateRecord(RaiseRecordEventArgs e)
		{
			if( UpdateRecord != null )
			{
				UpdateRecord( this, e );
			}
		}


		protected virtual void OnDeleteRecord(RaiseRecordEventArgs e)
		{
			if( DeleteRecord != null )
			{
				DeleteRecord( this, e );
			}
		}



		protected virtual void OnListedRecords(RecordEventArgs e)
		{
			if( ListedRecords != null )
			{
				ListedRecords( this, e );
			}
		}


		protected virtual void OnSelectedRecord(RecordEventArgs e)
		{
			if( SelectedRecord != null )
			{
				SelectedRecord( this, e );
			}
		}


		protected virtual void OnInsertedRecord(RecordEventArgs e)
		{
			if( InsertedRecord != null )
			{
				InsertedRecord( this, e );
			}
		}


		protected virtual void OnUpdatedRecord(RecordEventArgs e)
		{
			if( UpdatedRecord != null )
			{
				UpdatedRecord( this, e );
			}
		}


		protected virtual void OnDeletedRecord(RecordEventArgs e)
		{
			if( DeletedRecord != null )
			{
				DeletedRecord( this, e );
			}
		}


		#endregion


		private sRecordManagerCpnt(){}


		public sRecordManagerCpnt(System.ComponentModel.IContainer container)
		{
			container.Add( this );

			_sa = new SecurityAccessor( this, AceType.Record );
			_sr = _sa.Descriptor.SecurityResults;
		}



		#region IRecordManager2 Members

		public void RaiseListRecords(bool process, params object[] parameters)
		{
			if( process )
			{
				if ( _sr[AceType.Record, RecordRight.List].AccessAllowed )
				{
					_recordMode = RecordMode.List;
					RecordEventArgs r = ProcessEvent( ControlEvents.ListRecords );

					string msg = "ListRecords completed successfully.";
					if( r.Exception != null ) msg = "ListRecords completed with errors.";
					SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName, msg );

					OnListedRecords( r );
				}
				else
				{
					SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName,
						"ListRecords failed: access denied." );

					OnListedRecords( RecordEventArgs.Empty );
				}
			}
			else
			{
				SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName,
					"ListRecords deferred to caller." );

				OnListRecords( new RaiseRecordEventArgs( parameters, _sr[AceType.Record, RecordRight.List].AccessAllowed ) );
			}
		}


		public void RaiseSelectRecord(bool process, params object[] parameters)
		{
			if( process )
			{
				if ( _sr[AceType.Record, RecordRight.Select].AccessAllowed )
				{
					_recordMode = RecordMode.Select;
					RecordEventArgs r = ProcessEvent( ControlEvents.SelectRecord );

					string msg = "SelectRecord completed successfully.";
					if( r.Exception != null ) msg = "SelectRecord completed with errors.";
					SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName, msg );

					OnSelectedRecord( r );
				}
				else
				{
					SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName,
						"SelectRecord failed: access denied." );

					OnSelectedRecord( RecordEventArgs.Empty );
				}
			}
			else
			{
				SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName,
					"SelectRecord deferred to caller." );

				OnSelectRecord( new RaiseRecordEventArgs( parameters, _sr[AceType.Record, RecordRight.Select].AccessAllowed ) );
			}
		}


		public void RaiseInsertRecord(bool process, params object[] parameters)
		{
			if( process )
			{
				if ( _sr[AceType.Record, RecordRight.Insert].AccessAllowed )
				{
					_recordMode = RecordMode.Insert;
					RecordEventArgs r = ProcessEvent( ControlEvents.InsertRecord );

					string msg = "InsertRecord completed successfully.";
					if( r.Exception != null ) msg = "InsertRecord completed with errors.";
					SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName, msg );

					OnInsertedRecord( r );
				}
				else
				{
					SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName,
						"InsertRecord failed: access denied." );

					OnInsertedRecord( RecordEventArgs.Empty );
				}
			}
			else
			{
				SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName,
					"InsertRecord deferred to caller." );

				OnInsertRecord( new RaiseRecordEventArgs( parameters, _sr[AceType.Record, RecordRight.Insert].AccessAllowed ) );
			}
		}


		public void RaiseUpdateRecord(bool process, params object[] parameters)
		{
			if( process )
			{
				if ( _sr[AceType.Record, RecordRight.Update].AccessAllowed )
				{
					_recordMode = RecordMode.Update;
					RecordEventArgs r = ProcessEvent( ControlEvents.UpdateRecord );

					string msg = "UpdateRecord completed successfully.";
					if( r.Exception != null ) msg = "UpdateRecord completed with errors.";
					SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName, msg );

					OnUpdatedRecord( r );
				}
				else
				{
					SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName,
						"UpdateRecord failed: access denied." );

					OnUpdatedRecord( RecordEventArgs.Empty );
				}
			}
			else
			{
				SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName,
					"UpdateRecord deferred to caller." );

				OnUpdateRecord( new RaiseRecordEventArgs( parameters, _sr[AceType.Record, RecordRight.Update].AccessAllowed ) );
			}
		}


		public void RaiseDeleteRecord(bool process, params object[] parameters)
		{
			if( process )
			{
				if ( _sr[AceType.Record, RecordRight.Delete].AccessAllowed )
				{
					_recordMode = RecordMode.Delete;
					RecordEventArgs r = ProcessEvent( ControlEvents.DeleteRecord );

					string msg = "DeleteRecord completed successfully.";
					if( r.Exception != null ) msg = "DeleteRecord completed with errors.";
					SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName, msg );

					OnDeletedRecord( r );
				}
				else
				{
					SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName,
						"DeleteRecord failed: access denied." );

					OnDeletedRecord( RecordEventArgs.Empty );
				}
			}
			else
			{
				SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName,
					"DeleteRecord deferred to caller." );

				OnDeleteRecord( new RaiseRecordEventArgs( parameters, _sr[AceType.Record, RecordRight.Delete].AccessAllowed ) );
			}
		}

		
		[DefaultValue(RecordMode.None)]
		public Suplex.Forms.RecordMode RecordMode
		{
			get
			{
				return _recordMode;
			}
			set
			{
				_recordMode = value;
			}
		}


		#endregion


		[ParenthesizePropertyName( true ), Category( "Suplex" )]
		public string UniqueName
		{
			get
			{
				if( string.IsNullOrEmpty( _uniqueName ) )
				{
					_uniqueName = _UniqueNameDefault;
				}

				return _uniqueName;
			}
			set
			{
				if( _uniqueName != value && value != null && value != string.Empty )
				{
					if( _parentIm != null )
					{
						//if( _parentIm is ISecureControl )
						//{
						//    ((ISecureControl)_parentIm).SecureControls.Remove( _uniqueName );
						//    ((ISecureControl)_parentIm).SecureControls.Add( value, this );
						//}
						if( _parentIm is IValidationControl )
						{
							((IValidationControl)_parentIm).ValidationControls.Remove( _uniqueName );
							((IValidationControl)_parentIm).ValidationControls.Add( value, this );
						}
					}

					_uniqueName = value;
				}
			}
		}


		[DefaultValue(TypeCode.Object)]
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


		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
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


		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
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


		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Suplex.Data.DataAccessLayer DataAccessLayer
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


		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
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


		[DefaultValue(false)]
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


		[Browsable(false)]
		public string ToolTip
		{
			get
			{
				return _ToolTip;
			}
			set
			{
				_ToolTip = value;
			}
		}


		[Browsable(false)]
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				if( _text != value )
				{
					_text = value;
					OnTextChanged( EventArgs.Empty );
				}
			}
		}


		[Browsable(false)]
		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				if( _value != value )
				{
					_value = value;
					OnValueChanged();
				}
			}
		}


		/// <summary>
		/// Loads rules from a database using the DataAccessor for this control.
		/// Calls ResolveInternalReferences() automatically.
		/// </summary>
		public void LoadRules()
		{
			ValidationRuleBuilder.LoadRules( this );
		}


		/// <summary>
		/// Loads rules from a rules file.
		/// Calls ResolveInternalReferences() automatically.
		/// </summary>
		/// <param name="ruleFilePath">Path to the rules file.</param>
		public void LoadRules( string ruleFilePath )
		{
			ValidationRuleBuilder.LoadRules( this, ruleFilePath );
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
		protected void OnValidating( System.ComponentModel.CancelEventArgs e )
		{
			if ( !_Validated && this.ValidationRules.Count > 0 )
			{
				DataComparer.Result result = 
					this.ValidationRules.Process( this.Text, this.DataType, this.DataAccessLayer.Application, ControlEvents.Validating );

				if( result.Success )	//everything passed
				{
					OnValidateCompleted( this, new ValidationArgs() );
					e.Cancel = false;

					_Validated = true;
				}
				else					//something failed
				{
					OnValidateCompleted( this, new ValidationArgs(true, result.Message) );
					e.Cancel = true;
				}
			}

			if( !e.Cancel )
			{
				OnValidated( EventArgs.Empty );
			}
		}


		/// <summary>
		/// Fires on Validation success.  Processes FillMaps.
		/// </summary>
		protected void OnValidated( System.EventArgs e )
		{
			if ( _ProcessFillMaps )
			{
				this.FillMaps.Process( this.DataAccessLayer.Application, ControlEvents.Validating );
			}

			_ProcessFillMaps = false;
		}


		/// <summary>
		/// Resets flags indicating control should be validated and FillMaps should processed.
		/// </summary>
		protected void OnTextChanged( System.EventArgs e )
		{
			_Validated = false;
			_ProcessFillMaps = true;

			SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, _uniqueName, "TextChanged." );

			ProcessEvent( ControlEvents.TextChanged );
		}


		private void OnValueChanged()
		{
			_Validated = false;
			_ProcessFillMaps = true;

			SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, _uniqueName, "ValueChanged." );

			ProcessEvent( ControlEvents.ValueChanged );
		}


		private RecordEventArgs ProcessEvent( ControlEvents EventBinding )
		{
			if( this.EventBindings.ValidationEvents.Contains( EventBinding ) )
			{
				DataComparer.Result result = 
					this.ValidationRules.Process( this.Text, this.DataType, this.DataAccessLayer.Application, EventBinding  );

				if( result.Success )	//everything passed
				{
					OnValidateCompleted( this, new ValidationArgs() );
				}
				else					//something failed
				{
					OnValidateCompleted( this, new ValidationArgs(true, result.Message) );
				}
			}


			DataSet ds = null;
			Exception ex = null;		//06232005, was SqlException

			try
			{
				if( this.EventBindings.FillMapEvents.Contains( EventBinding ) )
				{
					this.FillMaps.Process( this.DataAccessLayer.Application, EventBinding );

					for( int n=0; n<this.FillMaps.Count; n++ )
					{
						if( FillMaps[n].EventBinding == EventBinding )
						{
							ds = (DataSet)FillMaps[n].Data;		//need?: ((DataSet)FillMaps[n].Data).Copy() ?
							//ds = new DataSet();
							//ds.Tables.Add( ((DataTable)FillMaps[n].Data).Copy() );
						}
					}
				}
			}
			catch( SqlException sqlex )
			{
				ex = sqlex;
				SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.Error, this.UniqueName, sqlex.Message );
			}

			catch( ExpressionException exprex )
			{
				ex = exprex;
				SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.Error, this.UniqueName, exprex.Message );
			}

			return new RecordEventArgs( ds, ex, true );
		}


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
		}
		#endregion


		#region ImComponent Members

		public ISecureControl Parent
		{
			get
			{
				return _parentIm;
			}
			set
			{
				if( _parentIm != value )
				{
					if( _parentIm != null )
					{
						//if( _parentIm is ISecureControl )
						//{
						//    ((ISecureControl)_parentIm).SecureControls.Remove( this.UniqueName );
						//}
						if( _parentIm is IValidationControl )
						{
							((IValidationControl)_parentIm).ValidationControls.Remove( this.UniqueName );
						}
					}
					if( value != null )
					{
						//if( value is ISecureControl )
						//{
						//    ((ISecureControl)value).SecureControls.Add( this.UniqueName, this );
						//}
						if( value is IValidationControl )
						{
							((IValidationControl)value).ValidationControls.Add( this.UniqueName, this );
						}
					}

					_parentIm = value;
				}
			}
		}

		#endregion
	}//class


}	//namespace