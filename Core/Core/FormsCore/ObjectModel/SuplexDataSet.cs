using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;


namespace Suplex.Forms.ObjectModel.Api
{
	public class SuplexDataSet : DataSet
	{
		public SuplexDataSet()
			: base()
		{
		}

		public SuplexDataSet(string dataSetName)
			: base( dataSetName )
		{
		}

		public SuplexDataSet(SerializationInfo info, StreamingContext context)
			: base( info, context )
		{
		}

		public SuplexDataSet(SerializationInfo info, StreamingContext context, bool constructSchema)
			: base( info, context, constructSchema )
		{
		}

		public override SchemaSerializationMode SchemaSerializationMode
		{
			get
			{
				return SchemaSerializationMode.ExcludeSchema;
			}
			set
			{
				base.SchemaSerializationMode = value;
			}
		}

		public SuplexDataSet(DataSet ds)
		{
			this.Convert( ds );
		}

		public void Convert(DataSet ds)
		{
			List<DataTable> tables = new List<DataTable>( ds.Tables.Count );
			for( int i = ds.Tables.Count - 1; i >= 0; i-- )
			{
				DataTable t = ds.Tables[i];
				ds.Tables.Remove( t );
				tables.Add( t );
			}
			for( int i = tables.Count - 1; i >= 0; i-- )
			{
				DataTable t = tables[i];
				tables.Remove( t );
				this.Tables.Add( t );
			}
		}
	}
}