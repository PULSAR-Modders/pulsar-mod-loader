using HarmonyLib;
using PulsarModLoader.Patches;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;

namespace PulsarModLoader.SaveData
{
    [HarmonyPatch(typeof(PLUILoadMenu), "Update")]
    class DisplayModdedSavePatch
    {
        public static List<string> MFileNames = new List<string>();
        static string AppendModdedLine(string originalText, PLUILoadMenu instance)
        {
            string Cachedname = PLNetworkManager.Instance.FileNameToRelative(instance.DataToLoad.FileName);
            if (Cachedname.StartsWith("Saves/"))
            {
                Cachedname = Cachedname.Remove(0, 6);
            }
            if (instance.DataToLoad != null && MFileNames.Contains(Cachedname))
            {
                //Logger.Info("Appending GameInfo Line");
                originalText += "\n<color=yellow>Modded</color>" + SaveDataManager.ReadMods;
            }
            return originalText;
        }
        static string CheckAddPMLSaveFileTag(string inFileName)
        {
            if(MFileNames.Contains(inFileName))
            {
                return "<color=yellow>M</color> " + inFileName;
            }
            else
            {
                return inFileName;
            }
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //Handle Load Preview display
            List<CodeInstruction> targetsequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PLUILoadMenu), "GameInfoLabel")),
                new CodeInstruction(OpCodes.Ldloc_S) //(byte)14
            };
            List<CodeInstruction> injectedsequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DisplayModdedSavePatch), "AppendModdedLine"))
            };

            IEnumerable<CodeInstruction> patchedInstructions = HarmonyHelpers.PatchBySequence(instructions, targetsequence, injectedsequence, checkMode: HarmonyHelpers.CheckMode.NONNULL);

            //Handle Loadmenu Elements display
            targetsequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_S), //(byte)7
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Ldloc_S) //(byte)8
            };
            injectedsequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DisplayModdedSavePatch), "CheckAddPMLSaveFileTag"))
            };
            return HarmonyHelpers.PatchBySequence(patchedInstructions, targetsequence, injectedsequence, checkMode: HarmonyHelpers.CheckMode.NONNULL);
        }
    }


    [HarmonyPatch(typeof(PLSaveGameIO), "AddSaveGameDataBasicFromDir")]
    class ListModdedSavesPatch
    {
        static void Postfix()
        {
            List<string> MFiles = new List<string>();
            foreach(SaveGameDataBasic saveDataBasic in PLSaveGameIO.Instance.SaveGamesBasic)
            {
                string Cachedname = saveDataBasic.FileName;
                string moddedFileName = SaveDataManager.getPMLSaveFileName(Cachedname);
                if (File.Exists(moddedFileName))
                {
                    Cachedname = PLNetworkManager.Instance.FileNameToRelative(Cachedname);
                    if (Cachedname.StartsWith("Saves/"))
                    {
                        Cachedname = Cachedname.Remove(0, 6);
                    }
                    MFiles.Add(Cachedname);
                }
            }
            DisplayModdedSavePatch.MFileNames = MFiles;
        }
    }
}
