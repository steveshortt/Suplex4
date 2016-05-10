using System.Data;
using Suplex.Forms;
using Suplex.Forms.ObjectModel.Api;

namespace Suplex.Api
{
	public partial class SuplexApi
	{
		public string GetSecurityString(string uniqueName)
		{
			return _splxDal.GetSecurityCacheString( uniqueName, _splxUser, ExternalGroupInfo.Empty );
		}

		public DataSet GetSecurity(string uniqueName)
		{
			//SecurityBuilder sb = new SecurityBuilder();
			//DataSet ds = sb.CreateSecurityCache( _splxDal.DataAccessor, uniqueName, _splxUser, ExternalGroupInfo.Empty );
			//return ds;

			//return _splxDal.GetSecurityCache( uniqueName, _splxUser, ExternalGroupInfo.Empty );

			return _splxDal.GetSecurityCache( uniqueName, _splxUser, ExternalGroupInfo.Empty );
		}

		public SuplexStore GetSecurityStore(string uniqueName)
		{
			return _splxDal.GetSecurityStore( uniqueName, _splxUser, ExternalGroupInfo.Empty );
		}
	}
}