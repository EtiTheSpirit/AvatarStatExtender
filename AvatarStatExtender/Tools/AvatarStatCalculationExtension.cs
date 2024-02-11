#nullable enable
#pragma warning disable CS0618
// #define REAL_HEPTA_AVATAR
// #define TEST_CHIMERA_ECLIPSE
using UnityEngine;
using SLZAvatar = SLZ.VRMK.Avatar;
using System;
using static SLZ.VRMK.Avatar;

namespace AvatarStatExtender.Tools {
	public static class AvatarStatCalculationExtension {

		private const float PI = 3.141592653582373f;

		/// <summary>
		/// Returns the exact position between two human bones.
		/// </summary>
		/// <param name="animator"></param>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		/* Known Correct - Do not modify! */
		private static Vector3 GetPositionBetween(Animator animator, HumanBodyBones first, HumanBodyBones second) {
			return Vector3.LerpUnclamped(animator.GetBoneTransform(first).position, animator.GetBoneTransform(second).position, 0.5f);
		}

		/// <summary>
		/// Returns the eye position of this avatar, depending on whether or not an eye center override is available.
		/// </summary>
		/// <param name="avatar"></param>
		/// <returns></returns>
		/* Known Correct - Do not modify! */
		public static Vector3 EyePosition(this SLZAvatar avatar) {
			if (avatar.eyeCenterOverride) return avatar.eyeCenterOverride.position;
			return GetPositionBetween(avatar.animator, HumanBodyBones.LeftEye, HumanBodyBones.RightEye);
		}

		/// <summary>
		/// Returns the height of the eyes on this avatar.
		/// </summary>
		/// <param name="avatar"></param>
		/// <returns></returns>
		/* Known Correct - Do not modify! */
		public static float GetEyeHeight(this SLZAvatar avatar) {
			return avatar.EyePosition().y - avatar.transform.position.y;
		}

		/// <summary>
		/// Provides the avatar's computed mass and stats.
		/// </summary>
		/// <param name="avatar">The avatar to get information on.</param>
		/// <param name="mass">The masses of every body part on this avatar.</param>
		/// <param name="stats">The stats computed from the avatar's body.</param>
		public static void ComputeAllStats(this SLZAvatar avatar, out AvatarMasses mass, out AvatarStats stats) {
			mass = ComputeMass(avatar, out AvatarComputationProperties additionalInfo);
			stats = ComputeBaseStats(avatar, mass, additionalInfo);
		}

		/// <summary>
		/// Utilizes adapted vanilla x86 code to compute the mass of an avatar based on its body proportions.
		/// </summary>
		/// <param name="avatar">The avatar to compute the mass of.</param>
		/// <param name="additionalInfo">This struct contains information that is relevant to avatar stat computations.</param>
		/// <param name="normalizeTo82">This is a correction factor and is never changed by the game; it is always 0.5 in vanilla calculations.</param>
		/// <returns></returns>
		/// <exception cref="NullReferenceException"></exception>
		public static AvatarMasses ComputeMass(this SLZAvatar avatar, out AvatarComputationProperties additionalInfo, float normalizeTo82 = 0.5f) {
			Transform avyAnimatorTrs = avatar.transform; // This is the same as the animator's transform.
			Animator animator = avatar.animator;

			#region Value Precomputation Reimplementation

			#region Eye and Spine Height
			Quaternion worldToAnimator = Quaternion.Inverse(avyAnimatorTrs.rotation);
			Vector3 neckWorld = animator.GetBoneTransform(HumanBodyBones.Neck).position;

			float eyeHeight = avatar.GetEyeHeight(); // is avatar._eyeHeight;
			Vector3 hipsPosition = animator.GetBoneTransform(HumanBodyBones.Hips).position;

			/* Known Correct - Do not modify! */
			float t1HeightPercent = (neckWorld.y - avyAnimatorTrs.position.y) / eyeHeight;

			/* Known Correct - Do not modify! */
			Vector3 sternumOffset = worldToAnimator * (GetPositionBetween(animator, HumanBodyBones.LeftUpperArm, HumanBodyBones.RightUpperArm) - neckWorld);

			/* Known Correct - Do not modify! */
			Vector3 sternumOffsetPercent = sternumOffset / eyeHeight;

			/* Known Correct - Do not modify! */
			Vector3 hipOffset = worldToAnimator * (GetPositionBetween(animator, HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg) - hipsPosition);

			/* Known Correct - Do not modify! */
			Vector3 hipOffsetPercent = hipOffset / eyeHeight;
			#endregion

			#region Sacrum Height
			// IDA PSEUDOCODE AS FOLLOWS:
			// hipsTransform260 = UnityEngine_Animator__GetBoneTransform(v259, HumanBodyBones.Hips, 0i64);
			// hipsPosition261 = UnityEngine_Transform__get_position(&v1582, hipsTransform260, 0i64);
			// hipsPositionY_263 = hipsPosition261->fields.y;
			// animatorTransform264 = UnityEngine_Component__get_transform(animator262, 0i64);
			// avatarPosition265 = UnityEngine_Transform__get_position(&v1583, animatorTransform264, 0i64);
			// this->fields._sacrumHeightPercent = (float)(hipsPositionY_263 - avatarPosition265->fields.y) / this->fields._eyeHeight;

			/* Known Correct - Do not modify! */
			float sacrumHeightPercent = (hipsPosition.y - avyAnimatorTrs.position.y) / eyeHeight;

			// The order MUST be set appropriately. Arm - Chest.
			/* (Both) Known Correct - Do not modify! */
			Vector3 chestToShoulderDiffRaw = animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position - animator.GetBoneTransform(HumanBodyBones.Chest).position;
			float chestToShoulderPerc = (worldToAnimator * chestToShoulderDiffRaw).x / eyeHeight;

			#endregion

			#region Arm and Leg Lengths
			/* (All four) Known Correct - Do not modify! */
			Vector3 humerous = worldToAnimator * (animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position - animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position);
			Vector3 forearm = worldToAnimator * (avatar.wristRt.position - animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position);
			Vector3 femur = worldToAnimator * (animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).position - animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position);
			Vector3 lowerLeg = worldToAnimator * (animator.GetBoneTransform(HumanBodyBones.RightFoot).position - animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).position);

#if REAL_HEPTA_AVATAR
			const float _armUpperPercentDefault = 0.18207f;
			const float _armLowerPercentDefault = 0.1477f;
			const float _legUpperPercentDefault = 0.268502f;
			const float _legLowerPercentDefault = 0.23832899f;
			float upperArmLength = _armUpperPercentDefault * eyeHeight;
			float upperLegLength = _armLowerPercentDefault * eyeHeight;
			float lowerArmLength = _legUpperPercentDefault * eyeHeight;
			float lowerLegLength = _legLowerPercentDefault * eyeHeight;
#else
			// The "length" values take the percents computed by the vanilla gizmo code (which divides by eyeheight) and then multiplies it by eyeheight.
			// In short, it's just:
			/* (All four) Known Correct - Do not modify! */
			float upperArmLength = humerous.magnitude;
			float upperLegLength = femur.magnitude;
			float lowerArmLength = forearm.magnitude;
			float lowerLegLength = lowerLeg.magnitude;
#endif

#if TEST_CHIMERA_ECLIPSE
			// Code specifically tested for my avatar.
			bool fuzzyEq(float a, float b, float epsilon = 0.001f) { return Mathf.Abs(a - b) < epsilon; }
			bool fuzzyEqVec(Vector3 a, Vector3 b, float epsilon = 0.01f) { return fuzzyEq(a.x, b.x, epsilon) && fuzzyEq(a.y, b.y, epsilon) && fuzzyEq(a.z, b.z, epsilon); }
			bool fuzzyEqEllipse(SLZAvatar.SoftEllipse a, SLZAvatar.SoftEllipse b, float epsilon = 0.01f) { return fuzzyEq(a.XRadius, b.XRadius, epsilon) && fuzzyEq(a.XBias, b.XBias, epsilon) && fuzzyEq(a.ZRadius, b.ZRadius, epsilon) && fuzzyEq(a.ZBias, b.ZBias, epsilon); }

			void assertEquality(string name, float a, float b) {
				if (!fuzzyEq(a, b)) {
					Debug.LogErrorFormat($"COMPUTATIONAL MISMATCH: {name} is invalid; expecting {b}, got {a}");
				}
			}
			void assertEqualityVec(string name, Vector3 a, Vector3 b) {
				if (!fuzzyEqVec(a, b)) {
					Debug.LogErrorFormat($"COMPUTATIONAL MISMATCH: {name} is invalid; expecting {b}, got {a}");
				}
			}
			void assertEqualityEllipse(string name, SoftEllipse a, SoftEllipse b) {
				if (!fuzzyEqEllipse(a, b)) {
					string aStr = $"SoftEllipse[XRadius={a.XRadius}, XBias={a.XBias}, ZRadius={a.ZRadius}, ZBias={a.ZBias}]";
					string bStr = $"SoftEllipse[XRadius={b.XRadius}, XBias={b.XBias}, ZRadius={b.ZRadius}, ZBias={b.ZBias}]";
					Debug.LogErrorFormat($"COMPUTATIONAL MISMATCH: {name} is invalid; expecting {bStr}, got {aStr}");
				}
			}

			assertEquality(nameof(eyeHeight), eyeHeight, 1.678f);
			assertEquality(nameof(t1HeightPercent), t1HeightPercent, 0.9288539f);
			assertEquality(nameof(upperArmLength), upperArmLength, 0.3358216f);
			assertEquality(nameof(lowerArmLength), lowerArmLength, 0.2566614f);
			assertEquality(nameof(upperLegLength), upperLegLength, 0.3676393f);
			assertEquality(nameof(lowerLegLength), lowerLegLength, 0.3939134f);
			assertEquality(nameof(sacrumHeightPercent), sacrumHeightPercent, 0.6044896f);
			assertEquality(nameof(chestToShoulderPerc), chestToShoulderPerc, 0.06153189f);
			assertEqualityVec(nameof(hipOffsetPercent), hipOffsetPercent, new Vector3(0f, -0.02f, 0f));
			assertEqualityVec(nameof(sternumOffsetPercent), sternumOffsetPercent, new Vector3(0f, -0.05f, -0.01f));
			assertEquality(nameof(avatar.ChestEllipseX), avatar.ChestEllipseX, 0.05157722f);
			assertEquality(nameof(avatar.ChestEllipseZ), avatar.ChestEllipseZ, 0.07117788f);
			assertEquality(nameof(avatar.ChestEllipseNegZ), avatar.ChestEllipseNegZ, 0.01534615f);
			assertEquality(nameof(avatar.SternumEllipseZ), avatar.SternumEllipseZ, 0.08507442f);
			assertEquality(nameof(avatar.SternumEllipseNegZ), avatar.SternumEllipseNegZ, 0.03401336f); 
			// TODO: bulgeBreast
			// TODO: bulgeUpperBack
			assertEquality(nameof(avatar.WaistEllipseZ), avatar.WaistEllipseZ, 0.06119755f);
			assertEquality(nameof(avatar.WaistEllipseNegZ), avatar.WaistEllipseNegZ, 0.02731629f);
			assertEquality(nameof(avatar.HipsEllipseZ), avatar.HipsEllipseZ, 0.06350059f);
			assertEquality(nameof(avatar.HipsEllipseNegZ), avatar.HipsEllipseNegZ, 0.0560604f);

			assertEqualityEllipse(nameof(avatar.elbowEllipse), avatar.elbowEllipse, new SoftEllipse(0.02049523f, 0.1421007f, 0.02142079f, 0.3482172f));
			assertEqualityEllipse(nameof(avatar.upperarmEllipse), avatar.upperarmEllipse, new SoftEllipse(0.02601284f, 0.3321574f, 0.03f, 0.05111256f));
			assertEqualityEllipse(nameof(avatar.forearmEllipse), avatar.forearmEllipse, new SoftEllipse(0.02612953f, 0.09840614f, 0.03f, 0.1463524f));
			assertEqualityEllipse(nameof(avatar.wristEllipse), avatar.wristEllipse, new SoftEllipse(0.02393321f, 0.4083341f, 0.03732799f, 0f));

			assertEqualityEllipse(nameof(avatar.thighUpperEllipse), avatar.thighUpperEllipse, new SoftEllipse(0.04617971f, 0.0497239f, 0.0449855f, 0.1498619f));
			assertEqualityEllipse(nameof(avatar.kneeEllipse), avatar.kneeEllipse, new SoftEllipse(0.03f, 0f, 0.03f, 0f));
			assertEqualityEllipse(nameof(avatar.calfEllipse), avatar.calfEllipse, new SoftEllipse(0.03f, 0.05693642f, 0.03f, -0.2401251f));
			assertEqualityEllipse(nameof(avatar.ankleEllipse), avatar.ankleEllipse, new SoftEllipse(0.01957383f, 0f, 0.01886195f, 0.005178876f));
			
#endif

			#endregion

			#endregion

			float torsoHeightProportion = (
				(sternumOffsetPercent.y + t1HeightPercent)
				- (hipOffsetPercent.y + sacrumHeightPercent)
			) * eyeHeight * 0.55f;

			float massChest = (avatar.ChestEllipseZ + avatar.ChestEllipseNegZ
				+ avatar.SternumEllipseZ + avatar.SternumEllipseNegZ
				+ avatar.bulgeBreast.apexZ + avatar.bulgeUpperBack.apexZ
			) * 0.5f * eyeHeight
			* (avatar.ChestEllipseX + avatar.ChestEllipseX) * eyeHeight
			* torsoHeightProportion * 0.7854f * 1000f;

			float massPelvis = (avatar.WaistEllipseZ + avatar.WaistEllipseNegZ
				+ avatar.HipsEllipseZ + avatar.HipsEllipseNegZ
				+ avatar.bulgeButt.apexZ + avatar.bulgeAbdomen.apexZ + avatar.bulgeLowerBack.apexZ
			) * 0.5f * eyeHeight
			* ((eyeHeight * avatar.HipsEllipseX) + avatar.WaistEllipseX)
			* torsoHeightProportion * 0.7854f * 1000f;

			float armBaseMass = Mathf.Pow(
				AverageTotalRadius(avatar.elbowEllipse, avatar.forearmEllipse, avatar.wristEllipse) * eyeHeight,
				2.0f
			) * PI * lowerArmLength * 1000f;

			float upperArmProportion = Mathf.Pow(
				AverageTotalRadius(avatar.upperarmEllipse, avatar.elbowEllipse) * eyeHeight,
				2.0f
			);

			float singleArmMass = (armBaseMass + (upperArmProportion * PI * upperArmLength * 1000.0f)) * 1.11538f;

			float legMassBaseFactor = Mathf.Pow(
				AverageTotalRadius(avatar.kneeEllipse, avatar.calfEllipse, avatar.ankleEllipse) * eyeHeight,
				2.0f
			) * PI * lowerLegLength * 1000.0f;

			float singleLegMass = legMassBaseFactor +
			Mathf.Pow(
				AverageTotalRadius(avatar.thighUpperEllipse, avatar.kneeEllipse) * eyeHeight,
				2.0f
			) * PI * upperLegLength * 1100.0f;

			float rawTotalMass = ((singleArmMass + singleArmMass) + (massPelvis + massChest) + (singleLegMass + singleLegMass)) * 1.0764263f;
			float torsoProportion = Mathf.Clamp(
				((avatar.ChestEllipseNegZ + avatar.ChestEllipseZ) / (avatar.WaistEllipseNegZ + avatar.WaistEllipseZ)) - 0.1f,
				0.7f,
				1.0f
			);
			float normalizer = (((82.0f - rawTotalMass) * Mathf.Clamp01(normalizeTo82)) + rawTotalMass) * torsoProportion / rawTotalMass;
			float massTotal = rawTotalMass * normalizer;

			AvatarMasses result = new AvatarMasses {
				massPelvis = massPelvis * normalizer,
				massTotal = massTotal,
				massChest = massChest * normalizer,
				massArm = normalizer * singleArmMass,
				massLeg = normalizer * singleLegMass,
			};
			result.massBody = result.massPelvis + result.massChest + result.massArm + result.massArm + result.massLeg + result.massLeg;
			result.massHead = result.massTotal - result.massBody;

			float distanceFromThumbProxToIndexProx = (animator.GetBoneTransform(HumanBodyBones.RightThumbProximal).position - animator.GetBoneTransform(HumanBodyBones.RightIndexProximal).position).magnitude;
			float lengthOfIndexProxSegment = (animator.GetBoneTransform(HumanBodyBones.RightIndexProximal).position - animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate).position).magnitude;
			float lengthOfIntermediateSegment = (animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate).position - animator.GetBoneTransform(HumanBodyBones.RightIndexDistal).position).magnitude;
			float handSizeMult = (distanceFromThumbProxToIndexProx + lengthOfIndexProxSegment + lengthOfIntermediateSegment) / 0.12762f;
			// TODO: ^ Might not actually be right.
			// The vector instructions were ... convoluted (to put it lightly) and reused some locals with different names which nuked readability.

			additionalInfo = new AvatarComputationProperties {
				eyeHeight = eyeHeight,
				hipOffset = hipOffset,
				hipOffsetPercent = hipOffsetPercent,
				hipsPosition = hipsPosition,
				lowerArmLength = lowerArmLength,
				lowerLegLength = lowerLegLength,
				sacrumHeightPercent = sacrumHeightPercent,
				sternumOffset = sternumOffset,
				sternumOffsetPercent = sternumOffsetPercent,
				upperArmLength = upperArmLength,
				upperLegLength = upperLegLength,
				t1HeightPercent = t1HeightPercent,
				chestToShoulderPerc = chestToShoulderPerc,
				handSizeMult = handSizeMult
			};
			return result;
		}

		/// <summary>
		/// Computes the base stats of the avatar. Note that this requires the masses and additional info packages, both of which
		/// are acquired through <see cref="ComputeMass(SLZAvatar, out AvatarComputationProperties, float)"/>.
		/// </summary>
		/// <param name="avatar"></param>
		/// <param name="masses"></param>
		/// <param name="additionalInfo"></param>
		/// <returns></returns>
		public static AvatarStats ComputeBaseStats(this SLZAvatar avatar, AvatarMasses masses, AvatarComputationProperties additionalInfo) {

			float eyeHeight = additionalInfo.eyeHeight;
			float foreheadWidth = avatar.ForeheadEllipseX / 0.044f;
			float eyeHeightFactor = eyeHeight / 1.638f;
			float massTotalFactor = masses.massTotal / 75.0f;

			float shoulderBroadness = ((additionalInfo.chestToShoulderPerc * 10.354f)
				+ (avatar.ChestEllipseX * 10.354f))
				* 0.5f;

			float spineHeight = ((additionalInfo.sternumOffsetPercent.y + additionalInfo.t1HeightPercent)
				- (additionalInfo.hipOffsetPercent.y + additionalInfo.sacrumHeightPercent))
				/ 0.31253165f;

			float upperArmCircumference = AverageTotalRadius(avatar.upperarmEllipse, avatar.elbowEllipse) * PI;
			upperArmCircumference /= (0.034f * PI);
			if (upperArmCircumference < 1.0) upperArmCircumference += (1.0f - upperArmCircumference) * 0.6f;

			float lowerArmCircumference = AverageTotalRadius(avatar.forearmEllipse, avatar.wristEllipse) * PI;
			lowerArmCircumference /= (0.026f * PI);
			if (lowerArmCircumference < 1.0) lowerArmCircumference += (1.0f - lowerArmCircumference) * 0.6f;

			float legQuadCircumference = AverageTotalRadius(avatar.thighUpperEllipse, avatar.kneeEllipse) * PI;
			legQuadCircumference /= (0.04525f * PI);

			float legLowerPercent = additionalInfo.lowerLegLength / additionalInfo.eyeHeight;
			float legUpperPercent = additionalInfo.upperLegLength / additionalInfo.eyeHeight;
			float armLowerPercent = additionalInfo.lowerArmLength / additionalInfo.eyeHeight;
			float armUpperPercent = additionalInfo.upperArmLength / additionalInfo.eyeHeight;

			float legFactor = (legLowerPercent + legUpperPercent) / 0.50683099f;
			float lowerLegSize = GrandTotalRadius(avatar.calfEllipse, avatar.ankleEllipse);

			float sasquatchHandRating = lowerArmCircumference * massTotalFactor * additionalInfo.handSizeMult;
			float quarterCalfSize = lowerLegSize * 0.25f;
			float legCircAndHeightPerMass = ((quarterCalfSize * PI / (0.031f * PI)) + legQuadCircumference) * 0.5f * (legFactor * legFactor) * eyeHeightFactor / massTotalFactor;
			if (legCircAndHeightPerMass < 1.0) legCircAndHeightPerMass = Mathf.Pow(legCircAndHeightPerMass, 0.3333333333f);

			float adjustedLegFactor = Mathf.Clamp01((Mathf.Abs(legFactor - 1.0f) - 1.0f) * -1.0f);
			float foreheadFactor = Mathf.Sqrt(Mathf.Abs(foreheadWidth));
			float adjustedForeheadFactor = Mathf.Clamp01((Mathf.Abs(foreheadFactor - 1.0f) - 1.0f) * -1.0f);

			AvatarStats result;
			result.speed = legCircAndHeightPerMass * eyeHeightFactor;
			result.vitality = (float)(spineHeight * massTotalFactor) * eyeHeightFactor;
			result.intelligence = foreheadWidth; // 12head
			result.agility = Mathf.Max((adjustedForeheadFactor + 1.0f) * 0.5f, 0.5f) * Mathf.Max(adjustedLegFactor, 0.5f);
			result.strengthUpper = shoulderBroadness * (massTotalFactor * (upperArmCircumference * (eyeHeightFactor / ((armLowerPercent + armUpperPercent) / 0.317f))));
			result.strengthLower = ((legQuadCircumference * massTotalFactor) * eyeHeightFactor) / legFactor;
			result.strengthGrip = sasquatchHandRating * eyeHeightFactor;
			return result;
		}

		private static float RadiusTotal(SLZAvatar.SoftEllipse ellipse) {
			return ellipse.XRadius + ellipse.ZRadius;
		}

		/// <summary>
		/// Adds the X and Z radii of the provided soft ellipses, and then divides by the amount of added radii.
		/// </summary>
		/// <param name="ellipses"></param>
		/// <returns></returns>
		private static float AverageTotalRadius(params SLZAvatar.SoftEllipse[] ellipses) {
			float total = 0;
			for (int i = 0; i < ellipses.Length; i++) {
				total += RadiusTotal(ellipses[i]);
			}
			total /= ellipses.Length << 1;
			return total;
		}

		/// <summary>
		/// Adds the X and Z radii of the provided soft ellipses.
		/// </summary>
		/// <param name="ellipses"></param>
		/// <returns></returns>
		private static float GrandTotalRadius(params SLZAvatar.SoftEllipse[] ellipses) {
			float total = 0;
			for (int i = 0; i < ellipses.Length; i++) {
				total += RadiusTotal(ellipses[i]);
			}
			return total;
		}

		/// <summary>
		/// The masses of every singular body part (that is, the arm/leg mass is the mass of only one arm or leg), as well as the total mass.
		/// </summary>
		public struct AvatarMasses {

			public float massTotal;
			public float massPelvis;
			public float massChest;
			public float massArm;
			public float massLeg;
			public float massBody;
			public float massHead;

			public override string ToString() {
				return $"Masses[Total={massTotal}kg, [Head={massHead}kg, Chest={massChest}kg, Pelvis={massPelvis}kg, One Arm={massArm}kg, One Leg={massLeg}]]";
			}

		}

		/// <summary>
		/// The stats of an avatar that are computed from its mass and other bodily information.
		/// </summary>
		public struct AvatarStats {

			public float agility;
			public float speed;
			public float strengthUpper;
			public float strengthLower;

			[Obsolete("This value is computed lazily; it is very likely wrong. It is strongly advised that you do NOT use this.")]
			public float strengthGrip;

			public float vitality;
			public float intelligence;

			public override string ToString() {
				return $"Stats[Agility={agility}Ns, Speed={speed}m/s, Lower Strength={strengthLower}, Upper Strength={strengthUpper}, Grip Strength (Secret)={strengthGrip}, Vitality={vitality}, Intelligence={intelligence}]";
			}

		}

		/// <summary>
		/// Additional information that is associated with avatars, used during internal computations.
		/// </summary>
		public struct AvatarComputationProperties {
			public float eyeHeight;
			public float t1HeightPercent;
			public Vector3 sternumOffset;
			public Vector3 sternumOffsetPercent;
			public Vector3 hipOffset;
			public Vector3 hipOffsetPercent;
			public Vector3 hipsPosition;
			public float sacrumHeightPercent;
			public float upperArmLength;
			public float upperLegLength;
			public float lowerArmLength;
			public float lowerLegLength;
			public float chestToShoulderPerc;
			public float handSizeMult;
		}

	}
}
#pragma warning restore CS0618