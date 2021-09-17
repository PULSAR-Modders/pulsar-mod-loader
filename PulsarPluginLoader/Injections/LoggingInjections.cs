using UnityEngine;

namespace PulsarModLoader.Injections
{
    public static class LoggingInjections
    {
        public static void LoggingCleanup()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        }
    }
}
