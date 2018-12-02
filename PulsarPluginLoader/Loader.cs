using Harmony;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PulsarPluginLoader
{
    public static class Loader
    {
        public static void CreateMethod(string targetAssemblyPath, string className, string newMethodName, Type returnType, Type[] parameterTypes)
        {
            if (parameterTypes == null)
            {
                parameterTypes = new Type[0];
            }

            AssemblyDefinition targetAssembly = LoadAssembly(targetAssemblyPath, null);

            MethodDefinition newMethod = new MethodDefinition(newMethodName, Mono.Cecil.MethodAttributes.Private, targetAssembly.MainModule.ImportReference(returnType));

            foreach (Type parameter in parameterTypes)
            {
                newMethod.Parameters.Add(new ParameterDefinition(targetAssembly.MainModule.ImportReference(parameter)));
            }

            newMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            targetAssembly.MainModule.GetType(className).Methods.Add(newMethod);

            SaveAssembly(targetAssembly, targetAssemblyPath);
        }

        public static void PatchMethod(string targetAssemblyPath, string targetClassName, string targetMethodName, Type sourceClassType, string sourceMethodName, bool useBackup = true)
        {
            Log($"Attempting to hook {targetAssemblyPath}");

            /* Load the assemblies */
            AssemblyDefinition targetAssembly = LoadAssembly(targetAssemblyPath, null);

            /* Find the methods involved */
            MethodDefinition targetMethod = targetAssembly.MainModule.GetType(targetClassName).Methods.First(m => m.Name == targetMethodName);
            MethodReference sourceMethod = targetAssembly.MainModule.ImportReference(sourceClassType.GetMethod(sourceMethodName));

            if (targetMethod == null || sourceMethod == null)
            {
                throw new ArgumentNullException("Couldn't find method in target assembly!");
            }

            Log("Loaded relevant assemblies.  Injecting hook...");

            /* Inject source method into front of target method */
            ILProcessor targetProcessor = targetMethod.Body.GetILProcessor();

            Instruction oldFirstInstruction = targetMethod.Body.Instructions[0];
            Instruction callToLoadPlugins = targetProcessor.Create(OpCodes.Call, sourceMethod);

            targetProcessor.InsertBefore(oldFirstInstruction, callToLoadPlugins);

            SaveAssembly(targetAssembly, targetAssemblyPath);
        }

        public static void InitializeHarmony()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("wiki.pulsar.ppl");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
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
            Log($"Writing hooked {Path.GetFileName(assemblyPath)} to disk...");
            try
            {
                assembly.Write(assemblyPath);
            }
            catch (Exception e) when (e is BadImageFormatException)
            {
                Log("Failed to modify corrupted assembly.  Try again with a clean assembly (e.g., verify files on Steam)");
            }
        }

        public static void CopyAssemblies(string targetAssemblyDir)
        {
            /* Copy important assemblies to target assembly's directory */
            string sourceDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] copyables = new string[] {
                Assembly.GetExecutingAssembly().Location,
                Path.Combine(sourceDir, "0Harmony.dll")
            };

            foreach (string sourcePath in copyables)
            {
                string destPath = Path.Combine(targetAssemblyDir, Path.GetFileName(sourcePath));
                Log($"Copying {Path.GetFileName(destPath)} to {Path.GetDirectoryName(destPath)}");
                try
                {
                    File.Copy(sourcePath, destPath, overwrite: true);
                }
                catch (IOException)
                {
                    Log("Copying failed!  Close the game and try again.");
                    Environment.Exit(0);
                }
            };
        }

        public static void Log(string message)
        {
            Console.WriteLine($"[PPL] {message}");
        }
    }
}
