using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;

namespace PulsarModLoader
{
	[HarmonyPatch]
    internal class SaveValueManager
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(PLGlobal), "OnApplicationQuit")]
		private static void ForceSaveAllConfigsOnApplicationQuit()
		{
			PulsarModLoader.Utilities.Logger.Info("OnApplicationQuit");
			foreach(object saveValue in AllSaveValues)
			{
				var type = saveValue.GetType();
				var id = (string)AccessTools.Field(type, "id").GetValue(saveValue);
				var mod = (Assembly)AccessTools.Field(type, "mod").GetValue(saveValue);
				var value = AccessTools.Field(type, "_value").GetValue(saveValue);
				ModToCacheValues[mod][id] = JToken.FromObject(value);
			}

			foreach(KeyValuePair<Assembly, JObject> keyValuePair in ModToCacheValues)
			{
				Assembly mod = keyValuePair.Key;
				var cfg = GetConfigFile(mod);
				File.WriteAllText(cfg, JsonConvert.SerializeObject(ModToCacheValues[mod], serializerSettings));
			}
		}

		public static string GetConfigFolder()
		{
			string ModConfigDir = Path.Combine(Directory.GetCurrentDirectory(), "ModConfigs");
            if (!Directory.Exists(ModConfigDir))
            {
                Directory.CreateDirectory(ModConfigDir);
            }
			return ModConfigDir;
        }

		private static JsonSerializerSettings serializerSettings;
		static SaveValueManager()
		{
			serializerSettings = new JsonSerializerSettings();
			serializerSettings.Formatting = Formatting.Indented;
		}

		internal static T GetValueFor<T>(SaveValue<T> saveValue, T @default)
		{
			AllSaveValues.Add(saveValue);

			var cfg = GetConfigFile(saveValue.mod); // Trying to open a config (Read | create a new one | take it from the cache)

			if (ModToCacheValues.TryGetValue(saveValue.mod, out JObject values)) // try to access the processed configuration
			{
				if (values.TryGetValue(saveValue.id, out JToken value)) // If it contains our field
					return value.ToObject<T>(); // then parse and return
												// otherwise add a default value to values and return it
			}
			else // If there is no such thing
			{
				ModToCacheValues.Add(saveValue.mod, new JObject()); // create an empty config
			}

			ModToCacheValues[saveValue.mod].Add(saveValue.id, JToken.FromObject(@default)); // add the missing value
			File.WriteAllText(cfg, JsonConvert.SerializeObject(ModToCacheValues[saveValue.mod], serializerSettings)); // save file
			return @default; // return default
		}

		private static string GetConfigFile(Assembly mod)
		{
			if (ModToConfigFile.ContainsKey(mod)) return ModToConfigFile[mod];
			string newFile = Path.Combine(GetConfigFolder(), mod.GetName().Name + ".json");
			ModToConfigFile.Add(mod, newFile);
			if (!ModToCacheValues.ContainsKey(mod) && File.Exists(newFile))
				ModToCacheValues.Add(mod, JObject.Parse(File.ReadAllText(newFile)));
			return newFile;
		}

		private static Dictionary<Assembly, string> ModToConfigFile = new Dictionary<Assembly, string>();

		private static Dictionary<Assembly, JObject> ModToCacheValues =
			new Dictionary<Assembly, JObject>();

		private static List<object> AllSaveValues = new List<object>();
	}

	public class SaveValue<T> : IEquatable<T>
	{
		public SaveValue(string id, T @default)
		{
			this.id = id;
			this.mod = Assembly.GetCallingAssembly();
			_value = SaveValueManager.GetValueFor<T>(this, @default);
		}

		internal string id;
		internal Assembly mod;

		internal T _value;

		public T Value
		{
			get => _value;
			set
			{
				_value = value;
			}
		}

		public static implicit operator T(SaveValue<T> v) => v._value;

		public override bool Equals(object obj) => _value.Equals(obj);

		public bool Equals(T other) => _value.Equals(other);

		public override string ToString() => _value.ToString();

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }

	// Example Usage
	//public static class MyCfg {
	//	public static SaveValue<string> CoolName = new SaveValue<string>("CoolName", "Default Nickname");
	//	public static SaveValue<int> Offset = new SaveValue<int>("Offset", 10);
	//}
}
