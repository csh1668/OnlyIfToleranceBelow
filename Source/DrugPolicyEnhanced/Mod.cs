using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace DrugPolicyEnhanced
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            var h = new Harmony("seohyeon.drugpolicyenhanced");
            h.PatchAll();

            LongEventHandler.QueueLongEvent(() =>
            {
                Patches.InitDict();
                Log.Message("Only if tolerance below: dict init done.");
            }, "seohyeon.drugpolicyenhanced", false, null);
        }
    }
}
