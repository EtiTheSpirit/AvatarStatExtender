#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarStatExtender.Data {
	/// <summary>
	/// This class is a representation of the audio data as seen in the custom audio extension.
	/// It exists in the editor, as it cannot be used during runtime as a direct result of limitations imposed
	/// by the Field Injector program.
	/// <para/>
	/// Now, there is nothing that says this can't be in the mod anyway, and just have a translator included.
	/// </summary>
#if UNITY_EDITOR || !IS_MOD_ENVIRONMENT
	public
#else
	internal
#endif
	class AudioEntry {

		/// <summary>
		/// The name of this audio group.
		/// </summary>
		public string Name { get; set; } = "New Sound Group";

		/// <summary>
		/// If true, use <see cref="CustomPlayType"/>
		/// </summary>
		public bool UseCustomPlayType => PlayType == AudioPlayType.Custom;

		/// <summary>
		/// The technique that this audio group uses to play.
		/// </summary>
		public AudioPlayType PlayType { get; set; }

		/// <summary>
		/// The custom delegate that this will invoke to determine how to play audio.
		/// </summary>
		public string? CustomPlayType { get; set; }

		/// <summary>
		/// The type of event that this audio reacts to.
		/// </summary>
		public AudioEventType EventType { get; set; }

		/// <summary>
		/// A string based event that this audio reacts to.
		/// <para/>
		/// Note that this <strong>must</strong> be all lowercase (and will assert such) for the sake of better compatibility between mods.
		/// If your damage event is generic, try coming up with as generalized a name as possible.
		/// <para/>
		/// If you wish to support multiple events, separate them with semicolon <c>;</c> like so: <c>yeeted;sent_to_shadow_realm;obliterated;deleted</c>
		/// </summary>
		public string? CustomEventTypes {
			get => _customEventTypes?.ToLower();
			set => _customEventTypes = value?.ToLower();
		}
		private string? _customEventTypes = null;

		/// <summary>
		/// The list of sounds in this entry.
		/// </summary>

		public List<AudioClip> Sounds { get; set; } = new List<AudioClip>();

		/// <summary>
		/// The range of pitch that automatic sound playing can use, (min, max).
		/// </summary>
		public Vector2 PitchRange { get; set; } = new Vector2(1, 1);

		/// <summary>
		/// The volume of this slider.
		/// </summary>
		public float Volume { get; set; } = 0.5f;

		/// <summary>
		/// The mixer that this should use when playing.
		/// </summary>
		public AudioMixerTarget Mixer { get; set; } = AudioMixerTarget.SFX;

		/// <summary>
		/// How this should should be updated when playing.
		/// </summary>
		public SoundFlags SoundFlags { get; set; } = SoundFlags.FollowEmitter | SoundFlags.RealtimePitchShift;

		/// <summary>
		/// If defined, this will override the template audio source used by default when playing sounds.
		/// </summary>
		public AudioSource? OverrideTemplateAudioSource { get; set; }

		/// <summary>
		/// Create a new, default audio entry.
		/// </summary>
		public AudioEntry() {
			Name = "New Sound Group";
			PlayType = AudioPlayType.Random;
			EventType = AudioEventType.HitBullet;
			Sounds = new List<AudioClip>();
		}

		/// <summary>
		/// Create an audio entry from existing data.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="playType"></param>
		/// <param name="customPlayType"></param>
		/// <param name="eventType"></param>
		/// <param name="customEventTypes"></param>
		/// <param name="sounds"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public AudioEntry(string name, AudioPlayType playType, string? customPlayType, AudioEventType eventType, string? customEventTypes, List<AudioClip> sounds, Vector2 pitchRange, float volume, AudioMixerTarget mixerTarget, SoundFlags soundFlags, AudioSource overrideTemplateAudioSrc) {
			Name = name ?? throw new ArgumentNullException(nameof(name));
			PlayType = playType;
			CustomPlayType = customPlayType;
			EventType = eventType;
			CustomEventTypes = customEventTypes;
			Sounds = sounds ?? throw new ArgumentNullException(nameof(sounds));
			PitchRange = pitchRange;
			Volume = volume;
			Mixer = mixerTarget;
			SoundFlags = soundFlags;
			OverrideTemplateAudioSource = overrideTemplateAudioSrc;
		}


	}
}