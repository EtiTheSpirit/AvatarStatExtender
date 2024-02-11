#nullable enable
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SLZAvatar = SLZ.VRMK.Avatar;

namespace AvatarStatExtender.Components {

	[RegisterTypeInIl2Cpp]
	internal sealed class QuickPrefabIdentifier : MonoBehaviour {
	
		public QuickPrefabIdentifier(IntPtr @this) : base(@this) { }

		public SLZAvatar? Prefab { get; set; }

	}
}
