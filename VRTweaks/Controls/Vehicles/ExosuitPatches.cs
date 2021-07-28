
using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.Vehicles
{
    class ExosuitPatches
    {

		[HarmonyPatch(typeof(Exosuit), nameof(Exosuit.UpdateActiveTarget))]
		public static class Exosuit_UpdateActiveTarget__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Exosuit __instance)
			{
				GameObject target = null;
				float num;
				Target.GetAllTarget(__instance.gameObject, 6f, out target, out num);
				__instance.activeTarget = __instance.GetInteractableRoot(target);
				if (__instance.activeTarget != null)
				{
					GUIHand component = Player.main.GetComponent<GUIHand>();
					GUIHand.Send(__instance.activeTarget, HandTargetEventType.Hover, component);
				}
				return false;
			}

		}
		/*[HarmonyPatch(typeof(ExosuitDrillArm), nameof(ExosuitDrillArm.OnHit))]
		public static class ExosuitDrillArm_OnHitt__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ExosuitDrillArm __instance)
			{
				Exosuit componentInParent = __instance.GetComponentInParent<Exosuit>();
				if (__instance.exosuit.CanPilot() && __instance.exosuit.GetPilotingMode())
				{
					Vector3 zero = Vector3.zero;
					GameObject gameObject = null;
					__instance.drillTarget = null;
					Vector3 vector;
					TraceFpsTarget.TraceFPSTarget(__instance.exosuit.gameObject, 5f, ref gameObject, ref zero, out vector, true);
					if (gameObject == null)
					{
						InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
						if (component != null && component.GetMostRecent() != null)
						{
							gameObject = component.GetMostRecent().gameObject;
						}
					}
					if (gameObject && __instance.drilling)
					{
						Drillable drillable = gameObject.FindAncestor<Drillable>();
						__instance.loopHit.Play();
						if (!drillable)
						{
							LiveMixin liveMixin = gameObject.FindAncestor<LiveMixin>();
							if (liveMixin)
							{
								liveMixin.IsAlive();
								liveMixin.TakeDamage(4f, zero, DamageType.Drill, null);
								__instance.drillTarget = gameObject;
							}
							VFXSurface component2 = gameObject.GetComponent<VFXSurface>();
							if (__instance.drillFXinstance == null)
							{
								__instance.drillFXinstance = VFXSurfaceTypeManager.main.Play(component2, __instance.vfxEventType, __instance.fxSpawnPoint.position, __instance.fxSpawnPoint.rotation, __instance.fxSpawnPoint);
							}
							else if (component2 != null && __instance.prevSurfaceType != component2.surfaceType)
							{
								__instance.drillFXinstance.GetComponent<VFXLateTimeParticles>().Stop();
								UnityEngine.Object.Destroy(__instance.drillFXinstance.gameObject, 1.6f);
								__instance.drillFXinstance = VFXSurfaceTypeManager.main.Play(component2, __instance.vfxEventType, __instance.fxSpawnPoint.position, __instance.fxSpawnPoint.rotation, __instance.fxSpawnPoint);
								__instance.prevSurfaceType = component2.surfaceType;
							}
							gameObject.SendMessage("BashHit", __instance, SendMessageOptions.DontRequireReceiver);
							return false;
						}
						GameObject gameObject2;
						drillable.OnDrill(__instance.fxSpawnPoint.position, __instance.exosuit, out gameObject2);
						__instance.drillTarget = gameObject2;
						if (__instance.fxControl.emitters[0].fxPS != null && !__instance.fxControl.emitters[0].fxPS.emission.enabled)
						{
							__instance.fxControl.Play(0);
							return false;
						}
					}
					else
					{
						__instance.StopEffects();
					}
				}
				return false;
			}
		}
		[HarmonyPatch(typeof(ExosuitClawArm), nameof(ExosuitClawArm.OnHit))]
		public static class ExosuitClawArm_OnHitt__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ExosuitClawArm __instance)
			{
				Exosuit componentInParent = __instance.GetComponentInParent<Exosuit>();
				if (componentInParent.CanPilot() && componentInParent.GetPilotingMode())
				{
					Vector3 position = default(Vector3);
					GameObject gameObject = null;
					Vector3 vector;
					TraceFpsTarget.TraceFPSTarget(componentInParent.gameObject, 6.5f, ref gameObject, ref position, out vector, true);
					if (gameObject == null)
					{
						InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
						if (component != null && component.GetMostRecent() != null)
						{
							gameObject = component.GetMostRecent().gameObject;
						}
					}
					if (gameObject)
					{
						LiveMixin liveMixin = gameObject.FindAncestor<LiveMixin>();
						if (liveMixin)
						{
							liveMixin.IsAlive();
							liveMixin.TakeDamage(50f, position, DamageType.Normal, null);
							global::Utils.PlayFMODAsset(__instance.hitFishSound, __instance.front, 50f);
						}
						else
						{
							global::Utils.PlayFMODAsset(__instance.hitTerrainSound, __instance.front, 50f);
						}
						VFXSurface component2 = gameObject.GetComponent<VFXSurface>();
						Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
						VFXSurfaceTypeManager.main.Play(component2, __instance.vfxEventType, position, Quaternion.Euler(euler), componentInParent.gameObject.transform);
						gameObject.SendMessage("BashHit", __instance, SendMessageOptions.DontRequireReceiver);
					}

				}
				return false;
			}
		}*/
	}
}
