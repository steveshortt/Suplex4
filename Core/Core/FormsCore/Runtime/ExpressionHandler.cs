using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Reflection;

using WinForms = System.Windows.Forms;
using WebForms = System.Web.UI;


using Suplex.Data;
using Suplex.General;


namespace Suplex.Forms
{
	
	public enum ExpressionType
	{
		None,
		Calculation,
		Method,
		Script,
		StoredProcedure,
		SQLString
	}


	[Serializable()]
	public class ExpressionElements
	{
		[NonSerialized] private Dictionary<string, IValidationControl> _vcc = null;
		private string _expression;
		private ExpressionType _exprType;

		public ExpressionElements(){}


		public ExpressionElements(string expression, ExpressionType exprType)
		{
			_vcc = new Dictionary<string, IValidationControl>();
			_expression = expression;
			_exprType = exprType;
		}


		public ExpressionElements(Dictionary<string, IValidationControl> validationControls, string expression, ExpressionType exprType)
		{
			_vcc = validationControls;
			_expression = expression;
			_exprType = exprType;
		}

		[System.Xml.Serialization.XmlIgnore()]
		public Dictionary<string, IValidationControl> ValidationControls
		{
			get { return _vcc; }
			set { _vcc = value; }
		}

		public string Expression
		{
			get { return _expression; }
			set { _expression = value; }
		}

		public ExpressionType ExprType
		{
			get { return _exprType; }
			set { _exprType = value; }
		}

		public override string ToString()
		{
			return String.Format( "Expression: {0}, ExpressionType: {1}", _expression, _exprType );
		}

	}
	

	/// <summary>
	/// Resolves calculation expressions.
	/// </summary>
	public class ExpressionHandler
	{
		private const string fldEx = @"\[[^/]+?\]";	//matches bracketed values, ex: [sControlName.sChildControlName]	old: \[\w+\] only matched non "." delimited expressions
		private const string dlmEx = @"->\w+|\[|\]";	//matches bracketed and property delimiters, used to strip them, ex: input: [sControlName.sChildControlName->PropertyName], output: sControlName.sChildControlName
		private const string prpEx = @"->\w+";			//matches property delimiter and property, ex: input: [sControlName.sChildControlName->PropertyName], output: ->PropertyName
		private const string sppEx = @"@\w+";			//matches SP Parms, ex: @fieldname
		private const string valEx = @"\{\w+\}";		//matches braced values, ex: {"myValue"}
		private const string brcEx = @"\{|\}";			//matches braces, ex: {}, used to strip them

		//private MSScriptControl.ScriptControlClass msScriptCtl = null;


		public ExpressionHandler(){}

		//~ExpressionHandler() { msScriptCtl = null; }


		public string[] ParseExpressionFields(string Expression)
		{
			string[] m = new string[0];

			if( Expression != null )
			{
				Regex regex = new Regex( fldEx );
				MatchCollection matches = regex.Matches(Expression);
				
				m = new string[matches.Count];
				for( int n=0; n<matches.Count; n++ )
				{
					m[n] = Regex.Replace( matches[n].Value, dlmEx, "" );
				}
			}

			return m;
		}

		public string Calculate(ExpressionElements xe, DataAccessor da)
		{

			if ( xe.Expression.ToLower().IndexOf("date") >= 0 )
			{
				return CalculateDate( xe, da );
			}

			
			bool haveValues = true;

			DataTable t = new DataTable();
			DataColumn dc;
			
			if( xe.ValidationControls != null )
			{
				Regex regex = new Regex( fldEx );
				MatchCollection matches = regex.Matches(xe.Expression);

				for( int n=0; n<matches.Count; n++ )
				{
					FieldInfo fi = ParseField( matches[n].Value, xe.ValidationControls );

					if( fi.HasValue )
					{
						dc = new DataColumn( fi.FQName,
							Type.GetType( ((IValidationControl)xe.ValidationControls[fi.FQName]).Validation.DataType.ToString() ) );
						
						dc.DefaultValue = fi.Value;

						t.Columns.Add( dc );
					}
					else
					{
						haveValues = false;
						break;
					}
				}//for
			}//if

			if( haveValues )
			{
				dc = new DataColumn( "CalculationResult", System.Type.GetType("System.String") );
				dc.Expression = Regex.Replace( xe.Expression, prpEx, "" );
				t.Columns.Add( dc );

				t.Rows.Add( new object[]{} );

				return t.Rows[0]["CalculationResult"].ToString();
			}
			else
			{
				return null;
			}

		}

		/// <summary>
		/// Calculates Date functions by using SQL syntax and SELECT command.
		/// Creates connection to database, but doesn't use it for anything.
		/// Need to write custom parser or find new soln.
		/// </summary>
		/// <param name="xe">ExpressionElements: Expression and Control refs.</param>
		/// <returns>String, calculation result.</returns>
		private string CalculateDate(ExpressionElements xe, DataAccessor da)
		{
			string dateCalc = "";
			
			ParseStringExpression( xe, ref dateCalc );

			dateCalc = "SELECT " + dateCalc + " AS CalculationResult";

			//do this w/o hitting database!
			//Data.DataAccessor da	= new Data.DataAccessor(); //made parm: 0829
			DataSet ds				= da.GetDataSet( dateCalc );
			
			if( ds.Tables != null && ds.Tables[0].Rows.Count > 0 )
			{
				return ds.Tables[0].Rows[0]["CalculationResult"].ToString();
			}
			else
			{
				return null;
			}
		}

		public string EvalScript(ExpressionElements xe)
		{
			MSScriptControl.ScriptControlClass msScriptCtl = new MSScriptControl.ScriptControlClass();
			msScriptCtl.Language = "VBScript";
			//if( msScriptCtl == null )
			//{
			//    msScriptCtl = new MSScriptControl.ScriptControlClass();
			//    msScriptCtl.Language = "VBScript";
			//}

			string script = null;
			string result = string.Empty;

			bool parsed = this.ParseStringExpression( xe, ref script );

			if( parsed )
			{
				try
				{
					result = msScriptCtl.Eval( script ).ToString();
				}
				catch( Exception ex )
				{
					throw new ExpressionException(
						string.Format( "Failed to Eval expression: {0}, Script: {1}.  Exception is: {2}",
						xe.ToString(), script == null ? "(null)" : script, ex.Message ), ex );
				}
			}
			else	//added 06232005
			{
				throw new ExpressionException(
					string.Format( "Failed to parse expression: {0}, Script: {1}",
					xe.ToString(), script == null ? "(null)" : script ) );
			}

			int i=System.Runtime.InteropServices.Marshal.ReleaseComObject( msScriptCtl );
			while( i > 0 )
			{
				System.Runtime.InteropServices.Marshal.ReleaseComObject( msScriptCtl );
			}
			msScriptCtl = null;

			return result;
		}

		public bool ParseMethodExpression(ExpressionElements xe, ref IValidationControl methodObject, ref string methodName, ref object[] parameters)
		{
			bool haveValues = true;
			string parm  = null;

			methodName = xe.Expression;

			if( xe.ValidationControls != null )
			{

				Regex regex = new Regex( fldEx );

				string[] el = methodName.Split('(');
				Match m = regex.Match( el[0] );
				methodName = Regex.Match( m.Value, prpEx ).Value.Replace( "->", "" );
				string moKey = Regex.Replace( m.Value, dlmEx, "" );
				methodObject = xe.ValidationControls[moKey];

				if( el.Length > 1 )
				{

					int any = el[1].IndexOfAny( new char[] {'[','{'} );

					//TODO: Add DataType parser for parms
					if( any != -1 )
					{
						string[] p = el[1].Split(',');
						object[] parms = new object[p.Length];
						Match match = null;
				
						for( int n=0; n<p.Length; n++ )
						{
							match = regex.Match( p[n] );
						
							if( match.Success )
							{
								FieldInfo fi = ParseField( match.Value, xe.ValidationControls );

								if( fi.HasValue )
								{
									parms[n] = fi.Value;
								}
								else
								{
									haveValues = false;
									break;
								}
							}
							else
							{
								match = Regex.Match( p[n], valEx );

								if( match.Success )
								{
									parm  = Regex.Replace( match.Value, brcEx, "" );
									if( parm == "null" ) parm = null;
									parms[n] = parm;
								}
								else
								{
									haveValues = false;
									break;
								}
							}
					
						}//for

						parameters = parms;
					}
					else
					{
						parameters = new object[0];
					}

					
				}

			}//if

			return haveValues;
		}

		[Obsolete( "Use ParseStringExpression instead.", false )]
		public bool ParseSQLExpression(ExpressionElements xe, ref string SQLString)
		{
			return ParseStringExpression( xe, ref SQLString );
		}

		public bool ParseStringExpression(ExpressionElements xe, ref string expression)
		{
			bool haveValues = true;

			expression = xe.Expression;

			if( xe.ValidationControls != null )
			{

				Regex regex = new Regex( fldEx );
				MatchCollection matches = regex.Matches( xe.Expression );

				for( int n = 0; n < matches.Count; n++ )
				{
					FieldInfo fi = ParseField( matches[n].Value, xe.ValidationControls );

					if( fi.HasValue )
					{
						//TODO: Test for single quote here, possibly test fi.ValueType to see if string, and encapsulate w/ single quotes
						expression = expression.Replace( matches[n].Value, fi.Value.ToString() );
					}
					else
					{
						haveValues = false;
						break;
					}
				}//for  

			}//if

			return haveValues;
		}

		//
		// TODO: Verify all hardcoded indexes and make sure they won't fail.
		// 
		public bool ParseStoredProcExpression(ExpressionElements xe, ref string storedProcedureName, ref SortedList inputParms, ref SortedList outputParms, DataAccessor da)
		{
			//Data.DataAccessor da = new Data.DataAccessor();	//made parm: 0829
			SortedList help_parms = new SortedList();

			string[] p = null;
			string parm = null;
			string fld = null;
			object value = null;

			bool haveValues = true;

			p = xe.Expression.Split(',');

			int any = p[0].IndexOfAny( new char[] {' ','@','=','[','{'} );

			if( p.Length == 1 && any == -1 )
			{
				storedProcedureName = p[0];
			}
			else
			{
				storedProcedureName = p[0].Substring( 0, p[0].IndexOf( " " ) );
				help_parms.Add("@objname", storedProcedureName);
				DataSet ds = da.GetDataSet( "sp_help", help_parms );
				DataTable dt = ds.Tables[1];
			

				for( int n=0; n<p.Length; n++ )
				{
					parm = Regex.Match( p[n], sppEx ).Value;
					DataRow[] dr = dt.Select("Parameter_name='" + parm + "'");

					fld = Regex.Match( p[n], fldEx ).Value;
					if( fld.Length > 0 )
					{
						FieldInfo fi = ParseField( fld, xe.ValidationControls );
						
						value = fi.Value;

						if( !fi.HasValue )
						{
							fld = Regex.Match( p[n], valEx ).Value;
							if( fld.Length > 0 )
							{
								fld = Regex.Replace( fld, brcEx, "" );	//strip the braces

								switch( fld.ToLower() )
								{
									case "empty":
									{
										value = String.Empty;
										break;
									}
									case "null":
									{
										value = Convert.DBNull;
										break;
									}
									default:
									{
										value = ConvertFieldType( dr[0]["Type"].ToString(), fld );
										break;
									}
								}
							}
							else
							{
								haveValues = false;
							}
							//	break;	//this will break the outer for loop
						}
						else if( fi.ValueType.Equals( typeof(string) ) )
						{
							value = ConvertFieldType( dr[0]["Type"].ToString(), fi.Value.ToString() );
						}
					}
					else
					{
						fld = Regex.Match( p[n], valEx ).Value;
						if( fld.Length > 0 )
						{
							fld = Regex.Replace( fld, brcEx, "" );	//strip the braces
						}

						value = ConvertFieldType( dr[0]["Type"].ToString(), fld );
					}

					
				
					if( p[n].ToLower().IndexOf( "output" ) <= 0 )
					{
						inputParms.Add( parm, value );
					}
					else
					{
						outputParms.Add( parm, value );
					}

					parm = null;
					fld = null;
					value = null;
				}
			}

			return haveValues;
		}

		/// <summary>
		/// Converts a string value to the proper type for call to a StoredProc.
		/// </summary>
		/// <param name="type">The type to which to convert.</param>
		/// <param name="field">The value to convert.</param>
		/// <returns>The converted field value.</returns>
		/// <remarks>
		///   ***********************  IMPT  ***********************
		///   In theory, this function should never fail.  Either the data should have passed 
		///   the field level validation test, or it was provided as a static value in the call.
		///   
		///   Question: If a Parse does fail, how should the caller handle it?  Should the caller
		///   abort the validation (process as if success), or should the validation fail?
		///   ***********************  IMPT  ***********************
		/// </remarks>
		private object ConvertFieldType(string type, string field)
		{
			object converted = null;

			switch( type )
			{
				case "int":
					converted = Int32.Parse( field );
					break;
				case "bit":
					converted = bool.Parse( field );
					break;
				case "datetime":
					converted = DateTime.Parse( field );
					break;
				case "decimal":
					converted = Decimal.Parse( field );
					break;
				default:
					converted = field;
					break;
			}

			return converted;
		}

		private ExpressionHandler.FieldInfo ParseField(string field, Dictionary<string, IValidationControl> vcc)
		{
			FieldInfo fi = new FieldInfo();

			fi.FQName = Regex.Replace( field, dlmEx, "" ); //strip the delimeters( remove [,],->prop )
			string prop = Regex.Match( field, prpEx ).Value.Replace( "->", "" );

			//if( prop.Length == 0 )
			//{
			//    fi.ValueType = typeof( string );
			//    fi.Value = ((Control)vcc[fi.FQName]).Text;
			//}
			//else
			//{
			if( string.IsNullOrEmpty( prop ) ) { prop = "Text"; }
			PropertyInfo pi = ReflectionUtils.GetProperty( vcc[fi.FQName], prop );
			fi.ValueType = pi.PropertyType;
			fi.Value = pi.GetValue( vcc[fi.FQName], null );
			//}

			return fi;
		}


		internal class FieldInfo
		{
			private string _fqName = null;
			private object _value = null;
			private bool _hasValue = false;
			private Type _type = null;

			public FieldInfo(){}

			
			/// <summary>
			/// Fully Qualified Name, ex: ContainerFieldA.ContainerFieldB.ControlUniqueName
			/// </summary>
			public string FQName
			{
				get { return _fqName; }
				set { _fqName = value; }
			}

			public object Value
			{
				get 
				{
					return _value;
				}
				set 
				{
					_value = value;

					if( _value != null )
					{
						_hasValue = true;
						if( _type.Equals( typeof(string) ) )
						{
							if( _value.ToString().Length == 0 )
							{
								_hasValue = false;
							}
						}
					}//if
				}//set
			}

			public bool HasValue
			{
				get { return _hasValue; }
			}

			public Type ValueType
			{
				get { return _type; }
				set { _type = value; }
			}
		}
	}//class



	public class ExpressionException : SystemException
	{
		public ExpressionException() : base(){}
		public ExpressionException(string message) : base( message ){}
		public ExpressionException(string message, Exception innerException) : base( message, innerException ){}
	}

}


#region bunk

		/*
		private string poop(string interval)
		{
			switch(interval)
			{
				case "yyyy":	//Year
				{
					break;
				}
				case "q":	//Quarter
				{
					break;
				}
				case "m":	//Month
				{
					break;
				}
				case "y":	//Day of year
				{
					break;
				}
				case "d":	//Day
				{
					break;
				}
				case "w":	//Weekday
				{
					break;
				}
				case "ww":	//Week
				{
					break;
				}
				case "h":	//Hour
				{
					break;
				}
				case "n":	//Minute
				{
					break;
				}
				case "s":	//Second
				{
					break;
				}
			}
		}
		*/
#endregion