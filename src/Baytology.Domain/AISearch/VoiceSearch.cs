using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.AISearch;

public sealed class VoiceSearch : Entity
{
    public Guid SearchRequestId { get; private set; }
    public string AudioFileUrl { get; private set; } = null!;
    public string? TranscribedText { get; private set; }
    public string? Language { get; private set; }
    public float? ConfidenceScore { get; private set; }
    public string? STTProvider { get; private set; }
    public string? ParsedQuery { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    private VoiceSearch() { }

    private VoiceSearch(Guid searchRequestId, string audioFileUrl, string? language = null)
        : base(Guid.NewGuid())
    {
        SearchRequestId = searchRequestId;
        AudioFileUrl = audioFileUrl;
        Language = language;
    }

    public static Result<VoiceSearch> Create(Guid searchRequestId, string? audioFileUrl, string? language = null)
    {
        if (searchRequestId == Guid.Empty)
            return SearchErrors.SearchRequestIdRequired;

        var normalizedAudioFileUrl = audioFileUrl?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedAudioFileUrl))
            return SearchErrors.AudioFileUrlRequired;

        if (normalizedAudioFileUrl.Length > 1000)
            return SearchErrors.AudioFileUrlTooLong;

        var normalizedLanguage = string.IsNullOrWhiteSpace(language)
            ? null
            : language.Trim();

        if (normalizedLanguage is not null && normalizedLanguage.Length > 10)
            return SearchErrors.LanguageTooLong;

        return new VoiceSearch(searchRequestId, normalizedAudioFileUrl, normalizedLanguage);
    }

    public void SetTranscription(string transcribedText, float confidence, string sttProvider, string? parsedQuery)
    {
        TranscribedText = transcribedText;
        ConfidenceScore = confidence;
        STTProvider = sttProvider;
        ParsedQuery = parsedQuery;
        ProcessedAt = DateTimeOffset.UtcNow;
    }
}
