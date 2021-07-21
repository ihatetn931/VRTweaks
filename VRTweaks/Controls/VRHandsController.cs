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
    public enum ControllerLayout
    {
        // Token: 0x04005E34 RID: 24116
        Automatic,
        // Token: 0x04005E35 RID: 24117
        Xbox360,
        // Token: 0x04005E36 RID: 24118
        XboxOne,
        // Token: 0x04005E37 RID: 24119
        PS4,
        // Token: 0x04005E38 RID: 24120
        Switch,
        // Token: 0x04006E2F RID: 28207
        OpenVR,
        Oculus
    }

    public class VRHandsController : MonoBehaviour
    {
        public static GameObject rightController;
        public static GameObject leftController;
        public static ArmsController armsController;
        public static Player player = Player.main;
        public static FullBodyBipedIK ik;
        public static Finger finger;
        public static PDA pda;
        private static VRHandsController _main;
        public static bool inMenu = false;
        public static Animator ani;
        public static AimIK aim;
        public static Vector3 rightControllerPos;
        public static Quaternion rightControllerRot;
        public static Vector3 targetPosRelativeToRight;
        public static Quaternion targetRotRelativeToRight;
        public static AnimatorStateInfo stateInfo;
        public static Quaternion RotationTest;
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
            if (MotionControlConfig.ToggleDebugControllerBoxes)
            {
                Time.fixedDeltaTime = (Time.timeScale / XRDevice.refreshRate);
                //  var update = controller.gameObject.AddComponent<UpdateHand>();
                //ErrorMessage.AddDebug("UpdateHand: " + update);
                Material newMaterial = new Material(Shader.Find("Sprites/Default"));
                rightController = new GameObject("rightController");
                ik = controller.GetComponent<FullBodyBipedIK>();
                armsController = controller;

                leftController = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leftController.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                leftController.GetComponent<Renderer>().material.color = Color.blue;

            }
            else
            {
                rightController = new GameObject("rightController");
                leftController = new GameObject("leftController");
            }

            rightController.transform.parent = player.camRoot.transform;
            leftController.transform.parent = player.camRoot.transform;
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
            InventoryItem heldTool = Inventory.main.quickSlots.heldItem;
            XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPos);
            XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rightRot);
            
            rightController.transform.localPosition = rightPos + new Vector3(0f, -0.13f, 0f);
            rightController.transform.localRotation = rightRot * Quaternion.Euler(0f, 190f, 270f);

            rightControllerPos = rightPos;
            rightControllerRot = rightRot;

            //Get left controller Position and Rotation
            XRInputManager.GetXRInputManager().leftController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPos);
            XRInputManager.GetXRInputManager().leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion leftRot);
            leftController.transform.localPosition = leftPos + new Vector3(0f, -0.13f, -0.14f) ;
            leftController.transform.localRotation  = leftRot * Quaternion.Euler(270f, 90f, 0f);

            ik.solver.rightHandEffector.target = rightController.transform;
            ik.solver.leftHandEffector.target = leftController.transform;
            LaserPointer.UpdatePointer(LaserPointer.DrawLine(rightController.transform.position, Camera.main.ScreenToWorldPoint(rightController.transform.position + rightController.transform.right)*10));


        }
        
        [HarmonyPatch(typeof(ArmsController), nameof(ArmsController.Start))]
        class ArmsController_Start_Patch
        {
            [HarmonyPostfix]
            public static void PostFix(ArmsController __instance)
            {
                if (!XRSettings.enabled || MotionControlConfig.EnableMotionControls == false)
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
                if (!XRSettings.enabled || MotionControlConfig.EnableMotionControls == false)
                {
                    return;
                }
                if (!player.cinematicModeActive)
                {
                    main.UpdateHandPositions();
                }
            }
        }

     /*   [HarmonyPatch(typeof(AimIKTarget), nameof(AimIKTarget.LateUpdate))]
        class AimIKTarget_LateUpdate_Patch
        {
            [HarmonyPrefix]
            public static bool PreFix(AimIKTarget __instance)
            {
                if (XRSettings.enabled && VROptions.aimRightArmWithHead)
                {
                    Transform aimingTransform = SNCameraRoot.main.GetAimingTransform();
                    __instance.transform.position = aimingTransform.position + aimingTransform.forward * 5f;
                    return false ;
                }
                __instance.transform.localPosition = __instance.origLocalPos;
                return false;
            }
        }*/
        /*     [HarmonyPatch(typeof(AimIKTarget), nameof(AimIKTarget.LateUpdate))]
             class FPSInputModule_UpdateCursor_Patch
             {
                 [HarmonyPrefix]
                 public static bool PreFix(AimIKTarget __instance)
                 {
                     if (XRSettings.enabled && VROptions.aimRightArmWithHead)
                     {
                         Transform aimingTransform = GetAimingTransform();
                         Vector3 vec = new Vector3(MainCamera.camera.ScreenToWorldPoint(rightControllerPos).x, MainCamera.camera.ScreenToWorldPoint(rightControllerPos).y, 10);
                         __instance.transform.position = vec;// + aimingTransform.right * 5f;

                         return false;
                     }
                     __instance.transform.localPosition = __instance.origLocalPos;
                     return false;
                 }
                 public static Transform GetAimingTransform()
                 {
                     return rightController.transform;
                 }
             }
             public static Transform GetAimingTransform()
             {
                 return rightController.transform;
             }*/
        /* [HarmonyPatch(typeof(ArmsController), nameof(ArmsController.Start))]
         public static class ArmsController_Start_PrefixPatch
         {
             [HarmonyPrefix]
             public static void Prefix(ArmsController __instance)
             {
                 Initialize(__instance);
                 __instance.animator = __instance.gameObject.GetComponent<Animator>();
                 __instance.player = global::Utils.GetLocalPlayerComp();
                 __instance.guiHand = __instance.player.guiHand;
                 __instance.InstallAnimationRules();
                 __instance.leftAim.FindAimer(__instance.gameObject, __instance.leftAimIKTransform);
                 __instance.rightAim.FindAimer(__instance.gameObject, __instance.rightAimIKTransform);
                 //vrIK = __instance.GetComponent<VRIK>();
                 __instance.lookTargetResetPos = __instance.lookTargetTransform.transform.localPosition;
                 __instance.pda = __instance.player.GetPDA();
             }
         }*/
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
    /* [HarmonyPatch(typeof(ArmsController), nameof(ArmsController.UpdateHandIKWeights))]
     class ArmsController_UpdateHandIKWeights_Patch
     {
         [HarmonyPrefix]
         public static bool Prefix(ArmsController __instance)
         {
             VRIK vrik = __instance.GetComponent<VRIK>();
             ErrorMessage.AddDebug("UpdateHandIKWeightsVRIK: " + vrik);
             float num = vrik.solver.rightArm.positionWeight;// this.ik.solver.rightHandEffector.positionWeight;
             float num2 = (vrik.solver.rightArm.target != null) ? 1f : 0f;
             float num3 = vrik.solver.leftArm.positionWeight;//this.ik.solver.leftHandEffector.positionWeight;
             float num4 = (vrik.solver.leftArm.target != null) ? 1f : 0f;
             if (__instance.ikToggleTime == 0f)
             {
                 num = num2;
                 num3 = num4;
             }
             else
             {
                 num = Mathf.MoveTowards(num, num2, Time.deltaTime / __instance.ikToggleTime);
                 num3 = Mathf.MoveTowards(num3, num4, Time.deltaTime / __instance.ikToggleTime);
             }
             //this.ik.solver.rightHandEffector.positionWeight = num;
             vrik.solver.rightArm.positionWeight = num;
             vrik.solver.rightArm.positionWeight = num;
            // this.ik.solver.rightHandEffector.rotationWeight = num;
           //  this.ik.solver.leftHandEffector.positionWeight = num3;
             vrik.solver.leftArm.positionWeight = num3;
             vrik.solver.leftArm.positionWeight = num3;
           //  this.ik.solver.leftHandEffector.rotationWeight = num3;
             if (num3 > 0f || num > 0f)
             {
                 if (__instance.timeTransitionStarted < 0f)
                 {
                    vrik.solver.IKPositionWeight = 1f;
                    // this.ik.solver.IKPositionWeight = 1f;
                      return false;
                 }
                 float num5 = Mathf.Clamp01((Time.time - __instance.timeTransitionStarted) / __instance.transitionTime);
                 vrik.solver.IKPositionWeight = num5;
                // this.ik.solver.IKPositionWeight = num5;
                 if (num5 >= 1f)
                 {
                     __instance.transitionTime = -1f;
                     __instance.timeTransitionStarted = -1f;
                     return false;
                 }
             }
             else
             {
                 //__instance.ik.solver.IKPositionWeight = 0f;
                 vrik.solver.IKPositionWeight = 0f;
             }
             return false;
         }
     }*/
}




