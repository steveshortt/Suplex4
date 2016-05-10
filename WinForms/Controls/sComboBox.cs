using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Specialized;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;
using Suplex.Security.Standard;

namespace Suplex.WinForms
{
	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	[ToolboxItem( true ), ToolboxBitmap( typeof( ComboBox ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.SelectedIndexChanged | ControlEvents.TextChanged )]
	public class sComboBox : System.Windows.Forms.ComboBox, IValidationControl
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationAccessor _va = null;


		private int _lastSelectedIndex = -1;
		private string _lastText = "";


		//typeahead support
		private System.Windows.Forms.ComboBox _shortcuts = new System.Windows.Forms.ComboBox();
		private string _shortcutMember = string.Empty;
		private bool _useTypeAhead = true;
		private bool _displayActiveList = false;
		private bool _restrictListItems = true;


		#region Events
		public event System.EventHandler ShortcutMemberChanged;

		protected void OnShortcutMemberChanged(object sender, System.EventArgs e)
		{
			if( ShortcutMemberChanged != null )
			{
				ShortcutMemberChanged( sender, e );
			}
		}
		#endregion


		public sComboBox() : base()
		{
			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationAccessor( this, TypeCode.String );
		}

		protected override void InitLayout()
		{
			_sa.EnsureDefaultState();

			_shortcuts.Top = 0;
			_shortcuts.Left = 0;
			_shortcuts.Height = 0;
			_shortcuts.Width = 0;
			_shortcuts.Visible = false;
			this.Parent.Controls.Add( _shortcuts );

			base.InitLayout ();
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
				vr = _va.ProcessEvent( this.GetCompareValue(), ControlEvents.Validating, processFillMaps );
			}
			return vr;
		}

		protected override void OnValidating(CancelEventArgs e)
		{
			if( _restrictListItems )
			{
				e.Cancel = !this.ItemIsInList;
			}

			if( !e.Cancel )
			{
				e.Cancel = !this.ProcessValidate( true ).Success;
			}

			base.OnValidating( e );
		}

		protected override void OnEnter(EventArgs e)
		{
			_va.ProcessEvent( this.GetCompareValue(), ControlEvents.Enter, true );
			base.OnEnter( e );
		}

		protected override void OnLeave(EventArgs e)
		{
			_va.ProcessEvent( this.GetCompareValue(), ControlEvents.Leave, true );
			base.OnLeave( e );
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
			{
				_va.ProcessEvent( this.GetCompareValue(), ControlEvents.EnabledChanged, true );
				//_sa.AuditAccess( this, AceType.UI, UIRight.Enabled, _auditEventHandler );

				base.OnEnabledChanged( e );
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
			{
				_va.ProcessEvent( this.GetCompareValue(), ControlEvents.VisibleChanged, true );
				//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Visible, _auditEventHandler );

				base.OnVisibleChanged( e );
			}
		}

		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "SelectedIndexChanged.", false );

			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				if( this.SelectedIndex != _lastSelectedIndex )
				{
					this.SetSelectionLengthToZero();
					_lastSelectedIndex = this.SelectedIndex;

					_va.ProcessEvent( this.GetCompareValue(), ControlEvents.SelectedIndexChanged, true );
				}

				base.OnSelectedIndexChanged( e );
			}
			else
			{
				this.SetSelectionLengthToZero();
				this.SelectedIndex = _lastSelectedIndex;
			}
		}

		protected override void OnTextChanged(System.EventArgs e)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "TextChanged.", false );

			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				_lastText = this.Text;

				_va.ProcessEvent( this.Text, ControlEvents.TextChanged, true );

				base.OnTextChanged( e );
			}
			else
			{
				//this.Text = _lastText;
			}
		}

		private string GetCompareValue()
		{
			return this.SelectedValue != null ? this.SelectedValue.ToString() : this.Text;
		}
		#endregion


		#region TypeAhead Overrides
		/// <summary>
		/// Resets flag indicating previous selected index.
		/// </summary>
		protected override void OnDisplayMemberChanged(EventArgs e)
		{
			_lastSelectedIndex = -1;
			this.SetSelectionLengthToZero();
			SetShortcuts();
			base.OnDisplayMemberChanged (e);
		}

		/// <summary>
		/// Resets flag indicating previous selected index.
		/// </summary>
		protected override void OnValueMemberChanged(EventArgs e)
		{
			_lastSelectedIndex = -1;
			this.SetSelectionLengthToZero();
			SetShortcuts();
			base.OnValueMemberChanged (e);
		}

		/// <summary>
		/// Resets flag indicating previous selected index.
		/// </summary>
		protected override void OnDataSourceChanged(EventArgs e)
		{
			_lastSelectedIndex = -1;
			this.SetSelectionLengthToZero();
			base.OnDataSourceChanged( e );
			SetShortcuts();
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if( !Char.IsControl(e.KeyChar) )
			{
				this.DroppedDown = _displayActiveList;
				string searchText = this.Text.Substring( 0, this.SelectionStart ) + e.KeyChar;
				int i = this.FindString( searchText );

				if( i > -1 )
				{
					e.Handled = true;
					this.SelectedIndex = i;
					this.SelectionStart = searchText.Length;
					this.SelectionLength = this.Text.Length - this.SelectionStart;

				}
				else
				{
					//NOTE: _shortcuts.DataSource == _shortcuts.DataSource, and .NET
					//will automatically keep the SelectedIndex of the two combos in sync,
					//but i found that it didn't always work, so i'm setting both SelectedIndexes
					//just to be sure
					i = _shortcuts.FindStringExact( searchText );
					if( i > -1 )
					{
						e.Handled = true;
						this.SelectedIndex = _shortcuts.SelectedIndex = i;
						Parent.SelectNextControl( this, true, true, true, true );
					}
				}
			}

			base.OnKeyPress( e );
		}

		private void SetSelectionLengthToZero()
		{
			if( this.SelectionLength > 0 )
			{
				this.SelectionLength = 0;
			}
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

		protected override void OnClick(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				base.OnClick( e );
			}
		}


		protected override void OnDropDown(EventArgs e)
		{
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				base.OnDropDown( e );
			}
		}
		#endregion


		#region TypeAhead Properties
		[System.ComponentModel.DefaultValue( true )]
		public bool UseTypeAhead
		{
			get { return _useTypeAhead; }
			set { _useTypeAhead = value; }
		}

		[System.ComponentModel.DefaultValue( false )]
		public bool DisplayActiveList
		{
			get { return _displayActiveList; }
			set { _displayActiveList = value; }
		}

		[System.ComponentModel.DefaultValue( true )]
		public bool RestrictItemsToList
		{
			get { return _restrictListItems; }
			set { _restrictListItems = value; }
		}

		//public stringdictionary itemshortcuts
		//{
		//    get { return _shortcuts; }
		//    set { _shortcuts = value; }
		//}

		public string ShortcutMember
		{
			get { return _shortcutMember; }
			set
			{
				if( _shortcutMember != value )
				{
					_shortcutMember = value;
					this.OnShortcutMemberChanged( this, System.EventArgs.Empty );
					SetShortcuts();
				}
			}
		}

		[System.ComponentModel.Browsable( false )]
		public bool ItemIsInList
		{
			get { return this.FindString( this.Text ) > -1; }
		}

		private void SetShortcuts()
		{
			if( this.DisplayMember != null && this.DisplayMember != string.Empty &&
				this.ValueMember != null && this.ValueMember != string.Empty &&
				_shortcutMember != null && _shortcutMember != string.Empty &&
				this.DataSource != null )
			{
				//_shortcuts.ValueMember could be this.ValueMember also, doesn't matter which,
				//because the shortcut search uses DisplayMember
				_shortcuts.DisplayMember = _shortcutMember;
				_shortcuts.ValueMember = _shortcutMember;
				_shortcuts.DataSource = this.DataSource;
			}
		}
		#endregion
	}	//class
}	//namespace