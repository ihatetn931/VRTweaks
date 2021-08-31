using FMODUnity;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;

namespace VRTweaks.Controls.Tools
{
    class ToolBuilder
    {
		[HarmonyPatch(typeof(BuilderTool), nameof(BuilderTool.UpdateCustomUseText))]
		public static class Builder_GetAimTransform__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(BuilderTool __instance)
			{
				if (Builder.isPlacing)
				{
					__instance.customUseText = Language.main.GetFormat<string, string>("BuilderWithGhostFormat", uGUI.FormatButton(GameInput.Button.LeftHand, false, " / ", false), uGUI.FormatButton(GameInput.Button.RightHand, false, " / ", false));
					if (Builder.canRotate)
					{
						string format = Language.main.GetFormat<string, string>("GhostRotateInputHint", uGUI.FormatButton(GameInput.Button.Reload, true, ", ", false) + uGUI.FormatButton(BuilderPatches.buttonRotateCW, true, ", ", false), uGUI.FormatButton(BuilderPatches.buttonRotateCCW, true, ", ", false));
						__instance.customUseText = string.Format("{0}\n{1}", __instance.customUseText, format);
						return false;
					}
				}
				else
				{
					__instance.customUseText = LanguageCache.GetButtonFormat("BuilderUseFormat", GameInput.Button.RightHand);
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(BuilderTool), nameof(BuilderTool.OnHover), new Type[] { typeof(BaseDeconstructable) })]
		public static class Builder_OnHoverBase__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(BaseDeconstructable deconstructable)
			{
				HandReticle main = HandReticle.main;
				string butt = Language.main.GetFormat<string>("DeconstructFormat", uGUI.FormatButton(GameInput.Button.MoveUp, false, " / ", false) + uGUI.FormatButton(GameInput.Button.MoveDown, false, " / ", false));
				main.SetText(HandReticle.TextType.Hand, deconstructable.Name, true, GameInput.Button.None);
				main.SetText(HandReticle.TextType.HandSubscript, butt, false, GameInput.Button.None);
				return false;
			}
		}

		[HarmonyPatch(typeof(BuilderTool), nameof(BuilderTool.OnHover), new Type[] { typeof(Constructable)})]
		public static class Builder_OnHoverConstructable__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Constructable constructable)
			{
				if (constructable.constructed && !constructable.deconstructionAllowed)
				{
					return false;
				}
				HandReticle main = HandReticle.main;
				string buttonFormat = LanguageCache.GetButtonFormat("DeconstructFormat", GameInput.Button.Deconstruct);
				string butt = Language.main.GetFormat<string>("DeconstructFormat", uGUI.FormatButton(GameInput.Button.MoveUp, false, " / ", false) + uGUI.FormatButton(GameInput.Button.MoveDown, false, " / ", false));
				if (constructable.constructed)
				{
					HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.Get(constructable.techType), false, GameInput.Button.None);
					HandReticle.main.SetText(HandReticle.TextType.HandSubscript, butt, false, GameInput.Button.None);
					return false;
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(Language.main.GetFormat<string, string>("ConstructDeconstructFormat", LanguageCache.GetButtonFormat("ConstructFormat", GameInput.Button.LeftHand), butt));
				foreach (KeyValuePair<TechType, int> keyValuePair in constructable.GetRemainingResources())
				{
					TechType key = keyValuePair.Key;
					string text = Language.main.Get(key);
					int value = keyValuePair.Value;
					if (value > 1)
					{
						stringBuilder.AppendLine(Language.main.GetFormat<string, int>("RequireMultipleFormat", text, value));
					}
					else
					{
						stringBuilder.AppendLine(text);
					}
				}
				main.SetText(HandReticle.TextType.Hand, Language.main.Get(constructable.techType), false, GameInput.Button.None);
				main.SetText(HandReticle.TextType.HandSubscript, stringBuilder.ToString(), false, GameInput.Button.None);
				main.SetProgress(constructable.amount);
				main.SetIcon(HandReticle.IconType.Progress, 1.5f);
				return false;
			}
		}

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
				Targeting.GetTarget(30f, out gameObject, out num);
				if (gameObject == null)
				{
					return false;
				}
				bool buttonHeld = GameInput.GetButtonHeld(GameInput.Button.LeftHand);
			//	bool buttonDown = GameInput.GetButtonDown(GameInput.Button.Deconstruct);
				float deconstruct1 = GameInput.GetButtonHeldTime(GameInput.Button.MoveUp);
				float deconstruct2 = GameInput.GetButtonHeldTime(GameInput.Button.MoveDown);
			//	bool buttonHeld2 = GameInput.GetButtonHeld(GameInput.Button.Deconstruct);
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
						if (deconstruct1 > 0.2f && deconstruct2 > 0.2f)
						{
							if (constructable.constructed)
							{
								Builder.ResetLast();
								constructable.SetState(false, false);
								return false;
							}
							__instance.Construct(constructable, false, deconstruct1 > 0.2f && deconstruct2 > 0.2f);
							return false;
						}
					}
					else if (deconstruct1 > 0.2f && deconstruct2 > 0.2f && !string.IsNullOrEmpty(text))
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
							if (deconstruct1 > 0.2f && deconstruct2 > 0.2f)
							{
								Builder.ResetLast();
								baseDeconstructable.Deconstruct();
								return false;
							}
						}
						else if (deconstruct1 > 0.2f && deconstruct2 > 0.2f && !string.IsNullOrEmpty(text))
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
