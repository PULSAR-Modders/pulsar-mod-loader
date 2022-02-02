using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components.CaptainsChair
{
    public class CaptainsChairModManager
    {
        public readonly int VanillaCaptainsChairMaxType = 0;
        private static CaptainsChairModManager m_instance = null;
        public readonly List<CaptainsChairMod> CaptainsChairTypes = new List<CaptainsChairMod>();
        public static CaptainsChairModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new CaptainsChairModManager();
                }
                return m_instance;
            }
        }

        CaptainsChairModManager()
        {
            VanillaCaptainsChairMaxType = Enum.GetValues(typeof(ECaptainsChairType)).Length;
            Logger.Info($"MaxTypeint = {VanillaCaptainsChairMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type CaptainsChairMod = typeof(CaptainsChairMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (CaptainsChairMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading CaptainsChair from assembly");
                        CaptainsChairMod CaptainsChairModHandler = (CaptainsChairMod)Activator.CreateInstance(t);
                        if (GetCaptainsChairIDFromName(CaptainsChairModHandler.Name) == -1)
                        {
                            CaptainsChairTypes.Add(CaptainsChairModHandler);
                            Logger.Info($"Added CaptainsChair: '{CaptainsChairModHandler.Name}' with ID '{GetCaptainsChairIDFromName(CaptainsChairModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add CaptainsChair from {mod.Name} with the duplicate name of '{CaptainsChairModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds CaptainsChair type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find CaptainsChair.
        /// </summary>
        /// <param name="CaptainsChairName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetCaptainsChairIDFromName(string CaptainsChairName)
        {
            for (int i = 0; i < CaptainsChairTypes.Count; i++)
            {
                if (CaptainsChairTypes[i].Name == CaptainsChairName)
                {
                    return i + VanillaCaptainsChairMaxType;
                }
            }
            return -1;
        }
        public static PLCaptainsChair CreateCaptainsChair(int Subtype, int level)
        {
            PLCaptainsChair InCaptainsChair;
            if (Subtype >= Instance.VanillaCaptainsChairMaxType)
            {
                InCaptainsChair = new PLCaptainsChair(ECaptainsChairType.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaCaptainsChairMaxType;
                if (subtypeformodded <= Instance.CaptainsChairTypes.Count && subtypeformodded > -1)
                {
                    CaptainsChairMod CaptainsChairType = Instance.CaptainsChairTypes[Subtype - Instance.VanillaCaptainsChairMaxType];
                    InCaptainsChair.SubType = Subtype;
                    InCaptainsChair.Name = CaptainsChairType.Name;
                    InCaptainsChair.Desc = CaptainsChairType.Description;
                    InCaptainsChair.m_IconTexture = CaptainsChairType.IconTexture;
                    InCaptainsChair.m_MarketPrice = CaptainsChairType.MarketPrice;
                    InCaptainsChair.CargoVisualPrefabID = CaptainsChairType.CargoVisualID;
                    InCaptainsChair.CanBeDroppedOnShipDeath = CaptainsChairType.CanBeDroppedOnShipDeath;
                    InCaptainsChair.Experimental = CaptainsChairType.Experimental;
                    InCaptainsChair.Unstable = CaptainsChairType.Unstable;
                    InCaptainsChair.Contraband = CaptainsChairType.Contraband;
                    InCaptainsChair.Price_LevelMultiplierExponent = CaptainsChairType.Price_LevelMultiplierExponent;
                }
            }
            else
            {
                InCaptainsChair = new PLCaptainsChair((ECaptainsChairType)Subtype, level);
            }
            return InCaptainsChair;
        }
    }
    //Converts hashes to CaptainsChairs.
    [HarmonyPatch(typeof(PLCaptainsChair), "CreateCaptainsChairFromHash")]
    class CaptainsChairHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = CaptainsChairModManager.CreateCaptainsChair(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLCaptainsChair), "LateAddStats")]
    class CaptainsChairLateAddStatsPatch
    {
        static void Postfix(PLShipStats inStats, PLCaptainsChair __instance)
        {
            int subtypeformodded = __instance.SubType - CaptainsChairModManager.Instance.VanillaCaptainsChairMaxType;
            if (subtypeformodded > -1 && subtypeformodded < CaptainsChairModManager.Instance.CaptainsChairTypes.Count && inStats != null)
            {
                CaptainsChairModManager.Instance.CaptainsChairTypes[subtypeformodded].LateAddStats(__instance);
            }
        }
    }
}
