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
		public void UpsertRightRoleRule(RightRoleRule rule, Guid parentUIElementId, Guid parRuleId, ref SqlTransaction tr)
		{
			SortedList inparms = this.GetRightRoleRuleParms( rule, parentUIElementId, parRuleId );

			SqlParameter id = new SqlParameter( "@SPLX_VALIDATION_RULE_ID", SqlDbType.UniqueIdentifier );
			id.Value = rule.Id;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_VALIDATION_RULE_ID", id );

			_da.ExecuteSP( "splx.splx_api_upsert_vr", inparms, ref outparms, false, tr );
			rule.Id = (Guid)id.Value;
			rule.IsDirty = false;
		}

		private SortedList GetRightRoleRuleParms(RightRoleRule rule, Guid parentUIElementId, Guid parRuleId)
		{
			sSortedList s = new sSortedList( "@VR_NAME", rule.Name );
			s.Add( "@VR_EVENT_BINDING", Convert.DBNull );			//doesn't apply to RightRoleRules, only to ValidationRules
			s.Add( "@VR_SORT_ORDER", rule.SortOrder );

			s.Add( "@VR_COMPARE_DATA_TYPE", rule.CompareDataType.ToString() );
			s.Add( "@VR_OPERATOR", rule.Operator.ToString() );

			s.Add( "@VR_COMPARE_VALUE1", rule.CompareValue1 );
			s.Add( "@VR_EXPRESSION_TYPE1", rule.ExpressionType1.ToString() );
			s.Add( "@VR_VALUE_TYPE1", rule.ValueType1.ToString() );

			s.Add( "@VR_COMPARE_VALUE2", rule.CompareValue2 );
			s.Add( "@VR_EXPRESSION_TYPE2", rule.ExpressionType2.ToString() );
			s.Add( "@VR_VALUE_TYPE2", rule.ValueType2.ToString() );

			s.Add( "@VR_FAIL_PARENT", rule.FailParent );
			s.Add( "@VR_ERROR_MESSAGE", Convert.DBNull );			//doesn't apply to RightRoleRules, only to ValidationRules
			s.Add( "@VR_ERROR_UIE_UNIQUE_NAME", Convert.DBNull );	//doesn't apply to RightRoleRules, only to ValidationRules

			s.Add( "@VR_RULE_TYPE", rule.LogicRuleType.ToString() );
			s.Add( "@VR_PARENT_ID", parRuleId == Guid.Empty ? Convert.DBNull : parRuleId );
			s.Add( "@SPLX_UI_ELEMENT_ID", parentUIElementId );

			return s;
		}
	}

	public class RightRoleRuleFactory : ISuplexObjectFactory<RightRoleRule>
	{
		public ISuplexObject CreateSuplexObjectBase(DataRow r)
		{
			return this.CreateObject( r );
		}

		public RightRoleRule CreateObject(DataRow r)
		{
			RightRoleRule rrr = new RightRoleRule();
			this.CreateObject( r, ref rrr );
			return rrr;
		}

		public void CreateObject(DataRow r, ref RightRoleRule rrr)
		{
			rrr.Id = new Guid( r["SPLX_VALIDATION_RULE_ID"].ToString() );
			rrr.Name = r["VR_NAME"].ToString();
			rrr.SortOrder = (int)r["VR_SORT_ORDER"];

			rrr.CompareValue1 = r["VR_COMPARE_VALUE1"].ToString();

			rrr.ValueType1 = Convert.IsDBNull( r["VR_VALUE_TYPE1"] ) ? ComparisonValueType.Empty :
				sg.MiscUtils.ParseEnum<ComparisonValueType>( r["VR_VALUE_TYPE1"] );

			rrr.ExpressionType1 = Convert.IsDBNull( r["VR_EXPRESSION_TYPE1"] ) ? ExpressionType.None :
				sg.MiscUtils.ParseEnum<ExpressionType>( r["VR_EXPRESSION_TYPE1"] );

			rrr.CompareValue2 = r["VR_COMPARE_VALUE2"].ToString();

			rrr.ValueType2 = Convert.IsDBNull( r["VR_VALUE_TYPE2"] ) ? ComparisonValueType.Empty :
				sg.MiscUtils.ParseEnum<ComparisonValueType>( r["VR_VALUE_TYPE2"] );

			rrr.ExpressionType2 = Convert.IsDBNull( r["VR_EXPRESSION_TYPE2"] ) ? ExpressionType.None :
				sg.MiscUtils.ParseEnum<ExpressionType>( r["VR_EXPRESSION_TYPE2"] );

			rrr.CompareDataType = Convert.IsDBNull( r["VR_COMPARE_DATA_TYPE"] ) ? TypeCode.Empty :
				sg.MiscUtils.ParseEnum<TypeCode>( r["VR_COMPARE_DATA_TYPE"] );

			rrr.Operator = Convert.IsDBNull( r["VR_OPERATOR"] ) ? ComparisonOperator.Empty :
				sg.MiscUtils.ParseEnum<ComparisonOperator>( r["VR_OPERATOR"] );

			rrr.FailParent = (bool)r["VR_FAIL_PARENT"];
			rrr.ErrorMessage = null;

			rrr.LogicRuleType = sg.MiscUtils.ParseEnum<LogicRuleType>( r["VR_RULE_TYPE"] );
		}
	}

	internal class RightRoleRulesLoadUtil
	{
		private DataTable _rulesLoadTable = null;
		private DataTable _rightRolesLoadTable = null;
		private RightRoleRuleFactory _ruleFactory = null;
		private RightRoleFactory _rightRoleFactory = null;
		string _uieIdFilter = null;

		public RightRoleRulesLoadUtil(DataTable rulesTable, DataTable rightRolesTable, string uieIdFilter)
		{
			if( string.IsNullOrEmpty( uieIdFilter ) )
			{
				throw new ArgumentException( "uieIdFilter cannot be Null or Empty" );
			}

			_rulesLoadTable = rulesTable;
			_rightRolesLoadTable = rightRolesTable;
			_uieIdFilter = uieIdFilter;

			_ruleFactory = new RightRoleRuleFactory();
			_rightRoleFactory = new RightRoleFactory();
		}

		public void LoadRules(ref RightRoleRuleCollection rules, LogicRuleType logicRuleType)
		{
			this.RecurseRules( null, rules, logicRuleType );
		}

		private void RecurseRules(string parentId, RightRoleRuleCollection rules, LogicRuleType logicRuleType)
		{
			DataRow[] rows = null;

			string parentFilter = string.IsNullOrEmpty( parentId ) ? "IS NULL" : string.Format( "= '{0}'", parentId );
			rows = _rulesLoadTable.Select(
				string.Format( "VR_PARENT_ID {0} AND VR_RULE_TYPE = '{1}' AND SPLX_UI_ELEMENT_ID = '{2}'",
				parentFilter, logicRuleType, _uieIdFilter ) );

			foreach( DataRow r in rows )
			{
				RightRoleRule rrr = _ruleFactory.CreateObject( r );

				rrr = rules.AddOrSynchronize( rrr ) as RightRoleRule;

				string filter =
					string.Format( "SPLX_UI_ELEMENT_RULE_ID = '{0}' AND RR_ROLE_TYPE = '{1}'",
					r["SPLX_VALIDATION_RULE_ID"].ToString(), RightRoleType.Success );
				rrr.RightRoles.LoadSuplexObjectTable( _rightRolesLoadTable, _rightRoleFactory,
					filter, null );

				filter =
					string.Format( "SPLX_UI_ELEMENT_RULE_ID = '{0}' AND RR_ROLE_TYPE = '{1}'",
					r["SPLX_VALIDATION_RULE_ID"].ToString(), RightRoleType.Else );
				rrr.ElseRoles.LoadSuplexObjectTable( _rightRolesLoadTable, _rightRoleFactory,
					filter, null );

				this.RecurseRules( r["SPLX_VALIDATION_RULE_ID"].ToString(), rrr.RightRoleRules, LogicRuleType.RightRoleIf );
				this.RecurseRules( r["SPLX_VALIDATION_RULE_ID"].ToString(), rrr.ElseRules, LogicRuleType.RightRoleElse );

				rrr.IsDirty = false;
			}
		}
	}

}