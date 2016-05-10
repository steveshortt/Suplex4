using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using api = Suplex.Forms.ObjectModel.Api;
using ss = Suplex.Security;

namespace SuplexApp
{
	public partial class AceRightsCtrl : UserControl
	{
		private Assembly _splxCore = null;

		private bool _isAuditAce = false;

		private ss.SecurityDescriptor _sd = new ss.SecurityDescriptor();
		private api.AccessControlEntryBase _ace = null;

		private bool _suppressRightsEval = false;
		private bool _suppressRightSelectionChanged = false;
		private bool _suppressCommit = false;
		private bool _suppressPropCommit = false;


		#region ctors, dlg handlers
		public AceRightsCtrl()
		{
			InitializeComponent();

			_splxCore = Assembly.Load( "Suplex.Core" );

			_ace = new api.UIAce();

			ArrayList aceTypes = new ArrayList( Enum.GetValues( typeof( ss.AceType ) ) );
			aceTypes.Remove( ss.AceType.None );
			aceTypes.Remove( ss.AceType.Native );
			lstAceType.ItemsSource = aceTypes;
		}
		#endregion

		#region public accessors
		public static readonly DependencyProperty AceProperty = DependencyProperty.Register( "Ace", typeof( api.AccessControlEntryBase ), typeof( AceRightsCtrl ) );
		public api.AccessControlEntryBase Ace
		{
			get { return GetValue( AceProperty ) as api.AccessControlEntryBase; }
			set { SetValue( AceProperty, value ); }
		}

		public static readonly DependencyProperty AceTypeProperty = DependencyProperty.Register( "AceType", typeof( ss.AceType ), typeof( AceRightsCtrl ) );
		public object AceType
		{
			get { return GetValue( AceTypeProperty ) as object; }
			set { SetValue( AceTypeProperty, value ); }
		}

		public static readonly DependencyProperty AceRightProperty = DependencyProperty.Register( "AceRight", typeof( object ), typeof( AceRightsCtrl ) );
		public object AceRight
		{
			get { return GetValue( AceRightProperty ) as object; }
			set { SetValue( AceRightProperty, value ); }
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if( !_suppressPropCommit )
			{
				if( e.Property == AceProperty )
				{
					this.InitDlg( (api.AccessControlEntryBase)e.NewValue );
				}
				else if( e.Property == AceTypeProperty )
				{
					_ace.AceType = (ss.AceType)e.NewValue;
					lstAceType.SelectedItem = (ss.AceType)e.NewValue;
				}
				else if( e.Property == AceRightProperty )
				{
					_ace.Right = e.NewValue;
					lstAceType.SelectedItem = e.NewValue;
					this.SetCheckedRights( _ace );
				}
			}

			base.OnPropertyChanged( e );
		}


		public void InitDlg(api.AccessControlEntryBase ace)
		{
			_suppressCommit = true;

			bool haveAce = ace != null;

			if( haveAce )
			{
				lstAceType.SelectedItem = null;
				lstAceType.SelectedItem = ace.AceType;
				//this.SetCheckedRights( ace );
            }
			else
			{
				lstAceType.SelectedItem = null;
				lstAceType.SelectedItem = _ace.AceType;
			}

			this.CheckReady();

			_suppressCommit = false;
		}
		#endregion


		#region acetype, rights
		private void lstAceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if( lstAceType.SelectedItem != null )
			{
				_suppressRightsEval = true;
				//lstRight.ItemsSource = ss.AceTypeRights.GetRights( (ss.AceType)lstAceType.SelectedValue ).Reverse();
				lstRight.Items.Clear();
				IEnumerable<object> rights = ss.AceTypeRights.GetRights( (ss.AceType)lstAceType.SelectedValue ).Reverse();
				foreach( object right in rights )
				{
					CheckBox cb = new CheckBox();
					cb.Content = right;
					cb.Checked += new RoutedEventHandler( Rights_CheckChanged );
					cb.Unchecked += new RoutedEventHandler( Rights_CheckChanged );
					lstRight.Items.Add( cb );
				}
				//lstRight.SelectedIndex = -1;
				_suppressRightsEval = false;

				long? id = null;
				if( _ace != null ) { id = _ace.Id; }

				Type aceType = _splxCore.GetType(
					string.Format( "Suplex.Forms.ObjectModel.Api.{0}{1}Ace",
					(ss.AceType)lstAceType.SelectedValue, _isAuditAce ? "Audit" : string.Empty ) );
				_ace = (api.AccessControlEntryBase)Activator.CreateInstance( aceType );

				if( id != null ) { _ace.Id = (long)id; }
			}

			if( _ace != null ) { _ace.IsDirty = true; }

			this.CheckReady();
		}

		private void SetCheckedRights(api.AccessControlEntryBase ace)
		{
			_suppressRightsEval = true;
			foreach( CheckBox cb in lstRight.Items )
			{
				cb.IsChecked =
					( (int)ace.Right & (int)cb.Content ) == (int)cb.Content;
			}
			_suppressRightsEval = false;
		}

		private int GetCheckedRights()
		{
			int mask = 0;

			foreach( CheckBox cb in lstRight.Items )
			{
				if( cb.IsChecked == true )
				{
					mask |= (int)cb.Content;
				}
			}

			return mask;
		}

		private void lstRight_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if( !_suppressRightSelectionChanged && ( (ListBox)sender ).SelectedItem != null )
			{
				CheckBox cb = (CheckBox)( (ListBox)sender ).SelectedItem;
				if( cb.IsChecked == true )
				{
					cb.IsChecked = false;
				}
				else
				{
					cb.IsChecked = true;
				}
			}
		}

		void Rights_CheckChanged(object sender, RoutedEventArgs e)
		{
			if( !_suppressRightsEval )
			{
				int allowedMask = 0;
				int deniedMask = 0;

				ListBoxItem item = (ListBoxItem)lstRight.ItemContainerGenerator.ContainerFromItem( sender );
				if( item != null )
				{
					_suppressRightSelectionChanged = true;
					item.IsSelected = true;
					_suppressRightSelectionChanged = false;
				}

				if( ( (CheckBox)sender ).IsChecked == false )
				{
					deniedMask |= (int)( (CheckBox)sender ).Content;
				}

				foreach( CheckBox cb in lstRight.Items )
				{
					if( cb.IsChecked == true )
					{
						allowedMask |= (int)cb.Content;
					}
				}

				this.EvalRights( allowedMask, deniedMask );

				if( _ace != null ) { _ace.IsDirty = true; }

				this.CheckReady();
			}
		}

		private void EvalRights(int allowedMask, int deniedMask)
		{
			_sd.Clear();
			Type aceType = _splxCore.GetType( string.Format( "Suplex.Security.{0}Ace", _ace.AceType ) );

			ss.IAccessControlEntry allowedAce = (ss.IAccessControlEntry)Activator.CreateInstance( aceType );
			allowedAce.Allowed = true;
			allowedAce.Right = allowedMask;

			ss.IAccessControlEntry deniedAce = (ss.IAccessControlEntry)Activator.CreateInstance( aceType );
			deniedAce.Allowed = false;
			deniedAce.Right = deniedMask;


			_sd.Dacl.Add( 0, allowedAce );
			_sd.Dacl.Add( 1, deniedAce );
			_sd.EvalSecurity( _ace.AceType );

			//suppress reentrancy into this function: IsChecked=true fires CheckBox_Checked
			_suppressRightsEval = true;
			foreach( CheckBox cb in lstRight.Items )
			{
				cb.IsChecked = _sd.SecurityResults[_ace.AceType, cb.Content].AccessAllowed;
			}
			_suppressRightsEval = false;
		}

		//the aceType parm could just as esaily be the global _ace.AceType;
		//	parmed it to enfore order operations.
		[Obsolete( "Doesn't seem to be necessary and is not in use, consider removing fn." )]
		private void ResetSecurityDescriptor(ss.AceType aceType)
		{
			_sd.Clear();
			_sd.SecurityResults.InitAceType( aceType );
		}

		private void lvSecurityGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if( _ace != null ) { _ace.IsDirty = true; }
			this.CheckReady();
		}

		private void tvUIElements_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if( _ace != null ) { _ace.IsDirty = true; }
			this.CheckReady();
		}

		private void CheckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
		{
			if( _ace != null ) { _ace.IsDirty = true; }
			this.CheckReady();
		}
		#endregion


		#region CheckReady/CommitAceChanges
		private bool CheckReady()
		{
			bool ok = lstAceType.SelectedItem != null && this.GetCheckedRights() > 0;
			if( ok ) { this.CommitAceChanges(); }
			return ok;
		}

		private void CommitAceChanges()
		{
			System.Diagnostics.Debug.WriteLine( string.Format( "_ace.IsDirty: {0}", _ace.IsDirty ) );
			if( !_suppressCommit && _ace.IsDirty )
			{
				int mask = this.GetCheckedRights();
				switch( _ace.AceType )
				{
					case ss.AceType.UI:
					{
						_ace.Right = (ss.UIRight)mask;
						break;
					}
					case ss.AceType.Record:
					{
						_ace.Right = (ss.RecordRight)mask;
						break;
					}
					case ss.AceType.FileSystem:
					{
						_ace.Right = (ss.FileSystemRight)mask;
						break;
					}
					case ss.AceType.Synchronization:
					{
						_ace.Right = (ss.SynchronizationRight)mask;
						break;
					}
				}

				_suppressPropCommit = true;
				AceType = _ace.AceType;
				AceRight = _ace.Right;
				Ace = _ace.CloneMemberwise();
				_suppressPropCommit = false;
			}
		}
		#endregion
	}
}