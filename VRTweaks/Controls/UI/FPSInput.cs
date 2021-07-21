using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.UI
{
    class FPSInput
    {
		public static Vector2 result;
		[HarmonyPatch(typeof(FPSInputModule), nameof(FPSInputModule.GetCursorScreenPosition))]
		public static class FPSInputModuler_GetCursorScreenPosition__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref Vector2 __result)
			{
				if (VROptions.GetUseGazeBasedCursor() || !Input.mousePresent || Cursor.lockState == CursorLockMode.Locked)
				{
					result = GraphicsUtil.GetScreenSize() * 0.5f;
				}
				else
				{
					if (VRHandsController.rightController != null)
					{
						result = new Vector2(VRHandsController.rightController.transform.position.x, VRHandsController.rightController.transform.position.y);
					}
				}
				__result = result;
				return false;
			}

		}
	}
}
