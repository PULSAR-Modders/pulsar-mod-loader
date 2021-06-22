using HarmonyLib;

namespace PulsarPluginLoader.Patches
{
    [HarmonyPatch(typeof(PLGameStatic))]
    internal static class AntiCheatBypass
    {
        [HarmonyPatch("OnInjectionCheatDetected")]
        [HarmonyPatch("OnInjectionCheatDetected_Private")]
        [HarmonyPatch("OnSpeedHackCheatDetected")]
        [HarmonyPatch("OnTimeCheatDetected")]
        [HarmonyPatch("OnObscuredCheatDetected")]
        internal static bool Prefix() => false;
    }
}
