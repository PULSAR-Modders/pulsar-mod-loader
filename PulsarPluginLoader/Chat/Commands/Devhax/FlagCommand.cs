using PulsarPluginLoader.Utilities;

namespace PulsarPluginLoader.Chat.Commands.Devhax
{
    public class FlagCommand : IChatCommand
    {

        public string[] CommandAliases()
        {
            return new string[] { "flag" };
        }

        public string Description()
        {
            return "Forces the player's ship to be flagged.";
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]}";
        }

        public bool Execute(string arguments)
        {
            PLShipInfo playerShip = PLEncounterManager.Instance?.PlayerShip;
            if(playerShip != null)
            {
                playerShip.IsFlagged = true;
                Messaging.Notification(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Ship flagged.");
            }
            

            return false;
        }
    }
}
