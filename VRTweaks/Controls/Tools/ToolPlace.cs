
using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.Tools
{
    class ToolPlace
    {
		[HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.LateUpdate))]
		public static class PlaceTool_LateUpdate__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(PlaceTool __instance)
			{
				if (__instance.usingPlayer != null)
				{
					Transform aimTransform = Builder.GetAimTransform();
					RaycastHit raycastHit = default(RaycastHit);
					bool flag = false;
					int num = UWE.Utils.RaycastIntoSharedBuffer(aimTransform.position, -aimTransform.forward, 5f, -5, QueryTriggerInteraction.UseGlobal);
					float num2 = float.PositiveInfinity;
					for (int i = 0; i < num; i++)
					{
						RaycastHit raycastHit2 = UWE.Utils.sharedHitBuffer[i];
						if (!raycastHit2.collider.isTrigger && !UWE.Utils.SharingHierarchy(__instance.gameObject, raycastHit2.collider.gameObject) && num2 > raycastHit2.distance)
						{
							flag = true;
							raycastHit = raycastHit2;
							num2 = raycastHit2.distance;
						}
					}
					Vector3 forward = Vector3.forward;
					Vector3 up = Vector3.up;
					Vector3 position;
					if (flag)
					{
						PlaceTool.SurfaceType surfaceType = PlaceTool.SurfaceType.Floor;
						if (Mathf.Abs(raycastHit.normal.y) < 0.3f)
						{
							surfaceType = PlaceTool.SurfaceType.Wall;
						}
						else if (raycastHit.normal.y < 0f)
						{
							surfaceType = PlaceTool.SurfaceType.Ceiling;
						}
						position = raycastHit.point;
						if (__instance.alignWithSurface || surfaceType == PlaceTool.SurfaceType.Wall)
						{
							forward = raycastHit.normal;
							up = Vector3.up;
						}
						else
						{
							forward = new Vector3(-aimTransform.forward.x, 0f, -aimTransform.forward.z).normalized;
							up = Vector3.up;
						}
						switch (surfaceType)
						{
							case PlaceTool.SurfaceType.Floor:
								__instance.validPosition = __instance.allowedOnGround;
								break;
							case PlaceTool.SurfaceType.Wall:
								__instance.validPosition = __instance.allowedOnWalls;
								break;
							case PlaceTool.SurfaceType.Ceiling:
								__instance.validPosition = __instance.allowedOnCeiling;
								break;
						}
					}
					else
					{
						position = aimTransform.position + -aimTransform.forward * 1.5f;
						forward = -aimTransform.forward;
						up = Vector3.up;
						__instance.validPosition = false;
					}
					if (GameInput.GetButtonHeld(Builder.buttonRotateCW))
					{
						__instance.additiveRotation = MathExtensions.RepeatAngle(__instance.additiveRotation - Time.deltaTime * Builder.additiveRotationSpeed);
					}
					else if (GameInput.GetButtonHeld(Builder.buttonRotateCCW))
					{
						__instance.additiveRotation = MathExtensions.RepeatAngle(__instance.additiveRotation + Time.deltaTime * Builder.additiveRotationSpeed);
					}
					Quaternion quaternion = Quaternion.LookRotation(forward, up);
					if (__instance.rotationEnabled)
					{
						quaternion *= Quaternion.AngleAxis(__instance.additiveRotation, up);
					}
					__instance.ghostModel.transform.position = position;
					__instance.ghostModel.transform.rotation = quaternion;
					if (flag)
					{
						Rigidbody componentInParent = raycastHit.collider.gameObject.GetComponentInParent<Rigidbody>();
						__instance.validPosition = (__instance.validPosition && (componentInParent == null || componentInParent.isKinematic || __instance.allowedOnRigidBody));
					}
					SubRoot currentSub = Player.main.GetCurrentSub();
					bool flag2 = false;
					if (flag)
					{
						flag2 = (raycastHit.collider.gameObject.GetComponentInParent<SubRoot>() != null);
					}
					if (flag && raycastHit.collider.gameObject.CompareTag("DenyBuilding"))
					{
						__instance.validPosition = false;
					}
					if (!__instance.allowedUnderwater && raycastHit.point.y < 0f)
					{
						__instance.validPosition = false;
					}
					if (currentSub == null)
					{
						__instance.validPosition = (__instance.validPosition && (__instance.allowedOnBase || !flag2));
					}
					if (((__instance.allowedInBase && currentSub) || (__instance.allowedOutside && !currentSub)) && flag)
					{
						GameObject gameObject = UWE.Utils.GetEntityRoot(raycastHit.collider.gameObject);
						if (!gameObject)
						{
							SceneObjectIdentifier componentInParent2 = raycastHit.collider.GetComponentInParent<SceneObjectIdentifier>();
							if (componentInParent2)
							{
								gameObject = componentInParent2.gameObject;
							}
							else
							{
								gameObject = raycastHit.collider.gameObject;
							}
						}
						if (currentSub == null)
						{
							__instance.validPosition = (__instance.validPosition && Builder.ValidateOutdoor(gameObject));
						}
						if (!__instance.allowedOnConstructable)
						{
							__instance.validPosition = (__instance.validPosition && gameObject.GetComponentInParent<Constructable>() == null);
						}
						__instance.validPosition &= Builder.CheckSpace(position, quaternion, PlaceTool.localBounds, PlaceTool.placeLayerMask, raycastHit.collider);
					}
					else
					{
						__instance.validPosition = false;
					}
					MaterialExtensions.SetColor(__instance.modelRenderers, ShaderPropertyID._Tint, __instance.validPosition ? PlaceTool.placeColorAllow : PlaceTool.placeColorDeny);
					if (__instance.hideInvalidGhostModel)
					{
						__instance.ghostModel.SetActive(__instance.validPosition);
					}
				}
				return false;
			}

		}
	}
}
