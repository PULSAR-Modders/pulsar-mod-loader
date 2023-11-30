using HarmonyLib;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace PulsarModLoader.Chat.Extensions
{
    [HarmonyPatch(typeof(PLInGameUI), "ColoredMsg")]
    public class HarmonyColoredMessage
    {
        public static string RemoveColor(string text)
        {
            while (true)
            {
                int index = text.IndexOf("<color=");
                if (index < 0)
                    break;
                int endIndex = index;
                bool valid = false;
                for (; endIndex < text.Length; endIndex++)
                {
                    if (text[endIndex] == '>')
                    {
                        valid = true;
                        break;
                    }
                }
                if (valid)
                {
                    text = text.Remove(index, endIndex - index + 1);
                }
                int index2 = text.IndexOf("</color>");
                if (index2 < 0)
                    break;
                text = text.Remove(index2, 8);
            }
            return text;
        }

        static void Prefix(ref string inMsg, bool isShadow)
        {
            if (isShadow)
            {
                inMsg = RemoveColor(inMsg);
            }
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
