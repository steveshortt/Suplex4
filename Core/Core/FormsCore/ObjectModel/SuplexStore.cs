using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;

using sg = Suplex.General;


namespace Suplex.Forms.ObjectModel.Api
{
	//TODO: track changes to individual collection items for SuplexStore.IsDirty?
	[DataContract()]
	public class SuplexStore : INotifyPropertyChanged
	{
		private bool _isDirty = false;

		#region ctors
		public SuplexStore()
		{
			this.Ctor( new StreamingContext() );
		}

		[OnDeserializing]
		void Ctor(StreamingContext context)
		{
			this.UIElements = new UIElementCollection();
			this.Users = new UserCollection();
			this.Groups = new GroupCollection();
			this.GroupMembership = new GroupMembershipCollection( this );
			this.SecurityPrincipals = new ObservableCollection<SecurityPrincipalBase>();

			this.UIElements.CollectionChanged += new NotifyCollectionChangedEventHandler( this.UIElements_CollectionChanged );
			this.Users.CollectionChanged += new NotifyCollectionChangedEventHandler( this.UsersGroups_CollectionChanged );
			this.Groups.CollectionChanged += new NotifyCollectionChangedEventHandler( this.UsersGroups_CollectionChanged );
			this.GroupMembership.CollectionChanged += new NotifyCollectionChangedEventHandler( this.GroupMembership_CollectionChanged );
		}

		[OnDeserialized]
		void Ctor_Post(StreamingContext context)
		{
			this.UIElements.CollectionChanged += new NotifyCollectionChangedEventHandler( this.UIElements_CollectionChanged );
			this.Users.CollectionChanged += new NotifyCollectionChangedEventHandler( this.UsersGroups_CollectionChanged );
			this.Groups.CollectionChanged += new NotifyCollectionChangedEventHandler( this.UsersGroups_CollectionChanged );
			this.GroupMembership.CollectionChanged += new NotifyCollectionChangedEventHandler( this.GroupMembership_CollectionChanged );
		}
		#endregion

		#region Public Props, SecurityPrincipals
		[DataMember()]
		public UIElementCollection UIElements { get; set; }
		[DataMember()]
		public UserCollection Users { get; set; }
		[DataMember()]
		public GroupCollection Groups { get; set; }
		[IgnoreDataMember()]
		public GroupMembershipCollection GroupMembership { get; set; }

		[XmlIgnore()]
		[DataMember()]
		public ObservableCollection<SecurityPrincipalBase> SecurityPrincipals { get; set; }
		#endregion

		#region Methods
		public void Clear()
		{
			if( this.UIElements!= null ) { this.UIElements.Clear(); }
			if( this.Users != null ) { this.Users.Clear(); }
			if( this.Groups != null ) { this.Groups.Clear(); }
			if( this.GroupMembership != null ) { this.GroupMembership.Clear(); }
			if( this.SecurityPrincipals != null ) { this.SecurityPrincipals.Clear(); }
		}
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

		#region internal handlers
		void UsersGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
				{
					this.AddSecurityPrincipals( e.NewItems );
					break;
				}
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
				{
					this.AddSecurityPrincipals( e.NewItems );
					this.RemoveSecurityPrincipals( e.OldItems );
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					this.RemoveSecurityPrincipals( e.OldItems );
					break;
				}
			}

			this.IsDirty = true;
		}
		private void AddSecurityPrincipals(IList items)
		{
			if( items != null )
			{
				foreach( SecurityPrincipalBase sp in items )
				{
					this.SecurityPrincipals.Add( sp );
				}
			}
		}
		private void RemoveSecurityPrincipals(IList items)
		{
			int count = this.SecurityPrincipals.Count;
			if( items != null )
			{
				for( int i = items.Count - 1; i >= 0; i-- )
				{
					SecurityPrincipalBase sp = (SecurityPrincipalBase)items[i];
					this.SecurityPrincipals.Remove( sp );

					//in REST conection, can't find the object in line above for some reason
					if( count == this.SecurityPrincipals.Count )
					{
						sp = this.SecurityPrincipals.SingleOrDefault( s => s.Id == sp.Id );
						this.SecurityPrincipals.Remove( sp );
					}
					count = this.SecurityPrincipals.Count;
				}
			}
		}

		void UIElements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.IsDirty = true;
		}

		void GroupMembership_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.IsDirty = true;
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
	}
}