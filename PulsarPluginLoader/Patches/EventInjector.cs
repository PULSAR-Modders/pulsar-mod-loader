using PulsarPluginLoader.Events;
using PulsarPluginLoader.Utilities;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using static System.Reflection.Emit.OpCodes;

namespace PulsarPluginLoader.Patches
{
    [HarmonyPatch(typeof(PLServer))]
    internal class EventInjector
    {
        [HarmonyTranspiler]
        [HarmonyPatch("AddPlayer")]
        private static IEnumerable<CodeInstruction> PatchPLServer_AddPlayer(IEnumerable<CodeInstruction> instructions)
        {
            return HarmonyHelpers.PatchBySequence(instructions,
                new CodeInstruction[] // target
                {
                    new CodeInstruction(Ldarg_1),
                    new CodeInstruction(Callvirt, AccessTools.Method(typeof(PLPlayer), "ResetTalentPoints")) // arg1.ResetTalentPoints();
                },
                new CodeInstruction[] // new
                {
                    new CodeInstruction(Ldarg_1),
                    new CodeInstruction(Call, AccessTools.Method(typeof(EventHelper), "OnPlayerAdded")) // EventHelper.OnPlayerAdded(arg1);
                }, HarmonyHelpers.PatchMode.BEFORE);
        }

        [HarmonyTranspiler]
        [HarmonyPatch("RemovePlayer")]
        private static IEnumerable<CodeInstruction> PatchPLServer_RemovePlayer(IEnumerable<CodeInstruction> instructions)
        {
            return HarmonyHelpers.PatchBySequence(instructions,
                new CodeInstruction[] // target
                {
                    new CodeInstruction(Ldarg_0),
                    new CodeInstruction(Call, AccessTools.Method(typeof(Photon.MonoBehaviour), "get_photonView")) // base.photonView...
                },
                new CodeInstruction[] // new
                {
                    new CodeInstruction(Ldarg_1),
                    new CodeInstruction(Call, AccessTools.Method(typeof(EventHelper), "OnPlayerRemoved")) // EventHelper.OnPlayerRemoved(arg1);
                }, HarmonyHelpers.PatchMode.BEFORE);
        }
    }
}
