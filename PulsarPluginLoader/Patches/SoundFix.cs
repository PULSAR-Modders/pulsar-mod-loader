using HarmonyLib;
using System;
using UnityEngine;

namespace PulsarPluginLoader.Patches
{
    [HarmonyPatch(typeof(PLMusic), "PostEvent", new Type[] { typeof(string), typeof(GameObject) })]
    class SoundFix
    {
        static bool Prefix()
        {
            return Application.isFocused;
        }
    }
}
