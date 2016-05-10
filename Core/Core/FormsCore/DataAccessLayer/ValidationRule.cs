using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using sf = Suplex.Forms;
using sg = Suplex.General;
using ss = Suplex.Security;

namespace Suplex.Forms.ObjectModel.Api
{
	public partial class SuplexDataAccessLayer
	{
		#region get
		public ValidationRule GetValidationRuleByIdShallow(string id)
		{
			DataSet ds = _da.GetDataSet( "splx.splx_api_sel_vr",
				new sSortedList( "@SPLX_VALIDATION_RULE_ID", id ) );

			if( ds.Tables[0].Rows.Count > 0 )
			{
				ValidationRuleFactory factory = new ValidationRuleFactory();
				return factory.CreateObject( ds.Tables[0].Rows[0] );
			}
			else
			{
				throw new RowNotInTableException( string.Format( "Unable to fetch ValidationRule '{0}' from the data store.", id ) );
			}
		}

		public ValidationRule GetValidationRuleByIdDeep(string id)
		{
			DataSet ds = _da.GetDataSet( "splx.splx_api_sel_vr_withchildren_composite",
				new sSortedList( "@SPLX_VALIDATION_RULE_ID", id ) );
			_da.NameTablesFromCompositeSelect( ref ds );

			if( ds.Tables["ValidationRules"].Rows.Count > 0 )
			{
				ValidationRuleFactory factory = new ValidationRuleFactory();
				string vrIdFilter =
					string.Format( "SPLX_VALIDATION_RULE_ID = '{0}'", id );
				DataRow[] rows = ds.Tables["ValidationRules"].Select( vrIdFilter );
				ValidationRule vr = factory.CreateObject( rows[0] );

				ValidationRulesLoadUtil vrLoader = new ValidationRulesLoadUtil(
					ds.Tables["ValidationRules"], ds.Tables["FillMaps"], ds.Tables["DataBindings"], id );
				ValidationRuleCollection rules = vr.ValidationRules;
				vrLoader.LoadRules( ref rules, LogicRuleType.ValidationIf );
				rules = vr.ElseRules;
				vrLoader.LoadRules( ref rules, LogicRuleType.ValidationElse );

				FillMapFactory fillMapFactory = new FillMapFactory();
				fillMapFactory.DataBindingsTable = ds.Tables["DataBindings"];
				string filter =
					string.Format( "SPLX_UIE_VR_PARENT_ID = '{0}' AND FME_IF_CLAUSE = '{1}'", id, true );
				vr.FillMaps.LoadSuplexObjectTable( ds.Tables["FillMaps"], fillMapFactory, filter, null );
				filter =
					string.Format( "SPLX_UIE_VR_PARENT_ID = '{0}' AND FME_IF_CLAUSE = '{1}'", id, false );
				vr.ElseMaps.LoadSuplexObjectTable( ds.Tables["FillMaps"], fillMapFactory, filter, null );

				return vr;
			}
			else
			{
				throw new RowNotInTableException( string.Format( "Unable to fetch ValidationRule '{0}' from the data store.", id ) );
			}
		}
		#endregion

		#region upsert
		public ValidationRule UpsertValidationRule(ValidationRule vr)
		{
			return this.UpsertValidationRule( vr, vr.ParentUIElementId, vr.ParentId );
		}

		public ValidationRule UpsertValidationRule(ValidationRule vr, Guid parentUIElementId, Guid parRuleId)
		{
			SortedList inparms = this.GetValidationRuleParms( vr, parentUIElementId, parRuleId );

			SqlParameter id = new SqlParameter( "@SPLX_VALIDATION_RULE_ID", SqlDbType.UniqueIdentifier );
			id.Value = vr.Id;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_VALIDATION_RULE_ID", id );

			_da.ExecuteSP( "splx.splx_api_upsert_vr", inparms, ref outparms );
			vr.Id = (Guid)id.Value;

			vr.IsDirty = false;

			return vr;
		}

		private SortedList GetValidationRuleParms(ValidationRule vr, Guid parentUIElementId, Guid parRuleId)
		{
			sSortedList s = new sSortedList( "@VR_NAME", vr.Name );
			s.Add( "@VR_EVENT_BINDING", vr.EventBinding.ToString() );
			s.Add( "@VR_SORT_ORDER", vr.SortOrder );

			s.Add( "@VR_COMPARE_DATA_TYPE", vr.CompareDataType.ToString() );
			s.Add( "@VR_OPERATOR", vr.Operator.ToString() );

			s.Add( "@VR_COMPARE_VALUE1", vr.CompareValue1 );
			s.Add( "@VR_EXPRESSION_TYPE1", vr.ExpressionType1.ToString() );
			s.Add( "@VR_VALUE_TYPE1", vr.ValueType1.ToString() );

			s.Add( "@VR_COMPARE_VALUE2", vr.CompareValue2 );
			s.Add( "@VR_EXPRESSION_TYPE2", vr.ExpressionType2.ToString() );
			s.Add( "@VR_VALUE_TYPE2", vr.ValueType2.ToString() );

			s.Add( "@VR_FAIL_PARENT", vr.FailParent );
			s.Add( "@VR_ERROR_MESSAGE", vr.ErrorMessage );
			s.Add( "@VR_ERROR_UIE_UNIQUE_NAME", vr.ErrorControl );

			s.Add( "@VR_RULE_TYPE", vr.LogicRuleType.ToString() );
			s.Add( "@VR_PARENT_ID", parRuleId == Guid.Empty ? Convert.DBNull : parRuleId );
			s.Add( "@SPLX_UI_ELEMENT_ID", parentUIElementId );

			return s;
		}

		public void UpsertValidationRuleForImport(ValidationRule vr, ref SqlTransaction tr, Guid parentUIElementId, Guid parRuleId)
		{
			SortedList inparms = this.GetValidationRuleParms( vr, parentUIElementId, parRuleId );

			SqlParameter id = new SqlParameter( "@SPLX_VALIDATION_RULE_ID", SqlDbType.UniqueIdentifier );
			id.Value = vr.Id;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_VALIDATION_RULE_ID", id );

			_da.ExecuteSP( "splx.splx_api_upsert_vr", inparms, ref outparms, false, tr );
			vr.Id = (Guid)id.Value;

			foreach( ValidationRule rule in vr.ValidationRules )
			{
				this.UpsertValidationRuleForImport( rule, ref tr, parentUIElementId, vr.Id );
			}
			foreach( ValidationRule rule in vr.ElseRules )
			{
				this.UpsertValidationRuleForImport( rule, ref tr, parentUIElementId, vr.Id );
			}
			foreach( FillMap map in vr.FillMaps )
			{
				this.UpsertFillMapForImport( map, ref tr );
			}
			foreach( FillMap map in vr.ElseMaps )
			{
				this.UpsertFillMapForImport( map, ref tr );
			}
		}
		#endregion

		public void DeleteLogicRuleById(string id)
		{
			SortedList inparms = new sSortedList( "@SPLX_VALIDATION_RULE_ID", id );
			_da.ExecuteSP( "splx.splx_api_del_vr", inparms );
		}

		internal void DeleteValidationRuleRecursive(ValidationRule vr, SqlTransaction tr)
		{
			//walk down the heirarchy to the bottom, delete on the way back up.
			foreach( ValidationRule childVr in vr.ValidationRules )
			{
				this.DeleteValidationRuleRecursive( childVr, tr );
			}

			//walk down the heirarchy to the bottom, delete on the way back up.
			foreach( ValidationRule childVr in vr.ElseRules )
			{
				this.DeleteValidationRuleRecursive( childVr, tr );
			}

			foreach( FillMap fm in vr.FillMaps )
			{
				this.DeleteFillMapById( fm.Id.ToString(), tr );
			}
			foreach( FillMap fm in vr.ElseMaps )
			{
				this.DeleteFillMapById( fm.Id.ToString(), tr );
			}

			SortedList inparms = new sSortedList( "@SPLX_VALIDATION_RULE_ID", vr.Id );
			_da.ExecuteSP( "splx.splx_api_del_vr", inparms, false, tr );
		}
	}

	public class ValidationRuleFactory : ISuplexObjectFactory<ValidationRule>
	{
		public ISuplexObject CreateSuplexObjectBase(DataRow r)
		{
			return this.CreateObject( r );
		}

		public ValidationRule CreateObject(DataRow r)
		{
			ValidationRule vr = new ValidationRule();
			this.CreateObject( r, ref vr );
			return vr;
		}

		public void CreateObject(DataRow r, ref ValidationRule vr)
		{
			vr.Id = new Guid( r["SPLX_VALIDATION_RULE_ID"].ToString() );
			vr.Name = r["VR_NAME"].ToString();
			vr.SortOrder = (int)r["VR_SORT_ORDER"];

			vr.EventBinding = Convert.IsDBNull( r["VR_EVENT_BINDING"] ) ? ControlEvents.None :
				sg.MiscUtils.ParseEnum<ControlEvents>( r["VR_EVENT_BINDING"] );

			vr.CompareValue1 = r["VR_COMPARE_VALUE1"].ToString();

			vr.ValueType1 = Convert.IsDBNull( r["VR_VALUE_TYPE1"] ) ? ComparisonValueType.Empty :
				sg.MiscUtils.ParseEnum<ComparisonValueType>( r["VR_VALUE_TYPE1"] );

			vr.ExpressionType1 = Convert.IsDBNull( r["VR_EXPRESSION_TYPE1"] ) ? ExpressionType.None :
				sg.MiscUtils.ParseEnum<ExpressionType>( r["VR_EXPRESSION_TYPE1"] );

			vr.CompareValue2 = r["VR_COMPARE_VALUE2"].ToString();

			vr.ValueType2 = Convert.IsDBNull( r["VR_VALUE_TYPE2"] ) ? ComparisonValueType.Empty :
				sg.MiscUtils.ParseEnum<ComparisonValueType>( r["VR_VALUE_TYPE2"] );

			vr.ExpressionType2 = Convert.IsDBNull( r["VR_EXPRESSION_TYPE2"] ) ? ExpressionType.None :
				sg.MiscUtils.ParseEnum<ExpressionType>( r["VR_EXPRESSION_TYPE2"] );

			vr.CompareDataType = Convert.IsDBNull( r["VR_COMPARE_DATA_TYPE"] ) ? TypeCode.Empty :
				sg.MiscUtils.ParseEnum<TypeCode>( r["VR_COMPARE_DATA_TYPE"] );

			vr.Operator = Convert.IsDBNull( r["VR_OPERATOR"] ) ? ComparisonOperator.Empty :
				sg.MiscUtils.ParseEnum<ComparisonOperator>( r["VR_OPERATOR"] );

			vr.LogicRuleType = sg.MiscUtils.ParseEnum<LogicRuleType>( r["VR_RULE_TYPE"] );

			vr.FailParent = (bool)r["VR_FAIL_PARENT"];
			vr.ErrorControl = r["VR_ERROR_UIE_UNIQUE_NAME"].ToString();
			vr.ErrorMessage = r["VR_ERROR_MESSAGE"].ToString();
		}
	}

	internal class ValidationRulesLoadUtil
	{
		private DataTable _rulesLoadTable = null;
		private DataTable _fillMapsLoadTable = null;
		private ValidationRuleFactory _ruleFactory = null;
		private FillMapFactory _fillMapFactory = null;
		string _uieIdFilter = null;

		public ValidationRulesLoadUtil(DataTable rulesTable, DataTable fillMapsTable, DataTable dataBindingsTable, string uieIdFilter)
		{
			if( string.IsNullOrEmpty( uieIdFilter ) )
			{
				throw new ArgumentException( "uieIdFilter cannot be Null or Empty" );
			}

			_rulesLoadTable = rulesTable;
			_fillMapsLoadTable = fillMapsTable;
			_uieIdFilter = uieIdFilter;

			_ruleFactory = new ValidationRuleFactory();
			_fillMapFactory = new FillMapFactory();
			_fillMapFactory.DataBindingsTable = dataBindingsTable;
		}

		public void LoadRules(ref ValidationRuleCollection rules, LogicRuleType logicRuleType)
		{
			this.RecurseRules( null, rules, logicRuleType );
		}

		private void RecurseRules(string parentId, ValidationRuleCollection rules, LogicRuleType logicRuleType)
		{
			DataRow[] rows = null;

			string parentFilter = string.IsNullOrEmpty( parentId ) ? "IS NULL" : string.Format( "= '{0}'", parentId );
			rows = _rulesLoadTable.Select(
				string.Format( "VR_PARENT_ID {0} AND VR_RULE_TYPE = '{1}' AND SPLX_UI_ELEMENT_ID = '{2}'",
				parentFilter, logicRuleType, _uieIdFilter ) );

			foreach( DataRow r in rows )
			{
				ValidationRule vr = _ruleFactory.CreateObject( r );

				vr = rules.AddOrSynchronize( vr ) as ValidationRule;

				//seriously, wtf is this (fixed, i think, 04072010)
				//seriously, wtf is this (fixed, i think, 08082010)
				//	[SPLX_UIE_VR_PARENT_ID] was [SPLX_UI_ELEMENT_RULE_ID] and {0} wasn't quoted
				string filter =
					string.Format( "SPLX_UIE_VR_PARENT_ID = '{0}' AND FME_IF_CLAUSE = '{1}'",
					r["SPLX_VALIDATION_RULE_ID"].ToString(), true );
				vr.FillMaps.LoadSuplexObjectTable( _fillMapsLoadTable, _fillMapFactory, filter, null );

				filter =
					string.Format( "SPLX_UIE_VR_PARENT_ID = '{0}' AND FME_IF_CLAUSE = '{1}'",
					r["SPLX_VALIDATION_RULE_ID"].ToString(), false );
				vr.ElseMaps.LoadSuplexObjectTable( _fillMapsLoadTable, _fillMapFactory, filter, null );

				this.RecurseRules( r["SPLX_VALIDATION_RULE_ID"].ToString(), vr.ValidationRules, LogicRuleType.ValidationIf );
				this.RecurseRules( r["SPLX_VALIDATION_RULE_ID"].ToString(), vr.ElseRules, LogicRuleType.ValidationElse );

				vr.IsDirty = false;
			}
		}
	}
}