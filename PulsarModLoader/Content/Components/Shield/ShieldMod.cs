﻿using UnityEngine;

namespace PulsarModLoader.Content.Components.Shield
{
    public abstract class ShieldMod : ComponentModBase
    {
        public ShieldMod()
        {
        }
        public override Texture2D IconTexture
        {
            get { return (Texture2D)Resources.Load("Icons/39_Sheilds"); }
        }
        public virtual float ShieldMax
        {
            get { return 70f; }
        }
        public virtual float ChargeRateMax
        {
            get { return 12f; }
        }
        public virtual float RecoveryRate
        {
            get { return 15f; }
        }
        public virtual float Deflection
        {
            get { return 1f; }
        }
        public virtual float MinIntegrityPercentForQuantumShield
        {
            get { return .9f; }
        }
        public virtual float MaxPowerUsage_Watts
        {
            get { return 4600; }
        }
        public virtual int MinIntegrityAfterDamage
        {
            get { return -1; }
        }
        public override int CargoVisualID => 39;
    }
}