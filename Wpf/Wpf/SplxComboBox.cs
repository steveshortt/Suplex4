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
	/// <summary>
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.SelectedIndexChanged | ControlEvents.TextChanged )]
	public class SplxComboBox : ComboBox, IValidationControl
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationAccessor _va = null;


		private int _lastSelectedIndex = -1;
		private string _lastText = "";

		private bool _restrictListItems = true;


		public SplxComboBox()
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
				vr = _va.ProcessEvent( this.GetCompareValue(), ControlEvents.Validating, processFillMaps );
			}
			return vr;
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

		protected override void OnSelectionChanged(SelectionChangedEventArgs e)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "SelectedIndexChanged.", false );

			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				if( this.SelectedIndex != _lastSelectedIndex )
				{
					//this.SetSelectionLengthToZero();
					_lastSelectedIndex = this.SelectedIndex;

					_va.ProcessEvent( this.GetCompareValue(), ControlEvents.SelectedIndexChanged, true );
				}

				base.OnSelectionChanged( e );
			}
			else
			{
				//this.SetSelectionLengthToZero();
				this.SelectedIndex = _lastSelectedIndex;
				e.Handled = true;
			}
		}

		private string GetCompareValue()
		{
			return this.SelectedValue != null ? this.SelectedValue.ToString() : this.Text;
		}

		//protected override void OnValidating(CancelEventArgs e)
		//{
		//    e.Cancel = this.ProcessValidate( true ).Error;

		//    base.OnValidating( e );
		//}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			_va.ProcessEvent( this.Text, ControlEvents.Enter, true );
			base.OnGotFocus( e );
		}

		/// <summary>
		/// Used to format text on field exit.
		/// </summary>
		protected override void OnLostFocus(RoutedEventArgs e)
		{
			//this looks dumb, but it actually causes the text to get Formatted:
			//see overridden Text property
			this.Text = this.Text;

			//OnValidating fires right after OnLeave, rules for data validation
			//should go on OnValidating, not on OnLeave.
			_va.ProcessEvent( this.Text, ControlEvents.Leave, true );

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
				base.IsReadOnly = !_sr[AceType.UI, UIRight.Operate].AccessAllowed;
				base.IsEnabled = _sr[AceType.UI, UIRight.Enabled].AccessAllowed;
				base.Visibility = _sr[AceType.UI, UIRight.Visible].AccessAllowed.ToVisibility( this.VisibilityDenied );
			}
		}

		public string GetSecurityState()
		{
			return string.Format( "Visibility: {0}, IsEnabled: {1}, IsReadOnly: {2}", this.Visibility, this.IsEnabled, this.IsReadOnly );
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