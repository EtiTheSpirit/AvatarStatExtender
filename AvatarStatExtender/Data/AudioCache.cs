#nullable enable
using AvatarStatExtender.Tools;
using AvatarStatExtender.Tools.Assets;
using SLZ.VRMK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SLZAvatar = SLZ.VRMK.Avatar;

namespace AvatarStatExtender.Data {

	/// <summary>
	/// Keeps a cache of audio components loaded by avatars. This cache is very verbose, at the cost of large memory usage
	/// (likely several hundred megabytes when many avatar mods are installed).
	/// </summary>
	internal static class AudioCache {

		private static readonly ISet<ReadOnlyAudioEntry> EMPTY = new ReadOnlyHashSet<ReadOnlyAudioEntry>();

		/// <summary>
		/// Binds an avatar prefab to a list of entries.
		/// <para/>
		/// Note that this uses <see cref="IntPtr"/> because il2cpp objects that are represented in managed form are comparable
		/// to "proxies" to a pointer rather than the actual tangible object itself.
		/// </summary>
		private static readonly Dictionary<IntPtr, ISet<ReadOnlyAudioEntry>> _cachePerAvatar = new();

		/// <summary>
		/// Binds an avatar prefab to a lookup from custom event name =&gt; entries in that avatar's list of sound groups.
		/// <para/>
		/// Note that this uses <see cref="IntPtr"/> because il2cpp objects that are represented in managed form are comparable
		/// to "proxies" to a pointer rather than the actual tangible object itself.
		/// </summary>
		private static readonly Dictionary<IntPtr, Dictionary<string, ISet<ReadOnlyAudioEntry>>> _cachePerAvatarByEventName = new();

		/// <summary>
		/// Binds an avatar prefab to a lookup from custom event name =&gt; entries in that avatar's list of sound groups.
		/// <para/>
		/// Note that this uses <see cref="IntPtr"/> because il2cpp objects that are represented in managed form are comparable
		/// to "proxies" to a pointer rather than the actual tangible object itself.
		/// </summary>
		private static readonly Dictionary<IntPtr, Dictionary<AudioEventType, ISet<ReadOnlyAudioEntry>>> _cachePerAvatarByEventEnum = new();

		/// <summary>
		/// Provided with an avatar, be it a clone from a player or the real prefab, this will return all <see cref="ReadOnlyAudioEntry"/> objects
		/// for the <see cref="AvatarExtendedAudioContainer"/> attached to the avatar, if it has one.
		/// <para/>
		/// If the groups have yet to be resolved, this will resolve and cache them. If the avatar has no extended container, it will return
		/// an empty array.
		/// </summary>
		/// <param name="avatar"></param>
		/// <returns></returns>
		public static ISet<ReadOnlyAudioEntry> GetAllSoundsOfAvatar(SLZAvatar avatar) {
			SLZAvatar? original = avatar.GetOriginalPrefab();
			if (original == null) {
				Log.Error($"Failed to find original prefab for avatar {avatar.name} when trying to collect all sounds. An empty set will be returned.");
				return EMPTY;
			}
			return GetAllSoundsOfAvatarPrefab(original);
		}

		/// <summary>
		/// The same as <see cref="GetAllSoundsOfAvatar(SLZAvatar)"/> but this assumes that the caller guarantees
		/// the incoming avatar is the prefab instance.
		/// </summary>
		/// <param name="original"></param>
		/// <returns></returns>
		private static ISet<ReadOnlyAudioEntry> GetAllSoundsOfAvatarPrefab(SLZAvatar originalAvy) {
			if (!originalAvy.IsPrefabAvatar()) throw new ArgumentException($"The avatar that called this method was not a prefab ({originalAvy.name})");
			IntPtr original = originalAvy.Pointer;
			if (_cachePerAvatar.TryGetValue(original, out ISet<ReadOnlyAudioEntry> list)) {
				return list;
			}
			if (originalAvy.GetComponent<AvatarExtendedAudioContainer>() is AvatarExtendedAudioContainer ctr && ctr != null) {
				Log.Trace("Avatar has a container for custom sounds...");
				IReadOnlyList<ReadOnlyAudioEntry> entries = ctr.GetEntries();
				_cachePerAvatar[original] = new ReadOnlyHashSet<ReadOnlyAudioEntry>(entries);
				_cachePerAvatarByEventName[original] = new Dictionary<string, ISet<ReadOnlyAudioEntry>>();
				_cachePerAvatarByEventEnum[original] = new Dictionary<AudioEventType, ISet<ReadOnlyAudioEntry>>();
				foreach (ReadOnlyAudioEntry entry in entries) {
					Log.Trace($"Handling entry: {entry.Name}");
					foreach (string custom in entry.ResolvedCustomEvents) {
						if (!_cachePerAvatarByEventName[original].TryGetValue(custom, out ISet<ReadOnlyAudioEntry> finalCtr)) {
							_cachePerAvatarByEventName[original][custom] = finalCtr = new HashSet<ReadOnlyAudioEntry>();
						}
						finalCtr.Add(entry);
						Log.Trace($"Entry {entry.Name} has registered a custom event: {custom}");
					}
					if (entry.EventType != default) {
						Log.Trace($"Entry {entry.Name} declares one or more vanilla event types...");
						for (int i = 0; i < AudioEventTypeExt.ALL_FLAGS.Length; i++) {
							AudioEventType type = AudioEventTypeExt.ALL_FLAGS[i];
							if ((entry.EventType & type) != 0) {
								if (!_cachePerAvatarByEventEnum[original].TryGetValue(type, out ISet<ReadOnlyAudioEntry> finalCtr)) {
									_cachePerAvatarByEventEnum[original][type] = finalCtr = new HashSet<ReadOnlyAudioEntry>();
								}
								finalCtr.Add(entry);
								Log.Trace($"Entry {entry.Name} has registered a vanilla event: {type}");
							}

						}
					}
				}
				Log.Trace($"The cache has been constructed for {ctr.name}");
				return _cachePerAvatar[original];
			} else {
				Log.Trace("Avatar does not have a container for custom sounds.");
			}
			_cachePerAvatarByEventName[original] = new Dictionary<string, ISet<ReadOnlyAudioEntry>>();
			_cachePerAvatarByEventEnum[original] = new Dictionary<AudioEventType, ISet<ReadOnlyAudioEntry>>();
			_cachePerAvatar[original] = EMPTY;
			return EMPTY;
		}

		/// <summary>
		/// Collects all of the sound groups relevant to this single custom event. Note that this does not propagate vanilla event type names.
		/// </summary>
		/// <param name="onAvatarObj"></param>
		/// <param name="singleCustomEventType"></param>
		/// <returns></returns>
		internal static IEnumerable<ReadOnlyAudioEntry> GetMatchingSoundsFromAvatarSingle(SLZAvatar prefab, string singleCustomEventType) {
			IntPtr onAvatar = prefab.Pointer;
			GetAllSoundsOfAvatarPrefab(prefab); // Just using this to populate the cache.
			if (_cachePerAvatarByEventName.TryGetValue(onAvatar, out Dictionary<string, ISet<ReadOnlyAudioEntry>> entriesLookup)) {
				if (entriesLookup.TryGetValue(singleCustomEventType, out ISet<ReadOnlyAudioEntry> entries)) {
					foreach (ReadOnlyAudioEntry entry in entries) yield return entry;
				} else {
					Log.Trace("Missing sound set in string cache. Skipping.");
				}
			} else {
				Log.Trace("Missing avatar in string cache. Skipping.");
			}
		}
		/// <summary>
		/// Collects all of the sound groups relevant to this single event. Note that this does not propagate vanilla event types.
		/// </summary>
		/// <param name="onAvatarObj"></param>
		/// <param name="singleEventType"></param>
		/// <returns></returns>
		internal static IEnumerable<ReadOnlyAudioEntry> GetMatchingSoundsFromAvatarSingle(SLZAvatar prefab, AudioEventType singleEventType) {
			IntPtr onAvatar = prefab.Pointer;

			GetAllSoundsOfAvatarPrefab(prefab); // Just using this to populate the cache.
			if (_cachePerAvatarByEventEnum.TryGetValue(onAvatar, out Dictionary<AudioEventType, ISet<ReadOnlyAudioEntry>> entriesLookup)) {
				if (entriesLookup.TryGetValue(singleEventType, out ISet<ReadOnlyAudioEntry> entries)) {
					foreach (ReadOnlyAudioEntry entry in entries) yield return entry;
				} else {
					Log.Trace("Missing sound set in enum cache. Skipping.");
				}
			} else {
				Log.Trace("Missing avatar in enum cache. Skipping.");
			}
		}

	}
}
