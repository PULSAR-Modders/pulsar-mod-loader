using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PulsarModLoader.Content.Components.MissionShipComponent
{
    public class MissionShipComponentModManager
    {
        public readonly int VanillaMissionShipComponentMaxType = 0;
        private static MissionShipComponentModManager m_instance = null;
        public readonly List<MissionShipComponentMod> MissionShipComponentTypes = new List<MissionShipComponentMod>();
        public static MissionShipComponentModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MissionShipComponentModManager();
                }
                return m_instance;
            }
        }

        MissionShipComponentModManager()
        {
            VanillaMissionShipComponentMaxType = 13;
            Logger.Info($"MaxTypeint = {VanillaMissionShipComponentMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type MissionShipComponentMod = typeof(MissionShipComponentMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (MissionShipComponentMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading MissionShipComponent from assembly");
                        MissionShipComponentMod MissionShipComponentModHandler = (MissionShipComponentMod)Activator.CreateInstance(t);
                        if (GetMissionShipComponentIDFromName(MissionShipComponentModHandler.Name) == -1)
                        {
                            MissionShipComponentTypes.Add(MissionShipComponentModHandler);
                            Logger.Info($"Added MissionShipComponent: '{MissionShipComponentModHandler.Name}' with ID '{GetMissionShipComponentIDFromName(MissionShipComponentModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add MissionShipComponent from {mod.Name} with the duplicate name of '{MissionShipComponentModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds MissionShipComponent type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find MissionShipComponent.
        /// </summary>
        /// <param name="MissionShipComponentName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetMissionShipComponentIDFromName(string MissionShipComponentName)
        {
            for (int i = 0; i < MissionShipComponentTypes.Count; i++)
            {
                if (MissionShipComponentTypes[i].Name == MissionShipComponentName)
                {
                    return i + VanillaMissionShipComponentMaxType;
                }
            }
            return -1;
        }
        public static PLMissionShipComponent CreateMissionShipComponent(int Subtype, int level)
        {
            PLMissionShipComponent InMissionShipComponent;
            if (Subtype >= Instance.VanillaMissionShipComponentMaxType)
            {
                InMissionShipComponent = new PLMissionShipComponent(0, level);
                int subtypeformodded = Subtype - Instance.VanillaMissionShipComponentMaxType;
                if (subtypeformodded <= Instance.MissionShipComponentTypes.Count && subtypeformodded > -1)
                {
                    MissionShipComponentMod MissionShipComponentType = Instance.MissionShipComponentTypes[Subtype - Instance.VanillaMissionShipComponentMaxType];
                    InMissionShipComponent.SubType = Subtype;
                    InMissionShipComponent.Name = MissionShipComponentType.Name;
                    InMissionShipComponent.Desc = MissionShipComponentType.Description;
                    InMissionShipComponent.m_IconTexture = MissionShipComponentType.IconTexture;
                    InMissionShipComponent.m_MarketPrice = MissionShipComponentType.MarketPrice;
                    InMissionShipComponent.CargoVisualPrefabID = MissionShipComponentType.CargoVisualID;
                    InMissionShipComponent.CanBeDroppedOnShipDeath = MissionShipComponentType.CanBeDroppedOnShipDeath;
                    InMissionShipComponent.Experimental = MissionShipComponentType.Experimental;
                    InMissionShipComponent.Unstable = MissionShipComponentType.Unstable;
                    InMissionShipComponent.Contraband = MissionShipComponentType.Contraband;
                    InMissionShipComponent.Price_LevelMultiplierExponent = MissionShipComponentType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InMissionShipComponent = new PLMissionShipComponent(Subtype, level);
            }
            return InMissionShipComponent;
        }
    }
    //Converts hashes to MissionShipComponents.
    [HarmonyPatch(typeof(PLMissionShipComponent), "CreateMissionComponentFromHash")]
    class MissionShipComponentHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = MissionShipComponentModManager.CreateMissionShipComponent(inSubType, inLevel);
            return false;
        }
    }
}
