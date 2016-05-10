using System.Collections;
using System.ComponentModel;
using System.Text;



namespace Suplex.Forms.ObjectModel.Api
{
	public partial class SuplexApiClient : INotifyPropertyChanged
	{
		private string JoinMembership(IList membership)
		{
			StringBuilder list = new StringBuilder();
			foreach( SecurityPrincipalBase value in membership )
			{
				list.AppendFormat( "{0};{1}{2}", value.Id, value.IsUserObject, "," );
			}
			return list.ToString().TrimEnd( ',' );
		}
	}
}