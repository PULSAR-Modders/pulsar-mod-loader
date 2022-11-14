using PulsarModLoader.Chat.Commands.CommandRouter;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Chat.Commands
{
    class DebugModeCommand : ChatCommand
    {
        public override string[] CommandAliases()
        {
            return new string[] { "debugmode", "dbm" };
        }

        public override string Description()
        {
            return "Toggles Exception notifications in-game";
        }

        public override void Execute(string arguments)
        {
            //Toggle DebugMode value
            PMLConfig.DebugMode.Value = !PMLConfig.DebugMode.Value;

            //notify player of new DebugMode value
            Messaging.Notification($"PMLDebugMode is now {PMLConfig.DebugMode}");

            if (!PMLConfig.DebugMode)
                PLInGameUI.Instance.CurrentVersionLabel.text = PulsarModLoader.Patches.GameVersion.Version;
        }

        public string UsageExample()
        {
            return $"{CommandAliases()[0]}";
        }
    }
}
