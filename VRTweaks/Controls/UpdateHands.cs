using UnityEngine;
using UnityEngine.XR;
using VRTweaks.Controls;

public class UpdateHand : MonoBehaviour
{
    void Awake()
    {
        ErrorMessage.AddDebug("It Awake");
    }
    public static void Start()
    {
        ErrorMessage.AddDebug("It Started");
    }
    void FixedUpdate()
    {
        ErrorMessage.AddDebug("FixedUpdate");
        if (!XRSettings.enabled || MotionControlConfig.EnableMotionControls == false)
        {
            return;
        }
        if (!VRHandsController.player.cinematicModeActive)
        {
            VRHandsController.main.UpdateHandPositions();
        }
    }
}