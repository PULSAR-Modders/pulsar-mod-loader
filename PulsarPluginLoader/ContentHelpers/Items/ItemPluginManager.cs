using HarmonyLib;
using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PulsarPluginLoader.ContentHelpers.Items
{
    public class ItemPluginManager
    {
        public readonly int VanillaItemMaxType = 0;
        private static ItemPluginManager m_instance = null;
        public readonly List<ItemPlugin> ItemTypes = new List<ItemPlugin>();
        public static ItemPluginManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ItemPluginManager();
                }
                return m_instance;
            }
        }
        ItemPluginManager()
        {
            VanillaItemMaxType = Enum.GetValues(typeof(EPawnItemType)).Length;
            Logger.Info($"ItemMaxTypeint = {VanillaItemMaxType - 1}");
            foreach (PulsarPlugin plugin in PluginManager.Instance.GetAllPlugins())
            {
                Assembly asm = plugin.GetType().Assembly;
                Type ItemPlugin = typeof(ItemPlugin);
                foreach (Type t in asm.GetTypes())
                {
                    if (ItemPlugin.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading Item from assembly");
                        ItemPlugin ItemPluginHandler = (ItemPlugin)Activator.CreateInstance(t);
                        GetItemIDsFromName(ItemPluginHandler.Name, out int MainType, out int SubType);
                        if (MainType == -1)
                        {
                            ItemTypes.Add(ItemPluginHandler);
                            GetItemIDsFromName(ItemPluginHandler.Name, out MainType, out SubType);
                            Logger.Info($"Added Item: '{ItemPluginHandler.Name}' with MainTypeID '{MainType}' and SubTypeID {SubType}");
                        }
                        else
                        {
                            Logger.Info($"Could not add Item from {plugin.Name} with the duplicate name of '{ItemPluginHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds Item type equivilent to given name and returns MainType ID and SubType ID needed to spawn. Returns -1 if couldn't find Item.
        /// </summary>
        /// <param name="ItemName">modded item's Name set in itemplugin.name</param>
        /// <returns>Maintype and Subtype</returns>
        public void GetItemIDsFromName(string ItemName, out int MainType, out int SubType)
        {
            MainType = -1;
            SubType = -1;
            for (int i = 0; i < ItemTypes.Count; i++)
            {
                if (ItemTypes[i].Name == ItemName)
                {
                    GetIntsFromIndex(i, out MainType, out SubType);
                    return;
                }
            }
        }
        /// <summary>
        /// Gets MainType and Subtype from modded index (internal thing made public)
        /// </summary>
        /// <param name="Index">Modded item index in modlist</param>
        /// <param name="MainType">MainType calculated from index</param>
        /// <param name="SubType">Subtype calculated from index</param>
        /// <returns>Maintype and Subtype</returns>
        public void GetIntsFromIndex(int Index, out int MainType, out int SubType)
        {
            SubType = Index % 64;
            MainType = VanillaItemMaxType + ((Index - SubType) / 64);
            Logger.Info($"Ids {MainType}, {SubType}");
            return;
        }
        /// <summary>
        /// Creates Vanilla and Modded PLPawnItems
        /// </summary>
        /// <param name="Maintype">Maintype</param>
        /// <param name="Subtype">Subtype</param>
        /// <param name="level">Level</param>
        /// <returns>Vanilla or Modded PLPawnItem</returns>
        public static PLPawnItem CreatePawnItem(int Maintype, int Subtype, int level)
        {
            PLPawnItem InItem = null;
            if (Subtype > 63)
            {
                /*if (Global.DebugLogging)
                {
                    Logger.Info("UpdateItem insubtype > 63");
                }*/
                Instance.GetActualMainAndSubTypesFromSubtype(Subtype, out Maintype, out Subtype);
            }
            if (Maintype >= Instance.VanillaItemMaxType)
            {
                int MainTypeformodded = (Maintype - Instance.VanillaItemMaxType) * 64 + Subtype;
                /*if (Global.DebugLogging)
                {
                    Logger.Info($"MainType for modded is {MainTypeformodded}");
                }*/
                if (MainTypeformodded <= Instance.ItemTypes.Count && MainTypeformodded > -1)
                {
                    /*if (Global.DebugLogging)
                    {
                        Logger.Info("Creating Item from list info");
                    }*/
                    ItemPlugin ItemType = Instance.ItemTypes[MainTypeformodded];
                    InItem = ItemType.PLPawnItem;
                    InItem.Level = level;
                    InItem.SubType = 64 + ((Maintype - Instance.VanillaItemMaxType) * 64) + Subtype;
                    Logger.Info($"CreatePawnItem gave item subtype {InItem.SubType}");
                }
            }
            if (InItem == null)
            {
                InItem = PLPawnItem.CreateFromInfo((EPawnItemType)Maintype, Subtype, level);
            }
            return InItem;
        }
        /// <summary>
        /// Used to get actual MainType and Subtype of PLPawnItem from it's cached Subtype. Must be wrapped in an if statement checking for subtype greater than 63
        /// </summary>
        /// <param name="InSubType">Subtype greater than 63 ()</param>
        /// <param name="MainType">Actual Maintype</param>
        /// <param name="SubType">Actual Subtype</param>
        /// <returns>Returns Actual Types from bogus subtype. Must be wrapped in an if statement checking for subtype greater than 63</returns>
        public void GetActualMainAndSubTypesFromSubtype(int InSubType, out int MainType, out int SubType)
        {
            if (InSubType > 63)
            {
                SubType = InSubType % 64;
                MainType = ((InSubType - 64 - SubType) / 64) + Instance.VanillaItemMaxType;
                /*if (Global.DebugLogging)
                {
                    Logger.Info($"in: {InSubType}, out: {MainType} {SubType}");
                }*/
            }
            else
            {
                Logger.Info("Wrap me in an if statement checking InSubType > 63");
                throw new System.NotImplementedException();
            }
        }
    }

    [HarmonyPatch(typeof(PLPawnItem), "GetPawnInfoFromHash")]
    class GetPawnInfoFromHashPatch
    {
        static bool Prefix(int inHash, ref uint actualSlotTypePart, ref uint subTypePart, ref uint levelPart)
        {
            actualSlotTypePart = (uint)(inHash & 63);
            subTypePart = ((uint)inHash >> 6 & 63U);
            levelPart = ((uint)inHash >> 12 & 15U);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLPawnItem), "getHash")]
    class PawnItemGetHash
    {
        static bool Prefix(PLPawnItem __instance, ref uint __result)
        {
            uint num;
            uint num2;
            if (__instance.SubType > 63)
            {
                ItemPluginManager.Instance.GetActualMainAndSubTypesFromSubtype(__instance.SubType, out int MainType, out int SubType);
                num = (uint)(MainType & 63);
                num2 = (uint)((uint)(SubType & 63) << 6);
                /*if (Global.DebugLogging)
                {
                    Logger.Info($"GetHash found subtype greater than 63 ({__instance.SubType}). output is {MainType}, {SubType}");
                }*/
            }
            else
            {
                num = (uint)(__instance.PawnItemType & (EPawnItemType)63);
                num2 = (uint)((uint)(__instance.SubType & 63) << 6);
            }
            uint num3 = (uint)((uint)(__instance.Level & 15) << 12);
            __result = (num | num2 | num3);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLPawnItem), "CreatePawnItemFromHash")]
    class CreatePawnItemFromHashPatch
    {
        static bool Prefix(int inHash, ref PLPawnItem __result)
        {
            PLPawnItem.GetPawnInfoFromHash(inHash, out uint inType, out uint inSubType, out uint inLevel);
            __result = ItemPluginManager.CreatePawnItem((int)inType, (int)inSubType, (int)inLevel);
            return false;
        }
    }

    [HarmonyPatch(typeof(PLPawnInventoryBase), "UpdateItem")]
    class UpdateItemPatch
    {
        static bool Prefix(PLPawnInventoryBase __instance, int inNetID, int inType, int inSubType, int inLevel, int inEquipID)
        {
            PLPawnItem itemAtNetID = __instance.GetItemAtNetID(inNetID);
            if (itemAtNetID != null)
            {
                itemAtNetID.EquipID = inEquipID;
                itemAtNetID.Level = inLevel;
                itemAtNetID.SubType = inSubType;
            }
            else
            {
                PLPawnItem plpawnItem = ItemPluginManager.CreatePawnItem(inType, inSubType, inLevel);
                if (plpawnItem != null)
                {
                    plpawnItem.NetID = inNetID;
                    plpawnItem.EquipID = inEquipID;
                    __instance.GetType().GetMethod("AddItem_Internal", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { inNetID, plpawnItem });
                }
            }
            if (PLNetworkManager.Instance.IsInternalBuild)
            {
                Logger.Info("UpdateItem:    player: " + ((__instance.PlayerOwner != null) ? __instance.PlayerOwner.GetPlayerName(false) : "null") + "    equipID: " + inEquipID.ToString());
            }
            if (PLTabMenu.Instance != null)
            {
                PLTabMenu.Instance.ShouldRecreateLocalInventory = true;
            }
            return false;
        }
    }
}
