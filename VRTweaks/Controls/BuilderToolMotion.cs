using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;
using UWE;

namespace VRTweaks.Controls
{
	internal class BuilderToolMotion
	{
		public static GameObject rightControl;

		[HarmonyPatch(typeof(Builder), "CheckDistance")]
		public static class Builder_Update__Patch
		{
			[HarmonyPrefix]
			public static bool Prefix(Vector3 worldPosition, float minDistance)
			{
				rightControl = new GameObject("rightControl");
				Player localPlayerComp = global::Utils.GetLocalPlayerComp();
				Vector3 localPosition;
				XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.devicePosition, out localPosition);
				Quaternion localRotation;
				XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.deviceRotation, out localRotation);
                rightControl.transform.localPosition = localPosition;
				rightControl.transform.localRotation = localRotation;
				rightControl.transform.parent = localPlayerComp.camRoot.transform;
				Transform transform = BuilderToolMotion.rightControl.transform;
				return (worldPosition - transform.position).magnitude >= minDistance;
			}
		}

		[HarmonyPatch(typeof(Builder), "SetDefaultPlaceTransform")]
		public static class Builder__Update__Patch
		{
			[HarmonyPrefix]
			public static bool Prefix(ref Vector3 position, ref Quaternion rotation)
			{
				rightControl = new GameObject("rightControl");
				Player localPlayerComp = global::Utils.GetLocalPlayerComp();
				Vector3 localPosition;
				XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.devicePosition, out localPosition);
				Quaternion localRotation;
				XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.deviceRotation, out localRotation);
				rightControl.transform.localPosition = localPosition;
				rightControl.transform.localRotation = localRotation;
				rightControl.transform.parent = localPlayerComp.camRoot.transform;
				Transform transform = BuilderToolMotion.rightControl.transform;
				position = transform.position + transform.forward * Builder.placeDefaultDistance;
				bool forceUpright = Builder.forceUpright;
				Vector3 forward;
				Vector3 up;
				if (forceUpright)
				{
					forward = -transform.forward;
					forward.y = 0f;
					forward.Normalize();
					up = Vector3.up;
				}
				else
				{
					forward = -transform.forward;
					up = transform.up;
				}
				rotation = Quaternion.LookRotation(forward, up);
				bool rotationEnabled = Builder.rotationEnabled;
				if (rotationEnabled)
				{
					rotation = Quaternion.AngleAxis(Builder.additiveRotation, up) * rotation;
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(Builder), "SetPlaceOnSurface")]
		public static class Builder___Update__Patch
		{
			[HarmonyPrefix]
			public static bool Prefix(RaycastHit hit, ref Vector3 position, ref Quaternion rotation)
			{
				rightControl = new GameObject("rightControl");
				Player localPlayerComp = global::Utils.GetLocalPlayerComp();
				Vector3 localPosition;
				XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.devicePosition, out localPosition);
				Quaternion localRotation;
				XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.deviceRotation, out localRotation);
				rightControl.transform.localPosition = localPosition;
				rightControl.transform.localRotation = localRotation;
				rightControl.transform.parent = localPlayerComp.camRoot.transform;
				Transform transform = rightControl.transform;
				Vector3 vector = Vector3.forward;
				Vector3 vector2 = Vector3.up;
				bool forceUpright = Builder.forceUpright;
				if (forceUpright)
				{
					vector = -transform.forward;
					vector.y = 0f;
					vector.Normalize();
					vector2 = Vector3.up;
				}
				else
				{
					switch (Builder.GetSurfaceType(hit.normal))
					{
						case SurfaceType.Ground:
							vector2 = hit.normal;
							vector = -transform.forward;
							vector.y -= Vector3.Dot(vector, vector2);
							vector.Normalize();
							break;
						case SurfaceType.Wall:
							vector = hit.normal;
							vector2 = Vector3.up;
							break;
						case SurfaceType.Ceiling:
							vector = hit.normal;
							vector2 = -transform.forward;
							vector2.y -= Vector3.Dot(vector2, vector);
							vector2.Normalize();
							break;
					}
				}
				position = hit.point;
				rotation = Quaternion.LookRotation(vector, vector2);
				bool rotationEnabled = Builder.rotationEnabled;
				if (rotationEnabled)
				{
					rotation = Quaternion.AngleAxis(Builder.additiveRotation, vector2) * rotation;
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(Builder), "CheckAsSubModule")]
		public static class Builder__Update___Patch
		{
			[HarmonyPrefix]
			public static bool Prefix(out Collider hitCollider)
			{
				hitCollider = null;
				rightControl = new GameObject("rightControl");
				Player localPlayerComp = global::Utils.GetLocalPlayerComp();
				Vector3 localPosition;
				XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.devicePosition, out localPosition);
				Quaternion localRotation;
				XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.deviceRotation, out localRotation);
                rightControl.transform.localPosition = localPosition;
                rightControl.transform.localRotation = localRotation;
                rightControl.transform.parent = localPlayerComp.camRoot.transform;
				Transform transform = rightControl.transform;
				bool flag = !Constructable.CheckFlags(Builder.allowedInBase, Builder.allowedInSub, Builder.allowedOutside, Builder.allowedUnderwater, transform);
				bool result;
				if (flag)
				{
					result = false;
				}
				else
				{
					Builder.placementTarget = null;
					RaycastHit hit;
					bool flag2 = !Physics.Raycast(transform.position, transform.forward, out hit, Builder.placeMaxDistance, Builder.placeLayerMask.value, QueryTriggerInteraction.Ignore);
					if (flag2)
					{
						result = false;
					}
					else
					{
						hitCollider = hit.collider;
						Builder.placementTarget = hitCollider.gameObject;
						Builder.SetPlaceOnSurface(hit, ref Builder.placePosition, ref Builder.placeRotation);
						bool flag3 = !Builder.CheckTag(hitCollider);
						if (flag3)
						{
							result = false;
						}
						else
						{
							bool flag4 = !Builder.CheckSurfaceType(Builder.GetSurfaceType(hit.normal));
							if (flag4)
							{
								result = false;
							}
							else
							{
								bool flag5 = !Builder.CheckDistance(hit.point, Builder.placeMinDistance);
								if (flag5)
								{
									result = false;
								}
								else
								{
									bool flag6 = !Builder.allowedOnConstructables && Builder.HasComponent<Constructable>(hitCollider.gameObject);
									if (flag6)
									{
										result = false;
									}
									else
									{
										bool flag7 = !Player.main.IsInSub();
										if (flag7)
										{
											GameObject gameObject = UWE.Utils.GetEntityRoot(Builder.placementTarget);
											bool flag8 = !gameObject;
											if (flag8)
											{
												gameObject = Builder.placementTarget;
											}
											bool flag9 = !Builder.ValidateOutdoor(gameObject);
											if (flag9)
											{
												return false;
											}
										}
										result = false;
									}
								}
							}
						}
					}
				}
				return result;
			}
		}
	}
}
