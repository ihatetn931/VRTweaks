

using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class MapRoomcs
    {
		[HarmonyPatch(typeof(BaseAddMapRoomGhost), nameof(BaseAddMapRoomGhost.UpdateRotation))]
		public static class Builder_ShowRotationControlsHint__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref bool geometryChanged, BaseAddMapRoomGhost __instance)
			{
				if (__instance.cellTypes.Length < 2 || Builder.UpdateRotation(__instance.cellTypes.Length))
				{
					return false;
				}
				Base.CellType cellType = __instance.GetCellType();
				__instance.ghostBase.SetCell(Int3.zero, cellType);
				__instance.RebuildGhostGeometry(true);
				geometryChanged = true;
				return false;
			}
		}

		[HarmonyPatch(typeof(BaseAddMapRoomGhost), nameof(BaseAddMapRoomGhost.UpdatePlacement))]
		public static class BaseAddMapRoomGhost_UpdatePlacement__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase, ref bool __result, BaseAddMapRoomGhost __instance)
			{
				positionFound = false;
				geometryChanged = false;
				__instance.UpdateRotation(ref geometryChanged);
				float num = ghostModelParentConstructableBase.placeDefaultDistance;
				Int3 @int = Base.CellSize[8];
				Vector3 direction = Vector3.Scale((@int - 1).ToVector3(), Base.halfCellSize);
				Vector3 position = camera.position;
				Vector3 forward = camera.forward;
				__instance.targetBase = BaseGhost.FindBase(camera, 20f);
				bool flag;
				if (__instance.targetBase != null)
				{
					positionFound = true;
					flag = true;
					Vector3 a = position + forward * num;
					Vector3 b = __instance.targetBase.transform.TransformDirection(direction);
					Int3 int2 = __instance.targetBase.WorldToGrid(a - b);
					Int3 maxs = int2 + @int - 1;
					Int3.Bounds bounds = new Int3.Bounds(int2, maxs);
					foreach (Int3 cell in bounds)
					{
						if (__instance.targetBase.GetCell(cell) != Base.CellType.Empty || __instance.targetBase.IsCellUnderConstruction(cell))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						foreach (Int3 cell2 in bounds)
						{
							Base.CellType cell3 = __instance.targetBase.GetCell(Base.GetAdjacent(cell2, Base.Direction.Above));
							Base.CellType cell4 = __instance.targetBase.GetCell(Base.GetAdjacent(cell2, Base.Direction.Below));
							flag &= (cell3 == Base.CellType.Empty && cell4 == Base.CellType.Empty);
						}
					}
					if (__instance.targetOffset != int2)
					{
						__instance.targetOffset = int2;
						__instance.RebuildGhostGeometry(true);
						geometryChanged = true;
					}
					ghostModelParentConstructableBase.transform.position = __instance.targetBase.GridToWorld(int2);
					ghostModelParentConstructableBase.transform.rotation = __instance.targetBase.transform.rotation;
				}
				else
				{
					Bounds aaBounds = Builder.aaBounds;
					num = BaseGhost.GetStartingDistance() + Mathf.Max(aaBounds.extents.x, aaBounds.extents.z);
					Vector3 b2 = ghostModelParentConstructableBase.transform.TransformDirection(direction);
					Vector3 a2;
					flag = __instance.PlaceWithBoundsCast(position, forward, aaBounds.extents, num, __instance.minHeightFromTerrain, __instance.maxHeightFromTerrain, out a2);
					ghostModelParentConstructableBase.transform.position = a2 - b2;
					if (flag)
					{
						__instance.targetOffset = Int3.zero;
					}
				}
				if (flag && ghostModelParentConstructableBase.transform.position.y > 35f)
				{
					flag = (BaseGhost.GetDistanceToGround(ghostModelParentConstructableBase.transform.position) <= 25f);
				}
				__result = flag;
				return false;
			}
		}
	}
}
