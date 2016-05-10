using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Xml.Serialization;
using System.Runtime.Serialization;

using sd = Suplex.Data;
using sf = Suplex.Forms;
using ss = Suplex.Security;


namespace Suplex.Forms.ObjectModel.Api
{
	//TODO: override serialization to handle having "default" settings (don't ser unless something was changed)
	//[Obsolete()]
	[DataContract()]
	public class SecurityDescriptor : ICloneable<SecurityDescriptor>, INotifyPropertyChanged
	{
		public const ss.AuditType DefaultSaclAuditTypeFilter =
			ss.AuditType.Information | ss.AuditType.Warning | ss.AuditType.Error |
			ss.AuditType.SuccessAudit | ss.AuditType.FailureAudit;

		private bool _daclInherit = true;
		private bool _saclInherit = true;
		private ss.AuditType _saclAuditTypeFilter = DefaultSaclAuditTypeFilter;

		private CompositeCollection _rightBindings = null;

		AceEqualityComparer _aceComparer = new AceEqualityComparer();

		public SecurityDescriptor() { }
		public SecurityDescriptor(UIElement owner)
		{
			this.Owner = owner;

			this.Dacl = new AceCollectionEx<UIAceDefault>( owner );
			this.Sacl = new AuditAceCollectionEx<UIAuditAceDefault>( owner );
			this.RightRoles = new RightRoleCollection( owner );
			this.RightRoleRules = new RightRoleRuleCollection( owner );

			this.Dacl.CollectionChanged += new NotifyCollectionChangedEventHandler( Child_CollectionChanged );
			this.Sacl.CollectionChanged += new NotifyCollectionChangedEventHandler( Child_CollectionChanged );
			this.RightRoles.CollectionChanged += new NotifyCollectionChangedEventHandler( Child_CollectionChanged );
			this.RightRoleRules.CollectionChanged += new NotifyCollectionChangedEventHandler( Child_CollectionChanged );
		}

		void Child_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.OnPropertyChanged( "Child_CollectionChanged" );
			this.OnPropertyChanged( "IsDefaultSecuritySettings" );
		}

		private void InitRightBindings()
		{
			RightRoleRule rightRolesHost = new RightRoleRule()
			{
				IsSealed = true,
				Name = "[Always]",
				RightRoles = this.RightRoles
			};
			CollectionContainer rr = new CollectionContainer() { Collection = this.RightRoles };
			rightRolesHost.ChildObjects.Clear();
			rightRolesHost.ChildObjects.Add( rr );

			CollectionContainer rrr = new CollectionContainer() { Collection = this.RightRoleRules };

			this.RightBindings = new CompositeCollection();
			this.RightBindings.Add( rightRolesHost );
			this.RightBindings.Add( rrr );
		}

		[XmlIgnore()]
		public UIElement Owner { get; set; }

		[DataMember]
		public bool DaclInherit
		{
			get { return _daclInherit; }
			set
			{
				if( _daclInherit != value )
				{
					_daclInherit = value;
					this.OnPropertyChanged( "DaclInherit" );
					this.OnPropertyChanged( "IsDefaultInheritanceSettings" );
					this.OnPropertyChanged( "IsDefaultSecuritySettings" );
				}
			}
		}

		[DataMember]
		public bool SaclInherit
		{
			get { return _saclInherit; }
			set
			{
				if( _saclInherit != value )
				{
					_saclInherit = value;
					this.OnPropertyChanged( "SaclInherit" );
					this.OnPropertyChanged( "IsDefaultInheritanceSettings" );
					this.OnPropertyChanged( "IsDefaultSecuritySettings" );
				}
			}
		}

		[DataMember]
		public ss.AuditType SaclAuditTypeFilter
		{
			get { return _saclAuditTypeFilter; }
			set
			{
				if( _saclAuditTypeFilter != value )
				{
					_saclAuditTypeFilter = value;
					this.OnPropertyChanged( "SaclAuditTypeFilter" );
					this.OnPropertyChanged( "IsDefaultSaclAuditTypeFilter" );
					this.OnPropertyChanged( "IsDefaultInheritanceSettings" );
					this.OnPropertyChanged( "IsDefaultSecuritySettings" );
				}
			}
		}

		[XmlIgnore()]
		public bool IsDefaultSaclAuditTypeFilter
		{
			get
			{
				return SaclAuditTypeFilter == DefaultSaclAuditTypeFilter;
			}
		}
		[XmlIgnore()]
		public bool IsDefaultInheritanceSettings
		{
			get
			{
				return DaclInherit && SaclInherit && (SaclAuditTypeFilter == DefaultSaclAuditTypeFilter);
			}
		}

		[XmlIgnore()]
		public bool IsDefaultSecuritySettings
		{
			get
			{
				return this.IsDefaultInheritanceSettings &&
					this.Dacl.Count == 0 && this.Sacl.Count == 0 &&
					this.RightRoles.Count == 0 && this.RightRoleRules.Count == 0;
			}
		}

		/// <summary>
		///this is a work-around prop for AceCollection/AuditAceCollection b/c
		///   the WpfToolkit DataGrid has a limitation that requires a generic argument
		///	  to provide a blank row for an empty collection
		/// </summary>
		[DataMember]
		public AceCollectionEx<UIAceDefault> Dacl { get; set; }
		[DataMember]
		public AuditAceCollectionEx<UIAuditAceDefault> Sacl { get; set; }
		[DataMember]
		public RightRoleCollection RightRoles { get; set; }
		[DataMember]
		public RightRoleRuleCollection RightRoleRules { get; set; }

		[XmlIgnore()]
		public int CompositeRightRuleCount
		{
			get { return this.RightRoleRules.CompositeRuleCount; }
			set { this.OnPropertyChanged( "CompositeRightRuleCount" ); }
		}
		[XmlIgnore()]
		public int CompositeRightRoleCount
		{
			get { return this.RightRoles.Count + this.RightRoleRules.CompositeRoleCount; }
			set { this.OnPropertyChanged( "CompositeRightRoleCount" ); }
		}
		//HACK: this is stub method to cover updating
		public void UpdateCompositeRightBindingCounts()
		{
			this.CompositeRightRuleCount = -1;
			this.CompositeRightRoleCount = -1;
		}

		[XmlIgnore()]
		[IgnoreDataMember()]
		public CompositeCollection RightBindings
		{
			get
			{
				if( _rightBindings == null )
				{
					this.InitRightBindings();
				}
				return _rightBindings;
			}
			private set { _rightBindings = value; }
		}

		#region ICloneable<SecurityDescriptor> Members
		public SecurityDescriptor Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			SecurityDescriptor sd = new SecurityDescriptor( this.Owner );

			if( (cloneDepth & ObjectType.Ace) == ObjectType.Ace )
			{
				sd.DaclInherit = this.DaclInherit;
				sd.SaclInherit = this.SaclInherit;
				sd.SaclAuditTypeFilter = this.SaclAuditTypeFilter;

				foreach( AccessControlEntryBase ace in this.Dacl )
				{
					if( cloneChildrenAsRef )
					{
						sd.Dacl.Add( ace );
					}
					else
					{
						sd.Dacl.Add( ace.CloneMemberwise() );
					}
				}
				foreach( AccessControlEntryAuditBase ace in this.Sacl )
				{
					if( cloneChildrenAsRef )
					{
						sd.Sacl.Add( ace );
					}
					else
					{
						sd.Sacl.Add( ace.CloneMemberwise() );
					}
				}
			}

			if( (cloneDepth & ObjectType.RightRole) == ObjectType.RightRole )
			{
				foreach( RightRole rr in this.RightRoles )
				{
					if( cloneChildrenAsRef )
					{
						sd.RightRoles.Add( rr );
					}
					else
					{
						sd.RightRoles.Add( rr.CloneMemberwise() );
					}
				}
			}

			if( (cloneDepth & ObjectType.RightRoleRule) == ObjectType.RightRoleRule )
			{
				foreach( RightRoleRule rrr in this.RightRoleRules )
				{
					if( cloneChildrenAsRef )
					{
						sd.RightRoleRules.Add( rrr );
					}
					else
					{
						sd.RightRoleRules.Add( rrr.Clone() );
					}
				}
			}

			return sd;
		}

		[DataMember]
		public List<long> DeleteDaclIds { get; set; }
		[DataMember]
		public List<long> DeleteSaclIds { get; set; }
		[DataMember]
		public List<long> DeleteRightRoleIds { get; set; }
		internal List<long> deleteRightRoleIds = null;
		[DataMember]
		public List<Guid> DeleteRightRoleRuleIds { get; set; }
		internal List<Guid> deleteRightRoleRuleIds = null;

		//todo: get the hell away from this and track changes internally:
		//	when an object IsDeleted = true, just leave it in collection and use UI Visibility settings to hide it,
		//  use IsDeleted=true at API to know what to delete
		public void SynchronizeDeleteIds(SecurityDescriptor sourceObject)
		{
			this.DeleteDaclIds = sourceObject.DeleteDaclIds;
			this.DeleteSaclIds = sourceObject.DeleteSaclIds;
			this.DeleteRightRoleIds = sourceObject.DeleteRightRoleIds;
			this.DeleteRightRoleRuleIds = sourceObject.DeleteRightRoleRuleIds;
		}

		internal bool HaveDeleteIds
		{
			get
			{
				return
					(this.DeleteDaclIds != null &&
					this.DeleteSaclIds != null &&
					this.DeleteRightRoleIds != null &&
					this.DeleteRightRoleRuleIds != null)
					&&
					(this.DeleteDaclIds.Count > 0 ||
					this.DeleteSaclIds.Count > 0 ||
					this.DeleteRightRoleIds.Count > 0 ||
					this.DeleteRightRoleRuleIds.Count > 0);
			}
		}
		public void Synchronize(SecurityDescriptor sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			if( (cloneDepth & ObjectType.Ace) == ObjectType.Ace )
			{
				this.DeleteDaclIds = new List<long>();
				this.DeleteSaclIds = new List<long>();
				this.DeleteRightRoleIds = new List<long>();
				this.deleteRightRoleIds = new List<long>();
				this.DeleteRightRoleRuleIds = new List<Guid>();
				this.deleteRightRoleRuleIds = new List<Guid>();

				this.DaclInherit = sourceObject.DaclInherit;
				this.SaclInherit = sourceObject.SaclInherit;
				this.SaclAuditTypeFilter = sourceObject.SaclAuditTypeFilter;

				//purge the Aces from |this| list that no longer exist in the sourceObject's Ace list.
				for( int i = this.Dacl.Count - 1; i > -1; i-- )
				{
					AccessControlEntryBase found = sourceObject.Dacl.GetByAceId( this.Dacl[i].Id );
					if( found == null )
					{
						DeleteDaclIds.Add( this.Dacl[i].Id );
						this.Dacl.RemoveAt( i );
					}
				}
				//Add the new Aces from the sourceObject to |this| list
				foreach( AccessControlEntryBase ace in sourceObject.Dacl )
				{

					if( cloneChildrenAsRef )
					{
						this.Dacl.Add( ace );
					}
					else
					{
						//int i = this.Dacl.IndexOf( ace );
						//bool exists = this.Dacl.Contains<AccessControlEntryBase>( ace, _aceComparer );
						//found.Allowed = ace.Allowed;
						//found.Inherit = ace.Inherit;
						//found.Right = ace.Right;
						//found.SecurityPrincipal = ace.SecurityPrincipal;

						AccessControlEntryBase found = this.Dacl.GetByAceId( ace.Id );
						if( found != null )
						{
							if( found.AceType == ace.AceType )
							{
								if( ace.IsDirty )
								{
									found.Synchronize( ace );
								}
							}
							else
							{
								int i = this.Dacl.IndexOf( found );
								this.Dacl[i] = ace.CloneMemberwise();
							}
						}
						else
						{
							this.Dacl.Add( ace.CloneMemberwise() );
						}
					}

					ace.IsDirty = false;
				}

				//purge the Aces from |this| list that no longer exist in the sourceObject's Ace list.
				for( int i = this.Sacl.Count - 1; i > -1; i-- )
				{
					AccessControlEntryAuditBase found = sourceObject.Sacl.GetByAceId( this.Sacl[i].Id );
					if( found == null )
					{
						DeleteSaclIds.Add( this.Sacl[i].Id );
						this.Sacl.RemoveAt( i );
					}
				}
				//Add the new Aces from the sourceObject to |this| list
				foreach( AccessControlEntryAuditBase ace in sourceObject.Sacl )
				{
					if( cloneChildrenAsRef )
					{
						this.Sacl.Add( ace );
					}
					else
					{
						AccessControlEntryAuditBase found = this.Sacl.GetByAceId( ace.Id );
						if( found != null )
						{
							if( found.AceType == ace.AceType )
							{
								if( ace.IsDirty )
								{
									found.Synchronize( ace );
								}
							}
							else
							{
								int i = this.Sacl.IndexOf( found );
								this.Sacl[i] = ace;
							}
						}
						else
						{
							this.Sacl.Add( ace.CloneMemberwise() );
						}
					}
				}
			}


			//purge the RightRoles from |this| list that no longer exist in the sourceObject's RightRoles list.
			for( int i = this.RightRoles.Count - 1; i > -1; i-- )
			{
				RightRole found = sourceObject.RightRoles.GetByRightRoleId( this.RightRoles[i].Id );
				if( found == null )
				{
					DeleteRightRoleIds.Add( this.RightRoles[i].Id );
					this.RightRoles.RemoveAt( i );
				}
			}
			//Add the new RightRoles from the sourceObject to |this| list
			if( (cloneDepth & ObjectType.RightRole) == ObjectType.RightRole )
			{
				foreach( RightRole rr in sourceObject.RightRoles )
				{
					if( cloneChildrenAsRef )
					{
						this.RightRoles.Add( rr );
					}
					else
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
				}
			}

			//purge the RightRoleRules from |this| list that no longer exist in the sourceObject's RightRoleRules list.
			for( int i = this.RightRoleRules.Count - 1; i > -1; i-- )
			{
				RightRoleRule found = sourceObject.RightRoleRules.GetByRightRoleRuleId( this.RightRoleRules[i].Id );
				if( found == null )
				{
					DeleteRightRoleRuleIds.Add( this.RightRoleRules[i].Id );
					this.RightRoleRules.RemoveAt( i );
				}
			}
			//Add the new RightRoleRules from the sourceObject to |this| list
			if( (cloneDepth & ObjectType.RightRoleRule) == ObjectType.RightRoleRule )
			{
				foreach( RightRoleRule rrr in sourceObject.RightRoleRules )
				{
					if( cloneChildrenAsRef )
					{
						this.RightRoleRules.Add( rrr );
					}
					else
					{
						RightRoleRule found = this.RightRoleRules.GetByRightRoleRuleId( rrr.Id );
						if( found != null )
						{
							//method-based recursion
							found.SynchronizeRecursive( rrr, ref deleteRightRoleRuleIds, ref deleteRightRoleIds );
						}
						else
						{
							this.RightRoleRules.Add( rrr.Clone() );
						}
					}
				}

				foreach( int rrId in deleteRightRoleIds )
				{
					this.DeleteRightRoleIds.Add( rrId );
				}
				foreach( Guid rrrId in deleteRightRoleRuleIds )
				{
					this.DeleteRightRoleRuleIds.Add( rrrId );
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

		#region ICloneable<SecurityDescriptor> Members
		IObjectModel ICloneableObject.Clone(bool generateNewId)
		{
			throw new NotImplementedException();
		}

		SecurityDescriptor ICloneable<SecurityDescriptor>.Clone()
		{
			throw new NotImplementedException();
		}

		SecurityDescriptor ICloneable<SecurityDescriptor>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		void ICloneable<SecurityDescriptor>.Synchronize(SecurityDescriptor sourceObject)
		{
			throw new NotImplementedException();
		}

		void ICloneable<SecurityDescriptor>.Synchronize(SecurityDescriptor sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}