using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Suplex.Wpf
{
	/// <summary>
	/// Interaction logic for LinkLabel.xaml
	/// </summary>
	public partial class sLinkLabel : UserControl
	{
		public sLinkLabel()
		{
			//InitializeComponent();
		}

		public override void EndInit()
		{
			TextBlock txtContentHandler = (TextBlock)this.FindName( "txtContentHandler" );
			base.EndInit();
		}

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter( e );
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave( e );
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove( e );
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp( e );
		}

		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged( oldContent, newContent );
			//txtContentHandler.Text = "g";
			//( (TextBlock)this.ContentTemplate ).Text = (string)newContent;
		}

		private string _foo = string.Empty;
		public string Foo { get { return _foo; } set { _foo = value; } }
	}
}
