
using System.Text;

namespace PulsarModLoader.Chat.Commands.CommandRouter
{
    public abstract class PublicCommand
    {
        /// <summary>
        /// Command aliases will fail silently if the alias is not unique
        /// </summary>
        /// <returns>An array containing names for the command that can be used by the player</returns>
        public abstract string[] CommandAliases();
        /// <summary>
        /// 
        /// </summary>
        /// <returns>A short description of what the command does</returns>
        public abstract string Description();
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Examples of how to use the command including what arguments are valid</returns>
        public virtual string[] UsageExamples()
        {
            string[][] arguments = Arguments();
            if (arguments != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"!{CommandAliases()[0]}");
                foreach (string[] argumentArray in arguments)
                {
                    sb.Append(" [");
                    foreach (string argument in argumentArray)
                    {
                        if (argument[0] == '%')
                        {
                            sb.Append(argument.Substring(1).Replace('_', ' '));
                        }
                        else
                        {
                            sb.Append(argument);
                        }
                        sb.Append(" | ");
                    }
                    sb.Remove(sb.Length - 3, 3);
                    sb.Append("]");
                }
                return new string[] { sb.ToString() };
            }
            else
            {
                return new string[] { $"!{CommandAliases()[0]}" };
            }
        }
        /// <summary>
        /// Example:<br/>
        /// /command &lt;player name | player role&gt; &lt;number&gt; [yes | no | customArgument]<br/>
        /// should be written as<br/>
        /// return new string[][] { new string[] { "%player_name", "%player_role" }, new string[] { "%number" }, new string[] { "yes", "no", "customArgument" } };
        /// </summary>
        /// <returns></returns>
        public virtual string[][] Arguments()
        {
            return null;
        }
        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="arguments">A string containing all of the text entered after the command</param>
        /// <param name="SenderID">The PlayerID of the player who sent the message</param>
        public abstract void Execute(string arguments, int SenderID);
    }
}
