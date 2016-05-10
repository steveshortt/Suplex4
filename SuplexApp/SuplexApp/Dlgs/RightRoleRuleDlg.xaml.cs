using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

using api = Suplex.Forms.ObjectModel.Api;


namespace SuplexApp
{
	/// <summary>
	/// Interaction logic for RightRoleRuleDlg.xaml
	/// </summary>
	public partial class RightRoleRuleDlg : Window
	{
		private bool _dlgSuccess = false;
		private bool _shuttingDown = false;

		public RightRoleRuleDlg()
		{
			InitializeComponent();

			Application.Current.MainWindow.Closing +=
				new System.ComponentModel.CancelEventHandler( this.MainDlg_Closing );
		}

		//when using a single instance of a Window, the instance is unusable after calling .Close()
		//the MainDlg_Closing and Window_Closing methods below are a work-around for this
		private void MainDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_shuttingDown = true;
			this.Close();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if( !_shuttingDown )
			{
				typeof( Window ).GetField( "_isClosing", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( this, false );
				e.Cancel = true;
				this.Hide();
			}
		}

        private void Ok_Click(object sender, EventArgs e)
        {
            this.DialogResult = _dlgSuccess = true;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = _dlgSuccess = false;
        }


        #region public accessors
		public api.RightRoleRule RightRoleRule
		{
            get { return (api.RightRoleRule)ruleCtrl.DataContext; }
		}

		//substitute prop for DialogResult
		//	necessary due to non-Window.Close() methodology in use above - DialogResult always returns null.
		public bool Success
		{
            get { return _dlgSuccess; }
		}

		public void InitDlg(api.RightRoleRule rule)
		{
			ruleCtrl.SetDataContext( rule );
		}

		public api.SuplexStore SplxStore
		{
			get { return ruleCtrl.SplxStore; }
			set { ruleCtrl.SplxStore = value; }
		}
		#endregion
	}
}