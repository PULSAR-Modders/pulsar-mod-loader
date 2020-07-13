namespace PulsarPluginLoader.Chat.Commands
{
    public interface IChatCommand
    {
        string[] CommandAliases();
        string Description();
        string UsageExample();
        bool Execute(string arguments, int SenderID);
        bool PublicCommand();
    }
}
