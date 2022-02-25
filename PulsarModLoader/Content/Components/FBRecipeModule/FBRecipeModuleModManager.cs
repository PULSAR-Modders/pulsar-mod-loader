using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static PulsarModLoader.Patches.HarmonyHelpers;
using Logger = PulsarModLoader.Utilities.Logger;

namespace PulsarModLoader.Content.Components.FBRecipeModule
{
    public class FBRecipeModuleModManager
    {
        public readonly int VanillaFBRecipeModuleMaxType = 0;
        private static FBRecipeModuleModManager m_instance = null;
        public readonly List<FBRecipeModuleMod> FBRecipeModuleTypes = new List<FBRecipeModuleMod>();
        public static FBRecipeModuleModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new FBRecipeModuleModManager();
                }
                return m_instance;
            }
        }

        FBRecipeModuleModManager()
        {
            VanillaFBRecipeModuleMaxType = Enum.GetValues(typeof(FBRecipe)).Length;
            Logger.Info($"MaxTypeint = {VanillaFBRecipeModuleMaxType - 1}");
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                Type FBRecipeModuleMod = typeof(FBRecipeModuleMod);
                foreach (Type t in asm.GetTypes())
                {
                    if (FBRecipeModuleMod.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        Logger.Info("Loading FBRecipeModule from assembly");
                        FBRecipeModuleMod FBRecipeModuleModHandler = (FBRecipeModuleMod)Activator.CreateInstance(t);
                        if (FBRecipeModuleModHandler.ItemTypeToProduce.Length < 2)
                        {
                            Logger.Info($"FBRecipeModule '{FBRecipeModuleModHandler.Name}' from {mod.Name} does not have enough objects in the ItemTypeToProduce field, must have 2.");
                            continue;
                        }
                        if (GetFBRecipeModuleIDFromName(FBRecipeModuleModHandler.Name) == -1)
                        {
                            FBRecipeModuleTypes.Add(FBRecipeModuleModHandler);
                            Logger.Info($"Added FBRecipeModule: '{FBRecipeModuleModHandler.Name}' with ID '{GetFBRecipeModuleIDFromName(FBRecipeModuleModHandler.Name)}'");
                        }
                        else
                        {
                            Logger.Info($"Could not add FBRecipeModule from {mod.Name} with the duplicate name of '{FBRecipeModuleModHandler.Name}'");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Finds FBRecipeModule type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find FBRecipeModule.
        /// </summary>
        /// <param name="FBRecipeModuleName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetFBRecipeModuleIDFromName(string FBRecipeModuleName)
        {
            for (int i = 0; i < FBRecipeModuleTypes.Count; i++)
            {
                if (FBRecipeModuleTypes[i].Name == FBRecipeModuleName)
                {
                    return i + VanillaFBRecipeModuleMaxType;
                }
            }
            return -1;
        }
        public static PLFBRecipeModule CreateFBRecipeModule(int Subtype, int level)
        {
            PLFBRecipeModule InFBRecipeModule;
            if (Subtype >= Instance.VanillaFBRecipeModuleMaxType)
            {
                InFBRecipeModule = new PLFBRecipeModule(FBRecipe.E_MAX, level);
                int subtypeformodded = Subtype - Instance.VanillaFBRecipeModuleMaxType;
                Logger.Info($"Subtype for modded is {subtypeformodded}");
                if (subtypeformodded <= Instance.FBRecipeModuleTypes.Count && subtypeformodded > -1)
                {
                    Logger.Info("Creating FBModule from list info");
                    FBRecipeModuleMod FBRecipeModuleType = Instance.FBRecipeModuleTypes[Subtype - Instance.VanillaFBRecipeModuleMaxType];
                    InFBRecipeModule.SubType = Subtype;
                    InFBRecipeModule.Name = FBRecipeModuleType.Name;
                    InFBRecipeModule.Desc = FBRecipeModuleType.Description;
                    InFBRecipeModule.m_IconTexture = FBRecipeModuleType.IconTexture;
                    InFBRecipeModule.m_MarketPrice = FBRecipeModuleType.MarketPrice;
                    InFBRecipeModule.CargoVisualPrefabID = FBRecipeModuleType.CargoVisualID;
                    InFBRecipeModule.CanBeDroppedOnShipDeath = FBRecipeModuleType.CanBeDroppedOnShipDeath;
                    InFBRecipeModule.Experimental = FBRecipeModuleType.Experimental;
                    InFBRecipeModule.Unstable = FBRecipeModuleType.Unstable;
                    InFBRecipeModule.Contraband = FBRecipeModuleType.Contraband;
                    InFBRecipeModule.Price_LevelMultiplierExponent = FBRecipeModuleType.Price_LevelMultiplierExponent;
                    InFBRecipeModule.CookDurationMs = FBRecipeModuleType.CookDurationMs;
                    InFBRecipeModule.CookedTimingOffsetMidpoint = FBRecipeModuleType.CookedTimingOffsetMidpoint;
                    InFBRecipeModule.PerfectlyCookedMaxTimingOffset = FBRecipeModuleType.PerfectlyCookedMaxTimingOffset;
                    InFBRecipeModule.CookedMaxTimingOffset = FBRecipeModuleType.CookedMaxTimingOffset;
                    InFBRecipeModule.FoodSupplyCost = FBRecipeModuleType.FoodSupplyCost;
                    InFBRecipeModule.BiscuitTypeToProduce = EFoodType.MAX;
                    InFBRecipeModule.IconResourcePath = string.Empty;
                }
            }
            else
            {
                InFBRecipeModule = new PLFBRecipeModule((FBRecipe)Subtype, level);
            }
            return InFBRecipeModule;
        }
    }
    //Converts hashes to FBRecipeModules.
    [HarmonyPatch(typeof(PLFBRecipeModule), "CreateRecipeFromHash")]
    class FBRecipeModuleHashFix
    {
        static bool Prefix(int inSubType, int inLevel, ref PLShipComponent __result)
        {
            __result = FBRecipeModuleModManager.CreateFBRecipeModule(inSubType, inLevel);
            return false;
        }
    }
    [HarmonyPatch(typeof(PLFluffyOven), "CreateElementForRecipe")]
    class RecipeDisplayIconPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Ldfld),
                new CodeInstruction(OpCodes.Call),

            };
            int LabelIndex = FindSequence(instructions, targetSequence, CheckMode.NONNULL) -3;
            object fieldRef = instructions.ToList()[LabelIndex].operand;
            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldfld, fieldRef),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RecipeDisplayIconPatch), "GetSprite")),
            };

            return PatchBySequence(instructions, targetSequence, injectedSequence, patchMode: PatchMode.REPLACE, checkMode: CheckMode.NONNULL);
        }
        static Sprite GetSprite(PLFBRecipeModule recipeModule)
        {
            if(recipeModule == null)
            {
                throw new Exception("Module Null");
            }
            string inpath = recipeModule.IconResourcePath;
            if(inpath == string.Empty)
            {
                int subtypeformodded = recipeModule.SubType - FBRecipeModuleModManager.Instance.VanillaFBRecipeModuleMaxType;
                if (subtypeformodded > -1 && subtypeformodded < FBRecipeModuleModManager.Instance.FBRecipeModuleTypes.Count)
                {
                    return FBRecipeModuleModManager.Instance.FBRecipeModuleTypes[subtypeformodded].OvenIcon;
                }
                throw new Exception("PulsarModLoader.Content.FBModuleModManager.RecipeDisplayIconPatch - Recipe Module not found");
            }
            return Resources.Load<Sprite>(inpath);
        }
    }
    [HarmonyPatch(typeof(PLFluffyOven), "ServerTakeBiscuit")]
    class ServerTakeBiscuitPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_0),   // pawninvidcounter
                new CodeInstruction(OpCodes.Ldc_I4_5),  // 5
                new CodeInstruction(OpCodes.Ldarg_0),   // this.
                new CodeInstruction(OpCodes.Ldfld),     // currentproducingmodule
                new CodeInstruction(OpCodes.Callvirt),  // getbiscuittypetoproduce
                new CodeInstruction(OpCodes.Ldarg_2),   // biscuitlevel
                new CodeInstruction(OpCodes.Ldc_I4_M1), // -1

            };
            int arrayindex = generator.DeclareLocal(typeof(int[])).LocalIndex;
            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                //grab current CurrentProducingModule, grab item type to produce, feed type and subtype to UpdateItem
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ServerTakeBiscuitPatch), "PatchMethod")),
                new CodeInstruction(OpCodes.Stloc, arrayindex),   //stores value to local var 1
                new CodeInstruction(OpCodes.Ldloc_0),             //-PawnInvIDCounter
                new CodeInstruction(OpCodes.Ldloc, arrayindex),   //load array
                new CodeInstruction(OpCodes.Ldc_I4_0),            //index of element in array
                new CodeInstruction(OpCodes.Ldelem_I4),           //-load element from array 
                new CodeInstruction(OpCodes.Ldloc, arrayindex),   //load array
                new CodeInstruction(OpCodes.Ldc_I4_1),            //index of element in array
                new CodeInstruction(OpCodes.Ldelem_I4),           //-load element from array
                new CodeInstruction(OpCodes.Ldarg_2),             //-biscuitlevel
                new CodeInstruction(OpCodes.Ldc_I4_M1),           //-1
            };
            return PatchBySequence(instructions, targetSequence, injectedSequence, patchMode: PatchMode.REPLACE, checkMode: CheckMode.NONNULL);
        }
        static int[] PatchMethod(PLFluffyOven instance)
        {
            PLFBRecipeModule module = instance.CurrentProducingModule;
            if (module.GetBiscuitTypeToProduce() == EFoodType.MAX)
            {
                int subtypeformodded = module.SubType - FBRecipeModuleModManager.Instance.VanillaFBRecipeModuleMaxType;
                if (subtypeformodded > -1 && subtypeformodded < FBRecipeModuleModManager.Instance.FBRecipeModuleTypes.Count)
                {
                    return FBRecipeModuleModManager.Instance.FBRecipeModuleTypes[subtypeformodded].ItemTypeToProduce;
                }
                else
                {
                    throw new Exception("PulsarModLoader.Content.FBModuleModManager.ServerTakeBiscuitPatch - Foodtype max with no module found");
                }
            }
            else
            {
                return new int[] { 5, (int)module.GetBiscuitTypeToProduce() };
            }
        }
    }
    [HarmonyPatch(typeof(PLFluffyOven), "Update")]
    class FluffyOvenUpdatePatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PLFluffyOven), "InternalBiscuitVisualInfo")),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(PLPawnItem), "get_SubType")),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PLFluffyOven), "CurrentProducingModule")),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(PLFBRecipeModule), "GetBiscuitTypeToProduce")),
            };
            int LabelIndex = FindSequence(instructions, targetSequence, CheckMode.NONNULL);
            Label thing = (Label)instructions.ToList()[LabelIndex].operand;
            int PatchMethodArrayindex = generator.DeclareLocal(typeof(int[])).LocalIndex;
            int outSubTypeindex = generator.DeclareLocal(typeof(int)).LocalIndex;
            int outMainTypeindex = generator.DeclareLocal(typeof(int)).LocalIndex;
            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Items.ItemModManager), "get_Instance")),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PLFluffyOven), "InternalBiscuitVisualInfo")),
                new CodeInstruction(OpCodes.Ldloca_S, outMainTypeindex),
                new CodeInstruction(OpCodes.Ldloca_S, outSubTypeindex),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Items.ItemModManager), "GetActualMainAndSubTypesFromPawnItem")),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ServerTakeBiscuitPatch), "PatchMethod")),
                new CodeInstruction(OpCodes.Stloc_S, PatchMethodArrayindex),
                new CodeInstruction(OpCodes.Ldloc_S, PatchMethodArrayindex),   //load array
                new CodeInstruction(OpCodes.Ldc_I4_0),            //index of element in array
                new CodeInstruction(OpCodes.Ldelem_I4),
                new CodeInstruction(OpCodes.Ldloc_S, outMainTypeindex),
                new CodeInstruction(OpCodes.Beq, thing),
                new CodeInstruction(OpCodes.Ldloc_S, PatchMethodArrayindex),   //load array
                new CodeInstruction(OpCodes.Ldc_I4_1),            //index of element in array
                new CodeInstruction(OpCodes.Ldelem_I4),
                new CodeInstruction(OpCodes.Ldloc_S, outSubTypeindex),
            };

            IEnumerable<CodeInstruction> firstModified = PatchBySequence(instructions, targetSequence, injectedSequence, patchMode: PatchMode.REPLACE, checkMode: CheckMode.NONNULL);
            List<CodeInstruction> targetSequence2 = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldc_I4_5),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PLFluffyOven), "CurrentProducingModule")),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(PLFBRecipeModule), "GetBiscuitTypeToProduce")),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PLPawnItem), "CreateFromInfo")),
                new CodeInstruction(OpCodes.Isinst)
            };
            PatchMethodArrayindex = generator.DeclareLocal(typeof(int[])).LocalIndex;
            List<CodeInstruction> injectedSequence2 = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ServerTakeBiscuitPatch), "PatchMethod")),
                new CodeInstruction(OpCodes.Stloc_S, PatchMethodArrayindex),
                new CodeInstruction(OpCodes.Ldloc_S, PatchMethodArrayindex),   //load array
                new CodeInstruction(OpCodes.Ldc_I4_0),            //index of element in array
                new CodeInstruction(OpCodes.Ldelem_I4),
                new CodeInstruction(OpCodes.Ldloc_S, PatchMethodArrayindex),   //load array
                new CodeInstruction(OpCodes.Ldc_I4_1),            //index of element in array
                new CodeInstruction(OpCodes.Ldelem_I4),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Items.ItemModManager), "CreatePawnItem"))
            };

            return PatchBySequence(firstModified, targetSequence2, injectedSequence2, patchMode: PatchMode.REPLACE, checkMode: CheckMode.NONNULL);
        }
    }
}
