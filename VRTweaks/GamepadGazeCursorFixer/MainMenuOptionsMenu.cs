using HarmonyLib;

namespace VRTweaks.GamepadGazeCursorFixer
{
    [HarmonyPatch(typeof(MainMenuOptions), nameof(MainMenuOptions.Update))]
    public static class MainMenuOptionsMenu
    {
        public static void Postfix(MainMenuOptions __instance)
        {
            if (__instance.tabbedPanel != null && GameInput.GetButtonDown(uGUI.button1))
            {
                if (__instance.tabbedPanel.dialog.open)
                {
                    __instance.tabbedPanel.dialog.Close();
                    return;
                }
                __instance.OnBack();
            }
        }
    }
}
