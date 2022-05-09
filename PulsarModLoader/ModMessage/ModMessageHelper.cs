﻿using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace PulsarModLoader
{
    [HarmonyPatch(typeof(PLServer), "Awake")]
    static class MMHInstantiate
    {
        static void Prefix(PLServer __instance)
        {
            __instance.gameObject.AddComponent(typeof(ModMessageHelper));
            Content.Dialogs.DialogsManager.Instance = new Content.Dialogs.DialogsManager();
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
            IEnumerable<PulsarMod> modList = ModManager.Instance.GetAllMods();
            foreach (PulsarMod mod in modList)
            {
                Assembly asm = mod.GetType().Assembly;
                Type modMessage = typeof(ModMessage);
                if(mod.MPFunctionality > 2)
                {
                    ServerHasMPMods = true;
                }
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
            PlayersWithMods = new Dictionary<PhotonPlayer, string>();
        }
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
        public void ReceiveConnectionMessage(string modList, string PMLVersion, PhotonMessageInfo pmi) //Pong
        {
            PhotonPlayer sender = pmi.sender;
            Utilities.Logger.Info($"ConnectionMessage received message from a sender with the following PML Version and modlist:\nPMLVersion: {PMLVersion}\nModlist:\n{modList}");            
            
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
                FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion
            });
        }
        [PunRPC]
        public void RecieveErrorMessage(string message)
        {
            PLNetworkManager.Instance.MainMenu.AddActiveMenu(new PLErrorMessageMenu(message));
        }
        [PunRPC]
        public void SendDialogCreate(int id, string dialogName, string text, string[] choices)
        {
            var dialog = Content.Dialogs.DialogsManager.Instance.CreateGenericDialog(id);
            dialog.AddText(false, text);
            dialog.SetChoices(choices);
            dialog.dialogName = dialogName;
        }
        [PunRPC]
        public void DialogClick(int id, string selectedChoice, PhotonMessageInfo pmi)
        {
            this.photonView.RPC("DialogSyncText", PhotonTargets.Others, id, true, selectedChoice);
            this.photonView.RPC("DialogSyncChoices", PhotonTargets.Others, id, new string[] { });
            DialogSyncText(id, true, selectedChoice);
            DialogSyncChoices(id, new string[] { });
            Content.Dialogs.DialogsManager.Instance.ActiveHostSideDialogs[id].OnClick(pmi.sender, selectedChoice);
        }
        [PunRPC]
        public void DialogSyncText(int id, bool right, string text) =>
            Content.Dialogs.DialogsManager.Instance.ActiveDialogs[id].AddText(right, text);
        [PunRPC]
        public void DialogSyncChoices(int id, string[] choices) =>
            Content.Dialogs.DialogsManager.Instance.ActiveDialogs[id].SetChoices(choices);
        [PunRPC]
        public void DialogDestroy(int id) =>
            Content.Dialogs.DialogsManager.Instance.DestroyDialog(id);
    }
}
