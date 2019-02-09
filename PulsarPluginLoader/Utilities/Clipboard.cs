namespace PulsarPluginLoader.Utilities
{
    public static class Clipboard
    {
        public static void Copy(string text)
        {
            NGUITools.clipboard = text;
        }

        public static string Paste()
        {
            return NGUITools.clipboard;
        }
    }
}
