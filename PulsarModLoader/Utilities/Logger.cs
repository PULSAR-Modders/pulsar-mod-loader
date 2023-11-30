using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PulsarModLoader.Utilities
{
    /// <summary>
    /// Adds lines to the player.log file
    /// </summary>
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(Directory.GetCurrentDirectory(), "PMLLog.txt");
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

        /// <summary>
        /// Adds lines to the player.log file
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            if (message == null)
            {
                Messaging.AntiNullReferenceException("message: null");
                return;
            }

            string line;
            if (PMLConfig.DebugMode)
            {
                MethodBase invoker = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
                string type = invoker.DeclaringType.ToString();
                string methodName = '.' + invoker.Name;
                if (methodName.Contains("..ctor"))
                {
                    methodName = methodName.Replace("..ctor", string.Empty);
                    type = "new " + type;
                }
                line = $"[PML-{type}{methodName}({string.Join(", ", invoker.GetParameters().Select(p => p.Name))})] {message}";
            }
            else 
                line = $"[PML] {message}";

            Console.WriteLine(line);

            if (Stream != null)
            {
                Stream.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {line}");
                Stream.Flush();
            }
        }
    }
}
