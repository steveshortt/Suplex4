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
	public delegate void sToolBarButtonClickEventHandler(object sender, sToolBarButtonClickEventArgs e);


	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(ToolBar))]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.CheckStateChanged )]
	public class sToolBar : ToolBar, ISecureContainer
	{

		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;

		private sToolBarButtonCollection	_buttons;





		#region Events

		new public event sToolBarButtonClickEventHandler ButtonClick;
		new public event sToolBarButtonClickEventHandler ButtonDropDown;

		private void onButtonClick(object sender, sToolBarButtonClickEventArgs e)
		{
			if( ButtonClick != null )
			{
				ButtonClick( sender, e );
			}
		}

		private void onButtonDropDown(object sender, sToolBarButtonClickEventArgs e)
		{
			if( ButtonDropDown != null )
			{
				ButtonDropDown( sender, e );
			}
		}
		#endregion


		public sToolBar() : base()
		{
			_buttons = new sToolBarButtonCollection( this );

			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
		}

		protected override void InitLayout()
		{
			_sa.EnsureDefaultState();

			base.InitLayout();
		}

		[ParenthesizePropertyName( true ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public string UniqueName
		{
			get
			{
				return string.IsNullOrEmpty( _uniqueName ) ? base.Name : _uniqueName;
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
			get { return _dal; }
			set { _dal = value; }
		}


		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		new public sToolBarButtonCollection Buttons
		{
			get { return _buttons; }
		}


		protected override void OnButtonClick(ToolBarButtonClickEventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				sToolBarButtonClickEventArgs m = new sToolBarButtonClickEventArgs( (sToolBarButton)e.Button );
				if( m.Button.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed )
				{
					this.onButtonClick( this, m );
				}
			}
		}


		protected override void OnButtonDropDown(ToolBarButtonClickEventArgs e)
		{
			sToolBarButtonClickEventArgs m = new sToolBarButtonClickEventArgs( (sToolBarButton)e.Button );
			bool btn_operate = m.Button.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed;

			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed && btn_operate )
			{
					this.onButtonDropDown( this, m );
			}
			else
			{
				/// this is done to suppress the DropDownMenu from dropping.
				/// suppressing the OnButtonDropDown event doesn't stop the menu.
				if( m.Button.DropDownMenu != null )
				{
					m.Button.DropDownMenu.Dispose();
				}
			}
		}


		protected override void OnEnabledChanged(EventArgs e)
		{
			//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Enabled, _auditEventHandler );

			_sa.AuditAction( AuditType.ControlDetail, null, "EnabledChanged.", false );

			base.OnEnabledChanged( e );
		}


//		protected override void OnVisibleChanged(EventArgs e)
//		{
//			//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Visible, _auditEventHandler );
//			
//			base.OnVisibleChanged( e );
//		}


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
			return (ICollection)_buttons;
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

		/*
		protected override void WndProc(ref Message m)
		{
			if( m.Msg != 529 &&
				m.Msg != 278 &&
				m.Msg != 279 )
			{
				base.WndProc( ref m );
			}
			else
			{
				System.Diagnostics.Debug.WriteLine( m.Msg.ToString() );
				m.Msg = 530;
				base.WndProc( ref m );
			}	
		}
		*/
		#endregion
	}//sToolBar


	public class sToolBarButton : ToolBarButton, ISecureControl
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;


		public sToolBarButton() : base ()
		{
			this.Initialize();
		}


		public sToolBarButton(string text) : base (text)
		{
			this.Initialize();
		}


		public sToolBarButton(string text, string uniqueName) : base (text)
		{
			_uniqueName = uniqueName;
			this.Initialize();
		}


		private void Initialize()
		{
			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
		}

		[ParenthesizePropertyName( true ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public string UniqueName
		{
			get
			{
				return string.IsNullOrEmpty( _uniqueName ) ? base.Name : _uniqueName;
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
			get { return _dal; }
			set { _dal = value; }
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


	}//sToolBarButton


	public class sToolBarButtonCollection : ToolBar.ToolBarButtonCollection
	{
		public sToolBarButtonCollection(ToolBar owner) : base(owner){}

		new public virtual sToolBarButton this[int index]
		{
			get
			{
				return (sToolBarButton)base[index];
			}
			set
			{
				base[index] = value;
			}
		}

		new public virtual sToolBarButton this[string key]
		{
			get
			{
				return (sToolBarButton)base[key];
			}
			//set { base[key] = value; }
		}

		new public virtual sToolBarButton Add(string text)
		{
			sToolBarButton b = new sToolBarButton( text );
			this.Add( b );
			return b;
		}

		public virtual sToolBarButton Add(string text, string uniqueName)
		{
			sToolBarButton b = new sToolBarButton( text, uniqueName );
			this.Add( b );
			return b;
		}

		public virtual int Add(sToolBarButton button)
		{
			return base.Add( (ToolBarButton)button );
		}

		public virtual void AddRange(sToolBarButton[] buttons)
		{
			base.AddRange( (ToolBarButton[])buttons );
		}

		public virtual bool Contains(sToolBarButton button)
		{
			return base.Contains( (ToolBarButton)button );
		}

		public virtual int IndexOf(sToolBarButton button)
		{
			return base.IndexOf( (ToolBarButton)button );
		}

		public virtual void Insert(int index, sToolBarButton button)
		{
			base.Insert( index, (ToolBarButton)button );
		}

		public virtual void Remove(sToolBarButton button)
		{
			base.Remove( (ToolBarButton)button );
		}
	}//sToolBarButtonCollection


	public class sToolBarButtonClickEventArgs : ToolBarButtonClickEventArgs
	{
		public sToolBarButtonClickEventArgs(sToolBarButton button) : base((ToolBarButton)button){}


		new public sToolBarButton Button
		{
			get
			{
				return (sToolBarButton)base.Button;
			}
		}
	}
}