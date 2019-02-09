using System;
using System.Collections.Generic;
using System.Reflection;

namespace PulsarPluginLoader
{
    public class ModMessageHelper
    {
        private static readonly Dictionary<string, ModMessage> modMessageHandlers = new Dictionary<string, ModMessage>();

        static ModMessageHelper()
        {
            IEnumerable<PulsarPlugin> pluginList = PluginManager.Instance.GetAllPlugins();
            foreach (PulsarPlugin plugin in pluginList)
            {
                string modID = plugin.HarmonyIdentifier();
                Assembly asm = plugin.GetType().Assembly;
                Type modMessage = typeof(ModMessage);
                foreach (Type t in asm.GetTypes())
                {
                    if (modMessage.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                    {
                        ModMessage modMessageHandler = (ModMessage)Activator.CreateInstance(t);
                        modMessageHandlers.Add(plugin.HarmonyIdentifier() + "#" + modMessageHandler.GetIdentifier(), modMessageHandler);
                    }
                }
            }
        }

        public static void ReceiveMessage(string modID, object[] arguments)
        {
            if (modMessageHandlers.TryGetValue(modID, out ModMessage modMessage))
            {
                modMessage.HandleRPC(arguments);
            }
        }
    }
}
