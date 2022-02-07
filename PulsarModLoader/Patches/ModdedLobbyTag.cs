using System.Collections.Generic;
using static System.Reflection.Emit.OpCodes;
using HarmonyLib;
using static PulsarModLoader.Patches.HarmonyHelpers;

namespace PulsarModLoader.Patches
{
    [HarmonyPatch(typeof(PLUIPlayMenu), "Update")]
    class ModdedLobbyTag
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(Ldfld),
                new CodeInstruction(Ldsfld),
                new CodeInstruction(Ldloc_S),
                new CodeInstruction(Callvirt),
                new CodeInstruction(Callvirt, AccessTools.Method(typeof(PLReadableStringManager), "GetFormattedResultFromInputString")),
                new CodeInstruction(Callvirt, AccessTools.Method(typeof(UnityEngine.UI.Text), "set_text"))
            };

            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(Call, AccessTools.Method(typeof(ModdedLobbyTag), "PatchMethod"))
            };
            return PatchBySequence(instructions, targetSequence, injectedSequence, patchMode: PatchMode.REPLACE, checkMode: CheckMode.NONNULL);
        }
        static void PatchMethod(PLUIPlayMenu.UIJoinGameElement jge)
        {
            if (jge == null)
                return;

            string formattedResultInputString = PLReadableStringManager.Instance.GetFormattedResultFromInputString(jge.Room.Name);

            if (jge.Room.CustomProperties.TryGetValue("isModded", out object _))
            {
                jge.GameName.text = "<size=20><color=yellow>M</color></size> " + formattedResultInputString;
            }
            else jge.GameName.text = formattedResultInputString;
        }
    }
}
