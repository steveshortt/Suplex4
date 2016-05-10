using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Suplex.WebForms
{
	/// <summary>
	/// Summary description for mMessageBox.
	/// </summary>
	public class sMessageBox : WebControl, INamingContainer
	{

		private string				_text;
		private string				_caption;
		private sMessageBoxButtons	_buttons;
		private sMessageBoxIcons	_icon;

		private string				_imagesPath = @"/images/";
		private string				_imagesPrefix = "msgbox_";
		private string				_imagesExtension = "png";

		private HorizontalAlign		_HorizontalAlign = HorizontalAlign.Center;

		private bool				_useViewState = true;


		#region public events
		public event sMessageBoxEventHandler Command;

		protected void OnCommand(sMessageBoxEventArgs e)
		{
			if( Command != null )
			{
				Command( this, e );
			}
		}
		#endregion


		#region public ctor/properties/methods
		public sMessageBox(){}


		public void Show(string text, string caption, sMessageBoxButtons buttons, sMessageBoxIcons icon)
		{
			_useViewState = false;

			_text = text;
			_caption = caption;
			_buttons = buttons;
			_icon = icon;

			this.Attributes = new sMessageBoxAttributes( _text, _caption, _buttons, _icon );

			this.Visible = true;

//			if( !ChildControlsCreated )
//			{
//				this.CreateChildControls();
//			}
		}

		public System.Web.UI.WebControls.HorizontalAlign HorizontalAlign
		{
			set
			{
				_HorizontalAlign = value;
				ViewState["__msgboxhAlign"] = value;
			}
			get
			{
				if(ViewState["__msgboxhAlign"] != null)
				{
					return (System.Web.UI.WebControls.HorizontalAlign)ViewState["__msgboxhAlign"];
				}
				else
				{
					return _HorizontalAlign;
				}
			}
		}

		public object Tag
		{
			get
			{
				return ViewState["__msgboxTag"];
			}
			set
			{
				ViewState["__msgboxTag"] = value;
			}
		}

		public string ImagesPath
		{
			get
			{
				return _imagesPath;
			}
			set
			{
				_imagesPath = value;
			}
		}

		public string ImagesPrefix
		{
			get
			{
				return _imagesPrefix;
			}
			set
			{
				_imagesPrefix = value;
			}
		}

		public string ImagesExtension
		{
			get
			{
				return _imagesExtension;
			}
			set
			{
				_imagesExtension = value;
			}
		}
		#endregion


		#region private implementation
		[Serializable]
		private struct sMessageBoxAttributes
		{
			public string Text;
			public string Caption;
			public sMessageBoxButtons Buttons;
			public sMessageBoxIcons Icon;

			public sMessageBoxAttributes(string text, string caption, sMessageBoxButtons buttons, sMessageBoxIcons icon)
			{
				Buttons = buttons;
				Caption = caption;
				Text = text;
				Icon = icon;
			}
		}

		private sMessageBoxAttributes Attributes
		{
			get
			{
				return (sMessageBoxAttributes)ViewState["__msgbox"];
			}
			set
			{
				ViewState["__msgbox"] = value;
			}
		}

		private void CheckViewState()
		{
			if( _useViewState )
			{
				if(ViewState["__msgbox"] != null)
				{
					_buttons = Attributes.Buttons;
					_caption = Attributes.Caption;
					_text = Attributes.Text;
					_icon = Attributes.Icon;
				}
			}
		}

		protected override void CreateChildControls()
		{
			CheckViewState();

			//outer alignment table
			Table table0 = new Table();
			table0.BorderWidth = Unit.Pixel(0);
			table0.CellSpacing = 0;
			table0.CellPadding = 0;
			table0.HorizontalAlign = this.HorizontalAlign;
			table0.Attributes.Add( "style", "margin-top:10px;margin-bottom:10px;" );

			TableRow row0 = new TableRow();
			table0.Rows.Add(row0);

			TableCell cell0 = new TableCell();
			row0.Cells.Add(cell0);



			//mMessageBox Main Table
			Table table = new Table();
			table.BorderWidth = Unit.Pixel(0);
			table.CellSpacing = 0;
			table.CellPadding = 3;


			//header row
			TableRow row = new TableRow();
			table.Rows.Add(row);

			TableCell cell = new TableCell();
			cell.HorizontalAlign = HorizontalAlign.Left;
			cell.CssClass = "formHeader";
			cell.Text = _caption;
			row.Cells.Add(cell);


			//message row
			row = new TableRow();
			table.Rows.Add(row);

			cell = new TableCell();
			cell.HorizontalAlign = HorizontalAlign.Center;
			cell.CssClass = "formLc";
			row.Cells.Add(cell);


			//inner message table
			Table table1 = new Table();
			table1.BorderWidth = Unit.Pixel(0);
			table1.CellSpacing = 0;
			table1.CellPadding = 2;

			TableRow row1 = new TableRow();
			table1.Rows.Add(row1);

			TableCell cell1 = new TableCell();
			cell1.HorizontalAlign = HorizontalAlign.Center;
			cell1.CssClass = "formLc";

			if( _icon != sMessageBoxIcons.None )
			{
				System.Web.UI.WebControls.Image imgIcon = new System.Web.UI.WebControls.Image();
				imgIcon.ImageUrl = string.Format( "{0}{1}{2}.{3}", _imagesPath, _imagesPrefix, _icon.ToString(), _imagesExtension );
				cell1.Controls.Add( imgIcon );
				cell1.HorizontalAlign = HorizontalAlign.Left;
				//cell1.Width = Unit.Pixel( 40 );
				row1.Cells.Add( cell1 );

//				cell1 = new TableCell();
//				cell1.HorizontalAlign = HorizontalAlign.Center;
//				cell1.CssClass = "formLc";
//				cell1.Text = "&nbsp;";
//				row1.Cells.Add(cell1);

				cell1 = new TableCell();
				cell1.HorizontalAlign = HorizontalAlign.Center;
				cell1.CssClass = "formLc";
			}

			cell1.Text = _text;
			row1.Cells.Add(cell1);

			cell.Controls.Add(table1);


			//buttons row
			row = new TableRow();
			table.Rows.Add(row);

			cell = new TableCell();
			cell.HorizontalAlign = HorizontalAlign.Center;
			cell.CssClass = "formLc";
			row.Cells.Add(cell);


			//inner button table
			Table table2 = new Table();
			table2.BorderWidth = Unit.Pixel(0);
			table2.CellSpacing = 0;
			table2.CellPadding = 2;

			TableRow row2 = new TableRow();
			table2.Rows.Add(row2);

			switch(_buttons)
			{
				case sMessageBoxButtons.OKOnly:
				{
					row2.Cells.Add(CreateButton(sMessageBoxResult.OK));
					break;
				}
				case sMessageBoxButtons.OKCancel:
				{
					row2.Cells.Add(CreateButton(sMessageBoxResult.OK));
					row2.Cells.Add(CreateButton(sMessageBoxResult.Cancel));
					break;
				}
				case sMessageBoxButtons.YesNo:
				{
					row2.Cells.Add(CreateButton(sMessageBoxResult.Yes));
					row2.Cells.Add(CreateButton(sMessageBoxResult.No));
					break;
				}
				case sMessageBoxButtons.YesNoCancel:
				{
					row2.Cells.Add(CreateButton(sMessageBoxResult.Yes));
					row2.Cells.Add(CreateButton(sMessageBoxResult.No));
					row2.Cells.Add(CreateButton(sMessageBoxResult.Cancel));
					break;
				}
				case sMessageBoxButtons.AbortRetryIgnore:
				{
					row2.Cells.Add(CreateButton(sMessageBoxResult.Abort));
					row2.Cells.Add(CreateButton(sMessageBoxResult.Retry));
					row2.Cells.Add(CreateButton(sMessageBoxResult.Ignore));
					break;
				}
				case sMessageBoxButtons.RetryCancel:
				{
					row2.Cells.Add(CreateButton(sMessageBoxResult.Retry));
					row2.Cells.Add(CreateButton(sMessageBoxResult.Cancel));
					break;
				}
			}
			cell.Controls.Add(table2);

			cell0.Controls.Add(table);

			this.Controls.Add(table0);
		}


		private TableCell CreateButton(sMessageBoxResult result)
		{
			TableCell cell = new TableCell();
			Button cmdButton = new Button();

			cmdButton.CssClass = "formButton";
			cmdButton.Command += new System.Web.UI.WebControls.CommandEventHandler(this.Button_Command);
			cmdButton.Text = result.ToString();
			cmdButton.CommandName = result.ToString();
			cmdButton.CommandArgument = ((int)result).ToString();
			//cmdButton.Width = Unit.Pixel(75);

			cell.Controls.Add(cmdButton);

			return cell;
		}


		private void Button_Command(object sender, System.Web.UI.WebControls.CommandEventArgs e)
		{
			sMessageBoxEventArgs args = new sMessageBoxEventArgs(Int32.Parse(e.CommandArgument.ToString()));
			OnCommand(args);

			this.ChildControlsCreated = false;
		}
		#endregion

	}


	public enum sMessageBoxButtons
	{
		OKOnly				= 0, //Display OK button only.
		OKCancel			= 1, //Display OK and Cancel buttons.
		AbortRetryIgnore	= 2, //Display Abort, Retry, and Ignore buttons.
		YesNoCancel			= 3, //Display Yes, No, and Cancel buttons.
		YesNo				= 4, //Display Yes and No buttons.
		RetryCancel			= 5 //Display Retry and Cancel buttons.
	}
	public enum sMessageBoxIcons
	{
		None				= 0, //no icon
		Error				= 16, //Display Critical Message icon.
		Question			= 32, //Display Warning Query icon.
		Exclamation			= 48, //Display Warning Message icon.
		Information			= 64 //Display Information Message icon.
	}
	public enum sMessageBoxResult
	{
		OK					= 1,
		Cancel				= 2,
		Abort				= 3,
		Retry				= 4,
		Ignore				= 5,
		Yes					= 6,
		No					= 7
	}


	#region MessageBoxEventHandler/MessageBoxEventArgs

	public delegate void sMessageBoxEventHandler(object sender, sMessageBoxEventArgs e);

	public class sMessageBoxEventArgs : EventArgs
	{
		private sMessageBoxResult _result;

		public sMessageBoxEventArgs(int result)
		{
			_result = (sMessageBoxResult)result;
		}


		public sMessageBoxResult Result
		{
			get
			{
				return _result;
			}
		}
	}

	#endregion

}
