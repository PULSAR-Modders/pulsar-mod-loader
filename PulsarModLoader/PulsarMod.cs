using HarmonyLib;
using PulsarModLoader.MPModChecks;
using System;
using System.Diagnostics;
using System.Reflection;

namespace PulsarModLoader
{
    /// <summary>
    /// Used by PML to signify a mod. Must have a unique harmonyID
    /// </summary>
    public abstract class PulsarMod
    {
        internal FileVersionInfo VersionInfo;

        /// <summary>
        /// 
        /// </summary>
        protected Harmony harmony;

        /// <summary>
        /// Mod enabled/disabled.
        /// </summary>
        protected bool enabled = true;

        /// <summary>
        /// Entry point of mod; do setup here (e.g., Harmony, databases, etc).  Runs once during game startup.
        /// </summary>
        public PulsarMod()
        {
            // Can't use Assembly.GetExecutingAssembly() or it grabs this assembly instead of the mod's!
            // Executing assembly is technically PML's during base class methods.
            Assembly asm = GetType().Assembly;

            harmony = new Harmony(HarmonyIdentifier());
            harmony.PatchAll(asm);
        }

        /// <summary>
        /// Removes a mod from the list and calls UnpatchAll (); do some extra cleanup here.
        /// </summary>
        public virtual void Unload() => ModManager.Instance.UnloadMod(this, ref harmony);

        /// <summary>
        /// Unique mod identifier used by Harmony and PML to differentiate between mods.<br/>
        /// Combination of AuthorName and ModName recommended. (Ex: ExampleAuthor403.ExampleMod)
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

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        [Obsolete]//Legacy support
        public virtual int MPFunctionality
        {
            get
            {
                return (int)MPRequirement.None;
            }
        }
        #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


        #pragma warning disable CS0612 // Type or member is obsolete
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
                if (MPFunctionality >= (int)MPRequirement.Host)//Legacy Support if statement
                {
                    return MPFunctionality;
                }
                else
                {
                    return (int)MPRequirement.None;
                }
            }
        }
        #pragma warning restore CS0612 // Type or member is obsolete
        
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
		/// A link to the version file containing information about the latest version of the mod. Null if there is no link for this mod.
		/// </summary>
		public virtual string VersionLink
        {
            get
            {
                return string.Empty;
            }
        }
    }
}
