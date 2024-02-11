using SLZ.AI;
using SLZ.Combat;
using SLZ.Marrow.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XansTools.Data {
	/// <summary>
	/// A recreation of the Attack data (see <see cref="Attack"/>) that fixes the data stored within.
	/// <para/>
	/// This is one of those cases of "I don't know why this fixes it, but it does, and if you use <see cref="Attack"/> it breaks".
	/// I suspect this is caused by some fuckery with <see cref="_backFacing"/> being byte vs. bool and how that plays with
	/// pointer garbage. Will have to research more, but what matters is that this struct fixes it.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct AttackInfo {
		private float _damage;
		private Vector3 _normal;
		private Vector3 _origin;
		private Vector3 _direction;
		private byte _backFacing;
		private int _orderInPool;
		private IntPtr _collider;
		private AttackType _attackType;
		private IntPtr _proxy;

		/// <summary>
		/// The absolute amount of damage this attack did.
		/// </summary>
		public float Damage {
			get => _damage;
			set => _damage = value;
		}

		/// <summary>
		/// The normal of the surface that this attack hit.
		/// </summary>
		public Vector3 Normal {
			get => _normal;
			set => _normal = value;
		}

		/// <summary>
		/// The direction this attack came from.
		/// </summary>
		public Vector3 Direction {
			get => _direction;
			set => _direction = value;
		}

		/// <summary>
		/// The origin of this attack source. Can be combined with direction to create a ray.
		/// </summary>
		public Vector3 Origin {
			get => _origin;
			set => _origin = value;
		}

		/// <summary>
		/// If true, the attack occurred from within the hit area.
		/// </summary>
		public bool IsBackFacing {
			get => _backFacing == 1;
			set => _backFacing = (byte)(value ? 1 : 0);
		}

		public int OrderInAttackPool {
			get => _orderInPool;
			set => _orderInPool = value;
		}

		/// <summary>
		/// The collider that the attack impacted with to result in this damage occurring.
		/// </summary>
		public Collider Collider {
			get => new Collider(_collider);
			set => _collider = value.Pointer;
		}

		/// <summary>
		/// What type(s) of damage this attack has on it.
		/// </summary>
		public AttackType DamageType {
			get => _attackType;
			set => _attackType = value;
		}


		public TriggerRefProxy Proxy {
			get => new TriggerRefProxy(_proxy);
			set => _proxy = value.Pointer;
		}

		/// <summary>
		/// Provide an <see cref="Attack"/> instance to return a new instance of this struct. This will fix values like the <see cref="_attackType"/>.
		/// </summary>
		/// <param name="attack"></param>
		/// <returns></returns>
		public static AttackInfo Fix(Attack attack) {
			unsafe {
				AttackInfo* atk = (AttackInfo*)attack.Pointer;
				return *atk;
			}
		}

		/// <summary>
		/// Provide an <see cref="Attack"/> instance to return a new instance of this struct. This will fix values like the <see cref="_attackType"/>.
		/// </summary>
		/// <param name="attack"></param>
		/// <returns></returns>
		public static AttackInfo From(IntPtr attack) {
			unsafe {
				AttackInfo* atk = (AttackInfo*)attack;
				return *atk;
			}
		}
	}

	/// <summary>
	/// The same as <see cref="AttackInfo"/> but it is immutable; it cannot be changed.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct ImmutableAttackInfo {
		private readonly float _damage;
		private readonly Vector3 _normal;
		private readonly Vector3 _origin;
		private readonly Vector3 _direction;
		private readonly byte _backFacing;
		private readonly int _orderInPool;
		private readonly IntPtr _collider;
		private readonly AttackType _attackType;
		private readonly IntPtr _proxy;
		// This is copied by a reinterpret cast. Do not add new fields. You will cause access violations.

		/// <summary>
		/// The absolute amount of damage this attack did.
		/// </summary>
		public float Damage {
			get => _damage;
		}

		/// <summary>
		/// The normal of the surface that this attack hit.
		/// </summary>
		public Vector3 Normal {
			get => _normal;
		}

		/// <summary>
		/// The direction this attack came from.
		/// </summary>
		public Vector3 Direction {
			get => _direction;
		}

		/// <summary>
		/// The origin of this attack source. Can be combined with direction to create a ray.
		/// </summary>
		public Vector3 Origin {
			get => _origin;
		}

		/// <summary>
		/// If true, the attack occurred from within the hit area.
		/// </summary>
		public bool IsBackFacing {
			get => _backFacing == 1;
		}

		public int OrderInAttackPool {
			get => _orderInPool;
		}

		/// <summary>
		/// The collider that the attack impacted with to result in this damage occurring.
		/// </summary>
		public Collider Collider {
			get => new Collider(_collider);
		}

		/// <summary>
		/// What type(s) of damage this attack has on it.
		/// </summary>
		public AttackType DamageType {
			get => _attackType;
		}


		public TriggerRefProxy Proxy {
			get => new TriggerRefProxy(_proxy);
		}
	}
}
