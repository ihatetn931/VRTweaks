

using HarmonyLib;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using VREnhancementsBZ;

namespace VRTweaks
{
	public static class VRCustomOptionsMenu
	{
		public static uGUI_OptionsPanel optionsPanel;
		public static bool dynamicHUD = true;
		public static bool pinnedReciepehud = true;
		public static float subtitleHeight = 250;
		public static float subtitleX = 300;
		public static float subtitleScale = 1;
		public static float toolTipscale = 1;
		public static float PDA_Distance = 0.4f;
		public static float HUD_Alpha = 1;
		public static float HUD_Distance = 1;
		public static float HUD_Scale = 1;
		public static int HUD_Separation = 0;

		[HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddTabs))]
		class SubtitlesPosition_Patch
		{
			static bool Prefix(uGUI_OptionsPanel __instance)
			{
				optionsPanel = __instance;
				__instance.AddGeneralTab();
				if (__instance.showGameModeOptionsTab)
				{
					__instance.AddGameModeOptionsTab();
				}
				__instance.AddGraphicsTab();
				if (GameInput.IsKeyboardAvailable())
				{
					__instance.AddKeyboardTab();
				}
				if (GameInput.IsControllerAvailable())
				{
					__instance.AddControllerTab();
				}
				__instance.AddAccessibilityTab();
				if (XRSettings.enabled)
				{
					AddVrOptionsTab();
				}
				__instance.AddTroubleshootingTab();
				if (GraphOverlay.main.active)
				{
					__instance.AddDebugGraphsTab();
				}
				return false;
			}
		}
		public static string ColorString(string text, Color color)
		{
			return "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + text + "</color>";
		}

		[HarmonyPatch(typeof(GameSettings), nameof(GameSettings.SerializeVRSettings))]
		class SerializeVRSettings_Patch
		{
			static bool Prefix(GameSettings.ISerializer serializer)
			{
				XRSettings.eyeTextureResolutionScale = serializer.Serialize("VR/RenderScale", XRSettings.eyeTextureResolutionScale);
				VROptions.gazeBasedCursor = serializer.Serialize("VR/GazeBasedCursor", VROptions.gazeBasedCursor);
				VROptions.pdaDistance = serializer.Serialize("VR/PDADistance", VROptions.pdaDistance);
				// VRGameOptions.enableVrAnimations = serializer.Serialize("VR/EnableVRAnimations", VRGameOptions.enableVrAnimations);
				//VROptions.groundMoveScale = serializer.Serialize("VR/GroundMoveScale", VROptions.groundMoveScale);
				subtitleScale = serializer.Serialize("VR/SubtitleScale", subtitleScale);
				subtitleHeight = serializer.Serialize("VR/SubtitleYPos", subtitleHeight);
				//PDA_Distance = serializer.Serialize("VR/PDA_Distance", PDA_Distance);
				dynamicHUD = serializer.Serialize("VR/DynamicHUD", dynamicHUD);
				HUD_Distance = serializer.Serialize("VR/HUD_Distance", HUD_Distance);
				HUD_Scale = serializer.Serialize("VR/HUD_Scale", HUD_Scale);
				HUD_Alpha = serializer.Serialize("VR/HUD_Alpha", HUD_Alpha);
				HUD_Separation = serializer.Serialize("VR/HUD_Separation", HUD_Separation);
				VROptions.disableInputPitch = serializer.Serialize("VR/DisableInputPitch", VROptions.disableInputPitch);
				VROptions.groundMoveScale = serializer.Serialize("VR/GroundMoveScale", VROptions.groundMoveScale);
				SnapTurningOptions.EnableSnapTurning = serializer.Serialize("VR/EnableSnapTurning", SnapTurningOptions.EnableSnapTurning);
				SnapTurningOptions.SnapAngleChoiceIndex = serializer.Serialize("VR/SnapAngleChoiceIndex", SnapTurningOptions.SnapAngleChoiceIndex);
				VROptions.aimRightArmWithHead = serializer.Serialize("VR/AimWithHead", VROptions.aimRightArmWithHead);
				CameraFixes.seaglideYOffset = serializer.Serialize("VR/SeaglideYOffset", CameraFixes.seaglideYOffset);
				subtitleX = serializer.Serialize("VR/SubtitleX", subtitleX);
				toolTipscale = serializer.Serialize("VR/TooltipScale", toolTipscale);
				return false;
			}
		}

		public static void AddVrOptionsTab()
		{
			int tabIndex = optionsPanel.AddTab("VROptions");
			optionsPanel.AddHeading(tabIndex, ColorString("VR User Interface Options", new Color(255, 140, 0)));

			optionsPanel.AddSliderOption(tabIndex, "Subtitle Height", subtitleHeight, 10, 2000, 50, 1, delegate (float v)
			{
				subtitleHeight = v;
				UIElementsFixes.SetSubtitleHeight(subtitleHeight);
			}, SliderLabelMode.Float, "0000", "Sets the Subtitle Height");

			optionsPanel.AddSliderOption(tabIndex, "Subtitle X Position", subtitleX, 10, 2000, 50, 1, delegate (float v)
			{
				subtitleX = v;
				UIElementsFixes.SetSubtitleX(subtitleX);
			}, SliderLabelMode.Float, "0000", "Sets the X Position (left and right)");

			optionsPanel.AddSliderOption(tabIndex, "Subtitle Scale", subtitleScale * 100, 50, 150, 100, 1, delegate (float v)
			{
				subtitleScale = v / 100;
				UIElementsFixes.SetSubtitleScale(subtitleScale);
			}, SliderLabelMode.Float, "000","Sets The Subtitle Scale");

			optionsPanel.AddSliderOption(tabIndex, "Tooltip Scale", toolTipscale * 100, 0.0001f, 1.0f, 0.002f, 0.0001f, delegate (float v)
			{
				toolTipscale = v / 100;
				UIElementsFixes.SetTooltipScale(toolTipscale);
			}, SliderLabelMode.Float, "0.0000", "Sets The Tooltip Scale");

			optionsPanel.AddSliderOption(tabIndex, "UIScale", MiscSettings.GetUIScale(DisplayOperationMode.Current), 0.1f, 1.4f, 1f, 0.1f, delegate (float value)
			{
				MiscSettings.SetUIScale(value, DisplayOperationMode.Current);
			}, SliderLabelMode.Float, "0.0", "Sets the UI Scale");

			optionsPanel.AddSliderOption(tabIndex, "Seaglide Y Offset", CameraFixes.seaglideYOffset, 0.01f, .50f, -0.18f, 0.01f, delegate (float value)
			{
				CameraFixes.seaglideYOffset = value;
			}, SliderLabelMode.Float, "0.00", "Higher Values Lowers Seaglide | Lower Values Highers Seaglide");

			optionsPanel.AddHeading(tabIndex, ColorString("Dynamic Hud Options", new Color(255, 140, 0)));
			optionsPanel.AddToggleOption(tabIndex, "Dynamic HUD", dynamicHUD, delegate (bool v)
			{
				dynamicHUD = v;
				UIElementsFixes.SetDynamicHUD(v);

			},"Hides Oxygen/Health/Food/Water bars while above 50%");
			optionsPanel.AddToggleOption(tabIndex, "Dynamic HUD Hide Pinned Reciepes with Quickslots (Only Works When Dynamic Hud Is Enabled)", pinnedReciepehud, delegate (bool v)
			{
				pinnedReciepehud = v;
				UIElementsFixes.SetDynamicHUDReciepe(v);
			}, "Syncs Reciepes with quickslots fade");

			optionsPanel.AddHeading(tabIndex, ColorString("Hud Adjustments", new Color(255, 140, 0)));
			optionsPanel.AddSliderOption(tabIndex, "HUD Opacity", HUD_Alpha * 100f, 40, 100, 70, 1f, delegate (float v)
			{
				HUD_Alpha = v / 100f;
				UIElementsFixes.UpdateHUDOpacity(HUD_Alpha);
			}, SliderLabelMode.Float, "000", "Higher Values is Less Transparent | Lower Values is More Transparent");

			optionsPanel.AddSliderOption(tabIndex, "HUD Distance", HUD_Distance / 0.5f, 2, 4, 3, 0.1f, delegate (float v)
			{
				HUD_Distance = v * 0.5f;
				UIElementsFixes.UpdateHUDDistance(HUD_Distance);
			}, SliderLabelMode.Float, "00.0", "Higher Values Makes Hud Further | Lower Values Makes Hud Closer");

			optionsPanel.AddSliderOption(tabIndex, "HUD Scale", HUD_Scale / 0.5f, 0.01f, 4, 0.7f, 0.01f, delegate (float v)
			{
				HUD_Scale = v * 0.5f;
				UIElementsFixes.UpdateHUDScale(HUD_Scale);
			}, SliderLabelMode.Float, "00.00", "Higher Values Increase Scale of Hud | Lower Values Decrease Scale of Hud");

			optionsPanel.AddChoiceOption(tabIndex, "HUD Separation", new string[] { "Default" ,"Small", "Medium", "Large" }, HUD_Separation, delegate (int separation)
			{
				HUD_Separation = separation;
				UIElementsFixes.UpdateHUDSeparation(separation);
			},"Hud Sepration Size");

			optionsPanel.AddHeading(tabIndex, ColorString("Misc VR Options", new Color(255, 140, 0)));
			optionsPanel.AddToggleOption(tabIndex, "Disable Y-Axis Input", VROptions.disableInputPitch, (bool v) => VROptions.disableInputPitch = v,"Disable Up And Down With Mouse/Controller");
			optionsPanel.AddSliderOption(tabIndex, "Ground Speed Scale", VROptions.groundMoveScale, 0.05f,1.0f,0.60f,0.01f, (float v) =>
			{
				if (v <= 0f)
				{
					// Never allow 0 scale speed (immobile)
					v = .05f;
				}
				VROptions.groundMoveScale = v;

			},SliderLabelMode.Float,"0.00","Sets your Movment speed while walking");
			optionsPanel.AddToggleOption(tabIndex, "Aim Right Arm With Head", VROptions.aimRightArmWithHead, (bool v) => VROptions.aimRightArmWithHead = v,"Aim tools with Head");

			optionsPanel.AddHeading(tabIndex, ColorString("VR Snap Turning", new Color(255, 140, 0)));//add new heading under the General Tab
			optionsPanel.AddToggleOption(tabIndex, "Enable Snap Turning", SnapTurningOptions.EnableSnapTurning, (bool v) => SnapTurningOptions.EnableSnapTurning = v,"Enable Snap Turning");
			optionsPanel.AddChoiceOption(tabIndex, "Snap Angle", SnapTurningOptions.SnapAngleChoices, SnapTurningOptions.SnapAngleChoiceIndex, (int index) => SnapTurningOptions.SnapAngleChoiceIndex = index,"Snap Angle Degrees");
			optionsPanel.AddBindingOption(tabIndex, "Keyboard Turn Left", GameInput.Device.Keyboard, GameInput.Button.LookLeft);
			optionsPanel.AddBindingOption(tabIndex, "Keyboard Turn Right", GameInput.Device.Keyboard, GameInput.Button.LookRight);
			optionsPanel.AddToggleOption(tabIndex, "Disable Mouse Look", SnapTurningOptions.DisableMouseLook, (bool v) => SnapTurningOptions.DisableMouseLook = v,"Disables Mouse Look");
		}
	}
}
