using HarmonyLib;
using RootMotion.FinalIK;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using VRTweaks.Controls;

namespace VRTweaks
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
        public ArmsController armsController;
        public static Player player = Player.main;
        public static FullBodyBipedIK ik;
        public static PDA pda;
       // public static LaserInput laserPointer;
        public static MotionCursor cursor;
        public static Animator animator;
        private static VRHandsController _main;
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
            var file = "Logs/All.txt";
            armsController = controller;
            player = global::Utils.GetLocalPlayerComp();
            ik = controller.GetComponent<FullBodyBipedIK>();
            animator = armsController.GetComponent<Animator>();
            pda = player.GetPDA();
            if (Controls.MotionControlConfig.ToggleDebugControllerBoxes)
            {
                //rightController = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rightController = new GameObject("rightController");
                //laserPointer = rightController.AddComponent<LaserInput>();
                cursor = rightController.AddComponent<MotionCursor>();

                leftController = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leftController.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                leftController.GetComponent<Renderer>().material.color = Color.blue;
            }
            else
            {
                rightController = new GameObject("rightController");
                leftController = new GameObject("leftController");
            }
            rightController.transform.parent = player.camRoot.transform;
            leftController.transform.parent = player.camRoot.transform;
        }
        public void UpdateHandPositions()
        {
            //Get right controller Position and Rotation
            InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
            XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPos);
            XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rightRot);
            rightController.transform.localPosition = rightPos + new Vector3(0f, -0.13f, -0.10f);
            rightController.transform.localRotation = rightRot * Quaternion.Euler(35f, 190f, 270f);

            //Get left controller Position and Rotation
            XRInputManager.GetXRInputManager().leftController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPos);
            XRInputManager.GetXRInputManager().leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion leftRot);
            leftController.transform.localPosition = leftPos + new Vector3(0f, -0.13f, -0.14f);
            leftController.transform.localRotation = leftRot * Quaternion.Euler(270f, 90f, 0f);

            //if player has a tool out activate the motion for that hand.
            if (heldItem != null)
            {
                if (heldItem.item.GetComponent<FlashLight>())
                {
                    ik.fixTransforms = false;
                    ik.solver.GetEndEffector(FullBodyBipedChain.RightArm).target = rightController.transform;
                }
                if (heldItem.item.GetComponent<BuilderTool>())
                {
                    ik.solver.GetEndEffector(FullBodyBipedChain.RightArm).target = rightController.transform;
                }
                if (heldItem.item.GetComponent<ScannerTool>())
                {
                    ik.solver.GetEndEffector(FullBodyBipedChain.RightArm).target = rightController.transform;
                }
                if (heldItem.item.GetComponent<PropulsionCannon>())
                {
                    ik.solver.GetEndEffector(FullBodyBipedChain.RightArm).target = rightController.transform;
                }
                if (heldItem.item.GetComponent<Knife>() || heldItem.item.GetComponent<HeatBlade>())
                {
                    ik.solver.GetEndEffector(FullBodyBipedChain.RightArm).target = rightController.transform;
                }
            }
        }

 /*       [HarmonyPatch(typeof(uGUI_QuickSlots), nameof(uGUI_QuickSlots.Update))]
        class uGUI_QuickSlotsr_Update_Patch
        {
            [HarmonyPostfix]
            public static void PostFix(uGUI_QuickSlots __instance)
            {
                __instance.rectTransform.SetParent(rightController.transform);
                __instance._rectTransform.anchoredPosition3D = rightController.transform.position;
            }
        }*/

        [HarmonyPatch(typeof(ArmsController))]
        [HarmonyPatch("Start")]
        class ArmsController_Start_Patch
        {
            [HarmonyPostfix]
            public static void PostFix(ArmsController __instance)
            {
                if (!XRSettings.enabled || Controls.MotionControlConfig.EnableMotionControls == false)
                {
                    return;
                }
                main.Initialize(__instance);
            }
        }
        [HarmonyPatch(typeof(ArmsController))]
        [HarmonyPatch(nameof(ArmsController.LateUpdate))]
        class ArmsController_Update_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(ArmsController __instance)
            {
                if (!XRSettings.enabled || Controls.MotionControlConfig.EnableMotionControls == false)
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
}


