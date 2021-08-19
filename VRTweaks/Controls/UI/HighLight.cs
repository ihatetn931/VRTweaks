using HarmonyLib;
using HighlightingSystem;
using UnityEngine;
using UWE;

namespace VRTweaks.Controls.UI
{
    class HighLight
    {
		[HarmonyPatch(typeof(Highlighting), nameof(Highlighting.OnUpdate))]
		public static class Highlighting_OnUpdate__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Highlighting __instance)
			{
				float time = Time.time;
				Player main = Player.main;
				GUIHand guihand = (main != null) ? main.guiHand : null;
				var camera = VRHandsController.rightController;//MainCamera.camera;
				Vector3 vector = (camera != null) ? camera.transform.position : Vector3.zero;
				Highlighting.Mode mode = Highlighting.GetMode();
				Highlighting.Mode mode2 = __instance.lastMode;
				__instance.lastMode = mode;
				GameObject gameObject = null;
				if (mode != Highlighting.Mode.Player)
				{
					if (mode == Highlighting.Mode.Exosuit)
					{
						gameObject = (Player.main.GetVehicle() as Exosuit).GetActiveTarget();
					}
				}
				else
				{
					gameObject = ((guihand != null) ? guihand.GetActiveTarget() : null);
				}
				int num = (gameObject != null) ? gameObject.GetInstanceID() : 0;
				if (__instance.lastActiveTargetId != num)
				{
					__instance.hoverStart = time;
				}
				__instance.lastActiveTargetId = num;
				if (mode != Highlighting.Mode.None && __instance.updateActiveHighligtersCoroutine == null)
				{
					__instance.updateActiveHighligtersCoroutine = __instance.StartCoroutine(__instance.UpdateActiveHighlightersAsync(vector));
				}
				if (mode != Highlighting.Mode.None)
				{
					foreach (Highlighter highlighter in __instance.highlighters)
					{
						if (!(highlighter == null))
						{
							Color highlightingColor = MiscSettings.highlightingColor;
							float magnitude = (highlighter.bounds.center - vector).magnitude;
							float a = Mathf.Clamp01(1f - (magnitude - __instance.radiusInner) / __instance.radiusRange);
							highlightingColor.a = __instance.CompensateAlpha(a);
							highlighter.ConstantOn(highlightingColor, 0f);
						}
					}
					if (gameObject != null)
					{
						GameObject highlightableRoot = __instance.GetHighlightableRoot(mode, gameObject);
						if (highlightableRoot != null)
						{
							Highlighter component = highlightableRoot.GetComponent<Highlighter>();
							if (component != null)
							{
								Color highlightingColor2 = MiscSettings.highlightingColor;
								float a2 = Mathf.Lerp(0.4f, 1f, Mathf.Cos((time - __instance.hoverStart) * 2f * 3.1415927f * __instance.hoverFlashingSpeed) * 0.5f + 0.5f);
								highlightingColor2.a = __instance.CompensateAlpha(a2);
								component.Hover(highlightingColor2);
							}
						}
					}
				}
				return false;
			}
		}
	}
}
