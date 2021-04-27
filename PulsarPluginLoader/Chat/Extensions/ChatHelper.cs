using PulsarPluginLoader.Chat.Commands;
using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PulsarPluginLoader.Chat.Extensions
{
    public class ChatHelper
    {
        public static int cursorPos = 0;
        public static int cursorPos2 = -1;

        public static bool publicCached = false;
        public static Tuple<string, string[][]>[] publicCommands = null;

        public static bool commandsCached = false;
        public static Tuple<string, string[][]>[] chatCommands = null;

        public static LinkedList<string> chatHistory = new LinkedList<string>();
        public static List<Tuple<string, int>> typingHistory = null;

        public static Tuple<string, string[][]>[] getCommands()
        {
            if (!commandsCached)
            {
                string[] aliases = ChatCommandRouter.Instance.getCommandAliases();
                chatCommands = new Tuple<string, string[][]>[aliases.Length];

                for (int i = 0; i < aliases.Length; i++)
                {
                    chatCommands[i] = new Tuple<string, string[][]>(aliases[i], ChatCommandRouter.Instance.GetCommand(aliases[i]).Item1.Arguments());
                }
            }
            return chatCommands;
        }

        public static Tuple<string, string[][]>[] getPublicCommands()
        {
            if (!publicCached)
            {
                HandlePublicCommands.RequestPublicCommands();
                return new Tuple<string, string[][]>[0];
            }
            return publicCommands;
        }

        public static string AutoComplete(string text, int cursorPos)
        {
            if (text[0] != '!' && text[0] != '/')
            {
                return text;
            }
            bool publicCommand = text[0] == '!';
            if (publicCommand && !publicCached)
            {
                HandlePublicCommands.RequestPublicCommands();
                return text;
            }
            string[] split = text.Substring(1).Split(' ');

            //Autocomplete command
            if (split.Length == 1 || (split.Length > 1 && cursorPos >= (text.Length - 1) - split[0].Length))
            {
                int cursor = cursorPos - (text.Length - 1) + split[0].Length;
                string splitText = split[0];
                string extraText = string.Empty;
                if (cursor > 0)
                {
                    splitText = split[0].Substring(0, split[0].Length - cursor);
                    extraText = split[0].Substring(split[0].Length - cursor);
                }
                string match = Match(splitText, new string[] { "%command" }, publicCommand);
                StringBuilder sb = new StringBuilder();
                sb.Append(text[0]);
                sb.Append(match);
                sb.Append(extraText);
                for (int i = 1; i < split.Length; i++)
                {
                    sb.Append(' ');
                    sb.Append(split[i]);
                }
                return sb.ToString();
            }

            //Autocomplete argument
            else if (split.Length > 1)
            {
                int commandIndex = -1;
                Tuple<string, string[][]>[] t = publicCommand ? getPublicCommands() : getCommands();
                for (int i = 0; i < t.Length; i++)
                {
                    if (t[i].Item1.ToLower() == split[0].ToLower())
                    {
                        commandIndex = i;
                        break;
                    }
                }
                if (commandIndex == -1)
                {
                    return text;
                }
                string[][] arguments = t[commandIndex].Item2;
                if (arguments == null)
                {
                    return text;
                }
                int cursor = cursorPos;
                int index = split.Length - 1;
                while (index > 0)
                {
                    if (cursor > split[index].Length)
                    {
                        cursor -= split[index].Length + 1;
                        index--;
                    }
                    else
                    {
                        break;
                    }
                }
                if (arguments.Length >= index)
                {
                    string splitText = split[index];
                    string extraText = string.Empty;
                    if (cursor > 0)
                    {
                        splitText = split[index].Substring(0, split[index].Length - cursor);
                        extraText = split[index].Substring(split[index].Length - cursor);
                    }
                    string match = Match(splitText, arguments[index - 1], publicCommand);
                    StringBuilder sb = new StringBuilder();
                    sb.Append(text[0]);
                    for (int i = 0; i < index; i++)
                    {
                        sb.Append(split[i]);
                        sb.Append(' ');
                    }
                    sb.Append(match);
                    sb.Append(extraText);
                    for (int i = index + 1; i < split.Length; i++)
                    {
                        sb.Append(' ');
                        sb.Append(split[i]);
                    }
                    return sb.ToString();
                }
                else
                {
                    return text;
                }
            }
            else
            {
                return text;
            }
        }

        private static string Match(string text, string[] arguments, bool publicCommand)
        {
            List<string> fixedArguments = new List<string>();
            foreach (string argument in arguments)
            {
                if (argument[0] == '%')
                {
                    switch (argument.Substring(1).ToLower())
                    {
                        case "command":
                            foreach (Tuple<string, string[][]> command in publicCommand ? getPublicCommands() : getCommands())
                            {
                                fixedArguments.Add(command.Item1);
                            }
                            break;
                        case "player_name":
                            foreach (PLPlayer player in PLServer.Instance.AllPlayers)
                            {
                                if (player != null && player.TeamID == 0)
                                {
                                    fixedArguments.Add(player.GetPlayerName());
                                }
                            }
                            break;
                        case "player_role":
                            fixedArguments.Add("captain");
                            fixedArguments.Add("pilot");
                            fixedArguments.Add("scientist");
                            fixedArguments.Add("weapon_specialist");
                            fixedArguments.Add("engineer");
                            break;
                    }
                }
                else
                {
                    fixedArguments.Add(argument);
                }
            }

            List<string> matches = new List<string>();
            foreach (string argument in fixedArguments)
            {
                if (argument.ToLower().StartsWith(text.ToLower()))
                {
                    matches.Add(argument);
                }
            }
            if (matches.Count == 1)
            {
                return matches.ToArray()[0];
            }
            else if (matches.Count > 1)
            {
                string partialMatch = matches[0];
                foreach (string match in matches)
                {
                    Messaging.Notification(match);
                    if (match.Length < partialMatch.Length)
                    {
                        partialMatch = partialMatch.Substring(0, match.Length);
                    }
                    for (int i = 0; i < match.Length && i < partialMatch.Length; i++)
                    {
                        if (match.ToLower()[i] != partialMatch.ToLower()[i])
                        {
                            partialMatch = partialMatch.Substring(0, i);
                        }
                    }
                }
                if (partialMatch.Length > 0)
                {
                    return partialMatch;
                }
            }
            return text;
        }
    }
}
