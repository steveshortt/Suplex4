using System;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Collections;
using System.Web.UI;

using Suplex.Forms;
using Suplex.Data;
using Suplex.Security;
using Suplex.Security.Standard;


using System.Diagnostics;
using System.Web.UI.Design;



namespace Suplex.WebForms
{
	/// <summary>
	/// Provides secuirty, extended validation properties, and auto-validation.
	/// </summary>
	[ToolboxItem( true ), System.Drawing.ToolboxBitmap( typeof( sSynchronizationManager ), "Resources.mSynchronizationManager.gif" )]
	[EventBindings( EventBindingsAttribute.BaseEvents.None,
		ControlEvents.Upload | ControlEvents.Download |
		ControlEvents.Initialize | ControlEvents.Validating | ControlEvents.TextChanged | ControlEvents.ValueChanged )]
	[Designer( typeof( sSynchronizationManagerDesigner ) )]
	public class sSynchronizationManager : System.Web.UI.Control, IValidationControl, ISynchronizationManager, ISecurityExtender, IWebExtra
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;
		private ValidationAccessor _va = null;

		private string _tag = null;
		private object _tagObject = null;


		private SynchronizationMode _synchronizationMode = SynchronizationMode.None;
		private string _text = string.Empty;
		private object _value = null;


		#region ISynchronizationManager Events
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseSynchronizationEventHandler Upload;
		[Category( "Suplex" )]
		public event Suplex.Forms.RaiseSynchronizationEventHandler Download;

		[Category( "Suplex" )]
		public event Suplex.Forms.SynchronizationEventHandler Downloaded;
		[Category( "Suplex" )]
		public event Suplex.Forms.SynchronizationEventHandler Uploaded;

		protected virtual void OnUpload(RaiseSynchronizationEventArgs e)
		{
			if( Upload != null )
			{
				Upload( this, e );
			}
		}
		protected virtual void OnDownload(RaiseSynchronizationEventArgs e)
		{
			if( Download != null )
			{
				Download( this, e );
			}
		}
		protected virtual void OnUploaded(SynchronizationEventArgs e)
		{
			if( Uploaded != null )
			{
				Uploaded( this, e );
			}
		}
		protected virtual void OnDownloaded(SynchronizationEventArgs e)
		{
			if( Downloaded != null )
			{
				Downloaded( this, e );
			}
		}
		#endregion


		public sSynchronizationManager()
		{
			_sa = new SecurityAccessor( this, AceType.Synchronization );
			_sr = _sa.Descriptor.SecurityResults;
			_va = new ValidationAccessor( this, TypeCode.Empty );
		}

		protected override void OnInit(EventArgs e)
		{
			_sa.EnsureDefaultState();

			base.OnInit( e );
		}

		protected override void OnPreRender(EventArgs e)
		{
			this.ApplySecurity();

			base.OnPreRender( e );
		}


		#region ISynchronizationManager Members

		public void RaiseUpload(params object[] parameters)	//bool process, 
		{
			//if( process )
			//{
			//    if( _sr[AceType.Synchronization, SynchronizationRight.Upload].AccessAllowed )
			//    {
			//        _synchronizationMode = SynchronizationMode.Upload;
			//        SynchronizationEventArgs s = ProcessEvent( ControlEvents.Upload );

			//        string msg = "Upload completed successfully.";
			//        if( r.Exception != null ) msg = "ListRecords completed with errors.";
			//        _sa.AuditAction( AuditType.ControlDetail, null, this.UniqueName, msg );

			//        OnUploaded( s );
			//    }
			//    else
			//    {
			//        _sa.AuditAction( AuditType.ControlDetail, null, this.UniqueName,
			//            "ListRecords failed: access denied." );

			//        OnUploaded( SynchronizationEventArgs.Empty );
			//    }
			//}
			//else
			//{
			_sa.AuditAction( AuditType.ControlDetail, null, "Upload deferred to caller.", false );

			OnUpload( new RaiseSynchronizationEventArgs( parameters, _sr[AceType.Synchronization, SynchronizationRight.Upload].AccessAllowed ) );
			//}
		}

		public void RaiseDownload(params object[] parameters)	//bool process, 
		{
			//if( process )
			//{
			//    if ( _sr[AceType.Record, RecordRights.Select].AccessAllowed )
			//    {
			//        _synchronizationMode = SynchronizationMode.Select;
			//        RecordEventArgs r = ProcessEvent( ControlEvents.SelectRecord );

			//        string msg = "SelectRecord completed successfully.";
			//        if( r.Exception != null ) msg = "SelectRecord completed with errors.";
			//        _sa.AuditAction( AuditType.ControlDetail, null, this.UniqueName, msg );

			//        OnSelectedRecord( r );
			//    }
			//    else
			//    {
			//        _sa.AuditAction( AuditType.ControlDetail, null, this.UniqueName,
			//            "SelectRecord failed: access denied." );

			//        OnSelectedRecord( RecordEventArgs.Empty );
			//    }
			//}
			//else
			//{
			_sa.AuditAction( AuditType.ControlDetail, null, "Download deferred to caller.", false );

			OnDownload( new RaiseSynchronizationEventArgs( parameters, _sr[AceType.Synchronization, SynchronizationRight.Download].AccessAllowed ) );
			//}
		}

		[DefaultValue( SynchronizationMode.None ), Category( "Suplex" )]
		public Suplex.Forms.SynchronizationMode SynchronizationMode
		{
			get { return _synchronizationMode; }
			set { _synchronizationMode = value; }
		}

		[Browsable( true )]
		public string Text
		{
			get { return _text; }
			set
			{
				if( _text != value )
				{
					_text = value;
					OnTextChanged( EventArgs.Empty );
				}
			}
		}

		[Browsable( true )]
		public string ImageUrl
		{
			get { return _text; }
			set
			{
				_text = value;
			}
		}

		[Browsable( false )]
		public object Value
		{
			get { return _value; }
			set
			{
				if( _value != value )
				{
					_value = value;
					OnValueChanged();
				}
			}
		}
		#endregion


		[ParenthesizePropertyName( true ), Category( "Suplex" )]
		public string UniqueName
		{
			get
			{
				return string.IsNullOrEmpty( _uniqueName ) ? base.UniqueID : _uniqueName;
			}
			set
			{
				_uniqueName = value;
			}
		}

		[Browsable( false ), Category( "Suplex" )]
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

		public ValidationResult ProcessValidate(bool processFillMaps)
		{
			return _va.ProcessEvent( null, ControlEvents.Validating, processFillMaps );
		}

		/// <summary>
		/// Resets flags indicating control should be validated and FillMaps should processed.
		/// </summary>
		protected void OnTextChanged(System.EventArgs e)
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "TextChanged.", false );

			_va.ProcessEvent( null, ControlEvents.TextChanged, true );
		}

		private void OnValueChanged()
		{
			_sa.AuditAction( AuditType.ControlDetail, null, "ValueChanged.", false );

			_va.ProcessEvent( null, ControlEvents.ValueChanged, true );
		}
		#endregion


		#region Security Implementation
		[TypeConverter( typeof( ExpandableObjectConverter ) ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
		Description( "Provides access to Security management properties and tools." )]
		public SecurityAccessor Security
		{
			get { return _sa; }
		}

		public void ApplySecurity()
		{
			if( !this.DesignMode )
			{
				base.Visible = false;
			}
		}

		//just a few shortcut properties...
		[Browsable( false )]
		public bool HasOneWayRight { get { return _sr[AceType.Synchronization, SynchronizationRight.OneWay].AccessAllowed; } }
		[Browsable( false )]
		public bool HasUploadRight { get { return _sr[AceType.Synchronization, SynchronizationRight.Upload].AccessAllowed; } }
		[Browsable( false )]
		public bool HasDownloadRight { get { return _sr[AceType.Synchronization, SynchronizationRight.Download].AccessAllowed; } }
		[Browsable( false )]
		public bool HasTwoWayRight { get { return _sr[AceType.Synchronization, SynchronizationRight.TwoWay].AccessAllowed; } }
		#endregion


		#region IWebExtra Members
		public string Tag
		{
			get { return _tag; }
			set { _tag = value; }
		}

		[Browsable( false )]
		public object TagObj
		{
			get { return _tagObject; }
			set { _tagObject = value; }
		}
		#endregion
	}//class

	public class sSynchronizationManagerDesigner : System.Web.UI.Design.ControlDesigner
	{
		public override void Initialize(System.ComponentModel.IComponent component)
		{
			base.Initialize( component );
			SetViewFlags( ViewFlags.CustomPaint, true );
		}

		public override string GetDesignTimeHtml()
		{
			//return string.Format( "<span style=\"background-color:#ccffcc; font-family:Verdana, Arial; font-size:9pt;\">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[{0}]&nbsp;</span>", base.ID );
			return string.Format( "<table border=\"0px\" cellpadding=\"0\" cellspacing=\"0\"><tr><td height=\"20px\">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[{0}]&nbsp;</td></tr></table>", base.ID );
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			e.Graphics.DrawImage( Properties.Resources.SyncManagerIcon.ToBitmap(), 3, 2 );
			base.OnPaint( e );
		}
	}


}//namespace