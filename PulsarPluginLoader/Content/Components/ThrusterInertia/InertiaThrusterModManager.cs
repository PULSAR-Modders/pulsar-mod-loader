using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using Logger = PulsarModLoader.Utilities.Logger;

namespace PulsarModLoader.Content.Components.InertiaThruster
{
    public class InertiaThrusterModManager
    {
        public readonly int VanillaInertiaThrusterMaxType = 0;
        private static InertiaThrusterModManager m_instance = null;
        public readonly List<InertiaThrusterMod> InertiaThrusterTypes = new List<InertiaThrusterMod>();
        public static InertiaThrusterModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new InertiaThrusterModManager();
                }
                return m_instance;
            }
        }

        InertiaThrusterModManager()
        {
            VanillaInertiaThrusterMaxType = Enum.GetValues(typeof(EInertiaThrusterType)).Length;
            Logger.Info($"MaxTypeint = {VanillaInertiaThrusterMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type InertiaThrusterMod = typeof(InertiaThrusterMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (InertiaThrusterMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading InertiaThruster from assembly");
                        InertiaThrusterMod InertiaThrusterModHandler = (InertiaThrusterMod)Activator.CreateInstance(t);
                        if (GetInertiaThrusterIDFromName(InertiaThrusterModHandler.Name) == -1)
                        {
                            InertiaThrusterTypes.Add(InertiaThrusterModHandler);
                            Logger.Info($"Added InertiaThruster: '{InertiaThrusterModHandler.Name}' with ID '{GetInertiaThrusterIDFromName(InertiaThrusterModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add InertiaThruster from {mod.Name} with the duplicate name of '{InertiaThrusterModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds InertiaThruster type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find InertiaThruster.
        /// </summary>
        /// <param name="InertiaThrusterName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetInertiaThrusterIDFromName(string InertiaThrusterName)
        {
            for (int i = 0; i < InertiaThrusterTypes.Count; i++)
            {
                if (InertiaThrusterTypes[i].Name == InertiaThrusterName)
                {
                    return i + VanillaInertiaThrusterMaxType;
                }
            }
            return -1;
        }
        public static PLInertiaThruster CreateInertiaThruster(int Subtype, int level)
        {
            PLInertiaThruster InInertiaThruster;
            if (Subtype >= Instance.VanillaInertiaThrusterMaxType)
            {
                InInertiaThruster = new PLInertiaThruster(EInertiaThrusterType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaInertiaThrusterMaxType;
                if (subtypeformodded <= Instance.InertiaThrusterTypes.Count && subtypeformodded > -1)
                {
                    InertiaThrusterMod InertiaThrusterType = Instance.InertiaThrusterTypes[Subtype - Instance.VanillaInertiaThrusterMaxType];
                    InInertiaThruster.SubType = Subtype;
                    InInertiaThruster.Name = InertiaThrusterType.Name;
                    InInertiaThruster.Desc = InertiaThrusterType.Description;
                    InInertiaThruster.GetType().GetField("m_IconTexture", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InInertiaThruster, InertiaThrusterType.IconTexture);
                    InInertiaThruster.GetType().GetField("m_MaxOutput", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InInertiaThruster, InertiaThrusterType.MaxOutput);
                    InInertiaThruster.GetType().GetField("m_MaxPowerUsage_Watts", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InInertiaThruster, InertiaThrusterType.MaxPowerUsage_Watts);
                    InInertiaThruster.GetType().GetField("m_MarketPrice", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InInertiaThruster, (ObscuredInt)InertiaThrusterType.MarketPrice);
                    InInertiaThruster.CargoVisualPrefabID = InertiaThrusterType.CargoVisualID;
                    InInertiaThruster.CanBeDroppedOnShipDeath = InertiaThrusterType.CanBeDroppedOnShipDeath;
                    InInertiaThruster.Experimental = InertiaThrusterType.Experimental;
                    InInertiaThruster.Unstable = InertiaThrusterType.Unstable;
                    InInertiaThruster.Contraband = InertiaThrusterType.Contraband;
                    InInertiaThruster.GetType().GetMethod("UpdateMaxPowerWatts", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(InInertiaThruster, new object[0]);
                    InInertiaThruster.GetType().GetField("Price_LevelMultiplierExponent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InInertiaThruster, InertiaThrusterType.Price_LevelMultiplierExponent);
                }
            }
            else
            {
                InInertiaThruster = new PLInertiaThruster((EInertiaThrusterType)Subtype, level);
            }
            return InInertiaThruster;
        }
    }
    //Converts hashes to InertiaThrusters.
    [HarmonyPatch(typeof(PLInertiaThruster), "CreateInertiaThrusterFromHash")]
    class InertiaThrusterHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = InertiaThrusterModManager.CreateInertiaThruster(inSubType, inLevel);
            return false;
        }
    }
}
