using AntivirusAppDemo.Interfaces;
using AntivirusAppDemo.Models;

namespace AntivirusAppDemo.Engine;

public class AntivirusEngine
{
    private readonly List<IScanner> _scanners = [];

    public void AddScanner(IScanner scanner)
    {
        _scanners.Add(scanner);
    }

    public void RemoveScanner(IScanner scanner)
    {
        _scanners.Remove(scanner);
    }

    public async Task<List<ScanResult>> ScanFileAsync(string filePath)
    {
        var results = new List<ScanResult>();

        foreach (var scanner in _scanners)
        {
            var result = await scanner.ScanAsync(filePath);
            results.Add(result);
        }

        return results;
    }

    public int ScannerCount => _scanners.Count;
}
