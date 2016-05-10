using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.ComponentModel;
using System.Windows;
using System.Collections;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;
using Suplex.Security.Standard;


namespace Suplex.Forms.SecureManager
{
	public abstract class SplxSecureManagerBase : IValidationControl, ISecurityExtender
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();

		/* NOTE: protected members */
		protected SecurityAccessor _sa = null;
		protected SecurityResultCollection _sr = null;
		protected ValidationAccessor _va = null;
		/* NOTE: protected members */

		private object _value = null;


		public SplxSecureManagerBase() { }


		public string UniqueName { get; set; }

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

		public virtual void ApplySecurity() { }

		public abstract string GetSecurityState();
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