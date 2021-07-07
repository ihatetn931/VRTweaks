﻿using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;

namespace VRTweaks.Controls
{
	[HarmonyPatch(typeof(GUIHand), nameof(GUIHand.OnUpdate))]
	public static class Knife_OnToolUse_Patch
	{
		[HarmonyPrefix]
		public static bool Prefix(GUIHand __instance)
		{
			///	Debug.Log("It Called");
			if (MotionControlConfig.EnableMotionControls == true)
			{
				__instance.usedToolThisFrame = false;
				__instance.usedAltAttackThisFrame = false;
				__instance.suppressTooltip = false;
				GameInput.Button button = GameInput.Button.LeftHand;
				GameInput.Button button2 = GameInput.Button.RightHand;
				GameInput.Button button3 = GameInput.Button.Reload;
				GameInput.Button button4 = GameInput.Button.Exit;
				GameInput.Button button5 = GameInput.Button.AltTool;
				GameInput.Button button6 = GameInput.Button.AutoMove;
				GameInput.Button button7 = GameInput.Button.PDA;
				__instance.UpdateInput(button);
				__instance.UpdateInput(button2);
				__instance.UpdateInput(button3);
				__instance.UpdateInput(button4);
				__instance.UpdateInput(button5);
				__instance.UpdateInput(GameInput.Button.Answer);
				__instance.UpdateInput(GameInput.Button.Exit);
				__instance.UpdateInput(button6);
				__instance.UpdateInput(button7);
				if (AvatarInputHandler.main.IsEnabled() && !uGUI.isIntro && !uGUI.isLoading)
				{
					uGUI_PopupNotification main = uGUI_PopupNotification.main;
					if (main != null && main.id == "Call")
					{
						if (__instance.GetInput(GameInput.Button.Answer, GUIHand.InputState.Down))
						{
							__instance.UseInput(GameInput.Button.Answer, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
							main.Answer();
							GameInput.ClearInput();
						}
						else if (__instance.GetInput(GameInput.Button.Exit, GUIHand.InputState.Down))
						{
							__instance.UseInput(GameInput.Button.Answer, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
							main.Decline();
							GameInput.ClearInput();
						}
					}
				}
				if (__instance.player.IsFreeToInteract() && (AvatarInputHandler.main.IsEnabled() || Builder.inputHandlerActive))
				{
					string text = string.Empty;
					InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
					Pickupable pickupable = (heldItem != null) ? heldItem.item : null;
					PlayerTool playerTool = (pickupable != null) ? pickupable.GetComponent<PlayerTool>() : null;
					bool flag = playerTool != null && playerTool is DropTool;
					EnergyMixin energyMixin = null;
					if (playerTool != null)
					{
						//Debug.Log("ToolName: " + playerTool.name + "  AnimToolName: " + playerTool.animToolName);
						text = playerTool.GetCustomUseText();
						energyMixin = playerTool.GetComponent<EnergyMixin>();
					}
					ItemAction itemAction = ItemAction.None;
					if ((playerTool == null || flag) && heldItem != null)
					{
						ItemAction allItemActions = Inventory.main.GetAllItemActions(heldItem);
						if ((allItemActions & ItemAction.Eat) != ItemAction.None)
						{
							itemAction = ItemAction.Eat;
						}
						else if ((allItemActions & ItemAction.Use) != ItemAction.None)
						{
							itemAction = ItemAction.Use;
						}
						if (itemAction == ItemAction.Eat)
						{
							UnityEngine.Object component = pickupable.GetComponent<Plantable>();
							LiveMixin component2 = pickupable.GetComponent<LiveMixin>();
							if (component == null && component2 != null)
							{
								itemAction = ItemAction.None;
							}
						}
						if (itemAction == ItemAction.None && (allItemActions & ItemAction.Drop) != ItemAction.None)
						{
							itemAction = ItemAction.Drop;
						}
						if (itemAction != ItemAction.None)
						{
							HandReticle.main.SetText(HandReticle.TextType.Use, GUIHand.GetActionString(itemAction, pickupable), true, GameInput.Button.RightHand);
						}
					}
					if (energyMixin != null && energyMixin.allowBatteryReplacement)
					{
						int num = Mathf.FloorToInt(energyMixin.GetEnergyScalar() * 100f);
						if (__instance.cachedTextEnergyScalar != num)
						{
							if (num <= 0)
							{
								__instance.cachedEnergyHudText = LanguageCache.GetButtonFormat("ExchangePowerSource", GameInput.Button.Reload);
							}
							else
							{
								__instance.cachedEnergyHudText = Language.main.GetFormat<float>("PowerPercent", energyMixin.GetEnergyScalar());
							}
							__instance.cachedTextEnergyScalar = num;
						}
						HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
						HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, __instance.cachedEnergyHudText);
					}
					else if (!string.IsNullOrEmpty(text))
					{
						HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
					}
					if (AvatarInputHandler.main.IsEnabled())
					{
						if (__instance.grabMode == GUIHand.GrabMode.None)
						{
							__instance.UpdateActiveTarget();
						}
						HandReticle.main.SetTargetDistance(__instance.activeHitDistance);
						if (__instance.activeTarget != null && !__instance.suppressTooltip)
						{
							TechType techType = CraftData.GetTechType(__instance.activeTarget);
							if (techType != TechType.None)
							{
								HandReticle.main.SetText(HandReticle.TextType.Hand, techType.AsString(false), true, GameInput.Button.None);
							}
							GUIHand.Send(__instance.activeTarget, HandTargetEventType.Hover, __instance);
						}
						if (Inventory.main.container.Contains(TechType.Scanner))
						{
							PDAScanner.UpdateTarget(8f);
							PDAScanner.ScanTarget scanTarget = PDAScanner.scanTarget;
							if (scanTarget.isValid && PDAScanner.CanScan(scanTarget) == PDAScanner.Result.Scan)
							{
								uGUI_ScannerIcon.main.Show();
							}
						}
						if (playerTool != null && (!flag || itemAction == ItemAction.Drop || itemAction == ItemAction.None))
						{
							//if (__instance.GetInput(button2, GUIHand.InputState.Down))
							if (playerTool.GetComponent<Knife>() && __instance.usedToolThisFrame == false)
							{
								//if (playerTool.OnRightHandDown())
								//if (VRHandsController.rightController.GetComponent<Rigidbody>().velocity.magnitude >= 100.0)
								XRInputManager.GetXRInputManager().rightController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 rightControllerVelocity);
								if (rightControllerVelocity.magnitude >= 1.5f)
								{
									//Debug.Log("Velocity: " + OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).magnitude);
									//__instance.UseInput(button2, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
									__instance.usedToolThisFrame = true;
									playerTool.OnToolActionStart();
								}
								else
								{
									__instance.usedToolThisFrame = false;
								}
							}
							if (__instance.GetInput(button2, GUIHand.InputState.Down) && !playerTool.GetComponent<Knife>())
							{
								if (playerTool.OnRightHandDown())
								{
									__instance.UseInput(button2, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
									__instance.usedToolThisFrame = true;
									playerTool.OnToolActionStart();
								}
							}
							else if (__instance.GetInput(button2, GUIHand.InputState.Held))
							{
								if (playerTool.OnRightHandHeld())
								{
									__instance.UseInput(button2, GUIHand.InputState.Down | GUIHand.InputState.Held);
								}
							}
							else if (__instance.GetInput(button2, GUIHand.InputState.Up) && playerTool.OnRightHandUp())
							{
								__instance.UseInput(button2, GUIHand.InputState.Up);
							}
							if (__instance.GetInput(button, GUIHand.InputState.Down))
							{
								if (playerTool.OnLeftHandDown())
								{
									__instance.UseInput(button, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
									playerTool.OnToolActionStart();
								}

							}
							else if (__instance.GetInput(button, GUIHand.InputState.Held))
							{
								if (playerTool.OnLeftHandHeld())
								{
									__instance.UseInput(button, GUIHand.InputState.Down | GUIHand.InputState.Held);
								}
							}
							else if (__instance.GetInput(button, GUIHand.InputState.Up) && playerTool.OnLeftHandUp())
							{
								__instance.UseInput(button, GUIHand.InputState.Up);
							}
							if (__instance.GetInput(button5, GUIHand.InputState.Down))
							{
								if (playerTool.OnAltDown())
								{
									__instance.UseInput(button5, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
									__instance.usedAltAttackThisFrame = true;
									playerTool.OnToolActionStart();
								}
							}
							else if (__instance.GetInput(button5, GUIHand.InputState.Held))
							{
								if (playerTool.OnAltHeld())
								{
									__instance.UseInput(button5, GUIHand.InputState.Down | GUIHand.InputState.Held);
								}
							}
							else if (__instance.GetInput(button5, GUIHand.InputState.Up) && playerTool.OnAltUp())
							{
								__instance.UseInput(button5, GUIHand.InputState.Up);
							}
							if (__instance.GetInput(button3, GUIHand.InputState.Down) && playerTool.OnReloadDown())
							{
								__instance.UseInput(button3, GUIHand.InputState.Down);
							}
							if (__instance.GetInput(button4, GUIHand.InputState.Down) && playerTool.OnExitDown())
							{
								__instance.UseInput(button4, GUIHand.InputState.Down);
							}
						}
						if (itemAction != ItemAction.None && __instance.GetInput(button2, GUIHand.InputState.Down))
						{
							if (itemAction == ItemAction.Drop)
							{
								__instance.UseInput(button2, GUIHand.InputState.Down | GUIHand.InputState.Held);
								Inventory.main.DropHeldItem(true);
							}
							else
							{
								__instance.UseInput(button2, GUIHand.InputState.Down | GUIHand.InputState.Held);
								Inventory.main.ExecuteItemAction(itemAction, heldItem);
							}
						}
						if (__instance.activeTarget != null)
						{
							if (__instance.GetInput(button, GUIHand.InputState.Down))
							{
								__instance.UseInput(button, GUIHand.InputState.Down | GUIHand.InputState.Held);
								GUIHand.Send(__instance.activeTarget, HandTargetEventType.Click, __instance);
							}
						}
						else if (KnownTech.Contains(TechType.SnowBall) && !__instance.player.isUnderwater.value && !Player.main.IsInside())
						{
							VFXSurfaceTypes vfxsurfaceTypes = VFXSurfaceTypes.none;
							int layerMask = 1 << LayerID.TerrainCollider | 1 << LayerID.Default;
							RaycastHit raycastHit;
							if (Physics.Raycast(MainCamera.camera.transform.position, MainCamera.camera.transform.forward, out raycastHit, 3f, layerMask) && raycastHit.collider.gameObject.layer == LayerID.TerrainCollider)
							{
								vfxsurfaceTypes = Utils.GetTerrainSurfaceType(raycastHit.point, raycastHit.normal, VFXSurfaceTypes.none);
							}
							if (vfxsurfaceTypes == VFXSurfaceTypes.snow)
							{
								HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
								HandReticle.main.SetText(HandReticle.TextType.Hand, "PickUpSnow", true, GameInput.Button.LeftHand);
								if (__instance.GetInput(button, GUIHand.InputState.Down))
								{
									__instance.UseInput(button, GUIHand.InputState.Down | GUIHand.InputState.Held);
									GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.snowBallPrefab);
									if (!Inventory.main.Pickup(gameObject.GetComponent<Pickupable>(), false))
									{
										UnityEngine.Object.Destroy(gameObject);
									}
									else
									{
										Utils.PlayFMODAsset(__instance.snowballPickupSound, MainCamera.camera.transform, 20f);
									}
								}
							}
						}
					}
				}
				if (AvatarInputHandler.main.IsEnabled() && __instance.GetInput(button6, GUIHand.InputState.Down))
				{
					__instance.UseInput(button6, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
					GameInput.SetAutoMove(!GameInput.GetAutoMove());
				}
				if (AvatarInputHandler.main.IsEnabled() && !uGUI.isIntro && !uGUI.isLoading && __instance.GetInput(button7, GUIHand.InputState.Down))
				{
					__instance.UseInput(button7, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
					__instance.player.GetPDA().Open(PDATab.None, null, null);
				}
			}
			else
            {
				__instance.usedToolThisFrame = false;
				__instance.usedAltAttackThisFrame = false;
				__instance.suppressTooltip = false;
				GameInput.Button button = GameInput.Button.LeftHand;
				GameInput.Button button2 = GameInput.Button.RightHand;
				GameInput.Button button3 = GameInput.Button.Reload;
				GameInput.Button button4 = GameInput.Button.Exit;
				GameInput.Button button5 = GameInput.Button.AltTool;
				GameInput.Button button6 = GameInput.Button.AutoMove;
				GameInput.Button button7 = GameInput.Button.PDA;
				__instance.UpdateInput(button);
				__instance.UpdateInput(button2);
				__instance.UpdateInput(button3);
				__instance.UpdateInput(button4);
				__instance.UpdateInput(button5);
				__instance.UpdateInput(GameInput.Button.Answer);
				__instance.UpdateInput(GameInput.Button.Exit);
				__instance.UpdateInput(button6);
				__instance.UpdateInput(button7);
				if (AvatarInputHandler.main.IsEnabled() && !uGUI.isIntro && !uGUI.isLoading)
				{
					uGUI_PopupNotification main = uGUI_PopupNotification.main;
					if (main != null && main.id == "Call")
					{
						if (__instance.GetInput(GameInput.Button.Answer, GUIHand.InputState.Down))
						{
							__instance.UseInput(GameInput.Button.Answer, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
							main.Answer();
							GameInput.ClearInput();
						}
						else if (__instance.GetInput(GameInput.Button.Exit, GUIHand.InputState.Down))
						{
							__instance.UseInput(GameInput.Button.Answer, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
							main.Decline();
							GameInput.ClearInput();
						}
					}
				}
				if (__instance.player.IsFreeToInteract() && (AvatarInputHandler.main.IsEnabled() || Builder.inputHandlerActive))
				{
					string text = string.Empty;
					InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
					Pickupable pickupable = (heldItem != null) ? heldItem.item : null;
					PlayerTool playerTool = (pickupable != null) ? pickupable.GetComponent<PlayerTool>() : null;
					bool flag = playerTool != null && playerTool is DropTool;
					EnergyMixin energyMixin = null;
					if (playerTool != null)
					{
						//Debug.Log("ToolName: " + playerTool.name + "  AnimToolName: " + playerTool.animToolName);
						text = playerTool.GetCustomUseText();
						energyMixin = playerTool.GetComponent<EnergyMixin>();
					}
					ItemAction itemAction = ItemAction.None;
					if ((playerTool == null || flag) && heldItem != null)
					{
						ItemAction allItemActions = Inventory.main.GetAllItemActions(heldItem);
						if ((allItemActions & ItemAction.Eat) != ItemAction.None)
						{
							itemAction = ItemAction.Eat;
						}
						else if ((allItemActions & ItemAction.Use) != ItemAction.None)
						{
							itemAction = ItemAction.Use;
						}
						if (itemAction == ItemAction.Eat)
						{
							UnityEngine.Object component = pickupable.GetComponent<Plantable>();
							LiveMixin component2 = pickupable.GetComponent<LiveMixin>();
							if (component == null && component2 != null)
							{
								itemAction = ItemAction.None;
							}
						}
						if (itemAction == ItemAction.None && (allItemActions & ItemAction.Drop) != ItemAction.None)
						{
							itemAction = ItemAction.Drop;
						}
						if (itemAction != ItemAction.None)
						{
							HandReticle.main.SetText(HandReticle.TextType.Use, GUIHand.GetActionString(itemAction, pickupable), true, GameInput.Button.RightHand);
						}
					}
					if (energyMixin != null && energyMixin.allowBatteryReplacement)
					{
						int num = Mathf.FloorToInt(energyMixin.GetEnergyScalar() * 100f);
						if (__instance.cachedTextEnergyScalar != num)
						{
							if (num <= 0)
							{
								__instance.cachedEnergyHudText = LanguageCache.GetButtonFormat("ExchangePowerSource", GameInput.Button.Reload);
							}
							else
							{
								__instance.cachedEnergyHudText = Language.main.GetFormat<float>("PowerPercent", energyMixin.GetEnergyScalar());
							}
							__instance.cachedTextEnergyScalar = num;
						}
						HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
						HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, __instance.cachedEnergyHudText);
					}
					else if (!string.IsNullOrEmpty(text))
					{
						HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
					}
					if (AvatarInputHandler.main.IsEnabled())
					{
						if (__instance.grabMode == GUIHand.GrabMode.None)
						{
							__instance.UpdateActiveTarget();
						}
						HandReticle.main.SetTargetDistance(__instance.activeHitDistance);
						if (__instance.activeTarget != null && !__instance.suppressTooltip)
						{
							TechType techType = CraftData.GetTechType(__instance.activeTarget);
							if (techType != TechType.None)
							{
								HandReticle.main.SetText(HandReticle.TextType.Hand, techType.AsString(false), true, GameInput.Button.None);
							}
							GUIHand.Send(__instance.activeTarget, HandTargetEventType.Hover, __instance);
						}
						if (Inventory.main.container.Contains(TechType.Scanner))
						{
							PDAScanner.UpdateTarget(8f);
							PDAScanner.ScanTarget scanTarget = PDAScanner.scanTarget;
							if (scanTarget.isValid && PDAScanner.CanScan(scanTarget) == PDAScanner.Result.Scan)
							{
								uGUI_ScannerIcon.main.Show();
							}
						}
						if (playerTool != null && (!flag || itemAction == ItemAction.Drop || itemAction == ItemAction.None))
						{
							if (__instance.GetInput(button2, GUIHand.InputState.Down))
							{
								if (playerTool.OnRightHandDown())
								{
									__instance.UseInput(button2, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
									__instance.usedToolThisFrame = true;
									playerTool.OnToolActionStart();
								}
							}
							else if (__instance.GetInput(button2, GUIHand.InputState.Held))
							{
								if (playerTool.OnRightHandHeld())
								{
									__instance.UseInput(button2, GUIHand.InputState.Down | GUIHand.InputState.Held);
								}
							}
							else if (__instance.GetInput(button2, GUIHand.InputState.Up) && playerTool.OnRightHandUp())
							{
								__instance.UseInput(button2, GUIHand.InputState.Up);
							}
							if (__instance.GetInput(button, GUIHand.InputState.Down))
							{
								if (playerTool.OnLeftHandDown())
								{
									__instance.UseInput(button, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
									playerTool.OnToolActionStart();
								}

							}
							else if (__instance.GetInput(button, GUIHand.InputState.Held))
							{
								if (playerTool.OnLeftHandHeld())
								{
									__instance.UseInput(button, GUIHand.InputState.Down | GUIHand.InputState.Held);
								}
							}
							else if (__instance.GetInput(button, GUIHand.InputState.Up) && playerTool.OnLeftHandUp())
							{
								__instance.UseInput(button, GUIHand.InputState.Up);
							}
							if (__instance.GetInput(button5, GUIHand.InputState.Down))
							{
								if (playerTool.OnAltDown())
								{
									__instance.UseInput(button5, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
									__instance.usedAltAttackThisFrame = true;
									playerTool.OnToolActionStart();
								}
							}
							else if (__instance.GetInput(button5, GUIHand.InputState.Held))
							{
								if (playerTool.OnAltHeld())
								{
									__instance.UseInput(button5, GUIHand.InputState.Down | GUIHand.InputState.Held);
								}
							}
							else if (__instance.GetInput(button5, GUIHand.InputState.Up) && playerTool.OnAltUp())
							{
								__instance.UseInput(button5, GUIHand.InputState.Up);
							}
							if (__instance.GetInput(button3, GUIHand.InputState.Down) && playerTool.OnReloadDown())
							{
								__instance.UseInput(button3, GUIHand.InputState.Down);
							}
							if (__instance.GetInput(button4, GUIHand.InputState.Down) && playerTool.OnExitDown())
							{
								__instance.UseInput(button4, GUIHand.InputState.Down);
							}
						}
						if (itemAction != ItemAction.None && __instance.GetInput(button2, GUIHand.InputState.Down))
						{
							if (itemAction == ItemAction.Drop)
							{
								__instance.UseInput(button2, GUIHand.InputState.Down | GUIHand.InputState.Held);
								Inventory.main.DropHeldItem(true);
							}
							else
							{
								__instance.UseInput(button2, GUIHand.InputState.Down | GUIHand.InputState.Held);
								Inventory.main.ExecuteItemAction(itemAction, heldItem);
							}
						}
						if (__instance.activeTarget != null)
						{
							if (__instance.GetInput(button, GUIHand.InputState.Down))
							{
								__instance.UseInput(button, GUIHand.InputState.Down | GUIHand.InputState.Held);
								GUIHand.Send(__instance.activeTarget, HandTargetEventType.Click, __instance);
							}
						}
						else if (KnownTech.Contains(TechType.SnowBall) && !__instance.player.isUnderwater.value && !Player.main.IsInside())
						{
							VFXSurfaceTypes vfxsurfaceTypes = VFXSurfaceTypes.none;
							int layerMask = 1 << LayerID.TerrainCollider | 1 << LayerID.Default;
							RaycastHit raycastHit;
							if (Physics.Raycast(MainCamera.camera.transform.position, MainCamera.camera.transform.forward, out raycastHit, 3f, layerMask) && raycastHit.collider.gameObject.layer == LayerID.TerrainCollider)
							{
								vfxsurfaceTypes = Utils.GetTerrainSurfaceType(raycastHit.point, raycastHit.normal, VFXSurfaceTypes.none);
							}
							if (vfxsurfaceTypes == VFXSurfaceTypes.snow)
							{
								HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
								HandReticle.main.SetText(HandReticle.TextType.Hand, "PickUpSnow", true, GameInput.Button.LeftHand);
								if (__instance.GetInput(button, GUIHand.InputState.Down))
								{
									__instance.UseInput(button, GUIHand.InputState.Down | GUIHand.InputState.Held);
									GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.snowBallPrefab);
									if (!Inventory.main.Pickup(gameObject.GetComponent<Pickupable>(), false))
									{
										UnityEngine.Object.Destroy(gameObject);
									}
									else
									{
										Utils.PlayFMODAsset(__instance.snowballPickupSound, MainCamera.camera.transform, 20f);
									}
								}
							}
						}
					}
				}
				if (AvatarInputHandler.main.IsEnabled() && __instance.GetInput(button6, GUIHand.InputState.Down))
				{
					__instance.UseInput(button6, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
					GameInput.SetAutoMove(!GameInput.GetAutoMove());
				}
				if (AvatarInputHandler.main.IsEnabled() && !uGUI.isIntro && !uGUI.isLoading && __instance.GetInput(button7, GUIHand.InputState.Down))
				{
					__instance.UseInput(button7, GUIHand.InputState.Down | GUIHand.InputState.Held | GUIHand.InputState.Up);
					__instance.player.GetPDA().Open(PDATab.None, null, null);
				}
			}
			return false;
		}
	}
}

