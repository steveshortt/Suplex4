using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Data;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using sg = Suplex.General;
using st = Suplex.Security.Standard;

using settings = Suplex.Properties.Settings;

namespace Suplex.Forms.ObjectModel.Api
{
	[DataContract()]
	[KnownType( typeof( User ) )]
	[KnownType( typeof( Group ) )]
	public abstract class SecurityPrincipalBase :
		st.ISecurityPrincipal, INotifyPropertyChanged, ICloneable<SecurityPrincipalBase>, INotifyDeleted
	{
		private string _id = string.Empty;
		private string _name = string.Empty;
		private string _description = string.Empty;
		private bool _isLocal = false;
		private bool _isBuiltIn = false;
		private bool _isEnabled = false;
		private bool _isValid = false;
		private bool _isAnonymous = false;

		private bool _isDirty = false;
		private bool _isDeleted = false;

		#region ISecurityPrincipal Members
		[DataMember()]
		public string Id
		{
			get { return _id; }
			set
			{
				if( _id != value )
				{
					_id = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Id" );
				}
			}
		}

		public Guid IdToGuid() { return Guid.Parse( _id ); }

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

		[DataMember()]
		public string Description
		{
			get { return _description; }
			set
			{
				if( _description != value )
				{
					_description = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Description" );
				}
			}
		}

		[DataMember()]
		public bool IsLocal
		{
			get { return _isLocal; }
			set
			{
				if( _isLocal != value )
				{
					_isLocal = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "IsLocal" );
					this.OnPropertyChanged( "Source" );
				}
			}
		}

		[DataMember()]
		public bool IsBuiltIn
		{
			get { return _isBuiltIn; }
			set
			{
				if( _isBuiltIn != value )
				{
					_isBuiltIn = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "IsBuiltIn" );
				}
			}
		}

		[DataMember()]
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				if( _isEnabled != value )
				{
					_isEnabled = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "IsEnabled" );
				}
			}
		}

		[XmlIgnore()]
		[IgnoreDataMember()]
		public virtual string Source
		{
			get
			{
				return IsUserObject ? "User" : _isLocal ? "Suplex" : "External";
			}
		}

		[XmlIgnore()]
		[IgnoreDataMember()]
		public virtual bool IsValid { get { return _isValid; } internal set { _isValid = value; } }
		[XmlIgnore()]
		[IgnoreDataMember()]
		public virtual bool IsAnonymous { get { return _isAnonymous; } internal set { _isAnonymous = value; } }
		[XmlIgnore()]
		[IgnoreDataMember()]
		public abstract bool IsUserObject { get; }
		#endregion

		[XmlIgnore()]
		[IgnoreDataMember()]
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

		[XmlIgnore()]
		[IgnoreDataMember()]
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

		#region ICloneable<SecurityPrincipalBase> Members
		abstract public IObjectModel Clone(bool generateNewId);

		SecurityPrincipalBase ICloneable<SecurityPrincipalBase>.Clone()
		{
			return (SecurityPrincipalBase)this.MemberwiseClone();
		}

		SecurityPrincipalBase ICloneable<SecurityPrincipalBase>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		void ICloneable<SecurityPrincipalBase>.Synchronize(SecurityPrincipalBase sourceObject)
		{
			this.Id = sourceObject.Id;
			this.Name = sourceObject.Name;
			this.Description = sourceObject.Description;
			this.IsLocal = sourceObject.IsLocal;
			this.IsBuiltIn = sourceObject.IsBuiltIn;
			this.IsEnabled = sourceObject.IsEnabled;
			this.IsValid = sourceObject.IsValid;
			this.IsAnonymous = sourceObject.IsAnonymous;
		}

		void ICloneable<SecurityPrincipalBase>.Synchronize(SecurityPrincipalBase sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}
		#endregion
	}


	[DataContract()]
	public class User : SecurityPrincipalBase, IObjectModel, ICloneable<User>, ISuplexObject
	{
		public User()
			: base()
		{ }

		public DateTime LastLogon { get; set; }

		[XmlIgnore()]
		public override bool IsUserObject { get { return true; } }

		#region IObjectModel Members
		public ObjectType ObjectType { get { return ObjectType.User; } }
		public ObjectType ValidChildObjectTypes { get { return ObjectType.None; } }
		public bool SupportsChildObjectType(ObjectType objectType)
		{
			return (this.ValidChildObjectTypes & objectType) == objectType;
		}
		[XmlIgnore()]
		[IgnoreDataMember()]
		public IObjectModel ParentObject { get; set; }
		#endregion

		#region ICloneable<User> Members
		public override IObjectModel Clone(bool generateNewId)
		{
			User clone = this.Clone();
			if( generateNewId )
			{
				clone.Id = Guid.NewGuid().ToString();
			}
			return clone;
		}

		public User Clone()
		{
			return (User)this.MemberwiseClone();
		}

		User ICloneable<User>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		public void Synchronize(User sourceObject)
		{
			((ICloneable<SecurityPrincipalBase>)this).Synchronize( sourceObject );
		}

		void ICloneable<User>.Synchronize(User sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}
		#endregion

		public override string ToString()
		{
			return base.Name;
		}

		#region ISuplexObject Members
		public string ObjectId
		{
			get { return this.Id.ToString(); }
		}
		#endregion
	}

	[CollectionDataContract()]
	public class UserCollection : ObservableObjectModelCollection<User>
	{
		public UserCollection()
			: base()
		{
		}
		public UserCollection(IObjectModel owner)
			: base( owner )
		{
		}

		public User GetById(string id)
		{
			return this.Single( u => u.Id == id );
		}

		public bool GetById(string id, out User user)
		{
			user = this.SingleOrDefault( u => u.Id == id );
			return user != null;
		}

		public User GetByName(string name)
		{
			return this.Single( u => u.Name.ToLower() == name.ToLower() );
		}

		public bool GetByName(string name, out User user)
		{
			user = this.SingleOrDefault( u => u.Name.ToLower() == name.ToLower() );
			return user != null;
		}
	}

	[DataContract()]
	public class UserData
	{
		[DataMember]
		public User User { get; set; }
		[DataMember]
		public string AddedGroupMembership { get; set; }
		[DataMember]
		public string RemovedGroupMembership { get; set; }
	}

	[DataContract()]
	public class Group : SecurityPrincipalBase, IObjectModel, ICloneable<Group>, INodeItem, ISuplexObject
	{
		private GroupCollection _groups = null;
		private UserCollection _users = null;
		private bool _isExpanded = false;
		private bool _isSelected = false;
		private bool _isEditing = false;
		private bool _showDetail = false;
		private bool _showDetailPanels = false;
		private bool _enableChildGroupsLazyLoad = false;

		private int _defaultMaskSize = settings.Default.RlsMaskSize;
		private int _maskSize = settings.Default.RlsMaskSize;
		private BitArray _mask = null;

		public Group()
			: base()
		{
			_groups = new GroupCollection();
			_users = new UserCollection();
			_mask = new BitArray( _maskSize );
		}

		[XmlIgnore()]
		[IgnoreDataMember()]
		public BitArray Mask
		{
			get { return _mask; }
			set
			{
				if( _mask != value )
				{
					_mask = value;
					this.OnPropertyChanged( "Mask" );
				}
			}
		}
		[XmlIgnore()]
		[IgnoreDataMember()]
		public int MaskSize
		{
			get { return _maskSize; }
			set
			{
				if( _maskSize != value )
				{
					_maskSize = value;
					this.OnPropertyChanged( "MaskSize" );
					this.Mask = new BitArray( _maskSize );
				}
			}
		}

		[DataMember()]
		public string MaskValue
		{
			get
			{	//todo: 10082012, bug: _maskSize not working from svc (switched to _mask.Length)
				int[] mask = new int[_mask.Length / 32];	//32 bits per int
				_mask.CopyTo( mask, 0 );

				string separator = ",";
				StringBuilder list = new StringBuilder();
				foreach( int value in mask )
				{
					list.AppendFormat( "{0}{1}", value != int.MinValue ? value.ToString() : int.MinValue.ToString().TrimStart( '-' ), separator );
				}
				return list.ToString().TrimEnd( separator.ToCharArray() );
				//return sg.MiscUtils.Join<int>( ",", mask );
			}
			set
			{
				string[] values = value.Split( ',' );
				int[] masks = new int[values.Length];	//todo: 10082012, bug: (_maskSize / 32) not working from svc
				for( int i = 0; i < values.Length; i++ )
				{
					if( values[i] == int.MinValue.ToString().TrimStart( '-' ) )
					{
						masks[i] = int.MinValue;
					}
					else
					{
						masks[i] = Int32.Parse( values[i] );
					}
				}

				_mask = new BitArray( masks );
				this.OnPropertyChanged( "MaskValue" );
			}
		}


		//public Group(bool lazyLoadChildGroups)
		//    : this()
		//{ if( lazyLoadChildGroups ) { _groups.Add( new LazyLoadDummyGroup() ); } }

		[XmlIgnore()]
		[IgnoreDataMember()]
		public override bool IsUserObject { get { return false; } }

		[XmlIgnore()]
		[IgnoreDataMember()]
		public override bool IsAnonymous { get { return false; } internal set { base.IsAnonymous = false; } }

		#region IObjectModel Members
		[IgnoreDataMember()]
		public ObjectType ObjectType { get { return ObjectType.Group; } }
		[IgnoreDataMember()]
		public ObjectType ValidChildObjectTypes { get { return ObjectType.Group; } }
		public bool SupportsChildObjectType(ObjectType objectType)
		{
			return (this.ValidChildObjectTypes & objectType) == objectType;
		}
		[XmlIgnore()]
		[IgnoreDataMember()]
		public IObjectModel ParentObject { get; set; }
		#endregion

		#region ICloneable<Group> Members
		public override IObjectModel Clone(bool generateNewId)
		{
			Group clone = this.Clone();
			if( generateNewId )
			{
				clone.Id = Guid.NewGuid().ToString();
			}
			return clone;
		}

		public Group Clone()
		{
			return (Group)this.MemberwiseClone();
		}

		Group ICloneable<Group>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		public void Synchronize(Group sourceObject)
		{
			((ICloneable<SecurityPrincipalBase>)this).Synchronize( sourceObject );
		}

		void ICloneable<Group>.Synchronize(Group sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}
		#endregion

		[XmlIgnore()]
		[DataMember()]
		public GroupCollection Groups
		{
			get
			{
				if( _groups == null )
				{
					_groups = new GroupCollection();
				}
				return _groups;
			}
		}
		//[XmlIgnore()]
		//public UserCollection Users { get { return _users; } }
		[XmlIgnore()]
		[IgnoreDataMember()]
		internal bool EnableChildGroupsLazyLoad
		{
			get { return _enableChildGroupsLazyLoad; }
			set
			{
				if( _enableChildGroupsLazyLoad != value )
				{
					_enableChildGroupsLazyLoad = value;

					if( _enableChildGroupsLazyLoad )
					{
						if( _groups.Count == 0 )//StoreConnection.IsConnected &&
						{
							_groups.Add( new LazyLoadDummyGroup() );
						}
					}
					else
					{
						if( this.WantsLazyLoad )
						{
							_groups.Clear();
						}
					}
				}
			}
		}

		[XmlIgnore()]
		[IgnoreDataMember()]
		private bool WantsLazyLoad
		{
			get
			{//StoreConnection.IsConnected &&
				return _groups.Count > 0 && _groups[0] is LazyLoadDummyGroup;
			}
		}

		public override string ToString()
		{
			return base.Name;
		}

		#region INodeItem Members
		[XmlIgnore()]
		[IgnoreDataMember()]
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
		[IgnoreDataMember()]
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
		[IgnoreDataMember()]
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
		[IgnoreDataMember()]
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
		[IgnoreDataMember()]
		public bool ShowDetailPanels
		{
			get { return _showDetailPanels; }
			set
			{
				_showDetailPanels = value;
				this.OnPropertyChanged( "ShowDetailPanels" );
			}
		}

		[XmlIgnore]
		[IgnoreDataMember()]
		CompositeCollection INodeItem.ChildObjects { get { return null; } }
		#endregion

		#region ISuplexObject Members
		public string ObjectId
		{
			get { return this.Id.ToString(); }
		}
		#endregion
	}

	public class LazyLoadDummyGroup : Group
	{
	}

	[CollectionDataContract()]
	public class GroupCollection : ObservableObjectModelCollection<Group>
	{
		private int _maskSize = settings.Default.RlsMaskSize;
		private BitArray _allMasks = null;

		public GroupCollection()
			: base()
		{
			base.CollectionViewSource.SortDescriptions.Add( new SortDescription( "Name", ListSortDirection.Ascending ) );
			_allMasks = new BitArray( _maskSize );
		}
		public GroupCollection(IObjectModel owner)
			: base( owner )
		{
			base.CollectionViewSource.SortDescriptions.Add( new SortDescription( "Name", ListSortDirection.Ascending ) );
			_allMasks = new BitArray( _maskSize );
		}

		public Group GetById(string id)
		{
			return this.Single( g => g.Id == id );
		}

		public bool GetById(string id, out Group group)
		{
			group = this.SingleOrDefault( g => g.Id == id );
			return group != null;
		}

		public Group GetByName(string name)
		{
			return this.Single( g => g.Name.ToLower() == name.ToLower() );
		}

		public bool GetByName(string name, out Group group)
		{
			group = this.SingleOrDefault( g => g.Name.ToLower() == name.ToLower() );
			return group != null;
		}

		[XmlIgnore()]
		public int MaskSize
		{
			get { return _maskSize; }
			set
			{
				if( _maskSize != value )
				{
					_maskSize = value;
					_allMasks = new BitArray( _maskSize );
				}
			}
		}
		//TODO: this assumes there's room in the mask, next to add error handling
		public BitArray GetNextMask()
		{
			int index = 0;
			while( _allMasks[index] )
			{
				index++;
			}

			BitArray nextMask = new BitArray( _maskSize );
			nextMask.Set( index, true );
			return nextMask;
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			switch( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
				{
					this.AddMask( e.NewItems );
					break;
				}
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
				{
					this.AddMask( e.NewItems );
					this.RemoveMask( e.OldItems );
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					this.RemoveMask( e.OldItems );
					break;
				}
			}

			base.OnCollectionChanged( e );
		}

		private void AddMask(IList groups)
		{
			if( groups != null )
			{
				foreach( Group g in groups )
				{
					if( g.Mask.Length < _maskSize )
					{
						byte[] mask = new byte[_maskSize / 8];
						g.Mask.CopyTo( mask, 0 );
						g.Mask = new BitArray( mask );
					}
					_allMasks.Or( g.Mask );
				}
			}
		}

		private void RemoveMask(IList groups)
		{
			if( groups != null )
			{
				foreach( Group g in groups )
				{
					if( g.Mask.Length < _maskSize )
					{
						byte[] mask = new byte[_maskSize / 8];
						g.Mask.CopyTo( mask, 0 );
						g.Mask = new BitArray( mask );
					}
					_allMasks.Xor( g.Mask );
				}
			}
		}
	}

	[DataContract()]
	public class GroupData
	{
		[DataMember]
		public Group Group { get; set; }
		[DataMember]
		public string AddedGroupMembership { get; set; }
		[DataMember]
		public string RemovedGroupMembership { get; set; }
	}

	public class GroupMembershipItem
	{
		public GroupMembershipItem() { }
		public GroupMembershipItem(Group group, SecurityPrincipalBase member)
		{
			this.Group = group;
			this.GroupId = group.Id;
			this.Member = member;
			this.MemberId = member.Id;
			this.MemberType = member is User ? ObjectType.User : ObjectType.Group;
		}

		internal GroupMembershipItem(string groupId, SecurityPrincipalBase member)
		{
			this.GroupId = groupId;
			this.Member = member;
			this.MemberId = member.Id;
			this.MemberType = member is User ? ObjectType.User : ObjectType.Group;
		}
		internal GroupMembershipItem(string groupId, string memberId, ObjectType memberType)
		{
			this.GroupId = groupId;
			this.MemberId = memberId;
			this.MemberType = memberType;
		}


		public Group Group { get; set; }
		public SecurityPrincipalBase Member { get; set; }

		internal string GroupId { get; set; }
		internal string MemberId { get; set; }
		internal ObjectType MemberType { get; set; }

		public override string ToString()
		{
			return string.Format( "{0}/{1}", this.Group.Name, this.Member.Name );
		}

		public string ToMembershipKey()
		{
			return string.Format( "{0}_{1}", this.Group.Id, this.Member.Id );
		}
	}

	public class GroupMembershipCollection : IXmlSerializable, INotifyCollectionChanged
	{
		private Dictionary<string, GroupMembershipItem> _innerList = null;

		private const string __entryElmt = "Entry";
		private const string __groupAtt = "Group";
		private const string __memberAtt = "Member";
		private const string __typeAtt = "Type";

		private SuplexStore _ownerStore = null;

		internal GroupMembershipCollection()
		{
			_innerList = new Dictionary<string, GroupMembershipItem>();

			this.ThrowConstraintErrors = false;
		}
		public GroupMembershipCollection(SuplexStore ownerStore)
			: this()
		{
			_ownerStore = ownerStore;
		}

		#region methods
		public void Clear()
		{
			_innerList.Clear();
			this.OnCollectionChanged( NotifyCollectionChangedAction.Reset );
		}

		public void Resolve()
		{
			foreach( GroupMembershipItem item in _innerList.Values )
			{
				item.Group = _ownerStore.Groups.GetById( item.GroupId );

				if( item.MemberType == ObjectType.User )
				{
					item.Member = _ownerStore.Users.GetById( item.MemberId );
					//item.Group.Users.Add( (User)item.Member );
				}
				else
				{
					item.Member = _ownerStore.Groups.GetById( item.MemberId );
					//item.Group.Groups.Add( (Group)item.Member );
				}
			}
		}

		internal SuplexStore OwnerStore { get { return _ownerStore; } set { _ownerStore = value; } }

		private string GetKey(Group group, SecurityPrincipalBase member)
		{
			return string.Format( "{0}_{1}", group.Id, member.Id );
		}
		private string GetKey(string groupId, SecurityPrincipalBase member)
		{
			return string.Format( "{0}_{1}", groupId, member.Id );
		}
		private string GetKey(string groupId, string memberId)
		{
			return string.Format( "{0}_{1}", groupId, memberId );
		}

		public void Add(Group group, SecurityPrincipalBase member)
		{
			if( group.Id == member.Id )
			{
				throw new ArgumentException( "GroupId and MemberId cannot be equal." );
			}

			string key = this.GetKey( group, member );
			if( !_innerList.ContainsKey( key ) )
			{
				_innerList.Add( key, new GroupMembershipItem( group, member ) );

				//if( member.IsUserObject )
				//{
				//    group.Users.Add( (User)member );
				//}
				//else
				//{
				//    group.Groups.Add( (Group)member );
				//}

				this.OnCollectionChanged( NotifyCollectionChangedAction.Add );
			}
			else
			{
				if( this.ThrowConstraintErrors )
				{
					throw new ArgumentException( string.Format( "GroupMembership key '{0}' already exists.", key ) );
				}
			}
		}
		public void Add(string groupId, SecurityPrincipalBase member)
		{
			if( groupId == member.Id )
			{
				throw new ArgumentException( "GroupId and MemberId cannot be equal." );
			}

			string key = this.GetKey( groupId, member );
			if( !_innerList.ContainsKey( key ) )
			{
				_innerList.Add( key, new GroupMembershipItem( groupId, member ) );
				this.OnCollectionChanged( NotifyCollectionChangedAction.Add );
			}
			else
			{
				if( this.ThrowConstraintErrors )
				{
					throw new ArgumentException( string.Format( "GroupMembership key '{0}' already exists.", key ) );
				}
			}
		}
		internal void Add(string groupId, string memberId, string objectType)
		{
			if( groupId == memberId )
			{
				throw new ArgumentException( "GroupId and MemberId cannot be equal." );
			}

			string key = this.GetKey( groupId, memberId );
			if( !_innerList.ContainsKey( key ) )
			{
				_innerList.Add( key, new GroupMembershipItem( groupId, memberId, objectType.ToLower() == "user" ? ObjectType.User : ObjectType.Group ) );
				this.OnCollectionChanged( NotifyCollectionChangedAction.Add );
			}
			else
			{
				if( this.ThrowConstraintErrors )
				{
					throw new ArgumentException( string.Format( "GroupMembership key '{0}' already exists.", key ) );
				}
			}
		}
		public IEnumerable<GroupMembershipItem> GetByGroup(Group group, bool groupsOnly)
		{
			return _innerList.Values.Where( item => item.GroupId == group.Id );
		}

		public IEnumerable<GroupMembershipItem> GetByMember(SecurityPrincipalBase member, bool recursive)
		{
			return _innerList.Values.Where( item => item.Member.Id == member.Id );
		}

		public IEnumerable<GroupMembershipItem> GetByGroupOrMember(SecurityPrincipalBase member, bool recursive)
		{
			return _innerList.Values.Where( item => item.GroupId == member.Id || item.Member.Id == member.Id );
		}

		//public MembershipList<SecurityPrincipalBase> GetGroupMembers_(Group group)
		//{
		//    MembershipList<SecurityPrincipalBase> membership = new MembershipList<SecurityPrincipalBase>();

		//    _innerList.Values.Where( grp => grp.GroupId == group.Id && grp.Group.IsEnabled );

		//    return membership;
		//}

		public MembershipList<SecurityPrincipalBase> GetGroupMembers(Group group)
		{
			MembershipList<SecurityPrincipalBase> membership = new MembershipList<SecurityPrincipalBase>();

			foreach( GroupMembershipItem item in _innerList.Values )
			{
				if( item.GroupId == group.Id && item.Member.IsEnabled )
				{
					membership.MemberList.Add( item.Member );
				}
			}

			membership.NonMemberList = new ObservableObjectModelCollection<SecurityPrincipalBase>();
			IEnumerator<SecurityPrincipalBase> nonMembers = _ownerStore.SecurityPrincipals.Except( membership.MemberList ).GetEnumerator();
			while( nonMembers.MoveNext() )
			{
				if( nonMembers.Current.Id != group.Id && nonMembers.Current.IsEnabled )
				{
					membership.NonMemberList.Add( nonMembers.Current );
				}
			}

			ObservableObjectModelCollection<SecurityPrincipalBase> nonMembs = membership.NonMemberList;
			ObservableObjectModelCollection<SecurityPrincipalBase> nestedMembs = membership.NestedMemberList;
			this.RecurseIneligibleNonMembersUp( group, ref nonMembs, ref nestedMembs );

			return membership;
		}

		public MembershipList<Group> GetMemberOf(SecurityPrincipalBase member)
		{
			MembershipList<Group> membership = new MembershipList<Group>();

			foreach( GroupMembershipItem item in _innerList.Values )
			{
				if( item.Member.Id == member.Id )
				{
					membership.MemberList.Add( item.Group );
				}
			}

			membership.NonMemberList = new ObservableObjectModelCollection<Group>();
			IEnumerator<Group> nonMembers = _ownerStore.Groups.Except( membership.MemberList ).GetEnumerator();
			while( nonMembers.MoveNext() )
			{
				if( nonMembers.Current.IsLocal && nonMembers.Current.Id != member.Id )
				{
					membership.NonMemberList.Add( nonMembers.Current );
				}
			}

			if( ((IObjectModel)member).ObjectType == ObjectType.Group )
			{
				//Group thisGroup = membership.NonMemberList.FirstOrDefault( group => group.Id == member.Id );
				//membership.NonMemberList.Remove( thisGroup );
				IEnumerable<GroupMembershipItem> members = this.GetByGroup( (Group)member, false );
				foreach( GroupMembershipItem gmi in members )
				{
					if( ((IObjectModel)gmi.Member).ObjectType == ObjectType.Group )
					{
						membership.NonMemberList.Remove( (Group)gmi.Member );
					}
				}
			}

			if( !member.IsUserObject )
			{
				ObservableObjectModelCollection<Group> nonMembs = membership.NonMemberList;
				ObservableObjectModelCollection<Group> nestedMembs = membership.NestedMemberList;
				this.RecurseIneligibleNonMembers<Group>( (Group)member, ref nonMembs, ref nestedMembs );
			}

			return membership;
		}

		public List<Group> GetGroupHierarchy(Group g)
		{
			List<Group> groups = new List<Group>();
			Stack<Group> parentGroups = new Stack<Group>();
			GroupEqualityComparer comparer = new GroupEqualityComparer();

			Stack<GroupMembershipItem> parentItems = new Stack<GroupMembershipItem>();
			IEnumerable<GroupMembershipItem> parents = _innerList.Values.Where( sp => sp.MemberId == g.Id );
			foreach( GroupMembershipItem gmi in parents )
			{
				parentItems.Push( gmi );
			}
			if( parentItems.Count > 0 )
			{
				while( parentItems.Count > 0 )
				{
					GroupMembershipItem p = parentItems.Pop();
					IEnumerable<GroupMembershipItem> ascendants = _innerList.Values.Where( sp => sp.MemberId == p.GroupId );
					int count = 0;
					foreach( GroupMembershipItem gmi in ascendants )
					{
						parentItems.Push( gmi );
						count++;
					}
					if( count == 0 )
					{
						if( !groups.Contains( p.Group, comparer ) )
						{
							parentGroups.Push( p.Group );
							groups.Add( p.Group );
						}
					}
				}
			}
			else
			{
				parentGroups.Push( g );
				groups.Add( g );
			}

			while( parentGroups.Count > 0 )
			{
				Group p = parentGroups.Pop();
				IEnumerable<GroupMembershipItem> descendants =
					_innerList.Values.Where( sp => sp.GroupId == p.Id && sp.MemberType == ObjectType.Group );
				foreach( GroupMembershipItem chi in descendants )
				{
					Group ch = chi.Member as Group;
					if( !p.Groups.Contains( ch, comparer ) )
					{
						p.Groups.Add( ch );
						parentGroups.Push( ch );
					}
				}
			}

			return groups;
		}

		private void RecurseIneligibleNonMembersUp(Group parent,
			ref ObservableObjectModelCollection<SecurityPrincipalBase> nonMembers, ref ObservableObjectModelCollection<SecurityPrincipalBase> nestedMembs)
		{
			foreach( GroupMembershipItem item in _innerList.Values )
			{
				if( item.MemberId == parent.Id )
				{
					//BUG: this doesn't always work when connected to database
					nonMembers.Remove( item.Group );
					nestedMembs.Add( item.Group );

					this.RecurseIneligibleNonMembersUp( item.Group, ref nonMembers, ref nestedMembs );
				}
			}
		}

		private void RecurseIneligibleNonMembers<T>
			(Group parent, ref ObservableObjectModelCollection<T> nonMembers, ref ObservableObjectModelCollection<T> nestedMembs)
			where T : SecurityPrincipalBase
		{
			foreach( GroupMembershipItem item in _innerList.Values )
			{
				if( item.GroupId == parent.Id )
				{
					if( !item.Member.IsUserObject )
					{
						nonMembers.Remove( (T)item.Member );
						nestedMembs.Add( (T)item.Member );

						this.RecurseIneligibleNonMembers<T>( (Group)item.Member, ref nonMembers, ref nestedMembs );
					}
				}
			}
		}

		public void Remove(Group group, SecurityPrincipalBase member)
		{
			this.InnerList.Remove( this.GetKey( group, member ) );

			//if( member.IsUserObject )
			//{
			//    group.Users.Remove( (User)member );
			//}
			//else
			//{
			//    group.Groups.Remove( (Group)member );
			//}
			this.OnCollectionChanged( NotifyCollectionChangedAction.Remove );
		}

		[Obsolete( "Not in use; consider removing", true )]
		public void Remove(string groupId, string memberId)
		{
			this.InnerList.Remove( this.GetKey( groupId, memberId ) );
			this.OnCollectionChanged( NotifyCollectionChangedAction.Remove );
		}

		[Obsolete( "Not in use; consider removing", true )]
		public void RemoveByGroup(Group group)
		{
			IEnumerable<GroupMembershipItem> items = this.GetByGroup( group, false );
			foreach( GroupMembershipItem gmi in items )
			{
				this.InnerList.Remove( gmi.ToMembershipKey() );
			}
			this.OnCollectionChanged( NotifyCollectionChangedAction.Remove );
		}

		[Obsolete( "Not in use; consider removing", true )]
		public void RemoveByMember(SecurityPrincipalBase member)
		{
			IEnumerable<GroupMembershipItem> items = this.GetByMember( member, false );
			foreach( GroupMembershipItem gmi in items )
			{
				this.InnerList.Remove( gmi.ToMembershipKey() );
			}
			this.OnCollectionChanged( NotifyCollectionChangedAction.Remove );
		}

		public void RemoveByGroupOrMember(SecurityPrincipalBase member)
		{
			List<string> keys = new List<string>();
			IEnumerable<GroupMembershipItem> items = this.GetByGroupOrMember( member, false );
			foreach( GroupMembershipItem gmi in items )
			{
				keys.Add( gmi.ToMembershipKey() );
			}
			for( int i = keys.Count-1; i > -1; i-- )
			{
				this.InnerList.Remove( keys[i] );
			}
			this.OnCollectionChanged( NotifyCollectionChangedAction.Remove );
		}

		public Dictionary<string, GroupMembershipItem> InnerList { get { return _innerList; } }

		public bool ThrowConstraintErrors { get; set; }
		#endregion


		#region IXmlSerializable Members
		//NOTE: from MSDN docs:
		//This method is reserved and should not be used. When implementing the IXmlSerializable interface,
		//	you should return a null reference from this method, and instead, if specifying a custom schema is required,
		//	apply the XmlSchemaProviderAttribute to the class.
		public XmlSchema GetSchema() { return null; }

		public void ReadXml(XmlReader reader)
		{
			reader.Read();	//skip first line
			while( reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == __entryElmt )
			{
				//if( reader[__typeAtt].ToLower() == "user" )
				//{
				//    this.Add( reader[__groupAtt], _ownerStore.Users.GetById( reader[__memberAtt] ) );
				//}
				//else
				//{
				//    this.Add( reader[__groupAtt], _ownerStore.Groups.GetById( reader[__memberAtt] ) );
				//}

				this.Add( reader[__groupAtt], reader[__memberAtt], reader[__typeAtt] );

				reader.Read();
			}
			reader.Read();
		}

		public void WriteXml(XmlWriter writer)
		{
			foreach( GroupMembershipItem item in _innerList.Values )
			{
				writer.WriteStartElement( __entryElmt );
				writer.WriteAttributeString( __groupAtt, item.GroupId );
				writer.WriteAttributeString( __memberAtt, item.Member.Id );
				writer.WriteAttributeString( __typeAtt, item.Member.IsUserObject ? "User" : "Group" );
				writer.WriteEndElement();
			}
		}
		#endregion

		public override string ToString()
		{
			return string.Format( "Count = {0}", _innerList.Count );
		}

		#region INotifyCollectionChanged Members
		//TODO: implement IList parameter support proper change notifications for .Add/.Remove
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		protected void OnCollectionChanged(NotifyCollectionChangedAction action)
		{
			if( this.CollectionChanged != null )
			{
				this.CollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
			}
		}
		#endregion
	}

	[DataContract()]
	public class MembershipList<T>
	{
		public MembershipList()
		{
			this.MemberList = new ObservableObjectModelCollection<T>();
			this.NonMemberList = new ObservableObjectModelCollection<T>();
			this.NestedMemberList = new ObservableObjectModelCollection<T>();
			this.NestedNonMemberList = new ObservableObjectModelCollection<T>();

			CollectionContainer memberList = new CollectionContainer() { Collection = this.MemberList };
			CollectionContainer nonMemberList = new CollectionContainer() { Collection = this.NonMemberList };
			CollectionContainer nestedMemberList = new CollectionContainer() { Collection = this.NestedMemberList };
			CollectionContainer nestedNonMemberList = new CollectionContainer() { Collection = this.NestedNonMemberList };

			this.Members = new CompositeCollection();
			this.Members.Add( memberList );
			this.Members.Add( nestedMemberList );

			this.NonMembers = new CompositeCollection();
			this.NonMembers.Add( nonMemberList );
			this.NonMembers.Add( nestedNonMemberList );
		}

		[DataMember()]
		public ObservableObjectModelCollection<T> MemberList { get; internal set; }
		[DataMember()]
		public ObservableObjectModelCollection<T> NonMemberList { get; internal set; }
		[DataMember()]
		public ObservableObjectModelCollection<T> NestedMemberList { get; internal set; }
		[DataMember()]
		public ObservableObjectModelCollection<T> NestedNonMemberList { get; internal set; }

		[IgnoreDataMember()]
		public CompositeCollection Members { get; internal set; }
		[IgnoreDataMember()]
		public CompositeCollection NonMembers { get; internal set; }
	}

	public class GroupEqualityComparer: IEqualityComparer<Group>
	{
		#region IEqualityComparer<Group> Members

		public bool Equals(Group x, Group y)
		{
			return x.Id.Equals( y.Id );
		}

		public int GetHashCode(Group obj)
		{
			return obj.GetHashCode();
		}

		#endregion
	}
}