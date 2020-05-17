using UnityEngine;

namespace PulsarPluginLoader.Utilities
{
    public static class Messaging
    {
        public static void ChatMessage(PLPlayer recipient, string message, int sendingPlayerId = -1)
        {
            if(sendingPlayerId == -1)
            {
                sendingPlayerId = PLNetworkManager.Instance.LocalPlayerID;
            }
            ChatMessage(recipient.GetPhotonPlayer(), message, sendingPlayerId);
        }

        public static void ChatMessage(PhotonPlayer recipient, string message, int sendingPlayerId = -1)
        {
            if (sendingPlayerId == -1)
            {
                sendingPlayerId = PLNetworkManager.Instance.LocalPlayerID;
            }
            PLServer.Instance.photonView.RPC("TeamMessage", recipient, new object[] {
                message,
                sendingPlayerId
            });
        }

        public static void ChatMessage(PhotonTargets targets, string message, int sendingPlayerId = -1)
        {
            if (sendingPlayerId == -1)
            {
                sendingPlayerId = PLNetworkManager.Instance.LocalPlayerID;
            }
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
        public static void Notification(string message, PLPlayer recipient = null, int subjectPlayerId = 0, int durationMs = 6000, bool addToShipLog = false)
        {
            if (recipient == null)
            {
                recipient = PLNetworkManager.Instance.LocalPlayer;
            }
            Notification(message, recipient.GetPhotonPlayer(), subjectPlayerId, durationMs, addToShipLog);
        }
        public static void Notification(string message, PhotonPlayer recipient, int subjectPlayerId = 0, int durationMs = 6000, bool addToShipLog = false)
        {
            PLServer.Instance.photonView.RPC("AddNotification", recipient, new object[] {
                message,
                subjectPlayerId,
                PLServer.Instance.GetEstimatedServerMs() + durationMs,
                addToShipLog
            });
        }

        public static void Notification(string message, PhotonTargets targets, int subjectPlayerId = 0, int durationMs = 6000, bool addToShipLog = false)
        {
            PLServer.Instance.photonView.RPC("AddNotification", targets, new object[] {
                message,
                subjectPlayerId,
                PLServer.Instance.GetEstimatedServerMs() + durationMs,
                addToShipLog
            });
        }

        public static void Centerprint(string message, PLPlayer recipient, string tag = "msg", Color color = new Color(), EWarningType type = EWarningType.E_NORMAL)
        {
            Centerprint(message, recipient.GetPhotonPlayer(), tag, color, type);
        }

        public static void Centerprint(string message, PhotonPlayer recipient, string tag = "msg", Color color = new Color(), EWarningType type = EWarningType.E_NORMAL)
        {
            PLServer.Instance.photonView.RPC("AddCrewWarning", recipient, new object[] {
                message,
                color,
                (int)type,
                tag
            });
        }

        public static void Centerprint(string message, PhotonTargets targets, string tag = "msg", Color color = new Color(), EWarningType type = EWarningType.E_NORMAL)
        {
            PLServer.Instance.photonView.RPC("AddCrewWarning", targets, new object[] {
                message,
                color,
                (int)type,
                tag
            });
        }

        public static void ShipLog(string message, string tag = "msg", Color color = new Color(), bool addOnlyLocally = false, PLShipInfoBase source = null, PLShipInfoBase destination = null, int turretID = -1, int damage = 0)
        {
            if (PhotonNetwork.isMasterClient)
            {
                PLServer.Instance.AddToShipLog(tag, message, color, addOnlyLocally, source, destination, turretID, damage);
            }
        }
    }
}
