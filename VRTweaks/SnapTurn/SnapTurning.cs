using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;

namespace VRTweaks.SnapTurn
{
    [HarmonyPatch(typeof(MainCameraControl), "Update")]
    public static class SnapTurning
    {
        private static float SnapAngle => SnapTurningOptions.SnapAngles[SnapTurningOptions.SnapAngleChoiceIndex];
        private static bool _didLookRight;
        private static bool _didLookLeft;
        private static bool _isLookingUpOrDown;
        private static bool _shouldSnapTurn;
        private static bool _shouldIgnoreLookRightOrLeft;
        private static bool _didClearInput;

        [HarmonyPrefix]
        public static bool Prefix(MainCameraControl __instance)
        {
            if (!SnapTurningOptions.EnableSnapTurning || Player.main.isPiloting)
            {
                return true; //Enter vanilla method
            }

            UpdateFields();

            if (_isLookingUpOrDown)
            {
                ClearInput();
                return true; 
            }

            if (_shouldSnapTurn)
            {
                UpdatePlayerRotation();
                return false; //Don't enter vanilla method if we snap turn
            }

            if (_shouldIgnoreLookRightOrLeft || SnapTurningOptions.DisableMouseLook)
            {
                ClearInput();
            }

            return true;
        }


        [HarmonyPostfix]
        public static void Postfix()
        {
            if(_didClearInput)
            {
                GameInput.clearInput = false;
            }
        }

        private static void UpdateFields()
        {
            var lookDelta = GameInput.GetLookDelta();

            _isLookingUpOrDown = Mathf.Abs(lookDelta.y) > Mathf.Abs(lookDelta.x);

            _didLookRight = !_isLookingUpOrDown && (GameInput.GetButtonDown(GameInput.Button.LookRight));
            _didLookLeft = !_isLookingUpOrDown && (GameInput.GetButtonDown(GameInput.Button.LookLeft));
            _shouldIgnoreLookRightOrLeft = GameInput.GetButtonHeld(GameInput.Button.LookRight) || GameInput.GetButtonHeld(GameInput.Button.LookLeft);
            _didLookLeft = !_isLookingUpOrDown && (GameInput.GetButtonDown(GameInput.Button.LookLeft));

            _shouldSnapTurn = XRSettings.enabled && (_didLookLeft || _didLookRight);
        }

        private static void UpdatePlayerRotation()
        {
            Player.main.transform.localRotation = Quaternion.Euler(GetNewEulerAngles());
        }

        private static Vector3 GetNewEulerAngles()
        {
            var newEulerAngles = Player.main.transform.localRotation.eulerAngles;

            if (_didLookRight)
            {
                newEulerAngles.y += SnapAngle;
            }
            else if (_didLookLeft)
            {
                newEulerAngles.y -= SnapAngle;
            }

            return newEulerAngles;
        }

        private static void ClearInput()
        {
            //Ignore camera rotation this frame
            GameInput.ClearInput();
            _didClearInput = true;
        }
    }
}
