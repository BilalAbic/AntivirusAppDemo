using Newtonsoft.Json;

namespace AntivirusAppDemo.Scanners;

public class VirusTotalResponse
{
    [JsonProperty("data")]
    public VirusTotalData? Data { get; set; }
}

public class VirusTotalData
{
    [JsonProperty("attributes")]
    public VirusTotalAttributes? Attributes { get; set; }
}

public class VirusTotalAttributes
{
    [JsonProperty("last_analysis_stats")]
    public VirusTotalStats? LastAnalysisStats { get; set; }

    [JsonProperty("meaningful_name")]
    public string? MeaningfulName { get; set; }
}

public class VirusTotalStats
{
    [JsonProperty("malicious")]
    public int Malicious { get; set; }

    [JsonProperty("suspicious")]
    public int Suspicious { get; set; }

    [JsonProperty("undetected")]
    public int Undetected { get; set; }

    [JsonProperty("harmless")]
    public int Harmless { get; set; }
}
