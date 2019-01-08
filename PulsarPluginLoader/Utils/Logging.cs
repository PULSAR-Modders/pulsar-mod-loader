using System;
using System.IO;

namespace PulsarPluginLoader.Utils
{
    public static class Logger
    {
        private static StreamWriter Stream = new StreamWriter("Log.txt", false);

        public static void Info(string message)
        {
            Console.WriteLine($"[PPL] {message}");
            Stream.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {message}");
            Stream.Flush();
        }
    }
}
