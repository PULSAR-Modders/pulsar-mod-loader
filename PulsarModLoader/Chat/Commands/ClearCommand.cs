using PulsarModLoader.Chat.Commands.CommandRouter;

namespace PulsarModLoader.Chat.Commands
{
    class ClearCommand : ChatCommand
    {
        public override string[] CommandAliases()
        {
            return new string[] { "clear" };
        }

        public override string Description()
        {
            return "Clears the chat window.";
        }

        public override void Execute(string arguments)
        {
            PLNetworkManager.Instance.ConsoleText.Clear();
        }
    }
}
