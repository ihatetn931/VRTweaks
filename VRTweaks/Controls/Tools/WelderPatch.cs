

using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.Tools
{
    class WelderPatch
    {
		[HarmonyPatch(typeof(Welder), nameof(Welder.UpdateTarget))]
		public static class Welder_UpdateTarget__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Welder __instance)
			{
				__instance.activeWeldTarget = null;
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
						LiveMixin liveMixin = gameObject.FindAncestor<LiveMixin>();
						if (liveMixin)
						{
							if (liveMixin.IsWeldable())
							{
								__instance.activeWeldTarget = liveMixin;
								return false;
							}
							WeldablePoint weldablePoint = gameObject.FindAncestor<WeldablePoint>();
							if (weldablePoint != null && weldablePoint.transform.IsChildOf(liveMixin.transform))
							{
								__instance.activeWeldTarget = liveMixin;
							}
						}
					}
				}
				return false;
			}
		}
	}
}
