#nullable enable
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AvatarStatExtender.Components {

	/// <summary>
	/// The realtime sound driver handles moving 3D sounds in realtime, changing their pitch in realtime with the timescale,
	/// and disposing of the sound once it has finished playing.
	/// </summary>
	[RegisterTypeInIl2Cpp]
	public sealed class RealtimeSoundDriver : MonoBehaviour {

		/// <summary>
		/// Internal constructor for il2cpp. This creates a managed representation of a Unity object
		/// being stored in unmanaged memory via the provided pointer.
		/// </summary>
		/// <param name="this">The location of the unmanaged object that this represents.</param>
		public RealtimeSoundDriver(IntPtr @this) : base(@this) { }

		[AllowNull]
		private AudioSource _src;
		private float _originalPitch = 1f;

		/// <summary>
		/// If true, this sound will match its pitch to the timescale in real time.
		/// </summary>
		public bool pitchShiftInRealtime = false;

		/// <summary>
		/// If true, this sound will update its transform to be at its parent transform, 
		/// plus the <see cref="LocalOffset"/> and <see cref="GlobalOffset"/>
		/// </summary>
		public bool followParent = false;

		/// <summary>
		/// A local position offset for the transform. This is applied first.
		/// <para/>
		/// This only works if you have it set to <see cref="followParent"/>.
		/// </summary>
		public Vector3 LocalOffset { get; set; }

		/// <summary>
		/// A global position offset for the transform. This is applied after the local transform.
		/// Note that this has a slight performance cost for nested sounds, as it must use matrix math
		/// to evaluate the new position.
		/// <para/>
		/// This only works if you have it set to <see cref="followParent"/>.
		/// </summary>
		public Vector3 GlobalOffset {
			get => _globalOffset.GetValueOrDefault();
			set {
				if (value == Vector3.zero) {
					_globalOffset = null;
				} else {
					_globalOffset = value;
				}
			}
		}
		private Vector3? _globalOffset;

		/// <summary>
		/// Adds a component to a sound emitter and sets up its properties.
		/// </summary>
		/// <param name="sound">The object emitting the sound.</param>
		/// <param name="followParent">If true, this will follow its parent.</param>
		/// <param name="pitchShiftInRealtime">Whether or not the pitch should be driven in real time.</param>
		public static void StartDriving(AudioSource sound, bool followParent, bool pitchShiftInRealtime) {
			RealtimeSoundDriver driver = sound.gameObject.AddComponent<RealtimeSoundDriver>();
			driver.pitchShiftInRealtime = pitchShiftInRealtime;
			driver.followParent = followParent;
		}

		private void Start() {
			_src = GetComponent<AudioSource>();
			_originalPitch = _src.pitch;
		}

		private void Update() {
			if (_src == null) {
				Destroy(gameObject);
				return;
			}
			if (!_src.isPlaying) {
				Destroy(gameObject);
				return;
			}
			if (followParent) {
				transform.localPosition = LocalOffset;
				if (_globalOffset.HasValue) {
					// More math :(
					if (transform.parent != null) {
						Vector3 offset = transform.parent.localToWorldMatrix * _globalOffset.Value;
						transform.localPosition += offset;
					} else {
						transform.localPosition += _globalOffset.Value;
					}
				}
			}
			if (pitchShiftInRealtime) {
				_src.pitch = _originalPitch * Time.timeScale;
			}

		}

	}
}
