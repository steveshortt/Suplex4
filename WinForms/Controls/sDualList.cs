using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using Suplex.Security;


/*
*************************************************************************************
IMPORTANT: If you edit this control in the designer, after completing
design changes be sure to remove the
	this.Name = "sDualList";
line of code.  Having the control named messes w/ the designer when using
it in other applications.
*************************************************************************************
*/


namespace Suplex.WinForms
{
	/// <summary>
	/// Summary description for mDualList.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(Suplex.WinForms.sButton), "Resources.mDualList.gif")]
	[DefaultEvent( "sDualList.ItemsMoved" )]
	public class sDualList : Suplex.WinForms.sUserControl
	{
		private bool _trackMoves = true;
		private bool _operateRight = false;

		private sDualList.DualListBox lstLeft;
		private sDualList.DualListBox lstRight;
		private System.Windows.Forms.Button cmdMoveLeft;
		private System.Windows.Forms.Button cmdMoveRight;
		private System.Windows.Forms.Label lblLeftTitle;
		private System.Windows.Forms.Label lblRightTitle;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;



		#region Events
		public event DualListEventHandler ItemsMoved;

		protected void OnItemsMoved(object sender, DualListEventArgs e)
		{
			if( ItemsMoved != null )
			{
				ItemsMoved( sender, e );
			}
		}
		#endregion





		public sDualList()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			lstLeft.OtherListBox = lstRight;
			lstRight.OtherListBox = lstLeft;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lstLeft = new Suplex.WinForms.sDualList.DualListBox();
			this.lstRight = new Suplex.WinForms.sDualList.DualListBox();
			this.cmdMoveLeft = new System.Windows.Forms.Button();
			this.cmdMoveRight = new System.Windows.Forms.Button();
			this.lblLeftTitle = new System.Windows.Forms.Label();
			this.lblRightTitle = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lstLeft
			// 
			this.lstLeft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left)));
			this.lstLeft.IntegralHeight = false;
			this.lstLeft.Location = new System.Drawing.Point(0, 16);
			this.lstLeft.Name = "lstLeft";
			this.lstLeft.Size = new System.Drawing.Size(120, 128);
			this.lstLeft.TabIndex = 0;
			this.lstLeft.DoubleClick += new System.EventHandler(this.MoveRight);
			this.lstLeft.SelectedIndexChanged += new System.EventHandler(this.lstLeft_SelectedIndexChanged);
			// 
			// lstRight
			// 
			this.lstRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lstRight.IntegralHeight = false;
			this.lstRight.Location = new System.Drawing.Point(192, 16);
			this.lstRight.Name = "lstRight";
			this.lstRight.Size = new System.Drawing.Size(120, 128);
			this.lstRight.TabIndex = 3;
			this.lstRight.DoubleClick += new System.EventHandler(this.MoveLeft);
			this.lstRight.SelectedIndexChanged += new System.EventHandler(this.lstRight_SelectedIndexChanged);
			// 
			// cmdMoveLeft
			// 
			this.cmdMoveLeft.Enabled = false;
			this.cmdMoveLeft.FlatStyle = FlatStyle.System;
			this.cmdMoveLeft.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cmdMoveLeft.Location = new System.Drawing.Point(128, 80);
			this.cmdMoveLeft.Name = "cmdMoveLeft";
			this.cmdMoveLeft.Size = new System.Drawing.Size(56, 23);
			this.cmdMoveLeft.TabIndex = 2;
			this.cmdMoveLeft.Text = "<<";
			this.cmdMoveLeft.Click += new System.EventHandler(this.MoveLeft);
			// 
			// cmdMoveRight
			// 
			this.cmdMoveRight.Enabled = false;
			this.cmdMoveRight.FlatStyle = FlatStyle.System;
			this.cmdMoveRight.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cmdMoveRight.Location = new System.Drawing.Point(128, 48);
			this.cmdMoveRight.Name = "cmdMoveRight";
			this.cmdMoveRight.Size = new System.Drawing.Size(56, 23);
			this.cmdMoveRight.TabIndex = 1;
			this.cmdMoveRight.Text = ">>";
			this.cmdMoveRight.Click += new System.EventHandler(this.MoveRight);
			// 
			// lblLeftTitle
			// 
			this.lblLeftTitle.Location = new System.Drawing.Point(0, 0);
			this.lblLeftTitle.Name = "lblLeftTitle";
			this.lblLeftTitle.Size = new System.Drawing.Size(120, 16);
			this.lblLeftTitle.TabIndex = 4;
			// 
			// lblRightTitle
			// 
			this.lblRightTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblRightTitle.Location = new System.Drawing.Point(192, 0);
			this.lblRightTitle.Name = "lblRightTitle";
			this.lblRightTitle.Size = new System.Drawing.Size(120, 16);
			this.lblRightTitle.TabIndex = 5;
			// 
			// mDualList
			// 
			this.Controls.Add(this.lblRightTitle);
			this.Controls.Add(this.lblLeftTitle);
			this.Controls.Add(this.cmdMoveRight);
			this.Controls.Add(this.cmdMoveLeft);
			this.Controls.Add(this.lstRight);
			this.Controls.Add(this.lstLeft);
			//this.Name = "sDualList";
			this.Size = new System.Drawing.Size(312, 144);
			this.ResumeLayout(false);

		}
		#endregion


		#region public accessors

		public sDualList.DualListBox LeftList
		{
			get
			{
				return lstLeft;
			}
		}


		public sDualList.DualListBox RightList
		{
			get
			{
				return lstRight;
			}
		}


		[Browsable( false )]
		public object LeftDataSource
		{
			get
			{
				return lstLeft.DataSource;
			}
			set
			{
				lstLeft.DataSource = value;
			}
		}


		[Browsable( false )]
		public object RightDataSource
		{
			get
			{
				return lstRight.DataSource;
			}
			set
			{
				lstRight.DataSource = value;
			}
		}


		public string DisplayMember
		{
			get
			{
				return lstLeft.DisplayMember;
			}
			set
			{
				lstLeft.DisplayMember = value;
			}
		}


		public string ValueMember
		{
			get
			{
				return lstLeft.ValueMember;
			}
			set
			{
				lstLeft.ValueMember = value;
			}
		}


		public string LeftTitle
		{
			get
			{
				return lblLeftTitle.Text;
			}
			set
			{
				lblLeftTitle.Text = value;
			}
		}


		public string RightTitle
		{
			get
			{
				return lblRightTitle.Text;
			}
			set
			{
				lblRightTitle.Text = value;
			}
		}


		[DefaultValue( false )]
		public bool Sorted
		{
			get
			{
				return lstLeft.Sorted;
			}
			set
			{
				lstLeft.Sorted = value;
				lstRight.Sorted = value;
			}
		}


		[DefaultValue( SelectionMode.One )]
		public SelectionMode SelectionMode
		{
			get
			{
				return lstLeft.SelectionMode;
			}
			set
			{
				lstLeft.SelectionMode = value;
				lstRight.SelectionMode = value;
			}
		}


		public void DataBind()
		{
			lstLeft.DataBind();
			lstRight.DataBind();
		}


		[DefaultValue( true )]
		public bool TrackMoves
		{
			get
			{
				return _trackMoves;
			}
			set
			{
				_trackMoves = value;
			}
		}


		[Browsable( false )]
		[ImmutableObject( true )]
		public ArrayList MovedLeft
		{
			get
			{
				return lstLeft.MovedItems;
			}
		}


		[Browsable( false )]
		[ImmutableObject( true )]
		public ArrayList MovedRight
		{
			get
			{
				return lstRight.MovedItems;
			}
		}


		#endregion


		#region implementation

		protected override void OnResize(EventArgs e)
		{
			int width = (this.Width-cmdMoveLeft.Width-16) / 2;

			lstLeft.Width = width;
			lstRight.Width = width;
			lblLeftTitle.Width = width;
			lblRightTitle.Width = width;

			cmdMoveLeft.Left = lstLeft.Width + 8;
			cmdMoveRight.Left = cmdMoveLeft.Left;
			lstRight.Left = cmdMoveLeft.Left + cmdMoveLeft.Width + 8;
			lblRightTitle.Left = lstRight.Left;

			//dunno why Left-stuff won't cooperate, so manual correction
			lstLeft.Height = lstRight.Height;
			lblLeftTitle.Left = lstLeft.Left;
			lblLeftTitle.Top = lstLeft.Top - lblLeftTitle.Height;

			cmdMoveRight.Top = (lstLeft.Height/2) - 8;
			cmdMoveLeft.Top = cmdMoveRight.Top + 32;


			base.OnResize (e);
		}


		private void lstLeft_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if( _operateRight )
			{
				cmdMoveRight.Enabled = lstLeft.SelectedItems.Count == 0 ? false : true;
			}
		}


		private void lstRight_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if( _operateRight )
			{
				cmdMoveLeft.Enabled = lstRight.SelectedItems.Count == 0 ? false : true;
			}
		}


		private void MoveLeft(object sender, System.EventArgs e)
		{
			if( _operateRight )
			{
				DualListEventArgs ea = new DualListEventArgs( lstRight.SelectedItems, true );

				for( int n=lstRight.SelectedItems.Count-1; n>=0; n-- )
				{
					if( _trackMoves )
					{
						lstLeft.MovedItems.Add( lstRight.SelectedItems[n] );
						if( lstRight.MovedItems.Contains( lstRight.SelectedItems[n] ) )
						{
							lstRight.MovedItems.Remove( lstRight.SelectedItems[n] );
						}
					}

					lstLeft.Items.Add( lstRight.SelectedItems[n] );
					lstRight.Items.Remove( lstRight.SelectedItems[n] );
				}

				if( ea.MovedItems.Count > 0 )
				{
					OnItemsMoved( this, ea );
				}
			}
		}


		private void MoveRight(object sender, System.EventArgs e)
		{
			if( _operateRight )
			{
				DualListEventArgs ea = new DualListEventArgs( lstLeft.SelectedItems, false );

				for( int n=lstLeft.SelectedItems.Count-1; n>=0; n-- )
				{
					if( _trackMoves )
					{
						lstRight.MovedItems.Add( lstLeft.SelectedItems[n] );
						if( lstLeft.MovedItems.Contains( lstLeft.SelectedItems[n] ) )
						{
							lstLeft.MovedItems.Remove( lstLeft.SelectedItems[n] );
						}
					}

					lstRight.Items.Add( lstLeft.SelectedItems[n] );
					lstLeft.Items.Remove( lstLeft.SelectedItems[n] );
				}

				if( ea.MovedItems.Count > 0 )
				{
					OnItemsMoved( this, ea );
				}
			}
		}


		#endregion


		#region security implementation
		public override void ApplySecurity()
		{
			base.ApplySecurity();

			if( !this.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				cmdMoveLeft.Enabled =
					cmdMoveRight.Enabled = false;
			}

			_operateRight = this.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed;

			this.lstLeft_SelectedIndexChanged( lstLeft, EventArgs.Empty );
			this.lstRight_SelectedIndexChanged( lstRight, EventArgs.Empty );
		}
		#endregion


		#region DualListBox

		public class DualListBox : ListBox
		{
			private object _dataSource = null;
			private DualListBox _otherListBox = null;
			private ArrayList _movedItems = new ArrayList();


			public DualListBox() : base(){}


			new public object DataSource
			{
				get
				{
					return _dataSource;
				}
				set
				{
					_dataSource = value;
					DataBind();
				}
			}


			internal void DataBind()
			{
				IEnumerator ds = null;

				this.Items.Clear();
				_movedItems.Clear();

				if( _dataSource != null )
				{
					if( _dataSource is DataTable )
					{
						ds = ((DataTable)_dataSource).DefaultView.GetEnumerator();
					}
					else
					{
						ds = ((IList)_dataSource).GetEnumerator();
					}

					while( ds.MoveNext() )
					{
						this.Items.Add( ds.Current );
					}
				}
			}


			internal DualListBox OtherListBox
			{
				get
				{
					return _otherListBox;
				}
				set
				{
					_otherListBox = value;
				}
			}


			protected override void OnDisplayMemberChanged(EventArgs e)
			{
				if( _otherListBox != null &&
					_otherListBox.DisplayMember != this.DisplayMember )
				{
					_otherListBox.DisplayMember = this.DisplayMember;
				}

				DataBind();

				base.OnDisplayMemberChanged (e);
			}


			protected override void OnValueMemberChanged(EventArgs e)
			{
				if( _otherListBox != null &&
					_otherListBox.ValueMember != this.ValueMember )
				{
					_otherListBox.ValueMember = this.ValueMember;
				}

				DataBind();

				base.OnValueMemberChanged (e);
			}


			[ImmutableObject( true )]
			public ArrayList MovedItems
			{
				get
				{
					return _movedItems;
				}
			}


			internal ArrayList movedItems
			{
				get
				{
					return _movedItems;
				}
			}

		}


		#endregion

	}


	#region Event Handler/Args
	public delegate void DualListEventHandler(object sender, DualListEventArgs e);

	public class DualListEventArgs : EventArgs
	{
		private ArrayList _movedItems = new ArrayList();
		private bool _movedLeft = true;

		new public static readonly DualListEventArgs Empty;
		static DualListEventArgs()
		{
			Empty = new DualListEventArgs();
		}


		public DualListEventArgs() : base() { }

		public DualListEventArgs(ICollection items, bool movedLeft) : base()
		{
			_movedLeft = movedLeft;

			IEnumerator items_enum = items.GetEnumerator();
			while( items_enum.MoveNext() )
			{
				_movedItems.Add( items_enum.Current );
			}
		}

		public ArrayList MovedItems
		{
			get { return _movedItems; }
		}

		public bool MovedLeft
		{
			get { return _movedLeft; }
		}

		public bool MovedRight
		{
			get { return !_movedLeft; }
		}
	}
	#endregion
}