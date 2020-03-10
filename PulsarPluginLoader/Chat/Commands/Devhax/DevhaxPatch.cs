using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarPluginLoader.Patches;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using static PulsarPluginLoader.Patches.HarmonyHelpers;

namespace PulsarPluginLoader.Chat.Commands.Devhax
{
    [HarmonyPatch(typeof(PLNetworkManager), "Update")]
    class DevhaxPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PLNetworkManager), "VersionString")),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Object), "ToString")),
                new CodeInstruction(OpCodes.Ldstr, "i"),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(String), "Contains")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObscuredBool), "op_Implicit", new Type[]{ typeof(bool) })),
                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(PLNetworkManager), "IsInternalBuild")),
            };

            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(DevhaxCommand), "IsEnabled")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObscuredBool), "op_Implicit", new Type[]{ typeof(bool) })),
                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(PLNetworkManager), "IsInternalBuild")),
            };

            return HarmonyHelpers.PatchBySequence(instructions, targetSequence, injectedSequence, PatchMode.REPLACE);
        }
    }
}
