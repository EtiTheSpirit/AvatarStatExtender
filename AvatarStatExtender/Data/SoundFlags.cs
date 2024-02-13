using System;

namespace AvatarStatExtender.Data {
	/// <summary>
	/// Controls how a sound is played.
	/// </summary>
	[Flags]
	public enum SoundFlags {

		/// <summary>
		/// No unique behaviors.
		/// </summary>
		None = 0,

		/// <summary>
		/// This sound must follow the object that it emits at. If not defined, the sound will "float" where it is emitted and never move.
		/// </summary>
		FollowEmitter = 1 << 0,

		/// <summary>
		/// This sound shifts its pitch with the game's timescale upon creation. See also: <see cref="RealtimePitchShift"/>.
		/// <para/>
		/// This flag is implicitly set by <see cref="RealtimePitchShift"/>.
		/// </summary>
		PitchShift = 1 << 1,

		/// <summary>
		/// This sound shifts its pitch with in-game timescaling in real time, rather than finding the timescale on creation and keeping that pitch (which
		/// sounds in game do by default).
		/// <para/>
		/// If set, <see cref="PitchShift"/> is also implicitly treated as set.
		/// </summary>
		RealtimePitchShift = 1 << 2,

	}
}
