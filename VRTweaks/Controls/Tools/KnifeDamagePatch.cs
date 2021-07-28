using FMOD.Studio;
using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;

namespace VRTweaks.Controls.Tools
{
	class KnifeDamagePatch
	{
		[HarmonyPatch(typeof(Knife), nameof(Knife.OnToolUseAnim))]
		class Knife__Patch
		{
			[HarmonyPrefix]
			public static bool Prefix(Knife __instance)
			{
				Vector3 position = default(Vector3);
				GameObject gameObject = null;
				Vector3 normal;
				TraceFpsTarget.TraceFPSTarget(Player.main.gameObject, __instance.attackDist, ref gameObject, ref position, out normal, true);
				if (gameObject == null)
				{
					InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
					if (component != null && component.GetMostRecent() != null)
					{
						gameObject = component.GetMostRecent().gameObject;
					}
				}
				ErrorMessage.AddDebug("KnifeGameObject: " + gameObject);
				if (gameObject)
				{
					LiveMixin liveMixin = gameObject.FindAncestor<LiveMixin>();
					ErrorMessage.AddDebug("KnifeliveMixin: " + liveMixin);
					if (Knife.IsValidTarget(liveMixin) && liveMixin)
					{
						bool wasAlive = liveMixin.IsAlive();
						XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 rightControllerVelocity);
						float calculatedDamge = rightControllerVelocity.magnitude * 50;
						Debug.Log("calculatedDamge: " + calculatedDamge);
						ErrorMessage.AddDebug("calculatedDamge: " + calculatedDamge);
						liveMixin.TakeDamage(calculatedDamge, position, __instance.damageType, global::Utils.GetLocalPlayer());
						__instance.GiveResourceOnDamage(gameObject, liveMixin.IsAlive(), wasAlive);
					}
					VFXSurface component2 = gameObject.GetComponent<VFXSurface>();
					Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
					VFXSurfaceTypeManager.main.Play(component2, __instance.vfxEventType, position, Quaternion.Euler(euler), Player.main.transform);
					VFXSurfaceTypes vfxsurfaceTypes = global::Utils.GetObjectSurfaceType(gameObject, VFXSurfaceTypes.none);
					if (vfxsurfaceTypes == VFXSurfaceTypes.none)
					{
						vfxsurfaceTypes = global::Utils.GetTerrainSurfaceType(position, normal, VFXSurfaceTypes.sand);
					}
					EventInstance fmodevent = global::Utils.GetFMODEvent(__instance.hitSound, __instance.transform.position);
					fmodevent.setParameterValueByIndex(__instance.surfaceParamIndex, (float)vfxsurfaceTypes);
					fmodevent.start();
					fmodevent.release();
				}

				global::Utils.PlayFMODAsset(Player.main.IsUnderwater() ? __instance.swingWaterSound : __instance.swingSound, __instance.transform.position, 20f);
				return false;
			}
		}
	}
}