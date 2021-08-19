
using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class BulkHead
    {
		[HarmonyPatch(typeof(BulkheadDoor), nameof(BulkheadDoor.GetSide))]
		public static class BulkheadDoor_GetSide__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref bool __result, BulkheadDoor __instance)
			{
				Transform aimingTransform = Builder.GetAimTransform();
				Vector3 position = aimingTransform.position;
				Vector3 position2 = __instance.frontSideDummy.position;
				Vector3 forward = __instance.frontSideDummy.forward;
				float num = Vector3.Dot(position - position2, forward);
				if (Mathf.Abs(num) > __instance.sideDistanceThreshold)
				{
					return num < 0f;
				}
				num = Vector3.Dot(aimingTransform.forward, forward);
				__result = Mathf.Approximately(num, 0f) || num > 0f;
				return false;
			}
		}

		[HarmonyPatch(typeof(BaseAddBulkheadGhost), nameof(BaseAddBulkheadGhost.UpdatePlacement))]
		public static class BaseAddBulkheadGhost_UpdatePlacement__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase, ref bool __result, BaseAddBulkheadGhost __instance)
			{
				positionFound = false;
				geometryChanged = false;
				Player main = Player.main;
				if (main == null || main.currentSub == null || !main.currentSub.isBase)
				{
					geometryChanged = __instance.SetupInvalid();
					return false;
				}
				__instance.targetBase = BaseGhost.FindBase(camera, 20f);
				if (__instance.targetBase == null)
				{
					geometryChanged = __instance.SetupInvalid();
					return false;
				}
				Vector3 normal = __instance.targetBase.transform.InverseTransformDirection(camera.forward);
				Base.Face adjacentFace = new Base.Face(__instance.targetBase.WorldToGrid(camera.position), Base.NormalToDirection(normal));
				if (__instance.IsCorridorConnector(adjacentFace))
				{
					adjacentFace = Base.GetAdjacentFace(adjacentFace);
					if (__instance.IsCorridorConnector(adjacentFace))
					{
						geometryChanged = __instance.SetupInvalid();
						return false;
					}
				}
				if (!__instance.targetBase.CanSetBulkhead(adjacentFace))
				{
					geometryChanged = __instance.SetupInvalid();
					return false;
				}
				Int3 @int = __instance.targetBase.NormalizeCell(adjacentFace.cell);
				if (__instance.face == null || __instance.face.Value.cell != adjacentFace.cell || __instance.face.Value.direction != adjacentFace.direction)
				{
					Base.CellType cell = __instance.targetBase.GetCell(@int);
					Int3 int2 = Base.CellSize[(int)cell];
					if (__instance.ghostBase.Shape.ToInt3() != int2)
					{
						__instance.ghostBase.SetSize(int2);
						__instance.ghostBase.AllocateMasks();
					}
					__instance.ghostBase.CopyFrom(__instance.targetBase, new Int3.Bounds(@int, @int + int2 - 1), @int * -1);
					Int3 cell2 = adjacentFace.cell - @int;
					Base.Face face = new Base.Face(cell2, adjacentFace.direction);
					__instance.ghostBase.SetFaceType(face, Base.FaceType.BulkheadClosed);
					__instance.ghostBase.ClearMasks();
					__instance.ghostBase.SetFaceMask(face, true);
					__instance.RebuildGhostGeometry(true);
					geometryChanged = true;
					__instance.face = new Base.Face?(adjacentFace);
				}
				ghostModelParentConstructableBase.transform.position = __instance.targetBase.GridToWorld(@int);
				ghostModelParentConstructableBase.transform.rotation = __instance.targetBase.transform.rotation;
				positionFound = true;
				if (__instance.targetBase.IsCellUnderConstruction(adjacentFace.cell) || __instance.targetBase.IsCellUnderConstruction(Base.GetAdjacent(adjacentFace)))
				{
					return false;
				}
				__instance.targetOffset = adjacentFace.cell;
				__result = ghostModelParentConstructableBase.transform.position.y <= 35f || BaseGhost.GetDistanceToGround(ghostModelParentConstructableBase.transform.position) <= 25f;
				return false;
			}
		}
	}
}
