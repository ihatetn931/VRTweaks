using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;

namespace VRTweaks.Controls.UI
{

	[HarmonyPatch(typeof(FPSInputModule), nameof(FPSInputModule.ProcessMouseEvent))]
	public static class FPSInputModuler_ProcessMouseEvent__Patch
	{
		[HarmonyPrefix]
		static void Prefix(FPSInputModule __instance)
		{
			/*__instance.gameObject.AddComponent<FPSInput>();
			var test = __instance.GetComponent<FPSInput>();
			FPSInput.fpsInput = __instance;
			//ErrorMessage.AddDebug("Test: " + test);;
			if (test != null)
			{
				test.ProcessMouseEvent(__instance);
			}*/
			if (HandReticle.main != null)
			{
				HandReticle.main.RequestCrosshairHide();
				Cursor.visible = false;
			}
		}
	}

	/*[HarmonyPatch(typeof(uGUI_InputGroup), nameof(uGUI_InputGroup.Update))]
	public static class uGUI_InputGroup_Deselect__Patch
	{
		[HarmonyPrefix]
		static bool Prefix(uGUI_InputGroup __instance)
		{
			if (__instance.focused && Input.GetKeyDown(KeyCode.Escape) || GameInput.GetButtonDown(GameInput.Button.Exit))
			{
				__instance.Deselect(null);
			}
			return false;
		}
	}*/

	[HarmonyPatch(typeof(FPSInputModule), nameof(FPSInputModule.Awake))]
	class FPSInputModule_Awake_Patch
	{
		[HarmonyPostfix]
		public static void PostFix()
		{
			if (!XRSettings.enabled || !MotionControlConfig.EnableMotionControls || Player.main != null)
			{
				return;
			}
			VRHandsController.main.InitializeMenu();
		}
	}

	[HarmonyPatch(typeof(FPSInputModule), nameof(FPSInputModule.UpdateCursor))]
	class FPSInputModule_UpdateCursor_Patch
	{
		[HarmonyPostfix]
		public static void Postfix()
		{
			if (!XRSettings.enabled || !MotionControlConfig.EnableMotionControls || Player.main != null)
			{
				return;
			}
			VRHandsController.main.UpdateMenuPositions();
		}
	}

	[HarmonyPatch(typeof(FPSInputModule), nameof(FPSInputModule.GetCursorScreenPosition))]
	public static class FPSInputModuler_GetCursorScreenPosition__Patch
	{
		public static Vector2 result;
		[HarmonyPrefix]
		static bool Prefix(ref Vector2 __result, FPSInputModule __instance)
		{
			if (VROptions.GetUseGazeBasedCursor() || !Input.mousePresent || Cursor.lockState == CursorLockMode.Locked)
			{
				if (VRHandsController.rightController != null && FPSInputModule.current != null && Camera.main != null)
				{
					result = Camera.main.WorldToScreenPoint(VRHandsController.rightController.transform.position + VRHandsController.rightController.transform.forward * FPSInputModule.current.maxInteractionDistance);
				}
			}
			else
			{
				if (VRHandsController.rightController != null && FPSInputModule.current != null && Camera.main != null)
				{
					result = Camera.main.WorldToScreenPoint(VRHandsController.rightController.transform.position + VRHandsController.rightController.transform.forward * FPSInputModule.current.maxInteractionDistance);//new Vector2(VRHandsController.rightController.transform.position.x, VRHandsController.rightController.transform.position.y);
				}
			}
			__result = result;
			return false;
		}
	}
}