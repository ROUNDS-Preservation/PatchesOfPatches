using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

// Root

namespace PatchOfPatches.Patches
{
    [HarmonyPatch(typeof(CharacterStatModifiers), "ResetStats")]
    public class Patch
    {
        // Token: 0x06000006 RID: 6 RVA: 0x00002094 File Offset: 0x00000294
        private static void Prefix(CharacterStatModifiers __instance)
        {
            HealthHandler component = __instance.gameObject.GetComponent<HealthHandler>();
            bool flag = component != null;
            if (flag)
            {
                component.regeneration = 0f;
            }
        }
    }
}
