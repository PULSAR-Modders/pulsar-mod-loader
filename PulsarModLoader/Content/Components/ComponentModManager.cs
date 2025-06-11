using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PulsarModLoader.Content.Components
{
    public enum Empty { } // Empty reference for when Enum doesnt exist
    public abstract class ComponentModManager<TMod, TEnum> where TMod : ComponentModBase
    {
        public readonly int VanillaMaxType;
        public readonly List<TMod> types = new List<TMod>();

        protected ComponentModManager(int vanillaMaxType = -1) // Override for Enum count being wrong
        {
            VanillaMaxType = vanillaMaxType;
            if (VanillaMaxType == -1) VanillaMaxType = Enum.GetValues(typeof(TEnum)).Length;
            Logger.Info($"{typeof(TMod).Name} MaxTypeint: {VanillaMaxType - 1}");

            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                foreach (Type t in asm.GetTypes())
                {
                    if (typeof(TMod).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        //Logger.Info($"Loading {typeof(TMod).Name} from assembly");
                        TMod handler = (TMod)Activator.CreateInstance(t);
                        if (GetIDFromName(handler.Name) == -1)
                        {
                            types.Add(handler);
                            Logger.Info($"Added {typeof(TMod).Name}: '{handler.Name}' with ID '{GetIDFromName(handler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add {typeof(TMod).Name} from {mod.Name} with the duplicate name of '{handler.Name}'");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find.
        /// </summary>
        /// <param name="name">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetIDFromName(string name)
        {
            for (int i = 0; i < types.Count; i++)
            {
                if (types[i].Name == name)
                {
                    return i + VanillaMaxType;
                }
            }
            return -1;
        }

        public IReadOnlyList<TMod> ModTypes => types;
    }
}
