using BoneLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Audio;

namespace AvatarStatExtender.Data {

	/// <summary>
	/// Additional utils related to the <see cref="AudioMixerTarget"/> enum.
	/// </summary>
	public static class AudioMixerTargetExt {

		/// <summary>
		/// Uses <see cref="Audio"/> to get ahold of the mixer corresponding to the provided <paramref name="target"/>.
		/// If the value is out of range, the master mixer is returned.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static AudioMixerGroup GetMixerForTarget(this AudioMixerTarget target) {
			switch (target) {
				case AudioMixerTarget.Gunshot:
					return Audio.GunshotMixer;
				case AudioMixerTarget.Music:
					return Audio.MusicMixer;
				case AudioMixerTarget.SFX:
					return Audio.SFXMixer;
				default:
					return Audio.MasterMixer;
			}
		}


	}
}
