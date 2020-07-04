using HarmonyLib;
using PulsarPluginLoader.Patches;
using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace PulsarPluginLoader.Chat.Commands
{
    [HarmonyPatch(typeof(PLNetworkManager), "ProcessCurrentChatText")]
    class ChatInputPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(PLNetworkManager), "IsTyping")),
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PLInGameUI), "Instance")),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(PLInGameUI), "ForceChatTextAsDirty")),
            };

            Label afterIf = generator.DefineLabel();
            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ChatCommandRouter), "get_Instance")),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PLNetworkManager), "CurrentChatText")),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ChatCommandRouter), "FindAndExecute", null, null)),
                new CodeInstruction(OpCodes.Brtrue, afterIf),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(String), "Empty")),
                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(PLNetworkManager), "CurrentChatText")),
                new CodeInstruction(OpCodes.Nop),
            };
            injectedSequence[injectedSequence.Count - 1].labels.Add(afterIf);

            instructions = HarmonyHelpers.PatchBySequence(instructions, targetSequence, injectedSequence);

            int index = -1;
            List<CodeInstruction> instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; i++)
            {
                if ("TeamMessage".Equals(instructionList[i].operand))
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                Label afterMessage = generator.DefineLabel();
                instructionList[index + 17].labels.Add(afterMessage);
                instructionList[index - 2].opcode = OpCodes.Ldarg_0;
                instructionList[index - 2].operand = null;
                List<CodeInstruction> toInsert = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PLNetworkManager), "CurrentChatText")),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ChatInputPatch), "MessageIsCommand")),
                    new CodeInstruction(OpCodes.Brtrue, afterMessage),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PLServer), "Instance"))
                };
                instructionList.InsertRange(index - 1, toInsert);
            }
            else
            {
                Logger.Info("Warning: \"TeamMessage\" not found. All commands will be displayed in chat.");
            }

            return instructionList;
        }

        public static bool MessageIsCommand(string message)
        {
            if (message.StartsWith("/"))
            {
                Messaging.Notification($"Command not found: {message.Split(' ')[0]}");
                return true;
            }
            return false;
        }
    }
}
