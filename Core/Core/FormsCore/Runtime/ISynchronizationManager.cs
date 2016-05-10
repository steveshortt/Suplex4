using System;
using System.Data;
using System.Data.SqlClient;


namespace Suplex.Forms
{
	public interface ISynchronizationManager
	{
		Suplex.Forms.SynchronizationMode SynchronizationMode { get; set; }

		event RaiseSynchronizationEventHandler Upload;
		event RaiseSynchronizationEventHandler Download;

		event SynchronizationEventHandler Uploaded;
		event SynchronizationEventHandler Downloaded;

		void RaiseUpload(params object[] parameters);		//bool process, 
		void RaiseDownload(params object[] parameters);	//bool process, 
	}


	public delegate void RaiseSynchronizationEventHandler(object sender, RaiseSynchronizationEventArgs e);

	public delegate void SynchronizationEventHandler(object sender, SynchronizationEventArgs e);

	public class RaiseSynchronizationEventArgs : EventArgs
	{
		private object[] _parameters = null;
		private bool _hasRight = false;


		new public static readonly RaiseSynchronizationEventArgs Empty;
		static RaiseSynchronizationEventArgs()
		{
			Empty = new RaiseSynchronizationEventArgs( null, false );
		}


		public RaiseSynchronizationEventArgs(object[] parameters, bool hasRight)
		{
			_parameters = parameters;
			_hasRight = hasRight;
		}


		public object[] Parameters { get { return _parameters; } }
		public bool HasRight { get { return _hasRight; } }
	}


	public class SynchronizationEventArgs : EventArgs
	{
		private System.Exception _e = null;
		private bool _hasRight = false;


		new public static readonly SynchronizationEventArgs Empty;
		static SynchronizationEventArgs()
		{
			Empty = new SynchronizationEventArgs( null, false );
		}


		public SynchronizationEventArgs(System.Exception exception, bool hasRight)
		{
			_e = exception;
			_hasRight = hasRight;
		}

		public System.Exception Exception { get { return _e; } }
		public bool HasException { get { return _e != null; } }
		public bool HasRight { get { return _hasRight; } }
	}


	public enum SynchronizationMode
	{
		None,
		Upload,
		Download
	}

}