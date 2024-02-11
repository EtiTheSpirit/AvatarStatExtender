#nullable enable
using AvatarStatExtender.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;

namespace AvatarStatExtender.Data {
	public sealed class ReadOnlyAudioEntry {

		/// <summary>
		/// This array keeps the audio clips from being garbage collected on the il2cpp side.
		/// <para/>
		/// Without this, the audio might be GCed anyway because il2cpp has its own separate collector.
		/// Managed objects serve more as "proxies" to these objects, providing outside access to native memory.
		/// By storing them in the il2cpp array, a reference is kept on the il2cpp side, preventing garbage collection.
		/// </summary>
		private readonly Il2CppReferenceArray<AudioClip> _keepAlive;
		private readonly Il2CppReferenceArray<AudioSource>? _sourceKeepAlive;

		/// <summary>
		/// The name of this audio group.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// If true, use <see cref="CustomPlayType"/>. Note that it has not been sanitized.
		/// </summary>
		public bool UseCustomPlayType => PlayType == AudioPlayType.Custom;

		/// <summary>
		/// The technique that this audio group uses to play.
		/// </summary>
		public AudioPlayType PlayType { get; }

		/// <summary>
		/// The custom delegate that this will invoke to determine how to play audio.
		/// </summary>
		public string? CustomPlayType { get; }

		/// <summary>
		/// The type of event that this audio reacts to. Note that unlike the editor counterpart,
		/// this includes vanilla events that were listed in <see cref="CustomEventTypes"/>.
		/// </summary>
		public AudioEventType EventType { get; }

		/// <summary>
		/// A string based event that this audio reacts to. Consider using <see cref="ResolvedCustomEvents"/>.
		/// <para/>
		/// Note that this <strong>must</strong> be all lowercase (and will assert such) for the sake of better compatibility between mods.
		/// If your damage event is generic, try coming up with as generalized a name as possible.
		/// <para/>
		/// If you wish to support multiple events, separate them with semicolon <c>;</c> like so: <c>yeeted;sent_to_shadow_realm;obliterated;deleted</c>
		/// </summary>
		public string? CustomEventTypes { get; }

		/// <summary>
		/// This list contains the evaluated and sanitized array resulting from <see cref="CustomEventTypes"/>.
		/// <para/>
		/// <strong>This does NOT contain vanilla event names that may have been
		/// manually typed in - those have been merged into <see cref="EventType"/>!</strong>
		/// </summary>
		public ReadOnlyHashSet<string> ResolvedCustomEvents { get; }

		/// <summary>
		/// The list of sounds in this entry.
		/// </summary>

		public IReadOnlyList<AudioClip> Sounds { get; }

		/// <summary>
		/// The range of pitch that automatic sound playing can use, (min, max).
		/// </summary>
		public Vector2 PitchRange { get; }

		/// <summary>
		/// The volume of this slider.
		/// </summary>
		public float Volume { get; }

		/// <summary>
		/// If defined, this will override the template audio source used by default when playing sounds.
		/// <para/>
		/// This is a clone of the original template object.
		/// </summary>
		public AudioSource? OverrideTemplateAudioSource { get; }


		internal ReadOnlyAudioEntry(AudioEntry real) {
			Name = real.Name;
			PlayType = real.PlayType;
			CustomPlayType = real.CustomPlayType;
			CustomEventTypes = real.CustomEventTypes;
			Sounds = new ReadOnlyCollection<AudioClip>(new List<AudioClip>(real.Sounds)); // Read only clone.
			PitchRange = real.PitchRange;
			Volume = real.Volume;
			OverrideTemplateAudioSource = OverrideTemplateAudioSource ? UnityEngine.Object.Instantiate(OverrideTemplateAudioSource) : null;

			SoundBlockNameSanitizer.CreateStringAndEnumMix(CustomEventTypes ?? string.Empty, out string[] customs, out AudioEventType eventType);
			EventType = eventType | real.EventType;
			ResolvedCustomEvents = new ReadOnlyHashSet<string>(customs);

			_keepAlive = new Il2CppReferenceArray<AudioClip>(Sounds.ToArray());
			if (OverrideTemplateAudioSource != null) {
				_sourceKeepAlive = new Il2CppReferenceArray<AudioSource>(new AudioSource[] { OverrideTemplateAudioSource });
			}
		}

		
	}
}
