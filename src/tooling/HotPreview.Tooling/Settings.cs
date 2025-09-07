using System.Text.Json;

namespace HotPreview.Tooling;

public static class Settings
{
    private static readonly object _sync = new();
    private static bool _loaded;
    private static SettingsModel _model = new()
    {
        BringAppToFrontOnNavigate = true
    };

    private sealed class SettingsModel
    {
        public bool BringAppToFrontOnNavigate { get; set; }
    }

    private static string GetSettingsDirectory()
    {
        string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string dir = Path.Combine(baseDir, "HotPreview");
        return dir;
    }

    private static string GetSettingsPath() => Path.Combine(GetSettingsDirectory(), "settings.json");

    private static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        lock (_sync)
        {
            if (_loaded)
            {
                return;
            }

            try
            {
                string path = GetSettingsPath();
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    SettingsModel? model = JsonSerializer.Deserialize<SettingsModel>(json);
                    if (model is not null)
                    {
                        _model = model;
                    }
                }
            }
            catch
            {
                // Ignore deserialization errors and fall back to defaults
            }

            _loaded = true;
        }
    }

    private static void Save()
    {
        lock (_sync)
        {
            try
            {
                string dir = GetSettingsDirectory();
                Directory.CreateDirectory(dir);
                string json = JsonSerializer.Serialize(_model, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(GetSettingsPath(), json);
            }
            catch
            {
                // Ignore persistence errors
            }
        }
    }

    public static bool BringAppToFrontOnNavigate
    {
        get
        {
            EnsureLoaded();
            return _model.BringAppToFrontOnNavigate;
        }
        set
        {
            EnsureLoaded();
            if (_model.BringAppToFrontOnNavigate != value)
            {
                _model.BringAppToFrontOnNavigate = value;
                Save();
            }
        }
    }
}
