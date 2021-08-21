
using HarmonyLib;

namespace VRTweaks.Controls.Tools
{
    class ToolPlayer
    {
	/*	[HarmonyPatch(typeof(PlayerTool), nameof(PlayerTool.SetHandIKTargetsEnabled))]
		public static class FPSInputModuler_ProcessMouseEvent__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(bool enabled,PlayerTool __instance)
			{
				if (enabled)
				{
					__instance.rightHandIKTarget = __instance.savedRightHandIKTarget;
					__instance.leftHandIKTarget = __instance.savedLeftHandIKTarget;

					__instance.ikAimRightArm = __instance.savedIkAimRightArm;
					__instance.ikAimLeftArm = __instance.savedIkAimLeftArm;

					__instance.useLeftAimTargetOnPlayer = __instance.savedUseLeftAimTargetOnPlayer;
				}
				else
				{
					__instance.rightHandIKTarget = null;
					__instance.leftHandIKTarget = null;
					__instance.ikAimRightArm = false;
					__instance.ikAimLeftArm = false;
					__instance.useLeftAimTargetOnPlayer = false;
				}
				__instance.forceConfigureIK = true;
				return false;
			}
		}*/
	}
}
