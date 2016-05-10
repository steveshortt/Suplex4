using System;
using System.Collections;
using System.ComponentModel;
using Suplex.Data;


namespace Suplex.Security
{
	public class SecurityDescriptor
	{
		private DiscretionaryAcl _dacl = new DiscretionaryAcl();
		private SystemAcl _sacl = new SystemAcl();
		private SecurityResultCollection _securityResults = new SecurityResultCollection();


		public SecurityDescriptor() { }


		public virtual DiscretionaryAcl Dacl
		{
			get { return _dacl; }
			set { _dacl = value; }
		}

		public virtual SystemAcl Sacl
		{
			get { return _sacl; }
			set { _sacl = value; }
		}

		public virtual SecurityResultCollection SecurityResults
		{
			get { return _securityResults; }
			set { _securityResults = value; }
		}

		public void Clear()
		{
			_dacl.Clear();
			_sacl.Clear();
		}

		/// <summary>
		/// Copies this SecurityDescriptor to the destination SecurityDescriptor, if the 
		/// destination SecurityDescriptor is marked to InheritAces.  Additionally, each
		/// Ace in the Dacl and Sacl must be marked Inheritable.
		/// </summary>
		/// <param name="securityDescriptor">Destination SecurityDescriptor</param>
		public void CopyTo(SecurityDescriptor securityDescriptor)
		{
			if( securityDescriptor.Dacl.InheritAces )
			{
				this.Dacl.CopyTo( securityDescriptor.Dacl );
			}

			if( securityDescriptor.Sacl.InheritAces )
			{
				this.Sacl.CopyTo( securityDescriptor.Sacl );
			}
		}

		/// <summary>
		/// Copies this SecurityDescriptor to the destination SecurityDescriptor, if the 
		/// destination SecurityDescriptor is marked to InheritAces.  Additionally, each
		/// Ace in the Dacl and Sacl must be marked Inheritable.
		/// </summary>
		/// <param name="securityDescriptor">Destination SecurityDescriptor</param>
		public void CopyTo(SecurityDescriptor securityDescriptor, string sourceName)
		{
			if( securityDescriptor.Dacl.InheritAces )
			{
				this.Dacl.CopyTo( securityDescriptor.Dacl, sourceName );
			}

			if( securityDescriptor.Sacl.InheritAces )
			{
				this.Sacl.CopyTo( securityDescriptor.Sacl, sourceName );
			}
		}

		/// <summary>
		/// Copies this SecurityDescriptor to the destination SecurityDescriptor, if the 
		/// destination SecurityDescriptor is marked to InheritAces.  Additionally, each
		/// Ace in the Dacl and Sacl must be marked Inheritable.
		/// </summary>
		/// <param name="securityDescriptor">Destination SecurityDescriptor</param>
		/// <param name="AceType">The type of Ace to be copied.</param>
		public void CopyTo(SecurityDescriptor securityDescriptor, AceType aceType)
		{
			if( securityDescriptor.Dacl.InheritAces )
			{
				this.Dacl.CopyTo( securityDescriptor.Dacl, aceType );
			}

			if( securityDescriptor.Sacl.InheritAces )
			{
				this.Sacl.CopyTo( securityDescriptor.Sacl, aceType );
			}
		}

		/// <summary>
		/// Copies this SecurityDescriptor to the destination SecurityDescriptor, if the 
		/// destination SecurityDescriptor is marked to InheritAces.  Additionally, each
		/// Ace in the Dacl and Sacl must be marked Inheritable.
		/// </summary>
		/// <param name="securityDescriptor">Destination SecurityDescriptor</param>
		/// <param name="AceType">The type of Ace to be copied.</param>
		public void CopyTo(SecurityDescriptor securityDescriptor, AceType aceType, string sourceName)
		{
			if( securityDescriptor.Dacl.InheritAces )
			{
				this.Dacl.CopyTo( securityDescriptor.Dacl, aceType, sourceName );
			}

			if( securityDescriptor.Sacl.InheritAces )
			{
				this.Sacl.CopyTo( securityDescriptor.Sacl, aceType, sourceName );
			}
		}

		public void EvalSecurity(AceType aceType)
		{
			this.Dacl.HasAccess( aceType, this.SecurityResults );
			this.Sacl.HasAudit( aceType, this.SecurityResults );
			//this.SecurityResults = SecurityResults;		//[AceType]
		}


		public override string ToString()
		{
			return string.Format( "Dacl:{0}, Sacl:{1}, R:{2}", _dacl.Count.ToString(), _sacl.Count.ToString(), _securityResults.ToString() );
		}
	}
}