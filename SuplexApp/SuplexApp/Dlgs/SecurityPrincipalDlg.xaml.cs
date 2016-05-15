using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using api = Suplex.Forms.ObjectModel.Api;
using sf = Suplex.Forms;

namespace SuplexApp
{
	public partial class SecurityPrincipalDlg : UserControl, ISuplexEditorDialog
	{
		private api.Group _editingTvwGroup = null;
		private SecurityPrincipalCtrl _spEditor = null;
		private api.SuplexStore _splxStore = null;
		string _filterString = string.Empty;
		CollectionViewSource _securityPrincipalsCvs = null;


		public SecurityPrincipalDlg()
		{
			InitializeComponent();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			_securityPrincipalsCvs = grdSec.FindResource( "SecurityPrincipalsCvs" ) as CollectionViewSource;
			_securityPrincipalsCvs.View.Filter = this.NameFilter;
		}

		public api.SuplexStore SplxStore
		{
			get { return _splxStore; }
			set
			{
				_splxStore = value;
				this.DataContext = _splxStore;
			}
		}
		public api.SuplexApiClient ApiClient { get; set; }

		private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			if( _securityPrincipalsCvs.View.Filter == null )
			{
				_securityPrincipalsCvs.View.Filter = this.NameFilter;
			}

			_filterString = ((TextBox)sender).Text;
			_securityPrincipalsCvs.View.Refresh();
		}

		bool NameFilter(object item)
		{
			api.SecurityPrincipalBase sp = item as api.SecurityPrincipalBase;
			return sp.Name.IndexOf( _filterString, StringComparison.OrdinalIgnoreCase ) >= 0;
		}

		#region Security Principal editing
		private api.User _activeUser;
		private api.User _cloneUser;
		private api.Group _activeGroup;
		private api.Group _cloneGroup;
		private void lvSecurity_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if( lvSecurity.SelectedItem != null )
			{
				this.EditSecurityPrincipal( (api.SecurityPrincipalBase)lvSecurity.SelectedItem );
			}
		}

		void SpEditor_SavedChanges(object sender, EventArgs e)
		{
			bool wantsCvsRefresh = false;

			if( ((api.SecurityPrincipalBase)_spEditor.SourceObject).IsUserObject )
			{
				List<api.Group> addedGroupMembership = new List<api.Group>();
				List<api.Group> removedGroupMembership = new List<api.Group>();

				//todo: this is bit inefficient, need to review (see below)
				foreach( api.Group memberOf in _spEditor.MemberOfAdded )
				{
					addedGroupMembership.Add( memberOf );
				}
				foreach( api.Group memberOf in _spEditor.MemberOfRemoved )
				{
					removedGroupMembership.Add( memberOf );
				}

				if( _activeUser == null )
				{
					wantsCvsRefresh = true;

					if( this.ApiClient.IsConnected )
					{
						_activeUser = this.ApiClient.UpsertUser( _cloneUser, addedGroupMembership, removedGroupMembership );
						_cloneUser.Id = _activeUser.Id;	//needed post database transaction
						_cloneUser.Name = _activeUser.Name;
					}
					else
					{
						_activeUser = _cloneUser.Clone();
					}

					this.SplxStore.Users.Add( _activeUser );

					lvSecurity.SelectedItem = _activeUser;
				}
				else
				{
					if( this.ApiClient.IsConnected )
					{
						_activeUser = this.ApiClient.UpsertUser( _cloneUser, addedGroupMembership, removedGroupMembership );
						_cloneUser.Name = _activeUser.Name;
					}
					else
					{
						_activeUser.Synchronize( _cloneUser );
					}
				}

				if( this.SplxStore.GroupMembership != null )
				{
					//todo: this is bit inefficient, need to review (see above)
					foreach( api.Group memberOf in _spEditor.MemberOfAdded )
					{
						this.SplxStore.GroupMembership.Add( memberOf, _activeUser );
					}
					foreach( api.Group memberOf in _spEditor.MemberOfRemoved )
					{
						this.SplxStore.GroupMembership.Remove( memberOf, _activeUser );
					}
				}
			}
			else
			{
				List<api.SecurityPrincipalBase> addedGroupMembership = new List<api.SecurityPrincipalBase>();
				List<api.SecurityPrincipalBase> removedGroupMembership = new List<api.SecurityPrincipalBase>();

				//todo: Add/Remove GroupNest recs
				//todo: this is a bit inefficient, need to review (see below)
				foreach( api.SecurityPrincipalBase member in _spEditor.MembersAdded )
				{
					addedGroupMembership.Add( member );
				}
				foreach( api.SecurityPrincipalBase member in _spEditor.MembersRemoved )
				{
					removedGroupMembership.Add( member );
				}

				if( _activeGroup == null )
				{
					wantsCvsRefresh = true;

					if( this.ApiClient.IsConnected )
					{
						_activeGroup = this.ApiClient.UpsertGroup( _cloneGroup, addedGroupMembership, removedGroupMembership );
						_cloneGroup.Id = _activeGroup.Id;	//needed post database transaction
						_cloneGroup.Name = _activeGroup.Name;
					}
					else
					{
						_activeGroup = _cloneGroup.Clone();
					}

					this.SplxStore.Groups.Add( _activeGroup );

					lvSecurity.SelectedItem = _activeGroup;
				}
				else
				{
					if( this.ApiClient.IsConnected )
					{
						_activeGroup = this.ApiClient.UpsertGroup( _cloneGroup, addedGroupMembership, removedGroupMembership );
						_cloneGroup.Name = _activeGroup.Name;
					}
					else
					{
						_activeGroup.Synchronize( _cloneGroup );
					}
				}

				if( this.SplxStore.GroupMembership != null )
				{
					//todo: this is a bit inefficient, need to review (see above)
					foreach( api.SecurityPrincipalBase member in _spEditor.MembersAdded )
					{
						this.SplxStore.GroupMembership.Add( _activeGroup, member );
					}
					foreach( api.SecurityPrincipalBase member in _spEditor.MembersRemoved )
					{
						this.SplxStore.GroupMembership.Remove( _activeGroup, member );
					}
				}
			}

			api.SecurityPrincipalBase sp = (api.SecurityPrincipalBase)_spEditor.SourceObject;
			sp = this.SplxStore.SecurityPrincipals.SingleOrDefault( s => s.Id == sp.Id );
			if( sp != null )
			{
				sp.Name = _spEditor.SourceObject.Name;
			}


			//hack: this seems dirty, but I coiuldn't find another way to do it.
			//CollectionViewSource cvs = grdSec.FindResource( "SecurityPrincipalsCvs" ) as CollectionViewSource;
			if( wantsCvsRefresh && _securityPrincipalsCvs != null )
			{
				_securityPrincipalsCvs.View.Refresh();
			}

			this.SplxStore.IsDirty = true;
		}

		void SpEditor_CancelledChanges(object sender, EventArgs e)
		{
			if( this.SecurityPrincipalVerifyCancelChanges() )
			{
				_cloneUser = null;
				_cloneGroup = null;
				_spEditor.SourceObject = null;
			}
		}

		private bool EditSecurityPrincipal(api.SecurityPrincipalBase sp)
		{
			bool ok = this.VerifySaveChanges();

			//proceed with edit: changes saved successfully or were abandoned, or nothing to verify
			if( ok && sp != null )
			{
				this.CreateSpEditor();

				////todo: ?? sp.RecordSelect();

				if( sp.IsUserObject )
				{
					_activeUser = (api.User)sp;
					_cloneUser = _activeUser.Clone();
					_spEditor.SetDataContext( _cloneUser );
				}
				else
				{
					_activeGroup = (api.Group)sp;
					_cloneGroup = _activeGroup.Clone();
					_spEditor.SetDataContext( _cloneGroup );
				}

				SecurityContentPanel.Child = _spEditor;
			}

			return ok;
		}

		private void AddNewUser_Click(object sender, RoutedEventArgs e)
		{
			bool ok = this.VerifySaveChanges();

			//proceed with edit: changes saved successfully or were abandoned, or nothing to verify
			if( ok )
			{
				this.CreateSpEditor();

				_cloneUser = new api.User();
				_cloneUser.Id = Guid.NewGuid().ToString();
				_cloneUser.IsEnabled = true;
				_spEditor.SetDataContext( _cloneUser );

				_activeUser = null;

				SecurityContentPanel.Child = _spEditor;
			}
		}

		private void AddNewGroup_Click(object sender, RoutedEventArgs e)
		{
			bool ok = this.VerifySaveChanges();

			//proceed with edit: changes saved successfully or were abandoned, or nothing to verify
			if( ok )
			{
				this.CreateSpEditor();

				_cloneGroup = new api.Group();
				_cloneGroup.Id = Guid.NewGuid().ToString();
				_cloneGroup.Mask = this.SplxStore.Groups.GetNextMask();
				_cloneGroup.IsEnabled = true;
				_spEditor.SetDataContext( _cloneGroup );

				_activeGroup = null;

				SecurityContentPanel.Child = _spEditor;
			}
		}

		//TODO: must also cleanup GroupMemebership
		private void DeleteSecurityPrincipal_Click(object sender, RoutedEventArgs e)
		{
			api.SecurityPrincipalBase sp = (api.SecurityPrincipalBase)lvSecurity.SelectedItem;

			MessageBoxResult result = MessageBox.Show( 
				string.Format( "Are you sure you want to delete {0} ({1})?", sp.Name, sp.IsUserObject ? "user" : "group" ),
				"Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes );

			if( result == MessageBoxResult.Yes )
			{
				sp.IsDeleted = true;

				switch( ((sf.IObjectModel)sp).ObjectType )
				{
					case sf.ObjectType.User:
					{
						this.ApiClient.DeleteUserById( sp.Id );

						api.User u = null;
						this.SplxStore.Users.GetById( sp.Id, out u );
						if( u != null )
						{
							this.SplxStore.Users.Remove( u );
						}
						break;
					}
					case sf.ObjectType.Group:
					{
						this.ApiClient.DeleteGroupById( sp.Id );

						api.Group g = null;
						this.SplxStore.Groups.GetById( sp.Id, out g );
						if( g != null )
						{
							this.SplxStore.Groups.Remove( g );
						}
						break;
					}
				}

				if( this.SplxStore.GroupMembership != null )
				{
					this.SplxStore.GroupMembership.RemoveByGroupOrMember( sp );
				}

				if( _spEditor != null && _spEditor.SourceObject != null &&
					((api.SecurityPrincipalBase)_spEditor.SourceObject).Id == sp.Id )
				{
					SecurityContentPanel.Child = null;
				}

				sp = null;

				//hack: this seems dirty, but I coiuldn't find another way to do it.
				if( _securityPrincipalsCvs != null )
				{
					_securityPrincipalsCvs.View.Refresh();
				}

				this.SplxStore.IsDirty = true;
			}
		}
		#endregion

		#region Security TreeView Handlers
		public bool VerifySaveChanges()
		{
			bool ok = false;
			if( _spEditor != null && _spEditor.SourceObject != null && _spEditor.SourceObject.IsDirty )
			{
				MessageBoxResult mbr =
					MessageBox.Show( string.Format( "Save changes to {0} '{1}'?", _spEditor.SourceObject.ObjectType, _spEditor.SourceObject.Name ),
					"Save changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes );

				switch( mbr )
				{
					case MessageBoxResult.Yes:
					{
						ok = _spEditor.SaveChanges();
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

		private bool SecurityPrincipalVerifyCancelChanges()
		{
			bool ok = this.VerifySaveChanges();
			if( ok )
			{
				SecurityContentPanel.Child = null;
				if( _editingTvwGroup != null )
				{
					_editingTvwGroup.IsEditing = false;
					_editingTvwGroup = null;
				}
			}

			return ok;
		}

		public void SaveIfDirty()
		{
			if( _spEditor != null && _spEditor.SourceObject != null && _spEditor.SourceObject.IsDirty )
			{
				_spEditor.SaveChanges();
			}
		}

		public void ClearContentPanel()
		{
			SecurityContentPanel.Child = null;
		}

		private void CreateSpEditor()
		{
			if( _spEditor == null )
			{
				_spEditor = new SecurityPrincipalCtrl();
				_spEditor.SplxStore = this.SplxStore;
				_spEditor.ApiClient = this.ApiClient;
				_spEditor.SavedChanges += new EventHandler( SpEditor_SavedChanges );
				_spEditor.CancelledChanges += new EventHandler( SpEditor_CancelledChanges );
			}
		}

		private void SecUserList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if( ((ListBox)sender).SelectedItem != null )
			{
				this.EditSecurityPrincipal( (api.SecurityPrincipalBase)((ListBox)sender).SelectedItem );
			}
		}
		#endregion
	}
}