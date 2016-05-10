using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using System.ComponentModel;

using Suplex.Forms;
using Suplex.Security;
using Suplex.Data;


namespace Suplex.Wpf
{
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.SelectedIndexChanged | ControlEvents.TextChanged )]
	public class SplxRadioButton : RadioButton, IValidationControl
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationAccessor _va = null;


		public SplxRadioButton()
			: base()
		{
			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationAccessor( this, TypeCode.String );

			this.VisibilityDenied = Visibility.Collapsed;
		}

		public override void BeginInit()
		{
			_sa.EnsureDefaultState();
			base.BeginInit();
		}



		[ParenthesizePropertyName( true ), Category( "Suplex" )]
		public string UniqueName
		{
			get
			{
				return string.IsNullOrEmpty( _uniqueName ) ? base.Name : _uniqueName;
			}
			set
			{
				_uniqueName = value;
			}
		}

		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Suplex.Data.DataAccessLayer DataAccessLayer
		{
			get { return _dal; }
			set { _dal = value; }
		}

		public Visibility VisibilityDenied { get; set; }

		#region Validation Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Validation management properties and tools." )]
		public IValidationAccessor Validation
		{
			get { return _va; }
		}

		public virtual Suplex.Forms.ValidationResult ProcessValidate(bool processFillMaps)
		{
			Suplex.Forms.ValidationResult vr = new Suplex.Forms.ValidationResult( this.UniqueName );
			if( this.IsEnabled )
			{
				vr = _va.ProcessEvent( string.Empty, ControlEvents.Validating, processFillMaps );
			}
			return vr;
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			bool processEvent = true;

			switch( e.Property.Name.ToLower() )
			{
				case "ischecked":
				{
					if( _va.EventBindings.ValidationEvents.Contains( ControlEvents.CheckChanged ) )
					{
						processEvent = _sr[AceType.UI, UIRight.Operate].AccessAllowed;
						if( processEvent )
						{
							_va.ProcessEvent( string.Empty, ControlEvents.CheckChanged, true );
						}
					}
					break;
				}
				case "isvisible":
				{
					if( _va.EventBindings.ValidationEvents.Contains( ControlEvents.VisibleChanged ) )
					{
						processEvent = _sr[AceType.UI, UIRight.Visible].AccessAllowed;
						if( processEvent )
						{
							_va.ProcessEvent( null, ControlEvents.VisibleChanged, true );
						}
					}
					break;
				}
				case "isenabled":
				{
					if( _va.EventBindings.ValidationEvents.Contains( ControlEvents.EnabledChanged ) )
					{
						processEvent = _sr[AceType.UI, UIRight.Enabled].AccessAllowed;
						if( processEvent )
						{
							_va.ProcessEvent( null, ControlEvents.EnabledChanged, true );
						}
					}
					break;
				}
			}

			if( processEvent )
			{
				base.OnPropertyChanged( e );
			}
		}

		public static readonly DependencyProperty IsCheckedFixedProperty =
			DependencyProperty.Register( "IsCheckedFixed", typeof( bool? ), typeof( SplxRadioButton ),
			new FrameworkPropertyMetadata( false,
				FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsCheckedFixedChanged ) );

		private static bool _isCheckedChanging = false;
		public static void IsCheckedFixedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			_isCheckedChanging = true;
			((SplxRadioButton)d).IsChecked = (bool)e.NewValue;
			_isCheckedChanging = false;
		}

		protected override void OnChecked(RoutedEventArgs e)
		{
			if( this.HandleCheckedUnchecked( e, true, "Checked" ) )
			{
				if( !_isCheckedChanging )
				{
					this.IsCheckedFixed = true;
				}
				base.OnChecked( e );
			}
		}
		protected override void OnUnchecked(RoutedEventArgs e)
		{
			if( this.HandleCheckedUnchecked( e, false, "Unchecked" ) )
			{
				if( !_isCheckedChanging )
				{
					this.IsCheckedFixed = false;
				}
				base.OnUnchecked( e );
			}
		}

		public bool IsCheckedFixed
		{
			get { return (bool)GetValue( IsCheckedFixedProperty ); }
			set
			{
				if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
				{
					SetValue( IsCheckedFixedProperty, value );
				}
			}
		}

		protected bool HandleCheckedUnchecked(RoutedEventArgs e, bool isChecked, string checkedEvent)
		{
			bool success = false;

			_sa.AuditAction( AuditType.ControlDetail, null, checkedEvent, false );

			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				_va.ProcessEvent( string.Empty, ControlEvents.CheckChanged, true );

				success = true;
			}

			return success;
		}

		//protected override void OnValidating(CancelEventArgs e)
		//{
		//    e.Cancel = this.ProcessValidate( true ).Error;

		//    base.OnValidating( e );
		//}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			_va.ProcessEvent( string.Empty, ControlEvents.Enter, true );
			base.OnGotFocus( e );
		}

		/// <summary>
		/// Used to format text on field exit.
		/// </summary>
		protected override void OnLostFocus(RoutedEventArgs e)
		{
			//OnValidating fires right after OnLeave, rules for data validation
			//should go on OnValidating, not on OnLeave.
			_va.ProcessEvent( string.Empty, ControlEvents.Leave, true );

			base.OnLostFocus( e );
		}
		#endregion


		#region Security Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Security management properties and tools." ), Category( "Suplex" )]
		public SecurityAccessor Security
		{
			get { return _sa; }
		}

		public void ApplySecurity()
		{
			if( !DesignerProperties.GetIsInDesignMode( this ) )
			{
				base.IsEnabled = _sr[AceType.UI, UIRight.Enabled].AccessAllowed;
				base.Visibility = _sr[AceType.UI, UIRight.Visible].AccessAllowed.ToVisibility( this.VisibilityDenied );
			}
		}

		public string GetSecurityState()
		{
			return string.Format( "Visibility: {0}, IsEnabled: {1}", this.Visibility, this.IsEnabled );
		}

		[DefaultValue( Visibility.Visible )]
		new public Visibility Visibility
		{
			get
			{
				return base.Visibility;
			}
			set
			{
				if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					base.Visibility = value;
				}
			}
		}

		[DefaultValue( true )]
		new public bool IsEnabled
		{
			get
			{
				return base.IsEnabled;
			}
			set
			{
				if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					base.IsEnabled = value;
				}
			}
		}
		#endregion
	}
}