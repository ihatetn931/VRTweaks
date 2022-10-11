/*using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
//Capture all the biomes the user has been in and make
namespace VRTweaks
{
    [System.Serializable]
    public static class HudData
    {
        public static float HudWidth;
        public static float HudScale;
    }
    public class Json
    {
        private static readonly string hudconfigFile = "hudconfig.json";
        public static string hudConfigPath = Path.Combine(GetAssemblyDirectory, hudconfigFile);

        private static string GetAssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public class HudData
        {
            public  float HudWidth { get; set; }
            public  float HudScale { get; set; }
        }


        public static void CreateJson()
        {
            var settings = new JsonSerializerSettings
            {
                CheckAdditionalContent = true,
                Formatting = Formatting.Indented
            };

            var data = new HudData
            {
                HudScale = Loader.VRHudScale,
                HudWidth = Loader.VRHudWidth,
            };

            string json = JsonConvert.SerializeObject(data, settings);
            File.AppendAllText(hudConfigPath, json);
        }

        public static void Save()
        {
            if (File.Exists(hudConfigPath))
            {
                var settings = new JsonSerializerSettings { CheckAdditionalContent = true, Formatting = Formatting.Indented };
                var read = File.ReadAllText(hudConfigPath);
                var json = JsonConvert.DeserializeObject<HudData>(read);
                json.HudScale = Loader.VRHudScale;
                json.HudWidth = Loader.VRHudWidth;
                string jsonString1 = JsonConvert.SerializeObject(json, settings);
                File.WriteAllText(hudConfigPath, jsonString1);
            }

        }

        public static void LoadHudConfig()
        {
            if (File.Exists(hudConfigPath))
            {
                var read = File.ReadAllText(hudConfigPath);
                var json = JsonConvert.DeserializeObject<HudData>(read);
                Debug.Log("[VRTweaks] Hud Scale: " + json.HudScale);
                Debug.Log("[VRTweaks] Hud Width: " + json.HudWidth);
                Loader.VRHudScale = json.HudScale;
                Loader.VRHudWidth = json.HudWidth;
            }
            else
            {
                CreateJson();
                return;
            }
        }

        public static void SaveHud()
        {
            if (!File.Exists(hudConfigPath))
            {
                CreateJson();
                return;
            }
            Save();
        }
    }
}*/