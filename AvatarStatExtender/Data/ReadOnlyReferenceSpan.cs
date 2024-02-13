using System;
#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvatarStatExtender.Data {

	/// <summary>
	/// Provides a read only window into an array.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class ReadOnlyReferenceSpan<T> : IEnumerable<T> {

		private readonly T[] _array;

		/// <summary>
		/// The length of the array.
		/// </summary>
		public int Length => _array.Length;

		/// <summary>
		/// Access an element of the array by its index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public T this[int index] {
			get => _array[index];
		}

		/// <summary>
		/// Create a new wrapper around the provided array.
		/// </summary>
		/// <param name="array"></param>
		public ReadOnlyReferenceSpan(T[] array) {
			_array = array;
		}

		/// <summary>
		/// Returns the numeric index of the given <paramref name="item"/> in this array, or <c>-1</c> if no such
		/// item exist in this array.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int IndexOf(T item) => Array.IndexOf(_array, item);

		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => _array.GetEnumerator();
	}
}
