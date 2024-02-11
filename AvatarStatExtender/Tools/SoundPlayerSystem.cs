#nullable enable
using AvatarStatExtender.API;
using AvatarStatExtender.Components;
using BoneLib;
using SLZ.Data;
using SLZ.Marrow.Interaction;
using SLZ.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace AvatarStatExtender.Tools {

	/// <summary>
	/// This class plays sounds arbitrarily. It gets its name in that it was designed for my private avatar mod, which was for the
	/// ECLIPSE class of Chimera, a very cool and good member of a suite of amazing robot avatars for VRC.
	/// </summary>
	public static class SoundPlayerSystem {

		/// <summary>
		/// Play the provided <see cref="AudioClip"/> using the provided settings. A new <see cref="GameObject"/> is created for this sound, which
		/// is returned from the method. Note that a component, <see cref="RealtimeSoundDriver"/>, is attached, which may set the object's position
		/// every frame.
		/// </summary>
		/// <param name="on">The transform to play the sound at.</param>
		/// <param name="sound">The sound to play.</param>
		/// <param name="volume">The volume of the sound, from 0 to 2.</param>
		/// <param name="pitch">The pitch of the sound, which also affects its speed.</param>
		/// <param name="playAs2D">If true, the audio plays in 2D (directly out of the speakers) instead of at a position in the world.</param>
		/// <param name="flags">Flags that dictate how the sound behaves while playing.</param>
		/// <param name="mixer">The mixer to play on. If null, this uses <see cref="Audio.SFXMixer"/>.</param>
		/// <returns></returns>
		public static GameObject PlaySound(Transform on, AudioClip sound, float volume = 1.0f, float pitch = 1.0f, bool playAs2D = false, SoundFlags flags = SoundFlags.FollowEmitter | SoundFlags.RealtimePitchShift, AudioMixerGroup? mixer = null) {
			Log.Trace($"Playing sound: {sound.name}...");
			GameObject obj = new GameObject();
			if (on == null) {
				on = Camera.main.gameObject.transform;
			}

			obj.transform.parent = on;
			obj.transform.localPosition = Vector3.zero;


			bool doPitchShifting = flags.HasFlag(SoundFlags.PitchShift);
			bool doRTPitchShifting = flags.HasFlag(SoundFlags.RealtimePitchShift);
			bool followParent = flags.HasFlag(SoundFlags.FollowEmitter);

			float pitchMod = pitch;
			if (doPitchShifting) {
				pitchMod *= Time.timeScale;
			}

			AudioSource audio = obj.AddComponent<AudioSource>();
			audio.outputAudioMixerGroup = mixer ? mixer : Audio.SFXMixer;
			audio.volume = volume;
			audio.pitch = pitchMod;
			audio.loop = false;
			audio.playOnAwake = false;
			audio.clip = sound;
			SetupDefaultNearbyCurves(audio, playAs2D);
			audio.Play();

			if (!doRTPitchShifting && !followParent) {
				UnityEngine.Object.Destroy(obj, sound.length + 0.2f); // Just destroy at the default time, plus some buffer room.
			} else {
				RealtimeSoundDriver.StartDriving(audio, followParent, doRTPitchShifting);
			}
			return obj;
		}

		/// <summary>
		/// Play the provided <see cref="PlayableSoundData"/> using its included settings. A new <see cref="GameObject"/> is created for this sound, which
		/// is returned from the method. Note that a component, <see cref="RealtimeSoundDriver"/>, is attached, which may set the object's position
		/// every frame depending on your <paramref name="flags"/>.
		/// <para/>
		/// While not ordinarily supported, the <paramref name="template"/> <em>can</em> make the sound loop. The <see cref="RealtimeSoundDriver"/> destroys
		/// the sound on the Update() that it's not playing, though.
		/// </summary>
		/// <param name="on">The transform to play the sound at.</param>
		/// <param name="sound">The sound to play.</param>
		/// <param name="flags">Flags that dictate how the sound behaves while playing.</param>
		/// <param name="mixer">The mixer to play on. If null, this uses <see cref="Audio.SFXMixer"/>.</param>
		/// <returns></returns>
		public static GameObject PlaySound(Transform on, PlayableSoundData sound, AudioSource? template) {
			Log.Trace($"Playing sound: {sound.sound.name}...");
			GameObject obj = new GameObject();
			if (on == null) {
				on = Camera.main.gameObject.transform;
			}

			obj.transform.parent = on;
			obj.transform.localPosition = Vector3.zero;

			SoundFlags flags = sound.playTechnique;
			AudioMixerGroup? mixer = sound.mixer;

			bool doPitchShifting = flags.HasFlag(SoundFlags.PitchShift);
			bool doRTPitchShifting = flags.HasFlag(SoundFlags.RealtimePitchShift);
			bool followParent = flags.HasFlag(SoundFlags.FollowEmitter);

			float pitchMod = sound.pitch;
			if (doPitchShifting) {
				pitchMod *= Time.timeScale;
			}

			AudioSource audio = obj.AddComponent<AudioSource>();
			audio.outputAudioMixerGroup = mixer ? mixer : Audio.SFXMixer;
			audio.volume = sound.volume;
			audio.pitch = pitchMod;
			audio.loop = false;
			audio.playOnAwake = false;
			audio.clip = sound.sound;
			if (template == null || template.name == "Nearby Spatial Source" || template.name == "Global Source") {
				SetupDefaultNearbyCurves(audio, false);
			} else {
				CopyCurves(audio, template);
			}
			audio.Play();

			if (!doRTPitchShifting && !followParent && !audio.loop) {
				UnityEngine.Object.Destroy(obj, sound.sound.length + 0.2f); // Just destroy at the default time, plus some buffer room.
			} else {
				RealtimeSoundDriver.StartDriving(audio, followParent, doRTPitchShifting);
			}
			return obj;
		}

		/// <summary>
		/// This method edits the curves of an audiosource such that it plays using the preset "Nearby Audio Source" or "Global Source" sound
		/// types used in the code. It originally existed in my private Chimera mod.
		/// </summary>
		/// <param name="src"></param>
		/// <param name="isPlayingAs2D"></param>
		private static void SetupDefaultNearbyCurves(AudioSource src, bool isPlayingAs2D) {
			if (!isPlayingAs2D) {
				// 3D Audio
				UnhollowerBaseLib.Il2CppStructArray<Keyframe> rolloff = new UnhollowerBaseLib.Il2CppStructArray<Keyframe>(3);
				rolloff[0] = new Keyframe(0.1f, 1f, 0f, 0f, 0.3333333f, 0.3333333f);
				rolloff[1] = new Keyframe(0.3540465f, 0.2750549f, -1.329637f, -1.329637f, 0.3333333f, 0.3333333f);
				rolloff[2] = new Keyframe(1f, 0f, -0.1762998f, -0.1762998f, 0.3204496f, 0.3333333f);

				UnhollowerBaseLib.Il2CppStructArray<Keyframe> spread = new UnhollowerBaseLib.Il2CppStructArray<Keyframe>(2);
				spread[0] = new Keyframe(0f, 0.5f, -0.3094383f, -0.3094383f, 0.4783377f, 0f);
				spread[1] = new Keyframe(1f, 0f, 0f, 0f, 0.3333333f, 0.3333333f);

				UnhollowerBaseLib.Il2CppStructArray<Keyframe> spatialBlend = new UnhollowerBaseLib.Il2CppStructArray<Keyframe>(4);
				spatialBlend[0] = new Keyframe(0f, 0f, 0f, 0f, 0.3333333f, 0.3333333f);
				spatialBlend[1] = new Keyframe(0.1896423f, 0.8151474f, 1.837119f, 1.837119f, 0.07648899f, 0.07007767f);
				spatialBlend[2] = new Keyframe(0.4214395f, 0.9818153f, 0.1665137f, 0.1665137f, 0.6917831f, 0f);
				spatialBlend[3] = new Keyframe(1f, 1f, 0f, 0f, 0.3333333f, 0.3333333f);

				UnhollowerBaseLib.Il2CppStructArray<Keyframe> reverbZoneMix = new UnhollowerBaseLib.Il2CppStructArray<Keyframe>(3);
				reverbZoneMix[0] = new Keyframe(0f, 0f, 0f, 0f, 0.3333333f, 0.3333333f);
				reverbZoneMix[1] = new Keyframe(0.3287202f, 0.1654926f, 1.535669f, 1.535669f, 0.3333333f, 0.3333333f);
				reverbZoneMix[2] = new Keyframe(0.4605339f, 1f, 0.04093663f, 0.04093663f, 0.1604803f, 0f);

				src.rolloffMode = AudioRolloffMode.Custom;
				src.SetCustomCurve(AudioSourceCurveType.CustomRolloff, new AnimationCurve(rolloff));
				src.SetCustomCurve(AudioSourceCurveType.Spread, new AnimationCurve(spread));
				src.SetCustomCurve(AudioSourceCurveType.SpatialBlend, new AnimationCurve(spatialBlend));
				src.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, new AnimationCurve(reverbZoneMix));
			} else {
				// 2D Audio - play without spatial blending and without reverb.
				src.spatialBlend = 0f;
				src.reverbZoneMix = 0f;
			}
			src.maxDistance = 7f;
		}
		private static void CopyCurves(AudioSource destination, AudioSource template) {
			destination.rolloffMode = AudioRolloffMode.Custom;
			destination.priority = template.priority;
			destination.bypassEffects = template.bypassEffects;
			destination.bypassListenerEffects = template.bypassListenerEffects;
			destination.bypassReverbZones = template.bypassReverbZones;
			destination.maxDistance = template.maxDistance;
			destination.minDistance = template.minDistance;
			destination.rolloffMode = template.rolloffMode;
			destination.velocityUpdateMode = template.velocityUpdateMode;
			destination.loop = template.loop;
			destination.SetCustomCurve(AudioSourceCurveType.CustomRolloff, destination.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
			destination.SetCustomCurve(AudioSourceCurveType.Spread, destination.GetCustomCurve(AudioSourceCurveType.Spread));
			destination.SetCustomCurve(AudioSourceCurveType.SpatialBlend, destination.GetCustomCurve(AudioSourceCurveType.SpatialBlend));
			destination.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, destination.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix));
		}

		/// <summary>
		/// Controls how a sound is played.
		/// </summary>
		[Flags]
		public enum SoundFlags {

			/// <summary>
			/// No unique behaviors.
			/// </summary>
			None = 0,

			/// <summary>
			/// This sound must follow the object that it emits at. If not defined, the sound will "float" where it is emitted and never move.
			/// </summary>
			FollowEmitter = 1 << 0,

			/// <summary>
			/// This sound shifts its pitch with the game's timescale upon creation. See also: <see cref="RealtimePitchShift"/>
			/// </summary>
			PitchShift = 1 << 1,

			/// <summary>
			/// This sound shifts its pitch with in-game timescaling in real time, rather than finding the timescale on creation and keeping that pitch (which
			/// sounds in game do by default).
			/// </summary>
			RealtimePitchShift = PitchShift | (1 << 2),

		}
	}
}
