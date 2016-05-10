using System;

namespace Suplex.WebForms
{
	/// <summary>
	/// Add some stuff not otherwise present on webcontrols.
	/// </summary>
	public interface IWebExtra
	{
		string Tag { get; set; }
		object TagObj { get; set; }
	}
}
