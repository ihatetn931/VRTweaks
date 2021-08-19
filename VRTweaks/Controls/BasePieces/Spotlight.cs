using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class Spotlight
    {
		[HarmonyPatch(typeof(BaseSpotLight), nameof(BaseSpotLight.UpdateGhostModel))]
		public static class BaseSpotLight_UpdateGhostModel__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform aimTransform, GameObject ghostModel, RaycastHit hit, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase, ref bool __result, BaseSpotLight __instance)
			{
				bool result = false;
				geometryChanged = false;
				if (hit.collider && hit.collider.gameObject)
				{
					ghostModel.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(aimTransform.forward, hit.normal), hit.normal);
					result = (Constructable.CheckFlags(__instance.allowedInBase, __instance.allowedInSub, __instance.allowedOutside, __instance.allowedUnderwater, aimTransform) && hit.collider.gameObject.GetComponentInParent<Base>() != null);
				}
				__result = result;
				return false;
			}
		}
	}
}
