using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvatarStatExtender {
	public static class Prefs {

		public static bool TraceLogging => _traceLogging.Value;

		public static bool OverrideAvatarStatsLoader => _overrideAvatarStatsLoader.Value;

		private static MelonPreferences_Entry<bool> _traceLogging;

		private static MelonPreferences_Entry<bool> _overrideAvatarStatsLoader;

		internal static void Initialize() {
			MelonPreferences_Category mainCat = MelonPreferences.CreateCategory("extendedAvyDriver_main");
			mainCat.DisplayName = "Main Settings";

			_traceLogging = mainCat.CreateEntry(
				"traceLogging",
				true,
				"Trace Logging",
				$"Trace Logging causes extremely (like, comically) specific things to be written into the console.{Environment.NewLine}" +
				"This tends to flood the console with information that is useless normally, but it is very useful for debugging."
			);

			_overrideAvatarStatsLoader = mainCat.CreateEntry(
				"overrideASL",
				true,
				"Override Avatar Stats Loader Mod",
				$"If true, and if FirEmerald's \"Avatar Stats Loader\" mod is installed, this mod will override custom{Environment.NewLine}" +
				$"stats that are saved in ASL's preferences. If false, any customizations from ASL are ignored and the{Environment.NewLine}" +
				$"settings defined in the avatar itself (for this mod) are always used."
			);

		}

	}
}
