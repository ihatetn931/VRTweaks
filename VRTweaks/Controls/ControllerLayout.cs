
using HarmonyLib;

namespace VRTweaks.Controls
{
    class ControllerLayout
    {
		[HarmonyPatch(typeof(GameInput), nameof(GameInput.SetupDefaultControllerBindings))]
		public static class GameInput_SetupDefaultControllerBindings__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(FPSInputModule __instance)
			{
				GameInput.ClearBindings(GameInput.Device.Controller);
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.Jump, GameInput.BindingSet.Primary, "ControllerButtonY");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.PDA, GameInput.BindingSet.Primary, "ControllerButtonBack");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.Deconstruct, GameInput.BindingSet.Primary, "ControllerDPadDown");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.Exit, GameInput.BindingSet.Primary, GameInput.SwapAcceptCancel ? "ControllerButtonA" : "ControllerButtonB");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.LeftHand, GameInput.BindingSet.Primary, GameInput.SwapAcceptCancel ? "ControllerButtonB" : "ControllerButtonA");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.LeftHand, GameInput.BindingSet.Secondary, "ControllerLeftTrigger");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.RightHand, GameInput.BindingSet.Primary, "ControllerRightTrigger");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.AltTool, GameInput.BindingSet.Primary, "ControllerDPadUp");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.TakePicture, GameInput.BindingSet.Primary, "ControllerButtonRightStick");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.Reload, GameInput.BindingSet.Primary, "ControllerButtonX");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.MoveForward, GameInput.BindingSet.Primary, "ControllerLeftStickUp");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.MoveBackward, GameInput.BindingSet.Primary, "ControllerLeftStickDown");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.MoveLeft, GameInput.BindingSet.Primary, "ControllerLeftStickLeft");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.MoveRight, GameInput.BindingSet.Primary, "ControllerLeftStickRight");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.MoveUp, GameInput.BindingSet.Primary, "ControllerButtonLeftBumper");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.MoveDown, GameInput.BindingSet.Primary, "ControllerButtonRightBumper");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.Sprint, GameInput.BindingSet.Primary, "ControllerButtonLeftStick");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.Answer, GameInput.BindingSet.Primary, "ControllerDPadDown");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.LookUp, GameInput.BindingSet.Primary, "ControllerRightStickUp");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.LookDown, GameInput.BindingSet.Primary, "ControllerRightStickDown");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.LookLeft, GameInput.BindingSet.Primary, "ControllerRightStickLeft");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.LookRight, GameInput.BindingSet.Primary, "ControllerRightStickRight");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.CycleNext, GameInput.BindingSet.Primary, "ControllerDPadRight");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.CyclePrev, GameInput.BindingSet.Primary, "ControllerDPadLeft");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UISubmit, GameInput.BindingSet.Primary, GameInput.SwapAcceptCancel ? "ControllerButtonB" : "ControllerButtonA");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UICancel, GameInput.BindingSet.Primary, GameInput.SwapAcceptCancel ? "ControllerButtonA" : "ControllerButtonB");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIClear, GameInput.BindingSet.Primary, "ControllerButtonX");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIAssign, GameInput.BindingSet.Primary, "ControllerButtonY");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIMenu, GameInput.BindingSet.Primary, "ControllerButtonHome");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UILeft, GameInput.BindingSet.Primary, "ControllerDPadLeft");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIRight, GameInput.BindingSet.Primary, "ControllerDPadRight");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIDown, GameInput.BindingSet.Primary, "ControllerDPadDown");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIUp, GameInput.BindingSet.Primary, "ControllerDPadUp");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UILeft, GameInput.BindingSet.Secondary, "ControllerLeftStickLeftMenu");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIRight, GameInput.BindingSet.Secondary, "ControllerLeftStickRightMenu");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIDown, GameInput.BindingSet.Secondary, "ControllerLeftStickDownMenu");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIUp, GameInput.BindingSet.Secondary, "ControllerLeftStickUpMenu");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIAdjustLeft, GameInput.BindingSet.Primary, "ControllerLeftStickLeft");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIAdjustRight, GameInput.BindingSet.Primary, "ControllerLeftStickRight");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIAdjustLeft, GameInput.BindingSet.Secondary, "ControllerDPadLeft");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIAdjustRight, GameInput.BindingSet.Secondary, "ControllerDPadRight");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIRightStickAdjustLeft, GameInput.BindingSet.Primary, "ControllerRightStickLeft");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIRightStickAdjustRight, GameInput.BindingSet.Primary, "ControllerRightStickRight");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UIPrevTab, GameInput.BindingSet.Primary, "ControllerButtonLeftBumper");
				GameInput.SetBindingInternal(GameInput.Device.Controller, GameInput.Button.UINextTab, GameInput.BindingSet.Primary, "ControllerButtonRightBumper");
				return false;
			}
		}
	}
}
