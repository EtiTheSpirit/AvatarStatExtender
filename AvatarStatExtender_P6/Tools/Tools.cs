#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace AvatarStatExtender.Tools {
	public static class Tools {
		private static readonly CultureInfo PERCENT_CULTURE;

		static Tools() {
			PERCENT_CULTURE = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
			PERCENT_CULTURE.NumberFormat.PercentPositivePattern = 1;  // Avoid putting a space between a number and its percentage
		}

		/// <summary>
		/// Identical to using C#'s built in <c>P</c> format option, but this doesn't put a space between the number and the %.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="digits"></param>
		/// <returns></returns>
		public static string FormatPercentage(this float value, int digits = 2) {
			string format = $"P{digits}";
			return value.ToString(format, PERCENT_CULTURE);
		}

		/// <summary>
		/// Identical to <see cref="Array.Resize{T}(ref T[], int)"/> but this will fill in
		/// additional space with the provided value (<paramref name="fillWith"/>) if the
		/// array is expanded.
		/// </summary>	
		public static void ResizeArrayDefault<T>(ref T[] array, int newLength, T fillWith) {
			int oldLength = array.Length;
			Array.Resize(ref array, newLength);
			if (oldLength < newLength) {
				for (int i = oldLength; i < newLength; i++) {
					array[i] = fillWith;
				}
			}
		}

	}
}