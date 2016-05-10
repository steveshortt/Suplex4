using System;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;

using Suplex.Data;
using Suplex.General;

namespace Suplex.Forms
{
	/// <summary>
	/// Summary description for StringValidator.
	/// </summary>
	public class DataComparer
	{
		private ExpressionHandler _expressionHandler = new ExpressionHandler();

		public DataComparer()
		{
		}


		/// <summary>
		/// Compares two string values. 
		/// </summary>
		/// <param name="value1">Either "this" text or a specified parameter.</param>
		/// <param name="value2">Comparison value. Use a bool to test Empty/NotEmpty/Required.</param>
		/// <param name="comparisonOperator">Enum value.</param>
		/// <returns>Suplex.General.Result struct.</returns>
		public Result CompareStrings(string value1, string value2, ComparisonOperator comparisonOperator)
		{
			Result result = new Result( true, "" );
			
			if( value2 != null && value2.Length > 0 )
			{
				switch( comparisonOperator )
				{
					case ComparisonOperator.Empty:
					case ComparisonOperator.NotEmpty:
					case ComparisonOperator.Required:
					{
						result = ComparePresence( value1, value2, comparisonOperator );
						break;
					}
					case ComparisonOperator.RegularExpression:
					{
						result = CompareRegex( value1, value2 );
						break;
					}
					default:
					{
						int compresult = value1.CompareTo(value2.ToString());
						result = EvalComparison( comparisonOperator, compresult, value1, value2 );
						break;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Compares two values of types other than String.
		/// </summary>
		/// <param name="typeCode">The TypeCode of the values to compare.</param>
		/// <param name="value1">Either "this" text or a specified parameter.</param>
		/// <param name="value2">Comparison value. Use a bool to test Empty/NotEmpty/Required.</param>
		/// <param name="comparisonOperator">Enum value.</param>
		/// <returns>Suplex.General.Result struct.</returns>
		public Result CompareType(TypeCode typeCode, object value1, object value2, ComparisonOperator comparisonOperator)
		{
			Result result = new Result( true, "" );

			Type dataType = Type.GetType( "System." + typeCode.ToString() );
			MethodInfo parse = dataType.GetMethod("Parse", new Type[] {typeof(string)});

			#region old datatype validation location
			/*
			 * this test was moved to be prior to rule recursion
			 * 
			//if we have any data, validate the type
			if( value1.ToString().Length > 0 )
			{
				try
				{
					parse.Invoke( dataType, new object[] {value1.ToString()} );
				}
				catch
				{
					result.success = false;
					result.message = "Value must be of type '" + typeCode.ToString() + "'.";
				}
			}
			*
			*
			*/
			#endregion

			//if correct type && we have data for a comparison value
			if( result.Success && value2 != null && value2.ToString().Length > 0 )
			{
				switch( comparisonOperator )
				{
					case ComparisonOperator.Empty:
					case ComparisonOperator.NotEmpty:
					case ComparisonOperator.Required:
					{
						result = ComparePresence( value1.ToString(), value2.ToString(), comparisonOperator );
						break;
					}
					case ComparisonOperator.RegularExpression:
					{
						result = CompareRegex( value1.ToString(), value2.ToString() );
						break;
					}
					default:
					{
						try
						{
							object var1 = Activator.CreateInstance(dataType);
							object var2 = Activator.CreateInstance(dataType);

							var1 = parse.Invoke( dataType, new object[] {value1.ToString()} );	//already know this works from above test
							var2 = parse.Invoke( dataType, new object[] {value2.ToString()} );	//test to see if comparison data is of correct type

							MethodInfo compare = dataType.GetMethod("CompareTo", 
								BindingFlags.Public | BindingFlags.Instance, 
								null,
								CallingConventions.Any,  
								new Type[] {typeof(object)},
								null);

							int compresult = (int)compare.Invoke( var1, new object[] {var2} );	//if didn't fail, then compare values
							result = EvalComparison( comparisonOperator, compresult, value1.ToString(), value2.ToString() );
						}
						catch
						{
							result.Success = true;
							result.Message = "Comparison value must be of type '" + typeCode.ToString() + "'.";
						}
						break;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Executes a stored procedure or SQL statement and evaluates if value1 exists in value2(list).
		/// </summary>
		/// <param name="value1">Data to validate. Static value or "this" text.</param>
		/// <param name="value2">Will be typecasted to ExpressionElements.</param>
		/// <param name="comparisonOperator">Enum value: InList or NotInList.</param>
		/// <returns>Suplex.General.Result struct.</returns>
		public Result CompareList(object value2, ComparisonOperator comparisonOperator, DataAccessor da)
		{
			Result result = new Result( true, "" );

			ExpressionElements xe = (ExpressionElements)value2;
			
			switch( xe.ExprType )
			{
				case ExpressionType.StoredProcedure:
				{
					result = ExecuteSP( xe, comparisonOperator, da );
					break;
				}
				case ExpressionType.SQLString:
				{
					result = ExecuteSQL( xe, comparisonOperator, da );
					break;
				}
				default:
				{
					result = new Result( false, "Improper ExpressionType. Could not evaluate expression." );
					break;
				}
			}

			return result;
		}

		/// <summary>
		/// Validates value for types other than String to verify data is of the specified type.
		/// </summary>
		/// <param name="typeCode">The TypeCode to validate against.</param>
		/// <param name="value">The value to validate.</param>
		/// <returns>Suplex.General.Result struct.</returns>
		public Result DataTypeCheck(TypeCode typeCode, object value)
		{
			Result result = new Result( true, "" );

			if( typeCode != TypeCode.String )	//TypeCode.String doesn't have a "Parse" method
			{

				Type dataType = Type.GetType( "System." + typeCode.ToString() );
				MethodInfo parse = dataType.GetMethod("Parse", new Type[] {typeof(string)});

				//if we have any data, validate the type
				if( value.ToString().Length > 0 )
				{
					try
					{
						parse.Invoke( dataType, new object[] {value.ToString()} );
					}
					catch
					{
						result.Success = false;
						result.Message = "Value must be of type '" + typeCode.ToString() + "'.";
					}
				}

			}

			return result;
		}

		/// <summary>
		/// Attempts to parse a StoredProcedure Expression and then execute it.
		/// </summary>
		/// <param name="xe">Contains the StoredProcedure Expression and field references, if any.</param>
		/// <param name="comparisonOperator">InList or NotInList</param>
		/// <returns>Suplex.General.Result struct.</returns>
		private Result ExecuteSP(ExpressionElements xe, ComparisonOperator comparisonOperator, DataAccessor da)
		{
			Result result = new Result( true, "" );
			bool sp_result = false;

			string SPName = null;
			SortedList inParms = new SortedList();
			SortedList outParms = new SortedList();

			bool parsed = _expressionHandler.ParseStoredProcExpression( xe, ref SPName, ref inParms, ref outParms, da );

			if( parsed )
			{
				//Data.DataAccessor da = new Data.DataAccessor();	//made parm: 0829
				if( outParms.Count > 0 )
				{
					da.ExecuteSP( SPName, inParms, ref outParms );

					foreach( DictionaryEntry op in outParms )
					{
						if( op.Key.ToString().ToLower() == "@success" )
						{
							sp_result = bool.Parse( op.Value.ToString() );
						}
					}
				}
				else
				{
					string[] cols = { "success", "exists", "ok", "allowed" };
					DataSet ds = da.GetDataSet( SPName, inParms );
					if( ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 )
					{
						foreach( string col in cols )
						{
							if( ds.Tables[0].Columns.Contains( col ) )
							{
								sp_result = bool.Parse( ds.Tables[0].Rows[0][col].ToString() );
								break;
							}
						}
					}
				}

				if( !sp_result )
				{
					switch( comparisonOperator )
					{
						case ComparisonOperator.InList:
						{
							result = new Result( false, "The requested item was not found in the list." );
							break;
						}
						case ComparisonOperator.NotInList:
						{
							result = new Result( false, "The requested item cannot be in the list." );
							break;
						}
					}
				}
			}
			else	//added 06232005
			{
				throw new ExpressionException(
					string.Format( "Failed to parse expression: {0}", xe.ToString() ) );
			}

			return result;
		}

		/// <summary>
		/// Attempts to parse a SQL Expression and then execute it.
		/// </summary>
		/// <param name="xe">Contains the SQL Expression and field references, if any.</param>
		/// <param name="comparisonOperator">InList or NotInList</param>
		/// <returns>Suplex.General.Result struct.</returns>
		private Result ExecuteSQL(ExpressionElements xe, ComparisonOperator comparisonOperator, DataAccessor da)
		{
			Result result = new Result( true, "" );
			string sql = null;

			bool parsed = _expressionHandler.ParseStringExpression( xe, ref sql );

			if( parsed )
			{
				//Data.DataAccessor da = new Data.DataAccessor();	//made parm: 0829
				DataSet ds = da.GetDataSet( sql );

				switch( comparisonOperator )
				{
					case ComparisonOperator.InList:
					{
						if( ds.Tables[0].Rows.Count == 0 )
						{
							result = new Result( false, "The requested item was not found in the list." );
						}
						break;
					}
					case ComparisonOperator.NotInList:
					{
						if( ds.Tables[0].Rows.Count > 0 )
						{
							result = new Result( false, "The requested item cannot be in the list." );
						}
						break;
					}
				} //end switch
			}
			else	//added 06232005
			{
				throw new ExpressionException(
					string.Format( "Failed to parse expression: {0}, SQL: {1}",
					xe.ToString(), sql == null ? "(null)" : sql ) );
			}

			return result;
		}

		/// <summary>
		/// Evaluates data for conformance to presence rules (Empty/NotEmpty/Required).
		/// </summary>
		/// <param name="value1">Data to validate.</param>
		/// <param name="value2">Must be value that can be converted to bool.</param>
		/// <param name="comparisonOperator">Enum value.</param>
		/// <returns>Suplex.General.Result struct.</returns>
		private Result ComparePresence(string value1, string value2, ComparisonOperator comparisonOperator)
		{
			Result result = new Result( true, "" );
			bool val2bool = false;

			if( value2.Length > 0 )
			{
				try
				{
					val2bool = bool.Parse(value2);
				}
				catch
				{
					result.Success = false;
					result.Message = "Could not perform comparison.";
				}
			}

			if( val2bool )
			{
				switch( comparisonOperator )
				{
					case ComparisonOperator.Empty:
					{
						if( value1.Length > 0 )
						{
							result.Message = "Value must be left empty.";
							result.Success = false;
						}
						break;
					}
					case ComparisonOperator.NotEmpty:
					{
						if( value1.Length == 0 )
						{
							result.Message = "Value must not be empty.";
							result.Success = false;
						}
						break;
					}
					case ComparisonOperator.Required:
					{
						if( value1.Length == 0 )
						{
							result.Message = "Required value.";
							result.Success = false;
						}
						break;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Performs a Regular Expression match comparison.
		/// </summary>
		/// <param name="value1">The value to match.</param>
		/// <param name="value2">The Regular Expression.</param>
		/// <returns>Suplex.General.Result struct.</returns>
		private Result CompareRegex(string value1, string value2)
		{
			Result result = new Result( true, "" );

			if( value2.Length > 0 )
			{
				Regex regex = new Regex(value2);
				Match match = regex.Match(value1);
			
				if ( !match.Success )
				{
					result.Success = false;
					result.Message = "Expression does not match required validation string.";
				}
			}

			return result;
		}

		/// <summary>
		/// Evaluates the integer result from a given Type's Compare method.
		/// </summary>
		/// <param name="comparisonOperator">Enum value.</param>
		/// <param name="ComparisonResult">Interger value result from a given Type's Compare method.</param>
		/// <param name="value1">Not currently used, can be null.  Possible future use for error message construction.</param>
		/// <param name="value2">The value that value1 was compared against.  Used to construct default error message.</param>
		/// <returns>Suplex.General.Result struct.</returns>
		private Result EvalComparison(ComparisonOperator comparisonOperator, int ComparisonResult, string value1, string value2)
		{
			Result result = new Result( true, "" );

			switch( comparisonOperator )
			{
				case ComparisonOperator.Equal:
				{
					if( ComparisonResult != 0 )
					{
						result.Message = "Value must equal '" + value2 + "'.";
						result.Success = false;
					}
					break;
				}
				case ComparisonOperator.NotEqual:
				{
					if( ComparisonResult == 0 )
					{
						result.Message = "Value must not equal '" + value2 + "'.";
						result.Success = false;
					}
					break;
				}
				case ComparisonOperator.GreaterThan:
				{
					if( ComparisonResult <= 0 )
					{
						result.Message = "Value must be greater than '" + value2 + "'.";
						result.Success = false;
					}
					break;
				}
				case ComparisonOperator.GreaterThanEqual:
				{
					if( ComparisonResult < 0)
					{
						result.Message = "Value must be greater than or equal to '" + value2 + "'.";
						result.Success = false;
					}
					break;
				}
				case ComparisonOperator.LessThan:
				{
					if( ComparisonResult >= 0 )
					{
						result.Message = "Value must be less than '" + value2 + "'.";
						result.Success = false;
					}
					break;
				}
				case ComparisonOperator.LessThanEqual:
				{
					if( ComparisonResult > 0 )
					{
						result.Message = "Value must be less than or equal to '" + value2 + "'.";
						result.Success = false;
					}
					break;
				}
			}

			return result;
		}
	}
}