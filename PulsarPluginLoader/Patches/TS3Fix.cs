using Harmony;

namespace PulsarPluginLoader.Patches
{
    [HarmonyPatch(typeof(PLVoiceChatManager), "GetVoiceChatChannelName")]
    class TS3Fix
    {
        private static string Postfix(string __result)
        {
            string versionString = PLNetworkManager.Instance.VersionString.GetDecrypted();

            return string.Concat(new string[]
            {
                "[",
                PLXMLOptionsIO.Instance.CurrentOptions.GetStringValue("PhotonRegion") + " - ",
                versionString.Substring(0, versionString.IndexOf('\n')),
                PhotonNetwork.room.name,
                "]"
            }).Normalize();
        }
    }
}
