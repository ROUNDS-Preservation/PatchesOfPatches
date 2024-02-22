using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;

// Willuwontu

namespace PatchOfPatches.Patches
{
    [HarmonyPatch]
    class BlockForce_Patch
    {

        static Type GetNestedIDoBlockTransitionType()
        {
            var nestedTypes = typeof(Block).GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic);
            Type nestedType = null;

            foreach (var type in nestedTypes)
            {
                if (type.Name.Contains("IDoBlock"))
                {
                    nestedType = type;
                    break;
                }
            }

            return nestedType;
        }

        static MethodBase TargetMethod()
        {
            return AccessTools.Method(GetNestedIDoBlockTransitionType(), "MoveNext");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (var i = 0; i < codes.Count; i++)
            {
                //UnityEngine.Debug.Log($"{i}: {codes[i].opcode}, {codes[i].operand}");
            }

            codes[131] = new CodeInstruction(OpCodes.Ldc_R4, 10f);
            codes[154] = new CodeInstruction(OpCodes.Ldc_R4, 10f);
            codes[136] = new CodeInstruction(OpCodes.Ldc_I4_1);
            codes[159] = new CodeInstruction(OpCodes.Ldc_I4_1);

            return codes.AsEnumerable();
        }
    }
}
