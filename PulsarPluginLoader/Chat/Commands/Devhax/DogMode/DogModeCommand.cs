using PulsarPluginLoader.Utilities;

namespace PulsarPluginLoader.Chat.Commands.Devhax.DogMode
{
    class DogModeCommand : IChatCommand
    {
        public static bool IsEnabled = false;

        public string[] CommandAliases()
        {
            return new string[] { "dogmode" };
        }

        public string Description()
        {
            return "Toggles invincibility for all players and the ship.";
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]}";
        }

        public bool Execute(string arguments)
        {
            if (PhotonNetwork.isMasterClient && DevhaxCommand.IsEnabled)
            {
                ToggleDogMode();

                string state = IsEnabled ? "ON" : "OFF";
                Messaging.Notification(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"DogMode: {state}");
            }
            else
            {
                string reason = !DevhaxCommand.IsEnabled ? "Cheats Disabled" : "Not Host";
                Messaging.Notification(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Command Failed: {reason}");
            }

            return false;
        }

        private void ToggleDogMode()
        {
            if (PhotonNetwork.isMasterClient)
            {
                IsEnabled = !IsEnabled;

                if (PLEncounterManager.Instance != null && PLEncounterManager.Instance.PlayerShip != null)
                {
                    PLEncounterManager.Instance.PlayerShip.IsGodModeActive = IsEnabled;
                }

                if (PLServer.Instance != null)
                {
                    foreach (PLPlayer player in PLServer.Instance.AllPlayers)
                    {
                        if (player != null && player.TeamID == 0)
                        {
                            player.IsGodModeActive = IsEnabled;
                        }
                    }
                }
            }
        }
    }
}