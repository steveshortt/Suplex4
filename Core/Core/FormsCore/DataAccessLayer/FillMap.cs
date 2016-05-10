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
		#region get
		public FillMap GetFillMapById(string id)
		{
			DataSet ds = _da.GetDataSet( "splx.splx_api_sel_fmbyid_composite",
				new sSortedList( "@SPLX_FILLMAP_EXPRESSION_ID", id ) );
			_da.NameTablesFromCompositeSelect( ref ds );

			if( ds.Tables["FillMaps"].Rows.Count > 0 )
			{
				FillMapFactory factory = new FillMapFactory();
				factory.DataBindingsTable = ds.Tables["Databindings"];
				return factory.CreateObject( ds.Tables["FillMaps"].Rows[0] );
			}
			else
			{
				throw new RowNotInTableException( string.Format( "Unable to fetch FillMap '{0}' from the data store.", id ) );
			}
		}
		#endregion

		#region upsert
		public FillMap UpsertFillMap(FillMap fm)
		{
			List<long> _deleteIds = null;
			SortedList inparms = this.GetFillMapParms( fm );

			int idAsInt = -1;
			Int32.TryParse( fm.Id.ToString(), out idAsInt );

			SqlParameter id = new SqlParameter( "@SPLX_FILLMAP_EXPRESSION_ID", SqlDbType.Int );
			id.Value = idAsInt;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_FILLMAP_EXPRESSION_ID", id );

			_da.OpenConnection();
			SqlTransaction tr = _da.Connection.BeginTransaction();
			try
			{
				_da.ExecuteSP( "splx.splx_api_upsert_fme", inparms, ref outparms, false, tr );
				fm.Id = (int)id.Value;

				StringBuilder dbIds = new StringBuilder();
				foreach( DataBinding db in fm.DataBindings )
				{
					if( db.IsDirty )
					{
						dbIds.AppendFormat( "{0},", db.Id );
						this.UpsertDataBinding( db, fm.Id, ref tr );
						db.IsDirty = false;
					}
				}
				if( _deleteIds != null && _deleteIds.Count > 0 )
				{
					foreach( int fmbId in _deleteIds )
					{
						_da.ExecuteSP( "splx.splx_api_del_fmb", new sSortedList( "@SPLX_FILLMAP_DATABINDING_ID", fmbId ), false, tr );
					}
				}
				//8/8/2010 not sure what this else block is for, and the sp doesn't exist...
				else
				{
					string idList = dbIds.ToString();
					if( !string.IsNullOrEmpty( idList ) )
					{
						idList = idList.Substring( 0, idList.Length - 1 );
						//_da.ExecuteSP( "splx.splx_api_del_fmb_byidlist", new sSortedList( "@SPLX_FILLMAP_DATABINDING_ID_LIST", idList ), false, tr );
					}
				}
				_deleteIds = null;

				tr.Commit();

				fm.IsDirty = false;
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

			return fm;
		}

		public void UpsertFillMapForImport(FillMap fm, ref SqlTransaction tr)
		{
			SortedList inparms = this.GetFillMapParms( fm );

			int idAsInt = -1;
			Int32.TryParse( fm.Id.ToString(), out idAsInt );

			SqlParameter id = new SqlParameter( "@SPLX_FILLMAP_EXPRESSION_ID", SqlDbType.Int );
			id.Value = idAsInt;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_FILLMAP_EXPRESSION_ID", id );

			_da.ExecuteSP( "splx.splx_api_upsert_fme", inparms, ref outparms, false, tr );
			fm.Id = (int)id.Value;

			foreach( DataBinding db in fm.DataBindings )
			{
				this.UpsertDataBinding( db, fm.Id, ref tr );
			}
		}

		private SortedList GetFillMapParms(FillMap fm)
		{
			sSortedList s = new sSortedList( "@FME_NAME", fm.Name );

			s.Add( "@FME_EVENT_BINDING", fm.EventBinding.ToString() );

			s.Add( "@FME_EXPRESSION", string.IsNullOrEmpty( fm.Expression ) ?
				Convert.DBNull : fm.Expression );

			s.Add( "@FME_EXPRESSION_TYPE", fm.ExpressionType.ToString() );

			s.Add( "@FME_IF_CLAUSE", fm.FillMapType == FillMapType.FillMapIf );

			s.Add( "@FME_SORT_ORDER", fm.SortOrder );

			//Guid parId = fm.ParentObject.ObjectType == ObjectType.UIElement ?
			//    ((UIElement)fm.ParentObject).Id : ((ValidationRule)fm.ParentObject).Id;
			s.Add( "@SPLX_UIE_VR_PARENT_ID", fm.ParentId );

			return s;
		}
		#endregion

		#region delete
		public void DeleteFillMapById(string id)
		{
			SortedList inparms = new sSortedList( "@SPLX_FILLMAP_EXPRESSION_ID", id );
			_da.ExecuteSP( "splx.splx_api_del_fme", inparms );
		}

		internal void DeleteFillMapById(string id, SqlTransaction tr)
		{
			SortedList inparms = new sSortedList( "@SPLX_FILLMAP_EXPRESSION_ID", id );
			_da.ExecuteSP( "splx.splx_api_del_fme", inparms, false, tr );
		}
		#endregion
	}

	public class FillMapFactory : ISuplexObjectFactory<FillMap>
	{
		private DataBindingFactory _dataBindingFactory = new DataBindingFactory();

		public DataTable DataBindingsTable { get; set; }

		public ISuplexObject CreateSuplexObjectBase(DataRow r)
		{
			return this.CreateObject( r );
		}

		public FillMap CreateObject(DataRow r)
		{
			FillMap fm = new FillMap();
			this.CreateObject( r, ref fm );
			return fm;
		}

		public void CreateObject(DataRow r, ref FillMap fm)
		{
			fm.Id = (int)(r["SPLX_FILLMAP_EXPRESSION_ID"]);
			fm.Name = (string)r["FME_NAME"];
			fm.EventBinding = Convert.IsDBNull( r["FME_EVENT_BINDING"] ) ?
				ControlEvents.None : sg.MiscUtils.ParseEnum<ControlEvents>( r["FME_EVENT_BINDING"], true );
			fm.Expression = r["FME_EXPRESSION"].ToString();
			fm.ExpressionType = Convert.IsDBNull( r["FME_EXPRESSION_TYPE"] ) ?
				ExpressionType.None : sg.MiscUtils.ParseEnum<sf.ExpressionType>( r["FME_EXPRESSION_TYPE"], true );
			fm.SortOrder = (int)r["FME_SORT_ORDER"];
			fm.FillMapType = (bool)r["FME_IF_CLAUSE"] ? FillMapType.FillMapIf : FillMapType.FillMapElse;

			string filter =
				string.Format( "SPLX_FILLMAP_EXPRESSION_ID = {0}", r["SPLX_FILLMAP_EXPRESSION_ID"].ToString() );
			fm.DataBindings.LoadSuplexObjectTable( this.DataBindingsTable, _dataBindingFactory, filter, null );

			fm.IsDirty = false;
		}
	}
}