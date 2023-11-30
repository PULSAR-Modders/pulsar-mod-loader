using PulsarModLoader.Chat.Commands.CommandRouter;
using PulsarModLoader.Utilities;


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace PulsarModLoader.Chat.Commands
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
#pragma warning restore CS1591
}
