using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DrugPolicyEnhanced
{
    /// <summary>
    /// Why?
    /// Since yayo combat replaces Dialog_ManageDrugPolicies:DoEntryRow to their own method,
    /// So I have to make a patch for yayocombat :(
    /// </summary>
    [HarmonyPatch]
    public static class Patch_YayoCombat_CalculateColumnsWidths
    {
        private static bool rowLayoutPending;
        private static float rowSliderX;
        private static float rowSliderWidth;

        public static bool Prepare()
        {
            if (!ModLister.AllInstalledMods.Any(x => x.Active && x.PackageId.ToLower().Contains("yayoscombat3")))
                return false;
            if (TargetMethod() == null)
            {
                Log.WarningOnce("Only if tolerance below: Yayo's Combat 3 is active, but its drug policy column method was not found. The tolerance column may overlap with it.", 0x1E9A7C3);
                return false;
            }
            return true;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("yayoCombat.HarmonyPatches.Dialog_ManageDrugPolicies_DoEntryRow:calculateColumnsWidths")
                   ?? AccessTools.Method("yayoCombat.patch_Dialog_ManageDrugPolicies_DoEntryRow:CalculateColumnsWidths");
        }

        internal static bool TryConsumeRowLayout(out float x, out float width)
        {
            x = rowSliderX;
            width = rowSliderWidth;
            var pending = rowLayoutPending;
            rowLayoutPending = false;
            return pending;
        }

        public static void Postfix(Rect rect, ref float addictionWidth, ref float allowJoyWidth,
            ref float scheduledWidth, ref float drugNameWidth, ref float frequencyWidth, ref float moodThresholdWidth,
            ref float joyThresholdWidth, ref float takeToInventoryWidth)
        {
#if v14
            frequencyWidth *= (0.35f - 0.06f) / 0.35f;
            moodThresholdWidth *= (0.15f - 0.03f) / 0.15f;
            joyThresholdWidth *= (0.15f - 0.03f) / 0.15f;
#endif
#if v15
            frequencyWidth *= (0.30f - 0.06f) / 0.30f;
            moodThresholdWidth *= (0.15f - 0.03f) / 0.15f;
            joyThresholdWidth *= (0.15f - 0.03f) / 0.15f;
#endif
            rowSliderX = rect.x + drugNameWidth + takeToInventoryWidth + addictionWidth + allowJoyWidth + scheduledWidth +
                         frequencyWidth + moodThresholdWidth + joyThresholdWidth;
            rowSliderWidth = joyThresholdWidth;
            rowLayoutPending = true;
        }
    }
}
