namespace AntivirusAppDemo.Services;

public class RealTimeProtectionService : IDisposable
{
    private readonly List<FileSystemWatcher> _watchers = [];
    private readonly HashSet<string> _monitoredPaths = [];
    
    public event EventHandler<FileDetectedEventArgs>? FileDetected;
    public bool IsEnabled { get; private set; }

    public void Enable(IEnumerable<string> pathsToMonitor)
    {
        if (IsEnabled) return;

        foreach (var path in pathsToMonitor)
        {
            if (!Directory.Exists(path) || _monitoredPaths.Contains(path)) 
                continue;

            var watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            watcher.Created += OnFileCreated;
            watcher.Changed += OnFileChanged;
            watcher.Renamed += OnFileRenamed;

            _watchers.Add(watcher);
            _monitoredPaths.Add(path);
        }

        IsEnabled = true;
    }

    public void Disable()
    {
        foreach (var watcher in _watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
        _watchers.Clear();
        _monitoredPaths.Clear();
        IsEnabled = false;
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        if (ShouldScan(e.FullPath))
            FileDetected?.Invoke(this, new FileDetectedEventArgs(e.FullPath, FileDetectionType.Created));
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (ShouldScan(e.FullPath))
            FileDetected?.Invoke(this, new FileDetectedEventArgs(e.FullPath, FileDetectionType.Modified));
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        if (ShouldScan(e.FullPath))
            FileDetected?.Invoke(this, new FileDetectedEventArgs(e.FullPath, FileDetectionType.Renamed));
    }

    private static bool ShouldScan(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return false;
            
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var riskyExtensions = new[] { ".exe", ".dll", ".bat", ".cmd", ".ps1", ".vbs", ".js", ".msi", ".scr", ".com" };
            
            return riskyExtensions.Contains(extension);
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        Disable();
    }
}

public class FileDetectedEventArgs(string filePath, FileDetectionType detectionType) : EventArgs
{
    public string FilePath { get; } = filePath;
    public FileDetectionType DetectionType { get; } = detectionType;
}

public enum FileDetectionType
{
    Created,
    Modified,
    Renamed
}
