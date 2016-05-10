using System;
using System.Data.SqlClient;
using System.Collections.Specialized;

namespace Suplex.Data
{
	public interface IDataAccessor
	{
		string ConnectionString
		{
			get;
			set;
		}


		SqlConnection Connection
		{
			get;
		}


		void OpenConnection();

		void CloseConnection();
	}



	public class DataAccessorCollection : NameObjectCollectionBase  
	{
		public DataAccessorCollection(){}


		// Gets or sets the value associated with the specified key.
		public IDataAccessor this[ string key ]  
		{
			get  
			{
				return (IDataAccessor)this.BaseGet( key );
			}
			set  
			{
				this.BaseSet( key, value );
			}
		}


		// Gets a key-and-value pair (DictionaryEntry) using an index.
		public IDataAccessor this[ int index ]  
		{
			get  
			{
				return (IDataAccessor)this.BaseGet( index );
			}
			set
			{
				this.BaseSet( index, value );
			}
		}


		// Gets a String array that contains all the keys in the collection.
		public string[] AllKeys  
		{
			get  
			{
				return( this.BaseGetAllKeys() );
			}
		}


		// Gets an Object array that contains all the values in the collection.
		public IDataAccessor[] Values  
		{
			get  
			{
				return (IDataAccessor[])this.BaseGetAllValues();
			}
		}


		// Gets a value indicating if the collection contains keys that are not null.
		public bool HasKeys  
		{
			get  
			{
				return this.BaseHasKeys();
			}
		}


		// Adds an entry to the collection.
		public void Add( string key, IDataAccessor value )  
		{
			this.BaseAdd( key, value );
		}


		// Removes an entry with the specified key from the collection.
		public void Remove( string key )  
		{
			this.BaseRemove( key );
		}


		// Removes an entry in the specified index from the collection.
		public void RemoveAt( int index )  
		{
			this.BaseRemoveAt( index );
		}


		// Clears all the elements in the collection.
		public void Clear()  
		{
			this.BaseClear();
		}


		public bool Contains( IDataAccessor value )
		{
			bool contains = false;
			IDataAccessor[] values = this.Values;

			int n=0;
			while( !contains && n<values.Length )
			{
				contains = values[n].Equals( value );
				n++;
			}

			return contains;
		}

	
		public bool ContainsKey( string key )
		{
			bool contains = false;
			string[] keys = base.BaseGetAllKeys();

			int n=0;
			while( !contains && n<keys.Length )
			{
				contains = keys[n].Equals( key );
				n++;
			}

			return contains;
		}
	}


}
