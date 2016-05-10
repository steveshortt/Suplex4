using System;
using System.Text;
using System.Reflection;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Web.UI.WebControls;
using System.Xml;
using System.IO;

using WinForms = System.Windows.Forms;
using WebForms = System.Web.UI;
using Wpf = System.Windows;

using dbg = System.Diagnostics.Debug;

using Suplex.Data;
using Suplex.General;
using Suplex.Security;
using api = Suplex.Forms.ObjectModel.Api;


namespace Suplex.Forms
{
	/// <summary>
	/// 
	/// </summary>
	public class ValidationRuleBuilder
	{
		private DataAccessLayer _dal = new DataAccessLayer();
		//private DataAccessor _da						= new DataAccessor();
		private DataSet _dsCache = null;
		//private ValidationControlCollection _topVcc	= null;
		private IValidationControl _topControl = null;
		private ControlType _ctrlType = ControlType.Unknown;
		private Assembly _controlsAssembly = null;
		private bool _asmLoaded = false;

		private ControlCache _vcNameCache = null;

		private EnumUtil _enumUtil = new EnumUtil();


		internal ValidationRuleBuilder() { }


		#region Load (public)
		private void LoadControlsAssembly(IValidationControl c)
		{
			if( !_asmLoaded )
			{
				_ctrlType = EnumUtil.GetControlType( c );

				switch( _ctrlType )
				{
					case ControlType.WinForms:
					{
						if( System.IO.File.Exists( System.Windows.Forms.Application.StartupPath + "\\Suplex.WinForms.dll" ) )
						{
							_controlsAssembly = Assembly.Load( "Suplex.WinForms" );
							_asmLoaded = true;
						}
						break;
					}
					case ControlType.WebForms:
					{
						if( System.IO.File.Exists( System.Web.HttpRuntime.BinDirectory + "\\Suplex.WebForms.dll" ) )
						{
							_controlsAssembly = Assembly.Load( "Suplex.WebForms" );
							_asmLoaded = true;
						}
						break;
					}
					case ControlType.Wpf:
					{
						if( System.IO.File.Exists( System.Windows.Forms.Application.StartupPath + "\\Suplex.Wpf.dll" ) )
						{
							_controlsAssembly = Assembly.Load( "Suplex.Wpf" );
							_asmLoaded = true;
						}
						break;
					}
				}
			}
		}

		/// <summary>
		/// Loads rules from a rules file.
		/// </summary>
		/// <param name="control">The IValidationControl from which to start.</param>
		/// <param name="filePath">The path to the rules file.</param>
		public void LoadRulesFromCache(IValidationControl control, DataSet validationCache)
		{
			LoadControlsAssembly( control );

			_dsCache = validationCache;
			_topControl = control;
			_dal = control.DataAccessLayer;
			_vcNameCache = new ControlCache();

			this.LoadRulesFromCache( control );
			this.ResolveInternalReferences( control );

			_vcNameCache = null;
			_topControl = null;
		}

		public DataSet CreateCache(DataAccessor platformDa, string uniqueName)
		{
			if( platformDa != null )
			{
				DataSet ds = platformDa.GetDataSet( "splx.splx_dal_sel_validationbyuie", new sSortedList( "@UIE_UNIQUE_NAME", uniqueName ) );
				platformDa.NameTablesFromCompositeSelect( ref ds );
				return ds;
			}
			else
			{
				throw new Exception( "Cannot load rules from database if ValidationControl DataAccessLayer.Platform is null." );
			}
		}

		public DataSet CreateCache(string filePath)
		{
			api.SerializationUtility ser = new api.SerializationUtility();
			DataSet ds = ser.DeserializeValidationToDataSet( filePath );
			return ds;
		}

		public DataSet CreateCache(System.IO.TextReader reader)
		{
			api.SerializationUtility ser = new api.SerializationUtility();
			DataSet ds = ser.DeserializeValidationToDataSet( reader );
			return ds;
		}

		public DataSet CreateCache(api.SuplexStore splxStore)
		{
			api.SerializationUtility ser = new api.SerializationUtility();
			DataSet ds = ser.DeserializeValidationToDataSet( splxStore );
			return ds;
		}

		public DataSet CreateCache(System.IO.Stream stream)
		{
			DataSet ds = new DataSet();
			ds.ReadXmlSchema( new StringReader( Properties.Resources.ValidationSchema ) );
			ds.ReadXml( stream, XmlReadMode.IgnoreSchema );
			return ds;
		}

		public void ClearCache()
		{
			if( _dsCache == null ) { _dsCache = new DataSet(); }
			_dsCache.Clear();
			_dsCache.Tables.Clear();
		}

		public string SuplexValidationSchema
		{
			get { return Properties.Resources.ValidationSchema; }
		}

		/// <summary>
		/// Copies rules and fillmaps from another control.
		/// </summary>
		/// <param name="sourceControl">The control from which to copy the rules.</param>
		/// <param name="destControl">The control onto which the rules are copied.</param>
		public void Load(IValidationControl sourceControl, IValidationControl destControl)
		{
			LoadControlsAssembly( sourceControl );

			_dal = destControl.DataAccessLayer;

			CopyRulesMapsFromCollection( sourceControl, destControl );
			ResolveInternalReferences( destControl );
		}
		#endregion


		#region ResolveInternalReferences
		public void ResolveInternalReferences(IValidationControl control)
		{
			_dal = control.DataAccessLayer;
			_topControl = control;
			IEnumerable topCC = _enumUtil.GetChildren( control );

			//RecurseControls( control.ValidationControls );	//03092004

			//03092004 **************************************************
			RecurseRulesForInternalReferences( control, control.Validation.ValidationRules, topCC );
			IterateMapsForInternalReferences( control, control.Validation.FillMaps, topCC );
			control.Validation.ValidationRules.Process( string.Empty, TypeCode.String, control.Validation.DataTypeErrorMessage, _dal.Application, ControlEvents.Initialize );	//09/07/2005
			control.Validation.FillMaps.Process( _dal.Application, ControlEvents.Initialize );

			RecurseControlsForInternalReferences( topCC );
			//if( topCC.Count > 0 ) { }	//removed 12/21/2008 w/ switch from ICollection to IEnumerable
			//03092004 **************************************************

			_topControl = null;
		}

		private void RecurseControlsForInternalReferences(IEnumerable cc)
		{
			IEnumerator ctls = cc.GetEnumerator();
			while( ctls.MoveNext() )
			{
				if( ctls.Current is IValidationControl )
				{
					IValidationControl vc = (IValidationControl)ctls.Current;

					vc.DataAccessLayer = _dal;
					RecurseRulesForInternalReferences( vc, vc.Validation.ValidationRules, cc );
					IterateMapsForInternalReferences( vc, vc.Validation.FillMaps, cc );
					vc.Validation.ValidationRules.Process( string.Empty, TypeCode.String, vc.Validation.DataTypeErrorMessage, _dal.Application, ControlEvents.Initialize );	//09/07/2005
					vc.Validation.FillMaps.Process( _dal.Application, ControlEvents.Initialize );
				}

				IEnumerable children = _enumUtil.GetChildren( ctls.Current );
				RecurseControlsForInternalReferences( children );
				//if( children.Count > 0 ) { }	//removed 12/21/2008 w/ switch from ICollection to IEnumerable
			}
		}

		private void RecurseRulesForInternalReferences(IValidationControl vc, ValidationRuleCollection rc, IEnumerable controlCollection)
		{
			IEnumerator rules = rc.GetEnumerator();
			while( rules.MoveNext() )
			{
				ValidationRule r = (ValidationRule)rules.Current;

				vc.Validation.EventBindings.ValidationEvents.Add( r.EventBinding );

				if( r.ValueType1 == ComparisonValueType.Control )
				{
					ControlCompareValue ccv = new ControlCompareValue( r.CompareValue1.ToString() );
					ccv.ControlRef = (IValidationControl)_vcNameCache.FindControl( ccv.ControlUniqueName, _topControl, controlCollection );
					r.CompareValue1 = ccv;

					if( ccv.ControlRef == null )
					{
						string msg = String.Format(
							"Could not resolve control reference '{0}'.\r\nTop control UniqueName is '{1}'.",
							ccv.ControlUniqueName, _topControl.UniqueName );

						//try	//this won't fly if it's a web project	catch{}
						if( _ctrlType == ControlType.WinForms )
						{
							UniqueNameErrorDlg err = new UniqueNameErrorDlg();
							err.ShowValidationControlError( _topControl, ccv.ControlUniqueName, msg );
						}
						else
						{
							ValidationControlUtils vcu = new ValidationControlUtils();
							msg += "\r\n\r\n" + vcu.DumpHierarchy( _topControl, false );
						}


						throw new SystemException( msg );
					}
				}

				if( r.ValueType2 == ComparisonValueType.Control )
				{
					ControlCompareValue ccv = new ControlCompareValue( r.CompareValue2.ToString() );
					ccv.ControlRef = (IValidationControl)_vcNameCache.FindControl( ccv.ControlUniqueName, _topControl, controlCollection );
					r.CompareValue2 = ccv;

					if( ccv.ControlRef == null )
					{
						string msg = String.Format(
							"Could not resolve control reference '{0}'.\r\nTop control UniqueName is '{1}'.",
							ccv.ControlUniqueName, _topControl.UniqueName );

						//try	//this won't fly if it's a web project	catch{}
						if( _ctrlType == ControlType.WinForms )
						{
							UniqueNameErrorDlg err = new UniqueNameErrorDlg();
							err.ShowValidationControlError( _topControl, ccv.ControlUniqueName, msg );
						}
						else
						{
							ValidationControlUtils vcu = new ValidationControlUtils();
							msg += "\r\n\r\n" + vcu.DumpHierarchy( _topControl, false );
						}


						throw new SystemException( msg );
					}
				}

				if( r.ValueType1 == ComparisonValueType.Expression )
				{
					ExpressionElements value = (ExpressionElements)r.CompareValue1;
					ResolveExpression( ref value, controlCollection );
					r.CompareValue1 = value;
				}

				if( r.ValueType2 == ComparisonValueType.Expression )
				{
					ExpressionElements value = (ExpressionElements)r.CompareValue2;
					ResolveExpression( ref value, controlCollection );
					r.CompareValue2 = value;
				}

				if( r.ErrorControl != null )
				{
					r.ErrorControl =
						(IValidationControl)_vcNameCache.FindControl( r.ErrorControl.ToString(), _topControl, controlCollection );
				}

				if( r.FillMaps.Count > 0 )
				{
					IterateMapsForInternalReferences( vc, r.FillMaps, controlCollection );
				}

				if( r.ElseMaps.Count > 0 )
				{
					IterateMapsForInternalReferences( vc, r.ElseMaps, controlCollection );
				}

				if( r.ValidationRules.Count > 0 )
				{
					RecurseRulesForInternalReferences( vc, r.ValidationRules, controlCollection );
				}

				if( r.ElseRules.Count > 0 )
				{
					RecurseRulesForInternalReferences( vc, r.ElseRules, controlCollection );
				}
			}
		}

		private void IterateMapsForInternalReferences(IValidationControl vc, FillMapCollection FillMaps, IEnumerable controlCollection)
		{
			IEnumerator maps = FillMaps.GetEnumerator();
			while( maps.MoveNext() )
			{
				FillMap map = (FillMap)maps.Current;
				ExpressionElements value = map.ExprElements;
				ResolveExpression( ref value, controlCollection );
				map.ExprElements = value;
				vc.Validation.EventBindings.FillMapEvents.Add( map.EventBinding );

				foreach( DataBinding db in map.DataBindings )
				{
					db.ControlRef = (IValidationControl)_vcNameCache.FindControl( db.ControlName, _topControl, controlCollection );		//old: db.ControlRef = (IValidationControl)vcc[db.ControlName];
					if( db.ControlRef == null )
					{
						string msg = String.Format(
							"Could not resolve control reference '{0}'.\r\nTop control UniqueName is '{1}'.",
							db.ControlName, _topControl.UniqueName );

						//try	//this won't fly if it's a web project	catch{}
						if( _ctrlType == ControlType.WinForms )
						{
							UniqueNameErrorDlg err = new UniqueNameErrorDlg();
							err.ShowValidationControlError( _topControl, db.ControlName, msg );
						}
						else
						{
							ValidationControlUtils vcu = new ValidationControlUtils();
							msg += "\r\n\r\n" + vcu.DumpHierarchy( _topControl, false );
						}


						throw new SystemException( msg );
					}
				}
			}
		}

		private void ResolveExpression(ref ExpressionElements value, IEnumerable vcc)
		{
			Dictionary<string, IValidationControl> vc = new Dictionary<string, IValidationControl>();

			ExpressionHandler eh = new ExpressionHandler();
			string[] flds = eh.ParseExpressionFields( value.Expression );

			for( int n = 0; n < flds.Length; n++ )
			{
				if( !vc.ContainsKey( flds[n] ) )
				{
					IValidationControl ctl = (IValidationControl)_vcNameCache.FindControl( flds[n], _topControl, vcc );
					if( ctl != null )
					{
						vc.Add( flds[n], ctl );		//old: vc.Add( flds[n], (IValidationControl)vcc[flds[n]] );
					}
					else
					{
						string msg = String.Format(
							"Could not resolve control reference '{0}' from\r\n'{1}'.\r\nTop control UniqueName is '{2}'.",
							flds[n], value.Expression, _topControl.UniqueName );

						//try	//this won't fly if it's a web project	catch{}
						if( _ctrlType == ControlType.WinForms )
						{
							UniqueNameErrorDlg err = new UniqueNameErrorDlg();
							err.ShowValidationControlError( _topControl, value.Expression, msg );
						}
						else
						{
							ValidationControlUtils vcu = new ValidationControlUtils();
							msg += "\r\n\r\n" + vcu.DumpHierarchy( _topControl, false );
						}


						throw new SystemException( msg );
					}
				}
			}
			value.ValidationControls = vc;
		}
		#endregion


		#region Load (private) - Build Rules and Maps
		[Obsolete( "Too slow, use LoadRulesFromCache()", true )]
		private void LoadRulesFromDB(object control)
		{
			if( control is IValidationControl )
			{
				IValidationControl vc = (IValidationControl)control;

				_dsCache.Clear();

				SortedList parms = new SortedList();
				parms.Add( "@UIE_UNIQUE_NAME", vc.UniqueName );

				//02102007, updated from GetDataSetMConn to GetDataSet w/ autoManageSqlConnection = false
				_dsCache.Tables.Clear();
				_dal.Platform.OpenConnection();
				//_dal.Platform.GetDataSet( "pf_selUIElementsByUniqueName", parms, _dsCache, "UIElements", false );
				//_dal.Platform.GetDataSet( "pf_selUIElementsByParent", parms, _dsCache, "ChildFields", false );
				//_dal.Platform.GetDataSet( "pf_selUIElementsToValidationRules", parms, _dsCache, "ValidationRules", false );
				//_dal.Platform.GetDataSet( "pf_selUIElementsToFillMapExpressions", parms, _dsCache, "FillMapExpressions", false );
				//_dal.Platform.GetDataSet( "pf_selUIElementsToFillMapDataBindings", parms, _dsCache, "FillMapDataBindings", false );
				_dal.Platform.CloseConnection();


				if( _dsCache.Tables["Field"].Rows.Count > 0 )
				{
					SetFieldProps( vc, _dsCache.Tables["Field"].Rows[0] );
				}


				if( vc.Validation.AllowDynamicControls )
				{
					DataRow[] childFlds = _dsCache.Tables["ChildFields"].Select();
					if( childFlds.Length > 0 )
					{
						AddChildControls( (IValidationContainer)vc, childFlds );
					}
				}


				//WTF??  this was 'SPLX_UI_ELEMENT_ID IS NULL' and i have NO IDEA why.  that select is inconsistent
				//  w/ the pf_selUIElementsToValidationRules sp, which selects from a view inner-joined on the
				//	element pk, so the SPLX_UI_ELEMENT_ID can never be NULL.  At this point, the only thing the
				//	select method is converting the DataRow collection to a DataRow array.
				//  Made change on 08/23/2004
				//Seriously, WTF?? i changed from above to 'SPLX_UI_ELEMENT_ID IS NOT NULL' when it shoulld have been
				//  'VR_PARENT_ID IS NULL'.  The pf_selUIElementsToValidationRules sp will never return any rows
				//  where 'SPLX_UI_ELEMENT_ID IS NOT NULL', so filtering on SPLX_UI_ELEMENT_ID is stoopid.
				//  Made change on 05/27/2005
				//Se NOTE below as to why this call is diff than LoadRulesFromFile.
				DataRow[] rules = _dsCache.Tables["ValidationRules"].Select( "VR_PARENT_ID IS NULL", "VR_SORT_ORDER ASC" );
				RecurseRules( vc.Validation.ValidationRules, rules );


				DataRow[] maps = _dsCache.Tables["FillMapExpressions"].Select(
					"UIE_UNIQUE_NAME = '" + vc.UniqueName + "'", "FME_SORT_ORDER ASC" );
				for( int n = 0; n < maps.Length; n++ )
				{
					vc.Validation.FillMaps.Add( BuildFillMap( maps[n] ) );
				}
			}

			//foreach( IValidationControl ctl in control.ValidationControls.Values )
			IEnumerator children = _enumUtil.GetChildren( control ).GetEnumerator();
			while( children.MoveNext() )
			{
				LoadRulesFromDB( children.Current );
			}

		}//method

		[Obsolete( "Too slow, use CreateCache(filePath) + LoadRulesFromCache()", true )]
		private void LoadRulesFromFile(IValidationControl control, string filePath)
		{
			_dsCache.Clear();
			_dsCache.ReadXml( filePath, XmlReadMode.ReadSchema );

			LoadRulesFromCache( control );
		}

		private void LoadRulesFromCache(object control)
		{
			if( control is IValidationControl )
			{
				IValidationControl vc = (IValidationControl)control;

				_vcNameCache.Add( vc );

				DataRow[] field = _dsCache.Tables["UIElements"].Select( "UIE_UNIQUE_NAME='" + vc.UniqueName + "'" );

				if( field.Length > 0 )
				{
					SetFieldProps( vc, field[0] );


					if( vc.Validation.AllowDynamicControls )
					{
						DataRow[] childFlds = _dsCache.Tables["UIElements"].Select( "UIE_PARENT_ID='" + field[0]["SPLX_UI_ELEMENT_ID"].ToString() + "'" );
						if( childFlds.Length > 0 )
						{
							AddChildControls( (IValidationContainer)vc, childFlds );
						}
					}



					//Seriously, WTF?? see above note
					//  Made change on 05/27/2005
					//NOTE: This initial call is slightly diff than LoadRulesFromDB b/c the _entire_ set of rules is loaded
					//  into memory (from the rules file), as opposed to only loading the rules for a given control at a time.
					//  [pf_selUIElementsToValidationRules] filters by control, so doesn't require SPLX_UI_ELEMENT_ID=["SPLX_UI_ELEMENT_ID"] filter.
					DataRow[] rules = _dsCache.Tables["ValidationRules"].Select( string.Format(
						"VR_PARENT_ID IS NULL AND VR_RULE_TYPE='{0}' AND SPLX_UI_ELEMENT_ID='{1}'",
						LogicRuleType.ValidationIf.ToString(), field[0]["SPLX_UI_ELEMENT_ID"].ToString() ),
						"VR_SORT_ORDER ASC" );
					RecurseRules( vc.Validation.ValidationRules, rules );


					DataRow[] maps = _dsCache.Tables["FillMapExpressions"].Select( string.Format(
						"FME_IF_CLAUSE = true AND SPLX_UIE_VR_PARENT_ID='{0}'", field[0]["SPLX_UI_ELEMENT_ID"].ToString() ),
						"FME_SORT_ORDER ASC" );
					for( int n = 0; n < maps.Length; n++ )
					{
						vc.Validation.FillMaps.Add( BuildFillMap( maps[n] ) );
					}
				}

				//NOTE: moved away from using event handlers in 2.0,
				//    in favor of direct handling in ValidationAccessor class
				if( vc != _topControl &&
					_topControl is IValidationContainer && _topControl.Validation.AutoValidateContainer )
				{
					//vc.Validation.ValidateCompleted +=
					//    ((ValidationContainerAccessor)_topControl.Validation).ValidationCompletedHandler;

					vc.Validation.SetValidationResultHandlerDelegate(
						( (ValidationContainerAccessor)_topControl.Validation ).CurrentValidationResultHandlerMethod );
				}
			}


			//foreach( IValidationControl ctl in control.ValidationControls.Values )
			IEnumerator children = _enumUtil.GetChildren( control ).GetEnumerator();
			while( children.MoveNext() )
			{
				LoadRulesFromCache( children.Current );
			}

		}//method

		private void SetFieldProps(IValidationControl control, DataRow properties)
		{
			if( properties["UIE_DATA_TYPE"] != Convert.DBNull && properties["UIE_DATA_TYPE"].ToString().Length > 0 )
			{
				TypeCode typeCode =
					MiscUtils.ParseEnum<TypeCode>( properties["UIE_DATA_TYPE"].ToString(), true );
				if( typeCode != TypeCode.Empty )
				{
					control.Validation.DataType = typeCode;
				}
					
			}

			if( properties["UIE_DATA_TYPE_ERR_MSG"].ToString().Length > 0 )
			{
				control.Validation.DataTypeErrorMessage = properties["UIE_DATA_TYPE_ERR_MSG"].ToString();
			}

			if( properties.Table.Columns.Contains( "UIE_ALLOW_UNDECLARED" )
				&& properties["UIE_ALLOW_UNDECLARED"].ToString().Length > 0 )
			{
				control.Validation.AllowDynamicControls = (bool)properties["UIE_ALLOW_UNDECLARED"];
			}

			if( control is Suplex.Forms.IValidationTextBox )
			{
				( (IValidationTextBox)control ).FormatString = properties["UIE_FORMAT_STRING"].ToString();
			}

			if( properties.Table.Columns.Contains( "UIE_DESC_TOOLTIP" )
				&& properties["UIE_DESC_TOOLTIP"].ToString().Length > 0
				&& (bool)properties["UIE_DESC_TOOLTIP"]
				&& properties["UIE_DESC"].ToString().Length > 0 )
			{
				control.Validation.ToolTip = properties["UIE_DESC"].ToString();
			}
		}

		private void RecurseRules(ValidationRuleCollection ruleColl, DataRow[] rules)
		{
			foreach( DataRow rule in rules )
			{
				ValidationRule vr = new ValidationRule()
				{
					EventBinding = MiscUtils.ParseEnum<ControlEvents>( rule["VR_EVENT_BINDING"].ToString(), true ),
					CompareDataType = MiscUtils.ParseEnum<TypeCode>( rule["VR_COMPARE_DATA_TYPE"].ToString(), true ),
					Operator = MiscUtils.ParseEnum<ComparisonOperator>( rule["VR_OPERATOR"].ToString(), true ),
					FailParent = (bool)rule["VR_FAIL_PARENT"],
					ErrorMessage = rule["VR_ERROR_MESSAGE"].ToString(),
					ErrorControl = rule["VR_ERROR_UIE_UNIQUE_NAME"] == Convert.DBNull ? null : rule["VR_ERROR_UIE_UNIQUE_NAME"].ToString(),
					ValueType1 = MiscUtils.ParseEnum<ComparisonValueType>( rule["VR_VALUE_TYPE1"].ToString(), true ),
					ValueType2 = MiscUtils.ParseEnum<ComparisonValueType>( rule["VR_VALUE_TYPE2"].ToString(), true )
				};


				if( vr.ValueType1 == ComparisonValueType.Expression )
				{
					vr.CompareValue1 =
						new ExpressionElements( rule["VR_COMPARE_VALUE1"].ToString(),
						MiscUtils.ParseEnum<ExpressionType>( rule["VR_EXPRESSION_TYPE1"].ToString(), true ) );
				}
				else
				{
					vr.CompareValue1
						= rule["VR_COMPARE_VALUE1"].ToString().Length == 0 ? null : rule["VR_COMPARE_VALUE1"];
				}

				if( vr.ValueType2 == ComparisonValueType.Expression )
				{
					vr.CompareValue2 =
						new ExpressionElements( rule["VR_COMPARE_VALUE2"].ToString(),
						MiscUtils.ParseEnum<ExpressionType>( rule["VR_EXPRESSION_TYPE2"].ToString(), true ) );
				}
				else
				{
					vr.CompareValue2
						= rule["VR_COMPARE_VALUE2"].ToString().Length == 0 ? null : rule["VR_COMPARE_VALUE2"];
				}

				ruleColl.Add( vr );


				//'If' maps
				DataRow[] mapex = _dsCache.Tables["FillMapExpressions"].Select( string.Format(
					"FME_IF_CLAUSE = true AND SPLX_UIE_VR_PARENT_ID = '{0}'", rule["SPLX_VALIDATION_RULE_ID"].ToString() ),
					"FME_SORT_ORDER ASC" );
				for( int n = 0; n < mapex.Length; n++ )
				{
					vr.FillMaps.Add( BuildFillMap( mapex[n] ) );
				}

				//'Else' maps
				mapex = _dsCache.Tables["FillMapExpressions"].Select( string.Format(
					"FME_IF_CLAUSE = false AND SPLX_UIE_VR_PARENT_ID = '{0}'", rule["SPLX_VALIDATION_RULE_ID"].ToString() ),
					"FME_SORT_ORDER ASC" );
				for( int n = 0; n < mapex.Length; n++ )
				{
					vr.ElseMaps.Add( BuildFillMap( mapex[n] ) );
				}


				//'If' rules
				DataRow[] chldrn = _dsCache.Tables["ValidationRules"].Select( string.Format(
					"VR_RULE_TYPE='{0}' AND VR_PARENT_ID = '{1}'", LogicRuleType.ValidationIf.ToString(), rule["SPLX_VALIDATION_RULE_ID"].ToString() ),
					"VR_SORT_ORDER ASC" );
				if( chldrn.Length > 0 )
				{
					RecurseRules( vr.ValidationRules, chldrn );
				}

				//'Else' rules
				chldrn = _dsCache.Tables["ValidationRules"].Select( string.Format(
					"VR_RULE_TYPE='{0}' AND VR_PARENT_ID = '{1}'", LogicRuleType.ValidationElse.ToString(), rule["SPLX_VALIDATION_RULE_ID"].ToString() ),
					"VR_SORT_ORDER ASC" );
				if( chldrn.Length > 0 )
				{
					RecurseRules( vr.ElseRules, chldrn );
				}
			}
		}//function

		private FillMap BuildFillMap(DataRow mapExpression)
		{
			FillMap fillMap = new FillMap()
			{
				EventBinding = MiscUtils.ParseEnum<ControlEvents>( mapExpression["FME_EVENT_BINDING"].ToString(), true )
			};

			ExpressionElements xe = new ExpressionElements();
			xe.Expression = mapExpression["FME_EXPRESSION"].ToString().Length > 0 ? mapExpression["FME_EXPRESSION"].ToString() : null;
			xe.ExprType = MiscUtils.ParseEnum<ExpressionType>( mapExpression["FME_EXPRESSION_TYPE"].ToString(), true );

			fillMap.ExprElements = xe;

			DataRow[] bindings = _dsCache.Tables["FillMapDataBindings"].Select(
				"SPLX_FILLMAP_EXPRESSION_ID='" + mapExpression["SPLX_FILLMAP_EXPRESSION_ID"].ToString() + "'", "SPLX_FILLMAP_DATABINDING_ID ASC" );
			for( int n = 0; n < bindings.Length; n++ )
			{
				DataBinding db = new DataBinding()
				{
					ControlName = bindings[n]["FMB_UIE_UNIQUE_NAME"].ToString(),
					PropertyName = bindings[n]["FMB_PROPERTY_NAME"].ToString(),
					DataMember = bindings[n]["FMB_VALUE"].ToString(),
					ConversionRequired = bool.Parse( bindings[n]["FMB_TYPECAST_VALUE"].ToString() ),
					OverrideValue = bool.Parse( bindings[n]["FMB_OVERRIDE_VALUE"].ToString() )
				};

				fillMap.DataBindings.Add( db );
			}

			return fillMap;
		}

		private void AddChildControls(IValidationContainer parentControl, DataRow[] childControls)
		{
			for( int n = 0; n < childControls.Length; n++ )
			{
				//if( !parentControl.ValidationControls.Contains( childControls[n]["UIE_UNIQUE_NAME"].ToString() ) )
				IEnumerable children = _enumUtil.GetChildren( parentControl );
				if( _enumUtil.FindByUniqueName( childControls[n]["UIE_UNIQUE_NAME"].ToString(), children ) == null )
				{
					Type control = null;
					if( _asmLoaded )
					{
						control = _controlsAssembly.GetType( string.Format( "Suplex.{0}.s{1}",
							_ctrlType, childControls[n]["UIE_CONTROL_TYPE"].ToString() ) );
					}

					if( control == null )
					{
						throw new SystemException( "Could not load control type " + childControls[n]["UIE_CONTROL_TYPE"].ToString() + ".", new Exception( "Assembly or type not loaded." ) );
					}
					else
					{
						IValidationControl vc = (IValidationControl)Activator.CreateInstance( control );
						vc.UniqueName = childControls[n]["UIE_UNIQUE_NAME"].ToString();

						Type dataType = Type.GetType( "System." + childControls[n]["UIE_DATA_TYPE"].ToString() );
						vc.Validation.DataType = Type.GetTypeCode( dataType );

						if( childControls[n]["UIE_CONTROL_TYPE"].ToString().ToLower() == "textbox" )
						{
							( (IValidationTextBox)vc ).FormatString = childControls[n]["UIE_FORMAT_STRING"].ToString();
						}

						parentControl.AddChild( vc );
					}
				}
			}//for
		}

		#region Build from control

		private void CopyRulesMapsFromCollection(IValidationControl sourceControl, IValidationControl destControl)
		{
			CopyValidationControlBaseProperties( sourceControl, destControl );
			CopyRulesFromCollection( sourceControl.Validation.ValidationRules, destControl.Validation.ValidationRules );
			CopyFillMapsFromCollection( sourceControl.Validation.FillMaps, destControl.Validation.FillMaps );

			//TODO: this!!!
			////foreach( IValidationControl src in sourceControl.ValidationControls.Values )
			////{
			////    IValidationControl dst = destControl.ValidationControls[src.UniqueName];
			////}
			IValidationControl src = null;
			IValidationControl dst = null;

			//this clause handles "undeclared" fields.
			//creates a field if it's not in the dest ctl ValidationControls collection
			//there's an implied dependency on the field having a FillMap or other routine set it's initial properties
			if( dst == null )
			{
				dst = CloneValidationControl( src );

				if( _controlsAssembly != null )
				{
					//((Control)destControl).Controls.Add( (Control)dvc );
					( (System.Windows.Forms.Control)destControl ).Controls.Add( (System.Windows.Forms.Control)dst );
				}
				else
				{
					( (System.Web.UI.Control)destControl ).Controls.Add( (System.Web.UI.Control)dst );
				}
			}

			CopyRulesMapsFromCollection( src, dst );
			//}
		}


		private void CopyRulesFromCollection(ValidationRuleCollection source, ValidationRuleCollection dest)
		{
			for( int n = 0; n < source.Count; n++ )
			{
				dest.Add( source[n].CloneUnResolved() );
			}
		}


		private void CopyFillMapsFromCollection(FillMapCollection source, FillMapCollection dest)
		{
			for( int n = 0; n < source.Count; n++ )
			{
				dest.Add( source[n].CloneUnResolved() );
			}
		}


		/// <summary>
		/// Makes a shallow copy of an IValidationControl, setting only the base IValidationControl props.
		/// ValidationRules and FillMaps collections are NOT copied.
		/// Other control properties (specific to the control type) are NOT set.
		/// </summary>
		/// <param name="controlToClone">The control to clone.</param>
		/// <returns>The cloned copy of type IValidationControl.</returns>
		private IValidationControl CloneValidationControl(IValidationControl controlToClone)
		{
			Type ctrlType = controlToClone.GetType();

			IValidationControl vc = (IValidationControl)Activator.CreateInstance( ctrlType );

			CopyValidationControlBaseProperties( controlToClone, vc );


			//			vc.AllowUndeclaredControls = controlToClone.AllowUndeclaredControls;
			//			vc.DataAccessLayer = controlToClone.DataAccessLayer;
			//			vc.DataType = controlToClone.DataType;
			//			vc.ToolTip = controlToClone.ToolTip;
			//			vc.UniqueName = controlToClone.UniqueName;
			//
			//			if( ctrlType.ToString().IndexOf("mTextBox") > 0 )
			//			{
			//				((IValidationTextBox)vc).FormatString = ((IValidationTextBox)controlToClone).FormatString;
			//			}

			return vc;
		}


		/// <summary>
		/// Copies only the base IValidationControl props.
		/// ValidationRules and FillMaps collections are NOT copied.
		/// Other control properties (specific to the control type) are NOT set.
		/// </summary>
		/// <param name="sourceControl">The control to copy from.</param>
		/// <param name="destControl">The control to copy to.</param>
		private void CopyValidationControlBaseProperties(IValidationControl sourceControl, IValidationControl destControl)
		{
			destControl.UniqueName = sourceControl.UniqueName;
			destControl.DataAccessLayer = sourceControl.DataAccessLayer;
			destControl.Validation.AllowDynamicControls = sourceControl.Validation.AllowDynamicControls;
			destControl.Validation.DataType = sourceControl.Validation.DataType;
			destControl.Validation.ToolTip = sourceControl.Validation.ToolTip;

			if( sourceControl is IValidationTextBox )
			{
				( (IValidationTextBox)destControl ).FormatString = ( (IValidationTextBox)sourceControl ).FormatString;
			}
		}


		#endregion
		#endregion

	}//class


	#region ControlCompareValue
	internal class ControlCompareValue
	{
		private string _unparsedCompareValue = "";
		private string _controlUniqueName = "";
		private IValidationControl _controlRef = null;
		private string _propertyName = "Text";


		public ControlCompareValue() { }

		public ControlCompareValue(IValidationControl control)
		{
			_controlRef = control;
		}

		public ControlCompareValue(IValidationControl control, string propertyName)
		{
			_controlRef = control;
			_propertyName = propertyName;
		}


		public ControlCompareValue(string unparsedCompareValue)
		{
			_unparsedCompareValue = unparsedCompareValue;

			int i = unparsedCompareValue.IndexOf( "->" );
			if( i > 0 )
			{
				_controlUniqueName = unparsedCompareValue.Substring( 0, i );
				_propertyName = unparsedCompareValue.Substring( i + 2, unparsedCompareValue.Length - ( i + 2 ) );
			}
			else
			{
				_controlUniqueName = unparsedCompareValue;
			}
		}

		public IValidationControl ControlRef
		{
			get
			{
				return _controlRef;
			}
			set
			{
				_controlRef = value;
			}
		}

		public string ControlUniqueName
		{
			get
			{
				return _controlUniqueName;
			}
			set
			{
				_controlUniqueName = value;
			}
		}

		public string PropertyName
		{
			get
			{
				return _propertyName;
			}
			set
			{
				_propertyName = value;
			}
		}

		public string UnparsedCompareValue
		{
			get
			{
				string value = null;

				if( _unparsedCompareValue != null && _unparsedCompareValue.Length > 0 )
				{
					value = _unparsedCompareValue;
				}
				else
				{
					if( _propertyName != null && _propertyName.Length > 0 )
					{
						value = string.Format( "{0}->{1}", _controlUniqueName, _propertyName );
					}
					else
					{
						value = _controlUniqueName;
					}
				}

				return value;
			}
		}

		public object GetValue()
		{
			return ReflectionUtils.GetProperty( _controlRef, _propertyName ).GetValue( _controlRef, null );
		}

		public override string ToString()
		{
			return String.Format( "UniqueName: {0}, PropertyName: {1}, UnparsedCompareValue: {2}", this._controlUniqueName, this._propertyName, this._unparsedCompareValue );
		}
	}
	#endregion


	public class ValidationControlUtils
	{
		private EnumUtil _enumUtil = new EnumUtil();

		public ValidationControlUtils() { }


		public string GetInlineCssString()
		{
			return string.Format( "\r\n<style type=\"text/css\">\r\n{0}\r\n</style>\r\n", Properties.Resources.diagsCSS );
		}

		public DiagInfoStreams DumpValidation(IValidationControl control, bool showDetail, bool copyToClipboard)
		{
			return DumpValidation( control, showDetail, copyToClipboard, true, false, null, null );
		}
		public DiagInfoStreams DumpValidation(IValidationControl control, bool showDetail, bool copyToClipboard,
			bool includeInLineCss, bool includeHtmlPageTags, string pageTitle, string cssPath)
		{
			DiagInfoStreams d = new DiagInfoStreams();
			DumpValidation( control, showDetail, d, 0 );

			d.Html.Insert( 0, string.Format( "<table border=0 cellspacing=0 cellpadding=0><tr><td>{0}</td></tr></table>\r\n<BR><BR>\r\n", d.Html2.ToString() ) );
			d.Html.Insert( 0, "<TABLE class=\"tLevel0\"><TR><TD class=\"HeaderTitle\">Validation Detail<a name=\"vd_top\"></a></TD></TR></TABLE><BR>" );
			d.Html.Insert( 0, "<div id=\"Suplex_Validation_Diagnostics\"><span class=\"suplex\">\r\n" );

			if( includeInLineCss || ( includeHtmlPageTags && string.IsNullOrEmpty( cssPath ) ) )
			{
				d.Html.Insert( 0, GetInlineCssString() );
			}

			if( includeHtmlPageTags )
			{
				string s = string.Format( "<html>\r\n<head>\r\n<title>{0}</title>\r\n<LINK href=\"{1}\" type=\"text/css\" rel=\"stylesheet\">\r\n</head>\r\n<body>\r\n",
					string.IsNullOrEmpty( pageTitle ) ? "Validation Detail" : pageTitle, cssPath );
				d.Html.Insert( 0, s );
			}

			d.Html.Append( "</span></div>\r\n\r\n\r\n" );

			if( includeHtmlPageTags )
			{
				d.Html.Append( "</body></html>" );
			}

			if( copyToClipboard )
			{
				try { WinForms.Clipboard.SetDataObject( d.Text.ToString() ); }
				catch { System.Diagnostics.Debug.WriteLine( d.Text.ToString() ); }
			}

			return d;
		}



		private void DumpValidation(object ctrl, bool showDetail, DiagInfoStreams d, int level)
		{
			string spacer = new string( ' ', level * 2 );
			string spacer2 = spacer.Replace( " ", "&nbsp;&nbsp;" );

			bool isValidationControl = ctrl is IValidationControl;


			int style = level;
			if( level > 0 )
			{
				if( ctrl is ISecureContainer )
				{
					style = ( style % 2 ) + 1;
				}
				else
				{
					style = 3;
				}
			}
			if( !isValidationControl ) { style = 4; }


			string displayName = EnumUtil.GetControlDisplayName( ctrl, string.Empty );
			string controlName = string.Format( "{0}&nbsp;&nbsp;[{1}]", displayName, ctrl.GetType().Name );

			d.Text.AppendFormat( "\r\n{0}{1}:\r\n", spacer, displayName );

			d.Html.AppendFormat(
				"<TABLE class=\"tLevel{0}\">\r\n<TR><TD class=\"Level{0}\"><a name=\"v_{1}\">{2}</a></TD><TD align=\"right\" class=\"Level{0}\"><a href=\"#vd_top\" class=\"top\">top</a></TD></TR>\r\n<TR>\r\n<TD colspan=\"2\">\r\n<TABLE class=\"Inner\">\r\n<TR>\r\n<TD colspan=\"2\">\r\n",
				style, displayName, controlName );
			d.Html2.AppendFormat( "{0}<a href=\"#v_{2}\" class=\"Level{1}\">{3}</a><br>\r\n",
				spacer2, style, displayName, controlName );	// 


			if( isValidationControl )
			{
				IValidationControl control = (IValidationControl)ctrl;

				if( showDetail ) { }

				d.Text.AppendFormat( "{0}  ValidationRules: {1}\r\n", spacer, control.Validation.ValidationRules.Count );
				d.Html.AppendFormat( "<TABLE class=\"Inner\"><TR><TD><b>ValidationRules: {0}</b></TD></TR><TR><TD>\r\n", control.Validation.ValidationRules.Count );
				RecurseRules( control.Validation.ValidationRules, d, level );
				d.Html.Append( "</TD></TR>" );

				d.Text.AppendFormat( "{0}  FillMaps: {1}\r\n", spacer, control.Validation.FillMaps.Count );
				d.Html.AppendFormat( "<TR><TD><BR><b>FillMaps: {0}</b></TD></TR><TR><TD>\r\n", control.Validation.FillMaps.Count );
				IterateMaps( control.Validation.FillMaps, d, level );
				d.Html.Append( "</TD></TR></TABLE>" );


				d.Html.Append( "<p></p></TD>\r\n</TR>\r\n<TR>\r\n<TD width=\"1%\">&nbsp;</TD>\r\n<TD>\r\n" );
			}

			IEnumerator children = _enumUtil.GetChildren( ctrl ).GetEnumerator();
			while( children.MoveNext() )
			{
				if( !( children.Current is WebForms.LiteralControl ) )
				{
					DumpValidation( children.Current, showDetail, d, level + 1 );
				}
			}

			d.Html.Append( "</TD>\r\n</TR>\r\n</TABLE>\r\n</TD>\r\n</TR>\r\n</TABLE>\r\n" );
		}


		private void RecurseRules(ValidationRuleCollection rules, DiagInfoStreams d, int level)
		{
			string spacer = new string( ' ', level * 2 );

			for( int n = 0; n < rules.Count; n++ )
			{
				d.Text.AppendFormat( "{0}   ValidationRule {1}:\r\n", spacer, n + 1 );
				d.Text.AppendFormat( "{0}     On {1} event, compare:\r\n", spacer, rules[n].EventBinding );
				d.Text.AppendFormat( "{0}      [V1]: {1} ({2}) -to-\r\n", spacer,
					rules[n].CompareValue1, rules[n].ValueType1 );
				d.Text.AppendFormat( "{0}      [V2]: {1} ({2})\r\n", spacer,
					rules[n].CompareValue2, rules[n].ValueType2 );
				d.Text.AppendFormat( "{0}      Using {{{1}}} as a {{{2}}} comparison.\r\n", spacer,
					rules[n].Operator, rules[n].CompareDataType );
				d.Text.AppendFormat( "{0}     On fail:\r\n{0}      FailParent = {1}\r\n{0}      Error message = \"{2}\"\r\n",
					spacer, rules[n].FailParent, rules[n].ErrorMessage );


				d.Html.Append( "<TABLE class=\"Inner\">" );
				d.Html.AppendFormat( "<TR><TD colspan=\"3\"><B><font class=\"fLevel2\">ValidationRule {0}:</font></B></TD></TR>\r\n", n + 1 );
				d.Html.Append( "<TR><TD width=\"1px\">&nbsp;</TD>" );
				d.Html.AppendFormat( "<TD colspan=\"2\">On <font class=\"hi4\">{0}</font> event, compare:</TD></TR>\r\n", rules[n].EventBinding );
				d.Html.Append( "<TR><TD width=\"1px\">&nbsp;</TD><TD width=\"1px\">&nbsp;</TD>" );
				d.Html.AppendFormat( "<TD>[V1]: <font class=\"hi4\">{0}</font> ({1}) -to-</TD>\r\n",
					rules[n].CompareValue1, rules[n].ValueType1 );
				d.Html.Append( "<TR><TD width=\"1px\">&nbsp;</TD><TD width=\"1px\">&nbsp;</TD>" );
				d.Html.AppendFormat( "<TD>[V2]: <font class=\"hi4\">{0}</font> ({1})</TD>\r\n",
					rules[n].CompareValue2, rules[n].ValueType2 );
				d.Html.Append( "<TR><TD width=\"1px\">&nbsp;</TD>" );
				d.Html.AppendFormat( "<TD colspan=\"2\">Using <font class=\"hi4\">{0}</font> as a <font class=\"hi4\">{1}</font> comparison.</TD></TR>\r\n",
					rules[n].Operator, rules[n].CompareDataType );
				d.Html.Append( "<TR><TD width=\"1px\">&nbsp;</TD><TD colspan=\"2\">On fail:</TD></TR>\r\n" );
				d.Html.Append( "<TR><TD width=\"1px\">&nbsp;</TD><TD width=\"1px\">&nbsp;</TD>" );
				d.Html.AppendFormat( "<TD>FailParent = <font class=\"hi4\">{0}</font></TD></TR>\r\n<TR><TD colspan=\"2\">&nbsp;</TD><TD>Error message = <font class=\"hi4\">\"{1}\"</font></TD></TR>\r\n",
					rules[n].FailParent, rules[n].ErrorMessage );


				d.Text.AppendFormat( "\r\n{0}     Child FillMaps: {1}\r\n", spacer, rules[n].FillMaps.Count );
				d.Html.Append( "<TR><TD width=\"1px\">&nbsp;</TD><TD width=\"1px\">&nbsp;</TD>" );
				d.Html.AppendFormat( "<TD><BR><B><font class=\"fLevel1\">Child FillMaps: {0}</font></B></TD></TR>\r\n", rules[n].FillMaps.Count );
				d.Html.Append( "<TR><TD width=\"1px\">&nbsp;</TD><TD width=\"1px\">&nbsp;</TD><TD>" );
				IterateMaps( rules[n].FillMaps, d, level );
				d.Html.Append( "</TD></TR>" );

				d.Text.AppendFormat( "\r\n{0}     Child ValidationRules: {1}\r\n\r\n", spacer, rules[n].ValidationRules.Count );
				d.Html.Append( "<TR><TD width=\"1px\">&nbsp;</TD><TD width=\"1px\">&nbsp;</TD>" );
				d.Html.AppendFormat( "<TD><BR><B><font class=\"fLevel1\">Child ValidationRules: {0}</font></B></TD></TR>\r\n", rules[n].ValidationRules.Count );
				d.Html.Append( "<TR><TD width=\"1px\">&nbsp;</TD><TD width=\"1px\">&nbsp;</TD><TD>" );
				RecurseRules( rules[n].ValidationRules, d, level + 1 );

				d.Html.Append( "</TD></TR></TABLE>" );
			}
		}


		private void IterateMaps(FillMapCollection maps, DiagInfoStreams d, int level)
		{
			string spacer = new string( ' ', level * 2 );

			for( int n = 0; n < maps.Count; n++ )
			{
				d.Text.AppendFormat( "{0}   FillMap {1}:\r\n", spacer, n + 1 );
				d.Text.AppendFormat( "{0}     EventBinding: {1}\r\n", spacer, maps[n].EventBinding );
				d.Text.AppendFormat( "{0}     ExpressionElements: {1}\r\n", spacer, maps[n].ExprElements );
				d.Text.AppendFormat( "{0}     DataBindings: {1}\r\n", spacer, maps[n].DataBindings.Count );

				d.Html.Append( "<TABLE class=\"Inner\">" );
				d.Html.AppendFormat( "<TR><TD colspan=\"2\"><B><font class=\"fLevel2\">FillMap {0}:</font></B></TD></TR>\r\n", n + 1 );
				d.Html.AppendFormat( "<TR><TD width=\"1px\">&nbsp;</TD><TD>EventBinding: <font class=\"hi4\">{0}</font></TD></TR>\r\n", maps[n].EventBinding );
				d.Html.AppendFormat( "<TR><TD width=\"1px\">&nbsp;</TD><TD>ExpressionElements: <font class=\"hi4\">{0}</font></TD></TR>\r\n", maps[n].ExprElements );
				d.Html.AppendFormat( "<TR><TD width=\"1px\">&nbsp;</TD><TD>DataBindings: {0}</TD></TR>\r\n", maps[n].DataBindings.Count );

				d.Html.Append( "<TR><TD width=\"1px\">&nbsp;</TD><TD><OL>" );
				for( int db = 0; db < maps[n].DataBindings.Count; db++ )
				{
					d.Text.AppendFormat( "{0}       {2}: {1}\r\n", spacer, maps[n].DataBindings[db], db + 1 );
					d.Html.AppendFormat( "<LI>ControlName: <font class=\"hi4\">{0}</font>, PropertyName: <font class=\"hi4\">{1}</font>, DataMember: <font class=\"hi4\">{2}</font>, ConversionRequired: <font class=\"hi4\">{3}</font>, OverrideValue: <font class=\"hi4\">{4}</font>\r\n",
						maps[n].DataBindings[db].ControlName, maps[n].DataBindings[db].PropertyName, maps[n].DataBindings[db].DataMember, maps[n].DataBindings[db].ConversionRequired, maps[n].DataBindings[db].OverrideValue );
				}
				d.Html.Append( "</OL></TD></TR></TABLE>" );
			}
		}


		public string DumpHierarchy(IValidationControl control, bool copyToClipboard)
		{
			StringBuilder txtBuffer = new StringBuilder();
			SortedList last = new SortedList();

			last[0] = true;

			DumpHierarchy( control, txtBuffer, 0, last );

			if( copyToClipboard )
			{
				try
				{
					System.Windows.Forms.Clipboard.SetDataObject( txtBuffer.ToString() );
				}
				catch
				{
					System.Diagnostics.Debug.WriteLine( txtBuffer.ToString() );
				}
			}

			return txtBuffer.ToString();
		}


		private void DumpHierarchy(object control, StringBuilder buffer, int treeLevel, SortedList last)
		{
			string displayName = EnumUtil.GetControlDisplayName( control, "/" );
			displayName = displayName.PadRight( 30, ' ' );
			displayName = string.Format( "{0}{1}", displayName, control.GetType().Name );

			if( treeLevel > 0 )
			{
				for( int n = 1; n < treeLevel; n++ )
				{
					if( !(bool)last[n] )
					{
						buffer.Append( "│   " );//alt179 or ¦
					}
					else
					{
						buffer.Append( "    " );
					}
				}

				if( (bool)last[treeLevel] )
				{
					buffer.AppendFormat( "└───{0}\r\n", displayName );//alt192+196 or '---
				}
				else
				{
					buffer.AppendFormat( "├───{0}\r\n", displayName );//alt195+196 or +---
				}
			}
			else
			{
				buffer.AppendFormat( "{0}\r\n", displayName );
			}


			int i = 1;
			int c = 0;
			IEnumerator children = _enumUtil.GetChildren( control ).GetEnumerator();
			while( children.MoveNext() )
			{
				if( !( children.Current is WebForms.LiteralControl ) )
				{
					c++;
				}
			}
			children.Reset();
			while( children.MoveNext() )
			{
				if( !( children.Current is WebForms.LiteralControl ) )
				{
					last[treeLevel + 1] = i++ == c;

					DumpHierarchy( children.Current, buffer, treeLevel + 1, last );
				}
			}
		}

	}


	public class DiagInfoStreams
	{
		private StringBuilder _textBuffer = new StringBuilder();
		private StringBuilder _htmlBuffer = new StringBuilder();
		private StringBuilder _htmlBuff2 = new StringBuilder();

		public DiagInfoStreams() { }

		public StringBuilder Text { get { return _textBuffer; } }
		public StringBuilder Html { get { return _htmlBuffer; } }
		internal StringBuilder Html2 { get { return _htmlBuff2; } }
	}
}