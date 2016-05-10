using System;
using System.Reflection;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

using Suplex.Data;
using Suplex.Forms;
using Suplex.General;
using Suplex.Security;
using Suplex.Security.Standard;

namespace Suplex.WebForms
{
	/// <summary>
	/// Provides secuirty, extended validation properties, and auto-validation.
	/// </summary>
	[ToolboxItem( true ),
		System.Drawing.ToolboxBitmap(typeof(System.Web.UI.WebControls.ContentPlaceHolder))]
	[EventBindings( EventBindingsAttribute.BaseEvents.None,
		ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.VisibleChanged | ControlEvents.TextChanged )]
	public class sContentPlaceHolder : System.Web.UI.WebControls.ContentPlaceHolder, IValidationContainer, IWebExtra
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationContainerAccessor _va = null;

		private string _tag = null;
		private object _tagObject = null;


		public sContentPlaceHolder() : base()
		{
			_sa = new SecurityAccessor( this, AceType.UI );
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
		#endregion


		#region IWebExtra Members
		public string Tag
		{
			get { return _tag; }
			set { _tag = value; }
		}

		[Browsable( false )] public object TagObj
		{
			get { return _tagObject; }
			set { _tagObject = value; }
		}
		#endregion
	}
}