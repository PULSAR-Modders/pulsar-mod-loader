using HarmonyLib;
using PulsarModLoader.MPModChecks;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PulsarModLoader
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

        [Obsolete]
        public Dictionary<PhotonPlayer, string> PlayersWithMods = new Dictionary<PhotonPlayer, string>();

        private static Dictionary<string, ModMessage> modMessageHandlers = new Dictionary<string, ModMessage>();

        [Obsolete]
        public string GetPlayerMods(PhotonPlayer inPlayer) //if the player exists, return the modlist, otherwise return the string 'NoPlayer'
        {
            if (PlayersWithMods.ContainsKey(inPlayer))
            {
                return "NoPlayer";
            }
            else
            {
                return "NoPlayer";
            }
        }

        ModMessageHelper()
        {
            modMessageHandlers = new Dictionary<string, ModMessage>();
            IEnumerable<PulsarMod> modList = ModManager.Instance.GetAllMods();
            foreach (PulsarMod mod in modList)
            {
                Assembly asm = mod.GetType().Assembly;
                Type modMessage = typeof(ModMessage);
                foreach (Type t in asm.GetTypes())
                {
                    if (modMessage.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                    {
                        ModMessage modMessageHandler = (ModMessage)Activator.CreateInstance(t);
                        modMessageHandlers.Add(mod.HarmonyIdentifier() + "#" + modMessageHandler.GetIdentifier(), modMessageHandler);
                    }
                }
            }
            ModMessage publicCommands = new Chat.Extensions.HandlePublicCommands();
            modMessageHandlers.Add("#" + publicCommands.GetIdentifier(), publicCommands);
        }
        protected override void Awake() //gameobject startup script
        {
            base.Awake();
            Instance = this;
        }

        [Obsolete]
        public string GetModName(string modName)
        {
            PulsarMod mod = ModManager.Instance.GetMod(modName);
            return $"{mod.Name} {mod.Version} MPF{mod.MPFunctionality}";
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
        public void RecieveErrorMessage(string message)
        {
            PLNetworkManager.Instance.MainMenu.AddActiveMenu(new PLErrorMessageMenu(message));
        }

        [PunRPC]
        public void ServerRecieveModList(byte[] recievedData, PhotonMessageInfo pmi)
        {
            MPUserDataBlock userDataBlock = MPModCheckManager.DeserializeHashfullMPUserData(recievedData);
            Logger.Info($"recieved modlist from user with the following info:\nPMLVersion: {userDataBlock.PMLVersion}\nModlist:{MPModCheckManager.GetModListAsString(userDataBlock.ModData)}");
            MPModCheckManager.Instance.AddNetworkedPeerMods(pmi.sender, userDataBlock);
        }

        /*[PunRPC]
        public void ClientRecieveModListFromServer(PhotonPlayer player, byte[] recievedData, PhotonMessageInfo pmi)
        {
            MPUserDataBlock userDataBlock = MPModCheckManager.DeserializeHashfullMPUserData(recievedData);
            MPModCheckManager.Instance.AddNetworkedPeerMods(player, userDataBlock);
        }*/

        [PunRPC]
        public void ClientRecieveModList(byte[] recievedData, PhotonMessageInfo pmi)
        {
            MPUserDataBlock userDataBlock = MPModCheckManager.DeserializeHashlessMPUserData(recievedData);
            Logger.Info($"recieved modlist from user with the following info:\nPMLVersion: {userDataBlock.PMLVersion}\nModlist:{MPModCheckManager.GetModListAsString(userDataBlock.ModData)}");
            MPModCheckManager.Instance.AddNetworkedPeerMods(pmi.sender, userDataBlock);
        }

        [PunRPC]
        public void ClientRequestModList(PhotonMessageInfo pmi)
        {
            PhotonPlayer sender = pmi.sender;
            photonView.RPC("ClientRecieveModList", sender, new object[]
            {
                MPModCheckManager.Instance.SerializeHashlessUserData()
            });

            if (MPModCheckManager.Instance.GetNetworkedPeerMods(sender) == null && !MPModCheckManager.Instance.RequestedModLists.Contains(sender))
            {
                MPModCheckManager.Instance.RequestedModLists.Add(sender);
                photonView.RPC("ClientRequestModList", sender, new object[] {});
            }
        }
    }
}
