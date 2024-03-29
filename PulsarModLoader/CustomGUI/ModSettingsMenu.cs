﻿namespace PulsarModLoader.CustomGUI
{
    public abstract class ModSettingsMenu
    {
        public abstract string Name();
        public abstract void Draw();
        public virtual void OnOpen() { }
        public virtual void OnClose() { }
    }
}