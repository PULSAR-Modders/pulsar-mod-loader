using Harmony;

namespace PulsarPluginLoader.Patches
{
    [HarmonyPatch(typeof(PLVoiceChatManager), "GetVoiceChatChannelName")]
    class TS3Fix
    {
        private static string Postfix(string __result)
        {
            return string.Concat(new string[]
            {
                "[",
                PLXMLOptionsIO.Instance.CurrentOptions.GetStringValue("PhotonRegion") + " - ",
                PLNetworkManager.Instance.VersionString.Substring(0, PLNetworkManager.Instance.VersionString.IndexOf('\n')),
                PhotonNetwork.room.name,
                "]"
            }).Normalize();
        }
    }
}
