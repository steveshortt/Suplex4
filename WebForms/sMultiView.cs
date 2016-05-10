using System;
using System.Reflection;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

using dbg = System.Diagnostics.Debug;

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
        System.Drawing.ToolboxBitmap( typeof( System.Web.UI.WebControls.MultiView ) )]
	[Themeable( true )]
	[EventBindings( EventBindingsAttribute.BaseEvents.None,
		ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.VisibleChanged | ControlEvents.TextChanged )]
	public class sMultiView : System.Web.UI.WebControls.MultiView, IValidationContainer, IPostBackEventHandler, IWebExtra
    {
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationContainerAccessor _va = null;

		private string _tag = null;
		private object _tagObject = null;


		private bool _templateInstantiated = false;

		//private int _lastActiveView = -1;


        #region Tabs vars
        private bool _renderTabs = false;
        private string _outerTableCssClass;
        private string _outerTableHeaderCellCssClass;
        private string _outerTableContentCellCssClass;
        private string _tabTableCssClass;
        private string _tabCellCssClass;
        private string _tabSelectedCellCssClass;
		//private string _tabSelectedLeftImage;
		//private string _tabSelectedRightImage;
		private bool _findSelectedByUrl = false;
		private MultiViewTabLinkStyle _tabLinkStyle = MultiViewTabLinkStyle.LinkButton;
		private MultiViewSelectedTabLinkStyle _selectedTabLinkStyle = MultiViewSelectedTabLinkStyle.Label;
        #endregion


        #region Events
		public event System.EventHandler ActiveViewSelected;
		public event System.EventHandler TemplateInstantiated;

		protected void OnActiveViewSelected(EventArgs e)
		{
			if( ActiveViewSelected != null )
			{
				ActiveViewSelected( this, EventArgs.Empty );
			}
		}

		protected void OnTemplateInstantiated(EventArgs e)
		{
			if( TemplateInstantiated != null )
			{
				TemplateInstantiated( this, EventArgs.Empty );
			}
		}
        #endregion


		#region ctor/validation/sec impl

		public sMultiView() : base()
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

		//protected override void OnInit(EventArgs e)
		//{
		//    //this.AuditAccess();

		//    //removed 07162006, doesn't quite work, needs some more effort
		//    //this.Page.PreLoad += new EventHandler( Page_PreLoad );	//07162006
		//    base.OnInit( e );
		//}

		#region TemplatedContent stuff
		[Obsolete( "07162006: Work on this later, can be made to work with a little effort.", true )]
		void Page_PreLoad(object sender, EventArgs e)
		{
			if( !_templateInstantiated )
			{
				InstantiateTemplatedContent();
			}
		}


		private int _activeViewIndex = -2;
		new public int ActiveViewIndex
		{
			get { return base.ActiveViewIndex; }
			set
			{
				base.ActiveViewIndex = value;
				_activeViewIndex = value;
			}
		}
		private int InnerActiveViewIndex
		{
			get {
				if( _activeViewIndex > -2 )
					return _activeViewIndex;
				else
					return base.ActiveViewIndex;
			}
			set { _activeViewIndex = value; }
		}

		[Obsolete( "07162006: Work on this later, can be made to work with a little effort.", true )]
		private void InstantiateTemplatedContent()
		{
			_templateInstantiated = true;
			sView view = null;
			int index = -1;

			if( this.Page.Request.Form["__EVENTTARGET"] != null )
			{
				spit( string.Format( "~~~~ MW seaching for: {0} ~~~~", this.ID ) );
				string[] targetParts = this.Page.Request.Form["__EVENTTARGET"].Split( this.IdSeparator );
				foreach( string trgt in targetParts )
				{
					spit( "~~> trgt: " + trgt );
					if( trgt == this.ID )
					{
						spit( "~~> found trgt: " + trgt );
						if( Int32.TryParse( this.Page.Request.Form["__EVENTARGUMENT"], out index ) )
						{
							view = (sView)this.Views[index];
						}
						break;
					}
				}
			}

			if( view == null )
			{
				spit( string.Format( "~~> view == null: {0}, {1}, ActiveViewIndex: {2}", this.ID, this.Parent.ID, this.ActiveViewIndex.ToString() ) );
				if( this.ActiveViewIndex > -1 )
				{
					//this.ActiveViewIndex = _activeViewIndex;
					view = (sView)this.Views[this.ActiveViewIndex];
					view.Title += "+";
				}
			}
			else
			{
				spit( string.Format( "~~> view != null: {0}, {1}, ActiveViewId: {2}", this.ID, this.Parent.ID, view.ID ) );
				view.Title += "*";
			}

			if( view != null )
			{
				if( view is sTemplatedView && ( (sTemplatedView)view ).ContentTemplate != null )
				{
					spit( string.Format( "~~> view != null: Has non-null TemplatedContent: {0}, {1}, ActiveViewId: {2}", this.ID, this.Parent.ID, view.ID ) );
					//view.ContentTemplate.InstantiateIn( view );
					( (sTemplatedView)view ).InstantiateContent();
					OnTemplateInstantiated( EventArgs.Empty );
				}
			}
		}
		private void spit(string message)
		{
			//dbg.WriteLine( message );
			this.Page.Trace.Warn( message );
		}

		//private int LastViewIndex
		//{
		//    get
		//    {
		//        if( ViewState["__lastViewIndex"] != null )
		//        { return (int)ViewState["__lastViewIndex"]; }
		//        else
		//        { return -1; }
		//    }
		//    set { ViewState["__lastViewIndex"] = value; }
		//}
		#endregion


		protected override void OnPreRender(EventArgs e)
		{
			this.ApplySecurity();

			base.OnPreRender( e );
		}

		protected override object SaveViewState()
		{
			object[] s = new object[4];

			s[0] = base.SaveViewState();
			s[1] = this.ActiveViewIndex;
			s[2] = _renderTabs;
			s[3] = _tag;

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
				this.ActiveViewIndex = (int)s[1];

			if( s[2] != null )
				_renderTabs = (bool)s[2];

			if( s[3] != null )
				_tag = (string)s[3];

			//this is not the world's greatest place to do this, but I couldn't find better place.
			// need this.ActiveViewIndex to be restored before attempting Content Instantiation.
			////InstantiateTemplatedContent();	//07162006
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
		#endregion


		#region Tabs Implementation
		public virtual bool RenderTabs
		{
			get { return _renderTabs; }
			set { _renderTabs = value; }
		}
		public virtual MultiViewTabLinkStyle TabLinkStyle
		{
			get { return _tabLinkStyle; }
			set { _tabLinkStyle = value; }
		}
		public virtual MultiViewSelectedTabLinkStyle SelectedTabLinkStyle
		{
			get { return _selectedTabLinkStyle; }
			set { _selectedTabLinkStyle = value; }
		}
		public virtual bool FindSelectedByUrl
		{
			get { return _findSelectedByUrl; }
			set { _findSelectedByUrl = value; }
		}
		public virtual string OuterTableCssClass
		{
			get { return _outerTableCssClass; }
			set { _outerTableCssClass = value; }
		}
		public virtual string OuterTableHeaderCellCssClass
		{
			get { return _outerTableHeaderCellCssClass; }
			set { _outerTableHeaderCellCssClass = value; }
		}
		public virtual string OuterTableContentCellCssClass
		{
			get { return _outerTableContentCellCssClass; }
			set { _outerTableContentCellCssClass = value; }
		}
		public virtual string TabTableCssClass
		{
			get { return _tabTableCssClass; }
			set { _tabTableCssClass = value; }
		}
		public virtual string TabCellCssClass
		{
			get { return _tabCellCssClass; }
			set { _tabCellCssClass = value; }
		}
		public virtual string TabSelectedCellCssClass
		{
			get { return _tabSelectedCellCssClass; }
			set { _tabSelectedCellCssClass = value; }
		}

		protected override void CreateChildControls()
		{
			//this.LastViewIndex = this.ActiveViewIndex;

			bool selected = false;

			if( _renderTabs )
			{
				int activeViewIndex = this.ActiveViewIndex;

				for( int v = 0; v < this.Views.Count; v++ )
				{
					selected = v == activeViewIndex;

					if( this.Views[v] is sView )
					{
						if( _findSelectedByUrl )
						{
							selected =
								this.Page.ResolveUrl( ( (sView)this.Views[v] ).NavigateUrl ) == this.Page.ResolveUrl( this.Page.Request.FilePath );
							if( selected ) this.ActiveViewIndex = v;
						}

						bool visible = ( (sView)this.Views[v] ).Security.Descriptor.SecurityResults[AceType.UI, UIRight.Visible].AccessAllowed &&
							( (sView)this.Views[v] ).TabVisible;
						bool enabled = ( (sView)this.Views[v] ).Security.Descriptor.SecurityResults[AceType.UI, UIRight.Enabled].AccessAllowed &&
							( (sView)this.Views[v] ).TabEnabled;
						bool operate = ( (sView)this.Views[v] ).Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed;

						if( !visible || !enabled || !operate )
						{
							if( selected )
							{
								//this.ActiveViewIndex = -1;
								activeViewIndex++;
							}
						}
					}
				}

				if( activeViewIndex != this.ActiveViewIndex && activeViewIndex <= this.Views.Count - 1 )
				{
					this.ActiveViewIndex = activeViewIndex;
				}
			}

			base.CreateChildControls();
		}

		protected override void Render(HtmlTextWriter writer)
        {
			bool tabIsLinkButton = true;
            bool selected = false;
			bool selIsLabel = true;

            if( _renderTabs )
            {
                Table tabs = new Table();
                tabs.CssClass = _tabTableCssClass;
                tabs.Rows.Add( new TableRow() );

				int activeViewIndex = this.ActiveViewIndex;

                for( int v = 0; v < this.Views.Count; v++ )
                {
					tabIsLinkButton = _tabLinkStyle == MultiViewTabLinkStyle.LinkButton;
					selIsLabel = _selectedTabLinkStyle == MultiViewSelectedTabLinkStyle.Label;
					selected = v == activeViewIndex;

                    TableCell tc = new TableCell();
					WebControl link = new LinkButton();

					if( this.Views[v] is sView )
					{
						if( _findSelectedByUrl )
						{
							selected =
								this.Page.ResolveUrl( ( (sView)this.Views[v] ).NavigateUrl ) == this.Page.ResolveUrl( this.Page.Request.FilePath );
							if( selected ) this.ActiveViewIndex = v;
						}

						tabIsLinkButton = ( (sView)this.Views[v] ).TabLinkStyle == MultiViewTabLinkStyle.LinkButton;
						selIsLabel = _selectedTabLinkStyle == MultiViewSelectedTabLinkStyle.Label;

						tc.Visible = ( (sView)this.Views[v] ).Security.Descriptor.SecurityResults[AceType.UI, UIRight.Visible].AccessAllowed &&
							( (sView)this.Views[v] ).TabVisible;
						tc.Enabled = ( (sView)this.Views[v] ).Security.Descriptor.SecurityResults[AceType.UI, UIRight.Enabled].AccessAllowed &&
							( (sView)this.Views[v] ).TabEnabled;
						bool operate = ( (sView)this.Views[v] ).Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed;

						if( selected && selIsLabel )
						{
							link = new Label();
							( (Label)link ).Text = ( (sView)this.Views[v] ).Title;
						}
						else
						{
							link.AccessKey = ( (sView)this.Views[v] ).TabAccessKey;

							if( tabIsLinkButton )
							{
								( (LinkButton)link ).Text = RenderUtil.RenderAccessKey( ( (sView)this.Views[v] ).Title, link );
								link.Attributes.Add( "href", "javascript:" + Page.ClientScript.GetPostBackEventReference( this, v.ToString() ) );
								//Page.GetPostBackClientEvent( this, v.ToString() ) ); //deprecated in 2.0
							}
							else
							{
								link = new HyperLink();
								( (HyperLink)link ).Text = RenderUtil.RenderAccessKey( ( (sView)this.Views[v] ).Title, link );
								( (HyperLink)link ).NavigateUrl = ( (sView)this.Views[v] ).NavigateUrl;
							}
						}

						tc.Controls.Add( link );

						link.Visible = tc.Visible;
						link.Enabled = tc.Enabled;

						if( !link.Visible || !link.Enabled || !operate )
						{
							if( !( link is Label ) )
							{
								link.Attributes["href"] = "#";
							}

							if( selected )
							{
								//this.ActiveViewIndex = -1;
								activeViewIndex++;
							}
						}
						if( link.Visible ) tabs.Rows[0].Cells.Add( tc );
					}
					else
					{
						( (LinkButton)link ).Text = this.Views[v].UniqueID.Replace( "_", " " );
					}

					tc.CssClass = selected && v == activeViewIndex ? _tabSelectedCellCssClass : _tabCellCssClass;

				}

				//NOTE: Can't do this here; Render is too late in the process to switch ActiveViewIndex:
				//		handlers and other stuff don't get hooked up properly.
				//		Duplicate logic above in CreateChildControls seems to work just fine.
				//if( activeViewIndex != this.ActiveViewIndex && activeViewIndex <= this.Views.Count - 1 )
				//{
				//    this.ActiveViewIndex = activeViewIndex;
				//}

                writer.Write( "<table class=\"{0}\"><tr><td class=\"{1}\">\r\n", _outerTableCssClass, _outerTableHeaderCellCssClass );
                tabs.RenderControl( writer );
                writer.Write( "\r\n</td></tr><tr><td class=\"{0}\">\r\n", _outerTableContentCellCssClass );
                base.Render( writer );
                writer.Write( "\r\n</td></tr></table>\r\n" );
            }
            else
            {
                base.Render( writer );
            }
        }

        #region IPostBackEventHandler Members
        public void RaisePostBackEvent(string eventArgument)
        {
			if( _renderTabs )
			{
				if( eventArgument != null && eventArgument != string.Empty )
				{
					int index = -1;
					if( Int32.TryParse( eventArgument, out index ) )
					{
						if( this.ActiveViewIndex != index )
						{
							this.ActiveViewIndex = index;
							//OnActiveViewChanged( EventArgs.Empty );	//02212007: redundant; setting this.ActiveViewIndex fires event automatically
						}
						else
						{
							OnActiveViewSelected( EventArgs.Empty );
						}
					}
					else
					{
						this.Page.Response.Redirect( HttpUtility.UrlDecode( eventArgument ) );
					}
				}
			}
        }
        #endregion

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


	public enum MultiViewTabLinkStyle
	{
		LinkButton,
		HyperLink
	}
	public enum MultiViewSelectedTabLinkStyle
	{
		Label,
		Link
	}
}