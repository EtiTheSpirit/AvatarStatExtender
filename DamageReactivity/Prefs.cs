using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XansTools {
	public static class Prefs {

		public static bool TraceLogging => _traceLogging.Value;

		private static MelonPreferences_Entry<bool> _traceLogging;
		internal static void Initialize() {
			MelonPreferences_Category mainCat = MelonPreferences.CreateCategory("xanstools_main");
			mainCat.DisplayName = "Main Settings";

			_traceLogging = mainCat.CreateEntry(
				"traceLogging",
				true,
				"Trace Logging",
				$"Trace Logging causes extremely (like, comically) specific things to be written into the console.{Environment.NewLine}" +
				"This tends to flood the console with information that is useless normally, but it is very useful for debugging."
			);

		}

	}
}
