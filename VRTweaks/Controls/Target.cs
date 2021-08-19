
using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls
{
    class Target
    {
		public static bool GetAllTarget(GameObject ignoreObj, float maxDistance, out GameObject result, out float distance)
		{
			if (ignoreObj != null)
			{
				Targeting.AddToIgnoreList(ignoreObj.transform);
			}
			return GetTargets(maxDistance, out result, out distance);
		}

		public static bool GetTargets(float maxDistance, out GameObject result, out float distance)
		{
			bool flag = false;
			Transform transform = Builder.GetAimTransform();// MainCamera.camera.transform;
			Vector3 position = transform.position;
			Vector3 forward = transform.right;
			Ray ray = new Ray(position, forward);
			int layerMask = -2097153;
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide;
			int numHits = UWE.Utils.RaycastIntoSharedBuffer(ray, maxDistance, layerMask, queryTriggerInteraction);
			DebugTargetConsoleCommand.radius = -1f;
			RaycastHit raycastHit;
			if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits, out raycastHit))
			{
				flag = true;
			}
			if (!flag)
			{
				foreach (float num in GameInput.IsPrimaryDeviceGamepad() ? Targeting.gamepadRadiuses : Targeting.standardRadiuses)
				{
					DebugTargetConsoleCommand.radius = num;
					ray.origin = position + forward * num;
					numHits = UWE.Utils.SpherecastIntoSharedBuffer(ray, num, maxDistance, layerMask, queryTriggerInteraction);
					if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits, out raycastHit))
					{
						flag = true;
						break;
					}
				}
			}
			Targeting.Reset();
			DebugTargetConsoleCommand.Stop();
			result = ((raycastHit.collider != null) ? raycastHit.collider.gameObject : null);
			distance = raycastHit.distance;
			return flag;
		}
	}
}
