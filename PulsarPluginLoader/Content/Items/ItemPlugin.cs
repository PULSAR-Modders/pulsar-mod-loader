namespace PulsarPluginLoader.Content.Items
{
    public abstract class ItemPlugin
    {
        protected ItemPlugin()
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
