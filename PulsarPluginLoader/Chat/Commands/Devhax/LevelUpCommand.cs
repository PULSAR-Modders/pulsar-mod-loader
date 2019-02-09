using PulsarPluginLoader.Utilities;
using System;

namespace PulsarPluginLoader.Chat.Commands.Devhax
{
    public class LevelUpCommand : IChatCommand
    {
        public string[] CommandAliases()
        {
            return new string[] { "levelup" };
        }

        public string Description()
        {
            return "Increases crew level by 1, or supplied amount.";
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]} [levels]";
        }

        public bool Execute(string arguments)
        {
            if (PhotonNetwork.isMasterClient && DevhaxCommand.IsEnabled)
            {
                int.TryParse(arguments, out int levels);
                levels = Math.Max(1, levels);

                // Each level takes Current Level + 1 XP
                // Total XP required for level 1 through n is n(n+1)/2
                // For arbitrary level deltas D and current level L, (D-1)(L+1 + L+d)/2
                int a = PLServer.Instance.CurrentCrewLevel + 1;
                int b = PLServer.Instance.CurrentCrewLevel + levels;
                int n = b - a + 1;
                PLServer.Instance.CurrentCrewXP = (int)Math.Ceiling(n * (a + b) / 2.0f);

                Messaging.Notification(PhotonTargets.All, $"Added {levels:N0} crew levels.");
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
