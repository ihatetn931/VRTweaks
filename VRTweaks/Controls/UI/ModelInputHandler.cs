using System;
using UnityEngine;
using UWE;
using VRTweaks.Controls.Tools;

namespace VRTweaks.Controls.UI
{
	// Token: 0x02000707 RID: 1799
	public class ModeInputHandler : IInputHandler
	{
		// Token: 0x06003E13 RID: 15891 RVA: 0x00011ECE File Offset: 0x000100CE
		bool IInputHandler.HandleInput()
		{
			return true;
		}

		// Token: 0x06003E14 RID: 15892 RVA: 0x00159A58 File Offset: 0x00157C58
		bool IInputHandler.HandleLateInput()
		{
			if (!this.canHandleInput)
			{
				return false;
			}
			FPSInputModule.current.EscapeMenu();
			Builder.Update();
			if (Player.main.GetLeftHandDown())
			{
				UWE.Utils.lockCursor = true;
			}
			if (GameInput.GetButtonHeldTime(GameInput.Button.Reload) > 0.1f)
			{
				FPSInputModule.current.lockRotation = true;
				if (GameInput.GetButtonHeld(BuilderPatches.buttonRotateCW) || GameInput.GetButtonDown(BuilderPatches.buttonRotateCW))
				{
					Builder.additiveRotation = MathExtensions.RepeatAngle(Builder.additiveRotation - Time.deltaTime * Builder.additiveRotationSpeed);
				}
				else if (GameInput.GetButtonHeld(BuilderPatches.buttonRotateCCW) || GameInput.GetButtonDown(BuilderPatches.buttonRotateCCW))
				{
					Builder.additiveRotation = MathExtensions.RepeatAngle(Builder.additiveRotation + Time.deltaTime * Builder.additiveRotationSpeed);
				}
			}
			else
            {
				FPSInputModule.current.lockRotation = false;
			}
			if (UWE.Utils.lockCursor && GameInput.GetButtonDown(GameInput.Button.LeftHand))
			{
				if (Builder.TryPlace())
				{
					return false;
				}
			}
			else if (this.focusFrame != Time.frameCount && GameInput.GetButtonDown(GameInput.Button.RightHand))
			{
				return false;
			}
			return true;
		}

		// Token: 0x06003E15 RID: 15893 RVA: 0x00159B10 File Offset: 0x00157D10
		void IInputHandler.OnFocusChanged(InputFocusMode mode)
		{
			switch (mode)
			{
				case InputFocusMode.Add:
				case InputFocusMode.Restore:
					this.focusFrame = Time.frameCount;
					UWE.Utils.lockCursor = true;
					return;
				case InputFocusMode.Remove:
					Builder.End();
					break;
				case InputFocusMode.Suspend:
					break;
				default:
					return;
			}
		}

		// Token: 0x04003CB7 RID: 15543
		public bool canHandleInput;

		// Token: 0x04003CB8 RID: 15544
		private int focusFrame = -1;
	}
}
