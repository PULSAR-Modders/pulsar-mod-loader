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
        /// <summary>
        /// Not Implemented.
        /// </summary>
        public static bool IsOldVersion;

        /// <summary>
        /// Called on mod load.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="mod"></param>
        public delegate void ModLoaded(string name, PulsarMod mod);

        /// <summary>
        /// Called on mod unload
        /// </summary>
        /// <param name="mod"></param>
        public delegate void ModUnloaded(PulsarMod mod);

        /// <summary>
        /// Called after all mods loaded
        /// </summary>
        public delegate void AllModsLoaded();

        /// <summary>
        /// Called on successfull mod load.
        /// </summary>
        public event ModLoaded OnModSuccessfullyLoaded;

        /// <summary>
        /// Called on mod unload
        /// </summary>
        public event ModUnloaded OnModUnloaded;

        /// <summary>
        /// Called after all mods loaded
        /// </summary>
        public event AllModsLoaded OnAllModsLoaded;

        /// <summary>
        /// PMLVersionInfo
        /// </summary>
        public FileVersionInfo PMLVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

        private readonly Dictionary<string, PulsarMod> activeMods;
        private readonly HashSet<string> modDirectories;

        internal List<ModUpdateCheck.UpdateModInfo> UpdatesAviable = new List<ModUpdateCheck.UpdateModInfo>(2);

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
            if ((PMLConfig.LastPMLUpdateCheck.Value.Day - DateTime.Today.Day) == 0) return;
			PMLConfig.LastPMLUpdateCheck.Value = DateTime.Today;

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
        /// Gets the PulsarMod Class of the given mod name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns PulsarMod of mod.</returns>
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
        /// Gets the PulsarMod Class of the given mod via the HarmonyID.
        /// </summary>
        /// <param name="HarmonyID"></param>
        /// <returns>Returns PulsarMod of mod.</returns>
        public PulsarMod GetModByHarmonyID(string HarmonyID)
        {
            foreach (PulsarMod mod in activeMods.Values ) 
            { 
                if(mod.HarmonyIdentifier() == HarmonyID)
                {
                    return mod;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if mod with mod name is loaded.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns true if loaded</returns>
        public bool IsModLoaded(string name)
        {
            return activeMods.ContainsKey(name);
        }

        /// <summary>
        /// Checks if mod with HarmonyID is loaded.
        /// </summary>
        /// <param name="HarmonyID"></param>
        /// <returns>Returns true if loaded</returns>
        public bool IsModLoadedByHarmonyID(string HarmonyID)
        {
            foreach (PulsarMod mod in activeMods.Values)
            {
                if (mod.HarmonyIdentifier() == HarmonyID)
                {
                    return true;
                }
            }
            return false;
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
                Assembly asm = Assembly.Load(File.ReadAllBytes(assemblyPath)); // load as bytes to avoid locking the file
				Type modType = asm.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(PulsarMod)));

                if (modType != null)
                {
                    PulsarMod mod = Activator.CreateInstance(modType) as PulsarMod;
					mod.VersionInfo = FileVersionInfo.GetVersionInfo(assemblyPath);
					activeMods.Add(mod.Name, mod);
                    OnModSuccessfullyLoaded?.Invoke(mod.Name, mod);

                    Logger.Info($"Loaded mod: {mod.Name} Version {mod.Version} Author: {mod.Author}");

                    if (ModUpdateCheck.IsUpdateAviable(mod))
                        Logger.Info($"↑ ↑ ↑ !This mod is outdated! ↑ ↑ ↑");

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
