using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using Suplex.Security;
using Suplex.Security.Standard;
using Suplex.Data;
using System.Net.Sockets;
using System.Text;
using System.Net;

namespace Suplex.Forms
{
	public abstract class AuditWriter
	{
		private User _defaultUserContext = User.Unknown;

		public virtual User DefaultUserContext
		{
			get { return _defaultUserContext; }
			set { _defaultUserContext = value; }
		}

		public void Write(User user, AuditType auditType, string source, AuditCategory category, string description)
		{
			this.Write( new AuditEventArgs( user, auditType, source, category, description ) );
		}

		public void Write(User user, AuditType auditType, string source, AuditCategory category, string description, AuditType auditTypeFilter)
		{
			this.Write( new AuditEventArgs( user, auditType, source, category, description ), auditTypeFilter );
		}

		public void Write(AuditEventArgs e, AuditType auditTypeFilter)
		{
			if( ( auditTypeFilter & e.AuditType ) == e.AuditType )
			{
				this.Write( e );
			}
		}

		public abstract void Write(AuditEventArgs e);
	}

	public class AuditFileWriter : AuditWriter
	{
		private string _auditFileName = null;

		public AuditFileWriter(string auditFileName)
		{
			_auditFileName = auditFileName;
		}

		public override void Write(AuditEventArgs e)
		{
			User user = e.User.Equals( User.Unknown ) ? this.DefaultUserContext : e.User;

			if( _auditFileName != null && _auditFileName.Length > 0 )
			{
				//creates file if necessary, opens if exists
				StreamWriter sr = new StreamWriter( _auditFileName, true );
				sr.WriteLine( "{0},{1},{2},{3},{4} ({5}),{6}\r\n{7}",
				new object[] { e.Date, e.AuditType.ToString(), e.Source, e.Category, user.ImpersonationName, user.Description, e.ComputerName, e.Description } );
				sr.Close();
			}
		}
	}

	public class AuditDatabaseWriter : AuditWriter
	{
		private DataAccessor _da = null;

		public AuditDatabaseWriter(DataAccessor platformDataAccessor)
		{
			_da = platformDataAccessor;
		}

		public override void Write(AuditEventArgs e)
		{
			if( _da != null )
			{
				string desc = e.Description;
				if( e.User.ImpersonationContext != null )
				{
					desc = string.Format( "Impersonation Context: {0}\r\n\r\n{1}", e.User.ImpersonationName, desc );
				}

				SortedList parms = new SortedList();

				parms.Add( "@AUDIT_DTTM", e.Date );
				parms.Add( "@SPLX_USER_ID", e.User.Id );
				parms.Add( "@USER_NAME", e.User.Name );
				parms.Add( "@COMPUTER_NAME", e.ComputerName );
				parms.Add( "@AUDIT_TYPE", e.AuditType.ToString() );
				parms.Add( "@AUDIT_SOURCE", e.Source );
				parms.Add( "@AUDIT_CATEGORY", e.Category );
				parms.Add( "@AUDIT_DESC", desc );

				_da.ExecuteSP( "splx.splx_dal_ins_auditlog", parms );
			}
		}
	}

	public class AuditEventLogWriter : AuditWriter
	{
		private string _logSource = "Suplex";
		private string _logName = "SuplexAudit";

		public AuditEventLogWriter() { }

		public AuditEventLogWriter(string logSource, string logName)
		{
			_logSource = logSource;
			_logName = logName;
		}

		public string LogSource
		{
			get { return _logSource; }
			set { _logSource = value; }
		}

		public string LogName
		{
			get { return _logName; }
			set { _logName = value; }
		}

		public override void Write(AuditEventArgs e)
		{
			// Create the source, if it does not already exist.
			if( !EventLog.SourceExists( _logSource ) )
			{
				EventLog.CreateEventSource( _logSource, _logName );
			}

			// Create an EventLog instance and assign its source.
			EventLog log = new EventLog();
			log.Source = _logSource;

			AuditType at = e.AuditType;
			if( at == AuditType.ControlDetail )
			{
				at = AuditType.Information;
			}

			EventLogEntryType type = (EventLogEntryType)Enum.Parse( typeof( EventLogEntryType ), at.ToString(), true );

			string desc = e.Description;
			if( e.User.ImpersonationContext != null )
			{
				desc = string.Format( "Impersonation Context: {0}\r\n\r\n{1}", e.User.ImpersonationName, desc );
			}

			// Write entry to the event log.
			log.WriteEntry( desc, type ); //, 100, 3 );
		}
	}

	public class AuditSyslogWriter : AuditWriter
	{
		private string _host = string.Empty;
		private int _port = 514;
		private SyslogFacility _facility = SyslogFacility.Local0;
		private SyslogSeverity _severity = SyslogSeverity.Information;
		private bool _haveSetSeverity = false;
		private static object syslogLock = new Object();

		public AuditSyslogWriter(string host)
		{
			_host = host;
		}
		public AuditSyslogWriter(string host, int port)
		{
			_host = host;
			_port = port;
		}

		public string Host { get { return _host; } set { _host = value; } }
		public int Port { get { return _port; } set { _port = value; } }
		public SyslogFacility Facility { get { return _facility; } set { _facility = value; } }
		public SyslogSeverity Severity
		{
			get { return _severity; }
			set
			{
				_severity = value;
				_haveSetSeverity = true;
			}
		}
		public bool UseStaticSeverity { get { return _haveSetSeverity; } set { _haveSetSeverity = value; } }

		public override void Write(AuditEventArgs e)
		{
			if( !_haveSetSeverity )
			{
				switch( e.AuditType )
				{
					case AuditType.Error:
					{
						_severity = SyslogSeverity.Error;
						break;
					}
					case AuditType.Warning:
					case AuditType.FailureAudit:
					{
						_severity = SyslogSeverity.Warning;
						break;
					}
					case AuditType.Information:
					case AuditType.SuccessAudit:
					{
						_severity = SyslogSeverity.Information;
						break;
					}
					case AuditType.ControlDetail:
					{
						_severity = SyslogSeverity.Debug;
						break;
					}
				}
			}

			this.Write( e, _facility, _severity );
		}

		public void Write(AuditEventArgs e, SyslogFacility facility, SyslogSeverity severity)
		{
			byte[] data = new byte[1024];
			int pos = 0;
			pos = WritePRIData( data, pos, e, facility, severity );
			pos = WriteHEADERData( data, pos, e );
			pos = WriteMSGData( data, pos, e );

			byte[] logEntry = new byte[pos];
			Array.Copy( data, logEntry, pos );

			if( string.IsNullOrEmpty( _host ) )
			{
				_host = Dns.GetHostEntry( Dns.GetHostName() ).AddressList[0].ToString();
			}

			lock( syslogLock )
			{
				UdpClient udp = new UdpClient( _host, _port );
				udp.Send( logEntry, logEntry.Length );
				udp.Close();
			}
		}

		/// <summary>
		/// Writes the PRI section.
		/// See also section 4.1.1 at http://www.faqs.org/rfcs/rfc3164.html
		/// </summary>
		/// <param name="data">byte[]</param>
		/// <param name="position">int</param>
		/// <param name="entry">LogEntry</param>
		/// <param name="facility">configured facility (user-level message/local0..local7)</param>
		/// <returns>Next byte position to write</returns>
		private int WritePRIData(byte[] data, int position, AuditEventArgs e, SyslogFacility facility, SyslogSeverity severity)
		{
			//int severity = 7;	// debug level severity
			//if( level == SyslogLevel.Error )
			//    severity = 3;
			//if( level == SyslogLevel.Warning )
			//    severity = 4;
			//if( level == SyslogLevel.Information )
			//    severity = 6;

			// write lead
			data[position++] = (byte)'<';
			// calc and write priority:
			string priority = ( (int)facility * 8 + (int)severity ).ToString();
			Encoding.ASCII.GetBytes( priority ).CopyTo( data, position );
			position += priority.Length;
			// write lead out
			data[position++] = (byte)'>';

			// return next byte pos. to write (rem. we work zero based!):
			return position;
		}

		/// <summary>
		/// Writes the HEADER section.
		/// See also section 4.1.2 at http://www.faqs.org/rfcs/rfc3164.html
		/// </summary>
		/// <param name="data">byte[]</param>
		/// <param name="position">int</param>
		/// <param name="entry">LogEntry</param>
		/// <returns>Next byte position to write</returns>
		private int WriteHEADERData(byte[] data, int position, AuditEventArgs e)
		{
			// timestamp
			// The TIMESTAMP field is the local time and is in the format 
			// of "Mmm dd hh:mm:ss" (without the quote marks)
			StringBuilder timestampBuilder = new StringBuilder( e.Date.ToString( "MMM" ) );
			if( e.Date.Day < 10 )
			{
				timestampBuilder.AppendFormat( "  {0}", e.Date.ToString( "d HH:mm:ss" ) );
			}
			else
			{
				timestampBuilder.AppendFormat( " {0}", e.Date.ToString( "d HH:mm:ss" ) );
			}
			string timestamp = timestampBuilder.ToString();
			Encoding.ASCII.GetBytes( timestamp ).CopyTo( data, position );
			position += timestamp.Length;
			data[position++] = 32;	// trailing space delimiter

			// hostname
			string machine = e.ComputerName;
			if( machine == null || machine.Length == 0 )
			{
				try { machine = Environment.MachineName; }
				catch { }
			}
			if( machine == null || machine.Length == 0 )
			{
				machine = "localhost";
			}
			Encoding.ASCII.GetBytes( machine ).CopyTo( data, position );
			position += machine.Length;
			data[position++] = 32;	// trailing space delimiter

			// return next byte pos. to write:
			return position;
		}

		/// <summary>
		/// Writes the MSG section.
		/// See also section 4.1.3 at http://www.faqs.org/rfcs/rfc3164.html
		/// </summary>
		/// <param name="data">byte[]</param>
		/// <param name="position">int</param>
		/// <param name="entry">LogEntry</param>
		/// <param name="message">String</param>
		/// <returns>Next byte position to write</returns>
		private int WriteMSGData(byte[] data, int position, AuditEventArgs e)
		{

			// TAG field
			string tag = String.Empty;
			string sep = !string.IsNullOrEmpty( e.Source ) && !string.IsNullOrEmpty( e.Category ) ? "/" : string.Empty;
			if( !string.IsNullOrEmpty( e.Source ) || !string.IsNullOrEmpty( e.Category ) )
			{
				tag = String.Format( "{0}{2}{1}", e.Source, e.Category, sep );
			}
			sep = !string.IsNullOrEmpty( e.User.ImpersonationName ) && !string.IsNullOrEmpty( tag ) ? "/" : string.Empty;
			if( !string.IsNullOrEmpty( e.User.ImpersonationName ) || !string.IsNullOrEmpty( tag ) )
			{
				tag = String.Format( "[{0}{2}{1}]:", e.User.ImpersonationName, tag, sep );
			}

			StringBuilder tagBuilder = new StringBuilder( tag );
			// check for ABNF alphanumeric 
			for( int i = 0; i < tagBuilder.Length; i++ )
				if( Char.IsWhiteSpace( tagBuilder[i] ) ||
					Char.IsControl( tagBuilder[i] ) ||
					Char.IsSymbol( tagBuilder[i] ) )
					tagBuilder[i] = '_';

			// check length:
			////tag = tagBuilder.ToString().Substring( 0, Math.Min( tagBuilder.Length, 32 - tag.Length ) );

			Encoding.ASCII.GetBytes( tag ).CopyTo( data, position );
			position += tag.Length;
			data[position++] = 32;	// trailing space delimiter

			// CONTENT
			string message = e.Description;
			if( message == null )	// should NOT happen. If so, we should NOT send the mesasge at all
				message = String.Empty;

			message = message.Substring( 0, Math.Min( data.Length - position, message.Length ) );
			Encoding.ASCII.GetBytes( message ).CopyTo( data, position );
			position += message.Length;

			// here we return length of data written:
			return position;
		}
	}

	public enum SyslogSeverity
	{
		Emergency = 0,
		Alert = 1,
		Critical = 2,
		Error = 3,
		Warning = 4,
		Notice = 5,
		Information = 6,
		Debug = 7
	}

	public enum SyslogFacility
	{
		Kernel = 0,
		User = 1,
		Mail = 2,
		Daemon = 3,
		Authorization = 4,
		Syslog = 5,
		Lpr = 6,
		News = 7,
		UUCP = 8,
		Cron = 9,
		Security = 10,
		FTP = 11,
		NTP = 12,
		LogAudit = 13,
		LogAlert = 14,
		Clock = 15,
		Local0 = 16,
		Local1 = 17,
		Local2 = 18,
		Local3 = 19,
		Local4 = 20,
		Local5 = 21,
		Local6 = 22,
		Local7 = 23
	}

	//smaller, less function implementation
	class x_AuditSyslogWriter_x : AuditWriter
	{
		private string _host = string.Empty;
		private int _port = 514;

		public x_AuditSyslogWriter_x(string host)
		{
			_host = host;
		}
		public x_AuditSyslogWriter_x(string host, int port)
		{
			_host = host;
			_port = port;
		}

		public string Host { get { return _host; } set { _host = value; } }
		public int Port { get { return _port; } set { _port = value; } }


		public override void Write(AuditEventArgs e)
		{
			if( string.IsNullOrEmpty( _host ) )
			{
				_host = Dns.GetHostEntry( Dns.GetHostName() ).AddressList[0].ToString();
			}

			byte[] desc = Encoding.ASCII.GetBytes( e.Description );

			UdpClient udp = new UdpClient( _host, _port );
			udp.Send( desc, desc.Length );
			udp.Close();
			udp = null;
		}
	}

}//namespace