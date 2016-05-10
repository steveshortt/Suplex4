using Suplex.Forms.ObjectModel.Api;

namespace Suplex.Api
{
	public partial class SuplexApi
	{
		public SuplexStore GetSuplexStore()
		{
			return _splxDal.GetSuplexStore();
		}
	}
}