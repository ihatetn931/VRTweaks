using HarmonyLib;
using RootMotion;
using RootMotion.FinalIK;
using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
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
        public void Initialize(ArmsController controller)
        {
            player = global::Utils.GetLocalPlayerComp();

            if (MotionControlConfig.EnableMotionControls)
            {
                Time.fixedDeltaTime = (Time.timeScale / XRDevice.refreshRate);

                ik = controller.GetComponent<FullBodyBipedIK>();
                armsController = controller;

                rightController = new GameObject("rightController");
                leftController = new GameObject("leftController");

                if(line == null)
                    rightController.AddComponent<LaserPointer>();
                line = rightController.GetComponent<LaserPointer>();
            }
            rightController.transform.parent = player.camRoot.transform;
            leftController.transform.parent = player.camRoot.transform;
            line.transform.SetParent(player.camRoot.transform);
            IKSolverFullBodyBiped solver = ik.solver;
            solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate, new IKSolver.UpdateDelegate(RotateShoulders));
        }

        public static bool skip;
        public static float weight = 2.5f;
        public static float offset = 1.2f;
        public static void RotateShoulders()
        {
            if (ik == null)
            {
                return;
            }
            if (ik.solver.IKPositionWeight <= 0f)
            {
                return;
            }
            if (skip)
            {
                skip = false;
                return;
            }

            RotateShoulder(FullBodyBipedChain.LeftArm, weight, offset);
            RotateShoulder(FullBodyBipedChain.RightArm, weight, offset);
            skip = true;
            ik.solver.Update();
        }
        public static IKMapping.BoneMap GetParentBoneMap(FullBodyBipedChain chain)
        {
            return ik.solver.GetLimbMapping(chain).GetBoneMap(IKMappingLimb.BoneMapType.Parent);
        }
        private static void RotateShoulder(FullBodyBipedChain chain, float weight, float offset)
        {

            Quaternion b = Quaternion.FromToRotation(GetParentBoneMap(chain).swingDirection, ik.solver.GetEndEffector(chain).position - GetParentBoneMap(chain).transform.position);
            Vector3 vector = ik.solver.GetEndEffector(chain).position - ik.solver.GetLimbMapping(chain).bone1.position;
            float num = ik.solver.GetChain(chain).nodes[0].length + ik.solver.GetChain(chain).nodes[1].length;
            float num2 = vector.magnitude / num - 1f + offset;
            num2 = Mathf.Clamp(num2 * weight, 0f, 1f);
            Quaternion lhs = Quaternion.Lerp(Quaternion.identity, b, num2 * ik.solver.GetEndEffector(chain).positionWeight * ik.solver.IKPositionWeight);
            ik.solver.GetLimbMapping(chain).parentBone.rotation = lhs * ik.solver.GetLimbMapping(chain).parentBone.rotation;
        }

        public void UpdateHandPositions()
        {
            if (!VROptions.gazeBasedCursor)
            {
                VROptions.gazeBasedCursor = true;
            }
            if (!uGUI.main.loading.IsLoading || uGUI.main)
            {
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
                    if (heldTool != null)
                    {
                        if (heldTool.item != null)
                        {
                            //  ErrorMessage.AddDebug("IKTransform" + ik.solver.rightHandEffector.target.transform);
                            // ErrorMessage.AddDebug("RightController Transform" + rightController.transform);
                            // if (ik.solver.rightHandEffector.target.transform != null && rightController.transform != null)
                            // {
                            // ik.solver.rightHandEffector.target.transform.position = rightController.transform.position;
                            //ik.solver.rightHandEffector.target.gameObject.transform.rotation = rightController.transform.rotation *Quaternion.Euler(0f, 190f, 270f);
                            //ik.solver.rightHandEffector.target.rotation = rightRot * Quaternion.Euler(0f, 190f, 270f);
                            //  }
                        }
                    }
                    if (player.pda.gameObject.activeSelf)
                    {
                        if (ik.solver.leftHandEffector.target.transform != null && leftController.transform != null)
                        {
                            ik.solver.leftHandEffector.target.gameObject.transform.position = leftController.transform.position;
                            ik.solver.leftHandEffector.target.gameObject.transform.rotation = leftController.transform.rotation;
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
                if (!XRSettings.enabled || !MotionControlConfig.EnableMotionControls)
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
                if (!XRSettings.enabled || !MotionControlConfig.EnableMotionControls)
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




