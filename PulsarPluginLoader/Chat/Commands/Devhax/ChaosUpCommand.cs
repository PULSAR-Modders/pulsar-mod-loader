using PulsarPluginLoader.Utilities;
using System;

namespace PulsarPluginLoader.Chat.Commands.Devhax
{
    class ChaosUpCommand : IChatCommand
    {
        public string[] CommandAliases()
        {
            return new string[] { "chaosup" };
        }

        public string Description()
        {
            return "Increases Chaos level by 2.0, or supplied amount.";
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]} [chaos]";
        }

        public bool Execute(string arguments)
        {
            if (PhotonNetwork.isMasterClient && DevhaxCommand.IsEnabled)
            {
                if (!float.TryParse(arguments, out float chaos))
                {
                    chaos = 2.00f;
                }
                chaos = Math.Max(0.00f, chaos);

                PLServer.Instance.ChaosLevel += chaos;

                Messaging.Notification(PhotonTargets.All, $"Added {chaos:N} Chaos.");
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
