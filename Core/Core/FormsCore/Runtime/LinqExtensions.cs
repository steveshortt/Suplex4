using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suplex.Forms
{
	public static class LinqExtensions
	{
		/// <summary>
		///     Recursively projects each nested element to an <see cref="IEnumerable{TSource}"/>
		///     and flattens the resulting sequences into one sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <param name="source">A sequence of values to project.</param>
		/// <param name="recursiveSelector">A transform to apply to each element.</param>
		/// <returns>
		///     An <see cref="IEnumerable{TSource}"/> whose elements are the
		///     result of recursively invoking the recursive transform function
		///     on each element and nested element of the input sequence.
		/// </returns>
		public static IEnumerable<TSource> SelectRecursive<TSource>(
			this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> recursiveSelector)
		{
			//Util.RequireNotNull( source, "start" );
			//Util.RequireNotNull( recursiveSelector, "children" );

			Stack<TSource> stack = new Stack<TSource>();

			source.Reverse().ForEach( stack.Push );

			while( stack.Count > 0 )
			{
				TSource current = stack.Pop();

				recursiveSelector( current ).Reverse().ForEach( stack.Push );

				yield return current;

			} // while

		} //*** SelectRecursive

		/// <summary>
		///     Performs the specified <paramref name="action"/> to 
		///     on each element of the specified <paramref name="source"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <param name="source">The sequence to which is applied the specified <paramref name="action"/>.</param>
		/// <param name="action">The action applied to each element in <paramref name="source"/>.</param>
		public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
		{
			//Util.RequireNotNull( source, "source" );
			//Util.RequireNotNull( action, "action" );

			foreach( TSource item in source )
			{
				action( item );
			}
		}
	}
}