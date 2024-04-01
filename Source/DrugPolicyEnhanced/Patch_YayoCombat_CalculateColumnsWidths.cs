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
        public static bool Prepare()
        {
            return ModLister.AllInstalledMods.Any(x => x.Active && x.PackageId.ToLower().Contains("yayoscombat3"));
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("yayoCombat.patch_Dialog_ManageDrugPolicies_DoEntryRow:CalculateColumnsWidths");
        }

        public static void Postfix(ref float addictionWidth, ref float allowJoyWidth,
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
        }
    }
}
