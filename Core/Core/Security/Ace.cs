using System;
using System.Collections;

namespace Suplex.Security
{

	/// <summary>
	/// Summary description for IAccessControlEntry.
	/// </summary>
	public interface IAccessControlEntry : ICloneable
	{
		AceType AceType { get; }
		object Right { get; set; }
		bool Allowed { get; set; }
		bool Inherit { get; set; }
		string InheritedFrom { get; set; }
	}


	public interface IAccessControlEntryAudit : IAccessControlEntry
	{
		bool Denied { get; set; }
	}



	public abstract class AccessControlEntryBase : IAccessControlEntry
	{
		private object _right = null;
		private bool _allowed;
		private bool _inherit = true;
		private string _inheritedFrom = string.Empty;


		public AccessControlEntryBase() { }


		public virtual AceType AceType
		{
			get { return AceType.None; }
		}

		public object Right
		{
			get { return _right; }
			set { _right = value; }
		}

		public bool Allowed
		{
			get { return _allowed; }
			set { _allowed = value; }
		}

		/// <summary>
		/// sum: Specifies whether this Ace should be inherited by child Acls.
		/// Set Inherit to false to block inheritance of this Ace.
		/// </summary>
		/// <remarks>
		/// rem: Specifies whether this Ace should be inherited by child Acls.
		/// Set Inherit to false to block inheritance of this Ace.
		/// </remarks>
		public bool Inherit
		{
			get { return _inherit; }
			set { _inherit = value; }
		}

		public string InheritedFrom
		{
			get { return _inheritedFrom; }
			set { _inheritedFrom = value; }
		}

		public override string ToString()
		{
			string allowed = string.Format( "Allowed:{0}", _allowed );
			if( this is IAccessControlEntryAudit )
			{
				allowed = string.Format( "Audit:Success-{0}/Failure-{1}", _allowed, ((IAccessControlEntryAudit)this).Denied );
			}

			return string.Format( "{0}/{1}: {2}, Inherit:{3}, InheritedFrom:{4}",
				AceType.ToString(), _right.ToString(), allowed, _inherit,
				string.IsNullOrEmpty( _inheritedFrom ) ? "(null)" : _inheritedFrom );
		}


		abstract public object Clone();
	}


	public class UIAce : AccessControlEntryBase
	{
		public UIAce()
		{
			this.Right = UIRight.FullControl;
			this.Allowed = false;
		}


		public UIAce(UIRight right, bool allowed)
		{
			this.Right = right;
			base.Allowed = allowed;
		}

		public override AceType AceType { get { return AceType.UI; } }

		public new UIRight Right
		{
			get { return (UIRight)base.Right; }
			set { base.Right = (UIRight)value; }
		}

		public override object Clone()
		{
			UIAce ace = new UIAce();
			AceCloner.Clone( this, ace );
			return ace;
		}
	}

	public class UIAuditAce : UIAce, IAccessControlEntryAudit
	{
		private bool _denied = false;


		public UIAuditAce() : base() { }

		public UIAuditAce(UIRight right, bool auditSuccess, bool auditFailure)
			: base( right, auditSuccess )
		{
			_denied = auditFailure;
		}

		public bool Denied
		{
			get { return _denied; }
			set { _denied = value; }
		}

		public override object Clone()
		{
			UIAuditAce ace = new UIAuditAce();
			AceCloner.Clone( this, ace );
			return ace;
		}
	}

	public class RecordAce : AccessControlEntryBase
	{
		public RecordAce()
		{
			this.Right = RecordRight.FullControl;
			this.Allowed = false;
		}

		public RecordAce(RecordRight right, bool allowed)
		{
			this.Right = right;
			base.Allowed = allowed;
		}

		public override AceType AceType { get { return AceType.Record; } }

		public new RecordRight Right
		{
			get { return (RecordRight)base.Right; }
			set { base.Right = (RecordRight)value; }
		}

		public override object Clone()
		{
			RecordAce ace = new RecordAce();
			AceCloner.Clone( this, ace );
			return ace;
		}
	}

	public class RecordAuditAce : RecordAce, IAccessControlEntryAudit
	{
		private bool _denied = false;

		public RecordAuditAce() : base() { }

		public RecordAuditAce(RecordRight right, bool auditSuccess, bool auditFailure)
			: base( right, auditSuccess )
		{
			_denied = auditFailure;
		}

		public bool Denied
		{
			get { return _denied; }
			set { _denied = value; }
		}

		public override object Clone()
		{
			RecordAuditAce ace = new RecordAuditAce();
			AceCloner.Clone( this, ace );
			return ace;
		}
	}

	public class FileSystemAce : AccessControlEntryBase
	{
		public FileSystemAce()
		{
			this.Right = FileSystemRight.FullControl;
			this.Allowed = false;
		}

		public FileSystemAce(FileSystemRight right, bool allowed)
		{
			this.Right = right;
			base.Allowed = allowed;
		}

		public override AceType AceType { get { return AceType.FileSystem; } }

		public new FileSystemRight Right
		{
			get { return (FileSystemRight)base.Right; }
			set { base.Right = (FileSystemRight)value; }
		}

		public override object Clone()
		{
			FileSystemAce ace = new FileSystemAce();
			AceCloner.Clone( this, ace );
			return ace;
		}
	}

	public class FileSystemAuditAce : FileSystemAce, IAccessControlEntryAudit
	{
		private bool _denied = false;

		public FileSystemAuditAce() : base() { }

		public FileSystemAuditAce(FileSystemRight right, bool auditSuccess, bool auditFailure)
			: base( right, auditSuccess )
		{
			_denied = auditFailure;
		}

		public bool Denied
		{
			get { return _denied; }
			set { _denied = value; }
		}

		public override object Clone()
		{
			FileSystemAuditAce ace = new FileSystemAuditAce();
			AceCloner.Clone( this, ace );
			return ace;
		}
	}

	public class SynchronizationAce : AccessControlEntryBase
	{
		public SynchronizationAce()
		{
			this.Right = SynchronizationRight.TwoWay;
			this.Allowed = false;
		}

		public SynchronizationAce(SynchronizationRight right, bool allowed)
		{
			this.Right = right;
			base.Allowed = allowed;
		}

		public override AceType AceType { get { return AceType.Synchronization; } }

		public new SynchronizationRight Right
		{
			get { return (SynchronizationRight)base.Right; }
			set { base.Right = (SynchronizationRight)value; }
		}

		public override object Clone()
		{
			SynchronizationAce ace = new SynchronizationAce();
			AceCloner.Clone( this, ace );
			return ace;
		}
	}

	public class SynchronizationAuditAce : SynchronizationAce, IAccessControlEntryAudit
	{
		private bool _denied = false;

		public SynchronizationAuditAce() : base() { }

		public SynchronizationAuditAce(SynchronizationRight right, bool auditSuccess, bool auditFailure)
			: base( right, auditSuccess )
		{
			_denied = auditFailure;
		}

		public bool Denied
		{
			get { return _denied; }
			set { _denied = value; }
		}

		public override object Clone()
		{
			SynchronizationAuditAce ace = new SynchronizationAuditAce();
			AceCloner.Clone( this, ace );
			return ace;
		}
	}



	internal class AceCloner
	{
		internal static void Clone(IAccessControlEntry source, IAccessControlEntry dest)
		{
			dest.Allowed = source.Allowed;
			dest.Inherit = source.Inherit;
			dest.InheritedFrom = source.InheritedFrom;
			dest.Right = source.Right;
		}


		internal static void Clone(IAccessControlEntryAudit source, IAccessControlEntryAudit dest)
		{
			dest.Allowed = source.Allowed;
			dest.Denied = source.Denied;
			dest.Inherit = source.Inherit;
			dest.InheritedFrom = source.InheritedFrom;
			dest.Right = source.Right;
		}
	}
}