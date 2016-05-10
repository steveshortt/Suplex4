using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms.Design;

namespace Suplex.WinForms
{
	internal class sTreeNodeCollectionEditor : CollectionEditor
	{
		// Methods
		public sTreeNodeCollectionEditor()
			: base( typeof( sTreeNodeCollection ) )
		{
		}

		protected override CollectionEditor.CollectionForm CreateCollectionForm()
		{
			return new sTreeNodeCollectionForm( this );
		}

		// Properties
		protected override string HelpTopic
		{
			get
			{
				return "net.ComponentModel.TreeNodeCollectionEditor";
			}
		}

		// Nested Types
		private class sTreeNodeCollectionForm : CollectionEditor.CollectionForm
		{
			// Fields
			private Button btnAddChild;
			private Button btnAddRoot;
			private Button btnCancel;
			private Button btnDelete;
			private sTreeNode curNode;
			private sTreeNodeCollectionEditor editor;
			private int intialNextNode;
			private Label label1;
			private Label label2;
			private Button moveDownButton;
			private Button moveUpButton;
			private TableLayoutPanel navigationButtonsTableLayoutPanel;
			private int nextNode;
			private static object NextNodeKey = new object();
			private TableLayoutPanel nodeControlPanel;
			private Button okButton;
			private TableLayoutPanel okCancelPanel;
			private TableLayoutPanel overarchingTableLayoutPanel;
			private VsPropertyGrid propertyGrid1;
			private sTreeView treeView1;

			// Methods
			public sTreeNodeCollectionForm(CollectionEditor editor)
				: base( editor )
			{
				this.editor = (sTreeNodeCollectionEditor)editor;
				this.InitializeComponent();
				this.HookEvents();
				this.intialNextNode = this.NextNode;
				this.SetButtonsState();
			}

			private void Add(sTreeNode parent)
			{
				sTreeNode node = null;
				string text = "new node";	// SR.GetString( "BaseNodeName" );
				if( parent == null )
				{
					int num;
					this.NextNode = ( num = this.NextNode ) + 1;
					node = this.treeView1.Nodes.Add( text + num.ToString( CultureInfo.InvariantCulture ) );
					node.Name = node.Text;
				}
				else
				{
					int num3;
					this.NextNode = ( num3 = this.NextNode ) + 1;
					node = parent.Nodes.Add( text + num3.ToString( CultureInfo.InvariantCulture ) );
					node.Name = node.Text;
					parent.Expand();
				}
				if( parent != null )
				{
					this.treeView1.SelectedNode = parent;
				}
				else
				{
					this.treeView1.SelectedNode = node;
					this.SetNodeProps( node );
				}
			}

			private void BtnAddChild_click(object sender, EventArgs e)
			{
				this.Add( this.curNode );
				this.SetButtonsState();
			}

			private void BtnAddRoot_click(object sender, EventArgs e)
			{
				this.Add( null );
				this.SetButtonsState();
			}

			private void BtnCancel_click(object sender, EventArgs e)
			{
				if( this.NextNode != this.intialNextNode )
				{
					this.NextNode = this.intialNextNode;
				}
			}

			private void BtnDelete_click(object sender, EventArgs e)
			{
				this.curNode.Remove();
				if( this.treeView1.Nodes.Count == 0 )
				{
					this.curNode = null;
					this.SetNodeProps( null );
				}
				this.SetButtonsState();
			}

			private void BtnOK_click(object sender, EventArgs e)
			{
				object[] objArray = new object[this.treeView1.Nodes.Count];
				for( int i = 0; i < objArray.Length; i++ )
				{
					objArray[i] = this.treeView1.Nodes[i].Clone();
				}
				base.Items = objArray;
				this.treeView1.Dispose();
				this.treeView1 = null;
			}

			private bool CheckParent(TreeNode child, TreeNode parent)
			{
				while( child != null )
				{
					if( parent == child.Parent )
					{
						return true;
					}
					child = child.Parent;
				}
				return false;
			}

			private void HookEvents()
			{
				this.okButton.Click += new EventHandler( this.BtnOK_click );
				this.btnCancel.Click += new EventHandler( this.BtnCancel_click );
				this.btnAddChild.Click += new EventHandler( this.BtnAddChild_click );
				this.btnAddRoot.Click += new EventHandler( this.BtnAddRoot_click );
				this.btnDelete.Click += new EventHandler( this.BtnDelete_click );
				this.propertyGrid1.PropertyValueChanged += new PropertyValueChangedEventHandler( this.PropertyGrid_propertyValueChanged );
				this.treeView1.AfterSelect += new sTreeViewEventHandler( this.treeView1_afterSelect );
				this.treeView1.DragEnter += new DragEventHandler( this.treeView1_DragEnter );
				this.treeView1.ItemDrag += new ItemDragEventHandler( this.treeView1_ItemDrag );
				this.treeView1.DragDrop += new sTreeViewDragEventHandler( this.treeView1_DragDrop );
				this.treeView1.DragOver += new sTreeViewDragEventHandler( this.treeView1_DragOver );
				base.HelpButtonClicked += new CancelEventHandler( this.sTreeNodeCollectionEditor_HelpButtonClicked );
				this.moveDownButton.Click += new EventHandler( this.moveDownButton_Click );
				this.moveUpButton.Click += new EventHandler( this.moveUpButton_Click );
			}

			private void InitializeComponent()
			{
				ComponentResourceManager manager = new ComponentResourceManager( typeof( sTreeNodeCollectionEditor ) );
				this.okCancelPanel = new TableLayoutPanel();
				this.okButton = new Button();
				this.btnCancel = new Button();
				this.nodeControlPanel = new TableLayoutPanel();
				this.btnAddRoot = new Button();
				this.btnAddChild = new Button();
				this.btnDelete = new Button();
				this.moveDownButton = new Button();
				this.moveUpButton = new Button();
				this.propertyGrid1 = new VsPropertyGrid( base.Context );
				this.label2 = new Label();
				this.treeView1 = new sTreeView();
				this.label1 = new Label();
				this.overarchingTableLayoutPanel = new TableLayoutPanel();
				this.navigationButtonsTableLayoutPanel = new TableLayoutPanel();
				this.okCancelPanel.SuspendLayout();
				this.nodeControlPanel.SuspendLayout();
				this.overarchingTableLayoutPanel.SuspendLayout();
				this.navigationButtonsTableLayoutPanel.SuspendLayout();
				base.SuspendLayout();
				manager.ApplyResources( this.okCancelPanel, "okCancelPanel" );
				this.okCancelPanel.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, 50f ) );
				this.okCancelPanel.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, 50f ) );
				this.okCancelPanel.Controls.Add( this.okButton, 0, 0 );
				this.okCancelPanel.Controls.Add( this.btnCancel, 1, 0 );
				this.okCancelPanel.Margin = new Padding( 3, 3, 0, 0 );
				this.okCancelPanel.Name = "okCancelPanel";
				this.okCancelPanel.RowStyles.Add( new RowStyle() );
				manager.ApplyResources( this.okButton, "okButton" );
				this.okButton.DialogResult = DialogResult.OK;
				this.okButton.Margin = new Padding( 0, 0, 3, 0 );
				this.okButton.Name = "okButton";
				manager.ApplyResources( this.btnCancel, "btnCancel" );
				this.btnCancel.DialogResult = DialogResult.Cancel;
				this.btnCancel.Margin = new Padding( 3, 0, 0, 0 );
				this.btnCancel.Name = "btnCancel";
				manager.ApplyResources( this.nodeControlPanel, "nodeControlPanel" );
				this.nodeControlPanel.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, 50f ) );
				this.nodeControlPanel.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, 50f ) );
				this.nodeControlPanel.Controls.Add( this.btnAddRoot, 0, 0 );
				this.nodeControlPanel.Controls.Add( this.btnAddChild, 1, 0 );
				this.nodeControlPanel.Margin = new Padding( 0, 3, 3, 3 );
				this.nodeControlPanel.Name = "nodeControlPanel";
				this.nodeControlPanel.RowStyles.Add( new RowStyle() );
				manager.ApplyResources( this.btnAddRoot, "btnAddRoot" );
				this.btnAddRoot.Margin = new Padding( 0, 0, 3, 0 );
				this.btnAddRoot.Name = "btnAddRoot";
				manager.ApplyResources( this.btnAddChild, "btnAddChild" );
				this.btnAddChild.Margin = new Padding( 3, 0, 0, 0 );
				this.btnAddChild.Name = "btnAddChild";
				manager.ApplyResources( this.btnDelete, "btnDelete" );
				this.btnDelete.Margin = new Padding( 0, 3, 0, 0 );
				this.btnDelete.Name = "btnDelete";
				manager.ApplyResources( this.moveDownButton, "moveDownButton" );
				this.moveDownButton.Margin = new Padding( 0, 1, 0, 3 );
				this.moveDownButton.Name = "moveDownButton";
				manager.ApplyResources( this.moveUpButton, "moveUpButton" );
				this.moveUpButton.Margin = new Padding( 0, 0, 0, 1 );
				this.moveUpButton.Name = "moveUpButton";
				manager.ApplyResources( this.propertyGrid1, "propertyGrid1" );
				this.propertyGrid1.LineColor = SystemColors.ScrollBar;
				this.propertyGrid1.Margin = new Padding( 3, 3, 0, 3 );
				this.propertyGrid1.Name = "propertyGrid1";
				this.overarchingTableLayoutPanel.SetRowSpan( this.propertyGrid1, 2 );
				manager.ApplyResources( this.label2, "label2" );
				this.label2.Margin = new Padding( 3, 1, 0, 0 );
				this.label2.Name = "label2";
				this.treeView1.AllowDrop = true;
				manager.ApplyResources( this.treeView1, "treeView1" );
				this.treeView1.HideSelection = false;
				this.treeView1.Margin = new Padding( 0, 3, 3, 3 );
				this.treeView1.Name = "treeView1";
				manager.ApplyResources( this.label1, "label1" );
				this.label1.Margin = new Padding( 0, 1, 3, 0 );
				this.label1.Name = "label1";
				manager.ApplyResources( this.overarchingTableLayoutPanel, "overarchingTableLayoutPanel" );
				this.overarchingTableLayoutPanel.ColumnStyles.Add( new ColumnStyle() );
				this.overarchingTableLayoutPanel.ColumnStyles.Add( new ColumnStyle() );
				this.overarchingTableLayoutPanel.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, 100f ) );
				this.overarchingTableLayoutPanel.Controls.Add( this.navigationButtonsTableLayoutPanel, 1, 1 );
				this.overarchingTableLayoutPanel.Controls.Add( this.label2, 2, 0 );
				this.overarchingTableLayoutPanel.Controls.Add( this.propertyGrid1, 2, 1 );
				this.overarchingTableLayoutPanel.Controls.Add( this.treeView1, 0, 1 );
				this.overarchingTableLayoutPanel.Controls.Add( this.label1, 0, 0 );
				this.overarchingTableLayoutPanel.Controls.Add( this.nodeControlPanel, 0, 2 );
				this.overarchingTableLayoutPanel.Controls.Add( this.okCancelPanel, 2, 3 );
				this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
				this.overarchingTableLayoutPanel.RowStyles.Add( new RowStyle() );
				this.overarchingTableLayoutPanel.RowStyles.Add( new RowStyle( SizeType.Percent, 100f ) );
				this.overarchingTableLayoutPanel.RowStyles.Add( new RowStyle() );
				this.overarchingTableLayoutPanel.RowStyles.Add( new RowStyle() );
				manager.ApplyResources( this.navigationButtonsTableLayoutPanel, "navigationButtonsTableLayoutPanel" );
				this.navigationButtonsTableLayoutPanel.ColumnStyles.Add( new ColumnStyle() );
				this.navigationButtonsTableLayoutPanel.Controls.Add( this.moveUpButton, 0, 0 );
				this.navigationButtonsTableLayoutPanel.Controls.Add( this.btnDelete, 0, 2 );
				this.navigationButtonsTableLayoutPanel.Controls.Add( this.moveDownButton, 0, 1 );
				this.navigationButtonsTableLayoutPanel.Margin = new Padding( 3, 3, 0x12, 3 );
				this.navigationButtonsTableLayoutPanel.Name = "navigationButtonsTableLayoutPanel";
				this.navigationButtonsTableLayoutPanel.RowStyles.Add( new RowStyle() );
				this.navigationButtonsTableLayoutPanel.RowStyles.Add( new RowStyle() );
				this.navigationButtonsTableLayoutPanel.RowStyles.Add( new RowStyle() );
				base.AcceptButton = this.okButton;
				manager.ApplyResources( this, "$this" );
				base.AutoScaleMode = AutoScaleMode.Font;
				base.CancelButton = this.btnCancel;
				base.Controls.Add( this.overarchingTableLayoutPanel );
				base.HelpButton = true;
				base.MaximizeBox = false;
				base.MinimizeBox = false;
				base.Name = "sTreeNodeCollectionEditor";
				base.ShowIcon = false;
				base.ShowInTaskbar = false;
				base.SizeGripStyle = SizeGripStyle.Show;
				this.okCancelPanel.ResumeLayout( false );
				this.okCancelPanel.PerformLayout();
				this.nodeControlPanel.ResumeLayout( false );
				this.nodeControlPanel.PerformLayout();
				this.overarchingTableLayoutPanel.ResumeLayout( false );
				this.overarchingTableLayoutPanel.PerformLayout();
				this.navigationButtonsTableLayoutPanel.ResumeLayout( false );
				base.ResumeLayout( false );
			}

			private void moveDownButton_Click(object sender, EventArgs e)
			{
				sTreeNode curNode = this.curNode;
				TreeNode parent = this.curNode.Parent;
				if( parent == null )
				{
					this.treeView1.Nodes.RemoveAt( curNode.Index );
					this.treeView1.Nodes[curNode.Index].Nodes.Insert( 0, curNode );
				}
				else
				{
					parent.Nodes.RemoveAt( curNode.Index );
					if( curNode.Index < parent.Nodes.Count )
					{
						parent.Nodes[curNode.Index].Nodes.Insert( 0, curNode );
					}
					else if( parent.Parent == null )
					{
						this.treeView1.Nodes.Insert( parent.Index + 1, curNode );
					}
					else
					{
						parent.Parent.Nodes.Insert( parent.Index + 1, curNode );
					}
				}
				this.treeView1.SelectedNode = curNode;
				this.curNode = curNode;
			}

			private void moveUpButton_Click(object sender, EventArgs e)
			{
				sTreeNode curNode = this.curNode;
				TreeNode parent = this.curNode.Parent;
				if( parent == null )
				{
					this.treeView1.Nodes.RemoveAt( curNode.Index );
					this.treeView1.Nodes[curNode.Index - 1].Nodes.Add( curNode );
				}
				else
				{
					parent.Nodes.RemoveAt( curNode.Index );
					if( curNode.Index == 0 )
					{
						if( parent.Parent == null )
						{
							this.treeView1.Nodes.Insert( parent.Index, curNode );
						}
						else
						{
							parent.Parent.Nodes.Insert( parent.Index, curNode );
						}
					}
					else
					{
						parent.Nodes[curNode.Index - 1].Nodes.Add( curNode );
					}
				}
				this.treeView1.SelectedNode = curNode;
				this.curNode = curNode;
			}

			protected override void OnEditValueChanged()
			{
				if( base.EditValue != null )
				{
					object[] items = base.Items;
					//////this.propertyGrid1.Site = new CollectionEditor.PropertyGridSite( base.Context, this.propertyGrid1 );
					sTreeNode[] nodes = new sTreeNode[items.Length];
					for( int i = 0; i < items.Length; i++ )
					{
						nodes[i] = (sTreeNode)( (sTreeNode)items[i] ).Clone();
					}
					this.treeView1.Nodes.Clear();
					this.treeView1.Nodes.AddRange( nodes );
					this.curNode = null;
					this.btnAddChild.Enabled = false;
					this.btnDelete.Enabled = false;
					TreeView treeView = this.TreeView;
					if( treeView != null )
					{
						this.SetImageProps( treeView );
					}
					if( ( items.Length > 0 ) && ( nodes[0] != null ) )
					{
						this.treeView1.SelectedNode = nodes[0];
					}
				}
			}

			private void PropertyGrid_propertyValueChanged(object sender, PropertyValueChangedEventArgs e)
			{
				this.label2.Text = this.treeView1.SelectedNode.Text;	// SR.GetString( "CollectionEditorProperties", new object[] { this.treeView1.SelectedNode.Text } );
			}

			private void SetButtonsState()
			{
				bool flag = this.treeView1.Nodes.Count > 0;
				this.btnAddChild.Enabled = flag;
				this.btnDelete.Enabled = flag;
				this.moveDownButton.Enabled = ( flag && ( ( this.curNode != this.LastNode ) || ( this.curNode.Level > 0 ) ) ) && ( this.curNode != this.treeView1.Nodes[this.treeView1.Nodes.Count - 1] );
				this.moveUpButton.Enabled = flag && ( this.curNode != this.treeView1.Nodes[0] );
			}

			private void SetImageProps(TreeView actualTreeView)
			{
				if( actualTreeView.ImageList != null )
				{
					this.treeView1.ImageList = actualTreeView.ImageList;
					this.treeView1.ImageIndex = actualTreeView.ImageIndex;
					this.treeView1.SelectedImageIndex = actualTreeView.SelectedImageIndex;
				}
				else
				{
					this.treeView1.ImageList = null;
					this.treeView1.ImageIndex = -1;
					this.treeView1.SelectedImageIndex = -1;
				}
				if( actualTreeView.StateImageList != null )
				{
					this.treeView1.StateImageList = actualTreeView.StateImageList;
				}
				else
				{
					this.treeView1.StateImageList = null;
				}
				this.treeView1.CheckBoxes = actualTreeView.CheckBoxes;
			}

			private void SetNodeProps(sTreeNode node)
			{
				if( node != null )
				{
					this.label2.Text = node.Name.ToString();	// SR.GetString( "CollectionEditorProperties", new object[] { node.Name.ToString() } );
				}
				else
				{
					this.label2.Text = "none";	// SR.GetString( "CollectionEditorPropertiesNone" );
				}
				this.propertyGrid1.SelectedObject = node;
			}

			private void sTreeNodeCollectionEditor_HelpButtonClicked(object sender, CancelEventArgs e)
			{
				e.Cancel = true;
				this.editor.ShowHelp();
			}

			private void treeView1_afterSelect(object sender, sTreeViewEventArgs e)
			{
				this.curNode = e.Node;
				this.SetNodeProps( this.curNode );
				this.SetButtonsState();
			}

			private void treeView1_DragDrop(object sender, DragEventArgs e)
			{
				sTreeNode data = (sTreeNode)e.Data.GetData( typeof( sTreeNode ) );
				Point p = new Point( 0, 0 );
				p.X = e.X;
				p.Y = e.Y;
				p = this.treeView1.PointToClient( p );
				TreeNode nodeAt = this.treeView1.GetNodeAt( p );
				if( data != nodeAt )
				{
					this.treeView1.Nodes.Remove( data );
					if( ( nodeAt != null ) && !this.CheckParent( nodeAt, data ) )
					{
						nodeAt.Nodes.Add( data );
					}
					else
					{
						this.treeView1.Nodes.Add( data );
					}
				}
			}

			private void treeView1_DragEnter(object sender, DragEventArgs e)
			{
				if( e.Data.GetDataPresent( typeof( sTreeNode ) ) )
				{
					e.Effect = DragDropEffects.Move;
				}
				else
				{
					e.Effect = DragDropEffects.None;
				}
			}

			private void treeView1_DragOver(object sender, sTreeViewDragEventArgs e)
			{
				Point p = new Point( 0, 0 );
				p.X = e.X;
				p.Y = e.Y;
				p = this.treeView1.PointToClient( p );
				TreeNode nodeAt = this.treeView1.GetNodeAt( p );
				this.treeView1.SelectedNode = nodeAt;
			}

			private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
			{
				sTreeNode item = (sTreeNode)e.Item;
				base.DoDragDrop( item, DragDropEffects.Move );
			}

			// Properties
			private sTreeNode LastNode
			{
				get
				{
					sTreeNode node = this.treeView1.Nodes[this.treeView1.Nodes.Count - 1];
					while( node.Nodes.Count > 0 )
					{
						node = node.Nodes[node.Nodes.Count - 1];
					}
					return node;
				}
			}

			private int NextNode
			{
				get
				{
					if( ( this.TreeView != null ) && ( this.TreeView.Site != null ) )
					{
						IDictionaryService service = (IDictionaryService)this.TreeView.Site.GetService( typeof( IDictionaryService ) );
						if( service != null )
						{
							object obj2 = service.GetValue( NextNodeKey );
							if( obj2 != null )
							{
								this.nextNode = (int)obj2;
							}
							else
							{
								this.nextNode = 0;
								service.SetValue( NextNodeKey, 0 );
							}
						}
					}
					return this.nextNode;
				}
				set
				{
					this.nextNode = value;
					if( ( this.TreeView != null ) && ( this.TreeView.Site != null ) )
					{
						IDictionaryService service = (IDictionaryService)this.TreeView.Site.GetService( typeof( IDictionaryService ) );
						if( service != null )
						{
							service.SetValue( NextNodeKey, this.nextNode );
						}
					}
				}
			}

			private TreeView TreeView
			{
				get
				{
					if( ( base.Context != null ) && ( base.Context.Instance is TreeView ) )
					{
						return (TreeView)base.Context.Instance;
					}
					return null;
				}
			}
		}
	}

	internal class VsPropertyGrid : PropertyGrid
	{
		// Methods
		public VsPropertyGrid(IServiceProvider serviceProvider)
		{
			if( serviceProvider != null )
			{
				IUIService service = serviceProvider.GetService( typeof( IUIService ) ) as IUIService;
				if( service != null )
				{
					base.ToolStripRenderer = (ToolStripProfessionalRenderer)service.Styles["VsToolWindowRenderer"];
				}
			}
		}
	}
 

}