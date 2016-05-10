using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;

using sf = Suplex.Forms;
using api = Suplex.Forms.ObjectModel.Api;
using ss = Suplex.Security;
using sg = Suplex.General;

namespace SuplexApp
{
	public partial class RightRoleDlg : Window
	{
		private api.SuplexStore _splxStore = new api.SuplexStore();

		private bool _dlgSuccess = false;
		private bool _shuttingDown = false;

		private bool _didGenerateContainers = false;
		Stack<sf.IObjectModel> _uieStack = null;
		ItemsControl _lastContainer = null;

		private long _nextId = System.DateTime.Now.Ticks;


		#region ctors, dlg handlers
		public RightRoleDlg()
		{
			InitializeComponent();

			Application.Current.MainWindow.Closing +=
				new System.ComponentModel.CancelEventHandler( this.MainDlg_Closing );

			this.BuildLists();
		}

		private void BuildLists()
		{
			tvUIElements.ItemContainerGenerator.StatusChanged += new EventHandler( ItemContainerGenerator_StatusChanged );

			ArrayList aceTypes = new ArrayList( Enum.GetValues( typeof( ss.AceType ) ) );
			aceTypes.Remove( ss.AceType.None );
			aceTypes.Remove( ss.AceType.Native );
			aceTypes.Remove( ss.AceType.UI );
			lstSrcAceType.ItemsSource = aceTypes;

			lstDstRight.ItemsSource = ss.AceTypeRights.GetRights( ss.AceType.UI ).Reverse();
			lstDstRight.SelectedIndex = -1;
		}

		//when using a single instance of a Window, the instance is unusable after calling .Close()
		//the MainDlg_Closing and Window_Closing methods below are a work-around for this
		private void MainDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_shuttingDown = true;
			this.Close();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if( !_shuttingDown )
			{
				typeof( Window ).GetField( "_isClosing", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( this, false );
				e.Cancel = true;
				this.Hide();
			}
		}
		#endregion

		#region public accessors
		public api.SuplexStore SplxStore
		{
			get { return _splxStore; }
			set
			{
				_splxStore = value;
				tvUIElements.DataContext = _splxStore.UIElements;
			}
		}
		public api.SuplexApiClient ApiClient { get; set; }

		public api.RightRole RightRole
		{
			get { return (api.RightRole)this.DataContext; }
		}

		//substitute prop for DialogResult
		//	necessary due to non-Window.Close() methodology in use above - DialogResult always returns null.
		public bool Success
		{
			get { return _dlgSuccess; }
		}

		public void InitDlg(api.RightRole rr)
		{
			bool haveRr = rr != null;


			if( haveRr )
			{
				cmdOk.Content = "OK";

				//select/expand the UIElement
				api.UIElement uie =
					( (api.UIElementCollection)tvUIElements.ItemsSource ).GetByUniqueName( rr.ControlUniqueName );
				if( uie != null )
				{
					rr.UIElement = uie;
					rr.UIElement.IsSelectedAlternate = true;

					if( _didGenerateContainers )
					{
						this.ExpandPath( rr.UIElement );
					}
				}
				else
				{
					if( tvUIElements.SelectedItem != null )
					{
						((api.UIElement)tvUIElements.SelectedItem).IsSelectedAlternate = false;
					}
				}
				// **************************

				this.DataContext = rr;
				rr.IsDirty = false;
			}
			else
			{
				lstSrcAceType.SelectedItem = null;
				lstSrcRight.ItemsSource = null;

				rr = new api.RightRole();
				rr.AceType = ss.AceType.UI;
				rr.RightName = ss.UIRight.FullControl.ToString();
				this.DataContext = rr;
				((api.RightRole)this.DataContext).Id = this.NextId;
			}

			this.CheckReady();
		}

		#region pain, pure pain
		//Method A: where double-clicking item /after/ all tvUIElements containers are generated
		//	this occurs when editing an element after adding
		//poking uie.IsExpanded doesn't work, so using this alternate approach:
		//	recurse up the tree to find root node in path, walk down and expand nodes
		private void ExpandPath(api.UIElement uie, ref TreeViewItem treeviewItem)
		{
			if( uie.ParentObject != null )
			{
				//recurse up
				this.ExpandPath( (api.UIElement)uie.ParentObject, ref treeviewItem );

				//walk down
				TreeViewItem item = ( (TreeViewItem)treeviewItem.ItemContainerGenerator.ContainerFromItem( uie ) );
				item.IsExpanded = true;
				treeviewItem = item;
			}
			else
			{
				TreeViewItem item = ( (TreeViewItem)tvUIElements.ItemContainerGenerator.ContainerFromItem( uie ) );
				item.IsExpanded = true;
				treeviewItem = item;
			}
		}


		//Method B: where double-clicking item /before/ all tvUIElements containers are generated.
		//	this occurs when editing an element before adding
		void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
		{
			if( !_didGenerateContainers &&
				tvUIElements.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated )
			{
				_didGenerateContainers = true;

				if( this.RightRole.UIElement != null )
				{
					this.ExpandPath( this.RightRole.UIElement );
				}
			}
		}

		private void ExpandPath(api.UIElement uie)
		{
			_uieStack = new Stack<sf.IObjectModel>();
			_uieStack.Push( uie );
			sf.IObjectModel parent = uie.ParentObject;

			while( parent != null )
			{
				_uieStack.Push( parent );
				( (api.UIElement)parent ).IsExpanded = true;
				parent = parent.ParentObject;
			}

			this.ExpandNowOrLater( tvUIElements );
		}

		private void ExpandNowOrLater(ItemsControl container)
		{
			if( ( _uieStack.Count > 0 ) && ( container != null ) )
			{
				sf.IObjectModel top = _uieStack.Pop();
				container = (ItemsControl)container.ItemContainerGenerator.ContainerFromItem( top );
				if( container != null )
				{
					_lastContainer = container;
					( (TreeViewItem)container ).IsExpanded = true;
					if( container.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated )
					{
						this.ExpandNowOrLater( _lastContainer );
					}
					else
					{
						container.ItemContainerGenerator.StatusChanged +=
							new EventHandler( ChildItemContainerGenerator_StatusChanged );
					}
				}
			}
		}

		void ChildItemContainerGenerator_StatusChanged(object sender, EventArgs e)
		{
			if( _lastContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated )
			{
				this.ExpandNowOrLater( _lastContainer );
			}
		}
		#endregion
		#endregion

		#region dlg handlers
		private long NextId { get { return _nextId++; } }

		private void lstSrcAceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if( lstSrcAceType.SelectedItem != null )
			{
				lstSrcRight.ItemsSource = ss.AceTypeRights.GetRights( (ss.AceType)lstSrcAceType.SelectedValue ).Reverse();
				lstSrcRight.SelectedIndex = -1;
			}

			this.CheckReady();
		}

		private void CheckReady()
		{
			cmdOk.IsEnabled = tvUIElements.SelectedItem != null &&
				lstSrcAceType.SelectedItem != null &&
				lstSrcRight.SelectedItem != null &&
				lstDstRight.SelectedItem != null;
		}

		private void cmdOk_Click(object sender, RoutedEventArgs e)
		{
			this.RightRole.UIElement = (api.UIElement)tvUIElements.SelectedItem;
			lstSrcAceType.GetBindingExpression( ListBox.SelectedItemProperty ).UpdateSource();
			lstSrcRight.GetBindingExpression( ListBox.SelectedItemProperty ).UpdateSource();
			lstDstRight.GetBindingExpression( ListBox.SelectedItemProperty ).UpdateSource();

			_dlgSuccess = true;
			this.DialogResult = _dlgSuccess;
		}

		private void cmdClose_Click(object sender, RoutedEventArgs e)
		{
			_dlgSuccess = false;
			this.DialogResult = _dlgSuccess;
		}

		private void Something_Changed(object sender, SelectionChangedEventArgs e)
		{
			this.CheckReady();
		}

		private void tvUIElements_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			this.CheckReady();
		}
		#endregion
	}
}