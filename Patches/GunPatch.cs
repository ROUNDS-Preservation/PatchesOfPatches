using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

// Willuwontu and TeamDK

namespace PatchOfPatches.Patches
{
    [HarmonyPatch(typeof(Gun))]
    class Gun_Patch
    {
        // Prefix patch is run before the original method is run.
        // Type Bool so that we can tell Harmony to skip the original method if we set anything.
        //   bulletID, numOfProj, charge are all parameters of the original method and used for calculations.
        //   __instance grabs the triggering Gun instance.
        //   ___forceShootDir grabs the forcShootDir field of our triggering instance.
        //   __result is the returned result of the method, AKA how we modify what value is returned.
        [HarmonyPrefix]
        [HarmonyPatch("getShootRotation")] // Patching the getShootRotation method in the gun object.
        static bool EvenSpread(Gun __instance, int bulletID, int numOfProj, float charge, Vector3 ___forceShootDir, ref Quaternion __result)
        {
            if ((__instance.spread != 0.0f) && (__instance.evenSpread != 0.0f)) // If the gun has spread and there's an even spread factor.
            {
                Vector3 vector = __instance.shootPosition.forward;
                if (___forceShootDir != Vector3.zero)
                {
                    vector = ___forceShootDir;
                }
                float d = __instance.multiplySpread * Mathf.Clamp(1f + charge * __instance.chargeSpreadTo, 0f, float.PositiveInfinity);
                float num = UnityEngine.Random.Range(-__instance.spread, __instance.spread);
                num /= (1f + __instance.projectileSpeed * 0.5f) * 0.5f;
                // New Code start

                // Direction the bullet would point in, if the shots were spread evenly.
                float even = bulletID * ((__instance.spread * 2) / (numOfProj - 1)) - __instance.spread;
                // Modify by the same factor that spread is modified by
                even /= (1f + __instance.projectileSpeed * 0.5f) * 0.5f;
                // Use evenness factor to determine how much we align with the random vs even spread.
                num = even + (1.0f - Mathf.Clamp(__instance.evenSpread * (1f + __instance.chargeEvenSpreadTo * charge), 0.0f, 1.0f)) * (num - even);

                // New Code end
                vector += Vector3.Cross(vector, Vector3.forward) * num * d;
                __result = Quaternion.LookRotation(__instance.lockGunToDefault ? __instance.shootPosition.forward : vector);
                return false; // Skip the original method.
            }
            return true; // Run the original method.
        }
    }
    [Serializable]
    public class GunAdditionalData
    {
        public Boolean arcTrajectoryRotationalCompensationDisabled;

        public GunAdditionalData()
        {
            arcTrajectoryRotationalCompensationDisabled = false;
        }
    }
    public static class GunExtension
    {
        public static readonly ConditionalWeakTable<Gun, GunAdditionalData> data =
            new ConditionalWeakTable<Gun, GunAdditionalData>();

        public static GunAdditionalData GetAdditionalData(this Gun gun)
        {
            return data.GetOrCreateValue(gun);
        }

        public static void AddData(this Gun gun, GunAdditionalData value)
        {
            try
            {
                data.Add(gun, value);
            }
            catch (Exception) { }
        }
    }
    // reset extra gun attributes when resetstats is called
    [HarmonyPatch(typeof(Gun), "ResetStats")]
    class GunPatchResetStats
    {
        private static void Prefix(Gun __instance)
        {
            __instance.GetAdditionalData().arcTrajectoryRotationalCompensationDisabled = false;
        }
    }
}
