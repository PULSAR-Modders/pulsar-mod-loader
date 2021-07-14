using HarmonyLib;
using PulsarPluginLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using Logger = PulsarPluginLoader.Utilities.Logger;

namespace PulsarPluginLoader.Content.Components.Turret
{
    public class TurretPluginManager
    {
        public readonly int VanillaTurretMaxType = 0;
        private static TurretPluginManager m_instance = null;
        public readonly List<TurretPlugin> TurretTypes = new List<TurretPlugin>();
        public static TurretPluginManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new TurretPluginManager();
                }
                return m_instance;
            }
        }

        TurretPluginManager()
        {
            VanillaTurretMaxType = 19;
            Logger.Info($"MaxTypeint = {VanillaTurretMaxType - 1}");
            foreach (PulsarPlugin plugin in PluginManager.Instance.GetAllPlugins())
            {
                Assembly asm = plugin.GetType().Assembly;
                Type TurretPlugin = typeof(TurretPlugin);
                foreach (Type t in asm.GetTypes())
                {
                    if (TurretPlugin.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading Turret from assembly");
                        TurretPlugin TurretPluginHandler = (TurretPlugin)Activator.CreateInstance(t);
                        if (GetTurretIDFromName(TurretPluginHandler.Name) == -1)
                        {
                            TurretTypes.Add(TurretPluginHandler);
                            Logger.Info($"Added Turret: '{TurretPluginHandler.Name}' with ID '{GetTurretIDFromName(TurretPluginHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add Turret from {plugin.Name} with the duplicate name of '{TurretPluginHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds Turret type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Turret.
        /// </summary>
        /// <param name="TurretName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetTurretIDFromName(string TurretName)
        {
            for (int i = 0; i < TurretTypes.Count; i++)
            {
                if (TurretTypes[i].Name == TurretName)
                {
                    return i + VanillaTurretMaxType;
                }
            }
            return -1;
        }
    }
    //Converts hashes to Turrets.
    [HarmonyPatch(typeof(PLTurret), "CreateTurretFromHash")]
    class TurretHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            int subtypeformodded = inSubType - TurretPluginManager.Instance.VanillaTurretMaxType;
            if (subtypeformodded <= TurretPluginManager.Instance.TurretTypes.Count && subtypeformodded > -1)
            {
                Logger.Info("Creating Turret from list info");
                __result = TurretPluginManager.Instance.TurretTypes[subtypeformodded].PLTurret;
                __result.Level = inLevel;
                return false;
            }
            return true;
        }
    }
    /*[HarmonyPatch(typeof(PLTurret), "LateAddStats")]
    class TurretLateAddStatsPatch
    {
        static void Postfix(PLShipStats inStats, PLTurret __instance)
        {
            int subtypeformodded = __instance.SubType - TurretPluginManager.Instance.VanillaTurretMaxType;
            if (subtypeformodded > -1 && subtypeformodded < TurretPluginManager.Instance.TurretTypes.Count && inStats != null)
            {
                TurretPluginManager.Instance.TurretTypes[subtypeformodded].LateAddStats(inStats);
            }
        }
    }*/
}
