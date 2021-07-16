using HarmonyLib;
using System;
using System.Diagnostics;
using System.Reflection;

namespace PulsarPluginLoader
{
    public abstract class PulsarPlugin
    {
        private FileVersionInfo VersionInfo;
        protected Harmony harmony;

        /// <summary>
        /// Entry point of plugin; do setup here (e.g., Harmony, databases, etc).  Runs once during game startup.
        /// </summary>
        public PulsarPlugin()
        {
            // Can't use Assembly.GetExecutingAssembly() or it grabs this assembly instead of the plugin's!
            // Executing assembly is technically PPL's during base class methods.
            Assembly asm = GetType().Assembly;
            VersionInfo = FileVersionInfo.GetVersionInfo(asm.Location);

            harmony = new Harmony(HarmonyIdentifier());
            harmony.PatchAll(asm);
        }

        /// <summary>
        /// Unique plugin identifier used by Harmony to differentiate between plugins.
        /// Reverse domain notation recommended (e.g., com.example.pulsar.plugins)
        /// </summary>
        /// <returns></returns>
        public abstract string HarmonyIdentifier();

        /// <summary>
        /// Removes a plugin from the list and calls UnpatchAll (); do some extra cleanup here.
        /// </summary>
        public virtual void Unload() => PluginManager.Instance.UnloadPlugin(this, ref harmony);

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
        /// Plugin's multiplayer requirements. use MPFunction.
        /// None: No Functionality
        /// HostOnly: Only the host is required to have it installed
        /// HostApproved: Host must have the mod installed, works better when client has it installed.
        /// All: All players must have the mod installed
        /// </summary>
        public virtual int MPFunctionality
        {
            get
            {
                return (int)MPFunction.None;
            }
        }
    }
}
