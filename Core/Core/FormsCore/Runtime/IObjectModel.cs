using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suplex.Forms
{
	[Flags()]
	public enum ObjectType
	{
		None = 0,
		UIElement = 1,
		ValidationRule = 2,
		ElseRule = 4,
		FillMap = 8,
		ElseMap = 16,
		User = 32,
		Group = 64,
		Ace = 128,
		RightRole = 256,
		RightRoleRule = 512
	}

	public interface IObjectModel
	{
		string Name { get; set; }
		ObjectType ObjectType { get; }
		ObjectType ValidChildObjectTypes { get; }
		bool SupportsChildObjectType(ObjectType objectType);
		IObjectModel ParentObject { get; set; }
		bool IsDirty { get; set; }
	}

	//this is maintained as a separate interface so I can typecast to it
	public interface ICloneableObject
	{
		IObjectModel Clone(bool generateNewId);
	}

    public interface ICloneable<T> : ICloneableObject
    {
        T Clone();
		T Clone(ObjectType cloneDepth, bool cloneChildrenAsRef);
		void Synchronize(T sourceObject);
		void Synchronize(T sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef);
	}
}