using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;

using Suplex.Forms;

namespace Suplex.WinForms
{
	/// <summary>
	/// Summary description for ValidationSummary.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(System.Web.UI.WebControls.ValidationSummary))]
	public class ValidationSummary : System.Windows.Forms.UserControl, IValidationSummaryControl
	{
		private SortedList _controls = new SortedList();
		private SortedList _sortedControls = null;

		private int _errCount = 0;
		private bool _autoHide = true;
		private bool _autoHideEnabled = true;

		private System.Windows.Forms.ErrorProvider _errProvider = null;

		private System.Drawing.Color _linkColor = Color.Red;
		private System.Drawing.Color _activeLinkColor = Color.Black;
		private System.Drawing.Color _flashColor = Color.Yellow;

		private int _rightSpacer = 50;

		private Control _currentControl = null;

		private System.Windows.Forms.GroupBox grpErrors;
		private System.Windows.Forms.Panel pnlErrors;
		private System.Windows.Forms.ImageList imglTacks;
		private System.Windows.Forms.Label lblAutoHide;
		private System.ComponentModel.IContainer components;




		public ValidationSummary()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

			ToolTip ah = new ToolTip();
			ah.SetToolTip( lblAutoHide, "Toggle Auto Hide" );

			this.Visible = false;

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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ValidationSummary));
			this.grpErrors = new System.Windows.Forms.GroupBox();
			this.pnlErrors = new System.Windows.Forms.Panel();
			this.imglTacks = new System.Windows.Forms.ImageList(this.components);
			this.lblAutoHide = new System.Windows.Forms.Label();
			this.grpErrors.SuspendLayout();
			this.SuspendLayout();
			//
			// grpErrors
			//
			this.grpErrors.Controls.Add(this.pnlErrors);
			this.grpErrors.Controls.Add(this.lblAutoHide);
			this.grpErrors.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grpErrors.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.grpErrors.Location = new System.Drawing.Point(0, 0);
			this.grpErrors.Name = "grpErrors";
			this.grpErrors.Size = new System.Drawing.Size(120, 120);
			this.grpErrors.TabIndex = 1;
			this.grpErrors.TabStop = false;
			this.grpErrors.Text = " Errors:";
			//
			// pnlErrors
			//
			this.pnlErrors.AutoScroll = true;
			this.pnlErrors.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlErrors.Location = new System.Drawing.Point(3, 16);
			this.pnlErrors.Name = "pnlErrors";
			this.pnlErrors.Size = new System.Drawing.Size(114, 101);
			this.pnlErrors.TabIndex = 1;
			//
			// imglTacks
			//
			this.imglTacks.ImageSize = new System.Drawing.Size(11, 11);
			this.imglTacks.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imglTacks.ImageStream")));
			this.imglTacks.TransparentColor = System.Drawing.Color.Transparent;
			//
			// lblAutoHide
			//
			this.lblAutoHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblAutoHide.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblAutoHide.ImageIndex = 0;
			this.lblAutoHide.ImageList = this.imglTacks;
			this.lblAutoHide.Location = new System.Drawing.Point(98, 2);
			this.lblAutoHide.Name = "lblAutoHide";
			this.lblAutoHide.Size = new System.Drawing.Size(16, 12);
			this.lblAutoHide.TabIndex = 2;
			this.lblAutoHide.Click += new System.EventHandler(this.lblAutohide_Click);
			//
			// ValidationSummary
			//
			this.Controls.Add(this.grpErrors);
			//this.Name = "ValidationSummary";
			this.Size = new System.Drawing.Size(120, 120);
			this.grpErrors.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion



		#region boring public accessors

		[Browsable(false)]
		[ImmutableObject(false)]
		public override string Text
		{
			get
			{
				return grpErrors.Text;
			}
			set
			{
				grpErrors.Text = value;
			}
		}


		[DefaultValue( " Errors:" )]
		public string TitleText
		{
			get
			{
				return grpErrors.Text;
			}
			set
			{
				grpErrors.Text = value;
			}
		}


		[DefaultValue( typeof(System.Drawing.Color), "Red" )]
		public System.Drawing.Color LinkColor
		{
			get
			{
				return _linkColor;
			}
			set
			{
				_linkColor = value;
			}
		}


		[DefaultValue( typeof(System.Drawing.Color), "Black" )]
		public System.Drawing.Color ActiveLinkColor
		{
			get
			{
				return _activeLinkColor;
			}
			set
			{
				_activeLinkColor = value;
			}
		}


		[DefaultValue( typeof(System.Drawing.Color), "Yellow" )]
		public System.Drawing.Color FlashColor
		{
			get
			{
				return _flashColor;
			}
			set
			{
				_flashColor = value;
			}
		}


		new public Color BackColor
		{
			get
			{
				return pnlErrors.BackColor;
			}
			set
			{
				pnlErrors.BackColor = value;
			}
		}


		[DefaultValue( true )]
		public bool AutoHideEnabled
		{
			get
			{
				return _autoHideEnabled;
			}
			set
			{
				_autoHideEnabled = true;
				lblAutoHide.Visible = value;
			}
		}


		[DefaultValue( true )]
		public bool AutoHideOn
		{
			get
			{
				return _autoHide;
			}
			set
			{
				_autoHide = value;

				lblAutoHide.ImageIndex = _autoHide ? 0 : 1;
			}
		}


		[Browsable(false)]
		public int ErrorCount
		{
			get
			{
				return _errCount;
			}
		}


		public ErrorProvider ParentErrorProvider
		{
			get
			{
				return _errProvider;
			}
			set
			{
				_errProvider = value;
			}
		}


		#endregion


		#region meat

		public void ClearErrors()
		{
			IEnumerator lbls = _controls.Values.GetEnumerator();
			while( lbls.MoveNext() )
			{
				ErrLabelShow e = (ErrLabelShow)lbls.Current;
				e.Text = string.Empty;

				if( _errProvider != null )
				{
					_errProvider.SetError( e.ErrControl, string.Empty );
				}
			}

			//determine visibility for entire control
			_errCount = 0;
			if( _autoHideEnabled && _autoHide )
			{
				this.Visible = false;
			}
		}


		public void SetError( IValidationControl control, string message )
		{
			//add the control to the collection if necessary, this ensures
			//errlabels are only created once
			if( !_controls.ContainsKey( control.UniqueName ) )
			{
				ErrLabelShow errLabel = new ErrLabelShow();
				errLabel.ErrControl = (Control)control;
				errLabel.VSParent = this;

				_controls[control.UniqueName] = errLabel;

				//pnlErrors.Controls.Add( errLabel.ErrLabelGo );
				pnlErrors.Controls.Add( errLabel );
			}


			//set the error message. the errlabel determines it's own visible state
			((ErrLabelShow)_controls[control.UniqueName]).Text = message;


			//redraw the control
			_sortedControls = new SortedList();
			_errCount = 0;
			int t = 0;
			string sortkey = string.Empty;
			IEnumerator ctrls = _controls.Keys.GetEnumerator();
			while( ctrls.MoveNext() ) //reorders the controls by TabIndex-UniqueName. this is a bit of a hack.
			{
				ErrLabelShow errLabelShow = (ErrLabelShow)_controls[ctrls.Current];
				sortkey = errLabelShow.ErrControl.TabIndex.ToString( "X" ).PadLeft( 4, '0' );
				sortkey = string.Format( "{0}-{1}", sortkey, ctrls.Current.ToString() );
				_sortedControls.Add( sortkey, errLabelShow );
			}
			ctrls = _sortedControls.Keys.GetEnumerator();
			while( ctrls.MoveNext() )
			{
				//ErrLabelShow e = (ErrLabelShow)ctrls.Current;
				ErrLabelShow e = (ErrLabelShow)_sortedControls[ctrls.Current];
				if( e.HasError )
				{
					e.Top = t;
					t = e.Top + e.Height;
					_errCount++;
				}
			}



			//determine visibility for entire control
			if( _autoHideEnabled && _autoHide )
			{
				this.Visible = _errCount > 0 ? true : false;
			}
		}


		internal int MaxMessageWidth
		{
			get
			{
				return pnlErrors.Width - _rightSpacer;
			}
		}


		private void lblAutohide_Click(object sender, System.EventArgs e)
		{
			_autoHide = !_autoHide;

			lblAutoHide.ImageIndex = _autoHide ? 0 : 1;

			if( _autoHide )
			{
				this.Visible = _errCount > 0 ? true : false;
			}
		}



		private bool SetCurrentControlCvStatus(bool performSearch, bool causesValidation)
		{
			bool cv = true;

			if( performSearch )
			{
				if( _currentControl == null || !_currentControl.Focused )
				{
					string cc0 = _currentControl == null ? "null" : _currentControl.Name;
					System.Diagnostics.Debug.WriteLine( string.Format( "Doing recursion: cc={0}: {1} ", cc0, DateTime.Now ) );
					_currentControl = FindCurrentControl();
				}
			}

			if( _currentControl != null )
			{
				cv = _currentControl.CausesValidation;
				_currentControl.CausesValidation = causesValidation;
			}

			return cv;
		}


		internal Control CurrentControl
		{
			get
			{
				return _currentControl;
			}
			set
			{
				_currentControl = value;
			}
		}


		private Control FindCurrentControl()
		{
			return RecurseFindControl( this.FindForm().Controls );
		}


		private Control RecurseFindControl(Control.ControlCollection controls)
		{
			Control ctl = null;

			foreach( Control control in controls )
			{
				if( control.Focused )
				{
					ctl = control;
					break;
				}
				else
				{
					ctl = RecurseFindControl( control.Controls );
					if( ctl != null )
					{
						break;
					}
				}
			}

			return ctl;
		}


		#endregion



		#region labels make pretty stuff

		#region <notes>
		/// <notes>
		/// I'm using Label controls instead of LinkLabel controls b/c .NET doesn't
		/// suppress a Label's click event during a validation error, which allows
		/// the "Show" label to still flash the offending control. The Mousemove
		/// and MouseLeave events simulate the LinkLabel effect.
		/// </notes>
		#endregion

		private abstract class ErrLabelBase : Label
		{
			protected Font fontHover = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			protected Font fontLeave = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));

			protected Control errControl = null;
			protected ValidationSummary vsParent = null;


			#region public accessors

			public ErrLabelBase() : base()
			{
				this.Cursor = Cursors.Hand;
			}


			public virtual Control ErrControl
			{
				get
				{
					return errControl;
				}
				set
				{
					errControl = value;
				}
			}


			public virtual ValidationSummary VSParent
			{
				get
				{
					return vsParent;
				}
				set
				{
					vsParent = value;

					this.ForeColor = value._linkColor;
				}
			}


			public System.Drawing.Color LinkColor
			{
				get
				{
					return vsParent.LinkColor;
				}
			}


			public System.Drawing.Color ActiveLinkColor
			{
				get
				{
					return vsParent.ActiveLinkColor;
				}
			}


			public System.Drawing.Color FlashColor
			{
				get
				{
					return vsParent.FlashColor;
				}
			}


			#endregion


			#region event handlers

			protected override void OnMouseMove(MouseEventArgs e)
			{
				this.Font = fontHover;

				base.OnMouseMove (e);
			}


			protected override void OnMouseLeave(EventArgs e)
			{
				this.Font = fontLeave;

				base.OnMouseLeave (e);
			}


			#endregion
		}


		private class ErrLabelShow : ErrLabelBase
		{
			private bool _hasError = false;
			private ErrLabelPrev _errPrev = new ErrLabelPrev();
			private ErrLabelGo _errGo = new ErrLabelGo();
			private ErrLabelNext _errNext = new ErrLabelNext();


			#region public accessors

			public ErrLabelShow() : base()
			{
				this.AutoSize = true;

				ToolTip tt = new ToolTip();
				tt.SetToolTip( this, "Show control" );
			}


			public ErrLabelPrev ErrLabelPrev
			{
				get
				{
					return _errPrev;
				}
			}


			public ErrLabelGo ErrLabelGo
			{
				get
				{
					return _errGo;
				}
			}


			public ErrLabelNext ErrLabelNext
			{
				get
				{
					return _errNext;
				}
			}


			public override Control ErrControl
			{
				get
				{
					return base.ErrControl;
				}
				set
				{
					base.ErrControl = value;

					_errPrev.ErrControl = value;
					_errGo.ErrControl = value;
					_errNext.ErrControl = value;
				}
			}


			public override ValidationSummary VSParent
			{
				get
				{
					return base.VSParent;
				}
				set
				{
					base.VSParent = value;

					_errPrev.VSParent = value;
					_errGo.VSParent = value;
					_errNext.VSParent = value;
				}
			}


			public override string Text
			{
				get
				{
					return base.Text;
				}
				set
				{
					this.AutoSize = true;

					base.Text = value;

					if( value != null && value != "" )
					{
						if( this.Width > vsParent.MaxMessageWidth )
						{
							int m = this.Width / vsParent.MaxMessageWidth;
							if( (this.Width % vsParent.MaxMessageWidth) != 0 ) m += 1;
							this.AutoSize = false;
							this.Height = 16 * m;
							this.Width = vsParent.MaxMessageWidth;
						}

						this.Visible = true;
						_errPrev.Visible = true;
						_errGo.Visible = true;
						_errNext.Visible = true;
						_hasError = true;
					}
					else
					{
						this.Visible = false;
						_errPrev.Visible = false;
						_errGo.Visible = false;
						_errNext.Visible = false;
						_hasError = false;
					}
				}
			}


			public bool HasError
			{
				get
				{
					return _hasError;
				}
			}


			#endregion


			#region event handlers

			protected override void InitLayout()
			{
				base.InitLayout ();

				this.Left = 30;

				base.Parent.Controls.Add( _errPrev );
				base.Parent.Controls.Add( _errGo );
				base.Parent.Controls.Add( _errNext );
			}


			protected override void OnClick(EventArgs e)
			{
				this.ForeColor = base.ActiveLinkColor;
				this.Refresh();

				System.Drawing.Color bc = this.ErrControl.BackColor;

				this.errControl.BackColor = base.FlashColor;
				this.errControl.Refresh();
				System.Threading.Thread.Sleep( 125 );

				this.errControl.BackColor = bc;
				this.errControl.Refresh();
				System.Threading.Thread.Sleep( 75 );

				this.errControl.BackColor = base.FlashColor;
				this.errControl.Refresh();
				System.Threading.Thread.Sleep( 125 );

				this.errControl.BackColor = bc;
				this.errControl.Refresh();

				this.ForeColor = base.LinkColor;
				this.Refresh();

				this.Parent.Focus();

				base.OnClick (e);
			}


			protected override void OnLocationChanged(EventArgs e)
			{
				_errPrev.Top = this.Top;
				_errGo.Top = this.Top;
				_errNext.Top = this.Top;

				base.OnLocationChanged (e);
			}


			#endregion
		}


		private class ErrLabelGo : ErrLabelBase
		{
			protected ToolTip tooltip = new ToolTip();

			public ErrLabelGo() : base()
			{
				this.Font = new System.Drawing.Font("Marlett", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				this.fontHover = new System.Drawing.Font("Marlett", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				this.fontLeave = new System.Drawing.Font("Marlett", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				this.TextAlign = ContentAlignment.MiddleLeft;

				this.Height = 16;
				this.Width = 15;
				this.Text = "i";

				tooltip.SetToolTip( this, "Go to control" );
			}


			protected override void InitLayout()
			{
				base.InitLayout ();
				this.Left = 5;
			}


			protected override void OnClick(EventArgs e)
			{
				this.ForeColor = base.ActiveLinkColor;
				this.Refresh();

				bool currCvStatus = this.VSParent.SetCurrentControlCvStatus( true, false );
				this.errControl.Focus();
				this.VSParent.SetCurrentControlCvStatus( false, currCvStatus );
				this.VSParent.CurrentControl = this.errControl;

				System.Threading.Thread.Sleep( 75 );

				this.ForeColor = base.LinkColor;
				this.Refresh();

				base.OnClick (e);
			}
		}


		private class ErrLabelPrev : ErrLabelGo
		{
			public ErrLabelPrev() : base()
			{
				this.Text = "3";
				tooltip.SetToolTip( this, "Previous control" );
			}


			protected override void InitLayout()
			{
				base.InitLayout ();
				this.Left = -3;
			}


			protected override void OnClick(EventArgs e)
			{
				base.OnClick (e);

				this.errControl.CausesValidation = false;
				this.errControl.Parent.SelectNextControl( this.errControl, false, true, true, true );
				this.errControl.CausesValidation = true;
			}
		}


		private class ErrLabelNext : ErrLabelGo
		{
			public ErrLabelNext() : base()
			{
				this.Text = "4";
				tooltip.SetToolTip( this, "Next control" );
			}


			protected override void InitLayout()
			{
				base.InitLayout ();
				this.Left = 15;
			}


			protected override void OnClick(EventArgs e)
			{
				base.OnClick (e);

				this.errControl.CausesValidation = false;
				this.errControl.Parent.SelectNextControl( this.errControl, true, true, true, true );
				this.errControl.CausesValidation = true;
			}
		}


		#endregion


	}
}