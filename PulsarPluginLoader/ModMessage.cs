
namespace PulsarPluginLoader
{
    public abstract class ModMessage
    {
        public string GetIdentifier()
        {
            return GetType().Namespace + "." + GetType().Name;
        }

        public static void SendRPC(string harmonyIdentifier, string handlerIdentifier, PhotonPlayer player, object[] arguments)
        {
            PLServer.Instance.photonView.RPC("ModMessage", player, new object[]
            {
                harmonyIdentifier + "#" + handlerIdentifier,
                arguments
            });
        }

        public static void SendRPC(string harmonyIdentifier, string handlerIdentifier, PhotonTargets targets, object[] arguments)
        {
            PLServer.Instance.photonView.RPC("ModMessage", targets, new object[]
            {
                harmonyIdentifier + "#" + handlerIdentifier,
                arguments
            });
        }

        public abstract void HandleRPC(object[] arguments);
    }
}
