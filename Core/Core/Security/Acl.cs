using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;


namespace Suplex.Security
{

	/// <summary>
	/// Provides a definition for a collection of IAccessControlEntries.
	/// </summary>
	/// <remarks>
	/// This AccessControlListBase structure inherits from SortedList to ease adding/removing ACEs.
	///   ACEs can be given a unique key for easy reference in the ACL, instead of having to 
	///   match by object equivalence.
	/// </remarks>
	public abstract class AccessControlListBase : Dictionary<int, IAccessControlEntry>
	{

		private bool _inheritAces = true;

		public AccessControlListBase() { }


		//// Indexer implementation.
		new public IAccessControlEntry this[int key]
		{
			get
			{
				return base[key];
			}
			set
			{
				if( this is DiscretionaryAcl )
				{
					((DiscretionaryAcl)this)[key] = value;
				}
				else
				{
					base[key] = value;
				}
			}
		}

		//new public virtual IAccessControlEntry this[object key]
		//{
		//    get
		//    {
		//        return (IAccessControlEntry)base[key];
		//    }
		//    set
		//    {
		//        //will add value if not in list, automatic
		//        base[key] = (IAccessControlEntry)value;
		//    }
		//}

		//public virtual void Add(object key, IAccessControlEntry ace)
		//{
		//    base.Add( key, ace );
		//}

		//public override void Clear()
		//{
		//    base.Clear();
		//}

		//public bool ContainsValue(IAccessControlEntry ace)
		//{
		//    return base.ContainsValue( ace );
		//}

		//public int IndexOfValue(IAccessControlEntry ace)
		//{
		//    return base.IndexOfValue( ace );
		//}

		//public override void Remove(object key)
		//{
		//    base.Remove( key );
		//}

		//public override void RemoveAt(int index)
		//{
		//    base.RemoveAt( index );
		//}

		//public virtual void SetByIndex(int index, IAccessControlEntry ace)
		//{
		//    base.SetByIndex( index, ace );
		//}

		public bool InheritAces
		{
			get { return _inheritAces; }
			set { _inheritAces = value; }
		}

		public bool ContainsAceType(AceType aceType)
		{
			bool result = false;
			foreach( IAccessControlEntry ace in this.Values )
			{
				if( ace.AceType == aceType )
				{
					result = true;
					break;
				}
			}
			return result;
		}

		/// <summary>
		/// Adds or updates all Aces in the destination ACL, as appropriate.
		/// </summary>
		/// <param name="acl">The destination ACL.</param>
		public void CopyTo(AccessControlListBase acl)
		{
			CopyTo( acl, string.Empty );
		}

		/// <summary>
		/// Adds or updates all Aces in the destination ACL, as appropriate.
		/// </summary>
		/// <param name="acl">The destination ACL.</param>
		public void CopyTo(AccessControlListBase acl, string sourceName)
		{
			foreach( int key in this.Keys )
			{
				if( ((IAccessControlEntry)this[key]).Inherit )
				{
					if( !(acl.ContainsKey( key ) &&
						(((IAccessControlEntry)acl[key]).InheritedFrom == string.Empty ||
						((IAccessControlEntry)acl[key]).InheritedFrom == null)) )
					{
						acl[key] = (IAccessControlEntry)((IAccessControlEntry)this[key]).Clone();

						((IAccessControlEntry)acl[key]).InheritedFrom = sourceName;
					}
				}
			}
		}

		/// <summary>
		/// Adds or updates Aces of the specified type in the destination ACL, as appropriate.
		/// </summary>
		/// <param name="acl">The destination ACL.</param>
		public void CopyTo(AccessControlListBase acl, AceType aceType)
		{
			this.CopyTo( acl, aceType, string.Empty );
		}

		/// <summary>
		/// Adds or updates Aces of the specified type in the destination ACL, as appropriate.
		/// </summary>
		/// <param name="acl">The destination ACL.</param>
		public void CopyTo(AccessControlListBase acl, AceType aceType, string sourceName)
		{
			foreach( int key in this.Keys )
			{
				IAccessControlEntry ace = ((IAccessControlEntry)this[key]);

				if( ace.Inherit && ace.AceType == aceType )
				{
					if( !(acl.ContainsKey( key ) &&
						(((IAccessControlEntry)acl[key]).InheritedFrom == string.Empty ||
						((IAccessControlEntry)acl[key]).InheritedFrom == null)) )
					{
						acl[key] = (IAccessControlEntry)ace.Clone();

						((IAccessControlEntry)acl[key]).InheritedFrom = sourceName;
					}
				}
			}
		}
	}



	/// <summary>
	/// Provides a collection of IAccessControlEntries.
	/// </summary>
	/// <remarks>
	/// This class is designed to mimic an NTFS DACL (sort of).  Whenever an IAccessControlEntry
	///   is Inserted or Updated, the _sortedValues list is sorted so IAccessControlEntry.Allowed 
	///   entries are at the beginning of the collection.  
	/// During enumeration, use DiscretionaryAcl.SortedAces so that Allowed ACEs are processed
	///   first and Denied ACEs are processed last, thereby allowing all possible permissions 
	///   before removing specifically denied permissions.
	/// This DACL structure inherits from SortedList to ease adding/removing ACEs.  ACEs can
	///   be given a unique key for easy reference in the DACL, instead of having to match by
	///   object equivalence.
	/// The InheritanceFlag on the DACL can allow/deny the entire DACL from inheriting ACEs, while
	///   the ACE-level inheritance flag is used to allow/deny an individual ACE from being inherited.
	/// </remarks>
	public class DiscretionaryAcl : AccessControlListBase
	{
		private ArrayList _sortedValues = new ArrayList();
		private AceSorter _aceSorter = new AceSorter( false );
		private int i = -1;

		public DiscretionaryAcl() { }


		// Indexer implementation.
		new public IAccessControlEntry this[int key]
		{
			get
			{
				//return (IAccessControlEntry)base[key];
				return base[key];
			}
			set
			{
				//will add value if not in list, automatic
				((Dictionary<int, IAccessControlEntry>)this)[key] = (IAccessControlEntry)value;

				//find object in _sortedValues before updating base[key] (below)
				//will add value if not in list, manual
				i = _sortedValues.IndexOf( base[key] );
				if( i >= 0 )
				{
					_sortedValues[i] = (IAccessControlEntry)value;
				}
				else
				{
					_sortedValues.Add( (IAccessControlEntry)value );
				}
				i = -1;
				_sortedValues.Sort( _aceSorter );
				//_sortedValues.Sort( _AllowedSorter );
			}
		}

		new public void Add(int key, IAccessControlEntry ace)
		{
			base.Add( key, ace );

			_sortedValues.Add( ace );
			_sortedValues.Sort( _aceSorter );
			//_sortedValues.Sort( _AllowedSorter );
		}

		new public void Clear()
		{
			_sortedValues.Clear();
			base.Clear();
		}

		new public bool Remove(int key)
		{
			_sortedValues.Remove( base[key] );
			return base.Remove( key );
		}

		//public override void RemoveAt(int index)
		//{
		//    i = _sortedValues.IndexOf( (IAccessControlEntry)base.GetByIndex(index) );
		//    if( i >= 0 )
		//    {
		//        _sortedValues.RemoveAt( i );
		//    }
		//    i = -1;

		//    base.RemoveAt( index );
		//}


		//public override void SetByIndex(int index, IAccessControlEntry ace)
		//{
		//    i = _sortedValues.IndexOf( (IAccessControlEntry)base.GetByIndex(index) );
		//    if( i >= 0 )
		//    {
		//        _sortedValues[i] = ace;
		//        _sortedValues.Sort( _aceSorter );
		//        //_sortedValues.Sort( _AllowedSorter );
		//    }
		//    i = -1;

		//    base.SetByIndex( index, ace );
		//}

		public ArrayList SortedAces
		{
			get
			{
				return _sortedValues;
			}
		}

		/// <summary>
		/// Adds or updates all Aces in the destination Dacl, as appropriate.
		/// </summary>
		/// <param name="Dacl">The destination Dacl.</param>
		/// <param name="CopyParameters">Indicates whether to copy Ace Parameters.</param>
		public void CopyTo(DiscretionaryAcl dacl)
		{
			base.CopyTo( dacl );
		}

		/// <summary>
		/// Adds or updates Aces of the specified type in the destination Dacl, as appropriate.
		/// </summary>
		/// <param name="Dacl">The destination Dacl.</param>
		/// <param name="CopyParameters">Indicates whether to copy Ace Parameters.</param>
		public void CopyTo(DiscretionaryAcl dacl, AceType aceType)
		{
			base.CopyTo( dacl, aceType );
		}

		public void HasAccess(AceType aceType, SecurityResultCollection securityResults)
		{
			if( !securityResults.ContainsAceType( aceType ) )
			{
				securityResults.AddAceType( aceType );
			}

			/*
			 * Logic: Look in the Dacl for aces of the given AceType, create a new mask of the combined rights.
			 * If Allowed: bitwise-OR the value into the mask.
			 * If Deined: if the mask contains the value, XOR the value out.
			 * The result mask contains only the allowed rights.
			 */
			int mask = 0;
			int aceRight = 0;
			foreach( IAccessControlEntry ace in this.SortedAces )
			{
				if( ace.AceType == aceType )
				{
					aceRight = (int)ace.Right;

					if( ace.Allowed )
					{
						mask |= aceRight;
					}
					else
					{
						if( (mask & aceRight) == aceRight )
						{
							mask ^= aceRight;
						}
					}
				}
			}

			/*
			 * For each right of the given acetype, perform a bitwise-AND to see if the right is specified in the mask.
			 */
			object[] rights = AceTypeRights.GetRights( aceType );
			for( int n = 0; n < rights.Length; n++ )
			{
				securityResults[aceType, rights[n]].AccessAllowed = (mask & (int)rights[n]) == (int)rights[n];
			}

			RightsAccessorAttribute attrib =
				(RightsAccessorAttribute)Attribute.GetCustomAttribute( AceTypeRights.RightType, typeof( RightsAccessorAttribute ) );
			if( attrib != null && attrib.HasMask )
			{
				this.HasAccessExtended( aceType, mask, securityResults, attrib );
			}
		}

		//HACK: Minor hack to address extended rights where the bitmask doesn't bear it out naturally.
		//NOTE: If I was smarter, I would be able to figure out the right bitmask for SyncRights and then probably drop the hack.
		private void HasAccessExtended(AceType aceType, int summaryMask, SecurityResultCollection sr, RightsAccessorAttribute ra)
		{
			switch( aceType )
			{
				case AceType.Synchronization:
				{
					//09302012, this piece not needed
					//sr[AceType.Synchronization, SynchronizationRight.TwoWay].AccessAllowed =
					//    (summaryMask & (int)SynchronizationRight.Upload) == (int)SynchronizationRight.Upload &&
					//    (summaryMask & (int)SynchronizationRight.Download) == (int)SynchronizationRight.Download;

					//this is just to block having "OneWay" as the only specified right
					sr[AceType.Synchronization, SynchronizationRight.OneWay].AccessAllowed =
						(summaryMask & (int)SynchronizationRight.Upload) == (int)SynchronizationRight.Upload ||
						(summaryMask & (int)SynchronizationRight.Download) == (int)SynchronizationRight.Download;
					break;
				}
			}
		}
	}



	internal class AceSorter : IComparer
	{
		private int multiplier = 1;

		public AceSorter(bool Ascending)
		{
			if( !Ascending )
			{
				multiplier = -1;
			}
		}


		public int Compare(object x, object y)
		{
			//compare first item to second item, mutliply by -1 to reverse result
			//bool:	-1:this=false && obj=true, 0:this=obj, 1:this=true && obj=false
			//int:	-1:a<b, 0:a=b, 1:a>b

			IAccessControlEntry xa = (IAccessControlEntry)x;
			IAccessControlEntry ya = (IAccessControlEntry)y;

			int xv = Convert.ToInt32( xa.Allowed ) * 100000 - ((int)xa.Right);
			int yv = Convert.ToInt32( ya.Allowed ) * 100000 - ((int)ya.Right);

			string xx = string.Format( "{0}{1}", xa.AceType, Math.Abs( xv ).ToString().PadLeft( 8, '0' ) );
			string yy = string.Format( "{0}{1}", ya.AceType, Math.Abs( yv ).ToString().PadLeft( 8, '0' ) );

			return xx.CompareTo( yy ) * multiplier;
		}
	}



	/// <summary>
	/// Provides a collection of IAccessControlEntries.
	/// </summary>
	/// <remarks>
	/// This class is designed to mimic an NTFS SACL.  
	/// </remarks>
	public class SystemAcl : AccessControlListBase
	{
		private AuditType _auditTypeFilter =
			AuditType.SuccessAudit | AuditType.FailureAudit | AuditType.Information | AuditType.Warning | AuditType.Error;

		public SystemAcl() { }


		/// <summary>
		/// Adds or updates all Aces in the destination Sacl, as appropriate.
		/// </summary>
		/// <param name="Sacl">The destination Sacl.</param>
		public void CopyTo(SystemAcl sacl)
		{
			//Sacl.AuditTypeFilter = this.AuditTypeFilter;

			sacl.AuditTypeFilter = this.InheritAuditTypeFilter( this.AuditTypeFilter, sacl.AuditTypeFilter );
			base.CopyTo( sacl );
		}

		/// <summary>
		/// Adds or updates Aces of the specified type in the destination Sacl, as appropriate.
		/// </summary>
		/// <param name="Sacl">The destination Sacl.</param>
		/// <param name="CopyParameters">Indicates whether to copy Ace Parameters.</param>
		public void CopyTo(SystemAcl sacl, AceType aceType)
		{
			//Sacl.AuditTypeFilter = this.AuditTypeFilter;

			sacl.AuditTypeFilter = InheritAuditTypeFilter( this.AuditTypeFilter, sacl.AuditTypeFilter );
			base.CopyTo( sacl, aceType );
		}

		private AuditType InheritAuditTypeFilter(AuditType source, AuditType dest)
		{
			IEnumerator atValues = Enum.GetValues( typeof( AuditType ) ).GetEnumerator();
			while( atValues.MoveNext() )
			{
				if( (source & (AuditType)atValues.Current) == (AuditType)atValues.Current &&
					(dest & (AuditType)atValues.Current) != (AuditType)atValues.Current )
				{
					dest |= (AuditType)atValues.Current;
				}
			}

			return dest;
		}

		public void HasAudit(AceType aceType, SecurityResultCollection securityResults)
		{
			if( !securityResults.ContainsAceType( aceType ) )
			{
				securityResults.AddAceType( aceType );
			}

			/*
			 * Logic: Look in the Sacl for aces of the given AceType, create a new mask of the combined rights.
			 * If Allowed: bitwise-OR the value into the allowedMask.
			 * If Denied: bitwise-OR the value into the deniedMask.
			 * The result mask contains only the allowed rights.
			 */
			int allowedMask = 0;
			int deniedMask = 0;
			int aceRight = 0;
			foreach( IAccessControlEntryAudit ace in this.Values )
			{
				if( ace.AceType == aceType )
				{
					aceRight = (int)ace.Right;

					if( ace.Allowed )
					{
						allowedMask |= aceRight;
					}
					if( ace.Denied )
					{
						deniedMask |= aceRight;
					}
				}
			}

			/*
			 * For each right of the given acetype, perform a bitwise-AND to see if the right is specified in the mask.
			 */
			object[] rights = AceTypeRights.GetRights( aceType );
			for( int n = 0; n < rights.Length; n++ )
			{
				securityResults[aceType, rights[n]].AuditSuccess = (allowedMask & (int)rights[n]) == (int)rights[n];
				securityResults[aceType, rights[n]].AuditFailure = (deniedMask & (int)rights[n]) == (int)rights[n];
			}
		}

		[Description( "Get or set what type audit messages will be generated." )]
		public AuditType AuditTypeFilter
		{
			get
			{
				return _auditTypeFilter;
			}
			set
			{
				_auditTypeFilter = value;
			}
		}
	}
}


#region bunk
//public delegate void SDCollectionEventHandler(object key, ISecurityDescriptor value);

//these were in Dacl:
//private AceSorterRight		_RightSorter	= new AceSorterRight( true );
//private AceSorterAllowed	_AllowedSorter	= new AceSorterAllowed( false );
#endregion