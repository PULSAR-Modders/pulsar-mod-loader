using PulsarPluginLoader.Chat.Commands.CommandRouter;
using PulsarPluginLoader.Utilities;

namespace PulsarPluginLoader.Chat.Commands
{
    class DebugModeCommand : ChatCommand
    {
        public static bool DebugMode = false;

        public override string[] CommandAliases()
        {
            return new string[] { "debugmode", "dbm"};
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
            PLXMLOptionsIO.Instance.CurrentOptions.SetStringValue("PPLDebugMode", DebugMode.ToString());

            //notify player of new DebugMode value
            Messaging.Notification($"PPLDebugMode is now {DebugMode}");
        }

        public string UsageExample()
        {
            return $"{CommandAliases()[0]}";
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PLServer), "Start")]
    class LoadSetting
    {
        static void Postfix()
        {//load DebugMoad from settings.xml
            if (bool.TryParse(PLXMLOptionsIO.Instance.CurrentOptions.GetStringValue("PPLDebugMode"), out bool result))
            {
                DebugModeCommand.DebugMode = result;
            }
        }
    }
}
