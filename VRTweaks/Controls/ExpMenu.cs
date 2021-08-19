﻿using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace VRTweaks.Controls
{
    public static class MotionControlConfig
    {
        public static bool EnableMotionControls;
    }

    class MotionControlMenu
    {
        public static uGUI_OptionsPanel panel;
        public static uGUI_Bindings binding;
        public static Toggle AimWithHeadSet;

        [HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddTabs))]
        class uGUI_OptionsPanel_AddTabs_Patch
        {
            static bool Prefix(uGUI_OptionsPanel __instance)
            {
                __instance.AddGeneralTab();
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
                if (!PlatformUtils.isConsolePlatform)
                {
                    __instance.AddTroubleshootingTab();
                }
                if (__instance != null)
                {
                    AddMotionTab(__instance);
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(uGUI_Bindings), nameof(uGUI_Bindings.Initialize))]
        class uGUI_Bindings_Initializes_Patch
        {
            static bool Prefix(GameInput.Device device, KeyCode button, Action<GameInput.Device, KeyCode, GameInput.BindingSet, string> bindCallback, uGUI_Bindings __instance)
            {
                binding = __instance;
                return true;
            }
        }

        [HarmonyPatch(typeof(GameSettings), nameof(GameSettings.SerializeSettings))]
        class GameSettings_SerializeSettings_Patch
        {
            static void Postfix(GameSettings.ISerializer serializer)
            {
                MotionControlConfig.EnableMotionControls = serializer.Serialize("Controls/ToggleMotionControls", MotionControlConfig.EnableMotionControls);
                VROptions.aimRightArmWithHead = serializer.Serialize("Controls/ToggleAimWtihHeadset", VROptions.aimRightArmWithHead);
            }
        }

        public static void AddMotionTab(uGUI_OptionsPanel oPanel)
        {
            if (oPanel != null)
            {
                panel = oPanel;
                int tabIndex = oPanel.AddTab("MotionControls");
                oPanel.AddHeading(tabIndex, "Motion Control Settings");
                oPanel.AddToggleOption(tabIndex, "Toggle Motions Controls", MotionControlConfig.EnableMotionControls, (bool v) => MotionControlConfig.EnableMotionControls = ToggleMotionsControls(v), "Toggle Motion Controls");
                AimWithHeadSet = oPanel.AddToggleOption(tabIndex, "Toggle Aim With Right Vr Controller ", VROptions.aimRightArmWithHead, (bool v) => VROptions.aimRightArmWithHead = v, "Toggles Aim With Headset");
                ToggleMotionsControls(MotionControlConfig.EnableMotionControls);
            }
        }

        public static bool ToggleMotionsControls(bool value)
        {
            AimWithHeadSet.gameObject.SetActive(value);
            return value;
        }
    }
}


