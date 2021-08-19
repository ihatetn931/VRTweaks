using HarmonyLib;
using VRTweaks.GamepadGazeCursorFixer.Utilities;

namespace VRTweaks.GamepadGazeCursorFixer
{
    [HarmonyPatch(typeof(uGUI_BuilderMenu), "UpdateRaycasterStatus")]
    public static class BuilderMenuRaycaster
    {
        public static bool Prefix(ref uGUI_GraphicRaycaster raycaster, uGUI_BuilderMenu __instance)
        {
            RaycastFixer.Fix(ref raycaster, __instance);
            return false;
        }
    }
}

