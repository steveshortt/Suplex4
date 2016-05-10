using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;

namespace Suplex.WebForms
{
	public class RenderUtil
	{
		public static string RenderAccessKey(string text, WebControl control)
		{
			int amp = text.IndexOf( "&" );
			if( amp > -1 )
			{
				control.AccessKey = text.Substring( amp + 1, 1 );
				text = text.Insert( amp + 2, "</u>" );
				text = text.Replace( "&", "<u>" );
			}

			return text;
		}
	}
}
