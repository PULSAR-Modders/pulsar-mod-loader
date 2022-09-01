namespace PulsarModLoader.MPModChecks
{
    public class MPModDataBlock
    {
        public MPModDataBlock(string HarmonyIdentifier, string ModName, string Version, MPFunction MPFunction, string ModID, byte[] Hash)
        {
            this.HarmonyIdentifier = HarmonyIdentifier;
            this.ModName = ModName;
            this.Version = Version;
            this.MPFunction = MPFunction;
            this.Hash = Hash;
            this.ModID = ModID;
        }

        public MPModDataBlock(string HarmonyIdentifier, string ModName, string Version, MPFunction MPFunction, string ModID)
        {
            this.HarmonyIdentifier = HarmonyIdentifier;
            this.ModName = ModName;
            this.Version = Version;
            this.MPFunction = MPFunction;
            this.Hash = new byte[32];
            this.ModID = ModID;
        }

        public string HarmonyIdentifier { get; }
        public string ModName { get; }
        public string Version { get; }
        public MPFunction MPFunction { get; }
        public byte[] Hash { get; }
        public string ModID { get; }
    }
}
