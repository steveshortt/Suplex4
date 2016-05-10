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
	public class sTabStrip : sPanel
	{
		private List<sTab> _tabs;
		private HiddenField hfSelIdx = null;

		private sTabStripHeader _header = null;
		private sTabStripFooter _footer = null;

		private sTabElementWrapper _outerWrapper = sTabElementWrapper.Div;
		private sTabElementWrapper _contentWrapper = sTabElementWrapper.Div;
		private sTabElementWrapper _tabBodyElements = sTabElementWrapper.Table;

		private string _normalCls = string.Empty;
		private string _selectedCls = string.Empty;
		private string _hoverCls = string.Empty;
		private string _disabledCls = string.Empty;
		private string _tabRow0CssClass = string.Empty;
		private string _tabRowNCssClass = string.Empty;
		private string _contentWrapperCls = string.Empty;
		private string _bodyElementCls = string.Empty;


		public sTabStrip()
			: base()
		{
			_tabs = new List<sTab>( 5 );

			hfSelIdx = new HiddenField();
			hfSelIdx.ID = "activeTab";
			this.SelectedTabIndex = 0;
		}

		#region properties
		public List<sTab> Tabs
		{
			get { return _tabs; }
			set { _tabs = value; }
		}

		[Obsolete( "Use SelectedTabIndex instead. [SetSelectedTabIndex] is maintained for backwards compatibility.", false )]
		public void SetSelectedTabIndex(int index)
		{
			this.SelectedTabIndex = index;
		}

		public int SelectedTabIndex
		{
			get
			{
				int i = 0;
				Int32.TryParse( hfSelIdx.Value, out i );
				return i;
			}
			set { hfSelIdx.Value = value.ToString(); }
		}

		public sTabElementWrapper TabOuterWrapper
		{
			get { return _outerWrapper; }
			set { _outerWrapper = value; }
		}
		public sTabElementWrapper TabContentWrapper
		{
			get { return _contentWrapper; }
			set { _contentWrapper = value; }
		}

		public string TabNormalCssClass
		{
			get { return _normalCls; }
			set { _normalCls = value; }
		}
		public bool HasTabNormalCssClass { get { return !string.IsNullOrEmpty( _normalCls ); } }

		public string TabSelectedCssClass
		{
			get { return _selectedCls; }
			set { _selectedCls = value; }
		}
		public bool HasTabSelectedCssClass { get { return !string.IsNullOrEmpty( _selectedCls ); } }

		public string TabHoverCssClass
		{
			get { return _hoverCls; }
			set { _hoverCls = value; }
		}
		public bool HasTabHoverCssClass { get { return !string.IsNullOrEmpty( _hoverCls ); } }

		public string TabDisabledCssClass
		{
			get { return _disabledCls; }
			set { _disabledCls = value; }
		}
		public bool HasTabDisabledCssClass { get { return !string.IsNullOrEmpty( _disabledCls ); } }


		public string TabRow0CssClass
		{
			get { return _tabRow0CssClass; }
			set { _tabRow0CssClass = value; }
		}
		public bool HasTabRow0CssClass { get { return !string.IsNullOrEmpty( _tabRow0CssClass ); } }

		public string TabRowNCssClass
		{
			get { return _tabRowNCssClass; }
			set { _tabRowNCssClass = value; }
		}
		public bool HasTabRowNCssClass { get { return !string.IsNullOrEmpty( _tabRowNCssClass ); } }

		public string TabContentWrapperCls
		{
			get { return _contentWrapperCls; }
			set { _contentWrapperCls = value; }
		}
		public string TabBodyElementCls
		{
			get { return _bodyElementCls; }
			set { _bodyElementCls = value; }
		}
		#endregion

		#region overrides
		protected override void OnInit(EventArgs e)
		{
			this.Controls.Add( hfSelIdx );

			base.OnInit( e );

			//if( !string.IsNullOrEmpty( this.Page.Request[hfSelIdx.UniqueID] ) )
			//{
			//    int i = 0;
			//    if( Int32.TryParse( this.Page.Request[hfSelIdx.UniqueID], out i ) )
			//    {
			//        this.SelectedTabIndex = i;
			//    }
			//}

			foreach( Control c in this.Controls )
			{
				if( c is sTab )
				{
					_tabs.Add( (sTab)c );
				}

				if( c is sTabStripHeader )
				{
					_header = (sTabStripHeader)c;
				}

				if( c is sTabStripFooter )
				{
					_footer = (sTabStripFooter)c;
				}
			}
		}

		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				HtmlTextWriterTag tagKey = base.TagKey;

				switch( _outerWrapper )
				{
					case sTabElementWrapper.Div:
					case sTabElementWrapper.None:
					{
						tagKey = base.TagKey;
						break;
					}
					case sTabElementWrapper.Table:
					{
						tagKey = HtmlTextWriterTag.Table;
						break;
					}
				}

				return tagKey;
			}
		}

		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			this.Attributes["splxTabStrip"] = hfSelIdx.ClientID;

			string classData = string.Format( "{0},{1},{2},{3}", _normalCls, _selectedCls, _hoverCls, _disabledCls );
			if( classData.Length > 3 )//3 is the number of commas
			{
				this.Attributes["classData"] = classData;
			}
			base.RenderBeginTag( writer );


			hfSelIdx.RenderControl( writer );


			if( _header != null )
			{
				_header.RenderControl( writer );
			}


			int r = 0;
			int row0 = 0;
			List<List<sTab>> rows = this.OrderTabs( out row0 );
			foreach( List<sTab> row in rows )
			{
				writer.WriteLine( "\r\n\t<ul id=\"{0}{1}row{2}\"{3}>", this.ClientID, this.ClientIDSeparator, r,
					this.GetClsString( r == rows.Count - 1 ? _tabRow0CssClass : _tabRowNCssClass ) );
				foreach( sTab tab in row )
				{
					if( tab.Visible )
					{
						writer.WriteLine( "\t\t<li id=\"{0}\" tabBody=\"{1}\"{2}{3}{4}>{5}</li>",
							tab.ClientTabID, tab.ClientID, this.GetTabCssClass( tab ),
							this.GetTabStyle( tab ), this.GetTabClickProxy( tab ), tab.TabTitle );
					}
				}
				writer.WriteLine( "\t</ul>" );
				r++;
			}


			switch( _contentWrapper )
			{
				case sTabElementWrapper.Div:
				{
					writer.WriteLine( "<div{0}>", this.GetClsString( _contentWrapperCls ) );
					break;
				}
				case sTabElementWrapper.Table:
				{
					writer.WriteLine( "<table{0}>", this.GetClsString( _contentWrapperCls ) );
					writer.WriteFullBeginTag( "tr" );
					writer.WriteFullBeginTag( "td" );
					break;
				}
			}

			switch( _tabBodyElements )
			{
				case sTabElementWrapper.Div:
				{
					writer.WriteLine( "<div{0}>", this.GetClsString( _bodyElementCls ) );
					break;
				}
				case sTabElementWrapper.Table:
				{
					writer.WriteLine( "<table{0}>", this.GetClsString( _bodyElementCls ) );
					writer.WriteFullBeginTag( "tr" );
					break;
				}
			}
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			foreach( sTab t in _tabs )
			{
				t.RenderControl( writer );
			}

			//base.RenderContents( writer );
		}

		public override void RenderEndTag(HtmlTextWriter writer)
		{
			switch( _tabBodyElements )
			{
				case sTabElementWrapper.Div:
				{
					writer.WriteEndTag( "div" );
					break;
				}
				case sTabElementWrapper.Table:
				{
					writer.WriteEndTag( "tr" );
					writer.WriteEndTag( "table" );
					break;
				}
			}

			switch( _contentWrapper )
			{
				case sTabElementWrapper.Div:
				{
					writer.WriteEndTag( "div" );
					break;
				}
				case sTabElementWrapper.Table:
				{
					writer.WriteEndTag( "td" );
					writer.WriteEndTag( "tr" );
					writer.WriteEndTag( "table" );
					break;
				}
			}

			if( _footer != null )
			{
				_footer.RenderControl( writer );
			}

			base.RenderEndTag( writer );
		}
		#endregion

		#region private functions
		private List<List<sTab>> OrderTabs(out int row0)
		{
			int r = 0;
			int c = _tabs.Count;
			List<List<sTab>> rows = new List<List<sTab>>(c);
			for( ; r < c; r++ )
			{
				rows.Add( new List<sTab>( 5 ) );
			}

			c--;
			int i = 0;
			row0 = 0;
			r = 0;
			foreach( sTab t in _tabs )
			{
				t.TabBodyElement = _tabBodyElements;

				r = t.TabRowIndex > c ? c : t.TabRowIndex;
				rows[r].Add( t );
				if( this.SelectedTabIndex == i )
				{
					row0 = r;
					t.IsActiveTab = true;
				}
				i++;
			}

			for( r = rows.Count - 1; r > -1; r-- )
			{
				if( rows[r].Count == 0 )
				{
					rows[r] = null;
					rows.RemoveAt( r );
				}
			}

			return rows;
		}

		private string GetTabCssClass(sTab tab)
		{
			string cls = string.Empty;

			if( tab.Enabled )
			{
				if( tab.IsActiveTab )
				{
					if( tab.HasTabSelectedCssClass )
					{
						cls = tab.TabSelectedCssClass;
					}
					else if( this.HasTabSelectedCssClass )
					{
						cls = this.TabSelectedCssClass;
					}
				}
				else
				{
					if( tab.HasTabNormalCssClass )
					{
						cls = tab.TabNormalCssClass;
					}
					else if( this.HasTabNormalCssClass )
					{
						cls = this.TabNormalCssClass;
					}
				}
			}
			else
			{
				if( tab.HasTabDisabledCssClass )
				{
					cls = tab.TabDisabledCssClass;
				}
				else if( this.HasTabDisabledCssClass )
				{
					cls = this.TabDisabledCssClass;
				}
			}

			return this.GetClsString( cls );
		}

		private string GetClsString(string cls)
		{
			return string.IsNullOrEmpty( cls ) ? string.Empty : string.Format( " class=\"{0}\"", cls );
		}

		private string GetTabStyle(sTab tab)
		{
			return tab.HasTabStyle ? string.Format( " style=\"{0}\"", tab.TabStyle ) : string.Empty;
		}

		private string GetTabClickProxy(sTab tab)
		{
			string click = string.Empty;
			if( tab.HasTabClickClientProxy )
			{
				Control ctrl = tab.FindControl( tab.TabClickClientProxy );
				if( ctrl != null )
				{
					click = string.Format( " clickProxy=\"{0}\"", ctrl.ClientID );
				}
			}
			return click;
			//return tab.HasOnClientClick ? string.Format( " tabClick=\"{0}\"", tab.OnClientClick ) : string.Empty;
		}
		#endregion
	}


	/// <summary>
	/// Provides secuirty, extended validation properties, and auto-validation.
	/// </summary>
	[ToolboxItem( true ),
		System.Drawing.ToolboxBitmap( typeof( System.Web.UI.WebControls.Panel ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.None,
		ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.VisibleChanged | ControlEvents.TextChanged )]
	public class sTab : sPanel
	{
		private sTabElementWrapper _tabBodyElements = sTabElementWrapper.Table;
		private bool _isActiveTab = false;

		private string _normalCls = string.Empty;
		private string _selectedCls = string.Empty;
		private string _hoverCls = string.Empty;
		private string _disabledCls = string.Empty;
		private string _tabStyle = string.Empty;

		private int _rowIndex = 0;



		public sTab()
			: base()
		{
			this.TabTitle = "Tab";
		}



		internal virtual sTabElementWrapper TabBodyElement { get { return _tabBodyElements; } set { _tabBodyElements = value; } }
		internal virtual bool IsActiveTab { get { return _isActiveTab; } set { _isActiveTab = value; } }

		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				HtmlTextWriterTag tagKey = base.TagKey;

				switch( _tabBodyElements )
				{
					case sTabElementWrapper.Div:
					{
						tagKey = base.TagKey;
						break;
					}
					case sTabElementWrapper.Table:
					case sTabElementWrapper.None:
					{
						tagKey = HtmlTextWriterTag.Td;
						break;
					}
				}

				return tagKey;
			}
		}

		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			if( _tabBodyElements != sTabElementWrapper.Div )
			{
				this.Attributes["valign"] = "top";
			}

			if( _isActiveTab && this.Enabled )
			{
				this.Attributes["style"] = "display: ;";
			}
			else
			{
				this.Attributes["style"] = "display: none;";
			}

			base.RenderBeginTag( writer );
		}

		#region properties
		public string TabTitle
		{
			get { return this.ViewState["__tabTitle"].ToString(); }
			set { this.ViewState["__tabTitle"] = value; }
		}

		public int TabRowIndex
		{
			get { return _rowIndex < 0 ? 0 : _rowIndex; }
			set { _rowIndex = value; }
		}

		public string ClientTabID { get { return string.Format( "{0}_tab", this.ClientID ); } }

		public string TabNormalCssClass
		{
			get { return _normalCls; }
			set { _normalCls = value; }
		}
		public bool HasTabNormalCssClass { get { return !string.IsNullOrEmpty( _normalCls ); } }

		public string TabSelectedCssClass
		{
			get { return _selectedCls; }
			set { _selectedCls = value; }
		}
		public bool HasTabSelectedCssClass { get { return !string.IsNullOrEmpty( _selectedCls ); } }

		public string TabHoverCssClass
		{
			get { return _hoverCls; }
			set { _hoverCls = value; }
		}
		public bool HasTabHoverCssClass { get { return !string.IsNullOrEmpty( _hoverCls ); } }

		public string TabDisabledCssClass
		{
			get { return _disabledCls; }
			set { _disabledCls = value; }
		}
		public bool HasTabDisabledCssClass { get { return !string.IsNullOrEmpty( _disabledCls ); } }

		public string TabStyle
		{
			get { return _tabStyle; }
			set { _tabStyle = value; }
		}
		public bool HasTabStyle { get { return !string.IsNullOrEmpty( _tabStyle ); } }

		//future use...
		//public string OnClientClick
		//{
		//    get { return this.ViewState["__onClientClick"] == null ? string.Empty : this.ViewState["__onClientClick"].ToString(); }
		//    set { this.ViewState["__onClientClick"] = value; }
		//}
		//public bool HasOnClientClick { get { return !string.IsNullOrEmpty( this.OnClientClick ); } }

		public string TabClickClientProxy
		{
			get { return this.ViewState["__clientClickProxy"] == null ? string.Empty : this.ViewState["__clientClickProxy"].ToString(); }
			set { this.ViewState["__clientClickProxy"] = value; }
		}
		public bool HasTabClickClientProxy { get { return !string.IsNullOrEmpty( this.TabClickClientProxy ); } }
		#endregion
	}


	public abstract class sTabStripPanelBase : sPanel
	{
		private sTabElementWrapper _tabBodyElements = sTabElementWrapper.Div;

		public sTabStripPanelBase()
			: base()
		{
		}

		public virtual sTabElementWrapper Wrapper { get { return _tabBodyElements; } set { _tabBodyElements = value; } }


		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				HtmlTextWriterTag tagKey = base.TagKey;

				switch( _tabBodyElements )
				{
					case sTabElementWrapper.Div:
					{
						tagKey = base.TagKey;
						break;
					}
					case sTabElementWrapper.Table:
					{
						tagKey = HtmlTextWriterTag.Table;
						break;
					}
					case sTabElementWrapper.None:
					{
						tagKey = HtmlTextWriterTag.Unknown;
						break;
					}
				}

				return tagKey;
			}
		}

		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			switch( _tabBodyElements )
			{
				case sTabElementWrapper.Div:
				{
					base.RenderBeginTag( writer );
					break;
				}
				case sTabElementWrapper.Table:
				{
					base.RenderBeginTag( writer );
					writer.WriteFullBeginTag( "tr" );
					writer.WriteFullBeginTag( "td" );
					break;
				}
				case sTabElementWrapper.None:
				{
					break;
				}
			}
		}

		public override void RenderEndTag(HtmlTextWriter writer)
		{
			switch( _tabBodyElements )
			{
				case sTabElementWrapper.Div:
				{
					writer.WriteEndTag( "div" );
					break;
				}
				case sTabElementWrapper.Table:
				{
					writer.WriteEndTag( "td" );
					writer.WriteEndTag( "tr" );
					writer.WriteEndTag( "table" );
					break;
				}
				case sTabElementWrapper.None:
				{
					break;
				}
			}
		}
	}


	/// <summary>
	/// Provides secuirty, extended validation properties, and auto-validation.
	/// </summary>
	[ToolboxItem( true ),
		System.Drawing.ToolboxBitmap( typeof( System.Web.UI.WebControls.Panel ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.None,
		ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.VisibleChanged | ControlEvents.TextChanged )]
	public class sTabStripHeader : sTabStripPanelBase
	{
	}


	/// <summary>
	/// Provides secuirty, extended validation properties, and auto-validation.
	/// </summary>
	[ToolboxItem( true ),
		System.Drawing.ToolboxBitmap( typeof( System.Web.UI.WebControls.Panel ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.None,
		ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.VisibleChanged | ControlEvents.TextChanged )]
	public class sTabStripFooter : sTabStripPanelBase
	{
	}


	public class sTabsSetupScript : WebControl
	{
		private const string __isPostBack = "[ispostback]";
		private string _functionName = "setupListTabs";
		private string _functionParamters = __isPostBack;

		public sTabsSetupScript() : base() { }

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


	public enum sTabElementWrapper
	{
		None,
		Div,
		Table
	}
}