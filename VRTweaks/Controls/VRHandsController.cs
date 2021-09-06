using HarmonyLib;
using RootMotion;
using RootMotion.FinalIK;
using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

namespace VRTweaks.Controls
{
    public class VRHandsController : MonoBehaviour
    {
        public static GameObject rightController;
        public static GameObject leftController;
        public static ArmsController armsController;
        public static Player player = Player.main;
        public static FullBodyBipedIK ik;
        public static PDA pda;
        private static VRHandsController _main;
        public static bool inMenu = false;
        public static LaserPointer line;

        public static VRHandsController main
        {
            get
            {
                if (_main == null)
                {
                    _main = new VRHandsController();
                }
                return _main;
            }
        }

        public void InitializeMenu()
        {
            if (MotionControlConfig.EnableMotionControls)
            {
                if (!VROptions.gazeBasedCursor)
                {
                    VROptions.gazeBasedCursor = true;
                }

                Time.fixedDeltaTime = (Time.timeScale / XRDevice.refreshRate);

                rightController = new GameObject("rightController");
               // rightController.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                leftController = new GameObject("leftController");
                //leftController.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

                if (line == null)
                    rightController.AddComponent<LaserPointer>();
                line = rightController.GetComponent<LaserPointer>();
            }

            var cam = MainCamera.camera.transform;
            rightController.transform.SetParent(cam, false);
            leftController.transform.SetParent(cam, false);
            line.transform.SetParent(cam, false);

        }

        public void UpdateMenuPositions()
        {
            if (!VROptions.gazeBasedCursor)
            {
                VROptions.gazeBasedCursor = true;
            }

            var cam = MainCamera.camera.transform;
            rightController.transform.SetParent(cam, false);
            leftController.transform.SetParent(cam,false);
            line.transform.SetParent(cam, false);

            XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPos);
            XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rightRot);

            rightController.transform.localPosition = rightPos;// + new Vector3(0f, 0f, 0f);
            rightController.transform.localRotation = rightRot;// * Quaternion.Euler(0f, 190f, 270f);

            //Get left controller Position and Rotation
            XRInputManager.GetXRInputManager().leftController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPos);
            XRInputManager.GetXRInputManager().leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion leftRot);

            leftController.transform.localPosition = leftPos;// + new Vector3(0f, -0.13f, -0.14f);
            leftController.transform.localRotation = leftRot;// * Quaternion.Euler(270f, 90f, 0f);
        }

        public static bool Started = false;
        public void Initialize(ArmsController controller)
        {
            player = global::Utils.GetLocalPlayerComp();

            if (MotionControlConfig.EnableMotionControls)
            {
                if (!VROptions.gazeBasedCursor)
                {
                    VROptions.gazeBasedCursor = true;
                }
                Time.fixedDeltaTime = (Time.timeScale / XRDevice.refreshRate);

                ik = controller.GetComponent<FullBodyBipedIK>();
                armsController = controller;

                rightController = new GameObject("rightController");
                leftController = new GameObject("leftController");

                if (line == null)
                    rightController.AddComponent<LaserPointer>();
                line = rightController.GetComponent<LaserPointer>();
            }
            rightController.transform.parent = player.camRoot.transform;
            leftController.transform.parent = player.camRoot.transform;
            line.transform.SetParent(player.camRoot.transform);
            Started = true;
            
        }

        public void UpdateHandPositions()
        {
            if (!VROptions.gazeBasedCursor)
            {
                VROptions.gazeBasedCursor = true;
            }

            InventoryItem heldTool = Inventory.main.quickSlots.heldItem;

            XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPos);
            XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rightRot);

            rightController.transform.localPosition = rightPos;// + new Vector3(0f, 0f, 0f);
            rightController.transform.localRotation = rightRot;// * Quaternion.Euler(0f, 190f, 270f);

            //Get left controller Position and Rotation
            XRInputManager.GetXRInputManager().leftController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPos);
            XRInputManager.GetXRInputManager().leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion leftRot);

            leftController.transform.localPosition = leftPos + new Vector3(0f, -0.13f, -0.14f);
            leftController.transform.localRotation = leftRot * Quaternion.Euler(270f, 90f, 0f);

            if (player != null)
            {
                if (player.pda != null)
                {
                    if (player.pda.gameObject.activeSelf)
                    {
                        if (ik.solver != null)
                        {
                            if(ik.solver.leftHandEffector != null)
                            {
                                if (ik.solver.leftHandEffector.target != null)
                                {
                                    if (leftController.transform != null)
                                    {
                                        ik.solver.leftHandEffector.target.gameObject.transform.position = leftController.transform.position;
                                        ik.solver.leftHandEffector.target.gameObject.transform.rotation = leftController.transform.rotation;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ArmsController), nameof(ArmsController.Start))]
        class ArmsController_Start_Patch
        {
            [HarmonyPostfix]
            public static void PostFix(ArmsController __instance)
            {
                if (!XRSettings.enabled || !MotionControlConfig.EnableMotionControls || Player.main == null || Started == true)
                {
                    return;
                }
                main.Initialize(__instance);
            }
        }

        [HarmonyPatch(typeof(ArmsController), nameof(ArmsController.LateUpdate))]
        class ArmsController_Update_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(ArmsController __instance)
            {
                if (!XRSettings.enabled || !MotionControlConfig.EnableMotionControls || Player.main == null || Started == false)
                {
                    return;
                }
                if (!player.cinematicModeActive)
                {
                    main.UpdateHandPositions();
                }
            }
        }
    }

    [HarmonyPatch(typeof(ArmsController), nameof(ArmsController.Reconfigure))]
    class ArmsController_Reconfigure_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerTool tool, ArmsController __instance)
        {
            //__instance.ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).bendGoal = __instance.leftHandElbow;
            //__instance.ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).weight = 2f;

            //__instance.ik.solver.GetBendConstraint(FullBodyBipedChain.RightArm).bendGoal = rightHandElbow;
            //__instance.ik.solver.GetBendConstraint(FullBodyBipedChain.RightArm).weight = 2f;
            if (tool == null)
            {
                __instance.leftAim.shouldAim = false;
                __instance.rightAim.shouldAim = false;

                __instance.ik.solver.rightHandEffector.target = null;
                __instance.ik.solver.leftHandEffector.target = null;
                __instance.aimIKVerticalRotationLimit.ResetRotationLimit();
                if (!__instance.pda.isActiveAndEnabled)
                {
                    if (__instance.leftWorldTarget)
                    {
                        __instance.ik.solver.leftHandEffector.target = __instance.leftWorldTarget;
                        //__instance.ik.solver.leftHandEffector.target = __instance.leftWorldTarget;
                        //__instance.ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).bendGoal = __instance.defaultLeftArmBendGoal;
                    }
                    if (__instance.rightWorldTarget)
                    {
                        __instance.ik.solver.rightHandEffector.target = __instance.rightWorldTarget;
                        //__instance.ik.solver.rightHandEffector.target = __instance.rightWorldTarget;
                        //__instance.ik.solver.GetBendConstraint(FullBodyBipedChain.RightArm).bendGoal = defaultRightArmBendGoal;
                    }
                    __instance.reconfigureWorldTarget = false;
                }
                if (__instance.inspecting && __instance.inspectObject != null && !__instance.inspectObject.Equals(null))
                {
                    __instance.rightAim.shouldAim = __instance.inspectObject.IKAimRightArm;
                    __instance.leftAim.shouldAim = __instance.inspectObject.IKAimLeftArm;
                }
            }
            else
            {
                if (!__instance.IsSpikeyTrapAttached())
                {
                    __instance.leftAim.shouldAim = tool.ikAimLeftArm;
                    if (tool.useLeftAimTargetOnPlayer)
                    {
                        __instance.ik.solver.leftHandEffector.target = __instance.attachedLeftHandTarget;
                        //__instance.ik.solver.leftHandEffector.target = __instance.attachedLeftHandTarget;
                    }
                    else
                    {
                        __instance.ik.solver.leftHandEffector.target = tool.leftHandIKTarget;
                        //__instance.ik.solver.leftHandEffector.target = tool.leftHandIKTarget;
                    }
                }
                else
                {
                    __instance.leftAim.shouldAim = tool.ikAimRightArm;
                    // __instance.ik.solver.leftHandEffector.target = null;
                    __instance.ik.solver.leftHandEffector.target = null;
                }
                __instance.rightAim.shouldAim = tool.ikAimRightArm;
                //__instance.ik.solver.rightHandEffector.target = tool.rightHandIKTarget;
                __instance.ik.solver.rightHandEffector.target = tool.rightHandIKTarget;

                __instance.leftAim.shouldAim = tool.ikAimLeftArm;
                __instance.ik.solver.leftHandEffector.target = tool.leftHandIKTarget;
                //__instance.ik.solver.leftHandEffector.target = tool.leftHandIKTarget;

                __instance.aimIKVerticalRotationLimit.SetRotationLimit(-tool.GetLookDownAngleLimit(), 90f);
            }
            SeaMonkeyStealCinematic activeCinematic = SeaMonkeyStealCinematic.activeCinematic;
            if (activeCinematic != null)
            {
                __instance.leftAim.shouldAim = true;
                __instance.ik.solver.leftHandEffector.target = activeCinematic.playerLeftHandAttach;
                //__instance.ik.solver.leftHandEffector.target = activeCinematic.playerLeftHandAttach;
            }
            return false;
        }
    }
}



