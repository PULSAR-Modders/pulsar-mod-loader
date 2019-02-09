using PulsarPluginLoader.Utilities;
using System;

namespace PulsarPluginLoader.Chat.Commands.Devhax
{
    class CreditsUpCommand : IChatCommand
    {
        public string[] CommandAliases()
        {
            return new string[] { "creditsup" };
        }

        public string Description()
        {
            return "Increases credits by 500,000, or supplied amount.";
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]} [credits]";
        }

        public bool Execute(string arguments)
        {
            if (PhotonNetwork.isMasterClient && DevhaxCommand.IsEnabled)
            {
                if (!int.TryParse(arguments, out int credits))
                {
                    credits = 500000;
                }
                credits = Math.Max(0, credits);

                PLServer.Instance.CurrentCrewCredits += credits;

                Messaging.Notification(PhotonTargets.All, $"Added {credits:N0} credits.");
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
