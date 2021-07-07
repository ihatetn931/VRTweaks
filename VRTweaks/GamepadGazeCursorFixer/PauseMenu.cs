using HarmonyLib;
using VRTweaks.GamepadGazeCursorFixer.Utilities;

namespace VRTweaks.GamepadGazeCursorFixer
{
    [HarmonyPatch(typeof(IngameMenu), "UpdateRaycasterStatus")]
    public static class PauseMenuRaycaster
    {
        public static bool Prefix(ref uGUI_GraphicRaycaster raycaster, IngameMenu __instance)
        {
            RaycastFixer.Fix(ref raycaster, __instance);
            return false;
        }
    }
}