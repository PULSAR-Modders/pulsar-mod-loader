
namespace PulsarPluginLoader.Injections
{
    public static class AntiCheatBypass
    {
        public static void Inject(string targetAssemblyPath)
        {
            InjectionTools.ShortCircuitMethod(targetAssemblyPath, "PLGameStatic", "OnInjectionCheatDetected");
            InjectionTools.ShortCircuitMethod(targetAssemblyPath, "PLGameStatic", "OnInjectionCheatDetected_Private");
            InjectionTools.ShortCircuitMethod(targetAssemblyPath, "PLGameStatic", "OnSpeedHackCheatDetected");
            InjectionTools.ShortCircuitMethod(targetAssemblyPath, "PLGameStatic", "OnTimeCheatDetected");
            InjectionTools.ShortCircuitMethod(targetAssemblyPath, "PLGameStatic", "OnObscuredCheatDetected");
        }
    }
}
