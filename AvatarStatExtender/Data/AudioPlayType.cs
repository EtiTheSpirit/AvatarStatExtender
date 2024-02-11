#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarStatExtender.Data {
	/// <summary>
	/// What type of audio gets played?
	/// </summary>
	public enum AudioPlayType {
		[InspectorName("Declare a custom delegate...")]
		Custom,

		[InspectorName("Choose One Randomly")]
		Random,

		[InspectorName("Play All At Once")]
		AllTogether
	}
}