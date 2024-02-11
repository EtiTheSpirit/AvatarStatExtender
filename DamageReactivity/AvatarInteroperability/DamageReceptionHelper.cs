using MelonLoader;
using SLZ.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XansTools.Data;
using XansTools.Patching;

namespace XansTools.AvatarInteroperability {

	/// <summary>
	/// This object helps to handle receiving and coalescing damage events from the <see cref="DamageReactionFacilitator"/>, which
	/// fires for verbose <see cref="Player_Health.OnReceivedDamage(Attack, PlayerDamageReceiver.BodyPart)"/> calls <em>and</em>
	/// general <see cref="Player_Health.TAKEDAMAGE(float)"/> calls, providing a single event that allows early intercepting
	/// (and/or reacting after) damage has occurred to the player, in a convenient and simple singular event.
	/// </summary>
	public static class DamageReceptionHelper {

		private static bool _isConnectedToEvents = false;

		/// <summary>
		/// This stack helps prevent re-entry 
		/// </summary>
		private static readonly Stack<Player_Health> _isAboutToExecuteTAKEDAMAGEFromRecv = new Stack<Player_Health>();

		/// <summary>
		/// Subscribe to this event to receive damage notifications.
		/// <para/>
		/// <strong>IMPORTANT:</strong> This event fires <strong>twice</strong>, once before, where the <c>attack</c> parameter can optionally be edited,
		/// and once after. You can tell which phase you are in via the <see cref="EventPhase"/> parameter.
		/// <para/>
		/// This event fires for both <see cref="Player_Health.TAKEDAMAGE(float)"/> calls <em>and</em> <see cref="Player_Health.OnReceivedDamage(SLZ.Combat.Attack, PlayerDamageReceiver.BodyPart)"/>,
		/// so pay attention to the presence of <c>editableAttack</c> and its related parameters, which are <see langword="null"/> if something calls <see cref="Player_Health.TAKEDAMAGE(float)"/>.
		/// </summary>
		public static event DamageReceptionDelegate OnDamageTaken {
			add {
				if (value == null) throw new ArgumentNullException(nameof(value));
				if (!_delegates.Add(value)) throw new InvalidOperationException($"Delegate {value} is already subscribed; cannot subscribe it.");
				if (!_isConnectedToEvents) {
					_isConnectedToEvents = true;
					DamageReactionFacilitator.OnPlayerDamageTaken += OnContextualDamageTaken;
					DamageReactionFacilitator.OnPlayerTAKEDAMAGECalled += OnAmbiguousDamageTaken;
				}
			}
			remove {
				if (value == null) throw new ArgumentNullException(nameof(value));
				if (!_delegates.Remove(value)) throw new InvalidOperationException($"Delegate {value} was not subscribed; cannot unsubscribe it.");
				if (_isConnectedToEvents && _delegates.Count == 0) {
					_isConnectedToEvents = false;
					DamageReactionFacilitator.OnPlayerDamageTaken -= OnContextualDamageTaken;
					DamageReactionFacilitator.OnPlayerTAKEDAMAGECalled -= OnAmbiguousDamageTaken;
				}
			}
		}

		private static readonly HashSet<DamageReceptionDelegate> _delegates = new HashSet<DamageReceptionDelegate>();

		/// <summary>
		/// This delegate is used when receiving damage events.
		/// </summary>
		/// <param name="health">The health object of the person that took the damage.</param>
		/// <param name="attackInfoAndPartAvailable">If <see langword="true"/>, <paramref name="attack"/> (and its original copy) have additional data in them, more than just the damage.</param>
		/// <param name="originalAttack">The original copy of all attack data before events were invoked. Always includes the damage. This may be missing data even if <paramref name="attackInfoAndPartAvailable"/> is <see langword="true"/> (which occurs if someone sets it).</param>
		/// <param name="attack">The attack data as it was since the last event handler got invoked. Only includes damage (and nothing else) when <paramref name="attackInfoAndPartAvailable"/> is <see langword="false"/>.</param>
		/// <param name="originalPart">The original body part that the attack hit before events were invoked.</param>
		/// <param name="part">The body part that was hit, as of the last event handler that got invoked.</param>
		/// <param name="phase">When this event is executing, either <see cref="EventPhase.Before"/> the original game method or <see cref="EventPhase.After"/>.</param>
		public delegate void DamageReceptionDelegate(
			Player_Health health,
			ref bool attackInfoAndPartAvailable,
			in ImmutableAttackInfo originalAttack, 
			ref AttackInfo attack, 
			in PlayerDamageReceiver.BodyPart originalPart, // To future Xan: This can be 0/default which represents no part since its flags.
			ref PlayerDamageReceiver.BodyPart part, 
			EventPhase phase
		);

		private static void ExecuteAllDelegates(Player_Health health, bool attackInfoAndPartAvailable, in ImmutableAttackInfo originalAttack, ref AttackInfo attack, in PlayerDamageReceiver.BodyPart originalPart, ref PlayerDamageReceiver.BodyPart part, EventPhase phase) {
			foreach (DamageReceptionDelegate del in _delegates) {
				AttackInfo attackBackup = attack;
				PlayerDamageReceiver.BodyPart partBackup = part;
				try {
					del.Invoke(
						health,
						ref attackInfoAndPartAvailable,
						originalAttack,
						ref attack,
						originalPart,
						ref part,
						phase
					);
				} catch (Exception ex) {
					// Undo any changes:
					part = partBackup;
					attack = attackBackup; // Reset iff it had a value that could be edited in the first place.

					MelonAssembly asm = MelonAssembly.GetMelonAssemblyOfMember(del.GetMethodInfo());
					if (asm != null) {
						Log.Error($"[Event: {nameof(OnDamageTaken)}] Delegate {del}, which is included in [{asm.Assembly}], raised an exception during execution.");
					} else {
						Log.Error($"[Event: {nameof(OnDamageTaken)}] Delegate {del} raised an exception during execution. This method is not part of a Melon, however, so your guess as to who did it is as good as mine.");
					}
					Log.Error(ex);
				}
			}
		}

		private static void OnContextualDamageTaken(Player_Health @this, in ImmutableAttackInfo originalAttack, ref AttackInfo attack, in PlayerDamageReceiver.BodyPart originalPart, ref PlayerDamageReceiver.BodyPart part, EventPhase phase) {
			// After execution of the first handler, where this variable was set, we need to unset it before anything else.
			if (phase == EventPhase.After) {
				if (_isAboutToExecuteTAKEDAMAGEFromRecv.Count > 0 && _isAboutToExecuteTAKEDAMAGEFromRecv.Peek() == @this) {
					_isAboutToExecuteTAKEDAMAGEFromRecv.Pop();
				}
			} else {
				// Catch case: Remove invalid elements. This should never happen.
				if (_isAboutToExecuteTAKEDAMAGEFromRecv.Count > 0 && _isAboutToExecuteTAKEDAMAGEFromRecv.Peek() == null) {
					Log.Error("DANGER: The stack to prevent damage function re-entry contained a destroyed Player_Health instance. " +
						"This should be impossible (unless someone destroyed it while taking damage?) and might cause serious bugs / failures in this event.");
					_isAboutToExecuteTAKEDAMAGEFromRecv.Pop();
				}
			}

			ExecuteAllDelegates(
				@this,
				true,
				originalAttack,
				ref attack,
				originalPart,
				ref part,
				phase
			);

			// Before exiting, do this.
			// OnDamageReceived calls TAKEDAMAGE
			// We want to cancel it iff it is executed on this instance of Player_Health
			if (phase == EventPhase.Before) {
				_isAboutToExecuteTAKEDAMAGEFromRecv.Push(@this);
			}
		}

		private static void OnAmbiguousDamageTaken(Player_Health @this, in float originalAmount, ref float amount, EventPhase phase) {
			if (_isAboutToExecuteTAKEDAMAGEFromRecv.Count > 0 && _isAboutToExecuteTAKEDAMAGEFromRecv.Peek() == @this) return;
			// ^ Do not execute if we are about to execute it for this one ourselves. Just let the vanilla path go.

			AttackInfo attack = default;
			PlayerDamageReceiver.BodyPart part = default;

			ExecuteAllDelegates(
				@this,
				false,
				default,
				ref attack,
				default,
				ref part,
				phase
			);
		}

	}
}
