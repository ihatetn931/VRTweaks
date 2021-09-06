﻿
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace VRTweaks.Controls.Vehicles
{
    class SeaglidePatches
    {
		[HarmonyPatch(typeof(VehicleInterface_MapController), nameof(VehicleInterface_MapController.Update))]
		public static class VehicleInterface_MapController_Update__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(VehicleInterface_MapController __instance)
			{
				bool flag = !Ocean.GetIsUnderwater(Player.main.transform.position + Vector3.down * 0.4f) && __instance.mapActive;
				__instance.staticInterferenceDisplay.gameObject.SetActive(flag);
				__instance.glitchedBeamFx.SetActive(flag);
				if (!flag)
				{
					__instance.mapStaticIntensity = -0.5f;
				}
				else
				{
					__instance.mapStaticIntensity += Time.deltaTime * 0.65f;
				}
				__instance.mapScript.SetMapGlitchIntensity(__instance.mapStaticIntensity);
				Color color = __instance.staticInterferenceDisplay.color;
				color.a = Mathf.Clamp01(__instance.mapStaticIntensity);
				__instance.staticInterferenceDisplay.color = color;
				if (__instance.prevMapStaticIntensity != __instance.mapStaticIntensity)
				{
					float t = __instance.glitchCurve.Evaluate(Mathf.Clamp01(__instance.mapStaticIntensity));
					foreach (KeyValuePair<Renderer, Color> keyValuePair in __instance.glitchableFxDic)
					{
						keyValuePair.Key.GetPropertyBlock(__instance.matBlock);
						Color value = Color.Lerp(keyValuePair.Value, __instance.glitchColor, t);
						__instance.matBlock.SetColor(ShaderPropertyID._Color, value);
						keyValuePair.Key.SetPropertyBlock(__instance.matBlock);
					}
				}
				__instance.prevMapStaticIntensity = __instance.mapStaticIntensity;
				if (!__instance.seaglide.HasEnergy())
				{
					__instance.mapScript.active = false;
					__instance.mapActive = false;
				}
				else if (Player.main != null && Player.main.currentSub != null)
				{
					__instance.mapScript.active = false;
					__instance.mapActive = false;
				}
				else
				{
					if (AvatarInputHandler.main.IsEnabled())
					{
						if (GameInput.GetButtonHeld(GameInput.Button.Jump) && GameInput.GetButtonDown(GameInput.Button.RightHand))
						{
								__instance.mapActive = !__instance.mapActive;
						}					
					}
					__instance.mapScript.active = __instance.mapActive;
				}
				if (__instance.mapScript.active)
				{
					__instance.playerDot.SetActive(true);
					__instance.lightVfx.SetActive(true);
					__instance.illumColor = Color.Lerp(__instance.seaglideIllumMat.GetColor(ShaderPropertyID._GlowColor), Color.white, Time.deltaTime);
					__instance.seaglideIllumMat.SetColor(ShaderPropertyID._GlowColor, __instance.illumColor);
					return false;
				}
				__instance.playerDot.SetActive(false);
				__instance.lightVfx.SetActive(false);
				__instance.illumColor = Color.Lerp(__instance.seaglideIllumMat.GetColor(ShaderPropertyID._GlowColor), Color.black, Time.deltaTime);
				__instance.seaglideIllumMat.SetColor(ShaderPropertyID._GlowColor, __instance.illumColor);
				return false;
			}
		}

		[HarmonyPatch(typeof(Seaglide), nameof(Seaglide.GetCustomUseText))]
		public static class Seaglide_GetCustomUseText_Patch
		{
			[HarmonyPrefix]
			public static bool Prefix(ref string __result, Seaglide __instance)
			{
				LanguageCache.ButtonText orAddNew = LanguageCache.buttonTextCache.GetOrAddNew("SeaglideMapToolip");
				var butt = Language.main.GetFormat<string>("SeaglideMapToolip", uGUI.FormatButton(GameInput.Button.Jump, false, " / ", false) + uGUI.FormatButton(GameInput.Button.RightHand, false, " / ", false));

				string buttonFormat = LanguageCache.GetButtonFormat("SeaglideLightsTooltip", GameInput.Button.RightHand);
				string buttonFormat2 = LanguageCache.GetButtonFormat("SeaglideMapToolip", GameInput.Button.Jump);
				if (__instance.cachedPrimaryUseText != buttonFormat || __instance.cachedAltUseText != buttonFormat2)
				{
					__instance.cachedPrimaryUseText = buttonFormat;
					__instance.cachedAltUseText = buttonFormat2;
					__instance.cachedUseText = string.Format("{0}, {1}", buttonFormat, butt);
				}
				__result = __instance.cachedUseText;
				return false;
			}
		}

		[HarmonyPatch(typeof(Seaglide), nameof(Seaglide.Update))]
		public static class Seaglide_Update_Patch
		{
			[HarmonyPrefix]
			public static bool Prefix(Seaglide __instance)
			{
				__instance.UpdateActiveState();
				__instance.UpdatePropeller();
				__instance.UpdatePropFX();
				__instance.UpdateUnderwaterState();
				__instance.UpdateEnergy();
				if (__instance.usingPlayer != null && __instance.toggleLights != null && !GameInput.GetButtonHeld(GameInput.Button.Jump))
				{
					__instance.toggleLights.CheckLightToggle();
				}
				if (__instance.activeState)
				{
					__instance.engineRPMManager.AccelerateInput(1f);
				}
				return false;
			}
		}

		/*[HarmonyPatch(typeof(Seaglide), nameof(Seaglide.UpdateActiveState))]
		public static class Seaglide_UpdateActiveState_Patch
		{
			[HarmonyPrefix]
			public static bool Prefix(Seaglide __instance)
			{
				bool flag = __instance.activeState;
				__instance.activeState = false;
				if (__instance.energyMixin.charge > 0f)
				{
					if (__instance.screenEffectModel != null)
					{
						__instance.screenEffectModel.SetActive(__instance.usingPlayer != null);
					}
					if (__instance.usingPlayer != null && __instance.usingPlayer.IsSwimming())
					{
						Vector3 moveDirection1 = GetMoveDirection();
						__instance.activeState = (moveDirection1.x != 0f || moveDirection1.z != 0f);
					}
					if (__instance.powerGlideActive)
					{
						__instance.activeState = true;
					}
				}
				if (flag != __instance.activeState)
				{
					__instance.SetVFXActive(__instance.activeState);
				}
				return false;
			}
			public static Vector3 moveDirection;

			public static void UpdateMoveDirection()
			{
				float num = 0f;
				num += GameInput.GetAnalogValueForButton(GameInput.Button.MoveForward);
				num -= GameInput.GetAnalogValueForButton(GameInput.Button.MoveBackward);
				float num2 = 0f;
				num2 -= GameInput.GetAnalogValueForButton(GameInput.Button.MoveLeft);
				num2 += GameInput.GetAnalogValueForButton(GameInput.Button.MoveRight);
				float num3 = 0f;
				num3 += GameInput.GetAnalogValueForButton(GameInput.Button.MoveUp);
				num3 -= GameInput.GetAnalogValueForButton(GameInput.Button.MoveDown);
				Vector3 moveDirection1 = VRHandsController.rightController.transform.position + VRHandsController.rightController.transform.forward * FPSInputModule.current.maxInteractionDistance;
				if (GameInput.autoMove && num * num + num2 * num2 > 0.010000001f)
				{
					GameInput.autoMove = false;
				}
				if (GameInput.autoMove)
				{
					moveDirection.Set(0f, moveDirection1.y, 1f);
				}
				else
				{
					moveDirection.Set(moveDirection1.x, moveDirection1.y, num);
				}
				/*if (GameInput.IsPrimaryDeviceGamepad())
				{
					if (GameInput.autoMove)
					{
						GameInput.isRunningMoveThreshold = false;
						return;
					}
					GameInput.isRunningMoveThreshold = (moveDirection.sqrMagnitude > 0.80999994f);
					if (!GameInput.isRunningMoveThreshold)
					{
						moveDirection /= 0.9f;
					}
				}
			}

			public static Vector3 GetMoveDirection()
			{
				return moveDirection;
			}
		}*/
	}
}
