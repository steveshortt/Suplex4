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


namespace Suplex.WinForms
{
	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	//[Obsolete( "Use sRecordManagerCpnt instead.", false )]
	[ToolboxItem( true ), ToolboxBitmap( typeof( Suplex.WinForms.sRecordManager ), "Resources.sRecordManager.gif" )]
	[EventBindings( EventBindingsAttribute.BaseEvents.None,
		ControlEvents.ListRecords | ControlEvents.SelectRecord | ControlEvents.InsertRecord | ControlEvents.UpdateRecord | ControlEvents.DeleteRecord |
		ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.TextChanged | ControlEvents.ValueChanged )]
	public class sRecordManager : Suplex.WinForms.sSecureManagerBase, IRecordManager
	{
		private RecordMode _recordMode = RecordMode.None;


		#region IRecordManager Events
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


		public sRecordManager() : base( Properties.Resources.RecordManagerImage )
		{
			_sa = new SecurityAccessor( this, AceType.Record );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationAccessor( this, TypeCode.Empty );
		}


		[DefaultValue( RecordMode.None )]
		public Suplex.Forms.RecordMode RecordMode
		{
			get { return _recordMode; }
			set { _recordMode = value; }
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


		public void RaiseListRecords(bool process, params object[] parameters)
		{
			if( process )
			{
				if ( _sr[AceType.Record, RecordRight.List].AccessAllowed )
				{
					_recordMode = RecordMode.List;
					RecordEventArgs r = HandleProcessEvent( ControlEvents.ListRecords );

					string msg = "ListRecords completed successfully.";
					if( r.Exception != null ) msg = "ListRecords completed with errors.";
					_sa.AuditAction( AuditType.ControlDetail, null, msg, false );

					OnListedRecords( r );
				}
				else
				{
					_sa.AuditAction( AuditType.ControlDetail, null, "ListRecords failed: access denied.", false );

					OnListedRecords( RecordEventArgs.Empty );
				}
			}
			else
			{
				_sa.AuditAction( AuditType.ControlDetail, null, "ListRecords deferred to caller.", false );

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
					RecordEventArgs r = HandleProcessEvent( ControlEvents.SelectRecord );

					string msg = "SelectRecord completed successfully.";
					if( r.Exception != null ) msg = "SelectRecord completed with errors.";
					_sa.AuditAction( AuditType.ControlDetail, null, msg, false );

					OnSelectedRecord( r );
				}
				else
				{
					_sa.AuditAction( AuditType.ControlDetail, null, "SelectRecord failed: access denied.", false );

					OnSelectedRecord( RecordEventArgs.Empty );
				}
			}
			else
			{
				_sa.AuditAction( AuditType.ControlDetail, null, "SelectRecord deferred to caller.", false );

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
					RecordEventArgs r = HandleProcessEvent( ControlEvents.InsertRecord );

					string msg = "InsertRecord completed successfully.";
					if( r.Exception != null ) msg = "InsertRecord completed with errors.";
					_sa.AuditAction( AuditType.ControlDetail, null, msg, false );

					OnInsertedRecord( r );
				}
				else
				{
					_sa.AuditAction( AuditType.ControlDetail, null, "InsertRecord failed: access denied.", false );

					OnInsertedRecord( RecordEventArgs.Empty );
				}
			}
			else
			{
				_sa.AuditAction( AuditType.ControlDetail, null, "InsertRecord deferred to caller.", false );

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
					RecordEventArgs r = HandleProcessEvent( ControlEvents.UpdateRecord );

					string msg = "UpdateRecord completed successfully.";
					if( r.Exception != null ) msg = "UpdateRecord completed with errors.";
					_sa.AuditAction( AuditType.ControlDetail, null, msg, false );

					OnUpdatedRecord( r );
				}
				else
				{
					_sa.AuditAction( AuditType.ControlDetail, null, "UpdateRecord failed: access denied.", false );

					OnUpdatedRecord( RecordEventArgs.Empty );
				}
			}
			else
			{
				_sa.AuditAction( AuditType.ControlDetail, null, "UpdateRecord deferred to caller.", false );

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
					RecordEventArgs r = HandleProcessEvent( ControlEvents.DeleteRecord );

					string msg = "DeleteRecord completed successfully.";
					if( r.Exception != null ) msg = "DeleteRecord completed with errors.";
					_sa.AuditAction( AuditType.ControlDetail, null, msg, false );

					OnDeletedRecord( r );
				}
				else
				{
					_sa.AuditAction( AuditType.ControlDetail, null, "DeleteRecord failed: access denied.", false );

					OnDeletedRecord( RecordEventArgs.Empty );
				}
			}
			else
			{
				_sa.AuditAction( AuditType.ControlDetail, null, "DeleteRecord deferred to caller.", false );

				OnDeleteRecord( new RaiseRecordEventArgs( parameters, _sr[AceType.Record, RecordRight.Delete].AccessAllowed ) );
			}
		}

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
	}//class
}	//namespace