using BoneLib;
using ExtendedAvatarStatDriver.Components;
using MelonLoader;
using SLZ.Rig;
using SLZ.VRMK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MelonLoader.MelonLogger;
using SLZAvatar = SLZ.VRMK.Avatar;

namespace AvatarStatExtender.Tools {

	/// <summary>
	/// The core class responsible for performing changes to the avatar information.
	/// </summary>
	public sealed class StatMarshaller {

		internal static void Initialize(HarmonyLib.Harmony harmony) {
			Log.Info("Patching SLZ::VRMK::Avatar::ComputeBaseStats...");
			harmony.Patch(
				QuickGetMethod<SLZAvatar>(nameof(SLZAvatar.ComputeBaseStats)),
				postfix: new HarmonyLib.HarmonyMethod(QuickGetMethod<StatMarshaller>(nameof(ComputeStatsPostfix)))
			);
			Log.Info("Patching SLZ::VRMK::Avatar::ComputeMass...");
			harmony.Patch(
				QuickGetMethod<SLZAvatar>(nameof(SLZAvatar.ComputeMass)),
				postfix: new HarmonyLib.HarmonyMethod(QuickGetMethod<StatMarshaller>(nameof(ComputeMassPostfix)))
			);
			Log.Info("Patching SLZ::Rig::RemapRig::JumpCharge...");
			harmony.Patch(
				QuickGetMethod<RemapRig>(nameof(RemapRig.JumpCharge)),
				postfix: new HarmonyLib.HarmonyMethod(QuickGetMethod<StatMarshaller>(nameof(JumpChargePostfix)))
			);
			Log.Info("Patches performed.");
		}

		private static void ComputeStatsPostfix(SLZAvatar __instance) => ApplyStats(__instance);

		private static void ComputeMassPostfix(SLZAvatar __instance) => ApplyStats(__instance);

		private static void JumpChargePostfix(bool chargeInput = true) {
			SLZAvatar avatar = Player.GetCurrentAvatar();
			OnJump(avatar, chargeInput);
		}

		private static void OnJump(SLZAvatar avatar, bool isJumpButtonDown) {
			AvatarStatDriver provider = avatar.gameObject.GetComponent<AvatarStatDriver>();
			if (provider == null) return;

			// Use a component for multiplayer networking.
			JumpTracker jumpCaps = avatar.GetComponent<JumpTracker>();
			if (jumpCaps == null) { 
				jumpCaps = avatar.gameObject.AddComponent<JumpTracker>();
			}

			PhysicsRig pRig = Player.GetPhysicsRig();
			PhysGrounder pGnd = pRig.physG;
			jumpCaps.MarkPlayerOnGround(pGnd.isGrounded);
			if (jumpCaps.TryJump(isJumpButtonDown)) {
				pRig.torso.rbPelvis.AddForce(Vector3.up * provider.extraJumpVerticalVelocity, ForceMode.VelocityChange);
			}
		}

		private static void ApplyStats(SLZAvatar avatar) {
			AvatarStatDriver provider = avatar.gameObject.GetComponent<AvatarStatDriver>();
			if (provider == null) return;

			Log.Debug("Enforcing that stats are applied to the desired avatar.");
			avatar._agility = provider.EffectiveAgility;
			avatar._speed = provider.EffectiveSpeed;
			avatar._strengthUpper = provider.EffectiveUpperStrength;
			avatar._strengthLower = provider.EffectiveLowerStrength;
			avatar._vitality = provider.EffectiveVitality;
			avatar._intelligence = provider.EffectiveIntelligence;

			avatar._massChest = provider.EffectiveChestMass;
			avatar._massPelvis = provider.EffectivePelvisMass;
			avatar._massLeg = provider.EffectiveLegMass;
			avatar._massArm = provider.EffectiveArmMass;
			avatar._massHead = provider.EffectiveHeadMass;
			avatar._massTotal = provider.EffectiveTotalMass;
			Log.Debug($"Stats and masses have been applied to {avatar.name}");
		}

		private static MethodInfo QuickGetMethod<T>(string name) {
			return typeof(T).GetMethod(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

	}
}
