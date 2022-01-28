using HarmonyLib;
using PulsarModLoader.Chat.Commands;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace PulsarModLoader.Patches
{
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(PLCachedFormatString<int,string,string>), "ToString", new[] { typeof(int), typeof(string), typeof(string) })]
    class GameVersion
    {
        static readonly string PMLVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

        static void Prefix(ref string Obj3)
        {
            if (Obj3.Contains("v"))
                Obj3 += $"\nPML {PMLVersion}";
        }
    }
}
