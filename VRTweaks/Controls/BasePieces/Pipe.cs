
using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class Pipe
    {
		[HarmonyPatch(typeof(OxygenPipe), nameof(OxygenPipe.UpdatePlacement))]
		public static class OxygenPipe_UpdatePlacement__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(OxygenPipe __instance)
			{
				__instance.PrepareGhostModel();
				OxygenPipe.ghostModel.gameObject.SetActive(true);
				OxygenPipe.ghostModel.transform.position = VRHandsController.rightController.transform.right * 2f + VRHandsController.rightController.transform.position;
				OxygenPipe.ghostModel.UpdateGhostAttach();
				IPipeConnection parent = OxygenPipe.ghostModel.GetParent();
				if (parent != null)
				{
					OxygenPipe.ghostModel.gameObject.SetActive(true);
					OxygenPipe.ghostModel.transform.position = __instance.GetPipePosition(parent);
					OxygenPipe.ghostModel.UpdatePipe();
					return false;
				}
				OxygenPipe.ghostModel.gameObject.SetActive(false);
				return false;
			}
		}

		[HarmonyPatch(typeof(BasePipeConnector), nameof(BasePipeConnector.UpdateGhostModel))]
		public static class BasePipeConnector_UpdateGhostModel__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform aimTransform, GameObject ghostModel, RaycastHit hit, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase, ref bool __result, BasePipeConnector __instance)
			{
				bool result = false;
				geometryChanged = false;
				if (hit.collider && hit.collider.gameObject)
				{
					bool flag = Player.main.IsInsideWalkable();
					ghostModel.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(aimTransform.right, hit.normal), hit.normal);
					result = (Constructable.CheckFlags(__instance.allowedInBase, __instance.allowedInSub, __instance.allowedOutside, __instance.allowedUnderwater, aimTransform) && !flag && hit.collider.gameObject.GetComponentInParent<Base>() != null);
				}
				__result = result;
				return false;
			}
		}
	}
}
