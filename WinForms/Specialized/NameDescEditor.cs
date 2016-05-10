using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using mUtilities.Forms;
using mUtilities.Data;
using mUtilities.Security;


namespace mUtilities.Forms.Specialized
{
	/// <summary>
	/// Provides a simple Name\Description Editor.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(mUtilities.Forms.mButton), "Resources.NameDesc.gif")]
	public class NameDescEditor : mUtilities.Forms.mUserControl
	{
		private DialogResult	_result			= DialogResult.None;
		private string			_name			= null;
		private string			_desc			= null;
		private bool			_cancelEnabled	= false;
		private ReadyState		_readyState		= ReadyState.Ok;
		private RecordMode		_recordMode		= RecordMode.None;
		private bool			_readOnly		= false;
		
		private mUtilities.Forms.mHiddenValue valPrimaryKey;
		private mUtilities.Forms.mLabel lblName;
		private mUtilities.Forms.mTextBox txtName;
		private mUtilities.Forms.mLabel lblDesc;
		private mUtilities.Forms.mTextBox txtDesc;
		private mUtilities.Forms.mLabel lblTitle;
		private mUtilities.Forms.mButton cmdSave;
		private mUtilities.Forms.mButton cmdCancel;
		private System.Windows.Forms.PictureBox imgTitle;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;


		#region Events
		public event System.EventHandler ButtonClick;
		public event System.EventHandler ReadyStateChanged;

		private void OnButtonClick( object sender, EventArgs e )
		{
			//_result = ((mButton)sender).DialogResult;
			
			cmdSave.Enabled = false;
			cmdCancel.Enabled = _cancelEnabled;
			this.ReadyState = ReadyState.Ok;


			if( ButtonClick != null )
			{
				ButtonClick( sender, e );
			}
		}


		private void OnReadyStateChanged( object sender, EventArgs e )
		{
			if( ReadyStateChanged != null )
			{
				ReadyStateChanged( sender, e );
			}
		}


		#endregion


		public NameDescEditor()
		{
			InitializeComponent();	// This call is required by the Windows.Forms Form Designer.

			cmdSave.Enabled = false;
			cmdCancel.Enabled = _cancelEnabled;
		}


		public NameDescEditor(string primaryKey, string name, string description)
		{
			InitializeComponent();	// This call is required by the Windows.Forms Form Designer.

			this.ItemPrimaryKey		= primaryKey;
			this.ItemName			= name;
			this.ItemDescription	= description;

			cmdSave.Enabled = false;
			cmdCancel.Enabled = _cancelEnabled;
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
			this.lblName = new mUtilities.Forms.mLabel();
			this.txtName = new mUtilities.Forms.mTextBox();
			this.lblDesc = new mUtilities.Forms.mLabel();
			this.txtDesc = new mUtilities.Forms.mTextBox();
			this.lblTitle = new mUtilities.Forms.mLabel();
			this.imgTitle = new System.Windows.Forms.PictureBox();
			this.cmdSave = new mUtilities.Forms.mButton();
			this.cmdCancel = new mUtilities.Forms.mButton();
			this.valPrimaryKey = new mUtilities.Forms.mHiddenValue();
			this.SuspendLayout();
			// 
			// lblName
			// 
			this.lblName.AuditWriter = null;
			//this.lblName.DataAccessor = null;
			this.lblName.DataType = System.TypeCode.String;
			this.lblName.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.lblName.Enabled = false;
			this.lblName.Location = new System.Drawing.Point(32, 64);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(72, 16);
			this.lblName.TabIndex = 0;
			this.lblName.Text = "Name:";
			this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblName.UniqueName = "ndeLblName";
			this.lblName.Visible = false;
			// 
			// txtName
			// 
			this.txtName.AuditWriter = null;
			//this.txtName.DataAccessor = null;
			this.txtName.DataType = System.TypeCode.String;
			this.txtName.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.txtName.Enabled = false;
			this.txtName.FormatString = null;
			this.txtName.Location = new System.Drawing.Point(112, 62);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(280, 20);
			this.txtName.TabIndex = 1;
			this.txtName.Text = "";
			this.txtName.UniqueName = "ndeTxtName";
			this.txtName.Visible = false;
			this.txtName.TextChanged += new System.EventHandler(this.ItemTextChanged);
			// 
			// lblDesc
			// 
			this.lblDesc.AuditWriter = null;
			//this.lblDesc.DataAccessor = null;
			this.lblDesc.DataType = System.TypeCode.String;
			this.lblDesc.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.lblDesc.Enabled = false;
			this.lblDesc.Location = new System.Drawing.Point(32, 98);
			this.lblDesc.Name = "lblDesc";
			this.lblDesc.Size = new System.Drawing.Size(72, 16);
			this.lblDesc.TabIndex = 2;
			this.lblDesc.Text = "Description:";
			this.lblDesc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblDesc.UniqueName = "ndeLblDesc";
			this.lblDesc.Visible = false;
			// 
			// txtDesc
			// 
			this.txtDesc.AuditWriter = null;
			//this.txtDesc.DataAccessor = null;
			this.txtDesc.DataType = System.TypeCode.String;
			this.txtDesc.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.txtDesc.Enabled = false;
			this.txtDesc.FormatString = null;
			this.txtDesc.Location = new System.Drawing.Point(112, 96);
			this.txtDesc.Name = "txtDesc";
			this.txtDesc.Size = new System.Drawing.Size(280, 20);
			this.txtDesc.TabIndex = 3;
			this.txtDesc.Text = "";
			this.txtDesc.UniqueName = "ndeTxtDesc";
			this.txtDesc.Visible = false;
			this.txtDesc.TextChanged += new System.EventHandler(this.ItemTextChanged);
			// 
			// lblTitle
			// 
			this.lblTitle.AuditWriter = null;
			//this.lblTitle.DataAccessor = null;
			this.lblTitle.DataType = System.TypeCode.String;
			this.lblTitle.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.lblTitle.Enabled = false;
			this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(64, 22);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(328, 20);
			this.lblTitle.TabIndex = 4;
			this.lblTitle.UniqueName = "ndeLblTitle";
			this.lblTitle.Visible = false;
			// 
			// imgTitle
			// 
			this.imgTitle.Location = new System.Drawing.Point(16, 16);
			this.imgTitle.Name = "imgTitle";
			this.imgTitle.Size = new System.Drawing.Size(32, 32);
			this.imgTitle.TabIndex = 5;
			this.imgTitle.TabStop = false;
			// 
			// cmdSave
			// 
			this.cmdSave.AuditWriter = null;
			//this.cmdSave.DataAccessor = null;
			this.cmdSave.DataType = System.TypeCode.String;
			this.cmdSave.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.cmdSave.Enabled = false;
			this.cmdSave.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdSave.Location = new System.Drawing.Point(228, 128);
			this.cmdSave.Name = "cmdSave";
			this.cmdSave.TabIndex = 8;
			this.cmdSave.Text = "Save";
			this.cmdSave.UniqueName = "ndeCmdSave";
			this.cmdSave.Visible = false;
			this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
			// 
			// cmdCancel
			// 
			this.cmdCancel.AuditWriter = null;
			//this.cmdCancel.DataAccessor = null;
			this.cmdCancel.DataType = System.TypeCode.String;
			this.cmdCancel.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.cmdCancel.Enabled = false;
			this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdCancel.Location = new System.Drawing.Point(316, 128);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.TabIndex = 9;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UniqueName = "ndeCmdCancel";
			this.cmdCancel.Visible = false;
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// valPrimaryKey
			// 
			this.valPrimaryKey.AuditWriter = null;
			//this.valPrimaryKey.DataAccessor = null;
			this.valPrimaryKey.DataType = System.TypeCode.String;
			this.valPrimaryKey.DefaultSecurityState = mUtilities.Security.DefaultSecurityStates.Unlocked;
			this.valPrimaryKey.Enabled = false;
			this.valPrimaryKey.Location = new System.Drawing.Point(8, 144);
			this.valPrimaryKey.Name = "valPrimaryKey";
			this.valPrimaryKey.Size = new System.Drawing.Size(16, 16);
			this.valPrimaryKey.TabIndex = 10;
			this.valPrimaryKey.TabStop = false;
			this.valPrimaryKey.UniqueName = "ndeValPrimaryKey";
			this.valPrimaryKey.Value = false;
			this.valPrimaryKey.Visible = false;
			// 
			// NameDescEditor
			// 
			this.Controls.Add(this.valPrimaryKey);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdSave);
			this.Controls.Add(this.imgTitle);
			this.Controls.Add(this.lblTitle);
			this.Controls.Add(this.txtDesc);
			this.Controls.Add(this.lblDesc);
			this.Controls.Add(this.txtName);
			this.Controls.Add(this.lblName);
			//this.Name = "NameDescEditor";
			this.Size = new System.Drawing.Size(408, 168);
			this.UniqueName = "ndeNameDescEditor";
			this.Load += new System.EventHandler(this.NameDescEditor_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void NameDescEditor_Load(object sender, System.EventArgs e)
		{
			cmdSave.Enabled = false;
			cmdCancel.Enabled = _cancelEnabled;
		}


		private void ItemTextChanged(object sender, System.EventArgs e)
		{
			if( txtName.Text.Length == 0 )
			{
				cmdSave.Enabled = false;
				cmdCancel.Enabled = _cancelEnabled;
			}
			else
			{
				cmdSave.Enabled = true;
				cmdCancel.Enabled = true;
			}

			this.ReadyState = ReadyState.Modified;
		}


		private void cmdSave_Click(object sender, System.EventArgs e)
		{
			_name = this.ItemName;
			_desc = this.ItemDescription;

			_result = DialogResult.OK;

			OnButtonClick( sender, e );
		}


		private void cmdCancel_Click(object sender, System.EventArgs e)
		{
			this.ItemName = _name;
			this.ItemDescription = _desc;

			cmdCancel.Enabled = _cancelEnabled;

			_result = DialogResult.Cancel;

			OnButtonClick( sender, e );
		}


		public Image TitleImage
		{
			get
			{
				return this.imgTitle.Image;
			}
			set
			{
				this.imgTitle.Image = value;

				if( value == null )
				{
					imgTitle.Visible = false;
					lblTitle.Left = imgTitle.Left;
				}
				else
				{

					imgTitle.Size = imgTitle.Image.Size;

					imgTitle.Visible = true;
					lblTitle.Left = imgTitle.Left + imgTitle.Width + 8;
					lblTitle.Top = ((imgTitle.Top + imgTitle.Height) / 2) + 4;
				}
			}
		}


		public string TitleCaption
		{
			get
			{
				return this.lblTitle.Text;
			}
			set
			{
				this.lblTitle.Text = value;
			}
		}


		public string ItemPrimaryKey
		{
			get
			{
				return valPrimaryKey.Text;
			}
			set
			{
				valPrimaryKey.Text = value;
			}
		}


		public string ItemName
		{
			get
			{
				return this.txtName.Text;
			}
			set
			{
				this.txtName.Text = value;
				_name = value;
				cmdSave.Enabled = false;
				cmdCancel.Enabled = _cancelEnabled;
				this.ReadyState = ReadyState.Ok;
			}
		}


		public string ItemDescription
		{
			get
			{
				return this.txtDesc.Text;
			}
			set
			{
				this.txtDesc.Text = value;
				_desc = value;
				cmdSave.Enabled = false;
				cmdCancel.Enabled = _cancelEnabled;
				this.ReadyState = ReadyState.Ok;
			}
		}


		public ReadyState ReadyState
		{
			get
			{
				return _readyState;
			}
			set
			{
				if( _readyState != value )
				{
					_readyState = value;
					OnReadyStateChanged( this, EventArgs.Empty );
				}
			}
		}


		public mUtilities.Forms.RecordMode RecordMode
		{
			get
			{
				return _recordMode;
			}
			set
			{
				_recordMode = value;
			}
		}


		public bool UseDialogMode
		{
			get
			{
				return _cancelEnabled;
			}
			set
			{
				_cancelEnabled = value;
				cmdCancel.Enabled = value;

				if( value )
				{
					this.cmdSave.DialogResult = System.Windows.Forms.DialogResult.OK;
					this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
				}
				else
				{
					this.cmdSave.DialogResult = System.Windows.Forms.DialogResult.None;
					this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.None;
				}
			}
		}


		public bool ReadOnly
		{
			get
			{
				return _readOnly;
			}
			set
			{
				_readOnly = value;
			}
		}


		public override void ApplySecurity(AceTypes AceType, ArrayList DaclParameters, ArrayList SaclParameters)
		{
			base.ApplySecurity (AceType, DaclParameters, SaclParameters);

			if( this.ReadOnly )
			{
				cmdSave.Visible = false;
				cmdCancel.Visible = false;
				_cancelEnabled = false;
				txtName.ReadOnly = true;
				txtDesc.ReadOnly = true;
			}
		}


		public DialogResult Result
		{
			get
			{
				return _result;
			}
		}


		public void Clear()
		{
			this.ItemPrimaryKey = "";
			this.ItemName = "";
			this.ItemDescription = "";
			this.TitleCaption = "";
		}


		public void SetFocus(string control)
		{
			switch( control.ToLower() )
			{
				case "title":
				{
					this.lblTitle.Focus();
					break;
				}
				case "name":
				{
					this.txtName.Focus();
					break;
				}
				case "description":
				case "desc":
				{
					this.txtDesc.Focus();
					break;
				}
				case "save":
				{
					this.cmdSave.Focus();
					break;
				}
				case "cancel":
				{
					this.cmdCancel.Focus();
					break;
				}
				default:
				{
					this.Focus();
					break;
				}
			}
		}

	}
}