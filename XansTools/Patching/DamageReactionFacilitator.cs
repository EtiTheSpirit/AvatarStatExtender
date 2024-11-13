#nullable enable
using XansTools.Data;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using System.Threading;
using XansTools.AvatarInteroperability;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Combat;
using MelonLoader.NativeUtils;

namespace XansTools.Patching {

	/// <summary>
	/// Provides events that allow mods to intercept and react to taking damage, be it a player or an npc.
	/// <para/>
	/// <strong>Consider using <see cref="DamageReceptionHelper.OnDamageTaken"/>.</strong> It joins the two events of this
	/// class into one and handles a bunch of weird edge cases for you.
	/// </summary>
	internal static class DamageReactionFacilitator {

		/// <summary>The name of the field storing the function pointer for Player_Health.OnReceivedDamage</summary>
		const string METHOD_PTR_FLD_NAME_PLAYER = "NativeMethodInfoPtr_OnReceivedDamage_Public_Virtual_Void_Attack_BodyPart_0";

		// <summary>The name of the field storing the function pointer for Enemy_Health.OnReceivedDamage</summary>
		//const string METHOD_PTR_FLD_NAME_ENEMY = "NativeMethodInfoPtr_OnReceivedDamage_Public_Void_Attack_BodyPart_0";

		/// <summary>The name of the field storing the function pointer for Player_Health.TAKEDAMAGE</summary>
		const string METHOD_PTR_FLD_NAME_PLAYER_TAKEDAMAGE = "NativeMethodInfoPtr_TAKEDAMAGE_Public_Virtual_Void_Single_0";

		/// <summary>
		/// This event fires when a player takes any sort of damage.
		/// <para/>
		/// It contains the attack responsible for the damage as well as the body part that it hit. For more specific body parts, the <see cref="AttackInfo"/>
		/// includes the collider that was impacted.
		/// <para/>
		/// <strong>This event fires twice!</strong> Pay attention to the <c>phase</c> parameter, which indicates if this event has fired before or after
		/// the attack occurred. Receivers can modify the incoming <c>attack</c> parameter to change values like its damage. Note that the original game
		/// method does changes to the damage and so early receivers (which execute <em>before</em> the game runs its code) will <strong>not</strong> see 
		/// things like damage bonuses from headshots.
		/// </summary>
		public static event PlayerDamageTakenDelegate? OnPlayerDamageTaken;
		public delegate void PlayerDamageTakenDelegate(Player_Health @this, in ImmutableAttackInfo originalAttack, ref AttackInfo attack, in PlayerDamageReceiver.BodyPart originalPart, ref PlayerDamageReceiver.BodyPart part, EventPhase phase);

		/// <summary>
		/// This event is called when <see cref="Player_Health.TAKEDAMAGE(float)"/> is called. This is called by OnReceivedDamage (see <see cref="OnPlayerDamageTaken"/>)
		/// but has, as should be obvious from its signature, far less context than 
		/// </summary>
		public static event PlayerTAKEDAMAGEDelegate? OnPlayerTAKEDAMAGECalled;
		public delegate void PlayerTAKEDAMAGEDelegate(Player_Health @this, in float originalAmount, ref float amount, EventPhase phase);

		private delegate void CommonDamageTakenDelegate(IntPtr @this, IntPtr attack, PlayerDamageReceiver.BodyPart part, IntPtr method);
		private delegate void CommonTAKEDAMAGEDelegate(IntPtr @this, float damage, IntPtr method);

		[AllowNull]
		private static NativeHook<CommonDamageTakenDelegate> _originalPlayerDamageTakenHook;
		[AllowNull]
		private static NativeHook<CommonTAKEDAMAGEDelegate> _originalPlayerTAKEDAMAGEHook;

		//[MemberNotNull(nameof(_originalPlayerDamageTaken), nameof(_originalPlayerTAKEDAMAGE))]
		internal static unsafe void Patch() {
			Log.Info("Damage Reaction Facilitator is performing patches.");
			CommonDamageTakenDelegate plrDamagePatch = OnPlayerDamageReceived;
			CommonTAKEDAMAGEDelegate takedamageCalled = PlayerTAKEDAMAGECalled;

			_originalPlayerDamageTakenHook = XansUtils.NativeHookAttachFrom<Player_Health, CommonDamageTakenDelegate>(METHOD_PTR_FLD_NAME_PLAYER, plrDamagePatch);
			_originalPlayerTAKEDAMAGEHook = XansUtils.NativeHookAttachFrom<Player_Health, CommonTAKEDAMAGEDelegate>(METHOD_PTR_FLD_NAME_PLAYER_TAKEDAMAGE, takedamageCalled);

		}

		private static unsafe void PlayerTAKEDAMAGECalled(IntPtr @this, float damage, IntPtr method) {
			if (false) {
				_originalPlayerTAKEDAMAGEHook.Trampoline(@this, damage, method); // Causes "Unmanaged method called from managed code"
				return;
			}
			

			Log.Trace($"Executing TAKEDAMAGE phase: Before...");
			
			Player_Health healthObj = new Player_Health(@this);
			float originalDamage = damage;
			try {
				OnPlayerTAKEDAMAGECalled?.Invoke(healthObj, originalDamage, ref damage, EventPhase.Before);
			} catch (Exception exc) {
				Log.Error($"Failed to execute {nameof(OnPlayerTAKEDAMAGECalled)} phase: Before!");
				Log.Error(exc);
			}
			Log.Trace($"Invoking original TAKEDAMAGE...");
			_originalPlayerTAKEDAMAGEHook.Trampoline(@this, damage, method);
			Log.Trace($"Executing TAKEDAMAGE phase: After...");
			try {
				OnPlayerTAKEDAMAGECalled?.Invoke(healthObj, originalDamage, ref damage, EventPhase.After);
			} catch (Exception exc) {
				Log.Error($"Failed to execute {nameof(OnPlayerTAKEDAMAGECalled)} phase: After!");
				Log.Error(exc);
			}
			Log.Trace("Exiting TAKEDAMAGE event code.");
		}

		private static unsafe void OnPlayerDamageReceived(IntPtr @this, IntPtr attack, PlayerDamageReceiver.BodyPart part, IntPtr method) {
			if (false) {
				_originalPlayerDamageTakenHook.Trampoline(@this, attack, part, method);
				return;
			}

			Player_Health healthObj = new Player_Health(@this);
			AttackInfo* atkPtr = (AttackInfo*)attack;
			Log.Trace(atkPtr->ToString());

			AttackInfo atk = *atkPtr;
			AttackInfo dupeRaw = atk; // Copy by value
			ImmutableAttackInfo dupe = *(ImmutableAttackInfo*)(&dupeRaw);
			PlayerDamageReceiver.BodyPart dupePart = part;

			Log.Trace("Executing player damage phase: Before...");
			try {
				OnPlayerDamageTaken?.Invoke(healthObj, dupe, ref atk, dupePart, ref part, EventPhase.Before);
			} catch (Exception exc) {
				Log.Error($"Failed to execute {nameof(OnPlayerDamageTaken)} phase: Before!");
				Log.Error(exc);
			}
			*atkPtr = atk;
			// ^ This is needed because atk may be stored elsewhere, so this effectively
			// ensures that the data gets copied over no matter where it might be.

			// Now execute the original.
			Log.Trace("Executing original player damage method...");
			_originalPlayerDamageTakenHook.Trampoline(@this, (IntPtr)atkPtr, part, method);
			atk = *atkPtr; // In case any mods modify it here, we should re-evaluate the stored value.

			// And then the post-execution event.
			Log.Trace("Executing player damage phase: After...");
			try {
				OnPlayerDamageTaken?.Invoke(healthObj, dupe, ref atk, dupePart, ref part, EventPhase.After);
			} catch (Exception exc) {
				Log.Error($"Failed to execute {nameof(OnPlayerDamageTaken)} phase: After!");
				Log.Error(exc);
			}
			Log.Trace("Exiting player damage event code.");
		}
		
	}
}
