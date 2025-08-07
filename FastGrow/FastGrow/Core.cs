using HarmonyLib;
using Il2CppScheduleOne.Growing;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityObject = UnityEngine.Object;

[assembly: MelonInfo(typeof(FastGrow.Core), "FastGrow", "1.0.1", "Xaender")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace FastGrow
{
    public class Core : MelonMod
    {
        private static MelonPreferences_Entry<float> growthMultiplier;

        [HarmonyPatch(typeof(Plant), "Initialize")]
        private class PlantPatch
        {
            private static void Postfix(Plant __instance)
            {
                __instance.GrowthTime = (int)(__instance.GrowthTime * growthMultiplier.Value);
            }
        }

        public override void OnInitializeMelon()
        {

            var category = MelonPreferences.CreateCategory("FastGrow", "FastGrow Settings");
            growthMultiplier = category.CreateEntry(
                "GrowthMultiplier",
                0.25f,
                "Growth Multiplier",
                "Lower = faster. 0.25 = 4x faster."
            );

            UnityObject.DontDestroyOnLoad(new GameObject("FastGrow"));

            var harmony = new HarmonyLib.Harmony("com.xaender.fastgrow");
            harmony.PatchAll();

            MelonPreferences.Save();
        }

        public override void OnDeinitializeMelon()
        {
            LoggerInstance.Msg("FastGrow deinitialized.");
        }
    }
}
