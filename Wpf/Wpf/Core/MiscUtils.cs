using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;


//Moved the WPF-specific stuff to here so non-WPF apps don't require a dependency on Windows.Media
namespace Suplex.Wpf
{
	public partial class MiscUtils
	{
		public MiscUtils() { }


		//clean version A
		public static T GetChild<T>(DependencyObject parent) where T : DependencyObject
		{
			int i = 0;
			bool found = false;
			DependencyObject child = null;
			while( i < VisualTreeHelper.GetChildrenCount( parent ) && !found )
			{
				child = VisualTreeHelper.GetChild( parent, i );
				found = child != null && child.GetType() == typeof( T );
				if( !found )
				{
					child = GetChild<T>( child );
					found = child != null && child.GetType() == typeof( T );
				}
				i++;
			}

			return child as T;
		}

		////clean version B
		//public static T GetChild<T>(DependencyObject parent) where T : DependencyObject
		//{
		//    DependencyObject child = null;
		//    for( int i = 0; i < VisualTreeHelper.GetChildrenCount( parent ); i++ )
		//    {
		//        child = VisualTreeHelper.GetChild( parent, i );
		//        if( child != null && child.GetType() != typeof( T ) )
		//        {
		//            child = GetChild<T>( child );
		//        }
		//        else
		//        {
		//            break;
		//        }
		//    }

		//    return child as T;
		//}

		//original code from:
		//http://leeontech.wordpress.com/2008/09/24/getting-the-control-inside-a-datatemplate/
		//public T GetChild_<T>(DependencyObject obj) where T : DependencyObject
		//{
		//    DependencyObject child = null;
		//    for( Int32 i = 0; i < VisualTreeHelper.GetChildrenCount( obj ); i++ )
		//    {
		//        child = VisualTreeHelper.GetChild( obj, i );
		//        if( child != null && child.GetType() == typeof( T ) )
		//        {
		//            break;
		//        }
		//        else if( child != null )
		//        {
		//            child = GetChild<T>( child );
		//            if( child != null && child.GetType() == typeof( T ) )
		//            {
		//                break;
		//            }
		//        }
		//    }
		//    return child as T;
		//}
	}
}