using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class BaseFace
    {
		[HarmonyPatch(typeof(BaseAddFaceGhost), nameof(BaseAddFaceGhost.UpdatePlacement))]
		public static class BaseAddFaceGhost_UpdatePlacement__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase, ref bool __result, BaseAddFaceGhost __instance)
			{
				positionFound = false;
				geometryChanged = false;
				RaycastHit raycastHit;
				if (!Physics.Raycast(camera.position, camera.forward, out raycastHit, placeMaxDistance, BaseGhost.placeLayerMask.value))
				{
					geometryChanged = __instance.SetupInvalid();
					return false;
				}
				Collider collider = raycastHit.collider;
				__instance.targetBase = collider.GetComponentInParent<Base>();
				if (!__instance.targetBase || __instance.targetBase.GetComponent<BaseGhost>() != null)
				{
					__instance.targetBase = BaseGhost.FindBase(camera, 20f);
				}
				if (!__instance.targetBase)
				{
					geometryChanged = __instance.SetupInvalid();
					return false;
				}
				BaseExplicitFace componentInParent = collider.GetComponentInParent<BaseExplicitFace>();
				Base.Face face;
				if (componentInParent == null || componentInParent.face == null)
				{
					if (collider.GetComponentInParent<BaseDeconstructable>() != null || !__instance.targetBase.PickFace(camera, out face))
					{
						geometryChanged = __instance.SetupInvalid();
						return false;
					}
				}
				else
				{
					face = componentInParent.face.Value;
				}
				if (!__instance.targetBase.CanSetFace(face, __instance.faceType))
				{
					face = Base.GetAdjacentFace(face);
					if (!__instance.targetBase.CanSetFace(face, __instance.faceType))
					{
						geometryChanged = __instance.SetupInvalid();
						return false;
					}
				}
				Int3 @int = __instance.targetBase.NormalizeCell(face.cell);
				__instance.targetOffset = @int;
				Base.CellType cell = __instance.targetBase.GetCell(@int);
				Int3 v = Base.CellSize[(int)cell];
				Int3.Bounds a = new Int3.Bounds(face.cell, face.cell);
				Int3.Bounds b = new Int3.Bounds(@int, @int + v - 1);
				Int3.Bounds bounds = Int3.Bounds.Union(a, b);
				geometryChanged = __instance.UpdateSize(bounds.size);
				if (__instance.ghostBase.GetComponentInChildren<IBaseAccessoryGeometry>() != null)
				{
					geometryChanged = true;
				}
				Base.Face face2 = new Base.Face(face.cell - __instance.targetBase.GetAnchor(), face.direction);
				if (__instance.anchoredFace == null || __instance.anchoredFace.Value != face2)
				{
					__instance.anchoredFace = new Base.Face?(face2);
					__instance.ghostBase.CopyFrom(__instance.targetBase, bounds, bounds.mins * -1);
					__instance.ghostBase.ClearMasks();
					Int3 cell2 = face.cell - @int;
					Base.Face face3 = new Base.Face(cell2, face.direction);
					__instance.ghostBase.SetFaceMask(face3, true);
					__instance.ghostBase.SetFaceType(face3, __instance.faceType);
					__instance.RebuildGhostGeometry(true);
					geometryChanged = true;
				}
				ghostModelParentConstructableBase.transform.position = __instance.targetBase.GridToWorld(@int);
				ghostModelParentConstructableBase.transform.rotation = __instance.targetBase.transform.rotation;
				positionFound = true;
				if (__instance.faceType == Base.FaceType.Hatch)
				{
					Base.Face adjacentFace = Base.GetAdjacentFace(face);
					if (__instance.targetBase.GetFace(adjacentFace) == Base.FaceType.Hatch)
					{
						return false;
					}
				}
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
