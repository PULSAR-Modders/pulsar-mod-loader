using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.Missile
{
    public class MissileModManager
    {
        public readonly int VanillaMissileMaxType = 0;
        private static MissileModManager m_instance = null;
        public readonly List<MissileMod> MissileTypes = new List<MissileMod>();
        public static MissileModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MissileModManager();
                }
                return m_instance;
            }
        }

        MissileModManager()
        {
            VanillaMissileMaxType = Enum.GetValues(typeof(ETrackerMissileType)).Length;
            Logger.Info($"MaxTypeint = {VanillaMissileMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type MissileMod = typeof(MissileMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (MissileMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading Missile from assembly");
                        MissileMod MissileModHandler = (MissileMod)Activator.CreateInstance(t);
                        if (GetMissileIDFromName(MissileModHandler.Name) == -1)
                        {
                            MissileTypes.Add(MissileModHandler);
                            Logger.Info($"Added Missile: '{MissileModHandler.Name}' with ID '{GetMissileIDFromName(MissileModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add Missile from {mod.Name} with the duplicate name of '{MissileModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds Missile type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Missile.
        /// </summary>
        /// <param name="MissileName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetMissileIDFromName(string MissileName)
        {
            for (int i = 0; i < MissileTypes.Count; i++)
            {
                if (MissileTypes[i].Name == MissileName)
                {
                    return i + VanillaMissileMaxType;
                }
            }
            return -1;
        }
        public static PLTrackerMissile CreateMissile(int Subtype, int level, int inSubTypeData = 0)
        {
            PLTrackerMissile InMissile;
            if (Subtype >= Instance.VanillaMissileMaxType)
            {
                InMissile = new PLTrackerMissile(ETrackerMissileType.MAX, level, inSubTypeData);
                int subtypeformodded = Subtype - Instance.VanillaMissileMaxType;
                if (subtypeformodded <= Instance.MissileTypes.Count && subtypeformodded > -1)
                {
                    MissileMod MissileType = Instance.MissileTypes[Subtype - Instance.VanillaMissileMaxType];
                    InMissile.SubType = Subtype;
                    InMissile.Name = MissileType.Name;
                    InMissile.Desc = MissileType.Description;
                    InMissile.m_IconTexture = MissileType.IconTexture;
                    InMissile.Damage = MissileType.Damage;
                    InMissile.Speed = MissileType.Speed;
                    InMissile.DamageType = MissileType.DamageType;
                    InMissile.MissileRefillPrice = MissileType.MissileRefillPrice;
                    InMissile.AmmoCapacity = MissileType.AmmoCapacity;
                    InMissile.PrefabID = MissileType.PrefabID;
                    InMissile.m_MarketPrice = MissileType.MarketPrice;
                    InMissile.CargoVisualPrefabID = MissileType.CargoVisualID;
                    InMissile.CanBeDroppedOnShipDeath = MissileType.CanBeDroppedOnShipDeath;
                    InMissile.Experimental = MissileType.Experimental;
                    InMissile.Unstable = MissileType.Unstable;
                    InMissile.Contraband = MissileType.Contraband;
                    InMissile.Price_LevelMultiplierExponent = MissileType.Price_LevelMultiplierExponent;
                    if (PhotonNetwork.isMasterClient)
                    {
                        InMissile.SubTypeData = (short)InMissile.AmmoCapacity;
                    }
                }
            }
            else
            {
                InMissile = new PLTrackerMissile((ETrackerMissileType)Subtype, level, inSubTypeData);
            }
            return InMissile;
        }
    }
    //Converts hashes to Missiles.
    [HarmonyPatch(typeof(PLTrackerMissile), "CreateTrackerMissileFromHash")]
    class MissileHashFix
    {
        static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
        {
            __result = MissileModManager.CreateMissile(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "LateAddStats")]
    class TrackerMissileLateAddStatsPatch
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance)
        {
            if(__instance is PLTrackerMissile)
            {
                int subtypeformodded = __instance.SubType - MissileModManager.Instance.VanillaTrackerMissileMaxType;
                if (subtypeformodded > -1 && subtypeformodded < MissileModManager.Instance.MissileTypes.Count && inStats != null)
                {
                    MissileModManager.Instance.MissileTypes[subtypeformodded].LateAddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "AddStats")]
    class MissileAddStats
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance) 
        {
            if(__instance is PLTrackerMissile) 
            {
                int subtypeformodded = __instance.SubType - MissileModManager.Instance.VanillaTrackerMissileMaxType;
                if (subtypeformodded > -1 && subtypeformodded < MissileModManager.Instance.MissileTypes.Count && inStats != null)
                {
                    MissileModManager.Instance.MissileTypes[subtypeformodded].AddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "Tick")]
    class MissileTick
    {
        static void Postfix(PLShipComponent __instance)
        {
            if (__instance is PLTrackerMissile)
            {
                int subtypeformodded = __instance.SubType - MissileModManager.Instance.VanillaTrackerMissileMaxType;
                if (subtypeformodded > -1 && subtypeformodded < MissileModManager.Instance.MissileTypes.Count)
                {
                    MissileModManager.Instance.MissileTypes[subtypeformodded].Tick(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "FinalLateAddStats")]
    class MissileFinalLateAddStats
    {
        static void Postfix(PLShipStats inStats, PLShipComponent __instance)
        {
            if (__instance is PLTrackerMissile)
            {
                int subtypeformodded = __instance.SubType - MissileModManager.Instance.VanillaTrackerMissileMaxType;
                if (subtypeformodded > -1 && subtypeformodded < MissileModManager.Instance.MissileTypes.Count && inStats != null)
                {
                    MissileModManager.Instance.MissileTypes[subtypeformodded].FinalLateAddStats(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "GetStatLineLeft")]
    class MissileGetStatLineLeft
    {
        static void Postfix(ref string __result, PLShipComponent __instance)
        {
            if (__instance is PLTrackerMissile)
            {
                int subtypeformodded = __instance.SubType - MissileModManager.Instance.VanillaTrackerMissileMaxType;
                if (subtypeformodded > -1 && subtypeformodded < MissileModManager.Instance.MissileTypes.Count)
                {
                    __result = MissileModManager.Instance.MissileTypes[subtypeformodded].GetStatLineLeft(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "GetStatLineRight")]
    class MissileGetStatLineRight
    {
        static void Postfix(ref string __result, PLShipComponent __instance)
        {
            if (__instance is PLTrackerMissile)
            {
                int subtypeformodded = __instance.SubType - MissileModManager.Instance.VanillaTrackerMissileMaxType;
                if (subtypeformodded > -1 && subtypeformodded < MissileModManager.Instance.MissileTypes.Count)
                {
                    __result = MissileModManager.Instance.MissileTypes[subtypeformodded].GetStatLineRight(__instance);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLShipComponent), "OnWarp")]
    class MissileOnWarp
    {
        static void Postfix(PLShipComponent __instance)
        {
            if (__instance is PLTrackerMissile)
            {
                int subtypeformodded = __instance.SubType - MissileModManager.Instance.VanillaTrackerMissileMaxType;
                if (subtypeformodded > -1 && subtypeformodded < MissileModManager.Instance.MissileTypes.Count)
                {
                    MissileModManager.Instance.MissileTypes[subtypeformodded].OnWarp(__instance);
                }
            }
        }
    }
}
