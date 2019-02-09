using UnityEngine;

namespace PulsarPluginLoader.Utilities
{
    public static class Messaging
    {
        public static void ChatMessage(PLPlayer recipient, string message, int sendingPlayerId)
        {
            ChatMessage(recipient.GetPhotonPlayer(), message, sendingPlayerId);
        }

        public static void ChatMessage(PhotonPlayer recipient, string message, int sendingPlayerId)
        {
            PLServer.Instance.photonView.RPC("TeamMessage", recipient, new object[] {
                message,
                sendingPlayerId
            });
        }

        public static void ChatMessage(PhotonTargets targets, string message, int sendingPlayerId)
        {
            PLServer.Instance.photonView.RPC("TeamMessage", targets, new object[] {
                message,
                sendingPlayerId
            });
        }

        public static void Echo(PLPlayer recipient, string message)
        {
            Echo(recipient.GetPhotonPlayer(), message);
        }

        public static void Echo(PhotonPlayer recipient, string message)
        {
            PLServer.Instance.photonView.RPC("ConsoleMessage", recipient, new object[] {
                message
            });
        }

        public static void Echo(PhotonTargets targets, string message)
        {
            PLServer.Instance.photonView.RPC("ConsoleMessage", targets, new object[] {
                message
            });
        }

        public static void Notification(PLPlayer recipient, string message, int subjectPlayerId = 0, int durationMs = 6000, bool addToShipLog = false)
        {
            Notification(recipient.GetPhotonPlayer(), message, subjectPlayerId, durationMs, addToShipLog);
        }

        public static void Notification(PhotonPlayer recipient, string message, int subjectPlayerId = 0, int durationMs = 6000, bool addToShipLog = false)
        {
            PLServer.Instance.photonView.RPC("AddNotification", recipient, new object[] {
                message,
                subjectPlayerId,
                PLServer.Instance.GetEstimatedServerMs() + durationMs,
                addToShipLog
            });
        }

        public static void Notification(PhotonTargets targets, string message, int subjectPlayerId = 0, int durationMs = 6000, bool addToShipLog = false)
        {
            PLServer.Instance.photonView.RPC("AddNotification", targets, new object[] {
                message,
                subjectPlayerId,
                PLServer.Instance.GetEstimatedServerMs() + durationMs,
                addToShipLog
            });
        }

        public static void Centerprint(PLPlayer recipient, string tag, string message, Color color, EWarningType type = EWarningType.E_NORMAL)
        {
            Centerprint(recipient.GetPhotonPlayer(), tag, message, color, type);
        }

        public static void Centerprint(PhotonPlayer recipient, string tag, string message, Color color, EWarningType type = EWarningType.E_NORMAL)
        {
            PLServer.Instance.photonView.RPC("AddCrewWarning", recipient, new object[] {
                message,
                color,
                (int)type,
                tag
            });
        }

        public static void Centerprint(PhotonTargets targets, string tag, string message, Color color, EWarningType type = EWarningType.E_NORMAL)
        {
            PLServer.Instance.photonView.RPC("AddCrewWarning", targets, new object[] {
                message,
                color,
                (int)type,
                tag
            });
        }

        public static void ShipLog(string tag, string message, Color color, bool addOnlyLocally = false, PLShipInfoBase source = null, PLShipInfoBase destination = null, int turretID = -1, int damage = 0)
        {
            if (PhotonNetwork.isMasterClient)
            {
                PLServer.Instance.AddToShipLog(tag, message, color, addOnlyLocally, source, destination, turretID, damage);
            }
        }
    }
}
