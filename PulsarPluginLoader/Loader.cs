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
        public static void Patch(string targetAssemblyPath, string targetClassName, string targetMethodName, Type sourceClassType, string sourceMethodName, bool useBackup = true)
        {
            Log(string.Format("Attempting to hook {0}", targetAssemblyPath));

            string targetAssemblyDir = Path.GetDirectoryName(targetAssemblyPath);

            if (!File.Exists(targetAssemblyPath))
            {
                throw new IOException(string.Format("Couldn't find file: {0}", targetAssemblyPath));
            }

            string backupPath = targetAssemblyPath + ".bak";
            if (!File.Exists(backupPath))
            {
                Log(string.Format("Making backup as {0}", Path.GetFileName(backupPath)));
                File.Copy(targetAssemblyPath, backupPath, overwrite: true);
            }
            else if (File.Exists(backupPath) && useBackup)
            {
                /* Restore the hopefully original Assembly for easier patching */
                Log(string.Format("Restoring the hopefully clean backup before hooking.  If you have issues, try deleting {0} and verifying files on Steam, especially after an official patch.", Path.GetFileName(backupPath)));
                File.Copy(backupPath, targetAssemblyPath, overwrite: true);
            }

            /* Specify directories containing dependencies of the assemblies */
            DefaultAssemblyResolver assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(targetAssemblyDir);

            /* Load the assemblies */
            AssemblyDefinition targetAssembly = AssemblyDefinition.ReadAssembly(targetAssemblyPath, new ReaderParameters { AssemblyResolver = assemblyResolver, ReadWrite = true, InMemory = true });

            /* Find the methods involved */
            MethodDefinition targetMethod = targetAssembly.MainModule.GetType(targetClassName).Methods.First(m => m.Name == targetMethodName);
            MethodReference sourceMethod = targetAssembly.MainModule.ImportReference(sourceClassType.GetMethod(sourceMethodName));

            if (targetMethod == null || sourceMethod == null)
            {
                throw new ArgumentNullException("Couldn't find method in target assembly!");
            }

            Log(string.Format("Loaded relevant assemblies.  Injecting hook...", targetAssemblyPath));

            /* Inject source method into front of target method */
            ILProcessor targetProcessor = targetMethod.Body.GetILProcessor();

            Instruction oldFirstInstruction = targetMethod.Body.Instructions[0];
            Instruction callToLoadPlugins = targetProcessor.Create(OpCodes.Call, sourceMethod);

            targetProcessor.InsertBefore(oldFirstInstruction, callToLoadPlugins);

            /* Save target assembly to disk */
            Log(string.Format("Writing hooked {0} to disk...", Path.GetFileName(targetAssemblyPath)));
            try
            {
                targetAssembly.Write(targetAssemblyPath);
            }
            catch (Exception e) when (e is BadImageFormatException)
            {
                Log("Failed to modify corrupted assembly.  Try again with a clean assembly (e.g., verify files on Steam)");
            }
        }

        public static void InitializeHarmony()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("wiki.pulsar.ppl");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
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
                Log(string.Format("Copying {0} to {1}", Path.GetFileName(destPath), Path.GetDirectoryName(destPath)));
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
            Console.WriteLine(string.Format("[PPL] {0}", message));
        }
    }
}
