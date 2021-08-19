using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class Waterpark
    {
		[HarmonyPatch(typeof(BaseAddWaterPark), nameof(BaseAddWaterPark.UpdatePlacement))]
		public static class BaseAddWaterPark_UpdatePlacement__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase, ref bool __result, BaseAddWaterPark __instance)
			{
				positionFound = false;
				geometryChanged = false;
				Player main = Player.main;
				if (main == null || main.currentSub == null || !main.currentSub.isBase)
				{
					geometryChanged = __instance.SetupInvalid();
					return false;
				}
				__instance.targetBase = BaseGhost.FindBase(camera, 20f);
				if (__instance.targetBase == null)
				{
					geometryChanged = __instance.SetupInvalid();
					return false;
				}
				ConstructableBase componentInParent = __instance.GetComponentInParent<ConstructableBase>();
				float distance = (componentInParent != null) ? componentInParent.placeDefaultDistance : 0f;
				Base.Face face = new Base.Face(__instance.GetCell(camera, __instance.targetBase, distance), Base.Direction.Below);
				if (!__instance.targetBase.CanSetWaterPark(face.cell))
				{
					geometryChanged = __instance.SetupInvalid();
					return false;
				}
				Int3 @int = __instance.targetBase.NormalizeCell(face.cell);
				__instance.targetOffset = @int;
				Base.CellType cell = __instance.targetBase.GetCell(@int);
				Base.Face face2 = new Base.Face(face.cell - __instance.targetBase.GetAnchor(), face.direction);
				if (__instance.anchoredFace == null || __instance.anchoredFace.Value != face2)
				{
					__instance.anchoredFace = new Base.Face?(face2);
					Int3 int2 = Base.CellSize[(int)cell];
					geometryChanged = __instance.UpdateSize(int2);
					__instance.ghostBase.CopyFrom(__instance.targetBase, new Int3.Bounds(@int, @int + int2 - 1), @int * -1);
					__instance.ghostBase.ClearMasks();
					Int3 cell2 = face.cell - @int;
					if (cell == Base.CellType.Room)
					{
						Base.Face face3 = new Base.Face(cell2, face.direction);
						for (int i = 0; i < 2; i++)
						{
							__instance.ghostBase.SetFaceType(face3, Base.FaceType.WaterPark);
							__instance.ghostBase.SetFaceMask(face3, true);
							face3.direction = Base.OppositeDirections[(int)face3.direction];
						}
						foreach (Base.Direction direction in Base.HorizontalDirections)
						{
							face3.direction = direction;
							__instance.ghostBase.SetFaceType(face3, Base.FaceType.Solid);
							__instance.ghostBase.SetFaceMask(face3, true);
						}
					}
					else if (cell == Base.CellType.LargeRoom || cell == Base.CellType.LargeRoomRotated)
					{
						Base.Face face4 = default(Base.Face);
						int num = (cell == Base.CellType.LargeRoom) ? 0 : 2;
						for (int k = 0; k < 2; k++)
						{
							face4.cell = cell2;
							ref Int3 ptr = ref face4.cell;
							int j = num;
							ptr[j] += k;
							Base.Direction[] horizontalDirections = Base.HorizontalDirections;
							j = 0;
							while (j < horizontalDirections.Length)
							{
								Base.Direction direction2 = horizontalDirections[j];
								if (cell == Base.CellType.LargeRoomRotated)
								{
									if (k != 0 || direction2 != Base.Direction.North)
									{
										if (k != 1 || direction2 != Base.Direction.South)
										{
											goto IL_2D1;
										}
									}
								}
								else if ((k != 0 || direction2 != Base.Direction.East) && (k != 1 || direction2 != Base.Direction.West))
								{
									goto IL_2D1;
								}
							IL_2F6:
								j++;
								continue;
							IL_2D1:
								face4.direction = direction2;
								__instance.ghostBase.SetFaceMask(face4, true);
								__instance.ghostBase.SetFaceType(face4, Base.FaceType.Solid);
								goto IL_2F6;
							}
							face4.direction = face.direction;
							for (int l = 0; l < 2; l++)
							{
								__instance.ghostBase.SetFaceType(face4, Base.FaceType.WaterPark);
								__instance.ghostBase.SetFaceMask(face4, true);
								face4.direction = Base.OppositeDirections[(int)face4.direction];
							}
						}
					}
					__instance.RebuildGhostGeometry(true);
					geometryChanged = true;
				}
				ghostModelParentConstructableBase.transform.position = __instance.targetBase.GridToWorld(@int);
				ghostModelParentConstructableBase.transform.rotation = __instance.targetBase.transform.rotation;
				positionFound = true;
				if (__instance.targetBase.IsCellUnderConstruction(face.cell))
				{
					return false;
				}
				if (cell == Base.CellType.LargeRoom || cell == Base.CellType.LargeRoomRotated)
				{
					Int3 v = new Int3(0, 1, 0);
					if (__instance.targetBase.IsCellUnderConstruction(face.cell + v) || __instance.targetBase.IsCellUnderConstruction(face.cell - v))
					{
						return false;
					}
				}
				__result = ghostModelParentConstructableBase.transform.position.y <= 35f || BaseGhost.GetDistanceToGround(ghostModelParentConstructableBase.transform.position) <= 25f;
				return false;
			}
		}
	}
}
