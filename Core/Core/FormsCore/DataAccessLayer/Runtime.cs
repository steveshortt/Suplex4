using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;

using sg = Suplex.General;
using ss = Suplex.Security;

namespace Suplex.Forms.ObjectModel.Api
{
	public partial class SuplexDataAccessLayer
	{
		public string GetSecurityCacheString(string uniqueName, ss.Standard.User user, ExternalGroupInfo egi)
		{
			SecurityBuilder sb = new SecurityBuilder();
			DataSet ds = sb.CreateSecurityCache( _da, uniqueName, user, egi );

			MemoryStream ms = new MemoryStream();
			ds.WriteXml( ms, XmlWriteMode.IgnoreSchema );
			ms.Position = 0;

			return ASCIIEncoding.UTF8.GetString( ms.ToArray() );
		}

		public DataSet GetSecurityCache(string uniqueName, ss.Standard.User user, ExternalGroupInfo egi)
		{
			SecurityBuilder sb = new SecurityBuilder();
			return sb.CreateSecurityCache( _da, uniqueName, user, egi );
		}

		public SuplexStore GetSecurityStore(string uniqueName, ss.Standard.User user, ExternalGroupInfo egi)
		{
			//DataSet ds = this.GetSecurityCache( uniqueName, user, egi );


			DataSet ds = _da.GetDataSet( "splx.splx_api_sel_uie_withchildren_composite",
				new sSortedList( "@UIE_UNIQUE_NAME", uniqueName ) );
			_da.NameTablesFromCompositeSelect( ref ds );

			SuplexStore store = new SuplexStore();
			UIElementFactory uieFactory = new UIElementFactory();

			store.UIElements.LoadSuplexObjectTable( ds.Tables["UIElements"], uieFactory, null, null );
			foreach( UIElement uie in store.UIElements )
			{
				UIElement uiElement = uie;
				uieFactory.PopulateSecurityDescriptor( ref uiElement, ds );
			}

			store.IsDirty = false;

			return store;
		}
	}
}