using HarmonyLib;
using System;
using UnityEngine;

namespace PulsarPluginLoader.Chat.Extensions
{
    [HarmonyPatch(typeof(PLInGameUI), "HandleChat")]
    class HarmonyHandleChat
    {
        public static int cursorPos = 0;
        public static int cursorPos2 = -1;

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
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        lastTimeLeft = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + /*(SystemInformation.KeyboardDelay + 1) **/ 250;
                        if (cursorPos < __state.Length)
                        {
                            cursorPos++;
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        lastTimeRight = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + /*(SystemInformation.KeyboardDelay + 1) **/ 250;
                        if (cursorPos > 0)
                        {
                            cursorPos--;
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.Home))
                    {
                        cursorPos = __state.Length;
                    }
                    if (Input.GetKeyDown(KeyCode.End))
                    {
                        cursorPos = 0;
                    }
                    if (Input.GetKey(KeyCode.LeftArrow) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimeLeft)
                    {
                        lastTimeLeft += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                        if (cursorPos < __state.Length)
                        {
                            cursorPos++;
                        }
                    }
                    if (Input.GetKey(KeyCode.RightArrow) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimeRight)
                    {
                        lastTimeRight += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                        if (cursorPos > 0)
                        {
                            cursorPos--;
                        }
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        cursorPos2 = -1;
                        lastTimeLeft = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + 500 /*(SystemInformation.KeyboardDelay + 1) * 250*/;
                        if (cursorPos < __state.Length)
                        {
                            cursorPos++;
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        cursorPos2 = -1;
                        lastTimeRight = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + 500 /*(SystemInformation.KeyboardDelay + 1) * 250*/;
                        if (cursorPos > 0)
                        {
                            cursorPos--;
                        }
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
                    if (Input.GetKey(KeyCode.LeftArrow) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimeLeft)
                    {
                        cursorPos2 = -1;
                        lastTimeLeft += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                        if (cursorPos < __state.Length)
                        {
                            cursorPos++;
                        }
                    }
                    if (Input.GetKey(KeyCode.RightArrow) && DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond > lastTimeRight)
                    {
                        cursorPos2 = -1;
                        lastTimeRight += 30 /*(long)(1 / ((SystemInformation.KeyboardSpeed + 1) * 0.859375))*/;
                        if (cursorPos > 0)
                        {
                            cursorPos--;
                        }
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
                    networkManager.CurrentChatText = networkManager.CurrentChatText.Insert(__state.Length - cursorPos, DateTime.Now.Millisecond >= 500 ? "|" : " ");
                    if (cursorPos2 != -1 && cursorPos2 != cursorPos)
                    {
                        networkManager.CurrentChatText = networkManager.CurrentChatText.Insert(__state.Length - cursorPos2 + (cursorPos > cursorPos2 ? 1 : 0), DateTime.Now.Millisecond >= 500 ? "¦" : " ");
                    }
                }
            }
            else
            {
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
    }
}
