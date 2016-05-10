using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;



public class BinaryUtil
{
	#region public SQL methods
	[Microsoft.SqlServer.Server.SqlFunction( Name = "Splx_ContainsOne" )]
	public static SqlBoolean ContainsOne(SqlBinary source, SqlBinary value)
	{
		if( value.IsNull )
		{
			throw new ArgumentNullException( "value" );
		}
		if( source.Length != value.Length )
		{
			throw new ArgumentException( "Arg_ArrayLengthsDiffer" );
		}

		Suplex.BitLib.BitArray src = new Suplex.BitLib.BitArray( source.Value );
		Suplex.BitLib.BitArray val = new Suplex.BitLib.BitArray( value.Value );

		return new SqlBoolean( src.ContainsOne( val ) );

		//bool haveMatch = false;
		//int index = 0;
		//while( !haveMatch && index < src.Length )
		//{
		//    haveMatch = src[index] && val[index];
		//    index++;
		//}

		//return new SqlBoolean( haveMatch );
	}

	[Microsoft.SqlServer.Server.SqlFunction( Name = "Splx_GetTableOr", DataAccess = DataAccessKind.Read )]
	public static SqlBinary GetTableOr(SqlString tableName, SqlString maskFieldName, SqlInt32 bitArrayLength)
	{
		byte[] mask = new byte[bitArrayLength.Value / 8];

		using( SqlConnection connection = new SqlConnection( "context connection=true" ) )
		{
			connection.Open();
			SqlCommand command = new SqlCommand( string.Format( "SELECT {1} FROM {0}", tableName.Value, maskFieldName.Value ), connection );
			SqlDataReader reader = command.ExecuteReader();

			BitArray bits = new BitArray( bitArrayLength.Value );
			using( reader )
			{
				while( reader.Read() )
				{
					BitArray rowBits = new BitArray( (byte[])reader[maskFieldName.Value] );
					bits.Or( rowBits );
				}
			}
			bits.CopyTo( mask, 0 );
		}

		return new SqlBinary( mask );
	}

	[Microsoft.SqlServer.Server.SqlFunction( Name = "Splx_And" )]
	public static SqlBinary And(SqlBinary source, SqlBinary value)
	{
		if( value.IsNull )
		{
			throw new ArgumentNullException( "value" );
		}
		if( source.Length != value.Length )
		{
			throw new ArgumentException( "Arg_ArrayLengthsDiffer" );
		}

		BitArray src = new BitArray( source.Value );
		BitArray val = new BitArray( value.Value );

		src.And( val );

		byte[] mask = new byte[source.Value.Length];
		src.CopyTo( mask, 0 );
		return new SqlBinary( mask );
	}

	[Microsoft.SqlServer.Server.SqlFunction( Name = "Splx_Or" )]
	public static SqlBinary Or(SqlBinary source, SqlBinary value)
	{
		if( value.IsNull )
		{
			throw new ArgumentNullException( "value" );
		}
		if( source.Length != value.Length )
		{
			throw new ArgumentException( "Arg_ArrayLengthsDiffer" );
		}

		BitArray src = new BitArray( source.Value );
		BitArray val = new BitArray( value.Value );

		src.Or( val );

		byte[] mask = new byte[source.Value.Length];
		src.CopyTo( mask, 0 );
		return new SqlBinary( mask );
	}

	[Microsoft.SqlServer.Server.SqlFunction( Name = "Splx_Not" )]
	public static SqlBinary Not(SqlBinary value)
	{
		if( value.IsNull )
		{
			throw new ArgumentNullException( "value" );
		}
		BitArray val = new BitArray( value.Value );

		byte[] mask = new byte[value.Value.Length];
		val.Not().CopyTo( mask, 0 );
		return new SqlBinary( mask );
	}

	[Microsoft.SqlServer.Server.SqlFunction( Name = "Splx_Xor" )]
	public static SqlBinary Xor(SqlBinary source, SqlBinary value)
	{
		if( value.IsNull )
		{
			throw new ArgumentNullException( "value" );
		}
		if( source.Length != value.Length )
		{
			throw new ArgumentException( "Arg_ArrayLengthsDiffer" );
		}

		BitArray src = new BitArray( source.Value );
		BitArray val = new BitArray( value.Value );

		src.Xor( val );

		byte[] mask = new byte[source.Value.Length];
		src.CopyTo( mask, 0 );
		return new SqlBinary( mask );
	}
	#endregion


	//private static Suplex.BitLib.BitArray _bits = null;
	//[Microsoft.SqlServer.Server.SqlFunction( Name = "FasterAnd" )]
	//public static SqlBoolean FasterAnd(SqlBinary buf1, SqlBinary buf2)
	//{
	//    if( _bits == null )
	//    {
	//        _bits = new Suplex.BitLib.BitArray( buf1.Value );
	//    }
	//    return new SqlBoolean( _bits.ContainsOne( new Suplex.BitLib.BitArray( buf2.Value ) ) );
	//}
}

//decompiled and included becase they sealed the class.  any good reason for that nonsense???
namespace Suplex.BitLib
{
	[Serializable, System.Runtime.InteropServices.ComVisible( true )]
	public class BitArray : ICollection, IEnumerable, ICloneable  //removed: sealed
	{
		// Fields
		private const int _ShrinkThreshold = 0x100;
		[NonSerialized]
		private object _syncRoot;
		private int _version;
		private int[] m_array;
		private int m_length;

		// Methods
		private BitArray()
		{
		}

		public BitArray(int length)
			: this( length, false )
		{
		}

		public BitArray(bool[] values)
		{
			if( values == null )
			{
				throw new ArgumentNullException( "values" );
			}
			this.m_array = new int[(values.Length + 0x1f) / 0x20];
			this.m_length = values.Length;
			for( int i = 0; i < values.Length; i++ )
			{
				if( values[i] )
				{
					this.m_array[i / 0x20] |= ((int)1) << (i % 0x20);
				}
			}
			this._version = 0;
		}

		public BitArray(byte[] bytes)
		{
			if( bytes == null )
			{
				throw new ArgumentNullException( "bytes" );
			}
			this.m_array = new int[(bytes.Length + 3) / 4];
			this.m_length = bytes.Length * 8;
			int index = 0;
			int num2 = 0;
			while( (bytes.Length - num2) >= 4 )
			{
				this.m_array[index++] = (((bytes[num2] & 0xff) | ((bytes[num2 + 1] & 0xff) << 8)) | ((bytes[num2 + 2] & 0xff) << 0x10)) | ((bytes[num2 + 3] & 0xff) << 0x18);
				num2 += 4;
			}
			switch( (bytes.Length - num2) )
			{
				case 1:
				goto Label_00DB;

				case 2:
				break;

				case 3:
				this.m_array[index] = (bytes[num2 + 2] & 0xff) << 0x10;
				break;

				default:
				goto Label_00FC;
			}
			this.m_array[index] |= (bytes[num2 + 1] & 0xff) << 8;
		Label_00DB:
			this.m_array[index] |= bytes[num2] & 0xff;
		Label_00FC:
			this._version = 0;
		}

		public BitArray(int[] values)
		{
			if( values == null )
			{
				throw new ArgumentNullException( "values" );
			}
			this.m_array = new int[values.Length];
			this.m_length = values.Length * 0x20;
			Array.Copy( values, this.m_array, values.Length );
			this._version = 0;
		}

		public BitArray(BitArray bits)
		{
			if( bits == null )
			{
				throw new ArgumentNullException( "bits" );
			}
			this.m_array = new int[(bits.m_length + 0x1f) / 0x20];
			this.m_length = bits.m_length;
			Array.Copy( bits.m_array, this.m_array, (int)((bits.m_length + 0x1f) / 0x20) );
			this._version = bits._version;
		}

		public BitArray(int length, bool defaultValue)
		{
			if( length < 0 )
			{
				throw new ArgumentOutOfRangeException( "length", "ArgumentOutOfRange_NeedNonNegNum" );
			}
			this.m_array = new int[(length + 0x1f) / 0x20];
			this.m_length = length;
			int num = defaultValue ? -1 : 0;
			for( int i = 0; i < this.m_array.Length; i++ )
			{
				this.m_array[i] = num;
			}
			this._version = 0;
		}

		public BitArray And(BitArray value)
		{
			if( value == null )
			{
				throw new ArgumentNullException( "value" );
			}
			if( this.m_length != value.m_length )
			{
				throw new ArgumentException( "Arg_ArrayLengthsDiffer" );
			}
			int num = (this.m_length + 0x1f) / 0x20;
			for( int i = 0; i < num; i++ )
			{
				this.m_array[i] &= value.m_array[i];
			}
			this._version++;
			return this;
		}

		/// <summary>
		/// Bitwise 'and' on 32bit chunks to determine if at least one bit matches (is set to 1) between [this] and [value].  Stops searching on first hit.
		/// </summary>
		/// <param name="value">The comparison value.</param>
		/// <returns>True if at least bit matches, False if none.</returns>
		public bool ContainsOne(BitArray value)
		{
			if( value == null )
			{
				throw new ArgumentNullException( "value" );
			}
			if( this.m_length != value.m_length )
			{
				throw new ArgumentException( "Arg_ArrayLengthsDiffer" );
			}
			bool haveMatch = false;
			int num = (this.m_length + 0x1f) / 0x20;
			for( int i = 0; i < num; i++ )
			{
				//if the 32nd bit of the array is set to 1, test for int.MinValue
				this.m_array[i] &= value.m_array[i];
				if( this.m_array[i] > 0 || this.m_array[i] == int.MinValue )
				{
					haveMatch = true;
					break;
				}
			}
			return haveMatch;
		}

		public object Clone()
		{
			return new BitArray( this.m_array ) { _version = this._version, m_length = this.m_length };
		}

		public void CopyTo(Array array, int index)
		{
			if( array == null )
			{
				throw new ArgumentNullException( "array" );
			}
			if( index < 0 )
			{
				throw new ArgumentOutOfRangeException( "index", "ArgumentOutOfRange_NeedNonNegNum" );
			}
			if( array.Rank != 1 )
			{
				throw new ArgumentException( "Arg_RankMultiDimNotSupported" );
			}
			if( array is int[] )
			{
				Array.Copy( this.m_array, 0, array, index, (this.m_length + 0x1f) / 0x20 );
			}
			else if( array is byte[] )
			{
				if( (array.Length - index) < ((this.m_length + 7) / 8) )
				{
					throw new ArgumentException( "Argument_InvalidOffLen" );
				}
				byte[] buffer = (byte[])array;
				for( int i = 0; i < ((this.m_length + 7) / 8); i++ )
				{
					buffer[index + i] = (byte)((this.m_array[i / 4] >> ((i % 4) * 8)) & 0xff);
				}
			}
			else
			{
				if( !(array is bool[]) )
				{
					throw new ArgumentException( "Arg_BitArrayTypeUnsupported" );
				}
				if( (array.Length - index) < this.m_length )
				{
					throw new ArgumentException( "Argument_InvalidOffLen" );
				}
				bool[] flagArray = (bool[])array;
				for( int j = 0; j < this.m_length; j++ )
				{
					flagArray[index + j] = ((this.m_array[j / 0x20] >> (j % 0x20)) & 1) != 0;
				}
			}
		}

		public bool Get(int index)
		{
			if( (index < 0) || (index >= this.m_length) )
			{
				throw new ArgumentOutOfRangeException( "index", "ArgumentOutOfRange_Index" );
			}
			return ((this.m_array[index / 0x20] & (((int)1) << (index % 0x20))) != 0);
		}

		public IEnumerator GetEnumerator()
		{
			return new BitArrayEnumeratorSimple( this );
		}

		public BitArray Not()
		{
			int num = (this.m_length + 0x1f) / 0x20;
			for( int i = 0; i < num; i++ )
			{
				this.m_array[i] = ~this.m_array[i];
			}
			this._version++;
			return this;
		}

		public BitArray Or(BitArray value)
		{
			if( value == null )
			{
				throw new ArgumentNullException( "value" );
			}
			if( this.m_length != value.m_length )
			{
				throw new ArgumentException( "Arg_ArrayLengthsDiffer" );
			}
			int num = (this.m_length + 0x1f) / 0x20;
			for( int i = 0; i < num; i++ )
			{
				this.m_array[i] |= value.m_array[i];
			}
			this._version++;
			return this;
		}

		public void Set(int index, bool value)
		{
			if( (index < 0) || (index >= this.m_length) )
			{
				throw new ArgumentOutOfRangeException( "index", "ArgumentOutOfRange_Index" );
			}
			if( value )
			{
				this.m_array[index / 0x20] |= ((int)1) << (index % 0x20);
			}
			else
			{
				this.m_array[index / 0x20] &= ~(((int)1) << (index % 0x20));
			}
			this._version++;
		}

		public void SetAll(bool value)
		{
			int num = value ? -1 : 0;
			int num2 = (this.m_length + 0x1f) / 0x20;
			for( int i = 0; i < num2; i++ )
			{
				this.m_array[i] = num;
			}
			this._version++;
		}

		public BitArray Xor(BitArray value)
		{
			if( value == null )
			{
				throw new ArgumentNullException( "value" );
			}
			if( this.m_length != value.m_length )
			{
				throw new ArgumentException( "Arg_ArrayLengthsDiffer" );
			}
			int num = (this.m_length + 0x1f) / 0x20;
			for( int i = 0; i < num; i++ )
			{
				this.m_array[i] ^= value.m_array[i];
			}
			this._version++;
			return this;
		}

		/// <summary>
		/// Bitwise 'and' on 32bit chunks to determine if at least one bit is set (to '1') in [this].  Stops searching on first hit.
		/// Returns True if at least bit is set (to '1'), False if none.
		/// </summary>
		public bool HasValue
		{
			get
			{
				bool hasValue = false;
				int num = (this.m_length + 0x1f) / 0x20;
				for( int i = 0; i < num; i++ )
				{
					//if the 32nd bit of the array is set to 1, test for int.MinValue
					hasValue = this.m_array[i] > 0 || this.m_array[i] == int.MinValue;
					if( hasValue ) { break; }
				}
				return hasValue;
			}
		}

		// Properties
		public int Count { get { return this.m_length; } }

		public bool IsReadOnly { get { return false; } }

		public bool IsSynchronized { get { return false; } }

		public bool this[int index]
		{
			get { return this.Get( index ); }
			set { this.Set( index, value ); }
		}

		public int Length
		{
			get { return this.m_length; }
			set
			{
				if( value < 0 )
				{
					throw new ArgumentOutOfRangeException( "value", "ArgumentOutOfRange_NeedNonNegNum" );
				}
				int num = (value + 0x1f) / 0x20;
				if( (num > this.m_array.Length) || ((num + 0x100) < this.m_array.Length) )
				{
					int[] destinationArray = new int[num];
					Array.Copy( this.m_array, destinationArray, (num > this.m_array.Length) ? this.m_array.Length : num );
					this.m_array = destinationArray;
				}
				if( value > this.m_length )
				{
					int index = ((this.m_length + 0x1f) / 0x20) - 1;
					int num3 = this.m_length % 0x20;
					if( num3 > 0 )
					{
						this.m_array[index] &= (((int)1) << num3) - 1;
					}
					Array.Clear( this.m_array, index + 1, (num - index) - 1 );
				}
				this.m_length = value;
				this._version++;
			}
		}

		public object SyncRoot
		{
			get
			{
				if( this._syncRoot == null )
				{
					System.Threading.Interlocked.CompareExchange( ref this._syncRoot, new object(), null );
				}
				return this._syncRoot;
			}
		}

		// Nested Types
		[Serializable]
		private class BitArrayEnumeratorSimple : IEnumerator, ICloneable
		{
			// Fields
			private BitArray bitarray;
			private bool currentElement;
			private int index;
			private int version;

			// Methods
			internal BitArrayEnumeratorSimple(BitArray bitarray)
			{
				this.bitarray = bitarray;
				this.index = -1;
				this.version = bitarray._version;
			}

			public object Clone()
			{
				return base.MemberwiseClone();
			}

			public virtual bool MoveNext()
			{
				if( this.version != this.bitarray._version )
				{
					throw new InvalidOperationException( "InvalidOperation_EnumFailedVersion" );
				}
				if( this.index < (this.bitarray.Count - 1) )
				{
					this.index++;
					this.currentElement = this.bitarray.Get( this.index );
					return true;
				}
				this.index = this.bitarray.Count;
				return false;
			}

			public void Reset()
			{
				if( this.version != this.bitarray._version )
				{
					throw new InvalidOperationException( "InvalidOperation_EnumFailedVersion" );
				}
				this.index = -1;
			}

			// Properties
			public virtual object Current
			{
				get
				{
					if( this.index == -1 )
					{
						throw new InvalidOperationException( "InvalidOperation_EnumNotStarted" );
					}
					if( this.index >= this.bitarray.Count )
					{
						throw new InvalidOperationException( "InvalidOperation_EnumEnded" );
					}
					return this.currentElement;
				}
			}
		}
	}
}