using HarmonyLib;
using Prepatcher;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DrugPolicyEnhanced
{
    public static class Patches
    {
        internal static Dictionary<string, bool> HasToleranceDict = new Dictionary<string, bool>();

        internal static void InitDict()
        {
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.IsDrug))
            {
                if (thingDef.IsAddictiveDrug && DrugStatsUtility.GetToleranceGiver(thingDef)?.hediffDef != null)
                    HasToleranceDict[thingDef.defName] = true;
                else
                    HasToleranceDict[thingDef.defName] = false;
            }
        }
        [PrepatcherField]
        public static extern ref float Sum(this Dialog_ManageDrugPolicies target);
        [PrepatcherField]
        public static extern ref float NewWidth(this Dialog_ManageDrugPolicies target);
        [PrepatcherField]
        [Prepatcher.DefaultValue(1.0f)]
        public static extern ref float OnlyIfToleranceBelow(this DrugPolicyEntry target);
        [HarmonyPatch(typeof(Dialog_ManageDrugPolicies))]
        public static class Patch_Dialog_ManageDrugPolicies
        {

            [HarmonyPatch("CalculateColumnsWidths"), HarmonyPostfix]
            public static void CalculateColumnsWidths_Postfix(Dialog_ManageDrugPolicies __instance, ref float addictionWidth, ref float allowJoyWidth, ref float scheduledWidth, ref float drugIconWidth, ref float drugNameWidth, ref float frequencyWidth, ref float moodThresholdWidth, ref float joyThresholdWidth, ref float takeToInventoryWidth)
            {
#if v14
                frequencyWidth *= (0.35f - 0.06f) / 0.35f;
                moodThresholdWidth *= (0.15f - 0.03f) / 0.15f;
                joyThresholdWidth *= (0.15f - 0.03f) / 0.15f;

                __instance.NewWidth() = joyThresholdWidth;
                __instance.Sum() = addictionWidth + allowJoyWidth + scheduledWidth + drugIconWidth + drugNameWidth + frequencyWidth +
                                   moodThresholdWidth + joyThresholdWidth + takeToInventoryWidth;
#endif
#if v15
                frequencyWidth *= (0.3f - 0.06f) / 0.3f;
                moodThresholdWidth *= (0.15f - 0.03f) / 0.15f;
                joyThresholdWidth *= (0.15f - 0.03f) / 0.15f;

                __instance.NewWidth() = joyThresholdWidth;
                __instance.Sum() = addictionWidth + allowJoyWidth + scheduledWidth + drugIconWidth + drugNameWidth + frequencyWidth +
                                   moodThresholdWidth + joyThresholdWidth + takeToInventoryWidth;
#endif

            }

            [HarmonyPatch("DoColumnLabels"), HarmonyPostfix]
            public static void DoColumnLabels_Postfix(Dialog_ManageDrugPolicies __instance, Rect rect)
            {
                Text.Anchor = TextAnchor.LowerCenter;
#if v14
                Rect newRect = new Rect(__instance.Sum(), rect.y, __instance.NewWidth(), rect.height);
#endif
#if v15
                Rect newRect = new Rect(__instance.Sum() + 2 * __instance.NewWidth(), rect.y, __instance.NewWidth(), rect.height);
#endif
                Widgets.Label(newRect, "ToleranceThresholdColumnLabel".Translate());
                TooltipHandler.TipRegionByKey(newRect, "ToleranceThresholdColumnDesc");
                Text.Anchor = TextAnchor.UpperLeft;
            }

            [HarmonyPatch("DoEntryRow"), HarmonyPostfix]
            public static void DoEntryRow_Postfix(Dialog_ManageDrugPolicies __instance, Rect rect, DrugPolicyEntry entry)
            {
                Text.Anchor = TextAnchor.LowerCenter;
                if (entry.drug.IsAddictiveDrug && (entry.allowedForJoy || entry.allowScheduled) 
                                               && HasToleranceDict.TryGetValue(entry.drug.defName, out bool tmp) && tmp)
                {
                    string label;
                    if (entry.OnlyIfToleranceBelow() < 1f)
                    {
                        label = entry.OnlyIfToleranceBelow().ToStringPercent();
                    }
                    else
                    {
                        label = "NoDrugUseRequirement".Translate();
                    }

                    entry.OnlyIfToleranceBelow() = Widgets.HorizontalSlider(
                        new Rect(__instance.Sum(), rect.y, __instance.NewWidth(), rect.height).ContractedBy(4f), entry.OnlyIfToleranceBelow(), 0f, 1f, true,
                        label, null, null, -1f);
                }
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }

        [HarmonyPatch(typeof(Pawn_DrugPolicyTracker))]
        public static class Patch_Pawn_DrugPolicyTracker
        {
            [HarmonyPatch("AllowedToTakeScheduledNow"), HarmonyPostfix]
            public static void AllowedToTakeScheduledNow_Postfix(Pawn_DrugPolicyTracker __instance, ref bool __result, ThingDef thingDef)
            {
                if (__result == false || (!HasToleranceDict.TryGetValue(thingDef.defName, out bool tmp) || !tmp))
                    return;
                var giver = DrugStatsUtility.GetToleranceGiver(thingDef);
                if (giver?.hediffDef == null)
                {
                    Log.Warning($"Only if tolerance below: Drug: {thingDef.defName} should have but doesn't have Tolerance Giver.");
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

        [HarmonyPatch(typeof(JoyGiver_Ingest))]
        public static class Patch_JoyGiver_Ingest
        {

            [HarmonyPatch("CanIngestForJoy"), HarmonyPostfix]
            public static void CanIngestForJoy_Postfix(ref bool __result, Pawn pawn, Thing t)
            {
                if (__result == false || !t.def.IsDrug || pawn.story?.traits.DegreeOfTrait(TraitDefOf.DrugDesire) > 0 || pawn.InMentalState)
                    return;
                if (!HasToleranceDict.TryGetValue(t.def.defName, out bool tmp) || !tmp)
                    return;
                var giver = DrugStatsUtility.GetToleranceGiver(t.def);
                if (giver?.hediffDef == null)
                {
                    Log.Warning($"Only if tolerance below: Drug: {t.def.defName} should have but doesn't have Tolerance Giver.");
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

        [HarmonyPatch(typeof(DrugPolicyEntry))]
        public static class Patch_DrugPolicyEntry
        {
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
}
