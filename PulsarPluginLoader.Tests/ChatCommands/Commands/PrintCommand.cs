using PulsarPluginLoader.Chat.Commands.CommandRouter;
using System;

namespace PulsarPluginLoader.Tests.Chat.Commands
{
    class PrintCommand : ChatCommand
    {
        public override string[] CommandAliases()
        {
            return new string[] { "print", "p" };
        }

        public override void Execute(string arguments)
        {
            Console.WriteLine(arguments);
        }

        public override string Description()
        {
            return "Repeats the input text by printing to console.";
        }

        public override string[] UsageExamples()
        {
            return new string[] { $"/{CommandAliases()[0]} <text>" };
        }
    }
}
