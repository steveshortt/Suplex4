using System;

namespace System.Collections
{
	public class sSortedList : SortedList
	{
		public sSortedList() : base() { }

		public sSortedList(string parameterName0, object value0) : base()
		{
			base.Add( parameterName0, value0 );
		}
		public sSortedList(string parameterName0, object value0, string parameterName1, object value1) : base()
		{
			base.Add( parameterName0, value0 );
			base.Add( parameterName1, value1 );
		}
		public sSortedList(string parameterName0, object value0, string parameterName1, object value1, string parameterName2, object value2) : base()
		{
			base.Add( parameterName0, value0 );
			base.Add( parameterName1, value1 );
			base.Add( parameterName2, value2 );
		}
		public sSortedList(params object[] keyValuePairs) : base()
		{
			AddItems( keyValuePairs );
		}
		public sSortedList(int initialCapacity, params object[] keyValuePairs) : base( initialCapacity )
		{
			AddItems( keyValuePairs );
		}

		private void AddItems(params object[] keyValuePairs)
		{
			if( keyValuePairs.Length % 2 != 0 )
			{
				throw new ArgumentException( "The keyValuePairs length is invalid; make sure the data is in key/value pairs." );
			}
			else
			{
				for( int n = 0; n < keyValuePairs.Length; )
				{
					base.Add( keyValuePairs[n++], keyValuePairs[n++] );
				}
			}
		}
	}
}
