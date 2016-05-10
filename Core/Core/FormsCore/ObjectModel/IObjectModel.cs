using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Runtime.Serialization;
using System.Windows.Data;
using System.Xml.Serialization;


namespace Suplex.Forms.ObjectModel.Api
{
	public interface ISuplexObject
	{
		[DataMember()]
		string ObjectId { get; }
		[DataMember()]
		string Name { get; set; }

		[IgnoreDataMember()]
		ObjectType ObjectType { get; }
	}
	public interface ISuplexObjectFactory
	{
		ISuplexObject CreateSuplexObjectBase(DataRow r);
	}
	public interface ISuplexObjectFactory<T> : ISuplexObjectFactory
	{
		void CreateObject(DataRow r, ref T record);
		T CreateObject(DataRow r);
	}

	public interface ISuplexObjectList
	{
		ISuplexObject AddOrSynchronize(ISuplexObject item);
	}

	public class ObservableObjectModelCollection<T> : ObservableCollection<T>	//todo:, ISuplexObjectList
	{
		private IObjectModel _owner = null;
		private CollectionViewSource _cvs = new CollectionViewSource();

		public ObservableObjectModelCollection()
			: base()
		{
			_cvs.Source = this;
		}
		public ObservableObjectModelCollection(IEnumerable<T> collection)
			: base( collection )
		{
			_cvs.Source = this;
			this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, collection ) );
		}
		public ObservableObjectModelCollection(IObjectModel owner)
			: this()
		{
			this.Owner = owner;
		}

		[XmlIgnore()]
		public CollectionViewSource CollectionViewSource { get { return _cvs; } }


		public IObjectModel Owner
		{
			get { return _owner; }
			set
			{
				_owner = value;
				this.EnsureOwner();
			}
		}

		//this method is necessary to ensure Owner when GetUnintializedObject is called instead of default constructor
		private void EnsureOwner()
		{
			foreach( IObjectModel item in this )
			{
				item.ParentObject = this.Owner;
			}
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			switch( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
				{
					this.SetOwner( e.NewItems, this.Owner );
					break;
				}
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
				{
					this.SetOwner( e.NewItems, this.Owner );
					this.SetOwner( e.OldItems, null );
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					this.SetOwner( e.OldItems, null );
					break;
				}
			}

			base.OnCollectionChanged( e );
		}

		private void SetOwner(IList items, IObjectModel owner)
		{
			if( items != null && items.Count > 0 && items[0] is IObjectModel )
			{
				for( int n = 0; n < items.Count; n++ )
				{
					((IObjectModel)items[n]).ParentObject = owner;

					if( items[n] is INotifyPropertyChanged && items[n] is INotifyDeleted )	//owner != null &&
					{
						((INotifyPropertyChanged)items[n]).PropertyChanged += new PropertyChangedEventHandler( this.Item_PropertyChanged );
					}
				}
			}
		}

		void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if( e.PropertyName == "IsDeleted" )
			{
				this.Remove( (T)sender );
			}
			if( this.Owner != null )
			{
				this.Owner.IsDirty = true;
			}
		}
	}

	public interface IObjectCollectionHost
	{
		void AddChildObject(IObjectModel child);
		void RemoveChildObject(IObjectModel child);
	}

	public interface INodeItem
	{
		bool IsExpanded { get; set; }
		bool IsSelected { get; set; }
		bool IsEditing { get; set; }
		bool ShowDetail { get; set; }
		bool ShowDetailPanels { get; set; }
		CompositeCollection ChildObjects { get; }
	}

	public interface INotifyDeleted
	{
		bool IsDeleted { get; set; }
	}
}