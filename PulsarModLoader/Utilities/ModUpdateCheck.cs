using PulsarModLoader.Chat.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Valve.Newtonsoft.Json;

namespace PulsarModLoader.Utilities
{
	internal static class ModUpdateCheck
	{
		internal static bool IsUpdateAviable(PulsarMod mod)
		{
#if DEBUG
			return false; // disabled for debug builds
#endif
			if (PMLConfig.DebugMode.Value) // disabled for debug mode
				return false;

			if (string.IsNullOrEmpty(mod.VersionLink)) // disabled for mods without link
				return false;

			using (var web = new System.Net.WebClient())
			{
				web.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.71 Safari/537.36");
				var file = JsonConvert.DeserializeObject<VersionFile>(web.DownloadString(mod.VersionLink));
				var result = CompareVersions(mod.Version, file.Version);

				if (result)
					ModManager.Instance.UpdatesAviable.Add(new UpdateModInfo() { Mod = mod, Data = file });

				return result;
			}
		}

		private static bool CompareVersions(string current, string fromServer)
		{
			var currentParsed = current.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
			var fromServerParsed = fromServer.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();

			for(int i = 0; i < currentParsed.Length; i++)
			{
				if (currentParsed[i] > fromServerParsed[i]) // return false if "1.4" vs "1.3"
					return false;

				if (currentParsed[i] < fromServerParsed[i]) // return true if "1.4" vs "1.5"
					return true;
			}

			if (fromServerParsed.Length > currentParsed.Length) // return true if CompareVersions("1.4", "1.4.1")
				return true;

			return false; // if "1.4" vs "1.4"
		}

		internal static void UpdateMod(UpdateModInfo info)
		{
			//info.Mod.Unload();
			var path = info.Mod.VersionInfo.FileName;
			using (var web = new System.Net.WebClient())
			{
				web.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.71 Safari/537.36");
				var dll = web.DownloadData(info.Data.DownloadLink);
				File.WriteAllBytes(path, dll);
				//ModManager.Instance.LoadMod(path);
			}
			info.IsUpdated = true;
		}

		internal struct VersionFile
		{
			public string Version;
			public string DownloadLink;
		}

		internal class UpdateModInfo
		{
			public PulsarMod Mod;
			public VersionFile Data;
			public bool IsUpdated = false;
		}
	}
}
