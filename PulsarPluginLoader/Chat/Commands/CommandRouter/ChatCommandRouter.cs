using HarmonyLib;
using PulsarPluginLoader.Chat.Commands.CommandRouter;
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
                    _instance.LoadCommandsFromAssembly(Assembly.GetExecutingAssembly(), null);

                    // Attach commands from plugins already loaded
                    foreach (PulsarPlugin p in PluginManager.Instance.GetAllPlugins())
                    {
                        _instance.OnPluginLoaded(p.Name, p);
                    }

                    // Subscribe to plugins loading in the future
                    PluginManager.Instance.OnPluginSuccessfullyLoaded += _instance.OnPluginLoaded;
                    PluginManager.Instance.OnPluginUnloaded += _instance.Unregister;
                }

                return _instance;
            }
        }

        public readonly Dictionary<string, Tuple<ChatCommand, PulsarPlugin>> commands;
        public readonly Dictionary<string, Tuple<PublicCommand, PulsarPlugin>> publicCommands;
        public readonly Dictionary<string, PulsarPlugin> conflictingAliases;
        public readonly Dictionary<string, PulsarPlugin> conflictingPublicAliases;

        public ChatCommandRouter()
        {
            commands = new Dictionary<string, Tuple<ChatCommand, PulsarPlugin>>();
            publicCommands = new Dictionary<string, Tuple<PublicCommand, PulsarPlugin>>();
            conflictingAliases = new Dictionary<string, PulsarPlugin>();
            conflictingPublicAliases = new Dictionary<string, PulsarPlugin>();
        }

        public void Register (ChatCommand cmd, PulsarPlugin plugin)
        {
            foreach (string alias in cmd.CommandAliases())
            {
                string lowerAlias = alias.ToLower();

                if (conflictingAliases.TryGetValue(lowerAlias, out PulsarPlugin plugin2))
                {
                    string name = plugin != null ? plugin.Name : "Pulsar Plugin Loader";
                    string name2 = plugin2 != null ? plugin2.Name : "Pulsar Plugin Loader";
                    Logger.Info($"Conflicting alias: {lowerAlias} from {name} and {name2}");
                }
                else
                {
                    if (commands.TryGetValue(lowerAlias, out Tuple<ChatCommand, PulsarPlugin> t))
                    {
                        conflictingAliases.Add(lowerAlias, plugin);
                        commands.Remove(lowerAlias);
                        string name = plugin != null ? plugin.Name : "Pulsar Plugin Loader";
                        string name2 = t.Item2 != null ? t.Item2.Name : "Pulsar Plugin Loader";
                        Logger.Info($"Conflicting alias: {lowerAlias} from {name} and {name2}");
                    }
                    else
                    {
                        commands.Add(lowerAlias, new Tuple<ChatCommand, PulsarPlugin>(cmd, plugin));
                    }
                }
            }
        }

        public void Register (PublicCommand cmd, PulsarPlugin plugin)
        {
            foreach (string alias in cmd.CommandAliases())
            {
                string lowerAlias = alias.ToLower();

                if (conflictingPublicAliases.TryGetValue(lowerAlias, out PulsarPlugin plugin2))
                {
                    string name = plugin != null ? plugin.Name : "Pulsar Plugin Loader";
                    string name2 = plugin2 != null ? plugin2.Name : "Pulsar Plugin Loader";
                    Logger.Info($"Conflicting public alias: {lowerAlias} from {name} and {name2}");
                }
                else
                {
                    if (publicCommands.TryGetValue(lowerAlias, out Tuple<PublicCommand, PulsarPlugin> t))
                    {
                        conflictingPublicAliases.Add(lowerAlias, plugin);
                        publicCommands.Remove(lowerAlias);
                        string name = plugin != null ? plugin.Name : "Pulsar Plugin Loader";
                        string name2 = t.Item2 != null ? t.Item2.Name : "Pulsar Plugin Loader";
                        Logger.Info($"Conflicting public alias: {lowerAlias} from {name} and {name2}");
                    }
                    else
                    {
                        publicCommands.Add(lowerAlias, new Tuple<PublicCommand, PulsarPlugin>(cmd, plugin));
                    }
                }
            }
        }

        public Tuple<ChatCommand, PulsarPlugin> GetCommand(string alias)
        {
            string lowerAlias = alias.ToLower();

            if (commands.TryGetValue(lowerAlias, out Tuple<ChatCommand, PulsarPlugin> t))
            {
                return t;
            }

            return null;
        }

        public Tuple<PublicCommand, PulsarPlugin> GetPublicCommand(string alias)
        {
            string lowerAlias = alias.ToLower();

            if (publicCommands.TryGetValue(lowerAlias, out Tuple<PublicCommand, PulsarPlugin> t))
            {
                return t;
            }

            return null;
        }

        public void Unregister(PulsarPlugin plugin)
        {
            List<string> commandsToRemove = new List<string>();
            
            foreach (var command in commands)
                if(command.Value.Item2 == plugin) 
                    commandsToRemove.Add(command.Key);
            foreach (var command in commandsToRemove)
                commands.Remove(command);
            
            commandsToRemove.Clear();
            
            foreach (var command in publicCommands)
                if(command.Value.Item2 == plugin) 
                    commandsToRemove.Add(command.Key);
            foreach (var command in commandsToRemove)
                publicCommands.Remove(command);
        }

        public IOrderedEnumerable<Tuple<ChatCommand, PulsarPlugin>> GetCommands()
        {
            return new HashSet<Tuple<ChatCommand, PulsarPlugin>>(commands.Values).OrderBy(t => t.Item1.CommandAliases()[0]);
        }

        public IOrderedEnumerable<Tuple<PublicCommand, PulsarPlugin>> GetPublicCommands()
        {
            return new HashSet<Tuple<PublicCommand, PulsarPlugin>>(publicCommands.Values).OrderBy(t => t.Item1.CommandAliases()[0]);
        }

        public string[] getCommandAliases()
        {
            return commands.Keys.ToArray();
        }

        public string[] getPublicCommandAliases()
        {
            return publicCommands.Keys.ToArray();
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

                if (commands.TryGetValue(alias, out Tuple<ChatCommand, PulsarPlugin> t))
                {
                    fallthroughToDevCommands = false;
                    try
                    {
                        t.Item1.Execute(arguments.Trim());
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
            LoadCommandsFromAssembly(asm, plugin);
        }

        private void LoadCommandsFromAssembly(Assembly asm, PulsarPlugin plugin)
        {
            Type ChatCmd = typeof(ChatCommand);
            Type PublicCmd = typeof(PublicCommand);

            foreach (Type t in asm.GetTypes())
            {
                if (ChatCmd.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                {
                    Register((ChatCommand)Activator.CreateInstance(t), plugin);
                }
                else if (PublicCmd.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                {
                    Register((PublicCommand)Activator.CreateInstance(t), plugin);
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

                if (ChatCommandRouter.Instance.publicCommands.TryGetValue(alias, out Tuple<PublicCommand, PulsarPlugin> t))
                {
                    try
                    {
                        t.Item1.Execute(arguments.Trim(), playerID);
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
