using HarmonyLib;
using VRTweaks.GamepadGazeCursorFixer.Utilities;

namespace VRTweaks.GamepadGazeCursorFixer
{
    [HarmonyPatch(typeof(uGUI_CraftingMenu), "SetRaycasterStatus")]
    public static class CraftingMenuRaycaster
    {
        public static bool Prefix(ref uGUI_GraphicRaycaster raycaster, uGUI_CraftingMenu __instance)
        {
            RaycastFixer.Fix(ref raycaster, __instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(uGUI_CraftingMenu), "uGUI_IIconManager.OnPointerClick")]
    public static class CraftingMenuOnPointerClick
    {
        private static bool _didSpoofKeyboard;

        public static bool Prefix()
        {
            if (VROptions.GetUseGazeBasedCursor())
            {
                KeyboardInputSpoofer.ShouldSpoofKeyboard = true;
                _didSpoofKeyboard = true;
            }

            return true;
        }

        public static void Postfix()
        {
            if (_didSpoofKeyboard)
            {
                KeyboardInputSpoofer.ShouldSpoofKeyboard = false;
            }
        }
    }

}
