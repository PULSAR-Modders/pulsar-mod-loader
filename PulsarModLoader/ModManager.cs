using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PulsarModLoader
{
    /// <summary>
    /// Manages all mods.
    /// </summary>
    public class ModManager
    {
        public static bool IsOldVersion;

        public delegate void ModLoaded(string name, PulsarMod mod);
        public delegate void ModUnloaded(PulsarMod mod);
        public delegate void AllModsLoaded();

        public event ModLoaded OnModSuccessfullyLoaded;
        public event ModUnloaded OnModUnloaded;
        public event AllModsLoaded OnAllModsLoaded;

        public FileVersionInfo PMLVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

        private readonly Dictionary<string, PulsarMod> activeMods;
        private readonly HashSet<string> modDirectories;

        private static ModManager _instance = null;

        /// <summary>
        /// Static instance of the mod manager.
        /// </summary>
        public static ModManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ModManager();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Manages all mods.
        /// </summary>
        public ModManager()
        {
            Logger.Info($"Starting {PMLVersionInfo.ProductName} v{PMLVersionInfo.FileVersion}");

            activeMods = new Dictionary<string, PulsarMod>();
            modDirectories = new HashSet<string>();

            // Add mods directories to AppDomain so mods referencing other as-yet-unloaded mods don't fail to find assemblies
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveModsDirectory);

            // Force Photon's static constructor to run so patching its methods doesn't fail
            RuntimeHelpers.RunClassConstructor(typeof(PhotonNetwork).TypeHandle);

            IsOldVersion = false;

#if !DEBUG
            try
            {
                using (var web = new System.Net.WebClient())
                {
                    web.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.71 Safari/537.36");

                    string[] info = web.DownloadString("https://api.github.com/repos/PULSAR-Modders/pulsar-mod-loader/releases/latest").Split('\n');
                    string versionFromInfo = info.First(i => i.Contains("tag_name"))
                        .Replace(@"  ""tag_name"": """, string.Empty)
                        .Replace(@""",", string.Empty);

                    if (!PMLVersionInfo.FileVersion.StartsWith(versionFromInfo))
                        IsOldVersion = true;
                }
            }
            catch { }
#endif
        }

        /// <summary>
        /// Gets the PulsarMod Class of the given mod.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>True if Loaded</returns>
        public PulsarMod GetMod(string name)
        {
            if (activeMods.TryGetValue(name, out PulsarMod mod))
            {
                return mod;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if given mod is loaded.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns true if loaded</returns>
        public bool IsModLoaded(string name)
        {
            return activeMods.ContainsKey(name);
        }

        /// <summary>
        /// Returns an IEnumerable of all loaded PulsarMods.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PulsarMod> GetAllMods()
        {
            return activeMods.Values;
        }

        /// <summary>
        /// Loads all mods found in the given directory
        /// </summary>
        /// <param name="modsDir"></param>
        public void LoadModsDirectory(string modsDir)
        {
            OnModSuccessfullyLoaded += Events.EventHelper.RegisterEventHandlers;
            Logger.Info($"Attempting to load mods from {modsDir}");

            // Manage directories
            if (!Directory.Exists(modsDir))
            {
                Directory.CreateDirectory(modsDir);
            }
            modDirectories.Add(modsDir);
            
            // Load mods
            foreach (string assemblyPath in Directory.GetFiles(modsDir, "*.dll"))
            {
                if (Path.GetFileName(assemblyPath) != "0Harmony.dll")
                {
                    LoadMod(assemblyPath);
                }
            }

            Logger.Info($"Finished loading {activeMods.Count} mods!");

            // Activate all managers
            _ = PulsarModLoader.Content.Items.ItemModManager.Instance;
            _ = PulsarModLoader.Content.Components.AutoTurret.AutoTurretModManager.Instance;
            _ = PulsarModLoader.Content.Components.CaptainsChair.CaptainsChairModManager.Instance;
            _ = PulsarModLoader.Content.Components.CPU.CPUModManager.Instance;
            _ = PulsarModLoader.Content.Components.Extractor.ExtractorModManager.Instance;
            _ = PulsarModLoader.Content.Components.FBRecipeModule.FBRecipeModuleModManager.Instance;
            _ = PulsarModLoader.Content.Components.Hull.HullModManager.Instance;
            _ = PulsarModLoader.Content.Components.HullPlating.HullPlatingModManager.Instance;
            _ = PulsarModLoader.Content.Components.InertiaThruster.InertiaThrusterModManager.Instance;
            _ = PulsarModLoader.Content.Components.ManeuverThruster.ManeuverThrusterModManager.Instance;
            _ = PulsarModLoader.Content.Components.MegaTurret.MegaTurretModManager.Instance;
            _ = PulsarModLoader.Content.Components.Missile.MissileModManager.Instance;
            _ = PulsarModLoader.Content.Components.MissionShipComponent.MissionShipComponentModManager.Instance;
            _ = PulsarModLoader.Content.Components.NuclearDevice.NuclearDeviceModManager.Instance;
            _ = PulsarModLoader.Content.Components.PolytechModule.PolytechModuleModManager.Instance;
            _ = PulsarModLoader.Content.Components.Reactor.ReactorModManager.Instance;
            _ = PulsarModLoader.Content.Components.Shield.ShieldModManager.Instance;
            _ = PulsarModLoader.Content.Components.Thruster.ThrusterModManager.Instance;
            _ = PulsarModLoader.Content.Components.Turret.TurretModManager.Instance;
            _ = PulsarModLoader.Content.Components.Virus.VirusModManager.Instance;
            _ = PulsarModLoader.Content.Components.WarpDrive.WarpDriveModManager.Instance;
            _ = PulsarModLoader.Content.Components.WarpDriveProgram.WarpDriveProgramModManager.Instance;

            OnAllModsLoaded?.Invoke();
        }

        private Assembly ResolveModsDirectory(object sender, ResolveEventArgs args)
        {
            // Search for dependency in every mods directory loaded so far
            foreach (string modsDir in modDirectories)
            {
                string assemblyPath = Path.Combine(modsDir, new AssemblyName(args.Name).Name + ".dll");

                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
            }

            // Failed to find dependency!  Assemblies missing from mods directory?
            return null;
        }

        /// <summary>
        /// Loads a mod by assemblypath.
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <returns>PulsarMod of loaded mod</returns>
        /// <exception cref="IOException"></exception>
        public PulsarMod LoadMod(string assemblyPath)
        {

            if (!File.Exists(assemblyPath))
            {
                throw new IOException($"Couldn't find file: {assemblyPath}");
            }

            try
            {
                Assembly asm = Assembly.LoadFile(assemblyPath);
                Type modType = asm.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(PulsarMod)));

                if (modType != null)
                {
                    PulsarMod mod = Activator.CreateInstance(modType) as PulsarMod;
                    activeMods.Add(mod.Name, mod);
                    OnModSuccessfullyLoaded?.Invoke(mod.Name, mod);

                    Logger.Info($"Loaded mod: {mod.Name} Version {mod.Version} Author: {mod.Author}");
                    return mod;
                }
                else
                {
                    Logger.Info($"Skipping {Path.GetFileName(assemblyPath)}; couldn't find mod entry point.");

                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.Info($"Failed to load mod: {Path.GetFileName(assemblyPath)}\n{e}");

                return null;
            }
        }

        internal void UnloadMod(PulsarMod mod, ref Harmony harmony)
        {
            activeMods.Remove(mod.Name); // Removes selected mod from activeMods
            harmony.UnpatchAll(mod.HarmonyIdentifier()); // Removes all patches from selected mod
            OnModUnloaded?.Invoke(mod);
            Logger.Info($"Unloaded mod: {mod.Name} Version {mod.Version} Author: {mod.Author}");
            GC.Collect();
        }
    }
}
