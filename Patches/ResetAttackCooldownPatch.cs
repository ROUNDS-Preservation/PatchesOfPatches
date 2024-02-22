using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

// Senyksia

namespace PatchOfPatches.Patches
{
    [HarmonyPatch(typeof(PlayerManager), "PlayerDied")]
    internal class PlayerManagerPatchPlayerDied
    {
        private static void Postfix(PlayerManager __instance, Player player)
        {
            Gun gun = player.GetComponent<Holding>().holdable.GetComponent<Gun>();
            gun.sinceAttack = gun.attackSpeed + 1f; // Reset attack cooldown (see Gun.IsReady())
        }
    }

    // On round start
    [HarmonyPatch(typeof(PlayerManager), "RevivePlayers")]
    internal class PlayerManagerPatchRevivePlayers
    {
        private static void Prefix(PlayerManager __instance)
        {
            for (int i = 0; i < __instance.players.Count; i++) // Loop through all players
            {
                Gun gun = __instance.players[i].GetComponent<Holding>().holdable.GetComponent<Gun>();
                gun.sinceAttack = gun.attackSpeed + 1f; // Reset attack cooldown (see Gun.IsReady())
            }
        }
    }
}
