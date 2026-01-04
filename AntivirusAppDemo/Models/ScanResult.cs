namespace AntivirusAppDemo.Models;

public class ScanResult
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName => Path.GetFileName(FilePath);
    public string? FileHash { get; set; }
    public bool IsThreat { get; set; }
    public string ScannerName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int ThreatScore { get; set; }
    public DateTime ScanDate { get; set; } = DateTime.Now;
}
