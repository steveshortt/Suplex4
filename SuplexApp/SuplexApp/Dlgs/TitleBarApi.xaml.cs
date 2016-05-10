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
using WinInterop = System.Windows.Interop;
using System.Runtime.InteropServices;

namespace SuplexApp
{
	/// <summary>
	/// Interaction logic for TitleBar.xaml
	/// </summary>
	public partial class TitleBarApi : UserControl
	{
		private Window _w = null;

		public TitleBarApi()
		{
			InitializeComponent();
		}

		public override void EndInit()
		{
			base.EndInit();

			if( this.Parent != null )	// !DesignerProperties.GetIsInDesignMode( this ) &&
			{
				_w = this.FindWindow( (FrameworkElement)this.Parent );

				Binding title = new Binding( "Title" );
				title.Source = _w;
				lblTitle.SetBinding( Label.ContentProperty, title );

				_w.AllowsTransparency = true;
				_w.Background = new SolidColorBrush( Colors.Transparent );
				_w.WindowStyle = WindowStyle.None;
				_w.ResizeMode = ResizeMode.CanResizeWithGrip;
			}
		}


		#region public properties
		public CornerRadius CornerRadius
		{
			get { return (CornerRadius)GetValue( CornerRadiusProperty ); }
			set { SetValue( CornerRadiusProperty, value ); }
		}

		public static DependencyProperty CornerRadiusProperty =
			DependencyProperty.Register(
				"CornerRadius", typeof( CornerRadius ), typeof( TitleBarApi ) );
		#endregion


		#region Window Move/Min/Max handlers
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if( !DesignerProperties.GetIsInDesignMode( this ) )
			{
				System.IntPtr handle = ( new WinInterop.WindowInteropHelper( _w ) ).Handle;
				WinInterop.HwndSource.FromHwnd( handle ).AddHook( new WinInterop.HwndSourceHook( WindowProc ) );
			}
		}

		private Window FindWindow(FrameworkElement f)
		{
			Window w = null;
			if( f is Window )
			{
				w = (Window)f;
			}
			else
			{
				w = this.FindWindow( (FrameworkElement)f.Parent );
			}

			return w;
		}


		private void Go_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_w.DragMove();
		}

		private void cmdClose_Click(object sender, RoutedEventArgs e)
		{
			_w.Close();
			//_w = null;
		}

		private void cmdMaxRes_Click(object sender, RoutedEventArgs e)
		{
			if( cmdMaxRes.Content.ToString() == "1" )
			{
				_w.WindowState = WindowState.Maximized;
				cmdMaxRes.Content = "2";
			}
			else
			{
				_w.WindowState = WindowState.Normal;
				cmdMaxRes.Content = "1";
			}
			lblTitle.Focus();
		}

		private void cmdMin_Click(object sender, RoutedEventArgs e)
		{
			_w.WindowState = WindowState.Minimized;
			lblTitle.Focus();
		}



		void win_SourceInitialized(object sender, EventArgs e)
		{
		}


		//public override void OnApplyTemplate()
		//{
		//    System.IntPtr handle = (new WinInterop.WindowInteropHelper(this)).Handle;
		//    WinInterop.HwndSource.FromHwnd(handle).AddHook(new WinInterop.HwndSourceHook(WindowProc));
		//}

		private static System.IntPtr WindowProc(
			  System.IntPtr hwnd,
			  int msg,
			  System.IntPtr wParam,
			  System.IntPtr lParam,
			  ref bool handled)
		{
			switch( msg )
			{
				case 0x0024:
				WmGetMinMaxInfo( hwnd, lParam );
				handled = true;
				break;
			}

			return (System.IntPtr)0;
		}

		private static void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
		{

			MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure( lParam, typeof( MINMAXINFO ) );

			// Adjust the maximized size and position to fit the work area of the correct monitor
			int MONITOR_DEFAULTTONEAREST = 0x00000002;
			System.IntPtr monitor = MonitorFromWindow( hwnd, MONITOR_DEFAULTTONEAREST );

			if( monitor != System.IntPtr.Zero )
			{

				MONITORINFO monitorInfo = new MONITORINFO();
				GetMonitorInfo( monitor, monitorInfo );
				RECT rcWorkArea = monitorInfo.rcWork;
				RECT rcMonitorArea = monitorInfo.rcMonitor;
				mmi.ptMaxPosition.x = Math.Abs( rcWorkArea.left - rcMonitorArea.left );
				mmi.ptMaxPosition.y = Math.Abs( rcWorkArea.top - rcMonitorArea.top );
				mmi.ptMaxSize.x = Math.Abs( rcWorkArea.right - rcWorkArea.left );
				mmi.ptMaxSize.y = Math.Abs( rcWorkArea.bottom - rcWorkArea.top );
			}

			Marshal.StructureToPtr( mmi, lParam, true );
		}


		/// <summary>
		/// POINT aka POINTAPI
		/// </summary>
		[StructLayout( LayoutKind.Sequential )]
		public struct POINT
		{
			/// <summary>
			/// x coordinate of point.
			/// </summary>
			public int x;
			/// <summary>
			/// y coordinate of point.
			/// </summary>
			public int y;

			/// <summary>
			/// Construct a point of coordinates (x,y).
			/// </summary>
			public POINT(int x, int y)
			{
				this.x = x;
				this.y = y;
			}
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct MINMAXINFO
		{
			public POINT ptReserved;
			public POINT ptMaxSize;
			public POINT ptMaxPosition;
			public POINT ptMinTrackSize;
			public POINT ptMaxTrackSize;
		};


		/// <summary>
		/// </summary>
		[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
		public class MONITORINFO
		{
			/// <summary>
			/// </summary>            
			public int cbSize = Marshal.SizeOf( typeof( MONITORINFO ) );

			/// <summary>
			/// </summary>            
			public RECT rcMonitor = new RECT();

			/// <summary>
			/// </summary>            
			public RECT rcWork = new RECT();

			/// <summary>
			/// </summary>            
			public int dwFlags = 0;
		}


		/// <summary> Win32 </summary>
		[StructLayout( LayoutKind.Sequential, Pack = 0 )]
		public struct RECT
		{
			/// <summary> Win32 </summary>
			public int left;
			/// <summary> Win32 </summary>
			public int top;
			/// <summary> Win32 </summary>
			public int right;
			/// <summary> Win32 </summary>
			public int bottom;

			/// <summary> Win32 </summary>
			public static readonly RECT Empty = new RECT();

			/// <summary> Win32 </summary>
			public int Width
			{
				get { return Math.Abs( right - left ); }  // Abs needed for BIDI OS
			}
			/// <summary> Win32 </summary>
			public int Height
			{
				get { return bottom - top; }
			}

			/// <summary> Win32 </summary>
			public RECT(int left, int top, int right, int bottom)
			{
				this.left = left;
				this.top = top;
				this.right = right;
				this.bottom = bottom;
			}


			/// <summary> Win32 </summary>
			public RECT(RECT rcSrc)
			{
				this.left = rcSrc.left;
				this.top = rcSrc.top;
				this.right = rcSrc.right;
				this.bottom = rcSrc.bottom;
			}

			/// <summary> Win32 </summary>
			public bool IsEmpty
			{
				get
				{
					// BUGBUG : On Bidi OS (hebrew arabic) left > right
					return left >= right || top >= bottom;
				}
			}
			/// <summary> Return a user friendly representation of this struct </summary>
			public override string ToString()
			{
				if( this == RECT.Empty ) { return "RECT {Empty}"; }
				return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
			}

			/// <summary> Determine if 2 RECT are equal (deep compare) </summary>
			public override bool Equals(object obj)
			{
				if( !( obj is Rect ) ) { return false; }
				return ( this == (RECT)obj );
			}

			/// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
			public override int GetHashCode()
			{
				return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
			}


			/// <summary> Determine if 2 RECT are equal (deep compare)</summary>
			public static bool operator ==(RECT rect1, RECT rect2)
			{
				return ( rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom );
			}

			/// <summary> Determine if 2 RECT are different(deep compare)</summary>
			public static bool operator !=(RECT rect1, RECT rect2)
			{
				return !( rect1 == rect2 );
			}


		}

		[DllImport( "user32" )]
		internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

		/// <summary>
		/// 
		/// </summary>
		[DllImport( "User32" )]
		internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
		#endregion
	}
}