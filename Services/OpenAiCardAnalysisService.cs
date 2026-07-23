using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IBSCardManager.Models;
using IBSCardManager.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace IBSCardManager.Services;

public sealed class OpenAiCardAnalysisService : IOpenAiCardAnalysisService
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private static readonly HashSet<string> ValidEvidenceSources = new(StringComparer.OrdinalIgnoreCase)
    {
        CardEvidenceSource.Front,
        CardEvidenceSource.Back,
        CardEvidenceSource.Both,
        CardEvidenceSource.Unknown,
        CardEvidenceSource.UserCorrected
    };

    private readonly HttpClient _httpClient;
    private readonly OpenAiCardAnalysisOptions _options;
    private readonly ILogger<OpenAiCardAnalysisService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;

    public OpenAiCardAnalysisService(
        HttpClient httpClient,
        IOptions<OpenAiCardAnalysisOptions> options,
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<OpenAiCardAnalysisService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _configuration = configuration;
        _cache = cache;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(GetApiKey());

    public async Task<CardAnalysisResponseEnvelope> AnalyzeAsync(CardAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new InvalidOperationException("No scanner analysis request was supplied.");

        var started = DateTimeOffset.UtcNow;
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("ChatGPT image analysis is not configured.");

        if (_options.LocalAnalysisOnly)
            throw new InvalidOperationException("ChatGPT image analysis is disabled because local analysis only is enabled.");

        if (string.IsNullOrWhiteSpace(request.FrontImagePath) || !File.Exists(request.FrontImagePath))
            throw new InvalidOperationException("The selected front image was not found.");

        var front = PrepareImage(request.FrontImagePath, "front");
        var back = string.IsNullOrWhiteSpace(request.BackImagePath)
            ? null
            : PrepareImage(request.BackImagePath, "back");

        var pairId = request.PairId ?? Path.GetFileName(front.Path);
        var frontHash = request.FrontImageHash ?? ScannerImageHashUtility.ComputeHash(front.Path);
        var backHash = request.BackImageHash ?? ScannerImageHashUtility.ComputeHash(back?.Path);
        var combinedHash = ScannerImageHashUtility.ComputeCombinedHash(front.Path, back?.Path);
        var cacheKey = $"openai-analysis:{_options.VisionModel}:{frontHash}:{backHash}";

        if (_options.ReuseCachedAnalysis && !string.IsNullOrWhiteSpace(frontHash) && _cache.TryGetValue<CardAnalysisResult>(cacheKey, out var cached))
        {
            _logger.LogInformation("Using saved analysis result for pair {PairId}. Model={Model} FrontHash={FrontHash} BackHash={BackHash}",
                pairId,
                _options.VisionModel,
                frontHash,
                backHash);

            return new CardAnalysisResponseEnvelope
            {
                Analysis = cached,
                Cached = true,
                Model = _options.VisionModel,
                Duration = DateTimeOffset.UtcNow - started,
                ResponseFieldCount = CountResultFields(cached)
            };
        }

        var messages = BuildMessages(front.DataUrl, back?.DataUrl, request.Hints);
        var payload = BuildPayload(messages);

        var maxAttempts = Math.Max(1, _options.MaxRetries + 1);
        var attempt = 0;

        while (attempt < maxAttempts)
        {
            attempt++;

            try
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linkedCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(15, _options.TimeoutSeconds)));

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var response = await _httpClient.SendAsync(httpRequest, linkedCts.Token);
                var responseText = await response.Content.ReadAsStringAsync(linkedCts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    if (IsTransientStatus(response.StatusCode) && attempt < maxAttempts)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(300 * Math.Pow(2, attempt)), cancellationToken);
                        continue;
                    }

                    _logger.LogWarning("OpenAI analysis failed for pair {PairId}. Status={Status} Attempt={Attempt}", pairId, (int)response.StatusCode, attempt);
                    throw new InvalidOperationException(MapFailureMessage(response.StatusCode));
                }

                using var doc = JsonDocument.Parse(responseText);
                var outputText = ExtractOutputText(doc.RootElement);
                if (string.IsNullOrWhiteSpace(outputText))
                    throw new InvalidOperationException("OpenAI returned no readable card analysis data.");

                var result = JsonSerializer.Deserialize<CardAnalysisResult>(outputText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                    throw new InvalidOperationException("OpenAI returned a schema response that could not be parsed.");

                ValidateResult(result);

                if (_options.ReuseCachedAnalysis && !string.IsNullOrWhiteSpace(frontHash))
                {
                    _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
                }

                var duration = DateTimeOffset.UtcNow - started;
                _logger.LogInformation(
                    "OpenAI analysis succeeded PairId={PairId} FrontHash={FrontHash} BackHash={BackHash} CombinedHash={CombinedHash} Model={Model} DurationMs={DurationMs} Fields={FieldCount} Confidence={Confidence}",
                    pairId,
                    frontHash,
                    backHash,
                    combinedHash,
                    _options.VisionModel,
                    duration.TotalMilliseconds,
                    CountResultFields(result),
                    result.Confidence);

                return new CardAnalysisResponseEnvelope
                {
                    Analysis = result,
                    Cached = false,
                    Model = _options.VisionModel,
                    Duration = duration,
                    ResponseFieldCount = CountResultFields(result)
                };
            }
            catch (OperationCanceledException) when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(300 * Math.Pow(2, attempt)), cancellationToken);
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(300 * Math.Pow(2, attempt)), cancellationToken);
            }
        }

        throw new InvalidOperationException("ChatGPT analysis failed after retries. Please retry.");
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey)) return false;

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(new
        {
            model = _options.VisionModel,
            input = new[]
            {
                new
                {
                    role = "user",
                    content = new[]
                    {
                        new { type = "input_text", text = "Reply with JSON: {\"ok\":true}" }
                    }
                }
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "connection_test",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        additionalProperties = false,
                        properties = new { ok = new { type = "boolean" } },
                        required = new[] { "ok" }
                    }
                }
            }
        }), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private object BuildPayload(List<object> messageContent)
    {
        var systemPrompt = "You are analyzing the front and back of one baseball or sports trading card. Use only visible evidence from the provided images. Do not invent a player, set, card number, parallel, or serial number. Return null when uncertain. Treat the front and back as evidence for the same card, but warn if they appear mismatched. Preserve exact spelling, accents, apostrophes, suffixes, prefixes, and leading zeros. Differentiate card number, serial number, serial maximum, printed copyright year, and product year. Use checklist terminology when visible.";

        return new
        {
            model = _options.VisionModel,
            store = false,
            input = new object[]
            {
                new
                {
                    role = "system",
                    content = new object[]
                    {
                        new { type = "input_text", text = systemPrompt }
                    }
                },
                new
                {
                    role = "user",
                    content = messageContent.ToArray()
                }
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "card_analysis_result",
                    strict = true,
                    schema = BuildSchema()
                }
            }
        };
    }

    private static object BuildSchema()
    {
        object FieldSchema(object valueSchema) => new
        {
            type = "object",
            additionalProperties = false,
            properties = new
            {
                value = valueSchema,
                confidence = new { type = "number" },
                evidenceSource = new { type = "string", @enum = new[] { "front", "back", "both", "unknown" } }
            },
            required = new[] { "value", "confidence", "evidenceSource" }
        };

        return new
        {
            type = "object",
            additionalProperties = false,
            properties = new
            {
                playerName = FieldSchema(new { type = new[] { "string", "null" } }),
                additionalSubjects = new { type = "array", items = FieldSchema(new { type = new[] { "string", "null" } }) },
                team = FieldSchema(new { type = new[] { "string", "null" } }),
                sport = FieldSchema(new { type = new[] { "string", "null" } }),
                year = FieldSchema(new { type = new[] { "integer", "null" } }),
                manufacturer = FieldSchema(new { type = new[] { "string", "null" } }),
                brand = FieldSchema(new { type = new[] { "string", "null" } }),
                product = FieldSchema(new { type = new[] { "string", "null" } }),
                productEdition = FieldSchema(new { type = new[] { "string", "null" } }),
                checklistSection = FieldSchema(new { type = new[] { "string", "null" } }),
                cardNumber = FieldSchema(new { type = new[] { "string", "null" } }),
                parallel = FieldSchema(new { type = new[] { "string", "null" } }),
                variation = FieldSchema(new { type = new[] { "string", "null" } }),
                rookie = FieldSchema(new { type = new[] { "boolean", "null" } }),
                autograph = FieldSchema(new { type = new[] { "boolean", "null" } }),
                relic = FieldSchema(new { type = new[] { "boolean", "null" } }),
                patch = FieldSchema(new { type = new[] { "boolean", "null" } }),
                shortPrint = FieldSchema(new { type = new[] { "boolean", "null" } }),
                serialNumber = FieldSchema(new { type = new[] { "string", "null" } }),
                serialMaximum = FieldSchema(new { type = new[] { "integer", "null" } }),
                printedCopyrightYear = FieldSchema(new { type = new[] { "integer", "null" } }),
                frontText = FieldSchema(new { type = new[] { "string", "null" } }),
                backText = FieldSchema(new { type = new[] { "string", "null" } }),
                confidence = new { type = "number" },
                warnings = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        additionalProperties = false,
                        properties = new
                        {
                            code = new { type = "string" },
                            message = new { type = "string" }
                        },
                        required = new[] { "code", "message" }
                    }
                },
                evidence = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        additionalProperties = false,
                        properties = new
                        {
                            field = new { type = "string" },
                            snippet = new { type = new[] { "string", "null" } },
                            source = new { type = "string", @enum = new[] { "front", "back", "both", "unknown" } }
                        },
                        required = new[] { "field", "snippet", "source" }
                    }
                }
            },
            required = new[]
            {
                "playerName", "additionalSubjects", "team", "sport", "year", "manufacturer", "brand", "product", "productEdition",
                "checklistSection", "cardNumber", "parallel", "variation", "rookie", "autograph", "relic", "patch", "shortPrint",
                "serialNumber", "serialMaximum", "printedCopyrightYear", "frontText", "backText", "confidence", "warnings", "evidence"
            }
        };
    }

    private List<object> BuildMessages(string frontDataUrl, string? backDataUrl, CardAnalysisHints? hints)
    {
        var messages = new List<object>
        {
            new
            {
                type = "input_text",
                text = "Image 1 is the FRONT. Image 2 is the BACK (if provided). Analyze as the same card. Return only strict JSON for the schema. Do not guess unsupported details and return null when unknown. Preserve exact card-number prefixes and leading zeros."
            },
            new { type = "input_image", image_url = frontDataUrl, detail = "high" }
        };

        if (!string.IsNullOrWhiteSpace(backDataUrl))
        {
            messages.Add(new { type = "input_image", image_url = backDataUrl, detail = "high" });
        }

        if (hints != null)
        {
            var hintText = $"User hints (optional): player={hints.PlayerName ?? "null"}, team={hints.Team ?? "null"}, year={hints.Year?.ToString() ?? "null"}, product={hints.Product ?? "null"}, cardNumber={hints.CardNumber ?? "null"}, parallel={hints.Parallel ?? "null"}, variation={hints.Variation ?? "null"}. Use only if visually supported.";
            messages.Add(new { type = "input_text", text = hintText });
        }

        return messages;
    }

    private PreparedImage PrepareImage(string path, string side)
    {
        if (!File.Exists(path)) throw new InvalidOperationException($"The {side} image file was not found.");

        var extension = Path.GetExtension(path);
        if (!SupportedExtensions.Contains(extension)) throw new InvalidOperationException("Unsupported image format. Use JPG, JPEG, PNG, or WEBP.");

        var bytes = File.ReadAllBytes(path);
        if (_options.MaxImageBytes > 0 && bytes.LongLength > _options.MaxImageBytes)
            throw new InvalidOperationException($"The {side} image exceeds the maximum configured analysis size.");

        var mime = extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };

        return new PreparedImage
        {
            Path = path,
            DataUrl = $"data:{mime};base64,{Convert.ToBase64String(bytes)}"
        };
    }

    private void ValidateResult(CardAnalysisResult result)
    {
        ValidateField(result.PlayerName);
        ValidateField(result.Team);
        ValidateField(result.Sport);
        ValidateField(result.Year);
        ValidateField(result.Manufacturer);
        ValidateField(result.Brand);
        ValidateField(result.Product);
        ValidateField(result.ProductEdition);
        ValidateField(result.ChecklistSection);
        ValidateField(result.CardNumber);
        ValidateField(result.Parallel);
        ValidateField(result.Variation);
        ValidateField(result.Rookie);
        ValidateField(result.Autograph);
        ValidateField(result.Relic);
        ValidateField(result.Patch);
        ValidateField(result.ShortPrint);
        ValidateField(result.SerialNumber);
        ValidateField(result.SerialMaximum);
        ValidateField(result.PrintedCopyrightYear);
        ValidateField(result.FrontText);
        ValidateField(result.BackText);

        foreach (var warning in result.Warnings)
        {
            warning.Code = warning.Code?.Trim() ?? string.Empty;
            warning.Message = warning.Message?.Trim() ?? string.Empty;
        }

        foreach (var evidence in result.Evidence)
        {
            if (!ValidEvidenceSources.Contains(evidence.Source ?? string.Empty)) evidence.Source = CardEvidenceSource.Unknown;
        }
    }

    private static void ValidateField<T>(CardFieldResult<T> field)
    {
        if (field == null) return;

        field.Confidence = decimal.Clamp(field.Confidence, 0m, 1m);
        if (!ValidEvidenceSources.Contains(field.EvidenceSource ?? string.Empty))
        {
            field.EvidenceSource = CardEvidenceSource.Unknown;
        }
    }

    private static bool IsTransientStatus(HttpStatusCode statusCode)
    {
        var code = (int)statusCode;
        return statusCode == HttpStatusCode.RequestTimeout || statusCode == (HttpStatusCode)429 || code >= 500;
    }

    private static string MapFailureMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => "ChatGPT image analysis is not configured correctly. Verify the API key.",
            HttpStatusCode.Forbidden => "ChatGPT image analysis request was denied.",
            HttpStatusCode.RequestTimeout => "ChatGPT image analysis timed out. Please retry.",
            (HttpStatusCode)429 => "ChatGPT image analysis is rate-limited. Please retry shortly.",
            _ => "OpenAI is currently unavailable for image analysis. Please retry."
        };
    }

    private string? GetApiKey()
    {
        return Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? _configuration["OpenAI:ApiKey"]
            ?? _options.ApiKey;
    }

    private static string? ExtractOutputText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out var direct) && direct.ValueKind == JsonValueKind.String) return direct.GetString();
        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array) return null;

        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array) continue;

            foreach (var part in content.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                    return text.GetString();
            }
        }

        return null;
    }

    private static int CountResultFields(CardAnalysisResult result)
    {
        var values = new object?[]
        {
            result.PlayerName.Value,
            result.Team.Value,
            result.Sport.Value,
            result.Year.Value,
            result.Manufacturer.Value,
            result.Brand.Value,
            result.Product.Value,
            result.ProductEdition.Value,
            result.ChecklistSection.Value,
            result.CardNumber.Value,
            result.Parallel.Value,
            result.Variation.Value,
            result.Rookie.Value,
            result.Autograph.Value,
            result.Relic.Value,
            result.Patch.Value,
            result.ShortPrint.Value,
            result.SerialNumber.Value,
            result.SerialMaximum.Value,
            result.PrintedCopyrightYear.Value,
            result.FrontText.Value,
            result.BackText.Value
        };

        return values.Count(x => x != null && !string.IsNullOrWhiteSpace(x.ToString()));
    }

    private sealed class PreparedImage
    {
        public string Path { get; init; } = string.Empty;
        public string DataUrl { get; init; } = string.Empty;
    }
}
