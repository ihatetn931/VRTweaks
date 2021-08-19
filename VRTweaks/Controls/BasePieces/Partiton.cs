using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class Partiton
    {
		[HarmonyPatch(typeof(BaseAddPartitionGhost), nameof(BaseAddPartitionGhost.UpdatePlacement))]
		public static class BaseAddPartitionGhost_UpdatePlacement__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase, ref bool __result, BaseAddPartitionGhost __instance)
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
				ConstructableBase componentInParent = __instance.GetComponentInParent<ConstructableBase>();
				float d = (componentInParent != null) ? componentInParent.placeDefaultDistance : 0f;
				Vector3 point = camera.position + camera.forward * d;
				Vector3 vector = __instance.targetBase.WorldToLocal(point);
				Int3 @int = __instance.targetBase.LocalToGrid(vector);
				Vector3 b = __instance.targetBase.GridToLocal(@int);
				Vector3 vector2 = vector - b;
				Base.Direction direction = (vector2.x > -vector2.z) ? ((vector2.x > vector2.z) ? Base.Direction.East : Base.Direction.North) : ((vector2.x > vector2.z) ? Base.Direction.South : Base.Direction.West);
				geometryChanged = __instance.UpdateDirection(direction);
				if (!__instance.targetBase.CanSetPartition(@int, __instance.partitionDirection))
				{
					geometryChanged |= __instance.SetupInvalid();
					return false;
				}
				Int3 int2 = __instance.targetBase.NormalizeCell(@int);
				Base.CellType cell = __instance.targetBase.GetCell(int2);
				Int3 v = Base.CellSize[(int)cell];
				Int3.Bounds a = new Int3.Bounds(@int, @int);
				Int3.Bounds b2 = new Int3.Bounds(int2, int2 + v - 1);
				Int3.Bounds bounds = Int3.Bounds.Union(a, b2);
				geometryChanged |= __instance.UpdateSize(bounds.size);
				Int3 int3 = @int - __instance.targetBase.GetAnchor();
				if (__instance.anchoredCell == null || __instance.anchoredCell.Value != int3)
				{
					__instance.anchoredCell = new Int3?(int3);
					__instance.ghostBase.CopyFrom(__instance.targetBase, bounds, bounds.mins * -1);
					__instance.ghostBase.ClearMasks();
					Int3 cell2 = @int - int2;
					Base.Face face = new Base.Face(cell2, __instance.partitionDirection);
					__instance.ghostBase.SetFaceMask(face, true);
					__instance.ghostBase.SetFaceType(face, Base.FaceType.Partition);
					__instance.RebuildGhostGeometry(true);
					geometryChanged = true;
				}
				ConstructableBase componentInParent2 = __instance.GetComponentInParent<ConstructableBase>();
				componentInParent2.transform.position = __instance.targetBase.GridToWorld(int2);
				componentInParent2.transform.rotation = __instance.targetBase.transform.rotation;
				positionFound = true;
				foreach (Int3 cell3 in bounds)
				{
					if (__instance.targetBase.IsCellUnderConstruction(cell3))
					{
						return false;
					}
				}
				__result = ghostModelParentConstructableBase.transform.position.y <= 35f || BaseGhost.GetDistanceToGround(ghostModelParentConstructableBase.transform.position) <= 25f;
				return false;
			}
		}
	}
}
