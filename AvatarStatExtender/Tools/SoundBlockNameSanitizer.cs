#nullable enable
using AvatarStatExtender.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvatarStatExtender.Tools {
	public static class SoundBlockNameSanitizer {

		private static readonly char[] SEMICOLON = { ';' };

		/// <summary>
		/// This regex matches sound event IDs.
		/// <para/>
		/// <c>[a-z0-9_\-\.]+(?:(?:\:)[a-z0-9_\-\.]+)?</c>
		/// </summary>
		public static readonly Regex SOUND_REGEX = new Regex(@"[a-z0-9_\-\.]+(?:(?:\:)[a-z0-9_\-\.]+)?");

		/// <summary>
		/// Takes a single sound (<em>not</em> a semicolon separated list!) and returns whether or not that name is valid.
		/// </summary>
		/// <param name="unfilteredSingle"></param>
		/// <returns></returns>
		public static bool IsSane(string unfilteredSingle) {
			Match match = SOUND_REGEX.Match(unfilteredSingle);
			return match.Success && match.Value == unfilteredSingle;
		}

		/// <summary>
		/// Provided with the raw, unfiltered string for a sound's event name(s), this will split it into
		/// an array only containing valid sounds as per the rules of sound event formatting.
		/// <para/>
		/// You can use <see href="https://regexr.com"/> to explain the regex (see <see cref="SOUND_REGEX"/>) to you. If you are in a future
		/// where <see href="https://regexr.com"/> no longer exists, God help you.
		/// </summary>
		/// <param name="unfiltered"></param>
		/// <returns></returns>
		public static string[] GetSanitizedEventList(string unfiltered) {
			if (unfiltered.Length == 0) return Array.Empty<string>();
			string[] split = unfiltered.Split(SEMICOLON, StringSplitOptions.RemoveEmptyEntries);
			string[] result = new string[split.Length];

			int resultIndex = 0;
			for (int i = 0; i < split.Length; i++) {
				string current = split[i].Trim();
				if (string.IsNullOrEmpty(current)) continue;
				if (current.Length > 50) continue;
				current = current.ToLower();

				Match match = SOUND_REGEX.Match(current);
				if (match.Success && match.Value == current) {
					// It matched something in current, and that match is identical to current (thus, the entirety of current is valid).
					result[resultIndex++] = current; // OK.
				}
			}

			Array.Resize(ref result, resultIndex);
			return result;
		}

		/// <summary>
		/// Provided with the raw, unfiltered string for a sound's event name(s), this will split it into
		/// an array only containing valid sounds as per the rules of sound event formatting.
		/// <para/>
		/// You can use <see href="https://regexr.com"/> to explain the regex (see <see cref="SOUND_REGEX"/>) to you. If you are in a future
		/// where <see href="https://regexr.com"/> no longer exists, God help you.
		/// </summary>
		/// <param name="unfiltered"></param>
		/// <returns></returns>
		public static string[] GetSanitizedEventList(string[] names) {
			if (names.Length == 0) return Array.Empty<string>();
			string[] result = new string[names.Length];

			int resultIndex = 0;
			for (int i = 0; i < names.Length; i++) {
				string current = names[i].Trim();
				if (string.IsNullOrEmpty(current)) continue;
				if (current.Length > 50) continue;
				current = current.ToLower();

				Match match = SOUND_REGEX.Match(current);
				if (match.Success && match.Value == current) {
					// It matched something in current, and that match is identical to current (thus, the entirety of current is valid).
					result[resultIndex++] = current; // OK.
				}
			}

			Array.Resize(ref result, resultIndex);
			return result;
		}

		/// <summary>
		/// Provided with the raw, unfiltered string for a sound's event name(s), this will return
		/// an appropriately filtered list. Invalid entries will be removed entirely.
		/// </summary>
		/// <param name="unfiltered"></param>
		/// <returns></returns>
		public static string GetSanitizedEventListJoined(string unfiltered) {
			string[] list = GetSanitizedEventList(unfiltered);
			if (list.Length == 0) return string.Empty;
			if (list.Length == 1) return list[0];
			return string.Join(";", list);
		}

		/// <summary>
		/// Provided with an unfiltered sound event list, this will translate it into a set of sanitized/filtered names,
		/// and vanilla enums that might have been typed into the list manually.
		/// </summary>
		/// <param name="unfiltered"></param>
		/// <param name="customs"></param>
		/// <param name="enums"></param>
		public static void CreateStringAndEnumMix(string unfiltered, out string[] customs, out AudioEventType enums) {
			customs = GetSanitizedEventList(unfiltered);
			enums = default;

			CreateStringAndEnumMix(ref customs, ref enums);
		}
		
		/// <summary>
		/// Provided with an unfiltered sound event list, this will translate it into a set of sanitized/filtered names,
		/// and vanilla enums that might have been typed into the list manually.
		/// </summary>
		/// <param name="unfilteredArray"></param>
		/// <param name="customs"></param>
		/// <param name="enums"></param>
		public static void CreateStringAndEnumMix(string[] unfilteredArray, out string[] customs, out AudioEventType enums) {
			customs = GetSanitizedEventList(unfilteredArray);
			enums = default;

			CreateStringAndEnumMix(ref customs, ref enums);
		}

		/// <summary>
		/// Common code for the other two similarly named methods.
		/// </summary>
		/// <param name="customs"></param>
		/// <param name="enums"></param>
		private static void CreateStringAndEnumMix(ref string[] customs, ref AudioEventType enums) {
			// Now I need to pull any values out that mimic vanilla ones.
			int nextWrittenIndex = 0;
			for (int i = 0; i < customs.Length; i++) {
				int indexOfName = AudioEventTypeExt.ALL_FLAGS_EVENT_NAMES.IndexOf(customs[i]);
				if (indexOfName < 0) {
					// This is a custom event. Write this to the next available
					// index of the customs array.
					// nextWrittenIndex will never be greater than i, so I can just rewrite to this array
					// without risk of affecting the check being done here.
					// This is a clever technique to not allocate a new array when removing elements, even
					// though these elements are unordered.
					customs[nextWrittenIndex++] = customs[i];
				} else {
					// Now if it *isn't* a custom event name, it needs to be stored into EventType
					enums |= AudioEventTypeExt.ALL_FLAGS[indexOfName];
					// Notably, nextWrittenIndex IS NOT incremented here.
					// This means that the next valid custom event that does end up getting written will replace
					// whatever this was.
				}
			}

			// And then to finalize, we just resize:
			Array.Resize(ref customs, nextWrittenIndex);
		}

	}
}
