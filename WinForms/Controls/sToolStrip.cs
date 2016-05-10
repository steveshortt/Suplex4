using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;
using Suplex.Security.Standard;

namespace Suplex.WinForms
{
	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	[ToolboxItem( true ), ToolboxBitmap( typeof( ToolStrip ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.CheckStateChanged )]
	public class sToolStrip : System.Windows.Forms.ToolStrip, ISecureContainer
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;


		public sToolStrip() : base()
		{
			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
		}

		protected override void InitLayout()
		{
			_sa.EnsureDefaultState();

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

		[Browsable( false ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Suplex.Data.DataAccessLayer DataAccessLayer
		{
			get { return _dal; }
			set { _dal = value; }
		}


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

		public virtual IEnumerable GetChildren()
		{
			return (ICollection)this.Items;
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

		/*
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if( _OperateRight )
			{
				base.OnMouseDown( e );
			}
		}
		*/
		#endregion
	}//class
}	//namespace