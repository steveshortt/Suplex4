using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

using ss = Suplex.Security;
using st = Suplex.Security.Standard;


namespace Suplex.Forms.ObjectModel.Api
{
	//see note in Resolve regarding INotifyDeleted
	[DataContract()]
	[KnownType( typeof( UIAce ) )]
	[KnownType( typeof( RecordAce ) )]
	[KnownType( typeof( FileSystemAce ) )]
	[KnownType( typeof( SynchronizationAce ) )]
	public class AccessControlEntryBase : ss.IAccessControlEntry, IObjectModel,
		INotifyPropertyChanged, INotifyDeleted, ISuplexObject		//, IDatabaseObject  , INotifyPropertyChanging
	{
		private bool _isSelected = false;

		private st.ISecurityPrincipal _securityPrincipal = null;
		//private UIElement _uie = null;

		private long _id = -1;
		private object _right = null;
		private bool _allowed = false;
		private bool _inherit = false;

		private bool _isDirty = false;

		private bool _isDeleted = false;

		public AccessControlEntryBase()
		{
			Inherit = true;
		}

		#region props
		[XmlIgnore()]
		[DataMember()]
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if( _isSelected != value )
				{
					_isSelected = value;
					this.OnPropertyChanged( "IsSelected" );
				}
			}
		}

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

		[XmlAttribute()]
		[DataMember()]
		public long Id
		{
			get { return _id; }
			set
			{
				if( value != _id )
				{
					_id = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Id" );
				}
			}
		}

		[XmlIgnore()]
		[DataMember()]
		public ss.AceType AceType { get; set; }
		[XmlIgnore()]
		[IgnoreDataMember()]
		public object Right
		{
			get { return _right; }
			set
			{
				if( value != _right )
				{
					_right = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Right" );
				}
			}
		}

        [DataMember()]
        public int AceRight
        {
            get { return (int)Right; }
            set { Right = value; }
        }

		[XmlAttribute()]
		[DataMember()]
		public bool Allowed
		{
			get { return _allowed; }
			set
			{
				if( value != _allowed )
				{
					_allowed = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Allowed" );
				}
			}
		}

		[XmlAttribute()]
		[DataMember()]
		public bool Inherit
		{
			get { return _inherit; }
			set
			{
				if( value != _inherit )
				{
					_inherit = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Inherit" );
				}
			}
		}

		[XmlIgnore()]
		[IgnoreDataMember()]
		public string InheritedFrom { get; set; }
		[XmlIgnore()]
		[IgnoreDataMember()]
		public ArrayList Parameters { get; set; }
		#endregion

		#region IObjectModel Members
		public string Name { get { return null; } set { } }
		public ObjectType ObjectType { get { return ObjectType.Ace; } }
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
					SetOwnerDirtyChecked();
					this.OnPropertyChanged( "IsDirty" );
				}
			}
		}
		#endregion

		#region ICloneable Members
		public virtual object Clone()
		{
			return this.MemberwiseClone();
		}

		public AccessControlEntryBase CloneMemberwise()
		{
			//AccessControlEntryBase ace = 
			//ss.AceCloner.Clone( this, ace );
			//ace.Id = this.Id;
			//ace.SecurityPrincipal = this.SecurityPrincipal;
			//ace.SecurityPrincipalId = this.SecurityPrincipalId;
			////ace.UIElement = this.UIElement;
			////ace.UIElementId = this.UIElementId;

			//undid this 12/24/2009 in favor of below
			//return (AccessControlEntryBase)this.MemberwiseClone();

			//added a line to specifically set the SecurityPrincipal to maintain the integrity of the reference:
			//	the MemberwiseClone() does not provide a reference to the same SecurityPrincipal object; rather, it returns a Clone
			AccessControlEntryBase ace = (AccessControlEntryBase)this.MemberwiseClone();
			ace.SecurityPrincipal = this.SecurityPrincipal;
			return ace;
		}
		#endregion


		#region special SuplexApp UI features
		//[XmlIgnore]
		//public UIElement UIElement
		//{
		//    get { return _uie; }
		//    set
		//    {
		//        _uie = value;
		//        this.UIElementId = _uie.Id;
		//    }
		//}
		//[XmlAttribute()]
		//public Guid UIElementId { get; set; }

		[XmlIgnore]
		public IObjectModel Owner { get; set; }
		void SetOwnerDirtyChecked()
		{
			if( Owner != null && this.IsDirty )
			{
				Owner.IsDirty = true;
			}
		}

		[XmlIgnore]
		public st.ISecurityPrincipal SecurityPrincipal
		{
			get
			{
				//if( _securityPrincipal == null && !string.IsNullOrEmpty( this.SecurityPrincipalId ) &&
				//    this.ParentObject != null && ((AceCollection)this.ParentObject).OwnerStore != null )
				//{
				//    this.Resolve( ((AceCollection)this.ParentObject).OwnerStore );
				//}
				return _securityPrincipal;
			}
			set
			{
				//note: this isn't wrapped in if( _securityPrincipal != value ) { }

				if( _securityPrincipal != null )
				{
					((SecurityPrincipalBase)_securityPrincipal).PropertyChanged -= this.SecurityPrincipal_PropertyChanged;
				}

				_securityPrincipal = value;

				if( _securityPrincipal != null )
				{
					this.SecurityPrincipalId = _securityPrincipal.Id;
					((SecurityPrincipalBase)_securityPrincipal).PropertyChanged += new PropertyChangedEventHandler( this.SecurityPrincipal_PropertyChanged );
				}

				this.IsDirty = true;
				this.OnPropertyChanged( "SecurityPrincipal" );
			}
		}

		protected void SecurityPrincipal_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if( e.PropertyName == "IsDeleted" )
			{
				this.IsDeleted = true;
			}
		}

		[XmlAttribute()]
		[DataMember()]
		public string SecurityPrincipalId { get; set; }

		internal void Resolve(SuplexStore suplexStore)
		{
			//this.UIElement = suplexStore.UIElements.GetById( this.UIElementId );
			if( this.SecurityPrincipal == null || this.SecurityPrincipal.Id != this.SecurityPrincipalId )
			{
				Group g = null;
				if( suplexStore.Groups.GetById( this.SecurityPrincipalId, out g ) )
				{
					this.SecurityPrincipal = g;
				}
				else
				{
					//this occurs when a SecurityPrincipal is deleted /before/ a Dacl/Sacl is lazy-resolved:
					// that is, if just the this.SecurityPrincipalId (string) is avail, then this.SecurityPrincipal has not yet
					// been resolved, so kill this Ace if the SecurityPrincipal no longer exists in the datastore
					this.IsDeleted = true;
				}
			}
		}
		#endregion

		public virtual bool WantsSynchronize(AccessControlEntryBase ace)
		{
			return
				this.Allowed != ace.Allowed ||
				this.Inherit != ace.Inherit ||
				this.Right != ace.Right ||
				this.SecurityPrincipal != ace.SecurityPrincipal;
		}

		public virtual void Synchronize(AccessControlEntryBase ace)
		{
			this.Allowed = ace.Allowed;
			this.Inherit = ace.Inherit;
			this.Right = ace.Right;
			//this.SecurityPrincipal = ace.SecurityPrincipal;

			if( this.SecurityPrincipal != ace.SecurityPrincipal )
			{
				((SecurityPrincipalBase)this.SecurityPrincipal).PropertyChanged -= this.SecurityPrincipal_PropertyChanged;
				this.SecurityPrincipal = ace.SecurityPrincipal;
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

		#region ISuplexObject Members
		public string ObjectId
		{
			get { return this.Id.ToString(); ; }
		}
		#endregion
	}

	[DataContract()]
	[KnownType( typeof( UIAuditAce ) )]
	[KnownType( typeof( RecordAuditAce ) )]
	[KnownType( typeof( FileSystemAuditAce ) )]
	[KnownType( typeof( SynchronizationAuditAce ) )]
	public class AccessControlEntryAuditBase : AccessControlEntryBase, ss.IAccessControlEntryAudit, ISuplexObject
	{
		private bool _denied = false;

		public AccessControlEntryAuditBase() : base() { }

		[XmlAttribute()]
		[DataMember()]
		public bool Denied
		{
			get { return _denied; }
			set
			{
				if( value != _denied )
				{
					_denied = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Denied" );
				}
			}
		}

		new public AccessControlEntryAuditBase CloneMemberwise()
		{
			//undid 12/26/2009 to parallel AccessControlEntryBase
			//return (AccessControlEntryAuditBase)this.MemberwiseClone();

			//added a line to specifically set the SecurityPrincipal to maintain the integrity of the reference:
			//	the MemberwiseClone() does not provide a reference to the same SecurityPrincipal object; rather, it returns a Clone
			AccessControlEntryAuditBase ace = (AccessControlEntryAuditBase)this.MemberwiseClone();
			ace.SecurityPrincipal = this.SecurityPrincipal;
			return ace;
		}

		public override bool WantsSynchronize(AccessControlEntryBase ace)
		{
			return base.WantsSynchronize( ace ) ||
				this.Denied != ((AccessControlEntryAuditBase)ace).Denied;
		}

		public override void Synchronize(AccessControlEntryBase ace)
		{
			base.Synchronize( ace );
			this.Denied = ((AccessControlEntryAuditBase)ace).Denied;
		}

		//public void Synchronize(AccessControlEntryAuditBase ace)
		//{
		//	this.Allowed = ace.Allowed;
		//	this.Denied = ace.Denied;
		//	this.Inherit = ace.Inherit;
		//	this.Right = ace.Right;
		//	//this.SecurityPrincipal = ace.SecurityPrincipal;

		//	if( this.SecurityPrincipal != ace.SecurityPrincipal )
		//	{
		//		((SecurityPrincipalBase)this.SecurityPrincipal).PropertyChanged -= base.SecurityPrincipal_PropertyChanged;
		//		this.SecurityPrincipal = ace.SecurityPrincipal;
		//	}
		//}
	}

	public interface IAceRight<T>
	{
		T iRight { get; set; }
	}

	[DataContract()]
	public class AccessControlEntry<T> : AccessControlEntryBase, IAceRight<T>
	{
		public AccessControlEntry()
			: base()
		{
			this.AceType = ss.AceTypeRights.GetAceType( typeof( T ) );
		}

		public AccessControlEntry(T right, bool allowed)
			: this()
		{
			this.iRight = right;
            //this.AceRight = right;
			this.Allowed = allowed;
		}


		[XmlAttribute( "Right" )]
		[DataMember( Name = "Right" )]
		public T iRight
		{
			get { return (T)base.Right; }
			set { base.Right = value; }
		}

        //to cover json serialization
        //[DataMember]
        //public T AceRight
        //{
        //    get { return (T)base.Right; }
        //    set { base.Right = value; }
        //}

        #region ICloneable Members
        public override object Clone()
		{
			AccessControlEntry<T> ace = new AccessControlEntry<T>();
			ss.AceCloner.Clone( this, ace );
			ace.Id = this.Id;
			ace.SecurityPrincipal = this.SecurityPrincipal;
			ace.SecurityPrincipalId = this.SecurityPrincipalId;
			//ace.UIElement = this.UIElement;
			//ace.UIElementId = this.UIElementId;
			return ace;
		}
		#endregion
	}

	[DataContract()]
	public class AccessControlEntryAudit<T> : AccessControlEntryAuditBase, IAceRight<T>
	{
		public AccessControlEntryAudit()
			: base()
		{
			this.AceType = ss.AceTypeRights.GetAceType( typeof( T ) );
		}

		public AccessControlEntryAudit(T right, bool allowed, bool denied)
			: this()
		{
			this.iRight = right;
			this.Allowed = allowed;
			this.Denied = denied;
		}


		[XmlElement( "Right" )]
		[DataMember( Name = "Right" )]
		public T iRight
		{
			get { return (T)base.Right; }
			set { base.Right = value; }
		}

		public override object Clone()
		{
			AccessControlEntryAudit<T> ace = new AccessControlEntryAudit<T>();
			ss.AceCloner.Clone( this, ace );
			ace.Id = this.Id;
			ace.SecurityPrincipal = this.SecurityPrincipal;
			ace.SecurityPrincipalId = this.SecurityPrincipalId;
			//ace.UIElement = this.UIElement;
			//ace.UIElementId = this.UIElementId;
			return ace;
		}
	}

	[DataContract()]
	public class UIAce : AccessControlEntry<ss.UIRight>
	{
		public UIAce() { }
		public UIAce(ss.UIRight right, bool allowed) : base( right, allowed ) { }
	}
	[DataContract()]
	public class UIAuditAce : AccessControlEntryAudit<ss.UIRight>
	{
		public UIAuditAce() { }
		public UIAuditAce(ss.UIRight right, bool allowed, bool denied) : base( right, allowed, denied ) { }
	}
	[DataContract()]
	public class RecordAce : AccessControlEntry<ss.RecordRight> { }
	[DataContract()]
	public class RecordAuditAce : AccessControlEntryAudit<ss.RecordRight> { }
	[DataContract()]
	public class FileSystemAce : AccessControlEntry<ss.FileSystemRight> { }
	[DataContract()]
	public class FileSystemAuditAce : AccessControlEntryAudit<ss.FileSystemRight> { }
	[DataContract()]
	public class SynchronizationAce : AccessControlEntry<ss.SynchronizationRight> { }
	[DataContract()]
	public class SynchronizationAuditAce : AccessControlEntryAudit<ss.SynchronizationRight> { }

	public class UIAceDefault : UIAce
	{
		public UIAceDefault() : base()
		{
			base.Allowed = true;
			base.iRight = Suplex.Security.UIRight.FullControl;
		}
		public UIAceDefault(ss.UIRight right, bool allowed) : base( right, allowed ) { }
	}
	public class UIAuditAceDefault : UIAuditAce
	{
		public UIAuditAceDefault()
			: base()
		{
			base.Allowed = true;
			base.Denied = true;
			base.iRight = Suplex.Security.UIRight.FullControl;
		}
		public UIAuditAceDefault(ss.UIRight right, bool allowed, bool denied) : base( right, allowed, denied ) { }
	}


	[XmlInclude( typeof( UIAce ) ), XmlInclude( typeof( RecordAce ) ), XmlInclude( typeof( FileSystemAce ) ), XmlInclude( typeof( SynchronizationAce ) )]
	[CollectionDataContract()]
	public class AceCollection : ObservableObjectModelCollection<AccessControlEntryBase>, ISuplexObjectList
	{
		List<AccessControlEntryBase> _removedItems = new List<AccessControlEntryBase>();

		public AceCollection() : base() { }
		public AceCollection(IObjectModel owner)
			: base( owner )
		{ }

		internal SuplexStore OwnerStore { get; set; }

		public List<AccessControlEntryBase> RemovedItems { get { return _removedItems; } }

		//public IEnumerable<AccessControlEntryBase> GetByUIElement(UIElement uie)
		//{
		//    return this.Where( ace => ace.UIElementId == uie.Id );
		//}

		//public IEnumerable<AccessControlEntryBase> GetByUIElement(UIElement uie, bool resolveGroups, bool resolveUIElements)
		//{
		//    IEnumerable<AccessControlEntryBase> aces = this.Where( ace => ace.UIElementId == uie.Id );

		//    if( resolveGroups || resolveUIElements )
		//    {
		//        IEnumerator<AccessControlEntryBase> aceList = aces.GetEnumerator();
		//        while( aceList.MoveNext() )
		//        {
		//            if( resolveGroups )
		//            {
		//                aceList.Current;
		//            }
		//            if( resolveUIElements )
		//            { }
		//        }
		//    }

		//    return aces;
		//}

		public AccessControlEntryBase GetByAceId(long id)
		{
			return this.SingleOrDefault( ace => ace.Id == id );
		}

		public ISuplexObject AddOrSynchronize(ISuplexObject item)
		{
			AccessControlEntryBase ace = (AccessControlEntryBase)item;

			AccessControlEntryBase exists = this.GetByAceId( ace.Id );
			if( exists == null )
			{
				this.Add( ace );
			}
			else
			{
				exists.Synchronize( ace );
				exists.SecurityPrincipalId = ace.SecurityPrincipalId;
				exists.IsDirty = false;
				ace = exists;
			}

			return ace;
		}

		public IEnumerable<AccessControlEntryBase> GetBySecurityPrincipal(st.ISecurityPrincipal principal)
		{
			return this.Where( ace => ace.SecurityPrincipalId == principal.Id );
		}

		public void Resolve()
		{
			this.Resolve( this.OwnerStore );
		}
		public void Resolve(SuplexStore ownerStore)
		{
			//foreach( AccessControlEntryBase ace in this.Items )
			//{
			//    ace.Resolve( ownerStore );
			//}
			for( int i = this.Items.Count - 1; i > -1; i-- )
			{
				this.Items[i].Resolve( ownerStore );
				this.Items[i].IsDirty = false;
			}
		}

		protected override void RemoveItem(int index)
		{
			_removedItems.Add( this[index] );
			base.RemoveItem( index );
		}
	}

	[XmlInclude( typeof( UIAuditAce ) ), XmlInclude( typeof( RecordAuditAce ) ), XmlInclude( typeof( FileSystemAuditAce ) ), XmlInclude( typeof( SynchronizationAuditAce ) )]
	[CollectionDataContract()]
	public class AuditAceCollection : ObservableObjectModelCollection<AccessControlEntryAuditBase>, ISuplexObjectList
	{
		List<AccessControlEntryBase> _removedItems = new List<AccessControlEntryBase>();

		public AuditAceCollection() : base() { }
		public AuditAceCollection(IObjectModel owner)
			: base( owner )
		{ }

		internal SuplexStore OwnerStore { get; set; }

		public List<AccessControlEntryBase> RemovedItems { get { return _removedItems; } }

		//public IEnumerable<AccessControlEntryAuditBase> GetByUIElement(UIElement uie)
		//{
		//    return this.Where( ace => ace.UIElementId == uie.Id );
		//}

		public AccessControlEntryAuditBase GetByAceId(long id)
		{
			return this.SingleOrDefault( ace => ace.Id == id );
		}

		public ISuplexObject AddOrSynchronize(ISuplexObject item)
		{
			AccessControlEntryAuditBase ace = (AccessControlEntryAuditBase)item;

			AccessControlEntryAuditBase exists = this.GetByAceId( ace.Id );
			if( exists == null )
			{
				this.Add( ace );
			}
			else
			{
				exists.Synchronize( ace );
				exists.SecurityPrincipalId = ace.SecurityPrincipalId;
				exists.IsDirty = false;
				ace = exists;
			}

			return ace;
		}

		public IEnumerable<AccessControlEntryAuditBase> GetBySecurityPrincipal(st.ISecurityPrincipal principal)
		{
			return this.Where( ace => ace.SecurityPrincipalId == principal.Id );
		}

		public void Resolve()
		{
			this.Resolve( this.OwnerStore );
		}
		public void Resolve(SuplexStore ownerStore)
		{
			for( int i = this.Items.Count - 1; i > -1; i-- )
			{
				this.Items[i].Resolve( ownerStore );
				this.Items[i].IsDirty = false;
			}
		}

		protected override void RemoveItem(int index)
		{
			_removedItems.Add( this[index] );
			base.RemoveItem( index );
		}
	}

	[XmlInclude( typeof( UIAce ) ), XmlInclude( typeof( RecordAce ) ), XmlInclude( typeof( FileSystemAce ) ), XmlInclude( typeof( SynchronizationAce ) )]
	[CollectionDataContract]
	public class AceCollectionEx<T> : AceCollection where T : AccessControlEntryBase
	{
		public AceCollectionEx() : base() { }
		public AceCollectionEx(IObjectModel owner)
			: base( owner )
		{ }
	}

	[XmlInclude( typeof( UIAuditAce ) ), XmlInclude( typeof( RecordAuditAce ) ), XmlInclude( typeof( FileSystemAuditAce ) ), XmlInclude( typeof( SynchronizationAuditAce ) )]
	[CollectionDataContract()]
	public class AuditAceCollectionEx<T> : AuditAceCollection where T : AccessControlEntryAuditBase
	{
		public AuditAceCollectionEx() : base() { }
		public AuditAceCollectionEx(IObjectModel owner)
			: base( owner )
		{ }
	}

	public class AceUtil
	{
		public AccessControlEntryBase ConvertAce(AccessControlEntryBase ace, ss.AceType newAceType)
		{
			Type aceType = System.Reflection.Assembly.GetExecutingAssembly().GetType(
				string.Format( "Suplex.Forms.ObjectModel.Api.{0}{1}Ace",
				newAceType, ace is ss.IAccessControlEntryAudit ? "Audit" : string.Empty ) );
			AccessControlEntryBase newAce = (AccessControlEntryBase)Activator.CreateInstance( aceType );

			newAce.Id = ace.Id;
			newAce.Allowed = ace.Allowed;
			newAce.Inherit = ace.Inherit;
			newAce.Right = ace.Right;
			newAce.SecurityPrincipal = ace.SecurityPrincipal;

			if( ace is ss.IAccessControlEntryAudit )
			{
				((ss.IAccessControlEntryAudit)newAce).Denied = ((ss.IAccessControlEntryAudit)ace).Denied;
			}

			return newAce;
		}
		public AccessControlEntryAuditBase ConvertAce(AccessControlEntryAuditBase ace, ss.AceType newAceType)
		{
			Type aceType = System.Reflection.Assembly.GetExecutingAssembly().GetType(
				string.Format( "Suplex.Forms.ObjectModel.Api.{0}{1}Ace",
				newAceType, ace is ss.IAccessControlEntryAudit ? "Audit" : string.Empty ) );
			AccessControlEntryAuditBase newAce = (AccessControlEntryAuditBase)Activator.CreateInstance( aceType );

			newAce.Id = ace.Id;
			newAce.Allowed = ace.Allowed;
			newAce.Denied = ace.Denied;
			newAce.Inherit = ace.Inherit;
			newAce.Right = ace.Right;
			newAce.SecurityPrincipal = ace.SecurityPrincipal;

			return newAce;
		}
	}

	public static class AceUtility
	{
		public static AccessControlEntryBase ConvertAce(AccessControlEntryBase ace, ss.AceType newAceType)
		{
			Type aceType = System.Reflection.Assembly.GetExecutingAssembly().GetType(
				string.Format( "Suplex.Forms.ObjectModel.Api.{0}{1}Ace",
				newAceType, ace is ss.IAccessControlEntryAudit ? "Audit" : string.Empty ) );
			AccessControlEntryBase newAce = (AccessControlEntryBase)Activator.CreateInstance( aceType );

			newAce.Id = ace.Id;
			newAce.Allowed = ace.Allowed;
			newAce.Inherit = ace.Inherit;
			newAce.Right = ace.Right;
			newAce.SecurityPrincipal = ace.SecurityPrincipal;

			if( ace is ss.IAccessControlEntryAudit )
			{
				((ss.IAccessControlEntryAudit)newAce).Denied = ((ss.IAccessControlEntryAudit)ace).Denied;
			}

			return newAce;
		}
	}

	public class AceEqualityComparer : IEqualityComparer<AccessControlEntryBase>
	{
		#region IEqualityComparer<IAccessControlEntry> Members
		public bool Equals(AccessControlEntryBase x, AccessControlEntryBase y)
		{
			return x.Id == y.Id;
		}

		public int GetHashCode(AccessControlEntryBase obj)
		{
			return obj.GetHashCode();
		}
		#endregion
	}
}