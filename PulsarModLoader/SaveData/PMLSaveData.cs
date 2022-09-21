namespace PulsarModLoader.SaveData
{
    public abstract class PMLSaveData
    {
        public PulsarMod MyMod;
        public abstract string Identifier();
        public virtual uint VersionID => 0;
        public abstract byte[] SaveData();
        public abstract void LoadData(byte[] Aata, uint VersionID);
    }
}
