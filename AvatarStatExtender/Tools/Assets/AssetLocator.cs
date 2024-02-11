#nullable enable
using AvatarStatExtender.Components;
using SLZ.Marrow.Pool;
using SLZ.Marrow.Warehouse;
using SLZ.Rig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using SLZAvatar = SLZ.VRMK.Avatar;

namespace AvatarStatExtender.Tools.Assets {

	/// <summary>
	/// Functions that assist with loading assets or finding information about them.
	/// </summary>
	public static class AssetLocator {

		/// <summary>
		/// Analyzes the provided avatar to determine if it is a prefab. To do so, it checks to see if its scene is
		/// a null scene.
		/// </summary>
		/// <param name="avatar"></param>
		/// <returns></returns>
		public static bool IsPrefabAvatar(this SLZAvatar avatar) {
			return avatar.gameObject.scene != SceneManager.GetActiveScene() && !avatar.gameObject.scene.IsValid();
		}

		/// <summary>
		/// An alternative method to find an avatar's crate by searching the entire warehouse.
		/// <para/>
		/// While not ideal, this is done because of the async nature of crate loading; the <see cref="RigManager"/>
		/// does not have its <see cref="RigManager.AvatarCrate"/> property set early enough due to it being
		/// updated in a coroutine, so the crate is set to the default (polyblank) rather than the current avatar
		/// when querying it.
		/// </summary>
		/// <param name="avatar"></param>
		/// <returns></returns>
		private static SLZAvatar? BruteForceFindCrateOfAvatar(SLZAvatar avatar) {
			if (avatar.IsPrefabAvatar()) {
				// Shortcut: We can tell that the provided object is a prefab because it exists in the void or something.
				// Honestly that kind of makes some level of sense. Neat.
				return avatar;
			}
			
			// Backup shortcut: Instances created from the prefab that are scanned here will have this component added to them.
			// If the instance passed in has this component, that means it has been used in this method before, and thus the
			// prefab is cached as a reference for quick access.
			QuickPrefabIdentifier? identifier = null;
			if (avatar.GetComponent<QuickPrefabIdentifier>() is QuickPrefabIdentifier identifierCoalesced && identifierCoalesced != null) {
				identifier = identifierCoalesced;
				if (identifier.Prefab != null) return identifier.Prefab;
			}

			// At this point we are genuinely unsure, gotta do it the long way.
			Log.Trace($"Searching the warehouse for the crate of {avatar.name}...");
			Animator avyAnim = avatar.GetComponent<Animator>();
			AvatarCrate[] crates = AssetWarehouse.Instance.GetCrates<AvatarCrate>().ToArray();
			for (int i = 0; i < crates.Length; i++) {
				AvatarCrate crate = crates[i];
				if (crate.MainGameObject != null && crate.MainGameObject.Asset is GameObject go && go != null) {
					if (go.GetComponent<Animator>() is Animator animator && animator != null) {
						if (animator.avatar == avyAnim.avatar) {
							Log.Trace($"Found the prefab! I'll cache it now for this clone.");
							SLZAvatar result = go.GetComponent<SLZAvatar>();
							if (identifier == null) {
								identifier = avatar.gameObject.AddComponent<QuickPrefabIdentifier>();
							}
							identifier.Prefab = result;
							return result;
						}
					}
				}
			}
			Log.Trace($"No luck, chief.");
			return null;
		}

		/// <summary>
		/// Provided with an avatar instance, this will return the original prefab for this avatar that all
		/// future instances are cloned from. Returns null if it cannot find the prefab.
		/// </summary>
		/// <param name="avatar"></param>
		/// <returns></returns>
		public static SLZAvatar? GetOriginalPrefab(this SLZAvatar avatar) {
#if true
			// For the reason why I do this instead of the other block of code below, read the docs of
			// this method:
			return BruteForceFindCrateOfAvatar(avatar);
#else
			AvatarCrateReference crateRef = avatar.GetRigManager().AvatarCrate;
			if (crateRef.IsValid()) {
				AvatarCrate avyCrate = crateRef.Crate;
				Log.Trace($"Avatar {avatar.name} has a crate: {avyCrate.name}.");
				if (avyCrate.MainAsset != null && avyCrate.MainAsset.Asset != null) {
					Log.Trace("Crate's main asset is present entirely.");
					GameObject go = (GameObject)avyCrate.MainAsset.Asset;
					if (go != null) {
						SLZAvatar potential = go.GetComponent<SLZAvatar>();
						Log.Trace($"Resolved avatar as {(potential ? potential.name : "null")}.");
						if (potential != null) return potential;
					} else {
						Log.Trace("The main asset was not a GameObject.");
					}
				} else {
					Log.Trace($"MISSING CRATE ASSET! MainAsset: {avyCrate.MainAsset} (if this is not null, then MainAsset.Asset is null).");
				}
			}
			return avatar;
#endif
		}
	}
}
