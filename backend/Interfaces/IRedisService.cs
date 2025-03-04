public interface IRedisService
{
    Task<bool> AddTranscriptionAsync(string userId, string youtubeUrl, string transcription);
    Task<TranscriptionData> GetTranscriptionAsync(string userId, string youtubeUrl);
    Task<List<TranscriptionData>> GetUserTranscriptionsAsync(string userId);
} 