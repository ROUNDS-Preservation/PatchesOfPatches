using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

// Pykess

namespace PatchOfPatches.Patches
{
    [HarmonyPatch(typeof(LegRaycasters), "FixedUpdate")]
    class LegRaycastersPatchFixedUpdate
    {
        private static bool Prefix(LegRaycasters __instance)
        {
            // reset totalStepTime which is a private field
            Traverse.Create(__instance).Field("totalStepTime").SetValue(0f);
            // read .legs which is a private field
            IkLeg[] legs = (IkLeg[])Traverse.Create(__instance).Field("legs").GetValue();
            for (int i = 0; i < legs.Length; i++)
            {
                if (!legs[i].footDown)
                {
                    // update totalStepTime which is a private field
                    Traverse.Create(__instance).Field("totalStepTime").SetValue((float)Traverse.Create(__instance).Field("totalStepTime").GetValue() + legs[i].stepTime);
                }
            }
            for (int j = 0; j < __instance.legCastPositions.Length; j++)
            {
                // this is the bug in the original game code, the distance the raycasters go is usually
                // distance = 1f * __instance.transform.root.localScale.x
                // this becomes too small to actually reach the ground if the player's size is too small (such as when they have stacked a few Glass Cannon cards)
                // so instead we will make sure the distance doesn't get smaller than a set value
                float distance = Math.Max(1f, 1f * __instance.transform.root.localScale.x);
                RaycastHit2D[] array = Physics2D.RaycastAll(__instance.legCastPositions[j].transform.position + Vector3.up * 0.5f, Vector2.down, distance, __instance.mask);
                for (int k = 0; k < array.Length; k++)
                {
                    if (array[k].transform && array[k].transform.root != __instance.transform.root)
                    {
                        // call HitGround which is a private method
                        typeof(LegRaycasters).InvokeMember("HitGround", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, __instance, new object[] { __instance.legCastPositions[j], array[k] });
                        break;
                    }
                }
            }

            return false; // do not run the original method or any postfixes

        }
    }
}
