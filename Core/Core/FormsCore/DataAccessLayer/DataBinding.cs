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
		internal void UpsertDataBinding(DataBinding db, long fmeId, ref SqlTransaction tr)
		{
			SortedList inparms = this.GetDataBindingParms( db, fmeId );

			int idAsInt = -1;
			Int32.TryParse( db.Id.ToString(), out idAsInt );

			SqlParameter id = new SqlParameter( "@SPLX_FILLMAP_DATABINDING_ID", SqlDbType.Int );
			id.Value = idAsInt;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_FILLMAP_DATABINDING_ID", id );

			_da.ExecuteSP( "splx.splx_api_upsert_fmb", inparms, ref outparms, false, tr );
			db.Id = (int)id.Value;

			db.IsDirty = false;
		}

		private SortedList GetDataBindingParms(DataBinding db,long fmeId)
		{
			sSortedList s = new sSortedList( "@FMB_UIE_UNIQUE_NAME", db.ControlName );

			s.Add( "@FMB_PROPERTY_NAME", db.PropertyName );
			s.Add( "@FMB_VALUE", db.DataMember );
			s.Add( "@FMB_TYPECAST_VALUE", db.ConversionRequired );
			s.Add( "@FMB_OVERRIDE_VALUE", db.OverrideValue );
			s.Add( "@SPLX_FILLMAP_EXPRESSION_ID", fmeId );

			return s;
		}
	}

	public class DataBindingFactory : ISuplexObjectFactory<DataBinding>
	{
		public ISuplexObject CreateSuplexObjectBase(DataRow r)
		{
			return this.CreateObject( r );
		}

		public DataBinding CreateObject(DataRow r)
		{
			DataBinding db = new DataBinding();
			this.CreateObject( r, ref db );
			return db;
		}

		public void CreateObject(DataRow r, ref DataBinding db)
		{
			db.Id = (int)(r["SPLX_FILLMAP_DATABINDING_ID"]);
			db.ControlName = (string)r["FMB_UIE_UNIQUE_NAME"];
			db.PropertyName = r["FMB_PROPERTY_NAME"].ToString();
			db.DataMember = r["FMB_VALUE"].ToString();
			db.ConversionRequired = (bool)r["FMB_TYPECAST_VALUE"];
			db.OverrideValue = (bool)r["FMB_OVERRIDE_VALUE"];

			db.IsDirty = false;
		}
	}
}