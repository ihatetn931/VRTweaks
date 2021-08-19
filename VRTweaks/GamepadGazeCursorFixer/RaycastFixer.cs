namespace VRTweaks.GamepadGazeCursorFixer.Utilities
{
    public static class RaycastFixer
    {
        public static void Fix(ref uGUI_GraphicRaycaster raycaster, uGUI_InputGroup __instance)
        {
            if (GameInput.IsPrimaryDeviceGamepad() && !VROptions.GetUseGazeBasedCursor())
            {
                raycaster.enabled = false;
            }
            else
            {
                raycaster.enabled = __instance.focused;
            }
        }
    }
}