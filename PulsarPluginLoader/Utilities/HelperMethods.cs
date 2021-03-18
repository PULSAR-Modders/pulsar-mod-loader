namespace PulsarPluginLoader.Utilities
{
    class HelperMethods
    {
        /// <summary>
        /// Returns first player found by the given player name. Returns null if not found.
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public static PLPlayer GetPlayerFromPlayerName(string playerName)
        {
            foreach (PLPlayer player in PLServer.Instance.AllPlayers)
            {
                if(player != null && player.GetPlayerName(false).ToLower().StartsWith(playerName.ToLower()))
                {
                    return player;
                }
            }
            return null;
        }
        /// <summary>
        /// Returns first player found by the given class name. Returns null if not found.
        /// </summary>
        /// <param name="ClassName"></param>
        /// <returns></returns>
        public static PLPlayer GetPlayerFromClassName(string ClassName)
        {
            string Class = ClassName.ToLower().Substring(0, 1);
            switch (Class)
            {
                case "c":
                    return PLServer.Instance.GetCachedFriendlyPlayerOfClass(0);
                case "p":
                    return PLServer.Instance.GetCachedFriendlyPlayerOfClass(1);
                case "s":
                    return PLServer.Instance.GetCachedFriendlyPlayerOfClass(2);
                case "w":
                    return PLServer.Instance.GetCachedFriendlyPlayerOfClass(3);
                case "e":
                    return PLServer.Instance.GetCachedFriendlyPlayerOfClass(4);
                default:
                    return null;
            }
        }
        /// <summary>
        /// Returns Class ID from string. Returns -1 if not found.
        /// </summary>
        /// <param name="ClassName"></param>
        /// <param name="Successfull"></param>
        /// <returns></returns>
        public static int GetClassIDFromClassName(string ClassName, out bool Successfull)
        {
            Successfull = true;
            switch (ClassName.Substring(0, 1).ToLower())
            {
                case "c":
                    return 0;
                case "p":
                    return 1;
                case "s":
                    return 2;
                case "w":
                    return 3;
                case "e":
                    return 4;
                default:
                    Successfull = false;
                    return -1;
            }
        }
        /// <summary>
        /// returns the ship tag if found. Otherwise returns null
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static PLShipInfoBase GetShipFromLetterTag(string arg)
        {
            char tag = arg.ToUpper()[0];
            foreach (PLShipInfoBase plshipInfoBase in PLEncounterManager.Instance.AllShips.Values)
            {
                if (plshipInfoBase != null && plshipInfoBase is PLShipInfo && plshipInfoBase.TagID != -1 && PLGlobal.getTagChar(plshipInfoBase.TagID) == tag)
                {
                    return plshipInfoBase;
                }
            }
            return null;
        }
    }
}
