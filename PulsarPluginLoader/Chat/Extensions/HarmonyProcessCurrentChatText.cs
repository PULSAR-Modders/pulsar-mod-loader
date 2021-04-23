using HarmonyLib;
using System.Collections.Generic;

namespace PulsarPluginLoader.Chat.Extensions
{
    [HarmonyPatch(typeof(PLNetworkManager), "ProcessCurrentChatText")]
    class HarmonyProcessCurrentChatText
    {
        private static readonly char[] newline = { '\n', '\r' };
        static void Prefix(PLNetworkManager __instance)
        {
            if (string.IsNullOrWhiteSpace(__instance.CurrentChatText))
            {
                return;
            }

            LinkedListNode<string> lastMessage = HarmonyNetworkUpdate.chatHistory.FindLast(__instance.CurrentChatText.TrimEnd(newline));
            if (lastMessage != null)
            {
                HarmonyNetworkUpdate.chatHistory.Remove(lastMessage);
            }
            HarmonyNetworkUpdate.chatHistory.AddLast(__instance.CurrentChatText.TrimEnd(newline));
            if (HarmonyNetworkUpdate.chatHistory.Count > 100)
            {
                HarmonyNetworkUpdate.chatHistory.RemoveFirst();
            }
        }
    }
}
