using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Reflection;
using System.DirectoryServices;
using System.Security.Principal;

using dbg = System.Diagnostics.Debug;

using Suplex.Data;
using Suplex.Security;
using Suplex.Security.Standard;
using ss = Suplex.Security.Standard;
using Suplex.General;
using api = Suplex.Forms.ObjectModel.Api;

using WinForms = System.Windows.Forms;
using WebForms = System.Web.UI;


namespace Suplex.Forms
{
	public class SecurityLoadParameters
	{
		public SecurityLoadParameters()
		{
			this.PropagateAuditWriter = true;
			this.ApplyDefaultSecurity = true;
			this.AllowImpersonation = true;
		}

		public ss.User User { get; set; }
		public ExternalGroupInfo ExternalGroupInfo { get; set; }
		public AuditEventHandler DefaultAuditEventHandler { get; set; }
		public bool PropagateAuditWriter { get; set; }
		public bool AllowImpersonation { get; set; }
		public bool ApplyDefaultSecurity { get; set; }

		internal bool Validate()
		{
			return this.User != null;
		}
	}

	public class SecurityBuilder
	{
		private Assembly _secasm = null;

		private DataSet _dsCache = null;
		private ExternalGroupInfo _egi = ExternalGroupInfo.Empty;

		//private DataSet _ds = new DataSet();

		private DataAccessLayer _dal = null;
		private DataAccessor _da = null;
		private Suplex.Security.Standard.User _user = null;

		private ISecureControl _topLsControl = null;
		private ISecureControl _topRrControl = null;
		private ISecureControl _topRrrControl = null;
		private ControlType _ctrlType = ControlType.Unknown;

		//private bool _isWindows							= true;

		private AuditEventHandler _auditEventHandler = null;
		private bool _propagateAuditWriter;
		private AuditWriter _auditWriter;

		private ControlCache _rrNameCache = null;
		private ControlCache _rrNonPropagating = null;

		private int _aclKey = 0;

		private EnumUtil _enumUtil = new EnumUtil();


		public SecurityBuilder() { }


		private void CreateSecurityAssembly()
		{
			if( _secasm == null )
			{
				_secasm = Assembly.GetExecutingAssembly();	//Assembly.Load( "Suplex.Security" );
			}
		}


		#region Load
		//public static string GetUserIdFromSecurityDataSetAcesTable(DataSet ds)
		//{
		//    string userId = Guid.Empty.ToString();
		//    if( ds.Tables.Contains( "Aces" ) && ds.Tables["Aces"].Rows.Count > 0 )
		//    {
		//        userId = ds.Tables["Aces"].Rows[0]["SPLX_USER_ID"].ToString();
		//    }
		//    return userId;
		//}

		public static void GetUserInfoFromSecurityDataSet(DataSet ds, out string userId, out byte[] rlsMask)
		{
			userId = Guid.Empty.ToString();
			rlsMask = new byte[] { };

			if( ds.Tables.Contains( "UserInfo" ) && ds.Tables["UserInfo"].Rows.Count > 0 )
			{
				userId = ds.Tables["UserInfo"].Rows[0]["SPLX_USER_ID"].ToString();
				rlsMask = (byte[])ds.Tables["UserInfo"].Rows[0]["GROUP_MEMBERSHIP_MASK"];
			}
		}

		#region CreateSecurityCache
		public DataSet CreateSecurityCache(DataAccessor da, string controlUniqueName, ss.User splxUser, ExternalGroupInfo egi)
		{
			if( string.IsNullOrEmpty( splxUser.Id ) )
			{
				splxUser.Id = Guid.Empty.ToString();	//default value
				splxUser.DataAccessor = da;
				splxUser.ResolveByName();
			}

			egi.TryBuildGroupsList( splxUser.Name );

			DataSet ds = da.GetDataSet( "splx.splx_dal_sel_security_byuserbyuie",
				new sSortedList(
				"@UIE_UNIQUE_NAME", controlUniqueName,
				"@SPLX_USER_ID", splxUser.Id,
				"@EXTERNAL_GROUP_LIST", egi.GroupsList ) );

			da.NameTablesFromCompositeSelect( ref ds );

			return ds;
		}

		public DataSet CreateSecurityCache(string filePath, ss.User splxUser)
		{
			if( !string.IsNullOrEmpty( filePath ) )
			{
				api.SerializationUtility ser = new api.SerializationUtility();
				DataSet ds = ser.DeserializeSecurityToDataSet( filePath );

				if( string.IsNullOrEmpty( splxUser.Id ) )
				{
					splxUser.Id = Guid.Empty.ToString();	//default value

					DataRow[] u = ds.Tables["SecurityPrincipals"].Select(
						string.Format( "PRINCIPAL_NAME = '{0}'", splxUser.Name ) );
					if( u.Length > 0 )
					{
						splxUser.Id = u[0]["PRINCIPAL_ID"].ToString();
						splxUser.IsEnabled = (bool)u[0]["PRINCIPAL_IS_ENABLED"];
					}
				}

				return ds;
			}
			else
			{
				return null;
			}
		}

		public DataSet CreateSecurityCache(System.IO.TextReader reader, ss.User splxUser)
		{
			if( reader != null )
			{
				api.SerializationUtility ser = new api.SerializationUtility();
				DataSet ds = ser.DeserializeSecurityToDataSet( reader );

				if( string.IsNullOrEmpty( splxUser.Id ) )
				{
					splxUser.Id = Guid.Empty.ToString();	//default value

					DataRow[] u = ds.Tables["SecurityPrincipals"].Select(
						string.Format( "PRINCIPAL_NAME = '{0}'", splxUser.Name ) );
					if( u.Length > 0 )
					{
						splxUser.Id = u[0]["PRINCIPAL_ID"].ToString();
						splxUser.IsEnabled = (bool)u[0]["PRINCIPAL_IS_ENABLED"];
					}
				}

				return ds;
			}
			else
			{
				return null;
			}
		}

		public DataSet CreateSecurityCache(api.SuplexStore splxStore, ss.User splxUser)
		{
			if( splxStore != null )
			{
				api.SerializationUtility ser = new api.SerializationUtility();
				DataSet ds = ser.DeserializeSecurityToDataSet( splxStore );

				if( string.IsNullOrEmpty( splxUser.Id ) )
				{
					splxUser.Id = Guid.Empty.ToString();	//default value

					DataRow[] u = ds.Tables["SecurityPrincipals"].Select(
						string.Format( "PRINCIPAL_NAME = '{0}'", splxUser.Name ) );
					if( u.Length > 0 )
					{
						splxUser.Id = u[0]["PRINCIPAL_ID"].ToString();
						splxUser.IsEnabled = (bool)u[0]["PRINCIPAL_IS_ENABLED"];
					}
				}

				return ds;
			}
			else
			{
				return null;
			}
		}

		//public rt.SecurityCache CreateSecurityRuntimeCache(DataAccessor da, string controlUniqueName, ss.User splxUser, ExternalGroupInfo egi)
		//{
		//    if( string.IsNullOrEmpty( splxUser.Id ) )
		//    {
		//        splxUser.Id = Guid.Empty.ToString();	//default value
		//        splxUser.DataAccessor = da;
		//        splxUser.ResolveByName();
		//    }

		//    egi.TryBuildGroupsList( splxUser.Name );

		//    DataSet ds = da.GetDataSet( "splx.splx_dal_sel_security_byuserbyuie",
		//        new sSortedList(
		//        "@UIE_UNIQUE_NAME", controlUniqueName,
		//        "@SPLX_USER_ID", splxUser.Id,
		//        "@EXTERNAL_GROUP_LIST", egi.GroupsList ) );

		//    da.NameTablesFromCompositeSelect( ref ds );

		//    om.Runtime.SecurityCacheUtil cacheUtil = new om.Runtime.SecurityCacheUtil();
		//    return cacheUtil.Load( ds );
		//}
		#endregion

		public void LoadRulesFromCache(ISecureControl control, SecurityLoadParameters slp,
			DataAccessor da, DataSet securityCache)
		{
			this.CreateSecurityAssembly();

			_dsCache = securityCache;

			_da = da;
			_user = slp.User;
			_auditEventHandler = slp.DefaultAuditEventHandler;
			_propagateAuditWriter = slp.PropagateAuditWriter;
			_auditWriter = control.Security.AuditWriter;

			_egi = slp.ExternalGroupInfo;

			_topLsControl = control;
			_ctrlType = EnumUtil.GetControlType( control );

			_rrNameCache = new ControlCache();

			Random r = new Random( (int)DateTime.Now.Ticks );
			_aclKey = r.Next();

			if( string.IsNullOrEmpty( _egi.GroupsList ) )
			{
				_egi.TryBuildGroupsList( _user.Name );
			}

			bool ok = true;
			string userId = _user.Id;
			string userName = _user.Name;
			if( slp.AllowImpersonation && _user.ImpersonationContext != null )
			{
				//string error = string.Empty;	( ref error )
				if( ImpersonationIsValid( _egi.GroupsList ) )
				{
					userId = slp.User.ImpersonationContext.Id;
					userName = slp.User.ImpersonationContext.Name;
					_egi.TryBuildGroupsList( slp.User.ImpersonationContext.Name );
				}
				else
				{
					ok = false;
					throw new Suplex.Forms.SecurityException( string.Format( "Impersonation access denied for user '{0}'.", _user.Name ) );
				}
			}

			//wi = new WindowsIdentity( userName );


			if( ok )
			{
				if( _dsCache == null && da != null )	//useSecurityCache &&
				{
					_dsCache = this.CreateSecurityCache( da, control.UniqueName, _user, _egi );
					this.BuildNameCache( _dsCache.Tables["RightRoles"].Select() );
				}

				if( _dsCache.Tables.Contains( "AceList" ) &&
					_dsCache.Tables["Aces"].Rows.Count == 0 )
				{
					this.DenormalizeAces( userId );
				}

				this.LoadRulesRecursive( control, userId, true, _egi.AllowAnonymous );

				this.ResolveInternalReferences( control );

				this.ResolveRightRolesPre( control, _enumUtil.GetChildren( control ) );

				string ui = "UserInfo";
				string gmm = "GROUP_MEMBERSHIP_MASK";
				if( securityCache.Tables.Contains( ui ) && securityCache.Tables[ui].Rows.Count > 0 )
				{
					_user.RlsMask = (byte[])securityCache.Tables[ui].Rows[0][gmm];
				}
			}

			_topLsControl = null;
		}
		#endregion

		private void BuildNameCache(DataRow[] roles)
		{
			foreach( DataRow role in roles )
			{
				if( !_rrNameCache.ContainsKey( role["RR_UIE_UNIQUE_NAME"].ToString() ) )
				{
					_rrNameCache.Add( role["RR_UIE_UNIQUE_NAME"].ToString(), null );
				}
			}
		}

		private void LoadRulesRecursive(object control, string userId, bool useSecurityCache, bool allowAnonymous)
		{
			if( control is ISecureControl )
			{
				if( useSecurityCache )
				{
					this.LoadRulesFromCache( (ISecureControl)control, userId, allowAnonymous );
				}
				else
				{
					LoadRulesFromDB( (ISecureControl)control, userId );
				}

				_rrNameCache.AddKnown( (ISecureControl)control );
			}

			IEnumerator children = _enumUtil.GetChildren( control ).GetEnumerator();
			while( children.MoveNext() )
			{
				this.LoadRulesRecursive( children.Current, userId, useSecurityCache, allowAnonymous );
			}
		}

		[Obsolete( "Delete me.", false )]
		private void LoadRulesFromDB(ISecureControl sc, string userId)
		{
			//sc.Security.UserContext = _user;

			//_ds.Clear();
			//SortedList parms = new SortedList();
			//parms.Add( "@sobUniqueName", sc.UniqueName );

			//_ds.Tables.Clear();
			//_da.OpenConnection();

			//_da.GetDataSet( "sc_selAclInfoByUniqueName", parms, _ds, "AclInfo", false );
			//_da.GetDataSet( "sc_selRightRolesByUniqueName", parms, _ds, "RightRoles", false );


			//parms.Add( "@usrPk", userId );
			//_da.GetDataSet( "sc_selAcesByItemByUser", parms, _ds, "Aces", false );

			//_da.CloseConnection();

			//SetAclInfo( sc.Security.Descriptor, _ds.Tables["AclInfo"].Select() );
			//IterateAces( sc.Security.Descriptor, _ds.Tables["Aces"].Select() );
			//DataRow[] rr = _ds.Tables["RightRoles"].Select();
			//IterateRightRoles( sc.Security.RightRoles, rr );
			//BuildNameCache( rr );

			//if( sc != _topLsControl )
			//{
			//    sc.Security.Audited += _auditEventHandler;

			//    if( _propagateAuditWriter )
			//    {
			//        sc.Security.AuditWriter = _auditWriter;
			//    }
			//}
		}

		private void LoadRulesFromCache(ISecureControl sc, string userId, bool allowAnonymous)
		{
			sc.Security.UserContext = _user;

			string uieFilter = string.Format( "UIE_UNIQUE_NAME='{0}'", sc.UniqueName );
			string aceFilter = string.Format( "SPLX_USER_ID='{0}' AND {1}", userId, uieFilter );
			if( allowAnonymous )
			{
				aceFilter = string.Format( "({0}) AND (SPLX_USER_ID IS NULL OR SPLX_USER_ID='{1}')", uieFilter, userId );
			}

			DataRow[] aces = _dsCache.Tables["Aces"].Select( aceFilter );
			DataRow[] aclInfo = _dsCache.Tables["AclInfo"].Select( uieFilter );

			string uieId = aclInfo.Length > 0 ? aclInfo[0]["SPLX_UI_ELEMENT_ID"].ToString() : Guid.Empty.ToString();
			string uieIdFilter =
				string.Format( "VR_PARENT_ID IS NULL AND VR_RULE_TYPE='{0}' AND SPLX_UI_ELEMENT_ID='{1}'",
				LogicRuleType.RightRoleIf, uieId );
			DataRow[] rightRoleRules = _dsCache.Tables["RightRoleRules"].Select( uieIdFilter, "VR_SORT_ORDER ASC" );

			string idFilter = string.Format( "SPLX_UI_ELEMENT_RULE_ID='{0}'", uieId );
			DataRow[] rightRoles = _dsCache.Tables["RightRoles"].Select( idFilter );	//string.Format( "RR_UIE_UNIQUE_NAME='{0}'", sc.UniqueName )

			this.SetAclInfo( sc.Security.Descriptor, aclInfo );
			this.IterateAces( sc.Security.Descriptor, aces );
			this.IterateRightRoles( sc.Security.RightRoles, rightRoles );
			this.RecurseRightRoleRules( sc.Security.RightRoleRules, rightRoleRules );

			if( sc != _topLsControl )
			{
				sc.Security.Audited += _auditEventHandler;

				if( _propagateAuditWriter )
				{
					sc.Security.AuditWriter = _auditWriter;
				}
			}
		}

		/* Denormalization, explained (royal pita):
		 * 1) Select all the rows from GroupMembership where user is directly a member
		 * 2a) Create a CSV list of those Group Names
		 * 2b) Add the CSV list of External Group Names
		 * 2c) Create the "GROUP_NAME" OR clause
		 * 3) Select the matching orClause rows from GroupMembership
		 * 4) Use matching rows to create a "MEMBER_ID" OR clause,
		 *	  which would be to say, "groups that the matching rows are members of"
		 * 5) recurse upward while to select all parent groups
		 * 
		 * the result of this is to select the user's group membership
		 * and all the groups those groups are nested in
		 * 
		 * 6) Select the Ids of ExternalGroups directly from SecurityPrincipal list in case any aces are assigned directly to ExtGroups
		 * 7) Create OR clause for any matching and ExtGroups
		 * 8) Select matching Aces from AceList (the Aces which were persisted in the file)
		 * 9) Copy them into the Aces table, adding in the UserId (UserId used by the LoadRulesFromCache select)
		 */
		private void DenormalizeAces(string userId)
		{
			DataRow[] gm = _dsCache.Tables["GroupMembership"].Select( string.Format( "MEMBER_ID = '{0}'", userId ) );
			string gmCsv = MiscUtils.Join( ",", gm, "GROUP_NAME" );
			if( string.IsNullOrEmpty( gmCsv ) ) { gmCsv = null; }

			string[] groups = string.Format( "{0},{1}", gmCsv, _egi.GroupsList ).Split( ',' );

			string orClause = string.Format( "GROUP_NAME = '{0}'", MiscUtils.Join<string>( "' OR GROUP_NAME = '", groups ) );

			List<string> memberOf = new List<string>();
			DataRow[] member = _dsCache.Tables["GroupMembership"].Select( orClause );
			while( member.Length > 0 )
			{
				orClause = string.Format( "MEMBER_ID = '{0}'", MiscUtils.Join( "' OR MEMBER_ID = '", member, "SPLX_GROUP_ID" ) );
				memberOf.Add( string.Format( "SPLX_GROUP_ID = '{0}'", MiscUtils.Join( "' OR SPLX_GROUP_ID = '", member, "SPLX_GROUP_ID" ) ) );
				member = _dsCache.Tables["GroupMembership"].Select( orClause );
			}

			string[] extGroups = _egi.GroupsList.Split( ',' );
			DataRow[] extGroupSPs = _dsCache.Tables["SecurityPrincipals"].Select(
				string.Format( "PRINCIPAL_NAME = '{0}'", MiscUtils.Join<string>( "' OR PRINCIPAL_NAME = '", extGroups ) ) );
			memberOf.Add( string.Format( "SPLX_GROUP_ID = '{0}'", MiscUtils.Join( "' OR SPLX_GROUP_ID = '", extGroupSPs, "PRINCIPAL_ID" ) ) );

			orClause = MiscUtils.Join<string>( " OR ", memberOf );
			if( !string.IsNullOrEmpty( orClause ) )
			{
				DataRow[] aces = _dsCache.Tables["AceList"].Select( orClause );
				foreach( DataRow ace in aces )
				{
					ace["SPLX_USER_ID"] = userId;
					if( !ace["ACE_TYPE"].ToString().EndsWith( "Ace" ) )
					{
						ace["ACE_TYPE"] = string.Format( "{0}{1}Ace", ace["ACE_TYPE"].ToString(),
							(bool)ace["IS_AUDIT_ACE"] ? "Audit" : string.Empty );
					}
					_dsCache.Tables["Aces"].Rows.Add( ace.ItemArray );
				}
			}
		}

		private bool ImpersonationIsValid(string externalGroupsList)
		{
			if( _user.Id == null )
			{
				return false;
			}
			else
			{
				SortedList inparms = new sSortedList(
					"@TASK_NAME", "impersonate",
					"@SPLX_USER_ID", _user.Id,
					"@EXTERNAL_GROUP_LIST", externalGroupsList
					);
				SortedList outparms = new sSortedList( "@ALLOWED", false );

				_da.ExecuteSP( "splx.splx_dal_sel_taskaccess", inparms, ref outparms );

				return (bool)outparms["@ALLOWED"];
			}
		}

		private void SetAclInfo(SecurityDescriptor sd, DataRow[] aclInfo)
		{
			if( aclInfo.Length > 0 )
			{
				sd.Dacl.InheritAces = (bool)aclInfo[0]["UIE_DACL_INHERIT"];
				sd.Sacl.InheritAces = (bool)aclInfo[0]["UIE_SACL_INHERIT"];

				if( aclInfo[0]["UIE_SACL_AUDIT_TYPE_FILTER"] != Convert.DBNull )
				{
					sd.Sacl.AuditTypeFilter = (AuditType)aclInfo[0]["UIE_SACL_AUDIT_TYPE_FILTER"];
				}
			}
		}

		private void IterateAces(SecurityDescriptor sd, DataRow[] aces)
		{
			//SortedList parms = new SortedList();
			//parms.Add( "@acpAce_fk", 0 );


			Type aceType = null;

			IAccessControlEntry aceObj = null;

			foreach( DataRow ace in aces )
			{
				aceType = _secasm.GetType( "Suplex.Security." + ace["ACE_TYPE"].ToString() );
				aceObj = (IAccessControlEntry)Activator.CreateInstance( aceType );

				aceObj.Allowed = bool.Parse( ace["ACE_ACCESS_TYPE1"].ToString() );
				aceObj.Inherit = bool.Parse( ace["ACE_INHERIT"].ToString() );

				//aceObj.Right = (int)ace["ACE_ACCESS_MASK"]; //this works, but the refl code works better
				ReflectionUtils.SetProperty( aceObj, "Right", ace["ACE_ACCESS_MASK"], true, true );


				/*
				 * Not in use, commented-out to save the database hit.
				 * 
				if( _da != null )
				{
					parms["@acpAce_fk"] = ace["acePk"].ToString();
					_da.OpenConnection();
					_da.GetDataSet( "sc_selAceParametersByAcePK", parms, _ds, "AceParameters", false );
					_da.CloseConnection();
					foreach( DataRow parm in _ds.Tables["AceParameters"].Rows )
					{
						( (IAccessControlEntry)aceObj ).Parameters.Add( parm["acpParameter"].ToString() );
					}
					_ds.Tables.Remove( "AceParameters" );
				}
				*/


				if( aceObj is IAccessControlEntryAudit )
				{
					((IAccessControlEntryAudit)aceObj).Denied = bool.Parse( ace["ACE_ACCESS_TYPE2"].ToString() );
					sd.Sacl.Add( (int)ace["SPLX_ACE_ID"], (IAccessControlEntryAudit)aceObj );
				}
				else
				{
					sd.Dacl.Add( (int)ace["SPLX_ACE_ID"], aceObj );
				}

			}//foreach

		}//IterateAces

		private void IterateRightRoles(RightRoleCollection rightRoles, DataRow[] roles)
		{
			RightRole rr = null;

			foreach( DataRow role in roles )
			{
				rr = new RightRole();
				rr.ControlUniqueName = role["RR_UIE_UNIQUE_NAME"].ToString();
				rr.AceType = MiscUtils.ParseEnum<AceType>( role["RR_ACE_TYPE"].ToString(), true );
				rr.RightName = role["RR_RIGHT_NAME"].ToString();
				rr.UIRight = MiscUtils.ParseEnum<UIRight>( role["RR_UI_RIGHT"].ToString(), true );

				rightRoles.Add( rr );
			}
		}

		private void RecurseRightRoleRules(RightRoleRuleCollection rightRoleRules, DataRow[] rules)
		{
			string ruleId = string.Empty;

			foreach( DataRow rule in rules )
			{
				RightRoleRule r = new RightRoleRule()
				{
					CompareDataType = MiscUtils.ParseEnum<TypeCode>( rule["VR_COMPARE_DATA_TYPE"].ToString(), true ),
					Operator = MiscUtils.ParseEnum<ComparisonOperator>( rule["VR_OPERATOR"].ToString(), true ),
					FailParent = (bool)rule["VR_FAIL_PARENT"],
					ErrorMessage = rule["VR_ERROR_MESSAGE"].ToString(),
					ValueType1 = MiscUtils.ParseEnum<ComparisonValueType>( rule["VR_VALUE_TYPE1"].ToString(), true ),
					ValueType2 = MiscUtils.ParseEnum<ComparisonValueType>( rule["VR_VALUE_TYPE2"].ToString(), true )
				};


				if( r.ValueType1 == ComparisonValueType.Expression )
				{
					r.CompareValue1 =
						new ExpressionElements( rule["VR_COMPARE_VALUE1"].ToString(),
						MiscUtils.ParseEnum<ExpressionType>( rule["VR_EXPRESSION_TYPE1"].ToString(), true ) );
				}
				else
				{
					r.CompareValue1
						= rule["VR_COMPARE_VALUE1"].ToString().Length == 0 ? null : rule["VR_COMPARE_VALUE1"];
				}

				if( r.ValueType2 == ComparisonValueType.Expression )
				{
					r.CompareValue2 =
						new ExpressionElements( rule["VR_COMPARE_VALUE2"].ToString(),
						MiscUtils.ParseEnum<ExpressionType>( rule["VR_EXPRESSION_TYPE2"].ToString(), true ) );
				}
				else
				{
					r.CompareValue2
						= rule["VR_COMPARE_VALUE2"].ToString().Length == 0 ? null : rule["VR_COMPARE_VALUE2"];
				}

				rightRoleRules.Add( r );

				ruleId = rule["SPLX_VALIDATION_RULE_ID"].ToString();

				//'If' RightRoles
				DataRow[] rr = _dsCache.Tables["RightRoles"].Select( string.Format(
					"SPLX_UI_ELEMENT_RULE_ID = '{0}' AND RR_ROLE_TYPE = '{1}'", ruleId, RightRoleType.Success ) );
				this.IterateRightRoles( r.RightRoles, rr );

				//'Else' RightRoles
				rr = _dsCache.Tables["RightRoles"].Select( string.Format(
					"SPLX_UI_ELEMENT_RULE_ID = '{0}' AND RR_ROLE_TYPE = '{1}'", ruleId, RightRoleType.Else ) );
				this.IterateRightRoles( r.ElseRoles, rr );


				//'If' rules
				DataRow[] chldrn = _dsCache.Tables["RightRoleRules"].Select( string.Format(
					"VR_RULE_TYPE='{0}' AND VR_PARENT_ID = '{1}'", LogicRuleType.RightRoleIf, ruleId ), "VR_SORT_ORDER ASC" );
				if( chldrn.Length > 0 )
				{
					this.RecurseRightRoleRules( r.RightRoleRules, chldrn );
				}

				//'Else' rules
				chldrn = _dsCache.Tables["RightRoleRules"].Select( string.Format(
					"VR_RULE_TYPE='{0}' AND VR_PARENT_ID = '{1}'", LogicRuleType.RightRoleElse, ruleId ), "VR_SORT_ORDER ASC" );
				if( chldrn.Length > 0 )
				{
					this.RecurseRightRoleRules( r.ElseRules, chldrn );
				}
			}
		}

		public void ResolveRightRolesPre(ISecureControl control, IEnumerable controls)
		{
			if( _topRrControl == null )
			{
				_topRrControl = control;
			}

			//init the RightRoleNameCache _only_ if it's null,
			//just so it's not null for future lookup requests
			if( _rrNameCache == null )
			{
				_rrNameCache = new ControlCache();
			}
			_rrNonPropagating = new ControlCache();

			this.ResolveRightRolesPreRecursive( control, controls );


			//_isWindows = control is WinForms.Control;

			//if( _isWindows )
			//{ ResolveRightRolesPreWin( (WinForms.Control)control, (WinForms.Control.ControlCollection)controls ); }
			//else
			//{ ResolveRightRolesPreWeb( (WebForms.Control)control, (WebForms.ControlCollection)controls ); }


			//impt: clear member variables
			_topRrControl = null;

			_rrNameCache = null;
		}

		private void ResolveRightRolesPreRecursive(object control, IEnumerable controls)
		{
			if( control is ISecureControl )
			{
				this.ResolveRightRolesPreInternal( (ISecureControl)control, controls );
			}

			IEnumerable controlControls = _enumUtil.GetChildren( control );
			IEnumerator children = controlControls.GetEnumerator();
			while( children.MoveNext() )
			{
				this.ResolveRightRolesPreRecursive( children.Current, controlControls );
			}
		}

		private void ResolveRightRolesPreInternal(ISecureControl control, IEnumerable controls)
		{
			ISecureControl ctl = null;

			for( int n = 0; n < control.Security.RightRoles.Count; n++ )
			{
				string un = control.Security.RightRoles[n].ControlUniqueName;

				ctl = _rrNameCache.FindControl( un, _topRrControl, controls );

				if( ctl != null )
				{
					control.Security.RightRoles[n].ControlRef = ctl;

					if( control.Security.Descriptor.Dacl.
						ContainsAceType( control.Security.RightRoles[n].AceType ) )
					{
						this.CopyAces( control, control.Security.RightRoles[n] );
					}
					else
					{
						_rrNonPropagating[control.UniqueName] = control;
					}
				}
				else
				{
					string msg = String.Format(
						"Could not resolve control reference '{0}' from\r\n'{1}'.\r\nTop control UniqueName is '{2}'.",
						un, control.UniqueName, _topRrControl.UniqueName );

					try	//this won't fly if it's a web project
					{
						UniqueNameErrorDlg err = new UniqueNameErrorDlg();
						err.ShowSecureControlError( _topRrControl, control.UniqueName, msg );
					}
					catch
					{
						SecureControlUtils scu = new SecureControlUtils();
						msg += "\r\n\r\n" + scu.DumpHierarchy( _topRrControl, false );
					}

					throw new SystemException( msg );
				}
			}
		}


		private void CopyAces(ISecureControl sourceCtl, RightRole role)
		{
			UIAce u_ace = null;
			UIAuditAce ua_ace = null;

			bool match = false;

			DiscretionaryAcl dacl = sourceCtl.Security.Descriptor.Dacl;
			SystemAcl sacl = sourceCtl.Security.Descriptor.Sacl;

			//logic: if the sourceCtl dacl has an ace that matches the acetype and right
			//desired for the rightrole translation then use it to create the UI destination ace
			foreach( KeyValuePair<int, IAccessControlEntry> entry in dacl )
			{
				IAccessControlEntry s_ace = entry.Value;
				if( s_ace.AceType == role.AceType &&
					s_ace.Right.ToString() == role.RightName )
				{
					match = true;

					u_ace = new UIAce( role.UIRight, s_ace.Allowed );
					u_ace.Inherit = s_ace.Inherit;
					u_ace.InheritedFrom = sourceCtl.UniqueName;

					//role.ControlRef.Security.Descriptor.Dacl.Add( dacl.GetKey(n), u_ace );		//12/11/2005: see note
					role.ControlRef.Security.Descriptor.Dacl.Add( entry.Key * -1, u_ace );
				}
			}

			/*	12/11/2005 Note:
			 * 
			 *	swapped to using acl key generation (instead just using the existing key),
			 *	  b/c when inheriting upwards from ImComponents the Dacl.CopyTo routine
			 *	  overwrites items in the collection w/ the same key (uses SortedList[key] = value).
			 *	  not sure why i never encountered this before.
			 *	algorithm: [((int)dacl.GetKey(n))*-1] -- takes negative of existing integer key.
			 *	  if this ever fails, switch to using [_aclKey++].
			 *	  using the [-1] method just makes it a little easier to recognize
			 *	  the origin of the ace.
			 * 
			 * */



			//logic, continued: if a matching ace couldn't be found above, then we need to derive what the value
			//of the ace would be if it existed.
			if( !match )
			{
				AceTypeRights at = new AceTypeRights( role.AceType );
				//				bool allowed = sourceCtl.SecurityDescriptor.SecurityResults[role.AceType, Enum.Parse(at.Right, role.RightName)].AccessAllowed;


				//this creates a new SecurityResultCollection to use a temporary storage for
				//the result of the Dacl for the given AceType  (role.AceType) for sourceCtl.
				//from there, we can extract the value for the specific right (role.RightName).
				SecurityResultCollection src = new SecurityResultCollection();
				src.AddAceType( role.AceType, false, false, false );
				sourceCtl.Security.Descriptor.Dacl.HasAccess( role.AceType, src );
				bool allowed = src[role.AceType, Enum.Parse( at.Right, role.RightName )].AccessAllowed;

				//create the UIAce w/ the named right (role.UIRight) and the 'allowed' value 
				//as derived above.  add it to the destination control (role.ControlRef) Dacl.
				u_ace = new UIAce( role.UIRight, allowed );
				u_ace.InheritedFrom = sourceCtl.UniqueName;
				role.ControlRef.Security.Descriptor.Dacl.Add( _aclKey++, u_ace );
			}


			match = false;
			foreach( KeyValuePair<int, IAccessControlEntry> entry in sacl )
			{
				IAccessControlEntryAudit sa_ace = (IAccessControlEntryAudit)entry.Value;
				if( sa_ace.AceType == role.AceType &&
					sa_ace.Right.ToString() == role.RightName )
				{
					ua_ace = new UIAuditAce( role.UIRight, sa_ace.Allowed, sa_ace.Denied );
					ua_ace.Inherit = sa_ace.Inherit;
					ua_ace.InheritedFrom = sourceCtl.UniqueName;

					role.ControlRef.Security.Descriptor.Sacl.Add( entry.Key * -1, ua_ace );
				}
			}
		}

		public void ResolveRightRolesPost()
		{
			if( _rrNonPropagating == null )
			{
				return;
			}

			UIAce u_ace = null;
			UIAuditAce u_aace = null;
			SecurityDescriptor scSD = null;

			foreach( ISecureControl sourceCtl in _rrNonPropagating.Values )
			{
				scSD = sourceCtl.Security.Descriptor;
				foreach( RightRole role in sourceCtl.Security.RightRoles )
				{
					AceTypeRights at = new AceTypeRights( role.AceType );

					if( role.ControlRef != null )
					{
						object right = Enum.Parse( at.Right, role.RightName );

						if( scSD.Dacl.ContainsAceType( role.AceType ) )
						{
							u_ace =
								new UIAce( role.UIRight, scSD.SecurityResults[role.AceType, right].AccessAllowed );
						}
						else
						{
							u_ace = new UIAce( role.UIRight, false );
						}
						u_ace.Inherit = false;
						u_ace.InheritedFrom = sourceCtl.UniqueName;

						role.ControlRef.Security.Descriptor.Dacl.Add( _aclKey++ * -1, u_ace );


						if( scSD.Sacl.ContainsAceType( role.AceType ) )
						{
							u_aace =
								new UIAuditAce( role.UIRight,
								scSD.SecurityResults[role.AceType, right].AuditSuccess,
								scSD.SecurityResults[role.AceType, right].AuditFailure );
							u_aace.Inherit = false;
							u_aace.InheritedFrom = sourceCtl.UniqueName;

							role.ControlRef.Security.Descriptor.Sacl.Add( _aclKey++ * -1, u_aace );
						}


						//this will apply security locally only (local to the ControlRef control, non-propagating)
						role.ControlRef.Security.Descriptor.EvalSecurity( AceType.UI );
						((ISecureControl)role.ControlRef).ApplySecurity();
					}
				}
			}

			_rrNonPropagating = null;
		}

		#region ResolveInternalReferences
		public void ResolveInternalReferences(ISecureControl control)
		{
			_dal = control.DataAccessLayer;
			_topRrrControl = control;
			IEnumerable topCC = _enumUtil.GetChildren( control );

			//RecurseControls( control.ValidationControls );	//03092004

			//03092004 **************************************************
			this.RecurseRulesForInternalReferences( control, control.Security.RightRoleRules, topCC );
			control.Security.RightRoleRules.Process( string.Empty, TypeCode.String, string.Empty, _dal.Application );

			this.RecurseControlsForInternalReferences( topCC );
			//if( topCC.Count > 0 ) { }	//removed 12/21/2008 w/ switch from ICollection to IEnumerable
			//03092004 **************************************************

			_topRrrControl = null;
		}

		private void RecurseControlsForInternalReferences(IEnumerable cc)
		{
			IEnumerator ctls = cc.GetEnumerator();
			while( ctls.MoveNext() )
			{
				if( ctls.Current is ISecureControl )
				{
					ISecureControl sc = (ISecureControl)ctls.Current;

					sc.DataAccessLayer = _dal;
					this.RecurseRulesForInternalReferences( sc, sc.Security.RightRoleRules, cc );
					sc.Security.RightRoleRules.Process( string.Empty, TypeCode.String, string.Empty, _dal.Application );
				}

				IEnumerable children = _enumUtil.GetChildren( ctls.Current );
				this.RecurseControlsForInternalReferences( children );
				//if( children.Count > 0 ) { }	//removed 12/21/2008 w/ switch from ICollection to IEnumerable
			}
		}

		private void RecurseRulesForInternalReferences(ISecureControl sc, RightRoleRuleCollection rc, IEnumerable controlCollection)
		{
			IEnumerator rules = rc.GetEnumerator();
			while( rules.MoveNext() )
			{
				RightRoleRule r = (RightRoleRule)rules.Current;

				if( r.ValueType1 == ComparisonValueType.Control )
				{
					ControlCompareValue ccv = new ControlCompareValue( r.CompareValue1.ToString() );
					ccv.ControlRef = (IValidationControl)_rrNameCache.FindControl( ccv.ControlUniqueName, _topRrrControl, controlCollection );
					r.CompareValue1 = ccv;

					if( ccv.ControlRef == null )
					{
						string msg = String.Format(
							"Could not resolve control reference '{0}'.\r\nTop control UniqueName is '{1}'.",
							ccv.ControlUniqueName, _topRrrControl.UniqueName );

						//try	//this won't fly if it's a web project	catch{}
						if( _ctrlType == ControlType.WinForms )
						{
							UniqueNameErrorDlg err = new UniqueNameErrorDlg();
							err.ShowSecureControlError( _topRrrControl, ccv.ControlUniqueName, msg );
						}
						else
						{
							SecureControlUtils scu = new SecureControlUtils();
							msg += "\r\n\r\n" + scu.DumpHierarchy( _topRrrControl, false );
						}


						throw new SystemException( msg );
					}
				}

				if( r.ValueType2 == ComparisonValueType.Control )
				{
					ControlCompareValue ccv = new ControlCompareValue( r.CompareValue2.ToString() );
					ccv.ControlRef = (IValidationControl)_rrNameCache.FindControl( ccv.ControlUniqueName, _topRrrControl, controlCollection );
					r.CompareValue2 = ccv;

					if( ccv.ControlRef == null )
					{
						string msg = String.Format(
							"Could not resolve control reference '{0}'.\r\nTop control UniqueName is '{1}'.",
							ccv.ControlUniqueName, _topRrrControl.UniqueName );

						//try	//this won't fly if it's a web project	catch{}
						if( _ctrlType == ControlType.WinForms )
						{
							UniqueNameErrorDlg err = new UniqueNameErrorDlg();
							err.ShowSecureControlError( _topRrrControl, ccv.ControlUniqueName, msg );
						}
						else
						{
							SecureControlUtils scu = new SecureControlUtils();
							msg += "\r\n\r\n" + scu.DumpHierarchy( _topRrrControl, false );
						}


						throw new SystemException( msg );
					}
				}

				if( r.ValueType1 == ComparisonValueType.Expression )
				{
					ExpressionElements value = (ExpressionElements)r.CompareValue1;
					this.ResolveExpression( ref value, controlCollection );
					r.CompareValue1 = value;
				}

				if( r.ValueType2 == ComparisonValueType.Expression )
				{
					ExpressionElements value = (ExpressionElements)r.CompareValue2;
					this.ResolveExpression( ref value, controlCollection );
					r.CompareValue2 = value;
				}

				if( r.RightRoleRules.Count > 0 )
				{
					this.RecurseRulesForInternalReferences( sc, r.RightRoleRules, controlCollection );
				}

				if( r.ElseRules.Count > 0 )
				{
					this.RecurseRulesForInternalReferences( sc, r.ElseRules, controlCollection );
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
					IValidationControl ctl = (IValidationControl)_rrNameCache.FindControl( flds[n], _topRrrControl, vcc );
					if( ctl != null )
					{
						vc.Add( flds[n], ctl );		//old: vc.Add( flds[n], (IValidationControl)vcc[flds[n]] );
					}
					else
					{
						string msg = String.Format(
							"Could not resolve control reference '{0}' from\r\n'{1}'.\r\nTop control UniqueName is '{2}'.",
							flds[n], value.Expression, _topRrrControl.UniqueName );

						//try	//this won't fly if it's a web project	catch{}
						if( _ctrlType == ControlType.WinForms )
						{
							UniqueNameErrorDlg err = new UniqueNameErrorDlg();
							err.ShowSecureControlError( _topRrrControl, value.Expression, msg );
						}
						else
						{
							SecureControlUtils scu = new SecureControlUtils();
							msg += "\r\n\r\n" + scu.DumpHierarchy( _topRrrControl, false );
						}


						throw new SystemException( msg );
					}
				}
			}
			value.ValidationControls = vc;
		}
		#endregion


		/// <summary>
		/// Recursively reverse-copies the security descriptor from source control to the destination control.
		/// Sets DACL and SACL inheritance flags to false.
		/// </summary>
		/// <param name="source">The source control from which the SD will be copied.</param>
		/// <param name="control">The destination control onto which this control's SD will be copied.</param>
		[System.ComponentModel.Description( "Recursively reverse-copies the security descriptor from this control to the 'control'.\r\nSets DACL and SACL inheritance flags to false." )]
		public void CloneSecurityState(ISecureControl source, ISecureControl dest)
		{
			//TODO: this!!!
			//foreach( string key in source.SecureControls.UniqueNameKeys )
			//{
			//    CloneSecurityState( source.SecureControls[key], dest.SecureControls[key] );
			//}

			dest.Security.UserContext = source.Security.UserContext;
			dest.Security.AuditWriter = source.Security.AuditWriter;
			dest.Security.DefaultState = source.Security.DefaultState;

			source.Security.Descriptor.CopyTo( dest.Security.Descriptor );
			dest.Security.Descriptor.Dacl.InheritAces = false;
			dest.Security.Descriptor.Sacl.InheritAces = false;

			ArrayList AceType = new ArrayList( source.Security.Descriptor.SecurityResults.GetAceTypes() );
			for( int n = 0; n < AceType.Count; n++ )
			{
				dest.Security.Apply( (AceType)AceType[n] );
			}
		}




		internal void ApplySecurityRecursive(object control, AceType aceType)
		{
			if( control is ISecureControl )
			{
				ISecureControl c = (ISecureControl)control;
				if( aceType == AceType.Native )
				{
					foreach( AceType t in c.Security.NativeAceTypes )
					{
						if( t != AceType.Native && t != AceType.None )
						{
							c.Security.Descriptor.EvalSecurity( t );
							c.ApplySecurity();
							c.Security.AuditAccess( t );
						}
					}
				}
				else
				{
					c.Security.Descriptor.EvalSecurity( aceType );
					c.ApplySecurity();
					c.Security.AuditAccess( aceType );
				}

				if( c is WinForms.Form &&
					((WinForms.Form)c).Menu != null )
				{
					this.PropagateSecurityRecursive( (ISecureContainer)c, ((WinForms.Form)c).Menu );
				}

				if( c is ISecureContainer )
				{
					this.PropagateSecurityRecursive( (ISecureContainer)c, c );
				}
			}

			//IEnumerator controls = _isWindows ?
			//    ( (WinForms.Control)control ).Controls.GetEnumerator() :
			//    ( (WebForms.Control)control ).Controls.GetEnumerator();
			if( control is WinForms.Form &&
				((WinForms.Form)control).Menu != null )
			{
				this.ApplySecurityRecursive( ((WinForms.Form)control).Menu, aceType );
			}

			IEnumerator controls = _enumUtil.GetChildren( control ).GetEnumerator();
			while( controls.MoveNext() )
			{
				this.ApplySecurityRecursive( controls.Current, aceType );
			}
		}

		internal void PropagateSecurityRecursive(ISecureContainer ancestorContainer, object currentParent)
		{
			ISecureControl ac = (ISecureControl)ancestorContainer;

			//IEnumerator sControls = parent.SecureControls.Values.GetEnumerator();
			//IEnumerator controls = _isWindows ?
			//    ( (WinForms.Control)currentParent ).Controls.GetEnumerator() :
			//    ( (WebForms.Control)currentParent ).Controls.GetEnumerator();

			IEnumerator controls = _enumUtil.GetChildren( currentParent ).GetEnumerator();


			//If parent is NOT an ISecurityExtender, look to see if there's any ISecurityExtenders in the
			//child control collection.  If so, get a copy of each one's SecurityDescriptor
			//and add it to the parent's SD.  This is done so as to treat ISecurityExtenders as "extensions"
			//of a control, but not of other ISecurityExtenders, and thereby allowing other ISecurityExtenders
			//that are descendents of parent's childern to inherit the ISecurityExtenders aces.
			if( !(ancestorContainer is ISecurityExtender) )		//added ISecurityExtender exclusion on 09/07/2005
			{
				while( controls.MoveNext() )
				{
					if( controls.Current is ISecurityExtender )
					{
						ISecureControl sc = (ISecureControl)controls.Current;
						sc.Security.Descriptor.CopyTo(
							((ISecureControl)ancestorContainer).Security.Descriptor, sc.UniqueName );
					}
				}
			}

			//now, copy parent's SecurityDescriptor to each child control.
			// - container controls get a complete copy of the SecurityDescriptor.
			// - non-container controls only get copy of native aces
			controls.Reset();
			while( controls.MoveNext() )
			{
				if( controls.Current is ISecureControl )
				{
					ISecureControl sc = (ISecureControl)controls.Current;

					if( sc is ISecureContainer )
					{
						ac.Security.Descriptor.CopyTo( sc.Security.Descriptor, ac.UniqueName );
					}
					else if( sc is ISecureControl )
					{
						for( int n = 0; n < sc.Security.NativeAceTypes.Length; n++ )
						{
							ac.Security.Descriptor.CopyTo( sc.Security.Descriptor, sc.Security.NativeAceTypes[n], ac.UniqueName );
						}
					}

					//sc.Security.ApplySecurity( AceType.Native );
				}
				else
				{
					this.PropagateSecurityRecursive( ancestorContainer, controls.Current );
				}
			}
		}



		/*
		 * Deprecated
		 * 
		private ISecureControl _currentPropogateParent = null;
		public void PropagateSecurity(object parent)
		{
			if( parent is ISecureControl )
			{
				_currentPropogateParent = (ISecureControl)parent;
			}

			//IEnumerator sControls = parent.SecureControls.Values.GetEnumerator();
			IEnumerator controls = _isWindows ?
				( (WinForms.Control)parent ).Controls.GetEnumerator() :
				( (WebForms.Control)parent ).Controls.GetEnumerator();

			//If parent is NOT an ISecurityExtender, look to see if there's any ImComponents in the
			//child control collection.  If so, get a copy of each one's SecurityDescriptor
			//and add it to the parent's SD.  This is done so as to treat ISecurityExtenders as "extensions"
			//of a control, but not of other ISecurityExtenders, and thereby allowing other ImComponents
			//that are descendents of parent's childern to inherit the ImComponents aces.
			if( !( _currentPropogateParent is ISecurityExtender ) )		//added ISecurityExtender exclusion on 09/07/2005
			{
				while( controls.MoveNext() )
				{
					if( controls.Current is ISecurityExtender )
					{
						ISecureControl sc = (ISecureControl)controls.Current;
						sc.Security.Descriptor.CopyTo( _currentPropogateParent.Security.Descriptor, false, sc.UniqueName );
					}
				}
			}

			//now, copy parent's SecurityDescriptor to each child control.
			// - container controls get a complete copy of the SecurityDescriptor.
			// - non-container controls only get copy of native aces
			controls.Reset();
			while( controls.MoveNext() )
			{
				if( controls.Current is ISecureControl )
				{
					ISecureControl sc = (ISecureControl)controls.Current;

					if( sc is ISecureContainer )
					{
						_currentPropogateParent.Security.Descriptor.CopyTo( sc.Security.Descriptor, false, _currentPropogateParent.UniqueName );
					}
					else
					{
						for( int n = 0; n < sc.Security.NativeAceTypes.Length; n++ )
						{
							_currentPropogateParent.Security.Descriptor.CopyTo( sc.Security.Descriptor, sc.Security.NativeAceTypes[n], false, _currentPropogateParent.UniqueName );
						}
					}

					sc.Security.ApplySecurity( AceType.Native );
				}
			}
		}
		*/
	}//class


	public struct ExternalGroupInfo
	{
		private string _ldapRoot;
		private bool _allowAnonymous;
		private string _extGroups;

		private string _authUser;
		private string _authPswd;

		private string _groupsList;

		public static readonly ExternalGroupInfo Empty;
		static ExternalGroupInfo()
		{
			Empty = new ExternalGroupInfo( string.Empty, false );
		}

		public static bool IsEmpty(ExternalGroupInfo egi)
		{
			return egi.Equals( ExternalGroupInfo.Empty );
		}

		public ExternalGroupInfo(string ldapRoot, bool allowAnonymous)
		{
			_ldapRoot = ldapRoot;
			_allowAnonymous = allowAnonymous;
			_extGroups = string.Empty;
			_groupsList = string.Empty;

			_authUser = string.Empty;
			_authPswd = string.Empty;
		}

		public ExternalGroupInfo(string ldapRoot, bool allowAnonymous, string externalGroupsList)
		{
			_ldapRoot = ldapRoot;
			_allowAnonymous = allowAnonymous;
			_extGroups = externalGroupsList;
			_groupsList = string.Empty;

			_authUser = string.Empty;
			_authPswd = string.Empty;
		}

		public ExternalGroupInfo(string ldapRoot, bool allowAnonymous, string externalGroupsList, string adAuthUserName, string adAuthPassword)
		{
			_ldapRoot = ldapRoot;
			_allowAnonymous = allowAnonymous;
			_extGroups = externalGroupsList;
			_groupsList = string.Empty;

			_authUser = adAuthUserName;
			_authPswd = adAuthPassword;
		}

		public string LdapRoot { get { return _ldapRoot; } }
		internal bool HasLdapRoot { get { return !string.IsNullOrEmpty( _ldapRoot ); } }
		public bool AllowAnonymous { get { return _allowAnonymous; } }
		public string ExternalGroups { get { return _extGroups; } }
		internal bool HasExternalGroups { get { return !string.IsNullOrEmpty( _extGroups ); } }
		public string GroupsList { get { return _groupsList; } }

		public void BuildGroupsList(string userName)
		{
			string groups = string.Empty;
			if( this.HasLdapRoot )
			{
				groups = this.GetNtGroupsCSV( userName );
			}
			if( this.HasExternalGroups )
			{
				if( string.IsNullOrEmpty( groups ) )
				{
					groups = _extGroups;
				}
				else
				{
					groups = string.Format( "{0},{1}", groups, _extGroups );
				}
			}

			_groupsList = groups;
		}

		internal bool TryBuildGroupsList(string userName)
		{
			_groupsList = string.Empty;

			bool success = false;

			try
			{
				this.BuildGroupsList( userName );
				success = true;
			}
			catch { }

			return success;
		}

		internal string GetNtGroupsCSV(string userName)
		{
			string result = string.Empty;

			if( !string.IsNullOrEmpty( _ldapRoot ) )
			{
				string[] user = userName.Split( '\\' );
				string name = user[0];
				if( user.Length > 1 )
				{
					name = user[1];
				}

				DirectoryEntry root = new DirectoryEntry( _ldapRoot );
				if( !string.IsNullOrEmpty( _authUser ) )
				{
					root.Username = _authUser;
					root.Password = _authPswd;
				}
				DirectorySearcher groups = new DirectorySearcher( root );
				groups.Filter = "sAMAccountName=" + name;
				groups.PropertiesToLoad.Add( "memberOf" );

				SearchResult sr = groups.FindOne();
				StringBuilder list = new StringBuilder();

				if( sr != null )
				{
					for( int i = 0; i <= sr.Properties["memberOf"].Count - 1; i++ )
					{
						string group = sr.Properties["memberOf"][i].ToString();
						list.AppendFormat( "{0},", group.Split( ',' )[0].Replace( "CN=", "" ) );
					}
				}

				result = list.ToString().TrimEnd( ',' );
			}

			return result;
		}
	}


	public class SecureControlUtils
	{
		private string blankCell = "<TD width=\"1px\">&nbsp;</TD>";

		private EnumUtil _enumUtil = new EnumUtil();

		public SecureControlUtils() { }

		[Obsolete( "Use SecurityAccessor.AuditAccess", true )]
		public void AuditAccess(ISecureControl control, AceType aceType, object right, AuditEventHandler auditHandler)
		{
			if( auditHandler != null )
			{
				string[] desc = { "Object Type: " + control.GetType().ToString(), "" };
				AuditType auditType = AuditType.SuccessAudit;
				bool raiseEvent = false;

				if( control.Security.Descriptor.SecurityResults[aceType, right].AccessAllowed &&
					control.Security.Descriptor.SecurityResults[aceType, right].AuditSuccess )
				{
					desc[1] = right.ToString() + " access granted.";
					raiseEvent = true;
				}

				if( !control.Security.Descriptor.SecurityResults[aceType, right].AccessAllowed &&
					control.Security.Descriptor.SecurityResults[aceType, right].AuditFailure )
				{
					desc[1] = right.ToString() + " access denied.";
					raiseEvent = true;
					auditType = AuditType.FailureAudit;
				}


				if( raiseEvent )
				{
					//desc[1] += " <---= ";
					object[] args = { control, new AuditEventArgs( control.Security.UserContext, auditType, control.UniqueName, AuditCategory.Access, desc ) };
					auditHandler.Method.Invoke( control, args );
				}
			}
		}

		[Obsolete( "Use SecurityAccessor.AuditAction", true )]
		public void AuditAction(ISecureControl control, AuditEventHandler auditHandler, AuditType auditType, string description)
		{
			if( auditHandler != null )
			{
				string[] desc = {	description,
									String.Format("Enabled: {0}.", control.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Enabled].AccessAllowed ? "Allowed" : "Denied"),
									String.Format("Operate: {0}.", control.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed ? "Allowed" : "Denied")	};

				object[] args = { control, new AuditEventArgs( control.Security.UserContext, auditType, control.UniqueName, AuditCategory.Action, desc ) };
				auditHandler.Method.Invoke( control, args );
			}
		}

		[Obsolete( "Use SecurityAccessor.AuditAction", true )]
		public void AuditAction(ISecureControl control, AuditEventHandler auditHandler, AuditType auditType, string source, string description)
		{
			if( auditHandler != null )
			{
				object[] args = { control, new AuditEventArgs( control.Security.UserContext, auditType, source, AuditCategory.Action, description ) };
				auditHandler.Method.Invoke( control, args );
			}
		}

		public string GetInlineCssString()
		{
			return string.Format( "\r\n<style type=\"text/css\">\r\n{0}\r\n</style>\r\n", Properties.Resources.diagsCSS );
		}

		public DiagInfoStreams DumpSecurity(ISecureControl control, bool showDetail, bool copyToClipboard)
		{
			return DumpSecurity( control, showDetail, copyToClipboard, true, false, null, null );
		}
		public DiagInfoStreams DumpSecurity(ISecureControl control, bool showDetail, bool copyToClipboard,
			bool includeInLineCss, bool includeHtmlPageTags, string pageTitle, string cssPath)
		{
			DiagInfoStreams d = new DiagInfoStreams();
			DumpSecurity( control, showDetail, d, 0 );

			d.Html.Insert( 0, string.Format( "<table border=0 cellspacing=0 cellpadding=0><tr><td>{0}</td></tr></table>\r\n<BR><BR>\r\n", d.Html2.ToString() ) );
			d.Html.Insert( 0, "<TABLE class=\"tLevel0\"><TR><TD class=\"HeaderTitle\">Security Detail<a name=\"sd_top\"></a></TD></TR></TABLE><BR>" );
			d.Html.Insert( 0, "<div id=\"Suplex_Security_Diagnostics\"><span class=\"suplex\">\r\n" );

			if( includeInLineCss || (includeHtmlPageTags && string.IsNullOrEmpty( cssPath )) )
			{
				d.Html.Insert( 0, GetInlineCssString() );
			}

			if( includeHtmlPageTags )
			{
				string s = string.Format( "<html>\r\n<head>\r\n<title>{0}</title>\r\n<LINK href=\"{1}\" type=\"text/css\" rel=\"stylesheet\">\r\n</head>\r\n<body>\r\n",
					string.IsNullOrEmpty( pageTitle ) ? "Security Detail" : pageTitle, cssPath );
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

		private void DumpSecurity(object ctrl, bool showDetail, DiagInfoStreams d, int level)
		{
			string spacer = new string( ' ', level * 2 );
			string spacer2 = spacer.Replace( " ", "&nbsp;&nbsp;" );

			bool isSecureControl = ctrl is ISecureControl;

			int style = level;
			if( level > 0 )
			{
				if( ctrl is ISecureContainer )
				{
					style = (style % 2) + 1;
				}
				else
				{
					style = 3;
				}
			}
			if( !isSecureControl ) { style = 4; }


			string displayName = EnumUtil.GetControlDisplayName( ctrl, string.Empty );
			string controlName = string.Format( "{0}&nbsp;&nbsp;[{1}]", displayName, ctrl.GetType().Name );

			d.Text.AppendFormat( "\r\n{0}{1}:\r\n", spacer, displayName );

			d.Html.AppendFormat(
				"<TABLE class=\"tLevel{0}\">\r\n<TR><TD class=\"Level{0}\"><a name=\"s_{1}\">{2}</a></TD><TD align=\"right\" class=\"Level{0}\"><a href=\"#sd_top\" class=\"top\">top</a></TD></TR>\r\n<TR>\r\n<TD colspan=\"2\">\r\n<TABLE class=\"Inner\">\r\n<TR>\r\n<TD colspan=\"2\">\r\n",
				style, displayName, controlName );
			d.Html2.AppendFormat( "{0}<a href=\"#s_{2}\" class=\"Level{1}\">{3}</a><br>\r\n",
				spacer2, style, displayName, controlName );	// 


			if( isSecureControl )
			{
				ISecureControl control = (ISecureControl)ctrl;


				#region showDetail
				if( showDetail )
				{
					//*** Dacl ***
					d.Text.AppendFormat( "{0}  Dacl:\r\n{0}    Inherit Aces: {1}\r\n", spacer, control.Security.Descriptor.Dacl.InheritAces );
					d.Text.AppendFormat( "{0}    Aces: {1}\r\n", spacer, control.Security.Descriptor.Dacl.Count );

					d.Html.Append( "<TABLE class=\"Inner\">\r\n<TR><TD colspan=\"2\"><font class=\"fLevel2\">Dacl:</font></TD></TR>\r\n" );
					d.Html.AppendFormat( "<TR>{0}<TD>Inherit Aces: <font class=\"hi{2}\">{1}</font></TD></TR>\r\n",
						blankCell, control.Security.Descriptor.Dacl.InheritAces, control.Security.Descriptor.Dacl.InheritAces ? 4 : 2 );
					d.Html.AppendFormat( "<TR>{0}<TD>Aces: {1}</TD></TR>\r\n", blankCell, control.Security.Descriptor.Dacl.Count );

					d.Html.AppendFormat( "<TR>{0}<TD><UL>\r\n", blankCell );
					foreach( KeyValuePair<int, IAccessControlEntry> entry in control.Security.Descriptor.Dacl )
					{
						IAccessControlEntry ace = entry.Value;

						d.Text.AppendFormat( "{0}      Ace {1}: Type-{2}, Right-{3}, Allowed-{4}, Inherit-{5}, InheritedFrom-{6}\r\n",
							spacer, entry.Key, ace.AceType, ace.Right, ace.Allowed, ace.Inherit,
							ace.InheritedFrom == string.Empty ? "(null)" : ace.InheritedFrom );

						d.Html.AppendFormat( "{0}Ace {1}: Type-{2}, Right-{3}, Allowed-{4}, Inherit-{5}, InheritedFrom-{6}\r\n",
							"<LI>", entry.Key, ace.AceType, ace.Right, ace.Allowed, ace.Inherit,
							ace.InheritedFrom == string.Empty ? "(null)" : string.Format( "<a href=\"#s_{0}\" class=\"Ctrl\">{0}</a>", ace.InheritedFrom ) );
					}
					d.Html.Append( "</UL></TD></TR>\r\n" );


					//*** Sacl ***
					d.Text.AppendFormat( "{0}  Sacl:\r\n{0}    Inherit Aces: {1}\r\n{0}    AuditTypeFilter: {2}\r\n",
						spacer, control.Security.Descriptor.Sacl.InheritAces, control.Security.Descriptor.Sacl.AuditTypeFilter );
					d.Text.AppendFormat( "{0}    Aces: {1}\r\n", spacer, control.Security.Descriptor.Sacl.Count );

					d.Html.Append( "<TR><TD colspan=\"2\"><font class=\"fLevel2\">Sacl:</font></TD></TR>\r\n" );
					d.Html.AppendFormat( "<TR>{0}<TD>Inherit Aces: <font class=\"hi{2}\">{1}</font></TD></TR>\r\n",
						blankCell, control.Security.Descriptor.Sacl.InheritAces, control.Security.Descriptor.Sacl.InheritAces ? 4 : 2 );
					d.Html.AppendFormat( "<TR>{0}<TD>AuditTypeFilter: {1}</TD></TR>\r\n", blankCell, control.Security.Descriptor.Sacl.AuditTypeFilter );
					d.Html.AppendFormat( "<TR>{0}<TD>Aces: {1}</TD></TR>\r\n", blankCell, control.Security.Descriptor.Sacl.Count );

					d.Html.AppendFormat( "<TR>{0}<TD><UL>\r\n", blankCell );
					//for( int n = 0; n < control.Security.Descriptor.Sacl.Count; n++ )
					foreach( KeyValuePair<int, IAccessControlEntry> entry in control.Security.Descriptor.Sacl )
					{
						IAccessControlEntryAudit ace = (IAccessControlEntryAudit)entry.Value;

						d.Text.AppendFormat( "{0}      Ace {1}: Type-{2}, Right-{3}, Success-{4}, Failure-{5}, Inherit-{6}, InheritedFrom-{7}\r\n",
							spacer, entry.Key, ace.AceType, ace.Right, ace.Allowed, ace.Denied, ace.Inherit,
							ace.InheritedFrom == string.Empty ? "(null)" : ace.InheritedFrom );

						d.Html.AppendFormat( "{0}Ace {1}: Type-{2}, Right-{3}, Success-{4}, Failure-{5}, Inherit-{6}, InheritedFrom-{7}\r\n",
							"<LI>", entry.Key, ace.AceType, ace.Right, ace.Allowed, ace.Denied, ace.Inherit,
							ace.InheritedFrom == string.Empty ? "(null)" : string.Format( "<a href=\"#s_{0}\" class=\"Ctrl\">{0}</a>", ace.InheritedFrom ) );
					}
					d.Html.Append( "</UL></TD></TR>\r\n" );


					//*** RightRoles ***
					d.Text.AppendFormat( "{0}  RightRoles: {1}\r\n", spacer, control.Security.RightRoles.Count );

					d.Html.Append( "<TR><TD colspan=\"2\"><font class=\"fLevel2\">RightRoles:</font></TD></TR>\r\n" );
					d.Html.AppendFormat( "<TR>{0}<TD><UL>\r\n", blankCell );

					for( int n = 0; n < control.Security.RightRoles.Count; n++ )
					{
						RightRole role = control.Security.RightRoles[n];
						string controlRefUniqueName =
							role.ControlRef != null ?
							string.Format( "{0} (Resolved)", role.ControlRef.UniqueName ) :
							string.Format( "{0} (Unresolved)", role.ControlUniqueName );

						d.Text.AppendFormat( "{0}    Translate: {1}\\{2} into UI\\{3} for {4}\r\n",
							spacer, role.AceType, role.RightName, role.UIRight, controlRefUniqueName );

						d.Html.AppendFormat( "{0}Translate: {1}\\{2} into UI\\{3} for {4}\r\n",
							"<LI>", role.AceType, role.RightName, role.UIRight,
							string.Format( "<a href=\"#s_{0}\" class=\"Ctrl\">{1}</a>",
							role.ControlUniqueName, controlRefUniqueName ) );
					}
					d.Html.Append( "</UL></TD></TR>\r\n" );
				}
				#endregion

				#region SecurityResults
				//*** SecurityResults ***
				d.Text.AppendFormat( "{0}  Resultant Security:\r\n", spacer );

				d.Html.Append( "<TR><TD colspan=\"2\"><font class=\"fLevel1\">Resultant Security:</font></TD></TR>\r\n" );
				d.Html.AppendFormat( "<TR>{0}<TD><UL>\r\n", blankCell );

				IEnumerator AceType = control.Security.Descriptor.SecurityResults.GetAceTypes().GetEnumerator();
				while( AceType.MoveNext() )
				{
					AceType aceType = (AceType)AceType.Current;
					object[] rights = AceTypeRights.GetRights( aceType );
					for( int n = rights.Length - 1; n >= 0; n-- )
					{
						d.Text.AppendFormat( "{0}    {1}-{2}: Allowed-{3}, AuditSuccess-{4}, AuditFailure-{5}\r\n",
							spacer, aceType, rights[n],
							control.Security.Descriptor.SecurityResults[aceType, rights[n]].AccessAllowed.ToString(),
							control.Security.Descriptor.SecurityResults[aceType, rights[n]].AuditSuccess.ToString(),
							control.Security.Descriptor.SecurityResults[aceType, rights[n]].AuditFailure.ToString() );

						d.Html.AppendFormat( "{0}{1}-{2}: Allowed-{3}, AuditSuccess-{4}, AuditFailure-{5}\r\n",
							"<LI>", aceType, rights[n],
							control.Security.Descriptor.SecurityResults[aceType, rights[n]].AccessAllowed.ToString(),
							control.Security.Descriptor.SecurityResults[aceType, rights[n]].AuditSuccess.ToString(),
							control.Security.Descriptor.SecurityResults[aceType, rights[n]].AuditFailure.ToString() );
					}
				}
				d.Html.Append( "</UL></TD></TR>\r\n" );
				#endregion

				#region State
				//*** State ***
				d.Text.AppendFormat( "{0}  UI Security State:\r\n", spacer );

				d.Html.Append( "<TR><TD colspan=\"2\"><font class=\"fLevel1\">UI Security State:</font></TD></TR>\r\n" );
				d.Html.AppendFormat( "<TR>{0}<TD><UL>\r\n", blankCell );

				d.Text.AppendFormat( "{0}    {1}\r\n", spacer, control.GetSecurityState() );

				d.Html.AppendFormat( "{0}{1}\r\n", "<LI>", control.GetSecurityState() );

				d.Html.Append( "</UL></TD></TR>\r\n" );
				#endregion


				d.Html.Append( "</TABLE>\r\n" );

				d.Html.Append( "<p></p></TD>\r\n</TR>\r\n<TR>\r\n<TD width=\"1%\">&nbsp;</TD>\r\n<TD>\r\n" );
			}

			IEnumerator children = _enumUtil.GetChildren( ctrl ).GetEnumerator();
			while( children.MoveNext() )
			{
				if( !(children.Current is WebForms.LiteralControl) )
				{
					DumpSecurity( children.Current, showDetail, d, level + 1 );
				}
			}


			d.Html.Append( "</TD>\r\n</TR>\r\n</TABLE>\r\n</TD>\r\n</TR>\r\n</TABLE>\r\n" );
		}

		public string DumpHierarchy(ISecureControl control, bool copyToClipboard)
		{
			StringBuilder buffer = new StringBuilder();
			SortedList last = new SortedList();

			last[0] = true;

			DumpHierarchy( control, buffer, 0, last );

			if( copyToClipboard )
			{
				try
				{
					WinForms.Clipboard.SetDataObject( buffer.ToString() );
				}
				catch
				{
					System.Diagnostics.Debug.WriteLine( buffer.ToString() );
				}
			}

			return buffer.ToString();
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
			//ICollection controls = control is WinForms.Control ?
			//    (ICollection)( (WinForms.Control)control ).Controls : (ICollection)( (WebForms.Control)control ).Controls;
			//IEnumerator children = controls.GetEnumerator();
			IEnumerator children = _enumUtil.GetChildren( control ).GetEnumerator();
			while( children.MoveNext() )
			{
				if( !(children.Current is WebForms.LiteralControl) )
				{
					c++;
				}
			}
			children.Reset();
			while( children.MoveNext() )
			{
				if( !(children.Current is WebForms.LiteralControl) )
				{
					last[treeLevel + 1] = i++ == c;

					DumpHierarchy( children.Current, buffer, treeLevel + 1, last );
				}
			}

			//int i = 1;
			//IEnumerator sControls = control.SecureControls.Values.GetEnumerator();
			//while( sControls.MoveNext() )
			//{
			//    last[treeLevel+1] = i++ == control.SecureControls.Count;

			//    DumpHierarchy( (ISecureControl)sControls.Current, buffer, treeLevel+1, last );
			//}
		}
	}


	public class SecurityException : ApplicationException
	{
		public SecurityException() : base() { }
		public SecurityException(string message) : base( message ) { }
		public SecurityException(string message, Exception innerException) : base( message, innerException ) { }
		public SecurityException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base( info, context ) { }
	}
}