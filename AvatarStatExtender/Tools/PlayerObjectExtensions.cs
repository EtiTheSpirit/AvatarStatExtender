#nullable enable
using AvatarStatExtender.Tools.Assets;
using SLZ.Rig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SLZAvatar = SLZ.VRMK.Avatar;
using UnityObject = UnityEngine.Object;

namespace AvatarStatExtender.Tools {

	/// <summary>
	/// Tools to help with player related things.
	/// </summary>
	public static class PlayerObjectExtensions {

		// Ordinarily I would avoid referencing by a string name for a GameObject, but Fusion code has
		// explicitly mentioned: "This should never change, incase other mods rely on it."
		// So that is exactly what I plan to do.
		private const string FUSION_PLAYER_REP_NAME = "[RigManager (FUSION PlayerRep)]";

		/// <summary>
		/// Returns the <see cref="RigManager"/>s belonging to all other players in a multiplayer server.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<RigManager> GetRemotePlayers() {
			return UnityObject.FindObjectsOfType<RigManager>().Where(rg => rg.gameObject.name == FUSION_PLAYER_REP_NAME);
		}

		/// <summary>
		/// Returns every player in the current scene, guaranteed to start with the local player. To get the local player,
		/// use <see cref="BoneLib.Player.rigManager"/>, not this.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<RigManager> GetAllPlayers() {
			yield return BoneLib.Player.rigManager;

			foreach (RigManager mgr in GetRemotePlayers()) {
				yield return mgr;
			}
		}

		/// <summary>
		/// Returns the <see cref="RigManager"/> associated with the provided avatar.
		/// </summary>
		/// <param name="avatar"></param>
		/// <returns></returns>
		public static RigManager? GetRigManager(this SLZAvatar avatar) {
			if (avatar.transform.parent == null) return null;
			return avatar.transform.parent.GetComponent<RigManager>();
		}

		/// <summary>
		/// Returns a list of all avatar <strong>prefabs</strong> that have been loaded
		/// into this scene, with multiplayer in mind. If two players are using the same avatar,
		/// then that avatar will only appear once in the result.
		/// <para/>
		/// This always begins with the local player's avatar.
		/// </summary>
		/// <returns></returns>
		public static SLZAvatar[] GetAllUniqueActiveAvatarPrefabs() {
			lock (_reusableUniqueAvatarSet) {
				_reusableUniqueAvatarSet.Clear();
				// Going to use this set because .Distinct allocates a new one

				IEnumerable<SLZAvatar> enumerable = GetAllPlayers()
					.Where(player => player.avatar != null)
					.Select(player => player.avatar.GetOriginalPrefab())
					.Where(avatar => avatar != null && _reusableUniqueAvatarSet.Add(avatar))!; 
					// ^ Null forgiving operator here. The Select line can select null avatars
					// but this Where statement prevents it (but the enumerator can't tell)
				return enumerable.ToArray();
			}
		}
		private static readonly HashSet<SLZAvatar> _reusableUniqueAvatarSet = new HashSet<SLZAvatar>(64);

		/// <summary>
		/// Returns a list of all avatars that are currently present in the scene that belong to players.
		/// <para/>
		/// This always begins with the local player's avatar.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<SLZAvatar> GetAllActiveAvatars() => GetAllPlayers().Where(player => player.avatar != null).Select(player => player.avatar);

		/// <summary>
		/// Returns the health belonging to this avatar, by accessing the <see cref="RigManager"/> (its parent object)
		/// and then resolving its <see cref="RigManager.health"/> as <see cref="Player_Health"/>.
		/// </summary>
		/// <param name="avatar"></param>
		/// <returns></returns>
		public static Player_Health? GetHealth(this SLZAvatar avatar) {
			RigManager? mgr = GetRigManager(avatar);
			if (mgr == null) return null;
			return mgr.health.Cast<Player_Health>();
		}

		/// <summary>
		/// Returns the avatar that this health belongs to, by grabbing it off of the <see cref="RigManager"/>.
		/// </summary>
		/// <param name="health"></param>
		/// <returns></returns>
		public static SLZAvatar? GetAvatar(this Player_Health health) {
			if (health._rigManager == null) return null;
			return health._rigManager.avatar;
		}
	}
}
