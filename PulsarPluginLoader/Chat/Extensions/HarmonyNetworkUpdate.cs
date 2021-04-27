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
        private static LinkedListNode<string> curentHistory = null;

        private static string currentChatText;

        private static bool textModified = false;

        private static long lastTimePaste = long.MaxValue;
        private static long lastTimeDelete = long.MaxValue;

        private static void SetChat(PLNetworkManager instance)
        {
            if (curentHistory == null)
            {
                instance.CurrentChatText = "";
            }
            else
            {
                instance.CurrentChatText = curentHistory.Value;
            }
        }

        private static void DeleteSelected()
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
                cursorPos = cursorPos2;
            }
            cursorPos2 = -1;
            currentChatText = currentChatText.Remove(pos, length);
        }

        static void Prefix(PLNetworkManager __instance)
        {
            currentChatText = __instance.CurrentChatText;
            if (__instance.IsTyping && (cursorPos > 0 || cursorPos2 > 0))
            {
                foreach (char c in Input.inputString)
                {
                    if (c == "\b"[0])
                    {
                        if (cursorPos2 != -1 && cursorPos2 != cursorPos)
                        {
                            DeleteSelected();
                        }
                        else
                        {
                            if (cursorPos != currentChatText.Length)
                            {
                                currentChatText = currentChatText.Remove(currentChatText.Length - cursorPos - 1, 1);
                            }
                        }
                        textModified = true;
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
                        currentChatText = currentChatText.Insert(currentChatText.Length - cursorPos, c.ToString());
                        textModified = true;
                    }
                }
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    lastTimeDelete = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + /*(SystemInformation.KeyboardDelay + 1) **/ 250;
                    if (cursorPos2 != -1 && cursorPos2 != cursorPos)
                    {
                        DeleteSelected();
                    }
                    else
                    {
                        currentChatText = currentChatText.Remove(currentChatText.Length - cursorPos, 1);
                        cursorPos--;
                    }
                    textModified = true;
                }
                if (Input.GetKey(KeyCode.Delete) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimeDelete)
                {
                    lastTimeDelete += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                    currentChatText = currentChatText.Remove(currentChatText.Length - cursorPos, 1);
                    cursorPos--;
                    textModified = true;
                }
            }
            if (__instance.IsTyping)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    if (currentChatText.StartsWith("/"))
                    {
                        string chatText = AutoComplete(currentChatText, cursorPos);
                        if (chatText != currentChatText)
                        {
                            textModified = true;
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
                                textModified = true;
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
                curentHistory = null;
                return;
            }

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.V))
                {
                    lastTimePaste = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + /*(SystemInformation.KeyboardDelay + 1) **/ 250;
                    if (cursorPos2 != -1 && cursorPos2 != cursorPos)
                    {
                        DeleteSelected();
                    }
                    currentChatText = currentChatText.Insert(currentChatText.Length - cursorPos, GUIUtility.systemCopyBuffer);
                    textModified = true;
                }
                if (Input.GetKey(KeyCode.V) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimePaste)
                {
                    lastTimePaste += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                    currentChatText = currentChatText.Insert(currentChatText.Length - cursorPos, GUIUtility.systemCopyBuffer);
                    textModified = true;
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
                    textModified = true;
                }
                if (Input.GetKeyDown(KeyCode.A))
                {
                    cursorPos = 0;
                    cursorPos2 = currentChatText.Length;
                }
            }

            if (textModified)
            {
                textModified = false;
                __instance.CurrentChatText = currentChatText;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                cursorPos = 0;
                cursorPos2 = -1;
                if (curentHistory == null)
                {
                    curentHistory = chatHistory.Last;
                }
                else
                {
                    curentHistory = curentHistory.Previous;
                }
                SetChat(__instance);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                cursorPos = 0;
                cursorPos2 = -1;
                if (curentHistory == null)
                {
                    curentHistory = chatHistory.First;
                }
                else
                {
                    curentHistory = curentHistory.Next;
                }
                SetChat(__instance);
            }
        }
    }
}
