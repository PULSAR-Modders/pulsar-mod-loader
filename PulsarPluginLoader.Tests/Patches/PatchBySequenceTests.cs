using HarmonyLib;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static PulsarPluginLoader.Patches.HarmonyHelpers;

namespace PulsarPluginLoader.Tests.Patches
{
    [TestFixture]
    class PatchBySequenceTests
    {
        private static bool AreEqualSequences(IEnumerable<CodeInstruction> first, IEnumerable<CodeInstruction> second)
        {
            if (first.Count() != second.Count())
            {
                return false;
            }

            for (int i = 0; i < first.Count(); i++)
            {
                CodeInstruction a = first.ElementAt(i);
                CodeInstruction b = second.ElementAt(i);

                if (!a.opcode.Equals(b.opcode) || !a.operand.Equals(b.operand))
                {
                    return false;
                }

            }

            return true;
        }

        [Test]
        public void CanInsert_Before()
        {
            List<CodeInstruction> original = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SampleClass), "IntField")),
                new CodeInstruction(OpCodes.Ldc_R4, 0),
                new CodeInstruction(OpCodes.Ldc_R4, 6),
            };

            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SampleClass), "IntField")),
                new CodeInstruction(OpCodes.Ldc_R4, 0),
                new CodeInstruction(OpCodes.Ldc_R4, 6),
            };

            List<CodeInstruction> patchSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_R4, 12),
                new CodeInstruction(OpCodes.Ldc_R4, 18),
            };

            List<CodeInstruction> actual = PatchBySequence(original, targetSequence, patchSequence, PatchMode.BEFORE).ToList();

            List<CodeInstruction> expected = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_R4, 12),
                new CodeInstruction(OpCodes.Ldc_R4, 18),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SampleClass), "IntField")),
                new CodeInstruction(OpCodes.Ldc_R4, 0),
                new CodeInstruction(OpCodes.Ldc_R4, 6),
            };

            Assert.IsTrue(AreEqualSequences(expected, actual));
        }

        [Test]
        public void CanInsert_After()
        {
            List<CodeInstruction> original = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SampleClass), "IntField")),
                new CodeInstruction(OpCodes.Ldc_R4, 0),
                new CodeInstruction(OpCodes.Ldc_R4, 6),
            };

            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SampleClass), "IntField")),
                new CodeInstruction(OpCodes.Ldc_R4, 0),
                new CodeInstruction(OpCodes.Ldc_R4, 6),
            };

            List<CodeInstruction> patchSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_R4, 12),
                new CodeInstruction(OpCodes.Ldc_R4, 18),
            };

            List<CodeInstruction> actual = PatchBySequence(original, targetSequence, patchSequence, PatchMode.AFTER).ToList();

            List<CodeInstruction> expected = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SampleClass), "IntField")),
                new CodeInstruction(OpCodes.Ldc_R4, 0),
                new CodeInstruction(OpCodes.Ldc_R4, 6),
                new CodeInstruction(OpCodes.Ldc_R4, 12),
                new CodeInstruction(OpCodes.Ldc_R4, 18),
            };

            Assert.IsTrue(AreEqualSequences(expected, actual));
        }

        [Test]
        public void CanReplace_SameLength()
        {
            List<CodeInstruction> original = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SampleClass), "IntField")),
                new CodeInstruction(OpCodes.Ldc_R4, 0),
                new CodeInstruction(OpCodes.Ldc_R4, 6),
            };

            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_R4, 0),
                new CodeInstruction(OpCodes.Ldc_R4, 6),
            };

            List<CodeInstruction> patchSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_R4, 12),
                new CodeInstruction(OpCodes.Ldc_R4, 18),
            };

            List<CodeInstruction> actual = PatchBySequence(original, targetSequence, patchSequence, PatchMode.REPLACE).ToList();

            List<CodeInstruction> expected = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SampleClass), "IntField")),
                new CodeInstruction(OpCodes.Ldc_R4, 12),
                new CodeInstruction(OpCodes.Ldc_R4, 18),
            };

            Assert.IsTrue(AreEqualSequences(expected, actual));
        }

        [Test]
        public void CanReplace_SmallerInsert()
        {
            List<CodeInstruction> original = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SampleClass), "IntField")),
                new CodeInstruction(OpCodes.Ldc_R4, 0),
                new CodeInstruction(OpCodes.Ldc_R4, 6),
            };

            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_R4, 0),
                new CodeInstruction(OpCodes.Ldc_R4, 6),
            };

            List<CodeInstruction> patchSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_R4, 12)
            };

            List<CodeInstruction> actual = PatchBySequence(original, targetSequence, patchSequence, PatchMode.REPLACE).ToList();

            List<CodeInstruction> expected = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SampleClass), "IntField")),
                new CodeInstruction(OpCodes.Ldc_R4, 12)
            };

            Assert.IsTrue(AreEqualSequences(expected, actual));
        }

        [Test]
        public void CanReplace_LargerInsert()
        {
            List<CodeInstruction> original = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SampleClass), "IntField")),
                new CodeInstruction(OpCodes.Ldc_R4, 0),
                new CodeInstruction(OpCodes.Ldc_R4, 6),
            };

            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_R4, 0),
                new CodeInstruction(OpCodes.Ldc_R4, 6),
            };

            List<CodeInstruction> patchSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_R4, 12),
                new CodeInstruction(OpCodes.Ldc_R4, 18),
                new CodeInstruction(OpCodes.Ldc_R4, 24),
            };

            List<CodeInstruction> actual = PatchBySequence(original, targetSequence, patchSequence, PatchMode.REPLACE).ToList();

            List<CodeInstruction> expected = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SampleClass), "IntField")),
                new CodeInstruction(OpCodes.Ldc_R4, 12),
                new CodeInstruction(OpCodes.Ldc_R4, 18),
                new CodeInstruction(OpCodes.Ldc_R4, 24),
            };

            Assert.IsTrue(AreEqualSequences(expected, actual));
        }
    }
}
