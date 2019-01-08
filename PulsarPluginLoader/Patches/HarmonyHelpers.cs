using Harmony;
using Harmony.ILCopying;
using PulsarPluginLoader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace PulsarPluginLoader.Patches
{
    public static class HarmonyHelpers
    {
        public static IEnumerable<CodeInstruction> PatchBySequence(IEnumerable<CodeInstruction> instructions, IEnumerable<CodeInstruction> targetSequence, IEnumerable<CodeInstruction> patchSequence, PatchMode patchMode = PatchMode.AFTER)
        {
            List<CodeInstruction> newInstructions = instructions.ToList();

            CodeInstruction targetStart = targetSequence.ElementAt(0);
            int targetSize = targetSequence.Count();

            for (int i = 0; i < newInstructions.Count; i++)
            {
                bool startsWithTargetInstruction = newInstructions[i].opcode == targetStart.opcode;
                bool targetSequenceStillFits = i + targetSize <= newInstructions.Count;

                if (startsWithTargetInstruction && targetSequenceStillFits)
                {
                    bool foundTargetSequence = true;

                    for (int x = 1; x < targetSize && foundTargetSequence; x++)
                    {
                        foundTargetSequence = newInstructions[i + x].opcode.Equals(targetSequence.ElementAt(x).opcode)
                            && (targetSequence.ElementAt(x).operand == null || newInstructions[i + x].operand.Equals(targetSequence.ElementAt(x).operand));
                    }

                    if (foundTargetSequence)
                    {
                        if (patchMode == PatchMode.BEFORE || patchMode == PatchMode.AFTER)
                        {
                            int indexToInsertAt = patchMode == PatchMode.AFTER ? i + targetSize : i;
                            newInstructions.InsertRange(indexToInsertAt, patchSequence);
                        }
                        else if (patchMode == PatchMode.REPLACE)
                        {
                            newInstructions[i].opcode = OpCodes.Nop;
                            newInstructions.RemoveRange(i + 1, targetSize - 1);
                            newInstructions.InsertRange(i + 1, patchSequence);
                        }
                        else
                        {
                            throw new ArgumentException($"Argument PatchMode patchMode == {patchMode}; invalid value!");
                        }

                        break;
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();

                        sb.AppendLine($"Failed to patch by sequence: couldn't find target sequence.  This might be okay in certain cases.");

                        // Cut down the stack trace because it's 20 lines of unhelpful reflection internals.
                        // Show enough to figure out which plugin + transpiler method is causing this:
                        sb.AppendLine($"Stack Trace:");
                        string[] stackTrace = new System.Diagnostics.StackTrace().ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        for (int lineNumber = 0; lineNumber < 2; lineNumber++)
                        {
                            sb.AppendLine(stackTrace[lineNumber]);
                        }

                        Logger.Info(sb.ToString());
                    }
                }
            }

            return newInstructions.AsEnumerable();
        }

        public enum PatchMode
        {
            BEFORE,
            AFTER,
            REPLACE
        }
    }
}
