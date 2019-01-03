using Harmony;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace PulsarPluginLoader.Tests
{
    [TestFixture]
    class PatchBySequenceTests
    {
        readonly List<CodeInstruction> targetSequence = new List<CodeInstruction>()
        {
            //new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PLServer), "ChaosLevel")),
            //new CodeInstruction(OpCodes.Ldc_R4, 0),
            //new CodeInstruction(OpCodes.Ldc_R4, 6),
        };
    }
}
