using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static PulsarPluginLoader.Chat.Extensions.ChatHelper;

namespace PulsarPluginLoader.Chat.Extensions
{
    [HarmonyPatch(typeof(PLInGameUI), "HandleChat")]
    class HarmonyHandleChat
    {
        private static long lastTimeLeft = long.MaxValue;
        private static long lastTimeRight = long.MaxValue;

        private static bool TagFound(string str, PLNetworkManager networkManager, int pos)
        {
            if (pos <= 0)
            {
                return false;
            }
            bool tagFound = false;
            for (int i = str.Length - pos; i < str.Length; i++)
            {
                if (networkManager.CurrentChatText[i] == '>')
                {
                    tagFound = true;
                    break;
                }
                else if (networkManager.CurrentChatText[i] == '<')
                {
                    break;
                }
            }
            if (tagFound)
            {
                tagFound = false;
                for (int i = str.Length - pos - 1; i >= 0; i--)
                {
                    if (networkManager.CurrentChatText[i] == '<')
                    {
                        tagFound = true;
                        break;
                    }
                    else if (networkManager.CurrentChatText[i] == '>')
                    {
                        break;
                    }
                }
            }
            return tagFound;
        }

        private static bool Test(string chatText, char[] match, int pos)
        {
            foreach (char c in match)
            {
                if (chatText[pos] == c)
                {
                    return true;
                }
            }
            return false;
        }

        private static int Search(string chatText, char[] match, bool left)
        {
            int pos = chatText.Length - cursorPos;
            bool last;

            if (left)
            {
                pos--;
                bool current = Test(chatText, match, pos);
                while (pos > 0)
                {
                    last = current;
                    current = Test(chatText, match, pos);
                    if (current && !last)
                    {
                        return chatText.Length - pos - 1;
                    }
                    pos--;
                }
                return chatText.Length;
            }
            else
            {
                bool current = Test(chatText, match, pos);
                while (pos < chatText.Length)
                {
                    last = current;
                    current = Test(chatText, match, pos);
                    if (current && !last)
                    {
                        return chatText.Length - pos;
                    }
                    pos++;
                }
                return 0;
            }
        }

        private static void HandleArrows(string chatText)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                lastTimeLeft = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + /*(SystemInformation.KeyboardDelay + 1) **/ 250;
                if (cursorPos < chatText.Length)
                {
                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        cursorPos = Search(chatText, new char[] { ' ', '>' }, true);
                    }
                    else
                    {
                        cursorPos++;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                lastTimeRight = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + /*(SystemInformation.KeyboardDelay + 1) **/ 250;
                if (cursorPos > 0)
                {
                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        cursorPos = Search(chatText, new char[] { ' ', '<' }, false);
                    }
                    else
                    {
                        cursorPos--;
                    }
                }
            }
            if (Input.GetKey(KeyCode.LeftArrow) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimeLeft)
            {
                lastTimeLeft += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                if (cursorPos < chatText.Length)
                {
                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        cursorPos = Search(chatText, new char[] { ' ', '>' }, true);
                    }
                    else
                    {
                        cursorPos++;
                    }
                }
            }
            if (Input.GetKey(KeyCode.RightArrow) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimeRight)
            {
                lastTimeRight += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                if (cursorPos > 0)
                {
                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        cursorPos = Search(chatText, new char[] { ' ', '<' }, false);
                    }
                    else
                    {
                        cursorPos--;
                    }
                }
            }
        }

        static void Prefix(PLInGameUI __instance, ref bool ___evenChatString, ref string __state)
        {
            PLNetworkManager networkManager = PLNetworkManager.Instance;
            if (networkManager.IsTyping)
            {
                ___evenChatString = false;
                __state = networkManager.CurrentChatText;

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (cursorPos2 == -1)
                    {
                        cursorPos2 = cursorPos;
                    }
                    HandleArrows(__state);
                    if (Input.GetKeyDown(KeyCode.Home))
                    {
                        cursorPos = __state.Length;
                    }
                    if (Input.GetKeyDown(KeyCode.End))
                    {
                        cursorPos = 0;
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                        (Input.GetKey(KeyCode.LeftArrow) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimeLeft) ||
                        (Input.GetKey(KeyCode.RightArrow) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimeRight))
                    {
                        cursorPos2 = -1;
                        HandleArrows(__state);
                    }
                    if (Input.GetKeyDown(KeyCode.Home))
                    {
                        cursorPos2 = -1;
                        cursorPos = __state.Length;
                    }
                    if (Input.GetKeyDown(KeyCode.End))
                    {
                        cursorPos2 = -1;
                        cursorPos = 0;
                    }
                }

                if (networkManager.CurrentChatText != null)
                {
                    if (TagFound(__state, networkManager, cursorPos) || TagFound(__state, networkManager, cursorPos2))
                    {
                        __instance.ChatLabel.supportRichText = false;
                        __instance.ChatShadowLabel.supportRichText = false;
                        __instance.ChatShadow2Label.supportRichText = false;
                    }
                    else
                    {
                        __instance.ChatLabel.supportRichText = true;
                        __instance.ChatShadowLabel.supportRichText = true;
                        __instance.ChatShadow2Label.supportRichText = true;
                    }
                    networkManager.CurrentChatText = networkManager.CurrentChatText.Insert(__state.Length - cursorPos, DateTime.Now.Millisecond >= 500 ? "|" : "'");
                    if (cursorPos2 != -1 && cursorPos2 != cursorPos)
                    {
                        networkManager.CurrentChatText = networkManager.CurrentChatText.Insert(__state.Length - cursorPos2 + (cursorPos > cursorPos2 ? 1 : 0), DateTime.Now.Millisecond >= 500 ? "¦" : "'");
                    }
                }
            }
            else
            {
                if (ChatHelper.isTyping)
                {
                    ChatHelper.isTyping = false;
                    ChatHelper.typingHistory = null;
                }

                cursorPos = 0;
                cursorPos2 = -1;
                __instance.ChatLabel.supportRichText = true;
                __instance.ChatShadowLabel.supportRichText = true;
                __instance.ChatShadow2Label.supportRichText = true;
            }
        }

        static void Postfix(PLInGameUI __instance, ref string __state)
        {
            if (PLNetworkManager.Instance.IsTyping)
            {
                PLNetworkManager.Instance.CurrentChatText = __state;
            }
        }

        //Fixes shadow in currently typing
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /*List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Brfalse_S),
                new CodeInstruction(OpCodes.Ldloc_S),
                new CodeInstruction(OpCodes.Ldstr),
                new CodeInstruction(OpCodes.Callvirt),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldloc_S),
                new CodeInstruction(OpCodes.Ldsfld),
                new CodeInstruction(OpCodes.Ldfld)
            };

            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyColoredMessage), "RemoveColor"))
            };

            return PatchBySequence(instructions, targetSequence, injectedSequence, PatchMode.AFTER, CheckMode.NEVER); */
            return new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Brfalse_S), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldstr), new CodeMatch(OpCodes.Callvirt), new CodeMatch(OpCodes.Pop), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldsfld), new CodeMatch(OpCodes.Ldfld))
                .Advance(7).Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyColoredMessage), "RemoveColor"))).InstructionEnumeration();
        }
    }
}
