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
	/// <see cref="SoundFlags"/> to determine how it behaves in the world while playing.
	/// </summary>
	public struct PlayableSoundData {

		/// <summary>
		/// These sound flags are the default.
		/// </summary>
		public const SoundFlags DEFAULT_SOUND_FLAGS = SoundFlags.FollowEmitter | SoundFlags.RealtimePitchShift;

		/// <summary>
		/// The sound to play.
		/// </summary>
		public AudioClip sound;

		/// <summary>
		/// The volume to play the sound with, limited between 0 and 2.
		/// </summary>
		public float volume;

		/// <summary>
		/// The pitch to play the sound with, limited between -1 and 2
		/// </summary>
		public float pitch;

		/// <summary>
		/// Determines how the sound is updated after being spawned in the world.
		/// </summary>
		public SoundFlags playTechnique;

		/// <summary>
		/// The mixer that this sound plays under. If null, it will be treated as the master mixer.
		/// </summary>
		public AudioMixerGroup? mixer;

		/// <summary>
		/// Select a sound from the entry using its provided index. The <see cref="pitch"/> and <see cref="volume"/>
		/// fields are set appropriately (including pitch randomization, if applicable).
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="index"></param>
		/// <param name="playTechnique"></param>
		/// <param name="mixer"></param>
		public PlayableSoundData(ReadOnlyAudioEntry entry, int index, SoundFlags playTechnique = DEFAULT_SOUND_FLAGS, AudioMixerGroup? mixer = null) {
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
		/// <param name="playTechnique"></param>
		/// <param name="mixer"></param>
		public PlayableSoundData(AudioClip sound, float volume = 0.5f, float pitch = 1f, SoundFlags playTechnique = DEFAULT_SOUND_FLAGS, AudioMixerGroup? mixer = null) {
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
		/// <param name="randomPitch"></param>
		/// <param name="playTechnique"></param>
		/// <param name="mixer"></param>
		public PlayableSoundData(AudioClip sound, float volume, Vector2 randomPitch, SoundFlags playTechnique = DEFAULT_SOUND_FLAGS, AudioMixerGroup? mixer = null) {
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
