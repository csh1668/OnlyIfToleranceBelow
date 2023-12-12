using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DrugPolicyEnhanced
{
    /// <summary>
    /// Patches for [KV] Save Storage, Outfit, Crafting, Drug, & Operation Settings [1.4]
    /// Because they uses their own format for save or load settings :(
    /// </summary>
    public static class Patches_SaveStorageSettings
    {
        [HarmonyPatch]
        public static class Patch_IOUtill_SavePolicySettings
        {
            public static bool Prepare()
            {
                return ModLister.AllInstalledMods.Any(x => x.Active && x.PackageId.ToLower().Contains("savestoragesettings.kv"));
            }
            public static MethodBase TargetMethod()
            {
                return AccessTools.Method("SaveStorageSettings.IOUtil:SavePolicySettings");
            }

            public static void Postfix(ref bool __result, DrugPolicy policy, FileInfo fi)
            {
                if (__result == false)
                    return;
                var linesOriginal = File.ReadAllLines(fi.FullName);
                var linesToAppend = new List<string> { "// Additional values from 'Only if tolerance below' mod." };
                foreach (var defName in linesOriginal.Where(x => x.StartsWith("defName:")).Select(x => x.Split(':')[1]))
                {
                    linesToAppend.Add($"|{defName}:{policy[ThingDef.Named(defName)].OnlyIfToleranceBelow()}");
                }

                File.AppendAllLines(fi.FullName, linesToAppend);
            }
        }

        [HarmonyPatch]
        public static class Patch_IOUtill_LoadPolicy
        {
            public static bool Prepare()
            {
                return ModLister.AllInstalledMods.Any(x => x.Active && x.PackageId.ToLower().Contains("savestoragesettings"));
            }
            public static MethodBase TargetMethod()
            {
                return AccessTools.Method("SaveStorageSettings.IOUtil:LoadPolicy");
            }

            public static void Postfix(DrugPolicy drugPolicy, FileInfo fi)
            {
                var lines = File.ReadAllLines(fi.FullName);
                foreach (var token in lines.Where(x => x.StartsWith("|")).Select(x => x.Substring(1)))
                {
                    Log.Message(token);
                    var splited = token.Split(':');
                    drugPolicy[ThingDef.Named(splited[0])].OnlyIfToleranceBelow() = float.Parse(splited[1]);
                }
            }
        }
    }
}
