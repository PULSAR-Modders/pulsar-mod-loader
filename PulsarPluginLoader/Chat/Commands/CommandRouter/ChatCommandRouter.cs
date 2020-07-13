using HarmonyLib;
using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PulsarPluginLoader.Chat.Commands
{
    public class ChatCommandRouter
    {
        private static ChatCommandRouter _instance = null;

        public static ChatCommandRouter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ChatCommandRouter();

                    // Attach commands from PPL since it doesn't count as a plugin
                    _instance.LoadCommandsFromAssembly(Assembly.GetExecutingAssembly());

                    // Attach commands from plugins already loaded
                    foreach (PulsarPlugin p in PluginManager.Instance.GetAllPlugins())
                    {
                        _instance.OnPluginLoaded(p.Name, p);
                    }

                    // Subscribe to plugins loading in the future
                    PluginManager.Instance.OnPluginSuccessfullyLoaded += _instance.OnPluginLoaded;
                }

                return _instance;
            }
        }

        public readonly Dictionary<string, IChatCommand> commands;

        public ChatCommandRouter()
        {
            commands = new Dictionary<string, IChatCommand>();
        }

        public void Register(IChatCommand cmd)
        {
            foreach (string alias in cmd.CommandAliases())
            {
                string lowerAlias = alias.ToLower();

                if (!commands.ContainsKey(lowerAlias))
                {
                    commands.Add(lowerAlias, cmd);
                }
                else
                {
                    throw new ArgumentException($"Failed to add chat command alias '{alias}'; alias already registered!");
                }
            }
        }

        public void Deregister(IChatCommand cmd)
        {
            if (cmd != null)
            {
                foreach (string alias in cmd.CommandAliases())
                {
                    string lowerAlias = alias.ToLower();

                    if (commands.TryGetValue(lowerAlias, out IChatCommand tempCmd) && tempCmd == cmd)
                    {
                        commands.Remove(lowerAlias);
                    }
                }
            }
        }

        public void Deregister(string alias)
        {
            Deregister(GetCommand(alias));
        }

        public IChatCommand GetCommand(string alias)
        {
            string lowerAlias = alias.ToLower();

            if (commands.TryGetValue(lowerAlias, out IChatCommand cmd))
            {
                return cmd;
            }

            return null;
        }

        public IEnumerable<IChatCommand> GetCommands()
        {
            return new HashSet<IChatCommand>(commands.Values).OrderBy(cmd => cmd.CommandAliases()[0]);
        }

        public bool FindAndExecute(string chatInput)
        {
            bool fallthroughToDevCommands = true;

            if (chatInput.StartsWith("/"))
            {
                // Strip surrounding whitespace, remove leading slash, and split command from arguments
                string[] splitInput = chatInput.Trim().Substring(1).Split(new char[] { ' ' }, 2);
                string alias = splitInput[0].ToLower();
                string arguments = splitInput.Length > 1 ? splitInput[1] : String.Empty;

                if (commands.TryGetValue(alias, out IChatCommand cmd) && !cmd.PublicCommand())
                {
                    try
                    {
                        fallthroughToDevCommands = cmd.Execute(arguments.Trim(), PLNetworkManager.Instance.LocalPlayerID);
                    }
                    catch
                    {
                        Logger.Info($"Chat Command Exception -- Input: {chatInput}");
                        throw;
                    }
                }
            }

            return fallthroughToDevCommands;
        }

        public void OnPluginLoaded(string name, PulsarPlugin plugin)
        {
            Assembly asm = plugin.GetType().Assembly;
            LoadCommandsFromAssembly(asm);
        }

        private void LoadCommandsFromAssembly(Assembly asm)
        {
            Type iChatCmd = typeof(IChatCommand);

            foreach (Type t in asm.GetTypes())
            {
                if (iChatCmd.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                {
                    Register((IChatCommand)Activator.CreateInstance(t));
                }
            }
        }
    }
    [HarmonyPatch(typeof(PLServer), "TeamMessage")]
    class ReceiveClientMessage //handles messages in global chat.
    {
        static void Postfix(string message, int playerID)
        {
            string text = message.Replace("[&%~[C", string.Empty).Replace(" ]&%~]", string.Empty);
            if (text.StartsWith("!") && PhotonNetwork.isMasterClient)
            {
                // Strip surrounding whitespace, remove leading slash, and split command from arguments
                string[] splitInput = text.Trim().Substring(1).Split(new char[] { ' ' }, 2);
                string alias = splitInput[0].ToLower();
                string arguments = splitInput.Length > 1 ? splitInput[1] : string.Empty;

                if (ChatCommandRouter.Instance.commands.TryGetValue(alias, out IChatCommand cmd) && cmd.PublicCommand())
                {
                    try
                    {
                        cmd.Execute(arguments.Trim(), playerID);
                    }
                    catch
                    {
                        Logger.Info($"Chat Command Exception -- Input: {text}");
                        throw;
                    }
                }
            }
        }
    }
}
