using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class Corridor
    {
		[HarmonyPatch(typeof(BaseAddCorridorGhost), nameof(BaseAddCorridorGhost.UpdateRotation))]
		public static class Builder_ShowRotationControlsHint__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref bool geometryChanged, BaseAddCorridorGhost __instance)
			{
				if (Builder.UpdateRotation(4))
				{
					return false;
				}
				__instance.corridorType = __instance.CalculateCorridorType();
				__instance.ghostBase.SetCorridor(Int3.zero, __instance.corridorType, __instance.isGlass);
				__instance.RebuildGhostGeometry(true);
				geometryChanged = true;
				return false;
			}
		}
	}
}
