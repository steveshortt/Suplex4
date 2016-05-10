using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using sf = Suplex.Forms;
using sg = Suplex.General;
using ss = Suplex.Security;

namespace Suplex.Forms.ObjectModel.Api
{
	public partial class SuplexDataAccessLayer
	{
		#region upsert
		private void UpsertGroupMembershipForImport(string groupId, SecurityPrincipalBase member, ref SqlTransaction tr)
		{
			if( member.IsUserObject )
			{
				try
				{
					SortedList parms = new sSortedList( "@SPLX_USER_ID", member.Id );
					parms.Add( "@SPLX_GROUP_ID", groupId );
					parms.Add( "@CURR_USER_ID", Guid.Empty );
					_da.ExecuteSP( "splx.splx_api_ins_groupmemb", parms, false, tr );
				}
				catch( SqlException ex )
				{
					if( !(ex.Number == 2601) ) //2601 is UniqueIndex violation (throw away the dups)
					{
						throw ex;
					}
				}
			}
			else
			{
				try
				{
					SortedList parms = new sSortedList( "@CHILD_GROUP_ID", member.Id );
					parms.Add( "@PARENT_GROUP_ID", groupId );
					parms.Add( "@CURR_USER_ID", Guid.Empty );
					_da.ExecuteSP( "splx.splx_api_ins_groupnest", parms, false, tr );
				}
				catch( SqlException ex )
				{
					if( !(ex.Number == 50000 && ex.Class == 16) && ex.Number != 2601 && ex.Number != 2627 ) //50000:16 is parent/child relationship error (ancestor/descendant error)
					{
						throw ex;
					}
				}
			}
		}
		#endregion
	}
}