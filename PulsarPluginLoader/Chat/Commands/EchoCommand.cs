using PulsarPluginLoader.Utilities;

namespace PulsarPluginLoader.Chat.Commands
{
    public class EchoCommand : IChatCommand
    {
        public string[] CommandAliases()
        {
            return new string[] { "echo", "e" };
        }

        public string Description()
        {
            return "Repeats the input text back through the chat box.";
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]} [text]";
        }

        public bool Execute(string arguments, int SenderID)
        {
            Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Echo: {arguments}");

            return false;
        }
        public bool PublicCommand()
        {
            return false;
        }
    }
}
