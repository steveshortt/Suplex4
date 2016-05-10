using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Suplex.WinForms.Specialized
{
	/// <summary>
	/// Summary description for GeneralDialog.
	/// </summary>
	public class GeneralDialog : System.Windows.Forms.Form
	{
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected Button cmdOk = new Button();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected Button cmdApply = new Button();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected Button cmdCancel = new Button();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected Button cmdYes = new Button();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected Button cmdNo = new Button();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected Button cmdAbort = new Button();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected Button cmdRetry = new Button();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected Button cmdIgnore = new Button();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected Button cmdSave = new Button();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected Button cmdCreate = new Button();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected Button cmdUpdate = new Button();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected Button cmdDelete = new Button();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		protected Button cmdClose = new Button();

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

		protected void OnButtonClick(object sender, EventArgs e)
		{
			if( ButtonClick != null )
			{
				ButtonClick( sender, e );
			}
		}
		#endregion


		#region constructors
		public GeneralDialog(GeneralDialogButtons buttons, GeneralDialogDefaultButton defaultButton,
			GeneralDialogCancelButton cancelButton, GeneralDialogButtonAlignment buttonAlignment)
		{
			_buttons = buttons;
			_defaultButton = defaultButton;
			_cancelButton = cancelButton;
			_alignment = buttonAlignment;

			Init();
		}

		public GeneralDialog(GeneralDialogButtons buttons, GeneralDialogDefaultButton defaultButton,
			GeneralDialogCancelButton cancelButton)
		{
			_buttons = buttons;
			_defaultButton = defaultButton;
			_cancelButton = cancelButton;

			Init();
		}

		public GeneralDialog(GeneralDialogButtons buttons, GeneralDialogDefaultButton defaultButton)
		{
			_buttons = buttons;
			_defaultButton = defaultButton;

			Init();
		}

		public GeneralDialog()
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
				int left = this.Size.Width - ( ( b.Length * 80 ) + 8 );

				if( _alignment == GeneralDialogButtonAlignment.Left )
				{
					aSty = AnchorStyles.Bottom | AnchorStyles.Left;
					left = 8;
				}

				for( int n = 0; n < b.Length; n++ )
				{
					this.Controls.Add( b[n] );
					b[n].Top = top;
					b[n].Left = left;
					b[n].Anchor = aSty;
					b[n].Visible = true;
					left += 80;
				}

				if( ( (int)_defaultButton ) + 1 <= b.Length )
				{
					b[(int)_defaultButton].Select();
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
			cmdOk.Click += new EventHandler( cmdButton_Click );

			cmdApply.Text = "&Apply";
			cmdApply.DialogResult = DialogResult.OK;
			cmdApply.FlatStyle = FlatStyle.System;
			cmdApply.Tag = GeneralDialogResult.Apply;
			cmdApply.Click += new EventHandler( cmdButton_Click );

			cmdCancel.Text = "&Cancel";
			cmdCancel.DialogResult = DialogResult.Cancel;
			cmdCancel.FlatStyle = FlatStyle.System;
			cmdCancel.Tag = GeneralDialogResult.Cancel;
			cmdCancel.Click += new EventHandler( cmdButton_Click );

			cmdYes.Text = "&Yes";
			cmdYes.DialogResult = DialogResult.Yes;
			cmdYes.FlatStyle = FlatStyle.System;
			cmdYes.Tag = GeneralDialogResult.Yes;
			cmdYes.Click += new EventHandler( cmdButton_Click );

			cmdNo.Text = "&No";
			cmdNo.DialogResult = DialogResult.No;
			cmdNo.FlatStyle = FlatStyle.System;
			cmdNo.Tag = GeneralDialogResult.No;
			cmdNo.Click += new EventHandler( cmdButton_Click );

			cmdAbort.Text = "&Abort";
			cmdAbort.DialogResult = DialogResult.Abort;
			cmdAbort.FlatStyle = FlatStyle.System;
			cmdAbort.Tag = GeneralDialogResult.Abort;
			cmdAbort.Click += new EventHandler( cmdButton_Click );

			cmdRetry.Text = "&Retry";
			cmdRetry.DialogResult = DialogResult.Retry;
			cmdRetry.FlatStyle = FlatStyle.System;
			cmdRetry.Tag = GeneralDialogResult.Retry;
			cmdRetry.Click += new EventHandler( cmdButton_Click );

			cmdIgnore.Text = "&Ignore";
			cmdIgnore.DialogResult = DialogResult.Ignore;
			cmdIgnore.FlatStyle = FlatStyle.System;
			cmdIgnore.Tag = GeneralDialogResult.Ignore;
			cmdIgnore.Click += new EventHandler( cmdButton_Click );

			cmdSave.Text = "&Save";
			cmdSave.DialogResult = DialogResult.OK;
			cmdSave.FlatStyle = FlatStyle.System;
			cmdSave.Tag = GeneralDialogResult.Save;
			cmdSave.Click += new EventHandler( cmdButton_Click );

			cmdCreate.Text = "C&reate";
			cmdCreate.DialogResult = DialogResult.OK;
			cmdCreate.FlatStyle = FlatStyle.System;
			cmdCreate.Tag = GeneralDialogResult.Create;
			cmdCreate.Click += new EventHandler( cmdButton_Click );

			cmdUpdate.Text = "&Update";
			cmdUpdate.DialogResult = DialogResult.OK;
			cmdUpdate.FlatStyle = FlatStyle.System;
			cmdUpdate.Tag = GeneralDialogResult.Update;
			cmdUpdate.Click += new EventHandler( cmdButton_Click );

			cmdDelete.Text = "&Delete";
			cmdDelete.DialogResult = DialogResult.OK;
			cmdDelete.FlatStyle = FlatStyle.System;
			cmdDelete.Tag = GeneralDialogResult.Delete;
			cmdDelete.Click += new EventHandler( cmdButton_Click );


			cmdClose.Text = "&Close";
			cmdClose.DialogResult = DialogResult.OK;
			cmdClose.FlatStyle = FlatStyle.System;
			cmdClose.Tag = GeneralDialogResult.Delete;
			cmdClose.Click += new EventHandler( cmdButton_Click );


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
		private void cmdButton_Click(object sender, EventArgs e)
		{
			_dialogResult = (GeneralDialogResult)( (Button)sender ).Tag;

			OnButtonClick( sender, e );
		}
		#endregion
	}
}