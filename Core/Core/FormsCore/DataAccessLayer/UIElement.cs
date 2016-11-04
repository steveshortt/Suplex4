using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using sg = Suplex.General;
using ss = Suplex.Security;

namespace Suplex.Forms.ObjectModel.Api
{
	public partial class SuplexDataAccessLayer
	{
        #region get
        public List<UIElement> GetUIElementList()
        {
            List<UIElement> uielements = new List<Api.UIElement>();
            DataSet ds = _da.GetDataSet( "splx.splx_api_sel_uielementbyparent_composite",
                new sSortedList( "@UIE_PARENT_ID", Convert.DBNull ) );
            _da.NameTablesFromCompositeSelect( ref ds );

            UIElementFactory uieFactory = new UIElementFactory();
            uielements.LoadSuplexObjectTable( ds.Tables["UIElements"], uieFactory, null, null );

            return uielements;
        }

        public UIElement GetUIElementByIdShallow(string id)
		{
			DataSet ds = _da.GetDataSet( "splx.splx_api_sel_uielementbyid_composite",
				new sSortedList( "@SPLX_UI_ELEMENT_ID", id ) );
			_da.NameTablesFromCompositeSelect( ref ds );

			UIElementFactory factory = new UIElementFactory();
			UIElement uie = factory.CreateObject( ds.Tables["UIElements"].Rows[0] );
			factory.PopulateSecurityDescriptor( ref uie, ds );

			return uie;
		}

		public UIElement GetUIElementByIdDeep(string id)
		{
			DataSet ds = _da.GetDataSet( "splx.splx_api_sel_uie_withchildren_composite",
				new sSortedList( "@SPLX_UI_ELEMENT_ID", id ) );
			_da.NameTablesFromCompositeSelect( ref ds );

			if( ds.Tables["UIElements"].Rows.Count > 0 )
			{
				//load this node and its immediate children (VRs and FMs)
				string uieIdFilter =
					string.Format( "SPLX_UI_ELEMENT_ID = '{0}'", id );
				DataRow[] rows = ds.Tables["UIElements"].Select( uieIdFilter );

				UIElementFactory factory = new UIElementFactory();
				UIElement uie = factory.CreateObject( rows[0] );
				factory.PopulateSecurityDescriptor( ref uie, ds );

				ValidationRulesLoadUtil vrLoader = new ValidationRulesLoadUtil(
					ds.Tables["ValidationRules"], ds.Tables["FillMaps"], ds.Tables["DataBindings"], id );
				ValidationRuleCollection rules = uie.ValidationRules;
				vrLoader.LoadRules( ref rules, LogicRuleType.ValidationIf );

				FillMapFactory fillMapFactory = new FillMapFactory();
				fillMapFactory.DataBindingsTable = ds.Tables["DataBindings"];
				string filter =
					string.Format( "SPLX_UIE_VR_PARENT_ID = '{0}' AND FME_IF_CLAUSE = '{1}'", id, true );
				uie.FillMaps.LoadSuplexObjectTable( ds.Tables["FillMaps"], fillMapFactory, filter, null );

				//recurse for child UIEs and thier children
				this.LoadUIElementFromDataSetDeep( ds, id, uie.UIElements, factory );

				return uie;
			}
			else
			{
				throw new RowNotInTableException( string.Format( "Unable to fetch UIElement '{0}' from the data store.", id ) );
			}
		}

		private void LoadUIElementFromDataSetDeep(DataSet ds, string parentId, UIElementCollection parentCollection, UIElementFactory uieFactory)
		{
			string uieIdFilter =
				string.IsNullOrEmpty( parentId ) ? "UIE_PARENT_ID IS NULL" :
				string.Format( "UIE_PARENT_ID = '{0}'", parentId );
			DataRow[] rows = ds.Tables["UIElements"].Select( uieIdFilter );

			foreach( DataRow r in rows )
			{
				UIElement uie = uieFactory.CreateObject( r );
				uieFactory.PopulateSecurityDescriptor( ref uie, ds );

				uie = parentCollection.AddOrSynchronize( uie ) as UIElement;

				if( ds.Tables.Contains( "ValidationRules" ) )
				{
					ValidationRulesLoadUtil vrLoader = new ValidationRulesLoadUtil(
						ds.Tables["ValidationRules"], ds.Tables["FillMaps"], ds.Tables["DataBindings"], uie.Id.ToString() );
					ValidationRuleCollection rules = uie.ValidationRules;
					vrLoader.LoadRules( ref rules, LogicRuleType.ValidationIf );

					FillMapFactory fillMapFactory = new FillMapFactory();
					fillMapFactory.DataBindingsTable = ds.Tables["DataBindings"];
					string filter =
						string.Format( "SPLX_UIE_VR_PARENT_ID = '{0}' AND FME_IF_CLAUSE = '{1}'", uie.Id.ToString(), true );
					uie.FillMaps.LoadSuplexObjectTable( ds.Tables["FillMaps"], fillMapFactory, filter, null );
				}

				this.LoadUIElementFromDataSetDeep( ds, uie.ObjectId, uie.UIElements, uieFactory );
			}
		}
		#endregion

		#region Upsert
		public UIElement UpsertUIElement(UIElement uie)
		{
			SortedList inparms = this.GetUIElementParms( uie );

			SqlParameter id = new SqlParameter( "@SPLX_UI_ELEMENT_ID", SqlDbType.UniqueIdentifier );
			id.Value = uie.Id;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_UI_ELEMENT_ID", id );

			_da.OpenConnection();
			SqlTransaction tr = _da.Connection.BeginTransaction();
			try
			{
				_da.ExecuteSP( "splx.splx_api_upsert_uie", inparms, ref outparms, false, tr );
				uie.Id = (Guid)id.Value;


				if( uie.SecurityDescriptor.HaveDeleteIds )
				{
					string daclIds = sg.MiscUtils.Join<long>( ",", uie.SecurityDescriptor.DeleteDaclIds );
					string saclIds = sg.MiscUtils.Join<long>( ",", uie.SecurityDescriptor.DeleteSaclIds );
					string rrIds = sg.MiscUtils.Join<long>( ",", uie.SecurityDescriptor.DeleteRightRoleIds );
					string rrrIds = sg.MiscUtils.Join<Guid>( ",", uie.SecurityDescriptor.DeleteRightRoleRuleIds );

					SortedList sd = new sSortedList( "@SPLX_ACE_ID_LIST", string.Format( "{0}, {1}", daclIds, saclIds ),
						"@SPLX_RIGHT_ROLE_ID_LIST", rrIds, "@SPLX_VALIDATION_RULE_ID_LIST",
						string.IsNullOrEmpty( rrrIds ) ? Convert.DBNull : rrrIds );

					_da.ExecuteSP( "splx.splx_api_del_uie_sd", sd, false, tr );

					uie.SecurityDescriptor.DeleteDaclIds.Clear();
					uie.SecurityDescriptor.DeleteSaclIds.Clear();
					uie.SecurityDescriptor.DeleteRightRoleIds.Clear();
					uie.SecurityDescriptor.DeleteRightRoleRuleIds.Clear();
				}


				foreach( AccessControlEntryBase ace in uie.SecurityDescriptor.Dacl )
				{
					if( ace.IsDirty )
					{
						this.UpsertAce( ace, uie.Id, ref tr );
						ace.IsDirty = false;
					}
				}

				foreach( AccessControlEntryAuditBase ace in uie.SecurityDescriptor.Sacl )
				{
					if( ace.IsDirty )
					{
						this.UpsertAce( ace, uie.Id, ref tr );
						ace.IsDirty = false;
					}
				}

				foreach( RightRole rr in uie.SecurityDescriptor.RightRoles )
				{
					if( rr.IsDirty )
					{
						this.UpsertRightRole( rr, uie.Id, ref tr );
						rr.IsDirty = false;
					}
				}

				this.RecurseRightRoleRulesForUIElementUpsert( uie.Id, Guid.Empty, uie.SecurityDescriptor.RightRoleRules, ref tr );


				tr.Commit();

				uie.IsDirty = false;
			}
			catch( Exception ex )
			{
				tr.Rollback();
				throw ex;
			}
			finally
			{
				_da.CloseConnection();
			}

			return uie;
		}

		private SortedList GetUIElementParms(UIElement uie)
		{
			sSortedList s = new sSortedList( "@UIE_NAME", uie.Name );
			s.Add( "@UIE_CONTROL_TYPE", uie.ControlType );
			s.Add( "@UIE_DESC", uie.Description );
			s.Add( "@UIE_DESC_TOOLTIP", uie.UseDescriptionAsTooltip );
			s.Add( "@UIE_UNIQUE_NAME", uie.UniqueName );
			s.Add( "@UIE_DATA_TYPE", uie.DataType.ToString() );
			s.Add( "@UIE_DATA_TYPE_ERR_MSG", uie.DataTypeErrorMessage );
			s.Add( "@UIE_FORMAT_STRING", uie.FormatString );
			s.Add( "@UIE_ALLOW_UNDECLARED", uie.AllowUndeclared );
			s.Add( "@UIE_PARENT_ID", uie.ParentId == Guid.Empty ? Convert.DBNull : uie.ParentId );
			s.Add( "@UIE_DACL_INHERIT", uie.SecurityDescriptor.DaclInherit );
			s.Add( "@UIE_SACL_INHERIT", uie.SecurityDescriptor.SaclInherit );
			s.Add( "@UIE_SACL_AUDIT_TYPE_FILTER", uie.SecurityDescriptor.SaclAuditTypeFilter );

			return s;
		}

		private void RecurseRightRoleRulesForUIElementUpsert(Guid parentUIElementId, Guid parentRuleId, RightRoleRuleCollection rules, ref SqlTransaction tr)
		{
			foreach( RightRoleRule rule in rules )
			{
				if( rule.IsDirty )
				{
					this.UpsertRightRoleRule( rule, parentUIElementId, parentRuleId, ref tr );
					rule.IsDirty = false;
				}

				foreach( RightRole role in rule.RightRoles )
				{
					if( role.IsDirty )
					{
						this.UpsertRightRole( role, rule.Id, ref tr );
						role.IsDirty = false;
					}
				}

				foreach( RightRole role in rule.ElseRoles )
				{
					if( role.IsDirty )
					{
						this.UpsertRightRole( role, rule.Id, ref tr );
						role.IsDirty = false;
					}
				}

				this.RecurseRightRoleRulesForUIElementUpsert( parentUIElementId, rule.Id, rule.RightRoleRules, ref tr );
				this.RecurseRightRoleRulesForUIElementUpsert( parentUIElementId, rule.Id, rule.ElseRules, ref tr );
			}
		}

		public void UpsertUIElementForImport(UIElement uie, ref SqlTransaction tr, bool includeSecurity)
		{
			SortedList inparms = this.GetUIElementParms( uie );

			SqlParameter id = new SqlParameter( "@SPLX_UI_ELEMENT_ID", SqlDbType.UniqueIdentifier );
			id.Value = uie.Id;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_UI_ELEMENT_ID", id );

			_da.ExecuteSP( "splx.splx_api_upsert_uie", inparms, ref outparms, false, tr );
			uie.Id = (Guid)id.Value;


			if( includeSecurity )
			{
				foreach( AccessControlEntryBase ace in uie.SecurityDescriptor.Dacl )
				{
					this.UpsertAce( ace, uie.Id, ref tr );
				}

				foreach( AccessControlEntryAuditBase ace in uie.SecurityDescriptor.Sacl )
				{
					this.UpsertAce( ace, uie.Id, ref tr );
				}

				foreach( RightRole rr in uie.SecurityDescriptor.RightRoles )
				{
					this.UpsertRightRole( rr, uie.Id, ref tr );
				}

				this.RecurseRightRoleRulesForUIElementUpsertForImport( uie.Id, Guid.Empty, uie.SecurityDescriptor.RightRoleRules, ref tr );
			}


			foreach( ValidationRule rule in uie.ValidationRules )
			{
				this.UpsertValidationRuleForImport( rule, ref tr, uie.Id, Guid.Empty );
			}

			foreach( FillMap map in uie.FillMaps )
			{
				this.UpsertFillMapForImport( map, ref tr );
			}

			foreach( UIElement childUie in uie.UIElements )
			{
				this.UpsertUIElementForImport( childUie, ref tr, includeSecurity );
			}
		}

		private void RecurseRightRoleRulesForUIElementUpsertForImport(Guid parentUIElementId, Guid parentRuleId, RightRoleRuleCollection rules, ref SqlTransaction tr)
		{
			foreach( RightRoleRule rule in rules )
			{
				this.UpsertRightRoleRule( rule, parentUIElementId, parentRuleId, ref tr );

				foreach( RightRole role in rule.RightRoles )
				{
					this.UpsertRightRole( role, rule.Id, ref tr );
				}

				foreach( RightRole role in rule.ElseRoles )
				{
					this.UpsertRightRole( role, rule.Id, ref tr );
				}

				this.RecurseRightRoleRulesForUIElementUpsertForImport( parentUIElementId, rule.Id, rule.RightRoleRules, ref tr );
				this.RecurseRightRoleRulesForUIElementUpsertForImport( parentUIElementId, rule.Id, rule.ElseRules, ref tr );
			}
		}
		#endregion

		#region delete
		public void DeleteUIElementById(string id)
		{
			SortedList inparms = new sSortedList( "@SPLX_UI_ELEMENT_ID", id );
			_da.ExecuteSP( "splx.splx_api_del_uie", inparms );
		}
		#endregion
	}

	public class UIElementFactory : ISuplexObjectFactory<UIElement>
	{
		public ISuplexObject CreateSuplexObjectBase(DataRow r)
		{
			return this.CreateObject( r );
		}

		public UIElement CreateObject(DataRow r)
		{
			UIElement uie = new UIElement();
			this.CreateObject( r, ref uie );
			return uie;
		}

		public void CreateObject(DataRow r, ref UIElement uie)
		{
			uie.Id = new Guid( r["SPLX_UI_ELEMENT_ID"].ToString() );
			uie.Name = (string)r["UIE_NAME"];
			uie.ControlType = (string)r["UIE_CONTROL_TYPE"];
			uie.Description = r["UIE_DESC"].ToString();
			uie.UseDescriptionAsTooltip = (bool)r["UIE_DESC_TOOLTIP"];
			uie.UniqueName = r["UIE_UNIQUE_NAME"].ToString();
			uie.AllowUndeclared = (bool)r["UIE_ALLOW_UNDECLARED"];
			uie.DataType = r["UIE_DATA_TYPE"] == Convert.DBNull ?
				TypeCode.String : sg.MiscUtils.ParseEnum<TypeCode>( r["UIE_DATA_TYPE"].ToString() );
			uie.DataTypeErrorMessage = r["UIE_DATA_TYPE_ERR_MSG"].ToString();
			uie.FormatString = r["UIE_FORMAT_STRING"].ToString();
			uie.ParentId = r["UIE_PARENT_ID"] == Convert.DBNull ? Guid.Empty : new Guid( r["UIE_PARENT_ID"].ToString() );

			uie.SecurityDescriptor.DaclInherit = r["UIE_DACL_INHERIT"] == Convert.DBNull ? true : (bool)r["UIE_DACL_INHERIT"];
			uie.SecurityDescriptor.SaclInherit = r["UIE_SACL_INHERIT"] == Convert.DBNull ? true : (bool)r["UIE_SACL_INHERIT"];
			uie.SecurityDescriptor.SaclAuditTypeFilter = r["UIE_SACL_AUDIT_TYPE_FILTER"] == Convert.DBNull ?
				SecurityDescriptor.DefaultSaclAuditTypeFilter :
				sg.MiscUtils.ParseEnum<ss.AuditType>( r["UIE_SACL_AUDIT_TYPE_FILTER"] );

			uie.IsDirty = false;
		}

		public void PopulateSecurityDescriptor(ref UIElement uie, DataSet ds)
		{
			string uieIdFilter = uie.Id.ToString();

			if( ds.Tables.Contains( "Aces" ) )
			{
				AceFactory aceFactory = new AceFactory();
				uie.SecurityDescriptor.Dacl.LoadSuplexObjectTable( ds.Tables["Aces"], aceFactory,
					string.Format( "IS_AUDIT_ACE = 0 AND SPLX_UI_ELEMENT_ID = '{0}'", uieIdFilter ), null );

				AuditAceFactory auditAceFactory = new AuditAceFactory();
				uie.SecurityDescriptor.Sacl.LoadSuplexObjectTable( ds.Tables["Aces"], auditAceFactory,
					string.Format( "IS_AUDIT_ACE = 1 AND SPLX_UI_ELEMENT_ID = '{0}'", uieIdFilter ), null );
			}

			if( ds.Tables.Contains( "UieRightRoles" ) )
			{
				RightRoleFactory rightRoleFactory = new RightRoleFactory();
				uie.SecurityDescriptor.RightRoles.LoadSuplexObjectTable( ds.Tables["UieRightRoles"], rightRoleFactory,
					string.Format( "SPLX_UI_ELEMENT_RULE_ID = '{0}'", uieIdFilter ), null );
			}

			if( ds.Tables.Contains( "RightRoleRules" ) && ds.Tables.Contains( "RrrRightRoles" ) )
			{
				RightRoleRuleCollection rules = uie.SecurityDescriptor.RightRoleRules;
				RightRoleRulesLoadUtil rrrUtil =
					new RightRoleRulesLoadUtil( ds.Tables["RightRoleRules"], ds.Tables["RrrRightRoles"], uieIdFilter );
				rrrUtil.LoadRules( ref rules, LogicRuleType.RightRoleIf );
			}
		}
	}
}