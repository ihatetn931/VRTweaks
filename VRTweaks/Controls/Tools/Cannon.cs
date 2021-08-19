using FMOD.Studio;
using HarmonyLib;
using System;
using UnityEngine;
using UWE;

namespace VRTweaks.Controls.Tools
{
	class Cannon
	{
		[HarmonyPatch(typeof(PropulsionCannon), nameof(PropulsionCannon.GetObjectPosition))]
		public static class PropulsionCannon_GetObjectPosition__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(GameObject go, ref Vector3 __result, PropulsionCannon __instance)
			{
				var camera = VRHandsController.rightController;//MainCamera.camera;
				Vector3 b = Vector3.zero;
				float num = 0f;
				if (go != null)
				{
					Bounds aabb = __instance.GetAABB(go);
					b = go.transform.position - aabb.center;
					Ray ray = new Ray(aabb.center, camera.transform.forward);
					float f = 0f;
					if (aabb.IntersectRay(ray, out f))
					{
						num = Mathf.Abs(f);
					}
					__instance.grabbedObjectCenter = aabb.center;
				}
				Vector3 position = Vector3.forward * (2.5f + num) + __instance.localObjectOffset;
				__result = camera.transform.TransformPoint(position) + b;
				return false;
			}
		}

		[HarmonyPatch(typeof(PropulsionCannon), nameof(PropulsionCannon.FixedUpdate))]
		public static class PropulsionCannon_FixedUpdate__Patch
		{
			[HarmonyPrefix]
			static bool Prefix( PropulsionCannon __instance)
			{
				if (__instance.grabbedObject != null)
				{
					if (!__instance.ValidateObject(__instance.grabbedObject) || __instance.pickupDistance * 1.5f < (__instance.grabbedObject.transform.position - VRHandsController.rightController.transform.position).magnitude)
					{
						__instance.ReleaseGrabbedObject();
					}
					else
					{
						Rigidbody component = __instance.grabbedObject.GetComponent<Rigidbody>();
						Vector3 value = __instance.targetPosition - __instance.grabbedObject.transform.position;
						float magnitude = value.magnitude;
						float d = Mathf.Clamp(magnitude, 1f, 4f);
						Vector3 vector = component.velocity + Vector3.Normalize(value) * __instance.attractionForce * d * Time.deltaTime / (1f + component.mass * __instance.massScalingFactor);
						Vector3 amount = vector * (10f + Mathf.Pow(Mathf.Clamp01(1f - magnitude), 1.75f) * 40f) * Time.deltaTime;
						vector = UWE.Utils.SlerpVector(vector, Vector3.zero, amount);
						component.velocity = vector;
					}
				}
				if (__instance.firstUseGrabbedObject != null)
				{
					__instance.grabbedEffect.transform.position = __instance.firstUseGrabbedObject.transform.position;
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(PropulsionCannon), nameof(PropulsionCannon.OnShoot))]
		public static class PropulsionCannon_OnShoot__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref bool __result, PropulsionCannon __instance)
			{
				if (__instance.grabbedObject != null)
				{
					float num;
					float num2;
					__instance.energyInterface.GetValues(out num, out num2);
					float d = Mathf.Min(1f, num / 4f);
					Rigidbody component = __instance.grabbedObject.GetComponent<Rigidbody>();
					float d2 = 1f + component.mass * __instance.massScalingFactor;
					Vector3 vector = VRHandsController.rightController.transform.forward * __instance.shootForce * d / d2;
					Vector3 velocity = component.velocity;
					if (Vector3.Dot(velocity, vector) < 0f)
					{
						component.velocity = vector;
					}
					else
					{
						component.velocity = velocity * 0.3f + vector;
					}
					__instance.grabbedObject.GetComponent<PropulseCannonAmmoHandler>().OnShot(false);
					__instance.launchedObjects.Add(__instance.grabbedObject);
					__instance.grabbedObject = null;
					__instance.energyInterface.ConsumeEnergy(4f);
					global::Utils.PlayFMODAsset(__instance.shootSound, __instance.transform, 20f);
					__instance.fxControl.Play(0);
				}
				else
				{
					GameObject gameObject = __instance.TraceForGrabTarget();
					if (gameObject != null)
					{
						__instance.GrabObject(gameObject);
					}
					else
					{
						global::Utils.PlayFMODAsset(__instance.grabFailSound, __instance.transform, 20f);
					}
				}
				__result = true;
				return false;
			}
		}

		[HarmonyPatch(typeof(PropulsionCannon), nameof(PropulsionCannon.TraceForGrabTarget))]
		public static class PropulsionCannon_TraceForGrabTarget__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref GameObject __result, PropulsionCannon __instance)
			{
				Vector3 position = VRHandsController.rightController.transform.position;//MainCamera.camera.transform.position;
				int layerMask = ~(1 << LayerMask.NameToLayer("Player"));
				int num = UWE.Utils.SpherecastIntoSharedBuffer(position, 5.2f, VRHandsController.rightController.transform.forward, __instance.pickupDistance, layerMask, QueryTriggerInteraction.UseGlobal);
				GameObject result = null;
				float num2 = float.PositiveInfinity;
				PropulsionCannon.checkedObjects.Clear();
				for (int i = 0; i < num; i++)
				{
					RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[i];
					if (!raycastHit.collider.isTrigger || raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Useable"))
					{
						GameObject entityRoot = UWE.Utils.GetEntityRoot(raycastHit.collider.gameObject);
						if (entityRoot != null && !PropulsionCannon.checkedObjects.Contains(entityRoot))
						{
							if (!__instance.launchedObjects.Contains(entityRoot))
							{
								float sqrMagnitude = (raycastHit.point - position).sqrMagnitude;
								if (sqrMagnitude < num2 && __instance.ValidateNewObject(entityRoot, raycastHit.point, true))
								{
									result = entityRoot;
									num2 = sqrMagnitude;
								}
							}
							PropulsionCannon.checkedObjects.Add(entityRoot);
						}
					}
				}
				__result = result;
				return false;
			}

		}
	}
}
