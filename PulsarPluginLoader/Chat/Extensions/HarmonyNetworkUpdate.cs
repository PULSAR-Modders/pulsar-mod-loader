using HarmonyLib;
using PulsarPluginLoader.Chat.Commands;
using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PulsarPluginLoader.Chat.Extensions
{
    [HarmonyPatch(typeof(PLNetworkManager), "Update")]
    class HarmonyNetworkUpdate
    {
        public static LinkedList<string> chatHistory = new LinkedList<string>();

        private static LinkedListNode<string> curentHistory = null;

        private static string currentChatText;

        private static bool textModified = false;

        private static long lastTimePaste = long.MaxValue;
        private static long lastTimeDelete = long.MaxValue;

        public static bool publicCached = false;
        public static string[] publicCommands = null;

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
            if (HarmonyHandleChat.cursorPos < HarmonyHandleChat.cursorPos2)
            {
                pos = currentChatText.Length - HarmonyHandleChat.cursorPos2;
                length = HarmonyHandleChat.cursorPos2 - HarmonyHandleChat.cursorPos;
            }
            else
            {
                pos = currentChatText.Length - HarmonyHandleChat.cursorPos;
                length = HarmonyHandleChat.cursorPos - HarmonyHandleChat.cursorPos2;
                HarmonyHandleChat.cursorPos = HarmonyHandleChat.cursorPos2;
            }
            HarmonyHandleChat.cursorPos2 = -1;
            currentChatText = currentChatText.Remove(pos, length);
        }

        static void Prefix(PLNetworkManager __instance)
        {
            currentChatText = __instance.CurrentChatText;
            if (__instance.IsTyping && (HarmonyHandleChat.cursorPos > 0 || HarmonyHandleChat.cursorPos2 > 0))
            {
                foreach (char c in Input.inputString)
                {
                    if (c == "\b"[0])
                    {
                        if (HarmonyHandleChat.cursorPos2 != -1 && HarmonyHandleChat.cursorPos2 != HarmonyHandleChat.cursorPos)
                        {
                            DeleteSelected();
                        }
                        else
                        {
                            if (HarmonyHandleChat.cursorPos != currentChatText.Length)
                            {
                                currentChatText = currentChatText.Remove(currentChatText.Length - HarmonyHandleChat.cursorPos - 1, 1);
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
                        if (HarmonyHandleChat.cursorPos2 != -1 && HarmonyHandleChat.cursorPos2 != HarmonyHandleChat.cursorPos)
                        {
                            DeleteSelected();
                        }
                        currentChatText = currentChatText.Insert(currentChatText.Length - HarmonyHandleChat.cursorPos, c.ToString());
                        textModified = true;
                    }
                }
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    lastTimeDelete = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + /*(SystemInformation.KeyboardDelay + 1) **/ 250;
                    if (HarmonyHandleChat.cursorPos2 != -1 && HarmonyHandleChat.cursorPos2 != HarmonyHandleChat.cursorPos)
                    {
                        DeleteSelected();
                    }
                    else
                    {
                        currentChatText = currentChatText.Remove(currentChatText.Length - HarmonyHandleChat.cursorPos, 1);
                        HarmonyHandleChat.cursorPos--;
                    }
                    textModified = true;
                }
                if (Input.GetKey(KeyCode.Delete) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimeDelete)
                {
                    lastTimeDelete += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                    currentChatText = currentChatText.Remove(currentChatText.Length - HarmonyHandleChat.cursorPos, 1);
                    HarmonyHandleChat.cursorPos--;
                    textModified = true;
                }
            }
            if (__instance.IsTyping)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    if (currentChatText.StartsWith("/"))
                    {
                        string chatText = AutoComplete.Complete(currentChatText, HarmonyHandleChat.cursorPos);
                        if (chatText != currentChatText)
                        {
                            textModified = true;
                            currentChatText = chatText;
                            HarmonyHandleChat.cursorPos2 = -1;
                        }
                    }
                    else if (currentChatText.StartsWith("!"))
                    {
                        if (publicCached)
                        {
                            string chatText = AutoComplete.Complete(currentChatText, HarmonyHandleChat.cursorPos);
                            if (chatText != currentChatText)
                            {
                                textModified = true;
                                currentChatText = chatText;
                                HarmonyHandleChat.cursorPos2 = -1;
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
                    if (HarmonyHandleChat.cursorPos2 != -1 && HarmonyHandleChat.cursorPos2 != HarmonyHandleChat.cursorPos)
                    {
                        DeleteSelected();
                    }
                    currentChatText = currentChatText.Insert(currentChatText.Length - HarmonyHandleChat.cursorPos, GUIUtility.systemCopyBuffer);
                    textModified = true;
                }
                if (Input.GetKey(KeyCode.V) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimePaste)
                {
                    lastTimePaste += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                    currentChatText = currentChatText.Insert(currentChatText.Length - HarmonyHandleChat.cursorPos, GUIUtility.systemCopyBuffer);
                    textModified = true;
                }
                if (Input.GetKeyDown(KeyCode.C) && HarmonyHandleChat.cursorPos2 != -1 && HarmonyHandleChat.cursorPos2 != HarmonyHandleChat.cursorPos)
                {
                    int pos;
                    int length;
                    if (HarmonyHandleChat.cursorPos < HarmonyHandleChat.cursorPos2)
                    {
                        pos = currentChatText.Length - HarmonyHandleChat.cursorPos2;
                        length = HarmonyHandleChat.cursorPos2 - HarmonyHandleChat.cursorPos;
                    }
                    else
                    {
                        pos = currentChatText.Length - HarmonyHandleChat.cursorPos;
                        length = HarmonyHandleChat.cursorPos - HarmonyHandleChat.cursorPos2;
                    }
                    GUIUtility.systemCopyBuffer = currentChatText.Substring(pos, length);
                }
                if (Input.GetKeyDown(KeyCode.X) && HarmonyHandleChat.cursorPos2 != -1 && HarmonyHandleChat.cursorPos2 != HarmonyHandleChat.cursorPos)
                {
                    int pos;
                    int length;
                    if (HarmonyHandleChat.cursorPos < HarmonyHandleChat.cursorPos2)
                    {
                        pos = currentChatText.Length - HarmonyHandleChat.cursorPos2;
                        length = HarmonyHandleChat.cursorPos2 - HarmonyHandleChat.cursorPos;
                    }
                    else
                    {
                        pos = currentChatText.Length - HarmonyHandleChat.cursorPos;
                        length = HarmonyHandleChat.cursorPos - HarmonyHandleChat.cursorPos2;
                    }
                    GUIUtility.systemCopyBuffer = currentChatText.Substring(pos, length);
                    DeleteSelected();
                    textModified = true;
                }
                if (Input.GetKeyDown(KeyCode.A))
                {
                    HarmonyHandleChat.cursorPos = 0;
                    HarmonyHandleChat.cursorPos2 = currentChatText.Length;
                }
            }

            if (textModified)
            {
                textModified = false;
                __instance.CurrentChatText = currentChatText;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                HarmonyHandleChat.cursorPos = 0;
                HarmonyHandleChat.cursorPos2 = -1;
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
                HarmonyHandleChat.cursorPos = 0;
                HarmonyHandleChat.cursorPos2 = -1;
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
