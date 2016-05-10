using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Xml.Serialization;
using System.Runtime.Serialization;

using ss = Suplex.Security;


namespace Suplex.Forms.ObjectModel.Api
{
	[DataContract]
	public class UIElement : IObjectModel, IObjectCollectionHost,
		ICloneable<UIElement>, INotifyPropertyChanged, INodeItem, INotifyDeleted, ISuplexObject
	{
		private string _name = string.Empty;
		private string _controlType = string.Empty;
		private string _uniqueName = string.Empty;
		private string _desc = string.Empty;
		private bool _descTooltip = false;
		private bool _allowUndeclared = false;
		private TypeCode _dataType = TypeCode.Empty;
		private string _dataTypeErrMsg = string.Empty;
		private string _formatString = string.Empty;

		private bool _isDirty = false;
		private IObjectModel _parentObject = null;

		public static ObjectType CloneShallow = ObjectType.None;
		public static ObjectType CloneSecurity = ObjectType.Ace | ObjectType.RightRole | ObjectType.RightRoleRule;
		public static ObjectType CloneValidation = ObjectType.ValidationRule | ObjectType.FillMap | ObjectType.ElseRule | ObjectType.ElseMap;
		public static ObjectType CloneDeep = ObjectType.UIElement | CloneSecurity | CloneValidation;

		CompositeCollection _childObjects = null;

		public UIElement()
		{
			this.Ctor( new StreamingContext() );
		}

		[OnDeserializing()]
		public void Ctor(StreamingContext context)
		{
			Id = Guid.NewGuid();

			this.UIElements = new UIElementCollection( this );
			this.ValidationRules = new ValidationRuleCollection( this );
			this.FillMaps = new FillMapCollection( this );

			this.SecurityDescriptor = new SecurityDescriptor( this );
			this.SecurityDescriptor.PropertyChanged += new PropertyChangedEventHandler( SecurityDescriptor_PropertyChanged );
		}

		//this method is necessary to ensure Owner as GetUnintializedObject is called instead of default constructor
		[OnDeserialized()]
		public void CtorPost(StreamingContext context)
		{
			this.UIElements.Owner = this;
			this.ValidationRules.Owner = this;
			this.FillMaps.Owner = this ;
		}

		void SecurityDescriptor_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.IsDirty = true;
		}

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
				CollectionContainer uie = new CollectionContainer() { Collection = this.UIElements };
				CollectionContainer vr = new CollectionContainer() { Collection = this.ValidationRules };
				CollectionContainer fm = new CollectionContainer() { Collection = this.FillMaps };

				_childObjects = new CompositeCollection();
				_childObjects.Add( vr );
				_childObjects.Add( fm );
				_childObjects.Add( uie );
			}
		}


		#region Validation
		[DataMember]
		public Guid Id { get; set; }
		[IgnoreDataMember]
		public string ObjectId { get { return this.Id.ToString(); } }

		[DataMember]
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

		[DataMember]
		public string ControlType
		{
			get { return _controlType; }
			set
			{
				if( _controlType != value )
				{
					_controlType = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "ControlType" );
				}
			}
		}

		[DataMember]
		public string UniqueName
		{
			get { return _uniqueName; }
			set
			{
				if( _uniqueName != value )
				{
					_uniqueName = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "UniqueName" );
				}
			}
		}

		[DataMember]
		public string Description
		{
			get { return _desc; }
			set
			{
				if( _desc != value )
				{
					_desc = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Description" );
				}
			}
		}

		[DataMember]
		public bool UseDescriptionAsTooltip
		{
			get { return _descTooltip; }
			set
			{
				if( _descTooltip != value )
				{
					_descTooltip = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "UseDescriptionAsTooltip" );
				}
			}
		}

		[DataMember]
		public bool AllowUndeclared
		{
			get { return _allowUndeclared; }
			set
			{
				if( _allowUndeclared != value )
				{
					_allowUndeclared = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "AllowUndeclared" );
				}
			}
		}

		[DataMember]
		public TypeCode DataType
		{
			get { return _dataType; }
			set
			{
				if( _dataType != value )
				{
					_dataType = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "DataType" );
				}
			}
		}

		[DataMember]
		public string DataTypeErrorMessage
		{
			get { return _dataTypeErrMsg; }
			set
			{
				if( _dataTypeErrMsg != value )
				{
					_dataTypeErrMsg = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "DataTypeErrorMessage" );
				}
			}
		}

		[DataMember]
		public string FormatString
		{
			get { return _formatString; }
			set
			{
				if( _formatString != value )
				{
					_formatString = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "FormatString" );
				}
			}
		}


		[XmlIgnore()]
		[DataMember()]
		public Guid ParentId { get; set; }

		[DataMember]
		public ValidationRuleCollection ValidationRules { get; set; }
		[DataMember]
		public FillMapCollection FillMaps { get; set; }
		[DataMember]
		public UIElementCollection UIElements { get; set; }

		[IgnoreDataMember]
		[XmlIgnore()]
		[Obsolete( "in use?", true )]
		public System.Collections.IList Children
		{
			get { return this.UIElements; }
			set { this.UIElements = (UIElementCollection)value; }
		}
		#endregion

		#region Security
		[DataMember]
		public SecurityDescriptor SecurityDescriptor { get; set; }
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


		#region IObjectModel Members
		public ObjectType ObjectType { get { return ObjectType.UIElement; } }
		public ObjectType ValidChildObjectTypes { get { return ObjectType.UIElement | ObjectType.ValidationRule | ObjectType.RightRoleRule | ObjectType.FillMap; } }
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
					//this.OnPropertyChanging( "ParentObject" );
					_parentObject = value;
					this.ParentId = value is UIElement ? ((UIElement)value).Id : Guid.Empty;
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
				case ObjectType.ValidationRule:
				{
					ValidationRule vr = (ValidationRule)child;
					if( vr.EventBinding == ControlEvents.None )
					{
						vr.EventBinding = ControlEvents.Validating;
					}

					this.ValidationRules.Add( vr );

					break;
				}
				case ObjectType.FillMap:
				{
					FillMap fm = (FillMap)child;
					if( fm.EventBinding == ControlEvents.None )
					{
						fm.EventBinding = ControlEvents.Validating;
					}

					this.FillMaps.Add( fm );
					break;
				}
				case ObjectType.UIElement:
				{
					this.UIElements.Add( (UIElement)child );
					break;
				}
			}

			child.ParentObject = this;
		}

		public void RemoveChildObject(IObjectModel child)
		{
			switch( child.ObjectType )
			{
				case ObjectType.UIElement:
				{
					this.UIElements.Remove( (UIElement)child );
					break;
				}
				case ObjectType.ValidationRule:
				{
					this.ValidationRules.Remove( (ValidationRule)child );
					break;
				}
				case ObjectType.FillMap:
				{
					this.FillMaps.Remove( (FillMap)child );
					break;
				}
			}

			child.ParentObject = null;
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

		#region ICloneable<UIElement> Members
		public IObjectModel Clone(bool generateNewId)
		{
			UIElement clone = this.Clone( CloneShallow, false );
			if( generateNewId )
			{
				clone.Id = Guid.NewGuid();
			}
			return clone;
		}
		UIElement ICloneable<UIElement>.Clone()
		{
			return this.Clone( CloneShallow, false );
		}
		public UIElement Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			UIElement uie = new UIElement();

			uie.Id = this.Id;
			uie.Name = this.Name;
			uie.ControlType = this.ControlType;
			uie.UniqueName = this.UniqueName;
			uie.Description = this.Description;
			uie.UseDescriptionAsTooltip = this.UseDescriptionAsTooltip;
			uie.AllowUndeclared = this.AllowUndeclared;
			uie.DataType = this.DataType;
			uie.DataTypeErrorMessage = this.DataTypeErrorMessage;
			uie.FormatString = this.FormatString;

			uie.ParentObject = this.ParentObject;
			uie.ParentId = this.ParentId;	//not needed if working in UI, added it just for completeness/consistency

			uie.SecurityDescriptor = this.SecurityDescriptor.Clone( cloneDepth, cloneChildrenAsRef );

			if( (cloneDepth & ObjectType.ValidationRule) == ObjectType.ValidationRule )
			{
				foreach( ValidationRule rule in this.ValidationRules )
				{
					if( cloneChildrenAsRef )
					{
						uie.ValidationRules.Add( rule );
					}
					else
					{
						uie.ValidationRules.Add( rule.Clone( cloneDepth, cloneChildrenAsRef ) );
					}
				}
			}
			if( (cloneDepth & ObjectType.FillMap) == ObjectType.FillMap )
			{
				foreach( FillMap fm in this.FillMaps )
				{
					if( cloneChildrenAsRef )
					{
						uie.FillMaps.Add( fm );
					}
					else
					{
						uie.FillMaps.Add( fm.Clone() );
					}
				}
			}
			if( (cloneDepth & ObjectType.UIElement) == ObjectType.UIElement )
			{
				foreach( UIElement u in this.UIElements )
				{
					if( cloneChildrenAsRef )
					{
						uie.UIElements.Add( u );
					}
					else
					{
						uie.UIElements.Add( u.Clone( cloneDepth, cloneChildrenAsRef ) );
					}
				}
			}

			return uie;
		}

		void ICloneable<UIElement>.Synchronize(UIElement sourceObject)
		{
			this.Synchronize( sourceObject, CloneShallow, false );
		}

		public void Synchronize(UIElement sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			this.Name = sourceObject.Name;
			this.ControlType = sourceObject.ControlType;
			this.UniqueName = sourceObject.UniqueName;
			this.Description = sourceObject.Description;
			this.UseDescriptionAsTooltip = sourceObject.UseDescriptionAsTooltip;
			this.AllowUndeclared = sourceObject.AllowUndeclared;
			this.DataType = sourceObject.DataType;
			this.DataTypeErrorMessage = sourceObject.DataTypeErrorMessage;
			this.FormatString = sourceObject.FormatString;
			this.ParentId = sourceObject.ParentId;

			this.SecurityDescriptor.Synchronize( sourceObject.SecurityDescriptor, cloneDepth, cloneChildrenAsRef );

			if( (cloneDepth & ObjectType.ValidationRule) == ObjectType.ValidationRule )
			{
				foreach( ValidationRule rule in sourceObject.ValidationRules )
				{
					if( cloneChildrenAsRef )
					{
						this.ValidationRules.Add( rule );
					}
					else
					{
						ValidationRule found = this.ValidationRules.GetByValidationRuleId( rule.Id );
						if( found != null )
						{
							found.Synchronize( rule, cloneDepth, cloneChildrenAsRef );
						}
						else
						{
							this.ValidationRules.Add( rule.Clone( cloneDepth, cloneChildrenAsRef ) );
						}
					}
				}
			}
			if( (cloneDepth & ObjectType.FillMap) == ObjectType.FillMap )
			{
				foreach( FillMap fm in sourceObject.FillMaps )
				{
					if( cloneChildrenAsRef )
					{
						this.FillMaps.Add( fm );
					}
					else
					{
						FillMap found = this.FillMaps.GetByFillMapId( fm.Id );
						if( found != null )
						{
							found.Synchronize( fm );
						}
						else
						{
							this.FillMaps.Add( fm.Clone() );
						}
					}
				}
			}
			if( (cloneDepth & ObjectType.UIElement) == ObjectType.UIElement )
			{
				foreach( UIElement u in sourceObject.UIElements )
				{
					if( cloneChildrenAsRef )
					{
						this.UIElements.Add( u );
					}
					else
					{
						UIElement found = this.UIElements.GetById( u.Id );
						if( found != null )
						{
							found.Synchronize( u, cloneDepth, cloneChildrenAsRef );
						}
						else
						{
							this.UIElements.Add( u.Clone( cloneDepth, cloneChildrenAsRef ) );
						}
					}
				}
			}
		}

		//This is a special sync only to be used after a database Create/Update statement.
		//It's needed to sync the Ids on Child collections - Aces/RightBindings, in this case
		public void SynchronizeSpecial(UIElement sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef, bool isConnected)
		{
			if( isConnected )
			{
				this.SecurityDescriptor.Synchronize( sourceObject.SecurityDescriptor, cloneDepth, cloneChildrenAsRef );
				this.IsDirty = false;
			}
		}
		#endregion

		#region INodeItem Members
		private bool _isExpanded = false;
		private bool _isSelected = false;
		private bool _isSelectedAlternate = false;
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
		public bool IsSelectedAlternate
		{
			get { return _isSelectedAlternate; }
			set
			{
				_isSelectedAlternate = value;
				this.OnPropertyChanged( "IsSelectedAlternate" );
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

		public override string ToString()
		{
			return string.Format( @"{0} [{1}]", this.UniqueName, this.Id );
		}
	}

	public class LazyLoadDummyUIElement : UIElement
	{
	}

	public class UIElementCollection : ObservableObjectModelCollection<UIElement>, ISuplexObjectList
	{
		public UIElementCollection() : base() { }
		public UIElementCollection(IObjectModel owner)
			: base( owner )
		{ }

		public UIElement GetById(Guid id)
		{
			return this.FirstOrDefault( uie => uie.Id == id );
		}

		public UIElement GetByUniqueName(string uniqueName)
		{
			return this.FirstOrDefault( uie => uie.UniqueName == uniqueName );
		}

		public UIElement GetByIdRecursive(Guid id)
		{
			//return this.First( uie => uie.Id == id );
			return this.SelectRecursive( uie => (IEnumerable<UIElement>)uie.UIElements )
				.FirstOrDefault( uie => uie.Id == id );
		}

		public UIElement GetByUniqueNameRecursive(string uniqueName)
		{
			//return this.First( uie => uie.Id == id );
			return this.SelectRecursive( uie => (IEnumerable<UIElement>)uie.UIElements )
				.FirstOrDefault( uie => uie.UniqueName == uniqueName );
		}

		#region ISuplexObjectList Members
		public ISuplexObject AddOrSynchronize(ISuplexObject item)
		{
			UIElement uie = (UIElement)item;

			UIElement exists = this.GetById( uie.Id );
			if( exists == null )
			{
				this.Add( uie );
			}
			else
			{
				exists.Synchronize( uie, UIElement.CloneSecurity, false );
				uie = exists;
			}

			return uie;
		}
		#endregion
	}
}