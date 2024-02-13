using MelonLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvatarStatExtender {

	/// <summary>
	/// Provides a wider variety of logging methods for convenience.
	/// </summary>
	public static class Log {

		[AllowNull]
		private static MelonLogger.Instance _log;

		internal static void Initialize(MelonLogger.Instance log) {
			_log = log;
		}

		/// <summary>
		/// Log a standard message in white text.
		/// </summary>
		/// <param name="msg"></param>
		public static void Info(object msg) {
			_log.Msg(msg);
		}

		/// <summary>
		/// Log a standard message in white text.
		/// </summary>
		/// <param name="msg"></param>
		public static void Info(string msg) {
			_log.Msg(msg);
		}

		/// <summary>
		/// Log a warning message in yellow text.
		/// </summary>
		/// <param name="msg"></param>
		public static void Warn(object msg) {
			_log.Warning(msg);
		}

		/// <summary>
		/// Log a warning message in yellow text.
		/// </summary>
		/// <param name="msg"></param>
		public static void Warn(string msg) {
			_log.Warning(msg);
		}

		/// <summary>
		/// Log an error message in red text.
		/// </summary>
		/// <param name="msg"></param>
		public static void Error(object msg) {
			_log.Error(msg);
		}

		/// <summary>
		/// Log an error message in red text.
		/// </summary>
		/// <param name="msg"></param>
		public static void Error(string msg) {
			_log.Error(msg);
		}

		/// <summary>
		/// Log a debugging message in gray text. Always prefixed with [DEBUG]
		/// </summary>
		/// <param name="msg"></param>
		public static void Debug(object msg) {
			_log.Msg(ConsoleColor.Gray, $"[DEBUG] {msg}");
		}

		/// <summary>
		/// Log a debugging message in gray text. Always prefixed with [DEBUG]
		/// </summary>
		/// <param name="msg"></param>
		public static void Debug(string msg) {
			_log.Msg(ConsoleColor.Gray, $"[DEBUG] {msg}");
		}

		/// <summary>
		/// If <see cref="Prefs.TraceLogging"/> is enabled, write a trace message in dark gray text. Always prefixed with [TRACE].
		/// </summary>
		/// <param name="msg"></param>
		public static void Trace(object msg) {
			if (Prefs.TraceLogging) {
				_log.Msg(ConsoleColor.DarkGray, $"[TRACE] {msg}");
			}
		}

		/// <summary>
		/// If <see cref="Prefs.TraceLogging"/> is enabled, write a trace message in dark gray text. Always prefixed with [TRACE].
		/// </summary>
		/// <param name="msg"></param>
		public static void Trace(string msg) {
			if (Prefs.TraceLogging) {
				_log.Msg(ConsoleColor.DarkGray, $"[TRACE] {msg}");
			}
		}

	}
}
