#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvatarStatExtender.Data {

	/// <summary>
	/// Provides utilities for the <see cref="AudioEventType"/> enum.
	/// </summary>
	public static class AudioEventTypeExt {

		/// <summary>
		/// The highest bit <em>index</em> that represents a valid <see cref="AudioEventType"/>.
		/// </summary>
		public static readonly int MAX_FLAG;

		/// <summary>
		/// All defined single bit values of <see cref="AudioEventType"/>, excluding <see cref="AudioEventType.Custom"/>.
		/// </summary>
		public static readonly ReadOnlyReferenceSpan<AudioEventType> ALL_FLAGS;

		/// <summary>
		/// The event names of each audio event type, synchronized with <see cref="ALL_FLAGS"/> (meaning it excludes custom as well).
		/// </summary>
		public static readonly ReadOnlyReferenceSpan<string> ALL_FLAGS_EVENT_NAMES;

		private static readonly Dictionary<AudioEventType, string> _nameLookup;
		private static readonly Dictionary<string, AudioEventType> _valueLookup;

		static AudioEventTypeExt() {
			Type audioEventType = typeof(AudioEventType);
			AudioEventType[] allFlags = new AudioEventType[32];
			string[] names = new string[32];

			Log.Trace("Starting up the audio type extension.");

			// EventNameAttribute
			int maxFlag = 0;
			for (int i = 0; i < 32; i++) {
				AudioEventType current = (AudioEventType)(1 << i);
				if (Enum.IsDefined(audioEventType, current)) {
					maxFlag = i;
					allFlags[i] = current;
					FieldInfo fld = audioEventType.GetField(Enum.GetName(audioEventType, current));
					EventNameAttribute attr = fld.GetCustomAttribute<EventNameAttribute>();
					if (attr != null) {
						names[i] = attr.name;
					} else {
						names[i] = string.Empty;
					}
					Log.Trace($"Associated {current} as {names[i]}.");
				} else {
					Log.Trace($"Found all of the types ({current} is not a defined value)");
					break;
				}
			}
			Array.Resize(ref allFlags, maxFlag + 1);
			Array.Resize(ref names, maxFlag + 1);
			MAX_FLAG = maxFlag;
			ALL_FLAGS = new ReadOnlyReferenceSpan<AudioEventType>(allFlags);
			ALL_FLAGS_EVENT_NAMES = new ReadOnlyReferenceSpan<string>(names);

			_nameLookup = new Dictionary<AudioEventType, string>();
			_valueLookup = new Dictionary<string, AudioEventType>();
			for (int i = 0; i <= maxFlag; i++) {
				_nameLookup[ALL_FLAGS[i]] = ALL_FLAGS_EVENT_NAMES[i];
				_valueLookup[ALL_FLAGS_EVENT_NAMES[i]] = ALL_FLAGS[i];
			}
		}

		/// <summary>
		/// Returns the event name of the provided single flag.
		/// </summary>
		/// <param name="flag"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static bool TryGetNameFromSingleFlag(AudioEventType flag, out string? result) {
			if ((flag & (flag - 1)) != 0) throw new ArgumentException($"There is more than one flag set ({flag:G}).");
			if (flag > ALL_FLAGS[ALL_FLAGS.Length - 1]) {
				result = null;
				return false;
			}
			return _nameLookup.TryGetValue(flag, out result);
		}

		/// <summary>
		/// Returns the flag associated 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static bool TryGetFromName(string name, out AudioEventType result) {
			return _valueLookup.TryGetValue(name, out result);
		}

	}
}
