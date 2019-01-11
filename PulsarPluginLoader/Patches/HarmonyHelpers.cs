using Harmony;
using Harmony.ILCopying;
using PulsarPluginLoader.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PulsarPluginLoader.Patches
{
    public static class HarmonyHelpers
    {
        public static IEnumerable<CodeInstruction> PatchBySequence(IEnumerable<CodeInstruction> instructions, IEnumerable<CodeInstruction> targetSequence, IEnumerable<CodeInstruction> patchSequence, PatchMode patchMode = PatchMode.AFTER, bool checkOperands = true)
        {
            List<CodeInstruction> newInstructions = instructions.ToList();

            CodeInstruction targetStart = targetSequence.ElementAt(0);
            int targetSize = targetSequence.Count();

            for (int i = 0; i < newInstructions.Count; i++)
            {
                bool startsWithTargetInstruction = newInstructions[i].opcode.Equals(targetStart.opcode);
                bool targetSequenceStillFits = i + targetSize <= newInstructions.Count;

                if (startsWithTargetInstruction && targetSequenceStillFits)
                {
                    bool foundTargetSequence = true;

                    for (int x = 1; x < targetSize && foundTargetSequence; x++)
                    {
                        foundTargetSequence = newInstructions[i + x].opcode.Equals(targetSequence.ElementAt(x).opcode)
                            && (!checkOperands || (
                                    (newInstructions[i + x].operand == null && targetSequence.ElementAt(x).operand == null) 
                                    || newInstructions[i + x].operand.Equals(targetSequence.ElementAt(x).operand)
                                )
                        );
                    }

                    if (foundTargetSequence)
                    {
                        if (patchMode == PatchMode.BEFORE || patchMode == PatchMode.AFTER)
                        {
                            int indexToInsertAt = patchMode == PatchMode.AFTER ? i + targetSize : i;
                            newInstructions.InsertRange(indexToInsertAt, patchSequence.Select(c => c.FullClone()));
                        }
                        else if (patchMode == PatchMode.REPLACE)
                        {
                            newInstructions.RemoveRange(i, targetSize);
                            newInstructions.InsertRange(i, patchSequence.Select(c => c.FullClone() ));
                        }
                        else
                        {
                            throw new ArgumentException($"Argument PatchMode patchMode == {patchMode}; invalid value!");
                        }

                        break;
                    }
                    else if (!targetSequenceStillFits)
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

        /// <summary>
        /// Logs the string form of an IEnumerable sequence to ease debugging.
        /// </summary>
        /// <param name="label">Text to display before the sequence.</param>
        /// <param name="sequence">Sequence to display, one element per line.</param>
        public static void LogSequence(string label, IEnumerable sequence)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(label);
            foreach (object c in sequence)
            {
                sb.AppendLine($"\t{c.ToString()}");
            }

            Logger.Info(sb.ToString());
        }

        /// <summary>
        /// Deep-copies the instruction, including labels and exception blocks.
        /// </summary>
        /// <param name="instruction">The instruction to fully clone.</param>
        /// <returns>Fully cloned instruction.</returns>
        public static CodeInstruction FullClone(this CodeInstruction instruction)
        {
            CodeInstruction clone = instruction.Clone();
            clone.labels = instruction.labels.ConvertAll(l => l); // TODO: Clone labels?
            clone.blocks = instruction.blocks.ConvertAll(b => b.Clone());

            return clone;
        }

        /// <summary>
        /// Deep-copies the exception block.
        /// </summary>
        /// <param name="block">The exception block to clone.</param>
        /// <returns>The cloned exception block.</returns>
        public static ExceptionBlock Clone(this ExceptionBlock block)
        {
            return new ExceptionBlock(block.blockType, block.catchType);
        }
    }
}
