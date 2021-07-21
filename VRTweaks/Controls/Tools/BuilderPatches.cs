using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRTweaks.Controls.Tools
{
	internal class BuilderPatches
	{
		[HarmonyPatch(typeof(Builder), nameof(Builder.CheckAsSubModule))]
		public static class Builder_CheckAsSubModule__Patch
		{
			[HarmonyPrefix]
			static bool Prefix( out Collider hitCollider, ref bool __result)
			{
				hitCollider = null;
				Transform aimTransform = Builder.GetAimTransform();
				if (!Constructable.CheckFlags(Builder.allowedInBase, Builder.allowedInSub, Builder.allowedOutside, Builder.allowedUnderwater, aimTransform))
				{
					return false;
				}
				Builder.placementTarget = null;
				RaycastHit hit;
				if (!Physics.Raycast(aimTransform.position, aimTransform.right, out hit, Builder.placeMaxDistance, Builder.placeLayerMask.value, QueryTriggerInteraction.Ignore))
				{
					return false;
				}
				hitCollider = hit.collider;
				Builder.placementTarget = hitCollider.gameObject;
				Builder.SetPlaceOnSurface(hit, ref Builder.placePosition, ref Builder.placeRotation);
				if (!Builder.CheckTag(hitCollider))
				{
					return false;
				}
				if (!Builder.CheckSurfaceType(Builder.GetSurfaceType(hit.normal)))
				{
					return false;
				}
				if (!Builder.CheckDistance(hit.point, Builder.placeMinDistance))
				{
					return false;
				}
				if (!Builder.allowedOnConstructables && Builder.HasComponent<Constructable>(hitCollider.gameObject))
				{
					return false;
				}
				if (!Player.main.IsInSub())
				{
					GameObject entityRoot = UWE.Utils.GetEntityRoot(Builder.placementTarget);
					if (!entityRoot)
					{
						entityRoot = Builder.placementTarget;
					}
					if (!Builder.ValidateOutdoor(entityRoot))
					{
						return false;
					}
				}
				__result =  true;
				return false; 
			}
		}

		[HarmonyPatch(typeof(Builder), nameof(Builder.GetAimTransform))]
		public static class Builder_GetAimTransform__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref Transform __result)
			{
				//VRHandsController.rightController.transform.rotation = MainCamera.camera.transform.rotation;
				__result = VRHandsController.rightController.transform;
				return false;
			}
		}

		/*[HarmonyPatch(typeof(Builder), nameof(Builder.UpdateRotation))]
		public static class Builder_UpdateRotation__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(int max,ref bool __result)
			{
				if (GameInput.GetButtonDown(Builder.buttonRotateCW) || VRHandsController.leftController.transform.rotation.y >= 60)
				{
					Builder.lastRotation = (Builder.lastRotation + max - 1) % max;
					return true;
				}
				if (GameInput.GetButtonDown(Builder.buttonRotateCCW) || VRHandsController.leftController.transform.rotation.y >= -60)
				{
					Builder.lastRotation = (Builder.lastRotation + 1) % max;
					return true;
				}
				__result = false;
				return false;
			}
		}*/

		[HarmonyPatch(typeof(Builder), nameof(Builder.SetDefaultPlaceTransform))]
		public static class Builder_SetDefaultPlaceTransform__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref Vector3 position, ref Quaternion rotation)
			{
				Transform aimTransform = Builder.GetAimTransform();
				position = aimTransform.position + aimTransform.right * Builder.placeDefaultDistance;
				Vector3 forward;
				Vector3 up;
				if (Builder.forceUpright)
				{
					forward = -aimTransform.right;
					forward.y = 0f;
					forward.Normalize();
					up = Vector3.up;
				}
				else
				{
					forward = -aimTransform.right;
					up = aimTransform.up;
				}
				rotation = Quaternion.LookRotation(forward, up);
				if (Builder.rotationEnabled)
				{
					rotation = Quaternion.AngleAxis(Builder.additiveRotation, up) * rotation;
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(Builder), nameof(Builder.SetPlaceOnSurface))]
		public static class Builder_SetPlaceOnSurface__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(RaycastHit hit, ref Vector3 position, ref Quaternion rotation)
			{
				Transform aimTransform = Builder.GetAimTransform();
				Vector3 vector = Vector3.right;
				Vector3 vector2 = Vector3.up;
				if (Builder.forceUpright)
				{
					vector = -aimTransform.right;
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
							vector = -aimTransform.right;
							vector.y -= Vector3.Dot(vector, vector2);
							vector.Normalize();
							break;
						case SurfaceType.Wall:
							vector = hit.normal;
							vector2 = Vector3.up;
							break;
						case SurfaceType.Ceiling:
							vector = hit.normal;
							vector2 = -aimTransform.right;
							vector2.y -= Vector3.Dot(vector2, vector);
							vector2.Normalize();
							break;
					}
				}
				position = hit.point;
				rotation = Quaternion.LookRotation(vector, vector2);
				if (Builder.rotationEnabled)
				{
					rotation = Quaternion.AngleAxis(Builder.additiveRotation, vector2) * rotation;
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(SNCameraRoot), nameof(SNCameraRoot.GetAimingTransform))]
		public static class SNCameraRoot_GetAimTransform__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref Transform __result)
			{
				__result = VRHandsController.rightController.transform;
				return false;
			}
		}
		

		[HarmonyPatch(typeof(Builder), nameof(Builder.UpdateAllowed))]
		public class Builder___UpdateAllowed__Patch
		{
			[HarmonyPrefix]
			public static bool Prefix(ref bool __result)
			{
				Builder.SetDefaultPlaceTransform(ref Builder.placePosition, ref Builder.placeRotation);
				bool flag = false;
				bool flag2 = false;
				using (ListPool<GameObject> listPool = Pool<ListPool<GameObject>>.Get())
				{
					List<GameObject> list = listPool.list;
					ConstructableBase componentInParent = Builder.ghostModel.GetComponentInParent<ConstructableBase>();
					if (componentInParent != null)
					{
						Transform transform = componentInParent.transform;
						transform.position = Builder.placePosition;
						transform.rotation = Builder.placeRotation;

						//Vector3 placePos = new Vector3(VRHandsController.rightController.transform.position.x, VRHandsController.rightController.transform.position.y, VRHandsController.rightController.transform.position.z);

						flag2 = componentInParent.UpdateGhostModel(Builder.GetAimTransform(), Builder.ghostModel, default(RaycastHit), out flag, componentInParent);
						//TODO: fix the raycast issue

						Builder.placePosition = transform.position;// + transform.right;

						Quaternion q = transform.rotation;
						q.eulerAngles = new Vector3(0, q.eulerAngles.y, 0);
						transform.rotation = q;
						Builder.placeRotation = transform.rotation;

						//LaserPointer.UpdatePointer(LaserPointer.DrawLine(transform.position, Camera.main.ScreenToWorldPoint(transform.position + transform.right) * Builder.placeMaxDistance));

						if (flag)
						{
							Builder.renderers = MaterialExtensions.AssignMaterial(Builder.ghostModel, Builder.ghostStructureMaterial, true);
							Builder.InitBounds(Builder.ghostModel);
						}
						Builder.GetObstacles(Builder.placePosition, Builder.placeRotation, Builder.bounds, null, list);
					}
					else
					{
						Collider allowedCollider;
						flag2 = Builder.CheckAsSubModule(out allowedCollider);
						Builder.CheckSpace(Builder.placePosition, Builder.placeRotation, Builder.bounds, Builder.placeLayerMask.value, allowedCollider, list);
					}
					flag2 &= (list.Count == 0);
					if (list.Count > 0)
					{
						for (int i = 0; i < list.Count; i++)
						{
							Builder.sRenderers.Clear();
							GameObject gameObject = list[i];
							if (!(gameObject.GetComponent<BaseCell>() != null))
							{
								gameObject.GetComponentsInChildren<Renderer>(Builder.sRenderers);
								for (int j = 0; j < Builder.sRenderers.Count; j++)
								{
									Renderer renderer = Builder.sRenderers[j];
									if (renderer.enabled && renderer.shadowCastingMode != ShadowCastingMode.ShadowsOnly && !(renderer is ParticleSystemRenderer))
									{
										Material[] sharedMaterials = renderer.sharedMaterials;
										int num = sharedMaterials.Length;
										for (int k = 0; k < num; k++)
										{
											Material material = sharedMaterials[k];
											if (material.renderQueue < 2450 && (!material.HasProperty(ShaderPropertyID._EnableCutOff) || material.GetFloat(ShaderPropertyID._EnableCutOff) <= 0f) && !material.IsKeywordEnabled("FX_BUILDING"))
											{
												Builder.obstaclesBuffer.DrawRenderer(renderer, Builder.builderObstacleMaterial, k);
											}
										}
									}
								}
							}
						}
					}
				}
				if (flag2)
				{
					Builder.ghostModel.GetComponentsInChildren<MountingBounds>(Builder.sMounts);
					int l = 0;
					int count = Builder.sMounts.Count;
					while (l < count)
					{
						if (!Builder.sMounts[l].IsMounted())
						{
							flag2 = false;
							break;
						}
						l++;
					}
					Builder.sMounts.Clear();
				}
				__result = flag2;
				return false;
			}
		}
	}
}
