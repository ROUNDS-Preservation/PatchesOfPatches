using static System.Reflection.Emit.OpCodes;
using static HarmonyLib.AccessTools;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;

namespace PatchOfPatches.Patches 
{
    [HarmonyPatch(typeof(HealthHandler), nameof(HealthHandler.DoDamage))]
    public class RespawnPatch 
    {
        // Adds a check to only call the death functions from one client. that way extra lives are only decrmented once per death.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) 
        {
            List<CodeInstruction> code = instructions.ToList();
            List<CodeInstruction> newCode = new List<CodeInstruction>();
            List<CodeInstruction> injectedCode = new List<CodeInstruction>() // this.data.view.IsMine
            {
                new CodeInstruction(Ldarg_0), 
                new CodeInstruction(Ldfld, Field(typeof(HealthHandler), "data")),
                new CodeInstruction(Ldfld, Field(typeof(CharacterData), nameof(CharacterData.view))),
                new CodeInstruction(Call, PropertyGetter(typeof(PhotonView),nameof(PhotonView.IsMine)))
            };

            for(int i  = 0; i < code.Count; i++) 
            {
                newCode.Add(code[i]);
                if(code[i].opcode == Bge_Un_S &&
                    code[i-1].opcode == Ldc_R4 &&
                    code[i-2].opcode == Ldfld && code[i-2].operand.ToString().Contains("health") &&
                    code[i-3].opcode == Ldfld && code[1-3].operand.ToString().Contains("data") &&
                    code[i-4].opcode == Ldarg_0) 
                {
                    newCode.AddRange(injectedCode);
                    newCode.Add(new CodeInstruction(Brfalse, code[i].operand)); // view is not mine, exit if.
                }
            }

            return newCode;
        }
    }
}
