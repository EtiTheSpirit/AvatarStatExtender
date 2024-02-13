using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AvatarStatExtender.Data {

	/// <summary>
	/// Which audio mixer does this target?
	/// </summary>
	public enum AudioMixerTarget {

		/// <summary>
		/// Target the master volume mixer.
		/// </summary>
		[InspectorName("Master")]
		Master,

		/// <summary>
		/// Target a mixer specific to gunshot SFX.
		/// </summary>
		[InspectorName("Gunshot")]
		Gunshot,

		/// <summary>
		/// Target the mixer for music.
		/// </summary>
		[InspectorName("Music")]
		Music,

		/// <summary>
		/// Target the mixer for general sound effects.
		/// </summary>
		[InspectorName("Other SFX")]
		SFX,

	}
}