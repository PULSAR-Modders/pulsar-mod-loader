using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using PulsarModLoader.Events;
using PulsarModLoader.Utilities;
using System;
using System.IO;
using System.Linq;

namespace PulsarModLoader.Injections
{
    public class EventInjector
    {
        public static void InjectEvents(string targetAssemblyPath)
        {
            Logger.Info("Injecting events");
            AssemblyDefinition targetAssembly = LoadAssembly(targetAssemblyPath, null);

            PatchPLServer_AddPlayer(targetAssembly);
            PatchPLServer_RemovePlayer(targetAssembly);

            SaveAssembly(targetAssembly, targetAssemblyPath);
        }

        private static void PatchPLServer_AddPlayer(AssemblyDefinition assembly)
        {
            Logger.Info("Patching PLServer.AddPlayer");
            string variableRequiredOrHarmonyCrashes = "AddPlayer";
            MethodDefinition targetMethod = assembly.MainModule.GetType("PLServer").Methods.First(m => m.Name == variableRequiredOrHarmonyCrashes);
            MethodReference patchMethod = assembly.MainModule.ImportReference(typeof(EventHelper).GetMethod("OnPlayerAdded"));

            ILProcessor processor = targetMethod.Body.GetILProcessor();
            Collection<Instruction> instructions = targetMethod.Body.Instructions;
            Instruction insn = instructions.Last();
            //Find inPlayer.ResetTalentPoints();
            while (insn.OpCode != OpCodes.Ldarg_1)
            {
                insn = insn.Previous;
            }

            processor.InsertBefore(insn, processor.Create(OpCodes.Ldarg_1));
            processor.InsertBefore(insn, processor.Create(OpCodes.Call, patchMethod));
        }

        private static void PatchPLServer_RemovePlayer(AssemblyDefinition assembly)
        {
            Logger.Info("Patching PLServer.RemovePlayer");
            string variableRequiredOrHarmonyCrashes = "RemovePlayer";
            MethodDefinition targetMethod = assembly.MainModule.GetType("PLServer").Methods.First(m => m.Name == variableRequiredOrHarmonyCrashes);
            MethodReference patchMethod = assembly.MainModule.ImportReference(typeof(EventHelper).GetMethod("OnPlayerRemoved"));

            ILProcessor processor = targetMethod.Body.GetILProcessor();
            Collection<Instruction> instructions = targetMethod.Body.Instructions;
            Instruction insn = instructions.First();
            //Find base.photonView.RPC("LogoutMessage",
            while (insn.OpCode != OpCodes.Ldarg_0)
            {
                insn = insn.Next;
            }

            processor.InsertBefore(insn, processor.Create(OpCodes.Ldarg_1));
            processor.InsertBefore(insn, processor.Create(OpCodes.Call, patchMethod));
        }

        private static AssemblyDefinition LoadAssembly(string assemblyPath, string[] depencencyDirectories)
        {
            if (!File.Exists(assemblyPath))
            {
                throw new IOException($"Couldn't find file: {assemblyPath}");
            }

            /* Specify directories containing dependencies of the assemblies */
            DefaultAssemblyResolver assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));

            if (depencencyDirectories != null)
            {
                foreach (string dir in depencencyDirectories)
                {
                    assemblyResolver.AddSearchDirectory(dir);
                }
            }

            /* Load the assembly */
            return AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { AssemblyResolver = assemblyResolver, ReadWrite = true, InMemory = true });
        }

        private static void SaveAssembly(AssemblyDefinition assembly, string assemblyPath)
        {
            Logger.Info($"Writing hooked {Path.GetFileName(assemblyPath)} to disk...");
            try
            {
                assembly.Write(assemblyPath);
            }
            catch (Exception e) when (e is BadImageFormatException)
            {
                Logger.Info("Failed to modify corrupted assembly.  Try again with a clean assembly (e.g., verify files on Steam)");
            }
        }
    }
}
