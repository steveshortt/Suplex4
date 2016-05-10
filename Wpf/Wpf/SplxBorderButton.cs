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
	public class SplxBorderButton : Button
	{
		public SplxBorderButton()
			: base()
		{
		}


		public CornerRadius CornerRadius
		{
			get { return (CornerRadius)GetValue( CornerRadiusProperty ); }
			set { SetValue( CornerRadiusProperty, value ); }
		}

		/// <summary>
		/// Identifies the Value dependency property.
		/// </summary>
		public static readonly DependencyProperty CornerRadiusProperty =
			DependencyProperty.Register(
				"CornerRadius", typeof( CornerRadius ), typeof( SplxBorderButton ) );
	}
}
