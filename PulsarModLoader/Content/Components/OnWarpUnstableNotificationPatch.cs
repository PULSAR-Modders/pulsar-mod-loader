using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using static PulsarModLoader.Patches.HarmonyHelpers;

namespace PulsarModLoader.Content.Components
{
    /* [HarmonyPatch()] // ???
    class OnWarpUnstableNotificationPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Sub),
                new CodeInstruction(OpCodes.Call),
            };

            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OnWarpUnstableNotificationPatch), "PatchMethod"))
            };

            return PatchBySequence(instructions, targetSequence, injectedSequence, checkMode: CheckMode.NEVER);
        }
        static void PatchMethod(PLShipComponent InComp)
        {
            PulsarModLoader.Utilities.Messaging.Notification(InComp.GetItemName(true) + " has degraded to Level " + (InComp.Level + 1));
        }
    } */
}
