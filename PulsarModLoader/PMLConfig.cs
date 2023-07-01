using System;
using UnityEngine;

namespace PulsarModLoader
{
    public static class PMLConfig
    {
        public static SaveValue<UnityEngine.TextAnchor> ModInfoTextAnchor = new SaveValue<TextAnchor>("ModInfoTextAnchor", TextAnchor.UpperLeft);

        public static SaveValue<bool> AutoPullReadme = new SaveValue<bool>("AutoPullReadme", false);

        public static SaveValue<bool> DebugMode = new SaveValue<bool>("DebugMode", false);

        public static SaveValue<bool> ZipModLoad = new SaveValue<bool>("ZipModLoad", true);
        public static SaveValue<bool> ZipModMode = new SaveValue<bool>("ZipModMode", false);

        public static SaveValue<DateTime> LastPMLUpdateCheck = new SaveValue<DateTime>("LastPMLUpdateCheck", DateTime.Today.AddDays(-2));

		public static void SetDefault()
        {
	        ModInfoTextAnchor.Value = TextAnchor.UpperLeft;
        }
    }
}
