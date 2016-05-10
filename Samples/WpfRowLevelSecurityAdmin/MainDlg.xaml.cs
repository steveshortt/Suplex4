using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Forms.ObjectModel.Api;
using ss = Suplex.Security.Standard;


namespace WpfRowLevelSecurityAdmin
{
	public partial class MainDlg : Window
	{
		private SuplexApiClient _apiClient = null;
		private DataSet _securityCache = null;
		private SecurityLoadParameters _securityLoadParameters = null;

		bool _suppressGrid = false;

		private DataAccessor _da = null;
		private TraceUtil _trace = new TraceUtil();
		const string data = "lkjdfopijnqwrvononoribnipevubpqeiurhpioqjenrvp983hpt9un3p9v8hr-98gb45u9gb-9qr7bv9[ub4v94q78gh9[345ngv-[9ervb[brv-[9br-[g9qurv-[9rpibvpaiufbvpiabrf0v87w4b5fubq7rtv245viuvgqiub5g[qo3iangao[ibervq9374fvuqpiaubf-a9wefvbaer9-bgvpeu9rg-9av8b-r76qw243tfga09874gb08tqvb0374gva7rnfef374vf334tf6vq834tvf7893nod4q78gh9[345ngv-[9ervb[brv-[9br-[g9qurv-[9rpibvpaiufbvpiabrf0v87w4b5fubq7rtv245viuvgqiub5g[qo3iangao[ibervq9374fvvgqiub5g[qo3iangao[ibervq9374fvuqpiaubf-a9wefvbaer9-bgvpeu9rg-9av8b-r76qw243tv245viuvgq";
		int _maskSize = 0;
		int _totalRows = 0;

		private PerfData _pdMasked = new PerfData();
		private PerfData _pdNative = new PerfData();

		List<User> _users = new List<User>();


		public MainDlg()
		{
			InitializeComponent();
			this.SetupSuplex_Api();
		}

		private void SetupSuplex_Api()
		{
			Properties.Settings s = Properties.Settings.Default;
			ConnectionProperties cp = new ConnectionProperties( s.DatabaseServer, s.DatabaseName );
			_da = new DataAccessor( cp.ConnectionString );


			string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
			ss.User user = new Suplex.Security.Standard.User( userName, true, _da );
			user.ResolveByName();

			_securityLoadParameters = new SecurityLoadParameters()
			{
				ExternalGroupInfo = new ExternalGroupInfo( null, true, "Everyone,Power Users" ),
				User = user
			};

			//_apiClient = new SuplexApiClient( "http://localhost:10712/SuplexApi.svc", WebMessageFormatType.Json );
			_apiClient = new SuplexApiClient( cp.ConnectionString );
			layoutRoot.Security.Clear( true );
			_securityCache = layoutRoot.Security.Load( _da, _securityLoadParameters );

			//SplxDiagnosticInfoDlg diagDlg = new SplxDiagnosticInfoDlg();
			//diagDlg.SetControl( layoutRoot );


			this.lookupPerms_Saved( null, EventArgs.Empty );
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_pdMasked = new PerfData();
			_pdNative = new PerfData();

			_totalRows = this.GetRowCount();
			if( _totalRows > 0 )
			{
				DataSet ds = _da.GetDataSet( Properties.Resources.tableInfo );
				DataRow[] rows = ds.Tables[0].Select( "COLUMN_NAME = 'rls_mask'" );
				_maskSize = (int)rows[0]["character_maximum_length"] * 8;

				//DataSet ds = _da.GetDataSet( "select top 1 rls_mask from foodata" );
				//_maskSize = ((byte[])ds.Tables[0].Rows[0]["rls_mask"]).Length;
			}

			txtMaskSize.Text = _maskSize.ToString();
			this.SetMaskSizeStatusLabel();

			txtRowCount.Text = (10000).ToString();

			lookupPerms.Initialize( _da );
		}

		private void cmdQuery_Click(object sender, RoutedEventArgs e)
		{
			string sp = null;
			TextBox txtLast = txtLastNative;
			TextBox txtAvg = txtAvgNative;
			TextBlock lblAvg = tbAvgNonMasked;
			PerfData pd = null;
			sSortedList parms = null;

			bool execSp = true;

			Random r = new Random( (int)DateTime.Now.Ticks );

			if( ((Button)sender).Name == cmdQueryMasked.Name )
			{
				sp = "sel_foodata_withmask";
				txtLast = txtLastMasked;
				txtAvg = txtAvgMasked;
				lblAvg = tbAverageMasked;
				pd = _pdMasked;

				byte[] buf = new byte[_maskSize];
				for( int m = 0; m < 5; m++ )
				{
					buf[r.Next( 0, _maskSize - 1 )] = 1;
				}
				parms = new sSortedList( "@rls_mask", buf );

				StringBuilder s = new StringBuilder( _maskSize );
				for( int i = 0; i < _maskSize; i++ )
				{
					s.Append( buf[i].ToString( "X" ) );
				}
				slMask.Text = s.ToString();	// string.Format( "Last Mask: {0}", s.ToString() );
				//doesn't work: string.Format( "Last Mask: {0}", System.Text.Encoding.Unicode.GetString( buf ) );
			}
			else
			{
				////int topN = _totalRows;
				//////if( _pdMasked.LastRowCount > 0 ) {  }
				////topN = _pdMasked.LastRowCount;
				////parms = new sSortedList( "@topN", topN );

				execSp = false;
				if( _pdMasked.LastResult != null )
				{
					execSp = _pdMasked.LastResult.Rows.Count > 0;
				}
				sp = "sel_foodata_nomask_tvp";
				pd = _pdNative;

				SqlParameter tvp = new SqlParameter( "@TVP", SqlDbType.Structured );
				tvp.Value = _pdMasked.LastResult;
				tvp.TypeName = "dbo.FooData_Tvp";

				parms = new sSortedList( "@TVP", tvp );
			}


			if( execSp )
			{
				_trace.IsEnabled = true;
				_trace.WriteKeyed( "GetData", null, "start", null );
				DataSet ds = _da.GetDataSet( sp, parms );
				_trace.WriteKeyed( "GetData", null, "stop", null );
				_trace.IsEnabled = false;
				_trace.ProcessKeyDeltas();

				pd.LastResult = ds.Tables[0];

				execSp = _pdMasked.LastResult.Rows.Count > 0;
				if( execSp )
				{
					txtLast.Text = string.Format( "Time: {1},  Rows: {0}",
						ds.Tables[0].Rows.Count, _trace.TraceRecords[_trace.TraceRecords.Count - 1].KeyDelta );

					txtAvg.Text =
						pd.ReCalc( ds.Tables[0].Rows.Count, _trace.TraceRecords[_trace.TraceRecords.Count - 1].KeyDelta );
					lblAvg.Text = string.Format( "Average after {0} executions:", pd.ExecutionCount );
				}

				if( !_suppressGrid )
				{
					lstResults.DataContext = ds.Tables[0];
				}
			}
		}

		private void cmdQueryBoth_Click(object sender, RoutedEventArgs e)
		{
			_pdMasked.Reset();
			_pdNative.Reset();

			pbCreateData.Value = 0;
			pbCreateData.Minimum = 0;
			pbCreateData.Maximum = int.Parse( txtExecuteBothCount.Text );
			sbiCreateData.Visibility = Visibility.Visible;

			_suppressGrid = true;
			for( int i = 0; i < (int)pbCreateData.Maximum; i++ )
			{
				this.cmdQuery_Click( cmdQueryMasked, null );
				this.cmdQuery_Click( cmdQueryNonMasked, null );

				Dispatcher.Invoke(
					DispatcherPriority.Background,
					(Action)delegate() { pbCreateData.Value = i; } );
			}
			_suppressGrid = false;

			sbiCreateData.Visibility = Visibility.Collapsed;
		}

		private void cmdCreateData_Click(object sender, RoutedEventArgs e)
		{
			bool ok = false;
			int maskSize = 0;
			int rowCount = 0;
			bool maskSizeChanged = false;
			string resetSize = null;

			if( !string.IsNullOrEmpty( txtMaskSize.Text ) )
			{
				ok = Int32.TryParse( txtMaskSize.Text, out maskSize );
				if( ok )
				{
					ok = (maskSize % 8) == 0;
					if( ok )
					{
						resetSize = (maskSize / 8).ToString();
						if( chkDDLMax.IsChecked.Value )
						{
							resetSize = "max";
						}

						maskSizeChanged = _maskSize != maskSize;
						_maskSize = maskSize;
					}
				}
			}

			if( ok && !string.IsNullOrEmpty( txtRowCount.Text ) )
			{
				ok = Int32.TryParse( txtRowCount.Text, out rowCount );
			}

			if( !ok )
			{
				MessageBox.Show( "Mask Size and Row Count are required and must be integers.", "Error" );
				return;
			}

			this.SetMaskSizeStatusLabel();

			if( chkClearFirst.IsChecked.Value )
			{
				_da.ExecuteSP( "del_foodata_trunc", null );
			}

			if( chkExecuteDDL.IsChecked.Value && maskSizeChanged )
			{
				string sql = Properties.Resources.ddl;
				sql = Regex.Replace( sql, "__masksize__", resetSize );

				int last = 0;
				string nq = string.Empty;
				MatchCollection matches = Regex.Matches( sql, "GO" );
				_da.OpenConnection();
				foreach( Match m in matches )
				{
					nq = sql.Substring( last, m.Index - last );
					_da.ExecuteNonQuery( nq, false, null );
					last = m.Index + 2;
				}
				_da.CloseConnection();
			}


			Random r = new Random( (int)DateTime.Now.Ticks );

			pbCreateData.Minimum = 0;
			pbCreateData.Maximum = rowCount;

			sbiCreateData.Visibility = Visibility.Visible;
			slMaskSize.Visibility = slRows.Visibility =
				slMask.Visibility = Visibility.Collapsed;

			_da.OpenConnection();
			sSortedList parms = new sSortedList( "@rls_mask", null, "@data", null, "@foo_lookup_id", null );
			for( int i = 0; i < rowCount; i++ )
			{
				byte[] buf = new byte[maskSize];
				for( int m = 0; m < 5; m++ )
				{
					buf[r.Next( 0, maskSize - 1 )] = 1;
				}

				parms["@rls_mask"] = buf;
				parms["@data"] = data.Substring( r.Next( 450 ), 50 );
				parms["@foo_lookup_id"] = r.Next( 1, 11 );	//11 is an exclusive value, the actual max value to be inserted is 10
				_da.ExecuteSP( "ins_foodata", parms, false );

				Dispatcher.Invoke(
					DispatcherPriority.Background,
					(Action)delegate() { pbCreateData.Value = i; } );
			}
			_da.CloseConnection();

			_pdMasked.Reset();
			_pdNative.Reset();
			_totalRows = this.GetRowCount();

			sbiCreateData.Visibility = Visibility.Collapsed;
			slMaskSize.Visibility = slRows.Visibility =
				slMask.Visibility = Visibility.Visible;
		}

		private void lookupPerms_Saved(object sender, EventArgs e)
		{
			LookupItemFactory lookupItemFactory = new LookupItemFactory();
			DataSet ds = _da.GetDataSet( "SELECT * FROM FooLookup" );
			lstLookupData.DataContext = lookupItemFactory.FromDataTable( ds.Tables[0] );
		}

		private int GetRowCount()
		{
			DataSet ds = _da.GetDataSet( "select count(*) 'count' from foodata" );
			int count = (int)ds.Tables[0].Rows[0]["count"];
			slRows.Text = string.Format( "Rows: {0}", count );
			return count;
		}

		private void SetMaskSizeStatusLabel()
		{
			slMaskSize.Text = string.Format( "Mask Size: {0}", _maskSize );
		}

		private void lstLookupData_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if( lstLookupData.SelectedItem != null )
			{
				lookupPerms.SetDataContext( lstLookupData.SelectedItem as LookupItem );
			}
		}

		private void lstLookupData_SelectedIndexChanged(object sender, EventArgs e)
		{
			if( lstLookupData.SelectedItem != null )
			{
				DataRow r = ((DataRowView)lstLookupData.SelectedItem).Row;
				DataSet ds = _da.GetDataSet( "sel_foolookup_row_permissions", new sSortedList( "@foo_lookup_id", r["foo_lookup_id"] ) );
				_da.NameTablesFromCompositeSelect( ref ds );

				//dlMembers.LeftDataSource = ds.Tables["GroupMembers"];
				//dlMembers.RightDataSource = ds.Tables["GroupNonMembers"];
			}
		}

		private void cmdApply_Click(object sender, EventArgs e)
		{
			//BitArray bits = new BitArray( 128 );
			//foreach( DataRowView r in dlMembers.LeftList.Items )
			//{
			//    bits.Or( new BitArray( (byte[])r.Row["group_mask"] ) );
			//}
			//byte[] mask = new byte[128 / 8];	//8 bits per byte
			//bits.CopyTo( mask, 0 );
			//_da.ExecuteSP( "[upd_foolookup_row_permissions]",
			//    new sSortedList(
			//        "@rls_mask", mask,
			//        "@foo_lookup_id", ((DataRowView)lstLookupData.SelectedItem).Row["foo_lookup_id"] ) );
		}
	}

	public class PerfData
	{
		private int _count = 0;
		private decimal _execTime = 0;
		private int _rows = 0;


		public string ReCalc(int rows, decimal execTime)
		{
			_count++;
			_rows += rows;
			_execTime += execTime;

			this.LastRowCount = rows;

			return this.ToString();
		}

		public void Reset()
		{
			_count = 0;
			_rows = 0;
			_execTime = 0;
		}

		public int ExecutionCount { get { return _count; } }
		public int AverageRowCount { get { return _rows; } }
		public int LastRowCount { get; protected set; }

		public DataTable LastResult { get; set; }

		public override string ToString()
		{
			return string.Format( "Time: {1},  Rows: {0}",
				_rows / _count,
				Decimal.Round( (decimal)(_execTime / _count), 6 ) );
		}
	}

	public class LookupItem
	{
		public int Id { get; set; }
		public byte[] RlsMaskValue { get; set; }
		public string RlsMask { get; set; }
		public string Value { get; set; }
	}

	public class LookupItemFactory
	{
		public List<LookupItem> FromDataTable(DataTable t)
		{
			List<LookupItem> items = new List<LookupItem>();
			foreach( DataRow r in t.Rows )
			{
				items.Add( this.FromDataRow( r ) );
			}
			return items;
		}
		public LookupItem FromDataRow(DataRow r)
		{
			LookupItem item = new LookupItem()
			{
				Id = (int)r["foo_lookup_id"],
				RlsMaskValue = (byte[])r["rls_mask"],
				Value = r["lookup_data"].ToString()
			};

			int maskSize = item.RlsMaskValue.Length;
			StringBuilder s = new StringBuilder( maskSize );
			for( int i = 0; i < maskSize; i++ )
			{
				s.Insert( 0, item.RlsMaskValue[i].ToString( "X" ) );
			}
			item.RlsMask = s.ToString();

			return item;
		}
	}
}