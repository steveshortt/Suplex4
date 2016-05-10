using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;
using Suplex.Security.Standard;

namespace Suplex.WinForms
{

	public delegate void sTreeViewEventHandler(object sender, sTreeViewEventArgs e);
	public delegate void sTreeViewCancelEventHandler(object sender, sTreeViewCancelEventArgs e);
	public delegate void sTreeViewDragEventHandler(object sender, sTreeViewDragEventArgs e);
	public delegate void sTreeViewMouseEventHandler(object sender, sTreeViewMouseEventArgs e);


	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(TreeView))]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.CheckStateChanged )]
	public class sTreeView : System.Windows.Forms.TreeView, ISecureContainer, ISecurityStateHost
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;

		private sTreeNodeCollection			_nodes					= null;


		#region Events
		new public event sTreeViewEventHandler AfterCheck;
		new public event sTreeViewEventHandler AfterCollapse;
		new public event sTreeViewEventHandler AfterExpand;
		new public event sTreeViewEventHandler AfterSelect;

		/// <summary>
		/// BeforeCheck requires UIAce.Enabled and UIAce.Operate rights.
		/// </summary>
		new public event sTreeViewMouseEventHandler MouseDown;
		/// <summary>
		/// BeforeCheck requires UIAce.Enabled and UIAce.Operate rights.
		/// </summary>
		new public event sTreeViewCancelEventHandler BeforeCheck;
		/// <summary>
		/// BeforeCollapse requires UIAce.Operate rights.
		/// </summary>
		new public event sTreeViewCancelEventHandler BeforeCollapse;
		/// <summary>
		/// BeforeExpand requires UIAce.Operate rights.
		/// </summary>
		new public event sTreeViewCancelEventHandler BeforeExpand;		
		/// <summary>
		/// BeforeSelect requires UIAce.Enabled and UIAce.Operate rights.
		/// </summary>
		new public event sTreeViewCancelEventHandler BeforeSelect;
		/// <summary>
		/// DragDrop requires UIAce.Operate rights.
		/// </summary>
		new public event sTreeViewDragEventHandler DragDrop;
		/// <summary>
		/// DragOver requires UIAce.Operate rights.
		/// </summary>
		new public event sTreeViewDragEventHandler DragOver;


		private void onAfterCheck(object sender, sTreeViewEventArgs e)
		{
			if( AfterCheck != null )
			{
				AfterSelect( sender, e );
			}
		}

		private void onAfterCollapse(object sender, sTreeViewEventArgs e)
		{
			if( AfterCollapse != null )
			{
				AfterCollapse( sender, e );
			}
		}

		private void onAfterExpand(object sender, sTreeViewEventArgs e)
		{
			if( AfterExpand != null )
			{
				AfterExpand( sender, e );
			}
		}

		private void onAfterSelect(object sender, sTreeViewEventArgs e)
		{
			if( AfterSelect != null )
			{
				AfterSelect( sender, e );
			}
		}

		private void onMouseDown(object sender, sTreeViewMouseEventArgs e)
		{
			if( MouseDown != null )
			{
				MouseDown( sender, e );
			}
		}

		private void onBeforeCheck(object sender, sTreeViewCancelEventArgs e)
		{
			if( BeforeCheck != null )
			{
				BeforeCheck( sender, e );
			}
		}

		private void onBeforeCollapse(object sender, sTreeViewCancelEventArgs e)
		{
			if( BeforeCollapse != null )
			{
				BeforeCollapse( sender, e );
			}
		}

		private void onBeforeExpand(object sender, sTreeViewCancelEventArgs e)
		{
			if( BeforeExpand != null )
			{
				BeforeExpand( sender, e );
			}
		}

		private void onBeforeSelect(object sender, sTreeViewCancelEventArgs e)
		{
			if( BeforeSelect != null )
			{
				BeforeSelect( sender, e );
			}
		}

		private void onDragDrop(object sender, sTreeViewDragEventArgs e)
		{
			if( DragDrop != null )
			{
				DragDrop( sender, e );
			}
		}

		private void onDragOver(object sender, sTreeViewDragEventArgs e)
		{
			if( DragOver != null )
			{
				DragOver( sender, e );
			}
		}


		#region Event Overrides
		/// <note>
		/// No security testing is necessary in the "On_x_After" events b/c
		/// the corresponding On_x_Before events will "cancel" the event if
		/// necessary rights are not present.
		/// </note>
		protected override void	OnAfterCheck(TreeViewEventArgs e)
		{
			sTreeViewEventArgs m = new sTreeViewEventArgs( e );
			this.onAfterCheck( this, m );
		}

		protected override void	OnAfterCollapse(TreeViewEventArgs e)
		{
			sTreeViewEventArgs m = new sTreeViewEventArgs( e );
			this.onAfterCollapse( this,	m );
		}

		protected override void	OnAfterExpand(TreeViewEventArgs	e)
		{
			sTreeViewEventArgs m = new sTreeViewEventArgs( e );
			this.onAfterExpand(	this, m	);
		}

		protected override void	OnAfterSelect(TreeViewEventArgs	e)
		{
			sTreeViewEventArgs m = new sTreeViewEventArgs( e );
			this.onAfterSelect(	this, m	);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			sTreeViewMouseEventArgs m = new sTreeViewMouseEventArgs( this, e.Button, e.Clicks, e.X, e.Y, e.Delta );
			
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				if( m.Node != null )
				{
					if( m.Node.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Enabled].AccessAllowed &&
						m.Node.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed )
					{
						this.onMouseDown( this, m );
					}
				}
				else
				{
					this.onMouseDown( this, m );
				}
			}
		}

		protected override void OnBeforeCheck(TreeViewCancelEventArgs e)
		{
			bool cancel = true;

			sTreeViewCancelEventArgs m = new sTreeViewCancelEventArgs( e );
			
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed &&
				m.Node.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Enabled].AccessAllowed &&
				m.Node.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				this.onBeforeCheck( this, m );
				cancel = m.Cancel;
			}

			e.Cancel = cancel;
		}

		protected override void OnBeforeCollapse(TreeViewCancelEventArgs e)
		{
			bool cancel = true;

			sTreeViewCancelEventArgs m = new sTreeViewCancelEventArgs( e );
			
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed &&
				m.Node.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				this.onBeforeCollapse( this, m );
				cancel = m.Cancel;
			}

			e.Cancel = cancel;
		}

		protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
		{
			bool cancel = true;

			sTreeViewCancelEventArgs m = new sTreeViewCancelEventArgs( e );
			
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed &&
				m.Node.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				this.onBeforeExpand( this, m );
				cancel = m.Cancel;
			}

			e.Cancel = cancel;
		}

		protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
		{
			bool cancel = true;

			sTreeViewCancelEventArgs m = new sTreeViewCancelEventArgs( e );
			
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed &&
				m.Node.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Enabled].AccessAllowed &&
				m.Node.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				this.onBeforeSelect( this, m );
				cancel = m.Cancel;
			}

			e.Cancel = cancel;
		}

		protected override void OnItemDrag(ItemDragEventArgs e)
		{
			sTreeNode node = (sTreeNode)e.Item;
			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed &&
				node.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				base.OnItemDrag( e );
			}
		}

		protected override void OnDragDrop(DragEventArgs e)
		{
			sTreeViewDragEventArgs m = new sTreeViewDragEventArgs( e.Data, e.KeyState, e.X, e.Y, e.AllowedEffect, e.Effect );
			m.Eval( this, typeof(sTreeNode) );

			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed &&
				m.TargetNode.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				this.onDragDrop( this, m );
			}
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			sTreeViewDragEventArgs m = new sTreeViewDragEventArgs( e.Data, e.KeyState, e.X, e.Y, e.AllowedEffect, e.Effect );
			m.Eval( this, typeof(sTreeNode) );

			if( _sr[AceType.UI, UIRight.Operate].AccessAllowed &&
				m.TargetNode.Security.Descriptor.SecurityResults[AceType.UI, UIRight.Operate].AccessAllowed )
			{
				this.onDragOver( this, m );
			}
		}
		#endregion
		#endregion


		public sTreeView() : base()
		{
			_nodes = new sTreeNodeCollection( base.Nodes, this );

			_sa = new SecurityAccessor( this, AceType.UI );
			_sr = _sa.Descriptor.SecurityResults;
		}

		protected override void InitLayout()
		{
			_sa.EnsureDefaultState();

			base.InitLayout();
		}


		protected TreeNodeCollection BaseTreeNodes
		{
			get
			{
				return base.Nodes;
			}
		}


		[ParenthesizePropertyName( true ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual string UniqueName
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

		[Browsable( false ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Suplex.Data.DataAccessLayer DataAccessLayer
		{
			get { return _dal; }
			set { _dal = value; }
		}

		// TODO: Add VSDesigner compatibility
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content ), MergableProperty( false )]
		new public virtual sTreeNodeCollection Nodes
		{
			get { return _nodes; }
		}


		protected override void OnEnabledChanged(EventArgs e)
		{
			//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Enabled, _auditEventHandler );

			_sa.AuditAction( AuditType.ControlDetail, null, "EnabledChanged.", false );

			base.OnEnabledChanged( e );
		}

//		protected override void OnVisibleChanged(EventArgs e)
//		{
//			//SecureControlUtils.AuditAccess( this, AceType.UI, UIRight.Visible, _auditEventHandler );
//			
//			base.OnVisibleChanged( e );
//		}


		internal bool IsDesignMode { get { return this.DesignMode; } }


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
			return this.Nodes;
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
		#endregion
	}

	public class sTreeNode : System.Windows.Forms.TreeNode, ISecureContainer, ISecurityStateHost
	{
		private string _uniqueName = null;
		private DataAccessLayer _dal = new DataAccessLayer();
		private SecurityAccessor _sa = null;
		private SecurityResultCollection _sr = null;

		private bool _visible = false;
		private bool _enabled = false;

		private ITreeNodeInfo				_nodeInfo				= new TreeNodeInfo();
		private sTreeNodeCollection			_nodes					= null;

		private TreeNodeCollection _lastOwner = null;
		private Color _lastEnabledColor = Color.Black;


		public sTreeNode() : base()
		{
			Initialize();
		}

		public sTreeNode(string text) : base(text)
		{
			Initialize();
		}

		protected TreeNodeCollection BaseTreeNodes
		{
			get { return base.Nodes; }
		}

		private void Initialize()
		{
			_sa = new SecurityAccessor( this, AceType.UI);
			_sr = _sa.Descriptor.SecurityResults;

			_nodes = new sTreeNodeCollection( base.Nodes, this );
		}

		[ParenthesizePropertyName( true ), Category( "Suplex" ),
		DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public virtual string UniqueName
		{
			get
			{
				//use _uniqueName, then base.Name, or default to base.Text
				return string.IsNullOrEmpty( _uniqueName ) ? string.IsNullOrEmpty( base.Name ) ? base.Text : base.Name : _uniqueName;
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

		public virtual ITreeNodeInfo NodeInfo
		{
			get { return _nodeInfo; }
			set { _nodeInfo = value; }
		}

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content ), MergableProperty( false )]
		new public virtual sTreeNodeCollection Nodes
		{
			get { return _nodes; }
		}

		public override object Clone()
		{
			object newNode = base.Clone();

			this.Security.Descriptor.CopyTo( ( (ISecureControl)newNode ).Security.Descriptor );
			( (ISecureControl)newNode ).ApplySecurity();

			//((TreeNodeInfo)this.NodeInfo).CopyTo( (TreeNodeInfo)((sTreeNode)newNode).NodeInfo );

			RecurseNodes( this, ( (sTreeNode)newNode ) );

			return newNode;
		}

		private void RecurseNodes(sTreeNode thisNode, sTreeNode newNode)
		{
			TreeNodeInfo ni = (TreeNodeInfo)newNode.NodeInfo;
			( (TreeNodeInfo)thisNode.NodeInfo ).CopyTo( ref ni );		//why ref??
			newNode.NodeInfo = ni;

			newNode.UniqueName = thisNode.UniqueName;

			for( int n = 0; n < thisNode.Nodes.Count; n++ )
			{
				RecurseNodes( thisNode.Nodes[n], newNode.Nodes[n] );
			}
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
			bool designMode = false;

			if( this.TreeView != null && this.TreeView is sTreeView )
			{
				designMode = ( (sTreeView)this.TreeView ).IsDesignMode;
			}

			if( !designMode )
			{
				if( !_sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					this.visible = false;
				}

				if( !_sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					this.enabled = false;
				}
			}
		}

		public virtual IEnumerable GetChildren()
		{
			return this.Nodes;
		}

		[DefaultValue( false )]
		public bool Visible
		{
			get { return this.visible; }
			set
			{
				if( _sr[AceType.UI, UIRight.Visible].AccessAllowed )
				{
					this.visible = value;
				}
			}
		}

		[DefaultValue( false )]
		public bool Enabled
		{
			get { return this.enabled; }
			set
			{
				if( _sr[AceType.UI, UIRight.Enabled].AccessAllowed )
				{
					this.enabled = value;
				}
			}
		}

		private bool visible
		{
			get { return _visible; }
			set
			{
				_visible = value;

				if( value )
				{
					if( _lastOwner != null )
					{
						_lastOwner.Add( this );
					}
				}
				else
				{
					if( this.Parent != null && this.Parent is TreeNode )
					{
						_lastOwner = this.Parent.Nodes;
						this.Parent.Nodes.Remove( this );
					}
					else
					{
						if( this.TreeView != null )
						{
							_lastOwner = this.TreeView.Nodes;
							this.TreeView.Nodes.Remove( this );
						}
					}
				}
			}
		}

		private bool enabled
		{
			get { return _enabled; }
			set
			{
				_enabled = value;
				if( value )
				{
					this.ForeColor = _lastEnabledColor;
				}
				else
				{
					_lastEnabledColor = this.ForeColor;
					this.ForeColor = SystemColors.GrayText;
				}
			}
		}
		#endregion
	}

	public class sTreeNode<T> : sTreeNode
	{
		private T _data;

		public sTreeNode() : base()
		{
		}

		public sTreeNode(string text) : base( text )
		{
		}

		public T NodeData
		{
			get { return _data; }
			set { _data = value; }
		}

		//public override object Clone()
		//{
		//    object newNode = base.Clone();

		//    RecurseNodes( this, ( (sTreeNode)newNode ) );

		//    return newNode;
		//}

		//private void RecurseNodes(sTreeNode<T> thisNode, sTreeNode<T> newNode)
		//{
		//    T ni = (T)newNode.NodeData;
		//    ( (T)thisNode.NodeData ).

		//    newNode.UniqueName = thisNode.UniqueName;

		//    for( int n = 0; n < thisNode.Nodes.Count; n++ )
		//    {
		//        RecurseNodes( thisNode.Nodes[n], newNode.Nodes[n] );
		//    }
		//}
	}

	public interface ITreeNodeInfo
	{
		object NodeType { get; set; }
	}

	public class TreeNodeInfo : ITreeNodeInfo
	{
		private object _nodeType = "";

		public TreeNodeInfo(){}

		public virtual object NodeType
		{
			get { return _nodeType; }
			set { _nodeType = value; }
		}

		public virtual void CopyTo(ref TreeNodeInfo treeNodeInfo)
		{
			treeNodeInfo = (TreeNodeInfo)this.MemberwiseClone();
		}
	}

	//[Editor( "System.Windows.Forms.Design.TreeNodeCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof( System.Drawing.Design.UITypeEditor ) )]
	//[Editor( typeof( sTreeNodeCollectionEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
	public class sTreeNodeCollection : IList, ICollection, IEnumerable
	{
		private TreeNodeCollection _nodes = null;
		private ISecurityStateHost _secHost = null;

		public sTreeNodeCollection(TreeNodeCollection nodes)
		{
			_nodes = nodes;
		}
		public sTreeNodeCollection(TreeNodeCollection nodes, ISecurityStateHost host)
		{
			_nodes = nodes;
			_secHost = host;
		}

		#region IList overrides
		public sTreeNode this[int index]
		{
			get
			{
				return ( (sTreeNode)_nodes[index] );
			}
			set
			{
				_nodes[index] = value;
			}
		}

		public int Add(sTreeNode node)
		{
			this.CheckSecurityState( node );
			return _nodes.Add( node );
		}
		public void AddRange(sTreeNode[] nodes)
		{
			foreach( TreeNode node in nodes )
			{
				_nodes.Add( node );
			}
		}
		public virtual sTreeNode Add(string text)
		{
			// this gives a typecasting error, but the method below works just fine
			//return (sTreeNode)_nodes.Add( text );

			// a little bit of a hack. it's not too bad though;
			// it's prolly what they do internally anyway
			sTreeNode node = new sTreeNode( text );

			if( this.Add( node ) > -1 )
			{
				return node;
			}
			else
			{
				return null;
			}
		}
		public virtual int IndexOf(sTreeNode node)
		{
			return ( _nodes.IndexOf( (TreeNode)node ) );
		}
		public virtual void Insert(int index, sTreeNode node)
		{
			this.CheckSecurityState( node );
			_nodes.Insert( index, (TreeNode)node );
		}
		public virtual void Remove(sTreeNode node)
		{
			_nodes.Remove( (TreeNode)node );
		}
		public virtual bool Contains(sTreeNode node)
		{
			// If value is not of type sTreeNode, this will return false.
			return ( _nodes.Contains( (TreeNode)node ) );
		}

		private void CheckSecurityState(sTreeNode node)
		{
			if( _secHost != null )
			{
				node.Security.DefaultState = _secHost.Security.DefaultState;
				node.Security.EnsureDefaultState();
			}
		}
		#endregion


		#region IList Members

		int IList.Add(object value)
		{
			return this.Add( (sTreeNode)value );
			//throw new Exception( "The method or operation is not implemented. {int IList.Add(object value)}" );
		}

		public void Clear()
		{
			_nodes.Clear();
		}

		bool IList.Contains(object value)
		{
			return this.Contains( (sTreeNode)value );
			//throw new Exception( "The method or operation is not implemented. {bool IList.Contains(object value)}" );
		}

		int IList.IndexOf(object value)
		{
			return this.IndexOf( (sTreeNode)value );
			//throw new Exception( "The method or operation is not implemented. {int IList.IndexOf(object value)}" );
		}

		void IList.Insert(int index, object value)
		{
			this.Insert( index, (sTreeNode)value );
			//throw new Exception( "The method or operation is not implemented. {void IList.Insert(int index, object value)}" );
		}

		public bool IsFixedSize
		{
			get { return ( (IList)_nodes ).IsFixedSize; }
		}

		public bool IsReadOnly
		{
			get { return ( (IList)_nodes ).IsReadOnly; }
		}

		void IList.Remove(object value)
		{
			this.Remove( (sTreeNode)value );
			//throw new Exception( "The method or operation is not implemented. {void IList.Remove(object value)}" );
		}

		public void RemoveAt(int index)
		{
			_nodes.RemoveAt( index );
		}

		object IList.this[int index]
		{
			get
			{
				return (sTreeNode)this[index];
				//throw new Exception( "The method or operation is not implemented {object IList.this[int index] get;}." );
			}
			set
			{
				this[index] = (sTreeNode)value;
				//throw new Exception( "The method or operation is not implemented. {object IList.this[int index] set;}" );
			}
		}

		#endregion

		#region ICollection Members

		public void CopyTo(Array array, int index)
		{
			_nodes.CopyTo( array, index );
		}

		public int Count
		{
			get { return _nodes.Count; }
		}

		public bool IsSynchronized
		{
			get { return ( (ICollection)_nodes ).IsSynchronized; }
		}

		public object SyncRoot
		{
			get { return ( (ICollection)_nodes ).SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return ( (ICollection)_nodes ).GetEnumerator();
		}

		#endregion
	}


	#region EventArg Classes

	public class sTreeViewEventArgs : EventArgs
	{
		private sTreeNode		_node	= null;
		private TreeViewAction	_action	= TreeViewAction.Unknown;

		public sTreeViewEventArgs(TreeViewEventArgs e)
		{
			this._node = (sTreeNode)e.Node;
			this._action = e.Action;
		}

		public virtual sTreeNode Node
		{
			get { return _node; }
		}

		public virtual TreeViewAction Action
		{
			get { return _action; }
		}
	}


	public class sTreeViewCancelEventArgs : System.ComponentModel.CancelEventArgs
	{
		private sTreeNode		_node	= null;
		private TreeViewAction	_action	= TreeViewAction.Unknown;

		public sTreeViewCancelEventArgs( TreeViewCancelEventArgs e ) : base( e.Cancel )
		{
			this._node = (sTreeNode)e.Node;
			this._action = e.Action;
		}


		public virtual sTreeNode Node
		{
			get
			{
				return _node;
			}
		}


		public virtual TreeViewAction Action
		{
			get
			{
				return _action;
			}
		}

	}


	public class sTreeViewDragEventArgs : DragEventArgs
	{
		private TreeNode	_draggedNode		= null;
		private TreeNode	_targetNode			= null;
		private bool		_targetIsChild		= false;
		private bool		_targetIsDragged	= false;
		private bool		_targetIsParent		= false;
		private bool		_targetIsValidNode	= false;


		public sTreeViewDragEventArgs(
			IDataObject data,
			int keyState,
			int x,
			int y,
			DragDropEffects allowedEffect,
			DragDropEffects effect ) : base ( data, keyState, x, y, allowedEffect, effect ){}

		
		public void Eval( sTreeView treeView, Type nodeType )
		{
			// Retrieve the client coordinates of the mouse position.
			Point targetPoint = treeView.PointToClient(new Point(this.X, this.Y));

			// Select the node at the mouse position.
			_targetNode = treeView.GetNodeAt(targetPoint);

			// Retrieve the node that was dragged.
			_draggedNode = (TreeNode)this.Data.GetData(nodeType);

			_targetIsChild = ContainsNode( _draggedNode, _targetNode );

			_targetIsDragged = _draggedNode.Equals( _targetNode );

			_targetIsParent = IsParent( _draggedNode, _targetNode );

			if( !_targetIsChild && !_targetIsDragged && !_targetIsParent )
			{
				_targetIsValidNode = true;
			}
		}

		public virtual sTreeNode DraggedNode
		{
			get { return (sTreeNode)_draggedNode; }
		}

		public virtual sTreeNode TargetNode
		{
			get { return (sTreeNode)_targetNode; }
		}

		public virtual bool TargetIsChild
		{
			get { return _targetIsChild; }
		}

		public virtual bool TargetIsDragged
		{
			get { return _targetIsDragged; }
		}

		public virtual bool TargetIsParent
		{
			get { return _targetIsParent; }
		}


		/// <summary>
		/// Summarizes the TargetIsChild, TargetIsDragged, and TargetIsParent properties.
		/// If all are true, TargetIsValidNode is true.
		/// </summary>
		public virtual bool TargetIsValidNode
		{
			get { return _targetIsValidNode; }
		}

		// Determine whether one node is a parent or ancestor of a second node.
		private bool ContainsNode(TreeNode node1, TreeNode node2)
		{
			// Check the parent node of the second node.
			if (node2.Parent == null) return false;
			if (node2.Parent.Equals(node1)) return true;

			// If the parent node is not null or equal to the first node, 
			// call the ContainsNode method recursively using the parent of 
			// the second node.
			return ContainsNode(node1, node2.Parent);
		}

		private bool IsParent(TreeNode node1, TreeNode node2)
		{
			bool result = false;
			
			if( node1.Parent != null &&
				node1.Parent.Equals( node2 ) ) 
			{
				result = true;
			}

			return result;
		}
	}


	public class sTreeViewMouseEventArgs : MouseEventArgs
	{
		private sTreeNode		_node	= null;

		public sTreeViewMouseEventArgs(sTreeView treeView, MouseButtons button, int clicks,
			int x, int y, int delta) : base( button, clicks, x, y, delta )
		{
			_node = (sTreeNode)treeView.GetNodeAt(x, y);
		}


		public virtual sTreeNode Node
		{
			get
			{
				return _node;
			}
		}
	}
	#endregion


	public interface ISecurityStateHost : ISecureControl
	{
	}
}