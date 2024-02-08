#define AVATAR_STATS_COMPUTABLE_ANYWAY
using System;
using UnityEngine;
using SLZAvatar = SLZ.VRMK.Avatar;

#if !UNITY_EDITOR && IS_MOD_ENVIRONMENT
// As it appears in the mod:
using static AvatarStatExtender.Tools.AvatarStatCalculationExtension;
#else
using static AvatarCalculators;
#endif

#if UNITY_EDITOR
[RequireComponent(typeof(SLZAvatar))]
#endif
public class AvatarStatDriver : MonoBehaviour {

#if !UNITY_EDITOR && IS_MOD_ENVIRONMENT
	public AvatarStatDriver(IntPtr @this) : base(@this) { }
#endif

	public bool useCustomStats;
	public bool useCustomMass;
	public bool useProportionalMassEditing;

	public bool useCustomAgility;
	public bool useCustomSpeed;
	public bool useCustomUpper;
	public bool useCustomLower;
	public bool useCustomVitality;
	public bool useCustomIntelligence;

	public float customAgility;
	public float customSpeed;
	public float customUpper;
	public float customLower;
	public float customVitality;
	public float customIntelligence;

	public bool useCustomChestMass;
	public bool useCustomPelvisMass;
	public bool useCustomArmMass;
	public bool useCustomLegMass;
	public bool useCustomHeadMass;

	public float customTotalMass;
	public float customChestMass;
	public float customPelvisMass;
	public float customArmMass;
	public float customLegMass;
	public float customHeadMass;

	public float extraJumpVerticalVelocity;

	private SLZAvatar _avatar;

#if !UNITY_EDITOR && !AVATAR_STATS_COMPUTABLE_ANYWAY && IS_MOD_ENVIRONMENT
	// These will be set by the actual mod itself, and can point to the runtime Avatar object rather than the custom stat implementation.

	/// <summary>Provide the original, vanilla value.</summary>
	public static Func<SLZAvatar, float> BonelabRuntimeGetAgility { get; set; }
	/// <summary>Provide the original, vanilla value.</summary>
	public static Func<SLZAvatar, float> BonelabRuntimeGetSpeed { get; set; }
	/// <summary>Provide the original, vanilla value.</summary>
	public static Func<SLZAvatar, float> BonelabRuntimeGetUpperStrength { get; set; }
	/// <summary>Provide the original, vanilla value.</summary>
	public static Func<SLZAvatar, float> BonelabRuntimeGetLowerStrength { get; set; }
	/// <summary>Provide the original, vanilla value.</summary>
	public static Func<SLZAvatar, float> BonelabRuntimeGetVitality { get; set; }
	/// <summary>Provide the original, vanilla value.</summary>
	public static Func<SLZAvatar, float> BonelabRuntimeGetIntelligence { get; set; }

	/// <summary>Provide the original, vanilla value.</summary>
	public static Func<SLZAvatar, float> BonelabRuntimeGetMassTotal { get; set; }
	/// <summary>Provide the original, vanilla value.</summary>
	public static Func<SLZAvatar, float> BonelabRuntimeGetMassPelvis { get; set; }
	/// <summary>Provide the original, vanilla value.</summary>
	public static Func<SLZAvatar, float> BonelabRuntimeGetMassChest { get; set; }
	/// <summary>Provide the original, vanilla value.</summary>
	public static Func<SLZAvatar, float> BonelabRuntimeGetMassArm { get; set; }
	/// <summary>Provide the original, vanilla value.</summary>
	public static Func<SLZAvatar, float> BonelabRuntimeGetMassLeg { get; set; }
	/// <summary>Provide the original, vanilla value.</summary>
	public static Func<SLZAvatar, float> BonelabRuntimeGetMassHead { get; set; }
#endif

	#region Vanilla Values

	#region Stats
	/// <summary>
	/// The value of this avatar's agility in the vanilla game.
	/// </summary>
	public float VanillaAgility {
		get {
#if UNITY_EDITOR || AVATAR_STATS_COMPUTABLE_ANYWAY
			_avatar.ComputeAllStats(out _, out AvatarStats stats);
			return stats.agility;
#else
			return BonelabRuntimeGetAgility?.Invoke(_avatar) ?? throw new InvalidOperationException("Please set the static Funcs of AvatarStatDriver");
#endif
		}
	}

	/// <summary>
	/// The value of this avatar's speed in the vanilla game.
	/// </summary>
	public float VanillaSpeed {
		get {
#if UNITY_EDITOR || AVATAR_STATS_COMPUTABLE_ANYWAY
			_avatar.ComputeAllStats(out _, out AvatarStats stats);
			return stats.speed;
#else
			return BonelabRuntimeGetSpeed?.Invoke(_avatar) ?? throw new InvalidOperationException("Please set the static Funcs of AvatarStatDriver");
#endif
		}
	}

	/// <summary>
	/// The value of this avatar's upper strength in the vanilla game.
	/// </summary>
	public float VanillaUpperStrength {
		get {
#if UNITY_EDITOR || AVATAR_STATS_COMPUTABLE_ANYWAY
			_avatar.ComputeAllStats(out _, out AvatarStats stats);
			return stats.strengthUpper;
#else
			return BonelabRuntimeGetUpperStrength?.Invoke(_avatar) ?? throw new InvalidOperationException("Please set the static Funcs of AvatarStatDriver");
#endif
		}
	}

	/// <summary>
	/// The value of this avatar's lower strength in the vanilla game.
	/// </summary>
	public float VanillaLowerStrength {
		get {
#if UNITY_EDITOR || AVATAR_STATS_COMPUTABLE_ANYWAY
			// FIXME: Do not compute every time this is referenced.
			_avatar.ComputeAllStats(out _, out AvatarStats stats);
			return stats.strengthLower;
#else
			return BonelabRuntimeGetLowerStrength?.Invoke(_avatar) ?? throw new InvalidOperationException("Please set the static Funcs of AvatarStatDriver");
#endif
		}
	}

	/// <summary>
	/// The value of this avatar's vitality in the vanilla game.
	/// </summary>
	public float VanillaVitality {
		get {
#if UNITY_EDITOR || AVATAR_STATS_COMPUTABLE_ANYWAY
			// FIXME: Do not compute every time this is referenced.
			_avatar.ComputeAllStats(out _, out AvatarStats stats);
			return stats.vitality;
#else
			return BonelabRuntimeGetVitality?.Invoke(_avatar) ?? throw new InvalidOperationException("Please set the static Funcs of AvatarStatDriver");
#endif
		}
	}

	/// <summary>
	/// The value of this avatar's intelligence in the vanilla game.
	/// </summary>
	public float VanillaIntelligence {
		get {
#if UNITY_EDITOR || AVATAR_STATS_COMPUTABLE_ANYWAY
			// FIXME: Do not compute every time this is referenced.
			_avatar.ComputeAllStats(out _, out AvatarStats stats);
			return stats.intelligence;
#else
			return BonelabRuntimeGetIntelligence?.Invoke(_avatar) ?? throw new InvalidOperationException("Please set the static Funcs of AvatarStatDriver");
#endif
		}
	}

	#endregion

	#region Masses

	public float VanillaTotalMass {
		get {
#if UNITY_EDITOR || AVATAR_STATS_COMPUTABLE_ANYWAY
			// FIXME: Do not compute every time this is referenced.
			AvatarMasses masses = _avatar.ComputeMass(out _);
			return masses.massTotal;
#else
			return BonelabRuntimeGetMassTotal?.Invoke(_avatar) ?? throw new InvalidOperationException("Please set the static Funcs of AvatarStatDriver");
#endif
		}
	}

	public float VanillaChestMass {
		get {
#if UNITY_EDITOR || AVATAR_STATS_COMPUTABLE_ANYWAY
			// FIXME: Do not compute every time this is referenced.
			AvatarMasses masses = _avatar.ComputeMass(out _);
			return masses.massChest;
#else
			return BonelabRuntimeGetMassChest?.Invoke(_avatar) ?? throw new InvalidOperationException("Please set the static Funcs of AvatarStatDriver");
#endif
		}
	}

	public float VanillaPelvisMass {
		get {
#if UNITY_EDITOR || AVATAR_STATS_COMPUTABLE_ANYWAY
			// FIXME: Do not compute every time this is referenced.
			AvatarMasses masses = _avatar.ComputeMass(out _);
			return masses.massPelvis;
#else
			return BonelabRuntimeGetMassPelvis?.Invoke(_avatar) ?? throw new InvalidOperationException("Please set the static Funcs of AvatarStatDriver");
#endif
		}
	}


	public float VanillaArmMass {
		get {
#if UNITY_EDITOR || AVATAR_STATS_COMPUTABLE_ANYWAY
			// FIXME: Do not compute every time this is referenced.
			AvatarMasses masses = _avatar.ComputeMass(out _);
			return masses.massArm;
#else
			return BonelabRuntimeGetMassArm?.Invoke(_avatar) ?? throw new InvalidOperationException("Please set the static Funcs of AvatarStatDriver");
#endif
		}
	}


	public float VanillaLegMass {
		get {
#if UNITY_EDITOR || AVATAR_STATS_COMPUTABLE_ANYWAY
			// FIXME: Do not compute every time this is referenced.
			AvatarMasses masses = _avatar.ComputeMass(out _);
			return masses.massLeg;
#else
			return BonelabRuntimeGetMassLeg?.Invoke(_avatar) ?? throw new InvalidOperationException("Please set the static Funcs of AvatarStatDriver");
#endif
		}
	}


	public float VanillaHeadMass {
		get {
#if UNITY_EDITOR || AVATAR_STATS_COMPUTABLE_ANYWAY
			// FIXME: Do not compute every time this is referenced.
			AvatarMasses masses = _avatar.ComputeMass(out _);
			return masses.massHead;
#else
			return BonelabRuntimeGetMassHead?.Invoke(_avatar) ?? throw new InvalidOperationException("Please set the static Funcs of AvatarStatDriver");
#endif
		}
	}

	#endregion

	#endregion

	public float EffectiveAgility {
		get {
			if (useCustomStats && useCustomAgility) {
				return customAgility;
			}
			return VanillaAgility;
		}
	}

	public float EffectiveSpeed {
		get {
			if (useCustomStats && useCustomSpeed) {
				return customSpeed;
			}
			return VanillaSpeed;
		}
	}

	public float EffectiveUpperStrength {
		get {
			if (useCustomStats && useCustomUpper) {
				return customUpper;
			}
			return VanillaUpperStrength;
		}
	}

	public float EffectiveLowerStrength {
		get {
			if (useCustomStats && useCustomLower) {
				return customLower;
			}
			return VanillaLowerStrength;
		}
	}

	public float EffectiveVitality {
		get {
			if (useCustomStats && useCustomVitality) {
				return customVitality;
			}
			return VanillaVitality;
		}
	}

	public float EffectiveIntelligence {
		get {
			if (useCustomStats && useCustomIntelligence) {
				return customIntelligence;
			}
			return VanillaIntelligence;
		}
	}

	/// <summary>
	/// The proportion of mass used when custom masses are enabled, and proportional mass editing is enabled.
	/// <para/>
	/// This value is multiplied with the vanilla body part masses to get their effective results. Returns 1.0f in all other cases where customization isn't applicable.
	/// </summary>
	public float MassProportion {
		get {
			if (useCustomMass && useProportionalMassEditing) {
#if UNITY_EDITOR || AVATAR_STATS_COMPUTABLE_ANYWAY
				AvatarMasses masses = _avatar.ComputeMass(out _);
				return customTotalMass / masses.massTotal;
#else
				return customTotalMass / BonelabRuntimeGetMassTotal?.Invoke(_avatar) ?? throw new InvalidOperationException("Please set the static Funcs of AvatarStatProvider");
#endif
			} else {
				return 1.0f;
			}
		}
	}

	/// <summary>
	/// The effective total mass of this avatar, which combines all other effective values, unless proportional editing is desired (from which the
	/// custom value is used) or if custom masses are disabled (from which the vanilla value is used).
	/// </summary>
	public float EffectiveTotalMass {
		get {
			if (useCustomMass) {
				if (useProportionalMassEditing) {
					return customTotalMass;
				} else {
					return EffectiveChestMass + EffectivePelvisMass + (EffectiveArmMass * 2f) + (EffectiveLegMass * 2) + EffectiveHeadMass;
				}
			}
			return VanillaTotalMass;
		}
	}


	/// <summary>
	/// The mass of the pelvis, computed from the desired settings.
	/// </summary>
	public float EffectivePelvisMass {
		get {
			if (useCustomMass) {
				if (useProportionalMassEditing && useCustomPelvisMass) {
					return VanillaPelvisMass * MassProportion;
				} else if (useCustomPelvisMass) {
					return customPelvisMass;
				}
			}
			return VanillaPelvisMass;
		}
	}

	/// <summary>
	/// The mass of the chest, computed from the desired settings.
	/// </summary>
	public float EffectiveChestMass {
		get {
			if (useCustomMass) {
				if (useProportionalMassEditing && useCustomChestMass) {
					return VanillaChestMass * MassProportion;
				} else if (useCustomChestMass) {
					return customChestMass;
				}
			}
			return VanillaChestMass;
		}
	}

	/// <summary>
	/// The effective mass of one arm, computed from the desired settings.
	/// </summary>
	public float EffectiveArmMass {
		get {
			if (useCustomMass) {
				if (useProportionalMassEditing && useCustomArmMass) {
					return VanillaArmMass * MassProportion;
				} else if (useCustomArmMass) {
					return customArmMass;
				}
			}
			return VanillaArmMass;
		}
	}

	/// <summary>
	/// The effective mass of one leg, computed from the desired settings.
	/// </summary>
	public float EffectiveLegMass {
		get {
			if (useCustomMass) {
				if (useProportionalMassEditing && useCustomLegMass) {
					return VanillaLegMass * MassProportion;
				} else if (useCustomLegMass) {
					return customLegMass;
				}
			}
			return VanillaLegMass;
		}
	}

	/// <summary>
	/// The effective head mass, computed from the desired settings.
	/// </summary>
	public float EffectiveHeadMass {
		get {
			if (useCustomMass) {
				if (useProportionalMassEditing && useCustomHeadMass) {
					return VanillaHeadMass * MassProportion;
				} else if (useCustomHeadMass) {
					return customHeadMass;
				}
			}
			return VanillaHeadMass;
		}
	}


	private void Awake() {
		_avatar = GetComponent<SLZAvatar>();
	}


#if UNITY_EDITOR

	[DoNotSerialize]
	public object inspector;

	private System.Reflection.MethodInfo _triggerValidatedRepaint;

	private object[] _args = new object[2];

	private void OnValidate() {
		_avatar = GetComponent<SLZAvatar>();
		if (inspector != null) {
			object inspectorCache = inspector;
			inspector = null; // Unset it so that it's invalidated. The inspector will
							  // react to the method when appropriate and reset it back to non-null.
			_triggerValidatedRepaint ??= inspectorCache.GetType().GetMethod("TriggerValidatedRepaint", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			_args[0] = this;
			_args[1] = GetComponent<SLZAvatar>();
			_triggerValidatedRepaint.Invoke(inspectorCache, _args);
		}
	}

	[UnityEditor.MenuItem("Xan's SLZ Tests/Compute Avatar Mass")]
	private static void ToolbarTestAvatarMass() {
		GameObject active = UnityEditor.Selection.activeGameObject;
		if (active) {
			SLZAvatar avy = active.GetComponent<SLZAvatar>();
			if (avy) {
				Debug.Log(avy.ComputeMass(out _));
			}
		}
	}

	[UnityEditor.MenuItem("Xan's SLZ Tests/Compute Avatar Stats")]
	private static void ToolbarTestAvatarStats() {
		GameObject active = UnityEditor.Selection.activeGameObject;
		if (active) {
			SLZAvatar avy = active.GetComponent<SLZAvatar>();
			if (avy) {
				AvatarMasses masses = avy.ComputeMass(out AvatarAdditionalInfo additionalInfo);
				Debug.Log(avy.ComputeBaseStats(masses, additionalInfo));
			}
		}
	}
#endif

}
