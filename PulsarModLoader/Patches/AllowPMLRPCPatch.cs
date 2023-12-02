using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace PulsarModLoader.Patches
{
    [HarmonyPatch(typeof(NetworkingPeer), "ExecuteRpc")]
    class AllowPMLRPCPatch
    {
        static bool PatchMethod(bool ShouldContinue, string MethodName)
        {
            if (MethodName == "ReceiveMessage" || MethodName == "ClientRecieveModList" || MethodName == "ServerRecieveModList" || MethodName == "ClientRequestModList")
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
                new CodeInstruction(OpCodes.Brfalse_S),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Stloc_S),
                new CodeInstruction(OpCodes.Ldloc_S),
            };

            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AllowPMLRPCPatch), "PatchMethod")),
            };
            return HarmonyHelpers.PatchBySequence(instructions, targetSequence, injectedSequence, checkMode: HarmonyHelpers.CheckMode.NEVER);
        }
    }
}
