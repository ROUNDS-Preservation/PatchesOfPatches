using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Pykess

namespace PatchOfPatches.Patches
{
    [Serializable]
    [HarmonyPatch(typeof(ProjectileHit), "RPCA_DoHit")]
    class ProjectileHitPatchRPCA_DotHit
    {
        // prefix to fix gun.unblockable
        private static void Prefix(ProjectileHit __instance, Vector2 hitPoint, Vector2 hitNormal, Vector2 vel, int viewID, int colliderID, ref bool wasBlocked)
        {
            if (__instance.ownPlayer != null && __instance.ownPlayer.GetComponent<Holding>().holdable.GetComponent<Gun>() != null && __instance.ownPlayer.GetComponent<Holding>().holdable.GetComponent<Gun>().unblockable)
            {
                wasBlocked = false;
            }
        }
    }
}
