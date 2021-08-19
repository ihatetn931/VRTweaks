using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class PartitionDoor
    {
		[HarmonyPatch(typeof(BaseAddPartitionDoorGhost), nameof(BaseAddPartitionDoorGhost.UpdatePlacement))]
		public static class BaseAddPartitionDoorGhost_UpdatePlacement__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase, ref bool __result, BaseAddPartitionDoorGhost __instance)
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
				Int3 @int = __instance.targetBase.WorldToGrid(camera.position + camera.forward * d);
				Base.Direction direction;
				if (!__instance.targetBase.CanSetPartitionDoor(@int, out direction))
				{
					geometryChanged = __instance.SetupInvalid();
					return false;
				}
				Int3 int2 = __instance.targetBase.NormalizeCell(@int);
				Base.CellType cell = __instance.targetBase.GetCell(int2);
				Int3 v = Base.CellSize[(int)cell];
				Int3.Bounds a = new Int3.Bounds(@int, @int);
				Int3.Bounds b = new Int3.Bounds(int2, int2 + v - 1);
				Int3.Bounds bounds = Int3.Bounds.Union(a, b);
				geometryChanged = __instance.UpdateSize(bounds.size);
				Base.Face face = new Base.Face(@int - __instance.targetBase.GetAnchor(), direction);
				if (__instance.anchoredFace == null || __instance.anchoredFace.Value != face)
				{
					__instance.anchoredFace = new Base.Face?(face);
					__instance.ghostBase.CopyFrom(__instance.targetBase, bounds, bounds.mins * -1);
					__instance.ghostBase.ClearMasks();
					Int3 cell2 = @int - int2;
					Base.Face face2 = new Base.Face(cell2, direction);
					__instance.ghostBase.SetFaceType(face2, Base.FaceType.PartitionDoor);
					__instance.ghostBase.SetFaceMask(face2, true);
					foreach (Base.Direction direction2 in Base.HorizontalDirections)
					{
						Base.Face face3 = new Base.Face(@int, direction2);
						if (__instance.targetBase.GetFace(face3) == Base.FaceType.Partition)
						{
							Base.Face face4 = new Base.Face(cell2, direction2);
							__instance.ghostBase.SetFaceMask(face4, true);
						}
					}
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
