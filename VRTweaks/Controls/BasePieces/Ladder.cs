using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class Ladder
    {
		[HarmonyPatch(typeof(BaseAddLadderGhost), nameof(BaseAddLadderGhost.UpdatePlacement))]
		public static class BaseAddLadderGhost_UpdatePlacement__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase, ref bool __result, BaseAddLadderGhost __instance)
			{
				positionFound = false;
				geometryChanged = false;
				RaycastHit raycastHit;
				if (!Physics.Raycast(camera.position, camera.forward, out raycastHit, placeMaxDistance, BaseGhost.placeLayerMask.value))
				{
					geometryChanged = __instance.SetupInvalid();
					return false;
				}
				__instance.targetBase = raycastHit.collider.GetComponentInParent<Base>();
				if (!__instance.targetBase)
				{
					__instance.targetBase = BaseGhost.FindBase(camera, 20f);
				}
				Base.Face face;
				if (!__instance.targetBase || !__instance.targetBase.PickFace(camera, out face))
				{
					geometryChanged = __instance.SetupInvalid();
					return false;
				}
				Base.Face face2;
				if (!__instance.targetBase.CanSetLadder(face, out face2))
				{
					geometryChanged = __instance.SetupInvalid();
					return false;
				}
				if (face.direction == Base.Direction.Below)
				{
					Base.Face face3 = face;
					face = face2;
					face2 = face3;
				}
				Int3 @int = __instance.targetBase.NormalizeCell(face.cell);
				Base.CellType cell = __instance.targetBase.GetCell(@int);
				Int3 v = Base.CellSize[(int)cell];
				Int3.Bounds a = new Int3.Bounds(face.cell, face2.cell);
				Int3.Bounds b = new Int3.Bounds(@int, @int + v - 1);
				Int3.Bounds bounds = Int3.Bounds.Union(a, b);
				Int3 size = bounds.size;
				geometryChanged = __instance.UpdateSize(size);
				if (__instance.isDirty || __instance.targetOffset != face.cell)
				{
					__instance.ghostBase.CopyFrom(__instance.targetBase, bounds, bounds.mins * -1);
					Base.Face face4 = new Base.Face(face.cell - bounds.mins, face.direction);
					Base.Face face5 = new Base.Face(face2.cell - bounds.mins, face2.direction);
					__instance.ghostBase.ClearMasks();
					__instance.ghostBase.SetFaceMask(face4, true);
					__instance.ghostBase.SetFaceMask(face5, true);
					__instance.ghostBase.SetFaceType(face4, Base.FaceType.Ladder);
					__instance.ghostBase.SetFaceType(face5, Base.FaceType.Ladder);
					for (int i = 1; i < face5.cell.y; i++)
					{
						Int3 cell2 = face5.cell;
						cell2.y = i;
						Base.Face face6 = new Base.Face(cell2, BaseAddLadderGhost.ladderFaceDir);
						__instance.ghostBase.SetFaceMask(face6, true);
						__instance.ghostBase.SetFaceType(face6, Base.FaceType.Ladder);
					}
					__instance.RebuildGhostGeometry(true);
					__instance.isDirty = false;
					geometryChanged = true;
				}
				ghostModelParentConstructableBase.transform.position = __instance.targetBase.GridToWorld(bounds.mins);
				ghostModelParentConstructableBase.transform.rotation = __instance.targetBase.transform.rotation;
				positionFound = true;
				foreach (Int3 cell3 in bounds)
				{
					if (__instance.targetBase.IsCellUnderConstruction(cell3))
					{
						return false;
					}
				}
				__instance.targetOffset = face.cell;
				__result = ghostModelParentConstructableBase.transform.position.y <= 35f || BaseGhost.GetDistanceToGround(ghostModelParentConstructableBase.transform.position) <= 25f;
				return false;
			}
		}
	}
}
