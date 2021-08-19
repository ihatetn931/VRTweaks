using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
    class Foundation
    {
		[HarmonyPatch(typeof(BaseAddWallFoundationGhost), nameof(BaseAddWallFoundationGhost.UpdatePlacement))]
		public static class BaseAddWallFoundationGhost_UpdatePlacement__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase, ref bool __result, BaseAddWallFoundationGhost __instance)
			{
				positionFound = false;
				geometryChanged = false;
				__instance.UpdateRotation(ref geometryChanged);
				ConstructableBase componentInParent = __instance.GetComponentInParent<ConstructableBase>();
				float num = componentInParent.placeDefaultDistance;
				Base.CellType cellType = __instance.GetCellType();
				Int3 v = Base.CellSize[(int)cellType];
				Int3 v2 = default(Int3);
				switch (cellType)
				{
					case Base.CellType.WallFoundationN:
						v2 = new Int3(0, 0, 0);
						break;
					case Base.CellType.WallFoundationW:
						v2 = new Int3(0, 0, -1);
						break;
					case Base.CellType.WallFoundationS:
						v2 = new Int3(0, 0, -1);
						break;
					case Base.CellType.WallFoundationE:
						v2 = new Int3(1, 0, -1);
						break;
				}
				Vector3 direction = Vector3.Scale(new Vector3(0.5f, 0f, -0.5f), Base.cellSize);
				Vector3 position = camera.position;
				Vector3 forward = camera.forward;
				__instance.targetBase = BaseGhost.FindBase(camera, 20f);
				bool flag;
				if (__instance.targetBase != null)
				{
					positionFound = true;
					flag = true;
					Vector3 a = position + forward * num;
					Vector3 b = __instance.targetBase.transform.TransformDirection(direction);
					Int3 @int = __instance.targetBase.WorldToGrid(a - b);
					@int += v2;
					Int3 int2 = @int + v - 1;
					Int3.Bounds bounds = new Int3.Bounds(@int, int2);
					foreach (Int3 cell in bounds)
					{
						if (__instance.targetBase.GetCell(cell) != Base.CellType.Empty || __instance.targetBase.IsCellUnderConstruction(cell))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						Int3.Bounds bounds2 = __instance.targetBase.Bounds;
						int y = bounds2.mins.y;
						int y2 = bounds2.maxs.y;
						Int3.Bounds bounds3 = new Int3.Bounds(@int, new Int3(int2.x, @int.y, int2.z));

						foreach (Int3 cell2 in bounds3)
						{
							for (int i = @int.y - 1; i >= y; i--)
							{

								Int3 test = cell2;
								test.y = i;
								//cell2.y = i;
								if (__instance.targetBase.IsCellUnderConstruction(test) || __instance.targetBase.GetCell(test) != Base.CellType.Empty)
								{
									flag = false;
									break;
								}
							}
							if (!flag)
							{
								break;
							}
							for (int j = int2.y + 1; j <= y2; j++)
							{
								Int3 test = cell2;
								test.y = j;
								Base.CellType cell3 = __instance.targetBase.GetCell(test);
								if (__instance.targetBase.IsCellUnderConstruction(test) || cell3 == Base.CellType.Foundation || cell3 == Base.CellType.WallFoundationN || cell3 == Base.CellType.WallFoundationW || cell3 == Base.CellType.WallFoundationS || cell3 == Base.CellType.WallFoundationE || cell3 == Base.CellType.Moonpool || cell3 == Base.CellType.MoonpoolRotated)
								{
									flag = false;
									break;
								}
								if (j == int2.y + 1 && (cell3 == Base.CellType.Observatory || cell3 == Base.CellType.MapRoom || cell3 == Base.CellType.MapRoomRotated))
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
					}
					if (__instance.targetOffset != @int)
					{
						__instance.targetOffset = @int;
						__instance.ghostBase.SetWallFoundation(Int3.zero, cellType);
						__instance.RebuildGhostGeometry(true);
						geometryChanged = true;
					}
					componentInParent.transform.position = __instance.targetBase.GridToWorld(@int);
					componentInParent.transform.rotation = __instance.targetBase.transform.rotation;
				}
				else
				{
					Bounds aaBounds = Builder.aaBounds;
					num = BaseGhost.GetStartingDistance() + Mathf.Max(aaBounds.extents.x, aaBounds.extents.z);
					aaBounds.extents = 1.05f * aaBounds.extents;
					Vector3 b2 = componentInParent.transform.TransformDirection(aaBounds.center);
					Vector3 a2;
					flag = __instance.PlaceWithForwardCast(position, forward, aaBounds.extents, num, out a2);
					componentInParent.transform.position = a2 - b2;
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
