using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Valve.Newtonsoft.Json;

namespace PulsarModLoader
{
    internal class SaveValueManager
	{
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

		internal static void SaveValueFor(string id, Assembly mod, string value)
		{
			ModToCacheValues[mod][id] = value;
			var cfg = GetConfigFile(mod);
			File.WriteAllText(cfg, JsonConvert.SerializeObject(ModToCacheValues[mod], serializerSettings));
		}

		internal static T GetValueFor<T>(string id, Assembly mod, T @default)
		{
			var cfg = GetConfigFile(mod);
			if (ModToCacheValues.TryGetValue(mod, out var values))
			{
				if (values.TryGetValue(id, out var ret))
				{
					if (@default is Enum)
						return (T)Enum.Parse(typeof(T), ret);

					return (T)AccessTools.Method(@default.GetType(), "Parse", new Type[] { typeof(string) })
						.Invoke(null, new[] { ret });
				}
			}
			else
			{
				ModToCacheValues.Add(mod, new Dictionary<string, string>());
				ModToCacheValues[mod].Add(id, @default.ToString());
				File.WriteAllText(cfg, JsonConvert.SerializeObject(ModToCacheValues[mod], serializerSettings));
				return @default;
			}

			ModToCacheValues[mod].Add(id, @default.ToString());
			return @default;
		}

		private static string GetConfigFile(Assembly mod)
		{
			if (ModToConfigFile.ContainsKey(mod)) return ModToConfigFile[mod];
			string newFile = Path.Combine(GetConfigFolder(), mod.GetName().Name + ".json");
			ModToConfigFile.Add(mod, newFile);
			if (!ModToCacheValues.ContainsKey(mod) && File.Exists(newFile))
				ModToCacheValues.Add(mod, JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(newFile)));
			return newFile;
		}

		private static Dictionary<Assembly, string> ModToConfigFile = new Dictionary<Assembly, string>();

		private static Dictionary<Assembly, Dictionary<string, string>> ModToCacheValues =
			new Dictionary<Assembly, Dictionary<string, string>>();
	}

	public class SaveValue<T> : IEquatable<T>
	{
		public SaveValue(string id, T @defualt)
		{
			this.id = id;
			this.mod = Assembly.GetCallingAssembly();
			_value = SaveValueManager.GetValueFor(id, mod, @defualt);
		}

		private string id;
		private Assembly mod;

		private T _value;

		public T Value
		{
			get => _value;
			set
			{
				if (!_value.Equals(value)) SetValue(value);
			}
		}

		private void SetValue(T newValue)
		{
			_value = newValue;
			SaveValueManager.SaveValueFor(id, mod, newValue.ToString());
		}

		public static implicit operator T(SaveValue<T> v) => v._value;

		public override bool Equals(object obj) => _value.Equals(obj);

		public bool Equals(T other) => _value.Equals(other);

		public override string ToString() => _value.ToString();
	}

	// Example Usage
	//public static class MyCfg {
	//	public static SaveValue<string> CoolName = new SaveValue<string>("CoolName", "Default Nickname");
	//	public static SaveValue<int> Offset = new SaveValue<int>("Offset", 10);
	//}
}
