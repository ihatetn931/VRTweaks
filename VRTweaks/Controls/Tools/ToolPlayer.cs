
using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.Tools
{
    class ToolPlayer
    {
		[HarmonyPatch(typeof(PlayerTool), nameof(PlayerTool.SetHandIKTargetsEnabled))]
		public static class FPSInputModuler_ProcessMouseEvent__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(bool enabled,PlayerTool __instance)
			{
				//__instance.leftHandIKTarget.position = __instance.savedLeftHandIKTarget.InverseTransformDirection(__instance.savedRightHandIKTarget.position);
				if (enabled)
				{
					__instance.rightHandIKTarget = __instance.savedRightHandIKTarget;
					var test = __instance.GetComponentsInChildren<Transform>();
					if (test != null)
					{
						foreach (var t in test)
						{
							if (t.Find("left_hand_grip") != null)
							{
								var tform = t.Find("left_hand_grip").transform;
								if(tform != null)
									__instance.leftHandIKTarget = tform;
                            }
						}
					}
					//__instance.leftHandIKTarget = __instance.transform.Find("left_hand_grip");//__instance.savedLeftHandIKTarget;
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
		}
	}
}
