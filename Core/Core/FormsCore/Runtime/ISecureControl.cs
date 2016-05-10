using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using Suplex.Data;
using Suplex.Security;
using Suplex.Security.Standard;
using System.Security.Principal;
using System.Text;

using Suplex.General;
using ss = Suplex.Security.Standard;
using api = Suplex.Forms.ObjectModel.Api;

using WinForms = System.Windows.Forms;
using WebForms = System.Web.UI;


namespace Suplex.Forms
{
	/// <summary>
	/// Summary description for ISecureControl.
	/// </summary>
	public interface ISecureControl
	{
		string UniqueName { get; set; }

		Suplex.Data.DataAccessLayer DataAccessLayer { get; set; }

		SecurityAccessor Security { get; }

		void ApplySecurity();
		string GetSecurityState();
	}

	/// <summary>
	/// Marker interface used to indicate that the implementing control's
	/// security should be copied to it's parent.
	/// </summary>
	public interface ISecurityExtender : ISecureControl
	{
	}

	/// <summary>
	/// Marker interface used to indicate that the implementing control's
	/// security should be copied to it's descendents.
	/// </summary>
	public interface ISecureContainer : ISecureControl
	{
		IEnumerable GetChildren();
	}


	public class SecurityAccessor
	{
		ISecureControl _owner = null;

		SecurityBuilder _securityBuilder = new SecurityBuilder();

		private User _userContext = User.Unknown;
		private DefaultSecurityState _defaultSecurityState = DefaultSecurityState.Locked;
		private SecurityDescriptor _securityDescriptor = new SecurityDescriptor();
		//private SecureControlCollection _secureControls = new SecureControlCollection();
		private AuditWriter _auditWriter = null;
		private AuditEventHandler _auditEventHandler = null;
		private RightRoleRuleCollection _rightRoleRules = null;
		private RightRoleCollection _rightRoles = new RightRoleCollection();
		//private AuditType _auditType = AuditType.ControlDetail;
		private AceType[] _nativeAceTypes = null;	//new AceType[1] { AceType.UI };

		private SecurityResultCollection _sr = null;

		//TODO: visit this
		private bool _isWindowsControl = true;

		private EnumUtil _enumUtil = new EnumUtil();


		#region Events
		public event AuditEventHandler Audited;

		/// <summary>
		/// Writes audit, Raises the Audited event.
		/// </summary>
		protected virtual void OnAudited(object sender, AuditEventArgs e)
		{
			if( (this.Descriptor.Sacl.AuditTypeFilter & e.AuditType) == e.AuditType )
			{
				if( this.AuditWriter != null )
				{
					this.AuditWriter.Write( e );
				}

				if( Audited != null )
				{
					this.Audited( sender, e );
				}
			}
		}
		#endregion


		#region ctors
		internal SecurityAccessor() { }

		public SecurityAccessor(ISecureControl owner)
		{
			_owner = owner;
			_isWindowsControl = _owner is WinForms.Control;
			_sr = this.Descriptor.SecurityResults;
			_rightRoleRules = new RightRoleRuleCollection( _owner );
		}

		public SecurityAccessor(ISecureControl owner, AceType nativeAceType)
		{
			this.Initialize( owner, new AceType[1] { nativeAceType }, DefaultSecurityState.Locked );
		}
		public SecurityAccessor(ISecureControl owner, AceType nativeAceType, DefaultSecurityState defaultSecurityState)
		{
			this.Initialize( owner, new AceType[1] { nativeAceType }, defaultSecurityState );
		}
		public SecurityAccessor(ISecureControl owner, AceType[] nativeAceTypes)
		{
			this.Initialize( owner, nativeAceTypes, DefaultSecurityState.Locked );
		}
		public SecurityAccessor(ISecureControl owner, AceType[] nativeAceTypes, DefaultSecurityState defaultSecurityState)
		{
			this.Initialize( owner, nativeAceTypes, defaultSecurityState );
		}

		private void Initialize(ISecureControl owner, AceType[] nativeAceTypes, DefaultSecurityState defaultSecurityState)
		{
			_owner = owner;
			_isWindowsControl = _owner is WinForms.Control;
			_nativeAceTypes = nativeAceTypes;
			_defaultSecurityState = defaultSecurityState;
			_auditEventHandler = new AuditEventHandler( this.OnAudited );
			_rightRoleRules = new RightRoleRuleCollection( _owner );

			_sr = this.Descriptor.SecurityResults;
			foreach( AceType acetype in _nativeAceTypes )
			{
				_sr.AddAceType( acetype, defaultSecurityState == DefaultSecurityState.Unlocked, false, false );
			}
		}
		#endregion


		#region properties
		[Browsable( false ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual User UserContext
		{
			get { return _userContext; }
			set { _userContext = value; }
		}

		[DefaultValue( typeof( DefaultSecurityState ), "Locked" ), DesignerSerializationVisibility( DesignerSerializationVisibility.Visible ), NotifyParentProperty( true )]
		[Description( "The initial security state for the control. Default is Unlocked for container controls, Locker for all others." )]
		public virtual DefaultSecurityState DefaultState
		{
			get { return _defaultSecurityState; }
			set { _defaultSecurityState = value; }
		}

		[Browsable( false ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual SecurityDescriptor Descriptor
		{
			get { return _securityDescriptor; }
			set { _securityDescriptor = value; }
		}

		//[Browsable( false )]
		//public virtual SecureControlCollection SecureControls
		//{
		//    get { return _secureControls; }
		//    set { _secureControls = value; }
		//}

		[Browsable( false ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual AuditWriter AuditWriter
		{
			get { return _auditWriter; }
			set { _auditWriter = value; }
		}

		[Browsable( false ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual AuditEventHandler AuditEventHandler
		{
			get { return _auditEventHandler; }
			set { _auditEventHandler = value; }
		}

		[Browsable( false ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual RightRoleRuleCollection RightRoleRules
		{
			get { return _rightRoleRules; }
			set { _rightRoleRules = value; }
		}

		[Browsable( false ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual RightRoleCollection RightRoles
		{
			get { return _rightRoles; }
			set { _rightRoles = value; }
		}

		[Browsable( false ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual AceType[] NativeAceTypes
		{
			get { return _nativeAceTypes; }
		}

		[Browsable( false ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		internal virtual bool IsWindowsControl
		{
			get { return _isWindowsControl; }
		}

		[Browsable( false ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected virtual ISecureControl Owner
		{
			get { return _owner; }
		}
		#endregion


		#region methods
		public virtual void EnsureDefaultState()
		{
			if( this.Descriptor.Dacl.Count == 0 && this.Descriptor.Sacl.Count == 0 )
			{
				foreach( AceType at in _nativeAceTypes )
				{
					_sr.InitAceType( at, _defaultSecurityState == DefaultSecurityState.Unlocked, false, false );
				}
			}
		}

		public virtual DataSet Load(string filePath, SecurityLoadParameters slp)
		{
			if( slp == null || !slp.Validate() )
			{
				throw new ArgumentNullException( "slp" );
			}

			DataSet securityCache = _securityBuilder.CreateSecurityCache( filePath, slp.User );

			this.Load( securityCache, slp );

			return securityCache;
		}

		public virtual DataSet Load(System.IO.TextReader reader, SecurityLoadParameters slp)
		{
			if( slp == null || !slp.Validate() )
			{
				throw new ArgumentNullException( "slp" );
			}

			DataSet securityCache = _securityBuilder.CreateSecurityCache( reader, slp.User );

			this.Load( securityCache, slp );

			return securityCache;
		}

		public virtual DataSet Load(api.SuplexStore splxStore, SecurityLoadParameters slp)
		{
			if( slp == null || !slp.Validate() )
			{
				throw new ArgumentNullException( "slp" );
			}

			DataSet securityCache = _securityBuilder.CreateSecurityCache( splxStore, slp.User );

			this.Load( securityCache, slp );

			return securityCache;
		}

		public virtual DataSet Load(Suplex.Data.DataAccessor plaformDa, SecurityLoadParameters slp)
		{
			if( slp == null || !slp.Validate() )
			{
				throw new ArgumentNullException( "slp" );
			}

			//this is legacy code from an old override, but i think it's moot given this series of overrides
			if( plaformDa == null )
			{
				plaformDa = _owner.DataAccessLayer.Platform;
			}

			DataSet securityCache =
				_securityBuilder.CreateSecurityCache( plaformDa, _owner.UniqueName, slp.User, slp.ExternalGroupInfo );

			this.Load( securityCache, slp );

			return securityCache;
		}

		public virtual DataSet Load(api.SuplexApiClient apiClient, SecurityLoadParameters slp)
		{
			//api.SuplexStore splxStore = apiClient.GetSecurity( _owner.UniqueName );

			//DataSet securityCache = _securityBuilder.CreateSecurityCache( splxStore, slp.User );

			DataSet securityCache = apiClient.GetSecurity( _owner.UniqueName, slp.User, slp.ExternalGroupInfo );

			this.Load( securityCache, slp );

			return securityCache;
		}

		public virtual void Load(DataSet securityCache, SecurityLoadParameters slp)
		{
			if( slp == null || !slp.Validate() )
			{
				throw new ArgumentNullException( "slp" );
			}

			_securityBuilder.LoadRulesFromCache( _owner, slp, null, securityCache );

			if( slp.ApplyDefaultSecurity )
			{
				this.Apply( AceType.Native );

				_securityBuilder.ResolveRightRolesPost();
			}
		}

		public virtual void ResolveRightRoles(bool preApplySecurity)
		{
			if( preApplySecurity )
			{
				//ICollection controls = _owner is WinForms.Control ?
				//    (ICollection)( (WinForms.Control)_owner ).Controls : (ICollection)( (WebForms.Control)_owner ).Controls;
				_securityBuilder.ResolveRightRolesPre( _owner, _enumUtil.GetChildren( _owner ) );
			}
			else
			{
				_securityBuilder.ResolveRightRolesPost();
			}
		}

		public virtual void Apply(AceType aceType)
		{
			if( aceType == AceType.None )
			{
				return;
			}

			_securityBuilder.ApplySecurityRecursive( _owner, aceType );
		}


		/*
		 * Deprecated
		 * 
		internal void ApplySecurityInternal(AceType aceType, ArrayList daclParameters, ArrayList saclParameters)
		{
			//this is just in case anyone ever stuffs Native or None into NativeAceTypes array
			if( aceType == AceType.Native || aceType == AceType.None )
			{
				return;
			}

			this.SecurityDescriptor.EvalSecurity( aceType, daclParameters, saclParameters );
			this.AuditAccess( aceType );
			if( _owner is ISecureContainer )
			{
				_securityBuilder.PropagateSecurity( _owner );
			}
		}
		*/

		public virtual void AuditAccess()
		{
			foreach( AceType acetype in _nativeAceTypes )
			{
				this.AuditAccess( acetype );
			}
		}

		public virtual void AuditAccess(AceType aceType)
		{
			//this is just in case anyone ever stuffs Native or None into NativeAceTypes array
			if( aceType == AceType.Native || aceType == AceType.None )
			{
				return;
			}

			//_auditType = AuditType.ControlDetail;

			object[] rights = AceTypeRights.GetRights( aceType );
			foreach( object right in rights )
			{
				this.AuditAccess( aceType, right );
			}
		}

		private void AuditAccess(AceType aceType, object right)
		{
			string[] desc = { "Object Type: " + _owner.GetType().ToString(), "" };
			AuditType auditType = AuditType.SuccessAudit;
			bool raiseEvent = false;

			if( _owner.Security.Descriptor.SecurityResults[aceType, right].AccessAllowed &&
				_owner.Security.Descriptor.SecurityResults[aceType, right].AuditSuccess )
			{
				desc[1] = right.ToString() + " access granted.";
				raiseEvent = true;
			}

			if( !_owner.Security.Descriptor.SecurityResults[aceType, right].AccessAllowed &&
				_owner.Security.Descriptor.SecurityResults[aceType, right].AuditFailure )
			{
				desc[1] = right.ToString() + " access denied.";
				raiseEvent = true;
				auditType = AuditType.FailureAudit;
			}


			if( raiseEvent )
			{
				this.OnAudited( _owner, new AuditEventArgs( _owner.Security.UserContext, auditType, _owner.UniqueName, AuditCategory.Access, desc ) );
			}
		}

		public void AuditAction(AuditType auditType, string source, string description, bool appendAccessDesc)
		{
			if( string.IsNullOrEmpty( source ) )
			{
				source = _owner.UniqueName;
			}

			string[] d = null;
			if( appendAccessDesc )
			{
				d = new string[] { description, GetAccessDescription() };
			}
			else
			{
				d = new string[] { description };
			}

			this.OnAudited( _owner, new AuditEventArgs( _owner.Security.UserContext, auditType, source, AuditCategory.Action, d
				) );
		}

		private string GetAccessDescription()
		{
			StringBuilder desc = new StringBuilder();
			foreach( AceType aceType in _nativeAceTypes )
			{
				object[] rights = AceTypeRights.GetRights( aceType );
				foreach( object right in rights )
				{
					desc.AppendFormat( "{0}: {1}, ", right, _owner.Security.Descriptor.SecurityResults[aceType, right].AccessAllowed ? "Allowed" : "Denied" );
				}
			}
			return desc.ToString().TrimEnd( ',', ' ' );
		}

		public virtual void Clear(bool recurseChildControls)
		{
			if( recurseChildControls )
			{
				this.ClearSecurityRecursive( _owner );
			}
			else
			{
				this.Descriptor.Clear();
			}
		}

		private void ClearSecurityRecursive(object control)
		{
			if( control is ISecureControl )
			{
				((ISecureControl)control).Security.Descriptor.Clear();
				((ISecureControl)control).Security.RightRoles.Clear();
				((ISecureControl)control).Security.RightRoleRules.Clear();
			}

			//IEnumerator controls = _isWindowsControl ?
			//    ((WinForms.Control)control).Controls.GetEnumerator() :
			//    ((WebForms.Control)control).Controls.GetEnumerator();
			EnumUtil enumUtil = new EnumUtil();
			IEnumerator controls = enumUtil.GetChildren( control ).GetEnumerator();
			while( controls.MoveNext() )
			{
				this.ClearSecurityRecursive( controls.Current );
			}

		}
		#endregion

		public void EvalRowLevelSecurity(RowLevelSecurityHelper rlsHelper, AceType aceType, IList rightsMap, bool allowOwnerOverride)
		{
			rlsHelper.Eval();

			foreach( object right in rightsMap )
			{
				if( (rlsHelper.Option & RowLevelSecurityHelper.EvalOption.Mask) == RowLevelSecurityHelper.EvalOption.Mask )
				{
					this.Descriptor.SecurityResults[aceType, right].AccessAllowed &= rlsHelper.HasMaskMatch;
				}

				if( allowOwnerOverride &&
					(rlsHelper.Option & RowLevelSecurityHelper.EvalOption.Owner) == RowLevelSecurityHelper.EvalOption.Owner )
				{
					this.Descriptor.SecurityResults[aceType, right].AccessAllowed |= rlsHelper.IsRowOwner;
				}
			}
		}
	}


	public class RowLevelSecurityHelper
	{
		[Flags()]
		public enum EvalOption
		{
			None = 0,
			Owner = 1,
			Mask = 2,
			Both = 3
		}

		public RowLevelSecurityHelper()
		{
			this.Option = EvalOption.Both;
		}

		public Guid RowOwnerId { get; set; }
		public Guid SecurityPrincipalId { get; set; }
		public byte[] RowRlsMask { get; set; }
		public byte[] SecurityPrincipalRlsMask { get; set; }
		public EvalOption Option { get; set; }

		public bool IsRowOwner { get; internal set; }
		public bool HasMaskMatch { get; internal set; }

		public void Eval()
		{
			if( (this.Option & EvalOption.Owner) == EvalOption.Owner )
			{
				this.IsRowOwner = this.RowOwnerId.Equals( this.SecurityPrincipalId );
			}

			if( (this.Option & EvalOption.Mask) == EvalOption.Mask )
			{
				this.HasMaskMatch = true;

				if( this.RowRlsMask != null && this.SecurityPrincipalRlsMask != null )
				{
					InternalBitArray rowRlsMask = new InternalBitArray( this.RowRlsMask );
					InternalBitArray securityPrincipalRlsMask = new InternalBitArray( this.SecurityPrincipalRlsMask );

					if( rowRlsMask.HasValue )
					{
						this.HasMaskMatch = rowRlsMask.ContainsOne( securityPrincipalRlsMask );
					}
				}
			}
		}
	}

	class InternalBitArray
	{
		// Fields
		private int _version;
		private int[] m_array;
		private int m_length;

		// Methods
		private InternalBitArray()
		{
		}

		public InternalBitArray(byte[] bytes)
		{
			if( bytes == null )
			{
				throw new ArgumentNullException( "bytes" );
			}
			this.m_array = new int[(bytes.Length + 3) / 4];
			this.m_length = bytes.Length * 8;
			int index = 0;
			int num2 = 0;
			while( (bytes.Length - num2) >= 4 )
			{
				this.m_array[index++] = (((bytes[num2] & 0xff) | ((bytes[num2 + 1] & 0xff) << 8)) | ((bytes[num2 + 2] & 0xff) << 0x10)) | ((bytes[num2 + 3] & 0xff) << 0x18);
				num2 += 4;
			}
			switch( (bytes.Length - num2) )
			{
				case 1:
				goto Label_00DB;

				case 2:
				break;

				case 3:
				this.m_array[index] = (bytes[num2 + 2] & 0xff) << 0x10;
				break;

				default:
				goto Label_00FC;
			}
			this.m_array[index] |= (bytes[num2 + 1] & 0xff) << 8;
		Label_00DB:
			this.m_array[index] |= bytes[num2] & 0xff;
		Label_00FC:
			this._version = 0;
		}

		public bool ContainsOne(InternalBitArray value)
		{
			if( value == null )
			{
				throw new ArgumentNullException( "value" );
			}
			if( this.m_length != value.m_length )
			{
				throw new ArgumentException( "Arg_ArrayLengthsDiffer" );
			}
			bool haveMatch = false;
			int num = (this.m_length + 0x1f) / 0x20;
			for( int i = 0; i < num; i++ )
			{
				this.m_array[i] &= value.m_array[i];
				if( this.m_array[i] > 0 )
				{
					haveMatch = true;
					break;
				}
			}
			return haveMatch;
		}

		public bool HasValue
		{
			get
			{
				bool hasValue = false;
				int num = (this.m_length + 0x1f) / 0x20;
				for( int i = 0; i < num; i++ )
				{
					hasValue = this.m_array[i] > 0;
					if( hasValue ) { break; }
				}
				return hasValue;
			}
		}
	}



	/*
		/// <summary>
		/// Provides a collection of ISecureControls.
		/// </summary>
		public class SecureControlCollection : NameObjectCollectionBase
		{
			private DictionaryEntry _de = new DictionaryEntry();


			public SecureControlCollection(){}


			// Gets a key-and-value pair (DictionaryEntry) using an index.
			public DictionaryEntry this[ int index ]
			{
				get
				{
					_de.Key = this.BaseGetKey(index);
					_de.Value = this.BaseGet(index);
					return( _de );
				}
			}


			// Gets or sets the value associated with the specified key.
			public ISecureControl this[ string uniqueName ]
			{
				get
				{
					return (ISecureControl)this.BaseGet( uniqueName );
				}
				set
				{
					this.BaseSet( uniqueName, value );
				}
			}


			// Gets a String array that contains all the keys in the collection.
			public string[] UniqueNameKeys
			{
				get
				{
					return this.BaseGetAllKeys();
				}
			}


			// Gets an Object array that contains all the values in the collection.
			public ISecureControl[] Values
			{
				get
				{
					return (ISecureControl[])this.BaseGetAllValues
						( Type.GetType( "Suplex.Forms.ISecureControl" ) );
				}
			}


			// Gets a String array that contains all the values in the collection.
			public string[] AllStringValues
			{
				get
				{
					return (string[]) this.BaseGetAllValues( Type.GetType( "System.String" ) );
				}
			}


			// Gets a String array that contains all the values in the collection.
			public object[] AllObjectValues
			{
				get
				{
					return this.BaseGetAllValues();
				}
			}


			new public bool IsReadOnly
			{
				get
				{
					return base.IsReadOnly;
				}
			}


			// Gets a value indicating if the collection contains keys that are not null.
			public Boolean HasKeys
			{
				get
				{
					return( this.BaseHasKeys() );
				}
			}


			/// <summary>
			/// Adds an entry to the collection; key must be unique.
			/// </summary>
			public void Add( string uniqueName, ISecureControl control )
			{
				this.BaseAdd( uniqueName, control );
			}


			/// <summary>
			/// Adds an entry to the collection keyed by UniqueName.  If the UniqueName key already exists, the value is updated.
			/// </summary>
			public void Add( ISecureControl control )
			{
				if( control == null )
				{
					throw new ArgumentNullException( "ISecureControl value" );
				}

				this.BaseSet( control.UniqueName, control );
			}


			// Clears all the elements in the collection.
			public void Clear()
			{
				this.BaseClear();
			}


			public bool Contains(string uniqueName)
			{
				return this.BaseGet( uniqueName ) != null;
			}


			public bool ContainsKey(string uniqueName)
			{
				return this.BaseGet( uniqueName ) != null;
			}


			// Removes an entry with the specified key from the collection.
			public void Remove( string uniqueName )
			{
				this.BaseRemove( uniqueName );
			}


			// Removes an entry in the specified index from the collection.
			public void RemoveAt( int index )
			{
				this.BaseRemoveAt( index );
			}

		}
	 * 
	 * */
}//namespace



/*
 * 
 * 
 	/// <summary>
	/// Provides a collection of SecureControls. (Controls that implement ISecureControl.)
	/// </summary>
	internal class SecureControlCollection_0 : SortedList
	{

		public SecureControlCollection_0(){}

		
		// Indexer implementation.
		public new ISecureControl this[object key]
		{
			get
			{
				return (ISecureControl)base[key];
			}
			set
			{
				base[key] = (ISecureControl)value;	//will add value if not in list
			}
		}


		public void Add(object key, ISecureControl secureControl)
		{
			base.Add(key, secureControl);
		}


		public bool ContainsValue(ISecureControl secureControl)
		{
			return base.ContainsValue( secureControl );
		}


		public int IndexOfValue(ISecureControl secureControl)
		{
			return base.IndexOfValue( secureControl );
		}


		public void SetByIndex(int index, ISecureControl secureControl)
		{
			base.SetByIndex( index, secureControl );
		}
	}
 * 
 * 
 */