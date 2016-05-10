using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Data;
using System.Xml.Serialization;


using sf = Suplex.Forms;




namespace Suplex.Forms.ObjectModel.Api
{
	//TODO: implement INotifyPropertyChanged on all properties
	[DataContract()]
	public class FillMap : IObjectModel, ICloneable<FillMap>,
		INotifyPropertyChanged, INotifyPropertyChanging, INodeItem, INotifyDeleted, ISuplexObject
	{
		private string _name = string.Empty;
		private sf.ControlEvents _eventBinding = ControlEvents.Validating;
		private int _sortOrder = 0;
		private bool _isDirty = false;

		private long _nextId = System.DateTime.Now.Ticks;
		private long NextId { get { return _nextId++; } }

		public FillMap()
		{
			this.Ctor( new StreamingContext() );
		}

		[OnDeserializing()]
		void Ctor(StreamingContext context)
		{
			this.Id = this.NextId;

			this.DataBindings = new DataBindingCollectionEx<DataBinding>( this );
			//this.iDataBindingsTemp = new DataBindingCollectionEx<DataBinding>( this );
			this.ExprElements = new sf.ExpressionElements( null, sf.ExpressionType.None );

			this.DataBindings.CollectionChanged += new NotifyCollectionChangedEventHandler( this.Child_CollectionChanged );
		}

		//this method is necessary to ensure Owner as GetUnintializedObject is called instead of default constructor
		[OnDeserialized()]
		public void CtorPost(StreamingContext context)
		{
			this.DataBindings.Owner = this;
		}

		void Child_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.IsDirty = true;
			this.OnPropertyChanged( "Child_CollectionChanged" );
		}

		[DataMember()]
		public FillMapType FillMapType { get; set; }

		[DataMember()]
		public long Id { get; set; }

		[DataMember()]
		public string Name
		{
			get { return _name; }
			set
			{
				if( _name != value )
				{
					_name = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Name" );
				}
			}
		}

		[XmlIgnore()]
		[IgnoreDataMember()]
		public sf.ExpressionElements ExprElements { get; set; }

		[DataMember()]
		public string Expression
		{
			get { return this.ExprElements.Expression; }
			set
			{
				if( this.ExprElements.Expression != value )
				{
					this.ExprElements.Expression = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Expression" );
				}
			}
		}

		[DataMember()]
		public sf.ExpressionType ExpressionType
		{
			get { return this.ExprElements.ExprType; }
			set
			{
				if( this.ExprElements.ExprType != value )
				{
					this.ExprElements.ExprType = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "ExpressionType" );
				}
			}
		}

		[DataMember()]
		public sf.ControlEvents EventBinding
		{
			get { return _eventBinding; }
			set
			{
				if( _eventBinding != value )
				{
					_eventBinding = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "EventBinding" );
				}
			}
		}

		[XmlIgnore()]
		[IgnoreDataMember()]
		public object Data { get; set; }

		[DataMember()]
		public int SortOrder
		{
			get { return _sortOrder; }
			set
			{
				if( _sortOrder != value )
				{
					_sortOrder = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "SortOrder" );
				}
			}
		}

		/// <summary>
		///this is a work-around prop for DataBindings b/c:
		/////a) i cant figure out how to make the UI not update DataBindings automatically
		///b) the WpfToolkit DataGrid has a limitation that requires a generic argument
		///	  to provide a blank row for an empty collection
		/// </summary>
		[DataMember()]
		public DataBindingCollectionEx<DataBinding> DataBindings { get; set; }

		#region IObjectModel Members
		public ObjectType ObjectType
		{
			get
			{
				if( this.FillMapType == sf.FillMapType.FillMapIf )
				{
					return ObjectType.FillMap;
				}
				else
				{
					return ObjectType.ElseMap;
				}
			}
		}
		public ObjectType ValidChildObjectTypes { get { return ObjectType.None; } }
		public bool SupportsChildObjectType(ObjectType objectType)
		{
			return (this.ValidChildObjectTypes & objectType) == objectType;
		}
		[XmlIgnore()]
		public IObjectModel ParentObject { get; set; }
		[DataMember()]
		public Guid ParentId { get; set; }
		public void ResolveParent()
		{
			if( this.ParentObject != null )
			{
				this.ParentId = this.ParentObject.ObjectType == ObjectType.UIElement ?
					((UIElement)this.ParentObject).Id : ((ValidationRule)this.ParentObject).Id;
			}
		}
		#endregion

		[XmlIgnore()]
		public bool IsDirty
		{
			get { return _isDirty; }
			set
			{
				if( _isDirty != value )
				{
					_isDirty = value;
					this.OnPropertyChanged( "IsDirty" );
				}
			}
		}

		#region ICloneable<FillMap> Members
		public IObjectModel Clone(bool generateNewId)
		{
			FillMap clone = new FillMap();

			clone.Id = generateNewId ? this.NextId : this.Id;
			clone.Name = this.Name;
			clone.ExprElements = new sf.ExpressionElements( this.ExprElements.Expression, this.ExprElements.ExprType );
			clone.EventBinding = this.EventBinding;
			clone.SortOrder = this.SortOrder;
			clone.ParentObject = this.ParentObject;
			clone.FillMapType = this.FillMapType;

			foreach( DataBinding d in this.DataBindings )
			{
				clone.DataBindings.Add( d.Clone( generateNewId ) );
			}

			return clone;
		}

		public FillMap Clone()
		{
			return (FillMap)this.Clone( false );
		}

		FillMap ICloneable<FillMap>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		private List<long> _deleteIds = null;
		public void Synchronize(FillMap sourceObject)
		{
			this.Name = sourceObject.Name;
			this.Expression = sourceObject.Expression;
			this.ExpressionType = sourceObject.ExpressionType;
			this.EventBinding = sourceObject.EventBinding;
			this.SortOrder = sourceObject.SortOrder;

			//purge the DataBindings from |this| list that no longer exist in the sourceObject's DataBinding list.
			_deleteIds = new List<long>();	//this might be a HACK
			for( int i = this.DataBindings.Count - 1; i > -1; i-- )
			{
				DataBinding found = sourceObject.DataBindings.GetByDataBindingId( this.DataBindings[i].Id );
				if( found == null )
				{
					_deleteIds.Add( this.DataBindings[i].Id );
					this.DataBindings.RemoveAt( i );
				}
			}
			foreach( DataBinding d in sourceObject.DataBindings )
			{
				DataBinding found = this.DataBindings.GetByDataBindingId( d.Id );
				if( found != null )
				{
					found.Synchronize( d );
				}
				else
				{
					this.DataBindings.Add( d.Clone() );
				}
			}
		}

		//This is a special sync only to be used after a database Create/Update statement.
		//It's needed to sync the Ids on Child collections - DataBindings, in this case
		public void SynchronizeSpecial(FillMap sourceObject)
		{
			for( int i = this.DataBindings.Count - 1; i > -1; i-- )
			{
				DataBinding found = sourceObject.DataBindings.GetByDataBindingId( this.DataBindings[i].Id );
				if( found == null )
				{
					this.DataBindings.RemoveAt( i );
				}
			}

			foreach( DataBinding d in sourceObject.DataBindings )
			{
				DataBinding found = this.DataBindings.GetByDataBindingId( d.Id );
				if( found != null )
				{
					found.Synchronize( d );
					found.IsDirty = false;
				}
				else
				{
					this.DataBindings.Add( d.Clone() );
				}
			}

			this.IsDirty = false;
		}

		void ICloneable<FillMap>.Synchronize(FillMap sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region INotifyPropertyChanged, INotifyPropertyChanging Members
		public event PropertyChangedEventHandler PropertyChanged;
		public event PropertyChangingEventHandler PropertyChanging;

		protected void OnPropertyChanged(string propertyName)
		{
			if( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
			}
		}

		protected void OnPropertyChanging(string propertyName)
		{
			if( PropertyChanging != null )
			{
				PropertyChanging( this, new PropertyChangingEventArgs( propertyName ) );
			}
		}
		#endregion

		#region INodeItem Members
		private bool _isExpanded = false;
		private bool _isSelected = false;
		private bool _isEditing = false;
		private bool _showDetail = false;
		private bool _showDetailPanels = false;
		[XmlIgnore()]
		public bool IsExpanded
		{
			get { return _isExpanded; }
			set
			{
				_isExpanded = value;
				this.OnPropertyChanged( "IsExpanded" );
			}
		}
		[XmlIgnore()]
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				_isSelected = value;
				this.OnPropertyChanged( "IsSelected" );
			}
		}
		[XmlIgnore()]
		public bool IsEditing
		{
			get { return _isEditing; }
			set
			{
				_isEditing = value;
				this.OnPropertyChanged( "IsEditing" );
			}
		}
		[XmlIgnore()]
		public bool ShowDetail
		{
			get { return _showDetail; }
			set
			{
				_showDetail = value;
				this.OnPropertyChanged( "ShowDetail" );
			}
		}
		[XmlIgnore()]
		public bool ShowDetailPanels
		{
			get { return _showDetailPanels; }
			set
			{
				_showDetailPanels = value;
				this.OnPropertyChanged( "ShowDetailPanels" );
			}
		}
		CompositeCollection INodeItem.ChildObjects
		{
			get { return null; }
		}

		#endregion

		#region INotifyDeleted Members
		private bool _isDeleted = false;
		[XmlIgnore()]
		public bool IsDeleted
		{
			get { return _isDeleted; }
			set
			{
				if( _isDeleted != value )
				{
					_isDeleted = value;
					this.OnPropertyChanged( "IsDeleted" );
				}
			}
		}
		#endregion

		#region ISuplexObject Members
		public string ObjectId
		{
			get { return this.Id.ToString(); }
		}
		#endregion
	}

	public class LazyLoadDummyFillMap : FillMap { }

	[CollectionDataContract()]
	public class FillMapCollection : ObservableObjectModelCollection<FillMap>, ISuplexObjectList
	{
		public FillMapCollection() : base() { }
		public FillMapCollection(IObjectModel owner)
			: base( owner )
		{ }

		public FillMap GetByFillMapId(long id)
		{
			return this.SingleOrDefault( fm => fm.Id == id );
		}

		public ISuplexObject AddOrSynchronize(ISuplexObject item)
		{
			FillMap fm = (FillMap)item;

			FillMap exists = this.GetByFillMapId( fm.Id );
			if( exists == null )
			{
				this.Add( fm );
			}
			else
			{
				exists.Synchronize( fm );
				exists.IsDirty = false;
				fm = exists;
			}

			return fm;
		}
	}
}