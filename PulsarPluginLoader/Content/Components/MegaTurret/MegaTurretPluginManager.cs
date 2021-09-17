using HarmonyLib;
using PulsarModLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using Logger = PulsarModLoader.Utilities.Logger;

namespace PulsarModLoader.Content.Components.MegaTurret
{
    public class MegaTurretPluginManager
    {
        public readonly int VanillaMegaTurretMaxType = 0;
        private static MegaTurretPluginManager m_instance = null;
        public readonly List<MegaTurretPlugin> MegaTurretTypes = new List<MegaTurretPlugin>();
        public static MegaTurretPluginManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MegaTurretPluginManager();
                }
                return m_instance;
            }
        }

        MegaTurretPluginManager()
        {
            VanillaMegaTurretMaxType = 7;
            Logger.Info($"MaxTypeint = {VanillaMegaTurretMaxType - 1}");
            foreach (PulsarMod plugin in PluginManager.Instance.GetAllPlugins())
            {
                Assembly asm = plugin.GetType().Assembly;
                Type MegaTurretPlugin = typeof(MegaTurretPlugin);
                foreach (Type t in asm.GetTypes())
                {
                    if (MegaTurretPlugin.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading MegaTurret from assembly");
                        MegaTurretPlugin MegaTurretPluginHandler = (MegaTurretPlugin)Activator.CreateInstance(t);
                        if (GetMegaTurretIDFromName(MegaTurretPluginHandler.Name) == -1)
                        {
                            MegaTurretTypes.Add(MegaTurretPluginHandler);
                            Logger.Info($"Added MegaTurret: '{MegaTurretPluginHandler.Name}' with ID '{GetMegaTurretIDFromName(MegaTurretPluginHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add MegaTurret from {plugin.Name} with the duplicate name of '{MegaTurretPluginHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds MegaTurret type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find MegaTurret.
        /// </summary>
        /// <param name="MegaTurretName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetMegaTurretIDFromName(string MegaTurretName)
        {
            for (int i = 0; i < MegaTurretTypes.Count; i++)
            {
                if (MegaTurretTypes[i].Name == MegaTurretName)
                {
                    return i + VanillaMegaTurretMaxType;
                }
            }
            return -1;
        }
    }
    //Converts hashes to MegaTurrets.
    [HarmonyPatch(typeof(PLMegaTurret), "CreateMainTurretFromHash")]
    class MegaTurretHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            int subtypeformodded = inSubType - MegaTurretPluginManager.Instance.VanillaMegaTurretMaxType;
            if (subtypeformodded <= MegaTurretPluginManager.Instance.MegaTurretTypes.Count && subtypeformodded > -1)
            {
                Logger.Info("Creating MegaTurret from list info");
                __result = MegaTurretPluginManager.Instance.MegaTurretTypes[subtypeformodded].PLMegaTurret;
                __result.Level = inLevel;
                return false;
            }
            return true;
        }
    }
}
