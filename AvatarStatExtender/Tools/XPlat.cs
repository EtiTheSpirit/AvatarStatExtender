#nullable enable
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvatarStatExtender.Tools {
	internal static class XPlat {

		public const MelonPlatformAttribute.CompatiblePlatforms META_QUEST = (MelonPlatformAttribute.CompatiblePlatforms)3;

		/// <summary>
		/// True if this user is running on an Oculus Quest
		/// </summary>
		public static bool IsQuestie => MelonUtils.CurrentPlatform == META_QUEST;

	}
}
