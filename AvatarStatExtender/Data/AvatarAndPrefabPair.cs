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

		public readonly SLZAvatar prefab;

		public readonly SLZAvatar clone;

		public AvatarAndPrefabPair(SLZAvatar prefab, SLZAvatar clone) {
			if (!prefab.IsPrefabAvatar()) throw new ArgumentException("The provided prefab was not actually a prefab; it seems to be a runtime instance.", nameof(prefab));
			if (clone.IsPrefabAvatar()) throw new ArgumentException("The provided clone was actually a prefab; it seems to be a stored crate.", nameof(clone));
			this.prefab = prefab;
			this.clone = clone;
		}

		public AvatarAndPrefabPair(SLZAvatar clone) {
			if (clone.IsPrefabAvatar()) throw new ArgumentException("The provided clone was actually a prefab; it seems to be a stored crate.", nameof(clone));
			SLZAvatar? prefab = clone.GetOriginalPrefab();
			if (prefab == null) throw new NullReferenceException($"Failed to resolve the original prefab from the provided clone avatar {clone.name}");
			this.prefab = prefab;
			this.clone = clone;
		}

	}
}
