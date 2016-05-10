using System;
using System.Data;
using System.Data.SqlClient;


namespace Suplex.Forms
{
	internal interface IRecordManager_Old
	{
		event RecordEventHandler ListedRecords;
		event RecordEventHandler SelectedRecord;
		event RecordEventHandler InsertedRecord;
		event RecordEventHandler UpdatedRecord;
		event RecordEventHandler DeletedRecord;

		void ListRecords(params object[] parameters);
		void SelectRecord(params object[] parameters);
		void InsertRecord(params object[] parameters);
		void UpdateRecord(params object[] parameters);
		void DeleteRecord(params object[] parameters);
	}


	public interface IRecordManager
	{
		Suplex.Forms.RecordMode RecordMode
		{
			get;
			set;
		}

		event RaiseRecordEventHandler ListRecords;
		event RaiseRecordEventHandler SelectRecord;
		event RaiseRecordEventHandler InsertRecord;
		event RaiseRecordEventHandler UpdateRecord;
		event RaiseRecordEventHandler DeleteRecord;

		event RecordEventHandler ListedRecords;
		event RecordEventHandler SelectedRecord;
		event RecordEventHandler InsertedRecord;
		event RecordEventHandler UpdatedRecord;
		event RecordEventHandler DeletedRecord;

		void RaiseListRecords(bool process, params object[] parameters);
		void RaiseSelectRecord(bool process, params object[] parameters);
		void RaiseInsertRecord(bool process, params object[] parameters);
		void RaiseUpdateRecord(bool process, params object[] parameters);
		void RaiseDeleteRecord(bool process, params object[] parameters);
	}


	public delegate void RaiseRecordEventHandler(object sender, RaiseRecordEventArgs e);

	public delegate void RecordEventHandler(object sender, RecordEventArgs e);

	public class RaiseRecordEventArgs : EventArgs
	{
		private object[] _parameters = null;
		private bool _hasRight = false;


		new public static readonly RaiseRecordEventArgs Empty;
		static RaiseRecordEventArgs()
		{
			Empty = new RaiseRecordEventArgs( null, false );
		}


		public RaiseRecordEventArgs(object[] parameters, bool hasRight)
		{
			_parameters = parameters;
			_hasRight = hasRight;
		}


		public object[] Parameters { get { return _parameters; } }
		public bool HasRight { get { return _hasRight; } }
	}


	public class RecordEventArgs : EventArgs
	{
		private DataSet _ds = null;
		private System.Exception _e = null;			//06/23/2005, was SqlException
		private bool _hasRight = false;


		new public static readonly RecordEventArgs Empty;
		static RecordEventArgs()
		{
			Empty = new RecordEventArgs( null, null, false );
		}


		public RecordEventArgs(DataSet dataSet, System.Exception exception, bool hasRight)
		{
			_ds = dataSet;
			_e = exception;
			_hasRight = hasRight;
		}


		public DataSet DataSet { get { return _ds; } }
		public System.Exception Exception { get { return _e; } }
		public bool HasException { get { return _e != null; } }
		public bool HasRight { get { return _hasRight; } }
	}


	public enum ReadyState
	{
		Ok,
		Modified,
		Error
	}


	public enum RecordMode
	{
		None,
		List,
		Select,
		Insert,
		Update,
		Delete
	}

}