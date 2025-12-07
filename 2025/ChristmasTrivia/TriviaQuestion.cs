using System.Text.Json.Serialization;

namespace ChristmasTrivia;

public class TriviaQuestion
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("question")]
    public string? Question { get; set; }

    [JsonPropertyName("options")]
    public List<string>? Options { get; set; }

    [JsonPropertyName("answer")]
    public List<int>? Answer { get; set; }

    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }

    [JsonIgnore]
    public bool IsMultiSelect => Answer is { Count: > 1 };
}
