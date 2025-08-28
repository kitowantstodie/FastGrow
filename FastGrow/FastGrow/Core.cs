using HarmonyLib;
using System.Collections.Generic;
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
        /// Scale MinPass growth result by 1/multiplier to ensure exact perceived speed.
        /// This avoids modifying GrowthTime and works for existing plants.
        /// </summary>
        [HarmonyPatch(typeof(Plant), "MinPass")]
        private class PlantMinPassScalePatch
        {
            private static readonly Dictionary<int, float> _previousProgressByPlantId = new Dictionary<int, float>();

            private static void Prefix(Plant __instance)
            {
                _previousProgressByPlantId[__instance.GetInstanceID()] = __instance.NormalizedGrowthProgress;
            }

            private static void Postfix(Plant __instance)
            {
                int id = __instance.GetInstanceID();
                if (!_previousProgressByPlantId.TryGetValue(id, out float previous))
                    return;
                _previousProgressByPlantId.Remove(id);

                if (__instance.NormalizedGrowthProgress >= 1f)
                    return;

                float delta = __instance.NormalizedGrowthProgress - previous;
                if (delta == 0f)
                    return;

                float safeMultiplier = Mathf.Clamp(growthMultiplier.Value, 0.01f, 100f);
                if (float.IsNaN(safeMultiplier) || float.IsInfinity(safeMultiplier))
                    safeMultiplier = 1f;

                float scale = 1f / safeMultiplier; // 0.05 -> 20x
                if (Mathf.Approximately(scale, 1f))
                    return;

                float desiredDelta = delta * scale;
                float newProgress = Mathf.Clamp(previous + desiredDelta, 0f, 1f);
                if (!Mathf.Approximately(newProgress, __instance.NormalizedGrowthProgress))
                {
                    __instance.SetNormalizedGrowthProgress(newProgress);
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
