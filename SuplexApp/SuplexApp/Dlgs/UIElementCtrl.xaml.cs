using System;
using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Suplex.Wpf;
using Suplex.Security;
using ss = Suplex.Security;
using api = Suplex.Forms.ObjectModel.Api;
using sf = Suplex.Forms;
using sg = Suplex.General;
using SuplexApp.Controls;


namespace SuplexApp
{
	/// <summary>
	/// Interaction logic for UIElementCtrl.xaml
	/// </summary>
	public partial class UIElementCtrl : SplxUserControl, IEditorControl<api.UIElement>
	{
		private api.SuplexStore _splxStore = new api.SuplexStore();

		private RightRoleDlg _rrDlg = null;
		private RightRoleRuleDlg _rrrDlg = null;
		private api.AceCollection _daclAces = null;
		private api.AuditAceCollection _saclAces = null;
		private api.RightRoleCollection _rightRoles = null;
		private api.RightRoleRuleCollection _rightRoleRules = null;
		//private ss.AuditType _auditTypeFilter;

		private bool _suppressAuditFilterSelectionChanged = false;

		#region Events
		public event EventHandler SavedChanges;
		public event EventHandler CancelledChanges;

		protected void OnSavedChanges(object sender, EventArgs e)
		{
			if( SavedChanges != null )
			{
				this.SavedChanges( sender, e );
			}
		}
		protected void OnCancelledChanges(object sender, EventArgs e)
		{
			if( CancelledChanges != null )
			{
				this.CancelledChanges( sender, e );
			}
		}
		#endregion

		public UIElementCtrl()
		{
			InitializeComponent();

			_rrDlg = new RightRoleDlg();
			_rrrDlg = new RightRoleRuleDlg();

			this.Validation.ValidationSummaryControl = vs;
			this.Validation.AutoValidateContainer = true;
			this.Validation.Load( new StringReader( Properties.Resources.uie ) );

			this.Security.Descriptor.Dacl.Add( 0, new UIAce( UIRight.FullControl, true ) );
			this.Security.Apply( AceType.Native );

			this.BuildCombos();
		}

		public bool SplxStoreHasGroups
		{
			get
			{
				return true; //this.SplxStore.Groups.Count > 0;
			}
		}

		public static readonly DependencyProperty SplxGroupsProperty = DependencyProperty.Register( "SplxGroups", typeof( api.GroupCollection ), typeof( UIElementCtrl ) );
		public api.GroupCollection SplxGroups
		{
			get { return GetValue( SplxGroupsProperty ) as api.GroupCollection; }
			set { SetValue( SplxGroupsProperty, value ); }
		}

		public void SetDataContext(api.UIElement uie)
		{
			cmbControlType.SelectedValue = "";

			uieEditor.DataContext = uie;

			//uie.SecurityDescriptor.Dacl.Resolve( this.SplxStore );
			//uie.SecurityDescriptor.Sacl.Resolve( this.SplxStore );
			_daclAces = uie.SecurityDescriptor.Dacl;
			_saclAces = uie.SecurityDescriptor.Sacl;

			foreach( api.AccessControlEntryBase ace in _daclAces )
			{
				ace.IsDirty = false;
			}
			foreach( api.AccessControlEntryAuditBase ace in _saclAces )
			{
				ace.IsDirty = false;
			}

			////lvPerms.ItemsSource = _daclAces;
			////lvAudit.ItemsSource = _saclAces;

			_rightRoles = uie.SecurityDescriptor.RightRoles;
			//lvRr.ItemsSource = _rightRoles;

			_rightRoleRules = uie.SecurityDescriptor.RightRoleRules;
			//tvRightRoleRules.DataContext = uie.SecurityDescriptor.iRightRoleRules;
			//tvRightRoleRules.ItemsSource = _rightRoleRules;

			tlvRB.DataContext = uie.SecurityDescriptor.RightBindings;

			this.ResolveRightRoleRules( _rightRoleRules, uie );

			this.AuditTypeFilter = uie.SecurityDescriptor.SaclAuditTypeFilter;

			this.SourceObject = uie;

			this.SourceObject.IsDirty = false;

			uie.SecurityDescriptor.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler( SecurityDescriptor_PropertyChanged );
		}

		void SecurityDescriptor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			this.SourceObject.IsDirty = true;
		}

		private void BuildCombos()
		{
			cmbControlType.ItemsSource = UIElementDlg.ControlTypes.Keys;

			ArrayList names = new ArrayList( Enum.GetValues( typeof( TypeCode ) ) );
			names.Sort();
			cmbDataType.ItemsSource = names;

			ArrayList auditTypes = new ArrayList( Enum.GetValues( typeof( ss.AuditType ) ) );
			foreach( ss.AuditType auditType in auditTypes )
			{
				CheckBox cb = new CheckBox();
				cb.Content = auditType;
				cb.Checked += new RoutedEventHandler( AuditFilter_CheckChanged );
				cb.Unchecked += new RoutedEventHandler( AuditFilter_CheckChanged );
				lstAuditFilter.Items.Add( cb );
			}
		}

		private void uieEditor_Loaded(object sender, RoutedEventArgs e)
		{
			if( this.SourceObject != null )
			{
				this.SourceObject.IsDirty = false;
			}
		}

		private void txtName_LostFocus(object sender, RoutedEventArgs e)
		{
			if( !string.IsNullOrEmpty( txtName.Text ) && string.IsNullOrEmpty( txtUniqueName.Text ) )
			{
				txtUniqueName.Text = txtName.Text;
			}
		}

		private void uieEditor_KeyDown(object sender, KeyEventArgs e)
		{
			if( e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control )
			{
				this.SaveChanges();
			}
			else if( e.Key == Key.Escape )
			{
				this.OnCancelledChanges( this, EventArgs.Empty );
			}
		}

		private void cmdOk_Click(object sender, RoutedEventArgs e)
		{
			this.SaveChanges();
		}

		private void cmdCancel_Click(object sender, RoutedEventArgs e)
		{
			this.OnCancelledChanges( this, EventArgs.Empty );
		}


		#region IEditorControl Members
		private long _nextId = System.DateTime.Now.Ticks;
		private long NextId { get { return _nextId++; } }
		api.AceUtil _aceUtil = new api.AceUtil();
		public bool SaveChanges()
		{
			vs.Reset();
			this.ProcessValidate( true );
			//List<sf.ValidationResult> results = ( (sf.ValidationContainerAccessor)this.Validation ).Results;
			//vs.DataContext = ((sf.ValidationContainerAccessor)this.Validation).Results;

			bool success = ((sf.ValidationContainerAccessor)this.Validation).ChildValidationSuccess;

			if( success )
			{
				success = this.ValidateAndConvertAces();
			}

			if( success )
			{
				this.SourceObject.IsDirty = false;

				this.OnSavedChanges( this, EventArgs.Empty );
			}

			return success;
		}

		public sf.IObjectModel SourceObject { get; set; }

		public api.SuplexStore SplxStore
		{
			get { return _splxStore; }
			set
			{
				_splxStore = value;

				this.SplxGroups = _splxStore.Groups;

				_rrDlg.SplxStore = _splxStore;
				_rrrDlg.SplxStore = _splxStore;
			}
		}
		public api.SuplexApiClient ApiClient { get; set; }
		#endregion

		#region RightBindings
		#region tlvRB Context Menu handlers: Not in use
		private void ctxRightBindingTree_Opened(object sender, RoutedEventArgs e)
		{

		}

		private void AddNewRightBinding_Click(object sender, RoutedEventArgs e)
		{

		}

		private void ExpandAllRightBinding_Click(object sender, RoutedEventArgs e)
		{

		}

		private void CollapseAllRightBinding_Click(object sender, RoutedEventArgs e)
		{

		}

		private void DeleteRightBinding_Click(object sender, RoutedEventArgs e)
		{

		}

		private void mnuEditRightBinding_Click(object sender, RoutedEventArgs e)
		{

		}
		#endregion

		private void ResolveRightRoleRules(api.RightRoleRuleCollection rules, sf.IObjectModel parent)
		{
			foreach( api.RightRoleRule r in rules )
			{
				r.ParentObject = parent;

				this.ResolveRightRoleRules( r.RightRoleRules, r );
			}
		}

		private void tlvRB_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if( tlvRB.SelectedItem is api.RightRoleRule )
			{
				api.RightRoleRule rrr = tlvRB.SelectedItem as api.RightRoleRule;

				cmdRightBindingAddCondition.IsEnabled = true;
				cmdRightBindingAddCondition.Content = string.Format( "Add {0}Condition", rrr.IsSealed ? "Root " : string.Empty );

				cmdRightBindingAddElseCondition.IsEnabled = cmdRightBindingAddElseRole.IsEnabled =
					cmdRightBindingDel.IsEnabled = cmdRightBindingEdit.IsEnabled = !rrr.IsSealed;

				cmdRightBindingAddRole.IsEnabled = true;
			}
			else if( tlvRB.SelectedItem is api.RightRole )
			{
				api.RightRole rr = tlvRB.SelectedItem as api.RightRole;

				cmdRightBindingAddCondition.IsEnabled =
					cmdRightBindingAddElseCondition.IsEnabled = cmdRightBindingAddElseRole.IsEnabled = false;
				cmdRightBindingDel.IsEnabled = cmdRightBindingEdit.IsEnabled = true;

				cmdRightBindingAddRole.IsEnabled = false;
			}
		}

		private void cmdRightBindingAddCondition_Click(object sender, RoutedEventArgs e)
		{
			api.RightRoleRule rule = new api.RightRoleRule();
			rule.LogicRuleType = ((Button)sender).Name == cmdRightBindingAddCondition.Name ?
				sf.LogicRuleType.RightRoleIf : sf.LogicRuleType.RightRoleElse;
			_rrrDlg.InitDlg( rule );
			_rrrDlg.ShowDialog();

			if( _rrrDlg.Success )
			{
				api.RightRoleRuleCollection ruleColl = null;
				if( tlvRB.SelectedItem == null || ((api.RightRoleRule)tlvRB.SelectedItem).IsSealed )
				{
					ruleColl = _rightRoleRules;
				}
				else
				{
					api.RightRoleRule rrr = tlvRB.SelectedItem as api.RightRoleRule;
					ruleColl = rrr.RightRoleRules;
				}

				if( ((Button)sender).Name == cmdRightBindingAddCondition.Name )
				{
					ruleColl.Add( _rrrDlg.RightRoleRule );
				}
				else
				{
					((api.RightRoleRule)tlvRB.SelectedItem).ElseRules.Add( _rrrDlg.RightRoleRule );
				}

				((api.UIElement)this.SourceObject).SecurityDescriptor.UpdateCompositeRightBindingCounts();

				this.SourceObject.IsDirty = true;
			}
		}

		private void cmdRightBindingAddRole_Click(object sender, RoutedEventArgs e)
		{
			_rrDlg.InitDlg( null );
			_rrDlg.ShowDialog();
			if( _rrDlg.Success )
			{
				api.RightRoleRule rrr = tlvRB.SelectedItem as api.RightRoleRule;
				if( ((Button)sender).Name == cmdRightBindingAddRole.Name )
				{
					rrr.RightRoles.Add( _rrDlg.RightRole );
				}
				else
				{
					_rrDlg.RightRole.RightRoleType = sf.RightRoleType.Else;
					rrr.ElseRoles.Add( _rrDlg.RightRole );
				}

				((api.UIElement)this.SourceObject).SecurityDescriptor.UpdateCompositeRightBindingCounts();

				this.SourceObject.IsDirty = true;
			}
		}

		private void cmdRightBindingEdit_Click(object sender, RoutedEventArgs e)
		{
			if( tlvRB.SelectedItem is api.RightRoleRule )
			{
				api.RightRoleRule clone = ((api.RightRoleRule)tlvRB.SelectedItem).Clone();
				_rrrDlg.InitDlg( clone );
				_rrrDlg.ShowDialog();
				if( _rrrDlg.Success )
				{
					((api.RightRoleRule)tlvRB.SelectedItem).Synchronize( clone );
					this.SourceObject.IsDirty = true;
				}
			}
			else if( tlvRB.SelectedItem is api.RightRole )
			{
				api.RightRole rr = ((api.RightRole)tlvRB.SelectedItem).CloneMemberwise();
				_rrDlg.InitDlg( rr );
				bool? ok = _rrDlg.ShowDialog();
				if( _rrDlg.Success && rr.IsDirty )
				{
					((api.RightRole)tlvRB.SelectedItem).Synchronize( rr );
					this.SourceObject.IsDirty = true;
				}
			}
		}

		private void tlvRB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if( tlvRB.SelectedItem is api.RightRole )
			{
				api.RightRole rr = ((api.RightRole)tlvRB.SelectedItem).CloneMemberwise();
				_rrDlg.InitDlg( rr );
				bool? ok = _rrDlg.ShowDialog();
				if( _rrDlg.Success && rr.IsDirty )
				{
					((api.RightRole)tlvRB.SelectedItem).Synchronize( rr );
					this.SourceObject.IsDirty = true;
				}
			}
		}

		private void cmdRightBindingDel_Click(object sender, RoutedEventArgs e)
		{
			if( tlvRB.SelectedItem is api.INotifyDeleted )
			{
				((api.INotifyDeleted)tlvRB.SelectedItem).IsDeleted = true;

				this.SourceObject.IsDirty = true;
			}

			((api.UIElement)this.SourceObject).SecurityDescriptor.UpdateCompositeRightBindingCounts();
		}
		#endregion

		#region Inheritance Settings
		private ss.AuditType AuditTypeFilter
		{
			get
			{
				ss.AuditType filter = (ss.AuditType)0;
				foreach( CheckBox cb in lstAuditFilter.Items )
				{
					if( cb.IsChecked == true )
					{
						filter |= (ss.AuditType)cb.Content;
					}
				}
				return filter;
			}
			set
			{
				foreach( CheckBox cb in lstAuditFilter.Items )
				{
					cb.IsChecked =
						(value & (ss.AuditType)cb.Content) == (ss.AuditType)cb.Content;
				}
			}
		}

		private void lstAuditFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if( !_suppressAuditFilterSelectionChanged && ((ListBox)sender).SelectedItem != null )
			{
				CheckBox cb = (CheckBox)((ListBox)sender).SelectedItem;
				if( cb.IsChecked == true )
				{
					cb.IsChecked = false;
				}
				else
				{
					cb.IsChecked = true;
				}

				((api.UIElement)this.SourceObject).SecurityDescriptor.SaclAuditTypeFilter = this.AuditTypeFilter;
			}
		}

		private void AuditFilter_CheckChanged(object sender, RoutedEventArgs e)
		{
			ListBoxItem item = (ListBoxItem)lstAuditFilter.ItemContainerGenerator.ContainerFromItem( sender );
			if( item != null )
			{
				_suppressAuditFilterSelectionChanged = true;
				item.IsSelected = true;
				((api.UIElement)this.SourceObject).SecurityDescriptor.SaclAuditTypeFilter = this.AuditTypeFilter;
				_suppressAuditFilterSelectionChanged = false;
			}
		}

		private void cmdSetDefaultAudutFilter_Click(object sender, RoutedEventArgs e)
		{
			this.AuditTypeFilter = api.SecurityDescriptor.DefaultSaclAuditTypeFilter;
			this.chkAuditFilter.IsChecked = false;
			this.chkAuditFilter.IsEnabled = true;
		}

		private void chkAuditFilter_Checked(object sender, RoutedEventArgs e)
		{
			this.chkAuditFilter.IsEnabled = false;
		}
		#endregion

		private void cmdPanelToggler_Click(object sender, RoutedEventArgs e)
		{
			ExpandCollapse state = sg.MiscUtils.ParseEnum<ExpandCollapse>( ((Control)sender).Tag );
			switch( state )
			{
				case ExpandCollapse.Expand:
				{
					expProps.IsExpanded = expPerms.IsExpanded = expAudit.IsExpanded =
						expRightBindings.IsExpanded = expSettings.IsExpanded = true;

					cmdPanelToggler.Tag = ExpandCollapse.Collapse;
					break;
				}
				case ExpandCollapse.Collapse:
				{
					expProps.IsExpanded = expPerms.IsExpanded = expAudit.IsExpanded =
						expRightBindings.IsExpanded = expSettings.IsExpanded = false;

					cmdPanelToggler.Tag = ExpandCollapse.Expand;
					break;
				}
				case ExpandCollapse.Invert:
				{
					expProps.IsExpanded = !expProps.IsExpanded;
					expPerms.IsExpanded = !expPerms.IsExpanded;
					expAudit.IsExpanded = !expAudit.IsExpanded;
					expRightBindings.IsExpanded = !expRightBindings.IsExpanded;
					expSettings.IsExpanded = !expSettings.IsExpanded;

					cmdPanelToggler.Tag = ExpandCollapse.Invert;
					break;
				}
			}

			cmdPanelToggler.Text = string.Format( "{0} All", cmdPanelToggler.Tag );
		}


		#region Ace Editing/Validation
		private bool ValidateAndConvertAces()
		{
			bool ok = true;
			api.UIElement uie = (api.UIElement)this.SourceObject;

			for( int i = uie.SecurityDescriptor.Dacl.Count - 1; i > -1; i-- )
			{
				api.AccessControlEntryBase ace = uie.SecurityDescriptor.Dacl[i];
				if( ace.GetType().ToString() != string.Format( "Suplex.Forms.ObjectModel.Api.{0}Ace", ace.AceType ) )
				{
					uie.SecurityDescriptor.Dacl[i] = _aceUtil.ConvertAce( ace, ace.AceType );
					ace = uie.SecurityDescriptor.Dacl[i];
				}
				if( ace.Id == -1 )
				{
					ace.Id = this.NextId;
				}

				if( ace.SecurityPrincipal != null )
				{
					//ace.IsDirty = false;
				}
				else
				{
					ace.IsSelected = true;
					ok = false;
				}
			}

			for( int i = uie.SecurityDescriptor.Sacl.Count - 1; i > -1; i-- )
			{
				api.AccessControlEntryAuditBase ace = uie.SecurityDescriptor.Sacl[i];
				if( ace.GetType().ToString() != string.Format( "Suplex.Forms.ObjectModel.Api.{0}AuditAce", ace.AceType ) )
				{
					uie.SecurityDescriptor.Sacl[i] = _aceUtil.ConvertAce( ace, ace.AceType );
					ace = uie.SecurityDescriptor.Sacl[i];
				}
				if( ace.Id == -1 )
				{
					ace.Id = this.NextId;
				}

				if( ace.SecurityPrincipal != null )
				{
					//ace.IsDirty = false;
				}
				else
				{
					ace.IsSelected = true;
					ok = false;
				}
			}

			return ok;
		}

		private void dgPerms_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			api.AccessControlEntryBase ace = (api.AccessControlEntryBase)e.Row.Item;
			if( e.Column.Header.ToString() == "Group" && ace.SecurityPrincipal == null )
			{
				e.Cancel = true;
			}
		}

		//setting the owner reference is used in tracking SourceObject IsDirty state
		//maybe a little hackish
		private void dgPerms_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if( dgPerms.SelectedItem != null && dgPerms.SelectedItem is api.AccessControlEntryBase )
			{
				api.AccessControlEntryBase ace = (api.AccessControlEntryBase)dgPerms.SelectedItem;
				ace.Owner = this.SourceObject;
			}
		}

		private void dgAudit_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			api.AccessControlEntryAuditBase ace = (api.AccessControlEntryAuditBase)e.Row.Item;
			if( e.Column.Header.ToString() == "Group" && ace.SecurityPrincipal == null )
			{
				e.Cancel = true;
			}
		}

		//setting the owner reference is used in tracking SourceObject IsDirty state
		//maybe a little hackish
		private void dgAudit_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if( dgAudit.SelectedItem != null && dgAudit.SelectedItem is api.AccessControlEntryBase )
			{
				api.AccessControlEntryBase ace = (api.AccessControlEntryBase)dgAudit.SelectedItem;
				ace.Owner = this.SourceObject;
			}
		}
		#endregion
	}

	public class RightRoleTemplateSelector : DataTemplateSelector
	{
		DataTemplate _rightBindingRule = null;
		DataTemplate _elseBindingRule = null;
		DataTemplate _successRightBinding = null;
		DataTemplate _elseRightBinding = null;

		public RightRoleTemplateSelector()
			: base()
		{
			Window window = Application.Current.MainWindow;
			_rightBindingRule = window.FindResource( "rightBindingRule" ) as DataTemplate;
			_elseBindingRule = window.FindResource( "elseBindingRule" ) as DataTemplate;
			_successRightBinding = window.FindResource( "successRightBinding" ) as DataTemplate;
			_elseRightBinding = window.FindResource( "elseRightBinding" ) as DataTemplate;
		}

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			DataTemplate template = null;
			if( item != null )
			{
				if( item is api.RightRoleRule )
				{
					switch( ((api.RightRoleRule)item).LogicRuleType )
					{
						case sf.LogicRuleType.RightRoleIf:
						{
							template = _rightBindingRule;
							break;
						}
						case sf.LogicRuleType.RightRoleElse:
						{
							template = _elseBindingRule;
							break;
						}
					}
				}
				else if( item is api.RightRole )
				{
					switch( ((api.RightRole)item).RightRoleType )
					{
						case sf.RightRoleType.Success:
						{
							template = _successRightBinding;
							break;
						}
						case sf.RightRoleType.Else:
						{
							template = _elseRightBinding;
							break;
						}
					}
				}
			}

			return template;
		}
	}
}