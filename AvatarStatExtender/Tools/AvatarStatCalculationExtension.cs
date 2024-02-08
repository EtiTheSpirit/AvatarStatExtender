using UnityEngine;
using SLZAvatar = SLZ.VRMK.Avatar;
using System;

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
		private static Vector3 GetPositionBetween(Animator animator, HumanBodyBones first, HumanBodyBones second) {
			return Vector3.LerpUnclamped(animator.GetBoneTransform(first).position, animator.GetBoneTransform(second).position, 0.5f);
		}

		/// <summary>
		/// Returns the eye position of this avatar, depending on whether or not an eye center override is available.
		/// </summary>
		/// <param name="avatar"></param>
		/// <returns></returns>
		public static Vector3 EyePosition(this SLZAvatar avatar) {
			if (avatar.eyeCenterOverride) return avatar.eyeCenterOverride.position;
			return GetPositionBetween(avatar.animator, HumanBodyBones.LeftEye, HumanBodyBones.RightEye);
		}

		/// <summary>
		/// Returns the height of the eyes on this avatar.
		/// </summary>
		/// <param name="avatar"></param>
		/// <returns></returns>
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
			mass = ComputeMass(avatar, out AvatarAdditionalInfo additionalInfo);
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
		public static AvatarMasses ComputeMass(this SLZAvatar avatar, out AvatarAdditionalInfo additionalInfo, float normalizeTo82 = 0.5f) {
			Transform avyAnimatorTrs = avatar.transform; // This is the same as the animator's transform.
			Animator animator = avatar.animator;

			#region Value Precomputation Reimplementation

			#region Eye and Spine Height
			Quaternion worldToAnimator = Quaternion.Inverse(avyAnimatorTrs.rotation);
			Vector3 neckWorld = animator.GetBoneTransform(HumanBodyBones.Neck).position;

			float eyeHeight = avatar.EyePosition().y - avyAnimatorTrs.position.y; // avatar._eyeHeight;
			float t1HeightPercent = (neckWorld.y - avyAnimatorTrs.position.y) / eyeHeight;
			Vector3 sternumOffset = worldToAnimator * (Vector3.LerpUnclamped(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position, animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position, .5f) - animator.GetBoneTransform(HumanBodyBones.Neck).position);
			Vector3 sternumOffsetPercent = sternumOffset / eyeHeight;
			Vector3 hipOffset = worldToAnimator * (Vector3.LerpUnclamped(animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position, animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position, .5f) - animator.GetBoneTransform(HumanBodyBones.Hips).position);
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

			// REIMPLEMENTATION:
			Vector3 hipsPosition = animator.GetBoneTransform(HumanBodyBones.Hips).position;
			float sacrumHeightPercent = (hipsPosition.y - avyAnimatorTrs.position.y) / eyeHeight;
			#endregion

			#region Arm and Leg Lengths
			Vector3 humerousLf = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position - animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position;
			Vector3 wristPos = avatar.wristLf ? avatar.wristLf.position : animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
			Vector3 forearmLf = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position - wristPos;
			Vector3 femurLf = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position - animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position;
			Vector3 lowerLegLf = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position - animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
			// The "length" values take the percents computed by the vanilla gizmo code (which divides by eyeheight) and then multiplies it by eyeheight.
			// In short, it's just:
			float upperArmLength = humerousLf.magnitude;
			float upperLegLength = femurLf.magnitude;
			float lowerArmLength = forearmLf.magnitude;
			float lowerLegLength = lowerLegLf.magnitude;
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
			* torsoHeightProportion * 785.4f;

			float massPelvis = (avatar.WaistEllipseZ + avatar.WaistEllipseNegZ
				+ avatar.HipsEllipseZ + avatar.HipsEllipseNegZ
				+ avatar.bulgeButt.apexZ + avatar.bulgeAbdomen.apexZ + avatar.bulgeLowerBack.apexZ
			) * 0.5f * eyeHeight
			* ((eyeHeight * avatar.HipsEllipseX) + avatar.WaistEllipseX)
			* torsoHeightProportion * 785.4f;

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

			float distanceFromThumbProxToIndexProx = (animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal).position - animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal).position).magnitude;
			float lengthOfIndexProxSegment = (animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal).position - animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate).position).magnitude;
			float lengthOfIntermediateSegment = (animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate).position - animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal).position).magnitude;
			float handSizeMult = (distanceFromThumbProxToIndexProx + lengthOfIndexProxSegment + lengthOfIntermediateSegment) / 0.12762f;
			// TODO: ^ Might not actually be right.
			// The vector instructions were ... convoluted (to put it lightly) and reused some locals with different names which nuked readability.

			float chestToShoulderPerc = (worldToAnimator * (animator.GetBoneTransform(HumanBodyBones.Chest).position - animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position)).x;

			additionalInfo = new AvatarAdditionalInfo {
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
		/// are acquired through <see cref="ComputeMass(SLZAvatar, out AvatarAdditionalInfo, float)"/>.
		/// </summary>
		/// <param name="avatar"></param>
		/// <param name="masses"></param>
		/// <param name="additionalInfo"></param>
		/// <returns></returns>
		public static AvatarStats ComputeBaseStats(this SLZAvatar avatar, AvatarMasses masses, AvatarAdditionalInfo additionalInfo) {

			float eyeHeight = additionalInfo.eyeHeight;
			float foreheadWidth = avatar.ForeheadEllipseX / 0.044f;
			float eyeHeightFactor = eyeHeight / 1.638f;
			float massTotalFactor = masses.massTotal / 75.0f;
			float shoulderBroadness = ((additionalInfo.chestToShoulderPerc / 0.096579596f)
			+ (avatar.ChestEllipseX / 0.096579596f))
			* 0.5f;

			float spineHeight = ((additionalInfo.sternumOffsetPercent.y + additionalInfo.t1HeightPercent)
			- (additionalInfo.hipOffsetPercent.y + additionalInfo.sacrumHeightPercent))
			/ 0.31253165f;

			float upperArmCircumference = AverageTotalRadius(avatar.upperarmEllipse, avatar.elbowEllipse) * PI;
			upperArmCircumference /= 0.34f * PI;
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
			float lowerLegSize = AverageTotalRadius(avatar.calfEllipse, avatar.ankleEllipse); //avatar.calfEllipse.ZRadius + avatar.calfEllipse.XRadius + avatar.ankleEllipse.XRadius + avatar.ankleEllipse.ZRadius;

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
			result.strengthUpper = shoulderBroadness * massTotalFactor * upperArmCircumference * eyeHeightFactor / ((armLowerPercent + armUpperPercent) / 0.317f);
			result.strengthLower = legQuadCircumference * massTotalFactor * eyeHeightFactor / legFactor;
			result.strengthGrip = sasquatchHandRating * eyeHeightFactor;
			return result;
		}

		private static float RadiusTotal(SLZAvatar.SoftEllipse ellipse) {
			return ellipse.XRadius + ellipse.ZRadius;
		}

		private static float AverageTotalRadius(params SLZAvatar.SoftEllipse[] ellipses) {
			float total = 0;
			for (int i = 0; i < ellipses.Length; i++) {
				total += RadiusTotal(ellipses[i]);
			}
			total /= ellipses.Length << 1;
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
		public struct AvatarAdditionalInfo {
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