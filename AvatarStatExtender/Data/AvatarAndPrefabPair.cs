#nullable enable
using AvatarStatExtender.Tools.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SLZAvatar = SLZ.VRMK.Avatar;

namespace AvatarStatExtender.Data {

	/// <summary>
	/// A utility data container that contains both an avatar's runtime clone (or, "you"), and the avatar's master prefab (the crate).
	/// </summary>
	public readonly ref struct AvatarAndPrefabPair {

		/// <summary>
		/// The original prefab of the avatar, as stored in the crate. This object is kept alive.
		/// </summary>
		public readonly SLZAvatar prefab;

		/// <summary>
		/// The clone of the avatar as it appears in runtime (the player model).
		/// </summary>
		public readonly SLZAvatar clone;

		/// <summary>
		/// Construct a new pair using the already provided prefab and clone.
		/// </summary>
		/// <param name="prefab"></param>
		/// <param name="clone"></param>
		/// <exception cref="ArgumentException">If the prefab isn't a prefab, or the clone isn't a clone.</exception>
		public AvatarAndPrefabPair(SLZAvatar prefab, SLZAvatar clone) {
			if (!prefab.IsPrefabAvatar()) throw new ArgumentException("The provided prefab was not actually a prefab; it seems to be a runtime instance.", nameof(prefab));
			if (clone.IsPrefabAvatar()) throw new ArgumentException("The provided clone was actually a prefab; it seems to be a stored crate.", nameof(clone));
			this.prefab = prefab;
			this.clone = clone;
		}

		/// <summary>
		/// Construct a new pair using the provided clone. The prefab is located from the clone.
		/// </summary>
		/// <param name="clone"></param>
		/// <exception cref="ArgumentException">The clone is a prefab.</exception>
		/// <exception cref="NullReferenceException">The prefab was not able to be located.</exception>
		public AvatarAndPrefabPair(SLZAvatar clone) {
			if (clone.IsPrefabAvatar()) throw new ArgumentException("The provided clone was actually a prefab; it seems to be a stored crate.", nameof(clone));
			SLZAvatar? prefab = clone.GetOriginalPrefab();
			if (prefab == null) throw new NullReferenceException($"Failed to resolve the original prefab from the provided clone avatar {clone.name}");
			this.prefab = prefab;
			this.clone = clone;
		}

	}
}
