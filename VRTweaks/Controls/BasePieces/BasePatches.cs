using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRTweaks.Controls.BasePieces
{
	class BasePatches
	{
		[HarmonyPatch(typeof(BaseGhost), nameof(BaseGhost.GetStartingDistance))]
		public static class BaseGhost_GetStartingDistance__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(ref float __result)
			{
				float t = Mathf.Clamp01(Vector3.Dot(VRHandsController.rightController.transform.right, Vector3.up));
				__result = Mathf.Lerp(12f, 3f, t);
				return false;
			}
		}
		
		[HarmonyPatch(typeof(Base), nameof(Base.PickFace))]
		public static class Base_PickFace__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform camera, out Base.Face face, ref bool __result, Base __instance)
			{
				//Ray ray = new Ray(camera.position, camera.right);
				Ray ray = __instance.WorldToLocalRay(new Ray(camera.position, camera.right));
				__result = __instance.RaycastFace(ray, out face);
				return false;
			}
		}
		
		[HarmonyPatch(typeof(Base), nameof(Base.PickCell))]
		public static class Base_PickCell__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Transform camera, Vector3 point, Int3 size, ref Int3 __result, Base __instance)
			{
				Ray ray = __instance.WorldToLocalRay(new Ray(camera.position, camera.right));
				//Ray ray = new Ray(camera.position, camera.right);
				Int3 @int = size - 1;
				Vector3 b = Int3.Scale(@int, Base.halfCellSize);
				Int3 int2 = new Int3(@int.x / 2, @int.y / 2, @int.z / 2);
				Int3 int3 = @int - int2 * 2;
				Base.Face face;
				Int3 int4;
				if (!__instance.RaycastFace(ray, out face))
				{
					Vector3 a = __instance.transform.InverseTransformPoint(point);
					int4 = __instance.LocalToGrid(a - b);
				}
				else
				{
					int4 = face.cell - int2;
					Plane facePlane = __instance.GetFacePlane(face);
					float d;
					if (facePlane.Raycast(ray, out d))
					{
						Vector3 vector = ray.origin + ray.direction * d;
						__instance.DrawLocalStar(vector, Color.red);
						Vector3 vector2 = vector - __instance.GetFaceLocalCenter(face);
						__instance.DrawLocalStar(__instance.GetFaceLocalCenter(face), Color.green);
						if (int3.x > 0 && facePlane.normal.x == 0f && vector2.x < 0f)
						{
							int4.x--;
						}
						if (int3.y > 0 && facePlane.normal.y == 0f && vector2.y < 0f)
						{
							int4.y--;
						}
						if (int3.z > 0 && facePlane.normal.z == 0f && vector2.z < 0f)
						{
							int4.z--;
						}
						__instance.DrawLocalStar(__instance.GridToLocal(int4), Color.yellow);
					}
				}
				__result = int4;
				return false;
			}
		}
		
	}
}
