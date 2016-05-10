using System;

namespace Suplex.General
{
	public interface ICloneable<T>
	{
		T Clone();
	}
}