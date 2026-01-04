using System.Net.Http.Headers;
using AntivirusAppDemo.Helpers;
using AntivirusAppDemo.Interfaces;
using AntivirusAppDemo.Models;
using Newtonsoft.Json;

namespace AntivirusAppDemo.Scanners;

public class VirusTotalScanner : IScanner
{
    private const string BaseUrl = "https://www.virustotal.com/api/v3/files/";
    private readonly HttpClient _httpClient;
    private string _apiKey;

    public string Name => "VirusTotal";

    public VirusTotalScanner(string apiKey = "YOUR_API_KEY_HERE")
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("x-apikey", _apiKey);
    }

    public void UpdateApiKey(string newApiKey)
    {
        _apiKey = newApiKey;
        _httpClient.DefaultRequestHeaders.Remove("x-apikey");
        _httpClient.DefaultRequestHeaders.Add("x-apikey", _apiKey);
    }

    public async Task<ScanResult> ScanAsync(string filePath)
    {
        var result = new ScanResult
        {
            FilePath = filePath,
            ScannerName = Name
        };

        try
        {
            if (!File.Exists(filePath))
            {
                result.Message = "Dosya bulunamadı";
                return result;
            }

            var hash = HashHelper.CalculateSHA256(filePath);
            result.FileHash = hash;
            
            var response = await _httpClient.GetAsync($"{BaseUrl}{hash}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                result.Message = "Dosya VirusTotal veritabanında bulunamadı";
                return result;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                result.Message = "Geçersiz API anahtarı";
                return result;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                result.Message = "Rate limit aşıldı, lütfen bekleyin";
                return result;
            }

            if (!response.IsSuccessStatusCode)
            {
                result.Message = $"API Hatası: {response.StatusCode}";
                return result;
            }

            var json = await response.Content.ReadAsStringAsync();
            var vtResponse = JsonConvert.DeserializeObject<VirusTotalResponse>(json);

            if (vtResponse?.Data?.Attributes?.LastAnalysisStats is { } stats)
            {
                var totalEngines = stats.Malicious + stats.Suspicious + stats.Undetected + stats.Harmless;
                var threatCount = stats.Malicious + stats.Suspicious;

                result.IsThreat = threatCount > 0;
                result.ThreatScore = totalEngines > 0 ? (threatCount * 100) / totalEngines : 0;
                result.Message = result.IsThreat
                    ? $"⚠️ Tehdit! {threatCount}/{totalEngines} motor tespit etti"
                    : $"✅ Temiz. {totalEngines} motor taradı";
            }
        }
        catch (HttpRequestException)
        {
            result.Message = "İnternet bağlantısı hatası";
        }
        catch (Exception ex)
        {
            result.Message = $"Hata: {ex.Message}";
        }

        return result;
    }
}
