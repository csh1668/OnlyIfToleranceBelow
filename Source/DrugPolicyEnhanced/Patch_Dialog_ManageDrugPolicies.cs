using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Prepatcher;
using RimWorld;
using UnityEngine;
using Verse;

namespace DrugPolicyEnhanced
{
    [HarmonyPatch(typeof(Dialog_ManageDrugPolicies))]
    public static class Patch_Dialog_ManageDrugPolicies
    {
        [PrepatcherField]
        public static extern ref float Sum(this Dialog_ManageDrugPolicies target);
        [PrepatcherField]
        public static extern ref float NewWidth(this Dialog_ManageDrugPolicies target);
        [HarmonyPatch("CalculateColumnsWidths"), HarmonyPostfix]
        public static void CalculateColumnsWidths_Postfix(Dialog_ManageDrugPolicies __instance, ref float addictionWidth, ref float allowJoyWidth, ref float scheduledWidth, ref float drugIconWidth, ref float drugNameWidth, ref float frequencyWidth, ref float moodThresholdWidth, ref float joyThresholdWidth, ref float takeToInventoryWidth)
        {
            frequencyWidth *= (0.35f - 0.06f) / 0.35f;
            moodThresholdWidth *= (0.15f - 0.03f) / 0.15f;
            joyThresholdWidth *= (0.15f - 0.03f) / 0.15f;

            __instance.NewWidth() = joyThresholdWidth;
            __instance.Sum() = addictionWidth + allowJoyWidth + scheduledWidth + drugIconWidth + drugNameWidth + frequencyWidth +
                      moodThresholdWidth + joyThresholdWidth + takeToInventoryWidth;
        }

        [HarmonyPatch("DoColumnLabels"), HarmonyPostfix]
        public static void DoColumnLabels_Postfix(Dialog_ManageDrugPolicies __instance, Rect rect)
        {
            Text.Anchor = TextAnchor.LowerCenter;
            Rect newRect = new Rect(__instance.Sum(), rect.y, __instance.NewWidth(), rect.height);
            Widgets.Label(newRect, "ToleranceThresholdColumnLabel".Translate());
            TooltipHandler.TipRegionByKey(newRect, "ToleranceThresholdColumnDesc");
            Text.Anchor = TextAnchor.UpperLeft;
        }

        [HarmonyPatch("DoEntryRow"), HarmonyPostfix]
        public static void DoEntryRow_Postfix(Dialog_ManageDrugPolicies __instance, Rect rect, DrugPolicyEntry entry)
        {
            Text.Anchor = TextAnchor.LowerCenter;
            if (entry.drug.IsAddictiveDrug && (entry.allowedForJoy || entry.allowScheduled))
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

                entry.OnlyIfToleranceBelow() = Widgets.HorizontalSlider_NewTemp(
                    new Rect(__instance.Sum(), rect.y, __instance.NewWidth(), rect.height).ContractedBy(4f), entry.OnlyIfToleranceBelow(), 0f, 1f, true,
                    label, null, null, -1f);
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
