using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

using sg = Suplex.General;

namespace Suplex.Forms.ObjectModel.Api
{
	public class SerializationUtility
	{
		private DataSet _validationDs = null;
		private DataSet _securityDs = null;

		public SerializationUtility()
		{ }

		#region Validation
		public DataSet DeserializeValidationToDataSet(string filePath)
		{
			_validationDs = new DataSet();
			_validationDs.ReadXmlSchema( new StringReader( Properties.Resources.ValidationSchema ) );

			if( this.IsDataSetFile( filePath ) )
			{
				_validationDs.ReadXml( filePath, XmlReadMode.IgnoreSchema );
			}
			else
			{
				SuplexStore splx = SuplexApiClient.LoadSuplexFile( filePath );
				this.RecurseUIElementsForValidation( Guid.Empty, splx.UIElements );
			}

			return _validationDs;
		}

		public DataSet DeserializeValidationToDataSet(TextReader reader)
		{
			_validationDs = new DataSet();
			_validationDs.ReadXmlSchema( new StringReader( Properties.Resources.ValidationSchema ) );

			if( this.IsDataSetFile( ref reader ) )
			{
				_validationDs.ReadXml( reader, XmlReadMode.IgnoreSchema );
			}
			else
			{
				SuplexStore splx = sg.XmlUtils.Deserialize<SuplexStore>( reader );
				this.RecurseUIElementsForValidation( Guid.Empty, splx.UIElements );
			}

			return _validationDs;
		}

		public DataSet DeserializeValidationToDataSet(SuplexStore splx)
		{
			_validationDs = new DataSet();
			_validationDs.ReadXmlSchema( new StringReader( Properties.Resources.ValidationSchema ) );

			this.RecurseUIElementsForValidation( Guid.Empty, splx.UIElements );

			return _validationDs;
		}

		public SuplexStore DeserializeValidationToCollection(string filePath)
		{
			SuplexStore splx = null;

			if( this.IsDataSetFile( filePath ) )
			{
				_validationDs = new DataSet();
				_validationDs.ReadXmlSchema( new StringReader( Properties.Resources.ValidationSchema ) );
				_validationDs.ReadXml( filePath, XmlReadMode.IgnoreSchema );
				splx.UIElements = new UIElementCollection();
				this.RecurseUIElementsForValidation( splx.UIElements, null );
			}
			else
			{
				splx = sg.XmlUtils.Deserialize<SuplexStore>( filePath );
			}

			return splx;
		}

		public SuplexStore DeserializeValidationToCollection(TextReader reader)
		{
			SuplexStore splx = null;

			if( this.IsDataSetFile( ref reader ) )
			{
				_validationDs = new DataSet();
				_validationDs.ReadXmlSchema( new StringReader( Properties.Resources.ValidationSchema ) );
				_validationDs.ReadXml( reader, XmlReadMode.IgnoreSchema );
				splx.UIElements = new UIElementCollection();
				this.RecurseUIElementsForValidation( splx.UIElements, null );
			}
			else
			{
				splx = sg.XmlUtils.Deserialize<SuplexStore>( reader );
			}

			return splx;
		}

		public bool IsDataSetFile(string filePath)
		{
			bool isDataSetFile = true;
			using( StreamReader sr = File.OpenText( filePath ) )
			{
				string s = sr.ReadLine();
				if( s != null )
				{
					s = sr.ReadLine();
					if( s != null )
					{
						isDataSetFile = !( s.ToLower().StartsWith( "<suplexstore" ) );
					}
				}
			}

			return isDataSetFile;
		}

		//TODO: need a less expensive way to do this:
		//	converting the reader back to a string, and then back to a reader seems dumb
		public bool IsDataSetFile(ref TextReader reader)
		{
			bool isDataSetFile = true;

			string rdr = reader.ReadToEnd();
			using( StringReader sr = new StringReader( rdr ) )
			{
				string s = sr.ReadLine();
				if( s != null )
				{
					s = sr.ReadLine();
					if( s != null )
					{
						isDataSetFile = !(s.ToLower().StartsWith( "<suplexstore" ));
					}
				}
			}

			reader = new StringReader( rdr );
			return isDataSetFile;
		}

		#region Recurse XML to build DataSet
		private void RecurseUIElementsForValidation(Guid uieParentId, UIElementCollection uies)
		{
			foreach( UIElement uie in uies )
			{
				DataRow r = _validationDs.Tables["UIElements"].NewRow();
				r["SPLX_UI_ELEMENT_ID"] = uie.Id.ToString();
				r["UIE_NAME"] = uie.Name;
				r["UIE_CONTROL_TYPE"] = uie.ControlType;
				r["UIE_DESC"] = string.IsNullOrEmpty( uie.Description ) ? Convert.DBNull : uie.Description;
				r["UIE_DESC_TOOLTIP"] = uie.UseDescriptionAsTooltip;
				r["UIE_UNIQUE_NAME"] = uie.UniqueName;
				r["UIE_DATA_TYPE"] = uie.DataType == TypeCode.Empty ? Convert.DBNull : uie.DataType.ToString();
				r["UIE_DATA_TYPE_ERR_MSG"] = string.IsNullOrEmpty( uie.DataTypeErrorMessage ) ? Convert.DBNull : uie.DataTypeErrorMessage;
				r["UIE_FORMAT_STRING"] = string.IsNullOrEmpty( uie.FormatString ) ? Convert.DBNull : uie.FormatString;
				r["UIE_ALLOW_UNDECLARED"] = uie.AllowUndeclared;
				r["UIE_PARENT_ID"] = uieParentId == Guid.Empty ? Convert.DBNull : uieParentId.ToString();
				_validationDs.Tables["UIElements"].Rows.Add( r );


				this.RecurseUIElementsForValidation( uie.Id, uie.UIElements );
				this.RecurseRulesForValidation( uie.Id, Convert.DBNull, uie.ValidationRules, LogicRuleType.ValidationIf );
				this.IterateMaps( uie.Id, uie.FillMaps, true );
			}
		}

		private void RecurseRulesForValidation(Guid uieId, object ruleParentId, ValidationRuleCollection rules, LogicRuleType ruleType)
		{
			foreach( ValidationRule rule in rules )
			{
				DataRow r = _validationDs.Tables["ValidationRules"].NewRow();
				r["SPLX_VALIDATION_RULE_ID"] = rule.Id.ToString();
				r["VR_NAME"] = rule.Name;
				r["VR_EVENT_BINDING"] = rule.EventBinding.ToString();
				r["VR_COMPARE_VALUE1"] = rule.CompareValue1;
				r["VR_EXPRESSION_TYPE1"] = rule.ExpressionType1;
				r["VR_VALUE_TYPE1"] = rule.ValueType1;
				r["VR_COMPARE_VALUE2"] = rule.CompareValue2;
				r["VR_EXPRESSION_TYPE2"] = rule.ExpressionType2;
				r["VR_VALUE_TYPE2"] = rule.ValueType2;
				r["VR_COMPARE_DATA_TYPE"] = rule.CompareDataType;
				r["VR_OPERATOR"] = rule.Operator;
				r["VR_ERROR_MESSAGE"] = rule.ErrorMessage;
				r["VR_ERROR_UIE_UNIQUE_NAME"] = string.IsNullOrEmpty( rule.ErrorControl ) ? Convert.DBNull : rule.ErrorControl;
				r["VR_FAIL_PARENT"] = rule.FailParent;
				r["VR_RULE_TYPE"] = ruleType.ToString();
				r["VR_PARENT_ID"] = ruleParentId;
				r["SPLX_UI_ELEMENT_ID"] = uieId.ToString();
				r["VR_SORT_ORDER"] = rule.SortOrder;
				_validationDs.Tables["ValidationRules"].Rows.Add( r );

				this.RecurseRulesForValidation( uieId, rule.Id, rule.ValidationRules, LogicRuleType.ValidationIf );
				this.IterateMaps( rule.Id, rule.FillMaps, true );
				this.RecurseRulesForValidation( uieId, rule.Id, rule.ElseRules, LogicRuleType.ValidationElse );
				this.IterateMaps( rule.Id, rule.ElseMaps, false );
			}
		}

		private void IterateMaps(Guid parentId, FillMapCollection maps, bool isIfClause)
		{
			foreach( FillMap fm in maps )
			{
				DataRow r = _validationDs.Tables["FillMapExpressions"].NewRow();
				r["SPLX_FILLMAP_EXPRESSION_ID"] = fm.Id;
				r["FME_NAME"] = fm.Name;
				r["FME_EVENT_BINDING"] = fm.EventBinding.ToString();
				r["FME_EXPRESSION"] = fm.ExprElements.Expression;
				r["FME_EXPRESSION_TYPE"] = fm.ExprElements.ExprType.ToString();
				r["FME_IF_CLAUSE"] = isIfClause;
				r["FME_SORT_ORDER"] = fm.SortOrder;
				r["SPLX_UIE_VR_PARENT_ID"] = parentId.ToString();
				_validationDs.Tables["FillMapExpressions"].Rows.Add( r );

				foreach( DataBinding db in fm.DataBindings )
				{
					DataRow dbr = _validationDs.Tables["FillMapDataBindings"].NewRow();
					dbr["SPLX_FILLMAP_DATABINDING_ID"] = db.Id;
					dbr["FMB_UIE_UNIQUE_NAME"] = db.ControlName;
					dbr["FMB_PROPERTY_NAME"] = db.PropertyName;
					dbr["FMB_VALUE"] = db.DataMember;
					dbr["FMB_TYPECAST_VALUE"] = true;
					dbr["FMB_OVERRIDE_VALUE"] = db.OverrideValue;
					dbr["SPLX_FILLMAP_EXPRESSION_ID"] = fm.Id;
					_validationDs.Tables["FillMapDataBindings"].Rows.Add( dbr );
				}
			}
		}
		#endregion

		#region Recurse DataSet to build XML
		private void RecurseUIElementsForValidation(UIElementCollection uies, string parentId)
		{
			string parId = string.IsNullOrEmpty( parentId ) ? "IS NULL" : string.Format( "= '{0}'", parentId );
			DataRow[] elements = _validationDs.Tables["UIElements"].Select( string.Format( "UIE_PARENT_ID {0}", parId ) );

			for( int i = 0; i < elements.Length; i++ )
			{
				UIElement uie = new UIElement();
				uie.Id = (Guid)elements[i]["SPLX_UI_ELEMENT_ID"];
				uie.Name = (string)elements[i]["UIE_NAME"];
				uie.ControlType = (string)elements[i]["UIE_CONTROL_TYPE"];
				uie.Description = (string)elements[i]["UIE_DESC"];
				uie.UseDescriptionAsTooltip = (bool)elements[i]["UIE_DESC_TOOLTIP"];
				uie.UniqueName = (string)elements[i]["UIE_UNIQUE_NAME"];
				uie.DataType = sg.MiscUtils.ParseEnum<TypeCode>( (string)elements[i]["UIE_DATA_TYPE"] );
				uie.DataTypeErrorMessage = (string)elements[i]["UIE_DATA_TYPE_ERR_MSG"];
				uie.FormatString = (string)elements[i]["UIE_FORMAT_STRING"];
				uie.AllowUndeclared = (bool)elements[i]["UIE_ALLOW_UNDECLARED"];
				uie.ParentId = string.IsNullOrEmpty( parentId ) ? Guid.Empty : new Guid( parentId );


				DataRow[] rules = _validationDs.Tables["ValidationRules"].Select( string.Format(
					"VR_PARENT_ID IS NULL AND VR_RULE_TYPE='{0}' AND SPLX_UI_ELEMENT_ID='{1}'",
					LogicRuleType.ValidationIf.ToString(), elements[i]["SPLX_UI_ELEMENT_ID"].ToString() ),
					"VR_SORT_ORDER ASC" );
				this.RecurseRulesForValidation( uie.ValidationRules, rules );


				DataRow[] maps = _validationDs.Tables["FillMapExpressions"].Select( string.Format(
					"FME_IF_CLAUSE = true AND SPLX_UIE_VR_PARENT_ID='{0}'", elements[i]["SPLX_UI_ELEMENT_ID"].ToString() ),
					"FME_SORT_ORDER ASC" );
				for( int n = 0; n < maps.Length; n++ )
				{
					uie.FillMaps.Add( this.BuildFillMap( maps[n] ) );
				}
			}
		}

		private void RecurseRulesForValidation(ValidationRuleCollection ruleColl, DataRow[] rules)
		{
			foreach( DataRow rule in rules )
			{
				ValidationRule vr = new ValidationRule();
				vr.Id = new Guid( rule["SPLX_VALIDATION_RULE_ID"].ToString() );
				vr.Name = rule["VR_NAME"].ToString();
				vr.EventBinding = sg.MiscUtils.ParseEnum<ControlEvents>( rule["VR_EVENT_BINDING"].ToString() );
				vr.SortOrder = (int)rule["VR_SORT_ORDER"];
				vr.CompareValue1 = rule["VR_COMPARE_VALUE1"].ToString();
				vr.ValueType1 = sg.MiscUtils.ParseEnum<ComparisonValueType>( rule["VR_VALUE_TYPE1"].ToString() );
				vr.ExpressionType1 = sg.MiscUtils.ParseEnum<ExpressionType>( rule["VR_EXPRESSION_TYPE1"].ToString() );
				vr.CompareValue2 = rule["VR_COMPARE_VALUE2"].ToString();
				vr.ValueType2 = sg.MiscUtils.ParseEnum<ComparisonValueType>( rule["VR_VALUE_TYPE2"].ToString() );
				vr.ExpressionType2 = sg.MiscUtils.ParseEnum<ExpressionType>( rule["VR_EXPRESSION_TYPE2"].ToString() );
				vr.CompareDataType = sg.MiscUtils.ParseEnum<TypeCode>( rule["VR_COMPARE_DATA_TYPE"].ToString() );
				vr.Operator = sg.MiscUtils.ParseEnum<ComparisonOperator>( rule["VR_OPERATOR"].ToString() );
				vr.FailParent = (bool)rule["VR_FAIL_PARENT"];
				vr.ErrorMessage = rule["VR_ERROR_MESSAGE"].ToString();
				vr.ErrorControl = rule["VR_ERROR_UIE_UNIQUE_NAME"].ToString();
				ruleColl.Add( vr );


				DataRow[] mapex = _validationDs.Tables["FillMapExpressions"].Select( string.Format(
					"FME_IF_CLAUSE = true AND SPLX_UIE_VR_PARENT_ID = '{0}'" + rule["SPLX_VALIDATION_RULE_ID"].ToString() ),
					"FME_SORT_ORDER ASC" );
				for( int n = 0; n < mapex.Length; n++ )
				{
					vr.FillMaps.Add( this.BuildFillMap( mapex[n] ) );
				}

				mapex = _validationDs.Tables["FillMapExpressions"].Select( string.Format(
					"FME_IF_CLAUSE = false AND SPLX_UIE_VR_PARENT_ID = '{0}'" + rule["SPLX_VALIDATION_RULE_ID"].ToString() ),
					"FME_SORT_ORDER ASC" );
				for( int n = 0; n < mapex.Length; n++ )
				{
					vr.ElseMaps.Add( this.BuildFillMap( mapex[n] ) );
				}


				DataRow[] chldrn = _validationDs.Tables["ValidationRules"].Select( string.Format(
					"VR_RULE_TYPE='{0}' AND VR_PARENT_ID = '{1}'", LogicRuleType.ValidationIf.ToString(), rule["SPLX_VALIDATION_RULE_ID"].ToString() ),
					"VR_SORT_ORDER ASC" );
				if( chldrn.Length > 0 )
				{
					this.RecurseRulesForValidation( vr.ValidationRules, chldrn );
				}

				chldrn = _validationDs.Tables["ValidationRules"].Select( string.Format(
					"VR_RULE_TYPE='{0}' AND VR_PARENT_ID = '{1}'", LogicRuleType.ValidationElse.ToString(), rule["SPLX_VALIDATION_RULE_ID"].ToString() ),
					"VR_SORT_ORDER ASC" );
				if( chldrn.Length > 0 )
				{
					this.RecurseRulesForValidation( vr.ElseRules, chldrn );
				}
			}
		}//function

		private FillMap BuildFillMap(DataRow map)
		{
			FillMap fm = new FillMap();

			fm.EventBinding = sg.MiscUtils.ParseEnum<ControlEvents>( map["FME_EVENT_BINDING"].ToString() );
			fm.ExprElements.Expression = map["FME_EXPRESSION"].ToString().Length > 0 ? map["FME_EXPRESSION"].ToString() : null;
			fm.ExprElements.ExprType = sg.MiscUtils.ParseEnum<ExpressionType>( map["FME_EXPRESSION_TYPE"].ToString() );

			DataRow[] bindings = _validationDs.Tables["FillMapDataBindings"].Select(
				string.Format( "SPLX_FILLMAP_EXPRESSION_ID='{0}'", map["SPLX_FILLMAP_EXPRESSION_ID"] ),
				"SPLX_FILLMAP_DATABINDING_ID ASC" );
			for( int n = 0; n < bindings.Length; n++ )
			{
				DataBinding db = new DataBinding();
				db.ControlName = bindings[n]["FMB_UIE_UNIQUE_NAME"].ToString();
				db.PropertyName = bindings[n]["FMB_PROPERTY_NAME"].ToString();
				db.DataMember = bindings[n]["FMB_VALUE"].ToString();
				db.ConversionRequired = bool.Parse( bindings[n]["FMB_TYPECAST_VALUE"].ToString() );
				db.OverrideValue = bool.Parse( bindings[n]["FMB_OVERRIDE_VALUE"].ToString() );

				fm.DataBindings.Add( db );
			}

			return fm;
		}
		#endregion
		#endregion

		#region Security
		public string SerializeSecurityToStringFromDataSet(DataSet ds)
		{
			//if( ds.Tables.Contains( "GROUP_MEMBERSHIP_MASK" ) )
			//{
			//    ds.Tables["GROUP_MEMBERSHIP_MASK"].TableName = "UserInfo";
			//    ds.Tables["UserInfo"].Columns.Add( "SPLX_USER_ID", typeof( string ) );
			//    ds.Tables["UserInfo"].Rows[0]["SPLX_USER_ID"] = SecurityBuilder.GetUserIdFromSecurityDataSetAcesTable( ds );
			//}

			MemoryStream ms = new MemoryStream();
			ds.WriteXml( ms, XmlWriteMode.IgnoreSchema );
			ms.Position = 0;

			return UnicodeEncoding.UTF8.GetString( ms.ToArray() );
		}

		public DataSet DeserializeSecurityToDataSetFromString(string str)
		{
			byte[] bytes = UnicodeEncoding.UTF8.GetBytes( str );
			MemoryStream ms = new MemoryStream( bytes );
			DataSet ds = new DataSet();
			ds.ReadXmlSchema( new StringReader( Properties.Resources.SecuritySchema ) );
			ds.ReadXml( ms );

			return ds;
		}

		public DataSet DeserializeSecurityToDataSet(string filePath)
		{
			SuplexStore splx = SuplexApiClient.LoadSuplexFile( filePath );
			return DeserializeSecurityToDataSet( splx );
		}

		public DataSet DeserializeSecurityToDataSet(TextReader reader)
		{
			SuplexApiClient apiClient = new SuplexApiClient();
			SuplexStore splx = apiClient.LoadFromReader( reader );
			return DeserializeSecurityToDataSet( splx );
		}

		public DataSet DeserializeSecurityToDataSet(SuplexStore splx)
		{
			_securityDs = new DataSet();
			_securityDs.ReadXmlSchema( new StringReader( Properties.Resources.SecuritySchema ) );

			DataTable aceList = _securityDs.Tables["Aces"].Clone();
			aceList.TableName = "AceList";
			_securityDs.Tables.Add( aceList );

			this.RecurseUIElementsForSecurity( Guid.Empty, splx.UIElements );
			this.IterateSecurityPrincipals( splx.SecurityPrincipals );
			this.IterateGroupMembership( splx.GroupMembership );

			return _securityDs;
		}

		#region Recurse XML to build DataSet
		private void RecurseUIElementsForSecurity(Guid uieParentId, UIElementCollection uies)
		{
			foreach( UIElement uie in uies )
			{
				DataRow r = _securityDs.Tables["AclInfo"].NewRow();
				r["SPLX_UI_ELEMENT_ID"] = uie.Id.ToString();
				r["UIE_UNIQUE_NAME"] = uie.UniqueName;
				r["UIE_DACL_INHERIT"] = uie.SecurityDescriptor.DaclInherit;
				r["UIE_SACL_INHERIT"] = uie.SecurityDescriptor.SaclInherit;
				r["UIE_SACL_AUDIT_TYPE_FILTER"] = (int)uie.SecurityDescriptor.SaclAuditTypeFilter;
				r["UIE_PARENT_ID"] = uieParentId == Guid.Empty ? Convert.DBNull : uieParentId.ToString();
				_securityDs.Tables["AclInfo"].Rows.Add( r );


				this.IterateAces( uie );
				this.RecurseUIElementsForSecurity( uie.Id, uie.UIElements );
				this.RecurseRulesForSecurity( uie.Id, Convert.DBNull, uie.SecurityDescriptor.RightRoleRules, LogicRuleType.RightRoleIf );
				this.IterateRoles( uie.Id, uie.UniqueName, uie.SecurityDescriptor.RightRoles );
			}
		}

		int _hackAceId = Int32.MaxValue;
		private void IterateAces(UIElement uie)
		{
			foreach( AccessControlEntryBase ace in uie.SecurityDescriptor.Dacl )
			{
				DataRow r = _securityDs.Tables["AceList"].NewRow();
				r["SPLX_ACE_ID"] = _hackAceId--;	// ace.Id;
				r["SPLX_GROUP_ID"] = ace.SecurityPrincipalId;
				r["ACE_ACCESS_MASK"] = ace.Right;
				r["ACE_ACCESS_TYPE1"] = ace.Allowed;
				r["ACE_ACCESS_TYPE2"] = Convert.DBNull;
				r["IS_AUDIT_ACE"] = false;
				r["ACE_INHERIT"] = ace.Inherit;
				r["ACE_TYPE"] = ace.AceType.ToString();
				r["SPLX_UI_ELEMENT_ID"] = uie.Id;
				r["UIE_UNIQUE_NAME"] = uie.UniqueName;
				_securityDs.Tables["AceList"].Rows.Add( r );
			}

			foreach( AccessControlEntryAuditBase ace in uie.SecurityDescriptor.Sacl )
			{
				DataRow r = _securityDs.Tables["AceList"].NewRow();
				r["SPLX_ACE_ID"] = _hackAceId--;	// ace.Id;
				r["SPLX_GROUP_ID"] = ace.SecurityPrincipalId;
				r["ACE_ACCESS_MASK"] = ace.Right;
				r["ACE_ACCESS_TYPE1"] = ace.Allowed;
				r["ACE_ACCESS_TYPE2"] = ace.Denied;
				r["IS_AUDIT_ACE"] = true;
				r["ACE_INHERIT"] = ace.Inherit;
				r["ACE_TYPE"] = ace.AceType.ToString();
				r["SPLX_UI_ELEMENT_ID"] = uie.Id;
				r["UIE_UNIQUE_NAME"] = uie.UniqueName;
				_securityDs.Tables["AceList"].Rows.Add( r );
			}
		}

		private void RecurseRulesForSecurity(Guid uieId, object ruleParentId, RightRoleRuleCollection rules, LogicRuleType ruleType)
		{
			foreach( RightRoleRule rule in rules )
			{
				DataRow r = _securityDs.Tables["RightRoleRules"].NewRow();
				r["SPLX_VALIDATION_RULE_ID"] = rule.Id.ToString();
				r["VR_NAME"] = rule.Name;
				r["VR_COMPARE_VALUE1"] = rule.CompareValue1;
				r["VR_EXPRESSION_TYPE1"] = rule.ExpressionType1;
				r["VR_VALUE_TYPE1"] = rule.ValueType1;
				r["VR_COMPARE_VALUE2"] = rule.CompareValue2;
				r["VR_EXPRESSION_TYPE2"] = rule.ExpressionType2;
				r["VR_VALUE_TYPE2"] = rule.ValueType2;
				r["VR_COMPARE_DATA_TYPE"] = rule.CompareDataType;
				r["VR_OPERATOR"] = rule.Operator;
				r["VR_ERROR_MESSAGE"] = rule.ErrorMessage;
				r["VR_FAIL_PARENT"] = rule.FailParent;
				r["VR_RULE_TYPE"] = ruleType.ToString();
				r["VR_PARENT_ID"] = ruleParentId;
				r["SPLX_UI_ELEMENT_ID"] = uieId.ToString();
				r["VR_SORT_ORDER"] = rule.SortOrder;
				_securityDs.Tables["RightRoleRules"].Rows.Add( r );

				this.RecurseRulesForSecurity( uieId, rule.Id, rule.RightRoleRules, LogicRuleType.RightRoleIf );
				this.IterateRoles( rule.Id, string.Empty, rule.RightRoles );
				this.RecurseRulesForSecurity( uieId, rule.Id, rule.ElseRules, LogicRuleType.RightRoleElse );
				this.IterateRoles( rule.Id, string.Empty, rule.ElseRoles );
			}
		}

		private void IterateRoles(Guid parentId, string uieUniqueName, RightRoleCollection roles)
		{
			foreach( RightRole role in roles )
			{
				DataRow r = _securityDs.Tables["RightRoles"].NewRow();
				r["UIE_UNIQUE_NAME"] = uieUniqueName;
				r["SPLX_RIGHT_ROLE_ID"] = _hackAceId--;	// role.Id;
				r["RR_ACE_TYPE"] = role.AceType.ToString();
				r["RR_RIGHT_NAME"] = role.RightName;
				r["RR_UIE_UNIQUE_NAME"] = role.ControlUniqueName;
				r["RR_UI_RIGHT"] = role.UIRight.ToString();
				r["RR_ROLE_TYPE"] = role.RightRoleType.ToString();
				r["SPLX_UI_ELEMENT_RULE_ID"] = parentId.ToString();
				_securityDs.Tables["RightRoles"].Rows.Add( r );
			}
		}

		private void IterateSecurityPrincipals(ObservableCollection<SecurityPrincipalBase> securityPrincipals)
		{
			foreach( SecurityPrincipalBase sp in securityPrincipals )
			{
				DataRow r = _securityDs.Tables["SecurityPrincipals"].NewRow();
				r["PRINCIPAL_ID"] = sp.Id.ToString();
				r["PRINCIPAL_NAME"] = sp.Name;
				r["PRINCIPAL_IS_ENABLED"] = sp.IsEnabled;
				r["PRINCIPAL_IS_USER"] = sp.IsUserObject;
				_securityDs.Tables["SecurityPrincipals"].Rows.Add( r );
			}
		}

		private void IterateGroupMembership(GroupMembershipCollection groupMembership)
		{
			foreach( GroupMembershipItem gm in groupMembership.InnerList.Values )
			{
				if( gm.Group.IsEnabled && gm.Member.IsEnabled )
				{
					DataRow r = _securityDs.Tables["GroupMembership"].NewRow();
					r["SPLX_GROUP_ID"] = gm.GroupId.ToString();
					r["GROUP_NAME"] = gm.Group.Name;
					r["MEMBER_ID"] = gm.MemberId.ToString();
					r["MEMBER_IS_USER"] = gm.MemberType == ObjectType.User;
					_securityDs.Tables["GroupMembership"].Rows.Add( r );
				}
			}
		}
		#endregion

		#endregion
	}
}