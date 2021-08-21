using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class BaseCell
    {
		[HarmonyPatch(typeof(BaseAddCellGhost), nameof(BaseAddCellGhost.UpdateRotation))]
		public static class BaseAddCellGhost_UpdateRotation__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref bool geometryChanged, BaseAddCellGhost __instance)
			{
				if (__instance.cellTypes.Count < 2 || Builder.UpdateRotation(__instance.cellTypes.Count))
				{
					return false;
				}
				Base.CellType cellType = __instance.GetCellType();
				Int3 @int = Base.CellSize[(int)cellType];
				if (@int != __instance.ghostBase.GetSize())
				{
					__instance.ghostBase.ClearGeometry();
					__instance.ghostBase.SetSize(@int);
				}
				__instance.ghostBase.ClearCell(Int3.zero);
				__instance.ghostBase.SetCell(Int3.zero, cellType);
				__instance.RebuildGhostGeometry(true);
				geometryChanged = true;
				return false;
			}
		}

		[HarmonyPatch(typeof(BaseAddCellGhost), nameof(BaseAddCellGhost.UpdatePlacement))]
		public static class BaseAddCellGhost_UpdatePlacement__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase, BaseAddCellGhost __instance, ref bool __result)
			{
				positionFound = false;
				geometryChanged = false;
				BaseAddCellGhost.overrideFaces.Clear();
				__instance.UpdateRotation(ref geometryChanged);
				Base.CellType cellType = __instance.GetCellType();
				float num = ghostModelParentConstructableBase.placeDefaultDistance;
				Int3 @int = Base.CellSize[(int)cellType];
				Vector3 direction = Vector3.Scale((@int - 1).ToVector3(), Base.halfCellSize);
				Vector3 position = camera.position;
				Vector3 forward = camera.forward;
				float searchDistance = 20f;
				if (cellType != Base.CellType.Moonpool)
				{
					if (cellType - Base.CellType.LargeRoom <= 1)
					{
						searchDistance = 50f;
						goto IL_87;
					}
					if (cellType != Base.CellType.MoonpoolRotated)
					{
						goto IL_87;
					}
				}
				searchDistance = 30f;
			IL_87:
				__instance.prevTargetBase = __instance.targetBase;
				__instance.targetBase = BaseGhost.FindBase(camera, searchDistance);
				bool flag;
				if (__instance.targetBase != null)
				{
					positionFound = true;
					flag = true;
					Vector3 a = position + forward * num;
					Vector3 b = __instance.targetBase.transform.TransformDirection(direction);
					Int3 int2 = __instance.targetBase.WorldToGrid(a - b);
					int2 = __instance.Snap(int2, cellType);
					Int3 int3 = int2 + @int - 1;
					Int3.Bounds bounds = new Int3.Bounds(int2, int3);
					foreach (Int3 cell in bounds)
					{
						if (__instance.targetBase.GetCell(cell) != Base.CellType.Empty || __instance.targetBase.IsCellUnderConstruction(cell))
						{
							flag = false;
							break;
						}
					}
					if (flag && !__instance.allowedAboveWater)
					{
						flag = (__instance.targetBase.GridToWorld(int2).y <= 0f);
					}
					if (flag)
					{
						if (cellType == Base.CellType.Foundation)
						{
							Int3.Bounds bounds2 = __instance.targetBase.Bounds;
							int y = bounds2.mins.y;
							int y2 = bounds2.maxs.y;
							Int3.Bounds bounds3 = new Int3.Bounds(int2, new Int3(int3.x, int2.y, int3.z));
							using (Int3.RangeEnumerator enumerator = bounds3.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									Int3 cell2 = enumerator.Current;
									for (int i = int2.y - 1; i >= y; i--)
									{
										cell2.y = i;
										if (__instance.targetBase.IsCellUnderConstruction(cell2) || __instance.targetBase.GetCell(cell2) != Base.CellType.Empty)
										{
											flag = false;
											break;
										}
									}
									if (!flag)
									{
										break;
									}
									for (int j = int3.y + 1; j <= y2; j++)
									{
										cell2.y = j;
										Base.CellType cell3 = __instance.targetBase.GetCell(cell2);
										if (__instance.targetBase.IsCellUnderConstruction(cell2) || cell3 == Base.CellType.Foundation || cell3 == Base.CellType.WallFoundationN || cell3 == Base.CellType.WallFoundationW || cell3 == Base.CellType.WallFoundationS || cell3 == Base.CellType.WallFoundationE || cell3 == Base.CellType.Moonpool || cell3 == Base.CellType.MoonpoolRotated)
										{
											flag = false;
											break;
										}
										if (j == int3.y + 1 && (cell3 == Base.CellType.Observatory || cell3 == Base.CellType.MapRoom || cell3 == Base.CellType.MapRoomRotated))
										{
											flag = false;
											break;
										}
									}
									if (!flag)
									{
										break;
									}
								}
								goto IL_60F;
							}
						}
						if (cellType == Base.CellType.Moonpool || cellType == Base.CellType.MoonpoolRotated)
						{
							Int3.Bounds bounds4 = new Int3.Bounds(Int3.zero, @int - 1);
							foreach (Int3 v in bounds4)
							{
								Base.CellType cell4 = __instance.targetBase.GetCell(Base.GetAdjacent(int2 + v, Base.Direction.Above));
								Base.CellType cell5 = __instance.targetBase.GetCell(Base.GetAdjacent(int2 + v, Base.Direction.Below));
								flag &= (cell4 == Base.CellType.Empty && cell5 == Base.CellType.Empty);
							}
							if (flag && __instance.WouldHaveProhibitedCorridors())
							{
								flag = false;
							}
							if (flag && __instance.WouldCreateMoonpoolObstructions(cellType, int2))
							{
								flag = false;
							}
						}
						else if (cellType == Base.CellType.Room || cellType == Base.CellType.LargeRoom || cellType == Base.CellType.LargeRoomRotated)
						{
							Int3 size = Base.CellSize[(int)cellType];
							Int3 adjacent = Base.GetAdjacent(int2, Base.Direction.Below);
							bool flag2 = __instance.targetBase.GetRawCellType(adjacent) == cellType;
							bool flag3 = __instance.targetBase.CompareCellTypes(adjacent, size, Base.CellType.Empty, false, true);
							bool flag4 = __instance.targetBase.CompareCellTypes(adjacent, size, Base.sFoundationCheckTypes, true);
							flag &= (flag2 || flag3 || flag4);
							if (flag && flag2)
							{
								Base.FaceType face = __instance.targetBase.GetFace(new Base.Face(adjacent, Base.Direction.Above));
								if (face == Base.FaceType.GlassDome || face == Base.FaceType.LargeGlassDome)
								{
									flag = false;
								}
							}
							if (flag && flag2)
							{
								Base.FaceType faceType;
								__instance.targetBase.GetGhostFace(new Base.Face(adjacent, Base.Direction.Above), out faceType);
								if (faceType == Base.FaceType.GlassDome || faceType == Base.FaceType.LargeGlassDome)
								{
									flag = false;
								}
							}
							Int3 adjacent2 = Base.GetAdjacent(int2, Base.Direction.Above);
							bool flag5 = __instance.targetBase.GetRawCellType(adjacent2) == cellType;
							bool flag6 = __instance.targetBase.CompareCellTypes(adjacent2, size, Base.CellType.Empty, false, true);
							flag &= (flag5 || flag6);
							if (flag && __instance.WouldHaveProhibitedCorridors())
							{
								flag = false;
							}
							if (flag && __instance.WouldCreateMoonpoolObstructions(cellType, int2))
							{
								flag = false;
							}
							if (flag)
							{
								if (flag5)
								{
									BaseAddCellGhost.overrideFaces.Add(new KeyValuePair<Base.Face, Base.FaceType>(new Base.Face(Int3.zero, Base.Direction.Above), Base.FaceType.Hole));
								}
								if (flag2)
								{
									BaseAddCellGhost.overrideFaces.Add(new KeyValuePair<Base.Face, Base.FaceType>(new Base.Face(Int3.zero, Base.Direction.Below), Base.FaceType.Hole));
								}
							}
						}
						else if (cellType == Base.CellType.Observatory)
						{
							flag &= (__instance.targetBase.GetCell(Base.GetAdjacent(int2, Base.Direction.Below)) == Base.CellType.Empty && __instance.targetBase.GetCell(Base.GetAdjacent(int2, Base.Direction.Above)) == Base.CellType.Empty);
							if (flag)
							{
								foreach (Base.Direction direction2 in Base.HorizontalDirections)
								{
									if (__instance.targetBase.GetCell(Base.GetAdjacent(int2, direction2)) == Base.CellType.Observatory)
									{
										flag = false;
										break;
									}
								}
							}
							if (flag)
							{
								int num2 = 0;
								if (__instance.targetBase.IsValidObsConnection(Base.GetAdjacent(int2, Base.Direction.North), Base.Direction.South))
								{
									num2++;
								}
								if (__instance.targetBase.IsValidObsConnection(Base.GetAdjacent(int2, Base.Direction.East), Base.Direction.West))
								{
									num2++;
								}
								if (__instance.targetBase.IsValidObsConnection(Base.GetAdjacent(int2, Base.Direction.South), Base.Direction.North))
								{
									num2++;
								}
								if (__instance.targetBase.IsValidObsConnection(Base.GetAdjacent(int2, Base.Direction.West), Base.Direction.East))
								{
									num2++;
								}
								if (num2 != 1)
								{
									flag = false;
								}
							}
						}
					}
				IL_60F:
					if (__instance.targetOffset != int2)
					{
						__instance.targetOffset = int2;
						__instance.ghostBase.SetCell(Int3.zero, cellType);
						for (int l = 0; l < BaseAddCellGhost.overrideFaces.Count; l++)
						{
							KeyValuePair<Base.Face, Base.FaceType> keyValuePair = BaseAddCellGhost.overrideFaces[l];
							__instance.ghostBase.SetFaceType(keyValuePair.Key, keyValuePair.Value);
						}
						BaseAddCellGhost.overrideFaces.Clear();
						__instance.RebuildGhostGeometry(true);
						geometryChanged = true;
					}
					ghostModelParentConstructableBase.transform.position = __instance.targetBase.GridToWorld(int2);
					ghostModelParentConstructableBase.transform.rotation = __instance.targetBase.transform.rotation;
				}
				else
				{
					if (__instance.prevTargetBase != null)
					{
						__instance.SetupGhost();
						geometryChanged = true;
					}
					Bounds aaBounds = Builder.aaBounds;
					num = BaseGhost.GetStartingDistance() + Mathf.Max(aaBounds.extents.x, aaBounds.extents.z);
					aaBounds.extents = 1.05f * aaBounds.extents;
					Vector3 b2 = ghostModelParentConstructableBase.transform.TransformDirection(aaBounds.center);
					Vector3 a2;
					flag = __instance.PlaceWithBoundsCast(position, forward, aaBounds.extents, num, __instance.minHeightFromTerrain, __instance.maxHeightFromTerrain, out a2);
					ghostModelParentConstructableBase.transform.position = a2 - b2;
					if (flag)
					{
						__instance.targetOffset = Int3.zero;
					}
				}
				if (flag && ghostModelParentConstructableBase.transform.position.y > 35f)
				{
					flag = (BaseGhost.GetDistanceToGround(ghostModelParentConstructableBase.transform.position) <= 25f);
				}
				__result = flag;
				return false;
			}
		}
	}
}
