using HarmonyLib;

namespace PulsarModLoader.Content.Components.Missile
{
    public class MissileModManager : ComponentModManager<MissileMod, ETrackerMissileType>
    {
        private static MissileModManager m_instance = null;

        public static MissileModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MissileModManager();
                }
                return m_instance;
            }
        }

        MissileModManager() {}
        
        public static PLTrackerMissile CreateMissile(int Subtype, int level, int inSubTypeData = 0)
        {
            PLTrackerMissile InMissile;
            if (Subtype >= Instance.VanillaMaxType)
            {
                InMissile = new PLTrackerMissile(ETrackerMissileType.MAX, level, inSubTypeData);
                int subtypeformodded = Subtype - Instance.VanillaMaxType;
                if (subtypeformodded <= Instance.types.Count && subtypeformodded > -1)
                {
                    MissileMod MissileType = Instance.types[Subtype - Instance.VanillaMaxType];
                    InMissile.SubType = Subtype;
                    InMissile.Name = MissileType.Name;
                    InMissile.Desc = MissileType.Description;
                    InMissile.m_IconTexture = MissileType.IconTexture;
                    InMissile.Damage = MissileType.Damage;
                    InMissile.Speed = MissileType.Speed;
                    InMissile.DamageType = MissileType.DamageType;
                    InMissile.MissileRefillPrice = MissileType.MissileRefillPrice;
                    InMissile.AmmoCapacity = MissileType.AmmoCapacity;
                    InMissile.PrefabID = MissileType.PrefabID;
                    InMissile.m_MarketPrice = MissileType.MarketPrice;
                    InMissile.CargoVisualPrefabID = MissileType.CargoVisualID;
                    InMissile.CanBeDroppedOnShipDeath = MissileType.CanBeDroppedOnShipDeath;
                    InMissile.Experimental = MissileType.Experimental;
                    InMissile.Unstable = MissileType.Unstable;
                    InMissile.Contraband = MissileType.Contraband;
                    InMissile.Price_LevelMultiplierExponent = MissileType.Price_LevelMultiplierExponent;
                    if (PhotonNetwork.isMasterClient)
                    {
                        InMissile.SubTypeData = (short)InMissile.AmmoCapacity;
                    }
                }
            }
            else
            {
                InMissile = new PLTrackerMissile((ETrackerMissileType)Subtype, level, inSubTypeData);
            }
            return InMissile;
        }
    }
    //Converts hashes to Missiles.
    [HarmonyPatch(typeof(PLTrackerMissile), "CreateTrackerMissileFromHash")]
    class MissileHashFix
    {
        static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
        {
            __result = MissileModManager.CreateMissile(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
}
