using System;
using System.Text;
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
		System.Drawing.ToolboxBitmap(typeof(System.Web.UI.WebControls.DropDownList))]
	[EventBindings( EventBindingsAttribute.BaseEvents.WebForms, ControlEvents.SelectedIndexChanged )]
	public class sDropDownList : System.Web.UI.WebControls.DropDownList, IValidationControl, IWebExtra
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationAccessor _va = null;

		private string _tag = null;
		private object _tagObject = null;


		private int _lastSelectedIndex = -1;


		public sDropDownList() : base()
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

			this.CreateJavaScript();

			base.OnPreRender( e );
		}

		protected override object SaveViewState()
		{
			object baseState = base.SaveViewState();

			object[] s = new object[2];

			s[0] = baseState;
			s[1] = _tag;

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
				vr = _va.ProcessEvent( this.SelectedValue, ControlEvents.Validating, processFillMaps );
			}
			return vr;
		}

		/// <summary>
		/// Resets flags indicating control should be validated and FillMaps should processed.
		/// </summary>
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "SelectedIndexChanged.", false );

			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				if( this.SelectedIndex != _lastSelectedIndex )
				{
					_lastSelectedIndex = this.SelectedIndex;

					_va.ProcessEvent( this.SelectedValue, ControlEvents.SelectedIndexChanged, true );
				}

				base.OnSelectedIndexChanged( e );
			}
			else
			{
				this.SelectedIndex = _lastSelectedIndex;
			}
		}

		/// <summary>
		/// Resets flag indicating previous selected index.
		/// </summary>
		protected override void OnDataBinding(EventArgs e)
		{
			_lastSelectedIndex = -1;

			base.OnDataBinding( e );
		}

		private void OnEnabledChanged()
		{
			if ( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				_va.ProcessEvent( this.SelectedValue, ControlEvents.EnabledChanged, true );
			}
		}

		private void OnVisibleChanged()
		{
			if ( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
			{
				_va.ProcessEvent( this.SelectedValue, ControlEvents.VisibleChanged, true );
			}
		}
		#endregion


		#region type-ahead stuff

		private void CreateJavaScript()
		{
			if( !this.Page.ClientScript.IsClientScriptBlockRegistered( this.Page.GetType(), "typeahead" ) )
			{
				this.Page.ClientScript.RegisterClientScriptBlock( this.Page.GetType(), "typeahead", GetScriptBlock() );
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			base.Attributes["onKeypress"] = "return (dodefaultaction()==''); ";
			base.Attributes["onKeydown"] = "return (dodefaultaction()==''); ";
			base.Attributes["onKeyup"] = "return (change(" + base.ClientID + "));";
			base.Attributes["onfocus"] = "txtval = '';";
			base.Attributes["onblur"] = "txtval = '';";

			base.Render( writer );
		}

		protected string GetScriptBlock()
		{
			StringBuilder script = new StringBuilder( "<script language=\"javascript\">\r\n<!--\r\n\r\n" );
			script.Append( "\tvar txtval = '';\r\n" );
			script.Append( "\tvar curlist;\r\n\r\n" );
			script.Append( "\tfunction select(trigger)\r\n" );
			script.Append( "\t{\r\n" );
			script.Append( "\t\tcurlist=trigger;\r\n" );
			script.Append( "\t\tfor( n=0; n<curlist.length; n++ )\r\n" );
			script.Append( "\t\t{\r\n" );
			script.Append( "\t\t\tif( curlist[n].value.toLowerCase().indexOf( txtval.toLowerCase() ) == 0 )\r\n" );
			script.Append( "\t\t\t{\r\n" );
			script.Append( "\t\t\t\tcurlist.selectedIndex = n;\r\n" );
			script.Append( "\t\t\t\tbreak;\r\n" );
			script.Append( "\t\t\t}\r\n" );
			script.Append( "\t\t\telse if( curlist[n].text.toLowerCase().indexOf( txtval.toLowerCase() ) == 0 )\r\n" );
			script.Append( "\t\t\t{\r\n" );
			script.Append( "\t\t\t\tcurlist.selectedIndex = n;\r\n" );
			script.Append( "\t\t\t\tbreak;\r\n" );
			script.Append( "\t\t\t}\r\n" );
			script.Append( "\t\t\telse\r\n" );
			script.Append( "\t\t\t{\r\n" );
			script.Append( "\t\t\t\tif( n == curlist.length-1 ) txtval='';\r\n" );
			script.Append( "\t\t\t\tcurlist.selectedIndex = 0;\r\n" );
			script.Append( "\t\t\t}\r\n" );
			script.Append( "\t\t}\r\n" );
			script.Append( "\t}\r\n\r\n" );
			script.Append( "\tfunction dodefaultaction(e)\r\n" );
			script.Append( "\t{\r\n" );
			script.Append( "\t\tvar code;\r\n" );
			script.Append( "\t\tif( !e ) var e = window.event;\r\n" );
			script.Append( "\t\tif( e.keyCode ) code = e.keyCode;\r\n" );
			script.Append( "\t\telse if( e.which ) code = e.which;\r\n" );
			script.Append( "\t\tif( code == '9' | code == '40' | code == '38' ) return '';\r\n" );
			script.Append( "\t\telse return code;\r\n" );
			script.Append( "\t}\r\n\r\n" );
			script.Append( "\tfunction change(trigger)\r\n" );
			script.Append( "\t{\r\n" );
			script.Append( "\t\tvar code = dodefaultaction();\r\n" );
			script.Append( "\t\tif(code == '')\r\n" );
			script.Append( "\t\t{\r\n" );
			script.Append( "\t\t\ttxtval='';\r\n" );
			script.Append( "\t\t\treturn false;\r\n" );
			script.Append( "\t\t}\r\n" );
			script.Append( "\t\telse\r\n" );
			script.Append( "\t\t{\r\n" );
			script.Append( "\t\t\tif(code == '8')\r\n" );
			script.Append( "\t\t\t{\r\n" );
			script.Append( "\t\t\t\ttxtval='';\r\n" );
			script.Append( "\t\t\t}\r\n" );
			script.Append( "\t\t\telse\r\n" );
			script.Append( "\t\t\t{\r\n" );
			script.Append( "\t\t\t\ttxtval = txtval + String.fromCharCode( code );\r\n" );
			script.Append( "\t\t\t}\r\n" );
			script.Append( "\t\t\tselect( trigger );\r\n" );
			script.Append( "\t\t\treturn true;\r\n" );
			script.Append( "\t\t}\r\n" );
			script.Append( "\t}\r\n\r\n" );
			script.Append( "// -->\r\n</script>" );

			return script.ToString();
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
				if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					if( base.Visible != value )
					{
						base.Visible = value;
						OnVisibleChanged();
					}
				}
			}
		}

		[DefaultValue( false )]
		public override bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
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