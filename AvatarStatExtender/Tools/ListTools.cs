#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvatarStatExtender.Tools {

	/// <summary>
	/// Provides list utilities.
	/// </summary>
	public static class ListTools {

		/// <summary>
		/// Returns an immutably read-only list, in that it is not a wrapper; it is truly read only,
		/// and flat out <em>doesn't have</em> an inner collection.
		/// <para/>
		/// This qualifies as <see cref="IList{T}"/> and <see cref="IReadOnlyList{T}"/> together.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IReadOnlyList<T> EmptyReadOnly<T>() => StubbedListImpl<T>.INSTANCE;

		private sealed class StubbedListImpl<T> : IList<T>, IReadOnlyList<T> {

			public static readonly IReadOnlyList<T> INSTANCE = new StubbedListImpl<T>();

			private static readonly EmptyEnumerator EMPTY_ENUMERATOR = new EmptyEnumerator();

			public T this[int index] => throw new IndexOutOfRangeException();

			T IList<T>.this[int index] {
				get => throw new IndexOutOfRangeException();
				set => throw new NotSupportedException();
			}

			public int Count { get; } = 0;

			public bool IsReadOnly { get; } = true;

			public IEnumerator<T> GetEnumerator() => EMPTY_ENUMERATOR;

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			public int IndexOf(T item) => -1;

			public void Insert(int index, T item) => throw new NotSupportedException();

			public void RemoveAt(int index) => throw new NotSupportedException();

			public void Add(T item) => throw new NotSupportedException();

			public void Clear() => throw new NotSupportedException();

			public bool Contains(T item) => false;

			public void CopyTo(T[] array, int arrayIndex) {
				if (arrayIndex >= array.Length) throw new ArgumentOutOfRangeException();
				// That's literally it.
			}

			public bool Remove(T item) => throw new NotSupportedException();

			private sealed class EmptyEnumerator : IEnumerator<T> {

				[AllowNull]
				public T Current { get; } = default;

				[AllowNull]
				object IEnumerator.Current => Current!;

				public void Dispose() { }

				public bool MoveNext() => false;

				public void Reset() { }

			}
		}
	}


}
