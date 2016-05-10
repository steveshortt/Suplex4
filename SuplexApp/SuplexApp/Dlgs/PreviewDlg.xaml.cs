using System.Reflection;
using System.Windows;

using api = Suplex.Forms.ObjectModel.Api;
using sf = Suplex.Forms;

namespace SuplexApp
{
	public partial class PreviewDlg : Window
	{
		private bool _shuttingDown = false;

		public PreviewDlg()
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

		public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register( "IsOpen", typeof( bool? ), typeof( PreviewDlg ) );

		public bool? IsOpen
		{
			get { return GetValue( IsOpenProperty ) as bool?; }
			set { SetValue( IsOpenProperty, value ); }
		}

		public void SetDataContext(api.ISuplexObject dataContext)
		{
			this.Title = dataContext.Name;

			switch( dataContext.ObjectType )
			{
				case sf.ObjectType.UIElement:
				{
					uiePrvw.Visibility = Visibility.Visible;
					uiePrvw.DataContext = dataContext;

					lrPrvw.Visibility = Visibility.Collapsed;
					fmPrvw.Visibility = Visibility.Collapsed;
					break;
				}
				case sf.ObjectType.ValidationRule:
				case sf.ObjectType.ElseRule:
				{
					uiePrvw.Visibility = Visibility.Collapsed;

					lrPrvw.Visibility = Visibility.Visible;
					lrPrvw.DataContext = dataContext;

					fmPrvw.Visibility = Visibility.Collapsed;
					break;
				}
				case sf.ObjectType.FillMap:
				case sf.ObjectType.ElseMap:
				{
					uiePrvw.Visibility = Visibility.Collapsed;
					lrPrvw.Visibility = Visibility.Collapsed;

					fmPrvw.Visibility = Visibility.Visible;
					fmPrvw.DataContext = dataContext;
					break;
				}
			}
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if( e.Property == IsOpenProperty )
			{
				bool? v = e.NewValue as bool?;
				if( v.Value )
				{
					this.Show();
				}
				else
				{
					this.Close();
				}
			}
			base.OnPropertyChanged( e );
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = System.Windows.WindowState.Minimized;
		}
	}
}