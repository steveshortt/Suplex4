using System;
using System.Collections;

namespace Suplex.General
{
	public static class MiscExtensions
	{
		public static bool Contains(this BitArray source, BitArray value)
		{
			if( value == null )
			{
				throw new ArgumentNullException( "value" );
			}
			if( source.Length != value.Length )
			{
				throw new ArgumentException( "Arg_ArrayLengthsDiffer" );
			}
			bool haveMatch = false;
			int num = (source.Length + 0x1f) / 0x20;
			for( int i = 0; i < num; i++ )
			{
				haveMatch = (source[i] & value[i]) == source[i];
				if( haveMatch )
				{
					break;
				}
			}
			return haveMatch;
		}
	}
}