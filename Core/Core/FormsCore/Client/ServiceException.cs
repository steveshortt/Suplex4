using System;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;

namespace Suplex.Forms.ObjectModel.Api
{
	public static class ExceptionConverter
	{
		public static Exception ToException(this WebException wex)
		{
			HttpWebResponse httpResponse = (HttpWebResponse)wex.Response;
			string text = string.Empty;
			using( Stream err = httpResponse.GetResponseStream() )
			{
				text = new StreamReader( err ).ReadToEnd();
			}
			Exception ex = Fault.Deserialize( text ).ToException();
			ex.Data.Add( "ServiceStatusCode", httpResponse.StatusCode );
			return ex;
		}

		public static ServiceException ToServiceException(this WebException wex)
		{
			HttpWebResponse httpResponse = (HttpWebResponse)wex.Response;
			string text = string.Empty;
			using( Stream err = httpResponse.GetResponseStream() )
			{
				text = new StreamReader( err ).ReadToEnd();
			}
			ServiceException ex = Fault.Deserialize( text ).ToServiceException();
			ex.ServiceStatusCode = httpResponse.StatusCode;
			return ex;
		}
	}

	public class ServiceException : Exception
	{
		public ServiceException() : base() { }
		public ServiceException(string message) : base( message ) { }
		public ServiceException(string message, Exception innerException) : base( message, innerException ) { }

		public HttpStatusCode ServiceStatusCode { get; internal set; }
		public string ServiceStackTrace { get; internal set; }
		public string ServiceExceptionType { get; internal set; }

		public override string ToString()
		{
			return string.Format( "{0}\r\n{1}\r\n{1}\r\n{2}\r\n{3}",
				base.ToString(), ServiceStatusCode, ServiceExceptionType, ServiceStackTrace );
		}
	}


	public class Fault
	{
		public Code Code { get; set; }
		public Reason Reason { get; set; }
		public Detail Detail { get; set; }

		public static Fault Deserialize(string data)
		{
			//the Regex cleans the xml of namespaces
			data = Regex.Replace( data, @"\s(xml|i:nil)[^""]+""[^""]+""", string.Empty );
			StringReader r = new StringReader( data );
			XmlSerializer s = new XmlSerializer( typeof( Fault ) );
			return (Fault)s.Deserialize( r );
		}

		public Exception ToException()
		{
			return Detail.ExceptionDetail.ToException();
		}

		public ServiceException ToServiceException()
		{
			return Detail.ExceptionDetail.ToServiceException();
		}
	}

	public class Code
	{
		public string Value { get; set; }
		public Code Subcode { get; set; }
	}

	public class Reason
	{
		public string Text { get; set; }
	}

	public class Detail
	{
		public ExceptionDetail ExceptionDetail { get; set; }
	}

	public class ExceptionDetail
	{
		public string HelpLink { get; set; }
		public ExceptionDetail InnerException { get; set; }
		public string Message { get; set; }
		public string StackTrace { get; set; }
		public string Type { get; set; }

		[XmlIgnore]
		public bool HasInnerException { get { return InnerException != null; } }

		public Exception ToException()
		{
			Exception innerEx = null;
			if( HasInnerException )
			{
				innerEx = InnerException.ToException();
			}

			Exception ex = new Exception( Message, innerEx )
			{
				HelpLink = HelpLink
			};
			ex.Data.Add( "ServiceExceptionType", Type );
			ex.Data.Add( "ServiceStackTrace", StackTrace );

			return ex;
		}

		public ServiceException ToServiceException()
		{
			ServiceException innerEx = null;
			if( HasInnerException )
			{
				innerEx = InnerException.ToServiceException();
			}

			ServiceException ex = new ServiceException( Message, innerEx )
			{
				HelpLink = HelpLink
			};
			ex.ServiceExceptionType = Type;
			ex.ServiceStackTrace = StackTrace;

			return ex;
		}
	}
}