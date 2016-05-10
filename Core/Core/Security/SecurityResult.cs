using System;
using System.Collections;
using System.Text;


namespace Suplex.Security
{

	public class SecurityResultCollection : ICollection
	{
		private SortedList _innerList = new SortedList( 3 );


		public SecurityResultCollection(){}


		public SecurityResult this[AceType aceType, object right]
		{
			get
			{
				return (SecurityResult)((SortedList)_innerList[aceType])[right];
			}
			set
			{
				((SortedList)_innerList[aceType])[right] = (SecurityResult)value;
			}
		}

		public void AddAceType(AceType aceType, bool accessAllowed, bool auditSuccess, bool auditFailure)
		{
			/// FIXED: 10/26/2004
			//TODO: possible bad implementation: prolly should change:
			// _innerList[aceType] = new SortedList( v.Length ) ...to:
			// _innerList.Add( aceType, new SortedList( v.Length ) );
			//That would facilitate accidentally overwriting the values for a given AceType.
			//Could add an "InitAceType(AceType aceType, Type rights)" method to explicitly re-init.
			/// FIXED: 10/26/2004

			object[] rights = AceTypeRights.GetRights( aceType );

			//_innerList[aceType] = new SortedList( rights.Length );
			_innerList.Add( aceType, new SortedList( rights.Length ) );

			for( int n=0; n<rights.Length; n++ )
			{
				((SortedList)_innerList[aceType])[rights[n]] =
					new SecurityResult( accessAllowed, auditSuccess, auditFailure );
			}
		}

		public void AddAceType(AceType aceType)
		{
			AddAceType( aceType, false, false, false );
		}

		public void InitAceType(AceType aceType, bool accessAllowed, bool auditSuccess, bool auditFailure)
		{
			object[] rights = AceTypeRights.GetRights( aceType );

			_innerList[aceType] = new SortedList( rights.Length );

			for( int n=0; n<rights.Length; n++ )
			{
				((SortedList)_innerList[aceType])[rights[n]] =
					new SecurityResult( accessAllowed, auditSuccess, auditFailure );
			}
		}

		public void InitAceType(AceType aceType)
		{
			InitAceType( aceType, false, false, false );
		}

		public void RemoveAceType(AceType aceType)
		{
			_innerList.Remove( aceType );
		}

		public void RemoveAceTypeAt(int aceTypeIndex)
		{
			_innerList.RemoveAt( aceTypeIndex );
		}

		public SecurityResult GetByIndex(int aceTypeIndex, int rightIndex)
		{
			return (SecurityResult)((SortedList)_innerList.GetByIndex( aceTypeIndex )).GetByIndex( rightIndex );
		}

		public SortedList GetAceTypeByIndex(int aceTypeIndex)
		{
			return (SortedList)_innerList.GetByIndex( aceTypeIndex );
		}

		public SortedList GetRightsByAceType(AceType aceType)
		{
			return (SortedList)_innerList[aceType];
		}

		public AceType GetAceKey(int aceTypeIndex)
		{
			return (AceType)_innerList.GetKey( aceTypeIndex );
		}

		public bool ContainsAceType(AceType aceType)
		{
			return _innerList.ContainsKey( aceType );
		}

		public ICollection GetAceTypes()
		{
			return _innerList.Keys;
		}

		#region ICollection Members

		public bool IsSynchronized
		{
			get
			{
				return _innerList.IsSynchronized;
			}
		}


		public int Count
		{
			get
			{
				return _innerList.Count;
			}
		}


		public void CopyTo(Array array, int index)
		{
			//TODO: implement this.
			throw new NotSupportedException();
		}


		public object SyncRoot
		{
			get
			{
				return _innerList.SyncRoot;
			}
		}


		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return new SecurityResultEnumerator( this );
		}


		#endregion

		public override string ToString()
		{
			StringBuilder ts = new StringBuilder();
			IEnumerator aceTypes = _innerList.Keys.GetEnumerator();
			while( aceTypes.MoveNext() )
			{
				AceType aceType = (AceType)aceTypes.Current;
				object[] rights = AceTypeRights.GetRights( aceType );
				for( int n = rights.Length - 1; n > -1; n-- )
				{
					ts.AppendFormat( "[{0}-{1}: {2}] ",
						aceType, rights[n], this[aceType, rights[n]].ToString() );

					//ts.AppendFormat( "[{0}-{1}: {2}/{3}/{4}] ",
					//    aceType, rights[n],
					//    FormatBool( this[aceType, rights[n]].AccessAllowed ),
					//    FormatBool( this[aceType, rights[n]].AuditSuccess ),
					//    FormatBool( this[aceType, rights[n]].AuditFailure ) );
				}
			}

			return ts.ToString().Trim();
		}

		//private string FormatBool(bool value)
		//{
		//    return value ? "T" : "F";
		//}
	}



	public class SecurityResult
	{
		bool _accessAllowed = false;
		bool _auditSuccess = false;
		bool _auditFailure = false;

		public SecurityResult(){}

		public SecurityResult(bool accessAllowed, bool auditSuccess, bool auditFailure)
		{
			_accessAllowed = accessAllowed;
			_auditSuccess = auditSuccess;
			_auditFailure = auditFailure;
		}

		public bool AccessAllowed
		{
			get { return _accessAllowed; }
			set { _accessAllowed = value; }
		}

		public bool AuditSuccess
		{
			get { return _auditSuccess; }
			set { _auditSuccess = value; }
		}

		public bool AuditFailure
		{
			get { return _auditFailure; }
			set { _auditFailure = value; }
		}

		public override string ToString()
		{
			return string.Format( "{0}/{1}/{2}",
				FormatBool( _accessAllowed ), FormatBool( _auditSuccess ), FormatBool( _auditFailure ) );
		}

		private string FormatBool(bool value)
		{
			return value ? "T" : "F";
		}
	}



	internal class SecurityResultEnumerator : IEnumerator
	{
		private int _srcIndex = -1, _rightIndex = -1;
		private SecurityResultCollection _src = new SecurityResultCollection();
		private SortedList _innerRights = new SortedList();


		public SecurityResultEnumerator(){}


		public SecurityResultEnumerator(SecurityResultCollection securityResults)
		{
			_src = securityResults;
		}


		public void Reset()
		{
			_srcIndex = -1;
			_rightIndex = -1;
			_innerRights = new SortedList();
		}


		public object Current
		{
			get
			{
				return new SecurityResultEntry(
					_src.GetAceKey( _srcIndex ),
					_innerRights.GetKey( _rightIndex ),
					(SecurityResult)_innerRights.GetByIndex( _rightIndex )
					);
			}
		}


		public bool MoveNext()
		{
			bool ok = true;

			if( _rightIndex < _innerRights.Count-1 )
			{
				_rightIndex++;
			}
			else
			{
				if( _srcIndex < _src.Count-1 )
				{
					_srcIndex++;
					_rightIndex = 0;
					_innerRights = _src.GetAceTypeByIndex( _srcIndex );
				}
				else
				{
					ok = false;
				}
			}

			return ok;
		}
	}



	public class SecurityResultEntry
	{
		private AceType _aceType;
		private object _right;
		private SecurityResult _securityResult;

		public SecurityResultEntry(){}

		public SecurityResultEntry(AceType aceType, object right, SecurityResult securityResult)
		{
			_aceType = aceType;
			_right = right;
			_securityResult = securityResult;
		}

		public AceType AceType
		{
			get { return _aceType; }
		}
		public object Right
		{
			get { return _right; }
		}
		public SecurityResult SecurityResult
		{
			get { return _securityResult; }
		}

		public override string ToString()
		{
			return string.Format( "{0}-{1}: {2}", _aceType, _right, _securityResult );
		}
	}

}//namespace