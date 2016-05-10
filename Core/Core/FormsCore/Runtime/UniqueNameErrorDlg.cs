using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Suplex.Forms
{
	/// <summary>
	/// Summary description for unError.
	/// </summary>
	public class UniqueNameErrorDlg : System.Windows.Forms.Form
	{
		private string _uniqueNameToResolve = "";
		private EnumUtil _enumUtil = new EnumUtil();


		private System.Windows.Forms.TextBox txtErrorMsg;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtNodePath;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TreeView tvwHierarchy;
		private System.Windows.Forms.Label label3;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public UniqueNameErrorDlg()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// Add any constructor code after InitializeComponent call
			//
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(UniqueNameErrorDlg));
			this.tvwHierarchy = new System.Windows.Forms.TreeView();
			this.txtErrorMsg = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.txtNodePath = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// tvwHierarchy
			// 
			this.tvwHierarchy.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.tvwHierarchy.ImageIndex = -1;
			this.tvwHierarchy.Location = new System.Drawing.Point(0, 104);
			this.tvwHierarchy.Name = "tvwHierarchy";
			this.tvwHierarchy.PathSeparator = ".";
			this.tvwHierarchy.SelectedImageIndex = -1;
			this.tvwHierarchy.Size = new System.Drawing.Size(552, 280);
			this.tvwHierarchy.TabIndex = 0;
			this.tvwHierarchy.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvwHierarchy_AfterSelect);
			// 
			// txtErrorMsg
			// 
			this.txtErrorMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtErrorMsg.Location = new System.Drawing.Point(0, 16);
			this.txtErrorMsg.Multiline = true;
			this.txtErrorMsg.Name = "txtErrorMsg";
			this.txtErrorMsg.ReadOnly = true;
			this.txtErrorMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtErrorMsg.Size = new System.Drawing.Size(552, 56);
			this.txtErrorMsg.TabIndex = 1;
			this.txtErrorMsg.Text = "";
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(160, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "Error:";
			// 
			// txtNodePath
			// 
			this.txtNodePath.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.txtNodePath.Location = new System.Drawing.Point(0, 403);
			this.txtNodePath.Multiline = true;
			this.txtNodePath.Name = "txtNodePath";
			this.txtNodePath.ReadOnly = true;
			this.txtNodePath.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtNodePath.Size = new System.Drawing.Size(552, 48);
			this.txtNodePath.TabIndex = 3;
			this.txtNodePath.Text = "";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(0, 387);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(160, 16);
			this.label2.TabIndex = 4;
			this.label2.Text = "Path to Selected Node:";
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(0, 88);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(160, 16);
			this.label3.TabIndex = 5;
			this.label3.Text = "Control Hierarchy:";
			// 
			// unError
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(552, 451);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtNodePath);
			this.Controls.Add(this.txtErrorMsg);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.tvwHierarchy);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(360, 360);
			this.Name = "unError";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Error Resolving UniqueName";
			this.ResumeLayout(false);

		}
		#endregion



		public void ShowValidationControlError(IValidationControl topControl, string uniqueNameToResolve, string errMsg)
		{
			txtErrorMsg.Text = errMsg;
			_uniqueNameToResolve = uniqueNameToResolve;

			BuildValidationControlError( tvwHierarchy.Nodes, topControl );

			this.ShowDialog();
		}

		private void BuildValidationControlError(TreeNodeCollection parentNodes, object control )
		{
			TreeNode node = parentNodes.Add( EnumUtil.GetControlDisplayName( control, "/" ) );
			if( control is IValidationControl )
			{
				if( _uniqueNameToResolve.IndexOf( ( (IValidationControl)control ).UniqueName ) > -1 )
				{
					node.EnsureVisible();
					node.BackColor = Color.Yellow;
				}
			}

			//IEnumerator controls = control.ValidationControls.Values.GetEnumerator();
			IEnumerator controls = _enumUtil.GetChildren( control ).GetEnumerator();
			while( controls.MoveNext() )
			{
				BuildValidationControlError( node.Nodes, controls.Current );	//(IValidationControl)
			}
		}

		public void ShowSecureControlError(ISecureControl topControl, string uniqueNameToResolve, string errMsg)
		{
			txtErrorMsg.Text = errMsg;
			_uniqueNameToResolve = uniqueNameToResolve;

			BuildSecureControlError( tvwHierarchy.Nodes, (Control)topControl );

			this.ShowDialog();
		}

		private void BuildSecureControlError(TreeNodeCollection parentNodes, Control control)
		{
			bool isSC = control is ISecureControl;
			TreeNode node = parentNodes.Add( isSC ? ( (ISecureControl)control ).UniqueName : control.Name );
			if( isSC && _uniqueNameToResolve.IndexOf( ( (ISecureControl)control ).UniqueName ) > -1 )
			{
				node.EnsureVisible();
				node.BackColor = Color.Yellow;
			}

			//IEnumerator controls = control.SecureControls.Values.GetEnumerator();
			//while( controls.MoveNext() )
			foreach ( Control c in control.Controls)
			{
				//BuildSecureControlError( node.Nodes, (ISecureControl)controls.Current );
				this.BuildSecureControlError( node.Nodes, c );
			}
		}

		//private void BuildSecureControlError(TreeNodeCollection parentNodes, ISecureControl control)
		//{
		//    TreeNode node = parentNodes.Add( control.UniqueName );
		//    if( _uniqueNameToResolve.IndexOf( control.UniqueName ) > -1 )
		//    {
		//        node.EnsureVisible();
		//        node.BackColor = Color.Yellow;
		//    }

		//    IEnumerator controls = control.SecureControls.Values.GetEnumerator();
		//    while( controls.MoveNext() )
		//    {
		//        BuildSecureControlError( node.Nodes, (ISecureControl)controls.Current );
		//    }
		//}

		private void tvwHierarchy_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			txtNodePath.Text = e.Node.FullPath;
		}
	}
}
