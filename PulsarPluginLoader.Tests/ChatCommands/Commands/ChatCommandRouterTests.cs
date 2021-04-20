using NUnit.Framework;
using PulsarPluginLoader.Chat.Commands;
using PulsarPluginLoader.Chat.Commands.CommandRouter;
using System;
using System.IO;

namespace PulsarPluginLoader.Tests.Chat.Commands
{
    [TestFixture]
    class ChatCommandRouterTests
    {
        [Test]
        public void CanRegisterCommand()
        {
            ChatCommandRouter ccr = new ChatCommandRouter();
            ChatCommand cmd = new PrintCommand();

            ccr.Register(cmd, null);

            Assert.AreSame(cmd, ccr.GetCommand(cmd.CommandAliases()[0]).Item1);
        }

        [Test]
        public void CanGetCommand()
        {
            ChatCommandRouter ccr = new ChatCommandRouter();
            ChatCommand cmd = new PrintCommand();

            ccr.Register(cmd, null);
            ChatCommand retrievedCmd = ccr.GetCommand(cmd.CommandAliases()[0]).Item1;

            Assert.AreSame(cmd, retrievedCmd);
        }

        [Test]
        public void CanExecuteCommand()
        {
            ChatCommandRouter ccr = new ChatCommandRouter();
            ChatCommand cmd = new PrintCommand();

            ccr.Register(cmd, null);

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                string alias = cmd.CommandAliases()[0];
                string expected = "Test Input";
                ccr.FindAndExecute($"/{alias} {expected}");

                Assert.AreEqual(expected, sw.ToString().TrimEnd());
            }
        }
    }
}
