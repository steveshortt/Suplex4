using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Suplex.Data;
using System.Collections;
using System.Data;
using Suplex.Forms.ObjectModel.Api;
using System.Collections.ObjectModel;

namespace WpfRowLevelSecurityAdmin
{
	public partial class LookupPermissionsDlg : UserControl
	{
		DataAccessor _da = null;

		#region events
		public event EventHandler Saved;
		protected void OnSaved()
		{
			if( Saved != null )
			{
				this.Saved( this, EventArgs.Empty );
			}
		}
		#endregion

		public LookupPermissionsDlg()
		{
			InitializeComponent();
		}

		public void Initialize(DataAccessor da)
		{
			_da = da;
		}

		public void SetDataContext(LookupItem item)
		{
			this.DataContext = item;

			DataSet ds = _da.GetDataSet( "sel_foolookup_row_permissions", new sSortedList( "@foo_lookup_id", item.Id ) );
			_da.NameTablesFromCompositeSelect( ref ds );

			ObservableCollection<Group> members = new ObservableCollection<Group>();
			ObservableCollection<Group> nonmembers = new ObservableCollection<Group>();
			GroupFactory factory = new GroupFactory();

			foreach( DataRow r in ds.Tables["GroupMembers"].Rows )
			{
				members.Add( factory.CreateObject( r ) );
			}
			foreach( DataRow r in ds.Tables["GroupNonMembers"].Rows )
			{
				nonmembers.Add( factory.CreateObject( r ) );
			}

			dlvMembership.LeftListDataContext = members;
			dlvMembership.RightListDataContext = nonmembers;
		}

		private void cmdOk_Click(object sender, RoutedEventArgs e)
		{
			BitArray mask = new BitArray(128);
			byte[] sqlMask = new byte[128];

			ObservableCollection<Group> members = dlvMembership.LeftListDataContext as ObservableCollection<Group>;
			foreach( Group g in members )
			{
				mask.Or( g.Mask );
			}
			mask.CopyTo( sqlMask, 0 );

			_da.ExecuteSP( "upd_foolookup_row_permissions",
				new sSortedList( "@foo_lookup_id", ((LookupItem)this.DataContext).Id,
					"@rls_mask", sqlMask ) );

			this.OnSaved();
		}
	}
}
