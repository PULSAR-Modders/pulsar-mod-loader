using PulsarPluginLoader.Utilities;

namespace PulsarPluginLoader.Chat.Commands.Devhax
{
    public class DevhaxCommand : IChatCommand
    {
        public static bool IsEnabled = false;

        public DevhaxCommand()
        {
            if (PLXMLOptionsIO.Instance != null)
            {
                IsEnabled = PLXMLOptionsIO.Instance.CurrentOptions.GetStringValue("Devhax").Equals(bool.TrueString);
            }
        }

        public string[] CommandAliases()
        {
            return new string[] { "devhax" };
        }

        public string Description()
        {
            return "Toggles \"internal build\" status and devhax.  Persists through client restarts.";
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]}";
        }

        public bool Execute(string arguments)
        {
            IsEnabled = !IsEnabled;
            PLXMLOptionsIO.Instance.CurrentOptions.SetStringValue("Devhax", IsEnabled.ToString());

            string state = IsEnabled ? "ON" : "OFF";
            Messaging.Notification(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Devhax: {state}");

            return false;
        }
    }
}
