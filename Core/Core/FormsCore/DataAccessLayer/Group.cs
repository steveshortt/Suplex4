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

using settings = Suplex.Properties.Settings;

namespace Suplex.Forms.ObjectModel.Api
{
	public partial class SuplexDataAccessLayer
	{
		#region select
		public Group GetGroupById(string id)
		{
			DataSet ds = _da.GetDataSet( "splx.splx_dal_sel_groupbyid",
				new sSortedList( "@SPLX_GROUP_ID", id ) );

			if( ds.Tables[0].Rows.Count > 0 )
			{
				GroupFactory factory = new GroupFactory();
				return factory.CreateObject( ds.Tables[0].Rows[0] );
			}
			else
			{
				throw new RowNotInTableException( string.Format( "Unable to fetch Group '{0}' from the data store.", id ) );
			}
		}

		public MembershipList<SecurityPrincipalBase> GetGroupMembers(string groupId)
		{
			MembershipList<SecurityPrincipalBase> list = new MembershipList<SecurityPrincipalBase>();
			GroupFactory groupFactory = new GroupFactory();
			UserFactory userFactory = new UserFactory();

			DataSet groupMembs = new DataSet();
			DataSet nestedMembs = new DataSet();
			_da.OpenConnection();
			_da.GetDataSet( "splx.splx_api_sel_groupmembbygroup", new sSortedList( "@SPLX_GROUP_ID", groupId ), groupMembs, "groupMembs", false );
			_da.GetDataSet( "splx.splx_api_sel_groupnestmembbygroup", new sSortedList( "@SPLX_GROUP_ID", groupId ), nestedMembs, "nestedMembs", false );
			_da.CloseConnection();
			_da.NameTablesFromCompositeSelect( ref groupMembs );
			_da.NameTablesFromCompositeSelect( ref nestedMembs );

			list.MemberList.LoadSuplexObjectTable( groupMembs.Tables["GroupMembership"], userFactory, null, null );
			list.NonMemberList.LoadSuplexObjectTable( groupMembs.Tables["GroupNonMembership"], userFactory, null, null );

			list.MemberList.LoadSuplexObjectTable( nestedMembs.Tables["GroupMembership"], groupFactory, null, null );
			list.NonMemberList.LoadSuplexObjectTable( nestedMembs.Tables["GroupNonMembership"], groupFactory, null, null );

			return list;
		}

		public List<Group> GetGroupHierarchy(string groupId)
		{
			List<Group> groups = new List<Group>();
			Stack<Group> parentGroups = new Stack<Group>();


			DataSet ds = _da.GetDataSet( "splx.splx_api_sel_grouphiermembbygroup",
				new sSortedList( "@SPLX_GROUP_ID", groupId ) );

			DataTable groupsTable = ds.Tables[0];
			GroupFactory factory = new GroupFactory();
			factory.ColumnPrefix = "PARENT_";

			DataRow[] parents = groupsTable.Select( string.Format( "CHILD_SPLX_GROUP_ID = '{0}'", groupId ) );
			if( parents.Length > 0 )
			{
				Stack<DataRow> parentRows = new Stack<DataRow>();
				foreach( DataRow r in parents )
				{
					parentRows.Push( r );
				}

				//walk up the table to find the ultimate parents
				while( parentRows.Count > 0 )
				{
					DataRow p = parentRows.Pop();
					DataRow[] ascendants = groupsTable.Select( string.Format( "CHILD_SPLX_GROUP_ID = '{0}'", p["PARENT_SPLX_GROUP_ID"] ) );
					if( ascendants.Length > 0 )
					{
						foreach( DataRow r in ascendants )
						{
							parentRows.Push( r );
						}
					}
					else
					{
						Group g = factory.CreateObject( p );
						if( !groups.Contains( g, factory ) )
						{
							parentGroups.Push( g );
							groups.Add( g );
						}
					}
				}
			}
			else
			{
				//this group has no ascendants (it's the ultimate parent)
				DataRow[] thisGroup = groupsTable.Select( string.Format( "PARENT_SPLX_GROUP_ID = '{0}'", groupId ) );
				if( thisGroup.Length > 0 )
				{
					Group g = factory.CreateObject( thisGroup[0] );
					parentGroups.Push( g );
					groups.Add( g );
				}
			}


			//recurse down the parent groups to build the tree
			factory.ColumnPrefix = "CHILD_";
			while( parentGroups.Count > 0 )
			{
				Group g = parentGroups.Pop();
				DataRow[] descendants = groupsTable.Select( string.Format( "PARENT_SPLX_GROUP_ID = '{0}'", g.Id ) );
				foreach( DataRow r in descendants )
				{
					Group ch = factory.CreateObject( r );
					if( !g.Groups.Contains( ch, factory ) )
					{
						g.Groups.Add( ch );
						parentGroups.Push( ch );
					}
				}
			}

			return groups;
		}

		public List<Group> GetGroupList()
		{
			List<Group> groups = new List<Group>();
			Stack<Group> parentGroups = new Stack<Group>();

			DataSet ds = _da.GetDataSet( "splx.splx_api_sel_groups", null );

			DataTable groupsTable = ds.Tables[0];
			GroupFactory factory = new GroupFactory();
			//factory.ColumnPrefix = "PARENT_";

			DataRow[] parents = groupsTable.Select(); //"PARENT_SPLX_GROUP_ID IS NULL"
			if( parents.Length > 0 )
			{
				Stack<DataRow> parentRows = new Stack<DataRow>();
				foreach( DataRow r in parents )
				{
					Group g = factory.CreateObject( r );
					parentGroups.Push( g );
					groups.Add( g );
				}
			}

			////recurse down the parent groups to build the tree
			//factory.ColumnPrefix = "CHILD_";
			//while( parentGroups.Count > 0 )
			//{
			//    Group g = parentGroups.Pop();
			//    DataRow[] descendants = groupsTable.Select( string.Format( "PARENT_SPLX_GROUP_ID = '{0}'", g.Id ) );
			//    foreach( DataRow r in descendants )
			//    {
			//        Group ch = factory.CreateObject( r );
			//        if( !g.Groups.Contains( ch, factory ) )
			//        {
			//            g.Groups.Add( ch );
			//            parentGroups.Push( ch );
			//        }
			//    }
			//}

			return groups;
		}
		#endregion

		#region upsert
		public Group UpsertGroup(Group group, List<SecurityPrincipalBase> addedGroupMembership, List<SecurityPrincipalBase> removedGroupMembership)
		{
			SortedList inparms = this.GetGroupInputParms( group );
			SortedList outparms = this.GetGroupOutputParms( group );
			SqlParameter id = (SqlParameter)outparms["@SPLX_GROUP_ID"];
			SqlParameter name = (SqlParameter)outparms["@GROUP_NAME"];

			_da.OpenConnection();

			_da.ExecuteSP( "splx.splx_api_upsert_group", inparms, ref outparms, false );
			group.Id = id.Value.ToString();
			group.Name = name.Value.ToString();

			this.IterateGroupAddedGroupMembership( group.Id, addedGroupMembership );
			this.IterateGroupRemovedGroupMembership( group.Id, removedGroupMembership );

			_da.CloseConnection();

			group.IsDirty = false;

			return group;
		}

		public void UpsertGroupForImport(Group group, List<SecurityPrincipalBase> addedGroupMembership, ref SqlTransaction tr)
		{
			SortedList inparms = this.GetGroupInputParms( group );
			SortedList outparms = this.GetGroupOutputParms( group );
			SqlParameter id = (SqlParameter)outparms["@SPLX_GROUP_ID"];
			SqlParameter name = (SqlParameter)outparms["@GROUP_NAME"];

			_da.ExecuteSP( "splx.splx_api_upsert_group", inparms, ref outparms, false, tr );
			group.Id = id.Value.ToString();
			group.Name = name.Value.ToString();

			if( addedGroupMembership != null )
			{
				foreach( SecurityPrincipalBase member in addedGroupMembership )
				{
					if( member.IsUserObject )
					{
						try
						{
							SortedList parms = new sSortedList( "@SPLX_USER_ID", member.Id );
							parms.Add( "@SPLX_GROUP_ID", group.Id );
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
							parms.Add( "@PARENT_GROUP_ID", group.Id );
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
			}
		}

		private void IterateGroupAddedGroupMembership(string groupId, List<SecurityPrincipalBase> addedGroupMembership)
		{
			if( addedGroupMembership != null )
			{
				foreach( SecurityPrincipalBase member in addedGroupMembership )
				{
					if( member.IsUserObject )
					{
						try
						{
							SortedList parms = new sSortedList( "@SPLX_USER_ID", member.Id );
							parms.Add( "@SPLX_GROUP_ID", groupId );
							parms.Add( "@CURR_USER_ID", Guid.Empty );
							_da.ExecuteSP( "splx.splx_api_ins_groupmemb", parms, false );
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
							_da.ExecuteSP( "splx.splx_api_ins_groupnest", parms, false );
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
			}
		}

		private void IterateGroupRemovedGroupMembership(string groupId, List<SecurityPrincipalBase> removedGroupMembership)
		{
			if( removedGroupMembership != null )
			{
				foreach( SecurityPrincipalBase member in removedGroupMembership )
				{
					if( member.IsUserObject )
					{
						SortedList parms = new sSortedList( "@SPLX_USER_ID", member.Id );
						parms.Add( "@SPLX_GROUP_ID", groupId );
						parms.Add( "@CURR_USER_ID", Guid.Empty );
						_da.ExecuteSP( "splx.splx_api_del_groupmemb", parms, false );
					}
					else
					{
						SortedList parms = new sSortedList( "@CHILD_GROUP_ID", member.Id );
						parms.Add( "@PARENT_GROUP_ID", groupId );
						parms.Add( "@CURR_USER_ID", Guid.Empty );
						_da.ExecuteSP( "splx.splx_api_del_groupnest", parms, false );
					}
				}
			}
		}

		private SortedList GetGroupInputParms(Group group)
		{
			int _maskSize = settings.Default.RlsMaskSize;

			sSortedList s = new sSortedList();	//"@GROUP_NAME", group.Name
			s.Add( "@GROUP_DESC", group.Description );
			s.Add( "@GROUP_LOCAL", group.IsLocal );
			s.Add( "@GROUP_ENABLED", group.IsEnabled );

			byte[] mask = new byte[_maskSize / 8];	//8 bits per byte
			group.Mask.CopyTo( mask, 0 );
			s.Add( "@GROUP_MASK", mask );

			return s;
		}

		private SortedList GetGroupOutputParms(Group group)
		{
			SqlParameter id = new SqlParameter( "@SPLX_GROUP_ID", SqlDbType.UniqueIdentifier );
			id.Value = new Guid( group.Id );
			id.Direction = ParameterDirection.InputOutput;
			SortedList s = new sSortedList( "@SPLX_GROUP_ID", id );

			SqlParameter name = new SqlParameter( "@GROUP_NAME", SqlDbType.VarChar );
			name.Value = group.Name;
			name.Direction = ParameterDirection.InputOutput;
			s.Add( "@GROUP_NAME", name );

			return s;
		}
		#endregion

		#region delete
		public void DeleteGroupById(string id)
		{
			_da.ExecuteSP( "splx.splx_api_del_group", new sSortedList( "@SPLX_GROUP_ID", id ) );
		}
		#endregion
	}

	public class GroupFactory : ISuplexObjectFactory<Group>, IEqualityComparer<Group>
	{
		public ISuplexObject CreateSuplexObjectBase(DataRow r)
		{
			return this.CreateObject( r );
		}

		public string ColumnPrefix { get; set; }

		public Group CreateObject(DataRow r)
		{
			Group g = new Group();
			this.CreateObject( r, ref g );
			return g;
		}

		public void CreateObject(DataRow r, ref Group g)
		{
			bool isFull = r.Table.Columns.Contains( string.Format( "{0}GROUP_DESC", this.ColumnPrefix ) );

			g.Id = r[string.Format( "{0}SPLX_GROUP_ID", this.ColumnPrefix )].ToString();
			g.Name = (string)r[string.Format( "{0}GROUP_NAME", this.ColumnPrefix )];
			if( isFull ) { g.Description = r[string.Format( "{0}GROUP_DESC", this.ColumnPrefix )].ToString(); }
			if( isFull ) { g.IsLocal = (bool)r[string.Format( "{0}GROUP_LOCAL", this.ColumnPrefix )]; }
			g.IsEnabled = (bool)r[string.Format( "{0}GROUP_ENABLED", this.ColumnPrefix )];
			if( r.Table.Columns.Contains( string.Format( "{0}GROUP_MASK", this.ColumnPrefix ) ) )
			{
				g.Mask = new BitArray( (byte[])r[string.Format( "{0}GROUP_MASK", this.ColumnPrefix )] );
			}
			g.IsDirty = false;
		}

		#region IEqualityComparer<Group> Members

		public bool Equals(Group x, Group y)
		{
			return x.Id.Equals( y.Id );
		}

		public int GetHashCode(Group obj)
		{
			return obj.GetHashCode();
		}

		#endregion
	}
}