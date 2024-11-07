using XansTools.Patching;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using XansTools.Data;
using XansTools;

[assembly: MelonInfo(typeof(XTCore), "Xan's Tools", "2.0.0", "Xan")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
[assembly: MelonPriority(1)] // Priority 1 instead of 0 so this loads late.
namespace XansTools {
	public class XTCore : MelonMod {

		public override void OnInitializeMelon() {
			base.OnInitializeMelon();
			Prefs.Initialize(); // MUST be the first action in my code.
			Log.Initialize(LoggerInstance);
			Player.Initialize(HarmonyInstance);
			DamageReactionFacilitator.Patch();

			//MethodBase test = methodof(OnInitializeMelon);

		}

	}
}
