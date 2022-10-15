using HarmonyLib;
using UnityEngine;


namespace VRTweaks
{
    [HarmonyPatch(typeof(PlayerBreathBubbles), "Start")]
    public static class PlayerBreathBubbles_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerBreathBubbles __instance)
        {
            //I Need to get this right
            //Place the bubbles right at about neck level but does not rotate with view
            __instance.anchor.position = new Vector3(0.0f, 1.6f, 0.0f);
            return true;
        }
    }
}