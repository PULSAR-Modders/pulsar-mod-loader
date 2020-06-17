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
        private Dictionary<string, ModMessage> modMessageHandlers = new Dictionary<string, ModMessage>();

        public string GetPlayerMods(PhotonPlayer inPlayer)
        {
            return PlayersWithMods[inPlayer];
        }

        ModMessageHelper()
        {
            IEnumerable<PulsarPlugin> pluginList = PluginManager.Instance.GetAllPlugins();
            foreach (PulsarPlugin plugin in pluginList)
            {
                string modID = plugin.HarmonyIdentifier();
                Assembly asm = plugin.GetType().Assembly;
                Type modMessage = typeof(ModMessage);
                if(plugin.MPFunctionality > 1)
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
        }
        protected override void Awake() //gameobject startup script
        {
            base.Awake();
            ModMessageHelper.Instance = this;
            modMessageHandlers = new Dictionary<string, ModMessage>();
            PlayersWithMods = new Dictionary<PhotonPlayer, string>();
        }
        [PunRPC]
        public void ReceiveMessage(string modID, object[] arguments, PhotonMessageInfo pmi)
        {
            Utilities.Logger.Info($"ModMessage received message for {modID}");
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
        public void ReceiveConnectionMessage(string modList, PhotonMessageInfo pmi) //Pong
        {
            PhotonPlayer sender = pmi.sender;
            Utilities.Logger.Info($"ConnectionMessage received message from a sender with the following modlist:\n{modList}");            
            
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
            Logger.Info("Received Connection Message, Sending");
            ModMessageHelper.Instance.photonView.RPC("ReceiveConnectionMessage", pmi.sender, new object[]
            {
                FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion + MPModChecks.GetModList()
            });
        }
        [PunRPC]
        public void RecieveErrorMessage(string message)
        {
            PLNetworkManager.Instance.MainMenu.AddActiveMenu(new PLErrorMessageMenu(message));
        }
    }
}
