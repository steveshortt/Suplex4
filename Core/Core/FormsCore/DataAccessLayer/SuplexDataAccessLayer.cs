using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

using Suplex.Data;


namespace Suplex.Forms.ObjectModel.Api
{
	public partial class SuplexDataAccessLayer
	{
		private DataAccessor _da = null;
		private string _splxSchema = "splx";

		public SuplexDataAccessLayer()
		{
			_da = new DataAccessor();
		}
		public SuplexDataAccessLayer(string connectionString)
		{
			_da = new DataAccessor( connectionString );
		}

		public string ConnectionString
		{
			get { return _da.ConnectionString; }
			set { _da.ConnectionString = value; }
		}

		public DataAccessor DataAccessor { get { return _da; } }

		public string SuplexSchema { get { return _splxSchema; } set { _splxSchema = value; } }
		internal string splxSchema { get { return string.Format( "{0}.", _splxSchema ); } }
	}
}