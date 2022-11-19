using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.Turret
{
    public class TurretModManager
    {
        public readonly int VanillaTurretMaxType = 0;
        private static TurretModManager m_instance = null;
        public readonly List<TurretMod> TurretTypes = new List<TurretMod>();
        public static TurretModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new TurretModManager();
                }
                return m_instance;
            }
        }

        TurretModManager()
        {
            VanillaTurretMaxType = Enum.GetValues(typeof(ETurretType)).Length;
            Logger.Info($"MaxTypeint = {VanillaTurretMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type TurretMod = typeof(TurretMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (TurretMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading Turret from assembly");
                        TurretMod TurretModHandler = (TurretMod)Activator.CreateInstance(t);
                        if (GetTurretIDFromName(TurretModHandler.Name) == -1)
                        {
                            TurretTypes.Add(TurretModHandler);
                            Logger.Info($"Added Turret: '{TurretModHandler.Name}' with ID '{GetTurretIDFromName(TurretModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add Turret from {mod.Name} with the duplicate name of '{TurretModHandler.Name}'");
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
            int subtypeformodded = inSubType - TurretModManager.Instance.VanillaTurretMaxType;
            if (subtypeformodded <= TurretModManager.Instance.TurretTypes.Count && subtypeformodded > -1)
            {
                Logger.Info("Creating Turret from list info");
                __result = TurretModManager.Instance.TurretTypes[subtypeformodded].PLTurret;
                __result.SubType = inSubType;
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
            int subtypeformodded = __instance.SubType - TurretModManager.Instance.VanillaTurretMaxType;
            if (subtypeformodded > -1 && subtypeformodded < TurretModManager.Instance.TurretTypes.Count && inStats != null)
            {
                TurretModManager.Instance.TurretTypes[subtypeformodded].LateAddStats(inStats);
            }
        }
    }*/
}
