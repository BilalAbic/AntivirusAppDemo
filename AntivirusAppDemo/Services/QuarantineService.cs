using System.IO.Compression;
using System.Text.Json;

namespace AntivirusAppDemo.Services;

public class QuarantineService
{
    private readonly string _quarantinePath;
    private readonly string _metadataFile;

    public QuarantineService()
    {
        _quarantinePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "AntivirusAppDemo", "Quarantine");
        _metadataFile = Path.Combine(_quarantinePath, "metadata.json");
        
        if (!Directory.Exists(_quarantinePath))
            Directory.CreateDirectory(_quarantinePath);
    }

    public async Task<bool> QuarantineFileAsync(string filePath, string reason)
    {
        try
        {
            if (!File.Exists(filePath)) return false;

            var fileInfo = new FileInfo(filePath);
            var quarantineId = Guid.NewGuid().ToString("N");
            var quarantinedFile = Path.Combine(_quarantinePath, $"{quarantineId}.quar");

            // Dosyayı şifreli zip olarak kaydet
            using (var archive = ZipFile.Open(quarantinedFile, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(filePath, fileInfo.Name);
            }

            // Metadata kaydet
            var metadata = await LoadMetadataAsync();
            metadata.Add(new QuarantineEntry
            {
                Id = quarantineId,
                OriginalPath = filePath,
                FileName = fileInfo.Name,
                QuarantineDate = DateTime.Now,
                Reason = reason,
                FileSize = fileInfo.Length
            });
            await SaveMetadataAsync(metadata);

            // Orijinal dosyayı sil
            File.Delete(filePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RestoreFileAsync(string quarantineId)
    {
        try
        {
            var metadata = await LoadMetadataAsync();
            var entry = metadata.FirstOrDefault(e => e.Id == quarantineId);
            if (entry == null) return false;

            var quarantinedFile = Path.Combine(_quarantinePath, $"{quarantineId}.quar");
            if (!File.Exists(quarantinedFile)) return false;

            // Orijinal klasörü oluştur
            var dir = Path.GetDirectoryName(entry.OriginalPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Dosyayı geri yükle
            using (var archive = ZipFile.OpenRead(quarantinedFile))
            {
                var zipEntry = archive.Entries.FirstOrDefault();
                zipEntry?.ExtractToFile(entry.OriginalPath, true);
            }

            // Karantina dosyasını sil
            File.Delete(quarantinedFile);
            metadata.Remove(entry);
            await SaveMetadataAsync(metadata);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeletePermanentlyAsync(string quarantineId)
    {
        try
        {
            var metadata = await LoadMetadataAsync();
            var entry = metadata.FirstOrDefault(e => e.Id == quarantineId);
            if (entry == null) return false;

            var quarantinedFile = Path.Combine(_quarantinePath, $"{quarantineId}.quar");
            if (File.Exists(quarantinedFile))
                File.Delete(quarantinedFile);

            metadata.Remove(entry);
            await SaveMetadataAsync(metadata);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<QuarantineEntry>> GetQuarantinedFilesAsync()
    {
        return await LoadMetadataAsync();
    }

    private async Task<List<QuarantineEntry>> LoadMetadataAsync()
    {
        if (!File.Exists(_metadataFile))
            return [];

        var json = await File.ReadAllTextAsync(_metadataFile);
        return JsonSerializer.Deserialize<List<QuarantineEntry>>(json) ?? [];
    }

    private async Task SaveMetadataAsync(List<QuarantineEntry> metadata)
    {
        var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_metadataFile, json);
    }
}

public class QuarantineEntry
{
    public string Id { get; set; } = string.Empty;
    public string OriginalPath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime QuarantineDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
