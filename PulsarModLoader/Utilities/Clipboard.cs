using UnityEngine;

namespace PulsarModLoader.Utilities
{
    /// <summary>
    /// Acesses clipboard copy and paste
    /// </summary>
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
