﻿using UnityEngine;

namespace PulsarModLoader.Content.Components.WarpDriveProgram
{
    public abstract class WarpDriveProgramMod : ComponentModBase
    {
        public WarpDriveProgramMod()
        {
        }
        public virtual int MaxLevelCharges
        {
            get { return 3; }
        }
        public virtual bool IsVirus
        {
            get { return false; }
        }
        public virtual int VirusSubtype
        {
            get { return 0; }
        }
        public virtual string ShortName
        {
            get { return ""; }
        }
        public virtual float ActiveTime
        {
            get { return 15f; }
        }
        public override Texture2D IconTexture
        {
            get { return PLGlobal.Instance.ProgramBGTexture;  }
        }
        public virtual void Execute(PLWarpDriveProgram InWarpDriveProgram)
        {

        }
        public override int CargoVisualID => 36;
    }
}
