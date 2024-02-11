#nullable enable
using AvatarStatExtender.Data;
using AvatarStatExtender.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace AvatarStatExtender.API {

	/// <summary>
	/// Contains an <see cref="AudioClip"/>, a <see cref="pitch"/>, a <see cref="volume"/>, and 
	/// <see cref="SoundPlayerSystem.SoundFlags"/> to determine how it behaves in the world while playing.
	/// </summary>
	public readonly struct PlayableSoundData {

		public const SoundPlayerSystem.SoundFlags DEFAULT_SOUND_FLAGS = SoundPlayerSystem.SoundFlags.FollowEmitter | SoundPlayerSystem.SoundFlags.RealtimePitchShift;

		public readonly AudioClip sound;

		public readonly float volume;

		public readonly float pitch;

		public readonly SoundPlayerSystem.SoundFlags playTechnique;

		public readonly AudioMixerGroup? mixer;

		/// <summary>
		/// Select a sound from the entry using its provided index. The <see cref="pitch"/> and <see cref="volume"/>
		/// fields are set appropriately (including pitch randomization, if applicable).
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="index"></param>
		public PlayableSoundData(ReadOnlyAudioEntry entry, int index, SoundPlayerSystem.SoundFlags playTechnique = DEFAULT_SOUND_FLAGS, AudioMixerGroup? mixer = null) {
			sound = entry.Sounds[index];
			volume = Mathf.Clamp(entry.Volume, 0f, 2f);
			this.playTechnique = playTechnique;
			this.mixer = mixer;

			float pitchMin = Mathf.Min(entry.PitchRange.x, entry.PitchRange.y);
			float pitchMax = Mathf.Max(entry.PitchRange.x, entry.PitchRange.y);
			pitchMin = Mathf.Clamp(pitchMin, -1f, 2f);
			pitchMax = Mathf.Clamp(pitchMax, -1f, 2f);
			if (pitchMin != pitchMax) {
				pitch = UnityEngine.Random.RandomRange(pitchMin, pitchMax);
			} else {
				pitch = pitchMin;
			}
		}

		/// <summary>
		/// Create a new sound, potentially including an entry that isn't even in the list.
		/// </summary>
		/// <param name="sound"></param>
		/// <param name="volume"></param>
		/// <param name="pitch"></param>
		public PlayableSoundData(AudioClip sound, float volume = 0.5f, float pitch = 1f, SoundPlayerSystem.SoundFlags playTechnique = DEFAULT_SOUND_FLAGS, AudioMixerGroup? mixer = null) {
			this.sound = sound;
			this.volume = Mathf.Clamp(volume, 0f, 2f);
			this.pitch = Mathf.Clamp(pitch, -1f, 2f);
			this.playTechnique = playTechnique;
			this.mixer = mixer;
		}

		/// <summary>
		/// Create a new sound, potentially including an entry that isn't even in the list.
		/// </summary>
		/// <param name="sound"></param>
		/// <param name="volume"></param>
		/// <param name="pitch"></param>
		public PlayableSoundData(AudioClip sound, float volume, Vector2 randomPitch, SoundPlayerSystem.SoundFlags playTechnique = DEFAULT_SOUND_FLAGS, AudioMixerGroup? mixer = null) {
			this.sound = sound;
			this.volume = Mathf.Clamp(volume, 0f, 2f);
			this.playTechnique = playTechnique;
			this.mixer = mixer;

			float pitchMin = Mathf.Min(randomPitch.x, randomPitch.y);
			float pitchMax = Mathf.Max(randomPitch.x, randomPitch.y);
			pitchMin = Mathf.Clamp(pitchMin, -1f, 2f);
			pitchMax = Mathf.Clamp(pitchMax, -1f, 2f);
			if (pitchMin != pitchMax) {
				pitch = UnityEngine.Random.RandomRange(pitchMin, pitchMax);
			} else {
				pitch = pitchMin;
			}
		}
	}
}
