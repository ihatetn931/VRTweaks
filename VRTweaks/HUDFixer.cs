
using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;
using UWE;
using VRTweaks;

namespace VRTweaks
{

	class HUDVROptions
	{

		static int generalTabIndex = 0;

		[HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddGeneralTab))]
		class uGUI_OptionsPanel_VROptionsPatch
		{
			static void Postfix(uGUI_OptionsPanel __instance)
			{
				__instance.AddHeading(generalTabIndex, "VR Hud Options");
				__instance.AddSliderOption(generalTabIndex, "Hud Scale", Loader.VRHudScale, 0.0001f, 1.0f, 1.0f, 0.0001f, (float scale) =>
				{
					Loader.VRHudScale = scale;
				}
				, SliderLabelMode.Float, "0.0000");

				__instance.AddSliderOption(generalTabIndex, "Hud Width", Loader.VRHudWidth, 10, 4000f, 1050f, 10f, (float width) =>
				{
					Loader.VRHudWidth = width;
				}
				, SliderLabelMode.Float, "0000");


			}

		}

		[HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.AddTab))]
		class uGUI_TabbedControlsPanel_GetGeneralTabPatch
		{
			static void Postfix(int __result, string label)
			{
				if (label.Equals("General"))
					generalTabIndex = __result;
			}
		}

		[HarmonyPatch(typeof(GameSettings), nameof(GameSettings.SerializeVRSettings))]
		class GameSettings_SerializeVRSettings_Patch
		{
			static void Postfix(GameSettings.ISerializer serializer)
			{
				Loader.VRHudScale = serializer.Serialize("VR/VRHUDScale", Loader.VRHudScale);
				Loader.VRHudWidth = serializer.Serialize("VR/VRHUDWidth", Loader.VRHudWidth);
			}
		}

		public static void OnSave(UserStorageUtils.SaveOperation saveOperation)
		{

			if (saveOperation.result == UserStorageUtils.Result.OutOfSpace && global::PlatformUtils.main.GetServices().GetDisplayOutOfSpaceMessage())
			{
				ErrorMessage.AddDebug("You Are Out Of Space");
			}
		}
	}
    


    [HarmonyPatch(typeof(uGUI_HealthBar), nameof(uGUI_HealthBar.LateUpdate))]
    public static class HUDFixer
    {
		[HarmonyPostfix]
		public static void Postfix(uGUI_HealthBar __instance)
		{
			//__instance.transform.parent.localPosition += new Vector3(400, 0, 0);
			if (GameInput.GetKey(KeyCode.KeypadPlus))
			{
				Loader.VRHudScale += 0.0001f;
			}
			if (GameInput.GetKey(KeyCode.KeypadMinus))
			{
				Loader.VRHudScale -= 0.0001f;
			}
			if (GameInput.GetKey(KeyCode.KeypadPlus) && GameInput.GetKey(KeyCode.LeftShift) || GameInput.GetKey(KeyCode.RightShift))
			{
				Loader.VRHudWidth += 10f;
			}
			if (GameInput.GetKey(KeyCode.KeypadMinus) && GameInput.GetKey(KeyCode.LeftShift) || GameInput.GetKey(KeyCode.RightShift))
			{
				Loader.VRHudWidth -= 10f;
			}
			CoroutineHost.StartCoroutine(GameSettings.SaveAsync(new GameSettings.OnSaveDelegate(HUDVROptions.OnSave)));
			MiscSettings.SetUIScale(0.5f);
		}
    }

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
			if (XRSettings.enabled)
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
			if (__instance._width != num9)
			{
				__instance._width = num9;
				__instance.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, __instance._width - Loader.VRHudWidth);
			}
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
}