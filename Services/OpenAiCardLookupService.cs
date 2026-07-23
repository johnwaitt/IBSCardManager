using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IBSCardManager.Models;

namespace IBSCardManager.Services;

public class OpenAiCardLookupService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAiCardLookupService> _logger;

    public OpenAiCardLookupService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAiCardLookupService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(GetApiKey());

    public async Task<AiCardLookupResult> IdentifyAsync(string frontPath, string? backPath, CancellationToken cancellationToken)
    {
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenAI card lookup is not configured. Set OPENAI_API_KEY or OpenAI:ApiKey.");

        var content = new List<object>
        {
            new
            {
                type = "input_text",
                text = "Identify this trading card from the front and optional back image. Return only valid JSON with these keys: subject, team, year, set, cardNumber, variety, serial, category, isRookie, isAutograph, isRelic, confidence, notes. Use null when uncertain. Confidence must be from 0 to 1. Do not invent a card number or parallel. Read the back for card number and set details when available."
            },
            new { type = "input_image", image_url = ToDataUrl(frontPath), detail = "high" }
        };
        if (!string.IsNullOrWhiteSpace(backPath) && File.Exists(backPath))
            content.Add(new { type = "input_image", image_url = ToDataUrl(backPath), detail = "high" });

        var body = new
        {
            model = _configuration["OpenAI:Model"] ?? "gpt-5-mini",
            store = false,
            input = new[] { new { role = "user", content } },
            text = new { format = new { type = "json_object" } }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenAI card lookup failed: {Status} {Body}", response.StatusCode, responseText);
            throw new InvalidOperationException($"OpenAI lookup failed ({(int)response.StatusCode}).");
        }

        using var document = JsonDocument.Parse(responseText);
        var outputText = ExtractOutputText(document.RootElement);
        if (string.IsNullOrWhiteSpace(outputText)) throw new InvalidOperationException("OpenAI returned no card data.");

        outputText = StripCodeFence(outputText);
        var result = JsonSerializer.Deserialize<AiCardLookupResult>(outputText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result ?? throw new InvalidOperationException("OpenAI returned card data that could not be read.");
    }

    private string? GetApiKey() => Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? _configuration["OpenAI:ApiKey"];

    private static string ToDataUrl(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        var mime = extension switch { ".png" => "image/png", ".webp" => "image/webp", _ => "image/jpeg" };
        return $"data:{mime};base64,{Convert.ToBase64String(File.ReadAllBytes(path))}";
    }

    private static string? ExtractOutputText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out var direct) && direct.ValueKind == JsonValueKind.String) return direct.GetString();
        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array) return null;
        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array) continue;
            foreach (var part in content.EnumerateArray())
                if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String) return text.GetString();
        }
        return null;
    }

    private static string StripCodeFence(string value)
    {
        value = value.Trim();
        if (!value.StartsWith("```", StringComparison.Ordinal)) return value;
        var firstLine = value.IndexOf('\n');
        var lastFence = value.LastIndexOf("```", StringComparison.Ordinal);
        return firstLine >= 0 && lastFence > firstLine ? value[(firstLine + 1)..lastFence].Trim() : value;
    }
}
