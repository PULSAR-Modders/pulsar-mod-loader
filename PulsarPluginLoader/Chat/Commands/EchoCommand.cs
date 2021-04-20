using PulsarPluginLoader.Chat.Commands.CommandRouter;
using PulsarPluginLoader.Utilities;

namespace PulsarPluginLoader.Chat.Commands
{
    public class EchoCommand : ChatCommand
    {
        public override string[] CommandAliases()
        {
            return new string[] { "echo", "e" };
        }

        public override string Description()
        {
            return "Repeats the input text back through the chat box.";
        }

        public override string[] UsageExamples()
        {
            return new string[] { $"/{CommandAliases()[0]} <text>" };
        }

        public override void Execute(string arguments)
        {
            Messaging.Echo(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Echo: {arguments}");
        }
    }
}
