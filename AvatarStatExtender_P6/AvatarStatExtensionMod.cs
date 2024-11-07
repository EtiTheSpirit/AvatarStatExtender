using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using AvatarStatExtender;
using AvatarStatExtender.BuiltInEvents;
using AvatarStatExtender.Tools;

using MelonLoader;

using UnityEngine;


[assembly: Guid("74cf3aaf-5536-4556-9992-906c2cdb571b")]
[assembly: MelonInfo(typeof(AvatarStatExtensionMod), "Avatar Stat Extender", "2.0.0", "Xan")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
namespace AvatarStatExtender {

	internal class AvatarStatExtensionMod : MelonMod {

		public override void OnInitializeMelon() {
			base.OnInitializeMelon();
			Prefs.Initialize();
			Log.Initialize(LoggerInstance);
			Log.Info("Initializing the avatar extender...");

			if (Prefs.TraceLogging) {
				Log.Warn("TRACE LOGGING IS ENABLED. Your log will be spammed with highly verbose debug statements.");
				Log.Warn("You can disable trace logging in the mod's preferences.");
			}

			Log.Debug("Creating Harmony...");
			HarmonyLib.Harmony harmony = HarmonyInstance;//new HarmonyLib.Harmony("Extended Avatar Driver");

			Log.Debug("Injecting fields from the stat component...");
			FieldInjector.SerialisationHandler.Inject<AvatarStatDriver>();
			FieldInjector.SerialisationHandler.Inject<AvatarExtendedAudioContainer>();

			Log.Debug("Injection complete. Preparing the stat marshaller and audio driver...");
			StatMarshaller.Initialize(harmony);
			SoundBroadcastMarshaller.Initialize(harmony);
		}

	}
}
