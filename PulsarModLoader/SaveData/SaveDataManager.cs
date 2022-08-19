using HarmonyLib;
using PulsarModLoader;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        static public string SaveDir = Directory.GetCurrentDirectory() + "/Saves";
        static public string LocalSaveDir = SaveDir + "/Local";

        List<PMLSaveData> SaveConfigs = new List<PMLSaveData>();

        void OnModLoaded(string modName, PulsarMod mod)
        {
            mod.GetType().Assembly.GetTypes().AsParallel().ForAll((type) =>
            {
                if (typeof(PMLSaveData).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    PMLSaveData SaveData = (PMLSaveData)Activator.CreateInstance(type);
                    SaveData.MyMod = mod;
                    SaveConfigs.Add(SaveData);
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
            }
        }

        public static string getPMLSaveFileName(string inFileName)
        {
            if(inFileName.EndsWith("LastRecoveredSave.plsave")) //Fix LastRecoveredSave
            {
                return inFileName.Replace("LastRecoveredSave.plsave", "LastRecoveredMSave.pmlsave");
            }
            return inFileName.Replace(".plsave", ".pmlsave");
        }

        public void SaveDatas(string inFileName)
        {
            //Stop if no save configs to save. Additionally check for older .pmlsave file under the same name and delete.
            string fileName = getPMLSaveFileName(inFileName);
            if (SaveConfigs.Count == 0)
            {
                if(File.Exists(fileName))
                {
                    Logger.Info("Old PMLSave found with no new data to save. Deleting old data.");
                    File.Delete(fileName);
                }
                return;
            }

            //Start Saving, create temp file
            string tempText = fileName + "_temp";
            FileStream fileStream = File.Create(tempText);
            BinaryWriter binaryWriter = new BinaryWriter(fileStream);

            //save for mods
            binaryWriter.Write(SaveConfigs.Count);                      //int32 representing total configs
            foreach (PMLSaveData saveData in SaveConfigs)
            {
                try
                {
                    PulsarModLoader.Utilities.Logger.Info($"Writing: {saveData.MyMod.HarmonyIdentifier()}::{saveData.Identifier()}");
                    MemoryStream dataStream = saveData.SaveData();          //Collect Save data from mod
                    int bytecount = (int)dataStream.Length;

                                                                            //SaveDataHeader
                    binaryWriter.Write(saveData.MyMod.HarmonyIdentifier()); //Write Mod Identifier
                    binaryWriter.Write(saveData.Identifier());              //Write PMLSaveData Identifier
                    binaryWriter.Write(saveData.VersionID);                 //Write PMLSaveData VersionID
                    binaryWriter.Write(bytecount);                          //Write stream byte count
                    dataStream.Position = 0;                                //Reset position of dataStream for reading

                    byte[] buffer = new byte[bytecount];
                    dataStream.Read(buffer, 0, bytecount);                  //move data to filestream
                    binaryWriter.BaseStream.Write(buffer, 0, bytecount);

                    dataStream.Close();
                }
                catch (Exception ex)
                {
                    Logger.Info($"Failed to save a mod data.\n{ex.Message}");
                }
            }

            //Finish Saving, close and save file to actual location
            binaryWriter.Close();
            fileStream.Close();
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            File.Move(tempText, fileName);
            string relativeFileName = PLNetworkManager.Instance.FileNameToRelative(fileName);
            Logger.Info("PMLSaveManager has saved file: " + relativeFileName);


            //Save to Steam
            /*bool localFile = false;
            if (relativeFileName.StartsWith(LocalSaveDir))
            {
                localFile = true;
            }
            if (!PLServer.Instance.IronmanModeIsActive && !localFile)
            {
                Logger.Info("Should be saving file to steam cloud");
                PLNetworkManager.Instance.SteamCloud_WriteFileName(relativeFileName, delegate (RemoteStorageFileWriteAsyncComplete_t pCallback, bool bIOFailure)
                {
                    OnRemoteFileWriteAsyncComplete(pCallback, bIOFailure, relativeFileName);
                });
            }*/
        }

        public void LoadDatas(string inFileName)
        {
            //start reading
            string fileName = getPMLSaveFileName(inFileName);
            if(!File.Exists(fileName))
            {
                return;
            }
            FileStream fileStream = File.OpenRead(fileName);
            BinaryReader binaryReader = new BinaryReader(fileStream);

            //read for mods
            int count = binaryReader.ReadInt32();                //int32 representing total configs
            string missingMods = "";
            string VersionMismatchedMods = "";
            for (int i = 0; i < count; i++)
            {
                //SaveDataHeader
                string harmonyIdent = binaryReader.ReadString(); //HarmonyIdentifier
                string SavDatIdent = binaryReader.ReadString();  //SaveDataIdentifier
                uint VersionID = binaryReader.ReadUInt32();      //VersionID
                int bytecount = binaryReader.ReadInt32();        //ByteCount
                PulsarModLoader.Utilities.Logger.Info($"Reading SaveData: {harmonyIdent}::{SavDatIdent} SaveDataVersion: {VersionID} bytecount: {bytecount} Pos: {binaryReader.BaseStream.Position}");


                bool foundReader = false;
                foreach (PMLSaveData savedata in SaveConfigs)
                {
                    if (savedata.MyMod.HarmonyIdentifier() == harmonyIdent && savedata.Identifier() == SavDatIdent)
                    {
                        if(VersionID != savedata.VersionID)
                        {
                            Logger.Info($"Mismatched SaveData VersionID. Read: {VersionID} SaveData: {savedata.VersionID}");
                            VersionMismatchedMods += "\n" + harmonyIdent;
                        }
                        MemoryStream stream = new MemoryStream();               //initialize new memStream

                        byte[] buffer = new byte[bytecount];
                        binaryReader.BaseStream.Read(buffer, 0, bytecount);     //move data to memStream
                        stream.Write(buffer, 0, bytecount);

                        stream.Position = 0;                                    //Reset position
                        try
                        {
                            savedata.LoadData(stream, VersionID);               //Send memStream to PMLSaveData
                        }
                        catch (Exception ex)
                        {
                            Logger.Info($"Failed to load {harmonyIdent}::{SavDatIdent}\n{ex.Message}");
                        }
                        stream.Close();
                        foundReader = true;
                    }
                }
                if (!foundReader)
                {
                    binaryReader.BaseStream.Position += bytecount;
                    missingMods += ("\n" + harmonyIdent);
                }
            }

            //Finish Reading
            binaryReader.Close();
            fileStream.Close();
            Logger.Info("PMLSaveManager has read file: " + PLNetworkManager.Instance.FileNameToRelative(fileName));

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
    }
    [HarmonyPatch(typeof(PLSaveGameIO), "SaveToFile")]
    class SavePatch
    {
        static void Postfix(string inFileName)
        {
            SaveDataManager.Instance.SaveDatas(inFileName);
        }
    }
    [HarmonyPatch(typeof(PLSaveGameIO), "LoadFromFile")]
    class LoadPatch
    {
        static void Postfix(string inFileName)
        {
            SaveDataManager.Instance.LoadDatas(inFileName);
        }
    }


    [HarmonyPatch(typeof(PLSaveGameIO), "DeleteSaveGame")]
    class DeletePatch
    {
        static void Postfix(PLSaveGameIO __instance)
        {
            string fileName = SaveDataManager.getPMLSaveFileName(__instance.LatestSaveGameFileName);
            if (fileName != "")
            {
                try
                {
                    Logger.Info("DeleteSaveGame  " + fileName);
                    File.Delete(__instance.LatestSaveGameFileName);
                }
                catch (Exception ex)
                {
                    Logger.Info("DeleteSaveGame EXCEPTION: " + ex.Message + ": Could not delete save file!");
                }
            }
        }
    }

    [HarmonyPatch(typeof(PLUILoadMenu), "OnClickDeleteSaveFile")]
    class LoadMenuDeletePatch
    {
        static void Postfix(string inFileName)
        {
            if (inFileName.EndsWith(".plsave"))
            {
                string PMLname = SaveDataManager.getPMLSaveFileName(inFileName);
                typeof(PLUILoadMenu).GetMethod("OnClickDeleteSaveFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(PLUILoadMenu.Instance, new object[] { PMLname });
            }
        }
    }
}
