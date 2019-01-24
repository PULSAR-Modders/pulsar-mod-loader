
namespace PulsarPluginLoader.Events
{
    public abstract class Event
    {
        public class PlayerJoinEvent : Event
        {
            public PLPlayer Player;

            public PlayerJoinEvent(PLPlayer player)
            {
                Player = player;
            }
        }
    }
}
