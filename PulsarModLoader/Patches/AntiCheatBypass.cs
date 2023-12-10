using HarmonyLib;

namespace PulsarModLoader.Patches
{
    public static class AntiCheatBypass
    {
        [HarmonyPatch(typeof(PLGameStatic), "OnInjectionCheatDetected")]
        class InjectionCheatDetectedPatch
        {
            static bool Prefix()
            {
                return false;
            }
        }
        [HarmonyPatch(typeof(PLGameStatic), "OnInjectionCheatDetected_Private")]
        public class OnInjectionCheatDetected_PrivatePatch
        {
            public static bool Prefix()
            {
                return false;
            }
        }
        [HarmonyPatch(typeof(PLGameStatic), "OnSpeedHackCheatDetected")]
        class OnSpeedHackCheatDetectedPatch
        {
            static bool Prefix()
            {
                return false;
            }
        }
        [HarmonyPatch(typeof(PLGameStatic), "OnTimeCheatDetected")]
        class OnTimeCheatDetectedPatch
        {
            static bool Prefix()
            {
                return false;
            }
        }
        [HarmonyPatch(typeof(PLGameStatic), "OnObscuredCheatDetected")]
        class OnObscuredCheatDetectedPatch
        {
            static bool Prefix()
            {
                return false;
            }
        }
    }
}
