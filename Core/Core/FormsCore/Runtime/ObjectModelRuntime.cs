using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;


namespace Suplex.Forms.ObjectModel.Runtime
{
	[DataContract]
	public class SecurityCache
	{
		public SecurityCache()
		{
			this.Aces = new List<AceData>();
			this.AceList = new List<AceData>();
			this.AclInfo = new List<AclInfoData>();
			this.RightRoles = new List<RightRoleData>();
			this.RightRoleRules = new List<RightRoleRuleData>();
			this.GroupMembership = new List<GroupMembershipData>();
		}

		[DataMember]
		public object group_membership_mask { get; internal set; }
		[DataMember]
		public List<AceData> Aces { get; internal set; }
		[DataMember]
		public List<AceData> AceList { get; internal set; }
		[DataMember]
		public List<AclInfoData> AclInfo { get; internal set; }
		[DataMember]
		public List<RightRoleData> RightRoles { get; internal set; }
		[DataMember]
		public List<RightRoleRuleData> RightRoleRules { get; internal set; }
		[DataMember]
		public List<GroupMembershipData> GroupMembership { get; internal set; }
	}

	[DataContract]
	public class AceData
	{
		[DataMember]
		public string uie_unique_name { get; internal set; }
		[DataMember]
		public string splx_user_id { get; internal set; }
		[DataMember]
		public int splx_ace_id { get; internal set; }
		[DataMember]
		public int ace_access_mask { get; internal set; }
		[DataMember]
		public bool ace_access_type1 { get; internal set; }
		[DataMember]
		public bool ace_access_type2 { get; internal set; }
		[DataMember]
		public bool is_audit_ace { get; internal set; }
		[DataMember]
		public bool ace_inherit { get; internal set; }
		[DataMember]
		public string ace_type { get; internal set; }
		[DataMember]
		public Guid splx_ui_element_id { get; internal set; }
	}

	[DataContract]
	public class AclInfoData
	{
		[DataMember]
		public string uie_unique_name { get; internal set; }
		[DataMember]
		public bool uie_dacl_inherit { get; internal set; }
		[DataMember]
		public bool uie_sacl_inherit { get; internal set; }
		[DataMember]
		public int uie_sacl_audit_type_filter { get; internal set; }
		[DataMember]
		public Guid splx_ui_element_id { get; internal set; }
		[DataMember]
		public Guid uie_parent_id { get; internal set; }
	}

	[DataContract]
	public class RightRoleData
	{
		[DataMember]
		public int splx_right_role_id { get; internal set; }
		[DataMember]
		public string rr_ace_type { get; internal set; }
		[DataMember]
		public string rr_right_name { get; internal set; }
		[DataMember]
		public string rr_uie_unique_name { get; internal set; }
		[DataMember]
		public string rr_ui_right { get; internal set; }
		[DataMember]
		public Guid splx_ui_element_rule_id { get; internal set; }
		[DataMember]
		public string rr_role_type { get; internal set; }
	}

	[DataContract]
	public class RightRoleRuleData
	{
		[DataMember]
		public Guid splx_vaildation_rule_id { get; internal set; }
		[DataMember]
		public string vr_name { get; internal set; }
		[DataMember]
		public string vr_event_binding { get; internal set; }
		[DataMember]
		public string vr_compare_value1 { get; internal set; }
		[DataMember]
		public string vr_expression_type1 { get; internal set; }
		[DataMember]
		public string vr_value_type1 { get; internal set; }
		[DataMember]
		public string vr_compare_value2 { get; internal set; }
		[DataMember]
		public string vr_expression_type2 { get; internal set; }
		[DataMember]
		public string vr_value_type2 { get; internal set; }
		[DataMember]
		public string vr_compare_data_type { get; internal set; }
		[DataMember]
		public string vr_operator { get; internal set; }
		[DataMember]
		public string vr_error_message { get; internal set; }
		[DataMember]
		public string vr_error_uie_unique_name { get; internal set; }
		[DataMember]
		public bool vr_fail_parent { get; internal set; }
		[DataMember]
		public string vr_rule_type { get; internal set; }
		[DataMember]
		public Guid vr_parent_id { get; internal set; }
		[DataMember]
		public Guid splx_ui_element_id { get; internal set; }
		[DataMember]
		public int vr_sort_order { get; internal set; }
	}

	[DataContract]
	public class GroupMembershipData
	{
		[DataMember]
		public Guid splx_group_id { get; internal set; }
		[DataMember]
		public string group_name { get; internal set; }
		[DataMember]
		public Guid member_id { get; internal set; }
		[DataMember]
		public bool member_is_user { get; internal set; }
	}

	public class SecurityCacheUtil
	{
		public SecurityCache Load(DataSet ds)
		{
			SecurityCache cache = new SecurityCache();

			#region Aces
			foreach( DataRow r in ds.Tables["Aces"].Rows )
			{
				AceData ace = new AceData();

				ace.uie_unique_name = r["uie_unique_name"].ToString();
				ace.splx_user_id = r["splx_user_id"].ToString();
				ace.splx_ace_id = (int)r["splx_ace_id"];
				ace.ace_access_mask = (int)r["ace_access_mask"];
				ace.ace_access_type1 = (bool)r["ace_access_type1"];
				ace.ace_access_type2 = (bool)r["ace_access_type2"];
				ace.is_audit_ace = (bool)r["is_audit_ace"];
				ace.ace_inherit = (bool)r["ace_inherit"];
				ace.ace_type = r["ace_type"].ToString();
				ace.splx_ui_element_id = (Guid)r["splx_ui_element_id"];

				cache.Aces.Add( ace );
			}
			#endregion

			#region AclInfo
			foreach( DataRow r in ds.Tables["AclInfo"].Rows )
			{
				AclInfoData acl = new AclInfoData();

				acl.uie_unique_name = r["uie_unique_name"].ToString();
				acl.uie_dacl_inherit = (bool)r["uie_dacl_inherit"];
				acl.uie_sacl_inherit = (bool)r["uie_sacl_inherit"];
				acl.uie_sacl_audit_type_filter = r["uie_sacl_audit_type_filter"] == Convert.DBNull ? 0 : (int)r["uie_sacl_audit_type_filter"];
				acl.splx_ui_element_id = (Guid)r["splx_ui_element_id"];
				acl.uie_parent_id = r["uie_parent_id"] == Convert.DBNull ? Guid.Empty : (Guid)r["uie_parent_id"];

				cache.AclInfo.Add( acl );
			}
			#endregion

			#region RightRoles
			foreach( DataRow r in ds.Tables["RightRoles"].Rows )
			{
				RightRoleData role = new RightRoleData();

				role.splx_right_role_id = (int)r["splx_right_role_id"];
				role.rr_ace_type = r["rr_ace_type"].ToString();
				role.rr_right_name = r["rr_right_name"].ToString();
				role.rr_uie_unique_name = r["rr_uie_unique_name"].ToString();
				role.rr_ui_right = r["rr_ui_right"].ToString();
				role.splx_ui_element_rule_id = (Guid)r["splx_ui_element_rule_id"];
				role.rr_role_type = r["rr_role_type"].ToString();

				cache.RightRoles.Add( role );
			}
			#endregion

			#region RightRoleRules
			foreach( DataRow r in ds.Tables["RightRoleRules"].Rows )
			{
				RightRoleRuleData rule = new RightRoleRuleData();

				rule.splx_vaildation_rule_id = (Guid)r["splx_vaildation_rule_id"];
				rule.vr_name = r["vr_name"].ToString();
				rule.vr_event_binding = r["vr_event_binding"].ToString();
				rule.vr_compare_value1 = r["vr_compare_value1"].ToString();
				rule.vr_expression_type1 = r["vr_expression_type1"].ToString();
				rule.vr_value_type1 = r["vr_value_type1"].ToString();
				rule.vr_compare_value2 = r["vr_compare_value2"].ToString();
				rule.vr_expression_type2 = r["vr_expression_type2"].ToString();
				rule.vr_value_type2 = r["vr_value_type2"].ToString();
				rule.vr_compare_data_type = r["vr_compare_data_type"].ToString();
				rule.vr_operator = r["vr_operator"].ToString();
				rule.vr_error_message = r["vr_error_message"].ToString();
				rule.vr_error_uie_unique_name = r["vr_error_uie_unique_name"].ToString();
				rule.vr_fail_parent = (bool)r["vr_fail_parent"];
				rule.vr_rule_type = r["vr_rule_type"].ToString();
				rule.vr_parent_id = r["vr_parent_id"] == Convert.DBNull ? Guid.Empty : (Guid)r["vr_parent_id"];
				rule.splx_ui_element_id = (Guid)r["splx_ui_element_id"];
				rule.vr_sort_order = (int)r["vr_sort_order"];

				cache.RightRoleRules.Add( rule );
			}
			#endregion

			return cache;
		}
	}
}