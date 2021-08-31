using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class BaseCell
    {
		[HarmonyPatch(typeof(BaseAddCellGhost), nameof(BaseAddCellGhost.UpdateRotation))]
		public static class BaseAddCellGhost_UpdateRotation__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref bool geometryChanged, BaseAddCellGhost __instance)
			{
				if (__instance.cellTypes.Count < 2 || Builder.UpdateRotation(__instance.cellTypes.Count))
				{
					return false;
				}
				Base.CellType cellType = __instance.GetCellType();
				Int3 @int = Base.CellSize[(int)cellType];
				if (@int != __instance.ghostBase.GetSize())
				{
					__instance.ghostBase.ClearGeometry();
					__instance.ghostBase.SetSize(@int);
				}
				__instance.ghostBase.ClearCell(Int3.zero);
				__instance.ghostBase.SetCell(Int3.zero, cellType);
				__instance.RebuildGhostGeometry(true);
				geometryChanged = true;
				return false;
			}
		}
	}
}
