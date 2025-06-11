using static PulsarModLoader.Content.Talents.TalentModManager;

namespace PulsarModLoader.Content.Talents
{
    public abstract class TalentMod
    {
        public TalentMod() { }
        /// <summary>
        /// Name of the Talent for ID-ing and appears in Research and Tab display.
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Description which appears in Research and Tab display.
        /// </summary>
        public virtual string Description { get { return ""; } }
        /// <summary>
        /// Maximum level Talent can be set to (Havent tested beyond 5).
        /// </summary>
        public virtual int MaxRank { get { return 3; } }
        /// <summary>
        /// ClassID of Talent. Basically just puts Talent at the top of the list with the class colour.
        /// </summary>
        public virtual int ClassID { get { return -1; } }
        /// <summary>
        /// Array of how many and which research materials are needed.
        /// </summary>
        public virtual int[] ResearchCost { get { return new int[6]; } }
        /// <summary>
        /// Number of warps to research the talent.
        /// </summary>
        public virtual int WarpsToResearch { get { return 3; } }
        /// <summary>
        /// Boolean that locks the Talent and makes it need researching.
        /// </summary>
        public virtual bool NeedsToBeResearched { get { return false; } }
        /// <summary>
        /// Name of talent that needs to be unlocked before this one is available.
        /// </summary>
        public virtual string ExtendsModdedTalent { get { return ""; } }
        /// <summary>
        /// Name of talent that needs to be unlocked before this one is available.
        /// </summary>
        public virtual ETalents ExtendsDefaultTalent { get { return ETalents.MAX; } }
        /// <summary>
        /// Minimum level required to unlock talent.
        /// </summary>
        public virtual int MinLevel { get { return 0; } }
        /// <summary>
        /// Talents can be hidden from the lists so that they can be hidden/not manually.
        /// Use TalentModManager.HideTalent(int talentID) or TalentModManager.UnHideTalent(int talentID)
        /// </summary>
        public virtual bool HiddenByDefault { get { return false; } }
        /// <summary>
        /// ClassID and Species talent applies to. Both enumerators found in TalentModManager.
        /// All, Captain, Pilot, Scientist, Weapons, Engineer | All, Human, Robot, Sylvassi
        /// </summary>
        public virtual (CharacterClass, CharacterSpecies) TalentAssignment { get { return ((CharacterClass)ClassID, CharacterSpecies.General); } }

        public virtual TalentInfo TalentInfo
        {
            get
            {
                TalentInfo info = new TalentInfo();
                info.Name = Name;
                info.Desc = Description;
                info.MaxRank = MaxRank;
                info.ClassID = ClassID;
                info.ResearchCost = ResearchCost;
                info.WarpsToResearch = WarpsToResearch;
                info.ExtendsTalent = ExtendsDefaultTalent;
                int extendsModded = TalentModManager.Instance.GetTalentIDFromName(ExtendsModdedTalent);
                info.TalentID = TalentModManager.Instance.GetTalentIDFromName(Name);
                if (extendsModded != -1) info.ExtendsTalent = (ETalents)extendsModded;
                info.MinLevel = MinLevel;
                return info;
            }
        }
    }
}
