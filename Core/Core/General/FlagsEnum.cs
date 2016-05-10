using System;

namespace Suplex.General
{

	/*
	 *	this works just fine, but i don't like all the typecasting
	 */


	public abstract class FlagsEnum
	{
		private int _enum = 0;
		private Type _type = null;

		
		public FlagsEnum()
		{			
		}


		public Type EnumType
		{
			set
			{
				_type = value;
			}
		}


		public virtual void Add( object value )
		{
			_enum |= EnumValue( value );
		}


		public virtual void Remove( object value )
		{
			_enum ^= EnumValue( value );
		}


		public virtual bool Contains( object value )
		{
			if( (_enum & EnumValue(value)) > 0 )
				return true;
			else
				return false;
		}

		private int EnumValue( object value )
		{
			object foo = Enum.Parse(_type, value.ToString());
			return Int32.Parse( Enum.Format( _type, Enum.Parse(_type, value.ToString()), "d") );
		}
	}


}



/*
 *	consumer...
 * 	
	public class ControlEventsCollection : FlagsEnum
	{
		public ControlEventsCollection()
		{
			base.EnumType = typeof( ControlEvents );
		}


		public void Add(ControlEvents value)
		{
			base.Add( value );
		}


		public void Remove(ControlEvents value)
		{
			base.Remove( value );
		}


		public bool Contains(ControlEvents value)
		{
			return base.Contains( value );
		}

	}//class
*/