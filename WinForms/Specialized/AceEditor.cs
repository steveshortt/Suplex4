using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Reflection;

using mUtilities.Forms;
using mUtilities.Security;
using mUtilities.General;


namespace mUtilities.Forms.Specialized
{
	/// <summary>
	/// Summary description for AceEditor.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(mUtilities.Forms.mButton), "Resources.AceEditor.gif")]
	public class AceEditor : mUtilities.Forms.mRecordControl
	{
		private ComboProps	_aceTrustee		= null;
		private ComboProps	_aceSecureItem	= null;

		private SortedList	_aceInfo		= new SortedList();

		private bool _trusteeOk		= false;
		private bool _secureItemOk	= false;
		private bool _aceTypeOk		= false;
		private bool _accessMaskOk	= false;

		private string _lastAceType = null;

		private bool _isAuditAce = false;
		private bool _chkAccessType1 = false;


		private mUtilities.Forms.mCheckBox chkAccessType2;
		private mUtilities.Forms.mCheckBox chkAccessType1;
		private mUtilities.Forms.mComboBox cmbAccessMask;
		private mUtilities.Forms.mLabel lblRight;
		private mUtilities.Forms.mComboBox cmbAceType;
		private mUtilities.Forms.mLabel lblAceType;
		private mUtilities.Forms.mComboBox cmbSecureItem;
		private mUtilities.Forms.mLabel lblSecureItem;
		private mUtilities.Forms.mHiddenValue valAcePk;
		private mUtilities.Forms.mLabel lblTrustee;
		private mUtilities.Forms.mComboBox cmbTrustee;
		private mUtilities.Forms.mButton cmdOk;
		private mUtilities.Forms.mButton cmdCancel;
		private mUtilities.Forms.Specialized.Border3ptHoriz border3ptHoriz;
		private mUtilities.Forms.mCheckBox chkInherit;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;


		#region Events
		public event System.EventHandler AceEditorOk;
		public event System.EventHandler AceEditorCancel;

		protected virtual void OnAceEditorOk(object sender)
		{
			if( AceEditorOk != null )
			{
				AceEditorOk( sender, EventArgs.Empty );
			}
		}


		protected virtual void OnAceEditorCancel(object sender)
		{
			if( AceEditorCancel != null )
			{
				AceEditorCancel( sender, EventArgs.Empty );
			}
		}
		#endregion


		public AceEditor()
		{
			InitializeComponent(); // This call is required by the Windows.Forms Form Designer.

			_aceTrustee = new ComboProps( this.cmbTrustee );
			_aceSecureItem = new ComboProps( this.cmbSecureItem );

			UIAce ace = new UIAce( UIRights.Operate, true );
			this.SecurityDescriptor.Dacl.Add( 0, ace );
			this.ApplySecurity( AceTypes.UI );

			InitAceTypes();

			chkAccessType1.Visible = false;
			chkAccessType2.Visible = false;
			chkInherit.Checked = false;
			cmdOk.Enabled = false;
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

		
		protected override bool ProcessDialogKey(Keys keyData)
		{
			if( keyData == Keys.Escape )
			{
				OnAceEditorCancel( cmdCancel );
			}
			else if( keyData == Keys.Enter )
			{
				if( cmdOk.Enabled )
				{
					OnAceEditorOk( cmdOk );
				}
			}

			return base.ProcessDialogKey (keyData);
		}

		
		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.chkAccessType2 = new mUtilities.Forms.mCheckBox();
			this.chkAccessType1 = new mUtilities.Forms.mCheckBox();
			this.cmbAccessMask = new mUtilities.Forms.mComboBox();
			this.lblRight = new mUtilities.Forms.mLabel();
			this.cmbAceType = new mUtilities.Forms.mComboBox();
			this.lblAceType = new mUtilities.Forms.mLabel();
			this.cmbSecureItem = new mUtilities.Forms.mComboBox();
			this.lblSecureItem = new mUtilities.Forms.mLabel();
			this.valAcePk = new mUtilities.Forms.mHiddenValue();
			this.lblTrustee = new mUtilities.Forms.mLabel();
			this.cmbTrustee = new mUtilities.Forms.mComboBox();
			this.cmdOk = new mUtilities.Forms.mButton();
			this.cmdCancel = new mUtilities.Forms.mButton();
			this.border3ptHoriz = new mUtilities.Forms.Specialized.Border3ptHoriz();
			this.chkInherit = new mUtilities.Forms.mCheckBox();
			this.SuspendLayout();
			// 
			// chkAccessType2
			// 
			this.chkAccessType2.AllowUndeclaredControls = true;
			this.chkAccessType2.AuditWriter = null;
			//this.chkAccessType2.DataAccessor = null;
			this.chkAccessType2.DataType = System.TypeCode.String;
			this.chkAccessType2.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.chkAccessType2.Enabled = false;
			this.chkAccessType2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkAccessType2.Location = new System.Drawing.Point(160, 136);
			this.chkAccessType2.Name = "chkAccessType2";
			this.chkAccessType2.TabIndex = 9;
			this.chkAccessType2.Text = "Audit Failure";
			this.chkAccessType2.UniqueName = "aceChkAccessType2";
			this.chkAccessType2.Visible = false;
			this.chkAccessType2.CheckedChanged += new System.EventHandler(this.CheckBox_Changed);
			// 
			// chkAccessType1
			// 
			this.chkAccessType1.AllowUndeclaredControls = true;
			this.chkAccessType1.AuditWriter = null;
			//this.chkAccessType1.DataAccessor = null;
			this.chkAccessType1.DataType = System.TypeCode.String;
			this.chkAccessType1.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.chkAccessType1.Enabled = false;
			this.chkAccessType1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkAccessType1.Location = new System.Drawing.Point(56, 136);
			this.chkAccessType1.Name = "chkAccessType1";
			this.chkAccessType1.TabIndex = 8;
			this.chkAccessType1.UniqueName = "aceChkAccessType1";
			this.chkAccessType1.Visible = false;
			this.chkAccessType1.CheckedChanged += new System.EventHandler(this.CheckBox_Changed);
			// 
			// cmbAccessMask
			// 
			this.cmbAccessMask.AllowUndeclaredControls = true;
			this.cmbAccessMask.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.cmbAccessMask.AuditWriter = null;
			//this.cmbAccessMask.DataAccessor = null;
			this.cmbAccessMask.DataType = System.TypeCode.String;
			this.cmbAccessMask.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.cmbAccessMask.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbAccessMask.Enabled = false;
			this.cmbAccessMask.Location = new System.Drawing.Point(56, 104);
			this.cmbAccessMask.Name = "cmbAccessMask";
			this.cmbAccessMask.Size = new System.Drawing.Size(208, 21);
			this.cmbAccessMask.TabIndex = 7;
			this.cmbAccessMask.UniqueName = "aceCmbAccessMask";
			this.cmbAccessMask.Visible = false;
			this.cmbAccessMask.SelectedIndexChanged += new System.EventHandler(this.cmbAccessMask_Changed);
			// 
			// lblRight
			// 
			this.lblRight.AllowUndeclaredControls = true;
			this.lblRight.AuditWriter = null;
			//this.lblRight.DataAccessor = null;
			this.lblRight.DataType = System.TypeCode.String;
			this.lblRight.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.lblRight.Enabled = false;
			this.lblRight.Location = new System.Drawing.Point(8, 108);
			this.lblRight.Name = "lblRight";
			this.lblRight.Size = new System.Drawing.Size(48, 16);
			this.lblRight.TabIndex = 6;
			this.lblRight.Text = "&Right";
			this.lblRight.UniqueName = "mLabel3";
			this.lblRight.Visible = false;
			// 
			// cmbAceType
			// 
			this.cmbAceType.AllowUndeclaredControls = true;
			this.cmbAceType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.cmbAceType.AuditWriter = null;
			//this.cmbAceType.DataAccessor = null;
			this.cmbAceType.DataType = System.TypeCode.String;
			this.cmbAceType.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.cmbAceType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbAceType.Enabled = false;
			this.cmbAceType.Location = new System.Drawing.Point(56, 72);
			this.cmbAceType.Name = "cmbAceType";
			this.cmbAceType.Size = new System.Drawing.Size(208, 21);
			this.cmbAceType.TabIndex = 5;
			this.cmbAceType.UniqueName = "aceCmbAceType";
			this.cmbAceType.Visible = false;
			this.cmbAceType.SelectedIndexChanged += new System.EventHandler(this.cmbAceType_Changed);
			// 
			// lblAceType
			// 
			this.lblAceType.AllowUndeclaredControls = true;
			this.lblAceType.AuditWriter = null;
			//this.lblAceType.DataAccessor = null;
			this.lblAceType.DataType = System.TypeCode.String;
			this.lblAceType.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.lblAceType.Enabled = false;
			this.lblAceType.Location = new System.Drawing.Point(8, 76);
			this.lblAceType.Name = "lblAceType";
			this.lblAceType.Size = new System.Drawing.Size(48, 16);
			this.lblAceType.TabIndex = 4;
			this.lblAceType.Text = "T&ype";
			this.lblAceType.UniqueName = "mLabel2";
			this.lblAceType.Visible = false;
			// 
			// cmbSecureItem
			// 
			this.cmbSecureItem.AllowUndeclaredControls = true;
			this.cmbSecureItem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.cmbSecureItem.AuditWriter = null;
			//this.cmbSecureItem.DataAccessor = null;
			this.cmbSecureItem.DataType = System.TypeCode.String;
			this.cmbSecureItem.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.cmbSecureItem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSecureItem.Enabled = false;
			this.cmbSecureItem.Location = new System.Drawing.Point(56, 40);
			this.cmbSecureItem.Name = "cmbSecureItem";
			this.cmbSecureItem.Size = new System.Drawing.Size(208, 21);
			this.cmbSecureItem.TabIndex = 3;
			this.cmbSecureItem.UniqueName = "aceCmbSecureItem";
			this.cmbSecureItem.Visible = false;
			this.cmbSecureItem.TextChanged += new System.EventHandler(this.cmbSecureItem_Changed);
			this.cmbSecureItem.SelectedIndexChanged += new System.EventHandler(this.cmbSecureItem_Changed);
			// 
			// lblSecureItem
			// 
			this.lblSecureItem.AllowUndeclaredControls = true;
			this.lblSecureItem.AuditWriter = null;
			//this.lblSecureItem.DataAccessor = null;
			this.lblSecureItem.DataType = System.TypeCode.String;
			this.lblSecureItem.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.lblSecureItem.Enabled = false;
			this.lblSecureItem.Location = new System.Drawing.Point(8, 44);
			this.lblSecureItem.Name = "lblSecureItem";
			this.lblSecureItem.Size = new System.Drawing.Size(48, 16);
			this.lblSecureItem.TabIndex = 2;
			this.lblSecureItem.Text = "&Item";
			this.lblSecureItem.UniqueName = "mLabel1";
			this.lblSecureItem.Visible = false;
			// 
			// valAcePk
			// 
			this.valAcePk.AllowUndeclaredControls = true;
			this.valAcePk.AuditWriter = null;
			//this.valAcePk.DataAccessor = null;
			this.valAcePk.DataType = System.TypeCode.String;
			this.valAcePk.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.valAcePk.Enabled = false;
			this.valAcePk.Location = new System.Drawing.Point(0, 0);
			this.valAcePk.Name = "valAcePk";
			this.valAcePk.Size = new System.Drawing.Size(16, 16);
			this.valAcePk.TabIndex = 26;
			this.valAcePk.TabStop = false;
			this.valAcePk.UniqueName = "aceValAcePk";
			this.valAcePk.Value = false;
			this.valAcePk.Visible = false;
			// 
			// lblTrustee
			// 
			this.lblTrustee.AllowUndeclaredControls = true;
			this.lblTrustee.AuditWriter = null;
			//this.lblTrustee.DataAccessor = null;
			this.lblTrustee.DataType = System.TypeCode.String;
			this.lblTrustee.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.lblTrustee.Enabled = false;
			this.lblTrustee.Location = new System.Drawing.Point(8, 12);
			this.lblTrustee.Name = "lblTrustee";
			this.lblTrustee.Size = new System.Drawing.Size(48, 16);
			this.lblTrustee.TabIndex = 0;
			this.lblTrustee.Text = "&Trustee";
			this.lblTrustee.UniqueName = "lblTrustee";
			this.lblTrustee.Visible = false;
			// 
			// cmbTrustee
			// 
			this.cmbTrustee.AllowUndeclaredControls = true;
			this.cmbTrustee.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.cmbTrustee.AuditWriter = null;
			//this.cmbTrustee.DataAccessor = null;
			this.cmbTrustee.DataType = System.TypeCode.String;
			this.cmbTrustee.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.cmbTrustee.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbTrustee.Enabled = false;
			this.cmbTrustee.Location = new System.Drawing.Point(56, 8);
			this.cmbTrustee.Name = "cmbTrustee";
			this.cmbTrustee.Size = new System.Drawing.Size(208, 21);
			this.cmbTrustee.TabIndex = 1;
			this.cmbTrustee.UniqueName = "aceCmbTrustee";
			this.cmbTrustee.Visible = false;
			this.cmbTrustee.TextChanged += new System.EventHandler(this.cmbTrustee_Changed);
			this.cmbTrustee.SelectedIndexChanged += new System.EventHandler(this.cmbTrustee_Changed);
			// 
			// cmdOk
			// 
			this.cmdOk.AllowUndeclaredControls = true;
			this.cmdOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdOk.AuditWriter = null;
			//this.cmdOk.DataAccessor = null;
			this.cmdOk.DataType = System.TypeCode.String;
			this.cmdOk.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOk.Enabled = false;
			this.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdOk.Location = new System.Drawing.Point(56, 208);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(72, 24);
			this.cmdOk.TabIndex = 12;
			this.cmdOk.Text = "&OK";
			this.cmdOk.UniqueName = "aceCmdOk";
			this.cmdOk.Visible = false;
			this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
			// 
			// cmdCancel
			// 
			this.cmdCancel.AllowUndeclaredControls = true;
			this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdCancel.AuditWriter = null;
			//this.cmdCancel.DataAccessor = null;
			this.cmdCancel.DataType = System.TypeCode.String;
			this.cmdCancel.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.Enabled = false;
			this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdCancel.Location = new System.Drawing.Point(144, 208);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(72, 24);
			this.cmdCancel.TabIndex = 13;
			this.cmdCancel.Text = "&Cancel";
			this.cmdCancel.UniqueName = "aceCmdCancel";
			this.cmdCancel.Visible = false;
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// border3ptHoriz
			// 
			this.border3ptHoriz.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.border3ptHoriz.Location = new System.Drawing.Point(8, 192);
			this.border3ptHoriz.Name = "border3ptHoriz";
			this.border3ptHoriz.Size = new System.Drawing.Size(256, 3);
			this.border3ptHoriz.TabIndex = 11;
			this.border3ptHoriz.TabStop = false;
			// 
			// chkInherit
			// 
			this.chkInherit.AllowUndeclaredControls = true;
			this.chkInherit.AuditWriter = null;
			//this.chkInherit.DataAccessor = null;
			this.chkInherit.DataType = System.TypeCode.String;
			this.chkInherit.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Locked;
			this.chkInherit.Enabled = false;
			this.chkInherit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkInherit.Location = new System.Drawing.Point(56, 160);
			this.chkInherit.Name = "chkInherit";
			this.chkInherit.TabIndex = 10;
			this.chkInherit.Text = "Block Inheritance";
			this.chkInherit.UniqueName = "aceChkInherit";
			this.chkInherit.Visible = false;
			this.chkInherit.CheckedChanged += new System.EventHandler(this.CheckBox_Changed);
			// 
			// AceEditor
			// 
			this.Controls.Add(this.chkInherit);
			this.Controls.Add(this.border3ptHoriz);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.cmbTrustee);
			this.Controls.Add(this.lblTrustee);
			this.Controls.Add(this.valAcePk);
			this.Controls.Add(this.chkAccessType2);
			this.Controls.Add(this.chkAccessType1);
			this.Controls.Add(this.cmbAccessMask);
			this.Controls.Add(this.lblRight);
			this.Controls.Add(this.cmbAceType);
			this.Controls.Add(this.lblAceType);
			this.Controls.Add(this.cmbSecureItem);
			this.Controls.Add(this.lblSecureItem);
			//this.Name = "AceEditor";
			this.Size = new System.Drawing.Size(272, 240);
			this.UniqueName = "aceAceEditor";
			this.ResumeLayout(false);

		}
		#endregion


		#region Public Properties/Methods

		[Browsable(false)]
		public string AcePrimaryKey
		{
			get
			{
				return valAcePk.Text;
			}
			set
			{
				valAcePk.Text = value;
			}
		}


		[Browsable(false)]
		public ComboProps AceTrustee
		{
			get
			{
				return _aceTrustee;
			}
		}


		[Browsable(false)]
		public ComboProps AceSecureItem
		{
			get
			{
				return _aceSecureItem;
			}
		}


		[Browsable(false)]
		public object AceTypeSelectedItem
		{
			get
			{
				return cmbAceType.SelectedItem;
			}
			set
			{
				cmbAceType.SelectedItem = value;
			}
		}


		[Browsable(false)]
		public bool AceTypeEnabled
		{
			get
			{
				return cmbAceType.Enabled;
			}
			set
			{
				cmbAceType.Enabled = value;
			}
		}


		[Browsable(false)]
		public object AceAccessMaskSelectedItem
		{
			get
			{
				return cmbAccessMask.SelectedItem;
			}
			set
			{
				cmbAccessMask.SelectedItem = value;
			}
		}


		[Browsable(false)]
		public int AceAccessMaskValue
		{
			get
			{
				//return cmbAccessMask.SelectedValue.ToString();
				return (int)((Array)cmbAccessMask.DataSource).GetValue(cmbAccessMask.SelectedIndex);
			}
			set
			{
				if( this.AceTypeSelectedItem != null )
				{
					cmbAccessMask.SelectedItem =
						Enum.Parse( ((AceTypeProps)_aceInfo[this.AceTypeSelectedItem.ToString()]).Type,
						value.ToString() );
				}
				//cmbAccessMask.SelectedValue = value;
			}
		}


		[Browsable(false)]
		public bool AceAccessType1
		{
			get
			{
				if( this.IsAuditAce )
				{
					return chkAccessType1.Checked;
				}
				else
				{
					return !chkAccessType1.Checked;
				}
			}
			set
			{
				_chkAccessType1 = value;
				
				if( this.IsAuditAce )
				{
					chkAccessType1.Checked = value;
				}
				else
				{
					chkAccessType1.Checked = !value;
				}
			}
		}


		[Browsable(false)]
		public bool AceAccessType2
		{
			get
			{
				return chkAccessType2.Checked;
			}
			set
			{
				chkAccessType2.Checked = value;
			}
		}


		[Browsable(false)]
		public bool AceInherit
		{
			get
			{
				return !chkInherit.Checked;
			}
			set
			{
				chkInherit.Checked = !value;
			}
		}


		[Browsable(false)]
		public bool IsAuditAce
		{
			get
			{
				return _isAuditAce;
			}
			set
			{
				_isAuditAce = value;

				if( value )
				{
					chkAccessType1.Checked = _chkAccessType1;
				}
				else
				{
					chkAccessType1.Checked = !_chkAccessType1;
				}

			}
		}


		public void Clear()
		{
			//cmbTrustee.Items.Clear();
			//cmbTrustee.Text = "";
			//cmbSecureItem.Items.Clear();
			//cmbSecureItem.Text = "";
			chkAccessType1.Checked = false;
			chkAccessType2.Checked = false;
			chkInherit.Checked = false;
			cmdOk.Enabled = false;
		}

		#endregion


		protected override void ProcessSelectRecord(params object[] parameters)
		{
			base.ProcessSelectRecord( parameters );
			cmdOk.Enabled = false;
		}


		private void InitAceTypes()
		{
			object aceType = null;
			Type right = null;
			Array rights = null;
			//int r = 0;
			ArrayList list = new ArrayList();

			Assembly asm = Assembly.Load( "mUtilities.Security" );
			Type[] types = asm.GetTypes();

			for( int n=0; n<types.Length; n++ )
			{
				if( ReflectionUtils.ImplementsInterface( types[n], new string[] {"mUtilities.Security.IAccessControlEntry"} )
					&& !types[n].IsInterface && !types[n].IsAbstract )
				{
					cmbAceType.Items.Add( types[n].Name );

					aceType = asm.CreateInstance( types[n].FullName );

					right = ReflectionUtils.GetProperty( aceType, "Right" ).PropertyType;
					rights = Enum.GetValues( right );

					AceTypeProps a = new AceTypeProps( right, rights,
						ReflectionUtils.ImplementsInterface(
						aceType, new string[] {"mUtilities.Security.IAccessControlEntryAudit"}) );

					_aceInfo.Add( types[n].Name, a );
				}//if
			}//for
		}//function


		private void cmbTrustee_Changed(object sender, System.EventArgs e)
		{
			_trusteeOk = CheckText( cmbTrustee );
			CheckReady();
		}


		private void cmbSecureItem_Changed(object sender, System.EventArgs e)
		{
			_secureItemOk = CheckText( cmbSecureItem );
			CheckReady();
		}


		private void cmbAceType_Changed(object sender, System.EventArgs e)
		{
			string typeAce = cmbAceType.SelectedItem.ToString();

			if( _lastAceType != typeAce )
			{
				cmbAccessMask.DataSource = ((AceTypeProps)_aceInfo[typeAce]).Rights;

				if( ((AceTypeProps)_aceInfo[typeAce]).IsAudit )
				{
					chkAccessType1.Text = "Audit Success";
					chkAccessType2.Visible = true;
					this.IsAuditAce = true;
				}
				else
				{
					chkAccessType1.Text = "Deny Access";
					chkAccessType2.Visible = false;
					this.IsAuditAce = false;
				}

				chkAccessType1.Checked = false;
				chkAccessType2.Checked = false;
				chkInherit.Checked = false;

				_lastAceType = typeAce;
			}

			chkAccessType1.Visible = true;

			_aceTypeOk = CheckText( cmbAceType );
			CheckReady();
		}


		private void cmbAccessMask_Changed(object sender, System.EventArgs e)
		{
			_accessMaskOk = CheckText( cmbAccessMask );
			CheckReady();
		}


		private void CheckBox_Changed(object sender, System.EventArgs e)
		{
			CheckReady();
		}


		private bool CheckText(ComboBox combo)
		{
			return combo.Text != null && combo.Text.Length > 0;
		}


		private void CheckReady()
		{
			cmdOk.Enabled = _trusteeOk && _secureItemOk && _aceTypeOk && _accessMaskOk;
		}


		private void cmdOk_Click(object sender, System.EventArgs e)
		{
			OnAceEditorOk( sender );
			cmdOk.Enabled = false;
		}


		private void cmdCancel_Click(object sender, System.EventArgs e)
		{
			OnAceEditorCancel( sender );
			cmdOk.Enabled = false;
		}
	}



	#region AceTypeProps
	class AceTypeProps
	{
		public AceTypeProps(Type type, Array rights, bool isAudit)
		{
			this.Type = type;
			this.Rights = rights;
			this.IsAudit = isAudit;
		}

		public Type Type = null;
		public Array Rights = null;
		public bool IsAudit = false;
	}

	#endregion


	#region ComboProps

	public class ComboProps
	{
		private ComboBox _innerCombo = null;


		internal ComboProps(ComboBox combo)
		{
			_innerCombo = combo;
		}


		public object DataSource
		{
			get
			{
				return _innerCombo.DataSource;
			}
			set
			{
				_innerCombo.DataSource = value;
			}
		}


		public string DisplayMember
		{
			get
			{
				return _innerCombo.DisplayMember;
			}
			set
			{
				_innerCombo.DisplayMember = value;
			}
		}


		public string ValueMember
		{
			get
			{
				return _innerCombo.ValueMember;
			}
			set
			{
				_innerCombo.ValueMember = value;
			}
		}


		public object SelectedItem
		{
			get
			{
				return _innerCombo.SelectedItem;
			}
			set
			{
				_innerCombo.SelectedItem = value;
			}
		}


		public object SelectedValue
		{
			get
			{
				return _innerCombo.SelectedValue;
			}
			set
			{
				_innerCombo.SelectedValue = value;
			}
		}


		public string Text
		{
			get
			{
				return _innerCombo.Text;
			}
			set
			{
				if( value == null )
				{
					_innerCombo.DropDownStyle = ComboBoxStyle.DropDownList;
				}
				else
				{
					_innerCombo.DropDownStyle = ComboBoxStyle.DropDown;
				}

				_innerCombo.Text = value;
			}
		}


		public bool Enabled
		{
			get
			{
				return _innerCombo.Enabled;
			}
			set
			{
				_innerCombo.Enabled = value;
			}
		}


		public object Tag
		{
			get
			{
				return _innerCombo.Tag;
			}
			set
			{
				_innerCombo.Tag = value;
			}
		}
	}


	#endregion
}


#region bunk

//		private void cmbAccessMask_SelectedIndexChanged(object sender, System.EventArgs e)
//		{
//			//chkAccessType1.Text = Enum.Parse( (Type)cmbAccessMask.Tag, cmbAccessMask.SelectedItem.ToString() ).ToString();
//			//chkAccessType1.Text = ((int)((Array)cmbAccessMask.DataSource).GetValue(cmbAccessMask.SelectedIndex)).ToString();
//		}


//Assembly asm = Assembly.Load( "mUtilities.Security" );
//object aceType =
//asm.CreateInstance( "mUtilities.Security." + cmbAceType.SelectedItem.ToString() );
//
//Type right = ReflectionUtils.GetProperty( aceType, "Right" ).PropertyType;
//cmbAccessMask.DataSource = Enum.GetValues( right );



////////testing
//////class AxMx
//////{
//////	private string	_right	= null;
//////	private int		_value	= -1;
//////
//////	public AxMx( string right, int value )
//////	{
//////		_right = right;
//////		_value = value;
//////	}
//////
//////
//////	public string RightName { get { return _right; } }
//////	public int RightValue { get { return _value; } }
//////
//////	public override string ToString()
//////	{
//////		return this.RightName + " - " + this.RightValue.ToString();
//////	}
//////
//////}
////////testing






////					r = 0;
////					for( ; r<rights.Length; r++ )
////					{
////						if( !cmbAccessMask.Items.Contains( rights.GetValue(r).ToString() ) )
////						{
////							cmbAccessMask.Items.Add( rights.GetValue(r).ToString() );
////							list.Add( new AxMx( rights.GetValue(r).ToString(), (int)rights.GetValue(r) ) );
////						}
////					}
//
//				}
//
//			}
//
//			//cmbAccessMask.DataSource = list;
//			//cmbAccessMask.DisplayMember = "RightName";
//			//cmbAccessMask.ValueMember = "RightValue";
//
//		}
//
//
//		//string x = ((int)rights.GetValue(r)).ToString();
//		//int x = (int)rights.GetValue(r);
//		//string x = rights.GetValue(r).ToString();
//
//		//						if( !cmbAccessMask.Items.Contains( x ) )
//		//						{
//		//							cmbAccessMask.Items.Add( x );
//		//						}

#endregion