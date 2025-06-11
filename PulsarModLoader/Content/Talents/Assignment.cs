using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static PulsarModLoader.Content.Talents.TalentModManager;
using static PulsarModLoader.Patches.HarmonyHelpers;

namespace PulsarModLoader.Content.Talents
{
    /* This is my old static method of assigning class talent trees.
     * It'd be preferable to rework to include
     *      - Mod class tree assignments
     *      - Mod talent tree removals
     * etc . . .
    */
    internal class TalentCreation
    {
        // Change what Talents are available for a player to use.
        [HarmonyPatch(typeof(PLTabMenu), "UpdateTDs")]
        class TalentAssignmentPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> target = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_S),       // playerFromPlayerID
                new CodeInstruction(OpCodes.Callvirt),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PLGlobal), "TalentsForClass", new Type[] { typeof(Int32) })),
            };
                int NextInstruction = FindSequence(instructions, target, CheckMode.NONNULL);
                List<CodeInstruction> patch = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),       // PLTabMenu Instance
                instructions.ToList()[NextInstruction - 3], // playerFromPlayerID
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TalentAssignmentPatch), "Patch", new Type[] { typeof(PLTabMenu), typeof(PLPlayer) }))
            };
                return PatchBySequence(instructions, target, patch, PatchMode.REPLACE, CheckMode.NONNULL);
            }
            public static List<ETalents> Patch(PLTabMenu instance, PLPlayer pLPlayer)
            {
                return TalentCreation.TalentsForClassSpecies(pLPlayer, pLPlayer.GetClassID());
            }
        }

        public static Dictionary<(CharacterClass, CharacterSpecies), List<ETalents>> cachedTalents = new Dictionary<(CharacterClass, CharacterSpecies), List<ETalents>>
        {
            #region General Talents
            // General Talents
            [(CharacterClass.General, CharacterSpecies.General)] = new List<ETalents>
            {
                ETalents.HEALTH_BOOST,
                ETalents.ARMOR_BOOST,
                ETalents.HEALTH_BOOST_2,
                ETalents.ARMOR_BOOST_2,
                ETalents.OXYGEN_TRAINING,
                ETalents.PISTOL_DMG_BOOST,
                ETalents.QUICK_RESPAWN,
                ETalents.ADVANCED_OPERATOR,
                ETalents.SENSOR_DISH_CERT,
                ETalents.WPNS_RELOAD_SPEED,
                ETalents.WPNS_RELOAD_SPEED_2,
                ETalents.INC_STAMINA,
                ETalents.INC_MAX_WEIGHT,
                ETalents.INC_JETPACK,
                ETalents.INC_TURRET_ZOOM,
                ETalents.INC_HEALING_RATE,
                ETalents.INC_ENEMY_ATRIUM_HEAL,
                ETalents.INC_ALLOW_ENCUMBERED_SPRINT,
                ETalents.ANTI_RAD_INJECTION,
                ETalents.SCI_SCANNER_PICKUPS,
                ETalents.SCI_SCANNER_RESEARCH_MAT,
                ETalents.ITEM_UPGRADER_OPERATOR,
                ETalents.COMPONENT_UPGRADER_OPERATOR,
            },
            [(CharacterClass.General, CharacterSpecies.Human)] = new List<ETalents>(),
            [(CharacterClass.General, CharacterSpecies.Sylvassi)] = new List<ETalents>(),
            [(CharacterClass.General, CharacterSpecies.Robot)] = new List<ETalents>(),
            #endregion
            #region Captain Talents
            // Captain Talents
            [(CharacterClass.Captain, CharacterSpecies.General)] = new List<ETalents>
            {
                ETalents.CAP_CREW_SPEED_BOOST,
                ETalents.CAP_SCAVENGER,
                ETalents.CAP_ARMOR_BOOST,
                ETalents.CAP_DIPLOMACY,
                ETalents.CAP_INTIMIDATION,
                ETalents.CAP_SCREEN_DEFENSE,
                ETalents.CAP_SCREEN_SAFETY
            },
            [(CharacterClass.Captain, CharacterSpecies.Human)] = new List<ETalents>(),
            [(CharacterClass.Captain, CharacterSpecies.Sylvassi)] = new List<ETalents>(),
            [(CharacterClass.Captain, CharacterSpecies.Robot)] = new List<ETalents>(),
            #endregion
            #region Pilot Talents
            // Pilot Talents
            [(CharacterClass.Pilot, CharacterSpecies.General)] = new List<ETalents>
            {
                ETalents.PIL_SHIP_TURNING,
                ETalents.PIL_SHIP_SPEED,
                ETalents.PIL_REDUCE_SYS_DAMAGE,
                ETalents.PIL_REDUCE_HULL_DAMAGE,
                ETalents.PIL_KEEN_EYES
            },
            [(CharacterClass.Pilot, CharacterSpecies.Human)] = new List<ETalents>(),
            [(CharacterClass.Pilot, CharacterSpecies.Sylvassi)] = new List<ETalents>(),
            [(CharacterClass.Pilot, CharacterSpecies.Robot)] = new List<ETalents>(),
            #endregion
            #region Scientist Talents
            // Scientist Talents
            [(CharacterClass.Scientist, CharacterSpecies.General)] = new List<ETalents>
            {
                ETalents.SCI_HEAL_NEARBY,
                ETalents.SCI_SENSOR_BOOST,
                ETalents.SCI_SENSOR_HIDE,
                ETalents.SCI_FREQ_AMPLIFIER,
                ETalents.SCI_RESEARCH_SPECIALTY,
                ETalents.SCI_PROBE_COOLDOWN,
                ETalents.SCI_PROBE_XP
            },
            [(CharacterClass.Scientist, CharacterSpecies.Human)] = new List<ETalents>(),
            [(CharacterClass.Scientist, CharacterSpecies.Sylvassi)] = new List<ETalents>(),
            [(CharacterClass.Scientist, CharacterSpecies.Robot)] = new List<ETalents>(),
            #endregion
            #region Weapons Specialist Talents
            // Weapons Talents
            [(CharacterClass.Weapons, CharacterSpecies.General)] = new List<ETalents>
            {
                ETalents.WPNS_TURRET_BOOST,
                ETalents.WPNS_MISSILE_EXPERT,
                ETalents.WPNS_COOLING,
                ETalents.WPNS_REDUCE_PAWN_DMG,
                ETalents.WPNS_BOOST_CREW_TURRET_CHARGE,
                ETalents.WPNS_BOOST_CREW_TURRET_DAMAGE,
                ETalents.WPN_SCREEN_HACKER,
                ETalents.WPN_AMMO_BOOST,
                ETalents.E_TURRET_COOLING_CREW_WEAPONS
            },
            [(CharacterClass.Weapons, CharacterSpecies.Human)] = new List<ETalents>(),
            [(CharacterClass.Weapons, CharacterSpecies.Sylvassi)] = new List<ETalents>(),
            [(CharacterClass.Weapons, CharacterSpecies.Robot)] = new List<ETalents>(),
            #endregion
            #region Engineer Talents
            // Engineer Talents
            [(CharacterClass.Engineer, CharacterSpecies.General)] = new List<ETalents>
            {
                ETalents.ENG_COOLANT_MIX_CUSTOM,
                ETalents.ENG_FIRE_REDUCTION,
                ETalents.ENG_REPAIR_DRONES,
                ETalents.ENG_WARP_CHARGE_BOOST,
                ETalents.ENG_AUX_POWER_BOOST,
                ETalents.ENG_SALVAGE,
                ETalents.ENG_COREPOWERBOOST,
                ETalents.ENG_CORECOOLINGBOOST,
                ETalents.E_TURRET_COOLING_CREW_ENGINEER
            },
            [(CharacterClass.Engineer, CharacterSpecies.Human)] = new List<ETalents>(),
            [(CharacterClass.Engineer, CharacterSpecies.Sylvassi)] = new List<ETalents>(),
            [(CharacterClass.Engineer, CharacterSpecies.Robot)] = new List<ETalents>(),
            #endregion
        };
        public static List<ETalents> TalentsForClassSpecies(PLPlayer pLPlayer, int ClassID = -1)
        {
            int RaceID = -1;
            if (ClassID != -1 && pLPlayer != null)
            {
                RaceID = pLPlayer.RaceID; // 0 = Human, 1 = Sylvassi, 2 = Robot
            }
            List<ETalents> eTalents = new List<ETalents>();
            eTalents.AddRange(cachedTalents[(CharacterClass.General, CharacterSpecies.General)]);
            if (pLPlayer != null && ClassID != -1)
            {
                eTalents.AddRange(cachedTalents[((CharacterClass)ClassID, CharacterSpecies.General)]);
                eTalents.AddRange(cachedTalents[(CharacterClass.General, (CharacterSpecies)RaceID)]);
                eTalents.AddRange(cachedTalents[((CharacterClass)ClassID, (CharacterSpecies)RaceID)]);
            }
            return eTalents;
        }
    }
}
