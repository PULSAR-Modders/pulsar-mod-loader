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
        public static void Patch(string targetAssemblyPath)
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
            else
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
            MethodDefinition targetMethod = targetAssembly.MainModule.GetType("PLGlobal").Methods.First(m => m.Name == "Awake");
            MethodReference sourceMethod = targetAssembly.MainModule.ImportReference(typeof(Loader).GetMethod("LoadPluginsDirectory"));

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

            /* Copy Loader's assembly to target assembly's directory */
            string sourcePath = Assembly.GetExecutingAssembly().Location;
            string destPath = Path.Combine(targetAssemblyDir, Path.GetFileName(Assembly.GetExecutingAssembly().Location));

            Log(string.Format("Copying {0} to {1}", Path.GetFileName(destPath), Path.GetDirectoryName(destPath)));
            File.Copy(sourcePath, destPath, overwrite: true);

            Log("Success!  You may now run the game normally.");
        }

        public static void LoadPluginsDirectory()
        {
            string pluginsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Plugins");

            Log(String.Format("Attempting to load plugins from {0}", pluginsDir));

            if (!Directory.Exists(pluginsDir))
            {
                Directory.CreateDirectory(pluginsDir);
            }

            int LoadedPluginCounter = 0;
            foreach (string assemblyPath in Directory.GetFiles(pluginsDir, "*.dll"))
            {
                if (Path.GetFileName(assemblyPath) != "0Harmony.dll")
                {
                    bool isLoaded = LoadPlugin(assemblyPath);

                    if (isLoaded)
                    {
                        LoadedPluginCounter += 1;
                    }
                }
            }

            Log(string.Format("Finished loading {0} plugins!", LoadedPluginCounter));
        }

        public static bool LoadPlugin(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
            {
                throw new IOException(string.Format("Couldn't find file: {0}", assemblyPath));
            }

            /* Find methods labeled as the plugin's entry point */
            Log(string.Format("Searching for plugin entry point in {0}", Path.GetFileName(assemblyPath)));

            Assembly asm = Assembly.LoadFrom(assemblyPath);
            foreach (Type t in asm.GetTypes())
            {
                foreach (MethodInfo m in t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    object[] attrs = m.GetCustomAttributes(typeof(PluginEntryPoint), inherit: false);
                    if (attrs != null && attrs.Length > 0)
                    {
                        Log(string.Format("Loading plugin via {0}", m.Name));
                        m.Invoke(null, null);
                        return true;
                    }
                }
            }

            Log(string.Format("Skipping {0}; couldn't find plugin entry point.", Path.GetFileName(assemblyPath)));
            return false;
        }

        public static void Log(string message)
        {
            Console.WriteLine(string.Format("[PPL] {0}", message));
        }
    }
}
