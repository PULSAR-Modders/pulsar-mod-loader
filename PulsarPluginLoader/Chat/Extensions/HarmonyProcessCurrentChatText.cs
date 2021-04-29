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

            LinkedListNode<string> lastMessage = ChatHelper.chatHistory.FindLast(__instance.CurrentChatText.TrimEnd(newline));
            if (lastMessage != null)
            {
                ChatHelper.chatHistory.Remove(lastMessage);
            }
            ChatHelper.chatHistory.AddLast(__instance.CurrentChatText.TrimEnd(newline));
            if (ChatHelper.chatHistory.Count > 100)
            {
                ChatHelper.chatHistory.RemoveFirst();
            }
        }
    }
}
