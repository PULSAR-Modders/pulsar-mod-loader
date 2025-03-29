using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PulsarModLoader.Injections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PulsarModLoader.Adaptor
{
    public class Patcher
    {

        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };
        internal static readonly ManualLogSource Log = Logger.CreateLogSource("PML-Adaptor");

        public static void Patch(AssemblyDefinition assembly)
        {
            if (IsModified(assembly))
            {
                Log.LogInfo("The assembly is already modified, and a backup could not be found.");
                return;
            }

            PatchMethod(assembly, "PLGlobal", "Start", typeof(LoggingInjections), "LoggingCleanup");
            PatchMethod(assembly, "PLGlobal", "Awake", typeof(HarmonyInjector), "InitializeHarmony");

        }

        public static bool IsModified(AssemblyDefinition targetAssembly)
        {
            string targetClassName = "PLGlobal";
            string targetMethodName = "Awake";

            // Find the methods involved
            MethodDefinition targetMethod = targetAssembly.MainModule.GetType(targetClassName).Methods.First(m => m.Name == targetMethodName);

            if (targetMethod == null)
            {
                throw new ArgumentNullException("Couldn't find method in target assembly!");
            }

            if (targetMethod.Body.Instructions[0].OpCode == OpCodes.Call)
            {
                return true;
            }
            return false;
        }

        public static void PatchMethod(AssemblyDefinition targetAssembly, string targetClassName, string targetMethodName, Type sourceClassType, string sourceMethodName)
        {
            Log.LogInfo($"Attempting {sourceClassType.ToString()} Injection");
            // Find the methods involved
            MethodDefinition targetMethod = targetAssembly.MainModule.GetType(targetClassName).Methods.First(m => m.Name == targetMethodName);
            MethodReference sourceMethod = targetAssembly.MainModule.ImportReference(sourceClassType.GetMethod(sourceMethodName));

            if (targetMethod == null || sourceMethod == null)
            {
                throw new ArgumentNullException("Couldn't find method in target assembly!");
            }

            Log.LogInfo("Found relevant method.  Injecting hook...");

            // Inject source method into front of target method
            ILProcessor targetProcessor = targetMethod.Body.GetILProcessor();

            Instruction oldFirstInstruction = targetMethod.Body.Instructions[0];
            Instruction callToInjectedMethod = targetProcessor.Create(OpCodes.Call, sourceMethod);

            targetProcessor.InsertBefore(oldFirstInstruction, callToInjectedMethod);
        }
    }
}
