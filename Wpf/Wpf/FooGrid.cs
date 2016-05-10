using System;
using System.Collections;
using System.Drawing;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;

namespace Suplex.Wpf
{
	[ToolboxItem( true ), ToolboxBitmap( typeof( Grid ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.None )]
	public class FooGrid : Grid, ISecureContainer, ILogicalChildrenHost
	{
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;

		public FooGrid()
		{
			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;

			this.DataAccessLayer = new DataAccessLayer();

			this.VisibilityDenied = Visibility.Collapsed;
		}

		public override void BeginInit()
		{
			_sa.EnsureDefaultState();
			base.BeginInit();
		}

		public void ApplySecurity()
		{
			base.IsEnabled = _sr[AceType.UI, UIRight.Enabled].AccessAllowed;
			base.Visibility = _sr[AceType.UI, UIRight.Visible].AccessAllowed.ToVisibility( this.VisibilityDenied );
		}

		public Visibility VisibilityDenied { get; set; }

		public DataAccessLayer DataAccessLayer { get; set; }

		public string GetSecurityState()
		{
			return string.Format( "Visibility: {0}, IsEnabled: {1}", this.Visibility, this.IsEnabled );
		}

		public SecurityAccessor Security
		{
			get { return _sa; }
		}

		public string UniqueName { get { return this.Name; } set { this.Name = value; } }

		public IEnumerable GetChildren()
		{
			return new WpfLogicalChildrenEnumeratorWrapper( this );
		}

		public IEnumerator LogicalChildrenEnumerator { get { return this.LogicalChildren; } }

		[DefaultValue( Visibility.Visible )]
		new public Visibility Visibility
		{
			get
			{
				return base.Visibility;
			}
			set
			{
				if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					base.Visibility = value;
				}
			}
		}

		[DefaultValue( true )]
		new public bool IsEnabled
		{
			get
			{
				return base.IsEnabled;
			}
			set
			{
				if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					base.IsEnabled = value;
				}
			}
		}
	}
}