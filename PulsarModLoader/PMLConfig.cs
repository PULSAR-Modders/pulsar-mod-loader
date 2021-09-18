using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Converters;

namespace PulsarModLoader
{
    public class PMLConfig
    {
        internal static PMLConfig instance { get; private set; }
        internal PMLConfig(bool @default)
        {
            if (@default)
            {
                instance = this;
                ModInfoTextAnchor = TextAnchor.UpperLeft;
            }
        }

        internal PMLConfig() => instance = this;

        internal static void CreateDefaultConfig(string path, bool save)
        {
            new PMLConfig(true);
            if (save) 
                SaveConfig(path);
        }

        internal static void CreateConfigFromFile(string path) =>
            JsonConvert.DeserializeObject<PMLConfig>(File.ReadAllText(path), settings());

        internal static void SaveConfig(string path) =>
            File.WriteAllText(path, JsonConvert.SerializeObject(instance, typeof(PMLConfig), settings()));
                //.Replace("{", "{\n\t").Replace("}", "\n}").Replace(",\"", ",\n\t\""));

        private static JsonSerializerSettings settings()
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new StringEnumConverter() { });
            return settings;
        }

        public TextAnchor ModInfoTextAnchor { get; set; }
    }
}
