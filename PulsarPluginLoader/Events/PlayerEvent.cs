
namespace PulsarPluginLoader.Events
{
    public class PlayerEvent : Event
    {
        public PLPlayer Player;

        public PlayerEvent(PLPlayer player)
        {
            Player = player;
        }
    }
}
