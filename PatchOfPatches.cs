using BepInEx; // requires BepInEx.dll and BepInEx.Harmony.dll
using UnityEngine; // requires UnityEngine.dll, UnityEngine.CoreModule.dll
using HarmonyLib; // requires 0Harmony.dll
using System;
using UnboundLib;
using UnboundLib.Cards;
using PatchOfPatches.Patches;


namespace PatchOfPatches
{
    [BepInPlugin(ModId, ModName, Version)]
    public class PatchOfPatches : BaseUnityPlugin
    {
        private const string ModId = "koala.rounds.patchpatchpatchpatchpatchpatchpatchpatchpatchpatchpatchpatchpatchpatchpatchpatchpatch";
        private const string ModName = "Patch of Patches";
        private const string Version = "0.0.0";

        private void Awake()
        {
            new Harmony(ModId).PatchAll();
        }

        

        private void Start()
        {

        }
        internal static bool IsAllowedToRunRespawn(CharacterData data)
        {
            if (data.healthHandler.isRespawning || data.dead)
            {
                return true;
            }

            return false;
        }
    }
}
