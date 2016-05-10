using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Suplex.Data;
using Suplex.Forms;
using System.Text.RegularExpressions;
using Suplex.WinForms;

namespace RlsExampleApp
{
	public partial class MainDlg : sForm
	{
		private DataAccessor _da = null;
		private TraceUtil _trace = new TraceUtil();
		const string data = "lkjdfopijnqwrvononoribnipevubpqeiurhpioqjenrvp983hpt9un3p9v8hr-98gb45u9gb-9qr7bv9[ub4v94q78gh9[345ngv-[9ervb[brv-[9br-[g9qurv-[9rpibvpaiufbvpiabrf0v87w4b5fubq7rtv245viuvgqiub5g[qo3iangao[ibervq9374fvuqpiaubf-a9wefvbaer9-bgvpeu9rg-9av8b-r76qw243tfga09874gb08tqvb0374gva7rnfef374vf334tf6vq834tvf7893nod4q78gh9[345ngv-[9ervb[brv-[9br-[g9qurv-[9rpibvpaiufbvpiabrf0v87w4b5fubq7rtv245viuvgqiub5g[qo3iangao[ibervq9374fvvgqiub5g[qo3iangao[ibervq9374fvuqpiaubf-a9wefvbaer9-bgvpeu9rg-9av8b-r76qw243tv245viuvgq";
		int _maskSize = 128;
		int _totalRows = 0;

		private PerfData _pdMasked = null;
		private PerfData _pdNative = null;

		public MainDlg()
		{
			InitializeComponent();

			this.SetupStuff();
		}

		private void SetupStuff()
		{
			ConnectionProperties splxCp = new ConnectionProperties( "(local)", "Suplex_Example", "suplex_example_admin", "testpassword" );
			DataAccessor splx = new DataAccessor( splxCp.ConnectionString );

			string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
			SecurityLoadParameters securityLoadParameters = new SecurityLoadParameters()
			{
				ExternalGroupInfo = new ExternalGroupInfo( null, true, "Everyone,Power Users" ),
				User = new Suplex.Security.Standard.User( userName, string.Empty )
			};
			this.Security.Load( splx, securityLoadParameters );


			ConnectionProperties cp = new ConnectionProperties( "(local)", "Suplex_Example" );
			_da = new DataAccessor( cp.ConnectionString );

			DataSet ds = _da.GetDataSet( "SELECT * FROM FooLookup" );
			lstLookupData.DisplayMember = "Lookup_Data";
			lstLookupData.ValueMember = "Foo_Lookup_Id";
			lstLookupData.DataSource = ds.Tables[0];
		}

		private void MainDlg_Load(object sender, EventArgs e)
		{
			_pdMasked = new PerfData();
			_pdNative = new PerfData();

			_totalRows = this.GetRowCount();
			if( _totalRows > 0 )
			{
				//DataSet ds = _da.GetDataSet( RlsExampleApp.Properties.Resources.fieldInfo );
				//_maskSize = (int)ds.Tables[0].Rows[0]["character_maximum_length"];
				DataSet ds = _da.GetDataSet( "select top 1 rls_mask from foodata" );
				_maskSize = ( (byte[])ds.Tables[0].Rows[0]["rls_mask"] ).Length;
			}

			txtMaskSize.Text = _maskSize.ToString();
			this.SetMaskSize();

			txtRowCount.Text = ( 10000 ).ToString();
		}

		private void cmdExecute_Click(object sender, EventArgs e)
		{
			string sp = "sel_foodata_nomask";
			TextBox txtLast = txtLastNative;
			TextBox txtAvg = txtAvgNative;
			Label lblAvg = lblAvgNonMasked;
			PerfData pd = _pdNative;
			sSortedList parms = null;


			Random r = new Random( (int)DateTime.Now.Ticks );

			if( ((Button)sender).Name == cmdQueryMasked.Name )
			{
				sp = "sel_foodata_withmask";
				txtLast = txtLastMasked;
				txtAvg = txtAvgMasked;
				lblAvg = lblAverageMasked;
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
				//slMask.Text = string.Format( "Last Mask: {0}", s.ToString() );
			}
			else
			{
				int topN = _totalRows;
				if( _pdMasked.LastRowCount > 0 ) { topN = _pdMasked.LastRowCount; }
				parms = new sSortedList( "@topN", topN );
			}


			_trace.IsEnabled = true;
			_trace.WriteKeyed( "GetData", null, "start", null );
			DataSet ds = _da.GetDataSet( sp, parms );
			_trace.WriteKeyed( "GetData", null, "stop", null );
			_trace.IsEnabled = false;
			_trace.ProcessKeyDeltas();

			txtLast.Text = string.Format( "Time: {1},  Rows: {0}",
				ds.Tables[0].Rows.Count, _trace.TraceRecords[_trace.TraceRecords.Count - 1].KeyDelta );

			txtAvg.Text =
				pd.ReCalc( ds.Tables[0].Rows.Count, _trace.TraceRecords[_trace.TraceRecords.Count - 1].KeyDelta );
			lblAvg.Text = string.Format( "Average after {0} executions:", pd.ExecutionCount );

			Application.DoEvents();
			//dgvResults.DataSource = ds.Tables[0];
		}

		private void cmdExecuteBoth_Click(object sender, EventArgs e)
		{
			_pdMasked.Reset();
			_pdNative.Reset();

			pbCreateData.Value = 0;
			pbCreateData.Minimum = 0;
			pbCreateData.Maximum = (int)udExecuteBothCount.Value;
			pbCreateData.Visible = true;

			for( int i = 0; i < udExecuteBothCount.Value; i++ )
			{
				this.cmdExecute_Click( cmdQueryMasked, EventArgs.Empty );

				Application.DoEvents();

				this.cmdExecute_Click( cmdQueryNonMasked, EventArgs.Empty );

				pbCreateData.Value = i;
				status.Refresh();
				Application.DoEvents();
			}

			pbCreateData.Visible = false;
		}

		private void cmdCreateData_Click(object sender, EventArgs e)
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
					resetSize = txtMaskSize.Text;
					if( chkDDLMax.Checked )
					{
						resetSize = "max";
					}

					maskSizeChanged = _maskSize != maskSize;
					_maskSize = maskSize;
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

			this.SetMaskSize();

			if( chkExecuteDDL.Checked && maskSizeChanged )
			{
				//string sql = "RlsExampleApp.Properties.Resources.binFields";
				//sql = Regex.Replace( sql, "__masksize__", resetSize );

				//int last = 0;
				//string nq = string.Empty;
				//MatchCollection matches = Regex.Matches( sql, "go" );
				//_da.OpenConnection();
				//foreach( Match m in matches )
				//{
				//    nq = sql.Substring( last, m.Index - last );
				//    _da.ExecuteNonQuery( nq, false, null );
				//    last = m.Index + 2;
				//}
				//_da.CloseConnection();
			}
			else
			{
				if( chkClearFirst.Checked )
				{
					_da.ExecuteSP( "del_foodata_trunc", null );
				}
			}


			Random r = new Random( (int)DateTime.Now.Ticks );

			pbCreateData.Minimum = 0;
			pbCreateData.Maximum = rowCount;

			pbCreateData.Visible = true;
			slMaskSize.Visible = slRows.Visible =
				slMask.Visible = false;

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
				parms["@foo_lookup_id"] = r.Next( 1, 11 );	//11 is an exclusive value, the actual max value ot be inserted is 104
				_da.ExecuteSP( "ins_foodata", parms, false );
				pbCreateData.Value = i;
				status.Refresh();
				Application.DoEvents();
			}
			_da.CloseConnection();

			_pdMasked.Reset();
			_pdNative.Reset();
			_totalRows = this.GetRowCount();

			pbCreateData.Visible = false;
			slMaskSize.Visible = slRows.Visible =
				slMask.Visible = true;
		}

		private int GetRowCount()
		{
			DataSet ds = _da.GetDataSet( "select count(*) 'count' from foodata" );
			int count = (int)ds.Tables[0].Rows[0]["count"];
			slRows.Text = string.Format( "Rows: {0}", count );
			return count;
		}

		private void SetMaskSize()
		{
			slMaskSize.Text = string.Format( "Mask Size: {0}", _maskSize );
		}

		private void lstLookupData_SelectedIndexChanged(object sender, EventArgs e)
		{
			if( lstLookupData.SelectedItem != null )
			{
				DataRow r = ((DataRowView)lstLookupData.SelectedItem).Row;
				DataSet ds = _da.GetDataSet( "sel_foolookup_row_permissions", new sSortedList( "@foo_lookup_id", r["foo_lookup_id"] ) );
				_da.NameTablesFromCompositeSelect( ref ds );

				dlMembers.LeftDataSource = ds.Tables["GroupMembers"];
				dlMembers.RightDataSource = ds.Tables["GroupNonMembers"];
			}
		}

		private void cmdApply_Click(object sender, EventArgs e)
		{
			BitArray bits = new BitArray( 128 );
			foreach( DataRowView r in dlMembers.LeftList.Items )
			{
				bits.Or( new BitArray( (byte[])r.Row["group_mask"] ) );
			}
			byte[] mask = new byte[128 / 8];	//8 bits per byte
			bits.CopyTo( mask, 0 );
			_da.ExecuteSP( "[upd_foolookup_row_permissions]",
				new sSortedList(
					"@rls_mask", mask,
					"@foo_lookup_id", ((DataRowView)lstLookupData.SelectedItem).Row["foo_lookup_id"] ) );
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

		public override string ToString()
		{
			return string.Format( "Time: {1},  Rows: {0}",
				_rows / _count,
				Decimal.Round( (decimal)( _execTime / _count ), 6 ) );
		}
	}
}