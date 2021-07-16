#if DEBUG
using PulsarPluginLoader.Chat.Commands.CommandRouter;
using System.IO;
using System.Reflection;

namespace PulsarPluginLoader.Chat.Commands
{
    class LoadPluginCommand : ChatCommand // Debug only
    {
        public override string[] CommandAliases()
        {
            return new string[] { "load" };
        }

        public override string Description()
        {
            return "Load selected plugin.";
        }

        public override string[][] Arguments() => new string[][] { new string[] { "<filename>" } };

        public override void Execute(string arguments) => PluginManager.Instance.LoadPlugin(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Mods", arguments + ".dll"));
    }
}
#endif
