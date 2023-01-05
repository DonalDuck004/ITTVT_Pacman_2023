using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace PacManWPF.Game
{

    class RuntimeSettings {
        [JsonInclude]
        public double Volume { get; internal set; } = 1;
        [JsonInclude]
        public int GraphicMode { get; internal set; } = 0;

        [JsonInclude]
        public bool AnimationsEnabled { get; internal set; } = true;
    }

    static class RuntimeSettingsHandler
    {
        public static RuntimeSettings INSTANCE { get; private set; }
        public const string CONFIG_NAME = "Config.json";

        public static double Volume => INSTANCE.Volume;
        public static double XAML_Volume => INSTANCE.Volume * 100;
        public static int GraphicMode => INSTANCE.GraphicMode;
        public static double XAML_GraphicMode => INSTANCE.GraphicMode;
        public static bool AnimationsEnabled => INSTANCE.AnimationsEnabled;
        public static bool XAML_AnimationsEnabled => INSTANCE.AnimationsEnabled;

        static RuntimeSettingsHandler(){
            RuntimeSettingsHandler.INSTANCE = RuntimeSettingsHandler.Load();
        }


        public static void SetVolume(int val) => SetVolume((double)val / 100);

        public static void SetVolume(double val)
        {
            if (val < 0 || val > 1)
                throw new ArgumentException("Volume must be between 0 and 1");
            
            RuntimeSettingsHandler.INSTANCE.Volume = val;
            RuntimeSettingsHandler.DumpToFile();
        }

        public static void SetGraphic(int val)
        {
            if (val < 0 || val >= UIWindow.GraphicOptions.Length)
                throw new ArgumentException("Graphic must be between 0 and 1");

            RuntimeSettingsHandler.INSTANCE.GraphicMode = val;
            RuntimeSettingsHandler.DumpToFile();
        }

        public static void SetAnimations(bool value)
        {
            RuntimeSettingsHandler.INSTANCE.AnimationsEnabled = value;
            RuntimeSettingsHandler.DumpToFile();
        }

        public static RuntimeSettings Load()
        {
            if (!File.Exists(CONFIG_NAME))
                return new();

            return JsonSerializer.Deserialize<RuntimeSettings>(File.ReadAllText(CONFIG_NAME))!; // The file has small size, so unbuffered reading is not a bad idea
        }

        public static void DumpToFile()
        {
            string json = JsonSerializer.Serialize(RuntimeSettingsHandler.INSTANCE, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(CONFIG_NAME, json, Encoding.UTF8);
        }

    }
}
