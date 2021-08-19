using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class Connector
    {
		[HarmonyPatch(typeof(BaseAddConnectorGhost), nameof(BaseAddConnectorGhost.UpdatePlacement))]
		public static class BaseAddConnectorGhost_UpdatePlacement__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase, ref bool __result, BaseAddConnectorGhost __instance)
			{
				positionFound = false;
				geometryChanged = false;
				__instance.targetBase = BaseGhost.FindBase(camera, 20f);
				if (__instance.targetBase == null)
				{
					return false;
				}
				float placeDefaultDistance = ghostModelParentConstructableBase.placeDefaultDistance;
				Vector3 position = camera.position;
				Vector3 forward = camera.forward;
				Vector3 point = position + forward * placeDefaultDistance;
				new Int3(1);
				Int3 @int = __instance.targetBase.WorldToGrid(point);
				Base.Face face;
				if (__instance.targetBase.GetCell(@int) != Base.CellType.Empty && __instance.targetBase.WorldToGrid(camera.position) == @int && __instance.targetBase.PickFace(camera, out face))
				{
					@int = Base.GetAdjacent(face);
				}
				if (!__instance.targetBase.CanSetConnector(@int))
				{
					return false;
				}
				ghostModelParentConstructableBase.transform.position = __instance.targetBase.GridToWorld(@int);
				ghostModelParentConstructableBase.transform.rotation = __instance.targetBase.transform.rotation;
				positionFound = true;
				__instance.targetOffset = @int;
				__result = ghostModelParentConstructableBase.transform.position.y <= 35f || BaseGhost.GetDistanceToGround(ghostModelParentConstructableBase.transform.position) <= 25f;
				return false;
			}
		}
	}
}
