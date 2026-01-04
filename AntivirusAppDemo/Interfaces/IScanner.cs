using AntivirusAppDemo.Models;

namespace AntivirusAppDemo.Interfaces;

public interface IScanner
{
    string Name { get; }
    Task<ScanResult> ScanAsync(string filePath);
}
