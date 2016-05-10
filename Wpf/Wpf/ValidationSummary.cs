using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using Suplex.Forms;


/// this is a default implementation, just get base functionality in place.
/// a better version of this control should expose a template-property to apply on
///		selection changed, which is stubbed:
///		TODO: Need array of style per error control type
///		[OR]  (better): Prop on the Suplex control itself to indicate it's error style
namespace Suplex.Wpf
{
	public class ValidationSummary : Control, IValidationSummaryControl
	{
		ListBox errorListBox = null;
		List<ValidationError> _errors = new List<ValidationError>();


		public ValidationSummary()
		{
			//DefaultStyleKeyProperty.OverrideMetadata( typeof( ValidationSummary ), new FrameworkPropertyMetadata( typeof( ValidationSummary ) ) );
			this.Visible = false;
			this.DataContext = _errors;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			errorListBox = base.GetTemplateChild( "PART_ListBox" ) as ListBox;
			if( errorListBox != null )
			{
				errorListBox.SelectionChanged += new SelectionChangedEventHandler( errorListBox_SelectionChanged );
				errorListBox.MouseDoubleClick += new MouseButtonEventHandler( errorListBox_MouseDoubleClick );
			}
		}

		#region public props
		public static readonly DependencyProperty ErrorBackgroundProperty =
			DependencyProperty.Register( "ErrorBackground", typeof( Brush ), typeof( ValidationSummary ),
			new FrameworkPropertyMetadata( new SolidColorBrush( Color.FromArgb( 153, 255, 0, 0 ) ) ) );
		public Brush ErrorBackground
		{
			get { return (Brush)this.GetValue( ErrorBackgroundProperty ); }
			set { this.SetValue( ErrorBackgroundProperty, value ); }
		}

		public static readonly DependencyProperty ErrorStyleProperty =
			DependencyProperty.Register( "ErrorStyle", typeof( Style ), typeof( ValidationSummary ),
			new FrameworkPropertyMetadata( null ) );
		public Style ErrorStyle
		{
			get { return (Style)this.GetValue( ErrorStyleProperty ); }
			set { this.SetValue( ErrorStyleProperty, value ); }
		}

		public static readonly DependencyProperty AutoFlashErrorProperty =
			DependencyProperty.Register( "AutoFlashError", typeof( bool ), typeof( ValidationSummary ),
			new FrameworkPropertyMetadata( true ) );
		public bool AutoFlashError
		{
			get { return (bool)this.GetValue( AutoFlashErrorProperty ); }
			set { this.SetValue( AutoFlashErrorProperty, value ); }
		}

		//public static readonly DependencyProperty ErrorTemplateProperty =
		//    DependencyProperty.Register( "ErrorTemplate", typeof( ControlTemplate ), typeof( ValidationSummary ),
		//    new FrameworkPropertyMetadata( null ) );
		//public ControlTemplate ErrorTemplate
		//{
		//    get { return (ControlTemplate)this.GetValue( ErrorTemplateProperty ); }
		//    set { this.SetValue( ErrorTemplateProperty, value ); }
		//}
		#endregion

		#region IValidationSummaryControl Members
		public void Reset()
		{
			foreach( ValidationError ve in _errors )
			{
				if( this.ErrorStyle == null )
				{
					((Control)ve.Control).Background = ((ValidationError)ve).Brush;
				}
				else
				{
					((Control)ve.Control).Style = ((ValidationError)ve).Style;
				}
			}
			_errors.Clear();
			this.Visible = false;
		}

		public void SetError(IValidationControl control, string errorMessage)
		{
			if( !string.IsNullOrEmpty( errorMessage ) )
			{
				ValidationError ve = new ValidationError()
				{
					Message = errorMessage,
					Control = control
				};
				_errors.Add( ve );
			}
		}

		public bool Visible
		{
			get { return this.IsVisible; }
			set { this.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
		}
		#endregion

		void errorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Action d = delegate() { };	//create an empty delegate
			DispatcherPriority r = DispatcherPriority.Render;

			if( errorListBox.SelectedItem != null )
			{
				ValidationError err = (ValidationError)errorListBox.SelectedItem;
				Control c = (Control)err.Control;

				if( this.ErrorStyle == null )
				{
					c.Background = this.ErrorBackground;
					c.Dispatcher.Invoke( d, r, null );

					if( this.AutoFlashError )
					{
						System.Threading.Thread.Sleep( 125 );

						c.Background = err.Brush;
						c.Dispatcher.Invoke( d, r, null );
						System.Threading.Thread.Sleep( 75 );

						c.Background = this.ErrorBackground;
						c.Dispatcher.Invoke( d, r, null );
						System.Threading.Thread.Sleep( 125 );

						c.Background = err.Brush;
						c.Dispatcher.Invoke( d, r, null );
					}
				}
				else
				{
					c.Style = this.ErrorStyle;
					c.Dispatcher.Invoke( d, r, null );

					if( this.AutoFlashError )
					{
						System.Threading.Thread.Sleep( 125 );

						c.Style = err.Style;
						c.Dispatcher.Invoke( d, r, null );
						System.Threading.Thread.Sleep( 75 );

						c.Style = this.ErrorStyle;
						c.Dispatcher.Invoke( d, r, null );
						System.Threading.Thread.Sleep( 125 );

						c.Style = err.Style;
						c.Dispatcher.Invoke( d, r, null );
					}
				}
			}
		}

		void errorListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if( errorListBox.SelectedItem != null )
			{
				if( this.ErrorStyle == null )
				{
					((Control)((ValidationError)errorListBox.SelectedItem).Control).Background = this.ErrorBackground;
				}
				else
				{
					((Control)((ValidationError)errorListBox.SelectedItem).Control).Style = this.ErrorStyle;
				}

				((Control)((ValidationError)errorListBox.SelectedItem).Control).Focus();
			}
			base.OnMouseDoubleClick( e );
		}
	}

	public class ValidationError
	{
		private IValidationControl _c;
		private Brush _b;
		//private ControlTemplate _t;
		private Style _s;

		public string Message { get; set; }
		public IValidationControl Control
		{
			get { return _c; }
			set
			{
				_c = value;
				_b = ((Control)value).Background.CloneCurrentValue();
				//_t = ( (Control)value ).Template;
				_s = ((Control)value).Style;
			}
		}
		public Brush Brush { get { return _b; } }
		//public ControlTemplate Template { get { return _t; } }
		public Style Style { get { return _s; } }

		public override string ToString()
		{
			return this.Message;
		}
	}
}