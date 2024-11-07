#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvatarStatExtender.Tools {

	/// <summary>
	/// A substitution for the native ImmutableHashSet type, which is not available.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class ReadOnlyHashSet<T> : ISet<T> {
		private readonly HashSet<T> _set;

		/// <inheritdoc/>
		public int Count { get; }

		/// <inheritdoc/>
		public bool IsReadOnly { get; } = true;

		/// <summary>
		/// Create a new empty immutable set.
		/// </summary>
		public ReadOnlyHashSet() {
			_set = new HashSet<T>();
			Count = 0;
		}

		/// <summary>
		/// Create a new immutable set wrapping the provided collection.
		/// </summary>
		/// <param name="set"></param>
		public ReadOnlyHashSet(IEnumerable<T> set) {
			_set = new HashSet<T>(set);
			Count = _set.Count;
		}

		/// <summary>
		/// Returns true if this set contains at least one of the items in the provided array.
		/// </summary>
		/// <param name="of"></param>
		/// <returns></returns>
		public bool ContainsAny(T[] of) {
			for (int i = 0; i < of.Length; i++) {
				if (Contains(of[i])) return true;
			}
			return false;
		}
		/// <inheritdoc/>
		public bool Add(T item) => throw new NotSupportedException();
		/// <inheritdoc/>
		public void UnionWith(IEnumerable<T> other) => throw new NotSupportedException();
		/// <inheritdoc/>
		public void IntersectWith(IEnumerable<T> other) => throw new NotSupportedException();
		/// <inheritdoc/>
		public void ExceptWith(IEnumerable<T> other) => throw new NotSupportedException();
		/// <inheritdoc/>
		public void SymmetricExceptWith(IEnumerable<T> other) => throw new NotSupportedException();
		/// <inheritdoc/>
		public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);
		/// <inheritdoc/>
		public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);
		/// <inheritdoc/>
		public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);
		/// <inheritdoc/>
		public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);
		/// <inheritdoc/>
		public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);
		/// <inheritdoc/>
		public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);
		/// <inheritdoc/>
		void ICollection<T>.Add(T item) => throw new NotSupportedException();
		/// <inheritdoc/>
		public void Clear() => throw new NotSupportedException();
		/// <inheritdoc/>
		public bool Contains(T item) => _set.Contains(item);
		/// <inheritdoc/>
		public void CopyTo(T[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);
		/// <inheritdoc/>
		public bool Remove(T item) => throw new NotSupportedException();
		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();
		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
