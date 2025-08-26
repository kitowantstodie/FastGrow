using HarmonyLib;
using Il2CppScheduleOne.Growing;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(FastGrow.Core), "FastGrow", "1.0.3", "Xaender & Bars")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace FastGrow
{
    public class Core : MelonMod
    {
        private static MelonPreferences_Entry<float> growthMultiplier;

        /// <summary>
        /// Patches the MinPass method to apply growth multiplier safely
        /// </summary>
        [HarmonyPatch(typeof(Plant), "MinPass")]
        private class PlantMinPassPatch
        {
            private static float _originalMultiplier;

            private static void Prefix(Plant __instance)
            {
                if (__instance.Pot == null)
                    return;

                // Store the original multiplier and apply our growth multiplier
                _originalMultiplier = __instance.Pot.GrowSpeedMultiplier;
                float safeMultiplier = Mathf.Clamp(growthMultiplier.Value, 0.01f, 100f);

                // Ensure we don't create invalid multipliers
                if (float.IsNaN(safeMultiplier) || float.IsInfinity(safeMultiplier))
                {
                    safeMultiplier = 1.0f;
                }

                __instance.Pot.GrowSpeedMultiplier *= safeMultiplier;
            }

            private static void Postfix(Plant __instance)
            {
                // Safety check and restore the original multiplier
                if (__instance.Pot != null)
                {
                    __instance.Pot.GrowSpeedMultiplier = _originalMultiplier;
                }
            }
        }

        public override void OnInitializeMelon()
        {
            var category = MelonPreferences.CreateCategory("FastGrow", "FastGrow Settings");
            growthMultiplier = category.CreateEntry(
                "GrowthMultiplier",
                0.25f,
                "Growth Multiplier",
                "Controls plant growth speed. Lower values = faster growth.\n" +
                "• 0.25 = 4x faster growth\n" +
                "• 1.0 = normal speed\n" +
                "• 2.0 = 2x slower growth\n" +
                "Range: 0.01 (100x faster) to 100.0 (100x slower)"
            );

            // Validate and clamp the multiplier value so it doesnt cause weird values
            ValidateGrowthMultiplier();

            LoggerInstance.Msg($"FastGrow initialized with multiplier: {growthMultiplier.Value}");
        }

        public override void OnPreferencesSaved(string filePath)
        {
            // Re-validate the multiplier again when preferences are saved
            ValidateGrowthMultiplier();
        }

        private void ValidateGrowthMultiplier()
        {
            float currentValue = growthMultiplier.Value;
            float clampedValue = Mathf.Clamp(currentValue, 0.01f, 100f);

            if (Mathf.Approximately(currentValue, clampedValue))
                return;

            LoggerInstance.Warning($"GrowthMultiplier value {currentValue} is out of range. Clamping to {clampedValue}");
            growthMultiplier.Value = clampedValue;
            MelonPreferences.Save();
        }
    }
}
