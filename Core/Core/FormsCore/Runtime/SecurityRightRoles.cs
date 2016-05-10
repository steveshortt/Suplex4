using System;
using System.Collections;

using Suplex.Security;
using System.Collections.Generic;

namespace Suplex.Forms
{
	public interface IRightRole
	{
		string ControlUniqueName { get; set; }
		ISecureControl ControlRef { get; set; }
		AceType AceType { get; set; }
		string RightName { get; set; }
		UIRight UIRight { get; set; }
	}

	public enum RightRoleType
	{
		Success,
		Else
	}

	public class RightRole
	{
		private string _controlUniqueName = null;
		private ISecureControl _controlRef = null;
		private AceType _aceType;
		private string _rightName = null;
		private UIRight _uiRight;


		public RightRole(){}


		public RightRole(string controlUniqueName, AceType aceType, string rightName, UIRight uiRight)
		{
			_controlUniqueName = controlUniqueName;
			_aceType = aceType;
			_rightName = rightName;
			_uiRight = uiRight;
		}

		public string ControlUniqueName
		{
			get { return _controlUniqueName; }
			set { _controlUniqueName = value; }
		}

		public ISecureControl ControlRef
		{
			get { return _controlRef; }
			set { _controlRef = value; }
		}

		public AceType AceType
		{
			get { return _aceType; }
			set { _aceType = value; }
		}

		public string RightName
		{
			get { return _rightName; }
			set { _rightName = value; }
		}

		public UIRight UIRight
		{
			get { return _uiRight; }
			set { _uiRight = value; }
		}
	}

	public class RightRoleCollection : List<RightRole>
	{
		public RightRoleCollection() : base() { }
		public RightRoleCollection(IEnumerable<RightRole> collection) : base( collection ) { }
		public RightRoleCollection(int capacity) : base( capacity ) { }
	}

/*
 * Deprecated
 * 
	public class RightRoleCollection : CollectionBase
	{
		public RightRole this[ int index ]
		{
			get
			{
				return( (RightRole)List[index] );
			}
			set
			{
				List[index] = value;
			}
		}


		public int Add( RightRole value )
		{
			return( List.Add( value ) );
		}


		public int IndexOf( RightRole value )
		{
			return( List.IndexOf( value ) );
		}


		public void Insert( int index, RightRole value )
		{
			List.Insert( index, value );
		}


		public void Remove( RightRole value )
		{
			List.Remove( value );
		}


		public bool Contains( RightRole value )
		{
			return( List.Contains( value ) );
		}
	}
 */
}