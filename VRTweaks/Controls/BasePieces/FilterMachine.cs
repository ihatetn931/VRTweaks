using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class FilterMachine
    {
		[HarmonyPatch(typeof(BaseFiltrationMachineGeometry), nameof(BaseFiltrationMachineGeometry.OnUpdate))]
		public static class BaseFiltrationMachineGeometry_OnUpdate__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(BaseFiltrationMachineGeometry __instance)
			{
				__instance.UpdateVisuals(false);
				if (__instance.cachedScanning)
				{
					float num = 0f;
					if (__instance.itemVFXScan != null)
					{
						num = __instance.itemVFXScan.GetCurrentYPos();
					}
					Vector3 position = __instance.transform.position;
					position.y = num;
					Shader.SetGlobalFloat(ShaderPropertyID._FabricatorPosY, num + 0.03f);
					for (int i = 0; i < __instance.beams.Length; i++)
					{
						Transform transform = __instance.beams[i];
						__instance.sparks[i].position = BaseFiltrationMachineGeometry.GetBeamEnd(transform.position, transform.right, position, Vector3.up);
					}
				}
				return false;
			}
		}
	}
}
