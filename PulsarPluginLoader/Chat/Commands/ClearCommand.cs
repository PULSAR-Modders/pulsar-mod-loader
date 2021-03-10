namespace PulsarPluginLoader.Chat.Commands
{
    class ClearCommand : IChatCommand
    {
        public string[] CommandAliases()
        {
            return new string[] { "clear" };
        }

        public string Description()
        {
            return "Clears the chat window.";
        }

        public bool Execute(string arguments, int SenderID)
        {
            PLNetworkManager.Instance.ConsoleText.Clear();
            return false;
        }

        public bool PublicCommand()
        {
            return false;
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]}";
        }
    }
}
