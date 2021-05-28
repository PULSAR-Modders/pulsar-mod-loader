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
            __instance.CurrentChatText = __instance.CurrentChatText.TrimEnd(newline);

            if (string.IsNullOrWhiteSpace(__instance.CurrentChatText))
            {
                return;
            }

            LinkedListNode<string> lastMessage = ChatHelper.chatHistory.FindLast(__instance.CurrentChatText);
            if (lastMessage != null)
            {
                ChatHelper.chatHistory.Remove(lastMessage);
            }
            ChatHelper.chatHistory.AddLast(__instance.CurrentChatText);
            if (ChatHelper.chatHistory.Count > 100)
            {
                ChatHelper.chatHistory.RemoveFirst();
            }
        }
    }
}
