using Harmony;
using PulsarPluginLoader.Patches;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace PulsarPluginLoader.Chat.Commands.Devhax.ToggleCam
{
    [HarmonyPatch(typeof(PLCameraMode_Scripted), "Tick")]
    class SpeedCam
    {
        public static readonly float MaxSpeed = 500f;

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PLCameraMode_Scripted), "Speed")),
                new CodeInstruction(OpCodes.Ldc_R4, 1f),
                new CodeInstruction(OpCodes.Ldc_R4, 5f),
                new CodeInstruction(OpCodes.Call,  AccessTools.Method(typeof(UnityEngine.Mathf), "Clamp", new System.Type[]{ typeof(float), typeof(float), typeof(float) })),
                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(PLCameraMode_Scripted), "Speed")),
            };

            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PLCameraMode_Scripted), "Speed")),
                new CodeInstruction(OpCodes.Ldc_R4, 1f),
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(SpeedCam), "MaxSpeed")),
                new CodeInstruction(OpCodes.Call,  AccessTools.Method(typeof(UnityEngine.Mathf), "Clamp", new System.Type[]{ typeof(float), typeof(float), typeof(float) })),
                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(PLCameraMode_Scripted), "Speed")),
            };

            return HarmonyHelpers.PatchBySequence(instructions, targetSequence, injectedSequence, HarmonyHelpers.PatchMode.REPLACE);
        }
    }
}
