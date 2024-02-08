using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvatarStatExtender {

	/// <summary>
	/// Provides a wider variety of logging methods for convenience.
	/// </summary>
	public static class Log {

		private static MelonLogger.Instance _log;

		internal static void Initialize(MelonLogger.Instance log) {
			_log = log;
		}

		public static void Info(object msg) {
			_log.Msg(msg);
		}

		public static void Info(string msg) {
			_log.Msg(msg);
		}

		public static void Warn(object msg) {
			_log.Warning(msg);
		}

		public static void Warn(string msg) {
			_log.Warning(msg);
		}

		public static void Error(object msg) {
			_log.Error(msg);
		}

		public static void Error(string msg) {
			_log.Error(msg);
		}

		public static void Debug(object msg) {
			_log.Msg(ConsoleColor.Gray, msg);
		}

		public static void Debug(string msg) {
			_log.Msg(ConsoleColor.Gray, msg);
		}

		public static void Trace(object msg) {
			if (Prefs.TraceLogging) {
				_log.Msg(ConsoleColor.DarkGray, msg);
			}
		}

		public static void Trace(string msg) {
			if (Prefs.TraceLogging) {
				_log.Msg(ConsoleColor.DarkGray, msg);
			}
		}

	}
}
