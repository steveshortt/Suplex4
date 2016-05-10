using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;

using Suplex.Forms;
using Suplex.Security;
using Suplex.Security.Standard;

namespace Suplex.WinForms
{
	/// <summary>
	/// Provides extended validation properties and auto-validation.
	/// </summary>
	[ToolboxItem( true ), ToolboxBitmap( typeof( Suplex.WinForms.sHiddenValue ), "Resources.mHiddenValue.gif" )]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.TextChanged | ControlEvents.ValueChanged )]
	public class sHiddenValue : sTextBox
	{
		private object	_value	= null;


		public sHiddenValue() : base()
		{
			this.TabStop = false;
			base.Visible = this.DesignMode;
			base.BackColor = Color.PowderBlue;
		}


		[Browsable(false)]
		public object Value
		{
			get { return _value; }
			set
			{
				if ( this.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed )
				{
					if( _value != value )
					{
						_value = value;
						this.OnValueChanged();
					}
				}
			}
		}

		private void OnValueChanged()
		{
			this.Security.AuditAction( AuditType.ControlDetail, null, "ValueChanged.", false );

			this.Validation.ProcessEvent( this.Value.ToString(), ControlEvents.ValueChanged, true );
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.Visible = this.DesignMode;
			//base.OnVisibleChanged (e);
		}

		[Browsable(false)]
		[DefaultValue(typeof(System.Drawing.Color), "PowderBlue")]
		public override Color BackColor
		{
			get { return base.BackColor; }
			set { base.BackColor = Color.PowderBlue; }
		}
	}
}