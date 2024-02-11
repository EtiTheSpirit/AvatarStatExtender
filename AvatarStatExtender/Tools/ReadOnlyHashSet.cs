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

		public int Count { get; }

		public bool IsReadOnly { get; } = true;

		public ReadOnlyHashSet() {
			_set = new HashSet<T>();
			Count = 0;
		}

		public ReadOnlyHashSet(IEnumerable<T> set) {
			_set = new HashSet<T>(set);
			Count = _set.Count;
		}

		public bool ContainsAny(T[] of) {
			for (int i = 0; i < of.Length; i++) {
				if (Contains(of[i])) return true;
			}
			return false;
		}

		public bool Add(T item) => throw new NotSupportedException();

		public void UnionWith(IEnumerable<T> other) => throw new NotSupportedException();

		public void IntersectWith(IEnumerable<T> other) => throw new NotSupportedException();

		public void ExceptWith(IEnumerable<T> other) => throw new NotSupportedException();

		public void SymmetricExceptWith(IEnumerable<T> other) => throw new NotSupportedException();

		public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);

		public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);

		public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);

		public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);

		public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);

		public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);

		void ICollection<T>.Add(T item) => throw new NotSupportedException();

		public void Clear() => throw new NotSupportedException();

		public bool Contains(T item) => _set.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);

		public bool Remove(T item) => throw new NotSupportedException();

		public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
