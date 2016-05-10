using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Web.UI.WebControls;

using Suplex.Data;
using Suplex.General;

namespace Suplex.Forms
{
	public interface IFillMap
	{
		string Name { get; set; }
		ExpressionElements ExprElements { get; set; }
		DataBindingCollection DataBindings { get; set; }
		ControlEvents EventBinding { get; set; }
		object Data { get; set; }
	}

	//note: this enum supports serializing fillmaps and isn't to be used in application/consumer code
	public enum FillMapType
	{
		FillMapIf,
		FillMapElse
	}

	public interface IDataBinding
	{
		IValidationControl ControlRef { get; set; }
		string ControlName { get; set; }
		string PropertyName { get; set; }
		object DataMember { get; set; }
		bool ConversionRequired { get; set; }
		bool OverrideValue { get; set; }
	}

	[Serializable()]
	public class FillMap : IFillMap, IObjectModel
	{
		public FillMap()
			: this( true, true )
		{
		}

		private FillMap(bool initDataBindings, bool initEventBinding)
		{
			if( initDataBindings )
			{
				DataBindings = new DataBindingCollection();
			}
			if( initEventBinding )
			{
				EventBinding = ControlEvents.None;
			}
		}


		public FillMap(ExpressionElements exprElements, DataBindingCollection dataBindings)
			: this( true, false )
		{
			ExprElements = exprElements;
			DataBindings = dataBindings;
		}


		public FillMap(ExpressionElements exprElements, DataBindingCollection dataBindings, ControlEvents eventBinding)
			: this( false, false )
		{
			ExprElements = exprElements;
			DataBindings = dataBindings;
			EventBinding = eventBinding;
		}


		public FillMap(ExpressionElements exprElements, DataBinding[] dataBindings, ControlEvents eventBinding)
			: this( false, false )
		{
			ExprElements = exprElements;
			EventBinding = eventBinding;

			for( int n = 0; n < dataBindings.Length; n++ )
			{
				DataBindings.Add( dataBindings[n] );
			}
		}


		public string Name { get; set; }
		public ExpressionElements ExprElements { get; set; }
		/// <summary>
		/// Serialization helper prop, shortcut to ExprElements.Expression
		/// </summary>
		public string Expression { get { return this.ExprElements.Expression; } set { this.ExprElements.Expression = value; } }
		/// <summary>
		/// Serialization helper prop, shortcut to ExprElements.ExprType
		/// </summary>
		public ExpressionType ExpressionType { get { return this.ExprElements.ExprType; } set { this.ExprElements.ExprType = value; } }
		public DataBindingCollection DataBindings { get; set; }
		public ControlEvents EventBinding { get; set; }
		public object Data { get; set; }


		#region ICloneable Members

		internal FillMap CloneUnResolved()
		{
			FillMap fm = new FillMap();

			//copy props
			fm.EventBinding = this.EventBinding;

			fm.ExprElements = new ExpressionElements( this.ExprElements.Expression, this.ExprElements.ExprType );

			for( int n = 0; n < this.DataBindings.Count; n++ )
			{
				fm.DataBindings.Add(
					new DataBinding(
					this.DataBindings[n].ControlName, this.DataBindings[n].PropertyName,
					this.DataBindings[n].DataMember, this.DataBindings[n].ConversionRequired,
					this.DataBindings[n].OverrideValue )
					);
			}

			return fm;
		}

		#endregion


		#region IObjectModel Members
		public ObjectType ObjectType { get { return ObjectType.FillMap; } }
		public ObjectType ValidChildObjectTypes { get { return ObjectType.None; } }
		public bool SupportsChildObjectType(ObjectType objectType)
		{
			return (this.ValidChildObjectTypes & objectType) == objectType;
		}
		[System.Xml.Serialization.XmlIgnore()]
		public IObjectModel ParentObject { get; set; }
		public bool IsDirty { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		#endregion
	}


	[Serializable()]
	public class DataBinding : IDataBinding
	{
		[System.Xml.Serialization.XmlIgnore()]
		public IValidationControl ControlRef { get; set; }
		public string ControlName { get; set; }
		public string PropertyName { get; set; }
		[System.Xml.Serialization.XmlElement( "Value" )]
		public object DataMember { get; set; }
		public bool ConversionRequired { get; set; }
		public bool OverrideValue { get; set; }

		public DataBinding() { }


		public DataBinding(string controlName, string propertyName, object dataMember, bool conversionRequired, bool overrideValue)
		{
			ControlRef = null;
			ControlName = controlName;
			PropertyName = propertyName;
			DataMember = dataMember;
			ConversionRequired = conversionRequired;
			OverrideValue = overrideValue;
		}


		public DataBinding(IValidationControl controlRef, string controlName, string propertyName, string dataMember, bool conversionRequired, bool overrideValue)
		{
			ControlRef = controlRef;
			ControlName = controlName;
			PropertyName = propertyName;
			DataMember = dataMember;
			ConversionRequired = conversionRequired;
			OverrideValue = overrideValue;
		}


		public override string ToString()
		{
			return String.Format( "ControlName: {0}, PropertyName: {1}, DataMember: {2}, ConversionRequired: {3}, OverrideValue: {4}",
				this.ControlName, this.PropertyName, this.DataMember, this.ConversionRequired, this.OverrideValue );
		}

	}


	/// <summary>
	/// Provides a collection of DataBindings.
	/// </summary>
	[Serializable()]
	public class DataBindingCollection : List<DataBinding>
	{
		public DataBindingCollection() { }
	}


	/// <summary>
	/// Provides a collection of FillMaps.
	/// </summary>
	[Serializable()]
	public class FillMapCollection : List<FillMap>
	{
		public FillMapCollection() { }

		public void Process(DataAccessor dataAccessor, ControlEvents EventBinding)
		{
			FillMapProcessor fmp = new FillMapProcessor();
			fmp.ProcessEventMaps( this, dataAccessor, EventBinding );
		}
	}


	internal class FillMapProcessor
	{
		internal DataAccessor _da = null;
		internal ExpressionHandler _expressionHandler = new ExpressionHandler();

		internal FillMapProcessor() { }


		internal void ProcessEventMaps(FillMapCollection fillMaps, DataAccessor da, ControlEvents eventBinding)
		{
			_da = da;

			foreach( FillMap map in fillMaps )
			{
				if( map.EventBinding == eventBinding || eventBinding == ControlEvents.None )		//03102004: Added "|| EventBinding == ControlEvents.None"
				{
					switch( map.ExprElements.ExprType )
					{
						case ExpressionType.Calculation:
						{
							ExecuteCalc( map );
							break;
						}
						case ExpressionType.Method:
						{
							ExecuteMethod( map );
							break;
						}
						case ExpressionType.StoredProcedure:
						{
							ExecuteSP( map );
							break;
						}
						case ExpressionType.SQLString:
						{
							ExecuteSQL( map );
							break;
						}
						case ExpressionType.Script:
						{
							ExecuteScript( map );
							break;
						}
						case ExpressionType.None:
						{
							DataBind( map );
							break;
						}
					}//switch
				}
			}

		}

		private void ExecuteCalc(FillMap map)
		{
			try
			{
				string value = _expressionHandler.Calculate( map.ExprElements, _da );

				DataTable t = new DataTable();
				t.Columns.Add( "CalculationResult" );
				DataRow r = t.NewRow();
				r["CalculationResult"] = value;
				t.Rows.Add( r );

				foreach( DataBinding db in map.DataBindings )
				{
					db.DataMember = "CalculationResult";
				}

				DataBind( map, t );
				map.Data = t;
			}
			catch( Exception ex )	//added 06232005
			{
				throw new ExpressionException(
					string.Format( "Failed to parse expression: {0}, Exception", map.ExprElements.ToString(), ex.Message ), ex );
			}
		}

		private void ExecuteMethod(FillMap map)
		{
			IValidationControl control = null;
			string method = null;
			object[] parms = null;

			bool parsed = _expressionHandler.ParseMethodExpression( map.ExprElements, ref control, ref method, ref parms );

			if( parsed )
			{
				MethodInfo mi = control.GetType().GetMethod( method );
				object result = mi.Invoke( control, parms );

				map.Data = result;


				//if( result != null )
				if( false )
				{
					if( result.GetType().IsValueType )
					{
						foreach( DataBinding db in map.DataBindings )
						{
							ReflectionUtils.SetProperty(
								db.ControlRef,					//(Control)
								db.PropertyName,
								result,
								db.OverrideValue,
								db.ConversionRequired
								);
						}
					}
					else if( result.GetType().IsClass )
					{
						DataBind( map, result );
					}
				}//if( result != null )
			}//if( parsed )
			else	//added 06232005
			{
				throw new ExpressionException(
					string.Format( "Failed to parse expression: {0}", map.ExprElements.ToString() ) );
			}
		}

		/// <summary>
		/// Attempts to parse a StoredProcedure Expression and then execute it.
		/// </summary>
		/// <param name="map">Contains the DataBinding Expression.</param>
		private void ExecuteSP(FillMap map)
		{
			string SPName = null;
			SortedList inParms = new SortedList();
			SortedList outParms = new SortedList();

			bool parsed = _expressionHandler.ParseStoredProcExpression( map.ExprElements, ref SPName, ref inParms, ref outParms, _da );

			if( parsed )
			{
				//Data.DataAccessor da = new Data.DataAccessor();	//static member: 082903
				DataSet ds = _da.GetDataSet( SPName, inParms );
				if( ds != null && ds.Tables.Count > 0 )
				{
					map.Data = ds;									//072304, was:	ds.Tables[0];
					DataBind( map, ds.Tables[0] );
				}
			}
			else	//added 06232005
			{
				System.Text.StringBuilder parms = new System.Text.StringBuilder();
				object value = null;
				foreach( DictionaryEntry ipe in inParms )
				{
					value = ipe.Value;
					if( value == null )
					{
						value = "(null)";
					}
					else if( value.ToString() == string.Empty )
					{
						value = "(empty string)";
					}

					parms.AppendFormat( "\r\nKey: {0}, Value: {1}", ipe.Key.ToString(), value );
				}

				throw new ExpressionException(
					string.Format( "Failed to parse expression: {0}\r\n\r\nParameter Information:{1}",
					map.ExprElements.ToString(), parms.ToString() ) );
			}
		}

		/// <summary>
		/// Attempts to parse a SQL Expression and then execute it.
		/// </summary>
		/// <param name="map">Contains the DataBinding Expression.</param>
		private void ExecuteSQL(FillMap map)
		{
			string sql = null;

			bool parsed = _expressionHandler.ParseStringExpression( map.ExprElements, ref sql );

			if( parsed )
			{
				//Data.DataAccessor da = new Data.DataAccessor();	//static member: 082903
				DataSet ds = _da.GetDataSet( sql );
				if( ds != null && ds.Tables.Count > 0 )
				{
					map.Data = ds;									//072304, was:	ds.Tables[0];
					DataBind( map, ds.Tables[0] );
				}
			}
			else	//added 06232005
			{
				throw new ExpressionException(
					string.Format( "Failed to parse expression: {0}, SQL: {1}",
					map.ExprElements.ToString(), sql == null ? "(null)" : sql ) );
			}
		}

		/// <summary>
		/// Attempts to parse a Script Expression and then execute it.
		/// </summary>
		/// <param name="map">Contains the DataBinding Expression.</param>
		private void ExecuteScript(FillMap map)
		{
			string value = _expressionHandler.EvalScript( map.ExprElements );

			foreach( DataBinding db in map.DataBindings )
			{
				ReflectionUtils.SetProperty(
					db.ControlRef,						//(Control)
					db.PropertyName,
					value,
					db.OverrideValue,
					db.ConversionRequired
					);
			}
		}

		private void DataBind(FillMap map)
		{
			foreach( DataBinding db in map.DataBindings )
			{
				ReflectionUtils.SetProperty(
					db.ControlRef,						//(Control)
					db.PropertyName,
					db.DataMember,
					db.OverrideValue,
					db.ConversionRequired
					);
			}
		}

		private void DataBind(FillMap map, DataTable data)
		{
			foreach( DataBinding db in map.DataBindings )
			{
				if( db.DataMember != null && db.DataMember.ToString().Length > 0 )	//&& data.Rows.Count > 0 )
				{
					if( data.Rows.Count > 0 )
					{
						ReflectionUtils.SetProperty(
							db.ControlRef,							//(Control)
							db.PropertyName,
							data.Rows[0][db.DataMember.ToString()],	//removed 03182004: .ToString(), (was data.Rows[0][db.DataMember.ToString()].ToString(),)
							db.OverrideValue,
							db.ConversionRequired
							);
					}
				}
				else
				{
					ReflectionUtils.SetProperty(
						db.ControlRef,								//(Control)
						db.PropertyName,
						data,
						db.OverrideValue,
						db.ConversionRequired
						);
				}//if
			}//foreach
		}

		private void DataBind(FillMap map, object data)
		{
			foreach( DataBinding db in map.DataBindings )
			{
				PropertyInfo pi = ReflectionUtils.GetProperty( data, db.PropertyName );

				ReflectionUtils.SetProperty(
					db.ControlRef,									//(Control)
					db.PropertyName,
					pi.GetValue( data, null ),
					db.OverrideValue,
					db.ConversionRequired
					);
			}
		}
	}
}