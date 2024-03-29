﻿using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

// Pykess

namespace PatchOfPatches.Patches
{
    // fix the erroneous logic in PlayerJump.Jump by completely replacing it. this is a terrible way of doing this.
    [HarmonyPatch(typeof(PlayerJump), "Jump")]
    class PlayerJumpPatchJump
    {
        private static bool Prefix(PlayerJump __instance, bool forceJump = false, float multiplier = 1f)
        {
            // read private/internal variables
            CharacterData data = (CharacterData)Traverse.Create(__instance).Field("data").GetValue();
            CharacterStatModifiers stats = (CharacterStatModifiers)Traverse.Create(__instance).Field("stats").GetValue();

            if (!forceJump)
            {
                if (data.sinceJump < 0.1f)
                {
                    return false;
                }
                if (data.currentJumps <= 0 && data.sinceWallGrab > 0.1f)
                {
                    return false;
                }
            }
            Vector3 a = Vector3.up;
            Vector3 vector = data.groundPos;
            if (__instance.JumpAction != null)
            {
                __instance.JumpAction();
            }
            bool flag = false;
            if (data.sinceWallGrab < 0.1f && !data.isGrounded)
            {
                a = Vector2.up * 0.8f + data.wallNormal * 0.4f;
                vector = data.wallPos;
                data.currentJumps = data.jumps;
                flag = true;
            }
            else
            {
                // another fix, if the player is not grounded at all (regardless of how long its been), then the particles should be at the player
                //if (data.sinceGrounded > 0.05f)
                if (!data.isGrounded)
                {
                    vector = __instance.transform.position;
                }
                // this is the error in the original method, so I've just commented it out
                //data.currentJumps = data.jumps;
            }
            // read more private/internal fields
            Vector2 velocity = (Vector2)Traverse.Create(data.playerVel).Field("velocity").GetValue();
            if (velocity.y < 0f)
            {
                // assign new velocity which is an internal field
                Traverse.Create(data.playerVel).Field("velocity").SetValue(new Vector2(velocity.x, 0f));
            }
            data.sinceGrounded = 0f;
            data.sinceJump = 0f;
            data.isGrounded = false;
            data.isWallGrab = false;
            data.currentJumps--;
            // read another private/internal field
            float mass = (float)Traverse.Create(data.playerVel).Field("mass").GetValue();
            // call private/internal method
            typeof(PlayerVelocity).InvokeMember("AddForce", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, data.playerVel, new object[] { a * multiplier * 0.01f * data.stats.jump * mass * (1f - stats.GetSlow()) * __instance.upForce, ForceMode2D.Impulse });
            if (!flag)
            {
                typeof(PlayerVelocity).InvokeMember("AddForce", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, data.playerVel, new object[] { Vector2.right * multiplier * __instance.sideForce * 0.01f * data.stats.jump * mass * (1f - stats.GetSlow()) * velocity.x, ForceMode2D.Impulse });
            }
            for (int i = 0; i < __instance.jumpPart.Length; i++)
            {
                __instance.jumpPart[i].transform.position = new Vector3(vector.x, vector.y, 5f) - a * 0f;
                __instance.jumpPart[i].transform.rotation = Quaternion.LookRotation(velocity);
                __instance.jumpPart[i].Play();
            }
            return false; // do not run the base function or any postfixes.
        }


    }

    // postfix CharacterStatModifiers ResetStats to make sure that the number of jumps is reset properly
    [HarmonyPatch(typeof(CharacterStatModifiers), "ResetStats")]
    class CharacterStatModifiersPatchResetStats
    {
        private static void Postfix(CharacterStatModifiers __instance)
        {
            ((CharacterData)Traverse.Create(__instance).Field("data").GetValue()).jumps = 1;

        }
    }
}
