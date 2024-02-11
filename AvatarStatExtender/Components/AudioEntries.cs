#nullable enable
using SLZ.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AvatarStatExtender.Data {

	/// <summary>
	/// This class provides methods to convert to and from an easier data format for <see cref="AvatarExtendedAudioContainer"/>.
	/// </summary>
#if UNITY_EDITOR
	public
#else
	internal
#endif
	static class AudioEntries {

		/// <summary>
		/// Iterates over all entries stored within the FieldInjector-friendly form of data stored in the <paramref name="container"/>,
		/// and returns each set of data as an <see cref="AudioEntry"/>.
		/// </summary>
		/// <param name="container"></param>
		/// <returns></returns>
		public static IEnumerable<AudioEntry> EntriesFromContainer(this AvatarExtendedAudioContainer container) {
			if (container == null) throw new ArgumentNullException(nameof(container), "The audio container cannot be null.");
			container.EnsureSymmetry();

			int length = container.names.Length; // Kind of arbitrary, but all arrays are equal anyway.
			for (int index = 0; index < length; index++) {
				yield return EntryFromContainer(container, index, true);
			}
		}

		/// <summary>
		/// Attempts to remove an audio entry by reference.
		/// </summary>
		/// <param name="container"></param>
		/// <param name="entry"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool TryRemoveEntry(this AvatarExtendedAudioContainer container, AudioEntry entry) {
			if (container == null) throw new ArgumentNullException(nameof(container), "The audio container cannot be null.");
			if (entry == null) throw new ArgumentNullException(nameof(entry), "The audio entry cannot be null.");
			int index = container.Entries.IndexOf(entry);
			if (index >= 0) {
				container.Entries.RemoveAt(index);
				container.OverwriteFromEntries(container.Entries);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Attempts to remove an audio entry by index.
		/// </summary>
		/// <param name="container"></param>
		/// <param name="entry"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool TryRemoveEntryAt(this AvatarExtendedAudioContainer container, int index) {
			if (container == null) throw new ArgumentNullException(nameof(container), "The audio container cannot be null.");
			if (index < 0 || index >= container.Entries.Count) return false;
			container.Entries.RemoveAt(index);
			container.OverwriteFromEntries(container.Entries);
			return true;
		}

		/// <summary>
		/// Converts a single value into an <see cref="AudioEntry"/> from the provided <paramref name="container"/>.
		/// </summary>
		/// <param name="container"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static AudioEntry EntryFromContainer(this AvatarExtendedAudioContainer container, int index, bool skipSymmetry = false) {
			if (container == null) throw new ArgumentNullException(nameof(container), "The audio container cannot be null.");
			if (!skipSymmetry) container.EnsureSymmetry();

			string name = container.names[index] ?? string.Empty;
			SerializableAudioArray variance = container.sounds[index];
			if (variance == null) {
				variance = new SerializableAudioArray();
				container.sounds[index] = variance;
			}
			AudioClip[] sounds = variance.Clips.ToArray();
			AudioPlayType playType = container.playTypes[index];
			AudioEventType eventType = container.eventTypes[index];
			string? customPlayType = container.customPlayTypes[index];
			string? customEventTypes = container.customEventTypes[index];
			Vector2 pitchRange = container.pitchRanges[index];
			float volume = container.volumes[index];
			AudioSource? templateSource = container.templateSources[index];
			return new AudioEntry(name, playType, customPlayType, eventType, customEventTypes, sounds.ToList(), pitchRange, volume, templateSource);
		}

		/// <summary>
		/// Iterates over all of the provided <see cref="AudioEntries"/> and overwrites the data stored within the provided
		/// <paramref name="container"/> such that it reflects the values provided by the entries.
		/// </summary>
		/// <param name="container"></param>
		/// <param name="entriesEnumerable"></param>
		public static void OverwriteFromEntries(this AvatarExtendedAudioContainer container, IEnumerable<AudioEntry> entriesEnumerable) {
			if (container == null) throw new ArgumentNullException(nameof(container), "The audio container cannot be null.");
			AudioEntry[] entries = entriesEnumerable.ToArray();
			int length = entries.Length;
			Array.Resize(ref container.names, length);
			Array.Resize(ref container.sounds.children, length);
			Array.Resize(ref container.playTypes, length);
			Array.Resize(ref container.eventTypes, length);
			Array.Resize(ref container.customPlayTypes, length);
			Array.Resize(ref container.customEventTypes, length);
			Array.Resize(ref container.pitchRanges, length);
			Array.Resize(ref container.volumes, length);
			Array.Resize(ref container.templateSources, length);
			for (int index = 0; index < length; index++) {
				OverwriteFromEntry(container, entries[index], index, true);
			}
		}

		/// <summary>
		/// Overwrites the values at the provided index with the provided entry.
		/// </summary>
		/// <param name="container"></param>
		/// <param name="entry"></param>
		/// <param name="index"></param>
		public static void OverwriteFromEntry(this AvatarExtendedAudioContainer container, AudioEntry entry, int index, bool skipSymmetry = false) {
			if (container == null) throw new ArgumentNullException(nameof(container), "The audio container cannot be null.");
			if (!skipSymmetry) container.EnsureSymmetry();
			if (entry == null) throw new InvalidOperationException($"AudioEntry #{index} is null.");

			container.names[index] = entry.Name;
			if (container.sounds[index] == null) container.sounds[index] = new SerializableAudioArray();
			container.sounds[index].Clips = entry.Sounds ?? new List<AudioClip>();
			container.playTypes[index] = entry.PlayType;
			container.eventTypes[index] = entry.EventType;
			container.customPlayTypes[index] = entry.CustomPlayType;
			container.customEventTypes[index] = entry.CustomEventTypes;
			container.pitchRanges[index] = entry.PitchRange;
			container.volumes[index] = entry.Volume;
			container.templateSources[index] = entry.OverrideTemplateAudioSource;
		}

	}
}
