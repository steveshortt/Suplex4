using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;


using Suplex.Data;
using Suplex.General;


namespace Suplex.Forms
{
	public interface ILogicRule
	{
		Guid Id { get; set; }
		string Name { get; set; }
		object CompareValue1 { get; set; }
		ComparisonValueType ValueType1 { get; set; }
		object CompareValue2 { get; set; }
		ComparisonValueType ValueType2 { get; set; }
		ComparisonOperator Operator { get; set; }
		bool FailParent { get; set; }
		string ErrorMessage { get; set; }
		TypeCode CompareDataType { get; set; }
	}
	public interface IValidationRule : ILogicRule
	{
		object ErrorControl { get; set; }
		FillMapCollection FillMaps { get; set; }
		ValidationRuleCollection ValidationRules { get; set; }
		FillMapCollection ElseMaps { get; set; }
		ValidationRuleCollection ElseRules { get; set; }
		ControlEvents EventBinding { get; set; }
	}
	public interface IRightRoleRule : ILogicRule
	{
		RightRoleRuleCollection RightRoleRules { get; set; }
		RightRoleCollection RightRoles { get; set; }
		RightRoleRuleCollection ElseRules { get; set; }
		RightRoleCollection ElseRoles { get; set; }
	}

	//note: this enum supports serializing rules and isn't to be used in application/consumer code
	public enum LogicRuleType
	{
		ValidationIf,		//primary "if" clause
		ValidationElse,		//"else" clause
		RightRoleIf,		//primary "if" clause
		RightRoleElse		//"else" clause
	}


	public enum ComparisonValueType
	{
		Empty,				//No value
		Singular,			//A static, constant value; either literal or regex
		Expression,			//A calculated value or set of values, provided in a list
		Control				//A value from a control
	}
	public enum ComparisonOperator
	{
		DataTypeCheck,		//A comparison for data type only. 
		Equal,				//A comparison for equality.
		NotEqual,			//A comparison for inequality.
		LessThan,			//A comparison for less than.
		LessThanEqual,		//A comparison for less than or equal to.
		GreaterThan,		//A comparison for greater than.
		GreaterThanEqual,	//A comparison for greater than or equal to.
		Required,			//A comparison for required data existence.
		InList,				//A comparison for a list match.
		NotInList,			//A comparison for a list exclusion.
		Empty,				//A comparison for data absence.
		NotEmpty,			//A comparison for data existence.
		RegularExpression	//A comparison for Regex match.
	}

	public abstract class LogicRule : ILogicRule
	{
		private object _compareValue1;
		private ComparisonValueType _valueType1;
		private object _compareValue2;
		private ComparisonValueType _valueType2;
		private ComparisonOperator _operator;
		private bool _failParent = true;
		private string _errorMessage = null;
		private TypeCode _compareDataType = TypeCode.String;

		public LogicRule(){}

		public LogicRule(object compareValue1, ComparisonValueType valueType1,
			object compareValue2, ComparisonValueType valueType2,
			ComparisonOperator comparisonOperator, TypeCode comparisonDataType)
		{
			_compareValue1 = compareValue1;
			_valueType1 = valueType1;
			_compareValue2 = compareValue2;
			_valueType2 = valueType2;
			_operator = comparisonOperator;
			_compareDataType = comparisonDataType;
			_errorMessage = null;
		}

		public LogicRule(object compareValue1, ComparisonValueType valueType1,
			object compareValue2, ComparisonValueType valueType2,
			ComparisonOperator comparisonOperator, TypeCode comparisonDataType, bool failParent, string errorMessage)
			: this( compareValue1, valueType1, compareValue2, valueType2, comparisonOperator, comparisonDataType )
		{
			_failParent = failParent;
			_errorMessage = errorMessage;
		}

		public virtual Guid Id { get; set; }
		public virtual string Name { get; set; }

		public virtual object CompareValue1
		{
			get { return _compareValue1; }
			set { _compareValue1 = value; }
		}
		public virtual ComparisonValueType ValueType1
		{
			get { return _valueType1; }
			set { _valueType1 = value; }
		}
		public virtual object CompareValue2
		{
			get { return _compareValue2; }
			set { _compareValue2 = value; }
		}
		public virtual ComparisonValueType ValueType2
		{
			get { return _valueType2; }
			set { _valueType2 = value; }
		}
		public virtual ComparisonOperator Operator
		{
			get { return _operator; }
			set { _operator = value; }
		}
		public virtual bool FailParent
		{
			get { return _failParent; }
			set { _failParent = value; }
		}
		public virtual string ErrorMessage
		{
			get { return _errorMessage; }
			set { _errorMessage = value; }
		}
		public virtual TypeCode CompareDataType
		{
			get { return _compareDataType; }
			set { _compareDataType = value; }
		}

		//TODO: update Clone proc to include Else members


		//#region ICloneable Members

		//internal LogicRule CloneUnResolved()
		//{
		//    LogicRule lr = new LogicRule();

		//    //copy props..
		//    lr.CompareDataType = this.CompareDataType;
		//    lr.ErrorMessage = this.ErrorMessage;
		//    lr.FailParent = this.FailParent;
		//    lr.Operator = this.Operator;
		//    lr.ValueType1 = this.ValueType1;
		//    lr.ValueType2 = this.ValueType2;
		//    lr.CompareValue1 = this.CompareValue1;
		//    lr.CompareValue2 = this.CompareValue2;

		//    if( this.CompareValue1 is ControlCompareValue )
		//    {
		//        lr.CompareValue1 = ( (ControlCompareValue)this.CompareValue1 ).UnparsedCompareValue;
		//    }
		//    if( this.CompareValue2 is ControlCompareValue )
		//    {
		//        lr.CompareValue2 = ( (ControlCompareValue)this.CompareValue2 ).UnparsedCompareValue;
		//    }

		//    return lr;
		//}

		//#endregion
	}


	/// <summary>
	/// A set of properties describing a given Validation Rule.
	/// </summary>
	[Serializable()]
	public class ValidationRule : LogicRule, IValidationRule, IObjectModel
	{
		private object _errorControl;
		private FillMapCollection _fillMaps = new FillMapCollection();
		private ValidationRuleCollection _rules = new ValidationRuleCollection();
		private FillMapCollection _elseMaps = new FillMapCollection();
		private ValidationRuleCollection _elseRules = new ValidationRuleCollection();
		private ControlEvents _eventBinding = ControlEvents.None;


		public ValidationRule()
			: base()
		{
		}

		public ValidationRule(object compareValue1, ComparisonValueType valueType1,
			object compareValue2, ComparisonValueType valueType2,
			ComparisonOperator comparisonOperator, TypeCode comparisonDataType)
			: base( compareValue1, valueType1, compareValue2, valueType2, comparisonOperator, comparisonDataType )
		{
		}

		public ValidationRule(object compareValue1, ComparisonValueType valueType1,
			object compareValue2, ComparisonValueType valueType2,
			ComparisonOperator comparisonOperator, TypeCode comparisonDataType, ControlEvents eventBinding)
			: base( compareValue1, valueType1, compareValue2, valueType2, comparisonOperator, comparisonDataType )
		{
			_eventBinding = eventBinding;
		}

		public ValidationRule(object compareValue1, ComparisonValueType valueType1,
			object compareValue2, ComparisonValueType valueType2,
			ComparisonOperator comparisonOperator, TypeCode comparisonDataType,
			bool failParent, string errorMessage, ControlEvents eventBinding)
			: base( compareValue1, valueType1, compareValue2, valueType2, comparisonOperator, comparisonDataType, failParent, errorMessage )
		{
			_eventBinding = eventBinding;
		}



		public object ErrorControl
		{
			get { return _errorControl; }
			set { _errorControl = value; }
		}
		public ValidationRuleCollection ValidationRules
		{
			get { return _rules; }
			set { _rules = value; }
		}
		public FillMapCollection FillMaps
		{
			get { return _fillMaps; }
			set { _fillMaps = value; }
		}
		public ValidationRuleCollection ElseRules
		{
			get { return _elseRules; }
			set { _elseRules = value; }
		}
		public FillMapCollection ElseMaps
		{
			get { return _elseMaps; }
			set { _elseMaps = value; }
		}
		public ControlEvents EventBinding
		{
			get { return _eventBinding; }
			set { _eventBinding = value; }
		}

		//TODO: update Clone proc to include Else members


		#region ICloneable Members

		internal ValidationRule CloneUnResolved()
		{
			ValidationRule vr = new ValidationRule();

			//copy props..
			vr.CompareDataType = this.CompareDataType;
			vr.ErrorMessage = this.ErrorMessage;
			vr.EventBinding = this.EventBinding;
			vr.FailParent = this.FailParent;
			vr.Operator = this.Operator;
			vr.ValueType1 = this.ValueType1;
			vr.ValueType2 = this.ValueType2;
			vr.CompareValue1 = this.CompareValue1;
			vr.CompareValue2 = this.CompareValue2;

			if( this.CompareValue1 is ControlCompareValue )
			{
				vr.CompareValue1 = ( (ControlCompareValue)this.CompareValue1 ).UnparsedCompareValue;
			}
			if( this.CompareValue2 is ControlCompareValue )
			{
				vr.CompareValue2 = ( (ControlCompareValue)this.CompareValue2 ).UnparsedCompareValue;
			}


			//iterate the FillMaps
			if( this.FillMaps.Count > 0 )
			{
				for( int m = 0; m < this.FillMaps.Count; m++ )
				{
					FillMap fm = this.FillMaps[m].CloneUnResolved();
					vr.FillMaps.Add( fm );
				}
			}

			//recurse the child ValidationRules
			if( this.ValidationRules.Count > 0 )
			{
				for( int n = 0; n < this.ValidationRules.Count; n++ )
				{
					vr.ValidationRules.Add( this.ValidationRules[n].CloneUnResolved() );
				}
			}


			return vr;
		}

		#endregion

		#region IObjectModel Members
		public ObjectType ObjectType { get { return ObjectType.ValidationRule; } }
		public ObjectType ValidChildObjectTypes { get { return ObjectType.ValidationRule | ObjectType.ElseRule | ObjectType.FillMap | ObjectType.ElseMap; } }
		public bool SupportsChildObjectType(ObjectType objectType)
		{
			return ( this.ValidChildObjectTypes & objectType ) == objectType;
		}
		[System.Xml.Serialization.XmlIgnore()]
		public IObjectModel ParentObject { get; set; }
		public bool IsDirty { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		#endregion
	}


	[Serializable()]
	public class RightRoleRule : LogicRule, IRightRoleRule, IObjectModel
	{
		private RightRoleRuleCollection _rules = new RightRoleRuleCollection();
		private RightRoleCollection _rightRoles = new RightRoleCollection();
		private RightRoleRuleCollection _elseRules = new RightRoleRuleCollection();
		private RightRoleCollection _elseRoles = new RightRoleCollection();


		public RightRoleRule()
			: base()
		{
		}

		public RightRoleRule(object compareValue1, ComparisonValueType valueType1,
			object compareValue2, ComparisonValueType valueType2,
			ComparisonOperator comparisonOperator, TypeCode comparisonDataType)
			: base( compareValue1, valueType1, compareValue2, valueType2, comparisonOperator, comparisonDataType )
		{
		}

		public RightRoleRule(object compareValue1, ComparisonValueType valueType1,
			object compareValue2, ComparisonValueType valueType2,
			ComparisonOperator comparisonOperator, TypeCode comparisonDataType,
			bool failParent, string errorMessage)
			: base( compareValue1, valueType1, compareValue2, valueType2, comparisonOperator, comparisonDataType, failParent, errorMessage )
		{
		}

		public RightRoleRuleCollection RightRoleRules
		{
			get { return _rules; }
			set { _rules = value; }
		}
		public RightRoleCollection RightRoles
		{
			get { return _rightRoles; }
			set { _rightRoles = value; }
		}
		public RightRoleRuleCollection ElseRules
		{
			get { return _elseRules; }
			set { _elseRules = value; }
		}
		public RightRoleCollection ElseRoles
		{
			get { return _elseRoles; }
			set { _elseRoles = value; }
		}

		//TODO: update Clone proc to include Else members


		#region ICloneable Members

		internal RightRoleRule CloneUnResolved()
		{
			RightRoleRule vr = new RightRoleRule();

			//copy props..
			vr.CompareDataType = this.CompareDataType;
			vr.ErrorMessage = this.ErrorMessage;
			vr.FailParent = this.FailParent;
			vr.Operator = this.Operator;
			vr.ValueType1 = this.ValueType1;
			vr.ValueType2 = this.ValueType2;
			vr.CompareValue1 = this.CompareValue1;
			vr.CompareValue2 = this.CompareValue2;

			if( this.CompareValue1 is ControlCompareValue )
			{
				vr.CompareValue1 = ( (ControlCompareValue)this.CompareValue1 ).UnparsedCompareValue;
			}
			if( this.CompareValue2 is ControlCompareValue )
			{
				vr.CompareValue2 = ( (ControlCompareValue)this.CompareValue2 ).UnparsedCompareValue;
			}


			//recurse the child RightRoleRules
			if( this.RightRoleRules.Count > 0 )
			{
				for( int n = 0; n < this.RightRoleRules.Count; n++ )
				{
					vr.RightRoleRules.Add( this.RightRoleRules[n].CloneUnResolved() );
				}
			}


			return vr;
		}

		#endregion

		#region IObjectModel Members
		public ObjectType ObjectType { get { return ObjectType.RightRoleRule; } }
		public ObjectType ValidChildObjectTypes { get { return ObjectType.RightRoleRule; } }
		public bool SupportsChildObjectType(ObjectType objectType)
		{
			return ( this.ValidChildObjectTypes & objectType ) == objectType;
		}
		[System.Xml.Serialization.XmlIgnore()]
		public IObjectModel ParentObject { get; set; }
		public bool IsDirty { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		#endregion
	}



	/// <summary>
	/// Provides a collection of ValidationRules.
	/// </summary>
	[Serializable()]
	public class ValidationRuleCollection : List<ValidationRule>
	{
		public ValidationRuleCollection() { }

		public ValidationResult Process(string defaultCompareValue, TypeCode defaultDataType, string dataTypeErrMessage, DataAccessor dataAccessor, ControlEvents eventBinding)
		{
			LogicRuleProcessor lrp = new LogicRuleProcessor();
			return lrp.ProcessValidationRules( this, defaultCompareValue, defaultDataType, dataTypeErrMessage, dataAccessor, eventBinding );
		}
	}

	[Serializable()]
	public class RightRoleRuleCollection : List<RightRoleRule>
	{
		private ISecureControl _owner = null;

		public RightRoleRuleCollection() { }
		public RightRoleRuleCollection(ISecureControl owner) { _owner = owner; }

		public ISecureControl Owner { get { return _owner; } set { _owner = value; } }

		public Result Process(string defaultCompareValue, TypeCode defaultDataType, string dataTypeErrMessage, DataAccessor dataAccessor)
		{
			LogicRuleProcessor lrp = new LogicRuleProcessor();
			return lrp.ProcessRightRoleRules( this, defaultCompareValue, defaultDataType, dataTypeErrMessage, dataAccessor, _owner );
		}
	}



	/// <summary>
	/// 
	/// </summary>
	internal class LogicRuleProcessor
	{
		private TypeCode _defaultDataType;
		private string _defaultCompareValue;
		private DataAccessor _da = null;
		private DataComparer _dataComparer = new DataComparer();

		internal LogicRuleProcessor() { }


		internal ValidationResult ProcessValidationRules(ValidationRuleCollection rc, string defaultCompareValue, TypeCode defaultDataType, string dataTypeErrMessage, DataAccessor appDataAccessor, ControlEvents eventBinding)
		{
			_defaultCompareValue = defaultCompareValue;
			_defaultDataType = defaultDataType;
			_da = appDataAccessor;

			//this test must occur here so basic datatype can be validated before 
			//any conditions are processed
			ValidationResult result =
				new ValidationResult( _dataComparer.DataTypeCheck( _defaultDataType, _defaultCompareValue ) );

			//if the comparison data is of the appropriate type, process the validation rules
			if( result.Success )
			{
				result = this.RecurseEventRules( rc, eventBinding );
			}
			else
			{
				if( !string.IsNullOrEmpty( dataTypeErrMessage ) )
				{
					result.Message = dataTypeErrMessage;
				}
			}

			return result;
		}

		private ValidationResult RecurseEventRules(ValidationRuleCollection rc, ControlEvents eventBinding)
		{
			//NOTE: optimistic result initialization here: if no rules to process, don't throw an error
			//	12/26/2008, modified ValidationResult to be pessimistic, this is one of a few that needs optimistic init
			ValidationResult result = new ValidationResult( true );

			IEnumerator rules = rc.GetEnumerator();
			while( rules.MoveNext() && result.Success )
			{
				ValidationRule r = (ValidationRule)rules.Current;

				if( r.EventBinding == eventBinding )//if-outer
				{
					result.SetResult( this.ProcessRule( r ) );

					if( result.Success )//if-inner
					{
						// process the FillMaps for this rule, only on success
						r.FillMaps.Process( _da, ControlEvents.None );	//03102004: Added ", ControlEvents.None"

						if( r.ValidationRules.Count > 0 )
						{
							result = this.RecurseEventRules( r.ValidationRules, ControlEvents.None );
						}
					}
					else
					{
						//stop processing rules due to failure
						//	but only pass result back as failure if( FailParent == true )

						if( r.FailParent )
						{
							if( !string.IsNullOrEmpty( r.ErrorMessage ) )
							{
								result.Message = r.ErrorMessage;
							}
							if( r.ErrorControl != null )
							{
								result.ErrorControl = (IValidationControl)r.ErrorControl;
							}
						}
						else
						{
							// process the ElseMaps for this rule, only on fail
							r.ElseMaps.Process( _da, ControlEvents.None );	//03102004: Added ", ControlEvents.None"

							if( r.ElseRules.Count > 0 )
							{
								result = this.RecurseEventRules( r.ElseRules, ControlEvents.None );
							}
							else
							{
								//03/04/09: home sick, pharyngitis, may need to revisit this change:
								//	this was setting result.Success to false, indicating failure, but should be true,
								//	indicating Success, or, more properly, non-logic-tree failure even though the condition failed.
								result.SetResult( true, string.Empty );
							}
						}
					}//if-inner

				}//if-outer
			}//while

			return result;
		}

		internal Result ProcessRightRoleRules(RightRoleRuleCollection rc, string defaultCompareValue, TypeCode defaultDataType, string dataTypeErrMessage, DataAccessor appDataAccessor, ISecureControl owner)
		{
			_defaultCompareValue = defaultCompareValue;
			_defaultDataType = defaultDataType;
			_da = appDataAccessor;

			//this test must occur here so basic datatype can be validated before 
			//any conditions are processed
			Result result = _dataComparer.DataTypeCheck( _defaultDataType, _defaultCompareValue );

			//if the comparison data is of the appropriate type, process the RightRole rules
			if( result.Success )
			{
				result = this.RecurseRightRoleRules( rc, owner );
			}
			else
			{
				if( !string.IsNullOrEmpty( dataTypeErrMessage ) )
				{
					result.Message = dataTypeErrMessage;
				}
			}

			return result;
		}

		private Result RecurseRightRoleRules(RightRoleRuleCollection rc, ISecureControl owner)
		{
			//NOTE: optimistic result initialization here: if no rules to process, don't throw an error
			Result result = new Result( true );

			IEnumerator rules = rc.GetEnumerator();
			while( rules.MoveNext() && result.Success )
			{
				RightRoleRule r = (RightRoleRule)rules.Current;

				result.SetResult( this.ProcessRule( r ) );

				if( result.Success )//if-inner
				{
					owner.Security.RightRoles.AddRange( r.RightRoles );

					if( r.RightRoleRules.Count > 0 )
					{
						result = this.RecurseRightRoleRules( r.RightRoleRules, owner );
					}
				}
				else
				{
					//stop processing rules due to failure
					//	but only pass result back as failure if( FailParent == true )

					if( r.FailParent )
					{
						if( !string.IsNullOrEmpty( r.ErrorMessage ) )
						{
							result.Message = r.ErrorMessage;
						}
					}
					else
					{
						owner.Security.RightRoles.AddRange( r.ElseRoles );

						if( r.ElseRules.Count > 0 )
						{
							result = this.RecurseRightRoleRules( r.ElseRules, owner );
						}
						else
						{
							//10/24/2010: setting result.Success to true to match logic in 03/04/09_ValidationRules_Note above
							result.SetResult( true, string.Empty );
						}
					}
				}//if-inner
			}//while

			return result;
		}

		private Result ProcessRule(ILogicRule rule)
		{
			Result result = new Result( true, null );

			TypeCode tc = _defaultDataType;

			switch( rule.Operator )
			{
				case ComparisonOperator.InList:
				case ComparisonOperator.NotInList:
				{
					result = _dataComparer.CompareList( rule.CompareValue2, rule.Operator, _da );
					break;
				}
				default:
				{
					string value1 = this.GetCompText( rule.CompareValue1, rule.ValueType1 );
					string value2 = this.GetCompText( rule.CompareValue2, rule.ValueType2 );

					if( rule.CompareValue1 != null || rule.CompareValue2 != null )
					{
						tc = rule.CompareDataType;
					}

					if( tc == TypeCode.String )
					{
						result = _dataComparer.CompareStrings( value1, value2, rule.Operator );
					}
					else
					{
						result = _dataComparer.CompareType( tc, value1, value2, rule.Operator );
					}
					break;
				}
			}

			return result;
		}

		private string GetCompText(object value, ComparisonValueType type)
		{
			//this initialization covers ComparisonValueType.Empty
			string ret = null;

			if( value == null )
			{
				ret = _defaultCompareValue;
			}
			else
			{
				switch( type )
				{
					case ComparisonValueType.Singular:
					{
						ret = value.ToString();
						break;
					}
					case ComparisonValueType.Control:
					{
						ret = ( (ControlCompareValue)value ).GetValue().ToString();
						break;
					}
					case ComparisonValueType.Expression:
					{
						ExpressionHandler eh = new ExpressionHandler();
						switch( ( (ExpressionElements)value ).ExprType )
						{
							case ExpressionType.Calculation:
							{
								ret = eh.Calculate( (ExpressionElements)value, _da );
								break;
							}
							case ExpressionType.Script:	//added Script support, 02102007
							{
								ret = eh.EvalScript( (ExpressionElements)value );
								break;
							}
						}
						break;
					}
				}
			}

			return ret;
		}
	}
}