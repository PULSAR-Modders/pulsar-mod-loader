using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using PulsarPluginLoader.Events;
using PulsarPluginLoader.Utils;
using System;
using System.IO;
using System.Linq;

namespace PulsarPluginLoader.Injections
{
    public class EventInjector
    {
        public static void InjectEvents(string targetAssemblyPath)
        {
            Logger.Info("Injecting events");
            AssemblyDefinition targetAssembly = LoadAssembly(targetAssemblyPath, null);

            PatchOnPlayerJoined(targetAssembly);

            SaveAssembly(targetAssembly, targetAssemblyPath);
        }

        private static void PatchOnPlayerJoined(AssemblyDefinition assembly)
        {
            Logger.Info("Patching PLServer.AddPlayer");
            MethodDefinition targetMethod = assembly.MainModule.GetType("PLServer").Methods.First(m => m.Name == "AddPlayer");
            MethodReference patchMethod = assembly.MainModule.ImportReference(typeof(EventHelper).GetMethod("OnPlayerJoin"));

            ILProcessor processor = targetMethod.Body.GetILProcessor();
            Collection<Instruction> instructions = targetMethod.Body.Instructions;
            Instruction insn = instructions.Last();
            //Find inPlayer.ResetTalentPoints();
            while (insn.OpCode != OpCodes.Ldarg_1) insn = insn.Previous;
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
