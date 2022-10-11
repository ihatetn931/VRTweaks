
/*using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;
using UWE;
using VRTweaks;

namespace VRTweaks
{

	[HarmonyPatch(typeof(uGUI_CanvasScaler), nameof(uGUI_CanvasScaler.UpdateFrustum))]
	public static class HUDFixer1
	{
		[HarmonyPostfix]
		public static bool Prefix(Camera cam, uGUI_CanvasScaler __instance)
		{
			if (__instance.currentMode == uGUI_CanvasScaler.Mode.Inversed && __instance._anchor != null)
			{
				return false;
			}
			Vector2 screenSize = GraphicsUtil.GetScreenSize();
			float num = screenSize.x / screenSize.y;
			float num2 = __instance.distance * Mathf.Tan(cam.fieldOfView * 0.5f * 0.017453292f);
			float num3 = num2 * num;
			num3 *= 2f;
			num2 *= 2f;
			/*if (XRSettings.enabled)
			{
				float num4 = 0.1f;
				num3 *= 1f + num4;
				num2 *= 1f + num4;
			}
			float num5 = num3 / screenSize.x;
			float num6 = num2 / screenSize.y;
			float a = screenSize.x / __instance.referenceResolution.x;
			float b = screenSize.y / __instance.referenceResolution.y;
			float num7 = Mathf.Min(a, b);
			if (__instance.scaleMode > uGUI_CanvasScaler.ScaleMode.DontScale)
			{
				num7 *= uGUI_CanvasScaler._uiScale;
			}
			float num8 = 1f / num7;
			float num9 = screenSize.x * num8;
			float num10 = screenSize.y * num8;
			float num11 = num5 * num7;
			float num12 = num6 * num7;
			uGUI_CanvasScaler._uiScale = Loader.VRHudScale;
			//if (__instance._width != num9)
			//{
			__instance._width = num9;
			__instance.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, __instance._width - Loader.VRHudWidth);
			//}
			if (__instance._height != num10)
			{
				__instance._height = num10;
				__instance.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, __instance._height);
			}
			if (__instance._scaleX != num11 || __instance._scaleY != num12)
			{
				__instance._scaleX = num11;
				__instance._scaleY = num12;
				__instance.rectTransform.localScale = new Vector3(__instance._scaleX, __instance._scaleY, __instance._scaleX);
			}
			__instance.SetScaleFactor(num7);
			return false;
		}
	}
}*/
