using HarmonyLib;
using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PulsarPluginLoader
{
    [HarmonyPatch(typeof(PLServer), "Awake")]
    static class MMHInstantiate
    {
        static void Prefix(PLServer __instance)
        {
            __instance.gameObject.AddComponent(typeof(ModMessageHelper));
        }
    }
    public class ModMessageHelper : PLMonoBehaviour
    {
        public static ModMessageHelper Instance;
        private static readonly Dictionary<string, ModMessage> modMessageHandlers = new Dictionary<string, ModMessage>();

        ModMessageHelper()
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
        [PunRPC]
        public void ReceiveMessage(string modID, object[] arguments)
        {
            Utilities.Logger.Info($"ModMessage received message for {modID}");
            if (modMessageHandlers.TryGetValue(modID, out ModMessage modMessage))
            {
                modMessage.HandleRPC(arguments);
            }
        }
        protected override void Awake()
        {
            base.Awake();
            ModMessageHelper.Instance = this;
        }
    }
}
