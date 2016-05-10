using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using sf = Suplex.Forms;
using sg = Suplex.General;
using api = Suplex.Forms.ObjectModel.Api;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace SuplexApp
{
	interface ISuplexEditorDialog
	{
		api.SuplexStore SplxStore { get; set; }
		api.SuplexApiClient ApiClient { get; set; }
		bool VerifySaveChanges();
		void SaveIfDirty();
		void ClearContentPanel();
	}

	interface IEditorControl
	{
		event EventHandler SavedChanges;
		event EventHandler CancelledChanges;
		bool SaveChanges();
		sf.IObjectModel SourceObject { get; set; }

		api.SuplexStore SplxStore { get; set; }
		api.SuplexApiClient ApiClient { get; set; }
	}

	interface IEditorControl<T> : IEditorControl
	{
		void SetDataContext(T dataContext);
	}

	public class SuplexTreeView : TreeView
	{
		public SuplexTreeView() : base() { }

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new SuplexTreeViewItem();
		}
	}

	public class SuplexTreeViewItem : TreeViewItem, INotifyPropertyChanged
	{
        private bool _showDetail = false;
		private bool _showDetailPanels = false;
		private bool _isEditing = false;
		private bool _isMouseOver = false;
		private bool _handlePropChangedEvent = true;

		public SuplexTreeViewItem() : base() { }

		public SuplexTreeViewItem(sf.IObjectModel data, DataTemplate headerTemplate)
			: base()
		{
			_handlePropChangedEvent = false;
			this.Header = data;
			this.HeaderTemplate = headerTemplate;
		}

		public sf.IObjectModel HeaderObject { get { return (sf.IObjectModel)this.Header; } }

		protected override void OnHeaderChanged(object oldHeader, object newHeader)
		{
			if( _handlePropChangedEvent &&
				newHeader is api.INodeItem && newHeader is INotifyPropertyChanged )
			{
				((INotifyPropertyChanged)newHeader).PropertyChanged += new PropertyChangedEventHandler( HeaderItem_PropertyChanged );
			}

			base.OnHeaderChanged( oldHeader, newHeader );
		}

		void HeaderItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch( e.PropertyName )
			{
				case "IsExpanded":
				{
					//this.IsExpanded = ((Suplex.Forms.ObjectModel.INodeItem)sender).IsExpanded;
					break;
				}
				case "IsSelected":
				{
					//this.IsSelected = ((Suplex.Forms.ObjectModel.INodeItem)sender).IsSelected;
					break;
				}
				case "IsEditing":
				{
					this.IsEditing = ((api.INodeItem)sender).IsEditing;
					break;
				}
				case "ShowDetail":
				{
					this.ShowDetail = ((api.INodeItem)sender).ShowDetail;
					break;
				}
				case "ShowDetailPanels":
				{
					this.ShowDetailPanels = ((api.INodeItem)sender).ShowDetailPanels;
					break;
				}
			}
		}

        public bool ShowDetail
        {
            get { return _showDetail; }
            set
            {
                if( _showDetail != value )
                {
                    _showDetail = value;
                    this.OnPropertyChanged( "ShowDetail" );
                }
            }
        }
		public bool ShowDetailPanels
		{
			get { return _showDetailPanels; }
			set
			{
				if( _showDetailPanels != value )
				{
					_showDetailPanels = value;
					this.OnPropertyChanged( "ShowDetailPanels" );
				}
			}
		}
		public bool IsEditing
        {
			get { return _isEditing; }
            set
            {
				if( _isEditing != value )
                {
					_isEditing = value;
					this.OnPropertyChanged( "IsEditing" );
                }
            }
        }

		public bool IsMouseOnItem
		{
			get { return this.IsSelected || this.IsMouseOver; }
		    set
		    {
		        if( _isMouseOver != value )
		        {
		            _isMouseOver = value;
		            this.OnPropertyChanged( "IsMouseOnItem" );
		        }
		    }
		}
		public void MouseOut()
		{
			this.OnPropertyChanged( "IsMouseOnItem" );
		}

		protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
		{
		    this.IsMouseOnItem = true;
		    base.OnMouseEnter( e );
		}
		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
		{
		    this.IsMouseOnItem = false;
		    base.OnMouseLeave( e );
		}

		//[Obsolete( "Obsolete", true )]
		//public DependencyObject FindExpander() { return sg.MiscUtils.GetChild<Expander>( this ); }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }
        #endregion

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new SuplexTreeViewItem();
		}
	}

	public class PreviewControl : UserControl
	{
		public static readonly DependencyProperty IsDetailExpandedProperty = DependencyProperty.Register( "IsDetailExpanded", typeof( bool? ), typeof( PreviewControl ) );

		public bool? IsDetailExpanded
		{
			get { return GetValue( IsDetailExpandedProperty ) as bool?; }
			set { SetValue( IsDetailExpandedProperty, value ); }
		}
	}

	public class SuplexMenuItem : MenuItem, api.ISuplexObject
	{
		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register( "Image", typeof( ImageSource ), typeof( SuplexMenuItem ) );

		public ImageSource Image
		{
			get { return GetValue( ImageProperty ) as ImageSource; }
			set { SetValue( ImageProperty, value ); }
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if( e.Property == ImageProperty )
			{
				this.Icon = new Image
				{
					Source = this.Image,
					Height = 16,
					Width = 16
				};
			}
			base.OnPropertyChanged( e );
		}

		#region ISuplexObject Members
		string api.ISuplexObject.ObjectId
		{
			get { return null; }
		}

		public sf.ObjectType ObjectType
		{
			get;
			set;
		}
		#endregion
	}
}