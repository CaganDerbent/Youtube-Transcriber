using StackExchange.Redis;
using System.Text.Json;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private const int CACHE_EXPIRY_DAYS = 7;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task<bool> AddTranscriptionAsync(string userId, string youtubeUrl, string transcription)
    {
        try
        {
            var currentDate = DateTime.Now;
 
            var transcriptionId = Guid.NewGuid().ToString();
            var transcriptionKey = $"transcription:{userId}:{transcriptionId}";


            var transcriptionData = new HashEntry[]
            {
                new HashEntry("url", youtubeUrl),
                new HashEntry("transcription", transcription),
                new HashEntry("date", currentDate.ToString("o"))
            };


            await _db.HashSetAsync(transcriptionKey, transcriptionData);


            var userTranscriptionsKey = $"user:{userId}:transcriptions";
            await _db.SetAddAsync(userTranscriptionsKey, transcriptionKey);

  
            await _db.KeyExpireAsync(transcriptionKey, TimeSpan.FromDays(CACHE_EXPIRY_DAYS));
            await _db.KeyExpireAsync(userTranscriptionsKey, TimeSpan.FromDays(CACHE_EXPIRY_DAYS));

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis error: {ex.Message}");
            throw;
        }
    }

    public async Task<List<TranscriptionData>> GetUserTranscriptionsAsync(string userId)
    {
        var result = new List<TranscriptionData>();
        var userTranscriptionsKey = $"user:{userId}:transcriptions";


        var transcriptionKeys = await _db.SetMembersAsync(userTranscriptionsKey);

        foreach (var key in transcriptionKeys)
        {
            var hashFields = await _db.HashGetAllAsync(key.ToString());
            if (hashFields.Any())
            {
                result.Add(new TranscriptionData
                {
                    Url = hashFields.FirstOrDefault(x => x.Name == "url").Value,
                    Transcription = hashFields.FirstOrDefault(x => x.Name == "transcription").Value,
                    Date = DateTime.Parse(hashFields.FirstOrDefault(x => x.Name == "date").Value)
                });
            }
        }

        return result;
    }

    public async Task<TranscriptionData> GetTranscriptionAsync(string userId, string youtubeUrl)
    {
        var userTranscriptionsKey = $"user:{userId}:transcriptions";
        var transcriptionKeys = await _db.SetMembersAsync(userTranscriptionsKey);

        foreach (var key in transcriptionKeys)
        {
            var hashFields = await _db.HashGetAllAsync(key.ToString());
            if (hashFields.Any() && hashFields.FirstOrDefault(x => x.Name == "url").Value == youtubeUrl)
            {
                return new TranscriptionData
                {
                    Url = hashFields.FirstOrDefault(x => x.Name == "url").Value,
                    Transcription = hashFields.FirstOrDefault(x => x.Name == "transcription").Value,
                    Date = DateTime.Parse(hashFields.FirstOrDefault(x => x.Name == "date").Value)
                };
            }
        }

        return null;
    }
} 