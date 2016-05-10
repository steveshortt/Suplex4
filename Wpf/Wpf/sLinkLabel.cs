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
using System.ComponentModel;
using System.Collections;

using Suplex.Forms;
using splx = Suplex.Forms;
using Suplex.Security;
using Suplex.Data;


namespace Suplex.Wpf
{
	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	[ToolboxItem( true ), System.Drawing.ToolboxBitmap( typeof( GroupBox ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.None )]
	public class sLinkLabel : TextBlock
	{
		// Create an underline text decoration. Default is underline.
		TextDecoration underline = new TextDecoration();
		TextDecorationCollection decorations = new TextDecorationCollection();

		public sLinkLabel()
			: base()
		{    // Create a solid color brush pen for the text decoration.
			//underline.Pen = new Pen( Brushes.Red, 1 );
			underline.PenThicknessUnit = TextDecorationUnit.FontRecommended;

			// Set the underline decoration to a TextDecorationCollection and add it to the text block.
			decorations.Add( underline );
		}

		public override void EndInit()
		{
			base.EndInit();
		}


		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.TextDecorations = decorations;
			base.OnMouseEnter( e );
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.TextDecorations = null;
			base.OnMouseLeave( e );
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp( e );
		}

		private string _foo = string.Empty;
		public string Foo { get { return _foo; } set { _foo = value; } }
	}

	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	[ToolboxItem( true ), System.Drawing.ToolboxBitmap( typeof( GroupBox ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.None )]
	public class xxLinkLabel : Label
	{
		private const string _linkLabel = "LinkLabel";

		public static readonly DependencyProperty UrlProperty = DependencyProperty.Register( "Url", typeof( Uri ), typeof( xxLinkLabel ) );

		[Category( "Common Properties" ), Bindable( true )]
		public Uri Url
		{
			get { return GetValue( UrlProperty ) as Uri; }
			set { SetValue( UrlProperty, value ); }
		}

		public static readonly DependencyProperty HyperlinkStyleProperty = DependencyProperty.Register( "HyperlinkStyle", typeof( Style ),
				typeof( xxLinkLabel ) );

		public Style HyperlinkStyle
		{
			get { return GetValue( HyperlinkStyleProperty ) as Style; }
			set { SetValue( HyperlinkStyleProperty, value ); }
		}

		public static readonly DependencyProperty HoverForegroundProperty = DependencyProperty.Register( "HoverForeground", typeof( Brush ),
				typeof( xxLinkLabel ) );

		[Category( "Brushes" ), Bindable( true )]
		public System.Windows.Media.Brush HoverForeground
		{
			get { return GetValue( HoverForegroundProperty ) as Brush; }
			set { SetValue( HoverForegroundProperty, value ); }
		}

		//public static readonly DependencyProperty LinkLabelBehavourProperty = DependencyProperty.Register( "LinkLabelBehavour",
		//        typeof( LinkLabelBehaviour ),
		//        typeof( LinkLabel ) );

		//[Category( "Common Properties" ), Bindable( true )]
		//public LinkLabelBehaviour LinkLabelBehavour
		//{
		//    get { return (LinkLabelBehaviour)GetValue( LinkLabelBehavourProperty ); }
		//    set { SetValue( LinkLabelBehavourProperty, value ); }
		//}

		static xxLinkLabel()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
				typeof( xxLinkLabel ),
				new FrameworkPropertyMetadata( typeof( xxLinkLabel ) ) );
		}
	}
}
