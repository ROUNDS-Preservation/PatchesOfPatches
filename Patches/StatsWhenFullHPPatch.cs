using HarmonyLib;
using ModdingUtils.AIMinion.Extensions;
using Sonigon;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnboundLib;

// Pykess

namespace PatchOfPatches.Patches
{
    // ADD FIELDS TO STATSWHENFULLHP
    [Serializable]
    public class StatsWhenFullHPAdditionalData
    {
        public float maxhealth_delta;
        public float size_delta;


        public StatsWhenFullHPAdditionalData()
        {
            maxhealth_delta = 0f;
            size_delta = 0f;
        }
    }
    public static class StatsWhenFullHPExtension
    {
        public static readonly ConditionalWeakTable<StatsWhenFullHP, StatsWhenFullHPAdditionalData> data =
            new ConditionalWeakTable<StatsWhenFullHP, StatsWhenFullHPAdditionalData>();

        public static StatsWhenFullHPAdditionalData GetAdditionalData(this StatsWhenFullHP statsWhenFullHP)
        {
            return data.GetOrCreateValue(statsWhenFullHP);
        }

        public static void AddData(this StatsWhenFullHP statsWhenFullHP, StatsWhenFullHPAdditionalData value)
        {
            try
            {
                data.Add(statsWhenFullHP, value);
            }
            catch (Exception) { }
        }
    }

    [Serializable]
    [HarmonyPatch(typeof(StatsWhenFullHP), "Update")]
    class StatsWhenFullHPPatchUpdate
    {
        // patch for StatsWhenFullHP.Update
        private static bool Prefix(StatsWhenFullHP __instance)
        {
            CharacterData data = (CharacterData)Traverse.Create(__instance).Field("data").GetValue();

            bool flag = data.health / (float)data.GetFieldValue("m_maxHealth") >= __instance.healthThreshold;
            if (flag != (bool)Traverse.Create(__instance).Field("isOn").GetValue())
            {
                Traverse.Create(__instance).Field("isOn").SetValue(flag);
                if ((bool)Traverse.Create(__instance).Field("isOn").GetValue())
                {
                    if (__instance.playSound)
                    {
                        SoundManager.Instance.PlayAtPosition(__instance.soundPristineGrow, SoundManager.Instance.GetTransform(), __instance.transform);
                    }
                    // save deltas
                    __instance.GetAdditionalData().maxhealth_delta = (float)data.GetFieldValue("m_maxHealth") * __instance.healthMultiplier - (float)data.GetFieldValue("m_maxHealth");
                    __instance.GetAdditionalData().size_delta = data.stats.sizeMultiplier * __instance.sizeMultiplier - data.stats.sizeMultiplier;

                    // apply deltas
                    data.health += data.health * __instance.healthMultiplier - data.health;
                    data.SetFieldValue("m_maxHealth", (float)data.GetFieldValue("m_maxHealth")+__instance.GetAdditionalData().maxhealth_delta);
                    data.stats.sizeMultiplier += __instance.GetAdditionalData().size_delta;

                    // update player stuff
                    typeof(CharacterStatModifiers).InvokeMember("ConfigureMassAndSize",
                                BindingFlags.Instance | BindingFlags.InvokeMethod |
                                BindingFlags.NonPublic, null, data.stats, new object[] { });
                    return false; // skip original method (BAD IDEA)
                }
                if (__instance.playSound)
                {
                    SoundManager.Instance.PlayAtPosition(__instance.soundPristineShrink, SoundManager.Instance.GetTransform(), __instance.transform);
                }

                //undoHealth
                var a = data.health;
                var b = (float)data.GetFieldValue("m_maxHealth");
                var c = (float)data.GetFieldValue("m_maxHealth") - __instance.GetAdditionalData().maxhealth_delta;
                data.health = (a * c) / b;

                // unapply deltas
                data.SetFieldValue("m_maxHealth", (float)data.GetFieldValue("m_maxHealth")-__instance.GetAdditionalData().maxhealth_delta);
                data.stats.sizeMultiplier -= __instance.GetAdditionalData().size_delta;

                // reset deltas
                __instance.GetAdditionalData().maxhealth_delta = 0f;
                __instance.GetAdditionalData().size_delta = 0f;

                // update player stuff
                typeof(CharacterStatModifiers).InvokeMember("ConfigureMassAndSize",
                            BindingFlags.Instance | BindingFlags.InvokeMethod |
                            BindingFlags.NonPublic, null, data.stats, new object[] { });
            }
            return false; // skip original method (BAD IDEA)
        }
    }
}
