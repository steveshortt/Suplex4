using System;
using System.Drawing;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;
using Suplex.Security.Standard;
using Suplex.General;
using System.Text;


namespace Suplex.WinForms
{
	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	[ToolboxItem( true ), ToolboxBitmap( typeof( ListView ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.Click | ControlEvents.SelectedIndexChanged )]
	public class sListView : System.Windows.Forms.ListView, IValidationControl
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationAccessor _va = null;

		private OperateLevel _operateLevel = OperateLevel.None;
		private int[] _lastSelectedIndices = null;

		#region databinding vars
		private bool _forceColumnRebuild = false;
		private object _dataSource = null;
		private DataView _data = null;
		private sListView.sColumnHeaderCollection _sColumnHeaders;
		private sListView.sListViewItemCollection _items = null;
		internal const string __lvDefaultKey = "lvDefaultKey";
		#endregion


		public sListView() : base()
		{
			#region databinding setup
			_data = new DataView( new DataTable() );
			_sColumnHeaders = new sColumnHeaderCollection( this );
			_items = new sListViewItemCollection( this );
			_sColumnHeaders.Data = _data;
			_items.Data = _data;
			#endregion

			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationAccessor( this, TypeCode.String );
		}

		protected override void InitLayout()
		{
			_sa.EnsureDefaultState();

			base.InitLayout();
		}


		[ParenthesizePropertyName( true ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public string UniqueName
		{
			get
			{
				return string.IsNullOrEmpty( _uniqueName ) ? base.Name : _uniqueName;
			}
			set
			{
				_uniqueName = value;
			}
		}

		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Suplex.Data.DataAccessLayer DataAccessLayer
		{
			get { return _dal; }
			set { _dal = value; }
		}


		#region Validation Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Validation management properties and tools." )]
		public IValidationAccessor Validation
		{
			get { return _va; }
		}

		public virtual ValidationResult ProcessValidate(bool processFillMaps)
		{
			ValidationResult vr = new ValidationResult( this.UniqueName );
			if( this.Enabled )
			{
				vr = _va.ProcessEvent( this.Text, ControlEvents.Validating, processFillMaps );
			}
			return vr;
		}

		protected override void OnValidating(CancelEventArgs e)
		{
			// Reset the selected items to whatever they were before
			// Operate right was denied. Other controls perform this
			// task in OnSelectedIndexChanged event.
			if( !_sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				this.SelectedItems.Clear();
				if( _lastSelectedIndices != null )
				{
					foreach( int index in _lastSelectedIndices )
					{
						this.Items[index].Selected = true;
					}
				}
			}

			e.Cancel = !this.ProcessValidate( true ).Success;

			base.OnValidating( e );
		}

		protected override void OnEnter(EventArgs e)
		{
			_va.ProcessEvent( this.Text, ControlEvents.Enter, true );
			base.OnEnter( e );
		}

		protected override void OnLeave(EventArgs e)
		{
			_va.ProcessEvent( this.Text, ControlEvents.Leave, true );
			base.OnLeave( e );
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				_va.ProcessEvent( this.Text, ControlEvents.EnabledChanged, true );
				//_sa.AuditAccess( this, AceType.UI, UIRight.Enabled, _auditEventHandler );

				base.OnEnabledChanged( e );
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
			{
				_va.ProcessEvent( this.Text, ControlEvents.VisibleChanged, true );
				//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Visible, _auditEventHandler );

				base.OnVisibleChanged( e );
			}
		}

		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "SelectedItemChanged.", false );

			bool ok = _sr[AceType.UI, UIRight.Operate].AccessAllowed;
			if( !ok )
			{
				ok = ( _operateLevel & OperateLevel.SingleClick ) == OperateLevel.SingleClick;
			}

			if( ok )
			{
				//not using this test b/c using ArrayList implementation
				//	if( this.SelectedIndices != _lastSelectedIndices )
				//_lastSelectedIndices = this.SelectedIndices;
				//Line above doesn't work b/c sets an obect reference, not a value a copy.
				//Soln below works but is pretty freaking inefficient, need a better solution.
				_lastSelectedIndices = null;
				_lastSelectedIndices = new int[this.SelectedIndices.Count];
				this.SelectedIndices.CopyTo( _lastSelectedIndices, 0 );

				if( this.SelectedItems.Count > 0 )
				{
					_va.ProcessEvent( this.SelectedItems[0].Text, ControlEvents.SelectedItemChanged, true );
				}

				base.OnSelectedIndexChanged( e );
			}
			// **********************************************
			// else {...}
			// can't reset SelectedIndex here -- causes this
			// event to fire and hence, circular event firing.
			// see OnValidating for reset-code.
			// **********************************************
		}
		#endregion


		#region Security Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Security management properties and tools." ), Category( "Suplex" )]
		public SecurityAccessor Security
		{
			get { return _sa; }
		}

		public void ApplySecurity()
		{
			if( !this.DesignMode )
			{
				if( !_sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					base.Enabled = false;
				}
				if( !_sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					base.Visible = false;
				}
			}
		}

		[DefaultValue( false )]
		new public bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					base.Visible = value;
				}
			}
		}

		[DefaultValue( false )]
		new public bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					base.Enabled = value;
				}
			}
		}

		[DefaultValue( OperateLevel.None )]
		public OperateLevel OperateLevel
		{
			get { return _operateLevel; }
			set { _operateLevel = value; }
		}

		protected override void OnClick(EventArgs e)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "Clicked.", false );

			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed ||
				( _operateLevel & OperateLevel.SingleClick ) == OperateLevel.SingleClick )
			{
				if( this.SelectedItems.Count > 0 )
				{
					_va.ProcessEvent( this.SelectedItems[0].Text, ControlEvents.Click, true );
				}
				base.OnClick( e );
			}
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "DoubleClicked.", false );

			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed ||
				( _operateLevel & OperateLevel.DoubleClick ) == OperateLevel.DoubleClick )
			{
				_va.ProcessEvent( this.SelectedItems[0].Text, ControlEvents.Click, true );
				base.OnDoubleClick( e );
			}
		}
		#endregion


		#region DataBinding stuff, ListViewItemAccessor, Columns, Items

		public DataTable ConvertToDataTable()
		{
			return Specialized.ReportingUtils.CreateDataSource( this );
		}


		[Obsolete("databound now", true)]
		private void Fill(DataView data)
		{
			this.Items.Clear();

			IComparer lvis = ListViewItemSorter;
			ListViewItemSorter = null;


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

//					if( _listViewItemAccessor != null )
//					{
//						_listViewItemAccessor.SetProperties( item, data[r] );
//					}
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
				ListViewItemSorter = lvis;
			}
		}



		public void RefreshColumnWidths()
		{
			_sColumnHeaders.RefreshWidths( true );
		}


		[DefaultValue(false)]
		public bool RebuildColumnsOnSetDataSource
		{
			get
			{
				return _forceColumnRebuild;
			}
			set
			{
				_forceColumnRebuild = value;
			}
		}


		/// <summary>
		/// Gets or sets the data source for the ListView; can be a DataSet, DataTable, or DataView.
		/// </summary>
		[Browsable(false)]
		[Description("Gets or sets the data source for the ListView; can be a DataSet, DataTable, or DataView.")]
		public object DataSource
		{
			get
			{
				return null != _dataSource ? _dataSource : _data;
			}
			set
			{
				if( _dataSource != value )
				{
					if( value != null )
					{
						SetDataSource( value );
					}
					else
					{
						_dataSource = value;
						_data = null;
						this.Clear();
					}
				}
			}
		}//DataSource

		private string _dataKeyString = string.Empty;
		private string[] _dataKeys = null;
		public string DataKeys
		{
			get { return _dataKeyString; }
			set
			{
				_dataKeyString = value;
				_dataKeys = _dataKeyString.Split( ',' );
			}
		}
		internal string[] dataKeys { get { return _dataKeys; } }
		internal void EnsureDataKeys()
		{
			if( string.IsNullOrEmpty( _dataKeyString ) )
			{
				_dataKeyString = __lvDefaultKey;

				_dataKeys = new string[] { __lvDefaultKey };
				DataColumn an = new DataColumn( __lvDefaultKey, typeof( int ) );
				_data.Table.Columns.Add( an );
				int pk = 1;
				foreach( DataRow r in _data.Table.Rows )
				{
					r[__lvDefaultKey] = pk++;
				}
				an.AutoIncrement = true;
				an.AutoIncrementSeed = pk++;
				an.AutoIncrementStep = 1;
				an.Unique = true;
				an.ColumnMapping = MappingType.Hidden;
			}
		}


		private void SetDataSource(object value)
		{
			bool valid = true;
			string err = string.Empty;
			// possible future use:
			// if (value != null && value as IList == null)
			// if (value as IListSource == null)
			// throw new Exception(SR.GetString("BadDataSourceForComplexBinding"));


			if( value is DataSet )
			{
				if( ((DataSet)value).Tables.Count == 0 )
				{
					valid = false;
					err = "The DataSet does not contain any DataTables";
				}
				else
				{
					_data = ((DataSet)value).Tables[0].DefaultView;
				}
			}
			else if( value is DataTable )
			{
				_data = ((DataTable)value).DefaultView;
			}
			else if( value is DataView )
			{
				_data = (DataView)value;
			}
			else//   value != null )
			{
				valid = false;
				err = "DataSource must be a DataSet, DataTable, or DataView";
			}

			if( valid )
			{
				this.EnsureDataKeys();

				_dataSource = value;
				_sColumnHeaders.Data = _data;
				_items.Data = _data;			//sets ListChangedEventHandler inside Items collection
				bool didRefresh = _sColumnHeaders.Refresh( _forceColumnRebuild );
				_items.RefreshFromData();
				_sColumnHeaders.RefreshWidths( _forceColumnRebuild || didRefresh );
			}
			else
			{
				throw new Exception( err );
			}

		}


		/// <summary>
		/// Gets the DataView to which the ListView is bound.  If the DataSource is a DataView, Data equals DataSource.
		/// </summary>
		[Browsable(false)]
		[Description("Gets the DataView to which the ListView is bound.  If the DataSource is a DataView, Data equals DataSource.")]
		public DataView Data
		{
			get
			{
				return _data;
			}
		}


		[Browsable(false)]
		public IListViewItemAccessor sListViewItemAccessor
		{
			get
			{
				return _items.sListViewItemAccessor;
			}
			set
			{
				_items.sListViewItemAccessor = value;
			}
		}




		public ListView.ColumnHeaderCollection NativeColumns
		{
			get
			{
				return base.Columns;
			}
		}


		public ListView.ListViewItemCollection NativeItems
		{
			get
			{
				return base.Items;
			}
		}


		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		new public sListView.sColumnHeaderCollection Columns
		{
			get
			{
				return _sColumnHeaders;
			}
		}


		new public sListView.sListViewItemCollection Items
		{
			get
			{
				return _items;
			}
		}


		protected override void OnColumnClick(ColumnClickEventArgs e)
		{
			//base.OnColumnClick (e);
			_items.Sort( _sColumnHeaders[e.Column] );
		}


		public void SuspendDataBinding()
		{
			_items.SuppressDataEvents = true;
		}
		public void ResumeDataBinding()
		{
			_items.SuppressDataEvents = false;
			_items.RefreshFromData();
		}
		public void SuspendItemBinding()
		{
			_items.SuppressItemEvents = true;
		}
		public void ResumeItemBinding()
		{
			_items.SuppressItemEvents = false;
			_items.RefreshFromItems();
		}


		#endregion


		#region sColumnHeaderCollection

		public class sColumnHeaderCollection : ColumnHeaderCollection
		{
			private int index = 1;
			private DataView _data = null;

			private bool _suppressDataEvents = false;


			#region Events

//			internal event EventHandler Adding;
			internal event EventHandler Added;
//			internal event EventHandler Removing;
			internal event EventHandler Removed;
			internal event EventHandler Cleared;

//			protected void OnAdding(object sender, EventArgs e)
//			{
//				if( Adding != null )
//				{
//					Adding( sender, e );
//				}
//			}

			protected void OnAdded(object sender, EventArgs e)
			{
				if( Added != null )
				{
					Added( sender, e );
				}
			}

//			protected void OnRemoving(object sender, EventArgs e)
//			{
//				if( Removing != null )
//				{
//					Removing( sender, e );
//				}
//			}

			protected void OnRemoved(object sender, EventArgs e)
			{
				if( Removed != null )
				{
					Removed( sender, e );
				}
			}

			protected void OnCleared(object sender, EventArgs e)
			{
				if( Cleared != null )
				{
					Cleared( sender, e );
				}
			}


			#endregion


			public sColumnHeaderCollection(sListView owner) : base( (ListView)owner ){}


			internal DataView Data
			{
				get
				{
					return _data;
				}
				set
				{
					_data = value;
				}
			}


			internal bool Refresh(bool forceRebuild)
			{
				bool result = false;
				if( forceRebuild || this.Count == 0 )
				{
					result = true;
					_suppressDataEvents = true;
					this.Clear();
					foreach( DataColumn c in _data.Table.Columns )
					{
						if( c.ColumnMapping != MappingType.Hidden )
						{
							this.Add( new sColumnHeader( c ) );
						}
					}
					_suppressDataEvents = false;
				}
				return result;
			}


			// -1: width of the longest item in the column
			// -2: width of the column heading
			internal void RefreshWidths(bool forceRebuild)
			{
				if( forceRebuild )
				{
//					int w = 0;
					foreach( sColumnHeader h in this )
					{
						h.Width = -2;
//						w = h.Width;
//						h.Width = -1;
//						if( w > h.Width ) h.Width = w;
					}
				}
			}


			new public sColumnHeader this[ int index ]
			{
				get
				{
					return( (sColumnHeader)base[index] );
				}
			}


			new public sColumnHeader Add( string text, int width, HorizontalAlignment textAlign )
			{
				return Add( text, width, textAlign,
					string.Empty, string.Empty, TypeCode.String, string.Empty ) ;
			}


			public sColumnHeader Add( string text, int width, string displayMember )
			{
				return Add( text, width, HorizontalAlignment.Left,
					displayMember, string.Empty, TypeCode.String, string.Empty ) ;
			}


			public sColumnHeader Add( string text, int width,
				string displayMember, TypeCode dataType, string formatString )
			{
				return Add( text, width, HorizontalAlignment.Left,
					displayMember, string.Empty, dataType, formatString ) ;
			}


			public sColumnHeader Add( string text, int width,
				string displayMember, string valueMember, TypeCode dataType, string formatString )
			{
				return Add( text, width, HorizontalAlignment.Left,
					displayMember, valueMember, dataType, formatString ) ;
			}


			public sColumnHeader Add( string text, int width, HorizontalAlignment textAlign,
				string displayMember, string valueMember, TypeCode dataType, string formatString )
			{
				sColumnHeader mch = new sColumnHeader();
				mch.Text = text;
				mch.Width = width;
				mch.TextAlign = textAlign;
				mch.DisplayMember = displayMember;
				mch.ValueMember = valueMember;
				mch.DataType = dataType;
				mch.FormatString = formatString;

				base.Add( (ColumnHeader)mch );

				AddColumn( mch );

				return mch;
			}


			public int Add( sColumnHeader value )
			{
				int i = base.Add( (ColumnHeader)value );
				AddColumn( value );
				return i;
			}


			public void AddRange(sColumnHeader[] values)
			{
				base.AddRange( (ColumnHeader[])values );
				foreach( sColumnHeader c in values )
				{
					AddColumn( c );
				}
			}


			override public void AddRange(ColumnHeader[] values)
			{
				//foreach( ColumnHeader c in values )
				for( int i=0; i<values.Length; i++ )
				{
					//if( !(c is sColumnHeader) )
					if( values[i] as sColumnHeader == null )
					{
						values[i] = new sColumnHeader( values[i] );
						//throw new NotSupportedException( "System.Windows.Forms.ColumnHeaders are not supported in a sColumnHeader collection." );
						//this.Add( new sColumnHeader( c ) );
					}

					this.Add( (sColumnHeader)values[i] );
//					else
//					{
//					}
				}
			}



			private void AddColumn(sColumnHeader columnHdr)
			{
				OnAdded( this, EventArgs.Empty );

				if( !_suppressDataEvents )
				{
					string columnName = columnHdr.displayMember;
					if( _data.Table.Columns.Contains( columnName ) )
					{
						columnName = columnHdr.displayMember + ((int)(_data.Table.Columns.IndexOf( columnHdr.displayMember ) + index++ )).ToString();
					}

					columnHdr.DataColumn =
						_data.Table.Columns.Add( columnName, Type.GetType( "System." + columnHdr.DataType.ToString() ) );
				}
			}


			public int IndexOf( sColumnHeader value )
			{
				return( base.IndexOf( value ) );
			}


			public void Insert( int index, sColumnHeader value )
			{
				base.Insert( index, (ColumnHeader)value );
			}


			new public void Insert( int index, string text, int width, HorizontalAlignment textAlign )
			{
				this.Insert( index, text, width, textAlign,
					string.Empty, string.Empty, TypeCode.String, string.Empty ) ;
			}


			public void Insert( int index, string text, int width, string displayMember )
			{
				this.Insert( index, text, width, HorizontalAlignment.Left,
					displayMember, string.Empty, TypeCode.String, string.Empty ) ;
			}


			public void Insert( int index, string text, int width,
				string displayMember, TypeCode dataType, string formatString )
			{
				this.Insert( index, text, width, HorizontalAlignment.Left,
					displayMember, string.Empty, dataType, formatString ) ;
			}


			public void Insert( int index, string text, int width,
				string displayMember, string valueMember, TypeCode dataType, string formatString )
			{
				this.Insert( index, text, width, HorizontalAlignment.Left,
					displayMember, valueMember, dataType, formatString ) ;
			}


			public void Insert( int index, string text, int width, HorizontalAlignment textAlign,
				string displayMember, string valueMember, TypeCode dataType, string formatString )
			{
				sColumnHeader mch = new sColumnHeader();
				mch.Text = text;
				mch.Width = width;
				mch.TextAlign = textAlign;
				mch.DisplayMember = displayMember;
				mch.ValueMember = valueMember;
				mch.DataType = dataType;
				mch.FormatString = formatString;

				base.Insert( index, (ColumnHeader)mch );

				AddColumn( mch );
			}


			public void Remove( sColumnHeader value )
			{
				base.Remove( (ColumnHeader)value );
				OnRemoved( value, EventArgs.Empty );
			}


			new public void RemoveAt(int index)
			{
				sColumnHeader value = this[index];
				base.RemoveAt( index );
				OnRemoved( value, EventArgs.Empty );
			}


			public bool Contains( sColumnHeader value )
			{
				// If value is not of type sColumnHeader, this will return false.
				return( base.Contains( value ) );
			}


			public override void Clear()
			{
//				if( _data != null )
//				{
//					_data.Table.Columns.Clear();
//				}

				base.Clear();
				OnCleared( this, EventArgs.Empty );
			}

		}


		#endregion


		#region sListViewItemCollection

		/// <summary>
		/// The regular ListView.ListViewItemCollection, backed by a DataView.
		/// </summary>
		public class sListViewItemCollection : ListView.ListViewItemCollection
		{
			private sListView _owner = null;
			private DataView _data = null;
			private IListViewItemAccessor _listViewItemAccessor = null;

			private bool _sortASC = true;

			private bool _suppressItemEvents = false;
			private bool _suppressDataEvents = false;

			private sListViewItem _workItem = null;
			private int _workIndex = -1;

			private bool _waitingForDataChanged = false;

			private Dictionary<int, sListViewItem> _rowsToItems = null;


			#region overrides/wrappers

			public sListViewItemCollection(sListView owner) : base( (ListView)owner )
			{
				_owner = owner;

				_rowsToItems = new Dictionary<int, sListViewItem>();
			}

			new public sListViewItem this[int displayIndex]
			{
				get
				{
					return (sListViewItem)base[displayIndex];
				}
				set
				{
					base[displayIndex] = value;
				}
			}

			[Description("Gets the sListViewItem bound to the specified row from the sListViewItems collection.")]
			public sListViewItem this[DataRow row]
			{
				get
				{
					return (sListViewItem)_rowsToItems[GetRowHashCode( row )];
				}
//				set
//				{
//					base[displayIndex] = value;
//				}
			}


			new public sListViewItem Add(string text)
			{
				sListViewItem item = new sListViewItem( text );
				return this.Add( item );
			}


			new public sListViewItem Add(string text, int imageIndex)
			{
				sListViewItem item = new sListViewItem( text, imageIndex );
				return this.Add( item );
			}


			public sListViewItem Add(sListViewItem value)
			{
				d( "------------" );
				d( "Entering ADD:" + value );

				_owner.EnsureDataKeys();

				_waitingForDataChanged = true;
				_workItem = value;

				if( !_suppressItemEvents )
				{
					if( _data.RowFilter != string.Empty || _data.Sort != string.Empty )
					{
						ProxyAdd( _workItem );
					}
					else
					{
						AddRow( _workItem, true );
					}
				}
				else
				{
					if( _listViewItemAccessor != null )
					{
						_listViewItemAccessor.EvalPropertyData( null );
					}

					base.Add( value );

					if( _listViewItemAccessor != null )
					{
						_listViewItemAccessor.SetProperties( value, null );
					}
				}

				d( "Exiting  ADD:" + value.Equals( _workItem ).ToString() );

//				_suppressDataEvents = true;
				_workItem.Text = _workItem.Text; //doing this to get formatting
//				_suppressDataEvents = false;

				return _workItem;
			}


			public void AddRange(sListViewItem[] values)
			{
				foreach( sListViewItem item in values )
				{
					this.Add( item );
				}
			}


			public override void Clear()
			{
				if( _data != null )
				{
					_data.Table.Clear();
				}

				base.Clear();
			}


			public void Remove(sListViewItem item)
			{
				DeleteRow( item.DataRow );
				base.Remove( (ListViewItem)item );
			}


			public override void RemoveAt(int index)
			{
				DeleteRow( ((sListViewItem)this[index]).DataRow );
				base.RemoveAt( index );
			}


			#endregion


			[Browsable(false)]
			internal IListViewItemAccessor sListViewItemAccessor
			{
				get
				{
					return _listViewItemAccessor;
				}
				set
				{
					_listViewItemAccessor = value;
				}
			}


			[Browsable(false)]
			internal DataView Data
			{
				get
				{
					return _data;
				}
				set
				{
					if( _data != value )
					{
						_data = value;
						_data.ListChanged += new ListChangedEventHandler( this.Data_ListChanged );
						_data.Table.RowChanged += new DataRowChangeEventHandler( this.DataTable_RowChanged );
						//_data.Table.RowDeleted += new DataRowChangeEventHandler( this.DataTable_RowChanged );
					}
				}
			}


			private void ProxyAdd(sListViewItem item)
			{
				if( !_suppressItemEvents )
				{
					DataRow r = _data.Table.NewRow();
					r[_owner.Columns[0].displayMember] = item.Text;
					for( int i=1; i<item.SubItems.Count; i++ )
					{
						sColumnHeader c = _owner.Columns[i];
						if( _data.Table.Columns.Contains( c.displayMember ) )
						{
							r[c.displayMember] = item.SubItems[c.Index].Text;
						}
					}
					d( "ProxyAdd add row" );
					_data.Table.Rows.Add( r );
				}
			}


			/// <notes>
			/// This should only be used if the DataView (_data) is NOT sorted.
			/// Otherwise, the ListViewItem gets added to the end of the list 
			/// while the Row is inserted at the proper index location, thus 
			/// rendering the two collections unsynchronized.
			/// </notes>
			private void AddRow(sListViewItem item, bool allowItemAdd)
			{
				if( !_suppressItemEvents )
				{
					_suppressDataEvents = true;
					DataRow r = _data.Table.NewRow();
					r[_owner.Columns[0].displayMember] = item.Text;
					for( int i=1; i<item.SubItems.Count; i++ )
					{
						sColumnHeader c = _owner.Columns[i];
						if( _data.Table.Columns.Contains( c.displayMember ) )
						{
							r[c.displayMember] = item.SubItems[c.Index].Text;
						}
					}
					_data.Table.Rows.Add( r );
					_suppressDataEvents = false;

					item.DataRow = r;
					item.ColumnHeaders = _owner.Columns;
					_rowsToItems.Add( GetRowHashCode( r ), item );

					if( _listViewItemAccessor != null )
					{
						_listViewItemAccessor.EvalPropertyData( r );
					}

					if( allowItemAdd ) base.Add( item );

					if( _listViewItemAccessor != null )
					{
						_listViewItemAccessor.SetProperties( item, r );
					}
				}
			}


			private void DeleteRow(DataRow row)
			{
				if( !_suppressItemEvents && row != null )
				{
					_suppressDataEvents = true;
					row.Delete();
					_rowsToItems.Remove( GetRowHashCode( row ) );
					_suppressDataEvents = false;
				}
			}



			private void Data_ListChanged(object sender, ListChangedEventArgs e)
			{
				d( string.Format( "ListChangedType: {0}\to[{1}], n[{2}]", e.ListChangedType, e.OldIndex, e.NewIndex ) );

				if( !_suppressDataEvents )
				{
					switch( e.ListChangedType )
					{
							#region ItemMoved
						case ListChangedType.ItemMoved:
						{
							if( _workIndex != e.NewIndex )
							{
								_suppressItemEvents = true;
								sListViewItem item = (sListViewItem)this[e.OldIndex];

								item.SuppressDataEvents = true;		//item.UpdateFromRow() will SuppressDataEvents, but need it for Remove.
								item.Remove();
								item.UpdateFromRow( _listViewItemAccessor );
								item.SuppressDataEvents = false;	//just for symmetry

								this.Insert( e.NewIndex, item );
								_suppressItemEvents = false;
							}

							break;
						}
							#endregion
							#region ItemAdded
						case ListChangedType.ItemAdded:
						{
							sListViewItem item = new sListViewItem();
							if( _waitingForDataChanged )
							{
								item = _workItem;
							}

							item.DataRow = _data[e.NewIndex].Row;
							item.ColumnHeaders = _owner.Columns;

							item.UpdateFromRow( _listViewItemAccessor );

							d( "ListChange:ItemAdd" );
							_workItem = item;
							_workIndex = e.NewIndex;

							_rowsToItems.Add( GetRowHashCode( item.Row ), item );

							break;
						}
							#endregion
							#region ItemChanged
						case ListChangedType.ItemChanged:
						{
							((sListViewItem)this[e.NewIndex]).UpdateFromRow( _listViewItemAccessor );
							break;
						}
							#endregion
							#region ItemDeleted
						case ListChangedType.ItemDeleted:
						{
							if( this.Count-1 >= e.NewIndex )
							{
								sListViewItem item = (sListViewItem)this[e.NewIndex];
								if( item != null )
								{
									_suppressItemEvents = true;
									item.SuppressDataEvents = true;
									item.Remove();
									item.SuppressDataEvents = false;
									_suppressItemEvents = false;
								}
							}

							break;
						}
							#endregion
							#region Reset
						case ListChangedType.Reset:
						{
							RefreshFromData();
							break;
						}
							#endregion
					}//switch
				}//if
			}




			/// <notes>
			/// DataTable_RowChanged handles Add events instead of Data_ListChanged (via ListChangedType.ItemAdded)
			/// because more than one ListChangedType.ItemAdded gets generated during an Add, but only one
			/// DataRowAction.Add is generated for the actual Row add.  The ListChangedType.ItemAdded handler above
			/// sets up the _workItem/_workIndex for this handler to process.
			/// </notes>
			private void DataTable_RowChanged(object sender, DataRowChangeEventArgs e)
			{
				if( !_suppressDataEvents )
				{
					d( "DataRowChange:\t\t" + e.Action );

					switch( e.Action )
					{
						case DataRowAction.Add:
						{
							d( "DataTable:RowAdded" );
							_suppressItemEvents = true;							
							_workItem.SuppressDataEvents = true;
							this.Insert( _workIndex, _workItem );
							_workItem.SuppressDataEvents = false;
							_waitingForDataChanged = false;
							_suppressItemEvents = false;
							break;
						}
							// case DataRowAction.Change:{break;}
							// case DataRowAction.Delete:{break;}
					}
				}
			}


			internal void Sort(sColumnHeader column)
			{
				_data.Sort = column.displayMember + (_sortASC ? " ASC" : " DESC");
				_sortASC = !_sortASC;
			}


			internal void RefreshFromData()
			{
				_suppressItemEvents = true;
				base.Clear();
				_rowsToItems.Clear();
				foreach( DataRowView rv in _data )
				{
					sListViewItem item = new sListViewItem();
					base.Add( item );

					item.ColumnHeaders = _owner.Columns;
					item.DataRow = rv.Row;
					item.UpdateFromRow( _listViewItemAccessor );

					_rowsToItems.Add( GetRowHashCode( rv.Row ), item );
				}
				_suppressItemEvents = false;
			}


			internal void RefreshFromItems()
			{
				_suppressDataEvents = true;
				_data.Table.Rows.Clear();
				string sort = _data.Sort;
				_data.Sort = "";
				_data.RowFilter = "";
				_rowsToItems.Clear();
				foreach( sListViewItem item in this )
				{
					AddRow( item, false );
				}
				_suppressDataEvents = false;
				_data.Sort = sort;
			}


//			internal sListViewItem GetItemByRow(DataRow row)
//			{
//				return (sListViewItem)_rowsToItems[row.GetHashCode()];
//			}


			internal bool SuppressDataEvents
			{
				get
				{
					return _suppressDataEvents;
				}
				set
				{
					_suppressDataEvents = value;
				}
			}


			internal bool SuppressItemEvents
			{
				get
				{
					return _suppressItemEvents;
				}
				set
				{
					_suppressItemEvents = value;
				}
			}


			private void d(string msg)
			{
				System.Diagnostics.Debug.WriteLine( string.Format( "{0}\t{1}", DateTime.Now.ToLongTimeString(), msg ) );
			}

			private int GetRowHashCode(DataRow r)
			{
				//return (int)r["N"];
				StringBuilder ht = new StringBuilder();
				foreach( string key in _owner.dataKeys )
				{
					ht.AppendFormat( "_{0}", r[key].ToString() );
				}
				return ht.ToString().GetHashCode();
			}

		}	//sListViewItemCollection


		#endregion

	}	//class



	#region sColumnHeader

	public class sColumnHeader : ColumnHeader
	{
		private DataColumn _column = null;

		private string _displayMember	= string.Empty;
		private string _valueMember		= string.Empty;
		private TypeCode _dataType		= TypeCode.String;
		private string _formatString	= string.Empty;
		internal TextBox _filterTextBox = new TextBox();



		public sColumnHeader() : base()
		{
			//_column						= new DataColumn();

			_filterTextBox.Top			= 0;
			_filterTextBox.BorderStyle	= System.Windows.Forms.BorderStyle.None;

			// _filterTextBox.Left		= _runningColWidth + 2;
			// _filterTextBox.Width		= this.Width - 2;
			// _filterTextBox.KeyPress += new KeyPressEventHandler(FilterTextBox_KeyPress);
		}


		public sColumnHeader(ColumnHeader c) : base()
		{
			_filterTextBox.Top			= 0;
			_filterTextBox.BorderStyle	= System.Windows.Forms.BorderStyle.None;

			this.Text = c.Text;
			this.TextAlign = c.TextAlign;
			this.Width = c.Width;

			//c = this;
		}


		internal sColumnHeader(DataColumn column) : base()
		{
			_column = column;

			_filterTextBox.Top			= 0;
			_filterTextBox.BorderStyle	= System.Windows.Forms.BorderStyle.None;

			_dataType = Type.GetTypeCode( column.DataType );
			_valueMember = column.ColumnName;
			this.Text = column.Caption;
		}


		internal DataColumn DataColumn
		{
			get
			{
				return _column;
			}
			set
			{
				_column = value;
			}
		}


		new public string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
				if( _displayMember == null || _displayMember == string.Empty )
				{
					this.DisplayMember = value;
				}
			}
		}


		[DefaultValue("")]
		public string DisplayMember
		{
			get
			{
				return _displayMember;
			}
			set
			{
				if( value == null )
				{
					_displayMember = string.Empty;
				}
				else
				{
					_displayMember = value;
					if( _column != null )
					{
						_column.ColumnName = value;
						_column.Caption = value;
					}
				}
			}
		}


		/// <summary>
		/// Contains DisplayMember if != string.Empty, else contains sColumnHeader.Text
		/// </summary>
		[DefaultValue("")]
		internal string displayMember
		{
			get
			{
				return _displayMember == string.Empty ? this.Text : _displayMember;
			}
		}


		[Browsable(false)]
		internal bool HasDisplayMember
		{
			get
			{
				return _displayMember != string.Empty;
			}
		}


		[DefaultValue("")]
		public string ValueMember
		{
			get
			{
				return _valueMember;
			}
			set
			{
				if( value == null )
				{
					_valueMember = string.Empty;
				}
				else
				{
					_valueMember = value;
				}
			}
		}


		[Browsable(false)]
		internal bool HasValueMember
		{
			get
			{
				return _valueMember != string.Empty;
			}
		}


		[DefaultValue(TypeCode.String)]
		public TypeCode DataType
		{
			get
			{
				return _dataType;
			}
			set
			{
				if( _dataType != value )
				{
					_dataType = value;
					if( _column != null )
					{
						_column.DataType = Type.GetType( "System." + value.ToString() );
					}
				}
			}
		}


		/// <summary>
		/// Used for format specifier in ToString() calls on non-String DataTypes.
		/// </summary>
		[DefaultValue("")]
		public string FormatString
		{
			get
			{
				return _formatString;
			}
			set
			{
				if( value == null )
				{
					_formatString = string.Empty;
				}
				else
				{
					_formatString = value;
				}
			}
		}


		/// <summary>
		/// Reserved for sListViewFilter use.
		/// </summary>
		internal TextBox FilterTextBox
		{
			get
			{
				return _filterTextBox;
			}
			set
			{
				_filterTextBox = value;
			}
		}


		//TODO: checkout Clone() & make sure it works
	}


	#endregion


	#region sListViewItem, sListViewSubItem, sListViewSubItemCollection

	public class sListViewItem : System.Windows.Forms.ListViewItem
	{
		#region sListViewItem

		private DataRow _row = null;
		private sListView.sColumnHeaderCollection _columns = null;
		private sColumnHeader _columnHdr = null;
		private sListViewItem.sListViewSubItemCollection _subItems = null;
		private bool _suppressDataEvents = false;

		public sListViewItem()
		{
			Init();
		}

		public sListViewItem(string text) : base(text)
		{
			Init();
		}

		public sListViewItem(string[] items) : base(items)
		{
			Init();
		}

		public sListViewItem(sListViewItem.sListViewSubItem[] subItems, int imageIndex) :
			base(subItems, imageIndex)
		{
			Init();
		}

		public sListViewItem(string text, int imageIndex) :
			base(text, imageIndex)
		{
			Init();
		}

		public sListViewItem(string[] items, int imageIndex) :
			base(items, imageIndex)
		{
			Init();
		}

		public sListViewItem(string[] items, int imageIndex, Color foreColor, Color backColor, Font font) :
			base(items, imageIndex, foreColor, backColor, font)
		{
			Init();
		}

		private void Init()
		{
			_subItems = new sListViewSubItemCollection( this );
		}
	

		internal DataRow DataRow
		{
			get
			{
				return _row;
			}
			set
			{
				_row = value;
			}
		}


		[Description("Gets the DataRow bound to this ListViewItem.")]
		public DataRow Row
		{
			get
			{
				return _row;
			}
		}


		internal sListView.sColumnHeaderCollection ColumnHeaders
		{
			get
			{
				return _columns;
			}
			set
			{
				_columns = value;
				_columnHdr = value[0];
			}
		}


		new public string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				if( _columnHdr != null && _columnHdr.FormatString != string.Empty )
				{
					base.Text = MiscUtils.Format( value, _columnHdr.DataType, _columnHdr.FormatString );
				}
				else
				{
					base.Text = value;
				}

				if( !_suppressDataEvents && _row != null && _columnHdr != null )
				{
					_row[_columnHdr.displayMember] = value;
				}
			}
		}


		public override void Remove()
		{
			if( !_suppressDataEvents && _row != null )
			{
				_row.Delete();
			}

			base.Remove();
		}



		internal bool SuppressDataEvents
		{
			get
			{
				return _suppressDataEvents;
			}
			set
			{
				_suppressDataEvents = value;
			}
		}



		/// <summary>
		/// Writes the item Text/Tag values, and the subitem Text values from the item's bound DataRow (suppresses DataEvents).
		/// </summary>
		internal void UpdateFromRow(IListViewItemAccessor lvia)
		{
			_suppressDataEvents = true;

			int i = 0;
			DataRow r = _row;

			if( lvia != null )
			{
				lvia.EvalPropertyData( _row );
			}

			sColumnHeader c = _columns[i];

			this.Text = r[c.displayMember].ToString();
			if( c.HasValueMember )
			{
				this.Tag = r[c.ValueMember];
			}

			i++;
			for( ; i<_columns.Count; i++ )
			{
				c = _columns[i];

				//if this.SubItems.Count > i then given item has enough subitems, else add them as necessary
				if( this.SubItems.Count > i )
				{
					this.SubItems[i].Text = r[c.displayMember].ToString();
				}
				else
				{
					this.SubItems.Add( r[c.displayMember].ToString() );
				}

				if( c.HasValueMember )
				{
					this.Tag = r[c.ValueMember];
				}
			}

			if( lvia != null )
			{
				lvia.SetProperties( this, _row );
			}

			_suppressDataEvents = false;
		}


		new public sListViewSubItemCollection SubItems
		{
			get
			{
				return _subItems;
			}
		}


		internal string ToString(bool showHashCode)
		{
			System.Text.StringBuilder s = new System.Text.StringBuilder( this.Text );

			for( int i=1; i<_subItems.Count; i++ )
			{
				s.AppendFormat( "/{0}", _subItems[i].Text );
			}

			if( showHashCode && _row != null )
			{
				s.AppendFormat( ",\tRowHashCode: {0}", GetRowHashCode( _row ) );
			}

			return s.ToString();
		}

		private int GetRowHashCode(DataRow r)
		{
			//return (int)r["N"];

			StringBuilder ht = new StringBuilder();
			foreach( string key in ( (sListView)this.ListView ).dataKeys )
			{
				ht.AppendFormat( "_{0}", r[key].ToString() );
			}
			return ht.ToString().GetHashCode();
		}
		#endregion


		#region sListViewSubItem
		public class sListViewSubItem : System.Windows.Forms.ListViewItem.ListViewSubItem
		{
			private sListViewItem _owner = null;
			private int _subItemIndex = 0;

			public sListViewSubItem() : base(){}

			public sListViewSubItem(sListViewItem owner, string text) :
				base( (ListViewItem)owner, text )
			{
				_owner = owner;
				_subItemIndex = _owner.SubItems.IndexOf( this );
			}

			public sListViewSubItem(sListViewItem owner, string text, Color foreColor, Color backColor, Font font) :
				base( (ListViewItem)owner, text, foreColor, backColor, font)
			{
				_owner = owner;
				_subItemIndex = _owner.SubItems.IndexOf( this );
			}


			internal int Index
			{
				get
				{
					return _subItemIndex;
				}
				set
				{
					_subItemIndex = value;
				}
			}


			new public string Text
			{
				get
				{
					return base.Text;
				}
				set
				{
					//correct???
					if( _owner.ColumnHeaders != null && _owner.ColumnHeaders[_subItemIndex].FormatString != string.Empty )
					{
						base.Text = MiscUtils.Format( value, _owner.ColumnHeaders[_subItemIndex].DataType, _owner.ColumnHeaders[_subItemIndex].FormatString );
					}
					else
					{
						base.Text = value;
					}

					if( !_owner.SuppressDataEvents && _owner.DataRow != null && _owner.ColumnHeaders != null )
					{
						_owner.DataRow[_owner.ColumnHeaders[_subItemIndex].displayMember] = value;
					}
				}
			}
		}
		#endregion


		#region sListViewSubItemCollection
		public class sListViewSubItemCollection : System.Windows.Forms.ListViewItem.ListViewSubItemCollection
		{
			sListViewItem _owner = null;

			public sListViewSubItemCollection(sListViewItem owner) : base( (ListViewItem)owner )
			{
				_owner = owner;
			}


			new public sListViewSubItem this[int index]
			{
				get  
				{
					return (sListViewSubItem)base[index];
				}
				set  
				{
					base[index] = (ListViewSubItem)value;
				}
			}



			public sListViewSubItem Add(sListViewSubItem item)
			{
				return AddSubItem( item );
			}

			new public sListViewSubItem Add(string text)
			{
				return AddSubItem( new sListViewSubItem( _owner, text ) );
			}

			new public sListViewSubItem Add(string text, Color foreColor, Color backColor, Font font)
			{
				return AddSubItem( new sListViewSubItem( _owner, text, foreColor, backColor, font ) );
			}

			public void AddRange(sListViewSubItem[] items)
			{
				foreach( sListViewSubItem item in items )
				{
					AddSubItem( item );
				}
			}

			new public void AddRange(string[] items)
			{
				foreach( string text in items )
				{
					AddSubItem( new sListViewSubItem( _owner, text ) );
				}
			}

			new public void AddRange(string[] items, Color foreColor, Color backColor, Font font)
			{
				foreach( string text in items )
				{
					AddSubItem( new sListViewSubItem( _owner, text, foreColor, backColor, font ) );
				}
			}


			private sListViewSubItem AddSubItem(sListViewSubItem subItem)
			{
				base.Add( (ListViewItem.ListViewSubItem)subItem );
				subItem.Index = this.IndexOf( subItem );
				subItem.Text = subItem.Text; //doing this to get formatting
				UpdateRow( subItem.Index, subItem.Text );


				return subItem;
			}


			public bool Contains(sListViewSubItem subItem)
			{
				return base.Contains( (ListViewItem.ListViewSubItem)subItem );
			}

			public int IndexOf(sListViewSubItem subItem)
			{
				return base.IndexOf( (ListViewItem.ListViewSubItem)subItem );
			}

			public void Insert(int index, sListViewSubItem subItem)
			{
				base.Insert( index, (ListViewItem.ListViewSubItem)subItem );
				subItem.Index = index;
				UpdateRow( subItem.Index, subItem.Text );
			}

			public void Remove(sListViewSubItem subItem)
			{
				UpdateRow( subItem.Index, Convert.DBNull );
				base.Remove( (ListViewItem.ListViewSubItem)subItem );
			}

			new public virtual void RemoveAt(int index)
			{
				UpdateRow( index, Convert.DBNull );
				base.RemoveAt( index );
			}


			private void UpdateRow(int subItemIndex, object value)
			{
				if( !_owner.SuppressDataEvents && _owner.DataRow != null && _owner.ColumnHeaders != null )
				{
					_owner.SuppressDataEvents = true;
					_owner.DataRow[_owner.ColumnHeaders[subItemIndex].displayMember] = value;
					_owner.SuppressDataEvents = false;
				}
			}

			private void UpdateRow(sListViewItem item)
			{
				////////////////// DataRow r = _data.NewRow();
				////////////////// r[_owner.Columns[0].displayMember] = item.Text;
				////////////////// for( int i=1; i<item.SubItems.Count; i++ )
				////////////////// {
				////////////////// sColumnHeader c = _owner.Columns[i];
				////////////////// if( _data.Columns.Contains( c.displayMember ) )
				////////////////// {
				////////////////// r[c.displayMember] = item.SubItems[c.Index].Text;
				////////////////// }
				////////////////// }
				////////////////// _data.Rows.Add( r );
				////////////////// //r.AcceptChanges();
				////////////////// //_data.AcceptChanges();
				////////////////// item.DataRow = r;
				////////////////// item.ColumnHeader = _owner.Columns[0];
			}

		}
		#endregion
	}

	#endregion



	public interface IListViewItemAccessor
	{
		void EvalPropertyData(object propertyData);
		void SetProperties(sListViewItem item, object propertyData);
	}
}	//namespace