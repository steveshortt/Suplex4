using System;
using System.Data;
using System.Data.SqlClient;


namespace Suplex.Forms
{
	public interface IFileSystemManager
	{
		Suplex.Forms.FileSystemMode FileSystemMode { get; set; }

		event RaiseFileSystemEventHandler Execute;
		event RaiseFileSystemEventHandler Delete;
		event RaiseFileSystemEventHandler Write;
		event RaiseFileSystemEventHandler Create;
		event RaiseFileSystemEventHandler Read;
		event RaiseFileSystemEventHandler List;
		event RaiseFileSystemEventHandler ChangePermissions;
		event RaiseFileSystemEventHandler ReadPermissions;
		event RaiseFileSystemEventHandler TakeOwnership;

		event FileSystemEventHandler Executed;
		event FileSystemEventHandler Deleted;
		event FileSystemEventHandler Wrote;
		event FileSystemEventHandler Created;
		event FileSystemEventHandler Raed;
		event FileSystemEventHandler Listed;
		event FileSystemEventHandler ChangedPermissions;
		event FileSystemEventHandler RaedPermissions;
		event FileSystemEventHandler TookOwnership;

		void RaiseExecute(params object[] parameters);
		void RaiseDelete(params object[] parameters);
		void RaiseWrite(params object[] parameters);
		void RaiseCreate(params object[] parameters);
		void RaiseRead(params object[] parameters);
		void RaiseList(params object[] parameters);
		void RaiseChangePermissions(params object[] parameters);
		void RaiseReadPermissions(params object[] parameters);
		void RaiseTakeOwnership(params object[] parameters);
	}


	public delegate void RaiseFileSystemEventHandler(object sender, RaiseFileSystemEventArgs e);

	public delegate void FileSystemEventHandler(object sender, FileSystemEventArgs e);

	public class RaiseFileSystemEventArgs : EventArgs
	{
		private object[] _parameters = null;
		private bool _hasRight = false;


		new public static readonly RaiseFileSystemEventArgs Empty;
		static RaiseFileSystemEventArgs()
		{
			Empty = new RaiseFileSystemEventArgs( null, false );
		}


		public RaiseFileSystemEventArgs(object[] parameters, bool hasRight)
		{
			_parameters = parameters;
			_hasRight = hasRight;
		}


		public object[] Parameters { get { return _parameters; } }
		public bool HasRight { get { return _hasRight; } }
	}


	public class FileSystemEventArgs : EventArgs
	{
		private System.Exception _e = null;
		private bool _hasRight = false;


		new public static readonly FileSystemEventArgs Empty;
		static FileSystemEventArgs()
		{
			Empty = new FileSystemEventArgs( null, false );
		}


		public FileSystemEventArgs(System.Exception exception, bool hasRight)
		{
			_e = exception;
			_hasRight = hasRight;
		}

		public System.Exception Exception { get { return _e; } }
		public bool HasException { get { return _e != null; } }
		public bool HasRight { get { return _hasRight; } }
	}


	public enum FileSystemMode
	{
		None,
		Execute,
		Delete,
		Write,
		Create,
		Read,
		List,
		ChangePermissions,
		ReadPermissions,
		TakeOwnership
	}
}