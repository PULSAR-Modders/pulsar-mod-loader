using UnityEngine;

namespace PulsarPluginLoader.Content.Components.Missile
{
    public abstract class MissilePlugin : ComponentPluginBase
    {
        public MissilePlugin()
        {
        }
        public virtual float Damage
        {
            get { return 360f; }
        }
        public virtual float Speed
        {
            get { return 12f; }
        }
        public virtual EDamageType DamageType
        {
            get { return EDamageType.E_PHYSICAL; }
        }
        public virtual int MissileRefillPrice
        {
            get { return 80; }
        }
        public virtual int AmmoCapacity
        {
            get { return 40; }
        }
        public virtual int PrefabID
        {
            get { return 0; }
        }
    }
}
