using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using static PulsarModLoader.Patches.HarmonyHelpers;

namespace PulsarModLoader.Patches
{
    [HarmonyPatch(typeof(PLLevelSync), "LateUpdate")] //free the cursor when GUI is active
    class FreeCursor
    {
        static bool PatchMethod()
        {
            return CustomGUI.GUIMain.Instance.ShouldUnlockCursor() || PLNetworkManager.Instance.CurrentGame.ShouldShowClassSelectionScreen();
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldsfld),
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Callvirt),
            };

            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FreeCursor), "PatchMethod"))
            };

            return PatchBySequence(instructions, targetSequence, injectedSequence, PatchMode.REPLACE, CheckMode.NONNULL);
        }
    }
    [HarmonyPatch(typeof(PLMouseLook), "Update")] //Keep the mouselook locked when GUI is active
    class LockMouselook
    {
        static bool PatchMethod()
        {
            if (CustomGUI.GUIMain.Instance == null) return false;

            return !PLInput.Instance.GetButton(PLInputBase.EInputActionName.unlock_mouse) && CustomGUI.GUIMain.Instance.ShouldUnlockCursor();
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldsfld),
                new CodeInstruction(OpCodes.Ldc_I4_S),
                new CodeInstruction(OpCodes.Callvirt),
            };

            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LockMouselook), "PatchMethod"))
            };

            return PatchBySequence(instructions, targetSequence, injectedSequence, PatchMode.REPLACE, CheckMode.NONNULL);
        }
    }
}
