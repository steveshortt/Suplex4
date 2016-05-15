using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Data;

namespace Suplex.Wpf
{
	public partial class DualListView : UserControl
	{
		private List<object> _itemsMovedLeft = null;
		private List<object> _itemsMovedRight = null;

		#region events
		public event EventHandler MoveLeftClick;
		public event EventHandler MoveRightClick;

		protected void OnMoveLeftClick(object sender, EventArgs e)
		{
			if( MoveLeftClick != null )
			{
				this.MoveLeftClick( sender, e );
			}
		}
		protected void OnMoveRightClick(object sender, EventArgs e)
		{
			if( MoveRightClick != null )
			{
				this.MoveRightClick( sender, e );
			}
		}
		#endregion

		public DualListView()
		{
			InitializeComponent();

			_itemsMovedLeft = new List<object>();
			_itemsMovedRight = new List<object>();
		}

		#region Dependency Properties
		public static readonly DependencyProperty LeftHeaderProperty = DependencyProperty.Register( "LeftHeader", typeof( object ), typeof( DualListView ) );
		public static readonly DependencyProperty LeftListItemTemplateProperty = DependencyProperty.Register( "LeftListItemTemplate", typeof( DataTemplate ), typeof( DualListView ) );
		public static readonly DependencyProperty LeftListViewProperty = DependencyProperty.Register( "LeftListView", typeof( ViewBase ), typeof( DualListView ) );
		public static readonly DependencyProperty LeftListDataContextProperty = DependencyProperty.Register( "LeftListDataContext", typeof( object ), typeof( DualListView ) );
		public static readonly DependencyProperty RightHeaderProperty = DependencyProperty.Register( "RightHeader", typeof( object ), typeof( DualListView ) );
		public static readonly DependencyProperty RightListItemTemplateProperty = DependencyProperty.Register( "RightListItemTemplate", typeof( DataTemplate ), typeof( DualListView ) );
		public static readonly DependencyProperty RightListViewProperty = DependencyProperty.Register( "RightListView", typeof( ViewBase ), typeof( DualListView ) );
		public static readonly DependencyProperty RightListDataContextProperty = DependencyProperty.Register( "RightListDataContext", typeof( object ), typeof( DualListView ) );
		public static readonly DependencyProperty AutoMoveItemsProperty = DependencyProperty.Register( "AutoMoveItems", typeof( bool? ), typeof( DualListView ) );
		public static readonly DependencyProperty TrackAutoMovedItemsProperty = DependencyProperty.Register( "TrackAutoMovedItems", typeof( bool? ), typeof( DualListView ) );
		public static readonly DependencyProperty IsDirtyProperty = DependencyProperty.Register( "IsDirty", typeof( bool? ), typeof( DualListView ) );

		public object LeftHeader
		{
			get { return GetValue( LeftHeaderProperty ) as object; }
			set { SetValue( LeftHeaderProperty, value ); }
		}

		public DataTemplate LeftListItemTemplate
		{
			get { return GetValue( LeftListItemTemplateProperty ) as DataTemplate; }
			set { SetValue( LeftListItemTemplateProperty, value ); }
		}

		public ViewBase LeftListView
		{
			get { return GetValue( LeftListViewProperty ) as ViewBase; }
			set { SetValue( LeftListViewProperty, value ); }
		}

		public object LeftListDataContext
		{
			get { return GetValue( LeftListDataContextProperty ) as object; }
			set { SetValue( LeftListDataContextProperty, value ); }
		}
		private IList leftListDataContext
		{
			get
			{
				if( lstLeft.DataContext is IList )
				{
					return (IList)lstLeft.DataContext;
				}
				else if( lstLeft.DataContext is CollectionViewSource )
				{
					return (IList)((CollectionViewSource)lstLeft.DataContext).Source;
				}
				else
				{
					throw new NotSupportedException( "DataContext type not IList or CollectionViewSource" );
				}
			}
		}

		public object RightHeader
		{
			get { return GetValue( RightHeaderProperty ) as object; }
			set { SetValue( RightHeaderProperty, value ); }
		}

		public DataTemplate RightListItemTemplate
		{
			get { return GetValue( RightListItemTemplateProperty ) as DataTemplate; }
			set { SetValue( RightListItemTemplateProperty, value ); }
		}

		public ViewBase RightListView
		{
			get { return GetValue( RightListViewProperty ) as ViewBase; }
			set { SetValue( RightListViewProperty, value ); }
		}

		public object RightListDataContext
		{
			get { return GetValue( RightListDataContextProperty ) as object; }
			set { SetValue( RightListDataContextProperty, value ); }
		}
		private IList rightListDataContext
		{
			get
			{
				if( lstRight.DataContext is IList )
				{
					return (IList)lstRight.DataContext;
				}
				else if( lstRight.DataContext is CollectionViewSource )
				{
					return (IList)((CollectionViewSource)lstRight.DataContext).Source;
				}
				else
				{
					throw new NotSupportedException( "DataContext type not IList or CollectionViewSource" );
				}
			}
		}

		public bool? AutoMoveItems
		{
			get { return GetValue( AutoMoveItemsProperty ) as bool?; }
			set { SetValue( AutoMoveItemsProperty, value ); }
		}

		public bool? TrackAutoMovedItems
		{
			get { return GetValue( TrackAutoMovedItemsProperty ) as bool?; }
			set { SetValue( TrackAutoMovedItemsProperty, value ); }
		}

		public bool? IsDirty
		{
			get { return GetValue( IsDirtyProperty ) as bool?; }
			set { SetValue( IsDirtyProperty, value ); }
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if( e.Property == LeftHeaderProperty )
			{
				leftHeader.Content = e.NewValue;
			}
			else if( e.Property == LeftListItemTemplateProperty )
			{
				lstLeft.ItemTemplate = (DataTemplate)e.NewValue;
			}
			else if( e.Property == LeftListViewProperty )
			{
				lstLeft.View = (ViewBase)e.NewValue;
			}
			else if( e.Property == LeftListDataContextProperty )
			{
				lstLeft.DataContext = e.NewValue;
				_itemsMovedLeft.Clear();
				_itemsMovedRight.Clear();
			}
			else if( e.Property == RightHeaderProperty )
			{
				rightHeader.Content = e.NewValue;
			}
			else if( e.Property == RightListItemTemplateProperty )
			{
				lstRight.ItemTemplate = (DataTemplate)e.NewValue;
			}
			else if( e.Property == RightListViewProperty )
			{
				lstRight.View = (ViewBase)e.NewValue;
			}
			else if( e.Property == RightListDataContextProperty )
			{
				lstRight.DataContext = e.NewValue;
				_itemsMovedLeft.Clear();
				_itemsMovedRight.Clear();
			}

			base.OnPropertyChanged( e );
		}
		#endregion

		#region Public Properties/Methods
		public IList LeftListSelectedItems { get { return lstLeft.SelectedItems; } }
		public IList RightListSelectedItems { get { return lstRight.SelectedItems; } }
		public List<object> ItemsMovedLeft { get { return _itemsMovedLeft; } }
		public List<object> ItemsMovedRight { get { return _itemsMovedRight; } }
		public void MoveItemLeft(object item, bool trackMovedItem)
		{
			if( trackMovedItem )
			{
				_itemsMovedLeft.Add( item );
				_itemsMovedRight.Remove( item );
			}

			leftListDataContext.Add( item );
			rightListDataContext.Remove( item );

			this.IsDirty = true;
		}
		public void MoveItemRight(object item, bool trackMovedItem)
		{
			if( trackMovedItem )
			{
				_itemsMovedRight.Add( item );
				_itemsMovedLeft.Remove( item );
			}

			rightListDataContext.Add( item );
			leftListDataContext.Remove( item );

			this.IsDirty = true;
		}
		#endregion

		#region handlers
		private void cmdMoveLeft_Click(object sender, RoutedEventArgs e)
		{
			if( this.AutoMoveItems == true )
			{
				bool movingSomething = lstRight.SelectedItems.Count > 0;

				object item = null;
				for( int i = lstRight.SelectedItems.Count - 1; i >= 0; i-- )
				{
					item = lstRight.SelectedItems[i];

					if( this.TrackAutoMovedItems == true )
					{
						_itemsMovedLeft.Add( item );
						_itemsMovedRight.Remove( item );
					}

					leftListDataContext.Add( item );
					rightListDataContext.Remove( item );
				}

				if( movingSomething )
				{
					this.IsDirty = true;
				}
			}

			this.OnMoveLeftClick( this, EventArgs.Empty );
		}

		private void lstRight_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			this.cmdMoveLeft_Click( cmdMoveLeft, null );
		}

		private void cmdMoveRight_Click(object sender, RoutedEventArgs e)
		{
			if( this.AutoMoveItems == true )
			{
				bool movingSomething = lstRight.SelectedItems.Count > 0;

				object item = null;
				for( int i = lstLeft.SelectedItems.Count - 1; i >= 0; i-- )
				{
					item = lstLeft.SelectedItems[i];

					if( this.TrackAutoMovedItems == true )
					{
						_itemsMovedRight.Add( item );
						_itemsMovedLeft.Remove( item );
					}

					rightListDataContext.Add( item );
					leftListDataContext.Remove( item );
				}

				if( movingSomething )
				{
					this.IsDirty = true;
				}
			}

			this.OnMoveRightClick( this, EventArgs.Empty );
		}

		private void lstLeft_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			this.cmdMoveRight_Click( cmdMoveRight, null );
		}
		#endregion
	}
}