using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Data;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Suplex.Data;
using sd = Suplex.Data;
using sf = Suplex.Forms;
using sg = Suplex.General;
using ss = Suplex.Security;
using st = Suplex.Security.Standard;

using settings = Suplex.Properties.Settings;



//the below classes represent SuplexApp-UI-friendly structrures:
//	swapped List<> collections for ObservableCollections<> and
//	overrode xml serialization for compliance
//located in Suplex.Core b/c i thought they might be useful for something else one day..

namespace Suplex.Forms.ObjectModel.Legacy
{
	//this is several kinds of a bad idea
	[Obsolete( "This was never a good idea.", false )]
	internal static class StoreConnection
	{
		public static SuplexStore SplxStore { get; set; }
		public static bool IsConnected { get; set; }
		public static DataAccessor da { get; set; }
	}

	//TODO: track changes to individual collection items for SuplexStore.IsDirty?
	public class SuplexStore : INotifyPropertyChanged
	{
		private bool _isDirty = false;
		private bool _hasFile = false;
		private FileInfo _fileStore = null;
		private bool _hasPublicPrivateKeyFile = false;
		private FileInfo _publicPrivateKeyFileStore = null;
		private bool _isConnected = false;
		private sd.ConnectionProperties _cp = null;
		private DataAccessor _da = null;

		#region ctors
		public SuplexStore()
		{
			this.UIElements = new UIElementCollection();
			this.Users = new UserCollection();
			this.Groups = new GroupCollection();
			this.GroupMembership = new GroupMembershipCollection( this );
			this.SecurityPrincipals = new ObservableCollection<SecurityPrincipalBase>();

			this.UIElements.CollectionChanged += new NotifyCollectionChangedEventHandler( this.UIElements_CollectionChanged );
			this.Users.CollectionChanged += new NotifyCollectionChangedEventHandler( this.UsersGroups_CollectionChanged );
			this.Groups.CollectionChanged += new NotifyCollectionChangedEventHandler( this.UsersGroups_CollectionChanged );
			this.GroupMembership.CollectionChanged += new NotifyCollectionChangedEventHandler( this.GroupMembership_CollectionChanged );
		}
		#endregion

		#region Public Props, SecurityPrincipals
		public UIElementCollection UIElements { get; set; }
		public UserCollection Users { get; set; }
		public GroupCollection Groups { get; set; }
		public GroupMembershipCollection GroupMembership { get; set; }

		[XmlIgnore()]
		public ObservableCollection<SecurityPrincipalBase> SecurityPrincipals { get; set; }
		#endregion

		#region Methods
		public void RecordSelectWholeStore(bool includeValidation, bool includeSecurity)
		{
			if( _da != null && (includeValidation || includeSecurity) )
			{
				_da.OpenConnection();

				DataSet uies = _da.GetDataSet( "splx_api_sel_uie_withchildren_composite",
						new sSortedList( "@SPLX_UI_ELEMENT_ID", Convert.DBNull, "@IncludeSecurity", includeSecurity ), false );
				_da.NameTablesFromCompositeSelect( ref uies );

				DataSet sec = null;
				if( includeSecurity )
				{
					sec = _da.GetDataSet( "splx_api_sel_groupmemb_nested_composite", null, false );
					_da.NameTablesFromCompositeSelect( ref sec );

					_da.GetDataSet( "splx_api_sel_users", null, sec, "Users", false );
					_da.GetDataSet( "splx_api_sel_groups", null, sec, "Groups", false );
				}

				_da.CloseConnection();


				if( includeSecurity )
				{
					this.Users.LoadFromDataTableWithSync( sec.Tables["Users"] );
					this.Groups.LoadFromDataTableWithSync( sec.Tables["Groups"] );
					this.GroupMembership.DatabaseRefresh( sec );
				}

				UIElement dummy = new UIElement();
				dummy.LoadFromDataSetRecursiveComposite( uies, null, this.UIElements, includeValidation );
			}
		}

		public void RecordUpsertWholeStore(bool includeUIElements, bool includeSecurity)
		{
			if( _da != null )
			{
				_da.OpenConnection();
				SqlTransaction tr = _da.Connection.BeginTransaction();

				try
				{
					if( includeSecurity )
					{
						foreach( SecurityPrincipalBase sp in this.SecurityPrincipals )
						{
							sp.RecordUpsertForImport( ref tr );
						}

						this.GroupMembership.RecordUpsertForImport( ref tr );
					}

					if( includeUIElements )
					{
						foreach( UIElement uie in this.UIElements )
						{
							uie.RecordUpsertForImport( ref tr, includeSecurity );
						}
					}

					tr.Commit();
				}
				catch( Exception ex )
				{
					tr.Rollback();
					throw ex;
				}
				finally
				{
					_da.CloseConnection();
				}
			}
		}

		[Obsolete( "Used recursive methods instead.", true )]
		private void RecordUpsertUieTree(INodeItem item)
		{
			((IDatabaseObject)item).RecordUpsert();

			if( item.ChildObjects != null )
			{
				foreach( CollectionContainer container in item.ChildObjects )
				{
					foreach( INodeItem child in container.Collection )
					{
						this.RecordUpsertUieTree( child );
					}
				}
			}
		}


		public void Clear()
		{
			this.IsConnected = false;
			this.HasFile = false;
			this.UIElements.Clear();
			this.Users.Clear();
			this.Groups.Clear();
			this.GroupMembership.Clear();
			this.SecurityPrincipals.Clear();
		}
		#endregion

		#region Store Connection
		[XmlIgnore()]
		public bool IsDirty
		{
			get { return _isDirty; }
			set
			{
				if( !this.IsConnected && _isDirty != value )
				{
					_isDirty = value;
					this.OnPropertyChanged( "IsDirty" );
				}
			}
		}

		#region Database
		public void Connect(string databaseServerName, string databaseName, string username, string password, bool loadDatabase)
		{
			if( string.IsNullOrEmpty( username ) || string.IsNullOrEmpty( password ) )
			{
				this.ConnectionProperties = new ConnectionProperties( databaseServerName, databaseName );
			}
			else
			{
				this.ConnectionProperties = new ConnectionProperties( databaseServerName, databaseName, username, password );
			}

			this.SuplexDataAccessor = new DataAccessor( this.ConnectionProperties.ConnectionString );

			StoreConnection.SplxStore = this;
			StoreConnection.IsConnected = true;
			StoreConnection.da = this.SuplexDataAccessor;


			if( loadDatabase )
			{
				this.RefreshDatabase();
			}
		}

		public void Disconnect()
		{
			StoreConnection.SplxStore = null;
			StoreConnection.IsConnected = false;
			StoreConnection.da = null;
			this.IsConnected = false;
		}

		[XmlIgnore()]
		public bool IsConnected
		{
			get { return _isConnected; }
			internal set
			{
				if( _isConnected != value )
				{
					if( value ) { this.IsDirty = false; }	//TODO:is this a good idead?
					_isConnected = value;
					this.OnPropertyChanged( "IsConnected" );
				}
			}
		}

		[XmlIgnore()]
		public sd.ConnectionProperties ConnectionProperties
		{
			get { return _cp; }
			internal set
			{
				if( _cp != value )
				{
					_cp = value;
					this.OnPropertyChanged( "ConnectionProperties" );
				}
			}
		}

		[XmlIgnore()]
		public DataAccessor SuplexDataAccessor
		{
			get { return _da; }
			internal set
			{
				if( _da != value )
				{
					_da = value;
					this.OnPropertyChanged( "SuplexDataAccessor" );
					this.IsConnected = true;
				}
			}
		}

		public void SetSuplexDataAccessor(DataAccessor da, bool resetStoreConnection)
		{
			this.SuplexDataAccessor = da;

			if( resetStoreConnection )
			{
				StoreConnection.SplxStore = this;
				StoreConnection.IsConnected = true;
				StoreConnection.da = this.SuplexDataAccessor;
			}
		}

		public void RefreshDatabase()
		{
			this.Clear();

			DataSet ds = new DataSet();
			_da.OpenConnection();
			_da.GetDataSet( "splx_api_sel_uielementbyparent",
				new sSortedList( "@UIE_PARENT_ID", Convert.DBNull ), ds, "UIElements", false );
			_da.GetDataSet( "splx_api_sel_users", null, ds, "Users", false );
			_da.GetDataSet( "splx_api_sel_groups", null, ds, "Groups", false );
			_da.CloseConnection();

			this.UIElements.LoadFromDataTable( ds.Tables["UIElements"] );	//, false
			this.Users.LoadFromDataTable( ds.Tables["Users"] );				//, false
			this.Groups.LoadFromDataTable( ds.Tables["Groups"] );			//, false

			this.IsConnected = true;
			this.IsDirty = false;
		}
		#endregion


		public static SuplexStore LoadSuplexFile(string path)
		{
			SuplexStore store = sg.XmlUtils.Deserialize<SuplexStore>( path );

			if( store.GroupMembership != null )
			{
				store.GroupMembership.OwnerStore = store;
				store.GroupMembership.Resolve();
			}

			store.IsDirty = false;

			return store;
		}

		#region file stuff
		[XmlIgnore()]
		public FileInfo File
		{
			get { return _fileStore; }
			internal set
			{
				if( _fileStore != value )
				{
					_fileStore = value;
					this.OnPropertyChanged( "File" );
					this.HasFile = value != null;
				}
			}
		}

		[XmlIgnore()]
		public FileInfo PublicPrivateKeyFile
		{
			get { return _publicPrivateKeyFileStore; }
			internal set
			{
				if( _publicPrivateKeyFileStore != value )
				{
					_publicPrivateKeyFileStore = value;
					this.OnPropertyChanged( "PublicPrivateKeyFile" );
					this.HasPublicPrivateKeyFile = value != null;
				}
			}
		}
		[XmlIgnore()]
		public string PublicPrivateKeyContainerName { get; set; }

		[XmlIgnore()]
		public bool HasFile
		{
			get { return _hasFile; }
			internal set
			{
				if( _hasFile != value )
				{
					_hasFile = value;
					this.OnPropertyChanged( "HasFile" );
				}
			}
		}

		[XmlIgnore()]
		public bool HasPublicPrivateKeyFile
		{
			get { return _hasPublicPrivateKeyFile; }
			internal set
			{
				if( _hasPublicPrivateKeyFile != value )
				{
					_hasPublicPrivateKeyFile = value;
					this.OnPropertyChanged( "HasPublicPrivateKeyFile" );
				}
			}
		}

		public void SetFile(string path)
		{
			if( path == null )
			{
				this.File = null;
			}
			else
			{
				this.File = new FileInfo( path );
			}
		}

		public void SetPublicPrivateKeyFile(string path)
		{
			if( path == null )
			{
				this.PublicPrivateKeyFile = null;
			}
			else
			{
				this.PublicPrivateKeyFile = new FileInfo( path );
			}
		}
		#endregion

		public SuplexStore LoadFile(string path)
		{
			this.SetFile( path );
			return this.LoadFile();
		}
		public SuplexStore LoadFile()
		{
			SuplexStore store = sg.XmlUtils.Deserialize<SuplexStore>( _fileStore.FullName );
			store.File = _fileStore;

			if( store.GroupMembership != null )
			{
				store.GroupMembership.OwnerStore = store;
				store.GroupMembership.Resolve();
			}

			store.IsDirty = false;

			return store;
		}
		public SuplexStore LoadFile(string suplexFilePath, string keyFilePath, string keyContainerName)
		{
			this.SetFile( suplexFilePath );
			RSACryptoServiceProvider rsaKey = sg.XmlUtils.LoadRsaKeys( keyContainerName, keyFilePath );
			SuplexStore store = sg.XmlUtils.Deserialize<SuplexStore>( _fileStore.FullName, rsaKey );
			store.File = _fileStore;

			if( store.GroupMembership != null )
			{
				store.GroupMembership.OwnerStore = store;
				store.GroupMembership.Resolve();
			}

			store.IsDirty = false;

			return store;
		}


		public SuplexStore LoadFromReader(TextReader reader)
		{
			//RSACryptoServiceProvider rsaKey = sg.XmlUtils.LoadRsaKeys( "XML_DSIG_RSA_KEY", "pub_only.txt" );
			//SuplexStore store = sg.XmlUtils.Deserialize<SuplexStore>( _fileStore.FullName, rsaKey );
			SuplexStore store = sg.XmlUtils.Deserialize<SuplexStore>( reader );
			store.File = null;

			if( store.GroupMembership != null )
			{
				store.GroupMembership.OwnerStore = store;
				store.GroupMembership.Resolve();
			}

			store.IsDirty = false;

			return store;
		}

		public void SaveFile(string path)
		{
			this.SetFile( path );
			this.SaveFile();
		}
		public void SaveFile()
		{
			if( this.HasPublicPrivateKeyFile )
			{
				RSACryptoServiceProvider rsaKey =
					sg.XmlUtils.LoadRsaKeys( this.PublicPrivateKeyContainerName, _publicPrivateKeyFileStore.FullName );
				sg.XmlUtils.Serialize<SuplexStore>( this, _fileStore.FullName, rsaKey );
			}
			else
			{
				sg.XmlUtils.Serialize<SuplexStore>( this, _fileStore.FullName );
			}
			this.IsDirty = false;
		}
		#endregion

		#region internal handlers
		void UsersGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
				{
					this.AddSecurityPrincipals( e.NewItems );
					break;
				}
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
				{
					this.AddSecurityPrincipals( e.NewItems );
					this.RemoveSecurityPrincipals( e.OldItems );
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					this.RemoveSecurityPrincipals( e.OldItems );
					break;
				}
			}

			this.IsDirty = true;
		}
		private void AddSecurityPrincipals(IList items)
		{
			if( items != null )
			{
				foreach( SecurityPrincipalBase sp in items )
				{
					this.SecurityPrincipals.Add( sp );
				}
			}
		}
		private void RemoveSecurityPrincipals(IList items)
		{
			if( items != null )
			{
				foreach( SecurityPrincipalBase sp in items )
				{
					this.SecurityPrincipals.Remove( sp );
				}
			}
		}

		void UIElements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.IsDirty = true;
		}

		void GroupMembership_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.IsDirty = true;
		}
		#endregion

		#region INotifyPropertyChanged Members
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string propertyName)
		{
			if( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
			}
		}
		#endregion
	}



	#region Validation Structures
	public class UIElement : sf.IUIElement, sf.IObjectModel, IObjectCollectionHost,
		sf.ICloneable<UIElement>, INotifyPropertyChanged, INodeItem,
		IDatabaseObject, INotifyDeleted
	{
		private string _name = string.Empty;
		private string _controlType = string.Empty;
		private string _uniqueName = string.Empty;
		private string _desc = string.Empty;
		private bool _descTooltip = false;
		private bool _allowUndeclared = false;
		private TypeCode _dataType = TypeCode.Empty;
		private string _dataTypeErrMsg = string.Empty;
		private string _formatString = string.Empty;

		private bool _isDirty = false;
		private sf.IObjectModel _parentObject = null;

		public static ObjectType CloneShallow = ObjectType.None;
		public static ObjectType CloneSecurity = ObjectType.Ace | ObjectType.RightRole | ObjectType.RightRoleRule;
		public static ObjectType CloneValidation = ObjectType.ValidationRule | ObjectType.FillMap | ObjectType.ElseRule | ObjectType.ElseMap;
		public static ObjectType CloneDeep = ObjectType.UIElement | CloneSecurity | CloneValidation;

		private CompositeCollection _cc = null;

		public UIElement()
		{
			Id = Guid.NewGuid();
			iUIElements = new UIElementCollection( this );
			iValidationRules = new ValidationRuleCollection( this );
			iFillMaps = new FillMapCollection( this );

			CollectionContainer iuie = new CollectionContainer() { Collection = this.iUIElements };
			CollectionContainer ivr = new CollectionContainer() { Collection = this.iValidationRules };
			CollectionContainer ifm = new CollectionContainer() { Collection = this.iFillMaps };

			this.ChildObjects = new CompositeCollection();
			this.ChildObjects.Add( iuie );
			this.ChildObjects.Add( ivr );
			this.ChildObjects.Add( ifm );

			//DaclInherit = true;
			//SaclInherit = true;
			//SaclAuditTypeFilter =
			//    ss.AuditType.Information | ss.AuditType.Warning | ss.AuditType.Error |
			//    ss.AuditType.SuccessAudit | ss.AuditType.FailureAudit;

			this.SecurityDescriptor = new SecurityDescriptor( this );
			this.SecurityDescriptor.PropertyChanged += new PropertyChangedEventHandler( SecurityDescriptor_PropertyChanged );
		}

		void SecurityDescriptor_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.IsDirty = true;
		}

		[XmlIgnore()]
		public CompositeCollection ChildObjects { get { return _cc; } private set { _cc = value; } }


		#region Validation
		public Guid Id { get; set; }

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

		public string ControlType
		{
			get { return _controlType; }
			set
			{
				if( _controlType != value )
				{
					_controlType = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "ControlType" );
				}
			}
		}

		public string UniqueName
		{
			get { return _uniqueName; }
			set
			{
				if( _uniqueName != value )
				{
					_uniqueName = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "UniqueName" );
				}
			}
		}

		public string Desc
		{
			get { return _desc; }
			set
			{
				if( _desc != value )
				{
					_desc = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Desc" );
				}
			}
		}

		public bool DescTooltip
		{
			get { return _descTooltip; }
			set
			{
				if( _descTooltip != value )
				{
					_descTooltip = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "DescTooltip" );
				}
			}
		}

		public bool AllowUndeclared
		{
			get { return _allowUndeclared; }
			set
			{
				if( _allowUndeclared != value )
				{
					_allowUndeclared = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "AllowUndeclared" );
				}
			}
		}

		public TypeCode DataType
		{
			get { return _dataType; }
			set
			{
				if( _dataType != value )
				{
					_dataType = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "DataType" );
				}
			}
		}

		public string DataTypeErrMsg
		{
			get { return _dataTypeErrMsg; }
			set
			{
				if( _dataTypeErrMsg != value )
				{
					_dataTypeErrMsg = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "DataTypeErrMsg" );
				}
			}
		}

		public string FormatString
		{
			get { return _formatString; }
			set
			{
				if( _formatString != value )
				{
					_formatString = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "FormatString" );
				}
			}
		}


		[XmlIgnore()]
		public Guid ParentId { get; set; }

		[XmlIgnore()]
		public sf.ValidationRuleCollection ValidationRules { get; set; }
		[XmlIgnore()]
		public sf.FillMapCollection FillMaps { get; set; }
		[XmlIgnore()]
		public sf.UIElementCollection UIElements { get; set; }

		[XmlArray( "ValidationRules" )]
		public ValidationRuleCollection iValidationRules { get; set; }
		[XmlArray( "FillMaps" )]
		public FillMapCollection iFillMaps { get; set; }
		[XmlArray( "UIElements" )]
		public UIElementCollection iUIElements { get; set; }
		#endregion

		#region Security
		[XmlIgnore()]
		public bool DaclInherit { get; set; }
		[XmlIgnore()]
		public bool SaclInherit { get; set; }
		[XmlIgnore()]
		public ss.AuditType SaclAuditTypeFilter { get; set; }

		public SecurityDescriptor SecurityDescriptor { get; set; }
		#endregion

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


		#region IObjectModel Members
		public sf.ObjectType ObjectType { get { return sf.ObjectType.UIElement; } }
		public sf.ObjectType ValidChildObjectTypes { get { return sf.ObjectType.UIElement | sf.ObjectType.ValidationRule | sf.ObjectType.RightRoleRule | sf.ObjectType.FillMap; } }
		public bool SupportsChildObjectType(sf.ObjectType objectType)
		{
			return (this.ValidChildObjectTypes & objectType) == objectType;
		}
		[XmlIgnore()]
		public sf.IObjectModel ParentObject
		{
			get { return _parentObject; }
			set
			{
				if( _parentObject != value )
				{
					//this.OnPropertyChanging( "ParentObject" );
					_parentObject = value;
					this.ParentId = value is UIElement ? ((UIElement)value).Id : Guid.Empty;
					this.OnPropertyChanged( "ParentObject" );
				}
			}
		}
		#endregion

		#region IObjectCollectionHost Members
		public void AddChildObject(Suplex.Forms.IObjectModel child)
		{
			switch( child.ObjectType )
			{
				case sf.ObjectType.ValidationRule:
				{
					ValidationRule vr = (ValidationRule)child;
					if( vr.EventBinding == ControlEvents.None )
					{
						vr.EventBinding = ControlEvents.Validating;
					}

					this.iValidationRules.Add( vr );

					break;
				}
				case sf.ObjectType.FillMap:
				{
					FillMap fm = (FillMap)child;
					if( fm.EventBinding == ControlEvents.None )
					{
						fm.EventBinding = ControlEvents.Validating;
					}

					this.iFillMaps.Add( fm );
					break;
				}
				case sf.ObjectType.UIElement:
				{
					this.iUIElements.Add( (UIElement)child );
					break;
				}
			}

			((IDatabaseObject)child).RecordUpsert();
		}
		public void RemoveChildObject(Suplex.Forms.IObjectModel child)
		{
			switch( child.ObjectType )
			{
				case sf.ObjectType.ValidationRule:
				{
					this.iValidationRules.Remove( (ValidationRule)child );
					break;
				}
				case sf.ObjectType.FillMap:
				{
					this.iFillMaps.Remove( (FillMap)child );
					break;
				}
				case sf.ObjectType.UIElement:
				{
					this.iUIElements.Remove( (UIElement)child );
					break;
				}
			}
		}
		#endregion

		#region INotifyPropertyChanged Members
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string propertyName)
		{
			if( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
			}
		}
		#endregion

		#region ICloneable<UIElement> Members
		public sf.IObjectModel Clone(bool generateNewId)
		{
			UIElement clone = this.Clone( CloneShallow, false );
			if( generateNewId )
			{
				clone.Id = Guid.NewGuid();
			}
			return clone;
		}
		UIElement ICloneable<UIElement>.Clone()
		{
			return this.Clone( CloneShallow, false );
		}
		public UIElement Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			UIElement uie = new UIElement();

			uie.Id = this.Id;
			uie.Name = this.Name;
			uie.ControlType = this.ControlType;
			uie.UniqueName = this.UniqueName;
			uie.Desc = this.Desc;
			uie.DescTooltip = this.DescTooltip;
			uie.AllowUndeclared = this.AllowUndeclared;
			uie.DataType = this.DataType;
			uie.DataTypeErrMsg = this.DataTypeErrMsg;
			uie.FormatString = this.FormatString;

			uie.ParentObject = this.ParentObject;

			uie.SecurityDescriptor = this.SecurityDescriptor.Clone( cloneDepth, cloneChildrenAsRef );

			if( (cloneDepth & ObjectType.ValidationRule) == ObjectType.ValidationRule )
			{
				foreach( ValidationRule rule in this.iValidationRules )
				{
					if( cloneChildrenAsRef )
					{
						uie.iValidationRules.Add( rule );
					}
					else
					{
						uie.iValidationRules.Add( rule.Clone( cloneDepth, cloneChildrenAsRef ) );
					}
				}
			}
			if( (cloneDepth & ObjectType.FillMap) == ObjectType.FillMap )
			{
				foreach( FillMap fm in this.iFillMaps )
				{
					if( cloneChildrenAsRef )
					{
						uie.iFillMaps.Add( fm );
					}
					else
					{
						uie.iFillMaps.Add( fm.Clone() );
					}
				}
			}
			if( (cloneDepth & ObjectType.UIElement) == ObjectType.UIElement )
			{
				foreach( UIElement u in this.iUIElements )
				{
					if( cloneChildrenAsRef )
					{
						uie.iUIElements.Add( u );
					}
					else
					{
						uie.iUIElements.Add( u.Clone( cloneDepth, cloneChildrenAsRef ) );
					}
				}
			}

			return uie;
		}

		void ICloneable<UIElement>.Synchronize(UIElement sourceObject)
		{
			this.Synchronize( sourceObject, CloneShallow, false );
		}

		public void Synchronize(UIElement sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			this.Name = sourceObject.Name;
			this.ControlType = sourceObject.ControlType;
			this.UniqueName = sourceObject.UniqueName;
			this.Desc = sourceObject.Desc;
			this.DescTooltip = sourceObject.DescTooltip;
			this.AllowUndeclared = sourceObject.AllowUndeclared;
			this.DataType = sourceObject.DataType;
			this.DataTypeErrMsg = sourceObject.DataTypeErrMsg;
			this.FormatString = sourceObject.FormatString;

			this.SecurityDescriptor.Synchronize( sourceObject.SecurityDescriptor, cloneDepth, cloneChildrenAsRef );

			if( (cloneDepth & ObjectType.ValidationRule) == ObjectType.ValidationRule )
			{
				foreach( ValidationRule rule in sourceObject.iValidationRules )
				{
					if( cloneChildrenAsRef )
					{
						this.iValidationRules.Add( rule );
					}
					else
					{
						ValidationRule found = this.iValidationRules.GetByValidationRuleId( rule.Id );
						if( found != null )
						{
							found.Synchronize( rule, cloneDepth, cloneChildrenAsRef );
						}
						else
						{
							this.iValidationRules.Add( rule.Clone( cloneDepth, cloneChildrenAsRef ) );
						}
					}
				}
			}
			if( (cloneDepth & ObjectType.FillMap) == ObjectType.FillMap )
			{
				foreach( FillMap fm in sourceObject.iFillMaps )
				{
					if( cloneChildrenAsRef )
					{
						this.iFillMaps.Add( fm );
					}
					else
					{
						FillMap found = this.iFillMaps.GetByFillMapId( fm.Id );
						if( found != null )
						{
							found.Synchronize( fm );
						}
						else
						{
							this.iFillMaps.Add( fm.Clone() );
						}
					}
				}
			}
			if( (cloneDepth & ObjectType.UIElement) == ObjectType.UIElement )
			{
				foreach( UIElement u in sourceObject.iUIElements )
				{
					if( cloneChildrenAsRef )
					{
						this.iUIElements.Add( u );
					}
					else
					{
						UIElement found = this.iUIElements.GetById( u.Id );
						if( found != null )
						{
							found.Synchronize( u, cloneDepth, cloneChildrenAsRef );
						}
						else
						{
							this.iUIElements.Add( u.Clone( cloneDepth, cloneChildrenAsRef ) );
						}
					}
				}
			}
		}

		//This is a special sync only to be used after a database Create/Update statement.
		//It's needed to sync the Ids on Child collections - Aces/RightBindings, in this case
		public void SynchronizeSpecial(UIElement sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			if( StoreConnection.IsConnected )
			{
				this.SecurityDescriptor.Synchronize( sourceObject.SecurityDescriptor, cloneDepth, cloneChildrenAsRef );
				this.IsDirty = false;
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

				if( this.WantsChildUIElementsLazyLoad )
				{
					iUIElements.Clear();
					iValidationRules.Clear();
					iFillMaps.Clear();

					StoreConnection.da.OpenConnection();

					DataSet uie =
						StoreConnection.da.GetDataSet( "splx_api_sel_uielementbyparent", new sSortedList( "@UIE_PARENT_ID",  this.Id ), false );

					DataSet vr =
						StoreConnection.da.GetDataSet( "splx_api_sel_vrbyuieparent_composite", new sSortedList( "@SPLX_UI_ELEMENT_ID", this.Id ), false );
					StoreConnection.da.NameTablesFromCompositeSelect( ref vr );

					DataSet fm =
						StoreConnection.da.GetDataSet( "splx_api_sel_fmbyparent_composite", new sSortedList( "@SPLX_UIE_VR_PARENT_ID", this.Id ), false );
					StoreConnection.da.NameTablesFromCompositeSelect( ref fm );

					StoreConnection.da.CloseConnection();


					iUIElements.LoadFromDataTable( uie.Tables[0] );	//TODO: WithResolve ?	//, false
					iValidationRules.LoadFromDataTable( vr, this.Id.ToString(), null, LogicRuleType.ValidationIf, true );
					iFillMaps.LoadFromDataTable( fm.Tables["FillMaps"], FillMapType.FillMapIf, fm.Tables["DataBindings"], this.Id.ToString() );
				}

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

				if( this.EnableLazyLoad && this.WantsDetailLazyLoad )
				{
					this.RecordSelect();
				}

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

		#region IDatabaseObject Members
		public void RecordSelectWithChildren()
		{
			if( StoreConnection.IsConnected )
			{
				DataSet ds = StoreConnection.da.GetDataSet( "splx_api_sel_uie_withchildren_composite",
					new sSortedList( "@SPLX_UI_ELEMENT_ID", this.Id ) );
				StoreConnection.da.NameTablesFromCompositeSelect( ref ds );

				if( ds.Tables["UIElements"].Rows.Count > 0 )
				{
					//load this node and its immediate children (VRs and FMs)
					this.LoadFromDataSet( ds, this.Id.ToString() );
					this.iValidationRules.LoadFromDataTable( ds, this.Id.ToString(), null, LogicRuleType.ValidationIf, false );
					this.iFillMaps.LoadFromDataTable( ds.Tables["FillMaps"], FillMapType.FillMapIf, ds.Tables["DataBindings"], this.Id.ToString() );
					this.EnableLazyLoad = false;

					//recurse for child UIEs and thier children
					this.LoadFromDataSetRecursiveComposite( ds, this.Id.ToString(), this.iUIElements, true );
				}
				else
				{
					throw new RowNotInTableException( string.Format( "Unable to fetch UIElement '{0}' from the data store.", this.Name ) );
				}
			}
		}

		public void RecordSelect()
		{
			if( StoreConnection.IsConnected )
			{
				DataSet ds = StoreConnection.da.GetDataSet( "splx_api_sel_uielementbyid_composite",
					new sSortedList( "@SPLX_UI_ELEMENT_ID", this.Id ) );
				StoreConnection.da.NameTablesFromCompositeSelect( ref ds );

				if( ds.Tables["UIElements"].Rows.Count > 0 )
				{
					this.LoadFromDataSet( ds, this.Id.ToString() );
				}
				else
				{
					throw new RowNotInTableException( string.Format( "Unable to fetch UIElement '{0}' from the data store.", this.Name ) );
				}
			}
		}

		public void RecordUpsert()
		{
			if( StoreConnection.IsConnected )
			{
				SortedList inparms = this.GetParms();

				SqlParameter id = new SqlParameter( "@SPLX_UI_ELEMENT_ID", SqlDbType.UniqueIdentifier );
				id.Value = this.Id;
				id.Direction = ParameterDirection.InputOutput;
				SortedList outparms = new sSortedList( "@SPLX_UI_ELEMENT_ID", id );

				StoreConnection.da.OpenConnection();
				SqlTransaction tr = StoreConnection.da.Connection.BeginTransaction();
				try
				{
					StoreConnection.da.ExecuteSP( "splx_api_upsert_uie", inparms, ref outparms, false, tr );
					this.Id = (Guid)id.Value;


					if( this.SecurityDescriptor.HaveDeleteIds )
					{
						string daclIds = sg.MiscUtils.Join<long>( ",", this.SecurityDescriptor.DeleteDaclIds );
						string saclIds = sg.MiscUtils.Join<long>( ",", this.SecurityDescriptor.DeleteSaclIds );
						string rrIds = sg.MiscUtils.Join<long>( ",", this.SecurityDescriptor.DeleteRightRoleIds );
						string rrrIds = sg.MiscUtils.Join<Guid>( ",", this.SecurityDescriptor.DeleteRightRoleRuleIds );

						SortedList sd = new sSortedList( "@SPLX_ACE_ID_LIST", string.Format( "{0}, {1}", daclIds, saclIds ),
							"@SPLX_RIGHT_ROLE_ID_LIST", rrIds, "@SPLX_VALIDATION_RULE_ID_LIST",
							string.IsNullOrEmpty( rrrIds ) ? Convert.DBNull : rrrIds );

						StoreConnection.da.ExecuteSP( "splx_api_del_uie_sd", sd, false, tr );
					}


					foreach( AccessControlEntryBase ace in this.SecurityDescriptor.Dacl )
					{
						if( ace.IsDirty )
						{
							ace.RecordUpsert( this.Id, ref tr );
							ace.IsDirty = false;
						}
					}

					foreach( AccessControlEntryAuditBase ace in this.SecurityDescriptor.Sacl )
					{
						if( ace.IsDirty )
						{
							ace.RecordUpsert( this.Id, ref tr );
							ace.IsDirty = false;
						}
					}

					foreach( RightRole rr in this.SecurityDescriptor.RightRoles )
					{
						if( rr.IsDirty )
						{
							rr.RecordUpsert( this.Id, ref tr );
							rr.IsDirty = false;
						}
					}

					this.RecurseRightRoleRulesForUpsert( this.Id, Guid.Empty, this.SecurityDescriptor.iRightRoleRules, ref tr );


					tr.Commit();

					this.IsDirty = false;
				}
				catch( Exception ex )
				{
					tr.Rollback();
					throw ex;
				}
				finally
				{
					StoreConnection.da.CloseConnection();
				}
			}
		}

		private void RecurseRightRoleRulesForUpsert(Guid parentUIElementId, Guid parentRuleId, RightRoleRuleCollection rules, ref SqlTransaction tr)
		{
			foreach( RightRoleRule rule in rules )
			{
				if( rule.IsDirty )
				{
					rule.RecordUpsert( parentUIElementId, parentRuleId, ref tr );
					rule.IsDirty = false;
				}

				foreach( RightRole role in rule.iRightRoles )
				{
					if( role.IsDirty )
					{
						role.RecordUpsert( rule.Id, ref tr );
						role.IsDirty = false;
					}
				}

				foreach( RightRole role in rule.iElseRoles )
				{
					if( role.IsDirty )
					{
						role.RecordUpsert( rule.Id, ref tr );
						role.IsDirty = false;
					}
				}

				this.RecurseRightRoleRulesForUpsert( parentUIElementId, rule.Id, rule.iRightRoleRules, ref tr );
				this.RecurseRightRoleRulesForUpsert( parentUIElementId, rule.Id, rule.iElseRules, ref tr );
			}
		}

		public void RecordUpsertForImport(ref SqlTransaction tr, bool includeSecurity)
		{
			SortedList inparms = this.GetParms();

			SqlParameter id = new SqlParameter( "@SPLX_UI_ELEMENT_ID", SqlDbType.UniqueIdentifier );
			id.Value = this.Id;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_UI_ELEMENT_ID", id );

			StoreConnection.da.ExecuteSP( "splx_api_upsert_uie", inparms, ref outparms, false, tr );
			this.Id = (Guid)id.Value;


			if( includeSecurity )
			{
				foreach( AccessControlEntryBase ace in this.SecurityDescriptor.Dacl )
				{
					ace.RecordUpsert( this.Id, ref tr );
				}

				foreach( AccessControlEntryAuditBase ace in this.SecurityDescriptor.Sacl )
				{
					ace.RecordUpsert( this.Id, ref tr );
				}

				foreach( RightRole rr in this.SecurityDescriptor.RightRoles )
				{
					rr.RecordUpsert( this.Id, ref tr );
				}

				this.RecurseRightRoleRulesForUpsertForImport( this.Id, Guid.Empty, this.SecurityDescriptor.iRightRoleRules, ref tr );
			}


			foreach( ValidationRule rule in this.iValidationRules )
			{
				rule.RecordUpsertForImport( ref tr, this.Id, Guid.Empty );
			}

			foreach( FillMap map in this.iFillMaps )
			{
				map.RecordUpsertForImport( ref tr );
			}

			foreach( UIElement uie in this.iUIElements )
			{
				uie.RecordUpsertForImport( ref tr, includeSecurity );
			}
		}

		private void RecurseRightRoleRulesForUpsertForImport(Guid parentUIElementId, Guid parentRuleId, RightRoleRuleCollection rules, ref SqlTransaction tr)
		{
			foreach( RightRoleRule rule in rules )
			{
				rule.RecordUpsert( parentUIElementId, parentRuleId, ref tr );

				foreach( RightRole role in rule.iRightRoles )
				{
					role.RecordUpsert( rule.Id, ref tr );
				}

				foreach( RightRole role in rule.iElseRoles )
				{
					role.RecordUpsert( rule.Id, ref tr );
				}

				this.RecurseRightRoleRulesForUpsertForImport( parentUIElementId, rule.Id, rule.iRightRoleRules, ref tr );
				this.RecurseRightRoleRulesForUpsertForImport( parentUIElementId, rule.Id, rule.iElseRules, ref tr );
			}
		}

		public void RecordDelete()
		{
			if( StoreConnection.IsConnected )
			{
				SortedList inparms = new sSortedList( "@SPLX_UI_ELEMENT_ID", this.Id );
				StoreConnection.da.ExecuteSP( "splx_api_del_uie", inparms );

				//StoreConnection.da.OpenConnection();
				//SqlTransaction tr = StoreConnection.da.Connection.BeginTransaction();
				//try
				//{
				//    this.RecordDeleteRecursive( tr );
				//}
				//catch( Exception ex )
				//{
				//    tr.Rollback();
				//    throw ex;
				//}
				//finally
				//{
				//    StoreConnection.da.CloseConnection();
				//}
			}
		}

		internal void RecordDeleteRecursive(SqlTransaction tr)
		{
			//walk down the heirarchy to the bottom, delete on the way back up.
			foreach( UIElement uie in this.iUIElements )
			{
				uie.RecordDeleteRecursive( tr );
			}

			//walk down the heirarchy to the bottom, delete on the way back up.
			foreach( ValidationRule vr in this.iValidationRules )
			{
				vr.RecordDeleteRecursive( tr );
			}

			foreach( FillMap fm in this.iFillMaps )
			{
				fm.RecordDelete( tr );
			}

			//walk down the heirarchy to the bottom, delete on the way back up.
			foreach( RightRoleRule rr in this.SecurityDescriptor.iRightRoleRules )
			{
				rr.RecordDeleteRecursive( tr );
			}

			SortedList inparms = new sSortedList( "@SPLX_UI_ELEMENT_ID", this.Id );
			StoreConnection.da.ExecuteSP( "splx_api_del_uie", inparms, false, tr );
		}

		internal void LoadFromDataSetRecursiveComposite(DataSet ds, string parentId, UIElementCollection parentCollection, bool includeValidation)
		{
			DataRow[] rows = string.IsNullOrEmpty( parentId ) ?
				ds.Tables["UIElements"].Select( "UIE_PARENT_ID IS NULL" ) :
				ds.Tables["UIElements"].Select( string.Format( "UIE_PARENT_ID = '{0}'", parentId ) );
			foreach( DataRow r in rows )
			{
				UIElement uie = new UIElement();
				uie.LoadFromDataSet( ds, r["SPLX_UI_ELEMENT_ID"].ToString() );

				UIElement exists = parentCollection.GetById( uie.Id );
				if( exists == null )
				{
					exists = uie;
					parentCollection.Add( exists );
				}
				else
				{
					exists.Synchronize( uie, UIElement.CloneSecurity, false );
				}


				if( includeValidation )
				{
					exists.iValidationRules.LoadFromDataTable( ds, uie.Id.ToString(), null, LogicRuleType.ValidationIf, false );
					exists.iFillMaps.LoadFromDataTable( ds.Tables["FillMaps"], FillMapType.FillMapIf, ds.Tables["DataBindings"], uie.Id.ToString() );
				}

				exists.EnableLazyLoad = false;
				exists.LoadFromDataSetRecursiveComposite( ds, r["SPLX_UI_ELEMENT_ID"].ToString(), exists.iUIElements, includeValidation );
			}
		}

		internal void LoadFromDataSet(DataSet ds, string uieIdFilter)
		{
			if( string.IsNullOrEmpty( uieIdFilter ) )
			{
				throw new ArgumentException( "uieIdFilter cannot be Null or Empty" );
			}

			DataRow r = ds.Tables["UIElements"].Select( string.Format( "SPLX_UI_ELEMENT_ID = '{0}'", uieIdFilter ) )[0];

			Id = new Guid( r["SPLX_UI_ELEMENT_ID"].ToString() );
			Name = (string)r["UIE_NAME"];
			ControlType = (string)r["UIE_CONTROL_TYPE"];
			Desc = r["UIE_DESC"].ToString();
			DescTooltip = (bool)r["UIE_DESC_TOOLTIP"];
			UniqueName = r["UIE_UNIQUE_NAME"].ToString();
			AllowUndeclared = (bool)r["UIE_ALLOW_UNDECLARED"];
			DataType = r["UIE_DATA_TYPE"] == Convert.DBNull ?
				TypeCode.String : sg.MiscUtils.ParseEnum<TypeCode>( r["UIE_DATA_TYPE"].ToString() );
			DataTypeErrMsg = r["UIE_DATA_TYPE_ERR_MSG"].ToString();
			FormatString = r["UIE_FORMAT_STRING"].ToString();

			this.SecurityDescriptor.DaclInherit = r["UIE_DACL_INHERIT"] == Convert.DBNull ? true : (bool)r["UIE_DACL_INHERIT"];
			this.SecurityDescriptor.SaclInherit = r["UIE_SACL_INHERIT"] == Convert.DBNull ? true : (bool)r["UIE_SACL_INHERIT"];
			this.SecurityDescriptor.SaclAuditTypeFilter = r["UIE_SACL_AUDIT_TYPE_FILTER"] == Convert.DBNull ?
				SecurityDescriptor.DefaultSaclAuditTypeFilter :
				sg.MiscUtils.ParseEnum<ss.AuditType>( r["UIE_SACL_AUDIT_TYPE_FILTER"] );

			if( ds.Tables.Contains( "Aces" ) )
			{
				this.SecurityDescriptor.Dacl.LoadFromDataTable( ds.Tables["Aces"], uieIdFilter );
				this.SecurityDescriptor.Sacl.LoadFromDataTable( ds.Tables["Aces"], uieIdFilter );
			}

			if( ds.Tables.Contains( "UieRightRoles" ) )
			{
				this.SecurityDescriptor.RightRoles.LoadFromDataTable( ds.Tables["UieRightRoles"], sf.RightRoleType.Success, uieIdFilter );
			}

			if( ds.Tables.Contains( "RightRoleRules" ) && ds.Tables.Contains( "RrrRightRoles" ) )
			{
				this.SecurityDescriptor.iRightRoleRules.LoadFromDataTable( ds.Tables["RightRoleRules"], ds.Tables["RrrRightRoles"], uieIdFilter );
			}


			this.WantsDetailLazyLoad = false;

			IsDirty = false;
		}

		private SortedList GetParms()
		{
			sSortedList s = new sSortedList( "@UIE_NAME", this.Name );
			s.Add( "@UIE_CONTROL_TYPE", this.ControlType );
			s.Add( "@UIE_DESC", this.Desc );
			s.Add( "@UIE_DESC_TOOLTIP", this.DescTooltip );
			s.Add( "@UIE_UNIQUE_NAME", this.UniqueName );
			s.Add( "@UIE_DATA_TYPE", this.DataType.ToString() );
			s.Add( "@UIE_DATA_TYPE_ERR_MSG", this.DataTypeErrMsg );
			s.Add( "@UIE_FORMAT_STRING", this.FormatString );
			s.Add( "@UIE_ALLOW_UNDECLARED", this.AllowUndeclared );
			s.Add( "@UIE_PARENT_ID", this.ParentId == Guid.Empty ? Convert.DBNull : this.ParentId );
			s.Add( "@UIE_DACL_INHERIT", this.SecurityDescriptor.DaclInherit );
			s.Add( "@UIE_SACL_INHERIT", this.SecurityDescriptor.SaclInherit );
			s.Add( "@UIE_SACL_AUDIT_TYPE_FILTER", this.SecurityDescriptor.SaclAuditTypeFilter );

			return s;
		}

		[XmlIgnore()]
		internal bool EnableLazyLoad
		{
			get { return _enableLazyLoad; }
			set
			{
				if( _enableLazyLoad != value )
				{
					_enableLazyLoad = value;

					if( _enableLazyLoad )
					{
						if( StoreConnection.IsConnected && iUIElements.Count == 0 )
						{
							iUIElements.Add( new LazyLoadDummyUIElement() );
						}
					}
					else
					{
						if( this.WantsChildUIElementsLazyLoad )
						{
							iUIElements.Clear();
						}
					}
				}
			}
		}

		[XmlIgnore()]
		private bool WantsChildUIElementsLazyLoad
		{
			get
			{
				return StoreConnection.IsConnected &&
					iUIElements.Count > 0 && iUIElements[0] is LazyLoadDummyUIElement;
			}
		}
		[XmlIgnore()]
		private bool WantsDetailLazyLoad
		{
			get
			{
				return StoreConnection.IsConnected && _wantsDetailLazyLoad;
			}
			set
			{
				_wantsDetailLazyLoad = value;
			}
		}
		#endregion

		#region INotifyDeleted Members
		private bool _isDeleted = false;
		[XmlIgnore()]
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
	}

	public class LazyLoadDummyUIElement : UIElement
	{
	}

	public class UIElementCollection : ObservableObjectModelCollection<UIElement>
	{
		public UIElementCollection() : base() { }
		public UIElementCollection(sf.IObjectModel owner)
			: base( owner )
		{ }

		public void LoadFromDataTable(DataTable t)	//bool append
		{
			this.Clear();

			DataTableReader r = new DataTableReader( t );
			while( r.Read() )
			{
				this.Add(
					new UIElement()
					{
						Id = new Guid( r["SPLX_UI_ELEMENT_ID"].ToString() ),
						Name = (string)r["UIE_NAME"],
						ControlType = (string)r["UIE_CONTROL_TYPE"],
						Desc = r["UIE_DESC"].ToString(),
						DescTooltip = (bool)r["UIE_DESC_TOOLTIP"],
						UniqueName = r["UIE_UNIQUE_NAME"].ToString(),
						DataType = r["UIE_DATA_TYPE"] == Convert.DBNull ?
							TypeCode.String : sg.MiscUtils.ParseEnum<TypeCode>( r["UIE_DATA_TYPE"].ToString() ),
						DataTypeErrMsg = r["UIE_DATA_TYPE_ERR_MSG"].ToString(),
						FormatString = r["UIE_FORMAT_STRING"].ToString(),

						EnableLazyLoad = true,
						IsDirty = false
					}
				);
			}
		}

		public UIElement GetById(Guid id)
		{
			//return this.First( uie => uie.Id == id );
			return this.SelectRecursive( uie => (IEnumerable<UIElement>)uie.iUIElements )
				.FirstOrDefault( uie => uie.Id == id );
		}

		public UIElement GetByUniqueName(string uniqueName)
		{
			//return this.First( uie => uie.Id == id );
			return this.SelectRecursive( uie => (IEnumerable<UIElement>)uie.iUIElements )
				.FirstOrDefault( uie => uie.UniqueName == uniqueName );
		}
	}

	//TODO: implement INotifyPropertyChanged on all properties
	public abstract class LogicRule : sf.ILogicRule, INotifyPropertyChanged, INotifyPropertyChanging,
		IDatabaseObject, INotifyDeleted
	{
		private string _name = string.Empty;
		private string _iCompareValue1 = string.Empty;
		private sf.ComparisonValueType _valueType1 = ComparisonValueType.Empty;
		private sf.ExpressionType _expressionType1 = ExpressionType.None;
		private string _iCompareValue2 = string.Empty;
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
		public abstract ObjectType LogicRuleObjectType { get; }

		public LogicRuleType LogicRuleType { get; set; }

		public Guid Id { get; set; }

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

		[XmlIgnore()]
		public object CompareValue1 { get; set; }
		[XmlElement( "CompareValue1" )]
		public string iCompareValue1
		{
			get { return _iCompareValue1; }
			set
			{
				if( _iCompareValue1 != value )
				{
					_iCompareValue1 = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "iCompareValue1" );
				}
			}
		}

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

		[XmlIgnore()]
		public object CompareValue2 { get; set; }
		[XmlElement( "CompareValue2" )]
		public string iCompareValue2
		{
			get { return _iCompareValue2; }
			set
			{
				if( _iCompareValue2 != value )
				{
					_iCompareValue2 = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "iCompareValue2" );
				}
			}
		}

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
		protected virtual LogicRule Clone(sf.ObjectType type)
		{
			LogicRule r = null;
			switch( type )
			{
				case sf.ObjectType.ValidationRule:
				{
					r = new ValidationRule();
					break;
				}
				case sf.ObjectType.RightRoleRule:
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
			r.iCompareValue1 = this.iCompareValue1;
			r.ValueType1 = this.ValueType1;
			r.ExpressionType1 = this.ExpressionType1;
			r.iCompareValue2 = this.iCompareValue2;
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
				case sf.ObjectType.ValidationRule:
				case sf.ObjectType.RightRoleRule:
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
			this.iCompareValue1 = sourceObject.iCompareValue1;
			this.ValueType1 = sourceObject.ValueType1;
			this.ExpressionType1 = sourceObject.ExpressionType1;
			this.iCompareValue2 = sourceObject.iCompareValue2;
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

		#region IDatabaseObject Members
		abstract public void RecordSelect();
		abstract public void RecordUpsert();
		public virtual void RecordDelete()
		{
			if( StoreConnection.IsConnected )
			{
				SortedList inparms = new sSortedList( "@SPLX_VALIDATION_RULE_ID", this.Id );
				StoreConnection.da.ExecuteSP( "splx_api_del_vr", inparms );

				//StoreConnection.da.OpenConnection();
				//SqlTransaction tr = StoreConnection.da.Connection.BeginTransaction();
				//try
				//{
				//    this.RecordDeleteRecursive( tr );
				//}
				//catch( Exception ex )
				//{
				//    tr.Rollback();
				//    throw ex;
				//}
				//finally
				//{
				//    StoreConnection.da.CloseConnection();
				//}
			}
		}

		abstract internal void RecordDeleteRecursive(SqlTransaction tr);
		#endregion

		#region INotifyDeleted Members
		private bool _isDeleted = false;
		[XmlIgnore()]
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
	}

	public class ValidationRule : LogicRule, sf.IValidationRule, sf.IObjectModel, IObjectCollectionHost, ICloneable<ValidationRule>, INodeItem
	{
		public static ObjectType CloneShallow = ObjectType.None;
		public static ObjectType CloneDeep = ObjectType.ValidationRule | ObjectType.FillMap | ObjectType.ElseRule | ObjectType.ElseMap;

		private sf.ControlEvents _eventBinding = ControlEvents.Validating;
		private string _iErrorControl = string.Empty;
		private sf.IObjectModel _parentObject = null;

		public ValidationRule()
			: base()
		{
			this.LogicRuleType = sf.LogicRuleType.ValidationIf;
			iFillMaps = new FillMapCollection( this );
			iValidationRules = new ValidationRuleCollection( this );
			iElseMaps = new FillMapCollection( this );
			iElseRules = new ValidationRuleCollection( this );

			CollectionContainer ivr = new CollectionContainer() { Collection = this.iValidationRules };
			CollectionContainer ifm = new CollectionContainer() { Collection = this.iFillMaps };
			CollectionContainer iem = new CollectionContainer() { Collection = this.iElseMaps };
			CollectionContainer ier = new CollectionContainer() { Collection = this.iElseRules };

			this.ChildObjects = new CompositeCollection();
			this.ChildObjects.Add( ivr );
			this.ChildObjects.Add( ifm );
			this.ChildObjects.Add( iem );
			this.ChildObjects.Add( ier );
		}

		[XmlIgnore()]
		public CompositeCollection ChildObjects { get; private set; }

		[XmlIgnore()]
		public override ObjectType LogicRuleObjectType { get { return sf.ObjectType.ValidationRule; } }

		[XmlIgnore()]
		public object ErrorControl { get; set; }
		[XmlElement( "ErrorControl" )]
		public string iErrorControl
		{
			get { return _iErrorControl; }
			set
			{
				if( _iErrorControl != value )
				{
					_iErrorControl = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "iErrorControl" );
				}
			}
		}

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


		[XmlIgnore()]
		public sf.FillMapCollection FillMaps { get; set; }
		[XmlIgnore()]
		public sf.ValidationRuleCollection ValidationRules { get; set; }
		[XmlIgnore()]
		public sf.FillMapCollection ElseMaps { get; set; }
		[XmlIgnore()]
		public sf.ValidationRuleCollection ElseRules { get; set; }

		[XmlArray( "FillMaps" )]
		public FillMapCollection iFillMaps { get; set; }
		[XmlArray( "ValidationRules" )]
		public ValidationRuleCollection iValidationRules { get; set; }
		[XmlArray( "ElseMaps" )]
		public FillMapCollection iElseMaps { get; set; }
		[XmlArray( "ElseRules" )]
		public ValidationRuleCollection iElseRules { get; set; }

		#region IObjectModel Members
		public sf.ObjectType ObjectType { get { return sf.ObjectType.ValidationRule; } }
		public sf.ObjectType ValidChildObjectTypes { get { return sf.ObjectType.ValidationRule | sf.ObjectType.ElseRule | sf.ObjectType.FillMap | sf.ObjectType.ElseMap; } }
		public bool SupportsChildObjectType(sf.ObjectType objectType)
		{
			return (this.ValidChildObjectTypes & objectType) == objectType;
		}
		[XmlIgnore()]
		public sf.IObjectModel ParentObject
		{
			get { return _parentObject; }
			set
			{
				if( _parentObject != value )
				{
					this.OnPropertyChanging( "ParentObject" );
					_parentObject = value;
					this.OnPropertyChanged( "ParentObject" );
				}
			}
		}
		#endregion

		#region IObjectCollectionHost Members
		//NOTE: child objects of ValidationRules don't support their own EventBindings
		public void AddChildObject(Suplex.Forms.IObjectModel child)
		{
			switch( child.ObjectType )
			{
				case sf.ObjectType.ValidationRule:
				{
					ValidationRule vr = (ValidationRule)child;

					vr.EventBinding = ControlEvents.None;

					if( vr.LogicRuleType == LogicRuleType.ValidationIf )
					{
						this.iValidationRules.Add( vr );
					}
					else
					{
						this.iElseRules.Add( vr );
					}
					break;
				}
				case sf.ObjectType.FillMap:
				{
					FillMap fm = (FillMap)child;

					fm.EventBinding = ControlEvents.None;

					if( fm.FillMapType == FillMapType.FillMapIf )
					{
						this.iFillMaps.Add( fm );
					}
					else
					{
						this.iElseMaps.Add( fm );
					}
					break;
				}
			}

			((IDatabaseObject)child).RecordUpsert();
		}
		public void RemoveChildObject(Suplex.Forms.IObjectModel child)
		{
			switch( child.ObjectType )
			{
				case sf.ObjectType.ValidationRule:
				{
					this.iValidationRules.Remove( (ValidationRule)child );
					break;
				}
				case sf.ObjectType.FillMap:
				{
					this.iFillMaps.Remove( (FillMap)child );
					break;
				}
			}
		}
		#endregion


		#region ICloneable<ValidationRule> Members
		public sf.IObjectModel Clone(bool generateNewId)
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
			ValidationRule clone = (ValidationRule)base.Clone( sf.ObjectType.ValidationRule );
			clone.iErrorControl = this.iErrorControl;
			clone.EventBinding = this.EventBinding;
			clone.ParentObject = this.ParentObject;

			if( (cloneDepth & ObjectType.ValidationRule) == ObjectType.ValidationRule )
			{
				foreach( ValidationRule vr in this.iValidationRules )
				{
					if( cloneChildrenAsRef )
					{
						clone.iValidationRules.Add( vr );
					}
					else
					{
						clone.iValidationRules.Add( vr.Clone( cloneDepth, cloneChildrenAsRef ) );
					}
				}
			}

			if( (cloneDepth & ObjectType.FillMap) == ObjectType.FillMap )
			{
				foreach( FillMap fm in this.iFillMaps )
				{
					if( cloneChildrenAsRef )
					{
						clone.iFillMaps.Add( fm );
					}
					else
					{
						clone.iFillMaps.Add( fm.Clone() );
					}
				}
			}

			if( (cloneDepth & ObjectType.ElseRule) == ObjectType.ElseRule )
			{
				foreach( ValidationRule vr in this.iElseRules )
				{
					if( cloneChildrenAsRef )
					{
						clone.iElseRules.Add( vr );
					}
					else
					{
						clone.iElseRules.Add( vr.Clone( cloneDepth, cloneChildrenAsRef ) );
					}
				}
			}

			if( (cloneDepth & ObjectType.ElseMap) == ObjectType.ElseMap )
			{
				foreach( FillMap fm in this.iElseMaps )
				{
					if( cloneChildrenAsRef )
					{
						clone.iElseMaps.Add( fm );
					}
					else
					{
						clone.iElseMaps.Add( fm.Clone() );
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
			this.iErrorControl = sourceObject.iErrorControl;
			this.EventBinding = sourceObject.EventBinding;

			if( (cloneDepth & ObjectType.ValidationRule) == ObjectType.ValidationRule )
			{
				foreach( ValidationRule rule in sourceObject.iValidationRules )
				{
					if( cloneChildrenAsRef )
					{
						this.iValidationRules.Add( rule );
					}
					else
					{
						ValidationRule found = this.iValidationRules.GetByValidationRuleId( rule.Id );
						if( found != null )
						{
							found.Synchronize( rule, cloneDepth, cloneChildrenAsRef );
						}
						else
						{
							this.iValidationRules.Add( rule.Clone( cloneDepth, cloneChildrenAsRef ) );
						}
					}
				}
			}
			if( (cloneDepth & ObjectType.FillMap) == ObjectType.FillMap )
			{
				foreach( FillMap fm in sourceObject.iFillMaps )
				{
					if( cloneChildrenAsRef )
					{
						this.iFillMaps.Add( fm );
					}
					else
					{
						FillMap found = this.iFillMaps.GetByFillMapId( fm.Id );
						if( found != null )
						{
							found.Synchronize( fm );
						}
						else
						{
							this.iFillMaps.Add( fm.Clone() );
						}
					}
				}
			}
			if( (cloneDepth & ObjectType.ElseRule) == ObjectType.ElseRule )
			{
				foreach( ValidationRule rule in sourceObject.iElseRules )
				{
					if( cloneChildrenAsRef )
					{
						this.iElseRules.Add( rule );
					}
					else
					{
						ValidationRule found = this.iElseRules.GetByValidationRuleId( rule.Id );
						if( found != null )
						{
							found.Synchronize( rule, cloneDepth, cloneChildrenAsRef );
						}
						else
						{
							this.iElseRules.Add( rule.Clone( cloneDepth, cloneChildrenAsRef ) );
						}
					}
				}
			}
			if( (cloneDepth & ObjectType.ElseMap) == ObjectType.ElseMap )
			{
				foreach( FillMap fm in sourceObject.iElseMaps )
				{
					if( cloneChildrenAsRef )
					{
						this.iElseMaps.Add( fm );
					}
					else
					{
						FillMap found = this.iElseMaps.GetByFillMapId( fm.Id );
						if( found != null )
						{
							found.Synchronize( fm );
						}
						else
						{
							this.iElseMaps.Add( fm.Clone() );
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

				if( this.WantsChildValidationRulesLazyLoad )
				{
					iValidationRules.Clear();
					DataSet ds =
						StoreConnection.da.GetDataSet( "splx_api_sel_vrbyparent_composite",
						new sSortedList( "@SPLX_VALIDATION_RULE_ID", this.Id ) );
					StoreConnection.da.NameTablesFromCompositeSelect( ref ds );
					iValidationRules.LoadFromDataTable( ds, null, this.Id.ToString(), LogicRuleType.ValidationIf, true );
					iElseRules.LoadFromDataTable( ds, null, this.Id.ToString(), LogicRuleType.ValidationElse, true );
				}

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

		#region IDatabaseObject Members
		public void RecordSelectWithChildren()
		{
			if( StoreConnection.IsConnected )
			{
				DataSet ds = StoreConnection.da.GetDataSet( "splx_api_sel_vr_withchildren_composite",
					new sSortedList( "@SPLX_VALIDATION_RULE_ID", this.Id ) );
				StoreConnection.da.NameTablesFromCompositeSelect( ref ds );

				if( ds.Tables["ValidationRules"].Rows.Count > 0 )
				{
					this.EnableLazyLoad = false;
					iValidationRules.LoadFromDataTable( ds, null, this.Id.ToString(), LogicRuleType.ValidationIf, false );
					iElseRules.LoadFromDataTable( ds, null, this.Id.ToString(), LogicRuleType.ValidationElse, false );
					iFillMaps.LoadFromDataTable( ds.Tables["FillMaps"], FillMapType.FillMapIf, ds.Tables["DataBindings"], this.Id.ToString() );
					iElseMaps.LoadFromDataTable( ds.Tables["FillMaps"], FillMapType.FillMapElse, ds.Tables["DataBindings"], this.Id.ToString() );
				}
				else
				{
					throw new RowNotInTableException( string.Format( "Unable to fetch ValidationRule '{0}' from the data store.", this.Name ) );
				}
			}
		}

		public override void RecordSelect()
		{
			if( StoreConnection.IsConnected )
			{
				DataSet ds = StoreConnection.da.GetDataSet( "splx_api_sel_vr",
					new sSortedList( "@SPLX_VALIDATION_RULE_ID", this.Id ) );

				if( ds.Tables[0].Rows.Count > 0 )
				{
					this.LoadFromDataRow( ds.Tables[0].Rows[0] );
				}
				else
				{
					throw new RowNotInTableException( string.Format( "Unable to fetch ValidationRule '{0}' from the data store.", this.Name ) );
				}
			}
		}

		private void LoadFromDataRow(DataRow r)
		{
			Id = new Guid( r["SPLX_VALIDATION_RULE_ID"].ToString() );
			Name = r["VR_NAME"].ToString();
			SortOrder = (int)r["VR_SORT_ORDER"];

			EventBinding = Convert.IsDBNull( r["VR_EVENT_BINDING"] ) ? sf.ControlEvents.None :
				sg.MiscUtils.ParseEnum<sf.ControlEvents>( r["VR_EVENT_BINDING"] );

			iCompareValue1 = r["VR_COMPARE_VALUE1"].ToString();

			ValueType1 = Convert.IsDBNull( r["VR_VALUE_TYPE1"] ) ? sf.ComparisonValueType.Empty :
				sg.MiscUtils.ParseEnum<sf.ComparisonValueType>( r["VR_VALUE_TYPE1"] );

			ExpressionType1 = Convert.IsDBNull( r["VR_EXPRESSION_TYPE1"] ) ? sf.ExpressionType.None :
				sg.MiscUtils.ParseEnum<sf.ExpressionType>( r["VR_EXPRESSION_TYPE1"] );

			iCompareValue2 = r["VR_COMPARE_VALUE2"].ToString();

			ValueType2 = Convert.IsDBNull( r["VR_VALUE_TYPE2"] ) ? sf.ComparisonValueType.Empty :
				sg.MiscUtils.ParseEnum<ComparisonValueType>( r["VR_VALUE_TYPE2"] );

			ExpressionType2 = Convert.IsDBNull( r["VR_EXPRESSION_TYPE2"] ) ? sf.ExpressionType.None :
				sg.MiscUtils.ParseEnum<sf.ExpressionType>( r["VR_EXPRESSION_TYPE2"] );

			CompareDataType = Convert.IsDBNull( r["VR_COMPARE_DATA_TYPE"] ) ? TypeCode.Empty :
				sg.MiscUtils.ParseEnum<TypeCode>( r["VR_COMPARE_DATA_TYPE"] );

			Operator = Convert.IsDBNull( r["VR_OPERATOR"] ) ? sf.ComparisonOperator.Empty :
				sg.MiscUtils.ParseEnum<sf.ComparisonOperator>( r["VR_OPERATOR"] );

			FailParent = (bool)r["VR_FAIL_PARENT"];
			iErrorControl = r["VR_ERROR_UIE_UNIQUE_NAME"].ToString();
			ErrorMessage = r["VR_ERROR_MESSAGE"].ToString();

			LogicRuleType = sg.MiscUtils.ParseEnum<sf.LogicRuleType>( r["VR_RULE_TYPE"] );

			EnableLazyLoad = true;
		}

		#region not implemented
		public override void RecordUpsert()
		{
			Guid parentUIElementId = Guid.Empty;
			Guid parRuleId = Guid.Empty;

			if( this.ParentObject.ObjectType == ObjectType.UIElement )
			{
				parentUIElementId = ((UIElement)this.ParentObject).Id;
			}
			else
			{
				//recurse-up to find the UIElement parent
				IObjectModel parentObject = this.ParentObject;
				while( parentObject.ObjectType != ObjectType.UIElement )
				{
					parentObject = parentObject.ParentObject;
				}

				parentUIElementId = ((UIElement)parentObject).Id;
				parRuleId = ((LogicRule)this.ParentObject).Id;
			}

			this.RecordUpsert( parentUIElementId, parRuleId );
		}

		public void RecordUpsert(Guid parentUIElementId, Guid parRuleId)
		{
			if( StoreConnection.IsConnected )
			{
				SortedList inparms = this.GetParms( parentUIElementId, parRuleId );

				SqlParameter id = new SqlParameter( "@SPLX_VALIDATION_RULE_ID", SqlDbType.UniqueIdentifier );
				id.Value = this.Id;
				id.Direction = ParameterDirection.InputOutput;
				SortedList outparms = new sSortedList( "@SPLX_VALIDATION_RULE_ID", id );

				StoreConnection.da.ExecuteSP( "splx_api_upsert_vr", inparms, ref outparms );
				this.Id = (Guid)id.Value;

				this.IsDirty = false;
			}
		}

		private SortedList GetParms(Guid parentUIElementId, Guid parRuleId)
		{
			sSortedList s = new sSortedList( "@VR_NAME", this.Name );
			s.Add( "@VR_EVENT_BINDING", this.EventBinding.ToString() );
			s.Add( "@VR_SORT_ORDER", this.SortOrder );

			s.Add( "@VR_COMPARE_DATA_TYPE", this.CompareDataType.ToString() );
			s.Add( "@VR_OPERATOR", this.Operator.ToString() );

			s.Add( "@VR_COMPARE_VALUE1", this.iCompareValue1 );
			s.Add( "@VR_EXPRESSION_TYPE1", this.ExpressionType1.ToString() );
			s.Add( "@VR_VALUE_TYPE1", this.ValueType1.ToString() );

			s.Add( "@VR_COMPARE_VALUE2", this.iCompareValue2 );
			s.Add( "@VR_EXPRESSION_TYPE2", this.ExpressionType2.ToString() );
			s.Add( "@VR_VALUE_TYPE2", this.ValueType2.ToString() );

			s.Add( "@VR_FAIL_PARENT", this.FailParent );
			s.Add( "@VR_ERROR_MESSAGE", this.ErrorMessage );
			s.Add( "@VR_ERROR_UIE_UNIQUE_NAME", this.iErrorControl );

			s.Add( "@VR_RULE_TYPE", this.LogicRuleType.ToString() );
			s.Add( "@VR_PARENT_ID", parRuleId == Guid.Empty ? Convert.DBNull : parRuleId );
			s.Add( "@SPLX_UI_ELEMENT_ID", parentUIElementId );

			return s;
		}

		public void RecordUpsertForImport(ref SqlTransaction tr, Guid parentUIElementId, Guid parRuleId)
		{
			SortedList inparms = this.GetParms( parentUIElementId, parRuleId );

			SqlParameter id = new SqlParameter( "@SPLX_VALIDATION_RULE_ID", SqlDbType.UniqueIdentifier );
			id.Value = this.Id;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_VALIDATION_RULE_ID", id );

			StoreConnection.da.ExecuteSP( "splx_api_upsert_vr", inparms, ref outparms, false, tr );
			this.Id = (Guid)id.Value;

			foreach( ValidationRule rule in this.iValidationRules )
			{
				rule.RecordUpsertForImport( ref tr, parentUIElementId, this.Id );
			}
			foreach( ValidationRule rule in this.iElseRules )
			{
				rule.RecordUpsertForImport( ref tr, parentUIElementId, this.Id );
			}
			foreach( FillMap map in this.iFillMaps )
			{
				map.RecordUpsertForImport( ref tr );
			}
			foreach( FillMap map in this.iElseMaps )
			{
				map.RecordUpsertForImport( ref tr );
			}
		}

		//public override void RecordDelete()	//Implemented on LogicRule

		internal override void RecordDeleteRecursive(SqlTransaction tr)
		{
			//walk down the heirarchy to the bottom, delete on the way back up.
			foreach( ValidationRule vr in this.iValidationRules )
			{
				vr.RecordDeleteRecursive( tr );
			}

			//walk down the heirarchy to the bottom, delete on the way back up.
			foreach( ValidationRule vr in this.iElseRules )
			{
				vr.RecordDeleteRecursive( tr );
			}

			foreach( FillMap fm in this.iFillMaps )
			{
				fm.RecordDelete( tr );
			}
			foreach( FillMap fm in this.iElseMaps )
			{
				fm.RecordDelete( tr );
			}

			SortedList inparms = new sSortedList( "@SPLX_VALIDATION_RULE_ID", this.Id );
			StoreConnection.da.ExecuteSP( "splx_api_del_vr", inparms, false, tr );
		}
		#endregion

		[XmlIgnore()]
		internal bool EnableLazyLoad
		{
			get { return _enableLazyLoad; }
			set
			{
				if( _enableLazyLoad != value )
				{
					_enableLazyLoad = value;

					if( _enableLazyLoad )
					{
						if( StoreConnection.IsConnected && this.iValidationRules.Count == 0)
						{
							this.iValidationRules.Add( new LazyLoadDummyValidationRule() );
						}
					}
					else
					{
						if( this.WantsChildValidationRulesLazyLoad )
						{
							iValidationRules.Clear();
						}
					}
				}
			}
		}

		[XmlIgnore()]
		private bool WantsChildValidationRulesLazyLoad
		{
			get
			{
				return StoreConnection.IsConnected &&
					iValidationRules.Count > 0 && iValidationRules[0] is LazyLoadDummyValidationRule;
			}
		}
		[XmlIgnore()]
		private bool WantsDetailLazyLoad
		{
			get
			{
				return StoreConnection.IsConnected && _wantsDetailLazyLoad;
			}
			set
			{
				_wantsDetailLazyLoad = value;
			}
		}
		#endregion
	}

	public class LazyLoadDummyValidationRule : ValidationRule { }

	public class ValidationRuleCollection : ObservableObjectModelCollection<ValidationRule>
	{
		private string _uieIdFilter = null;
		private DataTable _rulesLoadTable = null;
		private DataTable _fillMapsLoadTable = null;
		private DataTable _dataBindingsTable = null;
		private bool _enableLazyLoad = true;

		public ValidationRuleCollection() : base() { }
		public ValidationRuleCollection(sf.IObjectModel owner)
			: base( owner )
		{ }

		public ValidationRule GetByValidationRuleId(Guid id)
		{
			return this.SingleOrDefault( vr => vr.Id == id );
		}

		internal void LoadFromDataTable(DataSet ds, string uieIdFilter, string parentRuleId, sf.LogicRuleType logicRuleType, bool enableLazyLoad)
		{
			_uieIdFilter = string.IsNullOrEmpty( uieIdFilter ) ? string.Empty :
				string.Format( " AND SPLX_UI_ELEMENT_ID = '{0}'", uieIdFilter );

			_enableLazyLoad = enableLazyLoad;

			_rulesLoadTable = ds.Tables["ValidationRules"];
			_fillMapsLoadTable = ds.Tables["FillMaps"];
			_dataBindingsTable = ds.Tables["DataBindings"];

			this.RecurseRules( parentRuleId, this, logicRuleType );

			_rulesLoadTable = null;
			_fillMapsLoadTable = null;
			_dataBindingsTable = null;
		}

		private void RecurseRules(string parentId, ValidationRuleCollection rules, sf.LogicRuleType logicRuleType)
		{
			DataRow[] rows = null;
			if( string.IsNullOrEmpty( parentId ) )
			{
				rows = _rulesLoadTable.Select(
					string.Format( "VR_PARENT_ID IS NULL AND VR_RULE_TYPE = '{0}'{1}", logicRuleType, _uieIdFilter ) );
			}
			else
			{
				rows = _rulesLoadTable.Select(
					string.Format( "VR_PARENT_ID = '{0}' AND VR_RULE_TYPE = '{1}'{2}", parentId, logicRuleType, _uieIdFilter ) );
			}

			foreach( DataRow r in rows )
			{
				ValidationRule vr = new ValidationRule()
				{
					Id = new Guid( r["SPLX_VALIDATION_RULE_ID"].ToString() ),
					Name = r["VR_NAME"].ToString(),
					SortOrder = (int)r["VR_SORT_ORDER"],

					EventBinding = Convert.IsDBNull( r["VR_EVENT_BINDING"] ) ? sf.ControlEvents.None :
						sg.MiscUtils.ParseEnum<sf.ControlEvents>( r["VR_EVENT_BINDING"] ),

					iCompareValue1 = r["VR_COMPARE_VALUE1"].ToString(),

					ValueType1 = Convert.IsDBNull( r["VR_VALUE_TYPE1"] ) ? sf.ComparisonValueType.Empty :
						sg.MiscUtils.ParseEnum<sf.ComparisonValueType>( r["VR_VALUE_TYPE1"] ),

					ExpressionType1 = Convert.IsDBNull( r["VR_EXPRESSION_TYPE1"] ) ? sf.ExpressionType.None :
						sg.MiscUtils.ParseEnum<sf.ExpressionType>( r["VR_EXPRESSION_TYPE1"] ),

					iCompareValue2 = r["VR_COMPARE_VALUE2"].ToString(),

					ValueType2 = Convert.IsDBNull( r["VR_VALUE_TYPE2"] ) ? sf.ComparisonValueType.Empty :
						sg.MiscUtils.ParseEnum<ComparisonValueType>( r["VR_VALUE_TYPE2"] ),

					ExpressionType2 = Convert.IsDBNull( r["VR_EXPRESSION_TYPE2"] ) ? sf.ExpressionType.None :
						sg.MiscUtils.ParseEnum<sf.ExpressionType>( r["VR_EXPRESSION_TYPE2"] ),

					CompareDataType = Convert.IsDBNull( r["VR_COMPARE_DATA_TYPE"] ) ? TypeCode.Empty :
						sg.MiscUtils.ParseEnum<TypeCode>( r["VR_COMPARE_DATA_TYPE"] ),

					Operator = Convert.IsDBNull( r["VR_OPERATOR"] ) ? sf.ComparisonOperator.Empty :
						sg.MiscUtils.ParseEnum<sf.ComparisonOperator>( r["VR_OPERATOR"] ),

					FailParent = (bool)r["VR_FAIL_PARENT"],
					iErrorControl = r["VR_ERROR_UIE_UNIQUE_NAME"].ToString(),
					ErrorMessage = r["VR_ERROR_MESSAGE"].ToString(),

					LogicRuleType = logicRuleType,

					EnableLazyLoad = _enableLazyLoad
				};

				//seriously, wtf is this (fixed, i think, 04072010)
				//seriously, wtf is this (fixed, i think, 08082010)
				//	[SPLX_UIE_VR_PARENT_ID] was [SPLX_UI_ELEMENT_RULE_ID] and {0} wasn't quoted
				DataRow[] maps = _fillMapsLoadTable.Select(
					string.Format( "SPLX_UIE_VR_PARENT_ID = '{0}' AND FME_IF_CLAUSE = '{1}'",
					r["SPLX_VALIDATION_RULE_ID"].ToString(), true ) );
				vr.iFillMaps.LoadFromDataRows( maps, sf.FillMapType.FillMapIf, _dataBindingsTable );

				maps = _fillMapsLoadTable.Select(
					string.Format( "SPLX_UIE_VR_PARENT_ID = '{0}' AND FME_IF_CLAUSE = '{1}'",
					r["SPLX_VALIDATION_RULE_ID"].ToString(), false ) );
				vr.iElseMaps.LoadFromDataRows( maps, sf.FillMapType.FillMapElse, _dataBindingsTable );

				ValidationRule exists = rules.GetByValidationRuleId( vr.Id );
				if( exists == null )
				{
					exists = vr;
					rules.Add( exists );
				}
				else
				{
					exists.Synchronize( vr, ValidationRule.CloneDeep, false );
				}

				this.RecurseRules( r["SPLX_VALIDATION_RULE_ID"].ToString(), exists.iValidationRules, LogicRuleType.ValidationIf );
				this.RecurseRules( r["SPLX_VALIDATION_RULE_ID"].ToString(), exists.iElseRules, LogicRuleType.ValidationElse );
			}
		}
	}

	public class FillMap : sf.IFillMap, sf.IObjectModel, ICloneable<FillMap>,
		INotifyPropertyChanged, INotifyPropertyChanging, INodeItem, IDatabaseObject, INotifyDeleted
	{
		private string _name = string.Empty;
		private sf.ControlEvents _eventBinding = ControlEvents.Validating;
		private int _sortOrder = 0;
		private bool _isDirty = false;

		private long _nextId = System.DateTime.Now.Ticks;
		private long NextId { get { return _nextId++; } }

		public FillMap()
		{
			this.Id = this.NextId;

			this.iDataBindings = new DataBindingCollectionEx<DataBinding>( this );
			//this.iDataBindingsTemp = new DataBindingCollectionEx<DataBinding>( this );
			this.ExprElements = new sf.ExpressionElements( null, sf.ExpressionType.None );

			this.iDataBindings.CollectionChanged += new NotifyCollectionChangedEventHandler( this.Child_CollectionChanged );
		}

		void Child_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.IsDirty = true;
			this.OnPropertyChanged( "Child_CollectionChanged" );
		}

		public FillMapType FillMapType { get; set; }

		public long Id { get; set; }

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

		[XmlIgnore()]
		public sf.ExpressionElements ExprElements { get; set; }
		public string Expression
		{
			get { return this.ExprElements.Expression; }
			set
			{
				if( this.ExprElements.Expression != value )
				{
					this.ExprElements.Expression = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Expression" );
				}
			}
		}
		public sf.ExpressionType ExpressionType
		{
			get { return this.ExprElements.ExprType; }
			set
			{
				if( this.ExprElements.ExprType != value )
				{
					this.ExprElements.ExprType = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "ExpressionType" );
				}
			}
		}

		[XmlIgnore()]
		public sf.DataBindingCollection DataBindings { get; set; }
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
		[XmlIgnore()]
		public object Data { get; set; }

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

		//[XmlIgnore()]
		//public DataBindingCollection iDataBindings { get; set; }	Temp

		/// <summary>
		///this is a work-around prop for iDataBindings b/c:
		/////a) i cant figure out how to make the UI not update iDataBindings automatically
		///b) the WpfToolkit DataGrid has a limitation that requires a generic argument
		///	  to provide a blank row for an empty collection
		/// </summary>
		[XmlArray( "DataBindings" )]
		public DataBindingCollectionEx<DataBinding> iDataBindings { get; set; }

		#region IObjectModel Members
		public sf.ObjectType ObjectType { get { return sf.ObjectType.FillMap; } }
		public sf.ObjectType ValidChildObjectTypes { get { return sf.ObjectType.None; } }
		public bool SupportsChildObjectType(sf.ObjectType objectType)
		{
			return (this.ValidChildObjectTypes & objectType) == objectType;
		}
		[XmlIgnore()]
		public sf.IObjectModel ParentObject { get; set; }
		#endregion

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

		#region ICloneable<FillMap> Members
		public sf.IObjectModel Clone(bool generateNewId)
		{
			FillMap clone = new FillMap();

			clone.Id = generateNewId ? this.NextId : this.Id;
			clone.Name = this.Name;
			clone.ExprElements = new sf.ExpressionElements( this.ExprElements.Expression, this.ExprElements.ExprType );
			clone.EventBinding = this.EventBinding;
			clone.SortOrder = this.SortOrder;
			clone.ParentObject = this.ParentObject;
			clone.FillMapType = this.FillMapType;

			foreach( DataBinding d in this.iDataBindings )
			{
				clone.iDataBindings.Add( d.Clone( generateNewId ) );
			}

			return clone;
		}

		public FillMap Clone()
		{
			return (FillMap)this.Clone( false );
		}

		FillMap ICloneable<FillMap>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		private List<long> _deleteIds = null;
		public void Synchronize(FillMap sourceObject)
		{
			this.Name = sourceObject.Name;
			this.Expression = sourceObject.Expression;
			this.ExpressionType = sourceObject.ExpressionType;
			this.EventBinding = sourceObject.EventBinding;
			this.SortOrder = sourceObject.SortOrder;

			//purge the DataBindings from |this| list that no longer exist in the sourceObject's DataBinding list.
			_deleteIds = new List<long>();	//this might be a HACK
			for( int i = this.iDataBindings.Count - 1; i > -1; i-- )
			{
				DataBinding found = sourceObject.iDataBindings.GetByDataBindingId( this.iDataBindings[i].Id );
				if( found == null )
				{
					_deleteIds.Add( this.iDataBindings[i].Id );
					this.iDataBindings.RemoveAt( i );
				}
			}
			foreach( DataBinding d in sourceObject.iDataBindings )
			{
				DataBinding found = this.iDataBindings.GetByDataBindingId( d.Id );
				if( found != null )
				{
					found.Synchronize( d );
				}
				else
				{
					this.iDataBindings.Add( d.Clone() );
				}
			}
			//foreach( DataBinding d in sourceObject.iDataBindingsTemp )
			//{
			//    DataBinding found = this.iDataBindingsTemp.GetByDataBindingId( d.Id );
			//    if( found != null )
			//    {
			//        found.Synchronize( d );
			//    }
			//    else
			//    {
			//        this.iDataBindingsTemp.Add( d.Clone() );
			//    }
			//}
		}

		//This is a special sync only to be used after a database Create/Update statement.
		//It's needed to sync the Ids on Child collections - DataBindings, in this case
		public void SynchronizeSpecial(FillMap sourceObject)
		{
			if( StoreConnection.IsConnected )
			{
				for( int i = this.iDataBindings.Count - 1; i > -1; i-- )
				{
					DataBinding found = sourceObject.iDataBindings.GetByDataBindingId( this.iDataBindings[i].Id );
					if( found == null )
					{
						this.iDataBindings.RemoveAt( i );
					}
				}

				foreach( DataBinding d in sourceObject.iDataBindings )
				{
					DataBinding found = this.iDataBindings.GetByDataBindingId( d.Id );
					if( found != null )
					{
						found.Synchronize( d );
						found.IsDirty = false;
					}
					else
					{
						this.iDataBindings.Add( d.Clone() );
					}
				}

				this.IsDirty = false;
			}
		}

		void ICloneable<FillMap>.Synchronize(FillMap sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
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

		#region INodeItem Members
		private bool _isExpanded = false;
		private bool _isSelected = false;
		private bool _isEditing = false;
		private bool _showDetail = false;
		private bool _showDetailPanels = false;
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
		CompositeCollection INodeItem.ChildObjects
		{
			get { return null; }
		}

		#endregion

		#region IDatabaseObject Members
		public void RecordSelect()
		{
			if( StoreConnection.IsConnected )
			{
				DataSet ds = StoreConnection.da.GetDataSet( "splx_api_sel_fmbyid_composite",
					new sSortedList( "@SPLX_FILLMAP_EXPRESSION_ID", this.Id ) );
				StoreConnection.da.NameTablesFromCompositeSelect( ref ds );

				if( ds.Tables["FillMaps"].Rows.Count > 0 )
				{
					this.LoadFromDataRow( ds.Tables["FillMaps"].Rows[0] );
					this.iDataBindings.LoadFromDataTable( ds.Tables["Databindings"] );
					this.IsDirty = false;
				}
				else
				{
					throw new RowNotInTableException( string.Format( "Unable to fetch FillMap '{0}' from the data store.", this.Name ) );
				}
			}
		}

		private void LoadFromDataRow(DataRow r)
		{
			Id = (int)r["SPLX_FILLMAP_EXPRESSION_ID"];

			Name = r["FME_NAME"].ToString();

			EventBinding = Convert.IsDBNull( r["FME_EVENT_BINDING"] ) ? sf.ControlEvents.None :
				sg.MiscUtils.ParseEnum<sf.ControlEvents>( r["FME_EVENT_BINDING"] );

			Expression = Convert.IsDBNull( r["FME_EXPRESSION"] ) ? string.Empty :
				r["FME_EXPRESSION"].ToString();

			ExpressionType = Convert.IsDBNull( r["FME_EXPRESSION_TYPE"] ) ? sf.ExpressionType.None :
				sg.MiscUtils.ParseEnum<sf.ExpressionType>( r["FME_EXPRESSION_TYPE"] );

			FillMapType = (bool)r["FME_IF_CLAUSE"] ? sf.FillMapType.FillMapIf :
				sf.FillMapType.FillMapElse;

			SortOrder = (int)r["FME_SORT_ORDER"];
		}

		public void RecordUpsert()
		{
			if( StoreConnection.IsConnected )
			{
				SortedList inparms = this.GetParms();

				int idAsInt = -1;
				Int32.TryParse( this.Id.ToString(), out idAsInt );

				SqlParameter id = new SqlParameter( "@SPLX_FILLMAP_EXPRESSION_ID", SqlDbType.Int );
				id.Value = idAsInt;
				id.Direction = ParameterDirection.InputOutput;
				SortedList outparms = new sSortedList( "@SPLX_FILLMAP_EXPRESSION_ID", id );

				StoreConnection.da.OpenConnection();
				SqlTransaction tr = StoreConnection.da.Connection.BeginTransaction();
				try
				{
					StoreConnection.da.ExecuteSP( "splx_api_upsert_fme", inparms, ref outparms, false, tr );
					this.Id = (int)id.Value;

					StringBuilder dbIds = new StringBuilder();
					foreach( DataBinding db in this.iDataBindings )
					{
						if( db.IsDirty )
						{
							dbIds.AppendFormat( "{0},", db.Id );
							db.RecordUpsert( this.Id, ref tr );
							db.IsDirty = false;
						}
					}
					if( _deleteIds != null && _deleteIds.Count > 0 )
					{
						foreach( int fmbId in _deleteIds )
						{
							StoreConnection.da.ExecuteSP( "splx_api_del_fmb", new sSortedList( "@SPLX_FILLMAP_DATABINDING_ID", fmbId ), false, tr );
						}
					}
					//8/8/2010 not sure what this else block is for, and the sp doesn't exist...
					else
					{
						string idList = dbIds.ToString();
						if( !string.IsNullOrEmpty( idList ) )
						{
							idList = idList.Substring( 0, idList.Length - 1 );
							//StoreConnection.da.ExecuteSP( "splx_api_del_fmb_byidlist", new sSortedList( "@SPLX_FILLMAP_DATABINDING_ID_LIST", idList ), false, tr );
						}
					}
					_deleteIds = null;

					tr.Commit();

					this.IsDirty = false;
				}
				catch( Exception ex )
				{
					tr.Rollback();
					throw ex;
				}
				finally
				{
					StoreConnection.da.CloseConnection();
				}
			}
		}

		public void RecordUpsertForImport(ref SqlTransaction tr)
		{
			SortedList inparms = this.GetParms();

			int idAsInt = -1;
			Int32.TryParse( this.Id.ToString(), out idAsInt );

			SqlParameter id = new SqlParameter( "@SPLX_FILLMAP_EXPRESSION_ID", SqlDbType.Int );
			id.Value = idAsInt;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_FILLMAP_EXPRESSION_ID", id );

			StoreConnection.da.ExecuteSP( "splx_api_upsert_fme", inparms, ref outparms, false, tr );
			this.Id = (int)id.Value;

			foreach( DataBinding db in this.iDataBindings )
			{
				db.RecordUpsert( this.Id, ref tr );
			}
		}

		private SortedList GetParms()
		{
			sSortedList s = new sSortedList( "@FME_NAME", this.Name );

			s.Add( "@FME_EVENT_BINDING", this.EventBinding.ToString() );

			s.Add( "@FME_EXPRESSION", string.IsNullOrEmpty( this.Expression ) ?
				Convert.DBNull : this.Expression );

			s.Add( "@FME_EXPRESSION_TYPE", this.ExpressionType.ToString() );

			s.Add( "@FME_IF_CLAUSE", this.FillMapType == FillMapType.FillMapIf );

			s.Add( "@FME_SORT_ORDER", this.SortOrder );

			Guid parId = this.ParentObject.ObjectType == ObjectType.UIElement ?
				((UIElement)this.ParentObject).Id : ((ValidationRule)this.ParentObject).Id;
			s.Add( "@SPLX_UIE_VR_PARENT_ID", parId );

			return s;
		}

		public void RecordDelete()
		{
			if( StoreConnection.IsConnected )
			{
				SortedList inparms = new sSortedList( "@SPLX_FILLMAP_EXPRESSION_ID", this.Id );
				StoreConnection.da.ExecuteSP( "splx_api_del_fme", inparms );
			}
		}

		internal void RecordDelete(SqlTransaction tr)
		{
			SortedList inparms = new sSortedList( "@SPLX_FILLMAP_EXPRESSION_ID", this.Id );
			StoreConnection.da.ExecuteSP( "splx_api_del_fme", inparms, false, tr );
		}
		#endregion

		#region INotifyDeleted Members
		private bool _isDeleted = false;
		[XmlIgnore()]
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
	}

	public class LazyLoadDummyFillMap : FillMap { }

	public class FillMapCollection : ObservableObjectModelCollection<FillMap>
	{
		public FillMapCollection() : base() { }
		public FillMapCollection(sf.IObjectModel owner)
			: base( owner )
		{ }

		public FillMap GetByFillMapId(long id)
		{
			return this.SingleOrDefault( fm => fm.Id == id );
		}

		internal void LoadFromDataTable(DataTable mapTable, FillMapType fillMapType, DataTable dataBindingTable, string uieVrIdFilter)
		{
			//if( !append ) { this.Clear(); }

			this.Clear();	//TODO: 09/18/2010 verify this

			string idFilter = string.IsNullOrEmpty( uieVrIdFilter ) ? string.Empty :
				string.Format( " AND SPLX_UIE_VR_PARENT_ID = '{0}'", uieVrIdFilter );



			FillMap fm = null;
			DataRow[] rows = mapTable.Select( string.Format( "FME_IF_CLAUSE = {0}{1}", fillMapType == FillMapType.FillMapIf, idFilter ) );
			foreach( DataRow r in rows )
			{
				fm =
					new FillMap()
					{
						Id = (int)(r["SPLX_FILLMAP_EXPRESSION_ID"]),
						Name = (string)r["FME_NAME"],
						EventBinding = Convert.IsDBNull( r["FME_EVENT_BINDING"] ) ?
							sf.ControlEvents.None : sg.MiscUtils.ParseEnum<sf.ControlEvents>( r["FME_EVENT_BINDING"], true ),
						Expression = r["FME_EXPRESSION"].ToString(),
						ExpressionType = Convert.IsDBNull( r["FME_EXPRESSION_TYPE"] ) ?
							ExpressionType.None : sg.MiscUtils.ParseEnum<sf.ExpressionType>( r["FME_EXPRESSION_TYPE"], true ),
						SortOrder = (int)r["FME_SORT_ORDER"],
						FillMapType = fillMapType,

						IsDirty = false
					};

				DataRow[] bindings = dataBindingTable.Select(
					string.Format( "SPLX_FILLMAP_EXPRESSION_ID = {0}", r["SPLX_FILLMAP_EXPRESSION_ID"].ToString() ) );
				fm.iDataBindings.LoadFromDataRows( bindings );

				FillMap exists = this.GetByFillMapId( fm.Id );
				if( exists == null )
				{
					this.Add( fm );
				}
				else
				{
					exists.Synchronize( fm );
				}
			}
		}

		internal void LoadFromDataRows(DataRow[] maps, FillMapType fillMapType, DataTable dataBindingTable)
		{
			this.Clear();	//TODO: 09/18/2010 verify this

			FillMap fm = null;
			foreach( DataRow r in maps )
			{
				fm =
					new FillMap()
					{
						Id = (int)(r["SPLX_FILLMAP_EXPRESSION_ID"]),
						Name = (string)r["FME_NAME"],
						EventBinding = Convert.IsDBNull( r["FME_EVENT_BINDING"] ) ?
							sf.ControlEvents.None : sg.MiscUtils.ParseEnum<sf.ControlEvents>( r["FME_EVENT_BINDING"], true ),
						Expression = r["FME_EXPRESSION"].ToString(),
						ExpressionType = Convert.IsDBNull( r["FME_EXPRESSION_TYPE"] ) ?
							ExpressionType.None : sg.MiscUtils.ParseEnum<sf.ExpressionType>( r["FME_EXPRESSION_TYPE"], true ),
						SortOrder = (int)r["FME_SORT_ORDER"],
						FillMapType = fillMapType,

						IsDirty = false
					};

				DataRow[] bindings = dataBindingTable.Select(
					string.Format( "SPLX_FILLMAP_EXPRESSION_ID = {0}", r["SPLX_FILLMAP_EXPRESSION_ID"].ToString() ) );
				fm.iDataBindings.LoadFromDataRows( bindings );

				this.Add( fm );
			}
		}
	}

	public class DataBinding : sf.IDataBinding, INotifyPropertyChanged, ICloneable<DataBinding>
	{
		private string _controlName = string.Empty;
		private string _propertyName = string.Empty;
		private string _iDataMember = string.Empty;
		private bool _overrideValue = false;
		private bool _isDirty = false;

		private long _nextId = System.DateTime.Now.Ticks;
		private long NextId { get { return _nextId++; } }

		public DataBinding()
		{
			this.Id = this.NextId;
		}

		public long Id { get; set; }

		[XmlIgnore]
		public sf.IValidationControl ControlRef { get; set; }

		public string ControlName
		{
			get { return _controlName; }
			set
			{
				if( _controlName != value )
				{
					_controlName = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "ControlName" );
				}
			}
		}
		public string PropertyName
		{
			get { return _propertyName; }
			set
			{
				if( _propertyName != value )
				{
					_propertyName = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "PropertyName" );
				}
			}
		}
		[XmlIgnore]
		public object DataMember { get; set; }
		[XmlElement( "Value" )]
		public string iDataMember
		{
			get { return _iDataMember; }
			set
			{
				if( _iDataMember != value )
				{
					_iDataMember = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "iDataMember" );
				}
			}
		}
		[XmlIgnore]
		public bool ConversionRequired { get; set; }
		public bool OverrideValue
		{
			get { return _overrideValue; }
			set
			{
				if( _overrideValue != value )
				{
					_overrideValue = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "OverrideValue" );
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

		public override string ToString()
		{
			return string.Format( "Id: {4}, ControlName: {0}, PropertyName: {1}, iDataMember: {2}, OverrideValue: {3}",
				this.ControlName, this.PropertyName, this.iDataMember, this.OverrideValue, this.Id );
		}

		#region ICloneable<DataBinding> Members
		sf.IObjectModel ICloneableObject.Clone(bool generateNewId)
		{
			throw new NotImplementedException();
		}
		public DataBinding Clone(bool generateNewId)
		{
			DataBinding clone = this.Clone();
			if( generateNewId )
			{
				clone.Id = this.NextId;
			}
			return clone;
		}

		public DataBinding Clone()
		{
			DataBinding d = new DataBinding();

			d.Id = this.Id;
			d.ControlName = this.ControlName;
			d.PropertyName = this.PropertyName;
			d.iDataMember = this.iDataMember;
			d.OverrideValue = this.OverrideValue;

			return d;
		}

		DataBinding ICloneable<DataBinding>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		public void Synchronize(DataBinding sourceObject)
		{
			this.Id = sourceObject.Id;
			this.ControlName = sourceObject.ControlName;
			this.PropertyName = sourceObject.PropertyName;
			this.iDataMember = sourceObject.iDataMember;
			this.OverrideValue = sourceObject.OverrideValue;
		}

		void ICloneable<DataBinding>.Synchronize(DataBinding sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region INotifyPropertyChanged Members
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
			}
		}
		#endregion

		internal void RecordUpsert(long fmeId, ref SqlTransaction tr)
		{
			SortedList inparms = this.GetParms( fmeId );

			int idAsInt = -1;
			Int32.TryParse( this.Id.ToString(), out idAsInt );

			SqlParameter id = new SqlParameter( "@SPLX_FILLMAP_DATABINDING_ID", SqlDbType.Int );
			id.Value = idAsInt;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_FILLMAP_DATABINDING_ID", id );

			StoreConnection.da.ExecuteSP( "splx_api_upsert_fmb", inparms, ref outparms, false, tr );
			this.Id = (int)id.Value;

			this.IsDirty = false;
		}

		private SortedList GetParms(long fmeId)
		{
			sSortedList s = new sSortedList( "@FMB_UIE_UNIQUE_NAME", this.ControlName );

			s.Add( "@FMB_PROPERTY_NAME", this.PropertyName );
			s.Add( "@FMB_VALUE", this.iDataMember );
			s.Add( "@FMB_TYPECAST_VALUE", this.ConversionRequired );
			s.Add( "@FMB_OVERRIDE_VALUE", this.OverrideValue );
			s.Add( "@SPLX_FILLMAP_EXPRESSION_ID", fmeId );

			return s;
		}
	}

	public class DataBindingCollection : ObservableObjectModelCollection<DataBinding>
	{
		public DataBindingCollection() : base() { }
		public DataBindingCollection(sf.IObjectModel owner)
			: base( owner )
		{ }

		public DataBinding GetByDataBindingId(long id)
		{
			return this.SingleOrDefault( db => db.Id == id );
		}
	}

	public class DataBindingCollectionEx<T> : DataBindingCollection where T : DataBinding
	{
		public DataBindingCollectionEx() : base() { }
		public DataBindingCollectionEx(sf.IObjectModel owner)
			: base( owner )
		{ }

		internal void LoadFromDataTable(DataTable bindings)
		{
			//foreach( DataRow r in bindings.Rows )
			//{
			//    this.Add(
			//        new DataBinding()
			//        {
			//            Id = (int)(r["SPLX_FILLMAP_DATABINDING_ID"]),
			//            ControlName = (string)r["FMB_UIE_UNIQUE_NAME"],
			//            PropertyName = r["FMB_PROPERTY_NAME"].ToString(),
			//            iDataMember = r["FMB_VALUE"].ToString(),
			//            ConversionRequired = (bool)r["FMB_TYPECAST_VALUE"],
			//            OverrideValue = (bool)r["FMB_OVERRIDE_VALUE"],

			//            IsDirty = false
			//        }
			//    );
			//}

			this.LoadFromDataRows( bindings.Select() );
		}

		internal void LoadFromDataRows(DataRow[] bindings)
		{
			this.Clear();	//TODO: 09/18/2010 verify this

			DataBinding db = null;
			foreach( DataRow r in bindings )
			{
				db = new DataBinding()
				{
					Id = (int)(r["SPLX_FILLMAP_DATABINDING_ID"]),
					ControlName = (string)r["FMB_UIE_UNIQUE_NAME"],
					PropertyName = r["FMB_PROPERTY_NAME"].ToString(),
					iDataMember = r["FMB_VALUE"].ToString(),
					ConversionRequired = (bool)r["FMB_TYPECAST_VALUE"],
					OverrideValue = (bool)r["FMB_OVERRIDE_VALUE"],

					IsDirty = false
				};

				DataBinding exists = this.GetByDataBindingId( db.Id );
				if( exists == null )
				{
					this.Add( db );
				}
				else
				{
					exists.Synchronize( db );
					exists.IsDirty = false;
				}
			}
		}
	}
	#endregion


	#region Security Structures
	public abstract class SecurityPrincipalBase :
		st.ISecurityPrincipal, INotifyPropertyChanged, ICloneable<SecurityPrincipalBase>,
		INotifyDeleted, IDatabaseObject
	{
		private string _id = string.Empty;
		private string _name = string.Empty;
		private string _description = string.Empty;
		private bool _isLocal = false;
		private bool _isBuiltIn = false;
		private bool _isEnabled = false;
		private bool _isValid = false;
		private bool _isAnonymous = false;

		private bool _isDirty = false;
		private bool _isDeleted = false;

		#region ISecurityPrincipal Members
		public string Id
		{
			get { return _id; }
			set
			{
				if( _id != value )
				{
					_id = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Id" );
				}
			}
		}

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

		public string Description
		{
			get { return _description; }
			set
			{
				if( _description != value )
				{
					_description = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Description" );
				}
			}
		}

		public bool IsLocal
		{
			get { return _isLocal; }
			set
			{
				if( _isLocal != value )
				{
					_isLocal = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "IsLocal" );
				}
			}
		}

		public bool IsBuiltIn
		{
			get { return _isBuiltIn; }
			set
			{
				if( _isBuiltIn != value )
				{
					_isBuiltIn = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "IsBuiltIn" );
				}
			}
		}

		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				if( _isEnabled != value )
				{
					_isEnabled = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "IsEnabled" );
				}
			}
		}

		[XmlIgnore()]
		public virtual bool IsValid { get { return _isValid; } internal set { _isValid = value; } }
		[XmlIgnore()]
		public virtual bool IsAnonymous { get { return _isAnonymous; } internal set { _isAnonymous = value; } }
		[XmlIgnore()]
		public abstract bool IsUserObject { get; }
		#endregion

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

		[XmlIgnore()]
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

		#region INotifyPropertyChanged Members
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
			}
		}
		#endregion

		#region ICloneable<SecurityPrincipalBase> Members
		abstract public sf.IObjectModel Clone(bool generateNewId);

		SecurityPrincipalBase ICloneable<SecurityPrincipalBase>.Clone()
		{
			return (SecurityPrincipalBase)this.MemberwiseClone();
		}

		SecurityPrincipalBase ICloneable<SecurityPrincipalBase>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		void ICloneable<SecurityPrincipalBase>.Synchronize(SecurityPrincipalBase sourceObject)
		{
			this.Id = sourceObject.Id;
			this.Name = sourceObject.Name;
			this.Description = sourceObject.Description;
			this.IsLocal = sourceObject.IsLocal;
			this.IsBuiltIn = sourceObject.IsBuiltIn;
			this.IsEnabled = sourceObject.IsEnabled;
			this.IsValid = sourceObject.IsValid;
			this.IsAnonymous = sourceObject.IsAnonymous;
		}

		void ICloneable<SecurityPrincipalBase>.Synchronize(SecurityPrincipalBase sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region IDatabaseObject Members
		abstract public void RecordSelect();
		abstract public void RecordUpsert();
		abstract public void RecordUpsertForImport(ref SqlTransaction tr);
		abstract public void RecordDelete();
		#endregion
	}

	public class User : SecurityPrincipalBase, sf.IObjectModel, ICloneable<User>
	{
		public User()
			: base()
		{ }

		public User(DataRow r)
			: base()
		{
			this.LoadFromDataRow( r );
		}

		#region IDatabaseObject
		public override void RecordSelect()
		{
			if( StoreConnection.IsConnected )
			{
				DataSet ds = StoreConnection.da.GetDataSet( "splx_dal_sel_userbyid",
					new sSortedList( "@SPLX_USER_ID", this.Id ) );

				if( ds.Tables[0].Rows.Count > 0 )
				{
					this.LoadFromDataRow( ds.Tables[0].Rows[0] );
				}
				else
				{
					throw new RowNotInTableException( string.Format( "Unable to fetch User '{0}' from the data store.", this.Name ) );
				}
			}
		}
		public override void RecordUpsert()
		{
			if( StoreConnection.IsConnected )
			{
				SortedList inparms = this.GetParms();

				SqlParameter id = new SqlParameter( "@SPLX_USER_ID", SqlDbType.UniqueIdentifier );
				id.Value = new Guid( this.Id );
				id.Direction = ParameterDirection.InputOutput;
				SortedList outparms = new sSortedList( "@SPLX_USER_ID", id );

				StoreConnection.da.ExecuteSP( "splx_api_upsert_user", inparms, ref outparms );
				this.Id = id.Value.ToString();

				this.IsDirty = false;
			}
		}
		public override void RecordUpsertForImport(ref SqlTransaction tr)
		{
			SortedList inparms = this.GetParms();

			SqlParameter id = new SqlParameter( "@SPLX_USER_ID", SqlDbType.UniqueIdentifier );
			id.Value = new Guid( this.Id );
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_USER_ID", id );

			StoreConnection.da.ExecuteSP( "splx_api_upsert_user", inparms, ref outparms, false, tr );
			this.Id = id.Value.ToString();
		}
		public override void RecordDelete()
		{
			if( StoreConnection.IsConnected )
			{
				StoreConnection.da.ExecuteSP( "splx_api_del_user", new sSortedList( "@SPLX_USER_ID", this.Id ) );
				//this.IsDeleted = true;
			}
		}
		internal SortedList GetParms()
		{
			sSortedList s = new sSortedList( "@USER_NAME", this.Name );
			s.Add( "@USER_DESC", this.Description );
			s.Add( "@USER_LOCAL", this.IsLocal );
			s.Add( "@USER_ENABLED", this.IsEnabled );

			return s;
		}
		internal void LoadFromDataRow(DataRow r)
		{
			Id = r["SPLX_USER_ID"].ToString();
			Name = (string)r["USER_NAME"];
			Description = (string)r["USER_DESC"];
			IsLocal = (bool)r["USER_LOCAL"];
			IsEnabled = (bool)r["USER_ENABLED"];
			IsDirty = false;
		}
		#endregion

		[XmlIgnore()]
		public override bool IsUserObject { get { return true; } }

		#region IObjectModel Members
		public sf.ObjectType ObjectType { get { return sf.ObjectType.User; } }
		public sf.ObjectType ValidChildObjectTypes { get { return sf.ObjectType.None; } }
		public bool SupportsChildObjectType(sf.ObjectType objectType)
		{
			return (this.ValidChildObjectTypes & objectType) == objectType;
		}
		[XmlIgnore()]
		public sf.IObjectModel ParentObject { get; set; }
		#endregion

		#region ICloneable<User> Members
		public override sf.IObjectModel Clone(bool generateNewId)
		{
			User clone = this.Clone();
			if( generateNewId )
			{
				clone.Id = Guid.NewGuid().ToString();
			}
			return clone;
		}

		public User Clone()
		{
			return (User)this.MemberwiseClone();
		}

		User ICloneable<User>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		public void Synchronize(User sourceObject)
		{
			((ICloneable<SecurityPrincipalBase>)this).Synchronize( sourceObject );
		}

		void ICloneable<User>.Synchronize(User sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}
		#endregion

		public override string ToString()
		{
			return base.Name;
		}
	}

	public class UserCollection : ObservableObjectModelCollection<User>
	{
		public UserCollection()
			: base()
		{
		}
		public UserCollection(sf.IObjectModel owner)
			: base( owner )
		{
		}

		public void LoadFromDataTable(DataTable t)	//deprecated this: [bool append]
		{
			this.Clear();

			DataTableReader r = new DataTableReader( t );
			while( r.Read() )
			{
				this.Add(
					new User()
					{
						Id = r["SPLX_USER_ID"].ToString(),
						Name = (string)r["USER_NAME"],
						Description = (string)r["USER_DESC"],
						IsLocal = (bool)r["USER_LOCAL"],
						IsEnabled = (bool)r["USER_ENABLED"],
						IsDirty = false
					}
				);
			}
		}

		public void LoadFromDataTableWithSync(DataTable t)
		{
			User exists = null;

			DataTableReader r = new DataTableReader( t );
			while( r.Read() )
			{
				User user = new User()
				{
					Id = r["SPLX_USER_ID"].ToString(),
					Name = (string)r["USER_NAME"],
					Description = (string)r["USER_DESC"],
					IsLocal = (bool)r["USER_LOCAL"],
					IsEnabled = (bool)r["USER_ENABLED"],
					IsDirty = false
				};

				exists = null;
				if( this.GetById( user.Id, out exists ) )
				{
					exists.Synchronize( user );
				}
				else
				{
					this.Add( user );
				}
			}
		}

		public User GetById(string id)
		{
			return this.Single( u => u.Id == id );
		}

		public bool GetById(string id, out User user)
		{
			user = this.SingleOrDefault( u => u.Id == id );
			return user != null;
		}

		public User GetByName(string name)
		{
			return this.Single( u => u.Name.ToLower() == name.ToLower() );
		}

		public bool GetByName(string name, out User user)
		{
			user = this.SingleOrDefault( u => u.Name.ToLower() == name.ToLower() );
			return user != null;
		}
	}

	public class Group : SecurityPrincipalBase, sf.IObjectModel, ICloneable<Group>, INodeItem
	{
		private GroupCollection _groups = null;
		private UserCollection _users = null;
		private bool _isExpanded = false;
		private bool _isSelected = false;
		private bool _isEditing = false;
		private bool _showDetail = false;
		private bool _showDetailPanels = false;
		private bool _enableChildGroupsLazyLoad = false;

		private int _maskSize = settings.Default.RlsMaskSize;
		private BitArray _mask = null;

		public Group()
			: base()
		{
			_groups = new GroupCollection();
			_users = new UserCollection();
			_mask = new BitArray( _maskSize );
		}

		[XmlIgnore()]
		public BitArray Mask
		{
			get { return _mask; }
			set
			{
				if( _mask != value )
				{
					_mask = value;
					this.OnPropertyChanged( "Mask" );
				}
			}
		}
		[XmlIgnore()]
		public int MaskSize
		{
			get { return _maskSize; }
			set
			{
				if( _maskSize != value )
				{
					_maskSize = value;
					this.OnPropertyChanged( "MaskSize" );
					this.Mask = new BitArray( _maskSize );
				}
			}
		}
		public string MaskValue
		{
			get
			{
				int[] mask = new int[_maskSize / 32];	//32 bits per int
				_mask.CopyTo( mask, 0 );
				return sg.MiscUtils.Join<int>( ",", mask );
			}
			set
			{
				string[] values = value.Split( ',' );
				int[] masks = new int[_maskSize / 32];
				for( int i = 0; i < values.Length; i++ )
				{
					masks[i] = Int32.Parse( values[i] );
				}
				_mask = new BitArray( masks );
			}
		}


		//public Group(bool lazyLoadChildGroups)
		//    : this()
		//{ if( lazyLoadChildGroups ) { _groups.Add( new LazyLoadDummyGroup() ); } }

		#region IDatabaseObject
		public override void RecordSelect()
		{
			if( StoreConnection.IsConnected )
			{
				DataSet ds = StoreConnection.da.GetDataSet( "splx_dal_sel_groupbyid",
					new sSortedList( "@SPLX_GROUP_ID", this.Id ) );

				if( ds.Tables[0].Rows.Count > 0 )
				{
					this.LoadFromDataRow( ds.Tables[0].Rows[0] );
				}
				else
				{
					throw new RowNotInTableException( string.Format( "Unable to fetch Group '{0}' from the data store.", this.Name ) );
				}
			}
		}
		public override void RecordUpsert()
		{
			if( StoreConnection.IsConnected )
			{
				SortedList inparms = this.GetParms();

				SqlParameter id = new SqlParameter( "@SPLX_GROUP_ID", SqlDbType.UniqueIdentifier );
				id.Value = new Guid( this.Id );
				id.Direction = ParameterDirection.InputOutput;
				SortedList outparms = new sSortedList( "@SPLX_GROUP_ID", id );

				StoreConnection.da.ExecuteSP( "splx_api_upsert_group", inparms, ref outparms );
				this.Id = id.Value.ToString();

				this.IsDirty = false;
			}
		}
		public override void RecordUpsertForImport(ref SqlTransaction tr)
		{
			SortedList inparms = this.GetParms();

			SqlParameter id = new SqlParameter( "@SPLX_GROUP_ID", SqlDbType.UniqueIdentifier );
			id.Value = new Guid( this.Id );
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_GROUP_ID", id );

			StoreConnection.da.ExecuteSP( "splx_api_upsert_group", inparms, ref outparms, false, tr );
			this.Id = id.Value.ToString();
		}
		public override void RecordDelete()
		{
			if( StoreConnection.IsConnected )
			{
				StoreConnection.da.ExecuteSP( "splx_api_del_group", new sSortedList( "@SPLX_GROUP_ID", this.Id ) );
			}
		}
		internal SortedList GetParms()
		{
			sSortedList s = new sSortedList( "@GROUP_NAME", this.Name );
			s.Add( "@GROUP_DESC", this.Description );
			s.Add( "@GROUP_LOCAL", this.IsLocal );
			s.Add( "@GROUP_ENABLED", this.IsEnabled );

			byte[] mask = new byte[_maskSize / 8];	//8 bits per byte
			this.Mask.CopyTo( mask, 0 );
			s.Add( "@GROUP_MASK", mask );

			return s;
		}
		internal void LoadFromDataRow(DataRow r)
		{
			Id = r["SPLX_GROUP_ID"].ToString();
			Name = (string)r["GROUP_NAME"];
			Description = r["GROUP_DESC"].ToString();
			IsLocal = (bool)r["GROUP_LOCAL"];
			IsEnabled = (bool)r["GROUP_ENABLED"];
			Mask = new BitArray( (byte[])r["GROUP_MASK"] );
			IsDirty = false;
		}
		#endregion

		[XmlIgnore()]
		public override bool IsUserObject { get { return false; } }

		[XmlIgnore()]
		public override bool IsAnonymous { get { return false; } internal set { base.IsAnonymous = false; } }

		#region IObjectModel Members
		public sf.ObjectType ObjectType { get { return sf.ObjectType.Group; } }
		public sf.ObjectType ValidChildObjectTypes { get { return sf.ObjectType.Group; } }
		public bool SupportsChildObjectType(sf.ObjectType objectType)
		{
			return (this.ValidChildObjectTypes & objectType) == objectType;
		}
		[XmlIgnore()]
		public sf.IObjectModel ParentObject { get; set; }
		#endregion

		#region ICloneable<Group> Members
		public override sf.IObjectModel Clone(bool generateNewId)
		{
			Group clone = this.Clone();
			if( generateNewId )
			{
				clone.Id = Guid.NewGuid().ToString();
			}
			return clone;
		}

		public Group Clone()
		{
			return (Group)this.MemberwiseClone();
		}

		Group ICloneable<Group>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		public void Synchronize(Group sourceObject)
		{
			((ICloneable<SecurityPrincipalBase>)this).Synchronize( sourceObject );
		}

		void ICloneable<Group>.Synchronize(Group sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}
		#endregion

		[XmlIgnore()]
		public GroupCollection Groups { get { return _groups; } }
		[XmlIgnore()]
		public UserCollection Users { get { return _users; } }
		[XmlIgnore()]
		internal bool EnableChildGroupsLazyLoad
		{
			get { return _enableChildGroupsLazyLoad; }
			set
			{
				if( _enableChildGroupsLazyLoad != value )
				{
					_enableChildGroupsLazyLoad = value;

					if( _enableChildGroupsLazyLoad )
					{
						if( StoreConnection.IsConnected && _groups.Count == 0 )
						{
							_groups.Add( new LazyLoadDummyGroup() );
						}
					}
					else
					{
						if( this.WantsLazyLoad )
						{
							_groups.Clear();
						}
					}
				}
			}
		}

		[XmlIgnore()]
		private bool WantsLazyLoad
		{
			get
			{
				return StoreConnection.IsConnected &&
					_groups.Count > 0 && _groups[0] is LazyLoadDummyGroup;
			}
		}

		public override string ToString()
		{
			return base.Name;
		}

		#region INodeItem Members
		[XmlIgnore()]
		public bool IsExpanded
		{
			get { return _isExpanded; }
			set
			{
				_isExpanded = value;

				if( this.WantsLazyLoad )
				{
					_groups.Clear();
					DataSet ds =
						StoreConnection.da.GetDataSet( "splx_api_sel_groupnestmembbygroup", new sSortedList( "@SPLX_GROUP_ID", this.Id ) );
					_groups.LoadFromDataTableWithResolve( ds.Tables[0] );	//, false
				}

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

		[XmlIgnore]
		CompositeCollection INodeItem.ChildObjects { get { return null; } }
		#endregion
	}

	public class LazyLoadDummyGroup : Group
	{
	}

	public class GroupCollection : ObservableObjectModelCollection<Group>
	{
		private int _maskSize = settings.Default.RlsMaskSize;
		private BitArray _allMasks = null;

		public GroupCollection()
			: base()
		{
			base.CollectionViewSource.SortDescriptions.Add( new SortDescription( "Name", ListSortDirection.Ascending ) );
			_allMasks = new BitArray( _maskSize );
		}
		public GroupCollection(sf.IObjectModel owner)
			: base( owner )
		{
			base.CollectionViewSource.SortDescriptions.Add( new SortDescription( "Name", ListSortDirection.Ascending ) );
			_allMasks = new BitArray( _maskSize );
		}

		public void LoadFromDataTable(DataTable t)	//bool append
		{
			this.Clear();

			DataTableReader r = new DataTableReader( t );
			while( r.Read() )
			{
				this.Add(
					new Group()
					{
						Id = r["SPLX_GROUP_ID"].ToString(),
						Name = (string)r["GROUP_NAME"],
						Description = r["GROUP_DESC"].ToString(),
						IsLocal = (bool)r["GROUP_LOCAL"],
						IsEnabled = (bool)r["GROUP_ENABLED"],
						Mask = new BitArray( (byte[])r["GROUP_MASK"] ),
						IsDirty = false,
						EnableChildGroupsLazyLoad = true
					}
				);
			}
		}

		//TODO: thread this.
		//TODO: change splx_api_sel_groupnestmembbygroup to return full group info so as to
		//		remove the need for g.RecordSelect() [will change to straight this.Add(new Group(){...})]
		public void LoadFromDataTableWithResolve(DataTable t)	//bool append
		{
			string groupId = null;
			Group g = null;

			this.Clear();

			DataTableReader r = new DataTableReader( t );
			while( r.Read() )
			{
				groupId = r["SPLX_GROUP_ID"].ToString();
				if( !StoreConnection.SplxStore.Groups.GetById( groupId, out g ) )
				{
					g.Id = groupId;
					g.RecordSelect();
					g.EnableChildGroupsLazyLoad = true;
				}
				this.Add( g );
				g = null;
			}
		}

		public void LoadFromDataTableWithSync(DataTable t)
		{
			Group exists = null;

			DataTableReader r = new DataTableReader( t );
			while( r.Read() )
			{
				Group group = new Group()
				{
					Id = r["SPLX_GROUP_ID"].ToString(),
					Name = (string)r["GROUP_NAME"],
					Description = r["GROUP_DESC"].ToString(),
					IsLocal = (bool)r["GROUP_LOCAL"],
					IsEnabled = (bool)r["GROUP_ENABLED"],
					Mask = new BitArray( (byte[])r["GROUP_MASK"] ),
					IsDirty = false,
					EnableChildGroupsLazyLoad = true
				};

				exists = null;
				if( this.GetById( group.Id, out exists ) )
				{
					exists.Synchronize( group );
				}
				else
				{
					this.Add( group );
				}
			}
		}

		public Group GetById(string id)
		{
			return this.Single( g => g.Id == id );
		}

		public bool GetById(string id, out Group group)
		{
			group = this.SingleOrDefault( g => g.Id == id );
			return group != null;
		}

		public Group GetByName(string name)
		{
			return this.Single( g => g.Name.ToLower() == name.ToLower() );
		}

		public bool GetByName(string name, out Group group)
		{
			group = this.SingleOrDefault( g => g.Name.ToLower() == name.ToLower() );
			return group != null;
		}

		[XmlIgnore()]
		public int MaskSize
		{
			get { return _maskSize; }
			set
			{
				if( _maskSize != value )
				{
					_maskSize = value;
					_allMasks = new BitArray( _maskSize );
				}
			}
		}
		//TODO: this assumes there's room in the mask, next to add error handling
		public BitArray GetNextMask()
		{
			int index = 0;
			while( _allMasks[index] )
			{
				index++;
			}

			BitArray nextMask = new BitArray( _maskSize );
			nextMask.Set( index, true );
			return nextMask;
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			switch( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
				{
					this.AddMask( e.NewItems );
					break;
				}
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
				{
					this.AddMask( e.NewItems );
					this.RemoveMask( e.OldItems );
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					this.RemoveMask( e.OldItems );
					break;
				}
			}

			base.OnCollectionChanged( e );
		}

		private void AddMask(IList groups)
		{
			if( groups != null )
			{
				foreach( Group g in groups )
				{
					_allMasks.Or( g.Mask );
				}
			}
		}

		private void RemoveMask(IList groups)
		{
			if( groups != null )
			{
				foreach( Group g in groups )
				{
					_allMasks.Xor( g.Mask );
				}
			}
		}
	}

/*
	public class GroupMembershipCollection_1 : IXmlSerializable
	{
		private const string __groupIdFld = "groupId";
		private const string __memberIdFld = "memberId";
		private const string __entryElmt = "Entry";
		private const string __groupAtt = "Group";
		private const string __memberAtt = "Member";

		private SuplexStore _ownerStore = null;

		private DataTable _list = null;

		internal GroupMembershipCollection_1()
		{
			_list = new DataTable();
			_list.Columns.Add( new DataColumn( __groupIdFld ) );
			_list.Columns.Add( new DataColumn( __memberIdFld ) );
			_list.PrimaryKey = new DataColumn[] { _list.Columns[0], _list.Columns[1] };

			this.ThrowConstraintErrors = false;
		}
		public GroupMembershipCollection_1(SuplexStore ownerStore)
			: this()
		{
			_ownerStore = ownerStore;
		}

		internal SuplexStore OwnerStore { get { return _ownerStore; } set { _ownerStore = value; } }

		public void Add(string groupId, string memberId)
		{
			if( groupId == memberId )
			{
				throw new ArgumentException( "GroupId and MemberId cannot be equal." );
			}

			//less expensive than try/catch?
			//DataRow[] match = _list.Select( string.Format( "{0}='{1}' AND {2}='{3}'", __groupId, groupId, __memberId, memberId ) );
			try
			{
				DataRow r = _list.NewRow();
				r[__groupIdFld] = groupId;
				r[__memberIdFld] = memberId;
				_list.Rows.Add( r );
			}
			catch( ConstraintException ce )
			{
				if( this.ThrowConstraintErrors )
				{
					throw ce;
				}
			}
		}

		public List<SecurityPrincipalBase> GetByGroup(string groupId, bool recursive)
		{
			DataRow[] match = _list.Select( string.Format( "{0}='{1}'", __groupIdFld, groupId ) );

			User u = null;
			Group g = null;
			List<SecurityPrincipalBase> members = new List<SecurityPrincipalBase>();
			foreach( DataRow r in match )
			{
				if( _ownerStore.Users.GetById( (string)r[__memberIdFld], out u ) )
				{
					members.Add( u );
				}
				else if( _ownerStore.Groups.GetById( (string)r[__groupIdFld], out g ) )
				{
					members.Add( g );
				}
			}

			return members;
		}

		public GroupCollection GetByMember(string memberId, bool recursive)
		{
			DataRow[] match = _list.Select( string.Format( "{0}='{1}'", __memberIdFld, memberId ) );

			Group g = null;
			GroupCollection groups = new GroupCollection( null );
			foreach( DataRow r in match )
			{
				if( _ownerStore.Groups.GetById( (string)r[__groupIdFld], out g ) )
				{
					groups.Add( g );
				}
			}

			return groups;
		}

		public MembershipCollection GetGroupMembership(string groupId)
		{
			MembershipCollection memberList = new MembershipCollection();

			User u = null;
			Group g = null;
			List<SecurityPrincipalBase> members = new List<SecurityPrincipalBase>();
			foreach( DataRow r in _list.Rows )
			{
				if( r[__groupIdFld].ToString() == groupId )
				{
					if( _ownerStore.Users.GetById( (string)r[__memberIdFld], out u ) )
					{
						memberList.MemberList.Add( u );
					}
					else if( _ownerStore.Groups.GetById( (string)r[__memberIdFld], out g ) )
					{
						memberList.MemberList.Add( g );
					}
				}
				else
				{
					if( _ownerStore.Users.GetById( (string)r[__memberIdFld], out u ) )
					{
						memberList.NonMemberList.Add( u );
					}
					else if( _ownerStore.Groups.GetById( (string)r[__memberIdFld], out g ) )
					{
						memberList.NonMemberList.Add( g );
					}
				}
			}

			return memberList;
		}

		public void Remove(string groupId, string memberId)
		{ }

		public void RemoveByGroup(string groupId)
		{ }

		public void RemoveByMember(string memberId)
		{ }

		public DataTable List { get { return _list; } }

		public bool ThrowConstraintErrors { get; set; }


		#region IXmlSerializable Members
		//NOTE: from MSDN docs:
		//This method is reserved and should not be used. When implementing the IXmlSerializable interface,
		//	you should return a null reference from this method, and instead, if specifying a custom schema is required,
		//	apply the XmlSchemaProviderAttribute to the class.
		public XmlSchema GetSchema() { return null; }

		public void ReadXml(XmlReader reader)
		{
			reader.Read();	//skip first line
			while( reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == __entryElmt )
			{
				this.Add( reader[__groupAtt], reader[__memberAtt] );
				reader.Read();
			}
			reader.Read();
		}

		public void WriteXml(XmlWriter writer)
		{
			foreach( DataRow r in _list.Rows )
			{
				writer.WriteStartElement( __entryElmt );
				writer.WriteAttributeString( __groupAtt, (string)r[__groupIdFld] );
				writer.WriteAttributeString( __memberAtt, (string)r[__memberIdFld] );
				writer.WriteEndElement();
			}
		}
		#endregion

		public override string ToString()
		{
			return string.Format( "Count = {0}", _list.Rows.Count );
		}
	}
*/

	public class GroupMembershipItem
	{
		public GroupMembershipItem() { }
		public GroupMembershipItem(Group group, SecurityPrincipalBase member)
		{
			this.Group = group;
			this.GroupId = group.Id;
			this.Member = member;
			this.MemberId = member.Id;
			this.MemberType = member is User ? ObjectType.User : ObjectType.Group;
		}

		internal GroupMembershipItem(string groupId, SecurityPrincipalBase member)
		{
			this.GroupId = groupId;
			this.Member = member;
			this.MemberId = member.Id;
			this.MemberType = member is User ? ObjectType.User : ObjectType.Group;
		}
		internal GroupMembershipItem(string groupId, string memberId, sf.ObjectType memberType)
		{
			this.GroupId = groupId;
			this.MemberId = memberId;
			this.MemberType = memberType;
		}


		public Group Group { get; set; }
		public SecurityPrincipalBase Member { get; set; }

		internal string GroupId { get; set; }
		internal string MemberId { get; set; }
		internal sf.ObjectType MemberType { get; set; }

		public override string ToString()
		{
			return string.Format( "{0}/{1}", this.Group.Name, this.Member.Name );
		}

		public string ToMembershipKey()
		{
			return string.Format( "{0}_{1}", this.Group.Id, this.Member.Id );
		}
	}

	public class GroupMembershipCollection : IXmlSerializable, INotifyCollectionChanged
	{
		private Dictionary<string, GroupMembershipItem> _innerList = null;

		private const string __entryElmt = "Entry";
		private const string __groupAtt = "Group";
		private const string __memberAtt = "Member";
		private const string __typeAtt = "Type";

		private SuplexStore _ownerStore = null;

		internal GroupMembershipCollection()
		{
			_innerList = new Dictionary<string, GroupMembershipItem>();

			this.ThrowConstraintErrors = false;
		}
		public GroupMembershipCollection(SuplexStore ownerStore)
			: this()
		{
			_ownerStore = ownerStore;
		}

		public void Clear()
		{
			_innerList.Clear();
			this.OnCollectionChanged( NotifyCollectionChangedAction.Reset );
		}

		public void Resolve()
		{
			foreach( GroupMembershipItem item in _innerList.Values )
			{
				item.Group = _ownerStore.Groups.GetById( item.GroupId );

				if( item.MemberType == ObjectType.User )
				{
					item.Member = _ownerStore.Users.GetById( item.MemberId );
					item.Group.Users.Add( (User)item.Member );
				}
				else
				{
					item.Member = _ownerStore.Groups.GetById( item.MemberId );
					item.Group.Groups.Add( (Group)item.Member );
				}
			}
		}

		internal SuplexStore OwnerStore { get { return _ownerStore; } set { _ownerStore = value; } }

		private string GetKey(Group group, SecurityPrincipalBase member)
		{
			return string.Format( "{0}_{1}", group.Id, member.Id );
		}
		private string GetKey(string groupId, SecurityPrincipalBase member)
		{
			return string.Format( "{0}_{1}", groupId, member.Id );
		}
		private string GetKey(string groupId, string memberId)
		{
			return string.Format( "{0}_{1}", groupId, memberId );
		}

		public void Add(Group group, SecurityPrincipalBase member)
		{
			if( group.Id == member.Id )
			{
				throw new ArgumentException( "GroupId and MemberId cannot be equal." );
			}

			string key = this.GetKey( group, member );
			if( !_innerList.ContainsKey( key ) )
			{
				if( !StoreConnection.IsConnected )
				{
					_innerList.Add( key, new GroupMembershipItem( group, member ) );
				}

				if( member.IsUserObject )
				{
					if( StoreConnection.IsConnected )
					{
						try
						{
							SortedList parms = new sSortedList( "@SPLX_USER_ID", member.Id );
							parms.Add( "@SPLX_GROUP_ID", group.Id );
							parms.Add( "@CURR_USER_ID", Guid.Empty );
							StoreConnection.da.ExecuteSP( "splx_api_ins_groupmemb", parms );

							_innerList.Add( key, new GroupMembershipItem( group, member ) );
							group.Users.Add( (User)member );
						}
						catch( SqlException ex )
						{
							if( !(ex.Number == 2601) ) //2601 is UniqueIndex violation (throw away the dups)
							{
								throw ex;
							}
						}
					}
					else
					{
						group.Users.Add( (User)member );
					}
				}
				else
				{
					if( StoreConnection.IsConnected )
					{
						try
						{
							SortedList parms = new sSortedList( "@CHILD_GROUP_ID", member.Id );
							parms.Add( "@PARENT_GROUP_ID", group.Id );
							parms.Add( "@CURR_USER_ID", Guid.Empty );
							StoreConnection.da.ExecuteSP( "splx_api_ins_groupnest", parms );

							_innerList.Add( key, new GroupMembershipItem( group, member ) );
							group.Groups.Add( (Group)member );
						}
						catch( SqlException ex )
						{
							if( !(ex.Number == 50000 && ex.Class == 16) && ex.Number != 2601 ) //50000:16 is parent/child relationship error (ancestor/descendant error)
							{
								throw ex;
							}
						}
					}
					else
					{
						group.Groups.Add( (Group)member );
					}
				}

				this.OnCollectionChanged( NotifyCollectionChangedAction.Add );
			}
			else
			{
				if( this.ThrowConstraintErrors )
				{
					throw new ArgumentException( string.Format( "GroupMembership key '{0}' already exists.", key ) );
				}
			}
		}
		public void Add(string groupId, SecurityPrincipalBase member)
		{
			if( groupId == member.Id )
			{
				throw new ArgumentException( "GroupId and MemberId cannot be equal." );
			}

			string key = this.GetKey( groupId, member );
			if( !_innerList.ContainsKey( key ) )
			{
				_innerList.Add( key, new GroupMembershipItem( groupId, member ) );
				this.OnCollectionChanged( NotifyCollectionChangedAction.Add );
			}
			else
			{
				if( this.ThrowConstraintErrors )
				{
					throw new ArgumentException( string.Format( "GroupMembership key '{0}' already exists.", key ) );
				}
			}
		}
		internal void Add(string groupId, string memberId, string objectType)
		{
			if( groupId == memberId )
			{
				throw new ArgumentException( "GroupId and MemberId cannot be equal." );
			}

			string key = this.GetKey( groupId, memberId );
			if( !_innerList.ContainsKey( key ) )
			{
				_innerList.Add( key, new GroupMembershipItem( groupId, memberId, objectType.ToLower() == "user" ? ObjectType.User : ObjectType.Group ) );
				this.OnCollectionChanged( NotifyCollectionChangedAction.Add );
			}
			else
			{
				if( this.ThrowConstraintErrors )
				{
					throw new ArgumentException( string.Format( "GroupMembership key '{0}' already exists.", key ) );
				}
			}
		}
		public void RecordUpsertForImport(ref SqlTransaction tr)
		{
			if( !StoreConnection.IsConnected )
			{
				return;
			}

			Group group = null;
			SecurityPrincipalBase member = null;
			foreach( GroupMembershipItem item in _innerList.Values )
			{
				group = item.Group;
				member = item.Member;

				if( member.IsUserObject )
				{
					try
					{
						SortedList parms = new sSortedList( "@SPLX_USER_ID", member.Id );
						parms.Add( "@SPLX_GROUP_ID", group.Id );
						parms.Add( "@CURR_USER_ID", Guid.Empty );
						StoreConnection.da.ExecuteSP( "splx_api_ins_groupmemb", parms, false, tr );
					}
					catch( SqlException ex )
					{
						if( !(ex.Number == 2601) ) //2601 is UniqueIndex violation (throw away the dups)
						{
							throw ex;
						}
					}
				}
				else
				{
					try
					{
						SortedList parms = new sSortedList( "@CHILD_GROUP_ID", member.Id );
						parms.Add( "@PARENT_GROUP_ID", group.Id );
						parms.Add( "@CURR_USER_ID", Guid.Empty );
						StoreConnection.da.ExecuteSP( "splx_api_ins_groupnest", parms, false, tr );
					}
					catch( SqlException ex )
					{
						if( !(ex.Number == 50000 && ex.Class == 16) && ex.Number != 2601 ) //50000:16 is parent/child relationship error (ancestor/descendant error)
						{
							throw ex;
						}
					}
				}
			}
		}

		public IEnumerable<GroupMembershipItem> GetByGroup(Group group, bool groupsOnly)
		{
			return _innerList.Values.Where( item => item.GroupId == group.Id );
		}

		public IEnumerable<GroupMembershipItem> GetByMember(SecurityPrincipalBase member, bool recursive)
		{
			return _innerList.Values.Where( item => item.Member.Id == member.Id );
		}

		public IEnumerable<GroupMembershipItem> GetByGroupOrMember(SecurityPrincipalBase member, bool recursive)
		{
			return _innerList.Values.Where( item => item.GroupId == member.Id || item.Member.Id == member.Id );
		}

		//public MembershipList<SecurityPrincipalBase> GetGroupMembers_(Group group)
		//{
		//    MembershipList<SecurityPrincipalBase> membership = new MembershipList<SecurityPrincipalBase>();

		//    _innerList.Values.Where( grp => grp.GroupId == group.Id && grp.Group.IsEnabled );

		//    return membership;
		//}

		public MembershipList<SecurityPrincipalBase> GetGroupMembers(Group group)
		{
			MembershipList<SecurityPrincipalBase> membership = new MembershipList<SecurityPrincipalBase>();

			foreach( GroupMembershipItem item in _innerList.Values )
			{
				if( item.GroupId == group.Id && item.Member.IsEnabled )
				{
					membership.MemberList.Add( item.Member );
				}
			}

			membership.NonMemberList = new ObservableObjectModelCollection<SecurityPrincipalBase>();
			IEnumerator<SecurityPrincipalBase> nonMembers = _ownerStore.SecurityPrincipals.Except( membership.MemberList ).GetEnumerator();
			while( nonMembers.MoveNext() )
			{
				if( nonMembers.Current.Id != group.Id && nonMembers.Current.IsEnabled )
				{
					membership.NonMemberList.Add( nonMembers.Current );
				}
			}

			ObservableObjectModelCollection<SecurityPrincipalBase> nonMembs = membership.NonMemberList;
			ObservableObjectModelCollection<SecurityPrincipalBase> nestedMembs = membership.NestedMemberList;
			this.RecurseIneligibleNonMembersUp( group, ref nonMembs, ref nestedMembs );

			return membership;
		}

		public MembershipList<Group> GetMemberOf(SecurityPrincipalBase member)
		{
			MembershipList<Group> membership = new MembershipList<Group>();

			foreach( GroupMembershipItem item in _innerList.Values )
			{
				if( item.Member.Id == member.Id )
				{
					membership.MemberList.Add( item.Group );
				}
			}

			membership.NonMemberList = new ObservableObjectModelCollection<Group>();
			IEnumerator<Group> nonMembers = _ownerStore.Groups.Except( membership.MemberList ).GetEnumerator();
			while( nonMembers.MoveNext() )
			{
				if( nonMembers.Current.IsLocal && nonMembers.Current.Id != member.Id )
				{
					membership.NonMemberList.Add( nonMembers.Current );
				}
			}

			if( ((sf.IObjectModel)member).ObjectType == ObjectType.Group )
			{
				//Group thisGroup = membership.NonMemberList.FirstOrDefault( group => group.Id == member.Id );
				//membership.NonMemberList.Remove( thisGroup );
				IEnumerable<GroupMembershipItem> members = this.GetByGroup( (Group)member, false );
				foreach( GroupMembershipItem gmi in members )
				{
					if( ((sf.IObjectModel)gmi.Member).ObjectType == ObjectType.Group )
					{
						membership.NonMemberList.Remove( (Group)gmi.Member );
					}
				}
			}

			if( !member.IsUserObject )
			{
				ObservableObjectModelCollection<Group> nonMembs = membership.NonMemberList;
				ObservableObjectModelCollection<Group> nestedMembs = membership.NestedMemberList;
				this.RecurseIneligibleNonMembers<Group>( (Group)member, ref nonMembs, ref nestedMembs );
			}

			return membership;
		}

		private void RecurseIneligibleNonMembersUp(Group parent,
			ref ObservableObjectModelCollection<SecurityPrincipalBase> nonMembers, ref ObservableObjectModelCollection<SecurityPrincipalBase> nestedMembs)
		{
			foreach( GroupMembershipItem item in _innerList.Values )
			{
				if( item.MemberId == parent.Id )
				{
					//BUG: this doesn't always work when connected to database
					nonMembers.Remove( item.Group );
					nestedMembs.Add( item.Group );

					this.RecurseIneligibleNonMembersUp( item.Group, ref nonMembers, ref nestedMembs );
				}
			}
		}

		private void RecurseIneligibleNonMembers<T>
			(Group parent, ref ObservableObjectModelCollection<T> nonMembers, ref ObservableObjectModelCollection<T> nestedMembs)
			where T : SecurityPrincipalBase
		{
			foreach( GroupMembershipItem item in _innerList.Values )
			{
				if( item.GroupId == parent.Id )
				{
					if( !item.Member.IsUserObject )
					{
						nonMembers.Remove( (T)item.Member );
						nestedMembs.Add( (T)item.Member );

						this.RecurseIneligibleNonMembers<T>( (Group)item.Member, ref nonMembers, ref nestedMembs );
					}
				}
			}
		}

		public void Remove(Group group, SecurityPrincipalBase member)
		{
			this.InnerList.Remove( this.GetKey( group, member ) );

			if( member.IsUserObject )
			{
				group.Users.Remove( (User)member );

				if( StoreConnection.IsConnected )
				{
					SortedList parms = new sSortedList( "@SPLX_USER_ID", member.Id );
					parms.Add( "@SPLX_GROUP_ID", group.Id );
					parms.Add( "@CURR_USER_ID", Guid.Empty );
					StoreConnection.da.ExecuteSP( "splx_api_del_groupmemb", parms );
				}
			}
			else
			{
				group.Groups.Remove( (Group)member );

				if( StoreConnection.IsConnected )
				{
					//HACK: ..maybe should a Comparer or RemoveById method
					//this can happen due to lazy-loading, the group in the list isn't resolved against the main Groups list
					Group g = null;
					if( group.Groups.GetById( member.Id, out g ) )
					{
						group.Groups.Remove( g );
					}

					SortedList parms = new sSortedList( "@CHILD_GROUP_ID", member.Id );
					parms.Add( "@PARENT_GROUP_ID", group.Id );
					parms.Add( "@CURR_USER_ID", Guid.Empty );
					StoreConnection.da.ExecuteSP( "splx_api_del_groupnest", parms );
				}
			}
			this.OnCollectionChanged( NotifyCollectionChangedAction.Remove );
		}

		[Obsolete( "Not in use; consider removing", true )]
		public void Remove(string groupId, string memberId)
		{
			this.InnerList.Remove( this.GetKey( groupId, memberId ) );
			this.OnCollectionChanged( NotifyCollectionChangedAction.Remove );
		}

		[Obsolete( "Not in use; consider removing", true )]
		public void RemoveByGroup(Group group)
		{
			IEnumerable<GroupMembershipItem> items = this.GetByGroup( group, false );
			foreach( GroupMembershipItem gmi in items )
			{
				this.InnerList.Remove( gmi.ToMembershipKey() );
			}
			this.OnCollectionChanged( NotifyCollectionChangedAction.Remove );
		}

		[Obsolete( "Not in use; consider removing", true )]
		public void RemoveByMember(SecurityPrincipalBase member)
		{
			IEnumerable<GroupMembershipItem> items = this.GetByMember( member, false );
			foreach( GroupMembershipItem gmi in items )
			{
				this.InnerList.Remove( gmi.ToMembershipKey() );
			}
			this.OnCollectionChanged( NotifyCollectionChangedAction.Remove );
		}

		public void RemoveByGroupOrMember(SecurityPrincipalBase member)
		{
			List<string> keys = new List<string>();
			IEnumerable<GroupMembershipItem> items = this.GetByGroupOrMember( member, false );
			foreach( GroupMembershipItem gmi in items )
			{
				keys.Add( gmi.ToMembershipKey() );
			}
			for( int i = keys.Count-1; i > -1; i-- )
			{
				this.InnerList.Remove( keys[i] );
			}
			this.OnCollectionChanged( NotifyCollectionChangedAction.Remove );
		}

		public Dictionary<string, GroupMembershipItem> InnerList { get { return _innerList; } }

		public bool ThrowConstraintErrors { get; set; }

		#region IDatabaseObject
		public void DatabaseRefresh()
		{
			if( StoreConnection.IsConnected )
			{
				DataSet ds = StoreConnection.da.GetDataSet( "splx_api_sel_groupmemb_nested_composite", null );
				StoreConnection.da.NameTablesFromCompositeSelect( ref ds );
				this.DatabaseRefresh( ds );
			}
		}

		internal void DatabaseRefresh(DataSet ds)
		{
			string userId = null;
			string groupId = null;
			string nestedGroupId = null;
			string key = string.Empty;

			DataTableReader r = new DataTableReader( ds.Tables["GroupMembership"] );
			while( r.Read() )
			{
				userId = r["SPLX_USER_ID"].ToString();
				groupId = r["SPLX_GROUP_ID"].ToString();
				key = this.GetKey( groupId, userId );

				if( !_innerList.ContainsKey( key ) )
				{
					User u = null;
					if( !StoreConnection.SplxStore.Users.GetById( userId, out u ) )
					{
						u = new User()
						{
							Id = userId
						};
						u.RecordSelect();

						StoreConnection.SplxStore.Users.Add( u );
					}

					Group g = null;
					if( !StoreConnection.SplxStore.Groups.GetById( groupId, out g ) )
					{
						g = new Group()
						{
							Id = groupId
						};
						g.RecordSelect();

						StoreConnection.SplxStore.Groups.Add( g );
					}

					_innerList.Add( key, new GroupMembershipItem( g, u ) );
				}
			}

			r = new DataTableReader( ds.Tables["NestedGroups"] );
			while( r.Read() )
			{
				groupId = r["PARENT_GROUP_ID"].ToString();
				nestedGroupId = r["CHILD_GROUP_ID"].ToString();
				key = this.GetKey( groupId, nestedGroupId );
				if( !_innerList.ContainsKey( key ) )
				{
					Group g = null;
					if( !StoreConnection.SplxStore.Groups.GetById( groupId, out g ) )
					{
						g = new Group()
						{
							Id = groupId,
							EnableChildGroupsLazyLoad = true
						};
						g.RecordSelect();

						StoreConnection.SplxStore.Groups.Add( g );
					}

					Group ng = null;
					if( !StoreConnection.SplxStore.Groups.GetById( nestedGroupId, out ng ) )
					{
						ng = new Group()
						{
							Id = nestedGroupId,
							EnableChildGroupsLazyLoad = true
						};
						g.RecordSelect();

						StoreConnection.SplxStore.Groups.Add( ng );
					}

					_innerList.Add( key, new GroupMembershipItem( g, ng ) );
				}
			}
		}

		//TODO: Find the Users that don't exist to merge them in, plus add NonMembers
		public void RecordSelectGroupMembers(Group group)
		{
			if( StoreConnection.IsConnected )
			{
				//Dictionary<string, GroupMembershipItem> list = new Dictionary<string, GroupMembershipItem>();
				//UserCollection users = new UserCollection();

				string id = null;
				string key = string.Empty;
				DataSet groupMembs = new DataSet();
				DataSet nestedMembs = new DataSet();
				StoreConnection.da.OpenConnection();
				StoreConnection.da.GetDataSet( "splx_api_sel_groupmembbygroup", new sSortedList( "@SPLX_GROUP_ID", group.Id ), groupMembs, "groupMembs", false );
				StoreConnection.da.GetDataSet( "splx_api_sel_groupnestmembbygroup", new sSortedList( "@SPLX_GROUP_ID", group.Id ), nestedMembs, "nestedMembs", false );
				StoreConnection.da.CloseConnection();
				StoreConnection.da.NameTablesFromCompositeSelect( ref groupMembs );
				StoreConnection.da.NameTablesFromCompositeSelect( ref nestedMembs );

				DataTableReader r = new DataTableReader( groupMembs.Tables["GroupMembership"] );
				while( r.Read() )
				{
					id = r["SPLX_USER_ID"].ToString();
					key = this.GetKey( group.Id, id );
					if( !_innerList.ContainsKey( key ) )
					{
						User u = null;
						if( !StoreConnection.SplxStore.Users.GetById( id, out u ) )
						{
							u = new User()
							{
								Id = id,
							};
							u.RecordSelect();

							StoreConnection.SplxStore.Users.Add( u );
						}

						_innerList.Add( key, new GroupMembershipItem( group, u ) );
					}
				}

				r = new DataTableReader( groupMembs.Tables["GroupNonMembership"] );
				while( r.Read() )
				{
					id = r["SPLX_USER_ID"].ToString();
					User u = null;
					if( !StoreConnection.SplxStore.Users.GetById( id, out u ) )
					{
						u = new User()
						{
							Id = id,
						};
						u.RecordSelect();

						StoreConnection.SplxStore.Users.Add( u );
					}
				}

				r = new DataTableReader( nestedMembs.Tables["GroupMembership"] );
				while( r.Read() )
				{
					id = r["SPLX_GROUP_ID"].ToString();
					key = this.GetKey( group.Id, id );
					if( !_innerList.ContainsKey( key ) )
					{
						Group g = null;
						if( !StoreConnection.SplxStore.Groups.GetById( id, out g ) )
						{
							g = new Group()
							{
								Id = id,
								EnableChildGroupsLazyLoad = true
							};
							g.RecordSelect();

							StoreConnection.SplxStore.Groups.Add( g );
						}

						_innerList.Add( key, new GroupMembershipItem( group, g ) );
					}
				}

				r = new DataTableReader( nestedMembs.Tables["GroupNonMembership"] );
				while( r.Read() )
				{
					id = r["SPLX_GROUP_ID"].ToString();
					Group g = null;
					if( !StoreConnection.SplxStore.Groups.GetById( id, out g ) )
					{
						g = new Group()
						{
							Id = id,
							EnableChildGroupsLazyLoad = true
						};
						g.RecordSelect();

						StoreConnection.SplxStore.Groups.Add( g );
					}
				}
			}
		}

		public void RecordSelectGroupMemberOf(User user)
		{
			if( StoreConnection.IsConnected )
			{
				//Dictionary<string, GroupMembershipItem> list = new Dictionary<string, GroupMembershipItem>();
				//UserCollection users = new UserCollection();

				string id = null;
				string key = string.Empty;
				DataSet groupMembs =
					StoreConnection.da.GetDataSet( "splx_api_sel_groupmembbyuser", new sSortedList( "@SPLX_USER_ID", user.Id ) );
				StoreConnection.da.NameTablesFromCompositeSelect( ref groupMembs );

				DataTableReader r = new DataTableReader( groupMembs.Tables["GroupMembership"] );
				while( r.Read() )
				{
					id = r["SPLX_GROUP_ID"].ToString();
					key = this.GetKey( id, user.Id );
					if( !_innerList.ContainsKey( key ) )
					{
						Group g = null;
						if( !StoreConnection.SplxStore.Groups.GetById( id, out g ) )
						{
							g = new Group()
							{
								Id = id,
								EnableChildGroupsLazyLoad = true
							};
							g.RecordSelect();

							StoreConnection.SplxStore.Groups.Add( g );
						}

						_innerList.Add( key, new GroupMembershipItem( g, user ) );
					}
				}

				r = new DataTableReader( groupMembs.Tables["GroupNonMembership"] );
				while( r.Read() )
				{
					id = r["SPLX_GROUP_ID"].ToString();
					Group g = null;
					if( !StoreConnection.SplxStore.Groups.GetById( id, out g ) )
					{
						g = new Group()
						{
							Id = id,
							EnableChildGroupsLazyLoad = true
						};
						g.RecordSelect();

						StoreConnection.SplxStore.Groups.Add( g );
					}
				}

			}
		}

		[Obsolete( "Did this inline with object method", true )]
		public void RecordCreateGroupMemb(string groupId, string userId)
		{
			if( StoreConnection.IsConnected )
			{
				SortedList inparms =
					new sSortedList( "@SPLX_GROUP_ID", groupId, "@SPLX_USER_ID", userId, "@CURR_USER_ID", Convert.DBNull );

				StoreConnection.da.ExecuteSP( "splx_api_ins_groupmemb", inparms );
			}
		}
		[Obsolete( "Did this inline with object method", true )]
		public void RecordDelete(string groupId, string userId)
		{
			if( StoreConnection.IsConnected )
			{
				SortedList inparms =
					new sSortedList( "@SPLX_GROUP_ID", groupId, "@SPLX_USER_ID", userId, "@CURR_USER_ID", Convert.DBNull );

				StoreConnection.da.ExecuteSP( "splx_api_del_groupmemb", inparms );
			}
		}
		#endregion


		#region IXmlSerializable Members
		//NOTE: from MSDN docs:
		//This method is reserved and should not be used. When implementing the IXmlSerializable interface,
		//	you should return a null reference from this method, and instead, if specifying a custom schema is required,
		//	apply the XmlSchemaProviderAttribute to the class.
		public XmlSchema GetSchema() { return null; }

		public void ReadXml(XmlReader reader)
		{
			reader.Read();	//skip first line
			while( reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == __entryElmt )
			{
				//if( reader[__typeAtt].ToLower() == "user" )
				//{
				//    this.Add( reader[__groupAtt], _ownerStore.Users.GetById( reader[__memberAtt] ) );
				//}
				//else
				//{
				//    this.Add( reader[__groupAtt], _ownerStore.Groups.GetById( reader[__memberAtt] ) );
				//}

				this.Add( reader[__groupAtt], reader[__memberAtt], reader[__typeAtt] );

				reader.Read();
			}
			reader.Read();
		}

		public void WriteXml(XmlWriter writer)
		{
			foreach( GroupMembershipItem item in _innerList.Values )
			{
				writer.WriteStartElement( __entryElmt );
				writer.WriteAttributeString( __groupAtt, item.GroupId );
				writer.WriteAttributeString( __memberAtt, item.Member.Id );
				writer.WriteAttributeString( __typeAtt, item.Member.IsUserObject ? "User" : "Group" );
				writer.WriteEndElement();
			}
		}
		#endregion

		public override string ToString()
		{
			return string.Format( "Count = {0}", _innerList.Count );
		}

		#region INotifyCollectionChanged Members
		//TODO: implement IList parameter support proper change notifications for .Add/.Remove
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		protected void OnCollectionChanged(NotifyCollectionChangedAction action)
		{
			if( this.CollectionChanged != null )
			{
				this.CollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
			}
		}
		#endregion
	}

	public class MembershipList<T>
	{
		private ObservableObjectModelCollection<T> _memberList = null;
		private ObservableObjectModelCollection<T> _nonMemberList = null;
		private ObservableObjectModelCollection<T> _nestedMemberList = null;

		public MembershipList()
		{
			_memberList = new ObservableObjectModelCollection<T>();
			_nonMemberList = new ObservableObjectModelCollection<T>();
			_nestedMemberList = new ObservableObjectModelCollection<T>();
		}

		public ObservableObjectModelCollection<T> MemberList { get { return _memberList; } internal set { _memberList = value; } }
		public ObservableObjectModelCollection<T> NonMemberList { get { return _nonMemberList; } internal set { _nonMemberList = value; } }
		public ObservableObjectModelCollection<T> NestedMemberList { get { return _nestedMemberList; } internal set { _nestedMemberList = value; } }
	}

	//see note in Resolve regarding INotifyDeleted
	public abstract class AccessControlEntryBase : ss.IAccessControlEntry, sf.IObjectModel,
		INotifyPropertyChanged, INotifyDeleted		//, IDatabaseObject  , INotifyPropertyChanging
	{
		private bool _isSelected = false;

		private st.ISecurityPrincipal _securityPrincipal = null;
		//private UIElement _uie = null;

		private long _id = -1;
		private object _right = null;
		private bool _allowed = false;
		private bool _inherit = false;

		private bool _isDirty = false;

		private bool _isDeleted = false;

		public AccessControlEntryBase()
		{
			Inherit = true;
		}

		#region props
		[XmlIgnore()]
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if( _isSelected != value )
				{
					_isSelected = value;
					this.OnPropertyChanged( "IsSelected" );
				}
			}
		}

		[XmlIgnore()]
		public bool IsDeleted
		{
			get { return _isDeleted; }
			set
			{
				if( _isDeleted != value )
				{
					_isDeleted = value;
					this.OnPropertyChanged( "IsDeleted" );
				}
			}
		}

		[XmlAttribute()]
		public long Id
		{
			get { return _id; }
			set
			{
				if( value != _id )
				{
					_id = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Id" );
				}
			}
		}

		[XmlIgnore()]
		public ss.AceType AceType { get; set; }
		[XmlIgnore()]
		public object Right
		{
			get { return _right; }
			set
			{
				if( value != _right )
				{
					_right = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Right" );
				}
			}
		}

		[XmlAttribute()]
		public bool Allowed
		{
			get { return _allowed; }
			set
			{
				if( value != _allowed )
				{
					_allowed = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Allowed" );
				}
			}
		}

		[XmlAttribute()]
		public bool Inherit
		{
			get { return _inherit; }
			set
			{
				if( value != _inherit )
				{
					_inherit = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Inherit" );
				}
			}
		}

		[XmlIgnore()]
		public string InheritedFrom { get; set; }
		[XmlIgnore()]
		public ArrayList Parameters { get; set; }
		#endregion

		#region IObjectModel Members
		public string Name { get { return null; } set { } }
		public sf.ObjectType ObjectType { get { return sf.ObjectType.Ace; } }
		public sf.ObjectType ValidChildObjectTypes { get { return sf.ObjectType.None; } }
		public bool SupportsChildObjectType(sf.ObjectType objectType)
		{
			return (this.ValidChildObjectTypes & objectType) == objectType;
		}
		[XmlIgnore()]
		public sf.IObjectModel ParentObject { get; set; }
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
		public abstract object Clone();

		public AccessControlEntryBase CloneMemberwise()
		{
			//AccessControlEntryBase ace = 
			//ss.AceCloner.Clone( this, ace );
			//ace.Id = this.Id;
			//ace.SecurityPrincipal = this.SecurityPrincipal;
			//ace.SecurityPrincipalId = this.SecurityPrincipalId;
			////ace.UIElement = this.UIElement;
			////ace.UIElementId = this.UIElementId;

			//undid this 12/24/2009 in favor of below
			//return (AccessControlEntryBase)this.MemberwiseClone();

			//added a line to specifically set the SecurityPrincipal to maintain the integrity of the reference:
			//	the MemberwiseClone() does not provide a reference to the same SecurityPrincipal object; rather, it returns a Clone
			AccessControlEntryBase ace = (AccessControlEntryBase)this.MemberwiseClone();
			ace.SecurityPrincipal = this.SecurityPrincipal;
			return ace;
		}
		#endregion


		#region special SuplexApp UI features
		//[XmlIgnore]
		//public UIElement UIElement
		//{
		//    get { return _uie; }
		//    set
		//    {
		//        _uie = value;
		//        this.UIElementId = _uie.Id;
		//    }
		//}
		//[XmlAttribute()]
		//public Guid UIElementId { get; set; }

		[XmlIgnore]
		public st.ISecurityPrincipal SecurityPrincipal
		{
			get
			{
				//if( _securityPrincipal == null && !string.IsNullOrEmpty( this.SecurityPrincipalId ) &&
				//    this.ParentObject != null && ((AceCollection)this.ParentObject).OwnerStore != null )
				//{
				//    this.Resolve( ((AceCollection)this.ParentObject).OwnerStore );
				//}
				return _securityPrincipal;
			}
			set
			{
				//note: this isn't wrapped in if( _securityPrincipal != value ) { }

				if( _securityPrincipal != null )
				{
					((SecurityPrincipalBase)_securityPrincipal).PropertyChanged -= this.SecurityPrincipal_PropertyChanged;
				}

				_securityPrincipal = value;

				if( _securityPrincipal != null )
				{
					this.SecurityPrincipalId = _securityPrincipal.Id;
					((SecurityPrincipalBase)_securityPrincipal).PropertyChanged += new PropertyChangedEventHandler( this.SecurityPrincipal_PropertyChanged );
				}

				this.IsDirty = true;
				this.OnPropertyChanged( "SecurityPrincipal" );
			}
		}

		protected void SecurityPrincipal_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if( e.PropertyName == "IsDeleted" )
			{
				this.IsDeleted = true;
			}
		}

		[XmlAttribute()]
		public string SecurityPrincipalId { get; set; }

		internal void Resolve(SuplexStore suplexStore)
		{
			//this.UIElement = suplexStore.UIElements.GetById( this.UIElementId );
			if( this.SecurityPrincipal == null || this.SecurityPrincipal.Id != this.SecurityPrincipalId )
			{
				Group g = null;
				if( suplexStore.Groups.GetById( this.SecurityPrincipalId, out g ) )
				{
					this.SecurityPrincipal = g;
				}
				else
				{
					//this occurs when a SecurityPrincipal is deleted /before/ a Dacl/Sacl is lazy-resolved:
					// that is, if just the this.SecurityPrincipalId (string) is avail, then this.SecurityPrincipal has not yet
					// been resolved, so kill this Ace if the SecurityPrincipal no longer exists in the datastore
					this.IsDeleted = true;
				}
			}
		}
		#endregion

		public void Synchronize(AccessControlEntryBase ace)
		{
			this.Allowed = ace.Allowed;
			this.Inherit = ace.Inherit;
			this.Right = ace.Right;
			//this.SecurityPrincipal = ace.SecurityPrincipal;

			if( this.SecurityPrincipal != ace.SecurityPrincipal )
			{
				((SecurityPrincipalBase)this.SecurityPrincipal).PropertyChanged -= this.SecurityPrincipal_PropertyChanged;
				this.SecurityPrincipal = ace.SecurityPrincipal;
			}
		}

		#region INotifyPropertyChanged Members
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string propertyName)
		{
			if( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
			}
		}
		#endregion

		#region IDatabaseObject Members
		internal virtual void RecordSelect()
		{
			throw new NotImplementedException();
		}

		internal virtual void RecordUpsert(Guid uielementId, ref SqlTransaction tr)
		{
			SortedList inparms = this.GetParms( uielementId );

			int idAsInt = -1;
			Int32.TryParse( this.Id.ToString(), out idAsInt );

			SqlParameter id = new SqlParameter( "@SPLX_ACE_ID", SqlDbType.Int );
			id.Value = idAsInt;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_ACE_ID", id );

			StoreConnection.da.ExecuteSP( "splx_api_upsert_ace", inparms, ref outparms, false, tr );
			this.Id = (int)id.Value;
			this.IsDirty = false;
		}

		protected virtual SortedList GetParms(Guid uieId)
		{
			sSortedList s = new sSortedList( "@ACE_TRUSTEE_USER_GROUP_ID", this.SecurityPrincipalId );
			s.Add( "@SPLX_UI_ELEMENT_ID", uieId );
			s.Add( "@ACE_ACCESS_MASK", (int)this.Right );
			s.Add( "@ACE_ACCESS_TYPE1", this.Allowed );
			s.Add( "@ACE_INHERIT", this.Inherit );
			s.Add( "@ACE_TYPE", string.Format( "{0}Ace", this.AceType.ToString() ) );

			return s;
		}

		internal virtual void RecordDelete()
		{
			SortedList inparms = new sSortedList( "@SPLX_ACE_ID", this.Id );
			StoreConnection.da.ExecuteSP( "splx_api_del_ace", inparms );
		}
		#endregion
	}

	public abstract class AccessControlEntryAuditBase : AccessControlEntryBase, ss.IAccessControlEntryAudit
	{
		private bool _denied = false;

		public AccessControlEntryAuditBase() : base() { }

		[XmlAttribute()]
		public bool Denied
		{
			get { return _denied; }
			set
			{
				if( value != _denied )
				{
					_denied = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "Denied" );
				}
			}
		}

		new public AccessControlEntryAuditBase CloneMemberwise()
		{
			//undid 12/26/2009 to parallel AccessControlEntryBase
			//return (AccessControlEntryAuditBase)this.MemberwiseClone();

			//added a line to specifically set the SecurityPrincipal to maintain the integrity of the reference:
			//	the MemberwiseClone() does not provide a reference to the same SecurityPrincipal object; rather, it returns a Clone
			AccessControlEntryAuditBase ace = (AccessControlEntryAuditBase)this.MemberwiseClone();
			ace.SecurityPrincipal = this.SecurityPrincipal;
			return ace;
		}

		public void Synchronize(AccessControlEntryAuditBase ace)
		{
			this.Allowed = ace.Allowed;
			this.Denied = ace.Denied;
			this.Inherit = ace.Inherit;
			this.Right = ace.Right;
			//this.SecurityPrincipal = ace.SecurityPrincipal;

			if( this.SecurityPrincipal != ace.SecurityPrincipal )
			{
				((SecurityPrincipalBase)this.SecurityPrincipal).PropertyChanged -= base.SecurityPrincipal_PropertyChanged;
				this.SecurityPrincipal = ace.SecurityPrincipal;
			}
		}

		#region IDatabaseObject
		protected override SortedList GetParms(Guid uieId)
		{
			SortedList s = base.GetParms( uieId );
			s.Add( "@ACE_ACCESS_TYPE2", this.Denied );
			s["@ACE_TYPE"] = string.Format( "{0}AuditAce", this.AceType.ToString() );
			return s;
		}
		#endregion
	}

	public interface IAceRight<T>
	{
		T iRight { get; set; }
	}

	public class AccessControlEntry<T> : AccessControlEntryBase, IAceRight<T>
	{
		public AccessControlEntry()
			: base()
		{
			this.AceType = ss.AceTypeRights.GetAceType( typeof( T ) );
		}

		public AccessControlEntry(T right, bool allowed)
			: this()
		{
			this.iRight = right;
			this.Allowed = allowed;
		}


		[XmlAttribute( "Right" )]
		public T iRight
		{
			get { return (T)base.Right; }
			set { base.Right = value; }
		}

		#region ICloneable Members
		public override object Clone()
		{
			AccessControlEntry<T> ace = new AccessControlEntry<T>();
			ss.AceCloner.Clone( this, ace );
			ace.Id = this.Id;
			ace.SecurityPrincipal = this.SecurityPrincipal;
			ace.SecurityPrincipalId = this.SecurityPrincipalId;
			//ace.UIElement = this.UIElement;
			//ace.UIElementId = this.UIElementId;
			return ace;
		}
		#endregion
	}

	public class AccessControlEntryAudit<T> : AccessControlEntryAuditBase, IAceRight<T>
	{
		public AccessControlEntryAudit()
			: base()
		{
			this.AceType = ss.AceTypeRights.GetAceType( typeof( T ) );
		}

		public AccessControlEntryAudit(T right, bool allowed, bool denied)
			: this()
		{
			this.iRight = right;
			this.Allowed = allowed;
			this.Denied = denied;
		}


		[XmlElement( "Right" )]
		public T iRight
		{
			get { return (T)base.Right; }
			set { base.Right = value; }
		}

		public override object Clone()
		{
			AccessControlEntryAudit<T> ace = new AccessControlEntryAudit<T>();
			ss.AceCloner.Clone( this, ace );
			ace.Id = this.Id;
			ace.SecurityPrincipal = this.SecurityPrincipal;
			ace.SecurityPrincipalId = this.SecurityPrincipalId;
			//ace.UIElement = this.UIElement;
			//ace.UIElementId = this.UIElementId;
			return ace;
		}
	}

	public class UIAce : AccessControlEntry<ss.UIRight>
	{
		public UIAce() { }
		public UIAce(ss.UIRight right, bool allowed) : base( right, allowed ) { }
	}
	public class UIAuditAce : AccessControlEntryAudit<ss.UIRight>
	{
		public UIAuditAce() { }
		public UIAuditAce(ss.UIRight right, bool allowed, bool denied) : base( right, allowed, denied ) { }
	}
	public class RecordAce : AccessControlEntry<ss.RecordRight> { }
	public class RecordAuditAce : AccessControlEntryAudit<ss.RecordRight> { }
	public class SynchronizationAce : AccessControlEntry<ss.SynchronizationRight> { }
	public class SynchronizationAuditAce : AccessControlEntryAudit<ss.SynchronizationRight> { }

	public class UIAceDefault : UIAce
	{
		public UIAceDefault() : base()
		{
			base.Allowed = true;
			base.iRight = Suplex.Security.UIRight.FullControl;
		}
		public UIAceDefault(ss.UIRight right, bool allowed) : base( right, allowed ) { }
	}
	public class UIAuditAceDefault : UIAuditAce
	{
		public UIAuditAceDefault()
			: base()
		{
			base.Allowed = true;
			base.Denied = true;
			base.iRight = Suplex.Security.UIRight.FullControl;
		}
		public UIAuditAceDefault(ss.UIRight right, bool allowed, bool denied) : base( right, allowed, denied ) { }
	}


	[XmlInclude( typeof( UIAce ) ), XmlInclude( typeof( RecordAce ) ), XmlInclude( typeof( SynchronizationAce ) )]
	public class AceCollection : ObservableObjectModelCollection<AccessControlEntryBase>
	{
		public AceCollection() : base() { }
		public AceCollection(sf.IObjectModel owner)
			: base( owner )
		{ }

		internal SuplexStore OwnerStore { get; set; }

		//public IEnumerable<AccessControlEntryBase> GetByUIElement(UIElement uie)
		//{
		//    return this.Where( ace => ace.UIElementId == uie.Id );
		//}

		//public IEnumerable<AccessControlEntryBase> GetByUIElement(UIElement uie, bool resolveGroups, bool resolveUIElements)
		//{
		//    IEnumerable<AccessControlEntryBase> aces = this.Where( ace => ace.UIElementId == uie.Id );

		//    if( resolveGroups || resolveUIElements )
		//    {
		//        IEnumerator<AccessControlEntryBase> aceList = aces.GetEnumerator();
		//        while( aceList.MoveNext() )
		//        {
		//            if( resolveGroups )
		//            {
		//                aceList.Current;
		//            }
		//            if( resolveUIElements )
		//            { }
		//        }
		//    }

		//    return aces;
		//}

		public AccessControlEntryBase GetByAceId(long id)
		{
			return this.SingleOrDefault( ace => ace.Id == id );
		}

		public IEnumerable<AccessControlEntryBase> GetBySecurityPrincipal(st.ISecurityPrincipal principal)
		{
			return this.Where( ace => ace.SecurityPrincipalId == principal.Id );
		}

		public void Resolve()
		{
			this.Resolve( this.OwnerStore );
		}
		public void Resolve(SuplexStore ownerStore)
		{
			//foreach( AccessControlEntryBase ace in this.Items )
			//{
			//    ace.Resolve( ownerStore );
			//}
			for( int i = this.Items.Count - 1; i > -1; i-- )
			{
				this.Items[i].Resolve( ownerStore );
				this.Items[i].IsDirty = false;
			}
		}


		internal void LoadFromDataTable(DataTable t, string uieIdFilter)
		{
			if( string.IsNullOrEmpty( uieIdFilter ) )
			{
				throw new ArgumentException( "uieIdFilter cannot be Null or Empty" );
			}

			ss.AceType aceType = ss.AceType.None;
			AccessControlEntryBase ace = null;

			this.Clear();

			string filter = string.Format( "IS_AUDIT_ACE = 0 AND SPLX_UI_ELEMENT_ID = '{0}'", uieIdFilter );
			DataRow[] rows = t.Select( filter );
			foreach( DataRow r in rows )
			{
				aceType = sg.MiscUtils.ParseEnum<ss.AceType>( r["ACE_TYPE"].ToString().Replace( "Ace", string.Empty ) );
				switch( aceType )
				{
					case ss.AceType.UI:
					{
						ace = new UIAce();
						((UIAce)ace).iRight = sg.MiscUtils.ParseEnum<ss.UIRight>( (int)r["ACE_ACCESS_MASK"] );
						break;
					}
					case ss.AceType.Record:
					{
						ace = new RecordAce();
						((RecordAce)ace).iRight = sg.MiscUtils.ParseEnum<ss.RecordRight>( (int)r["ACE_ACCESS_MASK"] );
						break;
					}
					case ss.AceType.Synchronization:
					{
						ace = new SynchronizationAce();
						((SynchronizationAce)ace).iRight = sg.MiscUtils.ParseEnum<ss.SynchronizationRight>( (int)r["ACE_ACCESS_MASK"] );
						break;
					}
				}

				ace.Id = (int)r["SPLX_ACE_ID"];
				ace.Allowed = (bool)r["ACE_ACCESS_TYPE1"];
				ace.Inherit = (bool)r["ACE_INHERIT"];
				ace.SecurityPrincipalId = r["ACE_TRUSTEE_USER_GROUP_ID"].ToString();
				ace.IsDirty = false;

				AccessControlEntryBase exists = this.GetByAceId( ace.Id );
				if( exists == null )
				{
					this.Add( ace );
				}
				else
				{
					exists.Synchronize( ace );
					exists.SecurityPrincipalId = ace.SecurityPrincipalId;
					exists.IsDirty = false;
				}
			}
		}
	}

	[XmlInclude( typeof( UIAuditAce ) ), XmlInclude( typeof( RecordAuditAce ) ), XmlInclude( typeof( SynchronizationAuditAce ) )]
	public class AuditAceCollection : ObservableObjectModelCollection<AccessControlEntryAuditBase>
	{
		public AuditAceCollection() : base() { }
		public AuditAceCollection(sf.IObjectModel owner)
			: base( owner )
		{ }

		internal SuplexStore OwnerStore { get; set; }

		//public IEnumerable<AccessControlEntryAuditBase> GetByUIElement(UIElement uie)
		//{
		//    return this.Where( ace => ace.UIElementId == uie.Id );
		//}

		public AccessControlEntryAuditBase GetByAceId(long id)
		{
			return this.SingleOrDefault( ace => ace.Id == id );
		}

		public IEnumerable<AccessControlEntryAuditBase> GetBySecurityPrincipal(st.ISecurityPrincipal principal)
		{
			return this.Where( ace => ace.SecurityPrincipalId == principal.Id );
		}

		public void Resolve()
		{
			this.Resolve( this.OwnerStore );
		}
		public void Resolve(SuplexStore ownerStore)
		{
			for( int i = this.Items.Count - 1; i > -1; i-- )
			{
				this.Items[i].Resolve( ownerStore );
				this.Items[i].IsDirty = false;
			}
		}

		internal void LoadFromDataTable(DataTable t, string uieIdFilter)
		{
			if( string.IsNullOrEmpty( uieIdFilter ) )
			{
				throw new ArgumentException( "uieIdFilter cannot be Null or Empty" );
			}

			ss.AceType aceType = ss.AceType.None;
			AccessControlEntryAuditBase ace = null;

			this.Clear();

			string filter = string.Format( "IS_AUDIT_ACE = 1 AND SPLX_UI_ELEMENT_ID = '{0}'", uieIdFilter );
			DataRow[] rows = t.Select( filter );
			foreach( DataRow r in rows )
			{
				aceType = sg.MiscUtils.ParseEnum<ss.AceType>( r["ACE_TYPE"].ToString().Replace( "AuditAce", string.Empty ) );
				switch( aceType )
				{
					case ss.AceType.UI:
					{
						ace = new UIAuditAce();
						((UIAuditAce)ace).iRight = sg.MiscUtils.ParseEnum<ss.UIRight>( (int)r["ACE_ACCESS_MASK"] );
						break;
					}
					case ss.AceType.Record:
					{
						ace = new RecordAuditAce();
						((RecordAuditAce)ace).iRight = sg.MiscUtils.ParseEnum<ss.RecordRight>( (int)r["ACE_ACCESS_MASK"] );
						break;
					}
					case ss.AceType.Synchronization:
					{
						ace = new SynchronizationAuditAce();
						((SynchronizationAuditAce)ace).iRight = sg.MiscUtils.ParseEnum<ss.SynchronizationRight>( (int)r["ACE_ACCESS_MASK"] );
						break;
					}
				}

				ace.Id = (int)r["SPLX_ACE_ID"];
				ace.Allowed = (bool)r["ACE_ACCESS_TYPE1"];
				ace.Denied = (bool)r["ACE_ACCESS_TYPE2"];
				ace.Inherit = (bool)r["ACE_INHERIT"];
				ace.SecurityPrincipalId = r["ACE_TRUSTEE_USER_GROUP_ID"].ToString();
				ace.IsDirty = false;

				AccessControlEntryAuditBase exists = this.GetByAceId( ace.Id );
				if( exists == null )
				{
					this.Add( ace );
				}
				else
				{
					exists.Synchronize( ace );
					exists.SecurityPrincipalId = ace.SecurityPrincipalId;
					exists.IsDirty = false;
				}
			}
		}
	}

	[XmlInclude( typeof( UIAce ) ), XmlInclude( typeof( RecordAce ) ), XmlInclude( typeof( SynchronizationAce ) )]
	public class AceCollectionEx<T> : AceCollection where T : AccessControlEntryBase
	{
		public AceCollectionEx() : base() { }
		public AceCollectionEx(sf.IObjectModel owner)
			: base( owner )
		{ }
	}

	[XmlInclude( typeof( UIAuditAce ) ), XmlInclude( typeof( RecordAuditAce ) ), XmlInclude( typeof( SynchronizationAuditAce ) )]
	public class AuditAceCollectionEx<T> : AuditAceCollection where T : AccessControlEntryAuditBase
	{
		public AuditAceCollectionEx() : base() { }
		public AuditAceCollectionEx(sf.IObjectModel owner)
			: base( owner )
		{ }
	}

	public class AceUtil
	{
		public AccessControlEntryBase ConvertAce(AccessControlEntryBase ace, ss.AceType newAceType)
		{
			Type aceType = System.Reflection.Assembly.GetExecutingAssembly().GetType(
				string.Format( "Suplex.Forms.ObjectModel.Api.{0}{1}Ace",
				newAceType, ace is ss.IAccessControlEntryAudit ? "Audit" : string.Empty ) );
			AccessControlEntryBase newAce = (AccessControlEntryBase)Activator.CreateInstance( aceType );

			newAce.Id = ace.Id;
			newAce.Allowed = ace.Allowed;
			newAce.Inherit = ace.Inherit;
			newAce.Right = ace.Right;
			newAce.SecurityPrincipal = ace.SecurityPrincipal;

			if( ace is ss.IAccessControlEntryAudit )
			{
				((ss.IAccessControlEntryAudit)newAce).Denied = ((ss.IAccessControlEntryAudit)ace).Denied;
			}

			return newAce;
		}
		public AccessControlEntryAuditBase ConvertAce(AccessControlEntryAuditBase ace, ss.AceType newAceType)
		{
			Type aceType = System.Reflection.Assembly.GetExecutingAssembly().GetType(
				string.Format( "Suplex.Forms.ObjectModel.Api.{0}{1}Ace",
				newAceType, ace is ss.IAccessControlEntryAudit ? "Audit" : string.Empty ) );
			AccessControlEntryAuditBase newAce = (AccessControlEntryAuditBase)Activator.CreateInstance( aceType );

			newAce.Id = ace.Id;
			newAce.Allowed = ace.Allowed;
			newAce.Denied = ace.Denied;
			newAce.Inherit = ace.Inherit;
			newAce.Right = ace.Right;
			newAce.SecurityPrincipal = ace.SecurityPrincipal;

			return newAce;
		}
	}

	public static class AceUtility
	{
		public static AccessControlEntryBase ConvertAce(AccessControlEntryBase ace, ss.AceType newAceType)
		{
			Type aceType = System.Reflection.Assembly.GetExecutingAssembly().GetType(
				string.Format( "Suplex.Forms.ObjectModel.Api.{0}{1}Ace",
				newAceType, ace is ss.IAccessControlEntryAudit ? "Audit" : string.Empty ) );
			AccessControlEntryBase newAce = (AccessControlEntryBase)Activator.CreateInstance( aceType );

			newAce.Id = ace.Id;
			newAce.Allowed = ace.Allowed;
			newAce.Inherit = ace.Inherit;
			newAce.Right = ace.Right;
			newAce.SecurityPrincipal = ace.SecurityPrincipal;

			if( ace is ss.IAccessControlEntryAudit )
			{
				((ss.IAccessControlEntryAudit)newAce).Denied = ((ss.IAccessControlEntryAudit)ace).Denied;
			}

			return newAce;
		}
	}

	public class RightRole : sf.IRightRole, sf.IObjectModel, INotifyPropertyChanged, ICloneable<RightRole>, INotifyDeleted
	{
		private string _controlUniqueName = string.Empty;
		private ss.AceType _aceType = ss.AceType.None;
		private string _rightName = string.Empty;
		private ss.UIRight _uiRight = ss.UIRight.Visible;
		private object _right = null;
		private bool _isDirty = false;
		private bool _isDeleted = false;

		private long _nextId = System.DateTime.Now.Ticks;
		private long NextId { get { return _nextId++; } }

		public RightRole()
		{
			this.RightRoleType = RightRoleType.Success;
			this.Id = this.NextId;
		}

		public long Id { get; set; }

		//[XmlIgnore]
		public sf.RightRoleType RightRoleType { get; set; }

		private UIElement _uielement = null;
		[XmlIgnore]
		public UIElement UIElement
		{
			get
			{
				return _uielement;
			}
			set
			{
				_uielement = value;
				this.ControlUniqueName = value.UniqueName;
			}
		}

		public string ControlUniqueName
		{
			get { return _controlUniqueName; }
			set
			{
				if( _controlUniqueName != value )
				{
					_controlUniqueName = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "ControlUniqueName" );
				}
			}
		}

		[XmlIgnore]
		public ISecureControl ControlRef { get; set; }

		public ss.AceType AceType
		{
			get { return _aceType; }
			set
			{
				if( _aceType != value )
				{
					_aceType = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "AceType" );
				}
			}
		}

		[XmlIgnore]
		public object Right
		{
			get
			{
				if( _right == null )
				{
					ss.AceTypeRights atr = new ss.AceTypeRights( _aceType );
					return Enum.Parse( atr.Right, _rightName );
				}
				else
				{
					return _right;
				}
			}
			set
			{
				if( _right != value )
				{
					_right = value;
					//this.IsDirty = true;
					this.RightName = _right.ToString();
				}
			}
		}

		[XmlElement( "SourceRight" )]
		public string RightName
		{
			get { return _rightName; }
			set
			{
				if( _rightName != value )
				{
					_rightName = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "RightName" );
				}
			}
		}

		[XmlElement( "DestinationRight" )]
		public ss.UIRight UIRight
		{
			get { return _uiRight; }
			set
			{
				if( _uiRight != value )
				{
					_uiRight = value;
					this.IsDirty = true;
					this.OnPropertyChanged( "UIRight" );
				}
			}
		}


		#region IObjectModel Members
		public string Name { get { return null; } set { } }
		public sf.ObjectType ObjectType { get { return sf.ObjectType.RightRole; } }
		public sf.ObjectType ValidChildObjectTypes { get { return sf.ObjectType.None; } }
		public bool SupportsChildObjectType(sf.ObjectType objectType)
		{
			return (this.ValidChildObjectTypes & objectType) == objectType;
		}
		[XmlIgnore()]
		public sf.IObjectModel ParentObject { get; set; }

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

		#region INotifyPropertyChanged Members
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string propertyName)
		{
			if( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
			}
		}
		#endregion

		#region ICloneable<RightRole> Members
		public sf.IObjectModel Clone(bool generateNewId)
		{
			RightRole clone = this.Clone();
			if( generateNewId )
			{
				clone.Id = this.NextId;
			}
			return clone;
		}

		public RightRole CloneMemberwise()
		{
			return (RightRole)this.MemberwiseClone();
		}

		public RightRole Clone()
		{
			RightRole rr = new RightRole();

			rr.Id = this.Id;
			rr.ControlUniqueName = this.ControlUniqueName;
			rr.AceType = this.AceType;
			rr.RightName = this.RightName;
			rr.UIRight = this.UIRight;
			rr.RightRoleType = this.RightRoleType;

			return rr;
		}

		RightRole ICloneable<RightRole>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		public void Synchronize(RightRole sourceObject)
		{
			this.ControlUniqueName = sourceObject.ControlUniqueName;
			this.AceType = sourceObject.AceType;
			this.RightName = sourceObject.RightName;
			this.UIRight = sourceObject.UIRight;
			this.RightRoleType = sourceObject.RightRoleType;
		}

		void ICloneable<RightRole>.Synchronize(RightRole sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region INotifyDeleted Members
		[XmlIgnore()]
		public bool IsDeleted
		{
			get { return _isDeleted; }
			set
			{
				if( _isDeleted != value )
				{
					_isDeleted = value;
					this.OnPropertyChanged( "IsDeleted" );
				}
			}
		}
		#endregion

		#region IDatabaseObject Members
		internal void RecordSelect()
		{
			throw new NotImplementedException();
		}

		internal void RecordUpsert(Guid uieOrRuleId, ref SqlTransaction tr)
		{
			SortedList inparms = this.GetParms( uieOrRuleId );

			int idAsInt = -1;
			Int32.TryParse( this.Id.ToString(), out idAsInt );

			SqlParameter id = new SqlParameter( "@SPLX_RIGHT_ROLE_ID", SqlDbType.Int );
			id.Value = idAsInt;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_RIGHT_ROLE_ID", id );

			StoreConnection.da.ExecuteSP( "splx_api_upsert_rightrole", inparms, ref outparms, false, tr );
			this.Id = (int)id.Value;

			this.IsDirty = false;
		}

		internal virtual SortedList GetParms(Guid uieOrRuleId)
		{
			sSortedList s = new sSortedList( "@SPLX_UI_ELEMENT_RULE_ID", uieOrRuleId );
			s.Add( "@RR_UIE_UNIQUE_NAME", this.ControlUniqueName );
			s.Add( "@RR_ACE_TYPE", this.AceType.ToString() );
			s.Add( "@RR_RIGHT_NAME", this.RightName );
			s.Add( "@RR_UI_RIGHT", this.UIRight.ToString() );
			s.Add( "@RR_ROLE_TYPE", this.RightRoleType.ToString() );

			return s;
		}

		internal void RecordDelete()
		{
			SortedList inparms = new sSortedList( "@SPLX_RIGHT_ROLE_ID", this.Id );
			StoreConnection.da.ExecuteSP( "splx_api_del_rightrole", inparms );
		}
		#endregion
	}

	public class RightRoleCollection : ObservableObjectModelCollection<RightRole>
	{
		public RightRoleCollection() : base() { }
		public RightRoleCollection(sf.IObjectModel owner)
			: base( owner )
		{ }
		internal SuplexStore OwnerStore { get; set; }

		public RightRole GetByRightRoleId(long id)
		{
			return this.SingleOrDefault( rr => rr.Id == id );
		}


		internal void LoadFromDataTable(DataTable t, sf.RightRoleType rightRoleType, string uieIdFilter)
		{
			if( string.IsNullOrEmpty( uieIdFilter ) )
			{
				throw new ArgumentException( "uieIdFilter cannot be Null or Empty" );
			}

			this.LoadFromDataRows( t.Select( string.Format( "SPLX_UI_ELEMENT_RULE_ID = '{0}'", uieIdFilter ) ), rightRoleType );
		}

		internal void LoadFromDataRows(DataRow[] rows, sf.RightRoleType rightRoleType)
		{
			RightRole role = null;
			RightRole exists = null;

			this.Clear();

			foreach( DataRow r in rows )
			{
				role = new RightRole()
				{
					Id = (int)r["SPLX_RIGHT_ROLE_ID"],
					AceType = sg.MiscUtils.ParseEnum<ss.AceType>( r["RR_ACE_TYPE"].ToString() ),
					RightName = r["RR_RIGHT_NAME"].ToString(),
					ControlUniqueName = r["RR_UIE_UNIQUE_NAME"].ToString(),
					UIRight = sg.MiscUtils.ParseEnum<ss.UIRight>( r["RR_UI_RIGHT"].ToString() ),
					RightRoleType = rightRoleType,

					IsDirty = false
				};

				exists = this.GetByRightRoleId( role.Id );
				if( exists == null )
				{
					this.Add( role );
				}
				else
				{
					exists.Synchronize( role );
					exists.IsDirty = false;
				}
			}
		}
	}

	public class RightRoleRule : LogicRule, sf.IRightRoleRule, sf.IObjectModel, ICloneable<RightRoleRule>, INotifyDeleted
	{
		private sf.IObjectModel _parentObject = null;
		private bool _isDeleted = false;

		public RightRoleRule()
			: base()
		{
			this.LogicRuleType = sf.LogicRuleType.RightRoleIf;
			this.iRightRoleRules = new RightRoleRuleCollection( this );
			this.iElseRules = new RightRoleRuleCollection( this );
			this.iRightRoles = new RightRoleCollection( this );
			this.iElseRoles = new RightRoleCollection( this );

			CollectionContainer irrr = new CollectionContainer() { Collection = this.iRightRoleRules };
			CollectionContainer ieru = new CollectionContainer() { Collection = this.iElseRules };
			CollectionContainer irro = new CollectionContainer() { Collection = this.iRightRoles };
			CollectionContainer iero = new CollectionContainer() { Collection = this.iElseRoles };

			this.ChildObjects = new CompositeCollection();
			this.ChildObjects.Add( irro );
			this.ChildObjects.Add( iero );
			this.ChildObjects.Add( irrr );
			this.ChildObjects.Add( ieru );

			this.IsSealed = false;
		}

		[XmlIgnore()]
		public bool IsSealed { get; internal set; }

		[XmlIgnore()]
		public override ObjectType LogicRuleObjectType { get { return sf.ObjectType.RightRoleRule; } }

		[XmlIgnore()]
		public sf.RightRoleRuleCollection RightRoleRules { get; set; }
		[XmlIgnore()]
		public sf.RightRoleCollection RightRoles { get; set; }
		[XmlIgnore()]
		public sf.RightRoleRuleCollection ElseRules { get; set; }
		[XmlIgnore()]
		public sf.RightRoleCollection ElseRoles { get; set; }

		[XmlArray( "RightRoleRules" )]
		public RightRoleRuleCollection iRightRoleRules { get; set; }
		[XmlArray( "RightRoles" )]
		public RightRoleCollection iRightRoles { get; set; }
		[XmlArray( "ElseRules" )]
		public RightRoleRuleCollection iElseRules { get; set; }
		[XmlArray( "ElseRoles" )]
		public RightRoleCollection iElseRoles { get; set; }

		[XmlIgnore()]
		public CompositeCollection ChildObjects { get; private set; }

		public int CompositeRuleCount
		{
			get
			{
				return this.iRightRoleRules.CompositeRuleCount +
					this.iElseRules.CompositeRuleCount;
			}
		}

		public int CompositeRoleCount
		{
			get
			{
				return this.iRightRoles.Count +
					this.iElseRoles.Count;
			}
		}

		#region IObjectModel Members
		public sf.ObjectType ObjectType { get { return sf.ObjectType.RightRoleRule; } }
		public sf.ObjectType ValidChildObjectTypes { get { return sf.ObjectType.RightRoleRule; } }
		public bool SupportsChildObjectType(sf.ObjectType objectType)
		{
			return (this.ValidChildObjectTypes & objectType) == objectType;
		}
		[XmlIgnore()]
		public sf.IObjectModel ParentObject
		{
			get { return _parentObject; }
			set
			{
				if( _parentObject != value )
				{
					this.OnPropertyChanging( "ParentObject" );
					_parentObject = value;
					this.OnPropertyChanged( "ParentObject" );
				}
			}
		}
		#endregion

		#region IObjectCollectionHost Members
		public void AddChildObject(Suplex.Forms.IObjectModel child)
		{
			switch( child.ObjectType )
			{
				case sf.ObjectType.RightRoleRule:
				{
					this.iRightRoleRules.Add( (RightRoleRule)child );
					break;
				}
			}
		}
		public void RemoveChildObject(Suplex.Forms.IObjectModel child)
		{
			switch( child.ObjectType )
			{
				case sf.ObjectType.RightRoleRule:
				{
					this.iRightRoleRules.Remove( (RightRoleRule)child );
					break;
				}
			}
		}
		#endregion

		#region ICloneable<RightRoleRule> Members
		//TODO: verify that this works; compare to ValidationRule
		public sf.IObjectModel Clone(bool generateNewId)
		{
			RightRoleRule clone = this.Clone();
			if( generateNewId )
			{
				clone.Id = Guid.NewGuid();
			}
			return clone;
		}

		public RightRoleRule CloneMemberwise()
		{
			return (RightRoleRule)this.MemberwiseClone();
		}

		//method-based recursion
		public RightRoleRule Clone()
		{
			RightRoleRule clone = (RightRoleRule)base.Clone( sf.ObjectType.RightRoleRule );

			foreach( RightRoleRule rrr in this.iRightRoleRules )
			{
				clone.iRightRoleRules.Add( rrr.Clone() );
			}

			foreach( RightRole rr in this.iRightRoles )
			{
				clone.iRightRoles.Add( rr.CloneMemberwise() );
			}

			foreach( RightRoleRule rrr in this.iElseRules )
			{
				clone.iElseRules.Add( rrr.Clone() );
			}

			foreach( RightRole rr in this.iElseRoles )
			{
				clone.iElseRoles.Add( rr.CloneMemberwise() );
			}

			return clone;
		}

		RightRoleRule ICloneable<RightRoleRule>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		public void Synchronize(RightRoleRule sourceObject)
		{
			List<long> _deleteRightRoleIds = null;
			List<Guid> _deleteRrRuleIds = null;
			this.SynchronizeRecursive( sourceObject, ref _deleteRrRuleIds, ref _deleteRightRoleIds );
		}

		//method-based recursion
		public void SynchronizeRecursive(RightRoleRule sourceObject, ref List<Guid> deleteRrRuleIds, ref List<long> deleteRightRoleIds)
		{
			base.Synchronize( sourceObject );

			//purge the iRightRoleRules from |this| list that no longer exist in the sourceObject's iRightRoleRules list.
			for( int i = this.iRightRoleRules.Count - 1; i > -1; i-- )
			{
				RightRoleRule found = sourceObject.iRightRoleRules.GetByRightRoleRuleId( this.iRightRoleRules[i].Id );
				if( found == null )
				{
					if( deleteRrRuleIds != null ) { deleteRrRuleIds.Add( this.iRightRoleRules[i].Id ); }
					this.iRightRoleRules.RemoveAt( i );
				}
			}
			//Add the new iRightRoleRules from the sourceObject to |this| list
			foreach( RightRoleRule rrr in sourceObject.iRightRoleRules )
			{
				RightRoleRule found = this.iRightRoleRules.GetByRightRoleRuleId( rrr.Id );
				if( found != null )
				{
					found.SynchronizeRecursive( rrr, ref deleteRrRuleIds, ref deleteRightRoleIds );
				}
				else
				{
					this.iRightRoleRules.Add( rrr.Clone() );
				}
			}

			//purge the RightRoles from |this| list that no longer exist in the sourceObject's RightRoles list.
			for( int i = this.iRightRoles.Count - 1; i > -1; i-- )
			{
				RightRole found = sourceObject.iRightRoles.GetByRightRoleId( this.iRightRoles[i].Id );
				if( found == null )
				{
					if( deleteRightRoleIds != null ) { deleteRightRoleIds.Add( this.iRightRoles[i].Id ); }
					this.iRightRoles.RemoveAt( i );
				}
			}
			//Add the new RightRoles from the sourceObject to |this| list
			foreach( RightRole rr in sourceObject.iRightRoles )
			{
				RightRole found = this.iRightRoles.GetByRightRoleId( rr.Id );
				if( found != null )
				{
					found.Synchronize( rr );
				}
				else
				{
					this.iRightRoles.Add( rr.CloneMemberwise() );
				}
			}

			//purge the iRightRoleRules from |this| list that no longer exist in the sourceObject's iRightRoleRules list.
			for( int i = this.iElseRules.Count - 1; i > -1; i-- )
			{
				RightRoleRule found = sourceObject.iElseRules.GetByRightRoleRuleId( this.iElseRules[i].Id );
				if( found == null )
				{
					if( deleteRrRuleIds != null ) { deleteRrRuleIds.Add( this.iElseRules[i].Id ); }
					this.iElseRules.RemoveAt( i );
				}
			}
			//Add the new iRightRoleRules from the sourceObject to |this| list
			foreach( RightRoleRule rrr in sourceObject.iElseRules )
			{
				RightRoleRule found = this.iElseRules.GetByRightRoleRuleId( rrr.Id );
				if( found != null )
				{
					found.SynchronizeRecursive( rrr, ref deleteRrRuleIds, ref deleteRightRoleIds );
				}
				else
				{
					this.iElseRules.Add( rrr.Clone() );
				}
			}

			//purge the iElseRoles from |this| list that no longer exist in the sourceObject's iElseRoles list.
			for( int i = this.iElseRoles.Count - 1; i > -1; i-- )
			{
				RightRole found = sourceObject.iElseRoles.GetByRightRoleId( this.iElseRoles[i].Id );
				if( found == null )
				{
					if( deleteRightRoleIds != null ) { deleteRightRoleIds.Add( this.iElseRoles[i].Id ); }
					this.iElseRoles.RemoveAt( i );
				}
			}
			//Add the new iElseRoles from the sourceObject to |this| list
			foreach( RightRole rr in sourceObject.iElseRoles )
			{
				RightRole found = this.iElseRoles.GetByRightRoleId( rr.Id );
				if( found != null )
				{
					found.Synchronize( rr );
				}
				else
				{
					this.iElseRoles.Add( rr.CloneMemberwise() );
				}
			}
		}

		#region explicit/not implemented
		void ICloneable<RightRoleRule>.Synchronize(RightRoleRule sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		RightRoleRule ICloneable<RightRoleRule>.Clone()
		{
			throw new NotImplementedException();
		}

		sf.IObjectModel ICloneableObject.Clone(bool generateNewId)
		{
			throw new NotImplementedException();
		}
		#endregion
		#endregion

		#region INotifyDeleted Members
		[XmlIgnore()]
		public bool IsDeleted
		{
			get { return _isDeleted; }
			set
			{
				if( _isDeleted != value )
				{
					_isDeleted = value;
					this.OnPropertyChanged( "IsDeleted" );
				}
			}
		}
		#endregion

		#region IDatabaseObject Members
		public override void RecordSelect()
		{
			if( StoreConnection.IsConnected )
			{
				DataSet ds = StoreConnection.da.GetDataSet( "splx_api_sel_vr",
					new sSortedList( "@SPLX_VALIDATION_RULE_ID", this.Id ) );

				if( ds.Tables[0].Rows.Count > 0 )
				{
					this.LoadFromDataRow( ds.Tables[0].Rows[0] );
				}
				else
				{
					throw new RowNotInTableException( string.Format( "Unable to fetch RightBinding Rule '{0}' from the data store.", this.Name ) );
				}
			}
		}

		internal void LoadFromDataRow(DataRow r)
		{
			Id = new Guid( r["SPLX_VALIDATION_RULE_ID"].ToString() );
			Name = r["VR_NAME"].ToString();
			SortOrder = (int)r["VR_SORT_ORDER"];

			iCompareValue1 = r["VR_COMPARE_VALUE1"].ToString();

			ValueType1 = Convert.IsDBNull( r["VR_VALUE_TYPE1"] ) ? sf.ComparisonValueType.Empty :
				sg.MiscUtils.ParseEnum<sf.ComparisonValueType>( r["VR_VALUE_TYPE1"] );

			ExpressionType1 = Convert.IsDBNull( r["VR_EXPRESSION_TYPE1"] ) ? sf.ExpressionType.None :
				sg.MiscUtils.ParseEnum<sf.ExpressionType>( r["VR_EXPRESSION_TYPE1"] );

			iCompareValue2 = r["VR_COMPARE_VALUE2"].ToString();

			ValueType2 = Convert.IsDBNull( r["VR_VALUE_TYPE2"] ) ? sf.ComparisonValueType.Empty :
				sg.MiscUtils.ParseEnum<ComparisonValueType>( r["VR_VALUE_TYPE2"] );

			ExpressionType2 = Convert.IsDBNull( r["VR_EXPRESSION_TYPE2"] ) ? sf.ExpressionType.None :
				sg.MiscUtils.ParseEnum<sf.ExpressionType>( r["VR_EXPRESSION_TYPE2"] );

			CompareDataType = Convert.IsDBNull( r["VR_COMPARE_DATA_TYPE"] ) ? TypeCode.Empty :
				sg.MiscUtils.ParseEnum<TypeCode>( r["VR_COMPARE_DATA_TYPE"] );

			Operator = Convert.IsDBNull( r["VR_OPERATOR"] ) ? sf.ComparisonOperator.Empty :
				sg.MiscUtils.ParseEnum<sf.ComparisonOperator>( r["VR_OPERATOR"] );

			FailParent = (bool)r["VR_FAIL_PARENT"];
			ErrorMessage = null;

			LogicRuleType = sg.MiscUtils.ParseEnum<sf.LogicRuleType>( r["VR_RULE_TYPE"] );
		}

		#region not implemented
		public override void RecordUpsert()
		{
			throw new NotImplementedException();
		}
		#endregion

		public void RecordUpsert(Guid parentUIElementId, Guid parRuleId, ref SqlTransaction tr)
		{
			SortedList inparms = this.GetParms( parentUIElementId, parRuleId );

			SqlParameter id = new SqlParameter( "@SPLX_VALIDATION_RULE_ID", SqlDbType.UniqueIdentifier );
			id.Value = this.Id;
			id.Direction = ParameterDirection.InputOutput;
			SortedList outparms = new sSortedList( "@SPLX_VALIDATION_RULE_ID", id );

			StoreConnection.da.ExecuteSP( "splx_api_upsert_vr", inparms, ref outparms, false, tr );
			this.Id = (Guid)id.Value;
			this.IsDirty = false;
		}

		private SortedList GetParms(Guid parentUIElementId, Guid parRuleId)
		{
			sSortedList s = new sSortedList( "@VR_NAME", this.Name );
			s.Add( "@VR_EVENT_BINDING", Convert.DBNull );			//doesn't apply to RightRoleRules, only to ValidationRules
			s.Add( "@VR_SORT_ORDER", this.SortOrder );

			s.Add( "@VR_COMPARE_DATA_TYPE", this.CompareDataType.ToString() );
			s.Add( "@VR_OPERATOR", this.Operator.ToString() );

			s.Add( "@VR_COMPARE_VALUE1", this.iCompareValue1 );
			s.Add( "@VR_EXPRESSION_TYPE1", this.ExpressionType1.ToString() );
			s.Add( "@VR_VALUE_TYPE1", this.ValueType1.ToString() );

			s.Add( "@VR_COMPARE_VALUE2", this.iCompareValue2 );
			s.Add( "@VR_EXPRESSION_TYPE2", this.ExpressionType2.ToString() );
			s.Add( "@VR_VALUE_TYPE2", this.ValueType2.ToString() );

			s.Add( "@VR_FAIL_PARENT", this.FailParent );
			s.Add( "@VR_ERROR_MESSAGE", Convert.DBNull );			//doesn't apply to RightRoleRules, only to ValidationRules
			s.Add( "@VR_ERROR_UIE_UNIQUE_NAME", Convert.DBNull );	//doesn't apply to RightRoleRules, only to ValidationRules

			s.Add( "@VR_RULE_TYPE", this.LogicRuleType.ToString() );
			s.Add( "@VR_PARENT_ID", parRuleId == Guid.Empty ? Convert.DBNull : parRuleId );
			s.Add( "@SPLX_UI_ELEMENT_ID", parentUIElementId );

			return s;
		}

		//public override void RecordDelete()	//Implemented on LogicRule

		internal override void RecordDeleteRecursive(SqlTransaction tr)
		{
			//walk down the heirarchy to the bottom, delete on the way back up.
			foreach( RightRoleRule rr in this.iRightRoleRules )
			{
				rr.RecordDeleteRecursive( tr );
			}

			//walk down the heirarchy to the bottom, delete on the way back up.
			foreach( RightRoleRule rr in this.iElseRules )
			{
				rr.RecordDeleteRecursive( tr );
			}

			SortedList inparms = new sSortedList( "@SPLX_VALIDATION_RULE_ID", this.Id );
			StoreConnection.da.ExecuteSP( "splx_api_del_vr", inparms, false, tr );
		}
		#endregion
	}

	public class RightRoleRuleCollection : ObservableObjectModelCollection<RightRoleRule>
	{
		private DataTable _rulesLoadTable = null;
		private DataTable _rightRolesLoadTable = null;

		public RightRoleRuleCollection() : base() { }
		public RightRoleRuleCollection(sf.IObjectModel owner)
			: base( owner )
		{ }

		public RightRoleRule GetByRightRoleRuleId(Guid id)
		{
			return this.SingleOrDefault( rrr => rrr.Id == id );
		}

		internal void LoadFromDataTable(DataTable rulesTable, DataTable rightRolesTable, string uieIdFilter)
		{
			if( string.IsNullOrEmpty( uieIdFilter ) )
			{
				throw new ArgumentException( "uieIdFilter cannot be Null or Empty" );
			}

			_rulesLoadTable = rulesTable;
			_rightRolesLoadTable = rightRolesTable;
			this.RecurseRules( null, this, LogicRuleType.RightRoleIf, uieIdFilter );
			_rulesLoadTable = null;
			_rightRolesLoadTable = null;
		}

		private void RecurseRules(string parentId, RightRoleRuleCollection rules, sf.LogicRuleType logicRuleType, string uieIdFilter)
		{
			DataRow[] rows = null;
			if( string.IsNullOrEmpty( parentId ) )
			{
				rows = _rulesLoadTable.Select( string.Format( "VR_PARENT_ID IS NULL AND VR_RULE_TYPE = '{0}' AND SPLX_UI_ELEMENT_ID = '{1}'", logicRuleType, uieIdFilter ) );
			}
			else
			{
				rows = _rulesLoadTable.Select( string.Format( "VR_PARENT_ID = '{0}' AND VR_RULE_TYPE = '{1}' AND SPLX_UI_ELEMENT_ID = '{2}'", parentId, logicRuleType, uieIdFilter ) );
			}

			foreach( DataRow r in rows )
			{
				RightRoleRule rrr = new RightRoleRule();
				rrr.LoadFromDataRow( r );

				RightRoleRule exists = this.GetByRightRoleRuleId( rrr.Id );
				if( exists == null )
				{
					rules.Add( rrr );
				}
				else
				{
					exists.Synchronize( rrr );
					rrr = exists;
				}

				DataRow[] roles = _rightRolesLoadTable.Select(
					string.Format( "SPLX_UI_ELEMENT_RULE_ID = '{0}' AND RR_ROLE_TYPE = '{1}'",
					r["SPLX_VALIDATION_RULE_ID"].ToString(), sf.RightRoleType.Success ) );
				rrr.iRightRoles.LoadFromDataRows( roles, sf.RightRoleType.Success );

				roles = _rightRolesLoadTable.Select(
					string.Format( "SPLX_UI_ELEMENT_RULE_ID = '{0}' AND RR_ROLE_TYPE = '{1}'",
					r["SPLX_VALIDATION_RULE_ID"].ToString(), sf.RightRoleType.Else ) );
				rrr.iElseRoles.LoadFromDataRows( roles, sf.RightRoleType.Else );

				this.RecurseRules( r["SPLX_VALIDATION_RULE_ID"].ToString(), rrr.iRightRoleRules, LogicRuleType.RightRoleIf, uieIdFilter );
				this.RecurseRules( r["SPLX_VALIDATION_RULE_ID"].ToString(), rrr.iElseRules, LogicRuleType.RightRoleElse, uieIdFilter );

				rrr.IsDirty = false;
			}
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			switch( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
				{
					this.AddToCount( e.NewItems );
					break;
				}
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
				{
					this.AddToCount( e.NewItems );
					this.RemoveFromCount( e.OldItems );
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					this.RemoveFromCount( e.OldItems );
					break;
				}
			}

			base.OnCollectionChanged( e );
		}

		private void AddToCount(IList items)
		{
			if( items != null && items.Count > 0 && items[0] is sf.IObjectModel )
			{
				RightRoleRule r = null;
				for( int n = 0; n < items.Count; n++ )
				{
					r = (RightRoleRule)items[n];

					this.CompositeRuleCount += r.CompositeRuleCount + 1;
					this.CompositeRoleCount += r.CompositeRoleCount;
				}
			}
		}

		private void RemoveFromCount(IList items)
		{
			if( items != null && items.Count > 0 && items[0] is sf.IObjectModel )
			{
				RightRoleRule r = null;
				for( int n = 0; n < items.Count; n++ )
				{
					r = (RightRoleRule)items[n];

					this.CompositeRuleCount -= (r.CompositeRuleCount + 1);
					this.CompositeRoleCount -= r.CompositeRoleCount;
				}
			}
		}

		public int CompositeRuleCount { get; internal set; }
		public int CompositeRoleCount { get; internal set; }
	}

	//TODO: override serialization to handle having "default" settings (don't ser unless something was changed)
	//[Obsolete()]
	public class SecurityDescriptor : sf.ICloneable<SecurityDescriptor>, INotifyPropertyChanged
	{
		public const ss.AuditType DefaultSaclAuditTypeFilter =
			ss.AuditType.Information | ss.AuditType.Warning | ss.AuditType.Error |
			ss.AuditType.SuccessAudit | ss.AuditType.FailureAudit;

		private bool _daclInherit = true;
		private bool _saclInherit = true;
		private ss.AuditType _saclAuditTypeFilter = DefaultSaclAuditTypeFilter;

		private CompositeCollection _rightBindings = null;

		AceEqualityComparer _aceComparer = new AceEqualityComparer();

		public SecurityDescriptor() { }
		public SecurityDescriptor(UIElement owner)
		{
			this.Owner = owner;

			Dacl = new AceCollectionEx<UIAceDefault>( owner );
			Sacl = new AuditAceCollectionEx<UIAuditAceDefault>( owner );
			RightRoles = new RightRoleCollection( owner );
			iRightRoleRules = new RightRoleRuleCollection( owner );

			this.Dacl.CollectionChanged += new NotifyCollectionChangedEventHandler( Child_CollectionChanged );
			this.Sacl.CollectionChanged += new NotifyCollectionChangedEventHandler( Child_CollectionChanged );
			this.RightRoles.CollectionChanged += new NotifyCollectionChangedEventHandler( Child_CollectionChanged );
			this.iRightRoleRules.CollectionChanged += new NotifyCollectionChangedEventHandler( Child_CollectionChanged );
		}

		void Child_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.OnPropertyChanged( "Child_CollectionChanged" );
		}

		private void InitRightBindings()
		{
			RightRoleRule rightRolesHost = new RightRoleRule()
			{
				IsSealed = true,
				Name = "[Always]",
				iRightRoles = this.RightRoles
			};
			CollectionContainer rr = new CollectionContainer() { Collection = this.RightRoles };
			rightRolesHost.ChildObjects.Clear();
			rightRolesHost.ChildObjects.Add( rr );

			CollectionContainer irrr = new CollectionContainer() { Collection = this.iRightRoleRules };

			this.RightBindings = new CompositeCollection();
			this.RightBindings.Add( rightRolesHost );
			this.RightBindings.Add( irrr );
		}

		[XmlIgnore()]
		public UIElement Owner { get; set; }

		public bool DaclInherit
		{
			get { return _daclInherit; }
			set
			{
				if( _daclInherit != value )
				{
					_daclInherit = value;
					this.OnPropertyChanged( "DaclInherit" );
					this.OnPropertyChanged( "IsDefaultInheritanceSettings" );
				}
			}
		}
		public bool SaclInherit
		{
			get { return _saclInherit; }
			set
			{
				if( _saclInherit != value )
				{
					_saclInherit = value;
					this.OnPropertyChanged( "SaclInherit" );
					this.OnPropertyChanged( "IsDefaultInheritanceSettings" );
				}
			}
		}
		public ss.AuditType SaclAuditTypeFilter
		{
			get { return _saclAuditTypeFilter; }
			set
			{
				if( _saclAuditTypeFilter != value )
				{
					_saclAuditTypeFilter = value;
					this.OnPropertyChanged( "SaclAuditTypeFilter" );
					this.OnPropertyChanged( "IsDefaultSaclAuditTypeFilter" );
					this.OnPropertyChanged( "IsDefaultInheritanceSettings" );
				}
			}
		}

		[XmlIgnore()]
		public bool IsDefaultSaclAuditTypeFilter
		{
			get
			{
				return SaclAuditTypeFilter == DefaultSaclAuditTypeFilter;
			}
		}
		[XmlIgnore()]
		public bool IsDefaultInheritanceSettings
		{
			get
			{
				return DaclInherit && SaclInherit && (SaclAuditTypeFilter == DefaultSaclAuditTypeFilter);
			}
		}

		/// <summary>
		///this is a work-around prop for AceCollection/AuditAceCollection b/c
		///   the WpfToolkit DataGrid has a limitation that requires a generic argument
		///	  to provide a blank row for an empty collection
		/// </summary>
		public AceCollectionEx<UIAceDefault> Dacl { get; set; }
		public AuditAceCollectionEx<UIAuditAceDefault> Sacl { get; set; }
		public RightRoleCollection RightRoles { get; set; }
		[XmlArray( "RightRoleRules" )]
		public RightRoleRuleCollection iRightRoleRules { get; set; }

		[XmlIgnore()]
		public int CompositeRightRuleCount
		{
			get { return this.iRightRoleRules.CompositeRuleCount; }
			set { this.OnPropertyChanged( "CompositeRightRuleCount" ); }
		}
		[XmlIgnore()]
		public int CompositeRightRoleCount
		{
			get { return this.RightRoles.Count + this.iRightRoleRules.CompositeRoleCount; }
			set { this.OnPropertyChanged( "CompositeRightRoleCount" ); }
		}
		//HACK: this is stub method to cover updating
		public void UpdateCompositeRightBindingCounts()
		{
			this.CompositeRightRuleCount = -1;
			this.CompositeRightRoleCount = -1;
		}

		[XmlIgnore()]
		public CompositeCollection RightBindings
		{
			get
			{
				if( _rightBindings == null )
				{
					this.InitRightBindings();
				}
				return _rightBindings;
			}
			private set { _rightBindings = value; }
		}

		#region ICloneable<SecurityDescriptor> Members
		public SecurityDescriptor Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			SecurityDescriptor sd = new SecurityDescriptor( this.Owner );

			if( (cloneDepth & ObjectType.Ace) == ObjectType.Ace )
			{
				sd.DaclInherit = this.DaclInherit;
				sd.SaclInherit = this.SaclInherit;
				sd.SaclAuditTypeFilter = this.SaclAuditTypeFilter;

				foreach( AccessControlEntryBase ace in this.Dacl )
				{
					if( cloneChildrenAsRef )
					{
						sd.Dacl.Add( ace );
					}
					else
					{
						sd.Dacl.Add( ace.CloneMemberwise() );
					}
				}
				foreach( AccessControlEntryAuditBase ace in this.Sacl )
				{
					if( cloneChildrenAsRef )
					{
						sd.Sacl.Add( ace );
					}
					else
					{
						sd.Sacl.Add( ace.CloneMemberwise() );
					}
				}
			}

			if( (cloneDepth & ObjectType.RightRole) == ObjectType.RightRole )
			{
				foreach( RightRole rr in this.RightRoles )
				{
					if( cloneChildrenAsRef )
					{
						sd.RightRoles.Add( rr );
					}
					else
					{
						sd.RightRoles.Add( rr.CloneMemberwise() );
					}
				}
			}

			if( (cloneDepth & ObjectType.RightRoleRule) == ObjectType.RightRoleRule )
			{
				foreach( RightRoleRule rrr in this.iRightRoleRules )
				{
					if( cloneChildrenAsRef )
					{
						sd.iRightRoleRules.Add( rrr );
					}
					else
					{
						sd.iRightRoleRules.Add( rrr.Clone() );
					}
				}
			}

			return sd;
		}

		internal List<long> DeleteDaclIds = null;
		internal List<long> DeleteSaclIds = null;
		internal List<long> DeleteRightRoleIds = null;
		internal List<Guid> DeleteRightRoleRuleIds = null;
		internal bool HaveDeleteIds
		{
			get
			{
				return
					(this.DeleteDaclIds != null &&
					this.DeleteSaclIds != null &&
					this.DeleteRightRoleIds != null &&
					this.DeleteRightRoleRuleIds != null)
					&&
					(this.DeleteDaclIds.Count > 0 ||
					this.DeleteSaclIds.Count > 0 ||
					this.DeleteRightRoleIds.Count > 0 ||
					this.DeleteRightRoleRuleIds.Count > 0);
			}
		}
		public void Synchronize(SecurityDescriptor sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			if( (cloneDepth & ObjectType.Ace) == ObjectType.Ace )
			{
				DeleteDaclIds = new List<long>();
				DeleteSaclIds = new List<long>();
				DeleteRightRoleIds = new List<long>();
				DeleteRightRoleRuleIds = new List<Guid>();

				this.DaclInherit = sourceObject.DaclInherit;
				this.SaclInherit = sourceObject.SaclInherit;
				this.SaclAuditTypeFilter = sourceObject.SaclAuditTypeFilter;

				//purge the Aces from |this| list that no longer exist in the sourceObject's Ace list.
				for( int i = this.Dacl.Count - 1; i > -1; i-- )
				{
					AccessControlEntryBase found = sourceObject.Dacl.GetByAceId( this.Dacl[i].Id );
					if( found == null )
					{
						DeleteDaclIds.Add( this.Dacl[i].Id );
						this.Dacl.RemoveAt( i );
					}
				}
				//Add the new Aces from the sourceObject to |this| list
				foreach( AccessControlEntryBase ace in sourceObject.Dacl )
				{

					if( cloneChildrenAsRef )
					{
						this.Dacl.Add( ace );
					}
					else
					{
						//int i = this.Dacl.IndexOf( ace );
						//bool exists = this.Dacl.Contains<AccessControlEntryBase>( ace, _aceComparer );
						//found.Allowed = ace.Allowed;
						//found.Inherit = ace.Inherit;
						//found.Right = ace.Right;
						//found.SecurityPrincipal = ace.SecurityPrincipal;

						AccessControlEntryBase found = this.Dacl.GetByAceId( ace.Id );
						if( found != null )
						{
							if( found.AceType == ace.AceType )
							{
								if( ace.IsDirty )
								{
									found.Synchronize( ace );
								}
							}
							else
							{
								int i = this.Dacl.IndexOf( found );
								this.Dacl[i] = ace.CloneMemberwise();
							}
						}
						else
						{
							this.Dacl.Add( ace.CloneMemberwise() );
						}
					}

					ace.IsDirty = false;
				}

				//purge the Aces from |this| list that no longer exist in the sourceObject's Ace list.
				for( int i = this.Sacl.Count - 1; i > -1; i-- )
				{
					AccessControlEntryAuditBase found = sourceObject.Sacl.GetByAceId( this.Sacl[i].Id );
					if( found == null )
					{
						DeleteSaclIds.Add( this.Sacl[i].Id );
						this.Sacl.RemoveAt( i );
					}
				}
				//Add the new Aces from the sourceObject to |this| list
				foreach( AccessControlEntryAuditBase ace in sourceObject.Sacl )
				{
					if( cloneChildrenAsRef )
					{
						this.Sacl.Add( ace );
					}
					else
					{
						AccessControlEntryAuditBase found = this.Sacl.GetByAceId( ace.Id );
						if( found != null )
						{
							if( found.AceType == ace.AceType )
							{
								if( ace.IsDirty )
								{
									found.Synchronize( ace );
								}
							}
							else
							{
								int i = this.Sacl.IndexOf( found );
								this.Sacl[i] = ace;
							}
						}
						else
						{
							this.Sacl.Add( ace.CloneMemberwise() );
						}
					}
				}
			}


			//purge the RightRoles from |this| list that no longer exist in the sourceObject's RightRoles list.
			for( int i = this.RightRoles.Count - 1; i > -1; i-- )
			{
				RightRole found = sourceObject.RightRoles.GetByRightRoleId( this.RightRoles[i].Id );
				if( found == null )
				{
					DeleteRightRoleIds.Add( this.RightRoles[i].Id );
					this.RightRoles.RemoveAt( i );
				}
			}
			//Add the new RightRoles from the sourceObject to |this| list
			if( (cloneDepth & ObjectType.RightRole) == ObjectType.RightRole )
			{
				foreach( RightRole rr in sourceObject.RightRoles )
				{
					if( cloneChildrenAsRef )
					{
						this.RightRoles.Add( rr );
					}
					else
					{
						RightRole found = this.RightRoles.GetByRightRoleId( rr.Id );
						if( found != null )
						{
							found.Synchronize( rr );
						}
						else
						{
							this.RightRoles.Add( rr.CloneMemberwise() );
						}
					}
				}
			}

			//purge the iRightRoleRules from |this| list that no longer exist in the sourceObject's iRightRoleRules list.
			for( int i = this.iRightRoleRules.Count - 1; i > -1; i-- )
			{
				RightRoleRule found = sourceObject.iRightRoleRules.GetByRightRoleRuleId( this.iRightRoleRules[i].Id );
				if( found == null )
				{
					DeleteRightRoleRuleIds.Add( this.iRightRoleRules[i].Id );
					this.iRightRoleRules.RemoveAt( i );
				}
			}
			//Add the new iRightRoleRules from the sourceObject to |this| list
			if( (cloneDepth & ObjectType.RightRoleRule) == ObjectType.RightRoleRule )
			{
				foreach( RightRoleRule rrr in sourceObject.iRightRoleRules )
				{
					if( cloneChildrenAsRef )
					{
						this.iRightRoleRules.Add( rrr );
					}
					else
					{
						RightRoleRule found = this.iRightRoleRules.GetByRightRoleRuleId( rrr.Id );
						if( found != null )
						{
							//method-based recursion
							found.SynchronizeRecursive( rrr, ref DeleteRightRoleRuleIds, ref DeleteRightRoleIds );
						}
						else
						{
							this.iRightRoleRules.Add( rrr.Clone() );
						}
					}
				}
			}
		}
		#endregion

		#region INotifyPropertyChanged Members
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string propertyName)
		{
			if( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
			}
		}
		#endregion

		#region ICloneable<SecurityDescriptor> Members
		sf.IObjectModel ICloneableObject.Clone(bool generateNewId)
		{
			throw new NotImplementedException();
		}

		SecurityDescriptor ICloneable<SecurityDescriptor>.Clone()
		{
			throw new NotImplementedException();
		}

		SecurityDescriptor ICloneable<SecurityDescriptor>.Clone(ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		void ICloneable<SecurityDescriptor>.Synchronize(SecurityDescriptor sourceObject)
		{
			throw new NotImplementedException();
		}

		void ICloneable<SecurityDescriptor>.Synchronize(SecurityDescriptor sourceObject, ObjectType cloneDepth, bool cloneChildrenAsRef)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class AceEqualityComparer : IEqualityComparer<AccessControlEntryBase>
	{
		#region IEqualityComparer<IAccessControlEntry> Members
		public bool Equals(AccessControlEntryBase x, AccessControlEntryBase y)
		{
			return x.Id == y.Id;
		}

		public int GetHashCode(AccessControlEntryBase obj)
		{
			return obj.GetHashCode();
		}
		#endregion
	}
	#endregion


	public class ObservableObjectModelCollection<T> : ObservableCollection<T>
	{
		private CollectionViewSource _cvs = new CollectionViewSource();

		public ObservableObjectModelCollection()
			: base()
		{
			_cvs.Source = this;
		}
		public ObservableObjectModelCollection(IEnumerable<T> collection)
			: base( collection )
		{
			_cvs.Source = this;
			this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, collection ) );
		}
		public ObservableObjectModelCollection(sf.IObjectModel owner)
			: this()
		{
			this.Owner = owner;
		}

		[XmlIgnore()]
		public CollectionViewSource CollectionViewSource { get { return _cvs; } }


		public sf.IObjectModel Owner { get; set; }

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			switch( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
				{
					this.SetOwner( e.NewItems, this.Owner );
					break;
				}
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
				{
					this.SetOwner( e.NewItems, this.Owner );
					this.SetOwner( e.OldItems, null );
					break;
				}
				case NotifyCollectionChangedAction.Remove:
				{
					this.SetOwner( e.OldItems, null );
					break;
				}
			}

			base.OnCollectionChanged( e );
		}

		private void SetOwner(IList items, sf.IObjectModel owner)
		{
			if( items != null && items.Count > 0 && items[0] is sf.IObjectModel )
			{
				for( int n = 0; n < items.Count; n++ )
				{
					((sf.IObjectModel)items[n]).ParentObject = owner;

					if( items[n] is INotifyPropertyChanged && items[n] is INotifyDeleted )	//owner != null &&
					{
						((INotifyPropertyChanged)items[n]).PropertyChanged += new PropertyChangedEventHandler( this.Item_PropertyChanged );
					}
				}
			}
		}

		void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if( e.PropertyName == "IsDeleted" )
			{
				this.Remove( (T)sender );
			}
		}
	}

	public interface IObjectCollectionHost
	{
		void AddChildObject(sf.IObjectModel child);
		void RemoveChildObject(sf.IObjectModel child);
	}

	public interface INodeItem
	{
		bool IsExpanded { get; set; }
		bool IsSelected { get; set; }
		bool IsEditing { get; set; }
		bool ShowDetail { get; set; }
		bool ShowDetailPanels { get; set; }
		CompositeCollection ChildObjects { get; }
	}

	public interface INotifyDeleted
	{
		bool IsDeleted { get; set; }
	}

	public interface IDatabaseObject
	{
		void RecordSelect();
		void RecordUpsert();
		void RecordDelete();
	}
}