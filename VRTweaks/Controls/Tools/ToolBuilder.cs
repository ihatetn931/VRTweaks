using FMODUnity;
using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.Tools
{
    class ToolBuilder
    {
		[HarmonyPatch(typeof(BuilderTool), nameof(BuilderTool.HandleInput))]
		public static class BuilderTool_HandleInput__Patch
		{
			[HarmonyPrefix]
			static bool Prefix( BuilderTool __instance)
			{
				if (__instance.handleInputFrame == Time.frameCount)
				{
					return false;
				}
				__instance.handleInputFrame = Time.frameCount;
				if (!__instance.isDrawn || Builder.isPlacing || !AvatarInputHandler.main.IsEnabled())
				{
					return false;
				}
				if (__instance.TryDisplayNoPowerTooltip())
				{
					return false;
				}
				Targeting.AddToIgnoreList(Player.main.gameObject);
				GameObject gameObject;
				float num;
				Target.GetTargets(30f, out gameObject, out num);
				if (gameObject == null)
				{
					return false;
				}
				bool buttonHeld = GameInput.GetButtonHeld(GameInput.Button.LeftHand);
				bool buttonDown = GameInput.GetButtonDown(GameInput.Button.Deconstruct);
				bool buttonHeld2 = GameInput.GetButtonHeld(GameInput.Button.Deconstruct);
				Constructable constructable = gameObject.GetComponentInParent<Constructable>();
				if (constructable != null && num > constructable.placeMaxDistance)
				{
					constructable = null;
				}
				if (constructable != null)
				{
					__instance.OnHover(constructable);
					if (buttonHeld)
					{
						__instance.Construct(constructable, true, false);
						return false;
					}
					string text;
					if (constructable.DeconstructionAllowed(out text))
					{
						if (buttonHeld2)
						{
							if (constructable.constructed)
							{
								Builder.ResetLast();
								constructable.SetState(false, false);
								return false;
							}
							__instance.Construct(constructable, false, buttonDown);
							return false;
						}
					}
					else if (buttonDown && !string.IsNullOrEmpty(text))
					{
						RuntimeManager.PlayOneShot("event:/bz/ui/item_error", default(Vector3));
						ErrorMessage.AddMessage(text);
						return false;
					}
				}
				else
				{
					BaseDeconstructable baseDeconstructable = gameObject.GetComponentInParent<BaseDeconstructable>();
					if (baseDeconstructable == null)
					{
						BaseExplicitFace componentInParent = gameObject.GetComponentInParent<BaseExplicitFace>();
						if (componentInParent != null)
						{
							baseDeconstructable = componentInParent.parent;
						}
					}
					if (baseDeconstructable != null && num <= 11f)
					{
						string text;
						if (baseDeconstructable.DeconstructionAllowed(out text))
						{
							__instance.OnHover(baseDeconstructable);
							if (buttonDown)
							{
								Builder.ResetLast();
								baseDeconstructable.Deconstruct();
								return false;
							}
						}
						else if (buttonDown && !string.IsNullOrEmpty(text))
						{
							RuntimeManager.PlayOneShot("event:/bz/ui/item_error", default(Vector3));
							ErrorMessage.AddMessage(text);
						}
					}
				}
				return false;
			}

		}
	}
}
