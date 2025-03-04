using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Google.Cloud.Speech.V1;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using backend.Interfaces;




namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    
    public class SpeechController : ControllerBase
    {
        private readonly SpeechClient _speechClient;
        private readonly IRedisService _redisService;

        public SpeechController(SpeechClient speechClient, IRedisService redisService)
        {
            _speechClient = speechClient;
            _redisService = redisService;
        }

        public class TranscriptionRequest
        {
            public string url { get; set; }
        }

        private async Task<string> DownloadAudioWithYtDlp(string url, string outputPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"-x --audio-format wav --audio-quality 0 -o \"{outputPath}\" \"{url}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            var output = new StringBuilder();
            
            process.OutputDataReceived += (sender, e) => {
                if (e.Data != null)
                {
                    output.AppendLine(e.Data);
                    Console.WriteLine($"yt-dlp: {e.Data}");
                }
            };
            
            process.ErrorDataReceived += (sender, e) => {
                if (e.Data != null)
                {
                    output.AppendLine(e.Data);
                    Console.WriteLine($"yt-dlp error: {e.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"yt-dlp failed with exit code {process.ExitCode}. Output: {output}");
            }

            return outputPath;
        }

        private async Task ExtractAudioWithFFmpeg(string inputPath, string outputPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{inputPath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            var errorOutput = new StringBuilder();
            
            process.ErrorDataReceived += (sender, e) => {
                if (e.Data != null)
                {
                    errorOutput.AppendLine(e.Data);
                    Console.WriteLine($"FFmpeg: {e.Data}");
                }
            };

            process.Start();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                if (System.IO.File.Exists(outputPath))
                {
                    System.IO.File.Delete(outputPath);
                }
                throw new Exception($"FFmpeg failed with exit code {process.ExitCode}. Error: {errorOutput}");
            }
        }
        
        [HttpPost("totext")]
        public async Task<IActionResult> TranscribeYoutubeVideo([FromBody] TranscriptionRequest request)
        {
            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value.ToString(); // GenerateJWToken


                Console.WriteLine($"Processing URL: {request.url}");

                var downloadPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
                var finalWavPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");

                try
                {
                    await DownloadAudioWithYtDlp(request.url, downloadPath);
                    await ExtractAudioWithFFmpeg(downloadPath, finalWavPath);

                    var config = new RecognitionConfig
                    {
                        Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                        SampleRateHertz = 16000,
                        LanguageCode = "en-US",
                        EnableAutomaticPunctuation = true,
                    };

                    Console.WriteLine("Loading audio file for transcription");
                    var audio = await RecognitionAudio.FromFileAsync(finalWavPath);

                    Console.WriteLine("Starting synchronous transcription");
                    var response = await _speechClient.RecognizeAsync(config, audio);
                    
                    string transcription = string.Join("\n", response.Results.Select(r => r.Alternatives[0].Transcript));
                    Console.WriteLine("Transcription completed");

                    DateTime date = DateTime.UtcNow;

                    Console.WriteLine(userId,request.url,transcription);



                    await _redisService.AddTranscriptionAsync(userId, request.url, transcription);

                    return Ok(new { transcription });
                }
                finally
                {
                    if (System.IO.File.Exists(downloadPath))
                    {
                        System.IO.File.Delete(downloadPath);
                        Console.WriteLine("Cleaned up downloaded file");
                    }
                    if (System.IO.File.Exists(finalWavPath))
                    {
                        System.IO.File.Delete(finalWavPath);
                        Console.WriteLine("Cleaned up WAV file");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error processing video", error = ex.Message });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetTranscriptionHistory()
        {
            try
            { 
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // GenerateJWToken
                var history = await _redisService.GetUserTranscriptionsAsync(userId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving history", error = ex.Message });
            }
        }
    }
}
