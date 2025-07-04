using System;
using System.Collections.Generic;
using System.Reflection;
using PulsarModLoader.Utilities;
using CodeStage.AntiCheat.ObscuredTypes;
using PulsarModLoader.SaveData;
using System.IO;
using System.Linq;

namespace PulsarModLoader.Content.Talents
{
    public class TalentModManager
    {
        public enum CharacterClass
        {
            General = -1,
            Captain,
            Pilot,
            Scientist,
            Weapons,
            Engineer
        }
        public enum CharacterSpecies
        {
            General = -1,
            Human,
            Sylvassi,
            Robot
        }

        public readonly int vanillaTalentMaxType = 0;
        private static TalentModManager m_instance = null;
        public readonly List<TalentMod> TalentTypes = new List<TalentMod>();
        public static TalentModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new TalentModManager();
                }
                return m_instance;
            }
        }

        TalentModManager()
        {
            vanillaTalentMaxType = Enum.GetValues(typeof(ETalents)).Length;
            Logger.Info($"Talents Vanilla MaxTypeint = {vanillaTalentMaxType - 1}");
            extraTalentLockedStatus.Add(0, 0L);
            hiddenTalentStatus.Add(0, 0L);
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type talentMod = typeof(TalentMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (talentMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        TalentMod talentModHandler = (TalentMod)Activator.CreateInstance(t);
                        if (GetTalentIDFromName(talentModHandler.Name) == -1)
                        {
                            TalentTypes.Add(talentModHandler);
                            int newTalentID = GetTalentIDFromName(talentModHandler.Name);
                            TalentCreation.cachedTalents[talentModHandler.TalentAssignment].Add((ETalents)newTalentID);
                            Logger.Info($"Added Talent: '{talentModHandler.Name}' with ID '{newTalentID}'");
                            if (newTalentID / 64 >= extraTalentLockedStatus.Keys.Count)
                            {   // Extend the ObscureLong TalentLockedStatus for > 64 talents
                                extraTalentLockedStatus.Add(extraTalentLockedStatus.Keys.Count, 0L);
                            }
                            if (talentModHandler.NeedsToBeResearched)
                            {
                                LockTalent(newTalentID);
                                Logger.Info($"Talent '{talentModHandler.Name}' has been made researchable.");
                            }
                            if (newTalentID / 64 >= hiddenTalentStatus.Keys.Count)
                            {   // Extend the ObscureLong TalentLockedStatus for > 64 talents
                                hiddenTalentStatus.Add(hiddenTalentStatus.Keys.Count, 0L);
                            }
                            if (talentModHandler.HiddenByDefault)
                            {
                                HideTalent(newTalentID);
                                Logger.Info($"Talent '{talentModHandler.Name}' has been hidden by default.");
                            }
                        }
                        else
                        {
                            Logger.Info($"Could not add Talent from {mod.Name} with the duplicate name of '{talentModHandler.Name}'");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds Talent ID equivilent to given name. Returns -1 if couldn't find Talent.
        /// </summary>
        /// <param name="TalentName">Name of Talent</param>
        /// <returns>ID of Talent</returns>
        public int GetTalentIDFromName(string TalentName)
        {
            for (int i = 0; i < TalentTypes.Count; i++)
            {
                if (TalentTypes[i].Name == TalentName)
                {
                    return i + vanillaTalentMaxType;
                }
            }
            return -1;
        }

        /// <summary>
        /// ObscuredLong PLServer.TalentLockedStatus is used to make talents researchable based on the bit values in the ObscuredLong and is length limited to 64. This allows that to be extended.
        /// </summary>
        public Dictionary<int, ObscuredLong> extraTalentLockedStatus = new Dictionary<int, ObscuredLong>();

        /// <summary>
        /// Makes the talent need to be researched. (Sets talentID bit location in the TalentLockedStatus ObscuredLong to be locked)
        /// </summary>
        /// <param name="talentID">ID of Talent</param>
        public void LockTalent(int talentID)
        {
            int index = talentID / 64;
            int bitPosition = talentID % 64;

            if (index != 0) // Skip index == 0 as that is the default 64 range
            {
                long mask = 1L << bitPosition;
                extraTalentLockedStatus[index] |= mask; // Set bit to 1 (locked)
            }
        }

        /// <summary>
        /// Makes the talent no-longer need to be researched. (Sets talentID bit location in the TalentLockedStatus ObscuredLong to be Unlocked)
        /// </summary>
        /// <param name="talentID">ID of Talent</param>
        public void UnlockTalent(int talentID)
        {
            int index = talentID / 64;
            int bitPosition = talentID % 64;

            if (index != 0) // Skip index == 0 as that is the default 64 range
            {
                long mask = 1L << bitPosition;
                extraTalentLockedStatus[index] &= ~mask; // Set bit to 0 (unlocked)
            }
        }

        /// <summary>
        /// ObscuredLong is used to make talents hidden based on the bit values in the ObscuredLong.
        /// </summary>
        public Dictionary<int, ObscuredLong> hiddenTalentStatus = new Dictionary<int, ObscuredLong>();

        /// <summary>
        /// Hides the talent. (Sets talentID bit location in the hideTalentStatus ObscuredLong to be locked)
        /// </summary>
        /// <param name="talentID">ID of Talent</param>
        public void HideTalent(int talentID)
        {
            int index = talentID / 64;
            int bitPosition = talentID % 64;

            long mask = 1L << bitPosition;
            hiddenTalentStatus[index] |= mask; // Set bit to 1 (locked)
        }

        /// <summary>
        /// Unhides the talent. (Sets talentID bit location in the hideTalentStatus ObscuredLong to be Unlocked)
        /// </summary>
        /// <param name="talentID">ID of Talent</param>
        public void UnHideTalent(int talentID)
        {
            int index = talentID / 64;
            int bitPosition = talentID % 64;

            long mask = 1L << bitPosition;
            hiddenTalentStatus[index] &= ~mask; // Set bit to 0 (unlocked)
        }
    }
    public class PMLSaveTalents : PMLSaveData
    {
        public static bool HasLoadedElements = false;
        public PMLSaveTalents() { }
        public override string Identifier() => "Talents";

        public override void LoadData(byte[] Data, uint VersionID)
        {
            var dict = new Dictionary<int, ObscuredLong>();
            using (MemoryStream ms = new MemoryStream(Data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    int key = reader.ReadInt32();
                    long value = reader.ReadInt64();
                    if (!dict.ContainsKey(key)) dict.Add(key, value);
                    else
                    {
                        dict[key] = value;
                    }
                }
                if (!HasLoadedElements) TalentModManager.Instance.extraTalentLockedStatus = dict;
                else TalentModManager.Instance.extraTalentLockedStatus.Concat(dict);
                count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    int key = reader.ReadInt32();
                    long value = reader.ReadInt64();
                    if (!dict.ContainsKey(key)) dict.Add(key, value);
                    else
                    {
                        dict[key] = value;
                    }
                }
                if (!HasLoadedElements) TalentModManager.Instance.hiddenTalentStatus = dict;
                else TalentModManager.Instance.hiddenTalentStatus.Concat(dict);
            }
            HasLoadedElements = true;
        }

        public override byte[] SaveData()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                Dictionary<int, ObscuredLong> dict = TalentModManager.Instance.extraTalentLockedStatus;
                writer.Write(dict.Count);
                foreach (var kvp in dict)
                {
                    writer.Write(kvp.Key);
                    writer.Write((long)kvp.Value);
                }
                dict = TalentModManager.Instance.hiddenTalentStatus;
                writer.Write(dict.Count);
                foreach (var kvp in dict)
                {
                    writer.Write(kvp.Key);
                    writer.Write((long)kvp.Value);
                }
                return ms.ToArray();
            }
        }
    }
}
