using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Collections;
using System.Data;
using System.Windows.Forms;


namespace Suplex.WinForms.Specialized
{
	/// <summary>
	/// Summary description for Reporter.
	/// </summary>
	public class ReportingUtils
	{

		private static System.Web.UI.WebControls.HorizontalAlign _horizontalAlign = System.Web.UI.WebControls.HorizontalAlign.NotSet;
		private static string _inlineCss = string.Empty;


		/*
		private string			_StoredProcedureName;
		private SortedList		_SPParameters = null;
		private string			_Title;

		_StoredProcedureName	= StoredProcedureName;
		_SPParameters			= SPParameters;
		_Title					= Title;

		
		protected DataAccessor								da				= null;
		protected System.Collections.SortedList				CmdParameters	= null;
		protected System.Data.SqlClient.SqlCommand			sqlCmd			= null;
		protected System.Data.SqlClient.SqlDataReader		Reader			= null;
		*/



		public ReportingUtils(){}


		public static DataTable CreateDataSource( ListView listView )
		{
			DataTable t = new DataTable();
			foreach( ColumnHeader col in listView.Columns )
			{
				t.Columns.Add(col.Text);
			}

			DataRow r = null;
			foreach( ListViewItem item in listView.Items )
			{
				r = t.NewRow();
				for(int i = 0; i < item.SubItems.Count; i++)
				{
					r[i] = item.SubItems[i].Text;
				}
				t.Rows.Add(r);
			}
			t.AcceptChanges();

			return t;
		}


		public static string InlineCss { get { return _inlineCss; } set { _inlineCss = value; } }

		public static StringWriter CreateHtml( object reportDataSource, string title, string cssPath )
		{
			return CreateHtml( reportDataSource, null, title, cssPath );
		}


		public static StringWriter CreateHtml( object reportDataSource, System.Web.UI.WebControls.DataGrid reportDataGrid, string title, string cssPath )
		{

			StringWriter writer	= new StringWriter();
			HtmlTextWriter html = new HtmlTextWriter(writer);

			if( reportDataSource != null )
			{

				//outer alignment table
				Table table0 = new Table();
				table0.BorderWidth = Unit.Pixel( 0 );
				table0.CellSpacing = 0;
				table0.CellPadding = 1;
				table0.HorizontalAlign = _horizontalAlign;

				/*
				TableRow row0 = new TableRow();
				table0.Rows.Add(row0);

				TableCell cell0	= new TableCell();
				cell0.CssClass	= "reportTitle";
				cell0.Text		= title;
				row0.Cells.Add(cell0);
				*/

				TableRow row0 = new TableRow();
				table0.Rows.Add(row0);

				TableCell cell0 = new TableCell();
				row0.Cells.Add(cell0);

				if( reportDataGrid == null )
				{
					reportDataGrid = new System.Web.UI.WebControls.DataGrid();

					reportDataGrid.AutoGenerateColumns				= true;
					//reportDataGrid.BorderWidth						= Unit.Pixel(0);
					//reportDataGrid.CellPadding						= 3;
					//reportDataGrid.CellSpacing						= 1;
					reportDataGrid.Width							= Unit.Percentage(100);
					reportDataGrid.HeaderStyle.CssClass				= "reportHeader";
					reportDataGrid.ItemStyle.CssClass				= "reportHc";
					reportDataGrid.AlternatingItemStyle.CssClass	= "reportLc";
					reportDataGrid.FooterStyle.CssClass				= "reportFooter";
					reportDataGrid.ShowFooter						= true;
				}

				//da = new DataAccessor();
				//Reader = da.GetReader(_StoredProcedureName, _SPParameters);
				reportDataGrid.DataSource = reportDataSource;
				reportDataGrid.DataBind();
				//Reader.Close();

				cell0.Controls.Add( reportDataGrid );


				//write the report to HTML, return it
				html.WriteFullBeginTag("html");
				html.WriteLine();

				html.WriteFullBeginTag("head");
				html.WriteLine();

				html.WriteFullBeginTag("title");
				html.Write( title );
				html.WriteEndTag("title");
				html.WriteLine();

				if( !string.IsNullOrEmpty( cssPath ) )
				{
					html.WriteBeginTag( "link" );
					html.WriteAttribute( "rel", "stylesheet" );
					html.WriteAttribute( "type", "text/css" );
					html.WriteAttribute( "href", cssPath );
					html.Write( "/>" );
					html.WriteLine();
				}
				else
				{
					if( !string.IsNullOrEmpty( _inlineCss ) )
					{
						html.WriteBeginTag( "style" );
						html.WriteLineNoTabs( _inlineCss );
						html.WriteEndTag( "style" );
					}
				}

				html.WriteBeginTag("META");
				html.WriteAttribute("HTTP-EQUIV", "pragma");
				html.WriteAttribute("CONTENT", "no-cache");
				html.Write( ">" );
				html.WriteLine();

				html.WriteEndTag("head");
				html.WriteLine();

				html.WriteFullBeginTag("body");
				html.WriteLine();
				
				html.WriteFullBeginTag("h2");
				html.Write( title );
				html.WriteEndTag("h2");
				html.WriteLine();

				table0.RenderControl(html);

				html.WriteEndTag("body");
				html.WriteLine();
				
				html.WriteEndTag("html");
			}

			return writer;

		}



		public static void SaveAsCsv( DataTable data )
		{
			SaveFileDialog saveDlg = new SaveFileDialog();
			saveDlg.Filter = "Comma Separated Values (*.csv)|*.csv|All Files|*.*";
			saveDlg.Title = "Save Data";
			
			if( saveDlg.ShowDialog() == DialogResult.OK )
			{
				StringWriter csvWriter = new StringWriter();

				int c=0;
				object[] args = new object[data.Columns.Count];


				//column headers
				args[c] = data.Columns[c].ColumnName.Replace( "\"", "\"\"" );
				string format = "\"{" + c++ + "}\"";
				for( ; c<data.Columns.Count; c++ )
				{
					format += ",\"{" + c + "}\"";
					args[c] = data.Columns[c].ColumnName.Replace( "\"", "\"\"" );
				}
				csvWriter.WriteLine( format, args );


				//data
				for( int r=0; r<data.Rows.Count; r++ )
				{
					args[0] = data.Rows[r][0].ToString().Replace( "\"", "\"\"" );
					for( c = 1; c<data.Columns.Count; c++ )
					{
						args[c] = data.Rows[r][c].ToString().Replace( "\"", "\"\"" );
					}
					csvWriter.WriteLine( format, args );
				}

				//write it...
				StreamWriter report = new StreamWriter( saveDlg.FileName );
				report.Write( csvWriter.ToString() );
				report.Close();
			}
		}


	}
}
