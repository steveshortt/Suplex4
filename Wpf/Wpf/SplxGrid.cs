using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;
using sf = Suplex.Forms;


namespace Suplex.Wpf
{
	[ToolboxItem( true ), ToolboxBitmap( typeof( GroupBox ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.None )]
	public class SplxGrid : Grid, IValidationContainer, ILogicalChildrenHost
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationContainerAccessor _va = null;

		public SplxGrid()
		{
			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationContainerAccessor( this, TypeCode.Empty );

			this.VisibilityDenied = Visibility.Collapsed;

			//this.Diag_Setup();
			//DefaultStyleKeyProperty.OverrideMetadata( typeof( SplxGrid ), new FrameworkPropertyMetadata( typeof( SplxGrid ) ) );
		}

		public override void BeginInit()
		{
			_sa.EnsureDefaultState();
			base.BeginInit();
		}


		[ParenthesizePropertyName( true ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
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

		[Browsable( false ), Category( "Suplex" ),
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

		public virtual sf.ValidationResult ProcessValidate(bool processFillMaps)
		{
			sf.ValidationResult vr = new sf.ValidationResult( this.UniqueName );
			if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				vr = _va.ProcessEvent( null, ControlEvents.Validating, processFillMaps );
			}
			return vr;
		}

		public void AddChild(IValidationControl control)
		{
			this.Children.Add( control as System.Windows.UIElement );
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			//default is to process all events unless specifically denied
			bool processEvent = true;

			switch( e.Property.Name.ToLower() )
			{
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

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			_va.ProcessEvent( null, ControlEvents.Enter, true );
			base.OnGotFocus( e );
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			_va.ProcessEvent( null, ControlEvents.Leave, true );
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

		public virtual IEnumerable GetChildren()
		{
			return new WpfLogicalChildrenEnumeratorWrapper( this );
		}

		public IEnumerator LogicalChildrenEnumerator { get { return this.LogicalChildren; } }

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