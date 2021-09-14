using HarmonyLib;
using VRTweaks.GamepadGazeCursorFixer.Utilities;

namespace VRTweaks.GamepadGazeCursorFixer
{
    [HarmonyPatch(typeof(uGUI_PDA), "UpdateRaycasterStatus")]
    public static class PDAMenuRaycaster
    {
        public static bool Prefix(ref uGUI_GraphicRaycaster raycaster, uGUI_PDA __instance)
        {
            RaycastFixer.Fix(ref raycaster, __instance);
            return false;
        }
    }


    static class PDAQuickSlotAssignment
    {
        private static bool _didDisableGazeCursor;

        [HarmonyPatch(typeof(uGUI_InventoryTab), nameof(uGUI_InventoryTab.OnUpdate))]
        public static class PDAQuickSlotAssignmentBegin
        {
            public static void Postfix()
            {
                if (VROptions.GetUseGazeBasedCursor() && GameInput.GetButtonDown(uGUI.button3))
                {
                    Inventory.main.ExecuteItemAction(ItemDragManager.hoveredItem, 3);
                    VROptions.gazeBasedCursor = false;
                    _didDisableGazeCursor = true;
                }
            }
        }


        [HarmonyPatch(typeof(QuickSlots), nameof(QuickSlots.EndAssign))]
        public static class PDAQuickSlotAssignmentEnd
        {
            public static void Postfix()
            {
                if (_didDisableGazeCursor)
                {
                    VROptions.gazeBasedCursor = true;
                }
            }
        }

    }
}
