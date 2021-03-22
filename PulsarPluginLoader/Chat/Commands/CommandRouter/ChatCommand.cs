
namespace PulsarPluginLoader.Chat.Commands.CommandRouter
{
    public abstract class ChatCommand
    {
        /// <summary>
        /// This will throw an exception if the command name is not unique
        /// </summary>
        /// <returns>The command to be entered by the user</returns>
        public abstract string CommandName();
        /// <summary>
        /// Command aliases will fail silently if the alias is not unique
        /// </summary>
        /// <returns>An array containing alternate names for the command that can be used by the player</returns>
        public string[] CommandAliases()
        {
            return new string[] { };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>A short description of what the command does</returns>
        public abstract string Description();
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Examples of how to use the command including what arguments are valid</returns>
        public string[] UsageExamples()
        {
            return new string[] { $"/{CommandName()}" };
        }
        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="arguments">A string containing all of the text entered after the command</param>
        public abstract void Execute(string arguments);
    }
}
