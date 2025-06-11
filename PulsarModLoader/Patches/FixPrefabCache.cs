using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace PulsarModLoader.Patches
{
    /// <summary>
    /// Adding photonview objects to the PrefabCache sometimes causes an error.
    /// This fix was made by Badryuiner.
    /// </summary>
    [HarmonyPatch]
    internal static class FixPrefabCache
    {
        public static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("NetworkingPeer");
            return AccessTools.Method(type, "DoInstantiate");
        }

        public static void Prefix(ExitGames.Client.Photon.Hashtable evData, PhotonPlayer photonPlayer, ref GameObject resourceGameObject)
        {
            if (resourceGameObject == null)
            {
                string text = (string)evData[0];
                PhotonNetwork.PrefabCache.TryGetValue(text, out resourceGameObject);
            }
        }
    }
}
