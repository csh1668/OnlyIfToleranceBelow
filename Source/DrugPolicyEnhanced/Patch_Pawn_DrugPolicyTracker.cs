using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace DrugPolicyEnhanced
{
    [HarmonyPatch(typeof(Pawn_DrugPolicyTracker))]
    public static class Patch_Pawn_DrugPolicyTracker
    {
        [HarmonyPatch("AllowedToTakeScheduledNow"), HarmonyPostfix]
        public static void AllowedToTakeScheduledNow_Postfix(Pawn_DrugPolicyTracker __instance, ref bool __result, ThingDef thingDef)
        {
            if (__result == false)
                return;
            var giver = DrugStatsUtility.GetToleranceGiver(thingDef);
            if (giver?.hediffDef == null)
            {
                Log.Warning($"Drug: {thingDef.defName} should have but doesn't have Tolerance Giver.");
                return;
            }
            var hediff = __instance.pawn.health?.hediffSet?.GetFirstHediffOfDef(giver.hediffDef);
            var drugPolicyEntry = __instance.CurrentPolicy[thingDef];

            if (hediff != null && drugPolicyEntry != null && hediff.Severity >= drugPolicyEntry.OnlyIfToleranceBelow())
            {
                __result = false;
            }
        }
    }
}
