using HarmonyLib;
using PulsarModLoader.Patches;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;

namespace PulsarModLoader.SaveData
{
    class SaveDataManager
    {
        public SaveDataManager()
        {
            ModManager.Instance.OnModSuccessfullyLoaded += OnModLoaded;
            ModManager.Instance.OnModUnloaded += OnModRemoved;
            Instance = this;
        }
        public static SaveDataManager Instance;

        public static string ReadMods = "";
        static public string SaveDir = Directory.GetCurrentDirectory() + "/Saves";
        static public string LocalSaveDir = SaveDir + "/Local";

        List<PMLSaveData> SaveConfigs = new List<PMLSaveData>();
        public int SaveCount = 0;

        void OnModLoaded(string modName, PulsarMod mod)
        {
            mod.GetType().Assembly.GetTypes().AsParallel().ForAll((type) =>
            {
                if (typeof(PMLSaveData).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    PMLSaveData SaveData = (PMLSaveData)Activator.CreateInstance(type);
                    SaveData.MyMod = mod;
                    SaveConfigs.Add(SaveData);
                    SaveCount = SaveConfigs.Count;
                }
            });
        }

        void OnModRemoved(PulsarMod mod)
        {
            List<PMLSaveData> saveConfigsToRemove = new List<PMLSaveData>();
            SaveConfigs.AsParallel().ForAll((arg) =>
            {
                if (arg.GetType().Assembly == mod.GetType().Assembly)
                {
                    saveConfigsToRemove.Add(arg);
                }
            });
            for (byte s = 0; s < saveConfigsToRemove.Count; s++)
            {
                SaveConfigs.Remove(saveConfigsToRemove[s]);
                SaveCount = SaveConfigs.Count;
            }
        }

        public void SaveDatas(BinaryWriter writer)
        {
            //Stop if no save configs to save.
            if (SaveCount == 0)
            {
                writer.Close();
                return;
            }
            //Save VersionID for later, starting with 0
            writer.Write((uint)0);

            //Bytecount for log
            int TotalBytes = 0;


            //save for mods
            writer.Write(SaveCount);                      //int32 representing total configs
            foreach (PMLSaveData saveData in SaveConfigs)
            {
                try
                {
                    PulsarModLoader.Utilities.Logger.Info($"Writing: {saveData.MyMod.HarmonyIdentifier()}::{saveData.Identifier()} pos: {writer.BaseStream.Position}");
                    byte[] modData = saveData.SaveData();          //Collect Save data from mod

                    //SaveDataHeader
                    writer.Write(saveData.MyMod.HarmonyIdentifier()); //Write Mod Identifier
                    writer.Write(saveData.Identifier());              //Write PMLSaveData Identifier
                    writer.Write(saveData.VersionID);                 //Write PMLSaveData VersionID
                    writer.Write(modData.Length);                     //Write stream byte count
                    TotalBytes += modData.Length;                     //Add bytecount to log

                    //SaveData
                    if (modData.Length > 0)
                    {
                        writer.Write(modData);                        //write modData to filestream
                    }
                }
                catch (Exception ex)
                {
                    writer.Write("PMLSaveDataManager.DataCorruptionWarning");
                    writer.Write("DataCorruptionWarning");
                    writer.Write((uint)0);
                    writer.Write(0);
                    Logger.Info($"Failed to save a mod data.\n{ex.Message}\n");
                }
            }
            writer.Write(ulong.MaxValue);
            writer.Close();
            Logger.Info($"PMLSaveManager has finished saving file. Bytes: {TotalBytes}");
        }

        public void LoadDatas(BinaryReader reader, bool ldarg3)
        {
            //Stop reading if nothing to read 
            if (reader.BaseStream.Length <= reader.BaseStream.Position + 1 || !ldarg3)
            {
                reader.Close();
                return;
            }


            //read for mods
            uint PMLSaveVersion = reader.ReadUInt32();     //uint32 represnting PMLSaveVersion. This will probably be used in the future.
            int count = reader.ReadInt32();                //int32 representing total configs
            string missingMods = "";
            string VersionMismatchedMods = "";
            string readMods = "";
            int TotalBytes = 0;

            for (int i = 0; i < count; i++)
            {
                //SaveDataHeader
                string harmonyIdent = reader.ReadString(); //HarmonyIdentifier
                string SavDatIdent = reader.ReadString();  //SaveDataIdentifier
                uint VersionID = reader.ReadUInt32();      //VersionID
                int bytecount = reader.ReadInt32();        //ByteCount
                Logger.Info($"Reading SaveData: {harmonyIdent}::{SavDatIdent} SaveDataVersion: {VersionID} bytecount: {bytecount} Pos: {reader.BaseStream.Position}");
                readMods += "\n" + harmonyIdent;
                TotalBytes += bytecount;


                bool foundReader = false;
                foreach (PMLSaveData savedata in SaveConfigs)
                {
                    if (savedata.MyMod.HarmonyIdentifier() == harmonyIdent && savedata.Identifier() == SavDatIdent)
                    {
                        if (VersionID != savedata.VersionID)
                        {
                            Logger.Info($"Mismatched SaveData VersionID. Read: {VersionID} SaveData: {savedata.VersionID}");
                            VersionMismatchedMods += "\n" + harmonyIdent;
                        }

                        if (bytecount > 0)
                        {
                            try
                            {
                                savedata.LoadData(reader.ReadBytes(bytecount), VersionID);               //Send modData to PMLSaveData
                            }
                            catch (Exception ex)
                            {
                                Logger.Info($"Failed to load {harmonyIdent}::{SavDatIdent}\n{ex.Message}");
                            }
                        }
                        foundReader = true;
                    }
                }
                if (!foundReader)
                {
                    reader.BaseStream.Position += bytecount;
                    missingMods += ("\n" + harmonyIdent);
                }
            }

            //Finish Reading
            reader.Close();
            Logger.Info($"PMLSaveManager has finished reading file. Bytes: {TotalBytes}");
            ReadMods = readMods;

            if (missingMods.Length > 0)
            {
                PLNetworkManager.Instance.MainMenu.AddActiveMenu(new PLErrorMessageMenu($"Warning: Found save data for following missing mods: {missingMods}"));
                Logger.Info($"Warning: Found save data for following missing mods: {missingMods}");
            }
            if (!string.IsNullOrEmpty(VersionMismatchedMods))
            {
                PLNetworkManager.Instance.MainMenu.AddActiveMenu(new PLErrorMessageMenu($"Warning: The following mods used in this save have been updated: {VersionMismatchedMods}"));
                Logger.Info($"Warning: The following mods used in this save have been updated: {VersionMismatchedMods}");
            }
        }

        public static bool IsFileModded(string inFileName)
        {
            bool returnValue = false;
            if (File.Exists(inFileName))
            {
                FileStream fileStream = File.OpenRead(inFileName);
                {
                    fileStream.Position = fileStream.Length - 8;
                    using (BinaryReader reader = new BinaryReader(fileStream))
                    {
                        if (reader.ReadUInt64() == ulong.MaxValue)
                        {
                            returnValue = true;
                        }
                    }
                }
            }
            return returnValue;
        }
    }
    [HarmonyPatch(typeof(PLSaveGameIO), "SaveToFile")]
    class SavePatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetsequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(BinaryWriter), "Close")),
            };
            List<CodeInstruction> injectedsequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(SaveDataManager), "Instance")),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SaveDataManager), "SaveDatas")),
            };
            return HarmonyHelpers.PatchBySequence(instructions, targetsequence, injectedsequence, HarmonyHelpers.PatchMode.BEFORE);
        }
    }
    [HarmonyPatch(typeof(PLSaveGameIO), "LoadFromFile")]
    class LoadPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetsequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(BinaryReader), "Close"))
            };
            List<CodeInstruction> injectedsequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(SaveDataManager), "Instance")),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldarg_3),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SaveDataManager), "LoadDatas")),
            };
            return HarmonyHelpers.PatchBySequence(instructions, targetsequence, injectedsequence, HarmonyHelpers.PatchMode.REPLACE);
        }
    }
}
