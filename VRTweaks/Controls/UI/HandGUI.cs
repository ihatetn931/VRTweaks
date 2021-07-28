
using HarmonyLib;
using UnityEngine;
using UWE;

namespace VRTweaks.Controls.UI
{
	class HandGUI
	{
		[HarmonyPatch(typeof(GUIHand), nameof(GUIHand.OnUpdate))]
		public static class GUIHand_OnUpdate__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(GUIHand __instance)
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
							if (Physics.Raycast(VRHandsController.rightController.transform.position, VRHandsController.rightController.transform.right, out raycastHit, 3f, layerMask) && raycastHit.collider.gameObject.layer == LayerID.TerrainCollider)
							{
								vfxsurfaceTypes = global::Utils.GetTerrainSurfaceType(raycastHit.point, raycastHit.normal, VFXSurfaceTypes.none);
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
										global::Utils.PlayFMODAsset(__instance.snowballPickupSound, MainCamera.camera.transform, 20f);
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
				return false;
			}
		}

		[HarmonyPatch(typeof(GUIHand), nameof(GUIHand.GetGrabbingHandPosition))]
		public static class GUIHand_GetGrabbingHandPosition__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref Vector3 __result,GUIHand __instance)
            {
				//Camera playerCamera = __instance.GetPlayerCamera();
				__result = VRHandsController.rightController.transform.position + VRHandsController.rightController.transform.right * __instance.activeHitDistance;
				return false;
            }
		}

		[HarmonyPatch(typeof(GUIHand), nameof(GUIHand.GetFacingInSub))]
		public static class GUIHand_GetFacingInSub__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref Facing __result, GUIHand __instance)
			{
				Vector3 vector = __instance.player.GetCurrentSub().transform.InverseTransformDirection(__instance.player.transform.right);
				float x = vector.x;
				float z = vector.z;
				if (Mathf.Abs(x) > Mathf.Abs(z))
				{
					if (x <= 0f)
					{
						__result = Facing.West;
					}
					__result = Facing.East;
				}
				else
				{
					if (z <= 0f)
					{
						__result = Facing.South;
					}
					__result = Facing.North;
				}
				return false;
			}

		}

		[HarmonyPatch(typeof(GUIHand), nameof(GUIHand.UpdateActiveTarget))]
		public static class GUIHand_UpdateActiveTarget__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(GUIHand __instance)
			{
				PlayerTool tool = __instance.GetTool();
				if (tool != null && tool.GetComponent<PropulsionCannon>() != null && tool.GetComponent<PropulsionCannon>().IsGrabbingObject())
				{
					__instance.activeTarget = tool.GetComponent<PropulsionCannon>().GetNearbyGrabbedObject();
					__instance.suppressTooltip = true;
					return false;
				}
				if (tool != null && tool.DoesOverrideHand())
				{
					__instance.activeTarget = null;
					__instance.activeHitDistance = 0f;
					return false;
				}
				DebugTargetConsoleCommand.RecordNext();
				if (Target.GetAllTarget(Player.main.gameObject, 2f, out __instance.activeTarget, out __instance.activeHitDistance))
				{
					IHandTarget handTarget = null;
					Transform transform = __instance.activeTarget.transform;
					while (transform != null)
					{
						handTarget = transform.GetComponent<IHandTarget>();
						if (handTarget != null)
						{
							__instance.activeTarget = transform.gameObject;
							break;
						}
						transform = transform.parent;
					}
					if (handTarget == null)
					{
						HarvestType harvestType = TechData.GetHarvestType(CraftData.GetTechType(__instance.activeTarget));
						if (harvestType == HarvestType.Pick)
						{
							if (global::Utils.FindAncestorWithComponent<Pickupable>(__instance.activeTarget) == null)
							{
								LargeWorldEntity largeWorldEntity = global::Utils.FindAncestorWithComponent<LargeWorldEntity>(__instance.activeTarget);
								largeWorldEntity.gameObject.AddComponent<Pickupable>();
								largeWorldEntity.gameObject.AddComponent<WorldForces>().useRigidbody = largeWorldEntity.GetComponent<Rigidbody>();
								return false;
							}
						}
						else if (harvestType == HarvestType.None)
						{
							__instance.activeTarget = null;
							return false;
						}
					}
				}
				else
				{
					__instance.activeTarget = null;
					__instance.activeHitDistance = 0f;
				}
				return false;
			}

		}
	}
}
