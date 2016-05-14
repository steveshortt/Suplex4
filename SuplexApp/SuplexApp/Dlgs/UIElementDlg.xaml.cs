using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using api = Suplex.Forms.ObjectModel.Api;
using sf = Suplex.Forms;
using sg = Suplex.General;
using System.Windows.Threading;

namespace SuplexApp
{
	public partial class UIElementDlg : UserControl, ISuplexEditorDialog
	{
		public static SortedDictionary<string, List<string>> ControlTypes = new SortedDictionary<string, List<string>>();

		private Point _dragDropStartPoint;
		private sf.IObjectModel _dragdropSourceObject;
		private sf.IObjectModel _dragdropDestinationObject;

		bool _lastClickWasLeftButton = true;
		private bool _rightClickIsTreeView = false;
		private api.INodeItem _selectedTvwItem = null;
		private api.INodeItem _editingTvwItem = null;

		private IEditorControl _activeEditor = null;
		private UIElementCtrl _uieEditor = null;
		private LogicRuleCtrl _lrEditor = null;
		private FillMapCtrl _fmEditor = null;

		private IList _activeParentObjectChildItems = null;
		private api.INodeItem _activeParentNode = null;

		private api.UIElement _sourceUie = null;
		private api.UIElement _cloneUie = null;
		private api.ValidationRule _sourceVr = null;
		private api.ValidationRule _cloneVr = null;
		private api.FillMap _sourceFm = null;
		private api.FillMap _cloneFm = null;

		private UIElementCtrlViewModel _uieMenuViewModel = new UIElementCtrlViewModel();

		PreviewDlg _previewDlg = new PreviewDlg();


		public UIElementDlg()
		{
			InitializeComponent();
		}

		private api.SuplexStore _splxStore = null;
		public api.SuplexStore SplxStore
		{
			get { return _splxStore; }
			set
			{
				_splxStore = value;
				this.DataContext = _splxStore;

				if( _uieEditor != null )
				{
					_uieEditor.SplxStore = _splxStore;
				}
			}
		}
		public api.SuplexApiClient ApiClient { get; set; }


		#region Build this only once
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			this.CreateUieEditorAsync();

			this.BuildControlTypesList();

			tbMain.DataContext = _uieMenuViewModel;
			tvUie.ContextMenu.DataContext = _uieMenuViewModel;
		}

		private void BuildControlTypesList()
		{
			String appStartPath =
				System.IO.Path.GetDirectoryName( Process.GetCurrentProcess().MainModule.FileName );

			string[] assemblies = { "WinForms", "WebForms", "Wpf" };

			foreach( string assm in assemblies )
			{
				string assemblyFile = string.Format( "{0}\\Suplex.{1}.dll", appStartPath, assm );
				if( File.Exists( assemblyFile ) )
				{
					Assembly a = Assembly.LoadFrom( assemblyFile );
					Type[] types = a.GetTypes();
					foreach( Type t in types )
					{
						this.EvalControlType( t );
					}
				}
			}
		}

		private void EvalControlType(Type t)
		{
			if( t.GetInterface( "IValidationControl" ) != null &&
				t.IsDefined( typeof( sf.EventBindingsAttribute ), false ) )
			{
				sf.EventBindingsAttribute attrib =
					(sf.EventBindingsAttribute)Attribute.GetCustomAttribute( t, typeof( sf.EventBindingsAttribute ) );

				if( attrib.IsPublicControl )
				{
					//string key = string.Format( "{0}", t.Name.Substring( 1, t.Name.Length - 1 ) );
					string key = t.Name;

					if( !ControlTypes.ContainsKey( key ) )
					{
						ControlTypes.Add( key, new List<string>() );

						foreach( object eventBinding in attrib.GetList() )
						{
							ControlTypes[key].Add( eventBinding.ToString() );
						}
					}
					else
					{
						foreach( object eventBinding in attrib.GetList() )
						{
							if( !ControlTypes[key].Contains( eventBinding.ToString() ) )
							{
								ControlTypes[key].Add( eventBinding.ToString() );
							}
						}
					}

					ControlTypes[key].Sort();
				}
			}
		}
		#endregion

		#region UIE handlers
		private void CreateUieEditorAsync()
		{
			this.Dispatcher.BeginInvoke(
				DispatcherPriority.Background,
				new DispatcherOperationCallback( delegate
					{
						this.CreateUieEditor();
						return null;
					}
					), null );
		}


		private void CreateUieEditor()
		{
			if( _uieEditor == null )
			{
				_uieEditor = new UIElementCtrl();
				_uieEditor.SplxStore = this.SplxStore;
				_uieEditor.ApiClient = this.ApiClient;
				_uieEditor.SavedChanges += new EventHandler( UieEditor_SavedChanges );
				_uieEditor.CancelledChanges += new EventHandler( UieEditor_CancelledChanges );
			}
			else
			{
				ContentPanel.Child = _uieEditor;
			}
		}

		void UieEditor_SavedChanges(object sender, EventArgs e)
		{
			if( _sourceUie == null )
			{
				if( this.ApiClient.IsConnected )
				{
					_sourceUie = this.ApiClient.UpsertUIElement( _cloneUie );
					_cloneUie.Id = _sourceUie.Id;	//needed post database transaction
				}
				else
				{
					_sourceUie = _cloneUie.Clone( api.UIElement.CloneSecurity, false );
				}

				this.AddNewItemToTree( _sourceUie );
			}
			else
			{
				if( this.ApiClient.IsConnected )
				{
					_cloneUie.SecurityDescriptor.SynchronizeDeleteIds( _sourceUie.SecurityDescriptor );
					api.UIElement result = this.ApiClient.UpsertUIElement( _cloneUie );
					_sourceUie.Synchronize( _cloneUie, api.UIElement.CloneSecurity, false );
					_sourceUie.SynchronizeSpecial( result, api.UIElement.CloneSecurity, false, this.ApiClient.IsConnected );
				}
				else
				{
					_sourceUie.Synchronize( _cloneUie, api.UIElement.CloneSecurity, false );
				}
			}

			_sourceUie.SecurityDescriptor.Dacl.Resolve( this.SplxStore );
			_sourceUie.SecurityDescriptor.Sacl.Resolve( this.SplxStore );
			_cloneUie.SynchronizeSpecial( _sourceUie, api.UIElement.CloneSecurity, false, this.ApiClient.IsConnected );

			this.SplxStore.IsDirty = true;
		}

		void UieEditor_CancelledChanges(object sender, EventArgs e)
		{
			if( this.UieTreeVerifyCancelChanges() )
			{
				_cloneUie = null;
				_uieEditor.SourceObject = null;
			}
		}

		private void UieItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			Grid uiePrwvWrapper = (Grid)sender;
			if( uiePrwvWrapper.IsVisible )
			{
				api.UIElement uie = (api.UIElement)((UserControl)uiePrwvWrapper.FindName( "uiePrwv" )).DataContext;
				uie.SecurityDescriptor.Dacl.Resolve( this.SplxStore );
				uie.SecurityDescriptor.Sacl.Resolve( this.SplxStore );
			}
		}
		#endregion

		#region VR handlers
		private void CreateLrEditor()
		{
			if( _lrEditor == null )
			{
				_lrEditor = new LogicRuleCtrl();
				_lrEditor.SplxStore = this.SplxStore;
				_lrEditor.ApiClient = this.ApiClient;
				_lrEditor.SavedChanges += new EventHandler( LrEditor_SavedChanges );
				_lrEditor.CancelledChanges += new EventHandler( LrEditor_CancelledChanges );
			}

			ContentPanel.Child = _lrEditor;
		}

		void LrEditor_SavedChanges(object sender, EventArgs e)
		{
			if( _sourceVr == null )
			{
				if( this.ApiClient.IsConnected )
				{
					_sourceVr = this.ApiClient.UpsertValidationRule( _cloneVr );
					_cloneVr.Id = _sourceVr.Id;	//needed post database transaction
				}
				else
				{
					_sourceVr = _cloneVr.Clone( api.ValidationRule.CloneShallow, false );
				}

				this.AddNewItemToTree( _sourceVr );
			}
			else
			{
				if( this.ApiClient.IsConnected )
				{
					this.ApiClient.UpsertValidationRule( _cloneVr );
				}

				_sourceVr.Synchronize( _cloneVr, api.ValidationRule.CloneShallow, false );
			}

			this.SplxStore.IsDirty = true;
		}

		void LrEditor_CancelledChanges(object sender, EventArgs e)
		{
			if( this.UieTreeVerifyCancelChanges() )
			{
				_cloneVr = null;
				_lrEditor.SourceObject = null;
			}
		}
		#endregion

		#region FM handlers
		private void CreateFmEditor()
		{
			if( _fmEditor == null )
			{
				_fmEditor = new FillMapCtrl();
				_fmEditor.SplxStore = this.SplxStore;
				_fmEditor.ApiClient = this.ApiClient;
				_fmEditor.SavedChanges += new EventHandler( FmEditor_SavedChanges );
				_fmEditor.CancelledChanges += new EventHandler( FmEditor_CancelledChanges );
			}

			ContentPanel.Child = _fmEditor;
		}

		void FmEditor_SavedChanges(object sender, EventArgs e)
		{
			if( _sourceFm == null )
			{
				if( this.ApiClient.IsConnected )
				{
					_sourceFm = this.ApiClient.UpsertFillMap( _cloneFm );
					_cloneFm.Id = _sourceFm.Id;	//needed post database transaction
				}
				else
				{
					_sourceFm = _cloneFm.Clone();
				}

				this.AddNewItemToTree( _sourceFm );
			}
			else
			{
				if( this.ApiClient.IsConnected )
				{
					api.FillMap result = this.ApiClient.UpsertFillMap( _cloneFm );
					_sourceFm.SynchronizeSpecial( result );
					_cloneFm.SynchronizeSpecial( _sourceFm );
				}
				else
				{
					_sourceFm.Synchronize( _cloneFm );
				}
			}

			this.SplxStore.IsDirty = true;
		}

		void FmEditor_CancelledChanges(object sender, EventArgs e)
		{
			if( this.UieTreeVerifyCancelChanges() )
			{
				_cloneFm = null;
				_fmEditor.SourceObject = null;
			}
		}
		#endregion

		#region UieTree Save/Discard Changes Verify handlers
		public bool VerifySaveChanges()
		{
			bool ok = false;
			//bool haveEditingItem = _editingTvwItem != null;
			if( ContentPanel.Child != null && _activeEditor != null &&
				_activeEditor.SourceObject != null && _activeEditor.SourceObject.IsDirty )
			{
				MessageBoxResult mbr =
					MessageBox.Show( string.Format( "Save changes to {0} '{1}'?", _activeEditor.SourceObject.ObjectType, _activeEditor.SourceObject.Name ),
					"Save changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes );

				switch( mbr )
				{
					case MessageBoxResult.Yes:
					{
						ok = _activeEditor.SaveChanges();
						break;
					}
					case MessageBoxResult.No:
					{
						ok = true;
						break;
					}
					case MessageBoxResult.Cancel:
					{
						break;
					}
				}
			}
			else
			{
				//no item to verify or item is not dirty
				ok = true;
			}

			return ok;
		}

		private bool UieTreeVerifyCancelChanges()
		{
			bool ok = this.VerifySaveChanges();
			if( ok )
			{
				ContentPanel.Child = null;
				_activeEditor = null;
				if( _editingTvwItem != null )
				{
					_editingTvwItem.IsEditing = false;
					_editingTvwItem = null;
				}
			}

			return ok;
		}

		public void SaveIfDirty()
		{
			if( _activeEditor != null && _activeEditor.SourceObject != null && _activeEditor.SourceObject.IsDirty )
			{
				_activeEditor.SaveChanges();
			}
		}

		public void ClearContentPanel()
		{
			ContentPanel.Child = null;
		}
		#endregion

		#region UIElement context menu handlers
		private void ImagePopup_Click(object sender, MouseButtonEventArgs e)
		{
			PreviewDlg p = new PreviewDlg();
			p.Show();
		}

		private void ImageToggleMenu_Click(object sender, MouseButtonEventArgs e)
		{
			ctxUieTree.PlacementTarget = (Image)sender;
			ctxUieTree.Placement = PlacementMode.Bottom;
			ctxUieTree.IsOpen = !ctxUieTree.IsOpen;
		}

		private void tvUie_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			//fixes the issue where when scrolling, if the user has selected an item in the tree and moves the
			//mouse pointer off the scrollbar, the tvUie_MouseMove will detect the diff.X/Y has been exceeded
			//and allow the drag/drop action to take place.
			_lastClickWasLeftButton = false;
		}

		private void tvUie_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_dragDropStartPoint = e.GetPosition( null );
			_lastClickWasLeftButton = !IsMouseOverScrollbar( sender, _dragDropStartPoint );
		}

		private bool IsMouseOverScrollbar(object sender, Point mousePosition)
		{
			if( sender is Visual )
			{
				HitTestResult hit = VisualTreeHelper.HitTest( sender as Visual, mousePosition );

				if( hit == null ) return false;

				DependencyObject dObj = hit.VisualHit;
				while( dObj != null )
				{
					if( dObj is ScrollBar ) return true;

					if( (dObj is Visual) || (dObj is System.Windows.Media.Media3D.Visual3D) ) dObj = VisualTreeHelper.GetParent( dObj );
					else dObj = LogicalTreeHelper.GetParent( dObj );
				}
			}

			return false;
		}

		private void tvUie_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			_lastClickWasLeftButton = false;

			_rightClickIsTreeView = true;
			if( sender is TreeViewItem )
			{
				_rightClickIsTreeView = false;
				((api.INodeItem)((TreeViewItem)sender).Header).IsSelected = true;
			}
			else
			{
				if( _selectedTvwItem != null )
				{
					_selectedTvwItem.IsSelected = false;
				}
			}
		}

		private void tvUie_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if( _selectedTvwItem != null )
			{
				_selectedTvwItem.IsSelected = false;
			}

			_selectedTvwItem = tvUie.SelectedItem as api.INodeItem;
			if( _selectedTvwItem != null )
			{
				_selectedTvwItem.IsSelected = true;
				this.mnuEditItem_Click( null, null );
			}

			_uieMenuViewModel.Refresh( _selectedTvwItem as sf.IObjectModel );
		}

		private void AddNewObject_Click(object sender, RoutedEventArgs e)
		{
			if( !this.VerifySaveChanges() ) { return; }

			if( !_rightClickIsTreeView && _selectedTvwItem != null )
			{
				sf.IObjectModel selObject = (sf.IObjectModel)_selectedTvwItem;
				_activeParentNode = selObject as api.INodeItem;

				sf.ObjectType senderObjectType = ((api.ISuplexObject)sender).ObjectType;

				switch( senderObjectType )
				{
					case sf.ObjectType.UIElement:
					{
						this.CreateUieEditor();

						_activeEditor = _uieEditor;
						_activeParentObjectChildItems = ((api.UIElement)selObject).UIElements;

						_sourceUie = null;
						_cloneUie = new api.UIElement();
						_cloneUie.ParentObject = selObject;
						_uieEditor.SetDataContext( _cloneUie );
						break;
					}
					case sf.ObjectType.ValidationRule:
					{
						this.CreateLrEditor();

						_activeEditor = _lrEditor;

						_sourceVr = null;
						_cloneVr = new api.ValidationRule();
						_cloneVr.ParentObject = selObject;
						_lrEditor.SetDataContext( _cloneVr );

						switch( selObject.ObjectType )
						{
							case sf.ObjectType.UIElement:
							{
								_activeParentObjectChildItems = ((api.UIElement)selObject).ValidationRules;
								break;
							}
							case sf.ObjectType.ValidationRule:
							{
								_activeParentObjectChildItems = ((api.ValidationRule)selObject).ValidationRules;
								break;
							}
						}
						break;
					}
					case sf.ObjectType.ElseRule:
					{
						this.CreateLrEditor();

						_activeEditor = _lrEditor;
						_activeParentObjectChildItems = ((api.ValidationRule)selObject).ElseRules;

						_sourceVr = null;
						_cloneVr = new api.ValidationRule();
						_cloneVr.LogicRuleType = Suplex.Forms.LogicRuleType.ValidationElse;
						_cloneVr.ParentObject = selObject;
						_lrEditor.SetDataContext( _cloneVr );
						break;
					}
					case sf.ObjectType.FillMap:
					{
						this.CreateFmEditor();

						_activeEditor = _fmEditor;

						_sourceFm = null;
						_cloneFm = new api.FillMap();
						_cloneFm.ParentObject = selObject;
						_fmEditor.SetDataContext( _cloneFm );

						switch( selObject.ObjectType )
						{
							case sf.ObjectType.UIElement:
							{
								_activeParentObjectChildItems = ((api.UIElement)selObject).FillMaps;
								break;
							}
							case sf.ObjectType.ValidationRule:
							{
								_activeParentObjectChildItems = ((api.ValidationRule)selObject).FillMaps;
								break;
							}
						}
						break;
					}
					case sf.ObjectType.ElseMap:
					{
						this.CreateFmEditor();

						_activeEditor = _fmEditor;
						_activeParentObjectChildItems = ((api.ValidationRule)selObject).ElseMaps;

						_sourceFm = null;
						_cloneFm = new api.FillMap();
						_cloneFm.FillMapType = Suplex.Forms.FillMapType.FillMapElse;
						_cloneFm.ParentObject = selObject;
						_fmEditor.SetDataContext( _cloneFm );
						break;
					}
				}
			}
			else
			{
				this.CreateUieEditor();

				_activeEditor = _uieEditor;
				_activeParentObjectChildItems = this.SplxStore.UIElements;

				_sourceUie = null;
				_cloneUie = new api.UIElement();
				_uieEditor.SetDataContext( _cloneUie );

				_activeParentNode = null;
			}
		}

		private void AddNewItemToTree(sf.IObjectModel objectItem)
		{
			_activeParentObjectChildItems.Add( objectItem );

			if( _activeParentNode != null )
			{
				_activeParentNode.IsExpanded = true;
			}

			if( _editingTvwItem != null )
			{
				_editingTvwItem.IsEditing = false;
			}

			_editingTvwItem = (api.INodeItem)objectItem;
			_editingTvwItem.IsSelected = true;
			_editingTvwItem.IsEditing = true;
		}

		private void mnuEditItem_Click(object sender, RoutedEventArgs e)
		{
			//check to see if there's a dirty node and if to save changes
			if( !this.VerifySaveChanges() ) { return; }


			//proceed with edit: changes saved successfully or were abandoned, or nothing to verify
			sf.ObjectType senderObjectType = ((sf.IObjectModel)_selectedTvwItem).ObjectType;

			try
			{
				#region switch( senderObjectType )
				switch( senderObjectType )
				{
					case sf.ObjectType.UIElement:
					{
						this.CreateUieEditor();

						_sourceUie = (api.UIElement)_selectedTvwItem;
						////todo: ?? _sourceUie.RecordSelect();

						_sourceUie.SecurityDescriptor.Dacl.Resolve( this.SplxStore );
						_sourceUie.SecurityDescriptor.Sacl.Resolve( this.SplxStore );
						_cloneUie = _sourceUie.Clone( api.UIElement.CloneSecurity, false );
						_uieEditor.SetDataContext( _cloneUie );
						_activeEditor = _uieEditor;

						break;
					}
					case sf.ObjectType.ValidationRule:
					case sf.ObjectType.ElseRule:
					{
						this.CreateLrEditor();

						_sourceVr = (api.ValidationRule)_selectedTvwItem;
						////todo: ?? _sourceVr.RecordSelect();

						_cloneVr = _sourceVr.Clone( api.ValidationRule.CloneShallow, false );
						_lrEditor.SetDataContext( _cloneVr );
						_activeEditor = _lrEditor;

						break;
					}
					case sf.ObjectType.FillMap:
					case sf.ObjectType.ElseMap:
					{
						this.CreateFmEditor();

						_sourceFm = (api.FillMap)_selectedTvwItem;
						////todo: ?? _sourceFm.RecordSelect();

						_cloneFm = _sourceFm.Clone();
						_fmEditor.SetDataContext( _cloneFm );
						_activeEditor = _fmEditor;

						break;
					}
				}
				#endregion

				if( _editingTvwItem != null )
				{
					_editingTvwItem.IsEditing = false;
				}
				_selectedTvwItem.IsEditing = true;
				_editingTvwItem = _selectedTvwItem;
			}
			catch( Exception ex )
			{
				throw ex;
				//todo: this.HandleException( ex );
			}
		}

		private void ExpandAll_Click(object sender, RoutedEventArgs e)
		{
			this.ExpandOrCollapseAll( true );
		}
		private void CollapseAll_Click(object sender, RoutedEventArgs e)
		{
			this.ExpandOrCollapseAll( false );
		}
		private void ExpandOrCollapseAll(bool isExpanded)
		{
			if( _rightClickIsTreeView )
			{
				if( isExpanded )
				{
					this.SplxStore = this.ApiClient.GetSuplexStore( true, true );
				}

				foreach( api.INodeItem item in this.SplxStore.UIElements )
				{
					this.ExpandOrCollapseAll( item, isExpanded );
				}
			}
			else
			{
				if( isExpanded && this.ApiClient.IsConnected )
				{
					if( tvUie.SelectedItem is api.UIElement )
					{
						api.UIElement uie = (api.UIElement)tvUie.SelectedItem;
						api.UIElement thisUie = this.ApiClient.GetUIElementById( uie.ObjectId, false );
						uie.Synchronize( thisUie, api.UIElement.CloneDeep, false );

						//_apiClient.GetUIElementWithChildren( uie );
					}
					else if( tvUie.SelectedItem is api.ValidationRule )
					{
						api.ValidationRule vr = (api.ValidationRule)tvUie.SelectedItem;
						api.ValidationRule thisVr = this.ApiClient.GetValidationRuleById( vr.ObjectId, false );
						vr.Synchronize( thisVr, api.ValidationRule.CloneDeep, false );

						//_apiClient.GetValidationRuleWithChildren( vr );
					}
				}

				this.ExpandOrCollapseAll( (api.INodeItem)tvUie.SelectedItem, isExpanded );
			}
		}
		private void ExpandOrCollapseAll(api.INodeItem item, bool isExpanded)
		{
			item.IsExpanded = isExpanded;

			if( item.ChildObjects != null )
			{
				foreach( CollectionContainer container in item.ChildObjects )
				{
					foreach( api.INodeItem child in container.Collection )
					{
						child.IsExpanded = isExpanded;

						this.ExpandOrCollapseAll( child, isExpanded );
					}
				}
			}
		}

		private void DeleteObject_Click(object sender, RoutedEventArgs e)
		{
			sf.IObjectModel obj = (sf.IObjectModel)tvUie.SelectedItem;
			string msg = string.Format( "Are you sure you want to delete the selected {0}{1}?",
				obj.ObjectType,
				obj.ValidChildObjectTypes == sf.ObjectType.None ? string.Empty : " and all its child items" );

			MessageBoxResult r =
				MessageBox.Show( msg, "Confirm Delete",
				MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK );
			if( r == MessageBoxResult.OK )
			{
				((api.INotifyDeleted)obj).IsDeleted = true;
				switch( obj.ObjectType )
				{
					case sf.ObjectType.UIElement:
					{
						this.ApiClient.DeleteUIElementById( ((api.UIElement)obj).Id );
						break;
					}
					case sf.ObjectType.ValidationRule:
					case sf.ObjectType.ElseRule:
					{
						this.ApiClient.DeleteValidationRuleById( ((api.ValidationRule)obj).Id );
						break;
					}
					case sf.ObjectType.FillMap:
					case sf.ObjectType.ElseMap:
					{
						this.ApiClient.DeleteFillMapById( ((api.FillMap)obj).Id );
						break;
					}
				}
				this.ClearContentPanel( obj );
			}
		}

		private void ClearContentPanel(sf.IObjectModel deletedObj)
		{
			sf.IObjectModel editingObjectSource = null;
			if( ContentPanel.Child != null && _activeEditor.SourceObject != null )
			{
				switch( _activeEditor.SourceObject.ObjectType )
				{
					case sf.ObjectType.UIElement:
					{
						editingObjectSource = _sourceUie;
						break;
					}
					case sf.ObjectType.ValidationRule:
					case sf.ObjectType.ElseRule:
					{
						editingObjectSource = _sourceVr;
						break;
					}
					case sf.ObjectType.FillMap:
					case sf.ObjectType.ElseMap:
					{
						editingObjectSource = _sourceFm;
						break;
					}
				}

				//walk up the parent chain to see if editingObject is a descendant of the deletingObject
				sf.IObjectModel ascendant = editingObjectSource;
				while( ascendant != null )
				{
					if( ascendant.Equals( deletedObj ) )
					{
						ContentPanel.Child = null;
						break;
					}
					else
					{
						ascendant = ascendant.ParentObject;
					}
				}
			}
		}

		private void mnuToggleDetail_Click(object sender, RoutedEventArgs e)
		{
			_previewDlg.SetDataContext( _selectedTvwItem as api.ISuplexObject );
			_previewDlg.Show();
		}

		private void mnuTogglePanels_Click(object sender, RoutedEventArgs e)
		{
			_selectedTvwItem.ShowDetailPanels = !_selectedTvwItem.ShowDetailPanels;
		}
		#endregion

		#region treeview drag/drop
		private void tvUie_MouseMove(object sender, MouseEventArgs e)
		{
			if( cmdAllowDragDrop.IsChecked.Value &&
				e.LeftButton == MouseButtonState.Pressed && _lastClickWasLeftButton )
			{
				Point mousePos = e.GetPosition( tvUie );
				Vector diff = _dragDropStartPoint - mousePos;

				if( Math.Abs( diff.X ) > SystemParameters.MinimumHorizontalDragDistance &&
					Math.Abs( diff.Y ) > SystemParameters.MinimumVerticalDragDistance )
				{
					api.INodeItem selectedItem = (api.INodeItem)tvUie.SelectedItem;
					if( (selectedItem != null) )	//fix here: //&& selectedItem.IsMouseOver )
					{
						_dragdropSourceObject = (sf.IObjectModel)selectedItem;
						sf.IObjectModel finalObject = _dragdropSourceObject;


						DragDropEffects finalDropEffect =
							DragDrop.DoDragDrop( tvUie, selectedItem, DragDropEffects.Move | DragDropEffects.Copy );


						if( (finalDropEffect == DragDropEffects.Move) )
						{
							if( _dragdropSourceObject.ParentObject != null )
							{
								((api.IObjectCollectionHost)_dragdropSourceObject.ParentObject).RemoveChildObject( _dragdropSourceObject );

								//TODO: handle ValidationRule/FillMap logic for non-UIE parent here?
							}
							else
							{
								this.SplxStore.UIElements.Remove( (api.UIElement)_dragdropSourceObject );
							}

							if( _dragdropDestinationObject != null )
							{
								((api.IObjectCollectionHost)_dragdropDestinationObject).AddChildObject( _dragdropSourceObject );
							}
							else
							{
								this.SplxStore.UIElements.Add( (api.UIElement)_dragdropSourceObject );
							}
						}

						else if( (finalDropEffect == DragDropEffects.Copy) )
						{
							finalObject = ((sf.ICloneableObject)_dragdropSourceObject).Clone( true );
							if( _dragdropDestinationObject != null )
							{
								((api.IObjectCollectionHost)_dragdropDestinationObject).AddChildObject( finalObject );
							}
							else
							{
								this.SplxStore.UIElements.Add( (api.UIElement)finalObject );
							}
						}

						if( (finalDropEffect != DragDropEffects.None) )
						{
							switch( finalObject.ObjectType )
							{
								case sf.ObjectType.UIElement:
								{
									this.ApiClient.UpsertUIElement( (api.UIElement)finalObject );
									break;
								}
								case sf.ObjectType.ValidationRule:
								case sf.ObjectType.ElseRule:
								{
									this.ApiClient.UpsertValidationRule( (api.ValidationRule)finalObject );
									break;
								}
								case sf.ObjectType.FillMap:
								case sf.ObjectType.ElseMap:
								{
									this.ApiClient.UpsertFillMap( (api.FillMap)finalObject );
									break;
								}
							}
						}
					}
				}
			}
		}

		private TreeViewItem GetContainerFromObject(sf.IObjectModel node)
		{
			Stack<sf.IObjectModel> stack = new Stack<sf.IObjectModel>();
			stack.Push( node );
			sf.IObjectModel parent = node.ParentObject;

			while( parent != null )
			{
				stack.Push( parent );
				parent = parent.ParentObject;
			}

			ItemsControl container = tvUie;
			while( (stack.Count > 0) && (container != null) )
			{
				sf.IObjectModel top = stack.Pop();
				container = (ItemsControl)container.ItemContainerGenerator.ContainerFromItem( top );
			}

			return container as TreeViewItem;
		}

		private TreeViewItem GetNearestContainer(UIElement element)
		{
			// Walk up the element tree to the nearest tree view item.
			TreeViewItem container = element as TreeViewItem;
			while( (container == null) && (element != null) )
			{
				element = VisualTreeHelper.GetParent( element ) as UIElement;
				container = element as TreeViewItem;
			}

			return container;
		}

		private void tvUie_Drop(object sender, DragEventArgs e)
		{
			this.ValidateDropTarget( e, true );
		}

		private void tvUie_CheckDropTarget(object sender, DragEventArgs e)
		{
			this.ValidateDropTarget( e, false );
		}

		private void ValidateDropTarget(DragEventArgs e, bool setDestinationForDrop)
		{
			bool isValid = false;

			sf.IObjectModel src = _dragdropSourceObject;

			sf.IObjectModel dst = null;
			TreeViewItem nearestTreeViewItem = this.GetNearestContainer( e.OriginalSource as UIElement );
			if( nearestTreeViewItem != null )
			{
				dst = (sf.IObjectModel)nearestTreeViewItem.Header;
			}

			if( dst != null )
			{
				bool supports = dst.SupportsChildObjectType( src.ObjectType );
				bool isSelf = src.Equals( dst );
				bool destinationIsDescendant = this.DestinationIsDescendant( src, dst );
				bool destinationIsParent = this.DestinationIsParent( src, dst );
				isValid = supports && !destinationIsParent && !isSelf && !destinationIsDescendant;

				//Debug.WriteLine( "e.OriginalSource: " + e.OriginalSource );
				//Debug.WriteLine( "dst: " + DateTime.Now.ToLongTimeString() );
				//Debug.WriteLine( string.Format( "supports: {0}, isSelf: {1}, destinationIsDescendant: {2}, destinationIsParent: {3}", supports, isSelf, destinationIsDescendant, destinationIsParent ) );
			}
			else
			{
				//dragging an api.UIElement that's not a top node
				isValid = src is api.UIElement && ((api.UIElement)src).ParentObject != null;
			}


			if( isValid )
			{
				e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ?
					DragDropEffects.Copy : DragDropEffects.Move;

				if( setDestinationForDrop )
				{
					_dragdropDestinationObject = dst;
				}
			}
			else
			{
				e.Effects = DragDropEffects.None;
			}


			e.Handled = true;
		}

		private bool DestinationIsDescendant(sf.IObjectModel src, sf.IObjectModel dst)
		{
			// Check the parent node of the second node.
			if( dst.ParentObject == null ) return false;
			if( dst.ParentObject.Equals( src ) ) return true;

			// If the parent node is not null or equal to the first node, 
			// call the DestinationIsDescendant method recursively using the parent of 
			// the second node.
			return this.DestinationIsDescendant( src, dst.ParentObject );
		}

		private bool DestinationIsParent(sf.IObjectModel src, sf.IObjectModel dst)
		{
			bool result = false;

			if( src.ParentObject != null &&
				src.ParentObject.Equals( dst ) )
			{
				result = true;
			}

			return result;
		}

		private bool IsSelf(sf.IObjectModel node1, sf.IObjectModel node2)
		{
			return node1.Equals( node2 );
		}
		#endregion
	}
}