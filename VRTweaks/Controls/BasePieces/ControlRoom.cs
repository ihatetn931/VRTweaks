using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class ControlRoom
    {
		[HarmonyPatch(typeof(BaseControlRoom), nameof(BaseControlRoom.Update))]
		public static class BaseControlRoom_Update__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(BaseControlRoom __instance)
			{
				if (__instance.playerNearby)
				{
					if (__instance.navigatingMinimap)
					{
						HandReticle.main.SetIcon(HandReticle.IconType.Default, 1f);
						HandReticle.main.SetTextRaw(HandReticle.TextType.Use, __instance.controlTooltip);
					}
					__instance.UpdateLeaks();
					int num = Physics.RaycastNonAlloc(new Ray(VRHandsController.rightController.transform.position, VRHandsController.rightController.transform.right), __instance.hits, 2.5f);
					BaseMiniCell baseMiniCell = null;
					for (int i = 0; i < num; i++)
					{
						BaseMiniCell component = __instance.hits[i].collider.gameObject.GetComponent<BaseMiniCell>();
						if (component)
						{
							baseMiniCell = component;
							break;
						}
					}
					if (baseMiniCell && !__instance.minimapPrefabs.IsAllowedToUnpower(__instance.baseComp.GetCellType(baseMiniCell.cell)))
					{
						baseMiniCell = null;
					}
					if (baseMiniCell != __instance.mouseOverCell)
					{
						if (__instance.mouseOverCell)
						{
							__instance.mouseOverCell.color = (__instance.baseComp.IsPowered(__instance.mouseOverCell.cell) ? __instance.active : __instance.inactive);
						}
						__instance.mouseOverCell = baseMiniCell;
						if (__instance.mouseOverCell)
						{
							MinimapTooltip.main.cellType = __instance.mouseOverCell.cellType;
						}
					}
					if (__instance.mouseOverCell)
					{
						MinimapTooltip.main.gameObject.SetActive(true);
						if (GameInput.GetButtonUp(GameInput.Button.LeftHand))
						{
							__instance.mouseOverCell.OnClick();
							if (__instance.baseComp.IsPowered(baseMiniCell.cell))
							{
								Utils.PlayFMODAsset(__instance.powerUpSound, __instance.transform, 20f);
							}
							else
							{
								Utils.PlayFMODAsset(__instance.powerDownSound, __instance.transform, 20f);
							}
						}
						__instance.mouseOverCell.color = (__instance.baseComp.IsPowered(baseMiniCell.cell) ? __instance.activeHover : __instance.inactiveHover);
						HandReticle main = HandReticle.main;
						main.SetText(HandReticle.TextType.Hand, __instance.baseComp.IsPowered(__instance.mouseOverCell.cell) ? "UnpowerBaseSection" : "PowerBaseSection", true, GameInput.Button.LeftHand);
						main.SetIcon(HandReticle.IconType.Hand, 1f);
						MinimapTooltip.main.consumptionRate = __instance.baseComp.GetCellPowerConsumptionRate(__instance.mouseOverCell.cell);
						MinimapTooltip.main.creationRate = __instance.baseComp.GetCellPowerChargeRate(__instance.mouseOverCell.cell);
						MinimapTooltip.main.structuralIntegrity = __instance.baseComp.GetCellStructualIntegrity(__instance.mouseOverCell.cell);
						MinimapTooltip.main.powered = __instance.baseComp.IsPowered(__instance.mouseOverCell.cell);
					}
					else
					{
						MinimapTooltip.main.gameObject.SetActive(false);
					}
				}
				if (__instance.navigatingMinimap && __instance.minimapBase)
				{
					float num2 = 0f;
					bool flag = (__instance.minimapConsole.transform.position - Player.main.transform.position).magnitude > 3f;
					if (GameInput.GetButtonUp(GameInput.Button.RightHand) || flag || GameInput.GetButtonUp(GameInput.Button.Exit))
					{
						__instance.navigatingMinimap = false;
						Player.main.ExitLockedMode(false, true, null);
						return false;
					}
					Vector3 vector = __instance.minimapCenter.TransformDirection(GameInput.GetMoveDirection()) * Time.deltaTime * 1.3f;
					__instance.deltaMove += vector;
					Vector3 b = __instance.minimapBase.transform.InverseTransformVector(vector);
					Vector3 vector2 = __instance.minimapBase.transform.localPosition + b;
					if (!__instance.boundary.Contains(vector2))
					{
						vector2 = __instance.boundary.ClosestPoint(vector2);
					}
					__instance.minimapBase.transform.localPosition = vector2;
					if (GameInput.GetButtonDown(GameInput.Button.CycleNext))
					{
						num2 = 1f;
					}
					if (GameInput.GetButtonDown(GameInput.Button.CyclePrev))
					{
						num2 = -1f;
					}
					if (num2 > 0f)
					{
						Vector3 localEulerAngles = __instance.pivot.localEulerAngles;
						localEulerAngles.y += num2 * 15f;
						__instance.pivot.localEulerAngles = localEulerAngles;
					}
					if (num2 > 0f || Mathf.Abs(__instance.deltaMove.x) > BaseControlRoom.cellSize.x || Mathf.Abs(__instance.deltaMove.y) > BaseControlRoom.cellSize.y || Mathf.Abs(__instance.deltaMove.z) > BaseControlRoom.cellSize.z)
					{
						__instance.UpdateMinimapColliders();
					}
				}
				return false;
			}
		}
	}
}
