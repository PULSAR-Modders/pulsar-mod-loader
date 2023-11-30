using UnityEngine;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace PulsarModLoader.Utilities
{
    public static class Clipboard
    {
        public static void Copy(string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }

        public static string Paste()
        {
            return GUIUtility.systemCopyBuffer;
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
