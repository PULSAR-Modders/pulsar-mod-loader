using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.HullPlating
{
    public class HullPlatingModManager
    {
        public readonly int VanillaHullPlatingMaxType = 0;
        private static HullPlatingModManager m_instance = null;
        public readonly List<HullPlatingMod> HullPlatingTypes = new List<HullPlatingMod>();
        public static HullPlatingModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new HullPlatingModManager();
                }
                return m_instance;
            }
        }

        HullPlatingModManager()
        {
            VanillaHullPlatingMaxType = Enum.GetValues(typeof(ETrackerMissileType)).Length;
            Logger.Info($"MaxTypeint = {VanillaHullPlatingMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type HullPlatingMod = typeof(HullPlatingMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (HullPlatingMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading HullPlating from assembly");
                        HullPlatingMod HullPlatingModHandler = (HullPlatingMod)Activator.CreateInstance(t);
                        if (GetHullPlatingIDFromName(HullPlatingModHandler.Name) == -1)
                        {
                            HullPlatingTypes.Add(HullPlatingModHandler);
                            Logger.Info($"Added HullPlating: '{HullPlatingModHandler.Name}' with ID '{GetHullPlatingIDFromName(HullPlatingModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add HullPlating from {mod.Name} with the duplicate name of '{HullPlatingModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds HullPlating type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find HullPlating.
        /// </summary>
        /// <param name="HullPlatingName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetHullPlatingIDFromName(string HullPlatingName)
        {
            for (int i = 0; i < HullPlatingTypes.Count; i++)
            {
                if (HullPlatingTypes[i].Name == HullPlatingName)
                {
                    return i + VanillaHullPlatingMaxType;
                }
            }
            return -1;
        }
    }
    //Converts hashes to HullPlatings.
    [HarmonyPatch(typeof(PLHullPlating), "CreateHullPlatingFromHash")]
    class HullPlatingHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            int subtypeformodded = inSubType - HullPlatingModManager.Instance.VanillaHullPlatingMaxType;
            if (subtypeformodded <= HullPlatingModManager.Instance.HullPlatingTypes.Count && subtypeformodded > -1)
            {
                Logger.Info("Creating HullPlating from list info");
                __result = HullPlatingModManager.Instance.HullPlatingTypes[subtypeformodded].PLHullPlating;
                __result.SubType = inSubType;
                __result.Level = inLevel;
                return false;
            }
            return true;
        }
    }
}
