using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.AutoTurret
{
    /// <summary>
    /// Manages Modded AutoTurrets
    /// </summary>
    public class AutoTurretModManager
    {
        readonly int VanillaAutoTurretMaxType = 0;
        private static AutoTurretModManager m_instance = null;
        readonly List<AutoTurretMod> AutoTurretTypes = new List<AutoTurretMod>();

        /// <summary>
        /// Static Manager Instance
        /// </summary>
        public static AutoTurretModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new AutoTurretModManager();
                }
                return m_instance;
            }
        }

        AutoTurretModManager()
        {
            VanillaAutoTurretMaxType = 1;
            Logger.Info($"MaxTypeint = {VanillaAutoTurretMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type AutoTurretMod = typeof(AutoTurretMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (AutoTurretMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading AutoTurret from assembly");
                        AutoTurretMod AutoTurretModHandler = (AutoTurretMod)Activator.CreateInstance(t);
                        if (GetAutoTurretIDFromName(AutoTurretModHandler.Name) == -1)
                        {
                            AutoTurretTypes.Add(AutoTurretModHandler);
                            Logger.Info($"Added AutoTurret: '{AutoTurretModHandler.Name}' with ID '{GetAutoTurretIDFromName(AutoTurretModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add AutoTurret from {mod.Name} with the duplicate name of '{AutoTurretModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds AutoTurret type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find AutoTurret.
        /// </summary>
        /// <param name="AutoTurretName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetAutoTurretIDFromName(string AutoTurretName)
        {
            for (int i = 0; i < AutoTurretTypes.Count; i++)
            {
                if (AutoTurretTypes[i].Name == AutoTurretName)
                {
                    return i + VanillaAutoTurretMaxType;
                }
            }
            return -1;
        }

        //Converts hashes to AutoTurrets.
        [HarmonyPatch(typeof(PLAutoTurret), "CreateAutoTurretFromHash")]
        class AutoTurretHashFix
        {
            static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
            {
                int subtypeformodded = inSubType - AutoTurretModManager.Instance.VanillaAutoTurretMaxType;
                if (subtypeformodded <= AutoTurretModManager.Instance.AutoTurretTypes.Count && subtypeformodded > -1)
                {
                    Logger.Info("Creating AutoTurret from list info");
                    __result = AutoTurretModManager.Instance.AutoTurretTypes[subtypeformodded].PLAutoTurret;
                    __result.SubType = inSubType;
                    __result.Level = inLevel;
                    return false;
                }
                return true;
            }
        }
    }
}
