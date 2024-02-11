#nullable enable
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AvatarStatExtender.Components {

	/// <summary>
	/// Stores values relating to jumping. This is used to keep track of if the player can jump, so that the jump boost
	/// stat knows whether or not it can apply.
	/// </summary>
	[RegisterTypeInIl2Cpp]
	public sealed class JumpTracker : MonoBehaviour {

		public JumpTracker(IntPtr @this) : base(@this) { }

		private bool _hasPressedJumpButton = false;
		private bool _hasAlreadyJumped = false;
		private bool _isOnGround = false;

		/// <summary>
		/// Tell the system that the player is on the ground.
		/// </summary>
		/// <param name="isGrounded"></param>
		public void MarkPlayerOnGround(bool isGrounded) {
			_isOnGround = isGrounded;
		}

		/// <summary>
		/// Tries to switch this object into the state where it thinks it has jumped.
		/// Returns true if the jump force should be applied, false if not.
		/// </summary>
		/// <param name="isJumpButtonPressed"></param>
		/// <returns></returns>
		public bool TryJump(bool isJumpButtonPressed) {
			if (isJumpButtonPressed) {
				_hasPressedJumpButton = true;
				if (_isOnGround) {
					_hasAlreadyJumped = false;
				}
				return false;
			} else {
				bool canJump = false;
				if (_isOnGround && _hasPressedJumpButton && !_hasAlreadyJumped) {
					// On the ground, pressed it as of the last call, has not already jumped...
					_hasAlreadyJumped = true;
					canJump = true;
				} else if (_isOnGround && !_hasPressedJumpButton) {
					// On the ground, was NOT pressing it as of the last call
					_hasAlreadyJumped = false; // Reset this, we can jump again.
				}
				_hasPressedJumpButton = false;
				return canJump;
			}
		}

	}
}
