using HarmonyLib;

namespace VRTweaks.GamepadGazeCursorFixer.Utilities
{
    [HarmonyPatch(typeof(GameInput), nameof(GameInput.GetPrimaryDevice))]
    public static class KeyboardInputSpoofer
    {
        public static bool ShouldSpoofKeyboard { get; set; }

        public static bool Prefix(ref GameInput.Device __result)
        {
            if(ShouldSpoofKeyboard)
            {
                __result = GameInput.Device.Keyboard;
                return false;
            }

            return true;
        }
    }
}
