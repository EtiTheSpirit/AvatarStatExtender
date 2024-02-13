#nullable enable
using AvatarStatExtender.Data;
using AvatarStatExtender.Tools;
using SLZ.Rig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SLZAvatar = SLZ.VRMK.Avatar;

namespace AvatarStatExtender.API {

	/// <summary>
	/// This class handles all sound API members.
	/// </summary>
	public static class SoundAPI {

		/// <summary>
		/// This <see cref="AudioSelectionDelegate"/> picks one random sound. The first four arguments are unused and can be <see langword="default"/>.
		/// </summary>
		public static readonly AudioSelectionDelegate PLAYTYPE_DEFAULT_RANDOM = (_, _, _, _, group) => {
			return new PlayableSoundData[] {
				new PlayableSoundData(
					group,
					UnityEngine.Random.RandomRangeInt(0, group.Sounds.Count - 1),
					group.SoundFlags,
					group.Mixer.GetMixerForTarget()
				)
			};
		};

		/// <summary>
		/// This <see cref="AudioSelectionDelegate"/> returns all sounds. The first four arguments are unused and can be <see langword="default"/>.
		/// </summary>
		public static readonly AudioSelectionDelegate PLAYTYPE_DEFAULT_ALL = (_, _, _, _, group) => {
			return group.Sounds.Select(sound => new PlayableSoundData(
				sound, 
				group.Volume, 
				group.PitchRange,
				group.SoundFlags,
				group.Mixer.GetMixerForTarget()
			)).ToArray();
		};

		private static readonly Dictionary<string, AudioSelectionDelegate> _delegates = new Dictionary<string, AudioSelectionDelegate>();

		/// <summary>
		/// Invoke this method with one or more event names. These events will be broadcast to all audio 
		/// components that are in the current scene, which in turn will request their sounds are played.
		/// While you cannot pass in <see cref="AudioEventType"/>, you <em>can</em> manually enter the
		/// names of a vanilla event (but this is discouraged, hence why the enum is not an available parameter.
		/// <para/>
		/// Returns an array of all of the <see cref="GameObject"/>s that were created to play the sound effects.
		/// Each object is given the name of its <see cref="AudioClip"/>.
		/// </summary>
		/// <param name="names"></param>
		public static IList<GameObject> BroadcastSoundEvent(string name, params SLZAvatar[] toAvatars) {
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
			if (!SoundBlockNameSanitizer.IsSane(name)) throw new ArgumentOutOfRangeException(nameof(name), "Only one name is allowed, not a list. Names must be all lowercase, and contain only alphanumeric characters, periods, dashes, or underscores. Names may also have a single colon (:) splitting the name, for namespace:sound.");

			string? resolvedName = name;
			if (AudioEventTypeExt.TryGetFromName(name, out AudioEventType type)) {
				resolvedName = null;
			}

			List<GameObject> result = new List<GameObject>();
			IEnumerable<SLZAvatar> avatars = (toAvatars != null && toAvatars.Length > 0 && toAvatars.Any(avy => avy != null)) ? toAvatars : PlayerObjectExtensions.GetAllActiveAvatars();
			foreach (SLZAvatar avatarEarly in avatars) {
				if (avatarEarly == null) continue;
				AvatarAndPrefabPair avatar = new AvatarAndPrefabPair(avatarEarly);
				IEnumerable<ReadOnlyAudioEntry> entries = resolvedName == null ? Array.Empty<ReadOnlyAudioEntry>() : AudioCache.GetMatchingSoundsFromAvatarSingle(avatar.prefab, resolvedName);
				IEnumerable<GameObject> emittedSounds = BroadcastEntriesOf(avatar.prefab, avatar.clone, entries, type, resolvedName);
				result.AddRange(emittedSounds);
			}
			return result;
		}

		/// <summary>
		/// Internal method to send vanilla events out.
		/// </summary>
		/// <param name="builtInEvents"></param>
		internal static IList<GameObject> BroadcastBuiltInSoundEvent(AudioEventType builtInEvents, params SLZAvatar[] toAvatars) {
			if (builtInEvents != AudioEventType.Custom) {
				if ((builtInEvents & (builtInEvents - 1)) != 0) throw new ArgumentOutOfRangeException(nameof(builtInEvents), "Only one flag can be set at a time.");

				List<GameObject> result = new List<GameObject>();
				IEnumerable<SLZAvatar> avatars = (toAvatars != null && toAvatars.Length > 0 && toAvatars.Any(avy => avy != null)) ? toAvatars : PlayerObjectExtensions.GetAllActiveAvatars();
				foreach (SLZAvatar avatarEarly in avatars) {
					if (avatarEarly == null) continue;
					AvatarAndPrefabPair avatar = new AvatarAndPrefabPair(avatarEarly);
					IEnumerable<ReadOnlyAudioEntry> entries = AudioCache.GetMatchingSoundsFromAvatarSingle(avatar.prefab, builtInEvents);
					IEnumerable<GameObject> emittedSounds = BroadcastEntriesOf(avatar.prefab, avatar.clone, entries, builtInEvents, null);
					result.AddRange(emittedSounds);
				}
				return result;
			}
			return (IList<GameObject>)ListTools.EmptyReadOnly<GameObject>();
		}

		/// <summary>
		/// Provided with an avatar to receive the sounds, and the sounds that the avatar can play, this will play each sound.
		/// </summary>
		/// <param name="avatar"></param>
		/// <param name="entries"></param>
		private static IEnumerable<GameObject> BroadcastEntriesOf(SLZAvatar prefab, SLZAvatar clone, IEnumerable<ReadOnlyAudioEntry> entries, AudioEventType vanillaSource, string? customSource) {
			Log.Trace($"Broadcasting sounds to {prefab.name} ({vanillaSource}, {customSource})");
			foreach (ReadOnlyAudioEntry entry in entries) {
				PlayableSoundData[] sounds = Array.Empty<PlayableSoundData>();

				if (entry.UseCustomPlayType) {
					string customPlayType = entry.CustomPlayType!;
					if (!string.IsNullOrWhiteSpace(customPlayType)) {
						if (_delegates.TryGetValue(customPlayType, out AudioSelectionDelegate selector)) {
							try {
								sounds = selector(clone, customPlayType, vanillaSource, customSource, entry);
								Log.Trace($"Custom selector picked {sounds.Length} sound(s).");
							} catch (Exception err) {
								Log.Error($"A custom sound selector for sound group {entry.Name} (of {prefab.name}) raised an exception while picking audio.");
								Log.Error(err);
							}
						} else {
							Log.Warn($"Sound group {entry.Name} (of {prefab.name}) wanted to play a sound with a custom selector, but no such selector '{entry.CustomPlayType}' exists. This sound will be skipped.");
						}
					} else {
						Log.Warn($"Sound group {entry.Name} (of {prefab.name}) wanted to play a sound with a custom selector, but its selector string was null or whitespace.");
					}
				} else {
					if (entry.PlayType == AudioPlayType.Random) {
						Log.Trace("Choosing one random sound.");
						sounds = PLAYTYPE_DEFAULT_RANDOM(clone, "Internal Random Selector", vanillaSource, customSource, entry);
					} else if (entry.PlayType == AudioPlayType.AllTogether) {
						Log.Trace("Choosing all sounds.");
						sounds = PLAYTYPE_DEFAULT_ALL(clone, "Internal All Selector", vanillaSource, customSource, entry);
					} else {
						Log.Warn($"Sound group {entry.Name} (of {prefab.name}) wanted to play a sound with a built-in selector, but the index of the selector ({entry.PlayType:X8}) was undefined. This sound will be skipped.");
					}
				}

				RigManager? mgr = clone.GetRigManager();
				if (mgr == null) {
					Log.Error($"Avatar {clone} has no rig manager? Failed to play sound.");
					continue;
				}

				RealtimeSkeletonRig realHepta = mgr.realHeptaRig;
				if (realHepta == null) {
					Log.Error($"Avatar {clone} has no real hepta rig? Failed to play sound.");
					continue;
				}

				Transform chest = realHepta.m_chest;
				if (chest == null) {
					Log.Error($"Avatar {clone} has no chest set in its real hepta rig? Failed to play sound.");
					continue;
				}

				Log.Trace($"Playing {sounds.Length} sound(s).");
				for (int i = 0; i < sounds.Length; i++) {
					PlayableSoundData sound = sounds[i];
					if (sound.sound == null) {
						Log.Error($"A playable sound clip's sound was missing for entry {entry.Name} (on avatar {prefab.name}); was it returned as null? Was it garbage collected on the il2cpp side?");
						continue;
					}
					sound.mixer ??= AudioMixerTargetExt.GetMixerForTarget(entry.Mixer);
					yield return SoundPlayerSystem.PlaySound(chest, sound, entry.OverrideTemplateAudioSource);
				}
			}
		}

		/// <summary>
		/// Use this to register a Custom Sound Technique to the system. When an event fires on a sound group with the provided
		/// <paramref name="playType"/>, the provided <paramref name="delegate"/> will be executed to collect what sounds
		/// need to be played, and with what pitches/volumes.
		/// </summary>
		/// <param name="playType"></param>
		/// <param name="delegate"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public static void RegisterSoundPlayTypeSelectionHandler(string playType, AudioSelectionDelegate @delegate) {
			if (string.IsNullOrWhiteSpace(playType)) throw new ArgumentNullException(nameof(playType));
			if (@delegate == null) throw new ArgumentNullException(nameof(@delegate));

			if (_delegates.ContainsKey(playType)) throw new InvalidOperationException($"Play type {playType} was already registered.");
			_delegates[playType] = @delegate;
			Log.Trace($"Custom delegate {playType} was just registered.");
		}

		/// <summary>
		/// This delegate may fire multiple times, and allows returning custom sounds from a given category.
		/// </summary>
		/// <param name="avatar">The avatar that this is currently executing on.</param>
		/// <param name="playTypeName">The name of this delegate as you registered it.</param>
		/// <param name="vanillaEvent">If not <see cref="AudioEventType.Custom"/>, this was triggered by a vanilla event of this type.</param>
		/// <param name="customEvent">If not null, this was triggered by a custom event of this type.</param>
		/// <param name="triggeredGroup">The group that audio will be chosen out of. A reminder that you can use this for events.</param>
		/// <returns></returns>
		public delegate PlayableSoundData[] AudioSelectionDelegate(SLZAvatar avatar, string playTypeName, AudioEventType vanillaEvent, string? customEvent, ReadOnlyAudioEntry triggeredGroup);

	}
}
