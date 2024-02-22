using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

// Pykess and Root

namespace PatchOfPatches.Patches
{
    [HarmonyPatch(typeof(Player), "FullReset")]
    class PlayerPatchFullReset
    {
        private static void Prefix(Player __instance)
        {
            __instance.GetComponent<Holding>().holdable.GetComponent<Gun>().dontAllowAutoFire = false;
            Gravity component = __instance.gameObject.GetComponent<Gravity>();
            bool flag = component != null;
            if (flag)
            {
                component.gravityForce = 1000f;
                component.exponent = 1.5f;
            }
        }
    }
}
