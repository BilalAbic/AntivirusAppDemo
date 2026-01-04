using System.Text.Json;

namespace AntivirusAppDemo.Services;

public class SettingsService
{
    private readonly string _settingsPath;
    private AppSettings _settings;

    public SettingsService()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "AntivirusAppDemo");
        
        if (!Directory.Exists(appDataPath))
            Directory.CreateDirectory(appDataPath);

        _settingsPath = Path.Combine(appDataPath, "settings.json");
        _settings = LoadSettings();
    }

    public AppSettings Settings => _settings;

    private AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }
        
        return new AppSettings();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsPath, json);
    }

    public void UpdateApiKey(string apiKey)
    {
        _settings.VirusTotalApiKey = apiKey;
        Save();
    }

    public void UpdateRealTimeProtection(bool enabled)
    {
        _settings.RealTimeProtectionEnabled = enabled;
        Save();
    }

    public void UpdateMonitoredPaths(List<string> paths)
    {
        _settings.MonitoredPaths = paths;
        Save();
    }
}

public class AppSettings
{
    public string VirusTotalApiKey { get; set; } = "YOUR_API_KEY_HERE";
    public bool RealTimeProtectionEnabled { get; set; } = false;
    public bool StartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public int ScanIntervalMs { get; set; } = 15000;
    public List<string> MonitoredPaths { get; set; } = [
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
    ];
    public List<string> ExcludedExtensions { get; set; } = [".txt", ".log", ".md"];
}
