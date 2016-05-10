using System;
using System.Reflection;
using System.Collections;
using System.ComponentModel;


namespace Suplex.General
{
	/// <summary>
	/// Summary description for ReflectionUtils.
	/// </summary>
	public class ReflectionUtils
	{
		public ReflectionUtils()
		{
		}


		public static void SetProperties(object PropObj, SortedList PropertyNamesValues)
		{
			IEnumerator list = PropertyNamesValues.GetEnumerator();
			while( list.MoveNext() )
			{
				DictionaryEntry d = (DictionaryEntry)list.Current;
				SetProperty( PropObj, d.Key.ToString(), d.Value, true );
			}
		}


		public static bool SetProperty(object PropObj, string PropertyName, object Value, bool OverrideValue)
		{
			return SetProperty( PropObj, PropertyName, Value, OverrideValue, true );
		}


		public static bool SetProperty(object PropObj, string PropertyName, object Value, bool OverrideValue, bool Typecast)
		{
			bool setPropSuccess = false;

			bool match = true;

			string[] props = PropertyName.Split( new char[] {'.'} );
			
			if( props.Length > 0 )
			{
				object prop_obj = PropObj;
				PropertyInfo prop = null;
				
				int n = 0;
				while( n < props.Length && match )
				{
					try
					{
						prop = prop_obj.GetType().GetProperty( props[n] );

						//if( prop == null ) match = false;							//07042005
						match = prop != null;

						if( match && n < props.Length-1 )	// && props.Length > 1	//07042005
						{
							prop_obj = prop.GetValue( prop_obj, null );
						}
					}
					catch	//( AmbiguousMatchException e )
					{
						match = false;
					}

					if( !match )
					{
						PropertyInfo[] p = prop_obj.GetType().GetProperties();
						IEnumerator ps = p.GetEnumerator();

						while( ps.MoveNext() && !match )
						{
							if( ((PropertyInfo)ps.Current).Name.ToLower() == props[n].ToLower() &&
								((PropertyInfo)ps.Current).ReflectedType == PropObj.GetType() )
							{
								prop = (PropertyInfo)ps.Current;
								match = true;
							}
							else if( ((PropertyInfo)ps.Current).Name.ToLower() == props[n].ToLower() &&
								((PropertyInfo)ps.Current).DeclaringType == PropObj.GetType() )
							{
								prop = (PropertyInfo)ps.Current;
								match = true;
							}
						}
					}

					n++;

				}//while

				if( match )
				{
					object curr_value = null;
					
					if( prop.CanRead )
					{
						try
						{
							curr_value = prop.GetValue( prop_obj, null );
						}
						catch{}
					}

					if( prop.CanWrite )
					{
						if( ( curr_value == null || curr_value.ToString().Length == 0 ) || OverrideValue )
						{
							object new_value = Value;
							if( Typecast )
							{
								TypeConverter conv = TypeDescriptor.GetConverter( prop.PropertyType );
								if( conv.CanConvertFrom( typeof(string) ) )
								{
									new_value = conv.ConvertFromString( Value.ToString() );
								}
							}
					
							try
							{
								prop.SetValue( prop_obj, new_value, null );
								setPropSuccess = true;
							}
							catch(Exception ex)
							{
								System.Diagnostics.Debug.WriteLine( ex.Message, prop_obj.GetType().ToString() );
							}

						}
					}//if( prop.CanWrite )

				}//if( match )

			}//if

			return setPropSuccess;

		}//method


		public static PropertyInfo GetProperty(object PropObj, string PropertyName)
		{
			PropertyInfo prop = null;
			bool match = true;

			string[] props = PropertyName.Split( new char[] {'.'} );
			
			if( props.Length > 0 )
			{
				object prop_obj = PropObj;
				//PropertyInfo prop = null;
				
				int n = 0;
				while( n < props.Length && match )
				{
					try
					{
						prop = prop_obj.GetType().GetProperty( props[n] );

						//if( prop == null ) match = false;							//07042005
						match = prop != null;

						if( match && n < props.Length-1 )	// && props.Length > 1	//07042005
						{
							prop_obj = prop.GetValue( prop_obj, null );
						}
					}
					catch	//( AmbiguousMatchException e )
					{
						match = false;
					}

					if( !match )
					{
						PropertyInfo[] p = prop_obj.GetType().GetProperties();
						IEnumerator ps = p.GetEnumerator();

						while( ps.MoveNext() && !match )
						{
							if( ((PropertyInfo)ps.Current).Name.ToLower() == props[n].ToLower() &&
								((PropertyInfo)ps.Current).ReflectedType == PropObj.GetType() )
							{
								prop = (PropertyInfo)ps.Current;
								match = true;
							}
							else if( ((PropertyInfo)ps.Current).Name.ToLower() == props[n].ToLower() &&
								((PropertyInfo)ps.Current).DeclaringType == PropObj.GetType() )
							{
								prop = (PropertyInfo)ps.Current;
								match = true;
							}
						}
					}

					n++;

				}//while

			}//if

			return prop;

		}//method


		public static bool ImplementsInterface(object criteriaObj, string interfaceName)
		{
			return ImplementsInterface( criteriaObj.GetType(), new string[] {interfaceName} );
		}


		public static bool ImplementsInterface(object criteriaObj, string[] interfaceList)
		{
			return ImplementsInterface( criteriaObj.GetType(), interfaceList );
		}


		public static bool ImplementsInterface(Type type, string[] interfaceList)
		{
			bool result = false;
			int found = 0;

			// Specify the TypeFilter delegate that compares the interfaces against filter criteria.
			TypeFilter filter = new TypeFilter(InterfaceFilter);


			for( int i=0; i<interfaceList.Length; i++ )
			{
				Type[] interfaces = type.FindInterfaces(filter, interfaceList[i]);
				if ( interfaces.Length > 0 ) found++;
			}

			if ( found == interfaceList.Length ) 
			{
				result = true;
			}

			return result;
		}


		private static bool InterfaceFilter(Type TypeObj, Object CriteriaObj)
		{
			bool result = false;

			if( TypeObj.ToString() == CriteriaObj.ToString() )
			{
				result = true;
			}

			return result;
		}



	}//class
}