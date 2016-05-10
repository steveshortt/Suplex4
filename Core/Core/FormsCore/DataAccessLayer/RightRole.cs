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
		//public RightRole GetRightRoleById(string id)
		//{
		//    return new RightRole();
		//}

		public void UpsertRightRole(RightRole rr, Guid uieOrRuleId, ref SqlTransaction tr)
		{
			SortedList inparms = this.GetRightRoleParms( rr, uieOrRuleId );

			int idAsInt = -1;
			Int32.TryParse( rr.Id.ToString(), out idAsInt );

			SqlParameter id = new SqlParameter( "@SPLX_RIGHT_ROLE_ID", SqlDbType.Int );
			id.Value = idAsInt;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_RIGHT_ROLE_ID", id );

			_da.ExecuteSP( "splx.splx_api_upsert_rightrole", inparms, ref outparms, false, tr );
			rr.Id = (int)id.Value;

			rr.IsDirty = false;
		}

		internal virtual SortedList GetRightRoleParms(RightRole rr, Guid uieOrRuleId)
		{
			sSortedList s = new sSortedList( "@SPLX_UI_ELEMENT_RULE_ID", uieOrRuleId );
			s.Add( "@RR_UIE_UNIQUE_NAME", rr.ControlUniqueName );
			s.Add( "@RR_ACE_TYPE", rr.AceType.ToString() );
			s.Add( "@RR_RIGHT_NAME", rr.RightName );
			s.Add( "@RR_UI_RIGHT", rr.UIRight.ToString() );
			s.Add( "@RR_ROLE_TYPE", rr.RightRoleType.ToString() );

			return s;
		}

		internal void DeleteRightRoleById(string id)
		{
			SortedList inparms = new sSortedList( "@SPLX_RIGHT_ROLE_ID", id );
			_da.ExecuteSP( "splx.splx_api_del_rightrole", inparms );
		}
	}

	public class RightRoleFactory : ISuplexObjectFactory<RightRole>
	{
		public ISuplexObject CreateSuplexObjectBase(DataRow r)
		{
			return this.CreateObject( r );
		}

		public RightRole CreateObject(DataRow r)
		{
			RightRole rr = new RightRole();
			this.CreateObject( r, ref rr );
			return rr;
		}

		public void CreateObject(DataRow r, ref RightRole rr)
		{
			rr.Id = (int)r["SPLX_RIGHT_ROLE_ID"];
			rr.AceType = sg.MiscUtils.ParseEnum<ss.AceType>( r["RR_ACE_TYPE"].ToString() );
			rr.RightName = r["RR_RIGHT_NAME"].ToString();
			rr.ControlUniqueName = r["RR_UIE_UNIQUE_NAME"].ToString();
			rr.UIRight = sg.MiscUtils.ParseEnum<ss.UIRight>( r["RR_UI_RIGHT"].ToString() );
			rr.RightRoleType = sg.MiscUtils.ParseEnum<sf.RightRoleType>( r["RR_ROLE_TYPE"].ToString() );

			rr.IsDirty = false;
		}
	}
}