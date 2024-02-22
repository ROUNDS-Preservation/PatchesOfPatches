using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using System.Reflection.Emit;

// Pykess and Willuwontu

namespace PatchOfPatches.Patches
{
    [Serializable]
    [HarmonyPatch(typeof(HealthHandler), "CallTakeDamage")]
    class HealthHandlerPatch
    {
        // fix bug in base game method that caused CallTakeDamage to ignore Gun.unblockable
        private static void Postfix(HealthHandler __instance, Vector2 damage, Vector2 position, GameObject damagingWeapon, Player damagingPlayer, bool lethal)
        {
            CharacterData data = (CharacterData)Traverse.Create(__instance).Field("data").GetValue();
            // check if the original method skipped sending damage
            if (data.block.IsBlocking())
            {
                if (damagingPlayer != null && damagingPlayer.GetComponent<Holding>().holdable.GetComponent<Gun>() != null && damagingPlayer.GetComponent<Holding>().holdable.GetComponent<Gun>().unblockable)
                {
                    // if the opponent's gun had the unblockable tag, then this needs to call SendTakeDamage
                    data.view.RPC("RPCA_SendTakeDamage", RpcTarget.All, new object[]
                    {
                        damage,
                        position,
                        lethal,
                        (damagingPlayer != null) ? damagingPlayer.playerID : -1
                    });
                }
            }
        }
    }
    [Serializable]
    [HarmonyPatch(typeof(HealthHandler), "DoDamage")]
    class HealtHandlerPatchDoDamage
    {
        // patch for gun.unblockable
        private static void Prefix(HealthHandler __instance, Vector2 damage, Vector2 position, Color blinkColor, GameObject damagingWeapon, Player damagingPlayer, bool healthRemoval, bool lethal, ref bool ignoreBlock)
        {

            CharacterData data = (CharacterData)Traverse.Create(__instance).Field("data").GetValue();
            Player player = data.player;
            if (!data.isPlaying)
            {
                return;
            }
            if (data.dead)
            {
                return;
            }
            if (__instance.isRespawning)
            {
                return;
            }

            if (damagingPlayer != null && damagingPlayer.GetComponent<Holding>().holdable.GetComponent<Gun>() != null && damagingPlayer.GetComponent<Holding>().holdable.GetComponent<Gun>().unblockable)
            {
                ignoreBlock = true;
            }
        }
    }
    [HarmonyPatch(typeof(HealthHandler), "RPCA_Die_Phoenix")]
    class Respawn_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var codesToInsert = new List<CodeInstruction>();
            /* Replace
             *      if (this.data.dead)
             * with
             *      ReduceRespawns(data.player.PlayerID);
             */
            //var player = AccessTools.Field(typeof(HealthHandler), "player");
            //codes.RemoveRange(11, 7);
            //codes.InsertRange(11, codesToInsert);
            //codes[10].opcode = OpCodes.Nop;
            //codes.RemoveRange(11, 7);

            var reduceRespawns = AccessTools.Method(typeof(PatchOfPatches), nameof(PatchOfPatches.IsAllowedToRunRespawn), new Type[] { typeof(CharacterData) });

            //for (var i = 0; i < codes.Count; i++)
            //{
            //    UnityEngine.Debug.Log($"{i}: {codes[i].opcode}, {codes[i].operand}");
            //}

            //codes.RemoveRange(13, 17);
            codes[7] = new CodeInstruction(OpCodes.Call, reduceRespawns);

            //for (var i = 0; i < codes.Count; i++)
            //{
            //    UnityEngine.Debug.Log($"{i}: {codes[i].opcode}, {codes[i].operand}");
            //}

            return codes.AsEnumerable();
        }
    }
}
