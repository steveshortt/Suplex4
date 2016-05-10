using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;
using Suplex.Security.Standard;

namespace Suplex.WinForms
{
	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	[ToolboxItem( true ), ToolboxBitmap( typeof( RichTextBox ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.TextChanged )]
	public class sRichTextBox : System.Windows.Forms.RichTextBox, IValidationControl, IValidationTextBox
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationAccessor _va = null;


		private bool _selectNextOnML = false;
		private string _formatString = null;


		public sRichTextBox() : base()
		{
			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationAccessor( this, TypeCode.String );
		}

		protected override void InitLayout()
		{
			_sa.EnsureDefaultState();

			base.InitLayout();
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

		[DefaultValue( false )]
		public bool MaxLengthAdvancesSelection
		{
			get { return _selectNextOnML; }
			set { _selectNextOnML = value; }
		}

		/// <summary>
		/// Used for format specifier in ToString() calls on non-String DataTypes.
		/// </summary>
		public string FormatString
		{
			get { return _formatString; }
			set { _formatString = value; }
		}

		/// <summary>
		/// Overrides TextBox.Text on set: Formats text according to FormatString.
		/// If FormatString not specified, base.Text = value.
		/// </summary>
		public override string Text
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



		#region Validation Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Validation management properties and tools." )]
		public IValidationAccessor Validation
		{
			get { return _va; }
		}

		public virtual ValidationResult ProcessValidate(bool processFillMaps)
		{
			ValidationResult vr = new ValidationResult( this.UniqueName );
			if( this.Enabled )
			{
				vr = _va.ProcessEvent( this.Text, ControlEvents.Validating, processFillMaps );
			}
			return vr;
		}

		protected override void OnValidating(CancelEventArgs e)
		{
			e.Cancel = !this.ProcessValidate( true ).Success;

			base.OnValidating( e );
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				_va.ProcessEvent( this.Text, ControlEvents.EnabledChanged, true );
				//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Enabled, _auditEventHandler );

				base.OnEnabledChanged( e );
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
			{
				_va.ProcessEvent( this.Text, ControlEvents.VisibleChanged, true );
				//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Visible, _auditEventHandler );

				base.OnVisibleChanged( e );
			}
		}

		/// <summary>
		/// Resets flags indicating control should be validated and FillMaps should processed.
		/// </summary>
		protected override void OnTextChanged(System.EventArgs e)
		{
			string newText = string.Empty;
			if( this.Text.Length > 0 )
			{
				newText = this.Text.Length > 25 ? this.Text.Substring( 0, 25 ) : this.Text;
			}

			_sa.AuditAction( AuditType.ControlDetail, null,
				String.Format( "TextChanged. New text: [{0}]", newText ), false );


			_va.ProcessEvent( this.Text, ControlEvents.TextChanged, true );


			if( _selectNextOnML && ( this.TextLength == this.MaxLength ) )
			{
				this.Parent.SelectNextControl( this, true, true, true, true );
			}


			base.OnTextChanged( e );
		}



		protected override void OnEnter(EventArgs e)
		{
			_va.ProcessEvent( this.Text, ControlEvents.Enter, true );
			base.OnEnter( e );
		}

		/// <summary>
		/// Used to format text on field exit.
		/// </summary>
		protected override void OnLeave(EventArgs e)
		{
			//this looks dumb, but it actually causes the text to get Formatted:
			//see overridden Text property
			this.Text = this.Text;

			//OnValidating fires right after OnLeave, rules for data validation
			//should go on OnValidating, not on OnLeave.
			_va.ProcessEvent( this.Text, ControlEvents.Leave, true );

			base.OnLeave( e );
		}


		//		protected override void OnReadOnlyChanged(EventArgs e)
		//		{
		//			//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Operate, _auditEventHandler );
		//
		//			base.OnReadOnlyChanged( e );
		//		}
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
			if( !this.DesignMode )
			{
				if( !_sr[AceType.UI, UIRight.Operate].AccessAllowed )
				{
					base.ReadOnly = true;
				}
				if( !_sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					base.Enabled = false;
				}
				if( !_sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					base.Visible = false;
				}
			}
		}

		[DefaultValue( false )]
		new public bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					base.Visible = value;
				}
			}
		}

		[DefaultValue( false )]
		new public bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					base.Enabled = value;
				}
			}
		}

		[DefaultValue( false )]
		new public bool ReadOnly
		{
			get
			{
				return base.ReadOnly;
			}
			set
			{
				if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
				{
					base.ReadOnly = value;
				}
			}
		}
		#endregion
	}	//class
}	//namespace