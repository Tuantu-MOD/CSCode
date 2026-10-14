using Harmony;
using System.Reflection;
using UnityEngine;

namespace Tuantu
{
    public class Harmony_Patch
    {
        public static string packageid = "Tuantu_MOD";
        public static float energyMul;
        private static float _addhp;
        private static float _addmental;

        public Harmony_Patch()
        {
            HarmonyInstance harmonyinstance = HarmonyInstance.Create(packageid);
            MethodInfo method = typeof(Harmony_Patch).GetMethod("MultipleEnergy");
            harmonyinstance.Patch(typeof(EnergyModel).GetMethod("AddEnergy", AccessTools.all), new HarmonyMethod(method), null);
            method = typeof(Harmony_Patch).GetMethod("ResetMultiple");
            harmonyinstance.Patch(original: typeof(GameManager).GetMethod("StartStage", AccessTools.all), prefix: new HarmonyMethod(method), postfix: null);
            method = typeof(Harmony_Patch).GetMethod("AddRecoverHP");
            harmonyinstance.Patch(original: typeof(Sefira).GetMethod("get_regenerationValue", AccessTools.all), postfix: new HarmonyMethod(method), prefix: null);
            method = typeof(Harmony_Patch).GetMethod("AddRecoverMental");
            harmonyinstance.Patch(original: typeof(Sefira).GetMethod("get_regenerationMentalValue", AccessTools.all), postfix: new HarmonyMethod(method), prefix: null);
        }

        public static void MultipleEnergy(ref float added)
        {
            added += added * energyMul;
        }

        public static void ResetMultiple()
        {
            energyMul = 0;
            _addmental = 0;
            _addhp = 0;
        }

        public static void AddRecoverHP(ref float __result)
        {
            __result += _addhp;
        }

        public static void AddRecoverMental(ref float __result)
        {
            __result += _addmental;
        }

        public static void AddRecoverHPMental(float addhp, float addmental)
        {
            _addhp += addhp;
            _addmental += addmental;
        }

    }
}