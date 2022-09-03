using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Converters;

namespace PulsarModLoader
{
    public static class PMLConfig
    {
        public static SaveValue<UnityEngine.TextAnchor> ModInfoTextAnchor =
	        new SaveValue<TextAnchor>("ModInfoTextAnchor", TextAnchor.UpperLeft);

        public static void SetDefault()
        {
	        ModInfoTextAnchor.Value = TextAnchor.UpperLeft;
        }
    }
}
