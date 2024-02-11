#nullable enable
using AvatarStatExtender.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using static AvatarStatExtender.Tools.Tools;

#if UNITY_EDITOR
using UnityEditor;
#elif IS_MOD_ENVIRONMENT
using UnhollowerBaseLib.Attributes;
#endif
/// <summary>
/// This class contains audio variants for various assorted events. Some events are built in, but custom events are supported too.
/// </summary>
public class AvatarExtendedAudioContainer : MonoBehaviour
#if UNITY_EDITOR
, ISerializationCallbackReceiver

#endif
{

#if IS_MOD_ENVIRONMENT
	public AvatarExtendedAudioContainer(IntPtr @this) : base(@this) { }
#endif

	/// <summary>
	/// The names of each audio set.
	/// <para/>
	/// <strong>MODDERS, DO NOT USE THIS.</strong> Use <see cref="GetEntries"/> instead.
	/// </summary>
	public string[] names = Array.Empty<string>();

	/// <summary>
	/// The sounds array that exists, per set, containing audio variants. This uses a Bonelab existing type for compatibility.
	/// <para/>
	/// <strong>MODDERS, DO NOT USE THIS.</strong> Use <see cref="GetEntries"/> instead.
	/// </summary>
	/// <remarks>
	/// This has to use <see cref="SerializableAudioArray"/> to work around the fact that Unity doesn't serialize multilevel/jagged arrays.
	/// </remarks>
	public SerializableAudioArray2D sounds = new SerializableAudioArray2D();

	/// <summary>
	/// Part one of a two part abyssmal hack so my fucking audio can be saved god why did slz have to use il2cpp
	/// <para/>
	/// Do not use this directly, this would be private or internal but has to be public because of <c>s e r i a l i z a t i o n _ l i m i t a t i o n s</c>
	/// with the field injector. Modify <see cref="sounds"/> instead, then call <see cref="Sounds2DToAbyssmalMappedArray"/> to apply it here.
	/// </summary>
	public AudioClip[] allOfTheSounds = Array.Empty<AudioClip>();

	/// <summary>
	/// Part two of a two part abyssmal hack so my fucking audio can be saved god why did slz have to use il2cpp
	/// <para/>
	/// Do not use this directly, this would be private or internal but has to be public because of <c>s e r i a l i z a t i o n _ l i m i t a t i o n s</c>
	/// with the field injector. Modify <see cref="sounds"/> instead, then call <see cref="Sounds2DToAbyssmalMappedArray"/> to apply it here.
	/// </summary>
	public int[] lengthsOfEachSegment = Array.Empty<int>();

	/// <summary>
	/// The technique in which the sounds are played, per set.
	/// <para/>
	/// <strong>MODDERS, DO NOT USE THIS.</strong> Use <see cref="GetEntries"/> instead.
	/// </summary>
	public AudioEventType[] eventTypes = Array.Empty<AudioEventType>();

	/// <summary>
	/// The technique in which the sounds are played, per set.
	/// <para/>
	/// <strong>MODDERS, DO NOT USE THIS.</strong> Use <see cref="GetEntries"/> instead.
	/// </summary>
	public AudioPlayType[] playTypes = Array.Empty<AudioPlayType>();

	/// <summary>
	/// The names of custom event types.
	/// <para/>
	/// <strong>MODDERS, DO NOT USE THIS.</strong> Use <see cref="GetEntries"/> instead.
	/// </summary>
	public string?[] customEventTypes = Array.Empty<string>();

	/// <summary>
	/// The name of a custom play mode.
	/// <para/>
	/// <strong>MODDERS, DO NOT USE THIS.</strong> Use <see cref="GetEntries"/> instead.
	/// </summary>
	public string?[] customPlayTypes = Array.Empty<string>();

	/// <summary>
	/// A range of pitch for the sound when playing, (min, max), centered about 1.0f
	/// <para/>
	/// <strong>MODDERS, DO NOT USE THIS.</strong> Use <see cref="GetEntries"/> instead.
	/// </summary>
	public Vector2[] pitchRanges = Array.Empty<Vector2>();

	/// <summary>
	/// The volume of this sound group.
	/// <para/>
	/// <strong>MODDERS, DO NOT USE THIS.</strong> Use <see cref="GetEntries"/> instead.
	/// </summary>
	public float[] volumes = Array.Empty<float>();

	/// <summary>
	/// An audio source that will be cloned when playing the applicable sound effects.
	/// <para/>
	/// <strong>MODDERS, DO NOT USE THIS.</strong> Use <see cref="GetEntries"/> instead.
	/// </summary>
	public AudioSource?[] templateSources = Array.Empty<AudioSource>();

	/// <summary>
	/// Fixes an issue that is caused if the arrays get different lengths.
	/// </summary>
	public void EnsureSymmetry() {
		int largest = names.Length;
		largest = Math.Max(largest, names.Length);
		largest = Math.Max(largest, sounds.Children.Length);
		largest = Math.Max(largest, eventTypes.Length);
		largest = Math.Max(largest, playTypes.Length);
		largest = Math.Max(largest, customEventTypes.Length);
		largest = Math.Max(largest, customPlayTypes.Length);
		largest = Math.Max(largest, pitchRanges.Length);
		largest = Math.Max(largest, templateSources.Length);
		ResizeArrayDefault(ref names, largest, "New Sound Group");
		ResizeArrayDefault(ref sounds.children, largest, new SerializableAudioArray());
		ResizeArrayDefault(ref eventTypes, largest, AudioEventType.HitBullet);
		ResizeArrayDefault(ref playTypes, largest, AudioPlayType.Random);
		ResizeArrayDefault(ref customEventTypes, largest, null);
		ResizeArrayDefault(ref customPlayTypes, largest, null);
		ResizeArrayDefault(ref pitchRanges, largest, new Vector2(1, 1));
		ResizeArrayDefault(ref volumes, largest, 0.5f);
		ResizeArrayDefault(ref templateSources, largest, null);
	}

	/// <summary>
	/// "Decompresses" <see cref="sounds"/> into a long 1D array of every single combined sound, and a companion array
	/// of the lengths of each subsection in that 1D array, so that Unity can serialize it.
	/// </summary>
	public void Sounds2DToAbyssmalMappedArray() {
		List<AudioClip> result = new List<AudioClip>(128);
		List<int> lengths = new List<int>(128);
		foreach (SerializableAudioArray subArray in sounds.Children) {
			result.AddRange(subArray.clips);
			lengths.Add(subArray.clips.Count);
		}
		allOfTheSounds = result.ToArray();
		lengthsOfEachSegment = lengths.ToArray();
	}

	/// <summary>
	/// "Compresses" <see cref="sounds"/> by collecting all elements out of <see cref="allOfTheSounds"/>, a massive
	/// 1D array of every combined sound across all of the usually separate blocks, and segments the array
	/// based on the lengths provided by <see cref="lengthsOfEachSegment"/>.
	/// </summary>
	public void AbyssmalMappedArrayToSounds2D() {
		sounds.Children = new SerializableAudioArray[lengthsOfEachSegment.Length];
		int start = 0;
		for (int i = 0; i < lengthsOfEachSegment.Length; i++) {
			int subLength = lengthsOfEachSegment[i];
			sounds.Children[i] = new SerializableAudioArray();
			for (int subIndex = 0; subIndex < subLength; subIndex++) {
				sounds.Children[i].clips.Add(allOfTheSounds[start + subIndex]);
			}
			start += subLength;
		}
	}


#if !UNITY_EDITOR && IS_MOD_ENVIRONMENT

	/// <summary>
	/// Returns a cached list of all entries.
	/// </summary>
	/// <returns></returns>
	[HideFromIl2Cpp]
	internal IReadOnlyList<ReadOnlyAudioEntry> GetEntries() {
		if (_ROEntriesCache == null) {
			AbyssmalMappedArrayToSounds2D();
			UpdateEntries();
			_ROEntriesCache = new ReadOnlyCollection<ReadOnlyAudioEntry>(
				Entries.Select(entry => new ReadOnlyAudioEntry(entry)).ToList()
			);
		}
		return _ROEntriesCache;
	}
	private IReadOnlyList<ReadOnlyAudioEntry>? _ROEntriesCache;

#endif

	/// <summary>
	/// The audio entries associated with this container.
	/// </summary>
#if UNITY_EDITOR || !IS_MOD_ENVIRONMENT
	public List<AudioEntry> Entries { get; private set; } = new List<AudioEntry>();
#else
	internal List<AudioEntry> Entries {
		[HideFromIl2Cpp]
		get;
		[HideFromIl2Cpp]
		private set;
	} = new List<AudioEntry>();
#endif

	/// <summary>
	/// Creates <see cref="Entries"/> by copying all data in this object into the array.
	/// </summary>
	public void UpdateEntries() {
		Entries = this.EntriesFromContainer().ToList();
	}

#if UNITY_EDITOR
	public void OnBeforeSerialize() {
		if (Entries == null) {
			UpdateEntries();
		}

		this.OverwriteFromEntries(Entries);

		// Now that all that has been stored, we can apply the changes to the sounds array.
		Sounds2DToAbyssmalMappedArray();
	}

	public void OnAfterDeserialize() {
		AbyssmalMappedArrayToSounds2D(); // Load the data into the sounds field *first*
		UpdateEntries(); // Then copy it to the entries list.
	}

	/// <summary>
	/// Utilizes <see cref="SerializedProperty"/> values to save this object. This is a bit of an abuse
	/// on Unity's property system as it avoids setting the properties in realtime as you are expected
	/// to do. Still, this works, and the resulting code is much cleaner.
	/// </summary>
	/// <param name="this"></param>
#pragma warning disable CS0618
	public void SerializeAndSave(SerializedObject @this) {
		@this.Update();
		@this.FindProperty(nameof(names)).SetArrayValue(names);
		@this.FindProperty(nameof(eventTypes)).SetArrayValue(eventTypes);
		@this.FindProperty(nameof(playTypes)).SetArrayValue(playTypes);
		@this.FindProperty(nameof(customEventTypes)).SetArrayValue(customEventTypes);
		@this.FindProperty(nameof(customPlayTypes)).SetArrayValue(customPlayTypes);
		// @this.FindProperty(nameof(sounds)). // Sounds is serialized as part of @this itself rather than any property.
		@this.FindProperty(nameof(allOfTheSounds)).SetArrayValue(allOfTheSounds);
		@this.FindProperty(nameof(lengthsOfEachSegment)).SetArrayValue(lengthsOfEachSegment);
		@this.FindProperty(nameof(pitchRanges)).SetArrayValue(pitchRanges);
		@this.FindProperty(nameof(volumes)).SetArrayValue(volumes);
		@this.FindProperty(nameof(templateSources)).SetArrayValue(templateSources);
		EditorUtility.SetDirty(this);
		PrefabUtility.RecordPrefabInstancePropertyModifications(this);
		@this.ApplyModifiedProperties();
	}
#pragma warning restore CS0618
#endif

}