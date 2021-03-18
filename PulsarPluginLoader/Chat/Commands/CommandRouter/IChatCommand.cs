namespace PulsarPluginLoader.Chat.Commands
{
    public interface IChatCommand
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>An array containing all of the names for the command that can be used by the player</returns>
        string[] CommandAliases();
        /// <summary>
        /// 
        /// </summary>
        /// <returns>A short description of what the command does</returns>
        string Description();
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Examples of how to use the command including what arguments are valid</returns>
        string UsageExample();
        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="arguments">A string containing all of the text entered after the command</param>
        /// <param name="SenderID">The PlayerID of the player who sent the message</param>
        /// <returns>'true' if the commmand should be checked by dev-hax then sent to in-game chat<para />
        /// 'false' if the command should not be sent to chat or seen by dev-hax</returns>
        bool Execute(string arguments, int SenderID);
        /// <summary>
        /// Allows other players without the mod to use the command by typing "!&lt;command&gt;" in chat<para/>
        /// Only checked if the mod owner is the host
        /// </summary>
        /// <returns>'true' if other players should be able to use this command</returns>
        bool PublicCommand();
    }
}
