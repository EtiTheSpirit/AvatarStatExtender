#nullable enable

namespace XansTools.Data {

	/// <summary>
	/// Represents when an event has executed.
	/// </summary>
	public enum EventPhase {

		/// <summary>
		/// This event fired before the original method executed.
		/// </summary>
		Before,

		/// <summary>
		/// This event fired after the original method executed.
		/// </summary>
		After

	}
}
