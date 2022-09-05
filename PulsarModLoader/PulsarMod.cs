using HarmonyLib;
using PulsarModLoader.MPModChecks;
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
        /// Entry point of mod; do setup here (e.g., Harmony, databases, etc).  Runs once during game startup.
        /// </summary>
        public PulsarMod()
        {
            // Can't use Assembly.GetExecutingAssembly() or it grabs this assembly instead of the mod's!
            // Executing assembly is technically PML's during base class methods.
            Assembly asm = GetType().Assembly;
            VersionInfo = FileVersionInfo.GetVersionInfo(asm.Location);

            harmony = new Harmony(HarmonyIdentifier());
            harmony.PatchAll(asm);
        }

        /// <summary>
        /// Removes a mod from the list and calls UnpatchAll (); do some extra cleanup here.
        /// </summary>
        public virtual void Unload() => ModManager.Instance.UnloadMod(this, ref harmony);

        /// <summary>
        /// Unique mod identifier used by Harmony to differentiate between mods.<br/>
        /// Reverse domain notation recommended (e.g., com.example.pulsar.mods)
        /// </summary>
        /// <returns></returns>
        public abstract string HarmonyIdentifier();

        /// <summary>
        /// Version of mod.  Displayed in mod list.
        /// </summary>
        public virtual string Version
        {
            get
            {
                return VersionInfo.FileVersion;
            }
        }

        /// <summary>
        /// Author(s) of mod.  Displayed in mod list.
        /// </summary>
        public virtual string Author
        {
            get
            {
                return VersionInfo.CompanyName;
            }
        }

        /// <summary>
        /// Short (one line) description of mod.  Displayed in mod list.
        /// </summary>
        public virtual string ShortDescription
        {
            get
            {
                return VersionInfo.FileDescription;
            }
        }

        /// <summary>
        /// Long (mutli-line) description of mod.  Ideal for in-game readme or patch notes.  Displayed in mod details.
        /// </summary>
        public virtual string LongDescription
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Name of mod.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return VersionInfo.ProductName;
            }
        }

        [Obsolete]
        public virtual int MPFunctionality
        {
            get
            {
                return MPRequirements;
            }
        }
        /// <summary>
        /// Mod's multiplayer requirements. use MPModChecks.MPRequirement.<br/>
        /// None: No requirement<br/>
        /// Hidden: Hidden from mod lists<br/>
        /// Host: Host must have the mod installed, works better when client has it installed.<br/>
        /// All: All players must have the mod installed
        /// </summary>
        public virtual int MPRequirements
        {
            get
            {
                if (MPFunctionality >= (int)MPRequirement.Host)
                {
                    return MPFunctionality;
                }
                else
                {
                    return (int)MPRequirement.None;
                }
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

        /// <summary>
        /// Mod ID for future feature involving download IDs for a public webserver
        /// </summary>
        public virtual string ModID => "";
    }
}
