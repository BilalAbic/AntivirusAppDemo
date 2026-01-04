using Microsoft.Data.Sqlite;
using AntivirusAppDemo.Models;

namespace AntivirusAppDemo.Services;

public class DatabaseService : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _dbPath;

    public DatabaseService()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "AntivirusAppDemo");
        
        if (!Directory.Exists(appDataPath))
            Directory.CreateDirectory(appDataPath);

        _dbPath = Path.Combine(appDataPath, "antivirus.db");
        _connection = new SqliteConnection($"Data Source={_dbPath}");
        _connection.Open();
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        var command = _connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS ScanHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FilePath TEXT NOT NULL,
                FileName TEXT NOT NULL,
                FileHash TEXT,
                IsThreat INTEGER NOT NULL,
                ScannerName TEXT NOT NULL,
                Message TEXT,
                ThreatScore INTEGER,
                ScanDate TEXT NOT NULL
            );
            
            CREATE TABLE IF NOT EXISTS Settings (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            );
            
            CREATE TABLE IF NOT EXISTS Statistics (
                Id INTEGER PRIMARY KEY,
                TotalScans INTEGER DEFAULT 0,
                ThreatsFound INTEGER DEFAULT 0,
                FilesQuarantined INTEGER DEFAULT 0,
                LastScanDate TEXT
            );
            
            INSERT OR IGNORE INTO Statistics (Id, TotalScans, ThreatsFound, FilesQuarantined) VALUES (1, 0, 0, 0);
        """;
        command.ExecuteNonQuery();
    }

    public void SaveScanResult(ScanResult result, string? fileHash = null)
    {
        var command = _connection.CreateCommand();
        command.CommandText = """
            INSERT INTO ScanHistory (FilePath, FileName, FileHash, IsThreat, ScannerName, Message, ThreatScore, ScanDate)
            VALUES ($filePath, $fileName, $fileHash, $isThreat, $scannerName, $message, $threatScore, $scanDate)
        """;
        
        command.Parameters.AddWithValue("$filePath", result.FilePath);
        command.Parameters.AddWithValue("$fileName", Path.GetFileName(result.FilePath));
        command.Parameters.AddWithValue("$fileHash", fileHash ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$isThreat", result.IsThreat ? 1 : 0);
        command.Parameters.AddWithValue("$scannerName", result.ScannerName);
        command.Parameters.AddWithValue("$message", result.Message);
        command.Parameters.AddWithValue("$threatScore", result.ThreatScore);
        command.Parameters.AddWithValue("$scanDate", DateTime.Now.ToString("o"));
        
        command.ExecuteNonQuery();
        UpdateStatistics(result.IsThreat);
    }

    private void UpdateStatistics(bool isThreat)
    {
        var command = _connection.CreateCommand();
        command.CommandText = isThreat
            ? "UPDATE Statistics SET TotalScans = TotalScans + 1, ThreatsFound = ThreatsFound + 1, LastScanDate = $date WHERE Id = 1"
            : "UPDATE Statistics SET TotalScans = TotalScans + 1, LastScanDate = $date WHERE Id = 1";
        command.Parameters.AddWithValue("$date", DateTime.Now.ToString("o"));
        command.ExecuteNonQuery();
    }

    public void IncrementQuarantineCount()
    {
        var command = _connection.CreateCommand();
        command.CommandText = "UPDATE Statistics SET FilesQuarantined = FilesQuarantined + 1 WHERE Id = 1";
        command.ExecuteNonQuery();
    }

    public ScanStatistics GetStatistics()
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT TotalScans, ThreatsFound, FilesQuarantined, LastScanDate FROM Statistics WHERE Id = 1";
        
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new ScanStatistics
            {
                TotalScans = reader.GetInt32(0),
                ThreatsFound = reader.GetInt32(1),
                FilesQuarantined = reader.GetInt32(2),
                LastScanDate = reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3))
            };
        }
        return new ScanStatistics();
    }

    public List<ScanHistoryEntry> GetScanHistory(int limit = 100)
    {
        var results = new List<ScanHistoryEntry>();
        var command = _connection.CreateCommand();
        command.CommandText = $"SELECT * FROM ScanHistory ORDER BY ScanDate DESC LIMIT {limit}";
        
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new ScanHistoryEntry
            {
                Id = reader.GetInt32(0),
                FilePath = reader.GetString(1),
                FileName = reader.GetString(2),
                FileHash = reader.IsDBNull(3) ? null : reader.GetString(3),
                IsThreat = reader.GetInt32(4) == 1,
                ScannerName = reader.GetString(5),
                Message = reader.IsDBNull(6) ? null : reader.GetString(6),
                ThreatScore = reader.GetInt32(7),
                ScanDate = DateTime.Parse(reader.GetString(8))
            });
        }
        return results;
    }

    public void SaveSetting(string key, string value)
    {
        var command = _connection.CreateCommand();
        command.CommandText = "INSERT OR REPLACE INTO Settings (Key, Value) VALUES ($key, $value)";
        command.Parameters.AddWithValue("$key", key);
        command.Parameters.AddWithValue("$value", value);
        command.ExecuteNonQuery();
    }

    public string? GetSetting(string key)
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT Value FROM Settings WHERE Key = $key";
        command.Parameters.AddWithValue("$key", key);
        return command.ExecuteScalar()?.ToString();
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}

public class ScanStatistics
{
    public int TotalScans { get; set; }
    public int ThreatsFound { get; set; }
    public int FilesQuarantined { get; set; }
    public DateTime? LastScanDate { get; set; }
}

public class ScanHistoryEntry
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? FileHash { get; set; }
    public bool IsThreat { get; set; }
    public string ScannerName { get; set; } = string.Empty;
    public string? Message { get; set; }
    public int ThreatScore { get; set; }
    public DateTime ScanDate { get; set; }
}
