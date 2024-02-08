using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExtendedAvatarStatDriver.Components {

	/// <summary>
	/// Stores values relating to jumping.
	/// </summary>
	[RegisterTypeInIl2Cpp]
	public sealed class JumpTracker : MonoBehaviour {

		public JumpTracker(IntPtr @this) : base(@this) { }

		private bool _hasPressedJumpButton = false;
		private bool _hasAlreadyJumped = false;
		private bool _isOnGround = false;

		public void MarkPlayerOnGround(bool isGrounded) {
			_isOnGround = isGrounded;
		}

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
