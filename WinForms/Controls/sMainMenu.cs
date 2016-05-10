using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;
using Suplex.Security.Standard;


namespace Suplex.WinForms
{

	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(MainMenu))]
	public class sMainMenu : MainMenu, ISecureContainer
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;

		private sMenuItemCollection			_sMenuItems				= null;


		public sMainMenu() : base()
		{
			this.Initialize();
		}

		public sMainMenu( sMenuItem[] items ) : base( (MenuItem[])items )
		{
			this.Initialize();
		}

		public sMainMenu( string uniqueName, sMenuItem[] items ) : base( (MenuItem[])items )
		{
			this.UniqueName = uniqueName;

			this.Initialize();
		}

		private void Initialize()
		{
			_sMenuItems = new sMenuItemCollection( this );

			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
		}


		public virtual string UniqueName
		{
			get
			{
				if( string.IsNullOrEmpty( _uniqueName ) )
				{
					_uniqueName = this.GetType().ToString() + base.Handle.ToString();
				}

				return _uniqueName;
			}
			set
			{
				_uniqueName = value;
			}
		}


		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Suplex.Data.DataAccessLayer DataAccessLayer
		{
			get
			{
				return _dal;
			}
			set
			{
				_dal = value;
			}
		}


		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		new public sMenuItemCollection MenuItems
		{
			get
			{
				return _sMenuItems;
			}
		}


		new public sContextMenu GetContextMenu()
		{
			return (sContextMenu)base.GetContextMenu();
		}

		new public sMainMenu GetMainMenu()
		{
			return (sMainMenu)base.GetMainMenu();
		}

		#region Security Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Security management properties and tools." ), Category( "Suplex" )]
		public SecurityAccessor Security
		{
			get { return _sa; }
		}

		public void ApplySecurity()
		{
		}

		public virtual IEnumerable GetChildren()
		{
			return (ICollection)this.MenuItems;
		}
		#endregion
	}//sMainMenu



	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(ContextMenu))]
	public class sContextMenu : ContextMenu, ISecureContainer
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;

		private sMenuItemCollection			_sMenuItems				= null;


		public sContextMenu() : base()
		{
			this.Initialize();
		}

		public sContextMenu( sMenuItem[] items ) : base( (MenuItem[])items )
		{
			this.Initialize();
		}

		public sContextMenu( string uniqueName, sMenuItem[] items ) : base( (MenuItem[])items )
		{
			this.UniqueName = uniqueName;

			this.Initialize();
		}

		private void Initialize()
		{
			_sMenuItems = new sMenuItemCollection( this );

			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
		}


		public virtual string UniqueName
		{
			get
			{
				if( string.IsNullOrEmpty( _uniqueName ) )
				{
					_uniqueName = this.GetType().ToString() + base.Handle.ToString();
				}

				return _uniqueName;
			}
			set
			{
				_uniqueName = value;
			}
		}


		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Suplex.Data.DataAccessLayer DataAccessLayer
		{
			get
			{
				return _dal;
			}
			set
			{
				_dal = value;
			}
		}


		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		new public sMenuItemCollection MenuItems
		{
			get
			{
				return _sMenuItems;
			}
		}


		new public sContextMenu GetContextMenu()
		{
			return (sContextMenu)base.GetContextMenu();
		}


		new public sMainMenu GetMainMenu()
		{
			return (sMainMenu)base.GetMainMenu();
		}


		#region Security Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Security management properties and tools." ), Category( "Suplex" )]
		public SecurityAccessor Security
		{
			get { return _sa; }
		}

		public void ApplySecurity()
		{
		}

		public virtual IEnumerable GetChildren()
		{
			return (ICollection)this.MenuItems;
		}
		#endregion
	}//sContextMenu



	public class sMenuItem : MenuItem, ISecureContainer
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;

		private sMenuItemCollection			_sMenuItems				= null;


		public sMenuItem() : base( )
		{
			this.Initialize();
		}


		public sMenuItem(
			MenuMerge mergeType, int mergeOrder, Shortcut shortcut, string uniqueName, string text,
			EventHandler onClick, EventHandler onPopup, EventHandler onSelect, sMenuItem[] items) : 
			base(mergeType, mergeOrder, shortcut, text, onClick, onPopup, onSelect, (MenuItem[])items)
		{
			_uniqueName = uniqueName;

			this.Initialize();
		}


		public sMenuItem(string text, EventHandler onClick, Shortcut shortcut) : base(text, onClick, shortcut)
		{
			this.Initialize();
		}


		public sMenuItem(string text, sMenuItem[] items) : base(text, (MenuItem[])items)
		{
			this.Initialize();
		}


		public sMenuItem(string text, EventHandler onClick) : base(text, onClick)
		{
			this.Initialize();
		}


		public sMenuItem(string uniqueName, string text) : base(text)
		{
			_uniqueName = uniqueName;

			this.Initialize();
		}


		public sMenuItem(string text) : base(text)
		{
			this.Initialize();
		}



		private void Initialize()
		{
			_sMenuItems = new sMenuItemCollection( this );

			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
		}


		public virtual string UniqueName
		{
			get
			{
				if( string.IsNullOrEmpty( _uniqueName ) )
				{
					_uniqueName = base.Text;
				}

				return _uniqueName;
			}
			set
			{
				_uniqueName = value;
			}
		}


		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Suplex.Data.DataAccessLayer DataAccessLayer
		{
			get
			{
				return _dal;
			}
			set
			{
				_dal = value;
			}
		}


		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		new public sMenuItemCollection MenuItems
		{
			get
			{
				return _sMenuItems;
			}
		}


		new public sContextMenu GetContextMenu()
		{
			return (sContextMenu)base.GetContextMenu();
		}


		new public sMainMenu GetMainMenu()
		{
			return (sMainMenu)base.GetMainMenu();
		}
		

		protected override void OnClick(EventArgs e)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Clicked.", false );

			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				base.OnClick( e );
			}
		}

		public new sMenuItem CloneMenu()
		{
			//MenuItem baseItem = (MenuItem)base.MemberwiseClone();
			//sMenuItem menuItem = (sMenuItem)baseItem;

			//sMenuItem menuItem = ;

			//this.SecurityDescriptor.CopyTo( ((ISecureControl)menuItem).SecurityDescriptor, true );
			//((ISecureControl)menuItem).ApplySecurity( AceType.UI );

			return (sMenuItem)base.MemberwiseClone();
		}


		#region Security Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Security management properties and tools." ), Category( "Suplex" )]
		public SecurityAccessor Security
		{
			get { return _sa; }
		}

		public void ApplySecurity()
		{
			if( !this.DesignMode )
			{
				if( !_sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					base.Enabled = false;
				}
				if( !_sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					base.Visible = false;
				}
			}
		}

		public virtual IEnumerable GetChildren()
		{
			return (ICollection)this.MenuItems;
		}

		[DefaultValue( false )]
		new public bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					base.Visible = value;
				}
			}
		}

		[DefaultValue( false )]
		new public bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					base.Enabled = value;
				}
			}
		}
		#endregion


	}//sMenuItem



	public class sMenuItemCollection : Menu.MenuItemCollection
	{
		public sMenuItemCollection( Menu owner ) : base( owner ){}


		new public virtual sMenuItem this[int index]
		{
			get
			{
				return (sMenuItem)base[index];
			}
		}


		public virtual int Add(int index, sMenuItem item)
		{
			return base.Add( index, (MenuItem)item );
		}


		new public virtual sMenuItem Add(string caption)
		{
			//typecast exception: return (sMenuItem)base.Add( caption );

			sMenuItem m = new sMenuItem( caption );
			this.Add( m );
			return m;
		}


		new public virtual sMenuItem Add(string caption, EventHandler onClick)
		{
			//typecast exception: return (sMenuItem)base.Add( caption, onClick );

			sMenuItem m = new sMenuItem( caption, onClick );
			this.Add( m );
			return m;
		}


		public virtual sMenuItem Add(string caption, sMenuItem[] items)
		{
			//typecast exception: return (sMenuItem)base.Add( caption, (MenuItem[])items );

			sMenuItem m = new sMenuItem( caption, items );
			this.Add( m );
			return m;
		}


		public virtual int Add(sMenuItem item)
		{
			return base.Add( (MenuItem)item );
		}


		public virtual void AddRange(sMenuItem[] items)
		{
			base.AddRange( (MenuItem[])items );
		}


		public virtual bool Contains(sMenuItem item)
		{
			return base.Contains( (MenuItem)item );
		}


		public virtual int IndexOf(sMenuItem item)
		{
			return base.IndexOf( (MenuItem)item );
		}


		public virtual void Remove(sMenuItem item)
		{
			base.Remove( (MenuItem)item );
		}


	}//sMenuItemCollection



}//namespace