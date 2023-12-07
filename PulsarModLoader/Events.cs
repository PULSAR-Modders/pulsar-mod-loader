using HarmonyLib;
using PulsarModLoader.Chat.Extensions;
using PulsarModLoader.MPModChecks;
using PulsarModLoader.Patches;

namespace PulsarModLoader
{
    /// <summary>
    /// Contains events for commonly patched methods.
    /// </summary>
    public class Events
    {
        /// <summary>
        /// The current Events Instance.
        /// </summary>
        public static Events Instance = null;

        /// <summary>
        /// creates PulsarModLoader.Events Instance. Additionally adds PML events
        /// </summary>
        internal Events()
        {
            Instance = this;
        }

        /// <summary>
        /// used by EnterNewGameEvent
        /// </summary>
        public delegate void EnterNewGameDelegate();

        /// <summary>
        /// Postfixes PLGlobal.EnterNewGame(). Called by PLNetworkManager.ClientWaitForHubIDAndLoadLevel and PLNetworkManager.ServerWaitForHubIDAndLoadLevel.
        /// </summary>
        public event EnterNewGameDelegate EnterNewGameEvent;

        [HarmonyPatch(typeof(PLGlobal), "EnterNewGame")]
        class EnterNewGamePatch
        {
            static void Postfix()
            {
                Events.Instance.EnterNewGameEvent?.Invoke();
            }
        }

        /// <summary>
        /// Used by OnLeaveGameEvent
        /// </summary>
        public delegate void OnLeaveGameDelegate();

        /// <summary>
        /// Prefixes PLNetworkManager.OnLeaveGame(bool). Called by a variety of methods centered around intentionally and unintentionally leaving a game.
        /// </summary>
        public event OnLeaveGameDelegate OnLeaveGameEvent;

        [HarmonyPatch(typeof(PLNetworkManager), "OnLeaveGame")]
        class OnLeaveGamePatch
        {
            static void Prefix()
            {
                Events.Instance.OnLeaveGameEvent?.Invoke();
            }
        }


        /// <summary>
        /// Used By GameOverEvent
        /// </summary>
        /// <param name="backToMainMenu"></param>
        public delegate void GameOverDelegate(bool backToMainMenu);

        /// <summary>
        /// Prefixes PLNetworkManager.GameOver(Bool), Called by PLNetworkManager.OnLoeavGame, PLServer.SendGameOver, PLServer.ServerWaitForClientsToLeaveGame
        /// </summary>
        public event GameOverDelegate GameOverEvent;

        [HarmonyPatch(typeof(PLNetworkManager), "GameOver")]
        class GameOverPatch
        {
            static void Prefix(bool backToMainMenu)
            {
                Events.Instance.GameOverEvent?.Invoke(backToMainMenu);
            }
        }


        /// <summary>
        /// Used by SpawnNewPlayerEvent
        /// </summary>
        /// <param name="newPhotonPlayer"></param>
        /// <param name="inPlayerName"></param>
        public delegate void SpawnNewPlayerDelegate(PhotonPlayer newPhotonPlayer, string inPlayerName);

        /// <summary>
        /// Postfixes PLServer.SpawnNewPlayer(PhotonPlayer, String) Called By PLServer.LoginMessage. Onle called for players connecting.
        /// </summary>
        public event SpawnNewPlayerDelegate SpawnNewPlayerEvent;

        [HarmonyPatch(typeof(PLServer), "SpawnNewPlayer")]
        class SpawnNewPlayerPatch
        {
            static void Postfix(PhotonPlayer newPhotonPlayer, string inPlayerName)
            {
                //Updates PlayerList on Player Added.
                PhotonProperties.UpdatePlayerList();

                Events.Instance.SpawnNewPlayerEvent?.Invoke(newPhotonPlayer, inPlayerName);
            }
        }


        /// <summary>
        /// Used by RemovePlayerEvent
        /// </summary>
        /// <param name="player"></param>
        public delegate void RemovePlayerDelegate(PLPlayer player);

        /// <summary>
        /// Prefixes PLServer.RemovePlayer(PLPlayer) Called by PLNetworkManager.OnPhotonPlayerDisconnected, PLServer.ServerRemoveBot, PLServer.ServerRemoveCrewBotPlayer
        /// </summary>
        public event RemovePlayerDelegate RemovePlayerEvent;

        [HarmonyPatch(typeof(PLServer), "RemovePlayer")]
        class RemovePlayerPatch
        {
            static void Prefix(PLPlayer inPlayer)
            {
                //Updates PlayerList on Player Added
                PhotonProperties.UpdatePlayerList();

                Events.Instance.RemovePlayerEvent?.Invoke(inPlayer);
            }
        }


        /// <summary>
        /// Used by ServerStartEvent
        /// </summary>
        public delegate void ServerStartDelegate(PLServer instance);

        /// <summary>
        /// Postfixes PLServer.Start(). Called by Unity after creation of a server instance.
        /// </summary>
        public event ServerStartDelegate ServerStartEvent;

        [HarmonyPatch(typeof(PLServer), "Start")]
        class ServerStartPatch
        {
            static void Postfix(PLServer __instance)
            {
                //Chat Extensions
                ChatHelper.publicCached = false;
                HandlePublicCommands.RequestPublicCommands();

                Events.Instance.ServerStartEvent?.Invoke(__instance);
            }
        }


        /// <summary>
        /// Used by ClientModlistRecievedEvent
        /// </summary>
        /// <param name="DataSender"></param>
        public delegate void ClientModlistRecievedDelegate(PhotonPlayer DataSender);

        /// <summary>
        /// Called after a client modlist has been recieved by the MPModCheckManager instance.
        /// </summary>
        public event ClientModlistRecievedDelegate ClientModlistRecievedEvent;

        internal void CallClientModlistRecievedEvent(PhotonPlayer DataSender)
        {
            ClientModlistRecievedEvent?.Invoke(DataSender);
        }


        /// <summary>
        /// Used by ServerOnClientVerifiedEvent
        /// </summary>
        /// <param name="JoiningPhotonPlayer"></param>
        public delegate void ServerOnClientVerifiedDelegate(PhotonPlayer JoiningPhotonPlayer);

        /// <summary>
        /// Called after a client is succesfully verified, as a postfix to PLServer.ServerOnClientVerified(PhotonPlayer). 
        /// </summary>
        public ServerOnClientVerifiedDelegate ServerOnClientVerifiedEvent;

        [HarmonyPatch(typeof(PLServer), "ServerOnClientVerified")]
        class ServerOnClientVerifiedPatch
        {
            static void Postfix(PhotonPlayer client)
            {
                Events.Instance.ServerOnClientVerifiedEvent?.Invoke(client);
            }
        }
    }
}
