
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using Yangrc.VolumeCloud;

namespace VRTweaks
{
    class MenuSliderPatches
    {
		[HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddKeyboardTab))]
		class uGUI_OptionsPanel_AddKeyboardTab_Patch
		{
			static bool Prefix(uGUI_OptionsPanel __instance)
			{
				int tabIndex = __instance.AddTab("Keyboard");
				__instance.AddToggleOption(tabIndex, "InvertLook", GameInput.GetInvertMouse(), new UnityAction<bool>(__instance.OnInvertMouseChanged), null);
				__instance.AddChoiceOption(tabIndex, "RunMode", uGUI_OptionsPanel.runModeOptions, GameInput.GetRunMode(), new UnityAction<int>(__instance.OnRunModeChanged), "RunModeTooltip");
				__instance.AddSliderOption(tabIndex, "MouseSensitivity", GameInput.GetMouseSensitivity(), 0.01f, 1f, GameInput.GetMouseSensitivity(), 0.01f, new UnityAction<float>(__instance.OnMouseSensitivityChanged), SliderLabelMode.Percent, "0", null);
				__instance.AddBindings(tabIndex, GameInput.Device.Keyboard);
				return false;
			}
		}

		[HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddAccessibilityTab))]
		class uGUI_OptionsPanel_AddAccessibilityTab_Patch
		{
			static bool Prefix(uGUI_OptionsPanel __instance)
			{
				int tabIndex = __instance.AddTab("Accessibility");
				__instance.AddSliderOption(tabIndex, "UIScale", MiscSettings.GetUIScale(DisplayOperationMode.Current), 0.7f, 1.4f, MiscSettings.GetUIScale(DisplayOperationMode.Current), 0.01f, delegate (float value)
				{
					MiscSettings.SetUIScale(value, DisplayOperationMode.Current);
				}, SliderLabelMode.Float, "0.00", null);
				__instance.AddToggleOption(tabIndex, "PDAPause", MiscSettings.pdaPause, new UnityAction<bool>(__instance.OnPDAPauseChanged), null);
				__instance.AddToggleOption(tabIndex, "Flashes", MiscSettings.flashes, new UnityAction<bool>(__instance.OnFlashesChanged), null);
				__instance.AddToggleOption(tabIndex, "Highlighting", MiscSettings.highlighting, new UnityAction<bool>(__instance.OnHighlightingChanged), null);
				__instance.highlightingColorOption = __instance.AddColorOption(tabIndex, "HighlightingColor", MiscSettings.highlightingColor, new UnityAction<Color>(__instance.OnHighlightingColorChanged));
				__instance.OnHighlightingChanged(MiscSettings.highlighting);
				return false;
			}
		}

		[HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddGraphicsTab))]
		class uGUI_OptionsPanel_AddGraphicsTab_Patch
		{
			static bool Prefix(uGUI_OptionsPanel __instance)
			{
				int tabIndex = __instance.AddTab("Graphics");
				__instance.AddSliderOption(tabIndex, "Gamma", GammaCorrection.gamma, 0.1f, 2.8f, GammaCorrection.gamma, 0.01f, delegate (float value)
				{
					GammaCorrection.gamma = value;
				}, SliderLabelMode.Float, "0.00", null);
				int qualityPresetIndex = __instance.GetQualityPresetIndex();
				__instance.qualityPresetOption = __instance.AddChoiceOption(tabIndex, "Preset", uGUI_OptionsPanel.presetOptions, qualityPresetIndex, new UnityAction<int>(__instance.OnQualityPresetChanged), null);
				__instance.ApplyQualityPreset(qualityPresetIndex);
				__instance.AddHeading(tabIndex, "Advanced");
				if (uGUI_MainMenu.main)
				{
					int currentIndex;
					string[] detailOptions = uGUI_OptionsPanel.GetDetailOptions(out currentIndex);
					__instance.detailOption = __instance.AddChoiceOption(tabIndex, "Detail", detailOptions, currentIndex, new UnityAction<int>(__instance.OnDetailChanged), null);
				}
				__instance.waterQualityOption = __instance.AddChoiceOption<WaterSurface.Quality>(tabIndex, "WaterQuality", WaterSurface.GetQualityOptions(), WaterSurface.GetQuality(), new UnityAction<WaterSurface.Quality>(__instance.OnWaterQualityChanged), null);
				__instance.skyboxQualityOption = __instance.AddChoiceOption(tabIndex, "SkyboxQuality", uGUI_OptionsPanel.skyboxQualityOptions, VolumeCloudRenderer.GetQuality(), new UnityAction<int>(__instance.OnASkyboxqualityChanged), null);
				int currentIndex2;
				string[] antiAliasingOptions = uGUI_OptionsPanel.GetAntiAliasingOptions(out currentIndex2);
				__instance.aaModeOption = __instance.AddChoiceOption(tabIndex, "Antialiasing", antiAliasingOptions, currentIndex2, new UnityAction<int>(__instance.OnAAmodeChanged), null);
				__instance.aaQualityOption = __instance.AddChoiceOption(tabIndex, "AntialiasingQuality", uGUI_OptionsPanel.postFXQualityNames, UwePostProcessingManager.GetAaQuality(), new UnityAction<int>(__instance.OnAAqualityChanged), null);
				__instance.bloomOption = __instance.AddToggleOption(tabIndex, "Bloom", UwePostProcessingManager.GetBloomEnabled(), new UnityAction<bool>(__instance.OnBloomChanged), null);
				if (!XRSettings.enabled)
				{
					__instance.lensDirtOption = __instance.AddToggleOption(tabIndex, "LensDirt", UwePostProcessingManager.GetBloomLensDirtEnabled(), new UnityAction<bool>(__instance.OnBloomLensDirtChanged), null);
					if (!GraphicsUtil.IsOpenGL())
					{
						__instance.dofOption = __instance.AddToggleOption(tabIndex, "DepthOfField", UwePostProcessingManager.GetDofEnabled(), new UnityAction<bool>(__instance.OnDofChanged), null);
					}
					__instance.motionBlurQualityOption = __instance.AddChoiceOption(tabIndex, "MotionBlurQuality", uGUI_OptionsPanel.postFXQualityNames, UwePostProcessingManager.GetMotionBlurQuality(), new UnityAction<int>(__instance.OnMotionBlurQualityChanged), null);
				}
				__instance.aoQualityOption = __instance.AddChoiceOption(tabIndex, "AmbientOcclusion", uGUI_OptionsPanel.postFXQualityNames, UwePostProcessingManager.GetAoQuality(), new UnityAction<int>(__instance.OnAOqualityChanged), null);
				if (!XRSettings.enabled)
				{
					__instance.ssrQualityOption = __instance.AddChoiceOption(tabIndex, "ScreenSpaceReflections", uGUI_OptionsPanel.postFXQualityNames, UwePostProcessingManager.GetSsrQuality(), new UnityAction<int>(__instance.OnSSRqualityChanged), null);
					__instance.ditheringOption = __instance.AddToggleOption(tabIndex, "Dithering", UwePostProcessingManager.GetDitheringEnabled(), new UnityAction<bool>(__instance.OnDitheringChanged), null);
				}
				__instance.weatherQualityOption = __instance.AddChoiceOption(tabIndex, "WeatherQuality", uGUI_OptionsPanel.weatherQualityOptions, VFXWeatherManager.GetQuality(), new UnityAction<int>(__instance.OnAWeatherQualityChanged), null);
				return false;
			}
		}

		/*[HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddGeneralTab))]
		class uGUI_OptionsPanel_AddGeneralTab_Patch
		{
			static bool Prefix(uGUI_OptionsPanel __instance)
			{
				int tabIndex = __instance.AddTab("General");
				if (__instance.showLanguageOption)
				{
					int currentIndex;
					__instance.languages = uGUI_OptionsPanel.GetLanguageOptions(out currentIndex);
					string[] languageKeys = uGUI_OptionsPanel.GetLanguageKeys(__instance.languages);
					__instance.AddChoiceOption(tabIndex, "Language", languageKeys, currentIndex, new UnityAction<int>(__instance.OnLanguageChanged), null);
				}
				__instance.AddToggleOption(tabIndex, "RunInBackground", MiscSettings.runInBackground, new UnityAction<bool>(__instance.OnRunInBackgroundChanged), "RunInBackgroundTooltip");
				__instance.AddToggleOption(tabIndex, "NewsEnabled", MiscSettings.newsEnabled, new UnityAction<bool>(__instance.OnNewsEnabledChanged), null);
				__instance.AddHeading(tabIndex, "Subtitles");
				__instance.subtitlesOption = __instance.AddToggleOption(tabIndex, "SubtitlesEnabled", Language.main.showSubtitles, new UnityAction<bool>(__instance.OnShowSubtitlesChanged), null);
				GameObject gameObject = __instance.AddSliderOption(tabIndex, "SubtitlesSpeed", Subtitles.speed, 1f, 70f, 15f, 1f, new UnityAction<float>(__instance.OnSubtitlesSpeedChanged), SliderLabelMode.Int, "00", null);
				MenuTooltip menuTooltip;
				gameObject.TryGetComponent<MenuTooltip>(out menuTooltip);
				if (menuTooltip != null)
				{
					UnityEngine.Object.Destroy(menuTooltip);
				}
				gameObject.AddComponent<SubtitlesSpeedTooltip>();
				if (XRSettings.enabled || !XRSettings.enabled)
				{
					__instance.AddHeading(tabIndex, "Display");
				}
				if (!XRSettings.enabled)
				{
					if (__instance.AreDisplayOptionsEnabled())
					{
						string[] resolutionOptions = uGUI_OptionsPanel.GetResolutionOptions(out __instance.resolutions);
						int currentResolutionIndex = uGUI_OptionsPanel.GetCurrentResolutionIndex(__instance.resolutions);
						__instance.resolutionOption = __instance.AddChoiceOption(tabIndex, "Resolution", resolutionOptions, currentResolutionIndex, new UnityAction<int>(__instance.OnResolutionChanged), null);
						__instance.AddToggleOption(tabIndex, "Fullscreen", Screen.fullScreen, new UnityAction<bool>(__instance.OnFullscreenChanged), null);
						__instance.AddToggleOption(tabIndex, "Vsync", QualitySettings.vSyncCount > 0, new UnityAction<bool>(__instance.OnVSyncChanged), null);
						__instance.targetFrameRateOption = __instance.AddSliderOption(tabIndex, "FPSCap", (float)GraphicsUtil.GetFrameRate(), (float)GraphicsUtil.frameRateMin, (float)GraphicsUtil.frameRateMax, 144f, 1f, new UnityAction<float>(__instance.OnTargetFrameRateChanged), SliderLabelMode.Int, "0", null);
						__instance.OnVSyncChanged(QualitySettings.vSyncCount > 0);
						int num = Display.displays.Length;
						string[] array = new string[num];
						for (int i = 0; i < num; i++)
						{
							array[i] = (i + 1).ToString();
						}
						__instance.AddChoiceOption(tabIndex, "Display", array, DisplayManager.GetCurrentDisplayIndex(), new UnityAction<int>(__instance.OnDisplayChanged), null);
					}
					Player main = Player.main;
					bool flag = IntroVignette.isIntroActive || (main != null && main.inHovercraft);
					using (new uGUI_InteractableScope(!flag))
					{
						__instance.vFovSlider = __instance.AddSliderOption(tabIndex, "FieldOfView1", MiscSettings.fieldOfView, 30f, 90f, MiscSettings.fieldOfView, 1f, new UnityAction<float>(__instance.OnVFovChanged), SliderLabelMode.Int, "0", null).GetComponentInChildren<uGUI_SnappingSlider>();
						__instance.hFovSlider = __instance.AddSliderOption(tabIndex, "FieldOfView2", __instance.V2H(MiscSettings.fieldOfView), __instance.V2H(30f), __instance.V2H(90f), __instance.V2H(MiscSettings.fieldOfView), 1f, new UnityAction<float>(__instance.OnHFovChanged), SliderLabelMode.Int, "0", null).GetComponentInChildren<uGUI_SnappingSlider>();
						goto IL_3BE;
					}
				}
				__instance.AddSliderOption(tabIndex, "VRRenderScale", XRSettings.eyeTextureResolutionScale, GameSettings.GetMinVrRenderScale(), 1f, XRSettings.eyeTextureResolutionScale, 0.05f, new UnityAction<float>(__instance.OnVRRenderScaleChanged), SliderLabelMode.Percent, "0", null);
				__instance.AddSliderOption(tabIndex, "VRPDADistance", VROptions.pdaDistance, 0f, 1f, VROptions.pdaDistance, 0.05f, new UnityAction<float>(__instance.OnVRPDADistance), SliderLabelMode.Percent, "0", null);
			IL_3BE:
				__instance.AddHeading(tabIndex, "Sound");
				__instance.AddSliderOption(tabIndex, "MasterVolume", SoundSystem.GetMasterVolume(), SoundSystem.GetMasterVolume(), new UnityAction<float>(__instance.OnMasterVolumeChanged));
				__instance.AddSliderOption(tabIndex, "MusicVolume", SoundSystem.GetMusicVolume(), SoundSystem.GetMusicVolume(), new UnityAction<float>(__instance.OnMusicVolumeChanged));
				__instance.AddSliderOption(tabIndex, "VoiceVolume", SoundSystem.GetVoiceVolume(), SoundSystem.GetVoiceVolume(), new UnityAction<float>(__instance.OnVoiceVolumeChanged));
				__instance.AddSliderOption(tabIndex, "AmbientVolume", SoundSystem.GetAmbientVolume(), SoundSystem.GetAmbientVolume(), new UnityAction<float>(__instance.OnAmbientVolumeChanged));
				return false;
			}
		}*/

		[HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddControllerTab))]
		class uGUI_OptionsPanel_AddControllerTab_Patch
		{
			static bool Prefix(uGUI_OptionsPanel __instance)
			{
				int tabIndex = __instance.AddTab("Controller");
				if (GameInput.IsKeyboardAvailable())
				{
					__instance.AddToggleOption(tabIndex, "EnableController", GameInput.GetControllerEnabled(), new UnityAction<bool>(__instance.OnControllerEnabledChanged), null);
				}
				if (XRSettings.enabled)
				{
					__instance.AddToggleOption(tabIndex, "GazeBasedCursor", VROptions.gazeBasedCursor, new UnityAction<bool>(__instance.OnGazeBasedCursorChanged), null);
				}
				GameInput.ControllerLayout chosenControllerLayout = GameInput.GetChosenControllerLayout();
				__instance.AddChoiceOption(tabIndex, "ControllerLayout", uGUI_OptionsPanel.controllerLayoutOptions, (int)chosenControllerLayout, new UnityAction<int>(__instance.OnControllerLayoutChanged), null);
				__instance.AddToggleOption(tabIndex, "EnableRumble", GameInput.GetRumbleEnabled(), new UnityAction<bool>(__instance.OnRumbleEnabledChanged), null);
				__instance.AddToggleOption(tabIndex, "InvertLook", GameInput.GetInvertController(), new UnityAction<bool>(__instance.OnInvertControllerChanged), null);
				__instance.AddChoiceOption(tabIndex, "RunMode", uGUI_OptionsPanel.runModeOptions, GameInput.GetRunMode(), new UnityAction<int>(__instance.OnRunModeChanged), "RunModeTooltip");
				__instance.AddSliderOption(tabIndex, "HorizontalSensitivity", GameInput.GetControllerHorizontalSensitivity(), 0.05f, 1f, GameInput.GetControllerHorizontalSensitivity(), 0.05f, new UnityAction<float>(__instance.OnControllerHorizontalSensitivityChanged), SliderLabelMode.Percent, "0", null);
				__instance.AddSliderOption(tabIndex, "VerticalSensitivity", GameInput.GetControllerVerticalSensitivity(), 0.05f, 1f, GameInput.GetControllerVerticalSensitivity(), 0.05f, new UnityAction<float>(__instance.OnControllerVerticalSensitivityChanged), SliderLabelMode.Percent, "0", null);
				__instance.AddBindings(tabIndex, GameInput.Device.Controller);
				return false;
			}
		}
	}
}
