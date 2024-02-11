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

		public int Length => _array.Length;

		public T this[int index] {
			get => _array[index];
		}

		public ReadOnlyReferenceSpan(T[] array) {
			_array = array;
		}

		public int IndexOf(T item) => Array.IndexOf(_array, item);

		public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _array.GetEnumerator();
	}
}
