

using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls
{
    class TraceFpsTarget
    {
		public static bool TraceFPSTarget(GameObject ignoreObj, float maxDist, ref GameObject closestObj, ref Vector3 position, out Vector3 normal, bool includeUseableTriggers = true)
		{
			bool result = false;
			normal = Vector3.up;
			var camera = VRHandsController.rightController;
			Vector3 position2 = camera.transform.position;
			int num = UWE.Utils.RaycastIntoSharedBuffer(new Ray(position2, camera.transform.forward), maxDist, -2097153, QueryTriggerInteraction.UseGlobal);
			if (num == 0)
			{
				num = UWE.Utils.SpherecastIntoSharedBuffer(position2, 0.7f, camera.transform.forward, maxDist, -2097153, QueryTriggerInteraction.UseGlobal);
			}
			closestObj = null;
			float num2 = 0f;
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[i];
				if ((!(ignoreObj != null) || !Utils.IsAncestorOf(ignoreObj, raycastHit.collider.gameObject)) && (!raycastHit.collider || !raycastHit.collider.isTrigger || (includeUseableTriggers && raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Useable"))) && (closestObj == null || raycastHit.distance < num2))
				{
					closestObj = raycastHit.collider.gameObject;
					num2 = raycastHit.distance;
					position = raycastHit.point;
					normal = raycastHit.normal;
					result = true;
				}
			}
			return result;
		}

	}
}
