using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static PulsarPluginLoader.Chat.Extensions.ChatHelper;

namespace PulsarPluginLoader.Chat.Extensions
{
    [HarmonyPatch(typeof(PLNetworkManager), "Update")]
    class HarmonyNetworkUpdate
    {
        private static string currentChatText;

        private static long lastTimePaste = long.MaxValue;
        private static long lastTimeDelete = long.MaxValue;
        private static long lastTimeUndo = long.MaxValue;
        private static long lastTimeRedo = long.MaxValue;

        private static void SetChat(PLNetworkManager instance)
        {
            if (currentHistory == null)
            {
                instance.CurrentChatText = "";
            }
            else
            {
                instance.CurrentChatText = currentHistory.Value;
            }
        }

        private static void DeleteSelected()
        {
            ChatHelper.UpdateTypingHistory(currentChatText, false, true);
            int pos;
            int length;
            if (cursorPos < cursorPos2)
            {
                pos = currentChatText.Length - cursorPos2;
                length = cursorPos2 - cursorPos;
            }
            else
            {
                pos = currentChatText.Length - cursorPos;
                length = cursorPos - cursorPos2;
                cursorPos = cursorPos2;
            }
            cursorPos2 = -1;
            currentChatText = currentChatText.Remove(pos, length);
        }

        static void Prefix(PLNetworkManager __instance)
        {
            currentChatText = __instance.CurrentChatText;
            if (__instance.IsTyping)
            {
                foreach (char c in Input.inputString)
                {
                    if (c == "\b"[0])
                    {
                        if (cursorPos2 != -1 && cursorPos2 != cursorPos)
                        {
                            DeleteSelected();
                            ChatHelper.UpdateTypingHistory(currentChatText, false, true);
                        }
                        else
                        {
                            if (cursorPos != currentChatText.Length)
                            {
                                ChatHelper.UpdateTypingHistory(currentChatText, false);
                                currentChatText = currentChatText.Remove(currentChatText.Length - cursorPos - 1, 1);
                            }
                        }
                    }
                    else if (c == Environment.NewLine[0] || c == "\r"[0])
                    {
                        //Do nothing
                    }
                    else
                    {
                        if (cursorPos2 != -1 && cursorPos2 != cursorPos)
                        {
                            DeleteSelected();
                        }
                        ChatHelper.UpdateTypingHistory(currentChatText, true);
                        currentChatText = currentChatText.Insert(currentChatText.Length - cursorPos, c.ToString());
                    }
                }
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    lastTimeDelete = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + /*(SystemInformation.KeyboardDelay + 1) **/ 250;
                    if (cursorPos2 != -1 && cursorPos2 != cursorPos)
                    {
                        DeleteSelected();
                        ChatHelper.UpdateTypingHistory(currentChatText, false, true);
                    }
                    else
                    {
                        ChatHelper.UpdateTypingHistory(currentChatText, false);
                        currentChatText = currentChatText.Remove(currentChatText.Length - cursorPos, 1);
                        cursorPos--;
                    }
                }
                if (Input.GetKey(KeyCode.Delete) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimeDelete)
                {
                    ChatHelper.UpdateTypingHistory(currentChatText, false);
                    lastTimeDelete += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                    currentChatText = currentChatText.Remove(currentChatText.Length - cursorPos, 1);
                    cursorPos--;
                }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    if (currentChatText.StartsWith("/"))
                    {
                        string chatText = AutoComplete(currentChatText, cursorPos);
                        if (chatText != currentChatText)
                        {
                            ChatHelper.UpdateTypingHistory(currentChatText, true, true);
                            currentChatText = chatText;
                            cursorPos2 = -1;
                        }
                    }
                    else if (currentChatText.StartsWith("!"))
                    {
                        if (publicCached)
                        {
                            string chatText = AutoComplete(currentChatText, cursorPos);
                            if (chatText != currentChatText)
                            {
                                ChatHelper.UpdateTypingHistory(currentChatText, true, true);
                                currentChatText = chatText;
                                cursorPos2 = -1;
                            }
                        }
                        else
                        {
                            HandlePublicCommands.RequestPublicCommands();
                        }
                    }
                }
            }
        }

        static void Postfix(PLNetworkManager __instance)
        {
            if (!__instance.IsTyping)
            {
                currentHistory = null;
                return;
            }

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        lastTimeRedo = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + /*(SystemInformation.KeyboardDelay + 1) **/ 250;
                        ChatHelper.Redo(ref currentChatText);
                    }
                    if (Input.GetKey(KeyCode.Z) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimeRedo)
                    {
                        lastTimeRedo += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                        ChatHelper.Redo(ref currentChatText);
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        lastTimeUndo = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + /*(SystemInformation.KeyboardDelay + 1) **/ 250;
                        ChatHelper.Undo(ref currentChatText);
                    }
                    if (Input.GetKey(KeyCode.Z) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimeUndo)
                    {
                        lastTimeUndo += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                        ChatHelper.Undo(ref currentChatText);
                    }
                }
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    lastTimeRedo = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + /*(SystemInformation.KeyboardDelay + 1) **/ 250;
                    ChatHelper.Redo(ref currentChatText);
                }
                if (Input.GetKey(KeyCode.Y) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimeRedo)
                {
                    lastTimeRedo += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                    ChatHelper.Redo(ref currentChatText);
                }

                if (Input.GetKeyDown(KeyCode.V))
                {
                    ChatHelper.UpdateTypingHistory(currentChatText, true, true);
                    lastTimePaste = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + /*(SystemInformation.KeyboardDelay + 1) **/ 250;
                    if (cursorPos2 != -1 && cursorPos2 != cursorPos)
                    {
                        DeleteSelected();
                    }
                    currentChatText = currentChatText.Insert(currentChatText.Length - cursorPos, GUIUtility.systemCopyBuffer);
                    ChatHelper.UpdateTypingHistory(currentChatText, true, true);
                }
                if (Input.GetKey(KeyCode.V) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimePaste)
                {
                    ChatHelper.UpdateTypingHistory(currentChatText, true, true);
                    lastTimePaste += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                    currentChatText = currentChatText.Insert(currentChatText.Length - cursorPos, GUIUtility.systemCopyBuffer);
                    ChatHelper.UpdateTypingHistory(currentChatText, true, true);
                }
                if (Input.GetKeyDown(KeyCode.C) && cursorPos2 != -1 && cursorPos2 != cursorPos)
                {
                    int pos;
                    int length;
                    if (cursorPos < cursorPos2)
                    {
                        pos = currentChatText.Length - cursorPos2;
                        length = cursorPos2 - cursorPos;
                    }
                    else
                    {
                        pos = currentChatText.Length - cursorPos;
                        length = cursorPos - cursorPos2;
                    }
                    GUIUtility.systemCopyBuffer = currentChatText.Substring(pos, length);
                }
                if (Input.GetKeyDown(KeyCode.X) && cursorPos2 != -1 && cursorPos2 != cursorPos)
                {
                    ChatHelper.UpdateTypingHistory(currentChatText, false, true);
                    int pos;
                    int length;
                    if (cursorPos < cursorPos2)
                    {
                        pos = currentChatText.Length - cursorPos2;
                        length = cursorPos2 - cursorPos;
                    }
                    else
                    {
                        pos = currentChatText.Length - cursorPos;
                        length = cursorPos - cursorPos2;
                    }
                    GUIUtility.systemCopyBuffer = currentChatText.Substring(pos, length);
                    DeleteSelected();
                    ChatHelper.UpdateTypingHistory(currentChatText, false, true);
                }
                if (Input.GetKeyDown(KeyCode.A))
                {
                    cursorPos = 0;
                    cursorPos2 = currentChatText.Length;
                }
            }

            __instance.CurrentChatText = currentChatText;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                cursorPos = 0;
                cursorPos2 = -1;
                if (currentHistory == null)
                {
                    ChatHelper.UpdateTypingHistory(currentChatText, true, true);
                    currentHistory = chatHistory.Last;
                }
                else
                {
                    currentHistory = currentHistory.Previous;
                }
                SetChat(__instance);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                cursorPos = 0;
                cursorPos2 = -1;
                if (currentHistory == null)
                {
                    currentHistory = chatHistory.First;
                }
                else
                {
                    currentHistory = currentHistory.Next;
                }
                SetChat(__instance);
            }
        }
    }
}
