using HarmonyLib;
using System;
using System.Diagnostics;
using System.Reflection;

namespace PulsarModLoader
{
    public abstract class PulsarMod
    {
        private FileVersionInfo VersionInfo;
        protected Harmony harmony;
        protected bool enabled = true;

        /// <summary>
        /// Entry point of plugin; do setup here (e.g., Harmony, databases, etc).  Runs once during game startup.
        /// </summary>
        public PulsarMod()
        {
            // Can't use Assembly.GetExecutingAssembly() or it grabs this assembly instead of the plugin's!
            // Executing assembly is technically PPL's during base class methods.
            Assembly asm = GetType().Assembly;
            VersionInfo = FileVersionInfo.GetVersionInfo(asm.Location);

            harmony = new Harmony(HarmonyIdentifier());
            harmony.PatchAll(asm);
        }

        /// <summary>
        /// Removes a plugin from the list and calls UnpatchAll (); do some extra cleanup here.
        /// </summary>
        public virtual void Unload() => PluginManager.Instance.UnloadPlugin(this, ref harmony);

        /// <summary>
        /// Unique plugin identifier used by Harmony to differentiate between plugins.<br/>
        /// Reverse domain notation recommended (e.g., com.example.pulsar.plugins)
        /// </summary>
        /// <returns></returns>
        public abstract string HarmonyIdentifier();

        /// <summary>
        /// Version of plugin.  Displayed in plugin list.
        /// </summary>
        public virtual string Version
        {
            get
            {
                return VersionInfo.FileVersion;
            }
        }

        /// <summary>
        /// Author(s) of plugin.  Displayed in plugin list.
        /// </summary>
        public virtual string Author
        {
            get
            {
                return VersionInfo.CompanyName;
            }
        }

        /// <summary>
        /// Short (one line) description of plugin.  Displayed in plugin list.
        /// </summary>
        public virtual string ShortDescription
        {
            get
            {
                return VersionInfo.FileDescription;
            }
        }

        /// <summary>
        /// Long (mutli-line) description of plugin.  Ideal for in-game readme or patch notes.  Displayed in plugin details.
        /// </summary>
        public virtual string LongDescription
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Name of plugin.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return VersionInfo.ProductName;
            }
        }

        /// <summary>
        /// Plugin's multiplayer requirements. use MPFunction.<br/>
        /// None: No Functionality<br/>
        /// HostOnly: Only the host is required to have it installed<br/>
        /// HostApproved: Host must have the mod installed, works better when client has it installed.<br/>
        /// All: All players must have the mod installed
        /// </summary>
        public virtual int MPFunctionality
        {
            get
            {
                return (int)MPFunction.None;
            }
        }
        
        /// <summary>
        /// Up to the modder to implement
        /// </summary>
        /// <returns>true if <see cref="Disable"/>, <see cref="Enable"/>, and <see cref="IsEnabled"/> have been implemented, false by default</returns>
        public virtual bool CanBeDisabled()
        {
            return false;
        }

        /// <summary>
        /// The result is invalid if <see cref="CanBeDisabled"/> returns false
        /// </summary>
        /// <returns>true if the mod is enabled, false if the mod has been disabled</returns>
        public virtual bool IsEnabled()
        {
            return enabled;
        }

        /// <summary>
        /// Disables the mod. Up to the modder to implement this.<br/>
        /// If implemented, <see cref="CanBeDisabled()"/> should be modified to return true
        /// </summary>
        public virtual void Disable()
        {
            enabled = false;
        }

        /// <summary>
        /// Opposite of <see cref="Disable"/>.<br/>
        /// The mod should be active if neither Disable nor Enable has been called.
        /// </summary>
        public virtual void Enable()
        {
            enabled = true;
        }
    }
}
