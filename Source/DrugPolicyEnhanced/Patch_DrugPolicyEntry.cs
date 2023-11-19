using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Prepatcher;
using RimWorld;
using Verse;

namespace DrugPolicyEnhanced
{
    [HarmonyPatch(typeof(DrugPolicyEntry))]
    public static class Patch_DrugPolicyEntry
    {
        [PrepatcherField]
        [Prepatcher.DefaultValue(1.0f)]
        public static extern ref float OnlyIfToleranceBelow(this DrugPolicyEntry target);

        //private static float OnlyIfToleranceBelowInitializer(DrugPolicyEntry target)
        //{
        //    if (target.drug.IsPleasureDrug)
        //    {
        //        var giver = DrugStatsUtility.tolerance()
        //    }
        //    else
        //    {
        //        return 1f;
        //    }
        //}

        [HarmonyPatch("CopyFrom"), HarmonyPostfix]
        public static void CopyFrom_Postfix(DrugPolicyEntry __instance, DrugPolicyEntry other)
        {
            __instance.OnlyIfToleranceBelow() = other.OnlyIfToleranceBelow();
        }

        [HarmonyPatch("ExposeData"), HarmonyPostfix]
        public static void ExposeData_Postfix(DrugPolicyEntry __instance)
        {
            Scribe_Values.Look<float>(ref __instance.OnlyIfToleranceBelow(), "onlyIfToleranceBelow", 1f, false);
        }
    }
}
