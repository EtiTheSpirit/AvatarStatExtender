using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using HarmonyInstance = HarmonyLib.Harmony;
using static XansTools.Data.Utils;
using Il2CppSLZ.Rig;
using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Bonelab;
using UnityEngine;
using Il2CppSLZ.Interaction;
using Il2CppSLZ.VRMK;

namespace XansTools.Data {

	/// <summary>
	/// A modification of BoneLib's player class.
	/// </summary>
	public sealed class Player {

		internal static void Initialize(HarmonyInstance harmony) {
			harmony.Patch(QuickGetMethod<RigManager>("Awake"), postfix: new HarmonyMethod(QuickGetMethod<Player>(nameof(OnRigManagerAwake))));
		}
		private static void OnRigManagerAwake(RigManager __instance) {
			RigMgr = __instance;
			PhysRig = __instance.physicsRig;
		}

#pragma warning disable IDE0031 // Disallowed in Unity code.
		/// <summary>
		/// The player's current avatar, or null if there is no loaded rig manager.
		/// </summary>
		public static Avatar? Avatar => RigMgr != null ? RigMgr.avatar : null;
#pragma warning restore IDE0031 // Disallowed in Unity code.

		/// <summary>
		/// The <see cref="RigManager"/> belonging to this player.
		/// </summary>
		public static RigManager? RigMgr { get; private set; }

		/// <summary>
		/// The <see cref="PhysicsRig"/> belonging to this player.
		/// </summary>
		public static PhysicsRig? PhysRig { get; private set; }

		/// <summary>
		/// Returns the Player's current <see cref="Avatar"/>, or <see langword="null"/> if no <see cref="RigManager"/> is available. 
		/// </summary>
		public static Avatar? GetCurrentAvatar() => RigMgr != null ? RigMgr.avatar : null;
	}
}
