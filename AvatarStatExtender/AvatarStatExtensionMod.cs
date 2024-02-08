using AvatarStatExtender.Tools;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriangleNet;

namespace AvatarStatExtender {
	public class AvatarStatExtensionMod : MelonMod {

		public override void OnInitializeMelon() {
			base.OnInitializeMelon();
			Log.Initialize(LoggerInstance);
			Log.Info("Initializing the avatar extender...");

			Log.Debug("Creating Harmony...");
			HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("Extended Avatar Driver");

			Log.Debug("Injecting fields from the stat component...");
			FieldInjector.SerialisationHandler.Inject<AvatarStatDriver>();

			Log.Debug("Injection complete. Preparing the stat marshaller...");
			StatMarshaller.Initialize(harmony);
		}

	}
}
