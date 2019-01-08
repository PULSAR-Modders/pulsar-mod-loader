using Mono.Cecil;
using Mono.Cecil.Cil;
using PulsarPluginLoader.Utils;
using System;
using System.IO;
using System.Linq;

namespace PulsarPluginLoader.Injections
{
    public static class InjectionTools
    {
        public static void CreateMethod(string targetAssemblyPath, string className, string newMethodName, Type returnType, Type[] parameterTypes)
        {
            if (parameterTypes == null)
            {
                parameterTypes = new Type[0];
            }

            AssemblyDefinition targetAssembly = LoadAssembly(targetAssemblyPath, null);

            MethodDefinition newMethod = new MethodDefinition(newMethodName, MethodAttributes.Private, targetAssembly.MainModule.ImportReference(returnType));

            foreach (Type parameter in parameterTypes)
            {
                newMethod.Parameters.Add(new ParameterDefinition(targetAssembly.MainModule.ImportReference(parameter)));
            }

            newMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            targetAssembly.MainModule.GetType(className).Methods.Add(newMethod);

            SaveAssembly(targetAssembly, targetAssemblyPath);
        }

        public static void ShortCircuitMethod(string targetAssemblyPath, string targetClassName, string targetMethodName)
        {
            Logger.Info($"Attempting to short circuit method in {targetAssemblyPath}@{targetClassName}::{targetMethodName}");

            /* Load the assemblies */
            AssemblyDefinition targetAssembly = LoadAssembly(targetAssemblyPath, null);

            /* Find the methods involved */
            MethodDefinition targetMethod = targetAssembly.MainModule.GetType(targetClassName).Methods.First(m => m.Name == targetMethodName);

            if (targetMethod == null)
            {
                throw new ArgumentNullException("Couldn't find method in target assembly!");
            }

            Logger.Info("Loaded relevant assemblies.  Short circuiting method...");

            /* Inject return at start of target method */
            ILProcessor targetProcessor = targetMethod.Body.GetILProcessor();
            targetProcessor.InsertBefore(targetMethod.Body.Instructions[0], targetProcessor.Create(OpCodes.Ret));

            SaveAssembly(targetAssembly, targetAssemblyPath);
        }

        public static bool IsModified(string targetAssemblyPath)
        {
            string targetClassName = "PLGameStatic";
            string targetMethodName = "OnInjectionCheatDetected";

            /* Load the assemblies */
            AssemblyDefinition targetAssembly = LoadAssembly(targetAssemblyPath, null);

            /* Find the methods involved */
            MethodDefinition targetMethod = targetAssembly.MainModule.GetType(targetClassName).Methods.First(m => m.Name == targetMethodName);

            if (targetMethod == null)
            {
                throw new ArgumentNullException("Couldn't find method in target assembly!");
            }

            if (targetMethod.Body.Instructions[0].OpCode == OpCodes.Ret)
            {
                return true;
            }
            return false;
        }

        public static void PatchMethod(string targetAssemblyPath, string targetClassName, string targetMethodName, Type sourceClassType, string sourceMethodName)
        {
            Logger.Info($"Attempting to hook {targetAssemblyPath}");

            /* Load the assemblies */
            AssemblyDefinition targetAssembly = LoadAssembly(targetAssemblyPath, null);

            /* Find the methods involved */
            MethodDefinition targetMethod = targetAssembly.MainModule.GetType(targetClassName).Methods.First(m => m.Name == targetMethodName);
            MethodReference sourceMethod = targetAssembly.MainModule.ImportReference(sourceClassType.GetMethod(sourceMethodName));

            if (targetMethod == null || sourceMethod == null)
            {
                throw new ArgumentNullException("Couldn't find method in target assembly!");
            }

            Logger.Info("Loaded relevant assemblies.  Injecting hook...");

            /* Inject source method into front of target method */
            ILProcessor targetProcessor = targetMethod.Body.GetILProcessor();

            Instruction oldFirstInstruction = targetMethod.Body.Instructions[0];
            Instruction callToInjectedMethod = targetProcessor.Create(OpCodes.Call, sourceMethod);

            targetProcessor.InsertBefore(oldFirstInstruction, callToInjectedMethod);

            SaveAssembly(targetAssembly, targetAssemblyPath);
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
