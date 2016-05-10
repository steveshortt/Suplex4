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
using System.Windows.Media.Imaging;


namespace Suplex.Wpf
{
	[ToolboxItem(true)]
	//[ToolboxBitmap(typeof(Suplex.WinForms.sFileSystemManager), "Resources.sFileSystemManager.png")]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms,
		ControlEvents.TextChanged | ControlEvents.ValueChanged )]
	public class SplxFileSystemManager : Suplex.Wpf.SplxSecureManagerBase, IFileSystemManager
	{
		#region IFileSystemManager Events
		[Category("Suplex")]
		public event Suplex.Forms.RaiseFileSystemEventHandler Execute;
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseFileSystemEventHandler Delete;
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseFileSystemEventHandler Write;
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseFileSystemEventHandler Create;
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseFileSystemEventHandler Read;
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseFileSystemEventHandler List;
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseFileSystemEventHandler ChangePermissions;
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseFileSystemEventHandler ReadPermissions;
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseFileSystemEventHandler TakeOwnership;

		[Category("Suplex")]
		public event Suplex.Forms.FileSystemEventHandler Executed;
		[Category( "Suplex" )]
		public event Suplex.Forms.FileSystemEventHandler Deleted;
		[Category( "Suplex" )]
		public event Suplex.Forms.FileSystemEventHandler Wrote;
		[Category( "Suplex" )]
		public event Suplex.Forms.FileSystemEventHandler Created;
		[Category( "Suplex" )]
		public event Suplex.Forms.FileSystemEventHandler Raed;
		[Category( "Suplex" )]
		public event Suplex.Forms.FileSystemEventHandler Listed;
		[Category( "Suplex" )]
		public event Suplex.Forms.FileSystemEventHandler ChangedPermissions;
		[Category( "Suplex" )]
		public event Suplex.Forms.FileSystemEventHandler RaedPermissions;
		[Category( "Suplex" )]
		public event Suplex.Forms.FileSystemEventHandler TookOwnership;

		protected virtual void OnExecute(RaiseFileSystemEventArgs e)
		{
			if( Execute != null ) { Execute( this, e ); }
		}
		protected virtual void OnExecuted(FileSystemEventArgs e)
		{
			if( Executed != null ) { Executed( this, e ); }
		}

		protected virtual void OnDelete(RaiseFileSystemEventArgs e)
		{
			if( Delete != null ) { Delete( this, e ); }
		}
		protected virtual void OnDeleted(FileSystemEventArgs e)
		{
			if( Deleted != null ) { Deleted( this, e ); }
		}

		protected virtual void OnWrite(RaiseFileSystemEventArgs e)
		{
			if( Write != null ) { Write( this, e ); }
		}
		protected virtual void OnWrote(FileSystemEventArgs e)
		{
			if( Wrote != null ) { Wrote( this, e ); }
		}

		protected virtual void OnCreate(RaiseFileSystemEventArgs e)
		{
			if( Create != null ) { Create( this, e ); }
		}
		protected virtual void OnCreated(FileSystemEventArgs e)
		{
			if( Created != null ) { Created( this, e ); }
		}

		protected virtual void OnRead(RaiseFileSystemEventArgs e)
		{
			if( Read != null ) { Read( this, e ); }
		}
		protected virtual void OnRaed(FileSystemEventArgs e)
		{
			if( Raed != null ) { Raed( this, e ); }
		}

		protected virtual void OnList(RaiseFileSystemEventArgs e)
		{
			if( List != null ) { List( this, e ); }
		}
		protected virtual void OnListed(FileSystemEventArgs e)
		{
			if( Listed != null ) { Listed( this, e ); }
		}

		protected virtual void OnChangePermissions(RaiseFileSystemEventArgs e)
		{
			if( ChangePermissions != null ) { ChangePermissions( this, e ); }
		}
		protected virtual void OnChangedPermissions(FileSystemEventArgs e)
		{
			if( ChangedPermissions != null ) { ChangedPermissions( this, e ); }
		}

		protected virtual void OnReadPermissions(RaiseFileSystemEventArgs e)
		{
			if( ReadPermissions != null ) { ReadPermissions( this, e ); }
		}
		protected virtual void OnRaedPermissions(FileSystemEventArgs e)
		{
			if( RaedPermissions != null ) { RaedPermissions( this, e ); }
		}

		protected virtual void OnTakeOwnership(RaiseFileSystemEventArgs e)
		{
			if( TakeOwnership != null ) { TakeOwnership( this, e ); }
		}
		protected virtual void OnTookOwnership(FileSystemEventArgs e)
		{
			if( TookOwnership != null ) { TookOwnership( this, e ); }
		}
		#endregion

		public SplxFileSystemManager()
			: base()
		{
			_sa = new SecurityAccessor( this, AceType.FileSystem );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationAccessor( this, TypeCode.Empty );
		}


		[DefaultValue( FileSystemMode.None )]
		public FileSystemMode FileSystemMode { get; set; }

		//just a few shortcut properties...
		[Browsable( false )]
		public bool HasExecuteRight { get { return _sr[AceType.FileSystem, FileSystemRight.Execute].AccessAllowed; } }
		[Browsable( false )]
		public bool HasDeleteRight { get { return _sr[AceType.FileSystem, FileSystemRight.Delete].AccessAllowed; } }
		[Browsable( false )]
		public bool HasWritedRight { get { return _sr[AceType.FileSystem, FileSystemRight.Write].AccessAllowed; } }
		[Browsable( false )]
		public bool HasCreateRight { get { return _sr[AceType.FileSystem, FileSystemRight.Create].AccessAllowed; } }
		[Browsable( false )]
		public bool HasReadRight { get { return _sr[AceType.FileSystem, FileSystemRight.Read].AccessAllowed; } }
		[Browsable( false )]
		public bool HasListRight { get { return _sr[AceType.FileSystem, FileSystemRight.List].AccessAllowed; } }
		[Browsable( false )]
		public bool HasChangePermissionsRight { get { return _sr[AceType.FileSystem, FileSystemRight.ChangePermissions].AccessAllowed; } }
		[Browsable( false )]
		public bool HasReadPermissionsRight { get { return _sr[AceType.FileSystem, FileSystemRight.ReadPermissions].AccessAllowed; } }
		[Browsable( false )]
		public bool HasTakeOwnershipRight { get { return _sr[AceType.FileSystem, FileSystemRight.TakeOwnership].AccessAllowed; } }


		public void RaiseExecute(params object[] parameters)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Upload deferred to caller.", false );

			OnExecute( new RaiseFileSystemEventArgs( parameters, _sr[AceType.FileSystem, FileSystemRight.Execute].AccessAllowed ) );
		}

		public void RaiseDelete(params object[] parameters)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Download deferred to caller.", false );

			OnDelete( new RaiseFileSystemEventArgs( parameters, _sr[AceType.FileSystem, FileSystemRight.Delete].AccessAllowed ) );
		}

		public void RaiseWrite(params object[] parameters)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Download deferred to caller.", false );

			OnWrite( new RaiseFileSystemEventArgs( parameters, _sr[AceType.FileSystem, FileSystemRight.Write].AccessAllowed ) );
		}

		public void RaiseCreate(params object[] parameters)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Download deferred to caller.", false );

			OnCreate( new RaiseFileSystemEventArgs( parameters, _sr[AceType.FileSystem, FileSystemRight.Create].AccessAllowed ) );
		}

		public void RaiseRead(params object[] parameters)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Download deferred to caller.", false );

			OnRead( new RaiseFileSystemEventArgs( parameters, _sr[AceType.FileSystem, FileSystemRight.Read].AccessAllowed ) );
		}

		public void RaiseList(params object[] parameters)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Download deferred to caller.", false );

			OnList( new RaiseFileSystemEventArgs( parameters, _sr[AceType.FileSystem, FileSystemRight.List].AccessAllowed ) );
		}

		public void RaiseChangePermissions(params object[] parameters)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Download deferred to caller.", false );

			OnChangePermissions( new RaiseFileSystemEventArgs( parameters, _sr[AceType.FileSystem, FileSystemRight.ChangePermissions].AccessAllowed ) );
		}

		public void RaiseReadPermissions(params object[] parameters)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Download deferred to caller.", false );

			OnReadPermissions( new RaiseFileSystemEventArgs( parameters, _sr[AceType.FileSystem, FileSystemRight.ReadPermissions].AccessAllowed ) );
		}

		public void RaiseTakeOwnership(params object[] parameters)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Download deferred to caller.", false );

			OnTakeOwnership( new RaiseFileSystemEventArgs( parameters, _sr[AceType.FileSystem, FileSystemRight.TakeOwnership].AccessAllowed ) );
		}
	}
}