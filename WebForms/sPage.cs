using System;
using System.Reflection;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

using Suplex.Data;
using Suplex.Forms;
using Suplex.General;
using Suplex.Security;
using Suplex.Security.Standard;

using config = System.Configuration.ConfigurationManager;


namespace Suplex.WebForms
{
	/// <summary>
	/// Provides secuirty, extended validation properties, and auto-validation.
	/// </summary>
	[ToolboxItem( true ),
		System.Drawing.ToolboxBitmap(typeof(System.Web.UI.Page))]
	[EventBindings( EventBindingsAttribute.BaseEvents.None,
		ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.VisibleChanged | ControlEvents.TextChanged )]
	public class sPage : System.Web.UI.Page, IValidationContainer, IWebExtra
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationContainerAccessor _va = null;

		private string _tag = null;
		private object _tagObject = null;


		private const string __allowPageDiagnostics = "AllowPageDiagnostics";
		private bool _allowPageDiagnostics = false;
		private bool _dumpSecurityValidation = false;
		private bool _parseViewState = false;


		public sPage() : base()
		{
			_sa = new SecurityAccessor( this, AceType.UI, DefaultSecurityState.Unlocked );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationContainerAccessor( this, TypeCode.Empty );
		}

		protected override void OnInit(EventArgs e)
		{
			_sa.EnsureDefaultState();

			base.OnInit( e );
		}

		protected override void OnPreRender(EventArgs e)
		{
			this.ApplySecurity();

			this.DumpSecurityValidation();

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

		#region Diagnostic stuff
		protected override void OnPreInit(EventArgs e)
		{
			_allowPageDiagnostics =
				!string.IsNullOrEmpty( config.AppSettings[__allowPageDiagnostics] ) &&
				Boolean.Parse( config.AppSettings[__allowPageDiagnostics] );

			string diags = this.Request.QueryString["splx"];
			if( !string.IsNullOrEmpty( diags ) )
			{
				int level = 0;
				if( Int32.TryParse( diags, out level ) )
				{
					_dumpSecurityValidation = level > 0;
					_parseViewState = level > 1;
				}
			}

			base.OnPreInit( e );
		}

		protected override void SavePageStateToPersistenceMedium(object state)
		{
			base.SavePageStateToPersistenceMedium( state );

			if( _allowPageDiagnostics && _parseViewState )
			{
				this.Trace.IsEnabled = true;
				ViewStateParser.ParseViewState( state, 0, this.Trace, false );
			}
		}

		private void DumpSecurityValidation()
		{
			if( _allowPageDiagnostics && _dumpSecurityValidation )
			{
				DiagInfoStreams sec = SecureControlUtils.DumpSecurity( this, true, false );
				DiagInfoStreams val = ValidationControlUtils.DumpValidation( this, true, false );


				this.Response.Write( "<!-- " );
				this.Response.Write( DiagTitle( "Security Hierarchy" ) );
				this.Response.Write( SecureControlUtils.DumpHierarchy( this, false ) );

				this.Response.Write( DiagTitle( "Security Detail" ) );
				this.Response.Write( sec.Text  );

				this.Response.Write( DiagTitle( "Validation Hierarchy" ) );
				this.Response.Write( ValidationControlUtils.DumpHierarchy( this, false ) );

				this.Response.Write( DiagTitle( "Validation Detail" ) );
				this.Response.Write( val.Text );

				this.Response.Write( DiagTitle( "End Diagnotics" ) );
				this.Response.Write( "-->" );


				this.Response.Write( sec.Html );
				this.Response.Write( "<BR><BR>" );
				this.Response.Write( val.Html );
			}
		}
		private string DiagTitle(string title)
		{
			return string.Format( "\r\n\r\n************************************************** {0} **************************************************\r\n\r\n", title );
		}
		#endregion

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
			if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				vr = _va.ProcessEvent( null, ControlEvents.Validating, processFillMaps );
			}
			return vr;
		}

		private void OnEnabledChanged()
		{
			if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				_va.ProcessEvent( null, ControlEvents.EnabledChanged, true );
			}
		}

		private void OnVisibleChanged()
		{
			if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
			{
				_va.ProcessEvent( null, ControlEvents.VisibleChanged, true );
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
				if( !_sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					base.Visible = false;
				}
			}
		}

		public ICollection GetChildren()
		{
			return (ICollection)this.Controls;
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


	//	Source:
	//		http://aspalliance.com/135
	internal class ViewStateParser
	{
		private static TraceContext _trace = null;
		private static bool _includeTypeInfo = false;
		public static void ParseViewState(object vs, int level, TraceContext trace, bool includeTypeInfo)
		{
			_trace = trace;
			_includeTypeInfo = includeTypeInfo;
			ParseViewState( vs, level );
		}
		private static void ParseViewState(object vs, int level)
		{
			if( vs == null )
			{
				_trace.Warn( level.ToString(), Spaces( level ) + "null" );
			}
			else if( vs.GetType() == typeof( System.Web.UI.Triplet ) )
			{
				if( _includeTypeInfo ) _trace.Warn( level.ToString(), Spaces( level ) + "Triplet" );
				ParseViewState( (Triplet)vs, level );
			}
			else if( vs.GetType() == typeof( System.Web.UI.Pair ) )
			{
				if( _includeTypeInfo ) _trace.Warn( level.ToString(), Spaces( level ) + "Pair" );
				ParseViewState( (Pair)vs, level );
			}
			else if( vs.GetType() == typeof( System.Collections.ArrayList ) )
			{
				if( _includeTypeInfo ) _trace.Warn( level.ToString(), Spaces( level ) + "ArrayList" );
				ParseViewState( (IEnumerable)vs, level );
			}
			else if( vs.GetType().IsArray )
			{
				if( _includeTypeInfo ) _trace.Warn( level.ToString(), Spaces( level ) + "Array" );
				ParseViewState( (IEnumerable)vs, level );
			}
			else if( vs.GetType() == typeof( System.Web.UI.IndexedString ) )
			{
				_trace.Warn( level.ToString(), string.Format( "{0}{1}{2}", Spaces( level ), ( (IndexedString)vs ).Value, _includeTypeInfo ? " [IndexedString]" : "" ) );
			}
			else if( vs.GetType() == typeof( System.String ) )
			{
				_trace.Warn( level.ToString(), Spaces( level ) + "'" + vs.ToString() + "'" );
			}
			else if( vs.GetType().IsPrimitive )
			{
				_trace.Warn( level.ToString(), Spaces( level ) + vs.ToString() );
			}
			else
			{
				_trace.Warn( level.ToString(), string.Format( "{0}{1}", Spaces( level ), vs.GetType().ToString() ) );
			}
		}
		private static void ParseViewState(Triplet vs, int level)
		{
			ParseViewState( vs.First, level + 1 );
			ParseViewState( vs.Second, level + 1 );
			ParseViewState( vs.Third, level + 1 );
		}
		private static void ParseViewState(Pair vs, int level)
		{
			ParseViewState( vs.First, level + 1 );
			ParseViewState( vs.Second, level + 1 );
		}
		private static void ParseViewState(IEnumerable vs, int level)
		{
			foreach( object item in vs )
			{
				ParseViewState( item, level + 1 );
			}
		}
		private static string Spaces(int count)
		{
			string spaces = "";
			for( int index = 0; index < count; index++ )
			{
				spaces += "   ";
			}
			return spaces;
		}
	}

}