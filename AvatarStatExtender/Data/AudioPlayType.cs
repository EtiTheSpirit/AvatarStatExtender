#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarStatExtender.Data {
	/// <summary>
	/// What type of audio gets played?
	/// </summary>
	public enum AudioPlayType {

		/// <summary>
		/// The audio that gets played is chosen by a custom delegate by name, which must be registered via
		/// <see cref="API.SoundAPI.RegisterSoundPlayTypeSelectionHandler(string, API.SoundAPI.AudioSelectionDelegate)"/>
		/// </summary>
		[InspectorName("Declare a custom delegate...")]
		Custom,

		/// <summary>
		/// The system automatically selects one sound out of the list associated with the group.
		/// </summary>
		[InspectorName("Choose One Randomly")]
		Random,

		/// <summary>
		/// The system selects all sounds out of the group.
		/// </summary>
		[InspectorName("Play All At Once")]
		AllTogether
	}
}