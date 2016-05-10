using System.Collections.Generic;
using Suplex.Forms.ObjectModel.Api;

namespace Suplex.Api
{
	public partial class SuplexApi
	{
		public FillMap GetFillMapById(string id)
		{
			return _splxDal.GetFillMapById( id );
		}

		public void UpsertFillMap(FillMap fm)
		{
			_splxDal.UpsertFillMap( fm );
		}

		public void DeleteFillMapById(string id)
		{
			_splxDal.DeleteFillMapById( id );
		}
	}
}