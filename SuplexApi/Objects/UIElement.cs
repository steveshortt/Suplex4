using System.Collections.Generic;
using Suplex.Forms.ObjectModel.Api;

namespace Suplex.Api
{
	public partial class SuplexApi
	{
		public UIElement GetUIElementById(string id, bool shallow)
		{
			if( shallow )
			{
				return _splxDal.GetUIElementByIdShallow( id );
			}
			else
			{
				return _splxDal.GetUIElementByIdDeep( id );
			}
		}

		public UIElement UpsertUIElement(UIElement uie)
		{
			return _splxDal.UpsertUIElement( uie );
		}

		public void DeleteUIElementById(string id)
		{
			_splxDal.DeleteUIElementById( id );
		}
	}
}