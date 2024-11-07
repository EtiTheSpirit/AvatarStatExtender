#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace AvatarStatExtender.Data {

	[Serializable]
	public sealed class SerializableAudioArray {

		internal static void CallToStaticallyInitialize() { }

		public List<AudioClip> clips;

		/// <summary>
		/// A non-null reference to <see cref="clips"/>.
		/// </summary>
		public List<AudioClip> Clips {
			[MemberNotNull(nameof(clips))]
			get => clips ??= new List<AudioClip>();

			[MemberNotNull(nameof(clips))]
			set {
				clips = value ?? new List<AudioClip>();
			}
		}

		public AudioClip this[int index] {
			get => Clips[index];
			set => Clips[index] = value;
		}

		public SerializableAudioArray() {
			clips = new List<AudioClip>();
		}

	}

	[Serializable]
	public sealed class SerializableAudioArray2D {

		internal static void CallToStaticallyInitialize() { }

		public SerializableAudioArray[] children;

		/// <summary>
		/// A non-null reference to <see cref="children"/>.
		/// </summary>
		public SerializableAudioArray[] Children {
			[MemberNotNull(nameof(children))]
			get => children ??= Array.Empty<SerializableAudioArray>();

			[MemberNotNull(nameof(children))]
			set {
				children = value ?? Array.Empty<SerializableAudioArray>();
			}
		}

		public SerializableAudioArray this[int index] {
			get => Children[index];
			set => Children[index] = value;
		}

		public SerializableAudioArray2D() {
			children = Array.Empty<SerializableAudioArray>();
		}
	}

}