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
		#region delete these
		public SecurityDescriptor GetSecurityDescriptor(UIElement uie)
		{
			SecurityDescriptor s = new SecurityDescriptor( uie );
			s.Dacl = this.GetAces();
			s.Sacl = this.GetAuditAces();

			RightRole r = new RightRole()
			{
				Id = 1,
				Name = "rr",
				AceType = ss.AceType.Native,
				RightName = "blah",
				RightRoleType = RightRoleType.Else,
				UIRight = ss.UIRight.Enabled
			};
			RightRoleCollection rr = new RightRoleCollection();
			rr.Add( r );
			s.RightRoles = rr;

			return s;
		}
		public UIElement GetSuplex()
		{
			ValidationRule v = new ValidationRule()
			{
				Name = "vr",
				CompareDataType = TypeCode.String,
				CompareValue1 = "one",
				CompareValue2 = "two",
				ErrorControl = "errControl",
				ErrorMessage = "errMessage",
				EventBinding = ControlEvents.Validating,
				ExpressionType1 = ExpressionType.Calculation,
				ExpressionType2 = ExpressionType.Method,
				FailParent = true,
				Id = Guid.NewGuid(),
				LogicRuleType = LogicRuleType.ValidationIf,
				Operator = ComparisonOperator.LessThan,
				SortOrder = 0,
				ValueType1 = ComparisonValueType.Singular,
				ValueType2 = ComparisonValueType.Empty
			};

			ValidationRule r = new ValidationRule()
			{
				Name = "vx",
				CompareDataType = TypeCode.String,
				CompareValue1 = "one",
				CompareValue2 = "two",
				ErrorControl = "errControl",
				ErrorMessage = "errMessage",
				EventBinding = ControlEvents.Validating,
				ExpressionType1 = ExpressionType.Calculation,
				ExpressionType2 = ExpressionType.Method,
				FailParent = true,
				Id = Guid.NewGuid(),
				LogicRuleType = LogicRuleType.ValidationIf,
				Operator = ComparisonOperator.LessThan,
				SortOrder = 0,
				ValueType1 = ComparisonValueType.Singular,
				ValueType2 = ComparisonValueType.Empty
			};

			RightRoleRule rrr = new RightRoleRule()
			{
				Name = "vx",
				CompareDataType = TypeCode.String,
				CompareValue1 = "oneXone",
				CompareValue2 = "twoYtwo",
				ErrorMessage = "errMessage",
				ExpressionType1 = ExpressionType.Calculation,
				ExpressionType2 = ExpressionType.Method,
				FailParent = true,
				Id = Guid.NewGuid(),
				LogicRuleType = LogicRuleType.ValidationIf,
				Operator = ComparisonOperator.LessThan,
				SortOrder = 0,
				ValueType1 = ComparisonValueType.Singular,
				ValueType2 = ComparisonValueType.Empty
			};

			ValidationRuleCollection vrc = new ValidationRuleCollection();
			vrc.Add( v );
			v.ValidationRules.Add( r );

			FillMap f = new FillMap()
			{
				EventBinding = ControlEvents.Upload,
				Expression = "meow",
				ExpressionType = ExpressionType.Script,
				FillMapType = FillMapType.FillMapIf,
				Id = 3,
				Name = "fff",
				SortOrder = 2
			};
			DataBinding d = new DataBinding()
			{
				ControlName = "x",
				DataMember = "foo",
				Id = 4,
				OverrideValue = false,
				PropertyName = "something"
			};
			f.DataBindings.Add( d );

			UIElement uie = new UIElement();
			uie.ValidationRules.Add( v );
			uie.FillMaps.Add( f );
			uie.SecurityDescriptor = this.GetSecurityDescriptor( uie );
			uie.SecurityDescriptor.RightRoleRules.Add( rrr );

			return uie;
		}

		public AceCollectionEx<UIAceDefault> GetAces()
		{
			AceCollectionEx<UIAceDefault> list = new AceCollectionEx<UIAceDefault>();
			list.Add( new RecordAce()
			{
				Allowed = true,
				Id = 999,
				Inherit = true,
				iRight = ss.RecordRight.FullControl,
				SecurityPrincipalId = "xx"
			} );
			list.Add( new FileSystemAce()
			{
				Allowed = true,
				Id = 888,
				Inherit = true,
				iRight = ss.FileSystemRight.ChangePermissions,
				SecurityPrincipalId = "yy"
			} );
			return list;
		}

		public AuditAceCollectionEx<UIAuditAceDefault> GetAuditAces()
		{
			AuditAceCollectionEx<UIAuditAceDefault> list = new AuditAceCollectionEx<UIAuditAceDefault>();
			list.Add( new RecordAuditAce()
			{
				Allowed = true,
				Denied = true,
				Id = 909,
				Inherit = true,
				iRight = ss.RecordRight.FullControl,
				SecurityPrincipalId = "xx0"
			} );
			list.Add( new FileSystemAuditAce()
			{
				Allowed = true,
				Denied = false,
				Id = 808,
				Inherit = true,
				iRight = ss.FileSystemRight.ChangePermissions,
				SecurityPrincipalId = "yy0"
			} );
			return list;
		}
		#endregion


		public void UpsertAce(AccessControlEntryBase ace, Guid uielementId, ref SqlTransaction tr)
		{
			SortedList inparms = this.GetAceParms( ace, uielementId );

			int idAsInt = -1;
			Int32.TryParse( ace.Id.ToString(), out idAsInt );

			SqlParameter id = new SqlParameter( "@SPLX_ACE_ID", SqlDbType.Int );
			id.Value = idAsInt;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_ACE_ID", id );

			_da.ExecuteSP( "splx.splx_api_upsert_ace", inparms, ref outparms, false, tr );
			ace.Id = (int)id.Value;

			ace.IsDirty = false;
		}

		protected virtual SortedList GetAceParms(AccessControlEntryBase ace, Guid uieId)
		{
			sSortedList s = new sSortedList( "@ACE_TRUSTEE_USER_GROUP_ID", ace.SecurityPrincipalId );
			s.Add( "@SPLX_UI_ELEMENT_ID", uieId );
			s.Add( "@ACE_ACCESS_MASK", (int)ace.Right );
			s.Add( "@ACE_ACCESS_TYPE1", ace.Allowed );
			s.Add( "@ACE_INHERIT", ace.Inherit );
			s.Add( "@ACE_TYPE", string.Format( "{0}Ace", ace.AceType.ToString() ) );

			if( ace is AccessControlEntryAuditBase )
			{
				s.Add( "@ACE_ACCESS_TYPE2", ((AccessControlEntryAuditBase)ace).Denied );
				s["@ACE_TYPE"] = string.Format( "{0}AuditAce", ace.AceType.ToString() );
			}

			return s;
		}

		internal virtual void DeleteAceById(string id)
		{
			SortedList inparms = new sSortedList( "@SPLX_ACE_ID", id );
			_da.ExecuteSP( "splx.splx_api_del_ace", inparms );
		}
	}

	public class AceFactory : ISuplexObjectFactory<AccessControlEntryBase>
	{
		public ISuplexObject CreateSuplexObjectBase(DataRow r)
		{
			return this.CreateObject( r );
		}

		public AccessControlEntryBase CreateObject(DataRow r)
		{
			AccessControlEntryBase ace = null;
			this.CreateObject( r, ref ace );
			return ace;
		}

		public void CreateObject(DataRow r, ref AccessControlEntryBase ace)
		{
			ss.AceType aceType = ss.AceType.None;
			aceType = sg.MiscUtils.ParseEnum<ss.AceType>( r["ACE_TYPE"].ToString().Replace( "Ace", string.Empty ) );
			switch( aceType )
			{
				case ss.AceType.UI:
				{
					ace = new UIAce();
					((UIAce)ace).iRight = sg.MiscUtils.ParseEnum<ss.UIRight>( (int)r["ACE_ACCESS_MASK"] );
					break;
				}
				case ss.AceType.Record:
				{
					ace = new RecordAce();
					((RecordAce)ace).iRight = sg.MiscUtils.ParseEnum<ss.RecordRight>( (int)r["ACE_ACCESS_MASK"] );
					break;
				}
				case ss.AceType.FileSystem:
				{
					ace = new FileSystemAce();
					((FileSystemAce)ace).iRight = sg.MiscUtils.ParseEnum<ss.FileSystemRight>( (int)r["ACE_ACCESS_MASK"] );
					break;
				}
				case ss.AceType.Synchronization:
				{
					ace = new SynchronizationAce();
					((SynchronizationAce)ace).iRight = sg.MiscUtils.ParseEnum<ss.SynchronizationRight>( (int)r["ACE_ACCESS_MASK"] );
					break;
				}
			}

			ace.Id = (int)r["SPLX_ACE_ID"];
			ace.Allowed = (bool)r["ACE_ACCESS_TYPE1"];
			ace.Inherit = (bool)r["ACE_INHERIT"];
			ace.SecurityPrincipalId = r["ACE_TRUSTEE_USER_GROUP_ID"].ToString();
			ace.IsDirty = false;
		}
	}

	public class AuditAceFactory : ISuplexObjectFactory<AccessControlEntryAuditBase>
	{
		public ISuplexObject CreateSuplexObjectBase(DataRow r)
		{
			return this.CreateObject( r );
		}

		public AccessControlEntryAuditBase CreateObject(DataRow r)
		{
			AccessControlEntryAuditBase ace = null;
			this.CreateObject( r, ref ace );
			return ace;
		}

		public void CreateObject(DataRow r, ref AccessControlEntryAuditBase ace)
		{
			ss.AceType aceType = ss.AceType.None;
			aceType = sg.MiscUtils.ParseEnum<ss.AceType>( r["ACE_TYPE"].ToString().Replace( "AuditAce", string.Empty ) );
			switch( aceType )
			{
				case ss.AceType.UI:
				{
					ace = new UIAuditAce();
					((UIAuditAce)ace).iRight = sg.MiscUtils.ParseEnum<ss.UIRight>( (int)r["ACE_ACCESS_MASK"] );
					break;
				}
				case ss.AceType.Record:
				{
					ace = new RecordAuditAce();
					((RecordAuditAce)ace).iRight = sg.MiscUtils.ParseEnum<ss.RecordRight>( (int)r["ACE_ACCESS_MASK"] );
					break;
				}
				case ss.AceType.FileSystem:
				{
					ace = new FileSystemAuditAce();
					((FileSystemAuditAce)ace).iRight = sg.MiscUtils.ParseEnum<ss.FileSystemRight>( (int)r["ACE_ACCESS_MASK"] );
					break;
				}
				case ss.AceType.Synchronization:
				{
					ace = new SynchronizationAuditAce();
					((SynchronizationAuditAce)ace).iRight = sg.MiscUtils.ParseEnum<ss.SynchronizationRight>( (int)r["ACE_ACCESS_MASK"] );
					break;
				}
			}

			ace.Id = (int)r["SPLX_ACE_ID"];
			ace.Allowed = (bool)r["ACE_ACCESS_TYPE1"];
			ace.Denied = (bool)r["ACE_ACCESS_TYPE2"];
			ace.Inherit = (bool)r["ACE_INHERIT"];
			ace.SecurityPrincipalId = r["ACE_TRUSTEE_USER_GROUP_ID"].ToString();
			ace.IsDirty = false;
		}
	}
}