using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.MegaTurret
{
    public class MegaTurretModManager
    {
        public readonly int VanillaMegaTurretMaxType = 0;
        private static MegaTurretModManager m_instance = null;
        public readonly List<MegaTurretMod> MegaTurretTypes = new List<MegaTurretMod>();
        public static MegaTurretModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MegaTurretModManager();
                }
                return m_instance;
            }
        }

        MegaTurretModManager()
        {
            VanillaMegaTurretMaxType = 7;
            Logger.Info($"MaxTypeint = {VanillaMegaTurretMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type MegaTurretMod = typeof(MegaTurretMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (MegaTurretMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading MegaTurret from assembly");
                        MegaTurretMod MegaTurretModHandler = (MegaTurretMod)Activator.CreateInstance(t);
                        if (GetMegaTurretIDFromName(MegaTurretModHandler.Name) == -1)
                        {
                            MegaTurretTypes.Add(MegaTurretModHandler);
                            Logger.Info($"Added MegaTurret: '{MegaTurretModHandler.Name}' with ID '{GetMegaTurretIDFromName(MegaTurretModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add MegaTurret from {mod.Name} with the duplicate name of '{MegaTurretModHandler.Name}'");
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
            int subtypeformodded = inSubType - MegaTurretModManager.Instance.VanillaMegaTurretMaxType;
            if (subtypeformodded <= MegaTurretModManager.Instance.MegaTurretTypes.Count && subtypeformodded > -1)
            {
                Logger.Info("Creating MegaTurret from list info");
                __result = MegaTurretModManager.Instance.MegaTurretTypes[subtypeformodded].PLMegaTurret;
                __result.Level = inLevel;
                return false;
            }
            return true;
        }
    }
}
