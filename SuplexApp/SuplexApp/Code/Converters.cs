using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using sf = Suplex.Forms;
using api = Suplex.Forms.ObjectModel.Api;

namespace SuplexApp
{
	public class SaveImageConverter : IMultiValueConverter
	{
		//expected in "parameter":
		//0: Resource Path (ex: "Resources/ToolBar/file/")
		//1: color regular save icon
		//2: color save_secure icon
		//3: grey regular save icon
		//4: grey save_secure icon
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string imageName;

			string[] parts = parameter.ToString().Split( ',' );
			bool isEnabled = (bool)values[0];
			bool isSecure = (bool)values[1];

			if( isEnabled )
			{
				imageName = !isSecure ? parts[1] : parts[2];
			}
			else
			{
				imageName = !isSecure ? parts[3] : parts[4];
			}

			return new System.Windows.Media.Imaging.BitmapImage(
				new Uri( string.Format( "{0}{1}", parts[0], imageName ), UriKind.Relative ) );
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

	public class DialogTitleConverter : IMultiValueConverter
	{
		//expected in "parameter":
		//0: Main Application Title (ex: "Suplex")
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string connectionPath =
				values[0] == DependencyProperty.UnsetValue ? string.Empty : values[0].ToString();
			connectionPath = string.IsNullOrWhiteSpace( connectionPath ) ? string.Empty :
				string.Format( ": {0}", values[0].ToString() );


			//string dataSourceName = string.Empty;

			//bool isConnected = (bool)values[0];
			//string databaseConnectionString = values[1] == DependencyProperty.UnsetValue ? string.Empty : values[1].ToString();
			//string filePath = values[2] == DependencyProperty.UnsetValue ? string.Empty : values[2].ToString();
			//bool isDirty = (bool)values[3];

			//if( isConnected )
			//{
			//    if( !string.IsNullOrEmpty( databaseConnectionString ) )
			//    {
			//        dataSourceName = string.Format( ": {0}", databaseConnectionString );
			//    }
			//}
			//else
			//{
			//    if( !string.IsNullOrEmpty( filePath ) )
			//    {
			//        dataSourceName = string.Format( ": {0}{1}", filePath, isDirty ? "*" : "" );
			//    }
			//}

			return string.Format( "{0}{1}", parameter, connectionPath );
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

	public class UieTreeDataTemplateSelector : DataTemplateSelector
	{
		private DataTemplate _uieTemplate;
		private DataTemplate _valruleTemplate;
		private DataTemplate _elseruleTemplate;
		private DataTemplate _fillmapTemplate;
		private DataTemplate _elsemapTemplate;

		public UieTreeDataTemplateSelector()
			: base()
		{
		}

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if( _uieTemplate == null )
			{
				FrameworkElement el = ((FrameworkElement)container);

				_uieTemplate = el.FindResource( "UieItem" ) as DataTemplate;
				_valruleTemplate = el.FindResource( "RuleItem" ) as DataTemplate;
				_elseruleTemplate = el.FindResource( "ElseRuleItem" ) as DataTemplate;
				_fillmapTemplate = el.FindResource( "FillMapItem" ) as DataTemplate;
				_elsemapTemplate = el.FindResource( "ElseMapItem" ) as DataTemplate;
			}

			if( item != null )
			{
				if( item is api.UIElement )
					return _uieTemplate;
				else if( item is api.ValidationRule )
				{
					if( ((api.ValidationRule)item).LogicRuleType == sf.LogicRuleType.ValidationIf )
						return _valruleTemplate;
					else
						return _elseruleTemplate;
				}
				else if( item is api.FillMap )
				{
					if( ((api.FillMap)item).FillMapType == sf.FillMapType.FillMapIf )
						return _fillmapTemplate;
					else
						return _elsemapTemplate;
				}
			}

			return null;
		}
	}

	public class SecurityPrincipalDataTemplateSelector : DataTemplateSelector
	{
		DataTemplate _userTemplate = null;
		DataTemplate _groupTemplate = null;
		DataTemplate _extGroupTemplate = null;

		public SecurityPrincipalDataTemplateSelector()
			: base()
		{
		}

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if( _userTemplate == null )
			{
				FrameworkElement el = ((FrameworkElement)container);

				_userTemplate = el.FindResource( "UserItem" ) as DataTemplate;
				_groupTemplate = el.FindResource( "GroupItem" ) as DataTemplate;
				_extGroupTemplate = el.FindResource( "ExternalGroupItem" ) as DataTemplate;
			}

			if( item != null )
			{
				if( item is api.User )
				{
					return _userTemplate;
				}
				else
				{
					if( ((api.Group)item).IsLocal )
					{
						return _groupTemplate;
					}
					else
					{
						return _extGroupTemplate;
					}
				}
			}

			return null;
		}
	}

	public class GroupEqualityComparer : IEqualityComparer<api.GroupMembershipItem>
	{
		#region IEqualityComparer<GroupMembershipItem> Members

		public bool Equals(api.GroupMembershipItem x, api.GroupMembershipItem y)
		{
			return x.Group.Id == y.Group.Id;
		}

		public int GetHashCode(api.GroupMembershipItem obj)
		{
			return obj.GetHashCode();
		}

		#endregion
	}

	public class SecurityPrincipalTypeConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string[] images = parameter.ToString().Split( ',' );
			bool isUserObject = (bool)values[0];
			bool isLocal = (bool)values[1];
			bool isEnabled = (bool)values[2];

			string path = images[0];
			if( isUserObject )
			{
				path += isEnabled ? images[1] : images[2];
			}
			else
			{
				if( isLocal )
				{
					path += isEnabled ? images[3] : images[4];
				}
				else
				{
					path += isEnabled ? images[5] : images[6];
				}
			}

			Uri uriSource = new Uri( path, UriKind.Relative );
			return new System.Windows.Media.Imaging.BitmapImage( uriSource ); ;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

	public class SecurityPrincipalSourceConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool isUserObject = (bool)values[0];
			bool isLocal = (bool)values[1];
			return isUserObject ? string.Empty : isLocal ? "Suplex" : "External";
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}