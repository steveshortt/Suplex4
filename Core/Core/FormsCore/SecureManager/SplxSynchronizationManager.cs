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


namespace Suplex.Forms.SecureManager
{
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms,
		ControlEvents.Upload | ControlEvents.Download | ControlEvents.TextChanged | ControlEvents.ValueChanged )]
	public class SplxSynchronizationManager : SplxSecureManagerBase, ISynchronizationManager
	{
		private SynchronizationMode _synchronizationMode = SynchronizationMode.None;


		#region ISynchronizationManager Events
		[Category("Suplex")]
		public event Suplex.Forms.RaiseSynchronizationEventHandler Upload;
		[Category("Suplex")]
		public event Suplex.Forms.RaiseSynchronizationEventHandler Download;

		[Category("Suplex")]
		public event Suplex.Forms.SynchronizationEventHandler Downloaded;
		[Category("Suplex")]
		public event Suplex.Forms.SynchronizationEventHandler Uploaded;

		protected virtual void OnUpload(RaiseSynchronizationEventArgs e)
		{
			if( Upload != null )
			{
				Upload( this, e );
			}
		}
		protected virtual void OnDownload(RaiseSynchronizationEventArgs e)
		{
			if( Download != null )
			{
				Download( this, e );
			}
		}
		protected virtual void OnUploaded(SynchronizationEventArgs e)
		{
			if( Uploaded != null )
			{
				Uploaded( this, e );
			}
		}
		protected virtual void OnDownloaded(SynchronizationEventArgs e)
		{
			if( Downloaded != null )
			{
				Downloaded( this, e );
			}
		}
		#endregion


		public SplxSynchronizationManager()
			: base()
		{
			_sa = new SecurityAccessor( this, AceType.Synchronization );
			_sa.EnsureDefaultState();

			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationAccessor( this, TypeCode.Empty );
		}


		[DefaultValue( SynchronizationMode.None )]
		public Suplex.Forms.SynchronizationMode SynchronizationMode
		{
			get { return _synchronizationMode; }
			set { _synchronizationMode = value; }
		}

		//just a few shortcut properties...
		[Browsable( false )]
		public bool HasOneWayRight { get { return _sr[AceType.Synchronization, SynchronizationRight.OneWay].AccessAllowed; } }
		[Browsable( false )]
		public bool HasUploadRight { get { return _sr[AceType.Synchronization, SynchronizationRight.Upload].AccessAllowed; } }
		[Browsable( false )]
		public bool HasDownloadRight { get { return _sr[AceType.Synchronization, SynchronizationRight.Download].AccessAllowed; } }
		[Browsable( false )]
		public bool HasTwoWayRight { get { return _sr[AceType.Synchronization, SynchronizationRight.TwoWay].AccessAllowed; } }


		public void RaiseUpload(params object[] parameters)	//bool process, 
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Upload deferred to caller.", false );

			OnUpload( new RaiseSynchronizationEventArgs( parameters, _sr[AceType.Synchronization, SynchronizationRight.Upload].AccessAllowed ) );
		}

		public void RaiseDownload(params object[] parameters)	//bool process, 
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Download deferred to caller.", false );

			OnDownload( new RaiseSynchronizationEventArgs( parameters, _sr[AceType.Synchronization, SynchronizationRight.Download].AccessAllowed ) );
		}

		public override string GetSecurityState()
		{
			throw new NotImplementedException();
		}
	}
}


/*
			//if( process )
			//{
			//    if( _sr[AceType.Synchronization, SynchronizationRight.Upload].AccessAllowed )
			//    {
			//        _synchronizationMode = SynchronizationMode.Upload;
			//        SynchronizationEventArgs s = ProcessEvent( ControlEvents.Upload );

			//        string msg = "Upload completed successfully.";
			//        if( r.Exception != null ) msg = "ListRecords completed with errors.";
			//        SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName, msg );

			//        OnUploaded( s );
			//    }
			//    else
			//    {
			//        SecureControlUtils.AuditAction( this, _sa.AuditEventHandler, AuditType.ControlDetail, this.UniqueName,
			//            "ListRecords failed: access denied." );

			//        OnUploaded( SynchronizationEventArgs.Empty );
			//    }
			//}
			//else
			//{
 
 */