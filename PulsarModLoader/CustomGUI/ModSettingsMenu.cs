namespace PulsarModLoader.CustomGUI
{
    /// <summary>
    /// ModSettingsMenu, called by PML's ModManager (F5 menu). PML automatically finds and instantiates all classes inheriting from ModSettingsMenu.
    /// </summary>
    public abstract class ModSettingsMenu
    {
        /// <summary>
        /// MSM display name in ModManager
        /// </summary>
        /// <returns></returns>
        public abstract string Name();

        /// <summary>
        /// GUI Frame update call. Use UnityEngine.GUILayout, referencing UnityEngine.IMGUIModule.
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// Called on menu open.
        /// </summary>
        public virtual void OnOpen() { }

        /// <summary>
        /// Called on menu close.
        /// </summary>
        public virtual void OnClose() { }

        internal PulsarMod MyMod;
    }
}