using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

using Suplex.Data;
using Suplex.Forms;

namespace Suplex.WebForms
{
	/// <summary>
	/// Summary description for sValidationSummary.
	/// </summary>
	[DefaultProperty("Title"), 
		ToolboxData("<{0}:ValidationSummary runat=server></{0}:ValidationSummary>"),
		ToolboxItem(true),
		System.Drawing.ToolboxBitmap(typeof(System.Web.UI.WebControls.ValidationSummary))]
	public class sValidationSummary : Panel, INamingContainer, IValidationSummaryControl	//Control
	{
		private struct ErrItem
		{
			public IValidationControl	ErrControl;
			public string				ErrMessage;

			public ErrItem( IValidationControl errControl, string errMessage )
			{
				ErrControl = errControl;
				ErrMessage = errMessage;
			}
		}
		
		private SortedList _controls = new SortedList();
		short i = 0;
		//private Table table0			= new Table();


		private string		_Title				= "Error!";
		private SortedList	_MessageList		= new SortedList();
		private string		_OuterClass			= "ErrorOuter";
		private string		_InnerClass			= "ErrorInner";
		private string		_TitleClass			= "ErrorTitle";
		private string		_MessageLabelClass	= "ErrorMessageLabel";
		private string		_MessageBodyClass	= "ErrorMessageBody";
		private string _highlightColor = "yellow";
		private string _defaultColor = "white";
		private string _highlightCssClass = string.Empty;
		private string _defaultCssClass = string.Empty;
		private bool _createInRender = true;
		private bool _rendered = false;

		private System.Web.UI.WebControls.HorizontalAlign	_HorizontalAlign	= System.Web.UI.WebControls.HorizontalAlign.NotSet;
		private System.Web.UI.WebControls.Unit				_Width				= Unit.Percentage(65);
		private int											_Padding			= 10;

	
		[Bindable(true), 
		Category("Appearance"), 
		DefaultValue("")]
		public string Title
		{
			get { return _Title; }
			set { _Title = value; }
		}

		public SortedList MessageList
		{
			get { return _MessageList; }
			set { _MessageList = value; }
		}

		public string InnerClass
		{
			get { return _InnerClass; }
			set { _InnerClass = value; }
		}

		public string OuterClass
		{
			get { return _OuterClass; }
			set { _OuterClass = value; }
		}

		public string TitleClass
		{
			get { return _TitleClass; }
			set { _TitleClass = value; }
		}

		public string MessageLabelClass
		{
			get { return _MessageLabelClass; }
			set { _MessageLabelClass = value; }
		}

		public string MessageBodyClass
		{
			get { return _MessageBodyClass; }
			set { _MessageBodyClass = value; }
		}

		public string HighlightErrorColor
		{
			get { return _highlightColor; }
			set { _highlightColor = value; }
		}

		public string HighlightNormalColor
		{
			get { return _defaultColor; }
			set { _defaultColor = value; }
		}

		public string HighlightErrorCssClass
		{
			get { return _highlightCssClass; }
			set { _highlightCssClass = value; }
		}

		public string HighlightNormalCssClass
		{
			get { return _defaultCssClass; }
			set { _defaultCssClass = value; }
		}

		new public System.Web.UI.WebControls.HorizontalAlign HorizontalAlign
		{
			set
			{
				_HorizontalAlign = value;
				ViewState["halign"] = value;
			}
			get
			{
				if(ViewState["halign"] != null)
				{
					return (System.Web.UI.WebControls.HorizontalAlign)ViewState["halign"];
				}
				else
				{
					return _HorizontalAlign;
				}
			}
		}

		public System.Web.UI.WebControls.Unit ErrWidth
		{
			set
			{
				_Width = value;
				ViewState["errWidth"] = value;
			}
			get
			{
				if(ViewState["errWidth"] != null)
				{
					return (System.Web.UI.WebControls.Unit)ViewState["errWidth"];
				}
				else
				{
					return _Width;
				}
			}
		}

		public int Padding
		{
			set
			{
				_Padding = value;
				ViewState["errPadding"] = value;
			}
			get
			{
				if(ViewState["errPadding"] != null)
				{
					return (int)ViewState["errPadding"];
				}
				else
				{
					return _Padding;
				}
			}
		}

		[Obsolete( "Use SetError.", false )]
		public void ErrorAdd( IValidationControl control, string message )
		{
			((WebControl)control).TabIndex = i++;
			_controls[control.UniqueName] = new ErrItem( control, message );
		}

		[Obsolete( "Use SetError(control, null | string.Empty).", false )]
		public void ErrorRemove(IValidationControl control)
		{
			_controls.Remove( control.UniqueName );
		}

		public void ErrorClearAll()
		{
			_controls.Clear();
		}

		public int ErrorCount
		{
			get
			{
				return _controls.Count;
			}
		}

		[Obsolete( "Dead method.", false )]
		public void Refresh()
		{
			//this.RefreshList();

			//CreateChildControls();

			//this.CreateSummary();
		}

		#region IValidationSummaryControl Members

		public void SetError(IValidationControl control, string errorMessage)
		{
			if( !string.IsNullOrEmpty( errorMessage ) )
			{
				( (WebControl)control ).TabIndex = i++;
				_controls[control.UniqueName] = new ErrItem( control, errorMessage );
			}
			else
			{
				_controls.Remove( control.UniqueName );
			}
		}

		#endregion

		public bool CreateInRender { get { return _createInRender; } set { _createInRender = value; } }

		
		
//		/// <summary> 
//		/// Render this control to the output parameter specified.
//		/// </summary>
//		/// <param name="output"> The HTML writer to write out to </param>
//		protected override void Render(HtmlTextWriter output)
//		{
//			table0.RenderControl( output );
//		}


		protected override void Render(HtmlTextWriter writer)
		{
			if( _createInRender )
			{
				if( !_rendered )
				{
					_rendered = true;
					this.CreateSummary().RenderControl( writer );
				}
			}
			else
			{
				base.Render( writer );
			}
		}

		protected override void CreateChildControls()
		{
			CreateHighlightScript();

			if( !this.ChildControlsCreated && !_createInRender )
			{
				this.Controls.Add( this.CreateSummary() );
				this.ChildControlsCreated = true;
			}
			else
			{
				base.CreateChildControls();
			}
		}


		public void CreateHighlightScript()
		{
			System.Text.StringBuilder script = new System.Text.StringBuilder( "<script language=\"javascript\">\r\n<!--\r\n" );
			script.Append( "\tfunction hiliter(target, over) {\r\n" );
			script.Append( "\t\ttarget.focus();\r\n" );
			script.Append( "\t\ttarget.select();\r\n" );
			script.Append( "\t\tif( over == 'true' ) {\r\n" );
			if( string.IsNullOrEmpty( _highlightCssClass ) )
			{
				script.AppendFormat( "\t\t\ttarget.style.backgroundColor = \"{0}\";\r\n", _highlightColor );
			}
			else
			{
				script.AppendFormat( "\t\t\ttarget.className = \"{0}\";\r\n", _highlightCssClass );
			}
			script.Append( "\t\t} else {\r\n" );
			if( string.IsNullOrEmpty( _defaultCssClass ) )
			{
				script.AppendFormat( "\t\t\ttarget.style.backgroundColor = \"{0}\";\r\n", _defaultColor );
			}
			else
			{
				script.AppendFormat( "\t\t\ttarget.className = \"{0}\";\r\n", _defaultCssClass );
			}
			script.Append( "\t\t}\r\n" );
			script.Append( "\t}\r\n// -->\r\n</script>" );

			if( !this.Page.IsClientScriptBlockRegistered( "hiliter" ) )
			{
				this.Page.RegisterClientScriptBlock( "hiliter", script.ToString() );
			}
		}

		private Table CreateSummary()
		{
			//outer alignment table
			Table table0 = new Table();
			table0.BorderWidth = Unit.Pixel( 0 );
			table0.CellSpacing = 0;
			table0.CellPadding = this.Padding;
			table0.Width = this.ErrWidth;
			table0.HorizontalAlign = this.HorizontalAlign;

			TableRow row0 = new TableRow();
			table0.Rows.Add( row0 );

			TableCell cell0 = new TableCell();
			row0.Cells.Add( cell0 );




			//setup the outer table
			Table menuOuterTable = new Table();
			menuOuterTable.CssClass = _OuterClass;

			TableRow menuOuterRow = new TableRow();
			menuOuterTable.Rows.Add( menuOuterRow );

			TableCell menuOuterCell = new TableCell();
			menuOuterRow.Cells.Add( menuOuterCell );
			//end setup the outer table


			//setup the inner table
			Table menuInnerTable = new Table();
			menuInnerTable.CssClass = _InnerClass;

			TableRow menuInnerRow = new TableRow();
			menuInnerTable.Rows.Add( menuInnerRow );

			HyperLink title = new HyperLink();
			title.Text = _Title;
			title.Attributes.Add( "name", "validationSummary" );

			TableCell menuInnerCell = new TableCell();
			menuInnerCell.ColumnSpan = 3;
			menuInnerCell.CssClass = _TitleClass;
			//menuInnerCell.Text = _Title;
			menuInnerCell.Controls.Add( title );
			menuInnerRow.Cells.Add( menuInnerCell );

			//				menuInnerRow = new TableRow();
			//				menuInnerTable.Rows.Add(menuInnerRow);
			//
			//				menuInnerCell = new TableCell();
			//				menuInnerCell.ColumnSpan = 3;
			//				menuInnerCell.CssClass = _MessageBodyClass;
			//				menuInnerCell.Text = "&nbsp;";
			//				menuInnerRow.Cells.Add(menuInnerCell);


			//redraw the control
			SortedList _sortedControls = new SortedList();
			string sortkey = string.Empty;
			IEnumerator ctrls = _controls.Keys.GetEnumerator();
			while( ctrls.MoveNext() ) //reorders the controls by TabIndex-UniqueName. this is a bit of a hack.
			{
				ErrItem errItem = (ErrItem)_controls[ctrls.Current];
				sortkey = ( (WebControl)errItem.ErrControl ).TabIndex.ToString( "X" ).PadLeft( 4, '0' );
				sortkey = string.Format( "{0}-{1}", sortkey, ctrls.Current.ToString() );
				_sortedControls.Add( sortkey, errItem );
			}
			ctrls = _sortedControls.Keys.GetEnumerator();
			while( ctrls.MoveNext() )
			{
				ErrItem errItem = (ErrItem)_sortedControls[ctrls.Current];
				( (WebControl)errItem.ErrControl ).Attributes.Add( "id", ( (WebControl)errItem.ErrControl ).ClientID );
				string js1 = "javascript:hiliter(document.forms[0]." + ( (WebControl)errItem.ErrControl ).ClientID + ", 'true');";
				string js2 = "javascript:hiliter(document.forms[0]." + ( (WebControl)errItem.ErrControl ).ClientID + ", 'false');";

				menuInnerRow = new TableRow();
				menuInnerTable.Rows.Add( menuInnerRow );

				menuInnerCell = new TableCell();
				menuInnerCell.CssClass = _MessageBodyClass;
				//menuInnerCell.Text = "·";
				HyperLink sh = new HyperLink();
				sh.NavigateUrl = js1;
				sh.Attributes.Add( "onmouseover", js1 );
				sh.Attributes.Add( "onmouseout", js2 );
				sh.Text = "[¤]";
				sh.CssClass = _MessageLabelClass;
				menuInnerCell.Controls.Add( sh );
				menuInnerCell.VerticalAlign = VerticalAlign.Top;
				menuInnerRow.Cells.Add( menuInnerCell );

				menuInnerCell = new TableCell();
				menuInnerCell.CssClass = _MessageBodyClass;
				menuInnerCell.Text = "&nbsp;";
				menuInnerRow.Cells.Add( menuInnerCell );

				menuInnerCell = new TableCell();
				menuInnerCell.CssClass = _MessageBodyClass;
				//menuInnerCell.Text = errItem.ErrMessage;
				menuInnerCell.VerticalAlign = VerticalAlign.Top;

				HyperLink b = new HyperLink();
				b.NavigateUrl = js1;
				//					b.Attributes.Add( "onmouseenter", js1 );
				//					b.Attributes.Add( "onmouseleave", js2 );
				b.Text = errItem.ErrMessage;
				menuInnerCell.Controls.Add( b );
				menuInnerRow.Cells.Add( menuInnerCell );
			}
			//end setup the inner table


			menuOuterCell.Controls.Add( menuInnerTable );
			//this.Controls.Add(menuOuterTable);

			cell0.Controls.Add( menuOuterTable );
			//this.Controls.Add( table0 );

			return table0;
		}
	}
}
