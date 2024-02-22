using HarmonyLib;
using ModdingUtils.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// TeamDK

namespace PatchOfPatches.Patches
{
    [Serializable]
    [HarmonyPatch(typeof(GeneralInput), "Update")]
    class GeneralInputArcTrajectoryCompensationPatch
    {
        // remove arc compensation (makes gun shoot slightly above target reticle, causes misses at zero gravity/other straight line mechanics)
        private static void Postfix(GeneralInput __instance)
        {
            var gun = ((CharacterData)Traverse.Create(__instance).Field("data").GetValue()).weaponHandler.gun;
            if (gun.gravity == 0 || gun.GetAdditionalData().arcTrajectoryRotationalCompensationDisabled)
            {
                __instance.aimDirection -= Vector3.up * 0.13f / Mathf.Clamp(((CharacterData)Traverse.Create(__instance).Field("data").GetValue()).weaponHandler.gun.projectileSpeed, 1f, 100f);
            }
        }
    }
}
