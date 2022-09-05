namespace PulsarModLoader
{
    public class PMLKeybind
    {
        public string Name;
        public string ID;
        public string Category;
        public string Key;


        public PMLKeybind(string inName, string inID, string inCategory, string inKey)
        {
            
            this.Name = inName;
            this.ID = inID;
            this.Category = inCategory;
            this.Key = inKey;
        }
    }

}
