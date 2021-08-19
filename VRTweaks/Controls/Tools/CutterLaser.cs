using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.Tools
{
    class CutterLaser
    {
		[HarmonyPatch(typeof(LaserCutter), nameof(LaserCutter.UpdateTarget))]
		public static class LaserCutter_UpdateTarget__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(LaserCutter __instance)
			{
				__instance.activeCuttingTarget = null;
				if (__instance.usingPlayer != null)
				{
					Vector3 vector = default(Vector3);
					GameObject gameObject = null;
					Vector3 vector2;
					TraceFpsTarget.TraceFPSTarget(Player.main.gameObject, 2f, ref gameObject, ref vector, out vector2, true);
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
						Sealed @sealed = gameObject.FindAncestor<Sealed>();
						if (@sealed)
						{
							__instance.activeCuttingTarget = @sealed;
							HandReticle.main.SetProgress(@sealed.GetSealedPercentNormalized());
							HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
						}
					}
				}
				return false;
			}
		}
	}
}
