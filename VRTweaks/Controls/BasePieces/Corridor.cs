using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class Corridor
    {
		[HarmonyPatch(typeof(BaseAddCorridorGhost), nameof(BaseAddCorridorGhost.UpdatePlacement))]
		public static class BaseAddCorridorGhost_UpdatePlacement__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase, ref bool __result, BaseAddCorridorGhost __instance)
			{
				positionFound = false;
				geometryChanged = false;
				__instance.UpdateRotation(ref geometryChanged);
				float num = ghostModelParentConstructableBase.placeDefaultDistance;
				Vector3 position = camera.position;
				Vector3 forward = camera.right;
				__instance.targetBase = BaseGhost.FindBase(camera, 20f);
				bool flag;
				if (__instance.targetBase != null)
				{
					positionFound = true;
					flag = true;
					Vector3 point = position + forward * num;
					Int3 size = new Int3(1);
					Int3 @int = __instance.targetBase.WorldToGrid(point);
					if (__instance.targetBase.GetCell(@int) != Base.CellType.Empty || __instance.targetBase.IsCellUnderConstruction(@int))
					{
						@int = __instance.targetBase.PickCell(camera, point, size);
					}
					Base.Face face;
					if (__instance.targetBase.GetCell(@int) != Base.CellType.Empty && __instance.targetBase.WorldToGrid(camera.position) == @int && __instance.targetBase.PickFace(camera, out face))
					{
						@int = Base.GetAdjacent(face);
					}
					int y = __instance.targetBase.Bounds.mins.y;
					Int3 cell = @int;
					bool flag2;
					if (!__instance.CheckCorridorConnection(@int, out flag2))
					{
						int i = @int.y - 1;
						while (i >= y)
						{
							cell.y = i;
							if (__instance.targetBase.IsCellUnderConstruction(cell))
							{
								flag = false;
								break;
							}
							if (__instance.targetBase.GetCell(cell) != Base.CellType.Empty)
							{
								if (i < @int.y - 1)
								{
									flag = false;
									break;
								}
								break;
							}
							else
							{
								i--;
							}
						}
					}
					else if (flag2)
					{
						flag = false;
					}
					if (!__instance.targetBase.HasSpaceFor(@int, size))
					{
						flag = false;
					}
					Base.CellType cell2 = __instance.targetBase.GetCell(Base.GetAdjacent(@int, Base.Direction.Above));
					Base.CellType cell3 = __instance.targetBase.GetCell(Base.GetAdjacent(@int, Base.Direction.Below));
					if (cell2 == Base.CellType.Room || cell2 == Base.CellType.Observatory || cell2 == Base.CellType.Moonpool || cell2 == Base.CellType.MoonpoolRotated || cell2 == Base.CellType.MapRoom || cell2 == Base.CellType.MapRoomRotated || cell2 == Base.CellType.LargeRoom || cell2 == Base.CellType.LargeRoomRotated || cell3 == Base.CellType.Room || cell3 == Base.CellType.Observatory || cell3 == Base.CellType.Moonpool || cell3 == Base.CellType.MoonpoolRotated || cell3 == Base.CellType.MapRoom || cell3 == Base.CellType.MapRoomRotated || cell3 == Base.CellType.LargeRoom || cell3 == Base.CellType.LargeRoomRotated)
					{
						flag = false;
					}
					if (__instance.targetOffset != @int)
					{
						__instance.targetOffset = @int;
						__instance.RebuildGhostGeometry(true);
						geometryChanged = true;
					}
					ghostModelParentConstructableBase.transform.position = __instance.targetBase.GridToWorld(@int);
					ghostModelParentConstructableBase.transform.rotation = __instance.targetBase.transform.rotation;
				}
				else
				{
					Bounds aaBounds = Builder.aaBounds;
					num = BaseGhost.GetStartingDistance() + Mathf.Max(aaBounds.extents.x, aaBounds.extents.z);
					Vector3 position2;
					flag = __instance.PlaceWithBoundsCast(position, forward, aaBounds.extents, num, __instance.minHeightFromTerrain, __instance.maxHeightFromTerrain, out position2);
					ghostModelParentConstructableBase.transform.position = position2;
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
