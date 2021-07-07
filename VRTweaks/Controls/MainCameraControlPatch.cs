

using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;

namespace VRTweaks.Controls
{
	/*class MainCameraControlPatch
	{
		[HarmonyPatch(typeof(MainCameraControl), nameof(MainCameraControl.Update))]
		public static class MainCameraControl_Update_Patch
		{
			[HarmonyPrefix]
			public static bool Prefix(MainCameraControl __instance)
			{
				float deltaTime = Time.deltaTime;
				if (Player.main.IsUnderwater())
				{
					__instance.swimCameraAnimation = Mathf.Clamp01(__instance.swimCameraAnimation + deltaTime);
				}
				else
				{
					__instance.swimCameraAnimation = Mathf.Clamp01(__instance.swimCameraAnimation - deltaTime);
				}
				Vector3 velocity = __instance.playerController.velocity;
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				bool inExosuit = Player.main.inExosuit;
				bool flag4 = false;
				bool flag5 = uGUI_BuilderMenu.IsOpen();
				bool flag6 = false;
				if (Player.main != null)
				{
					flag = Player.main.GetPDA().isInUse;
					flag3 = (Player.main.motorMode == Player.MotorMode.Vehicle);
					flag2 = (flag || flag3 || __instance.cinematicMode);
					flag6 = Player.main.inHovercraft;
					if (XRSettings.enabled && VROptions.gazeBasedCursor)
					{
						flag2 = (flag2 || flag5);
					}
				}
				if (flag2 != __instance.wasInLockedMode || __instance.lookAroundMode != __instance.wasInLookAroundMode)
				{
					__instance.camRotationX = 0f;
					__instance.camRotationY = 0f;
					__instance.wasInLockedMode = flag2;
					__instance.wasInLookAroundMode = __instance.lookAroundMode;
				}
				bool flag7 = (!__instance.cinematicMode || __instance.lookAroundMode) && !flag && __instance.mouseLookEnabled && (flag3 || AvatarInputHandler.main == null || AvatarInputHandler.main.IsEnabled() || Builder.isPlacing);
				if (flag3 && !XRSettings.enabled && !inExosuit && !flag4)
				{
					flag7 = false;
				}
				if (__instance.deathSequence)
				{
					flag7 = false;
				}
				Transform transform = __instance.transform;
				float num = (float)((flag || __instance.lookAroundMode || Player.main.GetMode() == Player.Mode.LockedPiloting) ? 1 : -1);
				if (!flag2 || (__instance.cinematicMode && !__instance.lookAroundMode))
				{
					__instance.cameraOffsetTransform.localEulerAngles = UWE.Utils.LerpEuler(__instance.cameraOffsetTransform.localEulerAngles, Vector3.zero, deltaTime * 5f);
				}
				else
				{
					transform = __instance.cameraOffsetTransform;
					__instance.rotationY = Mathf.LerpAngle(__instance.rotationY, 0f, PDA.deltaTime * 15f);
					__instance.transform.localEulerAngles = new Vector3(Mathf.LerpAngle(__instance.transform.localEulerAngles.x, 0f, PDA.deltaTime * 15f), __instance.transform.localEulerAngles.y, 0f);
					__instance.cameraUPTransform.localEulerAngles = UWE.Utils.LerpEuler(__instance.cameraUPTransform.localEulerAngles, Vector3.zero, PDA.deltaTime * 15f);
				}
				if (!XRSettings.enabled)
				{
					float num2 = __instance.camPDAZOffset * num * PDA.deltaTime / __instance.cameraPDAZoomDuration;
					Vector3 localPosition = __instance.cameraOffsetTransform.localPosition;
					localPosition.z = Mathf.Clamp(localPosition.z + num2, 0f, __instance.camPDAZOffset);
					__instance.cameraOffsetTransform.localPosition = localPosition;
				}
				else
				{
					__instance.animator.SetFloat(MainCameraControl.pdaDistanceParamId, VROptions.pdaDistance);
				}
				Vector2 vector = Vector2.zero;
				if (flag7 && FPSInputModule.current.lastGroup == null)
				{
					vector = GameInput.GetLookDelta();
					if (XRSettings.enabled && VROptions.disableInputPitch)
					{
						vector.y = 0f;
					}
					if (inExosuit || flag4)
					{
						vector.x = 0f;
					}
					vector *= Player.main.mesmerizedSpeedMultiplier;
					if (Player.main.frozenMixin.IsFrozen())
					{
						vector *= Player.main.frozenMixin.cameraSpeedMultiplier;
					}
				}
				if (__instance.deathSequence)
				{
					vector = new Vector2(Mathf.Cos(Time.time * 8.5f) * 25f * Time.deltaTime, -50f * Time.deltaTime + Mathf.Cos(Time.time * 8f) * 9f * Time.deltaTime);
				}
				__instance.UpdateCamShake();
				if (__instance.cinematicMode && !__instance.lookAroundMode)
				{
					if (__instance.cinematicOverrideRotation)
					{
						__instance.camRotationX = __instance.transform.localEulerAngles.y;
						__instance.camRotationY = -__instance.transform.localEulerAngles.x;
					}
					else
					{
						__instance.camRotationX = Mathf.LerpAngle(__instance.camRotationX, 0f, deltaTime * 2f);
						__instance.camRotationY = Mathf.LerpAngle(__instance.camRotationY, 0f, deltaTime * 2f);
						__instance.transform.localEulerAngles = new Vector3(-__instance.camRotationY, __instance.camRotationX, 0f);
					}
				}
				else if (flag2)
				{
					if (!XRSettings.enabled)
					{
						bool flag8 = (!__instance.lookAroundMode && !inExosuit && !flag4) || flag;
						bool flag9 = !__instance.lookAroundMode || flag;
						__instance.camRotationX += vector.x;
						__instance.camRotationY += vector.y;
						__instance.camRotationX = Mathf.Clamp(__instance.camRotationX, -60f, 60f);
						__instance.camRotationY = Mathf.Clamp(__instance.camRotationY, -60f, 60f);
						if (flag6)
						{
							__instance.cameraOffsetTransform.eulerAngles = __instance.vehicleOverrideHeadRot;
						}
						else
						{
							if (flag9)
							{
								__instance.camRotationX = Mathf.LerpAngle(__instance.camRotationX, 0f, PDA.deltaTime * 10f);
							}
							if (flag8)
							{
								__instance.camRotationY = Mathf.LerpAngle(__instance.camRotationY, 0f, PDA.deltaTime * 10f);
							}
							__instance.cameraOffsetTransform.localEulerAngles = new Vector3(-__instance.camRotationY, __instance.camRotationX, 0f);
						}
					}
				}
				else
				{
					__instance.rotationX += vector.x;
					__instance.rotationY += vector.y;
					__instance.rotationY = Mathf.Clamp(__instance.rotationY, __instance.minimumY, __instance.maximumY);
					__instance.cameraUPTransform.localEulerAngles = new Vector3(Mathf.Min(0f, -__instance.rotationY), 0f, 0f);
					transform.localEulerAngles = new Vector3(Mathf.Max(0f, -__instance.rotationY), __instance.rotationX, 0f);
				}
				__instance.UpdateStrafeTilt();
				Vector3 localEulerAngles = __instance.transform.localEulerAngles + new Vector3(0f, 0f, __instance.cameraAngleMotion.y * __instance.cameraTiltMod + __instance.strafeTilt * 0.5f);
				float num3 = 0f - __instance.skin;
				if (!flag2 && __instance.GetCameraBob())
				{
					float target = Mathf.Min(1f, velocity.magnitude / 5f);
					__instance.smoothedSpeed = Mathf.MoveTowards(__instance.smoothedSpeed, target, deltaTime);
					num3 += (Mathf.Sin(Time.time * 6f) - 1f) * (0.02f + __instance.smoothedSpeed * 0.15f) * __instance.swimCameraAnimation;
				}
				if (__instance.impactForce > 0f)
				{
					__instance.impactBob = Mathf.Min(0.9f, __instance.impactBob + __instance.impactForce * deltaTime);
					__instance.impactForce -= Mathf.Max(1f, __instance.impactForce) * deltaTime * 5f;
				}
				num3 -= __instance.impactBob;
				num3 -= __instance.stepAmount;
				if (__instance.impactBob > 0f)
				{
					__instance.impactBob = Mathf.Max(0f, __instance.impactBob - Mathf.Pow(__instance.impactBob, 0.5f) * deltaTime * 3f);
				}
				__instance.stepAmount = Mathf.Lerp(__instance.stepAmount, 0f, deltaTime * Mathf.Abs(__instance.stepAmount));
				float num4 = __instance.shakeAmount / 20f;
				float x = UnityEngine.Random.Range(-num4, num4);
				float y = UnityEngine.Random.Range(-num4, num4);
				float z = UnityEngine.Random.Range(-num4, num4);
				Vector3 b = __instance.initialOffset + new Vector3(x, y, z) * __instance.camShake;
				__instance.shakeOffset = Vector3.Lerp(__instance.shakeOffset, b, deltaTime * 20f);
				Vector3 b2 = flag6 ? __instance.vehicleOverrideHeadPos : Vector3.zero;
				__instance.transform.localPosition = new Vector3(0f, num3, 0f) + b2 + __instance.shakeOffset;
				__instance.transform.localEulerAngles = localEulerAngles;
				if (Player.main.motorMode == Player.MotorMode.Vehicle)
				{
					__instance.transform.localEulerAngles = Vector3.zero;
				}
				Vector3 localEulerAngles2 = new Vector3(Mathf.LerpAngle(__instance.viewModel.localEulerAngles.x, 0f, deltaTime * 5f), __instance.transform.localEulerAngles.y, 0f);
				Vector3 localPosition2 = __instance.transform.localPosition;
				if (XRSettings.enabled)
				{
					if (flag2 && !flag3)
					{
						//localEulerAngles2.z = __instance.viewModelLockedYaw;
					}
					else
					{
						//localEulerAngles2.z = 0f;
					}
					if (!flag3 && !__instance.cinematicMode)
					{
						if (!flag2)
						{
							//Quaternion rotation = __instance.playerController.forwardReference.rotation;
							//localEulerAngles2.y = (__instance.gameObject.transform.parent.rotation.GetInverse() * rotation).eulerAngles.y;
						}
						//localPosition2 = __instance.gameObject.transform.parent.worldToLocalMatrix.MultiplyPoint(__instance.playerController.forwardReference.position);
					}
				}
				__instance.viewModel.transform.localEulerAngles = localEulerAngles2;
				__instance.viewModel.transform.localPosition = localPosition2;
				return false;
			}
		}
	}*/
}
