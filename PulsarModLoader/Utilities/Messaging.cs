using System.Linq;
using UnityEngine;

namespace PulsarModLoader.Utilities
{
    /// <summary>
    /// Handles messages for local and other clients.
    /// </summary>
    public static class Messaging
    {
        /// <summary>
        /// Inserts a line to text chat based on input params with sending player prefix
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="message"></param>
        /// <param name="sendingPlayerId"></param>
        public static void ChatMessage(PLPlayer recipient, string message, int sendingPlayerId = -1)
        {
            if (recipient == null || message == null)
            {
                AntiNullReferenceException($"{(recipient == null ? "recipent: null" : "recipent: PLPlayer")}, {(message == null ? "message: null" : $"message: \"{message}\"")}, sendingPlayerId: {sendingPlayerId}");
                return;
            }
            if (sendingPlayerId == -1)
            {
                sendingPlayerId = PLNetworkManager.Instance.LocalPlayerID;
            }
            ChatMessage(recipient.GetPhotonPlayer(), message, sendingPlayerId);
        }

        /// <summary>
        /// Inserts a line to text chat based on input params with sending player prefix
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="message"></param>
        /// <param name="sendingPlayerId"></param>
        public static void ChatMessage(PhotonPlayer recipient, string message, int sendingPlayerId = -1)
        {
            if (recipient == null || message == null)
            {
                AntiNullReferenceException($"{(recipient == null ? "recipent: null" : "recipent: PLPlayer")}, {(message == null ? "message: null" : $"message: \"{message}\"")}, sendingPlayerId: {sendingPlayerId}");
                return;
            }

            if (sendingPlayerId == -1)
            {
                sendingPlayerId = PLNetworkManager.Instance.LocalPlayerID;
            }
            PLServer.Instance.photonView.RPC("TeamMessage", recipient, new object[] {
                message,
                sendingPlayerId
            });
        }

        /// <summary>
        /// Inserts a line to text chat based on input params with sending player prefix
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="message"></param>
        /// <param name="sendingPlayerId"></param>
        public static void ChatMessage(PhotonTargets targets, string message, int sendingPlayerId = -1)
        {
            if (message == null)
            {
                AntiNullReferenceException($"targets: {targets}, {(message == null ? "message: null" : $"message: \"{message}\"")}, sendingPlayerId: {sendingPlayerId}");
                return;
            }

            if (sendingPlayerId == -1)
            {
                sendingPlayerId = PLNetworkManager.Instance.LocalPlayerID;
            }
            PLServer.Instance.photonView.RPC("TeamMessage", targets, new object[] {
                message,
                sendingPlayerId
            });
        }

        /// <summary>
        /// Inserts a line to text chat based on input params
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="message"></param>
        public static void Echo(PLPlayer recipient, string message)
        {
            if (recipient == null || message == null)
            {
                AntiNullReferenceException($"{(recipient == null ? "recipent: null" : "recipent: PLPlayer")}, {(message == null ? "message: null" : $"message: \"{message}\"")}");
                return;
            }

            Echo(recipient.GetPhotonPlayer(), message);
        }

        /// <summary>
        /// Inserts a line to text chat based on input params
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="message"></param>
        public static void Echo(PhotonPlayer recipient, string message)
        {
            if (recipient == null || message == null)
            {
                AntiNullReferenceException($"{(recipient == null ? "recipent: null" : "recipent: PhotonPlayer")}, {(message == null ? "message: null" : $"message: \"{message}\"")}");
                return;
            }

            PLServer.Instance.photonView.RPC("ConsoleMessage", recipient, new object[] {
                message
            });
        }

        /// <summary>
        /// Inserts a line to text chat based on input params
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="message"></param>
        public static void Echo(PhotonTargets targets, string message)
        {
            if (message == null)
            {
                AntiNullReferenceException($"targets: {targets}, {(message == null ? "message: null" : $"message: \"{message}\"")}");
                return;
            }

            PLServer.Instance.photonView.RPC("ConsoleMessage", targets, new object[] {
                message
            });
        }

        /// <summary>
        /// Adds a Notification to the left side of the screen based on input params
        /// </summary>
        /// <param name="message"></param>
        /// <param name="recipient"></param>
        /// <param name="subjectPlayerId"></param>
        /// <param name="durationMs"></param>
        /// <param name="addToShipLog"></param>
        public static void Notification(string message, PLPlayer recipient = null, int subjectPlayerId = 0, int durationMs = 6000, bool addToShipLog = false)
        {
            if (recipient == null && PLNetworkManager.Instance != null)
            {
                recipient = PLNetworkManager.Instance.LocalPlayer;
            }
            Notification(message, recipient.GetPhotonPlayer(), subjectPlayerId, durationMs, addToShipLog);
        }

        /// <summary>
        /// Adds a Notification to the left side of the screen based on input params
        /// </summary>
        /// <param name="message"></param>
        /// <param name="recipient"></param>
        /// <param name="subjectPlayerId"></param>
        /// <param name="durationMs"></param>
        /// <param name="addToShipLog"></param>
        public static void Notification(string message, PhotonPlayer recipient, int subjectPlayerId = 0, int durationMs = 6000, bool addToShipLog = false)
        {
            if (PLServer.Instance == null)
            {
                Logger.Info($"Notification attempted and PLServer was null. Message: {message}");
                return;
            }

            if (recipient == null || message == null)
            {
                AntiNullReferenceException($"{(message == null ? "message: null" : $"message: \"{message}\"")}, {(recipient == null ? "recipent: null" : "recipent: PLPlayer")}, subjectPlayerId: {subjectPlayerId}, durationMs: {durationMs}, addToShipLog: {addToShipLog}");
                return;
            }

            PLServer.Instance.photonView.RPC("AddNotification", recipient, new object[] {
                message,
                subjectPlayerId,
                PLServer.Instance.GetEstimatedServerMs() + durationMs,
                addToShipLog
            });
        }

        /// <summary>
        /// Adds a Notification to the left side of the screen based on input params
        /// </summary>
        /// <param name="message"></param>
        /// <param name="targets"></param>
        /// <param name="subjectPlayerId"></param>
        /// <param name="durationMs"></param>
        /// <param name="addToShipLog"></param>
        public static void Notification(string message, PhotonTargets targets, int subjectPlayerId = 0, int durationMs = 6000, bool addToShipLog = false)
        {
            if (PLServer.Instance == null)
            {
                Logger.Info($"Notification attempted and PLServer was null. Message: {message}");
                return;
            }

            if (message == null)
            {
                AntiNullReferenceException($"{(message == null ? "message: null" : $"message: \"{message}\"")}, targets: {targets}, subjectPlayerId: {subjectPlayerId}, durationMs: {durationMs}, addToShipLog: {addToShipLog}");
                return;
            }


            PLServer.Instance.photonView.RPC("AddNotification", targets, new object[] {
                message,
                subjectPlayerId,
                PLServer.Instance.GetEstimatedServerMs() + durationMs,
                addToShipLog
            });
        }

        /// <summary>
        /// Adds a CenterPrint Message to the top of the screen based on input params
        /// </summary>
        /// <param name="message"></param>
        /// <param name="recipient"></param>
        /// <param name="tag"></param>
        /// <param name="color"></param>
        /// <param name="type"></param>
        public static void Centerprint(string message, PLPlayer recipient, string tag = "msg", Color color = new Color(), EWarningType type = EWarningType.E_NORMAL)
        {
            if (recipient == null || message == null || tag == null)
            {
                AntiNullReferenceException($"{(recipient == null ? "recipent: null" : "recipent: PLPlayer")}, {(message == null ? "message: null" : $"message: \"{message}\"")}, ..., {(tag == null ? "tag: null" : $"tag: \"{tag}\"")}");
                return;
            }

            Centerprint(message, recipient.GetPhotonPlayer(), tag, color, type);
        }

        /// <summary>
        /// Adds a CenterPrint Message to the top of the screen based on input params
        /// </summary>
        /// <param name="message"></param>
        /// <param name="recipient"></param>
        /// <param name="tag"></param>
        /// <param name="color"></param>
        /// <param name="type"></param>
        public static void Centerprint(string message, PhotonPlayer recipient, string tag = "msg", Color color = new Color(), EWarningType type = EWarningType.E_NORMAL)
        {
            if (recipient == null || message == null || tag == null)
            {
                AntiNullReferenceException($"{(recipient == null ? "recipent: null" : "recipent: PhotonPlayer")}, {(message == null ? "message: null" : $"message: \"{message}\"")}, ..., {(tag == null ? "tag: null" : $"tag: \"{tag}\"")}");
                return;
            }

            PLServer.Instance.photonView.RPC("AddCrewWarning", recipient, new object[] {
                message,
                color,
                (int)type,
                tag
            });
        }

        /// <summary>
        /// Adds a CenterPrint Message to the top of the screen based on input params
        /// </summary>
        /// <param name="message"></param>
        /// <param name="targets"></param>
        /// <param name="tag"></param>
        /// <param name="color"></param>
        /// <param name="type"></param>
        public static void Centerprint(string message, PhotonTargets targets, string tag = "msg", Color color = new Color(), EWarningType type = EWarningType.E_NORMAL)
        {
            if (message == null || tag == null)
            {
                AntiNullReferenceException($"targets: {targets}, {(message == null ? "message: null" : $"message: \"{message}\"")}, {(tag == null ? "tag: null" : $"tag: \"{tag}\"")}");
                return;
            }

            PLServer.Instance.photonView.RPC("AddCrewWarning", targets, new object[] {
                message,
                color,
                (int)type,
                tag
            });
        }

        /// <summary>
        /// Adds entry to ship log based on input params
        /// </summary>
        /// <param name="message"></param>
        /// <param name="tag"></param>
        /// <param name="color"></param>
        /// <param name="addOnlyLocally"></param>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="turretID"></param>
        /// <param name="damage"></param>
        public static void ShipLog(string message, string tag = "msg", Color color = new Color(), bool addOnlyLocally = false, PLShipInfoBase source = null, PLShipInfoBase destination = null, int turretID = -1, int damage = 0)
        {
            if (PhotonNetwork.isMasterClient)
            {
                if (message == null || tag == null)
                {
                    AntiNullReferenceException($"{(message == null ? "message: null" : $"message: \"{message}\"")}, {(tag == null ? "tag: null" : $"tag: \"{tag}\"")}, ..., {(source == null ? "null source" : $"source: \"{source}\"")}, {(destination == null ? "null destination" : $"destination: \"{destination}\"")}");
                    return;
                }

                PLServer.Instance.AddToShipLog(tag, message, color, addOnlyLocally, source, destination, turretID, damage);
            }
        }

        internal static void AntiNullReferenceException(string args)
        {
            var stacktrace = new System.Diagnostics.StackTrace();
            var targetMethod = stacktrace.GetFrame(1).GetMethod();
            var who = stacktrace.GetFrames().Skip(1).FirstOrDefault(f => !ignore.Any(i => i == f.GetMethod().Name))?.GetMethod();

            Logger.Info($"NullReferenceException! Target -> {targetMethod.Name}({args}); Caller -> {who?.ReflectedType?.FullName}.{who?.Name}({who?.GetParameters()?.ToStringFull()});");

            if (PMLConfig.DebugMode)
                Messaging.Notification("NullReferenceException! Check the logs!");
        }

        static string[] ignore = { "ShipLog", "Centerprint", "Notification", "Echo", "ChatMessage" };
    }
}
