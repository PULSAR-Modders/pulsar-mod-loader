using PulsarPluginLoader.Chat.Commands;
using System;

namespace PulsarPluginLoader.Tests.Chat.Commands
{
    class PrintCommand : IChatCommand
    {
        public string[] CommandAliases()
        {
            return new string[] { "print", "p" };
        }

        public bool Execute(string arguments, int SenderID)
        {
            Console.WriteLine(arguments);
            return false;
        }

        public string Description()
        {
            return "Repeats the input text by printing to console.";
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]} [text]";
        }

        public bool PublicCommand()
        {
            return false;
        }
    }
}
