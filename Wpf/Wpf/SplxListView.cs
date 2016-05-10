using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Suplex.Wpf
{
	public class SplxListView : ListView
	{
		public static readonly DependencyProperty HasSelectedItemProperty = DependencyProperty.Register( "HasSelectedItem", typeof( bool? ), typeof( SplxListView ) );

		public SplxListView()
			: base()
		{
			this.HasSelectedItem = false;
		}

		public bool? HasSelectedItem
		{
			get { return GetValue( HasSelectedItemProperty ) as bool?; }
			internal set { SetValue( HasSelectedItemProperty, value ); }
		}

		public void RefreshHasSelectedItem()
		{
			this.HasSelectedItem = this.SelectedItem != null;
		}

		protected override void OnSelectionChanged(SelectionChangedEventArgs e)
		{
			this.HasSelectedItem = this.SelectedItem != null;
			base.OnSelectionChanged( e );
		}
	}
}
