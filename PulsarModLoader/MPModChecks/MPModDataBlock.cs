namespace PulsarModLoader.MPModChecks
{
    /// <summary>
    /// Holds data about Mods for MPModChecks
    /// </summary>
    public class MPModDataBlock
    {
        /// <summary>
        /// Creates an MPModDataBlock (With Hash)
        /// </summary>
        /// <param name="HarmonyIdentifier"></param>
        /// <param name="ModName"></param>
        /// <param name="Version"></param>
        /// <param name="MPRequirement"></param>
        /// <param name="ModID"></param>
        /// <param name="Hash"></param>
        public MPModDataBlock(string HarmonyIdentifier, string ModName, string Version, MPRequirement MPRequirement, string ModID, byte[] Hash)
        {
            this.HarmonyIdentifier = HarmonyIdentifier;
            this.ModName = ModName;
            this.Version = Version;
            this.MPRequirement = MPRequirement;
            this.Hash = Hash;
            this.ModID = ModID;
        }

        /// <summary>
        /// Creates an MPModDataBlock (without Hash)
        /// </summary>
        /// <param name="HarmonyIdentifier"></param>
        /// <param name="ModName"></param>
        /// <param name="Version"></param>
        /// <param name="MPRequirement"></param>
        /// <param name="ModID"></param>
        public MPModDataBlock(string HarmonyIdentifier, string ModName, string Version, MPRequirement MPRequirement, string ModID)
        {
            this.HarmonyIdentifier = HarmonyIdentifier;
            this.ModName = ModName;
            this.Version = Version;
            this.MPRequirement = MPRequirement;
            this.Hash = new byte[32];
            this.ModID = ModID;
        }

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string HarmonyIdentifier { get; }
        public string ModName { get; }
        public string Version { get; }
        public MPRequirement MPRequirement { get; }
        public byte[] Hash { get; }
        public string ModID { get; }

        #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
