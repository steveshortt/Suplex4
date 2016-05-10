using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace Suplex.WinForms
{
	/// <summary>
	/// Summary description for DataGrid.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(System.Windows.Forms.DataGrid))]
	public class mDataGrid : System.Windows.Forms.DataGrid
	{
		protected bool _ConfirmDelete = true;
		protected string _ConfirmationDlgTitle = "Confirm Delete";
		protected string _ConfirmationDlgMessage = "Are you sure you want to delete the selected record(s)?";
		
		public bool ConfirmDelete
		{
			get
			{
				return _ConfirmDelete;
			}
			set
			{
				_ConfirmDelete = value;
			}
		}


		public string ConfirmationDlgTitle
		{
			get
			{
				return _ConfirmationDlgTitle;
			}
			set
			{
				_ConfirmationDlgTitle = value;
			}
		}


		public string ConfirmationDlgMessage
		{
			get
			{
				return _ConfirmationDlgMessage;
			}
			set
			{
				_ConfirmationDlgMessage = value;
			}
		}


		protected override bool ProcessDialogKey( Keys keyData )
		{ 
			if( _ConfirmDelete && keyData == Keys.Delete )
			{ 
				DialogResult ConfirmDelete = 
					MessageBox.Show( _ConfirmationDlgMessage,
									_ConfirmationDlgTitle,
									MessageBoxButtons.YesNo, 
									MessageBoxIcon.Question, 
									MessageBoxDefaultButton.Button2);

				if( ConfirmDelete == DialogResult.No )
				{
					return true;
				}
			} 
 
			return base.ProcessDialogKey( keyData );
		} 
	}
}
