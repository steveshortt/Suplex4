using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;


namespace Suplex.General
{
	public partial class MiscUtils
	{
		public MiscUtils() { }


		public static object CheckEmpty(string value)
		{
			return value != null && value.Length > 0 ? value : Convert.DBNull;
		}

		public static string Format(object data, TypeCode typeCode, string format)
		{
			Type type = Type.GetType( "System." + typeCode.ToString() );
			return Format( data, type, format );
		}

		public static string Format(object data, Type type, string format)
		{
			if( type != null && type != typeof( string ) &&
				format != null && format != string.Empty )
			{
				try
				{
					MethodInfo parse = type.GetMethod( "Parse", new Type[] { typeof( string ) } );
					object var = Activator.CreateInstance( type );
					var = parse.Invoke( type, new object[] { data.ToString() } );
					MethodInfo tostring = var.GetType().GetMethod( "ToString", new Type[] { typeof( string ) } );
					return tostring.Invoke( var, new object[] { format } ).ToString();
				}
				catch
				{
					return data.ToString();
				}
			}
			else
			{
				return data.ToString();
			}
		}

		public static T ParseEnum<T>(string data)
		{
			return (T)Enum.Parse( typeof( T ), data );
		}
		public static T ParseEnum<T>(string data, bool ignoreCase)
		{
			return (T)Enum.Parse( typeof( T ), data, ignoreCase );
		}

		public static T ParseEnum<T>(object data)
		{
			return (T)Enum.Parse( typeof( T ), data.ToString() );
		}
		public static T ParseEnum<T>(object data, bool ignoreCase)
		{
			return (T)Enum.Parse( typeof( T ), data.ToString(), ignoreCase );
		}

		[Obsolete( "This will be deprecated when Suplex is upgraded to .NET 4", false )]
		public static string Join<T>(string separator, System.Collections.IEnumerable values)
		{
			if( values == null || separator == null )
			{
				return string.Empty;
			}

			StringBuilder list = new StringBuilder();
			foreach( T value in values )
			{
				list.AppendFormat( "{0}{1}", value.ToString(), separator );
			}
			return list.ToString().TrimEnd( separator.ToCharArray() );
		}

		[Obsolete( "This will be deprecated when Suplex is upgraded to .NET 4", false )]
		public static string Join<T>(string separator, IEnumerable<T> values)
		{
			if( values == null || separator == null )
			{
				return string.Empty;
			}

			StringBuilder list = new StringBuilder();
			foreach( T value in values )
			{
				list.AppendFormat( "{0}{1}", value.ToString(), separator );
			}
			return list.ToString().TrimEnd( separator.ToCharArray() );
		}

		[Obsolete( "This will be deprecated when Suplex is upgraded to .NET 4", false )]
		public static string Join(string separator, System.Data.DataRow[] rows, string column)
		{
			if( rows == null || separator == null || rows.Length == 0 )
			{
				return string.Empty;
			}

			StringBuilder list = new StringBuilder();
			foreach( System.Data.DataRow row in rows )
			{
				list.AppendFormat( "{0}{1}", row[column].ToString(), separator );
			}
			return list.ToString().TrimEnd( separator.ToCharArray() );
		}
	}

	public static class DataUtils
	{
		public static T IsDBNullOrValue<T>(this DataRow r, string field)
		{
			return r.IsDBNullOrValue<T>( field, default( T ) );
		}
		public static T IsDBNullOrValue<T>(this DataRow r, string field, T altValue)
		{
			T value = default( T );

			if( !string.IsNullOrWhiteSpace( field ) && r.Table.Columns.Contains( field ) )
			{
				if( typeof( T ).IsEnum )
				{
					value = r[field] == Convert.DBNull ? altValue : MiscUtils.ParseEnum<T>( r[field].ToString() );
				}
				else
				{
					value = r[field] == Convert.DBNull ? altValue : (T)r[field];
				}
			}

			return value;
		}
	}
}