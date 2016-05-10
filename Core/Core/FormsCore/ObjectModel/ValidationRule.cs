using System;
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
	public abstract class LogicRule : INotifyPropertyChanged, INotifyPropertyChanging, INotifyDeleted, ISuplexObject
	{
		private string _name = string.Empty;
		private string _compareValue1 = string.Empty;
		private sf.ComparisonValueType _valueType1 = ComparisonValueType.Empty;
		private sf.ExpressionType _expressionType1 = ExpressionType.None;
		private string _compareValue2 = string.Empty;
		private sf.ComparisonValueType _valueType2 = ComparisonValueType.Empty;
		private sf.ExpressionType _expressionType2 = ExpressionType.None;
		private sf.ComparisonOperator _operator = ComparisonOperator.Empty;
		private bool _failParent = false;
		private string _errorMessage = string.Empty;
		private TypeCode _compareDataType = TypeCode.Empty;
		private int _sortOrder = 0;
		private bool _isDirty = false;

		public LogicRule()
		{
			Id = Guid.NewGuid();
		}

		#region props
		[XmlIgnore()]
		[IgnoreDataMember()]
		public abstract ObjectType LogicRuleObjectType { get; }

		[DataMember()]
		public LogicRuleType LogicRuleType { get; set; }

		[DataMember()]
		public Guid Id { get; set; }

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
		public string CompareValue1
		{
			get { return _compareValue1; }
			set
			{
				if( _compareValue1 != value )
				{
					_compareValue1 = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "iCompareValue1" );
				}
			}
		}

		[DataMember()]
		public sf.ComparisonValueType ValueType1
		{
			get { return _valueType1; }
			set
			{
				if( _valueType1 != value )
				{
					_valueType1 = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "ValueType1" );
				}
			}
		}

		[DataMember()]
		public sf.ExpressionType ExpressionType1
		{
			get { return _expressionType1; }
			set
			{
				if( _expressionType1 != value )
				{
					_expressionType1 = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "ExpressionType1" );
				}
			}
		}

		[DataMember()]
		public string CompareValue2
		{
			get { return _compareValue2; }
			set
			{
				if( _compareValue2 != value )
				{
					_compareValue2 = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "iCompareValue2" );
				}
			}
		}

		[DataMember()]
		public sf.ComparisonValueType ValueType2
		{
			get { return _valueType2; }
			set
			{
				if( _valueType2 != value )
				{
					_valueType2 = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "ValueType2" );
				}
			}
		}

		[DataMember()]
		public sf.ExpressionType ExpressionType2
		{
			get { return _expressionType2; }
			set
			{
				if( _expressionType2 != value )
				{
					_expressionType2 = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "ExpressionType2" );
				}
			}
		}

		[DataMember()]
		public sf.ComparisonOperator Operator
		{
			get { return _operator; }
			set
			{
				if( _operator != value )
				{
					_operator = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Operator" );
				}
			}
		}

		[DataMember()]
		public bool FailParent
		{
			get { return _failParent; }
			set
			{
				if( _failParent != value )
				{
					_failParent = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "FailParent" );
				}
			}
		}

		[DataMember()]
		public string ErrorMessage
		{
			get { return _errorMessage; }
			set
			{
				if( _errorMessage != value )
				{
					_errorMessage = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "ErrorMessage" );
				}
			}
		}

		[DataMember()]
		public TypeCode CompareDataType
		{
			get { return _compareDataType; }
			set
			{
				if( _compareDataType != value )
				{
					_compareDataType = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "CompareDataType" );
				}
			}
		}

		[DataMember()]
		public Int32 SortOrder
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

		#region ICloneable Members
		protected virtual LogicRule Clone(ObjectType type)
		{
			LogicRule r = null;
			switch( type )
			{
				case ObjectType.ValidationRule:
				{
					r = new ValidationRule();
					break;
				}
				case ObjectType.RightRoleRule:
				{
					r = new RightRoleRule();
					break;
				}
				default:
				{
					throw new ArgumentException( "Type must be either ValidationRule or RightRoleRule." );
				}
			}

			r.Id = this.Id;
			r.Name = this.Name;
			r.CompareValue1 = this.CompareValue1;
			r.ValueType1 = this.ValueType1;
			r.ExpressionType1 = this.ExpressionType1;
			r.CompareValue2 = this.CompareValue2;
			r.ValueType2 = this.ValueType2;
			r.ExpressionType2 = this.ExpressionType2;
			r.Operator = this.Operator;
			r.FailParent = this.FailParent;
			r.ErrorMessage = this.ErrorMessage;
			r.CompareDataType = this.CompareDataType;
			r.LogicRuleType = this.LogicRuleType;

			return r;
		}

		protected virtual void Synchronize(LogicRule sourceObject)
		{
			switch( sourceObject.LogicRuleObjectType )
			{
				case ObjectType.ValidationRule:
				case ObjectType.RightRoleRule:
				{
					break;
				}
				default:
				{
					throw new ArgumentException( "Type must be either ValidationRule or RightRoleRule." );
				}
			}

			this.Name = sourceObject.Name;
			this.SortOrder = sourceObject.SortOrder;
			this.CompareValue1 = sourceObject.CompareValue1;
			this.ValueType1 = sourceObject.ValueType1;
			this.ExpressionType1 = sourceObject.ExpressionType1;
			this.CompareValue2 = sourceObject.CompareValue2;
			this.ValueType2 = sourceObject.ValueType2;
			this.ExpressionType2 = sourceObject.ExpressionType2;
			this.Operator = sourceObject.Operator;
			this.FailParent = sourceObject.FailParent;
			this.ErrorMessage = sourceObject.ErrorMessage;
			this.CompareDataType = sourceObject.CompareDataType;
			this.LogicRuleType = sourceObject.LogicRuleType;
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

		#region INotifyDeleted Members
		private bool _isDeleted = false;
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
					//if( value ) { this.RecordDelete(); }
				}
			}
		}
		#endregion

		#region ISuplexObject Members
		[IgnoreDataMember()]
		public string ObjectId
		{
			get { return this.Id.ToString(); }
		}

		[IgnoreDataMember()]
		public ObjectType ObjectType
		{
			get { return this.LogicRuleObjectType; }
		}
		#endregion
	}

	[DataContract()]
	public class ValidationRule : LogicRule, IObjectModel, IObjectCollectionHost, ICloneable<ValidationRule>, INodeItem
	{
		public static ObjectType CloneShallow = ObjectType.None;
		public static ObjectType CloneDeep = ObjectType.ValidationRule | ObjectType.FillMap | ObjectType.ElseRule | ObjectType.ElseMap;

		private sf.ControlEvents _eventBinding = ControlEvents.Validating;
		private string _errorControl = string.Empty;
		private IObjectModel _parentObject = null;

		CompositeCollection _childObjects = null;

		public ValidationRule()
			: base()
		{
			this.Ctor( new StreamingContext() );
		}

		[OnDeserializing()]
		void Ctor(StreamingContext context)
		{
			this.LogicRuleType = sf.LogicRuleType.ValidationIf;
			this.FillMaps = new FillMapCollection( this );
			this.ValidationRules = new ValidationRuleCollection( this );
			this.ElseMaps = new FillMapCollection( this );
			this.ElseRules = new ValidationRuleCollection( this );
		}

		//this method is necessary to ensure Owner as GetUnintializedObject is called instead of default constructor
		[OnDeserialized()]
		public void CtorPost(StreamingContext context)
		{
			this.FillMaps.Owner = this;
			this.ValidationRules.Owner = this;
			this.ElseMaps.Owner = this;
			this.ElseRules.Owner = this;
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
				CollectionContainer vr = new CollectionContainer() { Collection = this.ValidationRules };
				CollectionContainer fm = new CollectionContainer() { Collection = this.FillMaps };
				CollectionContainer em = new CollectionContainer() { Collection = this.ElseMaps };
				CollectionContainer er = new CollectionContainer() { Collection = this.ElseRules };

				_childObjects = new CompositeCollection();
				_childObjects.Add( fm );
				_childObjects.Add( em );
				_childObjects.Add( er );
				_childObjects.Add( vr );
			}
		}

		[XmlIgnore()]
		[IgnoreDataMember()]
		public override ObjectType LogicRuleObjectType { get { return ObjectType.ValidationRule; } }

		[DataMember()]
		public string ErrorControl
		{
			get { return _errorControl; }
			set
			{
				if( _errorControl != value )
				{
					_errorControl = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "ErrorControl" );
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


		[DataMember()]
		public FillMapCollection FillMaps { get; set; }
		[DataMember()]
		public ValidationRuleCollection ValidationRules { get; set; }
		[DataMember()]
		public FillMapCollection ElseMaps { get; set; }
		[DataMember()]
		public ValidationRuleCollection ElseRules { get; set; }

		#region IObjectModel Members
		public ObjectType ObjectType
		{
			get
			{
				if( this.LogicRuleType == sf.LogicRuleType.ValidationIf )
				{
					return ObjectType.ValidationRule;
				}
				else
				{
					return ObjectType.ElseRule;
				}
			}
		}
		public ObjectType ValidChildObjectTypes { get { return ObjectType.ValidationRule | ObjectType.ElseRule | ObjectType.FillMap | ObjectType.ElseMap; } }
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
					this.ResolveParents();
					this.OnPropertyChanged( "ParentObject" );
				}
			}
		}
		[DataMember()]
		public Guid ParentId { get; set; }
		[DataMember()]
		public Guid ParentUIElementId { get; set; }
		public void ResolveParents()
		{
			if( this.ParentObject != null )
			{
				if( this.ParentObject.ObjectType == ObjectType.UIElement )
				{
					this.ParentUIElementId = ((UIElement)this.ParentObject).Id;
					this.ParentId = Guid.Empty;
				}
				else
				{
					//recurse-up to find the UIElement parent
					IObjectModel parentObject = this.ParentObject;
					while( parentObject != null && parentObject.ObjectType != ObjectType.UIElement )
					{
						parentObject = parentObject.ParentObject;
					}

					if( parentObject != null )
					{
						this.ParentUIElementId = ((UIElement)parentObject).Id;
					}
					this.ParentId = ((LogicRule)this.ParentObject).Id;
				}
			}
		}
		#endregion

		#region IObjectCollectionHost Members
		//NOTE: child objects of ValidationRules don't support their own EventBindings
		public void AddChildObject(IObjectModel child)
		{
			switch( child.ObjectType )
			{
				case ObjectType.ValidationRule:
				case ObjectType.ElseRule:
				{
					ValidationRule vr = (ValidationRule)child;

					vr.EventBinding = ControlEvents.None;

					if( vr.LogicRuleType == LogicRuleType.ValidationIf )
					{
						this.ValidationRules.Add( vr );
					}
					else
					{
						this.ElseRules.Add( vr );
					}
					break;
				}
				case ObjectType.FillMap:
				case ObjectType.ElseMap:
				{
					FillMap fm = (FillMap)child;

					fm.EventBinding = ControlEvents.None;

					if( fm.FillMapType == FillMapType.FillMapIf )
					{
						this.FillMaps.Add( fm );
					}
					else
					{
						this.ElseMaps.Add( fm );
					}
					break;
				}
			}

			child.ParentObject = this;
		}

		public void RemoveChildObject(IObjectModel child)
		{
			switch( child.ObjectType )
			{
				case ObjectType.ValidationRule:
				{
					this.ValidationRules.Remove( (ValidationRule)child );
					break;
				}
				case ObjectType.ElseRule:
				{
					this.ElseRules.Remove( (ValidationRule)child );
					break;
				}
				case ObjectType.FillMap:
				{
					this.FillMaps.Remove( (FillMap)child );
					break;
				}
				case ObjectType.ElseMap:
				{
					this.ElseMaps.Remove( (FillMap)child );
					break;
				}
			}

			child.ParentObject = null;
		}
		#endregion


		#region ICloneable<ValidationRule> Members
		public IObjectModel Clone(bool generateNewId)
		{
			ValidationRule clone = this.Clone( CloneShallow, false );
			if( generateNewId )
			{
				clone.Id = Guid.NewGuid();
			}
			return clone;
		}

		ValidationRule ICloneable<ValidationRule>.Clone()
		{
			return this.Clone( CloneShallow, false );
		}

		public ValidationRule Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			ValidationRule clone = (ValidationRule)base.Clone( ObjectType.ValidationRule );
			clone.ErrorControl = this.ErrorControl;
			clone.EventBinding = this.EventBinding;
			clone.ParentObject = this.ParentObject;

			if( (cloneDepth & ObjectType.ValidationRule) == ObjectType.ValidationRule )
			{
				foreach( ValidationRule vr in this.ValidationRules )
				{
					if( cloneChildrenAsRef )
					{
						clone.ValidationRules.Add( vr );
					}
					else
					{
						clone.ValidationRules.Add( vr.Clone( cloneDepth, cloneChildrenAsRef ) );
					}
				}
			}

			if( (cloneDepth & ObjectType.FillMap) == ObjectType.FillMap )
			{
				foreach( FillMap fm in this.FillMaps )
				{
					if( cloneChildrenAsRef )
					{
						clone.FillMaps.Add( fm );
					}
					else
					{
						clone.FillMaps.Add( fm.Clone() );
					}
				}
			}

			if( (cloneDepth & ObjectType.ElseRule) == ObjectType.ElseRule )
			{
				foreach( ValidationRule vr in this.ElseRules )
				{
					if( cloneChildrenAsRef )
					{
						clone.ElseRules.Add( vr );
					}
					else
					{
						clone.ElseRules.Add( vr.Clone( cloneDepth, cloneChildrenAsRef ) );
					}
				}
			}

			if( (cloneDepth & ObjectType.ElseMap) == ObjectType.ElseMap )
			{
				foreach( FillMap fm in this.ElseMaps )
				{
					if( cloneChildrenAsRef )
					{
						clone.ElseMaps.Add( fm );
					}
					else
					{
						clone.ElseMaps.Add( fm.Clone() );
					}
				}
			}

			return clone;
		}

		void ICloneable<ValidationRule>.Synchronize(ValidationRule sourceObject)
		{
			this.Synchronize( sourceObject, CloneShallow, false );
		}

		public void Synchronize(ValidationRule sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			base.Synchronize( sourceObject );
			this.ErrorControl = sourceObject.ErrorControl;
			this.EventBinding = sourceObject.EventBinding;

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
			if( (cloneDepth & ObjectType.ElseRule) == ObjectType.ElseRule )
			{
				foreach( ValidationRule rule in sourceObject.ElseRules )
				{
					if( cloneChildrenAsRef )
					{
						this.ElseRules.Add( rule );
					}
					else
					{
						ValidationRule found = this.ElseRules.GetByValidationRuleId( rule.Id );
						if( found != null )
						{
							found.Synchronize( rule, cloneDepth, cloneChildrenAsRef );
						}
						else
						{
							this.ElseRules.Add( rule.Clone( cloneDepth, cloneChildrenAsRef ) );
						}
					}
				}
			}
			if( (cloneDepth & ObjectType.ElseMap) == ObjectType.ElseMap )
			{
				foreach( FillMap fm in sourceObject.ElseMaps )
				{
					if( cloneChildrenAsRef )
					{
						this.ElseMaps.Add( fm );
					}
					else
					{
						FillMap found = this.ElseMaps.GetByFillMapId( fm.Id );
						if( found != null )
						{
							found.Synchronize( fm );
						}
						else
						{
							this.ElseMaps.Add( fm.Clone() );
						}
					}
				}
			}
		}

		#endregion

		#region INodeItem Members
		private bool _isExpanded = false;
		private bool _isSelected = false;
		private bool _isEditing = false;
		private bool _showDetail = false;
		private bool _showDetailPanels = false;
		private bool _enableLazyLoad = false;
		private bool _wantsDetailLazyLoad = true;
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
		#endregion
	}

	public class LazyLoadDummyValidationRule : ValidationRule { }

	[CollectionDataContract()]
	public class ValidationRuleCollection : ObservableObjectModelCollection<ValidationRule>, ISuplexObjectList
	{
		public ValidationRuleCollection() : base() { }
		public ValidationRuleCollection(IObjectModel owner)
			: base( owner )
		{ }

		public ValidationRule GetByValidationRuleId(Guid id)
		{
			return this.SingleOrDefault( vr => vr.Id == id );
		}

		public ISuplexObject AddOrSynchronize(ISuplexObject item)
		{
			ValidationRule vr = (ValidationRule)item;

			ValidationRule exists = this.GetByValidationRuleId( vr.Id );
			if( exists == null )
			{
				this.Add( vr );
			}
			else
			{
				exists.Synchronize( vr, ValidationRule.CloneDeep, false );
				exists.IsDirty = false;
				vr = exists;
			}

			return vr;
		}
	}
}