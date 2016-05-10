using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;


namespace Suplex.Forms.ObjectModel.Api
{
	[DataContract()]
	public class DataBinding : INotifyPropertyChanged, ICloneable<DataBinding>, ISuplexObject
	{
		private string _controlName = string.Empty;
		private string _propertyName = string.Empty;
		private string _dataMember = string.Empty;
		private bool _overrideValue = false;
		private bool _isDirty = false;

		private long _nextId = System.DateTime.Now.Ticks;
		private long NextId { get { return _nextId++; } }

		public DataBinding()
		{
			this.Id = this.NextId;
		}

		[DataMember()]
		public long Id { get; set; }

		[DataMember()]
		public string ControlName
		{
			get { return _controlName; }
			set
			{
				if( _controlName != value )
				{
					_controlName = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "ControlName" );
				}
			}
		}

		[DataMember()]
		public string PropertyName
		{
			get { return _propertyName; }
			set
			{
				if( _propertyName != value )
				{
					_propertyName = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "PropertyName" );
				}
			}
		}

		[XmlElement( "Value" )]
		[DataMember( Name = "Value" )]
		public string DataMember
		{
			get { return _dataMember; }
			set
			{
				if( _dataMember != value )
				{
					_dataMember = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "DataMember" );
				}
			}
		}
		[XmlIgnore]
		public bool ConversionRequired { get; set; }

		[DataMember()]
		public bool OverrideValue
		{
			get { return _overrideValue; }
			set
			{
				if( _overrideValue != value )
				{
					_overrideValue = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "OverrideValue" );
				}
			}
		}

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

		public override string ToString()
		{
			return string.Format( "Id: {4}, ControlName: {0}, PropertyName: {1}, DataMember: {2}, OverrideValue: {3}",
				this.ControlName, this.PropertyName, this.DataMember, this.OverrideValue, this.Id );
		}

		#region ICloneable<DataBinding> Members
		IObjectModel ICloneableObject.Clone(bool generateNewId)
		{
			throw new NotImplementedException();
		}
		public DataBinding Clone(bool generateNewId)
		{
			DataBinding clone = this.Clone();
			if( generateNewId )
			{
				clone.Id = this.NextId;
			}
			return clone;
		}

		public DataBinding Clone()
		{
			DataBinding d = new DataBinding();

			d.Id = this.Id;
			d.ControlName = this.ControlName;
			d.PropertyName = this.PropertyName;
			d.DataMember = this.DataMember;
			d.OverrideValue = this.OverrideValue;

			return d;
		}

		DataBinding ICloneable<DataBinding>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		public void Synchronize(DataBinding sourceObject)
		{
			this.Id = sourceObject.Id;
			this.ControlName = sourceObject.ControlName;
			this.PropertyName = sourceObject.PropertyName;
			this.DataMember = sourceObject.DataMember;
			this.OverrideValue = sourceObject.OverrideValue;
		}

		void ICloneable<DataBinding>.Synchronize(DataBinding sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}
		#endregion

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

		#region ISuplexObject Members
		public string ObjectId
		{
			get { return this.Id.ToString(); }
		}

		string ISuplexObject.Name
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public ObjectType ObjectType
		{
			get { return ObjectType.None; }
		}
		#endregion
	}

	[CollectionDataContract()]
	public class DataBindingCollection : ObservableObjectModelCollection<DataBinding>, ISuplexObjectList
	{
		public DataBindingCollection() : base() { }
		public DataBindingCollection(IObjectModel owner)
			: base( owner )
		{ }

		public DataBinding GetByDataBindingId(long id)
		{
			return this.SingleOrDefault( db => db.Id == id );
		}

		#region ISuplexObjectList Members
		public ISuplexObject AddOrSynchronize(ISuplexObject item)
		{
			DataBinding db = (DataBinding)item;

			DataBinding exists = this.GetByDataBindingId( db.Id );
			if( exists == null )
			{
				this.Add( db );
			}
			else
			{
				exists.Synchronize( db );
				exists.IsDirty = false;
				db = exists;
			}

			return db;
		}
		#endregion
	}

	[CollectionDataContract()]
	public class DataBindingCollectionEx<T> : DataBindingCollection where T : DataBinding
	{
		public DataBindingCollectionEx() : base() { }
		public DataBindingCollectionEx(IObjectModel owner)
			: base( owner )
		{ }
	}
}