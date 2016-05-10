using System;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Collections;
using System.Web.UI;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;
using Suplex.Security.Standard;


using System.Diagnostics;
using System.Web.UI.Design;
using System.Drawing;



namespace Suplex.WebForms
{
	/// <summary>
	/// Provides secuirty, extended validation properties, and auto-validation.
	/// </summary>
	[ToolboxItem( true ), System.Drawing.ToolboxBitmap( typeof( sRecordManager ), "Resources.mRecordManager.gif" )]
	[EventBindings( EventBindingsAttribute.BaseEvents.None,
		ControlEvents.ListRecords | ControlEvents.SelectRecord | ControlEvents.InsertRecord | ControlEvents.UpdateRecord | ControlEvents.DeleteRecord |
		ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.TextChanged | ControlEvents.ValueChanged )]
	[Designer( typeof( sRecordManagerDesigner ) )]
	public class sRecordManager : System.Web.UI.Control, IValidationControl, IRecordManager, ISecurityExtender, IWebExtra
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationAccessor _va = null;

		private string _tag = null;
		private object _tagObject = null;


		private RecordMode _recordMode = RecordMode.None;
		private string _text = string.Empty;
		private object _value = null;


		#region IRecordManager2 Events
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseRecordEventHandler ListRecords;
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseRecordEventHandler SelectRecord;
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseRecordEventHandler InsertRecord;
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseRecordEventHandler UpdateRecord;
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseRecordEventHandler DeleteRecord;

		[Category( "Suplex" )]
		public event Suplex.Forms.RecordEventHandler ListedRecords;
		[Category( "Suplex" )]
		public event Suplex.Forms.RecordEventHandler SelectedRecord;
		[Category( "Suplex" )]
		public event Suplex.Forms.RecordEventHandler InsertedRecord;
		[Category( "Suplex" )]
		public event Suplex.Forms.RecordEventHandler UpdatedRecord;
		[Category( "Suplex" )]
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


		public sRecordManager()
		{
			_sa = new SecurityAccessor( this, AceType.Record );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationAccessor( this, TypeCode.Empty );
		}

		protected override void OnInit(EventArgs e)
		{
			_sa.EnsureDefaultState();

			base.OnInit( e );
		}

		protected override void OnPreRender(EventArgs e)
		{
			this.ApplySecurity();

			base.OnPreRender( e );
		}


		#region IRecordManager Members
		public void RaiseListRecords(bool process, params object[] parameters)
		{
			if( process )
			{
				if ( _sr[AceType.Record, RecordRight.List].AccessAllowed )
				{
					_recordMode = RecordMode.List;
					RecordEventArgs r = this.HandleProcessEvent( ControlEvents.ListRecords );

					string msg = "ListRecords completed successfully.";
					if( r.Exception != null ) msg = "ListRecords completed with errors.";
					_sa.AuditAction( AuditType.ControlDetail, null, msg, false );

					OnListedRecords( r );
				}
				else
				{
					_sa.AuditAction( AuditType.ControlDetail, null,
						"ListRecords failed: access denied.", true );

					OnListedRecords( RecordEventArgs.Empty );
				}
			}
			else
			{
				_sa.AuditAction( AuditType.ControlDetail, null,
					"ListRecords deferred to caller.", false );

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
					RecordEventArgs r = this.HandleProcessEvent( ControlEvents.SelectRecord );

					string msg = "SelectRecord completed successfully.";
					if( r.Exception != null ) msg = "SelectRecord completed with errors.";
					_sa.AuditAction( AuditType.ControlDetail, null, msg, false );

					OnSelectedRecord( r );
				}
				else
				{
					_sa.AuditAction( AuditType.ControlDetail, null,
						"SelectRecord failed: access denied.", true );

					OnSelectedRecord( RecordEventArgs.Empty );
				}
			}
			else
			{
				_sa.AuditAction( AuditType.ControlDetail, null,
					"SelectRecord deferred to caller.", false );

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
					RecordEventArgs r = this.HandleProcessEvent( ControlEvents.InsertRecord );

					string msg = "InsertRecord completed successfully.";
					if( r.Exception != null ) msg = "InsertRecord completed with errors.";
					_sa.AuditAction( AuditType.ControlDetail, null, msg, false );

					OnInsertedRecord( r );
				}
				else
				{
					_sa.AuditAction( AuditType.ControlDetail, null,
						"InsertRecord failed: access denied.", true );

					OnInsertedRecord( RecordEventArgs.Empty );
				}
			}
			else
			{
				_sa.AuditAction( AuditType.ControlDetail, null,
					"InsertRecord deferred to caller.", false );

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
					RecordEventArgs r = this.HandleProcessEvent( ControlEvents.UpdateRecord );

					string msg = "UpdateRecord completed successfully.";
					if( r.Exception != null ) msg = "UpdateRecord completed with errors.";
					_sa.AuditAction( AuditType.ControlDetail, null, msg, false );

					OnUpdatedRecord( r );
				}
				else
				{
					_sa.AuditAction( AuditType.ControlDetail, null,
						"UpdateRecord failed: access denied.", true );

					OnUpdatedRecord( RecordEventArgs.Empty );
				}
			}
			else
			{
				_sa.AuditAction( AuditType.ControlDetail, null,
					"UpdateRecord deferred to caller.", false );

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
					RecordEventArgs r = this.HandleProcessEvent( ControlEvents.DeleteRecord );

					string msg = "DeleteRecord completed successfully.";
					if( r.Exception != null ) msg = "DeleteRecord completed with errors.";
					_sa.AuditAction( AuditType.ControlDetail, null, msg, false );

					OnDeletedRecord( r );
				}
				else
				{
					_sa.AuditAction( AuditType.ControlDetail, null,
						"DeleteRecord failed: access denied.", true );

					OnDeletedRecord( RecordEventArgs.Empty );
				}
			}
			else
			{
				_sa.AuditAction( AuditType.ControlDetail, null,
					"DeleteRecord deferred to caller.", false );

				OnDeleteRecord( new RaiseRecordEventArgs( parameters, _sr[AceType.Record, RecordRight.Delete].AccessAllowed ) );
			}
		}

		[DefaultValue( RecordMode.None ), Category( "Suplex" )]
		public Suplex.Forms.RecordMode RecordMode
		{
			get { return _recordMode; }
			set { _recordMode = value; }
		}

		[Browsable( true )]
		public string Text
		{
			get { return _text; }
			set
			{
				if( _text != value )
				{
					_text = value;
					OnTextChanged( EventArgs.Empty );
				}
			}
		}

		[Browsable( false )]
		public object Value
		{
			get { return _value; }
			set
			{
				if( _value != value )
				{
					_value = value;
					OnValueChanged();
				}
			}
		}
		#endregion


		[ParenthesizePropertyName( true ), Category( "Suplex" )]
		public string UniqueName
		{
			get
			{
				return string.IsNullOrEmpty( _uniqueName ) ? base.UniqueID : _uniqueName;
			}
			set
			{
				_uniqueName = value;
			}
		}

		[Browsable( false ), Category( "Suplex" )]
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

		public ValidationResult ProcessValidate(bool processFillMaps)
		{
			return _va.ProcessEvent( null, ControlEvents.Validating, processFillMaps );
		}

		/// <summary>
		/// Resets flags indicating control should be validated and FillMaps should processed.
		/// </summary>
		protected void OnTextChanged( System.EventArgs e )
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "TextChanged.", false );

			_va.ProcessEvent( null, ControlEvents.TextChanged, true );
		}

		private void OnValueChanged()
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "ValueChanged.", false );

			_va.ProcessEvent( null, ControlEvents.ValueChanged, true );
		}
		#endregion


		private RecordEventArgs HandleProcessEvent(ControlEvents eventBinding)
		{
			DataSet ds = null;
			Exception ex = null; //06232005, was SqlException
			try
			{
				_va.ProcessEvent( null, eventBinding, true );

				foreach( FillMap map in _va.FillMaps )
				{
					if( map.EventBinding == eventBinding )
					{
						ds = (DataSet)map.Data; //need?: ((DataSet)FillMaps[n].Data).Copy() ?
						//ds = new DataSet();
						//ds.Tables.Add( ((DataTable)FillMaps[n].Data).Copy() );
					}
				}
			}
			catch( SqlException sqlex )
			{
				ex = sqlex;
				_sa.AuditAction( AuditType.Error, null, sqlex.Message, false );
			}
			catch( ExpressionException exprex )
			{
				ex = exprex;
				_sa.AuditAction( AuditType.Error, null, exprex.Message, false );
			}

			return new RecordEventArgs( ds, ex, true );
		}


		#region Security Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Security management properties and tools." )]
		public SecurityAccessor Security
		{
			get { return _sa; }
		}

		public void ApplySecurity()
		{
			if( !this.DesignMode )
			{
				base.Visible = false;
			}
		}

		//just a few shortcut properties...
		[Browsable( false )]
		public bool HasListRight { get { return _sr[AceType.Record, RecordRight.List].AccessAllowed; } }
		[Browsable( false )]
		public bool HasSelectRight { get { return _sr[AceType.Record, RecordRight.Select].AccessAllowed; } }
		[Browsable( false )]
		public bool HasInsertRight { get { return _sr[AceType.Record, RecordRight.Insert].AccessAllowed; } }
		[Browsable( false )]
		public bool HasUpdateRight { get { return _sr[AceType.Record, RecordRight.Update].AccessAllowed; } }
		[Browsable( false )]
		public bool HasDeleteRight { get { return _sr[AceType.Record, RecordRight.Delete].AccessAllowed; } }
		#endregion


		#region IWebExtra Members
		public string Tag
		{
			get { return _tag; }
			set { _tag = value; }
		}

		[Browsable( false )]
		public object TagObj
		{
			get { return _tagObject; }
			set { _tagObject = value; }
		}
		#endregion
	}//class


	public class sRecordManagerDesigner : ControlDesigner
	{
		public override void Initialize(System.ComponentModel.IComponent component)
		{
			base.Initialize( component );
			SetViewFlags( ViewFlags.CustomPaint, true );
		}

		public override string GetDesignTimeHtml()
		{
			//return string.Format( "<span style=\"background-color:#ccffff; font-family:Verdana, Arial; font-size:9pt;\">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[{0}]&nbsp;</span>", base.ID );
			return string.Format( "<table border=\"0px\" cellpadding=\"0\" cellspacing=\"0\"><tr><td height=\"20px\">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[{0}]&nbsp;</td></tr></table>", base.ID );
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			e.Graphics.DrawImage( Properties.Resources.RecordManagerImage, 3, 2 );
			//e.Graphics.DrawImage( Properties.Resources.RecordManagerImage, 6, 6 ).;
			base.OnPaint( e );
		}
	}

}//namespace