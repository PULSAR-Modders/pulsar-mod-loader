using UnityEngine;

namespace PulsarModLoader
{
    public static class PMLConfig
    {
        public static SaveValue<UnityEngine.TextAnchor> ModInfoTextAnchor =
	        new SaveValue<TextAnchor>("ModInfoTextAnchor", TextAnchor.UpperLeft);

        public static SaveValue<bool> DebugMode = new SaveValue<bool>("DebugMode", false);

        public static void SetDefault()
        {
	        ModInfoTextAnchor.Value = TextAnchor.UpperLeft;
        }
    }
}
