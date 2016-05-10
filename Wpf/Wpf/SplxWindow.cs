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
using System.Drawing;

using Suplex.Forms;
using sf = Suplex.Forms;
using Suplex.Security;
using Suplex.Data;


namespace Suplex.Wpf
{

	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	[ToolboxItem( true ), ToolboxBitmap( typeof( Window ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.None )]
	public class SplxWindow : Window, IValidationContainer
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationContainerAccessor _va = null;

		#region Diag Vars
		//private sDiagInfoCtrl _diagInfoCtrl;
		//private int _diagClickCount = 0;
		//private Timer _diagTimer;
		//private IContainer _diagComponents;
		#endregion


		public SplxWindow()
			: base()
		{
			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationContainerAccessor( this, TypeCode.Empty );

			this.VisibilityDenied = Visibility.Hidden;

			//this.Diag_Setup();
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
			this.Content = control;
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
			return LogicalTreeHelper.GetChildren( this );

			//return (ICollection)( new List<FrameworkElement>( (IEnumerable)LogicalTreeHelper.GetChildren( this ) ) );
			//12/21/2008
			////List<IInputElement> children = new List<IInputElement>();
			////IEnumerator ch = LogicalTreeHelper.GetChildren( this ).GetEnumerator();
			////while( ch.MoveNext() )
			////{
			////    if( ( ch.Current as IInputElement ) != null )
			////    {
			////        children.Add( (IInputElement)ch.Current );
			////    }
			////}
			////return (ICollection)children;
		}

		//protected override void OnClick(EventArgs e)
		//{
		//    if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
		//    {
		//        base.OnClick( e );
		//    }
		//}

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

		/*
		#region Diag Handlers
		private void Diag_Setup()
		{
			this._diagComponents = new System.ComponentModel.Container();

			this._diagTimer = new System.Windows.Forms.Timer( this._diagComponents );
			this._diagTimer.Enabled = false;
			this._diagTimer.Interval = 2500;
			this._diagTimer.Tick += new System.EventHandler( this.Diag_Timer_Tick );

			this._diagInfoCtrl = new sDiagInfoCtrl();
			this._diagInfoCtrl.Name = "_diagInfoCtrl";
			this._diagInfoCtrl.Location = new Point( 0, 0 );
			this._diagInfoCtrl.KeyPress += new System.Windows.Forms.KeyPressEventHandler( this.Diag_KeyPress );

			try
			{
				this.Controls.Add( _diagInfoCtrl );
			}
			catch( Exception ex )
			{
				System.Diagnostics.Debug.WriteLine( string.Format( "{0}-{1}", ex.Message, this.GetType() ) );
			}

			this.DoubleClick += new EventHandler( Diag_Form_DoubleClick );
		}

		private void Diag_Form_DoubleClick(object sender, System.EventArgs e)
		{
			_diagClickCount++;
			if( _diagClickCount > 1 )
			{
				_diagInfoCtrl.Focus();
				_diagTimer.Enabled = true;
			}
		}

		private void Diag_Timer_Tick(object sender, System.EventArgs e)
		{
			_diagClickCount = 0;
			_diagTimer.Enabled = false;
		}

		private void Diag_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if( _diagClickCount > 1 )
			{
				if( e.KeyChar == '`' )
				{
					e.Handled = true;
					DumpDiagInfo();
				}
			}
		}

		private void DumpDiagInfo()
		{
			if( this.TopLevelControl is sForm )
			{
				( (sForm)this.TopLevelControl ).DumpDiagInfo();
			}
			else if( this.TopLevelControl is sUserControl )
			{
				( (sUserControl)this.TopLevelControl ).DumpDiagInfo();
			}
		}
		#endregion
		*/
	}
}
