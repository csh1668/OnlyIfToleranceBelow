using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugPolicyEnhanced
{
    /// <summary>
    /// Specific uses, ex) for other mods, using reflection to accessing prepatcher fields added by this mod.
    /// </summary>
    public static class ExposedPrepatcherFields
    {
        public static void Set_Sum(Dialog_ManageDrugPolicies target, float value) => target.Sum() = value;
        public static float Get_Sum(Dialog_ManageDrugPolicies taget) => taget.Sum();
        public static void Set_NewWidth(Dialog_ManageDrugPolicies target, float value) => target.NewWidth() = value;
        public static float Get_NewWidth(Dialog_ManageDrugPolicies target) => target.NewWidth();
        public static void Set_OnlyIfToleranceBelow(DrugPolicyEntry target, float value) => target.OnlyIfToleranceBelow() = value;
        public static float Get_OnlyIfToleranceBelow(DrugPolicyEntry target) => target.OnlyIfToleranceBelow();
    }
}
