using System;
using System.IO;
using System.Reflection;

namespace PulsarModLoader.Utilities
{
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Log.txt");
        private static StreamWriter Stream;

        static Logger()
        {
            try
            {
                Stream = new StreamWriter(LogPath);
            }
            catch (IOException)
            {
                Stream = null;
            }
        }

        public static void Info(string message)
        {
            string line = $"[PPL] {message}";

            Console.WriteLine(line);

            if (Stream != null)
            {
                Stream.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {line}");
                Stream.Flush();
            }
        }
    }
}
