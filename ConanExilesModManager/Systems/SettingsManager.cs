using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ConanExilesModManager
{
    public static class SettingsManager
    {
        private static readonly string SettingsFilePath = "CEMM_Settings.json";

        public static Dictionary<string, string> LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            else
            {
                return new Dictionary<string, string>();
            }
        }

        public static bool SettingsFileExists()
        {
            return File.Exists(SettingsFilePath);
        }
        public static string SelectFolder()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true // Ensures the dialog is for folder selection
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) return dialog.FileName;

            return null;
        }
    

        public static void SaveSetting(string key, string value)
        {
            var settings = LoadSettings();
            settings[key] = value;
            SaveSettings(settings);
        }

        public static void SaveSettings(Dictionary<string, string> settings)
        {
            File.WriteAllText(SettingsFilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public static void SaveWindowPositionAndSize(Window window)
        {
            var settings = LoadSettings();
            settings["WindowTop"] = window.Top.ToString();
            settings["WindowLeft"] = window.Left.ToString();
            settings["WindowHeight"] = window.Height.ToString();
            settings["WindowWidth"] = window.Width.ToString();
            SaveSettings(settings);
        }

        public static void LoadWindowPositionAndSize(Window window)
        {
            var settings = LoadSettings();
            if (settings.TryGetValue("WindowTop", out var top))
                window.Top = double.Parse(top);
            if (settings.TryGetValue("WindowLeft", out var left))
                window.Left = double.Parse(left);
            if (settings.TryGetValue("WindowHeight", out var height))
                window.Height = double.Parse(height);
            if (settings.TryGetValue("WindowWidth", out var width))
                window.Width = double.Parse(width);
        }
    }
}
