using System;

namespace Suplex.Forms
{
	/// <summary>
	/// Summary description for UniqueNameErrWeb.
	/// </summary>
	public class UniqueNameErrWeb
	{
		public UniqueNameErrWeb(){}


//		public void ShowValidationControlError(IValidationControl topControl, string uniqueNameToResolve, string errMsg)
//		{
//			txtErrorMsg.Text = errMsg;
//			_uniqueNameToResolve = uniqueNameToResolve;
//
//			BuildValidationControlError( tvwHierarchy.Nodes, topControl );
//
//			this.ShowDialog();
//		}
//
//
//		private void BuildValidationControlError(TreeNodeCollection parentNodes, IValidationControl control )
//		{
//			TreeNode node = parentNodes.Add( control.UniqueName );
//			if( _uniqueNameToResolve.IndexOf( control.UniqueName ) > -1 )
//			{
//				node.EnsureVisible();
//				node.BackColor = Color.Yellow;
//			}
//
//			IEnumerator controls = control.ValidationControls.Values.GetEnumerator();
//			while( controls.MoveNext() )
//			{
//				BuildValidationControlError( node.Nodes, (IValidationControl)controls.Current );
//			}
//		}
//
//
//		public void ShowSecureControlError(ISecureControl topControl, string uniqueNameToResolve, string errMsg)
//		{
//			txtErrorMsg.Text = errMsg;
//			_uniqueNameToResolve = uniqueNameToResolve;
//
//			BuildSecureControlError( tvwHierarchy.Nodes, topControl );
//
//			this.ShowDialog();
//		}
//
//
//		private void BuildSecureControlError(TreeNodeCollection parentNodes, ISecureControl control )
//		{
//			TreeNode node = parentNodes.Add( control.UniqueName );
//			if( _uniqueNameToResolve.IndexOf( control.UniqueName ) > -1 )
//			{
//				node.EnsureVisible();
//				node.BackColor = Color.Yellow;
//			}
//
//			IEnumerator controls = control.SecureControls.Values.GetEnumerator();
//			while( controls.MoveNext() )
//			{
//				BuildSecureControlError( node.Nodes, (ISecureControl)controls.Current );
//			}
//		}
//
//
//		
//		
//		private void tvwHierarchy_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
//		{
//			txtNodePath.Text = e.Node.FullPath;
//		}
	}
}