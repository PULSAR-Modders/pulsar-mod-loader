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
            ModMessageHelper.Instance.photonView.RPC("ReceiveMessage", player, new object[]
            {
                harmonyIdentifier + "#" + handlerIdentifier,
                arguments
            });
        }

        public static void SendRPC(string harmonyIdentifier, string handlerIdentifier, PhotonTargets targets, object[] arguments)
        {
            ModMessageHelper.Instance.photonView.RPC("ReceiveMessage", targets, new object[]
            {
                harmonyIdentifier + "#" + handlerIdentifier,
                arguments
            });
        }

        public abstract void HandleRPC(object[] arguments, PhotonMessageInfo sender);
    }
}
