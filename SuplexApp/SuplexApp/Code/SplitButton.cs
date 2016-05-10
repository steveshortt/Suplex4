//  --------------------------------
//  Copyright (c) Huy Pham. All rights reserved.
//  This source code is made available under the terms of the Microsoft Public License (Ms-PL)
//  http://www.opensource.org/licenses/ms-pl.html
//  ---------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;

using sf = Suplex.Forms;
using api = Suplex.Forms.ObjectModel.Api;

namespace SuplexApp.Controls
{
	[TemplatePart( Name = "PART_Button", Type = typeof( ButtonBase ) )]
	public class SplitButton : ToggleButton, api.ISuplexObject
	{
		#region Dependency Properties

		public static readonly DependencyProperty DropDownContextMenuProperty = DependencyProperty.Register( "DropDownContextMenu", typeof( ContextMenu ), typeof( SplitButton ), new UIPropertyMetadata( null ) );
		public static readonly DependencyProperty IsMainButtonEnabledProperty = DependencyProperty.Register( "IsMainButtonEnabled", typeof( bool? ), typeof( SplitButton ) );
		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register( "Image", typeof( ImageSource ), typeof( SplitButton ) );
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof( string ), typeof( SplitButton ) );
		public static readonly DependencyProperty TargetProperty = DependencyProperty.Register( "Target", typeof( UIElement ), typeof( SplitButton ) );
		public static readonly DependencyProperty MainButtonCommandProperty = DependencyProperty.Register( "MainButtonCommand", typeof( ICommand ), typeof( SplitButton ), new FrameworkPropertyMetadata( null ) );
		public static readonly DependencyProperty DropDownButtonCommandProperty = DependencyProperty.Register( "DropDownButtonCommand", typeof( ICommand ), typeof( SplitButton ), new FrameworkPropertyMetadata( null ) );

		#endregion

		#region Constructors

		public SplitButton()
		{
			// Bind the ToogleButton.IsChecked property to the drop-down's IsOpen property 
			var binding = new Binding( "DropDownContextMenu.IsOpen" ) { Source = this };
			SetBinding( IsCheckedProperty, binding );
		}

		#endregion

		#region Properties

		public ContextMenu DropDownContextMenu
		{
			get { return GetValue( DropDownContextMenuProperty ) as ContextMenu; }
			set { SetValue( DropDownContextMenuProperty, value ); }
		}

		public bool? IsMainButtonEnabled
		{
			get { return GetValue( IsMainButtonEnabledProperty ) as bool?; }
			set { SetValue( IsMainButtonEnabledProperty, value ); }
		}

		public ImageSource Image
		{
			get { return GetValue( ImageProperty ) as ImageSource; }
			set { SetValue( ImageProperty, value ); }
		}

		public string Text
		{
			get { return GetValue( TextProperty ) as string; }
			set { SetValue( TextProperty, value ); }
		}

		public UIElement Target
		{
			get { return GetValue( TargetProperty ) as UIElement; }
			set { SetValue( TargetProperty, value ); }
		}

		public ICommand MainButtonCommand
		{
			get { return GetValue( MainButtonCommandProperty ) as ICommand; }
			set { SetValue( MainButtonCommandProperty, value ); }
		}

		public ICommand DropDownButtonCommand
		{
			get { return GetValue( DropDownButtonCommandProperty ) as ICommand; }
			set { SetValue( DropDownButtonCommandProperty, value ); }
		}

		#endregion

		#region Public Override Methods

		[DebuggerStepThrough()]
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			SetMainButtonCommand();
		}

		#endregion

		#region Protected Override Methods

		[DebuggerStepThrough()]
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged( e );

			if( e.Property == MainButtonCommandProperty )
				SetMainButtonCommand();

			if( e.Property == DropDownButtonCommandProperty )
				Command = DropDownButtonCommand;
		}

		protected override void OnClick()
		{
			if( DropDownContextMenu == null ) return;

			if( DropDownButtonCommand != null )
				DropDownButtonCommand.Execute( null );

			// If there is a drop-down assigned to this button, then position and display it 
			DropDownContextMenu.PlacementTarget = this;
			DropDownContextMenu.Placement = PlacementMode.Bottom;
			DropDownContextMenu.IsOpen = !DropDownContextMenu.IsOpen;
		}

		#endregion

		#region Private Methods

		[DebuggerStepThrough()]
		private void SetMainButtonCommand()
		{
			// Set up the event handlers
			if( Template != null )
			{
				var button = Template.FindName( "PART_Button", this ) as ButtonBase;
				if( button != null ) button.Command = MainButtonCommand;
			}
		}

		#endregion

		#region ISuplexObject Members

		string api.ISuplexObject.ObjectId
		{
			get { return null; }
		}

		public sf.ObjectType ObjectType
		{
			get;
			set;
		}

		#endregion
	}

	[TemplatePart( Name = "PART_Button", Type = typeof( ButtonBase ) )]
	public class SplitRadioButton : RadioButton
	{
		#region Dependency Properties

		public static readonly DependencyProperty DropDownContextMenuProperty = DependencyProperty.Register( "DropDownContextMenu", typeof( ContextMenu ), typeof( SplitRadioButton ), new UIPropertyMetadata( null ) );
		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register( "Image", typeof( ImageSource ), typeof( SplitRadioButton ) );
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof( string ), typeof( SplitRadioButton ) );
		public static readonly DependencyProperty TargetProperty = DependencyProperty.Register( "Target", typeof( UIElement ), typeof( SplitRadioButton ) );
		public static readonly DependencyProperty MainButtonCommandProperty = DependencyProperty.Register( "MainButtonCommand", typeof( ICommand ), typeof( SplitRadioButton ), new FrameworkPropertyMetadata( null ) );
		public static readonly DependencyProperty DropDownButtonCommandProperty = DependencyProperty.Register( "DropDownButtonCommand", typeof( ICommand ), typeof( SplitRadioButton ), new FrameworkPropertyMetadata( null ) );

		#endregion

		#region Constructors

		public SplitRadioButton()
		{
			// Bind the ToogleButton.IsChecked property to the drop-down's IsOpen property 
			//var binding = new Binding( "DropDownContextMenu.IsOpen" ) { Source = this };
			//SetBinding( IsCheckedProperty, binding );
		}

		#endregion

		#region Properties

		public ContextMenu DropDownContextMenu
		{
			get { return GetValue( DropDownContextMenuProperty ) as ContextMenu; }
			set { SetValue( DropDownContextMenuProperty, value ); }
		}

		public ImageSource Image
		{
			get { return GetValue( ImageProperty ) as ImageSource; }
			set { SetValue( ImageProperty, value ); }
		}

		public string Text
		{
			get { return GetValue( TextProperty ) as string; }
			set { SetValue( TextProperty, value ); }
		}

		public UIElement Target
		{
			get { return GetValue( TargetProperty ) as UIElement; }
			set { SetValue( TargetProperty, value ); }
		}

		public ICommand MainButtonCommand
		{
			get { return GetValue( MainButtonCommandProperty ) as ICommand; }
			set { SetValue( MainButtonCommandProperty, value ); }
		}

		public ICommand DropDownButtonCommand
		{
			get { return GetValue( DropDownButtonCommandProperty ) as ICommand; }
			set { SetValue( DropDownButtonCommandProperty, value ); }
		}

		#endregion

		#region Public Override Methods

		/// <summary>
		/// 
		/// </summary>
		[DebuggerStepThrough()]
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			SetMainButtonCommand();
		}

		#endregion

		#region Protected Override Methods

		[DebuggerStepThrough()]
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged( e );

			if( e.Property == MainButtonCommandProperty )
				SetMainButtonCommand();

			if( e.Property == DropDownButtonCommandProperty )
				Command = DropDownButtonCommand;
		}

		protected override void OnClick()
		{
			if( DropDownContextMenu == null ) return;

			if( DropDownButtonCommand != null )
				DropDownButtonCommand.Execute( null );

			// If there is a drop-down assigned to this button, then position and display it 
			DropDownContextMenu.PlacementTarget = this;
			DropDownContextMenu.Placement = PlacementMode.Bottom;
			DropDownContextMenu.IsOpen = !DropDownContextMenu.IsOpen;
		}

		#endregion

		#region Private Methods

		[DebuggerStepThrough()]
		private void SetMainButtonCommand()
		{
			// Set up the event handlers
			if( Template != null )
			{
				var button = Template.FindName( "PART_Button", this ) as ButtonBase;
				if( button != null ) button.Command = MainButtonCommand;
				button.Click += new RoutedEventHandler( button_Click );
			}
		}

		void button_Click(object sender, RoutedEventArgs e)
		{
			this.IsChecked = true;
		}

		#endregion
	}
}