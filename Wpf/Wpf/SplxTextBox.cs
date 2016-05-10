using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using System.Reflection;
using System.ComponentModel;

using Suplex.Forms;
using Suplex.Security;
using Suplex.Data;


namespace Suplex.Wpf
{
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.TextChanged )]
	public class SplxTextBox : TextBox, IValidationTextBox
	{
		//static sTextBox()
		//{
		//    DefaultStyleKeyProperty.OverrideMetadata( typeof( sTextBox ), new FrameworkPropertyMetadata( typeof( sTextBox ) ) );
		//}


		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationAccessor _va = null;


		private bool _selectNextOnML = false;
		private string _formatString = null;

		//this control is used to compare for the PasswordChar on this control.
		//using it to protect against default value of PasswordChar ever changing.
		private TextBox _passwordDefault = new TextBox();


		public SplxTextBox()
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

		[DefaultValue( false ), Category( "Suplex" )]
		public bool MaxLengthAdvancesSelection
		{
			get { return _selectNextOnML; }
			set { _selectNextOnML = value; }
		}

		/// <summary>
		/// Used for format specifier in ToString() calls on non-String DataTypes.
		/// </summary>
		[Category( "Suplex" )]
		public string FormatString
		{
			get { return _formatString; }
			set { _formatString = value; }
		}

		/// <summary>
		/// Overrides TextBox.Text on set: Formats text according to FormatString.
		/// If FormatString not specified, base.Text = value.
		/// </summary>
		new public string Text
		{
			get { return base.Text; }
			set
			{
				base.Text = FormatText( value );
			}
		}

		/// <summary>
		/// Formats text according to FormatString.
		/// If text cannot be formatted, the param value is returned (unchanged).
		/// </summary>
		/// <param name="text">The text to format.</param>
		/// <returns>Formatted text string.</returns>
		private string FormatText(string text)
		{
			if( this.Validation.DataType != TypeCode.String && !string.IsNullOrEmpty( this.FormatString ) )
			{
				try
				{
					Type dataType = Type.GetType( "System." + this.Validation.DataType.ToString() );
					MethodInfo parse = dataType.GetMethod( "Parse", new Type[] { typeof( string ) } );

					object var = Activator.CreateInstance( dataType );
					var = parse.Invoke( dataType, new object[] { text } );

					MethodInfo tostring = var.GetType().GetMethod( "ToString", new Type[] { typeof( string ) } );
					return tostring.Invoke( var, new object[] { _formatString } ).ToString();
				}
				catch
				{
					return text;
				}
			}
			else
			{
				return text;
			}
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
				vr = _va.ProcessEvent( this.Text, ControlEvents.Validating, processFillMaps );
			}
			return vr;
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			bool processEvent = true;

			switch( e.Property.Name.ToLower() )
			{
				case "text":
				{
					this.HandleOnTextChanged();
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

		/// <summary>
		/// Resets flags indicating control should be validated and FillMaps should processed.
		/// </summary>
		private void HandleOnTextChanged()
		{
			////_Validated = false;
			////_ProcessFillMaps = true;

			//testing...
			//SecureControlUtils.AuditAction( this, _sr, UIRight.Enabled.ToString(), _auditEventHandler, AuditTypes.ControlDetail, this.UniqueName + " TextChanged" );


			string newText = string.Empty;
			//check to make sure we 1) have text, and 2) this is not a passowrd textbox
			//////if( this.Text.Length > 0 && this.PasswordChar.Equals( _passwordDefault.PasswordChar ) )
			//////{
			//////    newText = this.Text.Length > 25 ? this.Text.Substring( 0, 25 ) : this.Text;
			//////}

			_sa.AuditAction( AuditType.ControlDetail, null,
				String.Format( "TextChanged. New text: [{0}]", newText ), false );


			_va.ProcessEvent( this.Text, ControlEvents.TextChanged, true );


			if( _selectNextOnML && ( this.Text.Length == this.MaxLength ) )
			{
				this.MoveFocus( new TraversalRequest( FocusNavigationDirection.Next ) );
			}
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
			get { return base.Visibility; }
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
			get { return base.IsEnabled; }
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
