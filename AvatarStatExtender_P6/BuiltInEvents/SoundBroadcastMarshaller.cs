#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XansTools.AvatarInteroperability;
using SLZAvatar = Il2CppSLZ.VRMK.Avatar;
using XansTools.Data;
using Il2CppSLZ.Marrow.Data;
using AvatarStatExtender.API;
using AvatarStatExtender.Data;
using AvatarStatExtender.Tools;
using HarmonyLib;
using AvatarStatExtender.Tools.Assets;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using static Il2CppSLZ.Marrow.PlayerDamageReceiver;

namespace AvatarStatExtender.BuiltInEvents {
	internal sealed class SoundBroadcastMarshaller {

		internal static void Initialize(HarmonyLib.Harmony harmony) {
			Log.Info("Initializing the sound broadcast marshaller.");
			DamageReceptionHelper.OnDamageTaken += OnDamageTaken;
			harmony.Patch(
				XansUtils.QuickGetMethod<RigManager>(nameof(RigManager.SwitchAvatar)),
				postfix: new HarmonyMethod(XansUtils.QuickGetMethod<SoundBroadcastMarshaller>(nameof(AfterSwitchingAvatar)))
			);
		}


		private static void OnDamageTaken(Player_Health health, ref bool attackInfoAndPartAvailable, in ImmutableAttackInfo originalAttack, ref AttackInfo attack, in BodyPart originalPart, ref BodyPart part, EventPhase phase) {
			if (phase == EventPhase.After) {
				Log.Trace("Damage event phase, after.");
				if (attack.Damage > 0) {
					SLZAvatar? victim = health.GetAvatar();
					if (victim == null) {
						Log.Error($"Player damage was reported however there was no known victim (the health object does not belong to an avatar?) Name: {health.name}");
					} else {
						if (attack.DamageType.HasFlag(AttackType.Piercing)) {
							Log.Trace($"Broadcasting hit bullet impact sound to {victim.name}.");
							SoundAPI.BroadcastBuiltInSoundEvent(AudioEventType.HitBullet, victim);
						} else {
							// n.b. this only works for TAKEDAMAGE because AttackType will be 0, and that satisfies this condition.
							Log.Trace($"Broadcasting hit non-bullet impact sound to {victim.name}.");
							SoundAPI.BroadcastBuiltInSoundEvent(AudioEventType.HitBlunt, victim);
						}
						Log.Trace($"Broadcasting general hit sound to {victim.name}.");
						SoundAPI.BroadcastBuiltInSoundEvent(AudioEventType.HitAny, victim);
					}
				}
			}
		}

		private static void AfterSwitchingAvatar(SLZAvatar newAvatar) {
			if (newAvatar == null) return;
			if (newAvatar.GetRigManager() == null) return;
			if (newAvatar.IsPrefabAvatar()) return;
			Log.Info($"Broadcasting spawn sound to {newAvatar.name}.");
			SoundAPI.BroadcastBuiltInSoundEvent(AudioEventType.Spawn, newAvatar);
		}

	}
}
