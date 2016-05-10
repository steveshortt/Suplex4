using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Suplex.Forms.ObjectModel.Api
{
	public static class ObjectModelExtensions
	{
		public static void LoadSuplexObjectTable(this IList list, DataTable dataTableToLoad, ISuplexObjectFactory factory, string optionalFilterExpression, string optionalSortColumnName)
		{
			string sortExpression = !string.IsNullOrEmpty( optionalSortColumnName ) ? string.Format( "{0} ASC", optionalSortColumnName ) : string.Empty;

			DataRow[] rows = null;
			if( string.IsNullOrEmpty( optionalFilterExpression ) )
			{
				rows = dataTableToLoad.Select( string.Empty, sortExpression );
			}
			else
			{
				rows = dataTableToLoad.Select( optionalFilterExpression, sortExpression );
			}

			foreach( DataRow r in rows )
			{
				ISuplexObject item = factory.CreateSuplexObjectBase( r );

				if( list is ISuplexObjectList )
				{
					//this facilitates a refresh from the database
					((ISuplexObjectList)list).AddOrSynchronize( item );
				}
				else
				{
					list.Add( item );
				}
			}
		}

		public static void LoadSuplexObjectTableRecursive(this IList list, DataTable dataTableToLoad, ISuplexObjectFactory factory, string parentColumnName, string optionalSortColumnName, string optionalStartId, string optionalStartIdColumnName)
		{
			string sortExpression = !string.IsNullOrEmpty( optionalSortColumnName ) ? string.Format( "{0} ASC", optionalSortColumnName ) : string.Empty;

			DataRow[] topNodes = dataTableToLoad.Select( string.Format( "{0} IS NULL", parentColumnName ), sortExpression );
			if( topNodes.Length == 0 && optionalStartId != null )
			{
				topNodes = dataTableToLoad.Select( string.Format( "{0} = {1}", optionalStartIdColumnName, optionalStartId ), sortExpression );
			}
			foreach( DataRow r in topNodes )
			{
				ISuplexObject item = factory.CreateSuplexObjectBase( r );
				list.Add( item );

				if( item is IObjectCollectionHost )
				{
					RecursRows( item, dataTableToLoad, factory, parentColumnName, sortExpression );
				}
			}
		}

		private static void RecursRows(ISuplexObject parentItem, DataTable dataTableToLoad, ISuplexObjectFactory factory, string parentColumnName, string sortExpression)
		{
			DataRow[] childNodes = dataTableToLoad.Select( string.Format( "{0} = '{1}'", parentColumnName, parentItem.ObjectId ), sortExpression );
			if( childNodes.Length > 0 )
			{
				foreach( DataRow r in childNodes )
				{
					ISuplexObject child = factory.CreateSuplexObjectBase( r );
					if( child is IObjectModel )
					{
						((IObjectCollectionHost)parentItem).AddChildObject( (IObjectModel)child );
					}

					if( child is IObjectCollectionHost )
					{
						RecursRows( child, dataTableToLoad, factory, parentColumnName, sortExpression );
					}
				}
			}
		}
	}
}