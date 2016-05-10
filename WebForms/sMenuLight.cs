using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;


using Suplex.Data;
using Suplex.Forms;
using Suplex.General;
using Suplex.Security;
using Suplex.Security.Standard;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Suplex.WebForms
{
	/// <summary>
	/// This is dirty.  So, so dirty.
	/// 05/06/2008: When they bury me, make sure this code is with me.
	/// </summary>
	[ToolboxItem(true),
		System.Drawing.ToolboxBitmap(typeof(System.Windows.Forms.MainMenu))]
	[EventBindings( EventBindingsAttribute.BaseEvents.None,
		ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.VisibleChanged | ControlEvents.TextChanged )]
	public class sMenuLight : Suplex.WebForms.sPanel, System.Web.UI.IPostBackEventHandler
	{
		private List<sMenuItemLight> _menuItems = null;
		private bool _enableMenuItemsViewState = false;
		private bool _obfuscatePostBackData = true;
		private bool _renderIfEmpty = false;
		private bool _allowImages = true;
		private List<ISecureControl> _controls = new List<ISecureControl>();
		private Dictionary<string, string> _linkSave = new Dictionary<string, string>();
		private Dictionary<string, string> _linkLoad = new Dictionary<string, string>();
		private Random vsKey = new Random();



		#region Events
		public event CommandEventHandler Command;

		protected void OnCommand(object sender, CommandEventArgs e)
		{
			if( Command != null )
			{
				Command( sender, e );
			}
		}
		#endregion


		#region constructors/overrides
		public sMenuLight() : base()
		{
		}

		protected override void OnInit(EventArgs e)
		{
			if( _autoLoadMenuItems )
			{
				this.LoadMenuItems( false, false );
			}

			base.OnInit( e );
		}

		protected override object SaveViewState()
		{
			object baseState = base.SaveViewState();

			object[] s = new object[3];

			s[0] = baseState;
			s[1] = _linkSave;
			s[2] = _enableMenuItemsViewState ? _menuItems : null;

			return s;
		}

		/// <summary>
		/// Load State from the array of objects that was saved at SavedViewState.
		/// </summary>
		/// <param name="savedState">State information to load.</param>
		protected override void LoadViewState(object savedState)
		{
			object[] s = (object[])savedState;

			if( s[0] != null )
				base.LoadViewState( s[0] );

			if( s[1] != null )
				_linkLoad = (Dictionary<string, string>)s[1];

			if( s[2] != null && _enableMenuItemsViewState )	// && ( _menuItems == null || _menuItems.Count == 0 ) )
			{
				_menuItems = (List<sMenuItemLight>)s[2];
				this.EnsureMenuItems();
			}
		}
		#endregion


		#region properties/implementation
		public List<sMenuItemLight> MenuLightItems
		{
			get { return _menuItems; }
		}

		public sMenuItemLight GetMenuLightItem(string uniqueName)
		{
			sMenuItemLight r = null;
			foreach( sMenuItemLight m in _menuItems )
			{
				if( m.UniqueName == uniqueName )
				{
					r = m;
					break;
				}
			}
			return r;
		}

		public bool AllowImages
		{
			get { return _allowImages; }
			set { _allowImages = value; }
		}

		public bool EnableMenuItemsViewState
		{
			get { return _enableMenuItemsViewState; }
			set { _enableMenuItemsViewState = value; }
		}
		public bool ObfuscatePostBackData
		{
			get { return _obfuscatePostBackData; }
			set { _obfuscatePostBackData = value; }
		}
		public bool RenderIfEmpty
		{
			get { return _renderIfEmpty; }
			set { _renderIfEmpty = value; }
		}
		public void ClearItems()
		{
			_menuItems.Clear();
			_controls.Clear();
		}


		public WebControl this[string uniqueName]
		{
			get
			{
				WebControl c = null;
				foreach( ISecureControl s in _controls )
				{
					if( s.UniqueName == uniqueName )
					{
						c = (WebControl)s;
						break;
					}
				}
				return c;
			}
		}

		public List<ISecureControl> MenuControlItems
		{
			get { return _controls; }
		}

		public void LoadMenuItems()
		{
			this.LoadMenuItems( true, false );
		}

		public void LoadMenuItems(bool appendItems)
		{
			this.LoadMenuItems( true, appendItems );
		}

		public void EnsureMenuItems()
		{
			this.LoadMenuItemsIntoControls();
		}

		private void LoadMenuItems(bool throwErr, bool appendItems)
		{
			if( _menuItemsFilePath != null && _menuItemsFilePath != string.Empty )
			{
				XmlSerializer formatter = new XmlSerializer( typeof( List<sMenuItemLight> ) );
				StreamReader reader = new StreamReader( this.Page.Server.MapPath( _menuItemsFilePath ) );
				if( appendItems )
				{
					_menuItems.AddRange( (List<sMenuItemLight>)formatter.Deserialize( reader ) );
				}
				else
				{
					_menuItems = (List<sMenuItemLight>)formatter.Deserialize( reader );
				}
				reader.Close();

				LoadMenuItemsIntoControls();
			}
			else
			{
				if( throwErr )
					throw new ArgumentNullException( "MenuItemsPath" );
			}
		}

		private void LoadMenuItemsIntoControls()
		{
			foreach( sMenuItemLight item in _menuItems )
			{
				if( item.Enabled )
				{
					WebControl menuLink = null;

					if( !item.PostBack )
					{
						menuLink = new sHyperLink();
						( (sHyperLink)menuLink ).Text = RenderUtil.RenderAccessKey( item.Text, menuLink );
						if( _allowImages && item.HasImageUrl )
						{
							( (sHyperLink)menuLink ).Text = item.Text.Replace( "&", string.Empty );
							( (sHyperLink)menuLink ).ImageUrl = item.ImageUrl;
						}
						( (sHyperLink)menuLink ).NavigateUrl = item.Parameter;
						( (sHyperLink)menuLink ).UniqueName = item.UniqueName;
					}
					else
					{
						string key = vsKey.Next( 100, 10000 ).ToString();
						while( _linkSave.ContainsKey( key ) )
						{
							key = vsKey.Next( 100, 10000 ).ToString();
						}
						menuLink = new sLinkButton();
						( (IButtonControl)menuLink ).Text = RenderUtil.RenderAccessKey( item.Text, menuLink );
						if( _allowImages && item.HasImageUrl )
						{
							menuLink = new sImageButton();
							( (IButtonControl)menuLink ).Text = item.Text.Replace( "&", string.Empty );
							( (sImageButton)menuLink ).ImageUrl = item.ImageUrl;
						}
						( (IButtonControl)menuLink ).CommandName = key;
						( (IButtonControl)menuLink ).CommandArgument = item.Parameter;
						( (ISecureControl)menuLink ).UniqueName = item.UniqueName;
						//( (sLinkButton)menuLink ).Command += new CommandEventHandler( mMenuLight_Command );
						//( (sLinkButton)menuLink ).Command += new CommandEventHandler( this.menuLink_Command );
						//_linkInfo = string.Format( "{0},{1}|{2}", _linkInfo, menuLink.ClientID, item.Parameter );
						_linkSave.Add( key, item.Parameter );
					}

					this.Controls.Add( menuLink );
					//this.SecureControls.Add( (ISecureControl)menuLink );
					_controls.Add( (ISecureControl)menuLink );
				}
			}
		}

		//void mMenuLight_Command(object sender, CommandEventArgs e)
		//{
		//    throw new Exception( "The method or operation is not implemented." );
		//}

		//LinkButton b = null;
		//protected override void CreateChildControls()
		//{
		//    foreach( mMenuItemLight item in _menuItems )
		//    {
		//        if( item.PostBack )
		//        {
		//            ( (sLinkButton)this.SecureControls[item.UniqueName] ).CommandArgument = item.Parameter;
		//        }
		//    }

		//    b = new LinkButton();
		//    b.Text = "linkbutton";
		//    b.CommandName = "xxx";
		//    b.CommandArgument = "yyy";
		//    this.Controls.Add( b );

		//    base.CreateChildControls();
		//}

		protected override void Render(HtmlTextWriter writer)
		{
			Table outer = new Table();
			outer.CssClass = _menuTableCssClass;
			bool haveContent = false;
			bool selected = false;
			string rowBreak = string.Empty;

			Dictionary<TableCell, sMenuItemLight> c2m = new Dictionary<TableCell, sMenuItemLight>();

			if( _orientation == Orientation.Horizontal )
			{
				rowBreak = "</tr><tr>";
				outer.Rows.Add( new TableRow() );
			}

			int r = 0;
			foreach( ISecureControl item in _controls )
			{
				if( item.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					haveContent = true;
					TableCell cell = new TableCell();
					if( _renderStyle == MenuRenderStyle.UnorderedList || _renderStyle == MenuRenderStyle.Div )
					{
						outer.Rows.Add( new TableRow() );
					}
					else if( _orientation == Orientation.Vertical )
					{
						r = outer.Rows.Add( new TableRow() );
					}
					this.Controls.Remove( (WebControl)item );
					outer.Rows[r].Cells.Add( cell );

					sMenuItemLight m = this.GetMenuLightItem( item.UniqueName );

					c2m.Add( cell, m );

					if( item is sHyperLink )
					{
						sHyperLink h = (sHyperLink)item;

						if( _selectedItemUniqueName != string.Empty )
						{
							//cell.CssClass = _selectedItemUniqueName == item.UniqueName ? _menuItemSelectedCellCssClass : _menuItemCellCssClass;
							cell.CssClass = this.GetCssClass( m, _selectedItemUniqueName == item.UniqueName, h );

							_findSelectedByUrl = false;
						}

						if( _findSelectedByUrl )
						{
							selected =
								this.Page.ResolveUrl( h.NavigateUrl ) == this.Page.ResolveUrl( this.Page.Request.FilePath );
							//cell.CssClass = selected ? _menuItemSelectedCellCssClass : _menuItemCellCssClass;
							cell.CssClass = this.GetCssClass( m, selected, h );
						}

						if( !h.Enabled && m.HasDisabledImageUrl )
						{
							h.ImageUrl = m.DisabledImageUrl;
						}

						cell.Controls.Add( (WebControl)item );
					}
					else
					{
						WebControl c = null;
						if( item is sImageButton )
						{
							c = this.TransformImageButtonToHyperLink( (sImageButton)item );
						}
						else
						{
							c = this.FixLinkButton( ( (IButtonControl)item ) );
						}

						//cell.CssClass = _selectedItemUniqueName == item.UniqueName ? _menuItemSelectedCellCssClass : _menuItemCellCssClass;
						cell.CssClass = this.GetCssClass( m, _selectedItemUniqueName == item.UniqueName, c );
						cell.Controls.Add( c );
					}

					if( m.HasItemId ) { ( (WebControl)item ).ID = m.ItemId; }
					if( m.HasContainerId ) { cell.ID = m.ContainerId; }
				}
			}

			#region render
			if( haveContent || _renderIfEmpty )
			{
				switch( _renderStyle )
				{
					case MenuRenderStyle.TabContainer:
					{
						string outerTableCssClass = _outerTableCssClass;
						string outerTableHeaderCellCssClass = _outerTableHeaderCellCssClass;
						string outerTableContentCellCssClass = _outerTableContentCellCssClass;
						if( !haveContent && _renderIfEmpty )
						{
							outerTableCssClass =
								string.IsNullOrEmpty( _emptyTableCssClass ) ? outerTableCssClass : _emptyTableCssClass;
							outerTableHeaderCellCssClass =
								string.IsNullOrEmpty( _emptyTableHeaderCellCssClass ) ? outerTableHeaderCellCssClass : _emptyTableHeaderCellCssClass;
							outerTableContentCellCssClass =
								string.IsNullOrEmpty( _emptyTableContentCellCssClass ) ? outerTableContentCellCssClass : _emptyTableContentCellCssClass;
						}

						writer.Write( "<table class=\"{0}\"><tr><td class=\"{1}\">\r\n", outerTableCssClass, outerTableHeaderCellCssClass );
						outer.RenderControl( writer );
						writer.Write( "\r\n</td>{1}<td class=\"{0}\">\r\n", outerTableContentCellCssClass, rowBreak );
						base.Render( writer );
						writer.Write( "\r\n</td></tr></table>\r\n" );

						break;
					}
					case MenuRenderStyle.CellsOnly:
					{
						if( _orientation == Orientation.Horizontal )
						{
							foreach( TableCell td in outer.Rows[0].Cells )
							{
								td.RenderControl( writer );
							}
						}
						else
						{
							foreach( TableRow tr in outer.Rows )
							{
								tr.RenderControl( writer );
							}
						}

						base.Render( writer );

						break;
					}
					case MenuRenderStyle.Normal:
					{
						outer.RenderControl( writer );
						base.Render( writer );

						break;
					}
					case MenuRenderStyle.UnorderedList:
					{
						string containerId = string.Empty;
						writer.Write( "\r\n<ul>\r\n" );
						foreach( TableCell td in outer.Rows[0].Cells )
						{
							containerId = c2m[td].HasContainerId ? string.Format( " id=\"{0}\"", c2m[td].ContainerId ) : string.Empty;
							if( c2m[td].HasPreRenderContent ) { writer.WriteLine( c2m[td].PreRenderContent ); }
							writer.Write( string.Format( "\t<li{0}{1}{2}>", containerId,
								string.IsNullOrEmpty( td.CssClass ) ? string.Empty : string.Format( " class=\"{0}\"", td.CssClass ),
								string.IsNullOrEmpty( c2m[td].Text ) ? string.Empty : string.Format( " title=\"{0}\"", c2m[td].Text.Replace( "&", string.Empty ) ) ) );
							td.Controls[0].RenderControl( writer );
							writer.Write( "</li>\r\n" );
							if( c2m[td].HasPostRenderContent ) { writer.WriteLine( c2m[td].PostRenderContent ); }
						}
						writer.Write( "</ul>\r\n" );
						break;
					}
					case MenuRenderStyle.Div:
					{
						string containerId = string.Empty;
						foreach( TableCell td in outer.Rows[0].Cells )
						{
							containerId = c2m[td].HasContainerId ? string.Format( " id=\"{0}\"", c2m[td].ContainerId ) : string.Empty;
							if( c2m[td].HasPreRenderContent ) { writer.WriteLine( c2m[td].PreRenderContent ); }
							writer.Write(
								string.Format( "\t<div{0}{1}{2}>", containerId,
								string.IsNullOrEmpty( td.CssClass ) ? string.Empty : string.Format( " class=\"{0}\"", td.CssClass ),
								string.IsNullOrEmpty( c2m[td].Text ) ? string.Empty : string.Format( " title=\"{0}\"", c2m[td].Text.Replace( "&", string.Empty ) ) ) );
							//writer.Write( string.Format( "\t<div{0}>", containerId ) );
							//( (WebControl)td.Controls[0] ).CssClass = td.CssClass;
							td.Controls[0].RenderControl( writer );
							writer.Write( "</div>\r\n" );
							if( c2m[td].HasPostRenderContent ) { writer.WriteLine( c2m[td].PostRenderContent ); }
						}
						break;
					}
				}
			}
			#endregion
		}

		private string GetCssClass(sMenuItemLight m, bool selected, WebControl w)
		{
			string css = m.HasCssClass ? m.CssClass : _menuItemCellCssClass;
			if( selected )
			{
				css = m.HasSelectedCssClass ? m.SelectedCssClass : _menuItemSelectedCellCssClass;
			}
			if( !w.Enabled )
			{
				css = m.HasDisabledCssClass ? m.DisabledCssClass : _menuItemDisabledCellCssClass;
			}

			return css;
		}

		private WebControl TransformImageButtonToHyperLink(sImageButton b)
		{
			sHyperLink h = new sHyperLink();

			string arg = b.CommandArgument;
			if( _obfuscatePostBackData )
			{
				arg = b.CommandName;
				if( !this.EnableViewState ) arg = GetEncoded( b.CommandArgument );
			}

			h.UniqueName = b.UniqueName;
			h.Text = ( (IButtonControl)b ).Text;
			h.NavigateUrl =
				this.Page.ClientScript.GetPostBackClientHyperlink( this, arg, true );

			if( _allowImages && !string.IsNullOrEmpty( b.ImageUrl ) )
			{
				h.ImageUrl = b.ImageUrl;
				sMenuItemLight m = this.GetMenuLightItem( b.UniqueName );
				if( !b.Enabled && m.HasDisabledImageUrl )
				{
					h.ImageUrl = m.DisabledImageUrl;
				}
			}

			h.Security.DefaultState = DefaultSecurityState.Unlocked;
			h.Security.EnsureDefaultState();
			h.Enabled = b.Enabled;
			h.Visible = b.Visible;

			b.Security.Descriptor.CopyTo( h.Security.Descriptor, true );
			b.Security.Apply( AceType.Native );

			return h;
		}

		private WebControl FixLinkButton(IButtonControl b)
		{
			string arg = b.CommandArgument;
			if( _obfuscatePostBackData )
			{
				arg = b.CommandName;
				if( !this.EnableViewState ) arg = GetEncoded( b.CommandArgument );
			}

			if( ( (WebControl)b ).Enabled )
			{
				( (WebControl)b ).Attributes.Add( "href",
					this.Page.ClientScript.GetPostBackClientHyperlink( this, arg, true ) );
			}
			else
			{
				( (WebControl)b ).Attributes.Add( "href", "#" );
			}
			return (WebControl)b;
		}

		//TODO: fix this foe enabled prop, return disabled label maybe
		private HtmlAnchor MakeAnchor(sLinkButton b)
		{
			string arg = b.CommandName;
			if( !this.EnableViewState ) arg = GetEncoded( b.CommandArgument );
			HtmlAnchor a = new HtmlAnchor();
			a.InnerText = b.Text;
			a.HRef = this.Page.ClientScript.GetPostBackClientHyperlink( this, arg, true );
			a.Visible = b.Visible;
			return a;
		}

		private string GetEncoded(string value)
		{
			int i = 0;
			Random r = new Random();
			int offset = r.Next( 0x10 );
			StringBuilder s = new StringBuilder( value.Length * 2 );

			byte[] b = Encoding.ASCII.GetBytes( value );
			for( int n = 0; n < b.Length; n++ )
			{
				i = (int)b[n] - offset;
				s.Append( i < 16 ? "0" + i.ToString( "X" ) : i.ToString( "X" ) );
			}
			s.Insert( 0, offset < 16 ? "0" + offset.ToString( "X" ) : offset.ToString( "X" ) );

			return s.ToString();
		}
		private string GetDecoded(string value)
		{
			int i = 0;
			int offset = (int)Convert.ToByte( value.Substring( 0, 2 ), 16 );
			value = value.Substring( 2, value.Length - 2 );

			int c = value.Length / 2;
			byte[] b = new byte[c];
			for( int n = 0; n < c; n++ )
			{
				i = (int)Convert.ToByte( value.Substring( n * 2, 2 ), 16 ) + offset;
				b[n] = Convert.ToByte( i );
			}
			return Encoding.ASCII.GetString( b );
		}
		#endregion


		#region IPostBackEventHandler Members
		public void RaisePostBackEvent(string eventArgument)
		{
			string arg = eventArgument;

			if( _obfuscatePostBackData )
			{
				if( this.EnableViewState )
				{
					arg = _linkLoad[eventArgument];
				}
				else
				{
					arg = GetDecoded( eventArgument );
				}
			}

			this.OnCommand( this, new CommandEventArgs( this.ID, arg ) );
		}
		#endregion

		//06232006, don't need this
		//protected override bool OnBubbleEvent(object source, EventArgs args)
		//{
		//    if( args is CommandEventArgs )
		//    {
		//        OnCommand( this, (CommandEventArgs)args );
		//        return true;
		//    }
		//    else
		//    {
		//        return base.OnBubbleEvent( source, args );
		//    }
		//}


		#region IMenu properties
		private bool _autoLoadMenuItems = true;
		private string _menuItemsFilePath = string.Empty;
		private bool _findSelectedByUrl = true;
		private string _selectedItemUniqueName = string.Empty;
		private Orientation _orientation = Orientation.Vertical;
		private ITemplate _content = null;
		private MenuRenderStyle _renderStyle = MenuRenderStyle.Normal;
		private string _outerTableCssClass = string.Empty;
		private string _outerTableHeaderCellCssClass = string.Empty;
		private string _outerTableContentCellCssClass = string.Empty;
		private string _menuTableCssClass = string.Empty;
		private string _menuItemCellCssClass = string.Empty;
		private string _menuItemSelectedCellCssClass = string.Empty;
		private string _menuItemDisabledCellCssClass = string.Empty;
		private string _emptyTableCssClass = string.Empty;
		private string _emptyTableHeaderCellCssClass = string.Empty;
		private string _emptyTableContentCellCssClass = string.Empty;

		public virtual bool AutoLoadMenuItems
		{
			get { return _autoLoadMenuItems; }
			set { _autoLoadMenuItems = value; }
		}
		public virtual string MenuItemsFilePath
		{
			get { return _menuItemsFilePath; }
			set { _menuItemsFilePath = value; }
		}
		public virtual bool FindSelectedByUrl
		{
			get { return _findSelectedByUrl; }
			set { _findSelectedByUrl = value; }
		}
		public virtual string SelectedItemUniqueName
		{
			get { return _selectedItemUniqueName; }
			set { _selectedItemUniqueName = value; }
		}
		public void SetSelected(string selectedItemUniqueName)
		{
			_selectedItemUniqueName = selectedItemUniqueName;
		}
		public virtual Orientation Orientation
		{
			get { return _orientation; }
			set { _orientation = value; }
		}
		protected virtual ITemplate Content		//switched from public on 06262006, not using this prop
		{
			get { return _content; }
			set { _content = value; }
		}
		public virtual MenuRenderStyle RenderStyle
		{
			get { return _renderStyle; }
			set { _renderStyle = value; }
		}
		public virtual string OuterTableCssClass
		{
			get { return _outerTableCssClass; }
			set { _outerTableCssClass = value; }
		}
		public virtual string OuterTableHeaderCellCssClass
		{
			get { return _outerTableHeaderCellCssClass; }
			set { _outerTableHeaderCellCssClass = value; }
		}
		public virtual string OuterTableContentCellCssClass
		{
			get { return _outerTableContentCellCssClass; }
			set { _outerTableContentCellCssClass = value; }
		}
		public virtual string MenuTableCssClass
		{
			get { return _menuTableCssClass; }
			set { _menuTableCssClass = value; }
		}
		public virtual string MenuItemCellCssClass
		{
			get { return _menuItemCellCssClass; }
			set { _menuItemCellCssClass = value; }
		}
		public virtual string MenuItemSelectedCellCssClass
		{
			get { return _menuItemSelectedCellCssClass; }
			set { _menuItemSelectedCellCssClass = value; }
		}
		public virtual string MenuItemDisabledCellCssClass
		{
			get { return _menuItemDisabledCellCssClass; }
			set { _menuItemDisabledCellCssClass = value; }
		}
		public virtual string EmptyTableCssClass
		{
			get { return _emptyTableCssClass; }
			set { _emptyTableCssClass = value; }
		}
		public virtual string EmptyTableHeaderCellCssClass
		{
			get { return _emptyTableHeaderCellCssClass; }
			set { _emptyTableHeaderCellCssClass = value; }
		}
		public virtual string EmptyTableContentCellCssClass
		{
			get { return _emptyTableContentCellCssClass; }
			set { _emptyTableContentCellCssClass = value; }
		}
		#endregion
	}

	public enum MenuRenderStyle
	{
		Normal,
		TabContainer,
		CellsOnly,
		UnorderedList,
		Div
	}


	#region sMenuItemLight
	public class sMenuItemLight
	{
		private string _text = string.Empty;
		private string _imageUrl = string.Empty;
		private string _disabledImageUrl = string.Empty;
		private string _itemId = string.Empty;
		private string _containerId = string.Empty;
		private string _cssClass = string.Empty;
		private string _selectedCssClass = string.Empty;
		private string _disabledCssClass = string.Empty;
		private string _parameter = string.Empty;
		private string _uniqueName = string.Empty;
		private bool _selected = false;
		private bool _enabled = true;
		private bool _postback = false;
		private string _preRenderContent = string.Empty;
		private string _postRenderContent = string.Empty;

		public sMenuItemLight() { }
		public sMenuItemLight(string text, string parameter)
		{
			_text = text;
			_parameter = parameter;
		}
		public sMenuItemLight(string text, string parameter, bool enabled)
		{
			_text = text;
			_parameter = parameter;
			_enabled = enabled;
		}

		public virtual string Text
		{
			get { return _text; }
			set { _text = value; }
		}
		public virtual string ImageUrl
		{
			get { return _imageUrl; }
			set { _imageUrl = value; }
		}
		public virtual string DisabledImageUrl
		{
			get { return _disabledImageUrl; }
			set { _disabledImageUrl = value; }
		}
		public virtual string ItemId
		{
			get { return _itemId; }
			set { _itemId = value; }
		}
		public virtual string ContainerId
		{
			get { return _containerId; }
			set { _containerId = value; }
		}
		public bool HasItemId { get { return !string.IsNullOrEmpty( _itemId ); } }
		public bool HasContainerId { get { return !string.IsNullOrEmpty( _containerId ); } }
		public virtual string CssClass
		{
			get { return _cssClass; }
			set { _cssClass = value; }
		}
		public virtual string SelectedCssClass
		{
			get { return _selectedCssClass; }
			set { _selectedCssClass = value; }
		}
		public virtual string DisabledCssClass
		{
			get { return _disabledCssClass; }
			set { _disabledCssClass = value; }
		}
		public bool HasImageUrl { get { return !string.IsNullOrEmpty( _imageUrl ); } }
		public bool HasDisabledImageUrl { get { return !string.IsNullOrEmpty( _disabledImageUrl ); } }
		public bool HasCssClass { get { return !string.IsNullOrEmpty( _cssClass ); } }
		public bool HasSelectedCssClass { get { return !string.IsNullOrEmpty( _selectedCssClass ); } }
		public bool HasDisabledCssClass { get { return !string.IsNullOrEmpty( _disabledCssClass ); } }
		public virtual string Parameter
		{
			get { return _parameter; }
			set { _parameter = value; }
		}
		public virtual string UniqueName
		{
			get { return _uniqueName; }
			set { _uniqueName = value; }
		}
		public virtual bool Selected
		{
			get { return _selected; }
			set { _selected = value; }
		}
		public virtual bool Enabled
		{
			get { return _enabled; }
			set { _enabled = value; }
		}
		public virtual bool PostBack
		{
			get { return _postback; }
			set { _postback = value; }
		}
		public virtual string PreRenderContent
		{
			get { return _preRenderContent; }
			set { _preRenderContent = value; }
		}
		public virtual string PostRenderContent
		{
			get { return _postRenderContent; }
			set { _postRenderContent = value; }
		}
		public bool HasPreRenderContent { get { return !string.IsNullOrEmpty( _preRenderContent ); } }
		public bool HasPostRenderContent { get { return !string.IsNullOrEmpty( _postRenderContent ); } }
	}
	#endregion
}