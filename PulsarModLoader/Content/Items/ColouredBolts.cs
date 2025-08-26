using HarmonyLib;
using PulsarModLoader;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PulsarModLoader.Content.Items
{
    // Code to add colourable bolts.
    // Implemented with the following:
    /*
       protected override GameObject CreateBoltGO()
       {
           GameObject Bolt = base.CreateBoltGO();
           Bolt.GetComponent<PLBolt>().AddData(new BoltColourData(this.MyGunInstance.MuzzleFlash.main.startColor.color));
           return Bolt;
       }
    */
    [Serializable]
    public class BoltColourData
    {
        public Color32 _colour;
        public BoltColourData(Color32 pColour)
        {
            _colour = pColour;
        }
    }

    public static class BoltColourPatch
    {
        private static readonly ConditionalWeakTable<PLBolt, BoltColourData> data = new ConditionalWeakTable<PLBolt, BoltColourData>();
        public static BoltColourData GetAdditionalData(this PLBolt pBolt)
        {
            if (data.TryGetValue(pBolt, out BoltColourData ret)) return ret;
            return null;
        }
        public static void AddData(this PLBolt pBolt, BoltColourData pValue)
        {
            try
            {
                data.Add(pBolt, pValue);
            }
            catch { }
        }
    }
    [HarmonyPatch(typeof(PLBolt))]
    public class PathPLBolt
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void Start(ref PLBolt __instance)
        {
            BoltColourData Data = __instance.GetAdditionalData();
            if (Data == null) return;
            Color32 c = Data._colour; //new Color32((byte)PathPLBolt.rand.Next(0, 255), (byte)PathPLBolt.rand.Next(0, 255), (byte)PathPLBolt.rand.Next(0, 255), byte.MaxValue);
            if (__instance is PLPierceBeamBolt)
            {
                PLPierceBeamBolt plpierceBeamBolt = __instance as PLPierceBeamBolt;
                if (plpierceBeamBolt.ShotMaterials != null)
                {
                    Material[] array = plpierceBeamBolt.ShotMaterials;
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i].SetColor("_LaserColor", c);
                    }
                }
                if (plpierceBeamBolt.AltShotMaterials != null)
                {
                    Material[] array = plpierceBeamBolt.AltShotMaterials;
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i].SetColor("_LaserColor", c);
                    }
                    return;
                }
            }
            else
            {
                __instance.LightGO.GetComponent<Light>().color = c;
                ParticleSystem.MainModule t = __instance.LightGO.GetComponent<ParticleSystem>().main;
                t.startColor = new ParticleSystem.MinMaxGradient(c, c);
                __instance.LightGO.GetComponent<TrailRenderer>().startColor = c;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("CreateBoltHitGO")]
        public static void CreateBoltHitGO(PLBolt __instance, ref GameObject __result)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            BoltColourData Data = __instance.GetAdditionalData();
            if (Data == null) return;
            Color32 c = Data._colour;
            ParticleSystem[] componentsInChildren;
            if (__instance is PLPierceBeamBolt)
            {
                componentsInChildren = __result.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].startColor = c;
                }
                Light[] componentsInChildren2 = __result.GetComponentsInChildren<Light>();
                for (int i = 0; i < componentsInChildren2.Length; i++)
                {
                    componentsInChildren2[i].color = c;
                }
                return;
            }
            Color color = __instance.LightGO.GetComponent<Light>().color;
            __result.GetComponent<ParticleSystem>().startColor = color;
            componentsInChildren = __result.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].startColor = color;
            }
            __result.GetComponentInChildren<Light>().color = color;
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
