using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarPluginLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using Logger = PulsarPluginLoader.Utilities.Logger;

namespace PulsarPluginLoader.Content.Components.InertiaThruster
{
    public class InertiaThrusterPluginManager
    {
        public readonly int VanillaInertiaThrusterMaxType = 0;
        private static InertiaThrusterPluginManager m_instance = null;
        public readonly List<InertiaThrusterPlugin> InertiaThrusterTypes = new List<InertiaThrusterPlugin>();
        public static InertiaThrusterPluginManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new InertiaThrusterPluginManager();
                }
                return m_instance;
            }
        }

        InertiaThrusterPluginManager()
        {
            VanillaInertiaThrusterMaxType = Enum.GetValues(typeof(EInertiaThrusterType)).Length;
            Logger.Info($"MaxTypeint = {VanillaInertiaThrusterMaxType - 1}");
            foreach (PulsarPlugin plugin in PluginManager.Instance.GetAllPlugins())
            {
                Assembly asm = plugin.GetType().Assembly;
                Type InertiaThrusterPlugin = typeof(InertiaThrusterPlugin);
                foreach (Type t in asm.GetTypes())
                {
                    if (InertiaThrusterPlugin.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading InertiaThruster from assembly");
                        InertiaThrusterPlugin InertiaThrusterPluginHandler = (InertiaThrusterPlugin)Activator.CreateInstance(t);
                        if (GetInertiaThrusterIDFromName(InertiaThrusterPluginHandler.Name) == -1)
                        {
                            InertiaThrusterTypes.Add(InertiaThrusterPluginHandler);
                            Logger.Info($"Added InertiaThruster: '{InertiaThrusterPluginHandler.Name}' with ID '{GetInertiaThrusterIDFromName(InertiaThrusterPluginHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add InertiaThruster from {plugin.Name} with the duplicate name of '{InertiaThrusterPluginHandler.Name}'");
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
                /*if (Global.DebugLogging)
                {
                    Logger.Info($"Subtype for modded is {subtypeformodded}");
                }*/
                if (subtypeformodded <= Instance.InertiaThrusterTypes.Count && subtypeformodded > -1)
                {
                    /*if (Global.DebugLogging)
                    {
                        Logger.Info("Creating InertiaThruster from list info");
                    }*/
                    InertiaThrusterPlugin InertiaThrusterType = Instance.InertiaThrusterTypes[Subtype - Instance.VanillaInertiaThrusterMaxType];
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
                    InInertiaThruster.GetType().GetField("Price_LevelMultiplierExponent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(InInertiaThruster, (ObscuredFloat)InertiaThrusterType.Price_LevelMultiplierExponent);
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
            __result = InertiaThrusterPluginManager.CreateInertiaThruster(inSubType, inLevel);
            return false;
        }
    }
}
