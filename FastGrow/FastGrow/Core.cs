using System;
using System.IO;
using HarmonyLib;
using Il2CppScheduleOne.Growing;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using NewtonsoftFormatting = Newtonsoft.Json.Formatting;
using NewtonsoftJson = Newtonsoft.Json.JsonConvert;
using UnityObject = UnityEngine.Object;

[assembly: MelonInfo(typeof(FastGrow.Core), "FastGrow", "1.0.0", "Xaender")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace FastGrow
{
    public class Core : MelonMod
    {
        private static float growthMultiplier = 0.25f;

        [HarmonyPatch(typeof(Plant), "Initialize")]
        private class PlantPatch
        {
            private static void Postfix(Plant __instance)
            {
                __instance.GrowthTime = (int)(__instance.GrowthTime * growthMultiplier);
            }
        }

        public override void OnInitializeMelon()
        {
            UnityObject.DontDestroyOnLoad(new GameObject("FastGrow"));
            LoggerInstance.Msg("FastGrow initialized.");
            LoadConfig();

            var harmony = new HarmonyLib.Harmony("com.xaender.fastgrow");
            harmony.PatchAll();
        }

        private void LoadConfig()
        {
            try
            {
                string path = Path.Combine(MelonEnvironment.UserDataDirectory, "FastGrowConfig.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var config = NewtonsoftJson.DeserializeObject<Config>(json);
                    growthMultiplier = config.GrowthMultiplier;
                    LoggerInstance.Msg($"Config loaded. Growth multiplier set to {growthMultiplier}.");
                }
                else
                {
                    var config = new Config { GrowthMultiplier = 0.25f };
                    File.WriteAllText(path, NewtonsoftJson.SerializeObject(config, NewtonsoftFormatting.Indented));
                    growthMultiplier = config.GrowthMultiplier;
                    LoggerInstance.Msg("Default config created with GrowthMultiplier = 0.25 (4x faster).");
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("Error loading config: " + ex.Message);
                growthMultiplier = 0.25f;
            }
        }

        private class Config
        {
            public float GrowthMultiplier { get; set; }
        }

        public override void OnDeinitializeMelon()
        {
            LoggerInstance.Msg("FastGrow deinitialized.");
        }
    }
}
