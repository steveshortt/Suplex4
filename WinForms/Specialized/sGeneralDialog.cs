using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Suplex.Forms;
using Suplex.WinForms;
using Suplex.Security;

namespace Suplex.WinForms.Specialized
{
	public class sGeneralDialog : Suplex.WinForms.sForm
	{
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected sButton cmdOk = new sButton();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected sButton cmdApply = new sButton();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected sButton cmdCancel = new sButton();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected sButton cmdYes = new sButton();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected sButton cmdNo = new sButton();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected sButton cmdAbort = new sButton();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected sButton cmdRetry = new sButton();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected sButton cmdIgnore = new sButton();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected sButton cmdSave = new sButton();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected sButton cmdCreate = new sButton();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected sButton cmdUpdate = new sButton();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected sButton cmdDelete = new sButton();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected sButton cmdClose = new sButton();

		protected GeneralDialogButtons _buttons = GeneralDialogButtons.None;
		protected GeneralDialogDefaultButton _defaultButton = GeneralDialogDefaultButton.Button1;
		protected GeneralDialogButtonAlignment _alignment = GeneralDialogButtonAlignment.Right;
		protected GeneralDialogCancelButton _cancelButton = GeneralDialogCancelButton.None;
		protected GeneralDialogResult _dialogResult = GeneralDialogResult.None;
		protected Size _buttonSize = new Size( 72, 23 );
		protected Point _buttonOffset = new Point( 8, 8 );
		protected int _buttonSpace = 8;
		protected bool _showButtons = true;

		protected SortedList _dlgButtons = new SortedList();


		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;


		#region events
		public event System.EventHandler ButtonClick;

		protected void OnButtonClick(object sender, EventArgs e)
		{
			if( ButtonClick != null )
			{
				ButtonClick( sender, e );
			}
		}
		#endregion


		#region constructors
		public sGeneralDialog(GeneralDialogButtons buttons, GeneralDialogDefaultButton defaultButton,
		GeneralDialogCancelButton cancelButton, GeneralDialogButtonAlignment buttonAlignment)
		{
			_buttons = buttons;
			_defaultButton = defaultButton;
			_cancelButton = cancelButton;
			_alignment = buttonAlignment;

			Init();
		}

		public sGeneralDialog(GeneralDialogButtons buttons, GeneralDialogDefaultButton defaultButton,
		GeneralDialogCancelButton cancelButton)
		{
			_buttons = buttons;
			_defaultButton = defaultButton;
			_cancelButton = cancelButton;

			Init();
		}

		public sGeneralDialog(GeneralDialogButtons buttons, GeneralDialogDefaultButton defaultButton)
		{
			_buttons = buttons;
			_defaultButton = defaultButton;

			Init();
		}

		public sGeneralDialog()
		{
			Init();
		}

		private void Init()
		{
			InitializeComponent(); // Required for Windows Form Designer support
			BuildButtonsList();
			RefreshButtons();
		}
		#endregion


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if( disposing )
			{
				if( components != null )
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
			this.AutoScaleBaseSize = new System.Drawing.Size( 5, 13 );
			this.ClientSize = new System.Drawing.Size( 292, 271 );
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			//this.Name = "GeneralDialog";

		}
		#endregion


		#region properties
		[DefaultValue( GeneralDialogButtons.None ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual GeneralDialogButtons GeneralDialogButtons
		{
			get { return _buttons; }
			set
			{
				if( _buttons != value )
				{
					_buttons = value;
					RefreshButtons();
				}
			}
		}

		public Size GeneralDialogButtonSize
		{
			get { return _buttonSize; }
			set
			{
				if( _buttonSize != value )
				{
					_buttonSize = value;
					RefreshButtons();
				}
			}
		}

		public Point GeneralDialogButtonOffset
		{
			get { return _buttonOffset; }
			set
			{
				if( _buttonOffset != value )
				{
					_buttonOffset = value;
					RefreshButtons();
				}
			}
		}

		public int GeneralDialogButtonSpace
		{
			get { return _buttonSpace; }
			set
			{
				if( _buttonSpace != value )
				{
					_buttonSpace = value;
					RefreshButtons();
				}
			}
		}

		private int GeneralDialogButtonTotalSpace
		{
			get { return _buttonSize.Width + _buttonSpace; }
		}

		[DefaultValue( GeneralDialogDefaultButton.Button1 ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual GeneralDialogDefaultButton GeneralDialogDefaultButton
		{
			get { return _defaultButton; }
			set
			{
				if( _defaultButton != value )
				{
					_defaultButton = value;
					RefreshButtons();
				}
			}
		}

		[DefaultValue( GeneralDialogButtonAlignment.Right ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual GeneralDialogButtonAlignment GeneralDialogButtonAlignment
		{
			get { return _alignment; }
			set
			{
				if( _alignment != value )
				{
					_alignment = value;
					RefreshButtons();
				}
			}
		}

		[DefaultValue( GeneralDialogCancelButton.None ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual GeneralDialogCancelButton GeneralDialogCancelButton
		{
			get { return _cancelButton; }
			set
			{
				if( _cancelButton != value )
				{
					_cancelButton = value;
					RefreshButtons();
				}
			}
		}

		[DefaultValue( true ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual bool ShowButtons
		{
			get { return _showButtons; }
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

		[Browsable( false ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual GeneralDialogResult GeneralDialogResult
		{
			get { return _dialogResult; }
		}
		#endregion


		#region methods
		new public virtual GeneralDialogResult ShowDialog(IWin32Window owner)
		{
			base.ShowDialog( owner );
			return _dialogResult;
		}

		new public virtual GeneralDialogResult ShowDialog()
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
				int top = this.ClientSize.Height - _buttonSize.Height - _buttonOffset.Y;
				int left = this.ClientSize.Width - ( ( ( b.Length * this.GeneralDialogButtonTotalSpace ) - _buttonSpace ) + _buttonOffset.X );

				if( _alignment == GeneralDialogButtonAlignment.Left )
				{
					aSty = AnchorStyles.Bottom | AnchorStyles.Left;
					left = _buttonOffset.X;
				}

				for( int n = 0; n < b.Length; n++ )
				{
					this.Controls.Add( b[n] );
					b[n].Top = top;
					b[n].Left = left;
					b[n].Size = _buttonSize;
					b[n].Anchor = aSty;
					b[n].Visible = true;
					left += this.GeneralDialogButtonTotalSpace;
				}

				if( ( (int)_defaultButton ) + 1 <= b.Length )
				{
					b[(int)_defaultButton].Select();
					this.AcceptButton = (Button)b[(int)_defaultButton];
				}
				else
				{
					this.AcceptButton = null;
				}

				if( ( (int)_cancelButton ) + 1 <= b.Length )
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
				this.AcceptButton = null;
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
			cmdOk.Click += new EventHandler( cmdOk_Click );
			//cmdOk.Security.DefaultState = DefaultSecurityState.Unlocked;

			cmdApply.Text = "&Apply";
			cmdApply.DialogResult = DialogResult.OK;
			cmdApply.FlatStyle = FlatStyle.System;
			cmdApply.Tag = GeneralDialogResult.Apply;
			cmdApply.Click += new EventHandler( cmdApply_Click );
			//cmdApply.Security.DefaultState = DefaultSecurityState.Unlocked;

			cmdCancel.Text = "&Cancel";
			cmdCancel.DialogResult = DialogResult.Cancel;
			cmdCancel.FlatStyle = FlatStyle.System;
			cmdCancel.Tag = GeneralDialogResult.Cancel;
			cmdCancel.Click += new EventHandler( cmdCancel_Click );
			//cmdCancel.Security.DefaultState = DefaultSecurityState.Unlocked;

			cmdYes.Text = "&Yes";
			cmdYes.DialogResult = DialogResult.Yes;
			cmdYes.FlatStyle = FlatStyle.System;
			cmdYes.Tag = GeneralDialogResult.Yes;
			cmdYes.Click += new EventHandler( cmdYes_Click );
			//cmdYes.Security.DefaultState = DefaultSecurityState.Unlocked;

			cmdNo.Text = "&No";
			cmdNo.DialogResult = DialogResult.No;
			cmdNo.FlatStyle = FlatStyle.System;
			cmdNo.Tag = GeneralDialogResult.No;
			cmdNo.Click += new EventHandler( cmdNo_Click );
			//cmdNo.Security.DefaultState = DefaultSecurityState.Unlocked;

			cmdAbort.Text = "&Abort";
			cmdAbort.DialogResult = DialogResult.Abort;
			cmdAbort.FlatStyle = FlatStyle.System;
			cmdAbort.Tag = GeneralDialogResult.Abort;
			cmdAbort.Click += new EventHandler( cmdAbort_Click );
			//cmdAbort.Security.DefaultState = DefaultSecurityState.Unlocked;

			cmdRetry.Text = "&Retry";
			cmdRetry.DialogResult = DialogResult.Retry;
			cmdRetry.FlatStyle = FlatStyle.System;
			cmdRetry.Tag = GeneralDialogResult.Retry;
			cmdRetry.Click += new EventHandler( cmdRetry_Click );
			//cmdRetry.Security.DefaultState = DefaultSecurityState.Unlocked;

			cmdIgnore.Text = "&Ignore";
			cmdIgnore.DialogResult = DialogResult.Ignore;
			cmdIgnore.FlatStyle = FlatStyle.System;
			cmdIgnore.Tag = GeneralDialogResult.Ignore;
			cmdIgnore.Click += new EventHandler( cmdIgnore_Click );
			//cmdIgnore.Security.DefaultState = DefaultSecurityState.Unlocked;

			cmdSave.Text = "&Save";
			cmdSave.DialogResult = DialogResult.OK;
			cmdSave.FlatStyle = FlatStyle.System;
			cmdSave.Tag = GeneralDialogResult.Save;
			cmdSave.Click += new EventHandler( cmdSave_Click );
			//cmdSave.Security.DefaultState = DefaultSecurityState.Unlocked;

			cmdCreate.Text = "C&reate";
			cmdCreate.DialogResult = DialogResult.OK;
			cmdCreate.FlatStyle = FlatStyle.System;
			cmdCreate.Tag = GeneralDialogResult.Create;
			cmdCreate.Click += new EventHandler( cmdCreate_Click );
			//cmdCreate.Security.DefaultState = DefaultSecurityState.Unlocked;

			cmdUpdate.Text = "&Update";
			cmdUpdate.DialogResult = DialogResult.OK;
			cmdUpdate.FlatStyle = FlatStyle.System;
			cmdUpdate.Tag = GeneralDialogResult.Update;
			cmdUpdate.Click += new EventHandler( cmdUpdate_Click );
			//cmdUpdate.Security.DefaultState = DefaultSecurityState.Unlocked;

			cmdDelete.Text = "&Delete";
			cmdDelete.DialogResult = DialogResult.OK;
			cmdDelete.FlatStyle = FlatStyle.System;
			cmdDelete.Tag = GeneralDialogResult.Delete;
			cmdDelete.Click += new EventHandler( cmdDelete_Click );
			//cmdDelete.Security.DefaultState = DefaultSecurityState.Unlocked;


			cmdClose.Text = "&Close";
			cmdClose.DialogResult = DialogResult.OK;
			cmdClose.FlatStyle = FlatStyle.System;
			cmdClose.Tag = GeneralDialogResult.Delete;
			cmdClose.Click += new EventHandler( cmdClose_Click );
			//cmdClose.Security.DefaultState = DefaultSecurityState.Unlocked;


			HideButtons();


			_dlgButtons.Add( GeneralDialogButtons.AbortRetryIgnore, new Button[] { cmdAbort, cmdRetry, cmdIgnore } );
			_dlgButtons.Add( GeneralDialogButtons.OK, new Button[] { cmdOk } );
			_dlgButtons.Add( GeneralDialogButtons.OKCancel, new Button[] { cmdOk, cmdCancel } );
			_dlgButtons.Add( GeneralDialogButtons.OKCancelApply, new Button[] { cmdOk, cmdCancel, cmdApply } );
			_dlgButtons.Add( GeneralDialogButtons.RetryCancel, new Button[] { cmdRetry, cmdCancel } );
			_dlgButtons.Add( GeneralDialogButtons.YesNo, new Button[] { cmdYes, cmdNo } );
			_dlgButtons.Add( GeneralDialogButtons.YesNoCancel, new Button[] { cmdYes, cmdNo, cmdCancel } );
			_dlgButtons.Add( GeneralDialogButtons.SaveCancel, new Button[] { cmdSave, cmdCancel } );
			_dlgButtons.Add( GeneralDialogButtons.CreateCancel, new Button[] { cmdCreate, cmdCancel } );
			_dlgButtons.Add( GeneralDialogButtons.UpdateCancel, new Button[] { cmdUpdate, cmdCancel } );
			_dlgButtons.Add( GeneralDialogButtons.DeleteCancel, new Button[] { cmdDelete, cmdCancel } );
			_dlgButtons.Add( GeneralDialogButtons.OKClose, new Button[] { cmdOk, cmdClose } );
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
		protected virtual void cmdOk_Click(object sender, EventArgs e)
		{
			BubbleEvent( sender, e );
		}

		protected virtual void cmdApply_Click(object sender, EventArgs e)
		{
			BubbleEvent( sender, e );
		}

		protected virtual void cmdCancel_Click(object sender, EventArgs e)
		{
			BubbleEvent( sender, e );
		}

		protected virtual void cmdYes_Click(object sender, EventArgs e)
		{
			BubbleEvent( sender, e );
		}

		protected virtual void cmdNo_Click(object sender, EventArgs e)
		{
			BubbleEvent( sender, e );
		}

		protected virtual void cmdAbort_Click(object sender, EventArgs e)
		{
			BubbleEvent( sender, e );
		}

		protected virtual void cmdRetry_Click(object sender, EventArgs e)
		{
			BubbleEvent( sender, e );
		}

		protected virtual void cmdIgnore_Click(object sender, EventArgs e)
		{
			BubbleEvent( sender, e );
		}

		protected virtual void cmdSave_Click(object sender, EventArgs e)
		{
			BubbleEvent( sender, e );
		}

		protected virtual void cmdCreate_Click(object sender, EventArgs e)
		{
			BubbleEvent( sender, e );
		}

		protected virtual void cmdUpdate_Click(object sender, EventArgs e)
		{
			BubbleEvent( sender, e );
		}

		protected virtual void cmdDelete_Click(object sender, EventArgs e)
		{
			BubbleEvent( sender, e );
		}

		protected virtual void cmdClose_Click(object sender, EventArgs e)
		{
			BubbleEvent( sender, e );
		}

		private void BubbleEvent(object sender, EventArgs e)
		{
			_dialogResult = (GeneralDialogResult)( (Button)sender ).Tag;

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
		None = 8
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