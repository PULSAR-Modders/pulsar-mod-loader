using HarmonyLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using PulsarPluginLoader.CustomGUI;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PulsarPluginLoader.Loader
{
    public static class EntryPoint
    {
        public static bool pluginsLoaded;
        private static void Main() // EntryPoint for UnityDoorStop
        {
            try {
                new Thread(InitPulsarPluginLoader).Start();
            }
            catch(Exception e) {
               Utilities.Logger.Info($"PPL EXCEPTION!!! \n{e}");
            }
        }

        private static void InitPulsarPluginLoader()
        {
            var cfgpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/PulsarPluginLoaderConfig.json";
            if (!File.Exists(cfgpath))
                PPLConfig.CreateDefaultConfig(cfgpath, true);
            else
                PPLConfig.CreateConfigFromFile(cfgpath);

            while (PLUIMainMenu.Instance == null) Task.Delay(500).Wait();

            // Force Photon's static constructor to run so patching its methods doesn't fail
            RuntimeHelpers.RunClassConstructor(typeof(PhotonNetwork).TypeHandle);

            new Harmony("wiki.pulsar.ppl").PatchAll(Assembly.GetExecutingAssembly());

            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

            UnityEngine.Object.DontDestroyOnLoad(new GameObject("ModManager", typeof(GUIMain))); // Init ModManager

            PluginManager.Instance.LoadPluginsDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Mods"));
            pluginsLoaded = true;
        }
    }
}
