using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace DrugPolicyEnhanced
{
    [HarmonyPatch(typeof(JoyGiver_Ingest))]
    public static class Patch_JoyGiver_Ingest
    {

        [HarmonyPatch("CanIngestForJoy"), HarmonyPostfix]
        public static void CanIngestForJoy_Postfix(ref bool __result, Pawn pawn, Thing t)
        {
            if (__result == false || !t.def.IsDrug || pawn.story?.traits.DegreeOfTrait(TraitDefOf.DrugDesire) > 0 || pawn.InMentalState)
                return;
            var giver = DrugStatsUtility.GetToleranceGiver(t.def);
            if (giver?.hediffDef == null)
            {
                Log.Warning($"Drug: {t.def.defName} should have but doesn't have Tolerance Giver.");
                return;
            }
            var hediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(giver.hediffDef);
            var drugPolicyEntry = pawn.drugs?.CurrentPolicy[t.def];

            if (hediff != null && drugPolicyEntry != null && hediff.Severity >= drugPolicyEntry.OnlyIfToleranceBelow())
            {
                __result = false;
            }
        }
    }
}
