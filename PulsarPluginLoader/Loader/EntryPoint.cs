using HarmonyLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using PulsarPluginLoader.CustomGUI;
using UnityEngine;

namespace PulsarPluginLoader.Loader
{
    public static class EntryPoint
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            Process game;

            if (args.Length == 0)
            {
                Process.Start("steam://rungameid/252870");
                Thread.Sleep(7000);
                game = Process.GetProcessesByName("PULSAR_LostColony")[0];
            }
            else
            {
                game = Process.Start(args[0]); // Works a little differently as intended
                Thread.Sleep(6000);
            }

            Console.WriteLine("Loading...");


            if (game != null)
            {
                var exepath = Assembly.GetEntryAssembly().Location;
                Loader.Load(game, File.ReadAllBytes(exepath), new[] { File.ReadAllBytes(exepath.Replace("PulsarPluginLoader.exe", "0Harmony.dll")) });
            }
            else throw new Exception("Game process is null");
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int MessageBox(int hWnd, String text, String caption, uint type);

        public static bool pluginsLoaded = false;

        private static void InitPulsarPluginLoader()
        {
            try
            {
                var cfgpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "PulsarPluginLoaderConfig.json";
                if (!File.Exists(cfgpath))
                    PPLConfig.CreateDefaultConfig(cfgpath, true);
                else
                    PPLConfig.CreateConfigFromFile(cfgpath);

                new Harmony("wiki.pulsar.ppl").PatchAll(Assembly.GetExecutingAssembly());

                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

                UnityEngine.Object.DontDestroyOnLoad(new GameObject("ModManager", typeof(GUIMain)));

                PluginManager.Instance.LoadPluginsDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), PPLConfig.instance.ModsFolder));
                pluginsLoaded = true;
            }
            catch(Exception e)
            {
                MessageBox(0, e.ToString(), "PPL EXCEPTION", 0);
            }
        }
    }
}
