namespace PulsarModLoader.Content.Items
{
    public abstract class ItemMod
    {
        protected ItemMod()
        {
        }

        public virtual string Name
        {
            get
            {
                { return ""; }
            }
        }
        public virtual PLPawnItem PLPawnItem
        {
            get
            {
                { return new PLPawnItem_AmmoClip(); }
            }
        }
    }
}
