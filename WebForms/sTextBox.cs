using System;
using System.Reflection;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;
using Suplex.Security.Standard;

namespace Suplex.WebForms
{
	/// <summary>
	/// Provides secuirty, extended validation properties, and auto-validation.
	/// </summary>
	[ToolboxItem( true ),
		System.Drawing.ToolboxBitmap(typeof(System.Web.UI.WebControls.TextBox))]
	[EventBindings( EventBindingsAttribute.BaseEvents.WebForms, ControlEvents.TextChanged )]
	public class sTextBox : System.Web.UI.WebControls.TextBox, IValidationTextBox, IWebExtra
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationAccessor _va = null;

		private string _tag = null;
		private object _tagObject = null;


		private string _formatString = null;


		public sTextBox() : base()
		{
			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationAccessor( this, TypeCode.String );
		}

		protected override void OnInit(EventArgs e)
		{
			_sa.EnsureDefaultState();

			base.OnInit( e );
		}

		protected override void OnPreRender(EventArgs e)
		{
			this.ApplySecurity();

			base.OnPreRender( e );
		}

		protected override object SaveViewState()
		{
			object baseState = base.SaveViewState();

			object[] s = new object[3];

			s[0] = baseState;
			s[1] = _tag;
			s[2] = _formatString;

			return s;
		}

		/// <summary>
		/// Load State from the array of objects that was saved at SavedViewState.
		/// </summary>
		/// <param name="savedState">State information to load.</param>
		protected override void LoadViewState(object savedState)
		{
			object[] s = (object[])savedState;

			if( s[0] != null )
				base.LoadViewState( s[0] );

			if( s[1] != null )
				_tag = (string)s[1];

			if( s[2] != null )
				_formatString = (string)s[2];
		}


		[ParenthesizePropertyName( true ), Category( "Suplex" )]
		public string UniqueName
		{
			get
			{
				return string.IsNullOrEmpty( _uniqueName ) ? base.UniqueID : _uniqueName;
			}
			set
			{
				_uniqueName = value;
			}
		}

		[Browsable( false ), Category( "Suplex" )]
		public Suplex.Data.DataAccessLayer DataAccessLayer
		{
			get { return _dal; }
			set { _dal = value; }
		}


		#region IValidationTextbox Members
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

		public int TextLength
		{
			get { return base.Text.Length; }
		}

		/// <summary>
		/// Formats text according to FormatString.
		/// If text cannot be formatted, the param value is returned (unchanged).
		/// </summary>
		/// <param name="text">The text to format.</param>
		/// <returns>Formatted text string.</returns>
		private string FormatText(string text)
		{
			if( this.Validation.DataType != TypeCode.String && 
				this.FormatString != null && this.FormatString.Length > 0 )
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
		#endregion


		#region Validation Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Validation management properties and tools." )]
		public IValidationAccessor Validation
		{
			get { return _va; }
		}

		public ValidationResult ProcessValidate(bool processFillMaps)
		{
			ValidationResult vr = null;
			if( this.Enabled )
			{
				vr = _va.ProcessEvent( this.Text, ControlEvents.Validating, processFillMaps );
			}
			return vr;
		}

		/// <summary>
		/// Resets flags indicating control should be validated and FillMaps should processed.
		/// </summary>
		protected override void OnTextChanged( System.EventArgs e )
		{
			if( this.TextMode != TextBoxMode.Password )
			{
				string newText = "";
				if( this.Text.Length > 0 )
				{
					newText = this.Text.Length > 25 ? this.Text.Substring( 0, 25 ) : this.Text;
				}

				_sa.AuditAction( AuditType.ControlDetail, null,
					String.Format( "TextChanged. New text: [{0}]", newText ), false );
			}

			_va.ProcessEvent( this.Text, ControlEvents.TextChanged, true );

			base.OnTextChanged( e );
		}

		private void OnEnabledChanged()
		{
			if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				_va.ProcessEvent( this.Text, ControlEvents.EnabledChanged, true );
			}
		}

		private void OnVisibleChanged()
		{
			if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
			{
				_va.ProcessEvent( this.Text, ControlEvents.VisibleChanged, true );
			}
		}
		#endregion


		#region Security Implementation

		[TypeConverter( typeof( ExpandableObjectConverter ) ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Security management properties and tools." )]
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
		public override bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				if ( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					if( base.Visible != value )
					{
						base.Visible = value;
						OnVisibleChanged();
					}
				}
			}
		}

		[DefaultValue(false)]
		public override bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				if ( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					if( base.Enabled != value )
					{
						base.Enabled = value;
						OnEnabledChanged();
					}
				}
			}
		}
		#endregion


		#region IWebExtra Members
		public string Tag
		{
			get { return _tag; }
			set { _tag = value; }
		}

		[Browsable( false )]
		public object TagObj
		{
			get { return _tagObject; }
			set { _tagObject = value; }
		}
		#endregion
	}
}