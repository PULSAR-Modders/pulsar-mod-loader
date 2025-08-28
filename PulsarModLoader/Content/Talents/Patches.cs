using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using static PulsarModLoader.Patches.HarmonyHelpers;
using static HarmonyLib.AccessTools;
using System.Linq;
using PulsarModLoader.SaveData;
using System.IO;
using System.Reflection;
using PulsarModLoader.CustomGUI;

namespace PulsarModLoader.Content.Talents
{
    public class HelperMethods
    {
        // Finds and overrides int 63 values, these are usually used to iterate to maximum talent count
        public static IEnumerable<CodeInstruction> Override63Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return PatchBySequence(instructions,
            new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)63),
            },
            new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, Method(typeof(HelperMethods), "Override63TranspilerPatch"))
            },
            PatchMode.REPLACE);
        }
        public static int Override63TranspilerPatch() => Enum.GetValues(typeof(ETalents)).Length + TalentModManager.Instance.TalentTypes.Count;
    }

    // Makes new Talent Infos retrievable
    [HarmonyPatch(typeof(PLGlobal), "GetTalentInfoForTalentType")]
    class TalentInfoFix
    {
        static bool Prefix(ETalents inTalent, ref TalentInfo __result)
        {
            //PulsarModLoader.Utilities.Logger.Info($"[GetTalentInfoForTalentType] {inTalent}");
            int subtypeformodded = (int)inTalent - TalentModManager.Instance.vanillaTalentMaxType;
            if (subtypeformodded <= TalentModManager.Instance.TalentTypes.Count && subtypeformodded > -1)
            {
                __result = TalentModManager.Instance.TalentTypes[subtypeformodded].TalentInfo;
                return false;
            }
            //PulsarModLoader.Utilities.Logger.Info($"[GetTalentInfoForTalentType] is vanilla");
            return true;
        }
    }

    /*[HarmonyPatch(typeof(PLTabMenu), "PressTD")]
    class rankTalentMessage
    {
        static void Prefix(PLTabMenu.TalentDisplay inTD)
        {
            TalentInfo talentInfoForTalentType = PLGlobal.GetTalentInfoForTalentType(inTD.MyType);
            PulsarModLoader.Utilities.Messaging.Echo(PhotonTargets.All, $"RankTalent {inTD.MyType} - {talentInfoForTalentType.Name} - {talentInfoForTalentType.TalentID}");
        }
    }*/

    // Increases the array size of Talents
    [HarmonyPatch(typeof(PLPlayer), "Start")]
    class StartTalentSizePatch
    {
        static void Postfix(PLPlayer __instance)
        {
            int TalentMaxSize = Enum.GetValues(typeof(ETalents)).Length + TalentModManager.Instance.TalentTypes.Count;
            __instance.Talents = new ObscuredInt[TalentMaxSize];
            __instance.TalentsLocalEditTime = new float[TalentMaxSize];
        }
    }

    // Allows new Talents to be synced with joining players
    [HarmonyPatch(typeof(PLPlayer), "SendTalentsToPhotonTargets")]
    public class TalentSerializePatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => HelperMethods.Override63Transpiler(instructions);
    }

    // Talents are unlocked based on the synced ObscuredLong this.TalentLockedStatus - Each bit represents one of the 64 talents, 1 = locked 0 = unlocked.
    // Since we are adding talents >64 we need to add to check and create our own ObscuredLong.
    #region Researchable/Not Talents
    // Allows new Talents to be researchable
    [HarmonyPatch(typeof(PLShipInfo), "UpdateResearchTalentChoices")]
    public class ResearchTalentSizePatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => HelperMethods.Override63Transpiler(instructions);
    }

    [HarmonyPatch(typeof(PLShipInfo), "Update")]
    public class ResearchTalentSizePatch2
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => HelperMethods.Override63Transpiler(instructions);
    }

    [HarmonyPatch(typeof(PLServer), "IsTalentUnlocked")]
    class ExtendIsTalentUnlocked
    {
        static bool Prefix(ETalents inTalent, ref bool __result)
        {
            int index = (int)inTalent / 64;
            int bitPosition = (int)inTalent % 64;
            if (index == 0) return true;
            else
            {
                if (!TalentModManager.Instance.extraTalentLockedStatus.TryGetValue(index, out ObscuredLong status))
                {
                    __result = true;
                    return false;
                }
                long mask = 1L << bitPosition;
                __result = (status & mask) == 0L;
                //PulsarModLoader.Utilities.Logger.Info($"[IsTalentUnlocked] {inTalent} = {__result}");
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(PLServer), "SetTalentAsUnlocked")]
    class ExtendSetTalentAsUnlocked
    {
        static bool Prefix(int inTalentID)
        {
            int index = inTalentID / 64;
            int bitPosition = inTalentID % 64;
            if (index == 0)
            {
                return true;
            }
            else
            {
                if (!TalentModManager.Instance.extraTalentLockedStatus.TryGetValue(index, out ObscuredLong status))
                {
                    return false;
                }
                long mask = 1L << bitPosition;
                TalentModManager.Instance.extraTalentLockedStatus[index] &= ~mask; // Set bit to 0 (unlocked)
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(PLCameraMode_MainMenu), "BeginMode")]
    class ResetPatch
    {
        static void Prefix()
        {
            PMLSaveTalents.HasLoadedElements = false;
        }
    }
    [HarmonyPatch(typeof(PLGameOverScreen), "OnEnter")]
    class ResetPatch2
    {
        static void Prefix()
        {
            PMLSaveTalents.HasLoadedElements = false;
        }
    }

    [HarmonyPatch(typeof(PLServer), "ResetTalentLockedStatus")]
    class ExtendResetTalentLockedStatus
    {
        static void Postfix()
        {
            if (PMLSaveTalents.HasLoadedElements) return;
            for (int i = 0; i < TalentModManager.Instance.extraTalentLockedStatus.Count; i++)
            {
                TalentModManager.Instance.extraTalentLockedStatus[i] = 0L;
            }
            for (int i = 0; i < TalentModManager.Instance.hiddenTalentStatus.Count; i++)
            {
                TalentModManager.Instance.hiddenTalentStatus[i] = 0L;
            }
            foreach (TalentMod talentMod in TalentModManager.Instance.TalentTypes)
            {
                if (talentMod != null)
                {
                    if (talentMod.NeedsToBeResearched) TalentModManager.Instance.LockTalent(TalentModManager.Instance.GetTalentIDFromName(talentMod.Name));
                    if (talentMod.HiddenByDefault) TalentModManager.Instance.HideTalent(TalentModManager.Instance.GetTalentIDFromName(talentMod.Name));
                }
            }
        }
    }
    #endregion

    // Patches PLServer Serialze to add syncing
    [HarmonyPatch(typeof(PLServer), "OnPhotonSerializeView")]
    public class SyncTalentsData
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_SendSync(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(PLServer), "JumpsNeededToResearchTalent")),
                new CodeInstruction(OpCodes.Call),
                new CodeInstruction(OpCodes.Box),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(PhotonStream), "SendNext")),
                new CodeInstruction(OpCodes.Br),
            };
            int location = FindSequence(instructions, targetSequence, CheckMode.NONNULL);
            return PatchBySequence(instructions,
            targetSequence,
            new List<CodeInstruction>()
            {
                instructionsList[location-7], // Ldarg_1
                instructionsList[location-6], // Ldarg_0
                instructionsList[location-5], // Ldfld - PLServer JumpsNeededToResearchTalent
                instructionsList[location-4], // Call
                instructionsList[location-3], // Box
                instructionsList[location-2], // Callvirt - PhotonStream SendNext
                instructionsList[location-7], // Ldarg_1
                new CodeInstruction(OpCodes.Call, Method(typeof(SyncTalentsData), "PatchSend")),
                instructionsList[location-1], // Br
            },
            PatchMode.REPLACE, CheckMode.NONNULL);
        }
        public static void PatchSend(PhotonStream stream)
        {
            // Send extraTalentLockedStatus
            Dictionary<int, ObscuredLong> newDict = TalentModManager.Instance.extraTalentLockedStatus;
            stream.SendNext(newDict.Count);
            foreach (var kvp in newDict)
            {
                stream.SendNext(kvp.Key);
                stream.SendNext((long)kvp.Value);
            }

            // Send hiddenTalentStatus
            newDict = TalentModManager.Instance.hiddenTalentStatus;
            stream.SendNext(newDict.Count);
            foreach (var kvp in newDict)
            {
                stream.SendNext(kvp.Key);
                stream.SendNext((long)kvp.Value);
            }
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_RecieveSync(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(PhotonStream), "ReceiveNext")),
                new CodeInstruction(OpCodes.Unbox_Any),
                new CodeInstruction(OpCodes.Call),
                new CodeInstruction(OpCodes.Stfld, Field(typeof(PLServer), "JumpsNeededToResearchTalent")),
                new CodeInstruction(OpCodes.Br),
            };
            int location = FindSequence(instructions, targetSequence, CheckMode.NONNULL);
            return PatchBySequence(instructions,
            targetSequence,
            new List<CodeInstruction>()
            {
                instructionsList[location-7], // Ldarg_0
                instructionsList[location-6], // Ldarg_1
                instructionsList[location-5], // Callvirt - PhotonStream ReceiveNext
                instructionsList[location-4], // Unbox_Any
                instructionsList[location-3], // Call
                instructionsList[location-2], // Stfld - PLServer JumpsNeededToResearchTalent
                instructionsList[location-6], // Ldarg_1
                new CodeInstruction(OpCodes.Call, Method(typeof(SyncTalentsData), "PatchReceive")),
                instructionsList[location-1], // Br
            },
            PatchMode.REPLACE, CheckMode.NONNULL);
        }
        public static void PatchReceive(PhotonStream stream)
        {
            // Receive extraTalentLockedStatus
            int count = (int)stream.ReceiveNext();
            var newDict = new Dictionary<int, ObscuredLong>();

            for (int i = 0; i < count; i++)
            {
                int key = (int)stream.ReceiveNext();
                long value = (long)stream.ReceiveNext();
                newDict[key] = value;
            }

            TalentModManager.Instance.extraTalentLockedStatus = newDict;

            // Receive hiddenTalentStatus
            count = (int)stream.ReceiveNext();
            newDict = new Dictionary<int, ObscuredLong>();

            for (int i = 0; i < count; i++)
            {
                int key = (int)stream.ReceiveNext();
                long value = (long)stream.ReceiveNext();
                newDict[key] = value;
            }

            TalentModManager.Instance.hiddenTalentStatus = newDict;
        }
    }

    // Could be useful to have Talents as a purchaseable or reward item. This implements the Hide/Unhide system using the same logic as the Researchable/Not system
    #region Hidden/Not Talents
    [HarmonyPatch(typeof(PLTabMenu), "UpdateTDs")]
    public class HideTalentsFromTabMenu
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_S),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(TalentInfo), "MaxRank")),
                new CodeInstruction(OpCodes.Bge_S),
                new CodeInstruction(OpCodes.Ldloc_S),
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Stfld, Field(typeof(PLTabMenu.TalentDisplay), "Available")),
                new CodeInstruction(OpCodes.Ldloc_S),
            };
            int location = FindSequence(instructions, targetSequence, CheckMode.NONNULL);
            targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_S),
                new CodeInstruction(OpCodes.Call, Method(typeof(PLGlobal), "GetTalentInfoForTalentType")),
                new CodeInstruction(OpCodes.Stloc_S),
            };
            int location2 = FindSequence(instructions, targetSequence, CheckMode.NONNULL);
            return PatchBySequence(instructions, targetSequence,
            new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                instructionsList[location2 + 3], // ldloc_s playerfromplayerid
                instructionsList[location2 - 3], // ldloc_s etalents
                new CodeInstruction(OpCodes.Call, Method(typeof(HideTalentsFromTabMenu), "Patch")),
                new CodeInstruction(OpCodes.Brfalse, instructionsList[location - 6].operand) // branch to ldloc_s 533
            },
            PatchMode.AFTER, CheckMode.NONNULL);
        }
        public static bool Patch(PLTabMenu instance, PLPlayer player, ETalents inTalent)
        {
            bool result = true;
            if (PMLSettings.HideResearchableTalentsFromTab.Value && !PLServer.Instance.IsTalentUnlocked(inTalent))
            {   // Hide Talent if hide researchable and its researchable.
                result = false;
            }
            if (result)
            {   // Hide Talent if hidden.
                int index = (int)inTalent / 64;
                int bitPosition = (int)inTalent % 64;
                if (!TalentModManager.Instance.hiddenTalentStatus.TryGetValue(index, out ObscuredLong status))
                {
                    return true;
                }
                long mask = 1L << bitPosition;
                result = (status & mask) == 0L;
            }
            if (!result)
            {   // Remove talent display if its supposed to be hidden and it exists already.
                List<PLTabMenu.TalentDisplay> allTDs = (List<PLTabMenu.TalentDisplay>)allTDsinfo.GetValue(instance);
                for (int i = 0; i < allTDs.Count; i++)
                {
                    PLTabMenu.TalentDisplay talentDisplay = allTDs[i];
                    if (talentDisplay.MyType == inTalent && talentDisplay.Player == player)
                    {
                        allTDs.RemoveAt(i);
                        break;
                    }
                }
                allTDsinfo.SetValue(instance, allTDs);
            }
            return result;
            //PulsarModLoader.Utilities.Logger.Info($"[IsTalentUnlocked] {inTalent} = {__result}");
        }
        private static FieldInfo allTDsinfo = AccessTools.Field(typeof(PLTabMenu), "allTDs");
    }

    [HarmonyPatch(typeof(PLGlobal), "IsTalentVisibleForResearch")]
    class HideFromResearchList
    {
        static void Postfix(ETalents inTalent, ref bool __result)
        {
            if (!__result) return;
            int index = (int)inTalent / 64;
            int bitPosition = (int)inTalent % 64;
            if (!TalentModManager.Instance.hiddenTalentStatus.TryGetValue(index, out ObscuredLong status))
            {
                return;
            }
            long mask = 1L << bitPosition;
            __result = (status & mask) == 0L;
        }
    }
    #endregion 
}
