using HarmonyLib;
using PulsarModLoader.Patches;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace PulsarModLoader.ModMessages
{
    [HarmonyPatch(typeof(NetworkingPeer), "ExecuteRpc")]
    class AllowPMLRPCPatch
    {
        static bool PatchMethod(bool ShouldContinue, string MethodName)
        {
            if (MethodName == "ReceiveConnectionMessage" || MethodName == "ReceiveMessage")
            {
                return true;
            }
            else
            {
                return ShouldContinue;
            }
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Stloc_S, (byte)10),
                    new CodeInstruction(OpCodes.Ldloc_S, (byte)10),
                };

            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AllowPMLRPCPatch), "PatchMethod")),
                };
            return HarmonyHelpers.PatchBySequence(instructions, targetSequence, injectedSequence, patchMode: HarmonyHelpers.PatchMode.AFTER);
        }
    }
}
