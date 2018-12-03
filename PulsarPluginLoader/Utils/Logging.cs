using System;

namespace PulsarPluginLoader.Utils
{
    public static class Logger
    {
        public static void Info(string message)
        {
            Console.WriteLine($"[PPL] {message}");
        }
    }
}
