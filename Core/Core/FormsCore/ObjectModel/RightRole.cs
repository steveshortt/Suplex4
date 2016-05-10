using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Data;
using System.Xml;
using System.Xml.Serialization;

using sf = Suplex.Forms;
using ss = Suplex.Security;



namespace Suplex.Forms.ObjectModel.Api
{
	[DataContract()]
	public class RightRole : IObjectModel, INotifyPropertyChanged, ICloneable<RightRole>, INotifyDeleted, ISuplexObject
	{
		private UIElement _uielement = null;
		private string _controlUniqueName = string.Empty;
		private ss.AceType _aceType = ss.AceType.None;
		private string _rightName = string.Empty;
		private ss.UIRight _uiRight = ss.UIRight.Visible;
		private object _right = null;
		private bool _isDirty = false;
		private bool _isDeleted = false;

		private long _nextId = System.DateTime.Now.Ticks;
		private long NextId { get { return _nextId++; } }

		public RightRole()
		{
			this.RightRoleType = RightRoleType.Success;
			this.Id = this.NextId;
		}

		[DataMember()]
		public long Id { get; set; }

		//[XmlIgnore]
		[DataMember()]
		public RightRoleType RightRoleType { get; set; }

		[XmlIgnore]
		[IgnoreDataMember()]
		public UIElement UIElement
		{
			get
			{
				return _uielement;
			}
			set
			{
				_uielement = value;
				this.ControlUniqueName = value.UniqueName;
			}
		}

		[DataMember()]
		public string ControlUniqueName
		{
			get { return _controlUniqueName; }
			set
			{
				if( _controlUniqueName != value )
				{
					_controlUniqueName = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "ControlUniqueName" );
				}
			}
		}

		[DataMember()]
		public ss.AceType AceType
		{
			get { return _aceType; }
			set
			{
				if( _aceType != value )
				{
					_aceType = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "AceType" );
				}
			}
		}

		[XmlIgnore]
		[IgnoreDataMember()]
		public object Right
		{
			get
			{
				if( _right == null )
				{
					ss.AceTypeRights atr = new ss.AceTypeRights( _aceType );
					return Enum.Parse( atr.Right, _rightName );
				}
				else
				{
					return _right;
				}
			}
			set
			{
				if( _right != value )
				{
					_right = value;
					//this.IsDirty = true;
					this.RightName = _right.ToString();
				}
			}
		}

		[XmlElement( "SourceRight" )]
		[DataMember( Name = "SourceRight" )]
		public string RightName
		{
			get { return _rightName; }
			set
			{
				if( _rightName != value )
				{
					_rightName = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "RightName" );
				}
			}
		}

		[XmlElement( "DestinationRight" )]
		[DataMember( Name = "DestinationRight" )]
		public ss.UIRight UIRight
		{
			get { return _uiRight; }
			set
			{
				if( _uiRight != value )
				{
					_uiRight = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "UIRight" );
				}
			}
		}


		#region IObjectModel Members
		public string Name { get { return null; } set { } }
		public ObjectType ObjectType { get { return ObjectType.RightRole; } }
		public ObjectType ValidChildObjectTypes { get { return ObjectType.None; } }
		public bool SupportsChildObjectType(ObjectType objectType)
		{
			return (this.ValidChildObjectTypes & objectType) == objectType;
		}
		[XmlIgnore()]
		public IObjectModel ParentObject { get; set; }

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

		#region ICloneable<RightRole> Members
		public IObjectModel Clone(bool generateNewId)
		{
			RightRole clone = this.Clone();
			if( generateNewId )
			{
				clone.Id = this.NextId;
			}
			return clone;
		}

		public RightRole CloneMemberwise()
		{
			return (RightRole)this.MemberwiseClone();
		}

		public RightRole Clone()
		{
			RightRole rr = new RightRole();

			rr.Id = this.Id;
			rr.ControlUniqueName = this.ControlUniqueName;
			rr.AceType = this.AceType;
			rr.RightName = this.RightName;
			rr.UIRight = this.UIRight;
			rr.RightRoleType = this.RightRoleType;

			return rr;
		}

		RightRole ICloneable<RightRole>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		public void Synchronize(RightRole sourceObject)
		{
			this.ControlUniqueName = sourceObject.ControlUniqueName;
			this.AceType = sourceObject.AceType;
			this.RightName = sourceObject.RightName;
			this.UIRight = sourceObject.UIRight;
			this.RightRoleType = sourceObject.RightRoleType;
		}

		void ICloneable<RightRole>.Synchronize(RightRole sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region INotifyDeleted Members
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
			get { return this.Id.ToString(); ; }
		}
		#endregion

		public override string ToString()
		{
			return string.Format( "{0}->{1}\\{2}", this.RightName, this.ControlUniqueName, this.UIRight );
		}
	}

	[CollectionDataContract()]
	public class RightRoleCollection : ObservableObjectModelCollection<RightRole>, ISuplexObjectList
	{
		public RightRoleCollection() : base() { }
		public RightRoleCollection(IObjectModel owner)
			: base( owner )
		{ }
		internal SuplexStore OwnerStore { get; set; }

		public RightRole GetByRightRoleId(long id)
		{
			return this.SingleOrDefault( rr => rr.Id == id );
		}

		public ISuplexObject AddOrSynchronize(ISuplexObject item)
		{
			RightRole role = (RightRole)item;

			RightRole exists = this.GetByRightRoleId( role.Id );
			if( exists == null )
			{
				this.Add( role );
			}
			else
			{
				exists.Synchronize( role );
				exists.IsDirty = false;
				role = exists;
			}

			return role;
		}
	}

	[DataContract()]
	public class RightRoleRule : LogicRule, IObjectModel, ICloneable<RightRoleRule>, INotifyDeleted
	{
		private IObjectModel _parentObject = null;
		private bool _isDeleted = false;

		CompositeCollection _childObjects = null;

		public RightRoleRule()
			: base()
		{
			this.Ctor( new StreamingContext() );
		}

		[OnDeserializing()]
		void Ctor(StreamingContext context)
		{
			this.LogicRuleType = sf.LogicRuleType.RightRoleIf;

			this.RightRoleRules = new RightRoleRuleCollection( this );
			this.ElseRules = new RightRoleRuleCollection( this );
			this.RightRoles = new RightRoleCollection( this );
			this.ElseRoles = new RightRoleCollection( this );

			this.IsSealed = false;
		}

		//this method is necessary to ensure Owner as GetUnintializedObject is called instead of default constructor
		[OnDeserialized()]
		public void CtorPost(StreamingContext context)
		{
			this.RightRoleRules.Owner = this;
			this.ElseRules.Owner = this;
			this.RightRoles.Owner = this;
			this.ElseRoles.Owner = this;
		}

		[XmlIgnore()]
		public bool IsSealed { get; internal set; }

		[XmlIgnore()]
		public override ObjectType LogicRuleObjectType { get { return ObjectType.RightRoleRule; } }

		[DataMember()]
		public RightRoleRuleCollection RightRoleRules { get; set; }
		[DataMember()]
		public RightRoleCollection RightRoles { get; set; }
		[DataMember()]
		public RightRoleRuleCollection ElseRules { get; set; }
		[DataMember()]
		public RightRoleCollection ElseRoles { get; set; }

		[XmlIgnore()]
		[IgnoreDataMember()]
		public CompositeCollection ChildObjects
		{
			get
			{
				this.EnsureChildObjects();
				return _childObjects;
			}
		}
		void EnsureChildObjects()
		{
			if( _childObjects == null )
			{
				CollectionContainer rrr = new CollectionContainer() { Collection = this.RightRoleRules };
				CollectionContainer eru = new CollectionContainer() { Collection = this.ElseRules };
				CollectionContainer rro = new CollectionContainer() { Collection = this.RightRoles };
				CollectionContainer ero = new CollectionContainer() { Collection = this.ElseRoles };

				_childObjects = new CompositeCollection();
				_childObjects.Add( rro );
				_childObjects.Add( ero );
				_childObjects.Add( rrr );
				_childObjects.Add( eru );
			}
		}

		public int CompositeRuleCount
		{
			get
			{
				return this.RightRoleRules.CompositeRuleCount +
					this.ElseRules.CompositeRuleCount;
			}
		}

		public int CompositeRoleCount
		{
			get
			{
				return this.RightRoles.Count +
					this.ElseRoles.Count;
			}
		}

		#region IObjectModel Members
		public ObjectType ObjectType { get { return ObjectType.RightRoleRule; } }
		public ObjectType ValidChildObjectTypes { get { return ObjectType.RightRoleRule; } }
		public bool SupportsChildObjectType(ObjectType objectType)
		{
			return (this.ValidChildObjectTypes & objectType) == objectType;
		}
		[XmlIgnore()]
		public IObjectModel ParentObject
		{
			get { return _parentObject; }
			set
			{
				if( _parentObject != value )
				{
					this.OnPropertyChanging( "ParentObject" );
					_parentObject = value;
					this.OnPropertyChanged( "ParentObject" );
				}
			}
		}
		#endregion

		#region IObjectCollectionHost Members
		public void AddChildObject(IObjectModel child)
		{
			switch( child.ObjectType )
			{
				case ObjectType.RightRoleRule:
				{
					this.RightRoleRules.Add( (RightRoleRule)child );
					break;
				}
			}

			child.ParentObject = this;
		}
		public void RemoveChildObject(IObjectModel child)
		{
			switch( child.ObjectType )
			{
				case ObjectType.RightRoleRule:
				{
					this.RightRoleRules.Remove( (RightRoleRule)child );
					break;
				}
			}

			child.ParentObject = null;
		}
		#endregion

		#region ICloneable<RightRoleRule> Members
		//TODO: verify that this works; compare to ValidationRule
		public IObjectModel Clone(bool generateNewId)
		{
			RightRoleRule clone = this.Clone();
			if( generateNewId )
			{
				clone.Id = Guid.NewGuid();
			}
			return clone;
		}

		public RightRoleRule CloneMemberwise()
		{
			return (RightRoleRule)this.MemberwiseClone();
		}

		//method-based recursion
		public RightRoleRule Clone()
		{
			RightRoleRule clone = (RightRoleRule)base.Clone( ObjectType.RightRoleRule );

			foreach( RightRoleRule rrr in this.RightRoleRules )
			{
				clone.RightRoleRules.Add( rrr.Clone() );
			}

			foreach( RightRole rr in this.RightRoles )
			{
				clone.RightRoles.Add( rr.CloneMemberwise() );
			}

			foreach( RightRoleRule rrr in this.ElseRules )
			{
				clone.ElseRules.Add( rrr.Clone() );
			}

			foreach( RightRole rr in this.ElseRoles )
			{
				clone.ElseRoles.Add( rr.CloneMemberwise() );
			}

			return clone;
		}

		RightRoleRule ICloneable<RightRoleRule>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		public void Synchronize(RightRoleRule sourceObject)
		{
			List<long> _deleteRightRoleIds = null;
			List<Guid> _deleteRrRuleIds = null;
			this.SynchronizeRecursive( sourceObject, ref _deleteRrRuleIds, ref _deleteRightRoleIds );
		}

		//method-based recursion
		public void SynchronizeRecursive(RightRoleRule sourceObject, ref List<Guid> deleteRrRuleIds, ref List<long> deleteRightRoleIds)
		{
			base.Synchronize( sourceObject );

			//purge the iRightRoleRules from |this| list that no longer exist in the sourceObject's iRightRoleRules list.
			for( int i = this.RightRoleRules.Count - 1; i > -1; i-- )
			{
				RightRoleRule found = sourceObject.RightRoleRules.GetByRightRoleRuleId( this.RightRoleRules[i].Id );
				if( found == null )
				{
					if( deleteRrRuleIds != null ) { deleteRrRuleIds.Add( this.RightRoleRules[i].Id ); }
					this.RightRoleRules.RemoveAt( i );
				}
			}
			//Add the new iRightRoleRules from the sourceObject to |this| list
			foreach( RightRoleRule rrr in sourceObject.RightRoleRules )
			{
				RightRoleRule found = this.RightRoleRules.GetByRightRoleRuleId( rrr.Id );
				if( found != null )
				{
					found.SynchronizeRecursive( rrr, ref deleteRrRuleIds, ref deleteRightRoleIds );
				}
				else
				{
					this.RightRoleRules.Add( rrr.Clone() );
				}
			}

			//purge the RightRoles from |this| list that no longer exist in the sourceObject's RightRoles list.
			for( int i = this.RightRoles.Count - 1; i > -1; i-- )
			{
				RightRole found = sourceObject.RightRoles.GetByRightRoleId( this.RightRoles[i].Id );
				if( found == null )
				{
					if( deleteRightRoleIds != null ) { deleteRightRoleIds.Add( this.RightRoles[i].Id ); }
					this.RightRoles.RemoveAt( i );
				}
			}
			//Add the new RightRoles from the sourceObject to |this| list
			foreach( RightRole rr in sourceObject.RightRoles )
			{
				RightRole found = this.RightRoles.GetByRightRoleId( rr.Id );
				if( found != null )
				{
					found.Synchronize( rr );
				}
				else
				{
					this.RightRoles.Add( rr.CloneMemberwise() );
				}
			}

			//purge the iRightRoleRules from |this| list that no longer exist in the sourceObject's iRightRoleRules list.
			for( int i = this.ElseRules.Count - 1; i > -1; i-- )
			{
				RightRoleRule found = sourceObject.ElseRules.GetByRightRoleRuleId( this.ElseRules[i].Id );
				if( found == null )
				{
					if( deleteRrRuleIds != null ) { deleteRrRuleIds.Add( this.ElseRules[i].Id ); }
					this.ElseRules.RemoveAt( i );
				}
			}
			//Add the new iRightRoleRules from the sourceObject to |this| list
			foreach( RightRoleRule rrr in sourceObject.ElseRules )
			{
				RightRoleRule found = this.ElseRules.GetByRightRoleRuleId( rrr.Id );
				if( found != null )
				{
					found.SynchronizeRecursive( rrr, ref deleteRrRuleIds, ref deleteRightRoleIds );
				}
				else
				{
					this.ElseRules.Add( rrr.Clone() );
				}
			}

			//purge the iElseRoles from |this| list that no longer exist in the sourceObject's iElseRoles list.
			for( int i = this.ElseRoles.Count - 1; i > -1; i-- )
			{
				RightRole found = sourceObject.ElseRoles.GetByRightRoleId( this.ElseRoles[i].Id );
				if( found == null )
				{
					if( deleteRightRoleIds != null ) { deleteRightRoleIds.Add( this.ElseRoles[i].Id ); }
					this.ElseRoles.RemoveAt( i );
				}
			}
			//Add the new iElseRoles from the sourceObject to |this| list
			foreach( RightRole rr in sourceObject.ElseRoles )
			{
				RightRole found = this.ElseRoles.GetByRightRoleId( rr.Id );
				if( found != null )
				{
					found.Synchronize( rr );
				}
				else
				{
					this.ElseRoles.Add( rr.CloneMemberwise() );
				}
			}
		}

		#region explicit/not implemented
		void ICloneable<RightRoleRule>.Synchronize(RightRoleRule sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		RightRoleRule ICloneable<RightRoleRule>.Clone()
		{
			throw new NotImplementedException();
		}

		IObjectModel ICloneableObject.Clone(bool generateNewId)
		{
			throw new NotImplementedException();
		}
		#endregion
		#endregion

		#region INotifyDeleted Members
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
	}

	[CollectionDataContract()]
	public class RightRoleRuleCollection : ObservableObjectModelCollection<RightRoleRule>, ISuplexObjectList
	{
		public RightRoleRuleCollection() : base() { }
		public RightRoleRuleCollection(IObjectModel owner)
			: base( owner )
		{ }

		public RightRoleRule GetByRightRoleRuleId(Guid id)
		{
			return this.SingleOrDefault( rrr => rrr.Id == id );
		}

		public ISuplexObject AddOrSynchronize(ISuplexObject item)
		{
			RightRoleRule rrr = (RightRoleRule)item;

			RightRoleRule exists = this.GetByRightRoleRuleId( rrr.Id );
			if( exists == null )
			{
				this.Add( rrr );
			}
			else
			{
				exists.Synchronize( rrr );
				exists.IsDirty = false;
				rrr = exists;
			}

			return rrr;
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			switch( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
				{
					this.AddToCount( e.NewItems );
					break;
				}
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
				{
					this.AddToCount( e.NewItems );
					this.RemoveFromCount( e.OldItems );
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					this.RemoveFromCount( e.OldItems );
					break;
				}
			}

			base.OnCollectionChanged( e );
		}

		private void AddToCount(IList items)
		{
			if( items != null && items.Count > 0 && items[0] is IObjectModel )
			{
				RightRoleRule r = null;
				for( int n = 0; n < items.Count; n++ )
				{
					r = (RightRoleRule)items[n];

					this.CompositeRuleCount += r.CompositeRuleCount + 1;
					this.CompositeRoleCount += r.CompositeRoleCount;
				}
			}
		}

		private void RemoveFromCount(IList items)
		{
			if( items != null && items.Count > 0 && items[0] is IObjectModel )
			{
				RightRoleRule r = null;
				for( int n = 0; n < items.Count; n++ )
				{
					r = (RightRoleRule)items[n];

					this.CompositeRuleCount -= (r.CompositeRuleCount + 1);
					this.CompositeRoleCount -= r.CompositeRoleCount;
				}
			}
		}

		public int CompositeRuleCount { get; internal set; }
		public int CompositeRoleCount { get; internal set; }
	}
}