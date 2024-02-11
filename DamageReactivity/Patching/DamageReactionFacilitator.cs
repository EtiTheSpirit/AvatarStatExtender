#nullable enable
using XansTools.Data;
using SLZ.AI;
using SLZ.Combat;
using SLZ.Marrow.Data;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using System.Threading;
using XansTools.AvatarInteroperability;

namespace XansTools.Patching {

	/// <summary>
	/// Provides events that allow mods to intercept and react to taking damage, be it a player or an npc.
	/// <para/>
	/// <strong>Consider using <see cref="DamageReceptionHelper"/>.</strong> It joins the two events of this
	/// class into one and handles a bunch of weird edge cases for you.
	/// </summary>
	internal static class DamageReactionFacilitator {

		/// <summary>The name of the field storing the function pointer for Player_Health.OnReceivedDamage</summary>
		const string METHOD_PTR_FLD_NAME_PLAYER = "NativeMethodInfoPtr_OnReceivedDamage_Public_Virtual_Void_Attack_BodyPart_0";

		/// <summary>The name of the field storing the function pointer for Enemy_Health.OnReceivedDamage</summary>
		const string METHOD_PTR_FLD_NAME_ENEMY = "NativeMethodInfoPtr_OnReceivedDamage_Public_Void_Attack_BodyPart_0";

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

		/*
		/// <summary>
		/// This event fires when an NPC takes any sort of damage.
		/// <para/>
		/// It contains the attack responsible for the damage as well as the body part that it hit. For more specific body parts, the <see cref="AttackInfo"/>
		/// includes the collider that was impacted.
		/// <para/>
		/// <strong>This event fires twice!</strong> Pay attention to the <c>phase</c> parameter, which indicates if this event has fired before or after
		/// the attack occurred. Receivers can modify the incoming <c>attack</c> parameter to change values like its damage. Note that the original game
		/// method does changes to the damage and so early receivers (which execute <em>before</em> the game runs its code) will <strong>not</strong> see 
		/// things like damage bonuses from headshots.
		/// </summary>
		public static event EnemyDamageTakenDelegate? OnEnemyDamageTaken;
		public delegate void EnemyDamageTakenDelegate(Enemy_Health @this, in ImmutableAttackInfo originalAttack, ref AttackInfo attack, ref PlayerDamageReceiver.BodyPart part, EventPhase phase);
		*/
		private static CommonDamageTakenDelegate? _originalPlayerDamageTaken;
		// private static CommonDamageTakenDelegate? _originalEnemyDamageTaken;
		private static CommonTAKEDAMAGEDelegate? _originalPlayerTAKEDAMAGE;
		private delegate void CommonDamageTakenDelegate(IntPtr @this, IntPtr attack, PlayerDamageReceiver.BodyPart part, IntPtr method);
		private delegate void CommonTAKEDAMAGEDelegate(IntPtr @this, float damage, IntPtr method);


		//[MemberNotNull(nameof(_originalPlayerDamageTaken), nameof(_originalPlayerTAKEDAMAGE))]
		internal static unsafe void Patch() {
			Log.Info("Damage Reaction Facilitator is performing patches.");
			CommonDamageTakenDelegate plrDamagePatch = OnPlayerDamageReceived;
			//CommonDamageTakenDelegate enemyDamagePatch = OnEnemyDamageReceived;
			CommonTAKEDAMAGEDelegate takedamageCalled = PlayerTAKEDAMAGECalled;

			_originalPlayerDamageTaken = Utils.NativeHookAttachFrom<Player_Health, CommonDamageTakenDelegate>(METHOD_PTR_FLD_NAME_PLAYER, plrDamagePatch);
			//_originalEnemyDamageTaken = Utils.NativeHookAttachFrom<Enemy_Health, CommonDamageTakenDelegate>(METHOD_PTR_FLD_NAME_ENEMY, enemyDamagePatch);
			_originalPlayerTAKEDAMAGE = Utils.NativeHookAttachFrom<Player_Health, CommonTAKEDAMAGEDelegate>(METHOD_PTR_FLD_NAME_PLAYER_TAKEDAMAGE, takedamageCalled);

		}

		private static unsafe void PlayerTAKEDAMAGECalled(IntPtr @this, float damage, IntPtr method) {
			Log.Trace($"Executing TAKEDAMAGE phase: Before...");
			Thread.BeginCriticalRegion(); // Is this even needed? I forgot if multithreaded execution is even possible.
										  // If it is, this basically says "hold your shit for a sec I gotta do something perfectly here, lemme cook rq"

			Player_Health healthObj = new Player_Health(@this);
			float originalDamage = damage;
			try {
				OnPlayerTAKEDAMAGECalled?.Invoke(healthObj, originalDamage, ref damage, EventPhase.Before);
			} catch (Exception exc) {
				Log.Error($"Failed to execute {nameof(OnPlayerTAKEDAMAGECalled)} phase: Before!");
				Log.Error(exc);
			}
			Log.Trace($"Invoking original TAKEDAMAGE...");
			_originalPlayerTAKEDAMAGE!(@this, damage, method);
			Log.Trace($"Executing TAKEDAMAGE phase: After...");
			try {
				OnPlayerTAKEDAMAGECalled?.Invoke(healthObj, originalDamage, ref damage, EventPhase.After);
			} catch (Exception exc) {
				Log.Error($"Failed to execute {nameof(OnPlayerTAKEDAMAGECalled)} phase: After!");
				Log.Error(exc);
			}

			Thread.EndCriticalRegion();
		}

		private static unsafe void OnPlayerDamageReceived(IntPtr @this, IntPtr attack, PlayerDamageReceiver.BodyPart part, IntPtr method) {
			Thread.BeginCriticalRegion(); // Is this even needed? I forgot if multithreaded execution is even possible.
										  // If it is, this basically says "hold your shit for a sec I gotta do something perfectly here, lemme cook rq"

			Player_Health healthObj = new Player_Health(@this);
			AttackInfo* atkPtr = (AttackInfo*)attack;
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
			_originalPlayerDamageTaken!(@this, (IntPtr)atkPtr, part, method);
			atk = *atkPtr; // In case any mods modify it here, we should re-evaluate the stored value.

			// And then the post-execution event.
			Log.Trace("Executing player damage phase: After...");
			try {
				OnPlayerDamageTaken?.Invoke(healthObj, dupe, ref atk, dupePart, ref part, EventPhase.After);
			} catch (Exception exc) {
				Log.Error($"Failed to execute {nameof(OnPlayerDamageTaken)} phase: After!");
				Log.Error(exc);
			}

			Thread.EndCriticalRegion();
		}

		// TO FUTURE PROGRAMMERS OR ME:
		// I did the enemy thing wrong. How? Didn't bother to check.
		// But when this is hooked it just stops all the other hooks from working. Probably some silly subtle mistake.
		// Enemy damage was insignificant to me though so /shrug

		/*
		private static unsafe void OnEnemyDamageReceived(IntPtr @this, IntPtr attack, PlayerDamageReceiver.BodyPart part, IntPtr method) {
			Enemy_Health healthObj = new Enemy_Health(@this);
			AttackInfo* atkPtr = (AttackInfo*)attack;
			AttackInfo atk = *atkPtr;
			AttackInfo dupeRaw = atk; // Copy by value
			ImmutableAttackInfo dupe = *(ImmutableAttackInfo*)(&dupeRaw);

			Log.Trace("Executing enemy damage phase: Before...");
			try {
				OnEnemyDamageTaken?.Invoke(healthObj, dupe, ref atk, ref part, EventPhase.Before);
			} catch (Exception exc) {
				Log.Error($"Failed to execute {nameof(OnEnemyDamageTaken)}!");
				Log.Error(exc);
			}
			*atkPtr = atk;
			// ^ This is needed because atk may be stored elsewhere, so this effectively
			// ensures that the data gets copied over no matter where it might be.

			// Now execute the original.
			Log.Trace("Executing original player damage method...");
			_originalEnemyDamageTaken!(@this, (IntPtr)atkPtr, part, method);
			atk = *atkPtr; // In case any mods modify it here, we should re-evaluate the stored value.

			// And then the post-execution event.
			Log.Debug("Executing enemy damage phase: After...");
			try {
				OnEnemyDamageTaken?.Invoke(healthObj, dupe, ref atk, ref part, EventPhase.After);
			} catch (Exception exc) {
				Log.Error($"Failed to execute {nameof(OnEnemyDamageTaken)}!");
				Log.Error(exc);
			}
		}
		*/
		
	}
}
