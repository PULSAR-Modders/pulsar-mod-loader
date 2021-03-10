namespace PulsarPluginLoader
{
    public abstract class ModMessage
    {
        /// <summary>
        /// Gets the unique identifier for this mod
        /// </summary>
        /// <returns>namespace.name</returns>
        public string GetIdentifier()
        {
            return GetType().Namespace + "." + GetType().Name;
        }

        /// <summary>
        /// Send data to a PhotonPlayer's mod specified by harmonyIdentifier and handlerIdentifier
        /// </summary>
        /// <param name="harmonyIdentifier">PulsarPluginLoader.PulsarPlugin.HarmonyIdentifier()</param>
        /// <param name="handlerIdentifier">PulsarPluginLoader.ModMessage.GetIdentifier()</param>
        /// <param name="player"></param>
        /// <param name="arguments"></param>
        public static void SendRPC(string harmonyIdentifier, string handlerIdentifier, PhotonPlayer player, object[] arguments)
        {
            ModMessageHelper.Instance.photonView.RPC("ReceiveMessage", player, new object[]
            {
                harmonyIdentifier + "#" + handlerIdentifier,
                arguments
            });
        }

        /// <summary>
        /// Send data to a PhotonTarget's mod specified by harmonyIdentifier and handlerIdentifier
        /// </summary>
        /// <param name="harmonyIdentifier">PulsarPluginLoader.PulsarPlugin.HarmonyIdentifier()</param>
        /// <param name="handlerIdentifier">PulsarPluginLoader.ModMessage.GetIdentifier()</param>
        /// <param name="targets"></param>
        /// <param name="arguments"></param>
        public static void SendRPC(string harmonyIdentifier, string handlerIdentifier, PhotonTargets targets, object[] arguments)
        {
            ModMessageHelper.Instance.photonView.RPC("ReceiveMessage", targets, new object[]
            {
                harmonyIdentifier + "#" + handlerIdentifier,
                arguments
            });
        }

        /// <summary>
        /// Recieve data from other players
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="sender"></param>
        public abstract void HandleRPC(object[] arguments, PhotonMessageInfo sender);
    }
}
