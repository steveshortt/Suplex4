using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Suplex.Security;
using Suplex.Wpf;
using api = Suplex.Forms.ObjectModel.Api;
using sf = Suplex.Forms;

namespace SuplexApp
{
	/// <summary>
	/// Interaction logic for SecurityPrincipalCtrl.xaml
	/// </summary>
	public partial class SecurityPrincipalCtrl : SplxUserControl, IEditorControl<api.SecurityPrincipalBase>
	{
		private api.MembershipList<api.SecurityPrincipalBase> _members = null;
		api.MembershipList<api.Group> _membership = null;

		#region events
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

		public SecurityPrincipalCtrl()
		{
			InitializeComponent();

			this.Validation.ValidationSummaryControl = vs;
			this.Validation.AutoValidateContainer = true;
			this.Validation.Load( new StringReader( Properties.Resources.sp ) );

			this.Security.Descriptor.Dacl.Add( 0, new UIAce( UIRight.FullControl, true ) );
			this.Security.Apply( AceType.Native );
		}

		#region IEditorControl<SecurityPrincipalBase> Members
		public void SetDataContext(api.SecurityPrincipalBase securityPrincipal)
		{
			this.DataContext = securityPrincipal;

			this.OldName = string.IsNullOrEmpty( securityPrincipal.Name ) ? string.Empty : securityPrincipal.Name;

			_members = null;
			_membership = null;

			this.IsUserObject = securityPrincipal.IsUserObject;

			if( securityPrincipal.IsUserObject )
			{
				this.SourceObject = (api.User)securityPrincipal;

				//this.SplxStore.GroupMembership.RecordSelectGroupMemberOf( (api.User)securityPrincipal );

				if( this.ApiClient.IsConnected )
				{
					_membership = this.ApiClient.GetUserGroupMemberOf( ((api.User)securityPrincipal).Id );
				}
				else
				{
					_membership = this.SplxStore.GroupMembership.GetMemberOf( securityPrincipal );
				}

				//dlvMemberOf.LeftListDataContext = _membership.MemberList;
				dlvMemberOf.LeftListDataContext = _membership.MemberList.CollectionViewSource;
				_membership.MemberList.CollectionViewSource.View.Filter = this.LeftFilter;

				dlvMemberOf.RightListDataContext = _membership.NonMemberList.CollectionViewSource;
				_membership.NonMemberList.CollectionViewSource.View.Filter = this.RightFilter;
			}
			else
			{
				this.SourceObject = (api.Group)securityPrincipal;

				//this.SplxStore.GroupMembership.RecordSelectGroupMembers( (api.Group)securityPrincipal );

				if( this.ApiClient.IsConnected )
				{
					_members = this.ApiClient.GetGroupMembers( ((api.Group)securityPrincipal).Id );
					//dlvMembers.LeftListDataContext = _members.MemberList;
					dlvMembers.LeftListDataContext = _members.MemberList.CollectionViewSource;
					_members.MemberList.CollectionViewSource.View.Filter = this.LeftFilter;

					//dlvMembers.RightListDataContext = _members.NonMemberList;
					dlvMembers.RightListDataContext = _members.NonMemberList.CollectionViewSource;
					_members.NonMemberList.CollectionViewSource.View.Filter = this.RightFilter;


					List<api.Group> hier = this.ApiClient.GetGroupHierarchy( ((api.Group)securityPrincipal).Id );
					tvwGroupHier.DataContext = hier;

					//hack: to get around ItemContainerStyle bug w/ TreeListView
					//Stack<TreeViewItem> groups = new Stack<TreeViewItem>();
					//foreach( api.Group g in tvwGroupHier.Items )
					//{
					//    groups.Push( tvwGroupHier.ItemContainerGenerator.ContainerFromItem( g ) as TreeViewItem );
					//}
					//while( groups.Count > 0 )
					//{
					//    TreeViewItem tvi = groups.Pop();
					//    tvi.IsExpanded = true;
					//    foreach( TreeViewItem ch in tvi.Items )
					//    {
					//        groups.Push( ch );
					//    }
					//}
				}
				else
				{
					_members = this.SplxStore.GroupMembership.GetGroupMembers( (api.Group)securityPrincipal );
					//dlvMembers.LeftListDataContext = _members.MemberList;
					dlvMembers.LeftListDataContext = _members.MemberList.CollectionViewSource;
					_members.MemberList.CollectionViewSource.View.Filter = this.LeftFilter;

					//dlvMembers.RightListDataContext = _members.NonMemberList;
					dlvMembers.RightListDataContext = _members.NonMemberList.CollectionViewSource;
					_members.NonMemberList.CollectionViewSource.View.Filter = this.RightFilter;

					List<api.Group> hier =
						this.SplxStore.GroupMembership.GetGroupHierarchy( (api.Group)securityPrincipal );
					tvwGroupHier.DataContext = hier;
				}
			}

			vs.Reset();

			this.SourceObject.IsDirty = false;
		}
		#endregion

		//used for validation:
		//so.. this should really possible in Suplex, i guess.
		public bool HasDatabaseConnection { get { return this.ApiClient.HasDatabaseConnection; } }
		public bool IsUserObject { get; private set; }
		private string OldName { get; set; }
		public bool NeedsNameExistsTest { get { return this.OldName.ToLower() != txtName.Text.ToLower(); } }
		public bool UserExists
		{
			get
			{
				api.User user = null;
				return this.SplxStore.Users.GetByName( txtName.Text, out user );
			}
		}
		public bool GroupExists
		{
			get
			{
				api.Group group = null;
				return this.SplxStore.Groups.GetByName( txtName.Text, out group );
			}
		}
		// ------------------ ------------------ ------------------ ------------------

		#region IEditorControl Members
		public bool SaveChanges()
		{
			//todo: i have no idea what this is for
			txtName.DataAccessLayer.Application =
				this.ApiClient.HasDatabaseConnection ? this.ApiClient.SplxDal.DataAccessor : null;
			//throw new NotImplementedException( "this.SplxStore.SuplexDataAccessor" );

			vs.Reset();
			this.ProcessValidate( true );

			bool success = ((sf.ValidationContainerAccessor)this.Validation).ChildValidationSuccess;

			if( success )
			{
				//Remote Group GroupMembership should only be managed in the true source, not Suplex
				api.SecurityPrincipalBase sp = (api.SecurityPrincipalBase)this.SourceObject;
				if( !sp.IsUserObject && !sp.IsLocal )
				{
					//api.ObservableObjectModelCollection<api.SecurityPrincipalBase> membs =
					//	(api.ObservableObjectModelCollection<api.SecurityPrincipalBase>)dlvMembers.LeftListDataContext;
					api.ObservableObjectModelCollection<api.SecurityPrincipalBase> membs =
						(api.ObservableObjectModelCollection<api.SecurityPrincipalBase>)((CollectionViewSource)dlvMembers.LeftListDataContext).Source;
					for( int i = membs.Count - 1; i > -1; i-- )
					{
						dlvMembers.MoveItemRight( membs[i], true );
					}
				}

				this.OldName = txtName.Text;

				this.SourceObject.IsDirty = false;

				this.OnSavedChanges( this, EventArgs.Empty );
			}

			return success;
		}

		public sf.IObjectModel SourceObject { get; set; }

		public api.SuplexStore SplxStore { get; set; }
		public api.SuplexApiClient ApiClient { get; set; }
		#endregion

		public List<object> MemberOfAdded { get { return dlvMemberOf.ItemsMovedLeft; } }
		public List<object> MemberOfRemoved { get { return dlvMemberOf.ItemsMovedRight; } }
		public List<object> MembersAdded { get { return dlvMembers.ItemsMovedLeft; } }
		public List<object> MembersRemoved { get { return dlvMembers.ItemsMovedRight; } }

		private void cmdOk_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			this.SaveChanges();
		}

		private void cmdCancel_Click(object sender, RoutedEventArgs e)
		{
			this.OnCancelledChanges( cmdCancel, EventArgs.Empty );
		}

		private void spEditor_KeyDown(object sender, KeyEventArgs e)
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

		private void Members_Changed_Left(object sender, EventArgs e)
		{
			this.SourceObject.IsDirty = true;

			//foreach( api.SecurityPrincipalBase sp in dlvMembers.ItemsMovedLeft )
			//{
			//    if( !sp.IsUserObject && _membership.NonMemberList.Contains( (api.Group)sp ) )
			//    {
			//        _membership.NonMemberList.Remove( (api.Group)sp );
			//    }
			//}
		}

		private void Members_Changed_Right(object sender, EventArgs e)
		{
			this.SourceObject.IsDirty = true;

			//foreach( api.SecurityPrincipalBase sp in dlvMembers.ItemsMovedRight )
			//{
			//    if( !sp.IsUserObject &&
			//        !_membership.NonMemberList.Contains( (api.Group)sp ) &&
			//        !_membership.NestedMemberList.Contains( (api.Group)sp ) )
			//    {
			//        _membership.NonMemberList.Add( (api.Group)sp );
			//    }
			//}
		}

		private void Membership_Changed_Left(object sender, EventArgs e)
		{
			this.SourceObject.IsDirty = true;

			//if( _members != null )
			//{
			//    foreach( api.Group g in dlvMemberOf.ItemsMovedLeft )
			//    {
			//        if( _members.NonMemberList.Contains( g ) )
			//        {
			//            _members.NonMemberList.Remove( g );
			//        }
			//    }
			//}
		}

		private void Membership_Changed_Right(object sender, EventArgs e)
		{
			this.SourceObject.IsDirty = true;

			//if( _members != null )
			//{
			//    foreach( api.Group g in dlvMemberOf.ItemsMovedRight )
			//    {
			//        if( !_members.NonMemberList.Contains( g ) )
			//        {
			//            _members.NonMemberList.Add( g );
			//        }
			//    }
			//}
		}

		#region filtering
		string _leftFilterText = string.Empty;
		string _rightFilterText = string.Empty;

		private void dlvMemberOfLeftFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			_leftFilterText = ((TextBox)sender).Text;
			_membership.MemberList.CollectionViewSource.View.Refresh();
		}

		private void dlvMemberOfRightFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			_rightFilterText = ((TextBox)sender).Text;
			_membership.NonMemberList.CollectionViewSource.View.Refresh();
		}

		private void dlvMembersLeftFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			_leftFilterText = ((TextBox)sender).Text;
			_members.MemberList.CollectionViewSource.View.Refresh();
		}

		private void dlvMembersRightFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			_rightFilterText = ((TextBox)sender).Text;
			_members.NonMemberList.CollectionViewSource.View.Refresh();
		}

		bool LeftFilter(object item)
		{
			api.SecurityPrincipalBase sp = item as api.SecurityPrincipalBase;
			return sp.Name.IndexOf( _leftFilterText, StringComparison.OrdinalIgnoreCase ) >= 0;
		}

		bool RightFilter(object item)
		{
			api.SecurityPrincipalBase sp = item as api.SecurityPrincipalBase;
			return sp.Name.IndexOf( _rightFilterText, StringComparison.OrdinalIgnoreCase ) >= 0;
		}
		#endregion
	}
}