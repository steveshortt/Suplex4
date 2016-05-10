using System;
using System.ComponentModel;
using System.Windows;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;


namespace Suplex.Wpf
{
	[ToolboxItem( false )]
	[EventBindings( EventBindingsAttribute.BaseEvents.None, ControlEvents.None, false )]
	public class SplxSecureManagerBase : System.Windows.Controls.Control, IValidationControl, ISecurityExtender
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();

		/* NOTE: protected members */
		protected SecurityAccessor _sa = null;
		protected SecurityResultCollection _sr = null;
		protected ValidationAccessor _va = null;
		/* NOTE: protected members */

		private object _value = null;


		public SplxSecureManagerBase() : base() { }

		public override void BeginInit()
		{
			_sa.EnsureDefaultState();
			this.Visibility = Visibility.Collapsed;
			base.BeginInit();
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


		#region Security Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Security management properties and tools." ), Category( "Suplex" )]
		public SecurityAccessor Security
		{
			get { return _sa; }
		}

		public virtual void ApplySecurity()
		{
		}

		public string GetSecurityState()
		{
			return string.Format( "Visibility: {0}, IsEnabled: {1}", this.Visibility, this.IsEnabled );
		}
		#endregion


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
			return _va.ProcessEvent( null, ControlEvents.Validating, processFillMaps );
		}

		[Browsable( false ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public object Value
		{
			get { return _value; }
			set
			{
				if( _value != value )
				{
					_value = value;
					this.OnValueChanged();
				}
			}
		}

		protected virtual void OnValueChanged()
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "ValueChanged.", false );

			_va.ProcessEvent( this.Value.ToString(), ControlEvents.ValueChanged, true );
		}
		#endregion
	}
}