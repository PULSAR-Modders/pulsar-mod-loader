namespace PulsarModLoader.SaveData
{
    /// <summary>
    /// Abstract class for creation of modded Savegame Data.
    /// </summary>
    public abstract class PMLSaveData
    {
        /// <summary>
        /// For internal use, do not modify
        /// </summary>
        public PulsarMod MyMod;

        /// <summary>
        /// Overridable Identifier for SaveData. Cannot have the Identifier between savedata in the same mod.
        /// </summary>
        /// <returns></returns>
        public abstract string Identifier();

        /// <summary>
        /// VersionID. Defaults to 0, used to differenctiate savedata formats across multiple versions of a mod.
        /// </summary>
        public virtual uint VersionID => 0;

        /// <summary>
        /// Saves data returned via Byte format. 
        /// </summary>
        /// <returns></returns>
        public abstract byte[] SaveData();
        
        /// <summary>
        /// Loads data from savegame files.
        /// </summary>
        /// <param name="Data">loaded data</param>
        /// <param name="VersionID"></param>
        public abstract void LoadData(byte[] Data, uint VersionID);
    }
}
