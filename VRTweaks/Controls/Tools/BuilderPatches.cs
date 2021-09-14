using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using VRTweaks.Controls.UI;

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
				if (!Physics.Raycast(aimTransform.position, aimTransform.forward, out hit, Builder.placeMaxDistance, Builder.placeLayerMask.value, QueryTriggerInteraction.Ignore))
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
		private static ModeInputHandler inputHandler = new ModeInputHandler();
		public static readonly GameInput.Button buttonRotateCW = GameInput.Button.LookRight;
		public static readonly GameInput.Button buttonRotateCCW = GameInput.Button.LookLeft;

		public static bool inputHandlerActive
		{
			get
			{
				return InputHandlerStack.main != null && InputHandlerStack.main.IsFocused(inputHandler);
			}
		}
		
		[HarmonyPatch(typeof(Builder), nameof(Builder.UpdateRotation))]
		class Builder_UpdateRotation_Patch
		{
			[HarmonyPrefix]
			public static bool Prefix(int max, bool __result)
			{
				if (GameInput.GetButtonHeldTime(GameInput.Button.Reload) > 0.1f)
				{
					FPSInputModule.current.lockRotation = true;
					if (GameInput.GetButtonDown(buttonRotateCW))
					{
						Builder.lastRotation = (Builder.lastRotation + max - 1) % max;
						__result = true;
					}
					if (GameInput.GetButtonDown(buttonRotateCCW))
					{
						Builder.lastRotation = (Builder.lastRotation + 1) % max;
						__result = true;
					}
				}
				else
				{
					FPSInputModule.current.lockRotation = false;
					__result = false;
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(Builder), nameof(Builder.End))]
		class Builder_End_Patch
		{
			[HarmonyPrefix]
			public static bool Prefix()
			{
				inputHandler.canHandleInput = false;
				if (Builder.ghostModel != null)
				{
					ConstructableBase componentInParent = Builder.ghostModel.GetComponentInParent<ConstructableBase>();
					if (componentInParent != null)
					{
						UnityEngine.Object.Destroy(componentInParent.gameObject);
					}
					UnityEngine.Object.Destroy(Builder.ghostModel);
				}
				Builder.prefab = null;
				Builder.ghostModel = null;
				Builder.canPlace = false;
				Builder.placementTarget = null;
				Builder.additiveRotation = 0f;
				Builder.obstaclesBuffer.Clear();
				return false;
			}
		}
		
		[HarmonyPatch(typeof(Builder), nameof(Builder.ShowRotationControlsHint))]
		public static class Builder_ShowRotationControlsHint__Patch
		{
			[HarmonyPrefix]
			static bool Prefix()
			{
				ErrorMessage.AddError(Language.main.GetFormat<string, string>("GhostRotateInputHint", uGUI.FormatButton(buttonRotateCW, 
					true, "InputSeparator", false), uGUI.FormatButton(buttonRotateCCW, true, "InputSeparator", false)));
				return false;
			}
		}

		[HarmonyPatch(typeof(Builder), nameof(Builder.Update))]
		public static class Builder_Update__Patch
		{
			[HarmonyPrefix]
			static bool Prefix()
			{
				Builder.obstaclesBuffer.Clear();
				Builder.canPlace = false;
				if (Builder.prefab == null)
				{
					return false;
				}
				if (Builder.CreateGhost())
				{
					inputHandler.canHandleInput = true;
					InputHandlerStack.main.Push(inputHandler);
				}
				Builder.canPlace = Builder.UpdateAllowed();
				Transform transform = Builder.ghostModel.transform;
				transform.position = Builder.placePosition + Builder.placeRotation * Builder.ghostModelPosition;
				transform.rotation = Builder.placeRotation * Builder.ghostModelRotation;
				transform.localScale = Builder.ghostModelScale;
				Color value = Builder.canPlace ? Builder.placeColorAllow : Builder.placeColorDeny;
				IBuilderGhostModel[] components = Builder.ghostModel.GetComponents<IBuilderGhostModel>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].UpdateGhostModelColor(Builder.canPlace, ref value);
				}
				Builder.ghostStructureMaterial.SetColor(ShaderPropertyID._Tint, value);
				return false;
			}
		}

		[HarmonyPatch(typeof(Builder), nameof(Builder.SetDefaultPlaceTransform))]
		public static class Builder_SetDefaultPlaceTransform__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref Vector3 position, ref Quaternion rotation)
			{
				Transform aimTransform = Builder.GetAimTransform();
				position = aimTransform.position + aimTransform.forward * Builder.placeDefaultDistance;
				Vector3 forward;
				Vector3 up;
				if (Builder.forceUpright)
				{
					forward = aimTransform.forward;
					forward.y = 0f;
					forward.Normalize();
					up = Vector3.up;
				}
				else
				{
					forward = aimTransform.forward;
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
				Vector3 vector = -Vector3.forward;
				Vector3 vector2 = Vector3.up;
				if (Builder.forceUpright)
				{
					vector = aimTransform.forward;
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
							vector = aimTransform.forward;
							vector.y -= Vector3.Dot(vector, vector2);
							vector.Normalize();
							break;
						case SurfaceType.Wall:
							vector = hit.normal;
							vector2 = Vector3.up;
							break;
						case SurfaceType.Ceiling:
							vector = hit.normal;
							vector2 = aimTransform.forward;
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

		[HarmonyPatch(typeof(Builder), nameof(Builder.GetAimTransform))]
		public static class Builder_GetAimTransform__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref Transform __result)
			{
				__result = VRHandsController.rightController.transform;
				return false;
			}
		}

		[HarmonyPatch(typeof(SNCameraRoot), nameof(SNCameraRoot.GetAimingTransform))]
		public static class SNCameraRoot_GetAimingTransform__Patch
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
