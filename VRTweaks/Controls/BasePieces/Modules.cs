using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class Modules
    {
		[HarmonyPatch(typeof(BaseAddModuleGhost), nameof(BaseAddModuleGhost.UpdatePlacement))]
		public static class BaseAddModuleGhost_UpdatePlacement__Patch
		{
			[HarmonyPrefix]
			public static bool Prefix(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase, BaseAddModuleGhost __instance, ref bool __result)
			{
				positionFound = false;
				geometryChanged = false;
				geometryChanged |= Builder.UpdateRotation(Base.HorizontalDirections.Length);
				Base.Direction direction = Base.HorizontalDirections[Builder.lastRotation % Base.HorizontalDirections.Length];
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
				Base.Face face = new Base.Face(__instance.targetBase.WorldToGrid(camera.position + camera.right * d), direction);
				if (!__instance.targetBase.CanSetModule(ref face, __instance.faceType))
				{
					geometryChanged = __instance.SetupInvalid();
					return false;
				}
				Int3 @int = __instance.targetBase.NormalizeCell(face.cell);
				Base.Face face2 = new Base.Face(face.cell - __instance.targetBase.GetAnchor(), face.direction);
				if (__instance.anchoredFace == null || __instance.anchoredFace.Value != face2)
				{
					__instance.anchoredFace = new Base.Face?(face2);
					Base.CellType cell = __instance.targetBase.GetCell(@int);
					Int3 int2 = Base.CellSize[(int)cell];
					geometryChanged = __instance.UpdateSize(int2);
					__instance.ghostBase.CopyFrom(__instance.targetBase, new Int3.Bounds(@int, @int + int2 - 1), @int * -1);
					Int3 cell2 = face.cell - @int;
					Base.Face face3 = new Base.Face(cell2, face.direction);
					__instance.ghostBase.SetFaceType(face3, __instance.faceType);
					__instance.ghostBase.ClearMasks();
					__instance.ghostBase.SetFaceMask(face3, true);
					__instance.RebuildGhostGeometry(true);
					geometryChanged = true;
				}
				ghostModelParentConstructableBase.transform.position = __instance.targetBase.GridToWorld(@int);
				ghostModelParentConstructableBase.transform.rotation = __instance.targetBase.transform.rotation;
				positionFound = true;
				__result = !__instance.targetBase.IsCellUnderConstruction(face.cell) && (ghostModelParentConstructableBase.transform.position.y <= 35f || BaseGhost.GetDistanceToGround(ghostModelParentConstructableBase.transform.position) <= 25f);
				return false;
			}
		}
	}
}
