using PulsarPluginLoader.Utilities;
using System;
using System.Linq;

namespace PulsarPluginLoader.Chat.Commands.Devhax
{
    class GiveScrapCommand : IChatCommand
    {
        public string[] CommandAliases()
        {
            return new string[] { "givescrap" };
        }

        public string Description()
        {
            return "Resets processing attempts and 8 processed scrap and up to 4 cargo scrap, or supplied amounts";
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]} [processed] [cargo]";
        }

        public bool Execute(string arguments)
        {
            if (PhotonNetwork.isMasterClient && DevhaxCommand.IsEnabled)
            {
                // Set default quantities
                int processedScrap = 8;
                int cargoScrap = 4;

                // Process inputs
                string[] splitArguments = arguments.Split(' ').Select(x => x.Trim()).ToArray();
                if (splitArguments.Length >= 1 && !string.IsNullOrEmpty(splitArguments[0]))
                {
                    int.TryParse(splitArguments[0], out processedScrap);
                }
                if (splitArguments.Length >= 2 && !string.IsNullOrEmpty(splitArguments[1]))
                {
                    int.TryParse(splitArguments[1], out cargoScrap);
                }

                processedScrap = Math.Max(0, processedScrap);
                cargoScrap = Math.Max(0, cargoScrap);

                // Reset scrap processing attempts
                foreach (PLPlayer player in PLServer.Instance.AllPlayers)
                {
                    player.ScrapProcessingAttemptsLeft = player.GetMaxScrapProcessingAttempts();
                }

                // Add processed scrap
                PLServer.Instance.CurrentUpgradeMats += processedScrap;

                // Add cargo scrap, if space available
                for (int i = 0; i < cargoScrap; i++)
                {
                    PLEncounterManager.Instance.PlayerShip.MyStats.AddShipComponent(new PLScrapCargo());
                }

                Messaging.Notification(PhotonTargets.All, $"Added {cargoScrap:N0} scrap to cargo.");
                Messaging.Notification(PhotonTargets.All, $"Added {processedScrap:N0} processed scrap.");
                Messaging.Notification(PhotonTargets.All, $"Reset scrap processing attempts.");
            }
            else
            {
                string reason = !DevhaxCommand.IsEnabled ? "Cheats Disabled" : "Not Host";
                Messaging.Notification(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Command Failed: {reason}");
            }

            return false;
        }
    }
}
