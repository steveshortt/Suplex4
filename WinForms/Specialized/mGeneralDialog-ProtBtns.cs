using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using mUtilities.Forms;
using mUtilities.Security;

namespace mUtilities.Forms.Specialized
{
	/// <summary>
	/// Summary description for GeneralDialog.
	/// </summary>
	public class mGeneralDialog : mUtilities.Forms.mForm
	{
		protected mButton cmdOk = new mButton();
		protected mButton cmdApply = new mButton();
		protected mButton cmdCancel = new mButton();
		protected mButton cmdYes = new mButton();
		protected mButton cmdNo = new mButton();
		protected mButton cmdAbort = new mButton();
		protected mButton cmdRetry = new mButton();
		protected mButton cmdIgnore = new mButton();
		protected mButton cmdSave = new mButton();
		protected mButton cmdCreate = new mButton();
		protected mButton cmdUpdate = new mButton();
		protected mButton cmdDelete = new mButton();
		protected mButton cmdClose = new mButton();
		
		protected GeneralDialogButtons _buttons = GeneralDialogButtons.None;
		protected GeneralDialogDefaultButton _defaultButton = GeneralDialogDefaultButton.Button1;
		protected GeneralDialogButtonAlignment _alignment = GeneralDialogButtonAlignment.Right;
		protected GeneralDialogCancelButton _cancelButton = GeneralDialogCancelButton.None;
		protected GeneralDialogResult _dialogResult = GeneralDialogResult.None;
		protected bool _showButtons = true;

		protected SortedList _dlgButtons = new SortedList();
		
		
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;


		#region events

		public event System.EventHandler ButtonClick;

		private void OnButtonClick( object sender, EventArgs e )
		{
			if( ButtonClick != null )
			{
				ButtonClick( sender, e );
			}
		}


		#endregion


		#region constructors

		public mGeneralDialog(
			GeneralDialogButtons buttons, GeneralDialogDefaultButton defaultButton,
			GeneralDialogCancelButton cancelButton, GeneralDialogButtonAlignment buttonAlignment)
		{
			_buttons = buttons;
			_defaultButton = defaultButton;
			_cancelButton = cancelButton;
			_alignment = buttonAlignment;

			Init();
		}


		public mGeneralDialog(
			GeneralDialogButtons buttons, GeneralDialogDefaultButton defaultButton,
			GeneralDialogCancelButton cancelButton)
		{
			_buttons = buttons;
			_defaultButton = defaultButton;
			_cancelButton = cancelButton;

			Init();
		}


		public mGeneralDialog(
			GeneralDialogButtons buttons, GeneralDialogDefaultButton defaultButton)
		{
			_buttons = buttons;
			_defaultButton = defaultButton;

			Init();
		}


		public mGeneralDialog()
		{
			Init();
		}


		private void Init()
		{
			InitializeComponent();	// Required for Windows Form Designer support
			BuildButtonsList();
			RefreshButtons();
		}


		#endregion


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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// GeneralDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 271);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GeneralDialog";

		}
		#endregion


		#region properties

		public virtual GeneralDialogButtons GeneralDialogButtons
		{
			get
			{
				return _buttons;
			}
			set
			{
				if( _buttons != value )
				{
					_buttons = value;
					RefreshButtons();
				}
			}
		}


		public virtual GeneralDialogDefaultButton GeneralDialogDefaultButton
		{
			get
			{
				return _defaultButton;
			}
			set
			{
				if( _defaultButton != value )
				{
					_defaultButton = value;
					RefreshButtons();
				}
			}
		}


		public virtual GeneralDialogButtonAlignment GeneralDialogButtonAlignment
		{
			get
			{
				return _alignment;
			}
			set
			{
				if( _alignment != value )
				{
					_alignment = value;
					RefreshButtons();
				}
			}
		}


		public virtual GeneralDialogCancelButton GeneralDialogCancelButton
		{
			get
			{
				return _cancelButton;
			}
			set
			{
				if( _cancelButton != value )
				{
					_cancelButton = value;
					RefreshButtons();
				}
			}
		}


		public virtual bool ShowButtons
		{
			get
			{
				return _showButtons;
			}
			set
			{
				if( !value )
				{
					HideButtons();
				}

				if( !_showButtons && value )
				{
					RefreshButtons();
				}

				_showButtons = value;
			}
		}


		public virtual GeneralDialogResult GeneralDialogResult
		{
			get
			{
				return _dialogResult;
			}
		}


		#endregion


		#region methods

		new public GeneralDialogResult ShowDialog(IWin32Window owner)
		{
			base.ShowDialog( owner );
			return _dialogResult;
		}

		new public GeneralDialogResult ShowDialog()
		{
			base.ShowDialog();
			return _dialogResult;
		}


		public virtual void RefreshButtons()
		{
			HideButtons();

			if( _buttons != GeneralDialogButtons.None )
			{
				Button[] b = (Button[])_dlgButtons[_buttons];
				AnchorStyles aSty = AnchorStyles.Bottom | AnchorStyles.Right;
				int top = this.Size.Height - 56;
				int left = this.Size.Width - ((b.Length * 80) + 8);
			
				if( _alignment == GeneralDialogButtonAlignment.Left )
				{
					aSty = AnchorStyles.Bottom | AnchorStyles.Left;
					left = 8;
				}

				for( int n=0; n<b.Length; n++ )
				{
					this.Controls.Add( b[n] );
					b[n].Top = top;
					b[n].Left = left;
					b[n].Anchor = aSty;
					b[n].Visible = true;
					left += 80;
				}

				if( ((int)_defaultButton)+1 <= b.Length )
				{
					b[(int)_defaultButton].Select();
				}

				if( ((int)_cancelButton)+1 <= b.Length )
				{
					//int i = this.Controls.IndexOf( (Button)b[(int)_cancelButton] );
					//this.CancelButton = (Button)this.Controls[i];
					this.CancelButton = (Button)b[(int)_cancelButton];
				}
				else
				{
					this.CancelButton = null;
				}
			}
			else
			{
				this.CancelButton = null;
			}
		}


		//this is kind of brute-force-ish, but it's simple enuf
		protected void BuildButtonsList()
		{
			cmdOk.Text = "&OK";
			cmdOk.DialogResult = DialogResult.OK;
			cmdOk.FlatStyle = FlatStyle.System;
			cmdOk.Tag = GeneralDialogResult.OK;
			cmdOk.Click += new EventHandler(cmdButton_Click);
			cmdOk.DefaultSecurityState = DefaultSecurityStates.Unlocked;

			cmdApply.Text = "&Apply";
			cmdApply.DialogResult = DialogResult.OK;
			cmdApply.FlatStyle = FlatStyle.System;
			cmdApply.Tag = GeneralDialogResult.Apply;
			cmdApply.Click += new EventHandler(cmdButton_Click);
			cmdApply.DefaultSecurityState = DefaultSecurityStates.Unlocked;

			cmdCancel.Text = "&Cancel";
			cmdCancel.DialogResult = DialogResult.Cancel;
			cmdCancel.FlatStyle = FlatStyle.System;
			cmdCancel.Tag = GeneralDialogResult.Cancel;
			cmdCancel.Click += new EventHandler(cmdButton_Click);
			cmdCancel.DefaultSecurityState = DefaultSecurityStates.Unlocked;

			cmdYes.Text = "&Yes";
			cmdYes.DialogResult = DialogResult.Yes;
			cmdYes.FlatStyle = FlatStyle.System;
			cmdYes.Tag = GeneralDialogResult.Yes;
			cmdYes.Click += new EventHandler(cmdButton_Click);
			cmdYes.DefaultSecurityState = DefaultSecurityStates.Unlocked;

			cmdNo.Text = "&No";
			cmdNo.DialogResult = DialogResult.No;
			cmdNo.FlatStyle = FlatStyle.System;
			cmdNo.Tag = GeneralDialogResult.No;
			cmdNo.Click += new EventHandler(cmdButton_Click);
			cmdNo.DefaultSecurityState = DefaultSecurityStates.Unlocked;

			cmdAbort.Text = "&Abort";
			cmdAbort.DialogResult = DialogResult.Abort;
			cmdAbort.FlatStyle = FlatStyle.System;
			cmdAbort.Tag = GeneralDialogResult.Abort;
			cmdAbort.Click += new EventHandler(cmdButton_Click);
			cmdAbort.DefaultSecurityState = DefaultSecurityStates.Unlocked;

			cmdRetry.Text = "&Retry";
			cmdRetry.DialogResult = DialogResult.Retry;
			cmdRetry.FlatStyle = FlatStyle.System;
			cmdRetry.Tag = GeneralDialogResult.Retry;
			cmdRetry.Click += new EventHandler(cmdButton_Click);
			cmdRetry.DefaultSecurityState = DefaultSecurityStates.Unlocked;

			cmdIgnore.Text = "&Ignore";
			cmdIgnore.DialogResult = DialogResult.Ignore;
			cmdIgnore.FlatStyle = FlatStyle.System;
			cmdIgnore.Tag = GeneralDialogResult.Ignore;
			cmdIgnore.Click += new EventHandler(cmdButton_Click);
			cmdIgnore.DefaultSecurityState = DefaultSecurityStates.Unlocked;

			cmdSave.Text = "&Save";
			cmdSave.DialogResult = DialogResult.OK;
			cmdSave.FlatStyle = FlatStyle.System;
			cmdSave.Tag = GeneralDialogResult.Save;
			cmdSave.Click += new EventHandler(cmdButton_Click);
			cmdSave.DefaultSecurityState = DefaultSecurityStates.Unlocked;

			cmdCreate.Text = "C&reate";
			cmdCreate.DialogResult = DialogResult.OK;
			cmdCreate.FlatStyle = FlatStyle.System;
			cmdCreate.Tag = GeneralDialogResult.Create;
			cmdCreate.Click += new EventHandler(cmdButton_Click);
			cmdCreate.DefaultSecurityState = DefaultSecurityStates.Unlocked;

			cmdUpdate.Text = "&Update";
			cmdUpdate.DialogResult = DialogResult.OK;
			cmdUpdate.FlatStyle = FlatStyle.System;
			cmdUpdate.Tag = GeneralDialogResult.Update;
			cmdUpdate.Click += new EventHandler(cmdButton_Click);
			cmdUpdate.DefaultSecurityState = DefaultSecurityStates.Unlocked;

			cmdDelete.Text = "&Delete";
			cmdDelete.DialogResult = DialogResult.OK;
			cmdDelete.FlatStyle = FlatStyle.System;
			cmdDelete.Tag = GeneralDialogResult.Delete;
			cmdDelete.Click += new EventHandler(cmdButton_Click);
			cmdDelete.DefaultSecurityState = DefaultSecurityStates.Unlocked;


			cmdClose.Text = "&Close";
			cmdClose.DialogResult = DialogResult.OK;
			cmdClose.FlatStyle = FlatStyle.System;
			cmdClose.Tag = GeneralDialogResult.Delete;
			cmdClose.Click += new EventHandler(cmdButton_Click);
			cmdClose.DefaultSecurityState = DefaultSecurityStates.Unlocked;


			HideButtons();


			_dlgButtons.Add( GeneralDialogButtons.AbortRetryIgnore, new Button[] {cmdAbort,cmdRetry,cmdIgnore} );
			_dlgButtons.Add( GeneralDialogButtons.OK, new Button[] {cmdOk} );
			_dlgButtons.Add( GeneralDialogButtons.OKCancel, new Button[] {cmdOk,cmdCancel} );
			_dlgButtons.Add( GeneralDialogButtons.OKCancelApply, new Button[] {cmdOk,cmdCancel,cmdApply} );
			_dlgButtons.Add( GeneralDialogButtons.RetryCancel, new Button[] {cmdRetry,cmdCancel} );
			_dlgButtons.Add( GeneralDialogButtons.YesNo, new Button[] {cmdYes,cmdNo} );
			_dlgButtons.Add( GeneralDialogButtons.YesNoCancel, new Button[] {cmdYes,cmdNo,cmdCancel} );
			_dlgButtons.Add( GeneralDialogButtons.SaveCancel, new Button[] {cmdSave,cmdCancel} );
			_dlgButtons.Add( GeneralDialogButtons.CreateCancel, new Button[] {cmdCreate,cmdCancel} );
			_dlgButtons.Add( GeneralDialogButtons.UpdateCancel, new Button[] {cmdUpdate,cmdCancel} );
			_dlgButtons.Add( GeneralDialogButtons.DeleteCancel, new Button[] {cmdDelete,cmdCancel} );
			_dlgButtons.Add( GeneralDialogButtons.OKClose, new Button[] {cmdOk,cmdClose} );
		}


		protected virtual void HideButtons()
		{
			cmdOk.Visible = false;
			cmdApply.Visible = false;
			cmdCancel.Visible = false;
			cmdYes.Visible = false;
			cmdNo.Visible = false;
			cmdAbort.Visible = false;
			cmdRetry.Visible = false;
			cmdIgnore.Visible = false;
			cmdSave.Visible = false;
			cmdCreate.Visible = false;
			cmdUpdate.Visible = false;
			cmdDelete.Visible = false;
			cmdClose.Visible = false;
		}


		#endregion


		#region event handlers

		private void cmdButton_Click(object sender, EventArgs e)
		{
			_dialogResult = (GeneralDialogResult)((Button)sender).Tag;

			OnButtonClick( sender, e );
		}


		#endregion
	}


	#region GeneralDialog enums

	public enum GeneralDialogButtonAlignment
	{
		Left,
		Right
	}


	public enum GeneralDialogCancelButton
	{
		Button1 = 0,
		Button2 = 1,
		Button3 = 2,
		None = 8
	}


	public enum GeneralDialogDefaultButton
	{
		Button1 = 0,
		Button2 = 1,
		Button3 = 2,
	}


	public enum GeneralDialogButtons
	{
		AbortRetryIgnore,
		OK,
		OKCancel,
		OKCancelApply,
		RetryCancel,
		YesNo,
		YesNoCancel,
		SaveCancel,
		CreateCancel,
		UpdateCancel,
		DeleteCancel,
		OKClose,
		None
	}


	public enum GeneralDialogResult
	{
		Abort,
		Retry,
		Ignore,
		OK,
		Cancel,
		Apply,
		Yes,
		No,
		None,
		Save,
		Create,
		Update,
		Delete,
		Close
	}


	#endregion


}





















