using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace AvatarStatExtender.Data {

	/// <summary>
	/// Additional utils related to the <see cref="AudioMixerTarget"/> enum.
	/// </summary>
	public static class AudioMixerTargetExt {

		/// <summary>
		/// Get ahold of the mixer corresponding to the provided <paramref name="target"/>.
		/// If the value is out of range, the master mixer is returned.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static AudioMixerGroup GetMixerForTarget(this AudioMixerTarget target) {
			switch (target) {
				case AudioMixerTarget.Gunshot:
					return GunshotMixer!;
				case AudioMixerTarget.Music:
					return MusicMixer!;
				case AudioMixerTarget.SFX:
					return SFXMixer!;
				default:
					return MasterMixer!;
			}
		}

		/// <summary>
		/// This mixer controls the volume of all other mixers.
		/// </summary>
		public static AudioMixerGroup? MasterMixer { 
			get {
				if (_master == null) GetAudioMixers();
				return _master;
			}
			private set => _master = value;
		}
		private static AudioMixerGroup? _master;

		/// <summary>
		/// This mixer controls the volume of music.
		/// </summary>
		public static AudioMixerGroup? MusicMixer {
			get {
				if (_music == null) GetAudioMixers();
				return _music;
			}
			private set => _music = value;
		}
		private static AudioMixerGroup? _music;

		/// <summary>
		/// This mixer controls the volume of misc. sfx
		/// </summary>
		public static AudioMixerGroup? SFXMixer {
			get {
				if (_sfx == null) GetAudioMixers();
				return _sfx;
			}
			private set => _sfx = value;
		}
		private static AudioMixerGroup? _sfx;

		/// <summary>
		/// This mixer controls the volume of gunshots.
		/// </summary>
		public static AudioMixerGroup? GunshotMixer {
			get {
				if (_gunshot == null) GetAudioMixers();
				return _gunshot;
			}
			private set => _gunshot = value;
		}
		private static AudioMixerGroup? _gunshot;


		/// <summary>
		/// Finds the music and sfx audio mixers.
		/// </summary>
		private static void GetAudioMixers() {
			if (_master != null && _music != null && _sfx != null && _gunshot != null) return;

			AudioMixerGroup[] mixers = Resources.FindObjectsOfTypeAll<AudioMixerGroup>();
			MasterMixer = mixers.FirstOrDefault(x => x.name == "Master");
			MusicMixer = mixers.FirstOrDefault(x => x.name == "Music");
			SFXMixer = mixers.FirstOrDefault(x => x.name == "SFX");
			GunshotMixer = mixers.FirstOrDefault(x => x.name == "GunShot");
		}
	}
}
