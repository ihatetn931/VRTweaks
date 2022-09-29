﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using HarmonyLib;
using System;
using Platform.Utils;
using FMODUnity;
using UnityEngine.Events;

namespace VRTweaks
{
    enum Controller
    {
        Left,
        Right
    }

    internal class XRInputManager
    {
        private static readonly XRInputManager _instance = new XRInputManager();
        private readonly List<InputDevice> xrDevices = new List<InputDevice>();
        public InputDevice leftController;
        public InputDevice rightController;


        private XRInputManager()
        {
            GetDevices();
        }

        public static XRInputManager GetXRInputManager()
        {
            return _instance;
        }

        void GetDevices()
        {
            InputDevices.GetDevices(xrDevices);

            foreach (InputDevice device in xrDevices)
            {
                if (device.name.Contains("Left"))
                {
                    leftController = device;
                }
                if (device.name.Contains("Right"))
                {
                    rightController = device;
                }
            }
        }

        InputDevice GetDevice(Controller name)
        {
            switch (name)
            {
                case Controller.Left:
                    return leftController;
                case Controller.Right:
                    return rightController;
                default: throw new Exception();
            }
        }

        public Vector2 Get(Controller controller, InputFeatureUsage<Vector2> usage)
        {
            InputDevice device = GetDevice(controller);
            Vector2 value = Vector2.zero;
            if (device != null && device.isValid)
            {
                device.TryGetFeatureValue(usage, out value);
            }
            else
            {
                GetDevices();
            }
            return value;
        }

        public Vector3 Get(Controller controller, InputFeatureUsage<Vector3> usage)
        {
            InputDevice device = GetDevice(controller);
            Vector3 value = Vector3.zero;
            if (device != null && device.isValid)
            {
                device.TryGetFeatureValue(usage, out value);
            }
            else
            {
                GetDevices();
            }
            return value;
        }

        public Quaternion Get(Controller controller, InputFeatureUsage<Quaternion> usage)
        {
            InputDevice device = GetDevice(controller);
            Quaternion value = Quaternion.identity;
            if (device != null && device.isValid)
            {
                device.TryGetFeatureValue(usage, out value);
            }
            else
            {
                GetDevices();
            }
            return value;
        }

        public float Get(Controller controller, InputFeatureUsage<float> usage)
        {
            InputDevice device = GetDevice(controller);
            float value = 0f;
            if (device != null && device.isValid)
            {
                device.TryGetFeatureValue(usage, out value);
            }
            else
            {
                GetDevices();
            }
            return value;
        }

        public bool Get(Controller controller, InputFeatureUsage<bool> usage)
        {
            InputDevice device = GetDevice(controller);
            bool value = false;
            if (device != null && device.isValid)
            {
                device.TryGetFeatureValue(usage, out value);
            }
            else
            {
                GetDevices();
            }
            return value;
        }

        public bool hasControllers()
        {
            bool hasController = false;
            if (leftController != null && leftController.isValid)
            {
                hasController = true;
            }
            if (rightController != null && rightController.isValid)
            {
                hasController = true;
            }
            return hasController;
        }

        [HarmonyPatch(typeof(GameInput), "UpdateAxisValues")]
        internal class UpdateAxisValuesPatch
        {
            public static bool Prefix(bool useKeyboard, bool useController, GameInput ___instance)
            {
                XRInputManager xrInput = GetXRInputManager();

                for (int i = 0; i < GameInput.axisValues.Length; i++)
                {
                    GameInput.axisValues[i] = 0f;
                }

                if (useController)
                {
                    if (GameInput.GetUseOculusInputManager() && XRSettings.loadedDeviceName != "Oculus")
                    {
                        Vector2 vector = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick, OVRInput.Controller.Active);
                        GameInput.axisValues[2] = vector.x;
                        GameInput.axisValues[3] = -vector.y;
                        Vector2 vector2 = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick, OVRInput.Controller.Active);
                        GameInput.axisValues[0] = vector2.x;
                        GameInput.axisValues[1] = -vector2.y;
                        GameInput.axisValues[4] = OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger, OVRInput.Controller.Active);
                        GameInput.axisValues[5] = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger, OVRInput.Controller.Active);
                        GameInput.axisValues[6] = 0f;
                        if (OVRInput.Get(OVRInput.RawButton.DpadLeft, OVRInput.Controller.Active))
                        {
                            GameInput.axisValues[6] -= 1f;
                        }
                        if (OVRInput.Get(OVRInput.RawButton.DpadRight, OVRInput.Controller.Active))
                        {
                            GameInput.axisValues[6] += 1f;
                        }
                        GameInput.axisValues[7] = 0f;
                        if (OVRInput.Get(OVRInput.RawButton.DpadUp, OVRInput.Controller.Active))
                        {
                            GameInput.axisValues[7] += 1f;
                        }
                        if (OVRInput.Get(OVRInput.RawButton.DpadDown, OVRInput.Controller.Active))
                        {
                            GameInput.axisValues[7] -= 1f;
                        }
                    }
                    else
                    {
                        GameInput.ControllerLayout controllerLayout = GameInput.GetControllerLayout();
                        if (controllerLayout == GameInput.ControllerLayout.Xbox360 || controllerLayout == GameInput.ControllerLayout.XboxOne || Application.platform == RuntimePlatform.PS4)
                        {
                            GameInput.axisValues[2] = Input.GetAxis("ControllerAxis1");
                            GameInput.axisValues[3] = Input.GetAxis("ControllerAxis2");
                            GameInput.axisValues[0] = Input.GetAxis("ControllerAxis4");
                            GameInput.axisValues[1] = Input.GetAxis("ControllerAxis5");
                            if (Application.platform == RuntimePlatform.PS4)
                            {
                                GameInput.axisValues[4] = Mathf.Max(Input.GetAxis("ControllerAxis8"), 0f);
                                GameInput.axisValues[5] = Mathf.Max(-Input.GetAxis("ControllerAxis3"), 0f);
                            }
                            else if (Application.platform == RuntimePlatform.XboxOne)
                            {
                                GameInput.axisValues[4] = Mathf.Max(Input.GetAxis("ControllerAxis3"), 0f);
                                GameInput.axisValues[5] = Mathf.Max(-Input.GetAxis("ControllerAxis3"), 0f);
                            }
                            else
                            {
                                GameInput.axisValues[4] = Mathf.Max(-Input.GetAxis("ControllerAxis3"), 0f);
                                GameInput.axisValues[5] = Mathf.Max(Input.GetAxis("ControllerAxis3"), 0f);
                            }
                            GameInput.axisValues[6] = Input.GetAxis("ControllerAxis6");
                            GameInput.axisValues[7] = Input.GetAxis("ControllerAxis7");
                        }
                        else if (controllerLayout == GameInput.ControllerLayout.Switch)
                        {
                            GameInput.axisValues[2] = InputUtils.GetAxis("ControllerAxis1");
                            GameInput.axisValues[3] = InputUtils.GetAxis("ControllerAxis2");
                            GameInput.axisValues[0] = InputUtils.GetAxis("ControllerAxis4");
                            GameInput.axisValues[1] = InputUtils.GetAxis("ControllerAxis5");
                            GameInput.axisValues[4] = Mathf.Max(InputUtils.GetAxis("ControllerAxis3"), 0f);
                            GameInput.axisValues[5] = Mathf.Max(-InputUtils.GetAxis("ControllerAxis3"), 0f);
                            GameInput.axisValues[6] = InputUtils.GetAxis("ControllerAxis6");
                            GameInput.axisValues[7] = InputUtils.GetAxis("ControllerAxis7");
                        }
                        else if (controllerLayout == GameInput.ControllerLayout.PS4)
                        {
                            GameInput.axisValues[2] = Input.GetAxis("ControllerAxis1");
                            GameInput.axisValues[3] = Input.GetAxis("ControllerAxis2");
                            GameInput.axisValues[0] = Input.GetAxis("ControllerAxis3");
                            GameInput.axisValues[1] = Input.GetAxis("ControllerAxis6");
                            GameInput.axisValues[4] = (Input.GetAxis("ControllerAxis4") + 1f) * 0.5f;
                            GameInput.axisValues[5] = (Input.GetAxis("ControllerAxis5") + 1f) * 0.5f;
                            GameInput.axisValues[6] = Input.GetAxis("ControllerAxis7");
                            GameInput.axisValues[7] = Input.GetAxis("ControllerAxis8");
                        }
                    }

                    if (xrInput.hasControllers())
                    {
                        if (XRSettings.loadedDeviceName == "Oculus")
                        {
                            Vector2 vector = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick, OVRInput.Controller.Active);
                            GameInput.axisValues[2] = vector.x;
                            GameInput.axisValues[3] = -vector.y;
                            Vector2 vector2 = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick, OVRInput.Controller.Active);
                            GameInput.axisValues[0] = vector2.x;
                            GameInput.axisValues[1] = -vector2.y;
                            // TODO: Use deadzone?
                            GameInput.axisValues[4] = OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger, OVRInput.Controller.Active);
                            GameInput.axisValues[5] = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger, OVRInput.Controller.Active);
                            if (OVRInput.Get(OVRInput.RawButton.DpadLeft, OVRInput.Controller.Active))
                            {
                                GameInput.axisValues[6] -= 1f;
                            }
                            if (OVRInput.Get(OVRInput.RawButton.DpadRight, OVRInput.Controller.Active))
                            {
                                GameInput.axisValues[6] += 1f;
                            }
                            GameInput.axisValues[7] = 0f;
                            if (OVRInput.Get(OVRInput.RawButton.DpadUp, OVRInput.Controller.Active))
                            {
                                GameInput.axisValues[7] += 1f;
                            }
                            if (OVRInput.Get(OVRInput.RawButton.DpadDown, OVRInput.Controller.Active))
                            {
                                GameInput.axisValues[7] -= 1f;
                            }
                        }
                        bool test = false;
                        //OpenVR Asix values
                        if (XRSettings.loadedDeviceName == "OpenVR" && test)
                        {
                            Vector2 vector = xrInput.Get(Controller.Left, CommonUsages.primary2DAxis);
                            GameInput.axisValues[2] = vector.x;
                            GameInput.axisValues[3] = -vector.y;
                            Vector2 vector2 = xrInput.Get(Controller.Right, CommonUsages.primary2DAxis);
                            GameInput.axisValues[0] = vector2.x;
                            GameInput.axisValues[1] = -vector2.y;
                            // TODO: Use deadzone?
                            GameInput.axisValues[4] = xrInput.Get(Controller.Left, CommonUsages.trigger).CompareTo(0.3f);
                            GameInput.axisValues[5] = xrInput.Get(Controller.Right, CommonUsages.trigger).CompareTo(0.3f);

                            //These axis I'm sure are used for something on other headsets
                            //axisValues[6] = xrInput.Get(Controller.Left, CommonUsages.secondary2DAxisTouch).CompareTo(0.1f);
                            //axisValues[7] = xrInput.Get(Controller.Right, CommonUsages.secondaryTouch).CompareTo(0.1f);
                        }
                    }
                }

                if (useKeyboard)
                {
                    GameInput.axisValues[10] = Input.GetAxis("Mouse ScrollWheel");
                    GameInput.axisValues[8] = Input.GetAxisRaw("Mouse X");
                    GameInput.axisValues[9] = Input.GetAxisRaw("Mouse Y");
                }
                for (int j = 0; j < GameInput.axisValues.Length; j++)
                {
                    GameInput.AnalogAxis axis = (GameInput.AnalogAxis)j;
                    GameInput.Device deviceForAxis = ___instance.GetDeviceForAxis(axis);
                    float f = GameInput.lastAxisValues[j] - GameInput.axisValues[j];
                    GameInput.lastAxisValues[j] = GameInput.axisValues[j];
                    if (deviceForAxis != GameInput.lastDevice)
                    {
                        float num = 0.1f;
                        if (Mathf.Abs(f) > num)
                        {
                            if (!PlatformUtils.isConsolePlatform)
                            {
                                GameInput.lastDevice = deviceForAxis;
                            }
                        }
                        else
                        {
                            GameInput.axisValues[j] = 0f;
                        }
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(FPSInputModule), nameof(FPSInputModule.EscapeMenu))]
        class FPSInputModule_EscapeMenu_Patch
        {
            //GameInput.GetButtonHeldTime(GameInput.Button.PDA) > 1.0f
            [HarmonyPrefix]
            static void Prefix(FPSInputModule __instance)
            {
                if (__instance.lockPauseMenu)
                {
                    return;
                }
                if (GameInput.GetButtonDown(GameInput.Button.UIMenu) && IngameMenu.main != null && !IngameMenu.main.selected || GameInput.GetButtonHeldTime(GameInput.Button.PDA) > 1.0f)
                {
                    IngameMenu.main.Open();
                    GameInput.ClearInput();
                }
            }
        }
    }
}

