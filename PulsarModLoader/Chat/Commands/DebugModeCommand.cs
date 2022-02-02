using PulsarModLoader.Chat.Commands.CommandRouter;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Chat.Commands
{
    class DebugModeCommand : ChatCommand
    {
        public static bool DebugMode = false;

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
            DebugMode = !DebugMode;

            //Write new DebugMode value to settings xml file
            PLXMLOptionsIO.Instance.CurrentOptions.SetStringValue("PMLDebugMode", DebugMode.ToString());

            //notify player of new DebugMode value
            Messaging.Notification($"PMLDebugMode is now {DebugMode}");

            if (!DebugMode)
                PLInGameUI.Instance.CurrentVersionLabel.text = PulsarModLoader.Patches.GameVersion.Version;
        }

        public string UsageExample()
        {
            return $"{CommandAliases()[0]}";
        }
    }
}
