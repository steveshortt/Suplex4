using System;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;
using Suplex.General;



/*
*************************************************************************************
IMPORTANT: If you edit this control in the designer, after completing
design changes be sure to remove the
	this.Name = "sListViewFilter";
line of code.  Having the control named messes w/ the designer when using
it in other applications.


// 07062005 -- changed UniqueName to overriden version: 'new public string UniqueName'
//   Don't do DateTime thing anymore.
Also, make sure the _listView.UniqueName is edited as follows:
	this._listView.UniqueName = "_listView" + DateTime.Now.Ticks.ToString();
	
// 07062005 -- Also, replaced command buttons w/ toolbars for better look.
//   Did't get rid of button_Click handlers in cas I change my mind later.
*************************************************************************************
*/


namespace Suplex.WinForms
{
	/// <summary>
	/// Summary description for sListViewFilter.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(Suplex.WinForms.sListViewFilter), "Resources.sListViewFilter.gif")]
	public class sListViewFilter : sUserControl
	{
////// private DataView _unfilteredView = null;
////// private DataView _currentView = null;
////// private Hashtable _rowsToItems = null;
////// private Hashtable _itemsToRows = null;
////// private ArrayList _hiddenItems = new ArrayList();


		private TextBox		_lastFilterTextBox			= null;
//		private int			_lastColumn					= 0;
		private int			_runningColWidth			= 0;
		const int			_spacer						= 1;
		private bool		_filterControlsEnabled		= true;
//		private bool		_filterAsDataView			= true;
		private bool		_isFiltered					= false;
		private ToolTip		_ShowHodeToolTip			= null;
		private sListViewFilter.FilterControlsVisibleState	_filterControlsVisible
			= FilterControlsVisibleState.Collapsed;

//////		//private sListViewFilter.sListViewItemCollection _items = null;

		int _initCount = 0;


		private System.Windows.Forms.ImageList imglFilterImages;
		private System.Windows.Forms.Panel pnlFilterCommandControls;
		private System.Windows.Forms.Panel pnlFilterTextControls;
		private System.Windows.Forms.StatusBar stbFilterInfo;
		private System.Windows.Forms.StatusBarPanel stbpFilterInfo;
		private System.Windows.Forms.Timer tmrEventSnyc;
		private Suplex.WinForms.sListView _listView;
		private System.Windows.Forms.ToolBarButton tbbFilterExecute;
		private System.Windows.Forms.ToolBarButton tbbFilterCancel;
		private System.Windows.Forms.ToolBarButton tbbFilterClear;
		private System.Windows.Forms.ToolBar tbFilterCommands;
		private System.Windows.Forms.ToolBar tbFilterHide;
		private System.Windows.Forms.ToolBarButton tbbFilterHide;
		private System.Windows.Forms.ContextMenu ctxMenu;
		private System.Windows.Forms.MenuItem mnuAnd;
		private System.Windows.Forms.MenuItem mnuSep;
		private System.Windows.Forms.MenuItem mnuEquals;
		private System.Windows.Forms.MenuItem mnuLike;
		private System.Windows.Forms.MenuItem mnuNotLike;
		private System.Windows.Forms.MenuItem mnuLessThan;
		private System.Windows.Forms.MenuItem mnuGreaterThan;
		private System.Windows.Forms.MenuItem mnuLessThanEqual;
		private System.Windows.Forms.MenuItem mnuGreaterThanEqual;
		private System.Windows.Forms.MenuItem mnuNotEqual;
		private System.Windows.Forms.MenuItem mnuOr;



		private System.ComponentModel.IContainer components;


		public sListViewFilter()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

////// _rowsToItems = new Hashtable();
////// _itemsToRows = new Hashtable();
////// _items = new sListViewItemCollection( _listView );
////// _items.ItemsSyncReady += new EventHandler( this.Items_ItemsSyncReady );

			// Add any initialization after the InitForm call
			InitializeWrappedEvents();

			CreateToolTips();

			this.Columns.Added += new EventHandler( this.Columns_Added );
			this.Columns.Removed += new EventHandler( this.Columns_Removed );
			this.Columns.Cleared += new EventHandler( this.Columns_Cleared );

			AvailFilter( true );
			ShowHideFilter( false );
		}


		//private void UnlockControls()
		//{
		//UIAce operate = new UIAce( UIRight.Operate, true );
		//this.SecurityDescriptor.Dacl.Add( 0, operate );
		//this.ApplySecurity( AceType.UI );
		//}


		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(sListViewFilter));
			this.pnlFilterCommandControls = new System.Windows.Forms.Panel();
			this.stbFilterInfo = new System.Windows.Forms.StatusBar();
			this.stbpFilterInfo = new System.Windows.Forms.StatusBarPanel();
			this.tbFilterHide = new System.Windows.Forms.ToolBar();
			this.tbbFilterHide = new System.Windows.Forms.ToolBarButton();
			this.imglFilterImages = new System.Windows.Forms.ImageList(this.components);
			this.tbFilterCommands = new System.Windows.Forms.ToolBar();
			this.tbbFilterExecute = new System.Windows.Forms.ToolBarButton();
			this.tbbFilterCancel = new System.Windows.Forms.ToolBarButton();
			this.tbbFilterClear = new System.Windows.Forms.ToolBarButton();
			this.pnlFilterTextControls = new System.Windows.Forms.Panel();
			this.tmrEventSnyc = new System.Windows.Forms.Timer(this.components);
			this._listView = new Suplex.WinForms.sListView();
			this.ctxMenu = new System.Windows.Forms.ContextMenu();
			this.mnuLike = new System.Windows.Forms.MenuItem();
			this.mnuNotLike = new System.Windows.Forms.MenuItem();
			this.mnuEquals = new System.Windows.Forms.MenuItem();
			this.mnuLessThan = new System.Windows.Forms.MenuItem();
			this.mnuGreaterThan = new System.Windows.Forms.MenuItem();
			this.mnuLessThanEqual = new System.Windows.Forms.MenuItem();
			this.mnuGreaterThanEqual = new System.Windows.Forms.MenuItem();
			this.mnuNotEqual = new System.Windows.Forms.MenuItem();
			this.mnuSep = new System.Windows.Forms.MenuItem();
			this.mnuAnd = new System.Windows.Forms.MenuItem();
			this.mnuOr = new System.Windows.Forms.MenuItem();
			this.pnlFilterCommandControls.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.stbpFilterInfo)).BeginInit();
			this.SuspendLayout();
			// 
			// pnlFilterCommandControls
			// 
			this.pnlFilterCommandControls.BackColor = System.Drawing.SystemColors.Control;
			this.pnlFilterCommandControls.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pnlFilterCommandControls.Controls.Add(this.stbFilterInfo);
			this.pnlFilterCommandControls.Controls.Add(this.tbFilterHide);
			this.pnlFilterCommandControls.Controls.Add(this.tbFilterCommands);
			this.pnlFilterCommandControls.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pnlFilterCommandControls.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlFilterCommandControls.Location = new System.Drawing.Point(0, 154);
			this.pnlFilterCommandControls.Name = "pnlFilterCommandControls";
			this.pnlFilterCommandControls.Size = new System.Drawing.Size(256, 22);
			this.pnlFilterCommandControls.TabIndex = 2;
			this.pnlFilterCommandControls.Click += new System.EventHandler(this.pnlFilterCommandControls_Click);
			// 
			// stbFilterInfo
			// 
			this.stbFilterInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.stbFilterInfo.Cursor = System.Windows.Forms.Cursors.Default;
			this.stbFilterInfo.Dock = System.Windows.Forms.DockStyle.None;
			this.stbFilterInfo.Location = new System.Drawing.Point(70, 0);
			this.stbFilterInfo.Name = "stbFilterInfo";
			this.stbFilterInfo.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																							 this.stbpFilterInfo});
			this.stbFilterInfo.ShowPanels = true;
			this.stbFilterInfo.Size = new System.Drawing.Size(160, 18);
			this.stbFilterInfo.SizingGrip = false;
			this.stbFilterInfo.TabIndex = 1;
			// 
			// stbpFilterInfo
			// 
			this.stbpFilterInfo.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
			this.stbpFilterInfo.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.stbpFilterInfo.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.Raised;
			this.stbpFilterInfo.Width = 160;
			// 
			// tbFilterHide
			// 
			this.tbFilterHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.tbFilterHide.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.tbFilterHide.AutoSize = false;
			this.tbFilterHide.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																							this.tbbFilterHide});
			this.tbFilterHide.ButtonSize = new System.Drawing.Size(16, 16);
			this.tbFilterHide.Cursor = System.Windows.Forms.Cursors.Hand;
			this.tbFilterHide.Divider = false;
			this.tbFilterHide.Dock = System.Windows.Forms.DockStyle.None;
			this.tbFilterHide.DropDownArrows = true;
			this.tbFilterHide.ImageList = this.imglFilterImages;
			this.tbFilterHide.Location = new System.Drawing.Point(231, -1);
			this.tbFilterHide.Name = "tbFilterHide";
			this.tbFilterHide.ShowToolTips = true;
			this.tbFilterHide.Size = new System.Drawing.Size(24, 22);
			this.tbFilterHide.TabIndex = 2;
			this.tbFilterHide.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbFilterCommands_ButtonClick);
			// 
			// tbbFilterHide
			// 
			this.tbbFilterHide.ImageIndex = 5;
			this.tbbFilterHide.Tag = "hide";
			this.tbbFilterHide.ToolTipText = "Hide Filter Controls";
			// 
			// imglFilterImages
			// 
			this.imglFilterImages.ImageSize = new System.Drawing.Size(16, 16);
			this.imglFilterImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imglFilterImages.ImageStream")));
			this.imglFilterImages.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// tbFilterCommands
			// 
			this.tbFilterCommands.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.tbFilterCommands.AutoSize = false;
			this.tbFilterCommands.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																								this.tbbFilterExecute,
																								this.tbbFilterCancel,
																								this.tbbFilterClear});
			this.tbFilterCommands.ButtonSize = new System.Drawing.Size(16, 16);
			this.tbFilterCommands.Cursor = System.Windows.Forms.Cursors.Hand;
			this.tbFilterCommands.Divider = false;
			this.tbFilterCommands.Dock = System.Windows.Forms.DockStyle.None;
			this.tbFilterCommands.DropDownArrows = true;
			this.tbFilterCommands.ImageList = this.imglFilterImages;
			this.tbFilterCommands.Location = new System.Drawing.Point(0, -1);
			this.tbFilterCommands.Name = "tbFilterCommands";
			this.tbFilterCommands.ShowToolTips = true;
			this.tbFilterCommands.Size = new System.Drawing.Size(70, 22);
			this.tbFilterCommands.TabIndex = 0;
			this.tbFilterCommands.Wrappable = false;
			this.tbFilterCommands.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbFilterCommands_ButtonClick);
			// 
			// tbbFilterExecute
			// 
			this.tbbFilterExecute.ImageIndex = 2;
			this.tbbFilterExecute.Tag = "execute";
			this.tbbFilterExecute.ToolTipText = "Execute Filter (Enter)";
			// 
			// tbbFilterCancel
			// 
			this.tbbFilterCancel.ImageIndex = 3;
			this.tbbFilterCancel.Tag = "cancel";
			this.tbbFilterCancel.ToolTipText = "Cancel Filter (Shift-Esc)";
			// 
			// tbbFilterClear
			// 
			this.tbbFilterClear.ImageIndex = 4;
			this.tbbFilterClear.Tag = "clear";
			this.tbbFilterClear.ToolTipText = "Clear Filter Controls";
			// 
			// pnlFilterTextControls
			// 
			this.pnlFilterTextControls.BackColor = System.Drawing.SystemColors.Control;
			this.pnlFilterTextControls.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pnlFilterTextControls.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlFilterTextControls.Location = new System.Drawing.Point(0, 0);
			this.pnlFilterTextControls.Name = "pnlFilterTextControls";
			this.pnlFilterTextControls.Size = new System.Drawing.Size(256, 17);
			this.pnlFilterTextControls.TabIndex = 0;
			// 
			// tmrEventSnyc
			// 
			this.tmrEventSnyc.Enabled = true;
			this.tmrEventSnyc.Interval = 50;
			this.tmrEventSnyc.Tick += new System.EventHandler(this.tmrEventSnyc_Tick);
			// 
			// _listView
			// 
			this._listView.Security.DefaultState = Suplex.Security.DefaultSecurityState.Unlocked;
			this._listView.Dock = System.Windows.Forms.DockStyle.Fill;
			this._listView.Enabled = true;
			this._listView.FullRowSelect = true;
			this._listView.HideSelection = false;
			this._listView.sListViewItemAccessor = null;
			this._listView.Location = new System.Drawing.Point(0, 17);
			this._listView.MultiSelect = false;
			this._listView.Name = "_listView";
			this._listView.Size = new System.Drawing.Size(256, 137);
			this._listView.TabIndex = 1;
			this._listView.UniqueName = "_listView";
			this._listView.View = System.Windows.Forms.View.Details;
			this._listView.Visible = true;
			this._listView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FilterTextBox_KeyPress);
//			this._listView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvListView_ColumnClick);
			// 
			// ctxMenu
			// 
			this.ctxMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					this.mnuLike,
																					this.mnuNotLike,
																					this.mnuEquals,
																					this.mnuLessThan,
																					this.mnuGreaterThan,
																					this.mnuLessThanEqual,
																					this.mnuGreaterThanEqual,
																					this.mnuNotEqual,
																					this.mnuSep,
																					this.mnuAnd,
																					this.mnuOr});
			this.ctxMenu.Popup += new System.EventHandler(this.ctxMenu_Popup);
			// 
			// mnuLike
			// 
			this.mnuLike.Checked = true;
			this.mnuLike.Index = 0;
			this.mnuLike.RadioCheck = true;
			this.mnuLike.Text = "Like";
			this.mnuLike.Click += new System.EventHandler(this.mnuComparisonOperator_Click);
			// 
			// mnuNotLike
			// 
			this.mnuNotLike.Index = 1;
			this.mnuNotLike.RadioCheck = true;
			this.mnuNotLike.Text = "Not Like";
			this.mnuNotLike.Click += new System.EventHandler(this.mnuComparisonOperator_Click);
			// 
			// mnuEquals
			// 
			this.mnuEquals.Index = 2;
			this.mnuEquals.RadioCheck = true;
			this.mnuEquals.Text = "=";
			this.mnuEquals.Click += new System.EventHandler(this.mnuComparisonOperator_Click);
			// 
			// mnuLessThan
			// 
			this.mnuLessThan.Index = 3;
			this.mnuLessThan.RadioCheck = true;
			this.mnuLessThan.Text = "<";
			this.mnuLessThan.Click += new System.EventHandler(this.mnuComparisonOperator_Click);
			// 
			// mnuGreaterThan
			// 
			this.mnuGreaterThan.Index = 4;
			this.mnuGreaterThan.RadioCheck = true;
			this.mnuGreaterThan.Text = ">";
			this.mnuGreaterThan.Click += new System.EventHandler(this.mnuComparisonOperator_Click);
			// 
			// mnuLessThanEqual
			// 
			this.mnuLessThanEqual.Index = 5;
			this.mnuLessThanEqual.RadioCheck = true;
			this.mnuLessThanEqual.Text = "<=";
			this.mnuLessThanEqual.Click += new System.EventHandler(this.mnuComparisonOperator_Click);
			// 
			// mnuGreaterThanEqual
			// 
			this.mnuGreaterThanEqual.Index = 6;
			this.mnuGreaterThanEqual.RadioCheck = true;
			this.mnuGreaterThanEqual.Text = ">=";
			this.mnuGreaterThanEqual.Click += new System.EventHandler(this.mnuComparisonOperator_Click);
			// 
			// mnuNotEqual
			// 
			this.mnuNotEqual.Index = 7;
			this.mnuNotEqual.RadioCheck = true;
			this.mnuNotEqual.Text = "<>";
			this.mnuNotEqual.Click += new System.EventHandler(this.mnuComparisonOperator_Click);
			// 
			// mnuSep
			// 
			this.mnuSep.Index = 8;
			this.mnuSep.RadioCheck = true;
			this.mnuSep.Text = "-";
			// 
			// mnuAnd
			// 
			this.mnuAnd.Checked = true;
			this.mnuAnd.Index = 9;
			this.mnuAnd.RadioCheck = true;
			this.mnuAnd.Text = "AND";
			this.mnuAnd.Click += new System.EventHandler(this.mnuLogicalOperator_Click);
			// 
			// mnuOr
			// 
			this.mnuOr.Index = 10;
			this.mnuOr.RadioCheck = true;
			this.mnuOr.Text = "OR";
			this.mnuOr.Click += new System.EventHandler(this.mnuLogicalOperator_Click);
			// 
			// sListViewFilter
			// 
			this.Controls.Add(this._listView);
			this.Controls.Add(this.pnlFilterCommandControls);
			this.Controls.Add(this.pnlFilterTextControls);
			//this.Name = "sListViewFilter";
			this.Size = new System.Drawing.Size(256, 176);
			this.Controls.SetChildIndex(this.pnlFilterTextControls, 0);
			this.Controls.SetChildIndex(this.pnlFilterCommandControls, 0);
			this.Controls.SetChildIndex(this._listView, 0);
			this.pnlFilterCommandControls.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.stbpFilterInfo)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion


		public override string UniqueName
		{
			get { return base.UniqueName; }
			set
			{
				base.UniqueName = value;
				_listView.UniqueName = value + "_listView";
			}
		}

		protected override void InitLayout()
		{
			this.InitializeColumnHeaders();

			base.InitLayout ();
		}


		#region Implementation


		#region easy part

		public sListView InnerListView
		{
			get
			{
				return _listView;
			}
		}


		/// <summary>
		/// Shows/Hides the FilterControls and access bar.
		/// </summary>
		[DefaultValue(sListViewFilter.FilterControlsVisibleState.Collapsed)]
		[Description("Shows/Hides the FilterControls and access bar. Hidden hides controls and access bar, Collapsed hides controls only.")]
		public sListViewFilter.FilterControlsVisibleState FilterControlsVisible
		{
			get
			{
				return _filterControlsVisible;
			}
			set
			{
				if( _filterControlsVisible != value )
				{
					_filterControlsVisible = value;

					switch( value )
					{
						case FilterControlsVisibleState.Hidden:
						{
							AvailFilter( false );
							ShowHideFilter( false );
							break;
						}
						case FilterControlsVisibleState.Collapsed:
						{
							AvailFilter( true );
							ShowHideFilter( false );
							break;
						}
						case FilterControlsVisibleState.Visible:
						{
							AvailFilter( true );
							ShowHideFilter( true );
							break;
						}
					}
				}
			}
		}


		/// <summary>
		/// Enables/Disables the FilterControls and the access bar.
		/// </summary>
		[DefaultValue(true)]
		[Description("Enables/Disables the FilterControls and the access bar.")]
		public bool FilterControlsEnabled
		{
			get
			{
				return _filterControlsEnabled;
			}
			set
			{
				_filterControlsEnabled = value;
				EnableFilter( value );
			}
		}


//		/// <summary>
//		/// If true, converts ListViewItems to DataView for fast and flexible filtering.
//		/// </summary>
//		[DefaultValue(true)]
//		[Description("If true, converts ListViewItems to DataView for fast and flexible filtering.")]
//		public bool FilterAsDataView
//		{
//			get
//			{
//				return _filterAsDataView;
//			}
//			set
//			{
//				_filterAsDataView = value;
//
//				if( !this.DesignMode )
//				{
//					foreach( sColumnHeader mch in _listView.Columns )
//					{
//						if( value )
//						{
//							mch.FilterTextBox.ContextMenu = ctxMenu;
//						}
//						else
//						{
//							mch.FilterTextBox.ContextMenu = null;
//						}
//					}
//				}
//
//			}
//		}


		public void ExecuteFilter()
		{
			if( this.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				ExecuteFilter_DataView( true );
			}
		}


		public void CancelFilter()
		{
			if( this.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				if( _isFiltered )
				{
//////					//kill the sorter before adding hidden items or it's slow to insert when the list is big
//////					IComparer lvis = _listView.ListViewItemSorter;
//////					_listView.ListViewItemSorter = null;


//////					if( _filterAsDataView )
//////					{
						_listView.Data.RowFilter = string.Empty;
//////						this.Fill( _unfilteredView, true );
//////						this.UpdateStatusInfo( false );
//////					}
//////					else
//////					{
//////						IEnumerator hi = _hiddenItems.GetEnumerator();
//////						while( hi.MoveNext() )
//////						{
//////							_listView.Items.Add ( (ListViewItem)hi.Current );
//////						}
//////						_hiddenItems.Clear();
//////					}


//////					if( lvis != null )
//////					{
//////						_listView.ListViewItemSorter = lvis;
//////					}


					UpdateStatusInfo( false );
				}
			}
		}


		public void ClearFilter()
		{
			if( this.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				foreach( sColumnHeader mch in this.Columns )
				{
					mch.FilterTextBox.Text = string.Empty;
				}
			}
		}


		private void ExecuteFilter_DataView(bool isFilterOperation)
		{
			#region build sql

			bool haveSql = false;
			string comp = string.Empty;
			StringBuilder sql = new StringBuilder();
			StringBuilder msg = new StringBuilder();
			foreach( sColumnHeader mch in this.Columns )
			{
				if( mch.FilterTextBox.TextLength > 0 )
				{
					haveSql = true;

					if( mch.Index != this.Columns.Count && sql.Length > 0 )
					{
						sql.AppendFormat( " {0} ", ((FilterOptions)this.Columns[mch.Index].FilterTextBox.Tag).LogicalOperator );
						msg.AppendFormat( " {0} ", ((FilterOptions)this.Columns[mch.Index].FilterTextBox.Tag).LogicalOperator );
					}

					comp = ((FilterOptions)mch.FilterTextBox.Tag).ComparisonOperator;

					// bracket ([]) the field names in case they contain spaces
					if( mch.FilterTextBox.Text.IndexOf( "[dm]" ) > -1 )
					{
						sql.Append( mch.FilterTextBox.Text.Replace( "[dm]", string.Format( "[{0}]", mch.displayMember ) ) );
						msg.Append( mch.FilterTextBox.Text.Replace( "[dm]", mch.Text ) );
					}
					else
					{
						string delim = "'";
						string dataTypeName = _listView.Data.Table.Columns[mch.displayMember].DataType.Name.ToLower();
						switch( dataTypeName )
						{
							case "string":
							{
								break;
							}
							default:
							{
								if( comp.ToLower() == "like" )
								{
									comp = ">=";
									((FilterOptions)mch.FilterTextBox.Tag).ComparisonOperator = comp;
								}
								else if( comp.ToLower() == "not like" )
								{
									comp = ">";
									((FilterOptions)mch.FilterTextBox.Tag).ComparisonOperator = comp;
								}
								break;
							}
						}


						if( comp.ToLower() == "like" )
						{
							sql.AppendFormat( "[{0}] LIKE {2}*{1}*{2}", mch.displayMember, mch.FilterTextBox.Text, delim );
							msg.AppendFormat( "[{0}] Like {2}*{1}*{2}", mch.Text, mch.FilterTextBox.Text, delim );
						}
						else if( comp.ToLower() == "not like" )
						{
							sql.AppendFormat( "[{0}] NOT LIKE {2}*{1}*{2}", mch.displayMember, mch.FilterTextBox.Text, delim );
							msg.AppendFormat( "[{0}] Not Like {2}*{1}*{2}", mch.Text, mch.FilterTextBox.Text, delim );
						}
						else
						{
							sql.AppendFormat( "[{0}] {1} {3}{2}{3}", mch.displayMember, comp, mch.FilterTextBox.Text, delim );
							msg.AppendFormat( "[{0}] {1} {3}{2}{3}", mch.Text, comp, mch.FilterTextBox.Text, delim );
						}
					}
				}
			}

			#endregion


			#region execute sql (filter or typeahead-search)

			//using haveSql instead of sql.Length b/c above columns-loop won't clear last char from sql
			if( haveSql )
			{
				if( isFilterOperation )
				{
					try
					{
						_listView.Data.RowFilter = sql.ToString();
						UpdateStatusInfo( true, "  ( " + msg.ToString() + " )" );
					}
					catch( Exception ex )
					{
						string err = string.Format( "Error in filter expression: {0}\r\nMessage: {1}", msg.ToString(), ex.Message );
						MessageBox.Show( this, err, "Error filtering data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
					}
				}
				else	//type-ahead operation...
				{
					try
					{
						DataRow[] rows = _listView.Data.Table.Select( sql.ToString().Replace("'*", "'") );
						if( rows.Length > 0 )
						{
							ListViewItem item = _listView.Items[rows[0]];
							item.Selected = true;
							item.Focused = true;
							item.EnsureVisible();
						}

						UpdateStatusInfo( _isFiltered, "  ( " + msg.ToString().Replace("'*", "'") + " )" );
					}
					catch( Exception ex )
					{
						System.Diagnostics.Debug.WriteLine( ex );
					}//throw away the errors
				}
			}
			else
			{
				UpdateStatusInfo( _isFiltered );
			}

			#endregion
		}




		//		[ImmutableObject(true)]
		//		public IEnumerator HiddenListViewItems
		//		{
		//			get
		//			{
		//				return HiddenItems.GetEnumerator();
		//			}
		//		}

		
		public void UpdateStatusInfo()
		{
			UpdateStatusInfo( _isFiltered );
		}


		public void UpdateStatusInfo( bool isFiltered )
		{
			UpdateStatusInfo( isFiltered, string.Empty );
		}


		public void UpdateStatusInfo( bool isFiltered, string msg )
		{
			_isFiltered = isFiltered;

			stbpFilterInfo.Text = _listView.Items.Count.ToString()
				+ " Items, "
				+ (isFiltered ? "Filtered" : "Unfiltered")
				+ msg;
		}




		private void Columns_Added(object sender, EventArgs e)
		{
			InitializeColumnHeaders();
		}


		private void Columns_Removed(object sender, EventArgs e)
		{
			sColumnHeader mch = (sColumnHeader)sender;
			if( pnlFilterTextControls.Controls.Contains( mch.FilterTextBox ) )
			{
				mch.FilterTextBox.KeyPress -= new KeyPressEventHandler( this.FilterTextBox_KeyPress );
				mch.FilterTextBox.LostFocus -= new EventHandler( this.FilterTextBox_LostFocus );
				mch.FilterTextBox.TextChanged -= new EventHandler( this.FilterTextBox_TextChanged );
				pnlFilterTextControls.Controls.Remove( mch.FilterTextBox );
			}
		}


		private void Columns_Cleared(object sender, EventArgs e)
		{
			pnlFilterTextControls.Controls.Clear();
		}


		private void InitializeColumnHeaders()
		{
			_runningColWidth = 0;

			foreach( sColumnHeader mch in _listView.Columns )
			{
				mch.FilterTextBox.Top = 0;
				mch.FilterTextBox.Left = _runningColWidth + 1;
				mch.FilterTextBox.Width	= mch.Width - (_spacer*2);

				if( !pnlFilterTextControls.Controls.Contains( mch.FilterTextBox ) )
				{
					mch.FilterTextBox.ContextMenu = ctxMenu;
					mch.FilterTextBox.Tag = new FilterOptions( mch.DataType );
					mch.FilterTextBox.KeyPress += new KeyPressEventHandler( this.FilterTextBox_KeyPress );
					mch.FilterTextBox.LostFocus += new EventHandler( this.FilterTextBox_LostFocus );
					mch.FilterTextBox.TextChanged += new EventHandler( this.FilterTextBox_TextChanged );
					pnlFilterTextControls.Controls.Add( mch.FilterTextBox );
				}

				_runningColWidth += mch.Width;
			}
		}


		private void FilterTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = true;
			switch( e.KeyChar )
			{
				case (char)Keys.Return:
				{
					if( !sender.Equals( _listView ) )
					{
						ExecuteFilter();
					}
					break;
				}
				case (char)Keys.Escape:
				{
					if( (Control.ModifierKeys & Keys.Shift) == Keys.Shift )
					{
						CancelFilter();
					}
					else
					{
						if( sender as TextBox != null )
						{
							((TextBox)sender).Text = string.Empty;
						}
					}
					break;
				}
				case (char)4:	//Control-D/d	//Keys.Delete doesn't work b/c windows handles it before we get it
				{
					if( (Control.ModifierKeys & Keys.Control) == Keys.Control )
					{
						ClearFilter();
					}
					else
					{
						e.Handled = false;
					}
					break;
				}
				case (char)5:	//Control-E/e
				{
					ExecuteFilter();
					break;
				}
				case (char)6:	//Control-F/f
				{
					if( (Control.ModifierKeys & Keys.Control) == Keys.Control )
					{
						if( sender.Equals( _listView ) )
						{
							if( _lastFilterTextBox == null )
							{
								this.Columns[0].FilterTextBox.Focus();
							}
							else
							{
								_lastFilterTextBox.Focus();
							}
						}
						else
						{
							_listView.Focus();
						}
					}
					else
					{
						e.Handled = false;
					}
					break;
				}
				default:
				{
					e.Handled = false;
					break;
				}
			}
		}


		private void FilterTextBox_TextChanged(object sender, EventArgs e)
		{
			ExecuteFilter_DataView( false );
		}


		private void FilterTextBox_LostFocus(object sender, EventArgs e)
		{
			_lastFilterTextBox = (TextBox)sender;
		}


		private void tbFilterCommands_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch( e.Button.Tag.ToString().ToLower() )
			{
				case "execute":
				{
					ExecuteFilter();
					break;
				}
				case "cancel":
				{
					CancelFilter();
					break;
				}
				case "clear":
				{
					ClearFilter();
					break;
				}
				case "hide":
				{
					ShowHideFilter( false );
					break;
				}
			}

			_listView.Focus();
		}


		private void ctxMenu_Popup(object sender, EventArgs e)
		{
			TextBox t = (TextBox)ctxMenu.SourceControl;
			FilterOptions f = (FilterOptions)t.Tag;

			foreach( MenuItem item in ctxMenu.MenuItems )
			{
				item.Checked = item.Text.ToLower() == f.ComparisonOperator.ToLower() ||
					item.Text.ToLower() == f.LogicalOperator.ToLower();
			}

			mnuLike.Visible = mnuNotLike.Visible = f.DataType == TypeCode.String;
		}


		private void mnuComparisonOperator_Click(object sender, System.EventArgs e)
		{
			TextBox t = (TextBox)ctxMenu.SourceControl;
			MenuItem m = (MenuItem)sender;
			((FilterOptions)t.Tag).ComparisonOperator = m.Text;
		}


		private void mnuLogicalOperator_Click(object sender, System.EventArgs e)
		{
			TextBox t = (TextBox)ctxMenu.SourceControl;
			MenuItem m = (MenuItem)sender;
			((FilterOptions)t.Tag).LogicalOperator = m.Text;
		}


		private void cmdFilterExecute_Click(object sender, System.EventArgs e)
		{
			ExecuteFilter();
			_listView.Focus();
		}


		private void cmdFilterCancel_Click(object sender, System.EventArgs e)
		{
			CancelFilter();
			_listView.Focus();
		}


		private void cmdFilterShowHide_Click(object sender, System.EventArgs e)
		{
			ShowHideFilter( false );
		}


		private void pnlFilterCommandControls_Click(object sender, System.EventArgs e)
		{
			ShowHideFilter( true );
		}


		private void cmdFilterClear_Click(object sender, System.EventArgs e)
		{
			ClearFilter();
			_listView.Focus();
		}


		private void AvailFilter( bool avail )
		{
			if( avail )
			{
				pnlFilterTextControls.Show();
				pnlFilterCommandControls.Show();
			}
			else
			{
				pnlFilterTextControls.Hide();
				pnlFilterCommandControls.Hide();
			}

			_listView.Focus();
		}


		private void ShowHideFilter( bool show )
		{
			if( show )
			{
				pnlFilterTextControls.Show();
				pnlFilterCommandControls.Height = 22;
				pnlFilterCommandControls.BorderStyle = BorderStyle.Fixed3D;
				pnlFilterCommandControls.BackColor = Color.FromKnownColor( KnownColor.Control );
				_ShowHodeToolTip.Active = false;
			}
			else
			{
				pnlFilterTextControls.Hide();
				pnlFilterCommandControls.Height = 3;
				pnlFilterCommandControls.BorderStyle = BorderStyle.None;
				pnlFilterCommandControls.BackColor = Color.FromKnownColor( KnownColor.ControlDarkDark );
				_ShowHodeToolTip.Active = true;
			}


			for( int n=0; n<pnlFilterCommandControls.Controls.Count; n++ )
			{
				pnlFilterCommandControls.Controls[n].Visible = show;
			}

			_listView.Focus();
		}


		private void EnableFilter( bool enable )
		{
			pnlFilterTextControls.Enabled = enable;
			pnlFilterCommandControls.Enabled = enable;
			_listView.Focus();
		}


		private void lvListView_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
//			string direction = "ASC";
//			if( _listView.Sorting != SortOrder.Ascending )
//			{
//				_listView.Sorting = SortOrder.Ascending;
//			}
//			else
//			{
//				_listView.Sorting = SortOrder.Descending;
//				direction = "DESC";
//			}
//
//			_listView.ListViewItemSorter = new ColumnSorter( e.Column, _listView.Sorting );
//			_lastColumn = e.Column;
//
//			if( _filterAsDataView )
//			{
//				string sort = string.Format( "[{0}] {1}", this.Columns[e.Column].displayMember, direction );
//
//				_unfilteredView.Table.DefaultView.Sort = sort;
//				_currentView.Table.DefaultView.Sort = sort;
//				_unfilteredView.Sort = sort;
//				_currentView.Sort = sort;
//			}
		}


		int cc = 0;
		private void tmrEventSnyc_Tick(object sender, System.EventArgs e)
		{
			if( pnlFilterTextControls.Controls.Count == this.Columns.Count )
			{
				_runningColWidth = 0;
				foreach( sColumnHeader mch in this.Columns )
				{
					if( mch.Width != mch.FilterTextBox.Width - (_spacer*2))
					{
						mch.FilterTextBox.Left	= _runningColWidth + _spacer;
						mch.FilterTextBox.Width	= mch.Width - (_spacer*2);
					}
					_runningColWidth += mch.Width;
				}
			}
			else
			{
				_initCount++;
				InitializeColumnHeaders();

				//this is just a debugging tool to test if the call here ever happens more than once (it shouldn't)
				if( !this.DesignMode && _initCount > 1 && cc < 3 )
				{
					cc++;
					MessageBox.Show( string.Format("Did tmrSync_InitializeColumnHeaders() on {0}\r\nColumns: {1}, FilterTextBoxes: {2}",
						this.Name, this.InnerListView.Columns.Count, this.pnlFilterTextControls.Controls.Count ) );
				}
			}
		}


		private void CreateToolTips()
		{
			//			CreateToolTip( this.cmdFilterExecute, "Execute Filter (Enter)" );
			//			CreateToolTip( this.cmdFilterCancel, "Cancel Filter (Shift-Esc)" );
			//			CreateToolTip( this.cmdFilterClear, "Clear Filter Controls" );
			//			CreateToolTip( this.cmdFilterShowHide, "Hide Filter Controls" );
			
			_ShowHodeToolTip = CreateToolTip( this.pnlFilterCommandControls, "Click to Show Filter Controls" );
			_ShowHodeToolTip.Active = false;
		}


		private ToolTip CreateToolTip( Control control, string toolTip )
		{
			ToolTip t = new ToolTip();

			// Set up the delays for the ToolTip.
			t.AutoPopDelay = 5000;
			t.InitialDelay = 5;
			t.ReshowDelay = 5;

			// Force the ToolTip text to be displayed whether or not the form is active.
			t.ShowAlways = true;
      
			t.SetToolTip( control, toolTip );

			return t;
		}


		#endregion



		#region hard part (everything here is obsolete)

		[Obsolete("databound now", true)]
		private DataTable CreateLinkSource()
		{
			DataTable t = new DataTable();
			foreach( sColumnHeader c in _listView.Columns )
			{
				t.Columns.Add( c.displayMember );
			}

			DataRow r = null;
			foreach( ListViewItem item in _listView.Items )
			{
				r = t.NewRow();
				for(int i = 0; i < item.SubItems.Count; i++)
				{
					r[i] = item.SubItems[i].Text;
				}
				t.Rows.Add( r );

//				_rowsToItems.Add( r.GetHashCode(), item );
//				_itemsToRows.Add( item.GetHashCode(), r.GetHashCode() );
			}
			t.AcceptChanges();

//			_currentView = new DataView( t );

			return t;
		}


		[Obsolete("databound now", true)]
		private void UpdateLinkSource()
		{
////////			DataTable t = _unfilteredView.Table;
//////			DataRow r = null;
//////			foreach( ListViewItem item in _listView.Items )
//////			{
//////				if( !_itemsToRows.ContainsKey( item.GetHashCode() ) )
//////				{
//////					r = t.NewRow();
//////					for(int i = 0; i < item.SubItems.Count; i++)
//////					{
//////						r[i] = item.SubItems[i].Text;
//////					}
//////
//////					t.Rows.Add( r );
////////					_rowsToItems.Add( r.GetHashCode(), item );
////////					_itemsToRows.Add( item.GetHashCode(), r.GetHashCode() );
//////				}
//////			}
//////			t.AcceptChanges();
		}


		[Obsolete("databound now", true)]
		private void Fill(DataView data, bool suppressEvents)
		{
//			this.Items.SuppressEvents = suppressEvents;
//			this.Items.Clear();
//			this._rowsToItems.Clear();
//			this._itemsToRows.Clear();

			IComparer lvis = _listView.ListViewItemSorter;
			_listView.ListViewItemSorter = null;


			int n=0;

			#region try
			try
			{
				sColumnHeader col = null;
				for( int r=0; r<data.Count; r++ )
				{
					n = 0;
					col = this.Columns[n];
					string itemText = MiscUtils.Format( data[r][col.displayMember], col.DataType, col.FormatString );
					ListViewItem item = this.Items.Add( itemText );
					if( col.HasValueMember )
					{
						item.Tag = data[r][col.ValueMember];
					}

					n++;
					for( ; n<this.Columns.Count; n++ )
					{
						col = this.Columns[n];
						itemText = MiscUtils.Format( data[r][col.displayMember], col.DataType, col.FormatString );
						item.SubItems.Add( itemText );
						if( col.HasValueMember )
						{
							item.Tag = data[r][col.ValueMember];
						}
					}

					if( _listView.sListViewItemAccessor != null )
					{
						_listView.sListViewItemAccessor.SetProperties( (sListViewItem)item, data[r] );
					}

//////					_rowsToItems.Add( data[r].GetHashCode(), item );
//////					_itemsToRows.Add( item.GetHashCode(), data[r].GetHashCode() );
				}
			}
				#endregion
				#region catch
			catch( Exception ex )
			{
				System.Text.StringBuilder msg = new System.Text.StringBuilder();

				msg.AppendFormat( "{0}\r\n", ex.Message );

				msg.AppendFormat( "\r\n{0}.{1}.Columns ({2})", this.Parent.Name, this.Name, this.Columns.Count );
				if( this.Columns.Count > 0 )
				{
					msg.AppendFormat( ": {0}", this.Columns[0].displayMember );
					n=1;
					for( ; n<this.Columns.Count; n++ )
					{
						msg.AppendFormat( ", {0}", this.Columns[n].displayMember );
					}
				}

				msg.AppendFormat( "\r\n\r\nData.Columns ({0})", data.Table.Columns.Count );
				if( data.Table.Columns.Count > 0 )
				{
					msg.AppendFormat( ": {0}", data.Table.Columns[0].ColumnName );
					n=1;
					for( ; n<data.Table.Columns.Count; n++ )
					{
						msg.AppendFormat( ", {0}", data.Table.Columns[n].ColumnName );
					}
				}

				throw new Exception( msg.ToString(), ex );
			}
			#endregion


			if( lvis != null )
			{
				_listView.ListViewItemSorter = lvis;
			}

//			this.Items.SuppressEvents = false;
		}


		[Obsolete("databound now", true)]
		private void Items_ItemsSyncReady(object sender, EventArgs e)
		{
//////			//create data-to-items link, if necc
//////			if( _unfilteredView == null )
//////			{
//////				DataTable t = CreateLinkSource();
//////				_unfilteredView = new DataView( t );
//////				UpdateStatusInfo( _isFiltered, " Items_ItemsSyncReady: Create" );
//////			}
//////			else
//////			{
//////				UpdateLinkSource();
//////				UpdateStatusInfo( _isFiltered, " Items_ItemsSyncReady: Update" );
//////			}
		}


		[Obsolete("databound now", true)]
		private void ExecuteFilter_ListItems()
		{
			string thistext = null;
			string srchtext = null;
			foreach( ListViewItem lvItem in _listView.Items )
			{
				bool keep = true;
				for(int i = 0; i < lvItem.SubItems.Count; i++)
				{
					thistext = null;
					srchtext = null;

					//.ToLower(): use case insensitivity while searching
					thistext = lvItem.SubItems[i].Text.ToLower();
					srchtext = this.Columns[i].FilterTextBox.Text.ToLower();

					if( srchtext != null && srchtext.Length > 0 )
					{
						//int n = thistext.IndexOf( srchtext );
						if( thistext.IndexOf( srchtext ) < 0 )
						{
							keep = false;
						}
					}
				}

				if( !keep )
				{
//					_hiddenItems.Add( lvItem );
					lvItem.Remove();
				}
			}

			UpdateStatusInfo( true );
		}


		[Obsolete("databound now", true)]
		private void ExecuteSearch_ListItems()
		{
			string thistext = null;
			string srchtext = null;
			foreach( ListViewItem lvItem in _listView.Items )
			{
				bool found = true;
				for(int i = 0; i < lvItem.SubItems.Count; i++)
				{
					//.ToLower(): use case insensitivity while searching
					thistext = lvItem.SubItems[i].Text.ToLower();
					srchtext = this.Columns[i].FilterTextBox.Text.ToLower();

					if( srchtext != null && srchtext.Length > 0 )
					{
						found = thistext.StartsWith( srchtext );
						if( !found ) break;
					}
				}

				if( found )
				{
					lvItem.Selected = true;
					lvItem.EnsureVisible();
					break;
				}
			}
		}


		#endregion



		#endregion


		#region Wrappers

		#region Properties

		[DefaultValue(false)]
		public bool RebuildColumnsOnSetDataSource
		{
			get
			{
				return _listView.RebuildColumnsOnSetDataSource;
			}
			set
			{
				_listView.RebuildColumnsOnSetDataSource = value;
			}
		}


		[Browsable(false)]
		public object DataSource
		{
			get
			{
				return _listView.DataSource;
			}
			set
			{
				_listView.DataSource = value;
				this.UpdateStatusInfo( false );
			}
		}


		[Browsable(false)]
		public DataView Data
		{
			get
			{
				return _listView.Data;
			}
		}


		[Browsable(false)]
		public IListViewItemAccessor sListViewItemAccessor
		{
			get
			{
				return _listView.sListViewItemAccessor;
			}
			set
			{
				_listView.sListViewItemAccessor = value;
			}
		}


		[DefaultValue(ItemActivation.Standard)]
		public ItemActivation Activation
		{
			get
			{
				return _listView.Activation;
			}
			set
			{
				_listView.Activation = value;
			}
		}


		[DefaultValue(ListViewAlignment.Top)]
		public ListViewAlignment Alignment
		{
			get
			{
				return _listView.Alignment;
			}
			set
			{
				_listView.Alignment = value;
			}
		}


		[DefaultValue(false)]
		public bool AllowColumnReorder
		{
			get
			{
				return _listView.AllowColumnReorder;
			}
			set
			{
				_listView.AllowColumnReorder = value;
			}
		}


		[DefaultValue(true)]
		public bool AutoArrange
		{
			get
			{
				return _listView.AutoArrange;
			}
			set
			{
				_listView.AutoArrange = value;
			}
		}


		public override Color BackColor
		{
			get
			{
				return _listView.BackColor;
			}
			set
			{
				_listView.BackColor = value;
			}
		}


		public override Image BackgroundImage
		{
			get
			{
				return _listView.BackgroundImage;
			}
			set
			{
				_listView.BackgroundImage = value;
			}
		}


		[DefaultValue(BorderStyle.Fixed3D)]
		public BorderStyle BorderStyle
		{
			get
			{
				return _listView.BorderStyle;
			}
			set
			{
				_listView.BorderStyle = value;
			}
		}


		[DefaultValue(false)]
		public bool CheckBoxes
		{
			get
			{
				return _listView.CheckBoxes;
			}
			set
			{
				_listView.CheckBoxes = value;
			}
		}


		[Browsable(false)]
		public ListView.CheckedIndexCollection CheckedIndices
		{
			get
			{
				return _listView.CheckedIndices;
			}
		}


		[Browsable(false)]
		public ListView.CheckedListViewItemCollection CheckedItems
		{
			get
			{
				return _listView.CheckedItems;
			}
		}


		//TODO: Override the ColumnHeaderCollection and ditch the ColumnHeaderAdd junk.
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public sListView.sColumnHeaderCollection Columns
		{
			get
			{
				return _listView.Columns;
			}
		}


		public ListViewItem FocusedItem
		{
			get
			{
				return _listView.FocusedItem;
			}
		}


		//		public override Font Font
		//		{
		//			get
		//			{
		//				return _listView.Font;
		//			}
		//			set
		//			{
		//				_listView.Font = value;
		//			}
		//		}


		public override Color ForeColor
		{
			get
			{
				return _listView.ForeColor;
			}
			set
			{
				_listView.ForeColor = value;
			}
		}


		[DefaultValue(true)]
		public bool FullRowSelect
		{
			get
			{
				return _listView.FullRowSelect;
			}
			set
			{
				_listView.FullRowSelect = value;
			}
		}


		[DefaultValue(false)]
		public bool GridLines
		{
			get
			{
				return _listView.GridLines;
			}
			set
			{
				_listView.GridLines = value;
			}
		}


		[DefaultValue(ColumnHeaderStyle.Clickable)]
		public ColumnHeaderStyle HeaderStyle
		{
			get
			{
				return _listView.HeaderStyle;
			}
			set
			{
				_listView.HeaderStyle = value;
			}
		}


		[DefaultValue(false)]
		public bool HideSelection
		{
			get
			{
				return _listView.HideSelection;
			}
			set
			{
				_listView.HideSelection = value;
			}
		}


		[DefaultValue(false)]
		public bool HoverSelection
		{
			get
			{
				return _listView.HoverSelection;
			}
			set
			{
				_listView.HoverSelection = value;
			}
		}


		[Browsable(false)]
		public sListView.sListViewItemCollection Items
		{
			get
			{
				return _listView.Items;
			}
		}


		[DefaultValue(false)]
		public bool LabelEdit
		{
			get
			{
				return _listView.LabelEdit;
			}
			set
			{
				_listView.LabelEdit = value;
			}
		}


		[DefaultValue(true)]
		public bool LabelWrap
		{
			get
			{
				return _listView.LabelWrap;
			}
			set
			{
				_listView.LabelWrap = value;
			}
		}


		[DefaultValue(null)]
		public ImageList LargeImageList
		{
			get
			{
				return _listView.LargeImageList;
			}
			set
			{
				_listView.LargeImageList = value;
			}
		}


		[Browsable(false)]
		public IComparer ListViewItemSorter
		{
			get
			{
				return _listView.ListViewItemSorter;
			}
			set
			{
				_listView.ListViewItemSorter = value;
			}
		}


		[DefaultValue(false)]
		public bool MultiSelect
		{
			get
			{
				return _listView.MultiSelect;
			}
			set
			{
				_listView.MultiSelect = value;
			}
		}


		[DefaultValue(true)]
		public bool Scrollable
		{
			get
			{
				return _listView.Scrollable;
			}
			set
			{
				_listView.Scrollable = value;
			}
		}


		[Browsable(false)]
		public ListView.SelectedIndexCollection SelectedIndices
		{
			get
			{
				return _listView.SelectedIndices;
			}
		}


		[Browsable(false)]
		public ListView.SelectedListViewItemCollection SelectedItems
		{
			get
			{
				return _listView.SelectedItems;
			}
		}


		[DefaultValue(null)]
		public ImageList SmallImageList
		{
			get
			{
				return _listView.SmallImageList;
			}
			set
			{
				_listView.SmallImageList = value;
			}
		}


		[DefaultValue(SortOrder.None)]
		public SortOrder Sorting
		{
			get
			{
				return _listView.Sorting;
			}
			set
			{
				_listView.Sorting = value;
			}
		}


		[DefaultValue(null)]
		public ImageList StateImageList
		{
			get
			{
				return _listView.StateImageList;
			}
			set
			{
				_listView.StateImageList = value;
			}
		}


		public override string Text
		{
			get
			{
				return _listView.Text;
			}
			set
			{
				_listView.Text = value;
			}
		}


		[Browsable(false)]
		public ListViewItem TopItem
		{
			get
			{
				return _listView.TopItem;
			}
		}


		[DefaultValue(View.Details)]
		public View View
		{
			get
			{
				return _listView.View;
			}
			set
			{
				_listView.View = value;
			}
		}


		[DefaultValue(OperateLevel.None)]
		public OperateLevel OperateLevel
		{
			get
			{
				return _listView.OperateLevel;
			}
			set
			{
				_listView.OperateLevel = value;
			}
		}


		public string DataKeys
		{
			get { return _listView.DataKeys; }
			set { _listView.DataKeys = value; }
		}


		#endregion


		#region Methods

		public void RefreshColumnWidths()
		{
			_listView.RefreshColumnWidths();
		}


		public void SuspendDataBinding()
		{
			_listView.SuspendDataBinding();
		}
		public void ResumeDataBinding()
		{
			_listView.ResumeDataBinding();
		}
		public void SuspendItemBinding()
		{
			_listView.SuspendItemBinding();
		}
		public void ResumeItemBinding()
		{
			_listView.ResumeItemBinding();
		}


		public void ArrangeIcons()
		{
			_listView.ArrangeIcons();
		}


		public void ArrangeIcons(ListViewAlignment value)
		{
			_listView.ArrangeIcons( value );
		}


		public void BeginUpdate()
		{
			_listView.BeginUpdate();
		}


		public void Clear()
		{
			_listView.Clear();
		}


		public void EndUpdate()
		{
			_listView.EndUpdate();
		}


		public void EnsureVisible(int index)
		{
			_listView.EnsureVisible( index );
		}


		public ListViewItem GetItemAt(int x, int y)
		{
			return _listView.GetItemAt( x, y );
		}


		public Rectangle GetItemRect(int index)
		{
			return _listView.GetItemRect( index );
		}


		public Rectangle GetItemRect(int index, ItemBoundsPortion portion)
		{
			return _listView.GetItemRect( index, portion );
		}


		public void Sort()
		{
			_listView.Sort();
		}


		#endregion


		#region Events

		public event LabelEditEventHandler AfterLabelEdit;

		public event LabelEditEventHandler BeforeLabelEdit;

		new public event EventHandler Click;
		
		public event ColumnClickEventHandler ColumnClick;

		new public event EventHandler DoubleClick;

		public event EventHandler ItemActivate;

		public event ItemCheckEventHandler ItemCheck;

		public event ItemDragEventHandler ItemDrag;

		public event EventHandler SelectedIndexChanged;

		private void InitializeWrappedEvents()
		{
			_listView.AfterLabelEdit += new LabelEditEventHandler(_listView_AfterLabelEdit);
			_listView.BeforeLabelEdit += new LabelEditEventHandler(_listView_BeforeLabelEdit);
			_listView.Click += new EventHandler(_listView_Click);
			_listView.ColumnClick += new ColumnClickEventHandler(_listView_ColumnClick);
			_listView.DoubleClick += new EventHandler(_listView_DoubleClick);
			_listView.ItemActivate += new EventHandler(_listView_ItemActivate);
			_listView.ItemCheck += new ItemCheckEventHandler(_listView_ItemCheck);
			_listView.ItemDrag += new ItemDragEventHandler(_listView_ItemDrag);
			_listView.SelectedIndexChanged += new EventHandler(_listView_SelectedIndexChanged);
		}


		protected void OnAfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			if( AfterLabelEdit != null )
			{
				AfterLabelEdit( sender, e );
			}
		}


		protected void OnBeforeLabelEdit(object sender, LabelEditEventArgs e)
		{
			if( BeforeLabelEdit != null )
			{
				BeforeLabelEdit( sender, e );
			}
		}


		protected void OnClick(object sender, EventArgs e)
		{
			if( Click != null )
			{
				Click( sender, e );
			}
		}


		protected void OnColumnClick(object sender, ColumnClickEventArgs e)
		{
			if( ColumnClick != null )
			{
				ColumnClick( sender, e );
			}
		}


		protected void OnDoubleClick(object sender, EventArgs e)
		{
			if( DoubleClick != null )
			{
				DoubleClick( sender, e );
			}
		}


		protected void OnItemActivate(object sender, EventArgs e)
		{
			if( ItemActivate != null )
			{
				ItemActivate( sender, e );
			}
		}


		protected void OnItemCheck(object sender, ItemCheckEventArgs e)
		{
			if( ItemCheck != null )
			{
				ItemCheck( sender, e );
			}
		}


		protected void OnItemDrag(object sender, ItemDragEventArgs e)
		{
			if( ItemDrag != null )
			{
				ItemDrag( sender, e );
			}
		}


		protected void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			if( SelectedIndexChanged != null )
			{
				SelectedIndexChanged( sender, e );
			}
		}


		private void _listView_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			OnAfterLabelEdit( sender, e );
		}


		private void _listView_BeforeLabelEdit(object sender, LabelEditEventArgs e)
		{
			OnBeforeLabelEdit( sender, e );
		}


		private void _listView_Click(object sender, EventArgs e)
		{
			OnClick( sender, e );
		}


		private void _listView_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			OnColumnClick( sender, e );
		}


		private void _listView_DoubleClick(object sender, EventArgs e)
		{
			OnDoubleClick( sender, e );
		}


		private void _listView_ItemActivate(object sender, EventArgs e)
		{
			OnItemActivate( sender, e );
		}


		private void _listView_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			OnItemCheck( sender, e );
		}


		private void _listView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			OnItemDrag( sender, e );
		}


		private void _listView_SelectedIndexChanged(object sender, EventArgs e)
		{
			OnSelectedIndexChanged( sender, e );
		}


		#endregion

		#endregion


		public enum FilterControlsVisibleState
		{
			Visible,
			Collapsed,
			Hidden
		}
	}


	#region utility classes

	class FilterOptions
	{
		internal string ComparisonOperator = "LIKE";
		internal string LogicalOperator = "AND";
		internal TypeCode DataType = TypeCode.String;

		public FilterOptions(){}

		public FilterOptions( TypeCode dataType )
		{
			DataType = dataType;
			if( dataType != TypeCode.String )
			{
				ComparisonOperator = ">=";
			}
		}

		public override string ToString()
		{
			return ComparisonOperator + "|" + LogicalOperator;
		}

	}


	class ColumnSorter : IComparer
	{
		private int currentColumn	= 0;
		private int multiplier		= 1;

		public ColumnSorter(int CurrentColumn, SortOrder NewSortOrder)
		{
			currentColumn = CurrentColumn;
			
			if( NewSortOrder == SortOrder.Descending )
			{
				multiplier = -1;
			}
		}


		public int Compare(object x, object y)
		{
			//compare first item to second item, true=ignore case
			//result: 1:a>b, 0:a=b, -1:a<b; mutliply by -1 to reverse result
			return String.Compare(((ListViewItem)x).SubItems[currentColumn].Text,
				((ListViewItem)y).SubItems[currentColumn].Text, true) * multiplier; 
		}
	}


	internal delegate void ListViewItemCollectionEventHandler(object sender, ListViewItemCollectionEventArgs e);

	internal class ListViewItemCollectionEventArgs : EventArgs
	{
		private ListViewItem _item = null;

		new public static readonly ListViewItemCollectionEventArgs Empty;
		static ListViewItemCollectionEventArgs()
		{
			Empty = new ListViewItemCollectionEventArgs();
		}

		public ListViewItemCollectionEventArgs()
		{
		}

		public ListViewItemCollectionEventArgs(ListViewItem item)
		{
			_item = item;
		}


		public ListViewItem ListViewItem
		{
			get
			{
				return _item;
			}
		}
	}


	#endregion
}