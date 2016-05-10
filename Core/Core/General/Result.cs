using System;
using System.Data;
using System.Data.SqlClient;

namespace Suplex.General
{
	public interface IResult
	{
		bool Success { get; }
		string Message { get; }
	}

	/// <summary>
	/// Base class for simple result reporting.
	/// Success defaults to false.
	/// </summary>
	public class Result : IResult
	{
		private bool _success = false;
		private string _message = string.Empty;


		public Result() { }

		public Result(bool success)
		{
			_success = success;
		}

		public Result(bool success, string message)
		{
			_success = success;
			_message = message;
		}

		public Result(Result result)
		{
			_success = result.Success;
			_message = result.Message;
		}

		public virtual bool Success
		{
			get { return _success; }
			internal protected set { _success = value; }
		}
		public virtual string Message
		{
			get { return _message; }
			internal protected set { _message = value; }
		}


		//internal bool success { get { return _success; } set { _success = value; } }
		//internal string message { get { return _message; } set { _message = value; } }

		internal virtual void SetResult(bool success, string message)
		{
			_success = success;
			_message = message;
		}
		internal virtual void SetResult(Result result)
		{
			_success = result.Success;
			_message = result.Message;
		}

		public override string ToString()
		{
			return string.Format( "Success: {0}, Message: {1}", _success, _message );
		}
	}


	/// <summary>
	/// Inherits Suplex.General.Result, includes SqlException, Success defaults to false.
	/// </summary>
	public class SqlResult : Result
	{
		private SqlException _sqlException = null;


		public SqlResult()
			: base()
		{ }

		public SqlResult(bool success)
			: base( success )
		{ }

		public SqlResult(bool success, string message)
			: base( success, message )
		{ }

		public SqlResult(Result result)
			: base( result )
		{ }

		public SqlResult(bool success, string message, SqlException sqlException)
			: base( success, message )
		{
			_sqlException = sqlException;
		}


		public virtual SqlException SqlException
		{
			get { return _sqlException; }
			internal protected set { _sqlException = value; }
		}


		//internal SqlException sqlException { get { return _sqlException; } set { _sqlException = value; } }

		internal virtual void SetResult(bool success, string message, SqlException sqlException)
		{
			base.SetResult(success, message);
			_sqlException = sqlException;
		}


		public override string ToString()
		{
			string sqlException = _sqlException != null ? _sqlException.Message : "[None]";
			return string.Format( "Success: {0}, Message: {1}, SqlException: {2}", base.Success, base.Message, sqlException );
		}
	}
}