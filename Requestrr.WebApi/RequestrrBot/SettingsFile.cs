using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Requestrr.WebApi.RequestrrBot
{
    public static class SettingsFile
    {
        private static object _lock = new object();

        private const string _settingsFile = "settings.json";
        private static string _settingsFolder = "config";
        private static string _settingsFolderLocation = "./";

        public static bool CommandLineSettings { get; set; } = false;
        public static string SettingsFolder {
            get => Path.Combine(_settingsFolderLocation, _settingsFolder);
            set
            {
                string valueData = Path.IsPathRooted(value) ? value : Program.CombindPath(value).Replace("\\", "/");
                string fullPath = valueData.Length == 0 ? string.Empty : (valueData[valueData.Length - 1] == '/' ? valueData.Substring(0, valueData.Length - 1) : valueData);
                int lastSlash = fullPath.LastIndexOf("/");
                if(lastSlash != -1)
                    lastSlash++;

                _settingsFolderLocation = lastSlash == -1 ? fullPath : fullPath.Substring(0, lastSlash);
                _settingsFolder = lastSlash == -1 ? string.Empty : fullPath.Substring(lastSlash, fullPath.Length - lastSlash);
            }
        }
        public static string FilePath { get => Path.Combine(SettingsFolder, _settingsFile); }

        public static string SettingPath { get => _settingsFolderLocation; }


        public static dynamic _cachedSettings = null;

        public static dynamic Read()
        {
            dynamic settings = null;

            lock (_lock)
            {
                if(_cachedSettings == null)
                {
                    _cachedSettings = JObject.Parse(File.ReadAllText(FilePath));
                }

                settings = _cachedSettings;
            }

            return settings;
        }

        public static void Write(Action<dynamic> modifyFunc)
        {
            lock (_lock)
            {
                modifyFunc(_cachedSettings);
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(_cachedSettings));
            }
        }
    }
}