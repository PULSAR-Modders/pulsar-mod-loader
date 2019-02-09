using NUnit.Framework;
using PulsarPluginLoader.Chat.Commands;
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
            IChatCommand cmd = new PrintCommand();

            ccr.Register(cmd);

            Assert.AreSame(cmd, ccr.GetCommand(cmd.CommandAliases()[0]));
        }

        [Test]
        public void CanDeregisterCommand()
        {
            ChatCommandRouter ccr = new ChatCommandRouter();
            IChatCommand cmd = new PrintCommand();

            ccr.Register(cmd);
            ccr.Deregister(cmd);

            Assert.IsNull(ccr.GetCommand(cmd.CommandAliases()[0]));
        }

        [Test]
        public void CantDeregisterWrongCommand()
        {
            ChatCommandRouter ccr = new ChatCommandRouter();
            IChatCommand cmd = new PrintCommand();

            ccr.Register(cmd);
            ccr.Deregister("asd");

            Assert.IsNotNull(ccr.GetCommand(cmd.CommandAliases()[0]));
        }

        [Test]
        public void CanGetCommand()
        {
            ChatCommandRouter ccr = new ChatCommandRouter();
            IChatCommand cmd = new PrintCommand();

            ccr.Register(cmd);
            IChatCommand retrievedCmd = ccr.GetCommand(cmd.CommandAliases()[0]);

            Assert.AreSame(cmd, retrievedCmd);
        }

        [Test]
        public void CanExecuteCommand()
        {
            ChatCommandRouter ccr = new ChatCommandRouter();
            IChatCommand cmd = new PrintCommand();

            ccr.Register(cmd);

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
