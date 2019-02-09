
namespace PulsarPluginLoader.Events
{
    public class Event
    {
        public bool IsCanceled = false;

        public virtual bool IsCancelable()
        {
            return false;
        }
    }
}
