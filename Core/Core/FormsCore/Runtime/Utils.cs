using System;
using System.Collections;
using System.Collections.Generic;

using WinForms = System.Windows.Forms;
using WebForms = System.Web.UI;
using Wpf = System.Windows;

using dbg = System.Diagnostics.Debug;


namespace Suplex.Forms
{
	internal class ControlCache : Dictionary<string, ISecureControl>
	{
		private EnumUtil _enumUtil = new EnumUtil();

		public ControlCache() { }

		public void Add(ISecureControl control)
		{
			this[control.UniqueName] = control;
		}

		//this method allows for a sparse collection of controls: the idea
		//is to only collect controls that are needed, as opposed to a collection
		//of all the controls in a heirarchy
		public void AddKnown(ISecureControl control)
		{
			if( this.ContainsKey( control.UniqueName ) &&
				this[control.UniqueName] == null )
			{
				this[control.UniqueName] = control;
			}
		}

		//check the list to see if the control is cached.
		//if not, try iterative and recursive searches to find it.
		public ISecureControl FindControl(string uniqueName, ISecureControl topControl, IEnumerable defaultSearchCollection)
		{
			ISecureControl control = null;

			if( !this.TryGetValue( uniqueName, out control ) )
			{
				control = _enumUtil.ResolveUniqueName( uniqueName, topControl, defaultSearchCollection );
			}
			else
			{
				dbg.WriteLine( string.Format( "ControlCache: Found {0} in NameCache.", uniqueName ) );
			}

			return control;
		}
	}

	internal enum ControlType
	{
		Unknown,
		WinForms,
		WebForms,
		Wpf
	}

	public interface ILogicalChildrenHost
	{
		IEnumerator LogicalChildrenEnumerator { get; }
	}

	internal class EnumUtil
	{
		public static ControlType GetControlType(object control)
		{
			ControlType ct = ControlType.Unknown;

			if( control is WinForms.Control )
				ct = ControlType.WinForms;
			else if( control is WebForms.Control )
				ct = ControlType.WebForms;
			else if( control is Wpf.IInputElement )
				ct = ControlType.Wpf;

			return ct;
		}

		//returning an EmptyCollection provides support for items such as
		//		Components -- items that are in a ISecureContainer or Controls child collection,
		//		but are not ISecureContainers or Controls themselves.
		//note: FrameworkElement/FrameworkContentElement inherit IInputElement as the common denominator,
		//		but the Wpf LogicalTreeHelper class doesn't have an appropriate overload, so have to
		//		handle them individually. dumm.
		public IEnumerable GetChildren(object parent)
		{
			IEnumerable e = null;

			if( parent is ISecureContainer )
				e = ( (ISecureContainer)parent ).GetChildren();
			else if( parent is WinForms.Control )
				e = (IEnumerable)( (WinForms.Control)parent ).Controls;
			else if( parent is WebForms.Control )
				e = (IEnumerable)( (WebForms.Control)parent ).Controls;
			else if( parent is Wpf.FrameworkElement )
				e = new WpfLogicalChildrenEnumeratorWrapper( parent, typeof( Wpf.FrameworkElement ) );	//Wpf.LogicalTreeHelper.GetChildren( (Wpf.FrameworkElement)parent );
			else if( parent is Wpf.FrameworkContentElement )
				e = new WpfLogicalChildrenEnumeratorWrapper( parent, typeof( Wpf.FrameworkContentElement ) );	//Wpf.LogicalTreeHelper.GetChildren( (Wpf.FrameworkContentElement)parent );
			else
				e = new EmptyCollection();

			return e;
		}

		public ISecureControl ResolveUniqueName(string uniqueName, ISecureControl topControl, IEnumerable defaultSearchCollection)
		{
			ISecureControl ctl = topControl;

			if( uniqueName != topControl.UniqueName )
			{

				/* if uniqueName os in the form of ctrl_A.ctrl_B.ctrl_C..., then split on the '.' and
				 * perform a simple recursion over the control collection to find it.
				 * Logic: controlToFind = _topControl[ctrl_A].Controls[ctrl_B].Controls[ctrl_C]...
				 */
				string[] names = uniqueName.Split( new char[] { '.' } );

				//ICollection scc = names.Length > 1 ?
				//    _isWindows ? (ICollection)( (WinForms.Control)_topRrControl ).Controls : (ICollection)( (WebForms.Control)_topRrControl ).Controls
				//    : defaultSearchCollection;
				IEnumerable cc = names.Length > 1 ? this.GetChildren( topControl ) : defaultSearchCollection;

				for( int n = 0; n < names.Length; n++ )
				{
					if( names[n] != topControl.UniqueName )
					{
						ctl = FindByUniqueName( names[n], cc );
					}

					if( ctl != null )
					{
						//scc = _isWindows ? (ICollection)( (WinForms.Control)ctl ).Controls : (ICollection)( (WebForms.Control)ctl ).Controls;
						cc = this.GetChildren( ctl );
					}
					else
					{
						break;
					}
				}

				/* if not found, then the specified dot-delimited uniqueName path was incorrect.
				 * try a complete recursive search to find it.
				 */
				if( ctl == null )
				{
					//dbg.WriteLine( string.Format( "SecurityBuilder: Entering recursive search for {0}.", names[names.Length-1] )  );
					ctl = ResolveUniqueNameRecursive( names[names.Length - 1], topControl );
					dbg.WriteLine( string.Format( "ControlCache: Exiting recursive search for {0}. Status: {1}", names[names.Length - 1], ctl == null ? "NOT Found." : "Found!" ) );
				}
				else
				{
					dbg.WriteLine( string.Format( "ControlCache: Found {0} in iterative search.", names[names.Length - 1] ) );
				}
			}

			return ctl;
		}

		public ISecureControl FindByUniqueName(string uniqueName, IEnumerable coll)
		{
			bool found = false;
			ISecureControl result = null;

			IEnumerator c = coll.GetEnumerator();
			while( !found && c.MoveNext() )
			{
				if( c.Current is ISecureControl )
				{
					found = ( (ISecureControl)c.Current ).UniqueName == uniqueName;
					if( found ) { result = ( (ISecureControl)c.Current ); }
				}
			}

			return result;
		}

		private ISecureControl ResolveUniqueNameRecursive(string uniqueName, object control)
		{
			ISecureControl ctl = null;
			//IEnumerator controls = _isWindows ?
			//    ( (WinForms.Control)control ).Controls.GetEnumerator() :
			//    ( (WebForms.Control)control ).Controls.GetEnumerator();
			IEnumerator controls = this.GetChildren( control ).GetEnumerator();
			while( controls.MoveNext() && ctl == null )
			{
				if( controls.Current is ISecureControl &&
					( (ISecureControl)controls.Current ).UniqueName == uniqueName )
				{
					ctl = (ISecureControl)controls.Current;
				}
				else
				{
					ctl = ResolveUniqueNameRecursive( uniqueName, controls.Current );
				}
			}

			return ctl;
		}

		public static string GetControlDisplayName(object control, string delimiter)
		{
			string displayName = control.ToString();

			if( control is ISecureControl )
			{
				displayName = string.Format( "{1}{0}{1}", ( (ISecureControl)control ).UniqueName, delimiter );
			}
			else if( control is WinForms.Control )
			{
				displayName = ( (WinForms.Control)control ).Name;
			}
			else if( control is WebForms.Control )
			{
				displayName = ( (WebForms.Control)control ).ID;
				displayName = string.IsNullOrEmpty( displayName ) ? ( (WebForms.Control)control ).ClientID : displayName;
			}

			return string.IsNullOrEmpty( displayName ) ?
				string.Format( "{0} [Unknown Name]", control.ToString() ) : displayName;
		}
	}

	public class WpfLogicalChildrenEnumeratorWrapper : IEnumerable
	{
		private System.Reflection.PropertyInfo _logicalChildren = null;
		private object _wpfElement = null;

		public WpfLogicalChildrenEnumeratorWrapper(object wpfElement)
		{
			_wpfElement = wpfElement;
		}

		public WpfLogicalChildrenEnumeratorWrapper(object wpfElement, System.Reflection.PropertyInfo logicalChildren)
		{
			_logicalChildren = logicalChildren;
			_wpfElement = wpfElement;
		}

		public WpfLogicalChildrenEnumeratorWrapper(object wpfElement, Type type)
		{
			System.Reflection.PropertyInfo logicalChildren = type.GetProperty( "LogicalChildren",
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic |
					System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.FlattenHierarchy );

			_logicalChildren = logicalChildren;
			_wpfElement = wpfElement;
		}

		//this ensures a unique enumerator instance, so long as it's newed-up from the source
		public IEnumerator GetEnumerator()
		{
			if( _wpfElement is ILogicalChildrenHost )
			{
				return ( (ILogicalChildrenHost)_wpfElement ).LogicalChildrenEnumerator;
			}
			else
			{
				IEnumerator enumerator = (IEnumerator)_logicalChildren.GetValue( _wpfElement, null );
				if( enumerator == null )
				{
					enumerator = EmptyEnumerator.Instance;
				}
				return enumerator;
			}
		}
	}


	//support for an empty, non-updateable collection
	public class EmptyCollection : ICollection
	{
		#region ICollection Members

		public void CopyTo(Array array, int index)
		{
			throw new Exception( "The method or operation is not implemented." );
		}

		public int Count
		{
			get { return 0; }
		}

		public bool IsSynchronized
		{
			get { return true; }
		}

		public object SyncRoot
		{
			get { return null; }
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return new EmptyEnumerator();
		}

		#endregion
	}

	public class EmptyEnumerator : IEnumerator
	{
		private static IEnumerator _instance;

		public EmptyEnumerator() { }

		public static IEnumerator Instance
		{
			get
			{
				if( _instance == null )
				{
					_instance = new EmptyEnumerator();
				}
				return _instance;
			}
		}

		#region IEnumerator Members

		public object Current
		{
			get { return null; }
		}

		public bool MoveNext()
		{
			return false;
		}

		public void Reset()
		{
		}

		#endregion
	}
}