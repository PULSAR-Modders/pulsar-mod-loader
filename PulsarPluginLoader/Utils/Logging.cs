using System;
using System.IO;
using System.Reflection;

namespace PulsarPluginLoader.Utils
{
    public static class Logger
    {
        private static string logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Log.txt");
        private static StreamWriter Stream = new StreamWriter(logPath, false);

        public static void Info(string message)
        {
            Console.WriteLine($"[PPL] {message}");
            Stream.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {message}");
            Stream.Flush();
        }
    }
}
