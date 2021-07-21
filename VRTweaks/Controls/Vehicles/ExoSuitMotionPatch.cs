using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace VRTweaks.Controls.Vehicles
{
	/*class ExoSuitMotionPatch
	{
		[HarmonyPatch(typeof(Exosuit), nameof(Exosuit.Update))]
		public static class Exosuit_Update_Patch
		{
			[HarmonyPrefix]
			public static bool Prefix(Exosuit __instance)
			{
				//__instance.Update();
				__instance.UpdateThermalReactorCharge();
				if (__instance.storageContainer.GetOpen())
				{
					__instance.openedFraction = Mathf.Clamp01(__instance.openedFraction + Time.deltaTime * 2f);
				}
				else
				{
					__instance.openedFraction = Mathf.Clamp01(__instance.openedFraction - Time.deltaTime * 2f);
				}
				__instance.storageFlap.localEulerAngles = new Vector3(__instance.startFlapPitch + __instance.openedFraction * 80f, 0f, 0f);
				bool pilotingMode = __instance.GetPilotingMode();
				bool flag = pilotingMode && !__instance.docked;
				if (pilotingMode)
				{
					Player.main.transform.localPosition = Vector3.zero;
					Player.main.transform.localRotation = Quaternion.identity;
					Vector3 vector = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
					__instance.lastMoveDirection = vector;
					bool flag2 = vector.y > 0f;
					bool buttonHeld = GameInput.GetButtonHeld(GameInput.Button.Sprint);
					bool flag3 = __instance.IsPowered() && __instance.liveMixin.IsAlive();
					if ((flag2 || buttonHeld) && flag3)
					{
						float num = 0f;
						if (flag2)
						{
							num += __instance.verticalJetConsumption;
						}
						if (buttonHeld)
						{
							num += __instance.horizontalJetConsumption;
						}
						__instance.thrustPower = Mathf.Clamp01(__instance.thrustPower - Time.deltaTime * __instance.thrustConsumption * num);
						if (flag2 && (__instance.onGround || Time.time - __instance.timeOnGround <= 1f) && !__instance.jetDownLastFrame)
						{
							__instance.ApplyJumpForce();
						}
						__instance.jetsActive = true;
						__instance.horizontalJetsActive = buttonHeld;
						__instance.verticalJetsActive = flag2;
					}
					else
					{
						__instance.jetsActive = false;
						__instance.horizontalJetsActive = false;
						__instance.verticalJetsActive = false;
						float num2 = 0.7f;
						if (__instance.onGround)
						{
							num2 = 4f;
						}
						else if (vector.x != 0f || vector.z != 0f)
						{
							num2 = -0.7f;
						}
						__instance.thrustPower = Mathf.Clamp01(__instance.thrustPower + Time.deltaTime * __instance.thrustConsumption * num2);
					}
					__instance.jetDownLastFrame = flag2;
					__instance.footStepSounds.soundsEnabled = !__instance.powersliding;
					__instance.movementEnabled = !__instance.powersliding;
					if (__instance.timeJetsActiveChanged + 0.3f <= Time.time)
					{
						if ((__instance.jetsActive || __instance.powersliding) && __instance.thrustPower > 0f && !__instance.areFXPlaying && !__instance.IsUnderwater())
						{
							__instance.fxcontrol.Play(0);
							__instance.areFXPlaying = true;
						}
						else if (__instance.areFXPlaying)
						{
							__instance.fxcontrol.Stop(0);
							__instance.areFXPlaying = false;
						}
					}
					if (__instance.powersliding)
					{
						__instance.loopingSlideSound.Play();
					}
					else
					{
						__instance.loopingSlideSound.Stop();
					}
					if (flag2 || vector.x != 0f || vector.z != 0f)
					{
						__instance.ConsumeEngineEnergy(0.083333336f * Time.deltaTime);
					}
					if (__instance.jetsActive)
					{
						__instance.thrustIntensity += Time.deltaTime / __instance.timeForFullVirbation;
					}
					else
					{
						__instance.thrustIntensity -= Time.deltaTime * 10f;
					}
					__instance.thrustIntensity = Mathf.Clamp01(__instance.thrustIntensity);
					if (AvatarInputHandler.main.IsEnabled() && !__instance.ignoreInput)
					{
						XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPos);
						XRInputManager.GetXRInputManager().leftController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPos);
						XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rightRot);
						XRInputManager.GetXRInputManager().leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion leftRot);
						XRInputManager.GetXRInputManager().leftController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 leftVel);
						XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 rightVel);
						Vector3 righteulerAngles = rightRot.eulerAngles;//__instance.transform.eulerAngles;
						righteulerAngles.x = rightRot.eulerAngles.x;//MainCamera.camera.transform.eulerAngles.x;
						Vector3 lefteulerAngles = leftRot.eulerAngles;//__instance.transform.eulerAngles;
						lefteulerAngles.x = leftRot.eulerAngles.x;//MainCamera.camera.transform.eulerAngles.x;
						Quaternion rightquaternion = Quaternion.Euler(righteulerAngles.x, righteulerAngles.y, righteulerAngles.z);
						Quaternion leftrotation = Quaternion.Euler(lefteulerAngles.x, lefteulerAngles.y, lefteulerAngles.z); ;
						__instance.leftArm.Update(ref leftrotation);
						__instance.rightArm.Update(ref rightquaternion);
						if (flag)
						{
							Vector3 b = MainCamera.camera.transform.position + leftrotation * Vector3.forward * 100f;
							Vector3 b2 = MainCamera.camera.transform.position + rightquaternion * Vector3.forward * 100f;
							__instance.aimTargetLeft.transform.position = b;// Vector3.Lerp(__instance.aimTargetLeft.transform.position, b, Time.deltaTime * 15f);
							__instance.aimTargetRight.transform.position = b2; //Vector3.Lerp(__instance.aimTargetRight.transform.position, b2, Time.deltaTime * 15f);
						}
						bool hasPropCannon = __instance.rightArm is ExosuitPropulsionArm || __instance.leftArm is ExosuitPropulsionArm;
						__instance.UpdateUIText(hasPropCannon);
						Debug.Log("rightVel: " + rightVel.magnitude);
						Debug.Log("leftVel: " + leftVel.magnitude);
						if (rightVel.magnitude >= 0.1)
						//if (GameInput.GetButtonDown(GameInput.Button.AltTool) && !__instance.rightArm.OnAltDown())
						{
							__instance.leftArm.OnAltDown();
						}
					}
					__instance.UpdateActiveTarget();
					__instance.UpdateSounds();
					if (__instance.powersliding && __instance.onGround && __instance.timeLastSlideEffect + 0.5f < Time.time)
					{
						if (__instance.IsUnderwater())
						{
							__instance.fxcontrol.Play(4);
						}
						else
						{
							__instance.fxcontrol.Play(3);
						}
						__instance.timeLastSlideEffect = Time.time;
					}
				}
				if (!flag)
				{
					bool flag4 = false;
					bool flag5 = false;
					XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPos);
					XRInputManager.GetXRInputManager().leftController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPos);
					if (!Mathf.Approximately(__instance.aimTargetLeft.transform.localPosition.y, 0f))
					{
						float y = Mathf.MoveTowards(__instance.aimTargetLeft.transform.localPosition.y, 0f, Time.deltaTime * 50f);
						__instance.aimTargetLeft.transform.localPosition = new Vector3(__instance.aimTargetLeft.transform.localPosition.x, y, __instance.aimTargetLeft.transform.localPosition.z);
					}
					else
					{
						flag4 = true;
					}
					if (!Mathf.Approximately(__instance.aimTargetRight.transform.localPosition.y, 0f))
					{
						float y2 = Mathf.MoveTowards(__instance.aimTargetRight.transform.localPosition.y, 0f, Time.deltaTime * 50f);
						__instance.aimTargetRight.transform.localPosition = new Vector3(__instance.aimTargetRight.transform.localPosition.x, y2, __instance.aimTargetRight.transform.localPosition.z);
					}
					else
					{
						flag5 = true;
					}
					if (flag4 && flag5)
					{
						__instance.SetIKEnabled(false);
					}
					__instance.UpdateAnimations();
					if (__instance.armsDirty)
					{
						__instance.UpdateExosuitArms();
					}
				}
				return false;
			}
		}
	}*/
}
