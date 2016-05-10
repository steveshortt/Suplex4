using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

using Suplex.Data;
using Suplex.Forms;
using Suplex.Security;
using Suplex.Security.Standard;
using System.Collections.Generic;

namespace Suplex.WinForms
{
	[ToolboxItem( true ), ToolboxBitmap( typeof( TreeView ) )]
	[EventBindings( EventBindingsAttribute.BaseEvents.WinForms, ControlEvents.CheckStateChanged )]
	public class stTreeView<T> : sTreeView
	{
		private stTreeNodeCollection<T> _nodes = null;


		public stTreeView()
			: base()
		{
			_nodes = new stTreeNodeCollection<T>( base.Nodes, this );
		}

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content ), MergableProperty( false )]
		new public virtual stTreeNodeCollection<T> Nodes
		{
			get { return _nodes; }
		}

		public override ICollection GetChildren()
		{
			return _nodes;
		}
	}

	public class stTreeNodeCollection<T> : IList<T>, ICollection<T>,
		IEnumerable<T>, IList, ICollection, IEnumerable
	{
		private sTreeNodeCollection _nodes = null;
		private ISecurityStateHost _secHost = null;

		public stTreeNodeCollection(sTreeNodeCollection nodes)
		{
			_nodes = nodes;
		}
		public stTreeNodeCollection(sTreeNodeCollection nodes, ISecurityStateHost host)
		{
			_nodes = nodes;
			_secHost = host;
		}

		#region IList<T> Members
		public T this[int index]
		{
			get
			{
				return (T)_nodes[index];
			}
			set
			{
				_nodes[index] = (sTreeNode)value;
			}
		}

		public int IndexOf(T item)
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		public void Insert(int index, T item)
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		public void RemoveAt(int index)
		{
			throw new Exception( "The method or operation is not implemented." );
		}
		#endregion

		#region ICollection<T> Members

		public void Add(T item)
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		public void Clear()
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		public bool Contains(T item)
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		public int Count
		{
			get { throw new Exception( "The method or operation is not implemented." ); }
		}

		public bool IsReadOnly
		{
			get { throw new Exception( "The method or operation is not implemented." ); }
		}

		public bool Remove(T item)
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		#endregion

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		#endregion

		#region IList Members

		int IList.Add(object value)
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		void IList.Clear()
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		bool IList.Contains(object value)
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		int IList.IndexOf(object value)
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		void IList.Insert(int index, object value)
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		bool IList.IsFixedSize
		{
			get { throw new Exception( "The method or operation is not implemented." ); }
		}

		bool IList.IsReadOnly
		{
			get { throw new Exception( "The method or operation is not implemented." ); }
		}

		void IList.Remove(object value)
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		void IList.RemoveAt(int index)
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		object IList.this[int index]
		{
			get
			{
				throw new Exception( "The method or operation is not implemented." );
			}
			set
			{
				throw new Exception( "The method or operation is not implemented." );
			}
		}

		#endregion

		#region ICollection Members

		void ICollection.CopyTo(Array array, int index)
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		int ICollection.Count
		{
			get { throw new Exception( "The method or operation is not implemented." ); }
		}

		bool ICollection.IsSynchronized
		{
			get { throw new Exception( "The method or operation is not implemented." ); }
		}

		object ICollection.SyncRoot
		{
			get { throw new Exception( "The method or operation is not implemented." ); }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		#endregion
	}
}