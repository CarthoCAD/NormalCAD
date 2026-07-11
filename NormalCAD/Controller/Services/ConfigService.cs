using System;
using System.IO;
using System.Text.Json;

namespace NormalCAD.Controller.Services
{
    public class AppConfig
    {
        public string Language { get; set; } = "";
        public string Theme { get; set; } = "Dark";
    }

    public static class ConfigService
    {
        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true
        };

        private static AppConfig? _current;

        public static AppConfig Current => _current ??= Load();

        private static string ConfigDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NormalCAD");

        private static string ConfigPath => Path.Combine(ConfigDirectory, "config.json");

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    if (config != null)
                        return config;
                }
            }
            catch
            {
                // Ignore corrupt or inaccessible config; fall back to defaults.
            }

            return new AppConfig();
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(ConfigDirectory);
                var json = JsonSerializer.Serialize(Current, _serializerOptions);
                File.WriteAllText(ConfigPath, json);
            }
            catch
            {
                // Persistence is best-effort; ignore failures.
            }
        }

        public static void Update(Action<AppConfig> mutate)
        {
            mutate(Current);
            Save();
        }
    }
}
