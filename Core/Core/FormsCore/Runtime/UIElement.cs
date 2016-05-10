using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Suplex.Security;

namespace Suplex.Forms
{
	public interface IUIElement
	{
		Guid Id { get; set; }
		string Name { get; set; }
		string ControlType { get; set; }
		string UniqueName { get; set; }
		string Desc { get; set; }
		bool DescTooltip { get; set; }
		bool AllowUndeclared { get; set; }
		TypeCode DataType { get; set; }
		string DataTypeErrMsg { get; set; }
		string FormatString { get; set; }
		Guid ParentId { get; set; }
		bool DaclInherit { get; set; }
		bool SaclInherit { get; set; }
		AuditType SaclAuditTypeFilter { get; set; }

		ValidationRuleCollection ValidationRules { get; set; }
		FillMapCollection FillMaps { get; set; }

		UIElementCollection UIElements { get; set; }
	}

	public class UIElement : IUIElement, IObjectModel
	{
		public UIElement()
		{
			Id = Guid.NewGuid();
			this.ValidationRules = new ValidationRuleCollection();
			this.FillMaps = new FillMapCollection();
			this.UIElements = new UIElementCollection();
		}

		public Guid Id { get; set; }
		public string Name { get; set; }
		public string ControlType { get; set; }
		public string UniqueName { get; set; }
		public string Desc { get; set; }
		public bool DescTooltip { get; set; }
		public bool AllowUndeclared { get; set; }
		public TypeCode DataType { get; set; }
		public string DataTypeErrMsg { get; set; }
		public string FormatString { get; set; }
		public Guid ParentId { get; set; }
		public bool DaclInherit { get; set; }
		public bool SaclInherit { get; set; }
		public AuditType SaclAuditTypeFilter { get; set; }

		public ValidationRuleCollection ValidationRules { get; set; }
		public FillMapCollection FillMaps { get; set; }

		public UIElementCollection UIElements { get; set; }


		#region IObjectModel Members
		public ObjectType ObjectType { get { return ObjectType.UIElement; } }
		public ObjectType ValidChildObjectTypes { get { return ObjectType.UIElement | ObjectType.ValidationRule | ObjectType.RightRoleRule | ObjectType.FillMap; } }
		public bool SupportsChildObjectType(ObjectType objectType)
		{
			return ( this.ValidChildObjectTypes & objectType ) == objectType;
		}
		[System.Xml.Serialization.XmlIgnore()]
		public IObjectModel ParentObject { get; set; }
		public bool IsDirty { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		#endregion
	}

	public class UIElementCollection : List<UIElement>
	{ }
}