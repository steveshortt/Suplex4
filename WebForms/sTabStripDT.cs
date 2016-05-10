using System;
using System.Reflection;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Collections.Generic;

using Suplex.Data;
using Suplex.Forms;
using Suplex.General;
using Suplex.Security;
using Suplex.Security.Standard;

namespace Suplex.WebForms
{
	/// <summary>
	/// Provides secuirty, extended validation properties, and auto-validation.
	/// </summary>
	[ToolboxItem( true ),
		System.Drawing.ToolboxBitmap( typeof( System.Web.UI.WebControls.Panel ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.None,
		ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.VisibleChanged | ControlEvents.TextChanged )]
	internal class sTabStripDT : sPanel, INamingContainer
	{
		private const string _divEndTag = "</div>";

		private List<sTabDT> _tabs;
		private string _rowCssClass = "tab-row";
		private string _tabPaneCssClass = string.Empty;
		private string _tabPaneFooterCssClass = string.Empty;
		private int _selectedIndex = -1;

		private TabStripRenderStyleDT _renderStyle = TabStripRenderStyleDT.Normal;
		//private ITemplate _headerTemplate = null;
		//private ContentPlaceHolder _headerContent = null;
		private sTabStripDialogHeaderDT _header = null;
		private string _dialogWrapperId0 = string.Empty;
		private string _dialogWrapperCssClass0 = string.Empty;
		private string _dialogWrapperId1 = string.Empty;
		private string _dialogWrapperCssClass1 = string.Empty;
		private string _dialogWrapperId2 = string.Empty;
		private string _dialogWrapperCssClass2 = string.Empty;
		private string _dialogHeaderWrapperId = string.Empty;
		private string _dialogHeaderWrapperCssClass = string.Empty;
		private string _dialogTabsWrapperId = string.Empty;
		private string _dialogTabsWrapperCssClass = string.Empty;

		private string _tableTabsOuterTableCssClass = string.Empty;
		private string _tableTabsButtonTableCssClass = string.Empty;
		private string _tableTabsContentTableCssClass = string.Empty;
		private string _tableTabsHoverTabCssClass = string.Empty;
		private string _tableTabsSelectedTabCssClass = string.Empty;


		public sTabStripDT()
			: base()
		{
			_tabs = new List<sTabDT>( 5 );
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit( e );

			foreach( Control c in this.Controls )
			{
				if( c is sTabDT )
				{
					_tabs.Add( (sTabDT)c );
				}

				if( c is sTabStripDialogHeaderDT )
				{
					_header = (sTabStripDialogHeaderDT)c;
				}
			}
		}


		protected override void CreateChildControls()
		{
			//if( _headerTemplate != null )
			//{
			//    _headerContent = new ContentPlaceHolder();
			//    this.Controls.Add( _headerContent );
			//    _headerTemplate.InstantiateIn( _headerContent );
			//}

			base.CreateChildControls();

		}


		public List<sTabDT> Tabs
		{
			get { return _tabs; }
			set { _tabs = value; }
		}

		public sTabStripDialogHeaderDT DialogHeader
		{
			get { return _header; }
			set { _header = value; }
		}

		public string TabRowCssClass
		{
			get { return _rowCssClass; }
			set { _rowCssClass = value; }
		}

		public string TabPaneCssClassSuffix
		{
			get { return _tabPaneCssClass; }
			set { _tabPaneCssClass = value; }
		}

		public string TabPaneFooterCssClass
		{
			get { return _tabPaneFooterCssClass; }
			set { _tabPaneFooterCssClass = value; }
		}

		public void SetSelectedTabIndex(int index)
		{
			_selectedIndex = index;
		}


		public TabStripRenderStyleDT RenderStyle
		{
			get { return _renderStyle; }
			set { _renderStyle = value; }
		}

		//[PersistenceMode( PersistenceMode.InnerProperty )]
		//public ITemplate DialogHeaderTemplate
		//{
		//    get { return _headerTemplate; }
		//    set { _headerTemplate = value; }
		//}

		public string DialogWrapperId0
		{
			get { return _dialogWrapperId0; }
			set { _dialogWrapperId0 = value; }
		}
		public string DialogWrapperCssClass0
		{
			get { return _dialogWrapperCssClass0; }
			set { _dialogWrapperCssClass0 = value; }
		}
		public string DialogWrapperId1
		{
			get { return _dialogWrapperId1; }
			set { _dialogWrapperId1 = value; }
		}
		public string DialogWrapperCssClass1
		{
			get { return _dialogWrapperCssClass1; }
			set { _dialogWrapperCssClass1 = value; }
		}
		public string DialogWrapperId2
		{
			get { return _dialogWrapperId2; }
			set { _dialogWrapperId2 = value; }
		}
		public string DialogWrapperCssClass2
		{
			get { return _dialogWrapperCssClass2; }
			set { _dialogWrapperCssClass2 = value; }
		}
		public string DialogHeaderWrapperId
		{
			get { return _dialogHeaderWrapperId; }
			set { _dialogHeaderWrapperId = value; }
		}
		public string DialogHeaderWrapperCssClass
		{
			get { return _dialogHeaderWrapperCssClass; }
			set { _dialogHeaderWrapperCssClass = value; }
		}
		public string DialogTabsWrapperId
		{
			get { return _dialogTabsWrapperId; }
			set { _dialogTabsWrapperId = value; }
		}
		public string DialogTabsWrapperCssClass
		{
			get { return _dialogTabsWrapperCssClass; }
			set { _dialogTabsWrapperCssClass = value; }
		}

		public string TableTabsOuterTableCssClass
		{
			get { return _tableTabsOuterTableCssClass; }
			set { _tableTabsOuterTableCssClass = value; }
		}
		public string TableTabsButtonTableCssClass
		{
			get { return _tableTabsButtonTableCssClass; }
			set { _tableTabsButtonTableCssClass = value; }
		}
		public string TableTabsContentTableCssClass
		{
			get { return _tableTabsContentTableCssClass; }
			set { _tableTabsContentTableCssClass = value; }
		}
		public string TableTabsHoverTabCssClass
		{
			get { return _tableTabsHoverTabCssClass; }
			set { _tableTabsHoverTabCssClass = value; }
		}
		public string TableTabsSelectedTabCssClass
		{
			get { return _tableTabsSelectedTabCssClass; }
			set { _tableTabsSelectedTabCssClass = value; }
		}


		public string TabsClientScript { get { return Properties.Resources.tabstrip_js; } }
		public string TabsCssBlue { get { return Properties.Resources.tabstrip_css_blue; } }
		public string TabsCssGray { get { return Properties.Resources.tabstrip_css_gray; } }

		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			switch( _renderStyle )
			{
				case TabStripRenderStyleDT.Normal:
				{
					this.RenderBeginTagNormal( writer );
					break;
				}
				case TabStripRenderStyleDT.Dialog:
				{
					this.RenderBeginTagDialog( writer );
					this.RenderBeginTagNormal( writer );
					break;
				}
				case TabStripRenderStyleDT.Table:
				{
					this.RenderBeginTagTable( writer );
					break;
				}
			}

			//writer.Write( string.Format( "<div class=\"{0}\">", _rowCssClass ) );
		}

		private void RenderBeginTagNormal(HtmlTextWriter writer)
		{
			this.Attributes.Add( "class", string.Format( "tab-pane{0}", _tabPaneCssClass ) );
			this.Attributes.Add( "title", string.Format( "{0}!{1}", _rowCssClass, _selectedIndex ) );
			base.RenderBeginTag( writer );
		}

		private void RenderBeginTagDialog(HtmlTextWriter writer)
		{
			writer.WriteLine( string.Format( "\t{0}", this.GetBeginDivTag( _dialogWrapperId0, _dialogWrapperCssClass0 ) ) );
			writer.WriteLine( string.Format( "\t\t{0}", this.GetBeginDivTag( _dialogWrapperId1, _dialogWrapperCssClass1 ) ) );
			writer.WriteLine( string.Format( "\t\t\t{0}", this.GetBeginDivTag( _dialogWrapperId2, _dialogWrapperCssClass2 ) ) );
			writer.WriteLine( string.Format( "\t\t\t\t{0}", this.GetBeginDivTag( _dialogHeaderWrapperId, _dialogHeaderWrapperCssClass ) ) );

			if( _header != null )
			{
				_header.RenderControl( writer );
			}

			writer.WriteLine( string.Format( "\t\t\t\t{0}", _divEndTag ) );
			writer.WriteLine( string.Format( "\t\t\t\t{0}", this.GetBeginDivTag( _dialogTabsWrapperId, _dialogTabsWrapperCssClass ) ) );
		}

		private void RenderBeginTagTable(HtmlTextWriter writer)
		{
			writer.WriteLine( "<table id=\"{0}\" border=0 cellpadding=0 cellspacing=0{1}>", this.ClientID,
				!string.IsNullOrEmpty( _tableTabsOuterTableCssClass ) ? string.Format( " class=\"{0}\"", _tableTabsOuterTableCssClass ) : string.Empty );
			writer.WriteLine( "\t<tr>" );
			writer.WriteLine( "\t\t<td height=\"1%\">" );
			writer.WriteLine( "\t\t\t<table{0}>",
				!string.IsNullOrEmpty( _tableTabsButtonTableCssClass ) ? string.Format( " class=\"{0}\"", _tableTabsButtonTableCssClass ) : string.Empty );
			writer.WriteLine( "\t\t\t\t<tr>" );

			int c = 0;
			foreach( sTabDT t in _tabs )
			{
				t.DisplayTab = 0 == c++;
				writer.WriteLine( "\t\t\t\t\t<td id=\"{0}\" class=\"{1}\"><div onmouseover=\"doHover({0},'{4}','{5}')\" onmouseout=\"doHoverOut({0},'{4}','{1}')\"><a href=\"#\" onclick=\"selectTab({6},{0},{2},'{1}','{4}')\">{3}</a></div></td>",
					t.ClientButtonID, this.CssClass, t.ClientID, t.TabTitle, _tableTabsSelectedTabCssClass, _tableTabsHoverTabCssClass, this.ClientID );
			}

			writer.WriteLine( "\t\t\t\t</tr>" );
			writer.WriteLine( "\t\t\t</table>" );
			writer.WriteLine( "\t\t</td>" );
			writer.WriteLine( "\t</tr>" );
			writer.WriteLine( "\t<tr>" );
			writer.WriteLine( "\t\t<td valign=\"top\">" );
			writer.WriteLine( "\t\t\t<table{0}>",
				!string.IsNullOrEmpty( _tableTabsContentTableCssClass ) ? string.Format( " class=\"{0}\"", _tableTabsContentTableCssClass ) : string.Empty );
			writer.WriteLine( "\t\t\t\t<tr>" );

		}

		private string GetBeginDivTag(string id, string cssClass)
		{
			return string.Format( "<div{0}{1}>",
				!string.IsNullOrEmpty( id ) ? string.Format( " id=\"{0}\"", id ) : string.Empty,
				!string.IsNullOrEmpty( cssClass ) ? string.Format( " class=\"{0}\"", cssClass ) : string.Empty );
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			foreach( sTabDT t in _tabs )
			{
				t.RenderControl( writer );
			}

			//base.RenderContents( writer );
		}

		public override void RenderEndTag(HtmlTextWriter writer)
		{
			if( _renderStyle != TabStripRenderStyleDT.Table )
			{
				writer.WriteLine( string.Format( "<div class=\"{0}\">{1}", _tabPaneFooterCssClass, _divEndTag ) );

				base.RenderEndTag( writer );

				if( _renderStyle == TabStripRenderStyleDT.Dialog )
				{
					writer.WriteLine( string.Format( "\t\t\t\t{0}", _divEndTag ) );
					writer.WriteLine( string.Format( "\t\t\t{0}", _divEndTag ) );
					writer.WriteLine( string.Format( "\t\t{0}", _divEndTag ) );
					writer.WriteLine( string.Format( "\t{0}", _divEndTag ) );
				}
			}
			else
			{
				writer.WriteLine( "\t\t\t\t</tr>" );
				writer.WriteLine( "\t\t\t</table>" );
				writer.WriteLine( "\t\t</td>" );
				writer.WriteLine( "\t</tr>" );
				writer.WriteLine( "</table>" );
			}
		}

		//public override void RenderEndTag(HtmlTextWriter writer)
		//{
		// writer.Write( "</div>" );
		// base.RenderEndTag( writer );
		//}
	}


	internal enum TabStripRenderStyleDT
	{
		Normal,
		Dialog,
		Table
	}


	/// <summary>
	/// Provides secuirty, extended validation properties, and auto-validation.
	/// </summary>
	[ToolboxItem( true ),
		System.Drawing.ToolboxBitmap( typeof( System.Web.UI.WebControls.Panel ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.None,
		ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.VisibleChanged | ControlEvents.TextChanged )]
	internal class sTabDT : sPanel
	{
		//private string _tabTitle = "Tab";
		private string _tabPageCssClass = string.Empty;
		private string _tabTitleCssFamily = "tab";
		private string _tabContentWrapperCssClass = string.Empty;

		public sTabDT()
			: base()
		{
			this.TabTitle = "Tab";
		}

		public string TabTitle
		{
			get { return this.ViewState["__tabTitle"].ToString(); }
			set { this.ViewState["__tabTitle"] = value; }
		}

		public string TabPageCssClassSuffix
		{
			get { return _tabPageCssClass; }
			set { _tabPageCssClass = value; }
		}

		public string TabTitleCssFamily
		{
			get { return _tabTitleCssFamily; }
			set { _tabTitleCssFamily = value; }
		}

		public string TabContentWrapperCssClass
		{
			get { return _tabContentWrapperCssClass; }
			set { _tabContentWrapperCssClass = value; }
		}

		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			if( ( (sTabStripDT)this.Parent ).RenderStyle != TabStripRenderStyleDT.Table )
			{
				this.Attributes.Add( "class", string.Format( "tab-page{0}", _tabPageCssClass ) );
				base.RenderBeginTag( writer );
				writer.Write( string.Format( "<h2 class=\"{0}{3}\"{2}>{1}</h2>", _tabTitleCssFamily, this.TabTitle, !this.Enabled ? " disabled=\"disabled\"" : string.Empty, !this.Enabled ? "_disabled" : string.Empty ) );
				if( !string.IsNullOrEmpty( _tabContentWrapperCssClass ) )
				{
					writer.WriteLine( string.Format( "<div class=\"{0}\">", _tabContentWrapperCssClass ) );
				}
			}
			else
			{
				writer.WriteLine( "<td id=\"{0}\" class=\"{1}\" style=\"display: {2};\">", this.ClientID, this.CssClass, _displayTab ? "" : "none" );
			}
		}

		public override void RenderEndTag(HtmlTextWriter writer)
		{
			if( ( (sTabStripDT)this.Parent ).RenderStyle != TabStripRenderStyleDT.Table )
			{
				if( ( (sTabStripDT)this.Parent ).RenderStyle != TabStripRenderStyleDT.Table &&
					!string.IsNullOrEmpty( _tabContentWrapperCssClass ) )
				{
					writer.WriteLine( "</div>" );
				}
				base.RenderEndTag( writer );
			}
			else
			{
				writer.WriteLine( "</td>" );
			}
		}

		public string ClientButtonID { get { return string.Format( "{0}Btn", this.ClientID ); } }
		private bool _displayTab = false;
		internal bool DisplayTab { get { return _displayTab; } set { _displayTab = value; } }
	}


	/// <summary>
	/// Provides secuirty, extended validation properties, and auto-validation.
	/// </summary>
	[ToolboxItem( true ),
		System.Drawing.ToolboxBitmap( typeof( System.Web.UI.WebControls.Panel ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.None,
		ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.VisibleChanged | ControlEvents.TextChanged )]
	internal class sTabStripDialogHeaderDT : sPanel
	{
		public sTabStripDialogHeaderDT()
			: base()
		{
		}
	}


	internal class sTabsSetupScriptDT : WebControl
	{
		private const string __isPostBack = "[ispostback]";
		private string _functionName = "setupAllTabs";
		private string _functionParamters = __isPostBack;

		public sTabsSetupScriptDT() : base() { }

		public string FunctionName
		{
			get { return _functionName; }
			set { _functionName = value; }
		}

		public string FunctionParamters
		{
			get { return _functionParamters; }
			set { _functionParamters = value; }
		}

		protected override void Render(HtmlTextWriter writer)
		{
			string parms = _functionParamters == __isPostBack ? this.Page.IsPostBack.ToString().ToLower() : _functionParamters;
			writer.Write( string.Format( "<script type=\"text/javascript\">{0}({1});</script>", _functionName, parms ) );
			//base.Render( writer );
		}
	}
}