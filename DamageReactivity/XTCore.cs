using XansTools.Patching;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace XansTools {
	public class XTCore : MelonMod {

		public override void OnInitializeMelon() {
			base.OnInitializeMelon();
			Prefs.Initialize(); // MUST be the first action in my code.
			Log.Initialize(LoggerInstance);
			DamageReactionFacilitator.Patch();

			//MethodBase test = methodof(OnInitializeMelon);

		}

	}
}
