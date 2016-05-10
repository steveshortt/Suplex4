using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;

using sg = Suplex.General;
using ss = Suplex.Security;
using System.Data.SqlClient;

namespace Suplex.Forms.ObjectModel.Api
{
	public partial class SuplexDataAccessLayer
	{
		public SuplexStore GetSuplexStore()
		{
			//this.Clear();

			SuplexStore store = new SuplexStore();

			DataSet ds = new DataSet();
			_da.OpenConnection();

			_da.GetDataSet( "splx.splx_api_sel_uielementbyparent_composite",
				new sSortedList( "@UIE_PARENT_ID", Convert.DBNull ), ds, "UIElements", false );
			_da.NameTablesFromCompositeSelect( ref ds );

			_da.GetDataSet( "splx.splx_api_sel_users", null, ds, "Users", false );
			_da.GetDataSet( "splx.splx_api_sel_groups", null, ds, "Groups", false );
			_da.CloseConnection();

			UIElementFactory uieFactory = new UIElementFactory();
			store.UIElements.LoadSuplexObjectTable( ds.Tables["UIElements"], uieFactory, null, null );
			foreach( UIElement uie in store.UIElements )
			{
				UIElement uiElement = uie;
				uieFactory.PopulateSecurityDescriptor( ref uiElement, ds );
			}

			UserFactory userFactory = new UserFactory();
			store.Users.LoadSuplexObjectTable( ds.Tables["Users"], userFactory, null, null );

			GroupFactory groupFactory = new GroupFactory();
			store.Groups.LoadSuplexObjectTable( ds.Tables["Groups"], groupFactory, null, null );

			store.IsDirty = false;

			return store;
		}

		public SuplexStore GetSuplexStore(bool includeValidation, bool includeSecurity)
		{
			SuplexStore store = new SuplexStore();

			if( _da != null && (includeValidation || includeSecurity) )
			{
				_da.OpenConnection();

				DataSet uies = _da.GetDataSet( "splx.splx_api_sel_uie_withchildren_composite",
						new sSortedList( "@SPLX_UI_ELEMENT_ID", Convert.DBNull, "@IncludeSecurity", includeSecurity ), false );
				_da.NameTablesFromCompositeSelect( ref uies );

				DataSet sec = null;
				if( includeSecurity )
				{
					sec = _da.GetDataSet( "splx.splx_api_sel_groupmemb_nested_composite", null, false );
					_da.NameTablesFromCompositeSelect( ref sec );

					_da.GetDataSet( "splx.splx_api_sel_users", null, sec, "Users", false );
					_da.GetDataSet( "splx.splx_api_sel_groups", null, sec, "Groups", false );
				}

				_da.CloseConnection();


				UIElementFactory uieFactory = new UIElementFactory();
				store.UIElements.LoadSuplexObjectTableRecursive( uies.Tables["UIElements"], uieFactory, "uie_parent_id", null, null, null );

				if( includeSecurity )
				{
					Stack<UIElementCollection> uiElementCollections = new Stack<UIElementCollection>();
					uiElementCollections.Push( store.UIElements );

					while( uiElementCollections.Count > 0 )
					{
						UIElementCollection currentCollection = uiElementCollections.Pop();
						foreach( UIElement uie in currentCollection )
						{
							UIElement uiElement = uie;
							uieFactory.PopulateSecurityDescriptor( ref uiElement, uies );

							if( uiElement.UIElements != null && uiElement.UIElements.Count > 0 )
							{
								uiElementCollections.Push( uiElement.UIElements );
							}
						}
					}

					UserFactory userFactory = new UserFactory();
					store.Users.LoadSuplexObjectTable( sec.Tables["Users"], userFactory, null, null );

					GroupFactory groupFactory = new GroupFactory();
					store.Groups.LoadSuplexObjectTable( sec.Tables["Groups"], groupFactory, null, null );
				}
			}

			store.IsDirty = false;

			return store;
		}

		public void UpsertWholeStore(SuplexStore importStore, bool includeValidation, bool includeSecurity)
		{
			if( _da != null )
			{
				_da.OpenConnection();
				SqlTransaction tr = _da.Connection.BeginTransaction();

				try
				{
					if( includeSecurity )
					{
						foreach( SecurityPrincipalBase sp in importStore.SecurityPrincipals )
						{
							if( sp.IsUserObject )
							{
								this.UpsertUserForImport( (User)sp, null, ref tr );
							}
							else
							{
								this.UpsertGroupForImport( (Group)sp, null, ref tr );

								IEnumerable<GroupMembershipItem> gm = importStore.GroupMembership.GetByGroup( (Group)sp, false );
								foreach( GroupMembershipItem gmi in gm )
								{
									this.UpsertGroupMembershipForImport( sp.Id, gmi.Member, ref tr );
								}
							}
						}
					}

					if( includeValidation )
					{
						foreach( UIElement uie in importStore.UIElements )
						{
							this.UpsertUIElementForImport( uie, ref tr, includeSecurity );
						}
					}

					tr.Commit();
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
			}
		}
	}
}