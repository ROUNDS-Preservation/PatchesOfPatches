using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnboundLib;

// Pykess

namespace PatchOfPatches.Patches
{
    [Serializable]
    public class ToggleStatsAdditionalData
    {
        public float maxhealth_delta;
        public float movementSpeed_delta;


        public ToggleStatsAdditionalData()
        {
            maxhealth_delta = 0f;
            movementSpeed_delta = 0f;
        }
    }
    public static class ToggleStatsExtension
    {
        public static readonly ConditionalWeakTable<ToggleStats, ToggleStatsAdditionalData> data =
            new ConditionalWeakTable<ToggleStats, ToggleStatsAdditionalData>();

        public static ToggleStatsAdditionalData GetAdditionalData(this ToggleStats toggleStats)
        {
            return data.GetOrCreateValue(toggleStats);
        }

        public static void AddData(this ToggleStats toggleStats, ToggleStatsAdditionalData value)
        {
            try
            {
                data.Add(toggleStats, value);
            }
            catch (Exception) { }
        }
    }

    [Serializable]
    [HarmonyPatch(typeof(ToggleStats), "TurnOn")]
    class ToggleStatsPatchTurnOn
    {
        // patch for ToggleStats.TurnOn
        private static bool Prefix(ToggleStats __instance)
        {
            CharacterData data = (CharacterData)Traverse.Create(__instance).Field("data").GetValue();

            // save deltas
            __instance.GetAdditionalData().maxhealth_delta = (float)data.GetFieldValue("m_maxHealth") * __instance.hpMultiplier - (float)data.GetFieldValue("m_maxHealth");
            __instance.GetAdditionalData().movementSpeed_delta = data.stats.movementSpeed * __instance.movementSpeedMultiplier - data.stats.movementSpeed;

            // apply deltas
            data.health += data.health * __instance.hpMultiplier - data.health;
            data.SetFieldValue("m_maxHealth", (float)data.GetFieldValue("m_maxHealth")+__instance.GetAdditionalData().maxhealth_delta);
            data.stats.movementSpeed += __instance.GetAdditionalData().movementSpeed_delta;

            // update player stuff
            typeof(CharacterStatModifiers).InvokeMember("ConfigureMassAndSize",
                        BindingFlags.Instance | BindingFlags.InvokeMethod |
                        BindingFlags.NonPublic, null, data.stats, new object[] { });

            return false; // skip original method (BAD IDEA)

        }
    }

    [Serializable]
    [HarmonyPatch(typeof(ToggleStats), "TurnOff")]
    class ToggleStatsPatchTurnOff
    {
        // patch for ToggleStats.TurnOff
        private static bool Prefix(ToggleStats __instance)
        {
            CharacterData data = (CharacterData)Traverse.Create(__instance).Field("data").GetValue();


            //undoHealth
            var a = data.health;
            var b = (float)data.GetFieldValue("m_maxHealth");
            var c = (float)data.GetFieldValue("m_maxHealth") - __instance.GetAdditionalData().maxhealth_delta;
            data.health = (a * c) / b;

            // unapply deltas
            data.SetFieldValue("m_maxHealth", (float)data.GetFieldValue("m_maxHealth")-__instance.GetAdditionalData().maxhealth_delta);
            data.stats.movementSpeed -= __instance.GetAdditionalData().movementSpeed_delta;

            // reset deltas
            __instance.GetAdditionalData().maxhealth_delta = 0f;
            __instance.GetAdditionalData().movementSpeed_delta = 0f;

            // update player stuff
            typeof(CharacterStatModifiers).InvokeMember("ConfigureMassAndSize",
                        BindingFlags.Instance | BindingFlags.InvokeMethod |
                        BindingFlags.NonPublic, null, data.stats, new object[] { });

            return false; // skip original method (BAD IDEA)

        }
    }
}
