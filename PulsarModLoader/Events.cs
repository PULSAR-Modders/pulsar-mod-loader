using HarmonyLib;
using PulsarModLoader.Chat.Extensions;
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
        public Events()
        {
            Instance = this;
        }

        /// <summary>
        /// Called by Event EnterNewGame
        /// </summary>
        public delegate void EnterNewGameEvent();

        /// <summary>
        /// Postfixes PLGlobal.EnterNewGame(). Called by PLNetworkManager.ClientWaitForHubIDAndLoadLevel and PLNetworkManager.ServerWaitForHubIDAndLoadLevel.
        /// </summary>
        public event EnterNewGameEvent EnterNewGame;

        [HarmonyPatch(typeof(PLGlobal), "EnterNewGame")]
        class EnterNewGamePatch
        {
            static void Postfix()
            {
                Events.Instance.EnterNewGame?.Invoke();
            }
        }

        /// <summary>
        /// Called by Event OnLeaveGame
        /// </summary>
        public delegate void OnLeaveGameEvent();

        /// <summary>
        /// Prefixes PLNetworkManager.OnLeaveGame(bool). Called by a variety of methods centered around intentionally and unintentionally leaving a game.
        /// </summary>
        public event OnLeaveGameEvent OnLeaveGame;

        [HarmonyPatch(typeof(PLNetworkManager), "OnLeaveGame")]
        class OnLeaveGamePatch
        {
            static void Prefix()
            {
                Events.Instance.OnLeaveGame?.Invoke();
            }
        }


        /// <summary>
        /// Called By Event GameOver
        /// </summary>
        /// <param name="backToMainMenu"></param>
        public delegate void GameOverEvent(bool backToMainMenu);

        /// <summary>
        /// Prefixes PLNetworkManager.GameOver(Bool), Called by PLNetworkManager.OnLoeavGame, PLServer.SendGameOver, PLServer.ServerWaitForClientsToLeaveGame
        /// </summary>
        public event GameOverEvent GameOver;

        [HarmonyPatch(typeof(PLNetworkManager), "GameOver")]
        class GameOverPatch
        {
            static void Prefix(bool backToMainMenu)
            {
                Events.Instance.GameOver?.Invoke(backToMainMenu);
            }
        }


        /// <summary>
        /// Called by Event SpawnNewPlayer
        /// </summary>
        /// <param name="newPhotonPlayer"></param>
        /// <param name="inPlayerName"></param>
        public delegate void SpawnNewPlayerEvent(PhotonPlayer newPhotonPlayer, string inPlayerName);

        /// <summary>
        /// Postfixes PLServer.SpawnNewPlayer(PhotonPlayer, String) Called By PLServer.LoginMessage. Onle called for players connecting.
        /// </summary>
        public event SpawnNewPlayerEvent SpawnNewPlayer;

        [HarmonyPatch(typeof(PLServer), "SpawnNewPlayer")]
        class SpawnNewPlayerPatch
        {
            static void Postfix(PhotonPlayer newPhotonPlayer, string inPlayerName)
            {
                //Updates PlayerList on Player Added.
                PhotonProperties.UpdatePlayerList();

                Events.Instance.SpawnNewPlayer?.Invoke(newPhotonPlayer, inPlayerName);
            }
        }


        /// <summary>
        /// Called by Event RemovePlayer
        /// </summary>
        /// <param name="player"></param>
        public delegate void RemovePlayerEvent(PLPlayer player);

        /// <summary>
        /// Prefixes PLServer.RemovePlayer(PLPlayer) Called by PLNetworkManager.OnPhotonPlayerDisconnected, PLServer.ServerRemoveBot, PLServer.ServerRemoveCrewBotPlayer
        /// </summary>
        public event RemovePlayerEvent RemovePlayer;

        [HarmonyPatch(typeof(PLServer), "RemovePlayer")]
        class RemovePlayerPatch
        {
            static void Prefix(PLPlayer inPlayer)
            {
                //Updates PlayerList on Player Added
                PhotonProperties.UpdatePlayerList();

                Events.Instance.RemovePlayer?.Invoke(inPlayer);
            }
        }


        /// <summary>
        /// Called by Event ServerStart
        /// </summary>
        public delegate void ServerStartEvent(PLServer instance);

        /// <summary>
        /// Postfixes PLServer.Start(). Called by Unity after creation of a server instance.
        /// </summary>
        public event ServerStartEvent ServerStart;

        [HarmonyPatch(typeof(PLServer), "Start")]
        class ServerStartPatch
        {
            static void Postfix(PLServer __instance)
            {
                //Chat Extensions
                ChatHelper.publicCached = false;
                HandlePublicCommands.RequestPublicCommands();

                Events.Instance.ServerStart?.Invoke(__instance);
            }
        }
    }
}
