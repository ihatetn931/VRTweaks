using HarmonyLib;
using UnityEngine;

namespace VRTweaks.Controls.Tools
{
    class PlayerPda
    {
		[HarmonyPatch(typeof(PDAScanner), nameof(PDAScanner.UpdateTarget))]
		public static class PDAScanner_UpdateTarget__Patch
		{
			[HarmonyPrefix]
			static bool Prefix(float distance)
			{
				if (PDAScanner.isScanning && PDAScanner.scanTarget.gameObject)
				{
					if (!PDAScanner.scanTarget.gameObject.activeInHierarchy)
					{
						PDAScanner.scanTarget.Invalidate();
						return false;
					}
					if ((Player.main.gameObject.transform.position - PDAScanner.scanTarget.gameObject.transform.position).magnitude < 3.5f)
					{
						return false;
					}
				}
				PDAScanner.ScanTarget scanTarget = default(PDAScanner.ScanTarget);
				scanTarget.Invalidate();
				Targeting.AddToIgnoreList(Player.main.gameObject);
				GameObject candidate;
				float num;
				Target.GetTargets(distance, out candidate, out num);
				scanTarget.Initialize(candidate);
				if (PDAScanner.scanTarget.techType != scanTarget.techType || PDAScanner.scanTarget.gameObject != scanTarget.gameObject || PDAScanner.scanTarget.uid != scanTarget.uid)
				{
					float progress;
					if (scanTarget.hasUID && PDAScanner.cachedProgress.TryGetValue(scanTarget.uid, out progress))
					{
						scanTarget.progress = progress;
					}
					PDAScanner.scanTarget = scanTarget;
				}
				return false;
			}

		}
	}
}
