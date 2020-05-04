using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PulsarPluginLoader.Chat.Commands
{
    class DebugModeCommand : IChatCommand
    {
        public string[] CommandAliases()
        {
            //load DebugMoad from settings.xml
            if (bool.TryParse(PLXMLOptionsIO.Instance.CurrentOptions.GetStringValue("PPLDebugMode"), out bool result))
            {
                ExceptionWarningPatch.DebugMode = result;
            }
            return new string[] { "debugmode", "dbm"};
        }

        public string Description()
        {
            return "Toggles Exception notifications in-game";
        }

        public bool Execute(string arguments)
        {
            //Toggle DebugMode value
            ExceptionWarningPatch.DebugMode = !ExceptionWarningPatch.DebugMode;

            //Write new DebugMode value to settings xml file
            PLXMLOptionsIO.Instance.CurrentOptions.SetStringValue("PPLDebugMode", ExceptionWarningPatch.DebugMode.ToString());

            //notify player of new DebugMode value
            Messaging.Notification($"PPLDebugMode is now {ExceptionWarningPatch.DebugMode}");
            return false;
        }

        public string UsageExample()
        {
            return $"{CommandAliases()[0]}";
        }
    }
}
