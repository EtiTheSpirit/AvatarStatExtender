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

namespace XansTools.Patching {

	/// <summary>
	/// Provides events that allow mods to intercept and react to taking damage, be it a player or an npc.
	/// </summary>
	public static class DamageReactionFacilitator {

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
		public delegate void PlayerDamageTakenDelegate(Player_Health @this, in ImmutableAttackInfo originalAttack, ref AttackInfo attack, ref PlayerDamageReceiver.BodyPart part, EventPhase phase);

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
		private static CommonDamageTakenDelegate? _originalEnemyDamageTaken;
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
			Player_Health healthObj = new Player_Health(@this);
			float originalDamage = damage;
			try {
				OnPlayerTAKEDAMAGECalled?.Invoke(healthObj, originalDamage, ref damage, EventPhase.Before);
			} catch (Exception exc) {
				Log.Error($"Failed to execute {nameof(OnPlayerTAKEDAMAGECalled)}!");
				Log.Error(exc);
			}
			Log.Trace($"Invoking original TAKEDAMAGE...");
			_originalPlayerTAKEDAMAGE!(@this, damage, method);
			Log.Trace($"Executing TAKEDAMAGE phase: After...");
			try {
				OnPlayerTAKEDAMAGECalled?.Invoke(healthObj, originalDamage, ref damage, EventPhase.After);
			} catch (Exception exc) {
				Log.Error($"Failed to execute {nameof(OnPlayerTAKEDAMAGECalled)}!");
				Log.Error(exc);
			}
		}

		private static unsafe void OnPlayerDamageReceived(IntPtr @this, IntPtr attack, PlayerDamageReceiver.BodyPart part, IntPtr method) {
			Player_Health healthObj = new Player_Health(@this);
			AttackInfo* atkPtr = (AttackInfo*)attack;
			AttackInfo atk = *atkPtr;
			AttackInfo dupeRaw = atk; // Copy by value
			ImmutableAttackInfo dupe = *(ImmutableAttackInfo*)(&dupeRaw);

			Log.Trace("Executing player damage phase: Before...");
			try {
				OnPlayerDamageTaken?.Invoke(healthObj, dupe, ref atk, ref part, EventPhase.Before);
			} catch (Exception exc) {
				Log.Error($"Failed to execute {nameof(OnPlayerDamageTaken)}!");
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
				OnPlayerDamageTaken?.Invoke(healthObj, dupe, ref atk, ref part, EventPhase.After);
			} catch (Exception exc) {
				Log.Error(exc);
			}
		}
		/*
		private static unsafe void OnEnemyDamageReceived(IntPtr @this, IntPtr attack, PlayerDamageReceiver.BodyPart part, IntPtr method) {
			Enemy_Health healthObj = new Enemy_Health(@this);
			AttackInfo* atkPtr = (AttackInfo*)attack;
			AttackInfo atk = *atkPtr;
			AttackInfo dupeRaw = atk; // Copy by value
			ImmutableAttackInfo dupe = *(ImmutableAttackInfo*)(&dupeRaw);

			Log.Trace("Executing player damage phase: Before...");
			try {
				OnEnemyDamageTaken?.Invoke(healthObj, dupe, ref atk, ref part, EventPhase.Before);
			} catch (Exception exc) {
				Log.Error($"Failed to execute {nameof(OnPlayerDamageTaken)}!");
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
			Log.Debug("Executing player damage phase: After...");
			try {
				OnEnemyDamageTaken?.Invoke(healthObj, dupe, ref atk, ref part, EventPhase.After);
			} catch (Exception exc) {
				Log.Error(exc);
			}
		}
		*/
		/// <summary>
		/// A recreation of the Attack data (see <see cref="Attack"/>) that fixes the data stored within.
		/// <para/>
		/// This is one of those cases of "I don't know why this fixes it, but it does, and if you use <see cref="Attack"/> it breaks".
		/// I suspect this is caused by some fuckery with <see cref="_backFacing"/> being byte vs. bool and how that plays with
		/// pointer garbage. Will have to research more, but what matters is that this struct fixes it.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct AttackInfo {
			private float _damage;
			private Vector3 _normal;
			private Vector3 _origin;
			private Vector3 _direction;
			private byte _backFacing;
			private int _orderInPool;
			private IntPtr _collider;
			private AttackType _attackType;
			private IntPtr _proxy;

			/// <summary>
			/// The absolute amount of damage this attack did.
			/// </summary>
			public float Damage {
				get => _damage;
				set => _damage = value;
			}

			/// <summary>
			/// The normal of the surface that this attack hit.
			/// </summary>
			public Vector3 Normal {
				get => _normal;
				set => _normal = value;
			}

			/// <summary>
			/// The direction this attack came from.
			/// </summary>
			public Vector3 Direction {
				get => _direction;
				set => _direction = value;
			}

			/// <summary>
			/// The origin of this attack source. Can be combined with direction to create a ray.
			/// </summary>
			public Vector3 Origin {
				get => _origin;
				set => _origin = value;
			}

			/// <summary>
			/// If true, the attack occurred from within the hit area.
			/// </summary>
			public bool IsBackFacing {
				get => _backFacing == 1;
				set => _backFacing = (byte)(value ? 1 : 0);
			}

			public int OrderInAttackPool {
				get => _orderInPool;
				set => _orderInPool = value;
			}

			/// <summary>
			/// The collider that the attack impacted with to result in this damage occurring.
			/// </summary>
			public Collider Collider {
				get => new Collider(_collider);
				set => _collider = value.Pointer;
			}

			/// <summary>
			/// What type(s) of damage this attack has on it.
			/// </summary>
			public AttackType DamageType {
				get => _attackType;
				set => _attackType = value;
			}


			public TriggerRefProxy Proxy {
				get => new TriggerRefProxy(_proxy);
				set => _proxy = value.Pointer;
			}

			/// <summary>
			/// Provide an <see cref="Attack"/> instance to return a new instance of this struct. This will fix values like the <see cref="_attackType"/>.
			/// </summary>
			/// <param name="attack"></param>
			/// <returns></returns>
			public static AttackInfo Fix(Attack attack) {
				unsafe {
					AttackInfo* atk = (AttackInfo*)attack.Pointer;
					return *atk;
				}
			}

			/// <summary>
			/// Provide an <see cref="Attack"/> instance to return a new instance of this struct. This will fix values like the <see cref="_attackType"/>.
			/// </summary>
			/// <param name="attack"></param>
			/// <returns></returns>
			public static AttackInfo From(IntPtr attack) {
				unsafe {
					AttackInfo* atk = (AttackInfo*)attack;
					return *atk;
				}
			}
		}

		/// <summary>
		/// The same as <see cref="AttackInfo"/> but it is immutable; it cannot be changed.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public readonly struct ImmutableAttackInfo {
			private readonly float _damage;
			private readonly Vector3 _normal;
			private readonly Vector3 _origin;
			private readonly Vector3 _direction;
			private readonly byte _backFacing;
			private readonly int _orderInPool;
			private readonly IntPtr _collider;
			private readonly AttackType _attackType;
			private readonly IntPtr _proxy;

			/// <summary>
			/// The absolute amount of damage this attack did.
			/// </summary>
			public float Damage {
				get => _damage;
			}

			/// <summary>
			/// The normal of the surface that this attack hit.
			/// </summary>
			public Vector3 Normal {
				get => _normal;
			}

			/// <summary>
			/// The direction this attack came from.
			/// </summary>
			public Vector3 Direction {
				get => _direction;
			}

			/// <summary>
			/// The origin of this attack source. Can be combined with direction to create a ray.
			/// </summary>
			public Vector3 Origin {
				get => _origin;
			}

			/// <summary>
			/// If true, the attack occurred from within the hit area.
			/// </summary>
			public bool IsBackFacing {
				get => _backFacing == 1;
			}

			public int OrderInAttackPool {
				get => _orderInPool;
			}

			/// <summary>
			/// The collider that the attack impacted with to result in this damage occurring.
			/// </summary>
			public Collider Collider {
				get => new Collider(_collider);
			}

			/// <summary>
			/// What type(s) of damage this attack has on it.
			/// </summary>
			public AttackType DamageType {
				get => _attackType;
			}


			public TriggerRefProxy Proxy {
				get => new TriggerRefProxy(_proxy);
			}
		}
	}
}
