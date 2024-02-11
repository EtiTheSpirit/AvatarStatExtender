#nullable enable
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

namespace XansTools.Data {

	/// <summary>
	/// An event wrapper that invokes each individual subscriber in a try/catch block.
	/// This sacrifices speed in favor of safety for a public interface, and also improves
	/// logging by requiring a mod to "blame" for any exceptions that are raised.
	/// </summary>
	/// <typeparam name="TArgs">A package for the arguments of this call.</typeparam>
	/// <typeparam name="TReturn">The return value. This can be <see cref="object"/> and then always return null when not applicable.</typeparam>
	[Obsolete("This is not yet ready and may not actually be used.")]
	public sealed class BlamedEvent<TArgs, TReturn> {

		private readonly Dictionary<MelonMod, Accessor> _accessorCache = new Dictionary<MelonMod, Accessor>();

		/// <summary>
		/// A display name for this event that can be used in logging.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// This delegate can should update the provided <typeparamref name="TArgs"/> after every event invocation.
		/// This can be used if the arguments store the last return value.
		/// </summary>
		internal UpdateArgsPackageDelegate? UpdateArgsWithReturnValue { get; set; }

		internal delegate void UpdateArgsPackageDelegate(in TReturn retIn, ref TArgs argsPackage);

		internal BlamedEvent(string name) {
			Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("The name of an event must be a non-null name with at least one or more non-whitespace character(s).");
		}

		internal void InvokeAll(ref TArgs argsPackage, in EventPhase phase, ref TReturn currentReturnValue) {
			foreach (Accessor accessor in _accessorCache.Values) {
				foreach (Accessor.ModOwnedSubscriber subscriber in accessor.GetSubscribers(phase)) {
					try {
						currentReturnValue = subscriber.Callback(argsPackage, phase);
						UpdateArgsWithReturnValue?.Invoke(currentReturnValue, ref argsPackage);
					} catch (Exception exc) {
						try {
							subscriber.Mod.LoggerInstance.Error("This error message has been sent by XansTools so that the Lum log analyzer bot can detect this error as part of this mod. The real error is right below.");
						} catch { }
						Log.Error($"Mod [{accessor.Mod.ID}]'s event callback [{subscriber.Callback} ({subscriber.Callback.Method?.Name})] has raised an exception while {Name} was firing {phase} the original method. Exception follows:");
						Log.Error(exc);
					}
				}
			}
		}

		/// <summary>
		/// Returns an instance of <see cref="Accessor"/> which allows your mod to (un)subscribe to and from this event.
		/// </summary>
		/// <param name="yourMod"></param>
		public Accessor GetSubscriptionService(MelonMod yourMod) {
			if (yourMod == null) throw new ArgumentNullException(nameof(yourMod), "You must provide an instance of your mod for error debugging purposes.");
			if (!_accessorCache.TryGetValue(yourMod, out Accessor? accessor)) {
				accessor = new Accessor(this, yourMod);
			}
			return accessor;
		}

		public class Accessor {

			private readonly Dictionary<Func<TArgs, EventPhase, TReturn>, (ModOwnedSubscriber?, ModOwnedSubscriber?)> _subscribersByDelegate = new Dictionary<Func<TArgs, EventPhase, TReturn>, (ModOwnedSubscriber?, ModOwnedSubscriber?)>();

			public BlamedEvent<TArgs, TReturn> Owner { get; }

			public MelonMod Mod { get; }

			internal Accessor(BlamedEvent<TArgs, TReturn> owner, MelonMod mod) {
				Owner = owner;
				Mod = mod;
			}

			internal IEnumerable<ModOwnedSubscriber> GetSubscribers(EventPhase phase) {
				foreach ((ModOwnedSubscriber?, ModOwnedSubscriber?) subscriber in _subscribersByDelegate.Values) {
					ModOwnedSubscriber? target = null;
					if (phase == EventPhase.Before) {
						target = subscriber.Item1;
					} else if (phase == EventPhase.After) {
						target = subscriber.Item2;
					}
					if (target != null) {
						yield return target;
					} else {
						continue;
					}
				}
			}

			/// <summary>
			/// Subscribe this event 
			/// </summary>
			/// <param name="delegate">The delegate to subscribe to this event, which will be invoked when this event fires.</param>
			/// <param name="unsubscribeAutomaticallyIfMelonUnloaded">If true, and if the mod is unloaded, then the provided delegate will be automatically unsubscribed.</param>
			public void Subscribe(Func<TArgs, EventPhase, TReturn> @delegate, EventPhase phase, bool unsubscribeAutomaticallyIfMelonUnloaded = true) {
				if (@delegate == null) throw new ArgumentNullException(nameof(@delegate));
				if (phase != EventPhase.Before && phase != EventPhase.After) throw new ArgumentOutOfRangeException(nameof(phase), "Invalid event phase.");

				if (_subscribersByDelegate.TryGetValue(@delegate, out (ModOwnedSubscriber?, ModOwnedSubscriber?) phasePair)) {
					bool alreadyRegistered = false;
					if (phase == EventPhase.Before) {
						alreadyRegistered = phasePair.Item1 != null;
					} else if (phase == EventPhase.After) {
						alreadyRegistered = phasePair.Item2 != null;
					}
					if (alreadyRegistered) {
						throw new InvalidOperationException($"Mod [{Mod.ID}] tried to subscribe to event {Owner.Name} with the same delegate more than once for the same phase ({phase}). You can only subscribe any given method once for a phase.");
					}
				}

				ModOwnedSubscriber subscriber = new ModOwnedSubscriber(this, Mod, @delegate, unsubscribeAutomaticallyIfMelonUnloaded);
				phasePair = default;
				if (phase == EventPhase.Before) {
					phasePair.Item1 = subscriber;
				} else {
					phasePair.Item2 = subscriber;
				}
				_subscribersByDelegate[@delegate] = phasePair;
			}

			public void Unsubscribe(Func<TArgs, EventPhase, TReturn> @delegate, EventPhase phase) {
				Unsubscribe(@delegate, phase, false);
			}

			private void Unsubscribe(Func<TArgs, EventPhase, TReturn> @delegate, EventPhase phase, bool softFail) {
				if (@delegate == null) throw new ArgumentNullException(nameof(@delegate));
				if (phase != EventPhase.Before && phase != EventPhase.After) throw new ArgumentOutOfRangeException(nameof(phase), "Invalid event phase.");

				if (_subscribersByDelegate.TryGetValue(@delegate, out (ModOwnedSubscriber?, ModOwnedSubscriber?) subscribers)) {
					//_subscribersByDelegate.Remove(@delegate);
					if (phase == EventPhase.Before) {
						if (subscribers.Item1 != null) {
							subscribers.Item1 = null;
						}
						_subscribersByDelegate[@delegate] = subscribers;
						return;
					} else {
						if (subscribers.Item2 != null) {
							subscribers.Item2 = null;
						}
						_subscribersByDelegate[@delegate] = subscribers;
						return;
					}
				}
				if (!softFail) {
					throw new InvalidOperationException($"Mod [{Mod.ID}] tried to unsubscribe from event {Owner.Name} but the provided delegate (Method: {@delegate.Method?.Name ?? "<no method>"}) was not a subscriber.");
				}
			}

			private void UnsubscribeAll(ModOwnedSubscriber subscriber) {
				Unsubscribe(subscriber.Callback, EventPhase.Before, true);
				Unsubscribe(subscriber.Callback, EventPhase.After, true);
			}


			internal sealed class ModOwnedSubscriber : IEquatable<ModOwnedSubscriber> {

				public Accessor Owner { get; }

				public MelonMod Mod { get; }

				public Func<TArgs, EventPhase, TReturn> Callback { get; }

				public ModOwnedSubscriber(Accessor owner, MelonMod mod, Func<TArgs, EventPhase, TReturn> callback, bool unsubscribeAutomaticallyIfMelonUnloaded = true) {
					Owner = owner;
					Mod = mod;
					Callback = callback;
					if (unsubscribeAutomaticallyIfMelonUnloaded) {
						SetupAutoUnsubscribe();
					}
				}

				internal void SetupAutoUnsubscribe() {
					Mod.OnUnregister.Subscribe(() => {
						Owner.UnsubscribeAll(this);
					}, unsubscribeOnFirstInvocation: true);
				}

				public override bool Equals(object obj) {
					if (obj is Accessor accessor) {
						return Equals(accessor);
					}
					return false;
				}

				public override int GetHashCode() {
					return unchecked((int)((ulong)Mod.GetHashCode() * (ulong)Callback.GetHashCode()));
				}

				public bool Equals(ModOwnedSubscriber? other) {
					if (other is null) return false;
					return Owner == other.Owner && Mod == other.Mod;
				}

				public static bool operator ==(ModOwnedSubscriber? left, ModOwnedSubscriber? right) => left?.Equals(right) ?? false;

				public static bool operator !=(ModOwnedSubscriber? left, ModOwnedSubscriber? right) => !(left == right);
			}
		}


	}
}
