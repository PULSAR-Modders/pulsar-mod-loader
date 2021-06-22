using HarmonyLib;
using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

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
        public static bool ServerHasMPMods = false;
        public static ModMessageHelper Instance;
        public Dictionary<PhotonPlayer, string> PlayersWithMods;
        private static Dictionary<string, ModMessage> modMessageHandlers = new Dictionary<string, ModMessage>();

        public string GetPlayerMods(PhotonPlayer inPlayer) //if the player exists, return the modlist, otherwise return the string 'NoPlayer'
        {
            if (PlayersWithMods.ContainsKey(inPlayer))
            {
                return PlayersWithMods[inPlayer];
            }
            else
            {
                return "NoPlayer";
            }
        }

        ModMessageHelper()
        {
            modMessageHandlers = new Dictionary<string, ModMessage>();
            IEnumerable<PulsarPlugin> pluginList = PluginManager.Instance.GetAllPlugins();
            foreach (PulsarPlugin plugin in pluginList)
            {
                Assembly asm = plugin.GetType().Assembly;
                Type modMessage = typeof(ModMessage);
                if(plugin.MPFunctionality > 2)
                {
                    ServerHasMPMods = true;
                }
                foreach (Type t in asm.GetTypes())
                {
                    if (modMessage.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                    {
                        ModMessage modMessageHandler = (ModMessage)Activator.CreateInstance(t);
                        modMessageHandlers.Add(plugin.HarmonyIdentifier() + "#" + modMessageHandler.GetIdentifier(), modMessageHandler);
                    }
                }
            }
            ModMessage publicCommands = new Chat.Extensions.HandlePublicCommands();
            modMessageHandlers.Add("#" + publicCommands.GetIdentifier(), publicCommands);
        }
        protected override void Awake() //gameobject startup script
        {
            base.Awake();
            ModMessageHelper.Instance = this;
            PlayersWithMods = new Dictionary<PhotonPlayer, string>();
        }
        public string GetModName(string pluginName)
        {
            PulsarPlugin plugin = PluginManager.Instance.GetPlugin(pluginName);
            return $"{plugin.Name} {plugin.Version} MPF{plugin.MPFunctionality}";
        }
        [PunRPC]
        public void ReceiveMessage(string modID, object[] arguments, PhotonMessageInfo pmi)
        {
            //Utilities.Logger.Info($"ModMessage received message for {modID}");
            if (modMessageHandlers.TryGetValue(modID, out ModMessage modMessage))
            {
                modMessage.HandleRPC(arguments, pmi);
            }
            else
            {
                Utilities.Logger.Info($"ModMessage for {modID} doesn't exist");
            }
        }
        [PunRPC]
        public void ReceiveConnectionMessage(string modList, string PPLVersion, PhotonMessageInfo pmi) //Pong
        {
            PhotonPlayer sender = pmi.sender;
            Utilities.Logger.Info($"ConnectionMessage received message from a sender with the following PPL Version and modlist:\nPPLVersion: {PPLVersion}\nModlist:\n{modList}");            
            
            if (!PlayersWithMods.ContainsKey(sender))
            {
                PlayersWithMods.Add(sender, modList);
                Utilities.Logger.Info("Added Sender to PlayersWithMods list");
            }
            else
            {
                Utilities.Logger.Info("Couldn't find sender");
            }
        }
        [PunRPC]
        public void SendConnectionMessage(PhotonMessageInfo pmi) //Ping
        {
            Logger.Info("Received ping, Sending pong");
            ModMessageHelper.Instance.photonView.RPC("ReceiveConnectionMessage", pmi.sender, new object[]
            {
                MPModChecks.GetModList(),
                PluginManager.VERSION
            });
        }
        [PunRPC]
        public void RecieveErrorMessage(string message)
        {
            PLNetworkManager.Instance.MainMenu.AddActiveMenu(new PLErrorMessageMenu(message));
        }
    }
}
