using System;
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AvatarStatExtender.Data {
	[Flags]
	public enum AudioEventType {

		[InspectorName("Declare a custom event name...")]
		Custom          = 0,

		[InspectorName("Projectile Damage (hitbullet)"), EventName("hitbullet")]
		HitBullet       = 1 << 0,

		[InspectorName("Non-Projectile Damage (hitblunt)"), EventName("hitblunt")]
		HitBlunt        = 1 << 1,

		[InspectorName("Any Damage (hitany)"), EventName("hitany")]
		HitAny			= 1 << 2,

		[InspectorName("Switching Avatar (spawn)"), EventName("spawn")]
		Spawn           = 1 << 3,
	}



	[AttributeUsage(AttributeTargets.Field)]
	public sealed class EventNameAttribute : Attribute {

		public readonly string name;

		public EventNameAttribute(string name) {
			this.name = name;
		}
	}
}