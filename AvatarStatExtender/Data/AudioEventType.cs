using System;
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AvatarStatExtender.Data {

	/// <summary>
	/// Built in audio event types, which are used to play sounds when certain things happen in game.
	/// </summary>
	[Flags]
	public enum AudioEventType {

		/// <summary>
		/// Denotes a custom, string-based event type will be used instead of this enum.
		/// </summary>
		[InspectorName("Declare a custom event name...")]
		Custom          = 0,

		/// <summary>
		/// This event fires when the avatar gets hit by something with the "Piercing" damage type,
		/// which is typically reserved for bullets and projectiles..
		/// </summary>
		[InspectorName("Projectile Damage (hitbullet)"), EventName("hitbullet")]
		HitBullet       = 1 << 0,

		/// <summary>
		/// This event fires when the avatar gets hit by any non-piercing damage type.
		/// </summary>
		[InspectorName("Non-Projectile Damage (hitblunt)"), EventName("hitblunt")]
		HitBlunt        = 1 << 1,

		/// <summary>
		/// This event fires when the avatar takes any damage.
		/// </summary>
		[InspectorName("Any Damage (hitany)"), EventName("hitany")]
		HitAny			= 1 << 2,

		/// <summary>
		/// This event fires when the avatar spawns.
		/// </summary>
		[InspectorName("Switching Avatar (spawn)"), EventName("spawn")]
		Spawn           = 1 << 3,
	}


	/// <summary>
	/// This attribute is used by the system to bind a preset string event name to an enum event.
	/// </summary>

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class EventNameAttribute : Attribute {

		/// <summary>
		/// The string-based name of an event.
		/// </summary>
		public readonly string name;

		/// <summary>
		/// Construct a new attribute denoting the string-based name of an audio event.
		/// </summary>
		/// <param name="name"></param>
		public EventNameAttribute(string name) {
			this.name = name;
		}
	}
}