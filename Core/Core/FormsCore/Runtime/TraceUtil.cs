using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using color = System.Drawing.Color;
using System.Collections.ObjectModel;


namespace Suplex.Forms
{
	public class TraceUtil
	{
		private bool _isEnabled = false;
		private List<TraceRecord> _trace = new List<TraceRecord>( 150 );
		private DateTime _startTime = DateTime.MinValue;
		private TraceRecord _lastRecord = null;
		private decimal _fromLastThreshold = Int32.MaxValue;
		private decimal _deltaThreshold = Int32.MaxValue;

		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				_isEnabled = value;
				if( _startTime == DateTime.MinValue )
				{
					_startTime = DateTime.Now;
				}
			}
		}
		public decimal FromLastErrorThreshold
		{
			get { return _fromLastThreshold; }
			set { _fromLastThreshold = value; }
		}
		public decimal DeltaErrorThreshold
		{
			get { return _deltaThreshold; }
			set { _deltaThreshold = value; }
		}

		public ReadOnlyCollection<TraceRecord> TraceRecords { get { return _trace.AsReadOnly(); } }


		public void Warn(string message)
		{
			WarnKeyed( null, null, message, null );
		}
		public void Warn(string category, string message)
		{
			WarnKeyed( null, category, message, null );
		}
		public void Warn(string category, string message, Exception errorInfo)
		{
			WarnKeyed( null, category, message, errorInfo );
		}
		public void WarnKeyed(string key, string category, string message, Exception errorInfo)
		{
			AddTraceRecord( key, category, message, errorInfo, true );
		}


		public void Write(string message)
		{
			WriteKeyed( null, null, message, null );
		}
		public void Write(string category, string message)
		{
			WriteKeyed( null, category, message, null );
		}
		public void Write(string category, string message, Exception errorInfo)
		{
			WriteKeyed( null, category, message, errorInfo );
		}
		public void WriteKeyed(string key, string category, string message, Exception errorInfo)
		{
			AddTraceRecord( key, category, message, errorInfo, false );
		}


		private void AddTraceRecord(string key, string category, string message, Exception errorInfo, bool isWarning)
		{
			if( _isEnabled )
			{
				TraceRecord r = new TraceRecord( key, category, message, errorInfo, isWarning );
				if( _lastRecord != null )
				{
					r.CalculateTimeSpans( _startTime, _lastRecord.TimeStamp );
				}

				_trace.Add( r );

				_lastRecord = r;
			}
		}


		public void ProcessKeyDeltas()
		{
			Dictionary<string, TraceRecord> d = new Dictionary<string, TraceRecord>();
			foreach( TraceRecord r in _trace )
			{
				if( r.IsKeyed )
				{
					if( d.ContainsKey( r.Key ) )
					{
						r.CalculateKeyDelta( d[r.Key].TimeStamp );
					}
					d[r.Key] = r;
				}
			}
		}
	}

	public class TraceRecord
	{
		private string _key;
		private string _category;
		private string _message;
		private bool _isWarning;
		private DateTime _timeStamp;
		private Exception _errorInfo;
		private Decimal _timeFromFirst;
		private Decimal _timeFromLast;
		private Decimal _keyDelta = 0;

		public TraceRecord()
		{
			_timeStamp = DateTime.Now;
		}

		public TraceRecord(string message, bool isWarning)
			: this( null, null, message, null, isWarning )
		{
		}
		public TraceRecord(string category, string message, bool isWarning)
			: this( null, category, message, null, isWarning )
		{
		}
		public TraceRecord(string key, string category, string message, Exception errorInfo, bool isWarning)
		{
			_key = key;
			_category = category;
			_message = message;
			_errorInfo = errorInfo;
			_isWarning = isWarning;
			_timeStamp = DateTime.Now;
		}

		public void CalculateTimeSpans(DateTime startTime, DateTime lastTimeStamp)
		{
			double fromFirst = ( ( (TimeSpan)( _timeStamp.Subtract( startTime ) ) ).TotalSeconds );
			_timeFromFirst = Decimal.Round( (decimal)fromFirst, 16 );

			double fromLast = ( ( (TimeSpan)( _timeStamp.Subtract( lastTimeStamp ) ) ).TotalSeconds );
			_timeFromLast = Decimal.Round( (decimal)fromLast, 6 );
		}
		public void CalculateKeyDelta(DateTime lastKeyTimeStamp)
		{
			double fromLastKey = ( ( (TimeSpan)( _timeStamp.Subtract( lastKeyTimeStamp ) ) ).TotalSeconds );
			_keyDelta = Decimal.Round( (decimal)fromLastKey, 6 );
		}

		public string Key { get { return _key; } }
		public bool IsKeyed { get { return !string.IsNullOrEmpty( _key ); } }
		public string Category { get { return _category; } }
		public string Message { get { return _message; } }
		public Exception ErrorInfo { get { return _errorInfo; } }
		public bool IsWarning { get { return _isWarning; } }
		public DateTime TimeStamp { get { return _timeStamp; } }
		public Decimal TimeFromFirst { get { return _timeFromFirst; } }
		public Decimal TimeFromLast { get { return _timeFromLast; } }
		public Decimal KeyDelta { get { return _keyDelta; } }
	}
}