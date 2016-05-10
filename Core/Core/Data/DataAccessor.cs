using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;

using System.ComponentModel;

namespace Suplex.Data
{

	/// <summary>
	/// Abstracted data accessor utilities.
	/// </summary>
	public class DataAccessor : IDataAccessor
	{
		private string _connectionString = string.Empty;
		private System.Data.SqlClient.SqlConnection _sqlConn = new System.Data.SqlClient.SqlConnection();
		private System.Xml.XmlReader _xmlReader = null;


		[Description( "Creates a new instance of the DataAccessor using an empty connection string." )]
		public DataAccessor() { }

		[Description( "Creates a new instance of the DataAccessor using the specified connection string." )]
		public DataAccessor(string connectionString)
		{
			_connectionString = connectionString;
		}

		[Description( "Opens a database connection using the specified DataAccessor.ConnectionString." )]
		public void OpenConnection()
		{
			if( _sqlConn != null ) //was (_sqlConn == null || _sqlConn.State == ConnectionState. .Closed)
			{
				_sqlConn.ConnectionString = _connectionString;
				_sqlConn.Open();
			}
		}

		[Description( "Gets the SqlConnection object for this DataAccessor instance." )]
		public SqlConnection Connection { get { return _sqlConn; } }

		[Description( "Closes the connection to the database." )]
		public void CloseConnection()
		{
			if( _sqlConn != null && _sqlConn.State == ConnectionState.Open )
			{
				_sqlConn.Close();
			}
		}

		/// <summary>
		/// Get/Set connection string for this database.
		/// </summary>
		[Description( "Gets or sets the connection string for this DataAccessor instance." )]
		public string ConnectionString
		{
			get { return _connectionString; }
			set { _connectionString = value; }
		}

		[Description( "Opens a new connection and calls ExecuteReader with CommandBehavior.CloseConnection." )]
		public System.Data.SqlClient.SqlDataReader GetReader(string storedProcedureName)
		{
			SqlConnection sqlConn = new SqlConnection( _connectionString );
			SqlCommand sqlCmd = new SqlCommand( storedProcedureName, sqlConn );
			sqlCmd.CommandType = CommandType.StoredProcedure;
			try
			{
				sqlConn.Open();
				// Generate the reader. CommandBehavior.CloseConnection causes the
				// the connection to be closed when the reader object is closed
				return ( sqlCmd.ExecuteReader( CommandBehavior.CloseConnection ) );
			}
			catch
			{
				sqlConn.Close();
				throw;
			}

		}
		[Description( "Opens a new connection and calls ExecuteReader with CommandBehavior.CloseConnection." )]
		public System.Data.SqlClient.SqlDataReader GetReader(string storedProcedureName, SortedList commandParameters)
		{
			SqlConnection sqlConn = new SqlConnection( _connectionString );
			SqlCommand sqlCmd = new SqlCommand( storedProcedureName, sqlConn );
			sqlCmd.CommandType = CommandType.StoredProcedure;

			if( commandParameters != null )
			{
				foreach( DictionaryEntry cmdParm in commandParameters )
				{
					//sqlCmd.Parameters.Add( cmdParm.Key.ToString(), cmdParm.Value );	//06072006: depredated in 2.0
					sqlCmd.Parameters.AddWithValue( cmdParm.Key.ToString(), cmdParm.Value );
				}
			}

			try
			{
				sqlConn.Open();
				// Generate the reader. CommandBehavior.CloseConnection causes the
				// the connection to be closed when the reader object is closed
				return ( sqlCmd.ExecuteReader( CommandBehavior.CloseConnection ) );
			}
			catch
			{
				sqlConn.Close();
				throw;
			}
		}

		[Description( "Opens a new connection, calls ExecuteNonQuery, then closes the connection." )]
		public void ExecuteSP(string storedProcedureName, SortedList inputParameters)
		{
			ExecuteSP( storedProcedureName, inputParameters, true );
		}
		[Description( "Opens a new connection, calls ExecuteNonQuery, then closes the connection." )]
		public void ExecuteSP(string storedProcedureName, SortedList inputParameters, SqlTransaction trans)
		{
			SortedList _tmp = new SortedList();
			ExecuteSP( storedProcedureName, inputParameters, ref _tmp, true, trans );
		}
		[Description( "Calls ExecuteNonQuery; opens/closes the connection if autoManageSqlConnection=true.\r\nIf autoManageSqlConnection=false, connection must opened/closed manually." )]
		public void ExecuteSP(string storedProcedureName, SortedList inputParameters, bool autoManageSqlConnection)
		{
			SortedList _tmp = new SortedList();
			ExecuteSP( storedProcedureName, inputParameters, ref _tmp, autoManageSqlConnection );
		}
		[Description( "Calls ExecuteNonQuery; opens/closes the connection if autoManageSqlConnection=true.\r\nIf autoManageSqlConnection=false, connection must opened/closed manually." )]
		public void ExecuteSP(string storedProcedureName, SortedList inputParameters, bool autoManageSqlConnection, SqlTransaction trans)
		{
			SortedList _tmp = new SortedList();
			ExecuteSP( storedProcedureName, inputParameters, ref _tmp, autoManageSqlConnection, trans );
		}
		[Description( "Opens a new connection, calls ExecuteNonQuery, then closes the connection." )]
		public void ExecuteSP(string storedProcedureName, SortedList inputParameters, ref SortedList outputParameters)
		{
			ExecuteSP( storedProcedureName, inputParameters, ref outputParameters, true, null );
		}
		[Description( "Calls ExecuteNonQuery; opens/closes the connection if autoManageSqlConnection=true.\r\nIf autoManageSqlConnection=false, connection must opened/closed manually." )]
		public void ExecuteSP(string storedProcedureName, SortedList inputParameters, ref SortedList outputParameters, bool autoManageSqlConnection)
		{
			ExecuteSP( storedProcedureName, inputParameters, ref outputParameters, autoManageSqlConnection, null );
		}
		[Description( "Calls ExecuteNonQuery; opens/closes the connection if autoManageSqlConnection=true.\r\nIf autoManageSqlConnection=false, connection must opened/closed manually." )]
		public void ExecuteSP(string storedProcedureName, SortedList inputParameters, ref SortedList outputParameters, bool autoManageSqlConnection, SqlTransaction trans)
		{

			System.Data.SqlClient.SqlCommand sqlCmd = new System.Data.SqlClient.SqlCommand();
			sqlCmd.CommandType = CommandType.StoredProcedure;
			if( autoManageSqlConnection ) { this.OpenConnection(); } //added 11/12/04, //06072006: added autoManageSqlConnection
			sqlCmd.Connection = this.Connection; //openConnection(); //11/12/04
			if( trans != null ) { sqlCmd.Transaction = trans; }
			sqlCmd.CommandText = storedProcedureName;

			/*
			SqlParameter itemPK;
			itemPK = sqlCmd.Parameters.Add("@pk", 0);
			itemPK.Direction = ParameterDirection.Output;
			*/

			if( inputParameters != null )
			{
				foreach( DictionaryEntry cmdParm in inputParameters )
				{
					if( cmdParm.Value != null && cmdParm.Value.GetType() == typeof( SqlParameter ) )
					{
						sqlCmd.Parameters.Add( (SqlParameter)cmdParm.Value );
					}
					else
					{
						//sqlCmd.Parameters.Add( cmdParm.Key.ToString(), cmdParm.Value );	//06072006: depredated in 2.0
						sqlCmd.Parameters.AddWithValue( cmdParm.Key.ToString(), cmdParm.Value );
					}
				}
			}



			SortedList outParms = null;
			if( outputParameters.Count > 0 )
			{
				outParms = new SortedList();
				SqlParameter parm;
				foreach( DictionaryEntry cmdParm in outputParameters )
				{
					if( cmdParm.Value != null && cmdParm.Value.GetType() == typeof( SqlParameter ) )
					{
						parm = sqlCmd.Parameters.Add( (SqlParameter)cmdParm.Value );
					}
					else
					{
						//parm = sqlCmd.Parameters.Add( cmdParm.Key.ToString(), cmdParm.Value );	//06072006: depredated in 2.0
						parm = sqlCmd.Parameters.AddWithValue( cmdParm.Key.ToString(), cmdParm.Value );
					}

					if( parm.Direction == ParameterDirection.Input )
					{
						parm.Direction = ParameterDirection.Output;
					}

					outParms.Add( cmdParm.Key.ToString(), parm );
				}
			}

			try
			{
				sqlCmd.ExecuteNonQuery();
			}
			catch( SqlException sqlex )
			{
				throw sqlex;
			}
			finally
			{
				if( autoManageSqlConnection ) { CloseConnection(); }	//02062010: seems like this should be here.
			}

			if( outputParameters.Count > 0 )
			{
				foreach( DictionaryEntry cmdParm in outParms )
				{
					outputParameters[cmdParm.Key.ToString()] = ( (SqlParameter)outParms[cmdParm.Key.ToString()] ).Value;
				}
			}

			if( autoManageSqlConnection ) { CloseConnection(); }	//06072006: added autoManageSqlConnection
		}

		[Description( "Calls ExecuteNonQuery; opens/closes the connection if autoManageSqlConnection=true.\r\nIf autoManageSqlConnection=false, connection must opened/closed manually." )]
		public void ExecuteNonQuery(string sql)
		{
			this.ExecuteNonQuery( sql, true, null );
		}
		[Description( "Calls ExecuteNonQuery; opens/closes the connection if autoManageSqlConnection=true.\r\nIf autoManageSqlConnection=false, connection must opened/closed manually." )]
		public void ExecuteNonQuery(string sql, bool autoManageSqlConnection, SqlTransaction trans)
		{

			System.Data.SqlClient.SqlCommand sqlCmd = new System.Data.SqlClient.SqlCommand();
			if( autoManageSqlConnection ) { this.OpenConnection(); }
			sqlCmd.Connection = this.Connection;
			if( trans != null ) { sqlCmd.Transaction = trans; }
			sqlCmd.CommandText = sql;

			try
			{
				sqlCmd.ExecuteNonQuery();
			}
			catch( SqlException sqlex )
			{
				throw sqlex;
			}

			if( autoManageSqlConnection ) { CloseConnection(); }
		}

		[Description( "Opens a new connection, creates and fills a new DataSet, then closes the connection." )]
		public System.Data.DataSet GetDataSet(string storedProcedureName, SortedList commandParameters)
		{
			return this.GetDataSet( storedProcedureName, commandParameters, true, null );
		}
		[Description( "Opens a new connection, creates and fills a new DataSet, then closes the connection." )]
		public System.Data.DataSet GetDataSet(string storedProcedureName, SortedList commandParameters, SqlTransaction trans)
		{
			return this.GetDataSet( storedProcedureName, commandParameters, true, trans );
		}
		[Description( "Creates and fills a new DataSet; opens/closes the connection if autoManageSqlConnection=true.\r\nIf autoManageSqlConnection=false, connection must opened/closed manually." )]
		public System.Data.DataSet GetDataSet(string storedProcedureName, SortedList commandParameters, bool autoManageSqlConnection)
		{
			return this.GetDataSet( storedProcedureName, commandParameters, autoManageSqlConnection, null );
		}
		[Description( "Creates and fills a new DataSet; opens/closes the connection if autoManageSqlConnection=true.\r\nIf autoManageSqlConnection=false, connection must opened/closed manually." )]
		public System.Data.DataSet GetDataSet(string storedProcedureName, SortedList commandParameters, bool autoManageSqlConnection, SqlTransaction trans)
		{
			SqlConnection sqlConn = autoManageSqlConnection ? new SqlConnection( _connectionString ) : this.Connection;	//06072006:autoManageSqlConnection (new)

			SqlCommand sqlCmd = new SqlCommand( storedProcedureName, sqlConn );
			sqlCmd.CommandType = CommandType.StoredProcedure;
			if( trans != null ) { sqlCmd.Transaction = trans; }

			if( commandParameters != null )
			{
				foreach( DictionaryEntry cmdParm in commandParameters )
				{
					if( cmdParm.Value != null && cmdParm.Value.GetType() == typeof( SqlParameter ) )
					{
						sqlCmd.Parameters.Add( (SqlParameter)cmdParm.Value );
					}
					else
					{
						//sqlCmd.Parameters.Add( cmdParm.Key.ToString(), cmdParm.Value ); //03242004: changed cmdParm.Value.ToString(), .ToString()	//06072006: depredated in 2.0
						sqlCmd.Parameters.AddWithValue( cmdParm.Key.ToString(), cmdParm.Value );
					}
				}
			}

			try
			{
				if( autoManageSqlConnection ) { sqlConn.Open(); }	//06072006:autoManageSqlConnection (new)

				SqlDataAdapter sqlDA = new SqlDataAdapter( sqlCmd );
				DataSet ds = new DataSet();
				sqlDA.Fill( ds );
				return ds;
			}
			catch( SqlException sqlex )
			{
				Debug.WriteLine( sqlex.Message );
				throw sqlex;
				//return null;
			}
			finally
			{
				if( autoManageSqlConnection ) { sqlConn.Close(); }	//06072006:autoManageSqlConnection (new)
			}
		}
		[Description( "Opens a new connection, adds/refreshes the specified table in the specified DataSet, then closes the connection." )]
		public void GetDataSet(string storedProcedureName, SortedList commandParameters, DataSet dataSet, string tableName)
		{
			this.GetDataSet( storedProcedureName, commandParameters, dataSet, tableName, true, null );
		}
		[Description( "Adds/refreshes the specified table in the specified DataSet; opens/closes the connection if autoManageSqlConnection=true.\r\nIf autoManageSqlConnection=false, connection must opened/closed manually." )]
		public void GetDataSet(string storedProcedureName, SortedList commandParameters, DataSet dataSet, string tableName, bool autoManageSqlConnection)
		{
			this.GetDataSet( storedProcedureName, commandParameters, dataSet, tableName, autoManageSqlConnection, null );
		}
		[Description( "Adds/refreshes the specified table in the specified DataSet; opens/closes the connection if autoManageSqlConnection=true.\r\nIf autoManageSqlConnection=false, connection must opened/closed manually." )]
		public void GetDataSet(string storedProcedureName, SortedList commandParameters, DataSet dataSet, string tableName, bool autoManageSqlConnection, SqlTransaction trans)
		{
			SqlConnection sqlConn = autoManageSqlConnection ? new SqlConnection( _connectionString ) : this.Connection;	//06072006:autoManageSqlConnection (new)

			SqlCommand sqlCmd = new SqlCommand( storedProcedureName, sqlConn );
			sqlCmd.CommandType = CommandType.StoredProcedure;
			if( trans != null ) { sqlCmd.Transaction = trans; }

			if( commandParameters != null )
			{
				foreach( DictionaryEntry cmdParm in commandParameters )
				{
					//sqlCmd.Parameters.Add( cmdParm.Key.ToString(), cmdParm.Value ); //03242004: changed cmdParm.Value.ToString(), .ToString()	//06072006: depredated in 2.0
					sqlCmd.Parameters.AddWithValue( cmdParm.Key.ToString(), cmdParm.Value );
				}
			}

			try
			{
				if( autoManageSqlConnection ) { sqlConn.Open(); }	//06072006:autoManageSqlConnection (new)

				SqlDataAdapter sqlDA = new SqlDataAdapter( sqlCmd );
				sqlDA.Fill( dataSet, tableName );
			}
			catch( SqlException sqlex )
			{
				Debug.WriteLine( sqlex.Message );
				throw sqlex;
				//return null;
			}
			finally
			{
				if( autoManageSqlConnection ) { sqlConn.Close(); }	//06072006:autoManageSqlConnection (new)
			}
		}

		[Description( "Opens a new connection, creates and fills a new DataSet, then closes the connection." )]
		public System.Data.DataSet GetDataSet(string sql)
		{
			SqlConnection sqlConn = new SqlConnection( _connectionString );

			try
			{
				sqlConn.Open();
				SqlDataAdapter sqlDA = new SqlDataAdapter( sql, sqlConn );
				DataSet ds = new DataSet();
				sqlDA.Fill( ds );
				return ds;
			}
			catch( SqlException sqlex )
			{
				Debug.WriteLine( sqlex.Message );
				throw sqlex;
				//return null;
			}
			finally
			{
				sqlConn.Close();
			}
		}



		[Description( "Opens a new connection and calls ExecuteXmlReader.  Does not close the XmlReader or the connection." )]
		public System.Xml.XmlReader XMLReaderOpen(string storedProcedureName, SortedList commandParameters)
		{
			OpenConnection();
			SqlCommand sqlCmd = new SqlCommand( storedProcedureName, _sqlConn );
			sqlCmd.CommandType = CommandType.StoredProcedure;

			if( commandParameters != null )
			{
				foreach( DictionaryEntry cmdParm in commandParameters )
				{
					//sqlCmd.Parameters.Add( cmdParm.Key.ToString(), cmdParm.Value );	//06072006: depredated in 2.0
					sqlCmd.Parameters.AddWithValue( cmdParm.Key.ToString(), cmdParm.Value );
				}
			}

			try
			{
				_xmlReader = sqlCmd.ExecuteXmlReader();
				return _xmlReader;
			}
			catch
			{
				CloseConnection();
				throw;
			}
		}

		[Description( "Closed the XmlReader and connection." )]
		public void XMLReaderClose()
		{
			_xmlReader.Close();
			CloseConnection();
		}

		[Description( "Opens a new connection, calls ExecuteNonQuery, then closes the connection.  Does not close the XmlReader or the connection." )]
		public SqlException DeleteItem(string storedProcedureName, string primaryKeyName, object primaryKeyValue)
		{
			this.OpenConnection();
			SqlCommand sqlCmd = new SqlCommand( storedProcedureName, this.Connection );
			sqlCmd.CommandType = CommandType.StoredProcedure;
			//sqlCmd.Parameters.Add( primaryKeyName, primaryKeyValue );	//06072006: depredated in 2.0
			sqlCmd.Parameters.AddWithValue( primaryKeyName, primaryKeyValue );

			try
			{
				sqlCmd.ExecuteNonQuery();
			}
			catch( SqlException sqlex )
			{
				return sqlex;
			}
			finally
			{
				this.CloseConnection();
			}

			return null;
		}

		[Description( "Names the tables according to the last table containing a CSV list of table names." )]
		public void NameTablesFromCompositeSelect(ref DataSet ds)
		{
			NameTablesFromCompositeSelect( ref ds, true, ',' );
		}
		[Description( "Names the tables according to the last table containing a delimited list of table names." )]
		public void NameTablesFromCompositeSelect(ref DataSet ds, bool namesTableIsLast, params char[] delimiter)
		{
			int index = namesTableIsLast ? ds.Tables.Count - 1 : 0;

			string[] tables = ds.Tables[index].Rows[0][0].ToString().Split( delimiter );
			for( int n = 0; n < tables.Length; n++ )
			{
				ds.Tables[n].TableName = tables[n];
			}
			ds.Tables[index].TableName = "TableNames";
			//ds.Tables.RemoveAt( index );
		}
	}








	public delegate void SqlExceptionEventHandler(object sender, SqlExceptionEventArgs e);

	public class SqlExceptionEventArgs : EventArgs
	{

		private SqlException _e;

		public SqlExceptionEventArgs(SqlException e)
		{
			_e = e;
		}

		public SqlException Exception
		{
			get
			{
				return _e;
			}
		}
	}
}
