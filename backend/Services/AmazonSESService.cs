using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class AmazonSESService
    {
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly IConfiguration _configuration;


        public AmazonSESService(IConfiguration configuration, ILogger<AmazonSESService> logger)
        {
            _configuration = configuration;


            var awsRegion = _configuration["AWS:Region"] ?? throw new InvalidOperationException("AWS Region not configured");
            var accessKey = _configuration["AWS:AccessKey"] ?? throw new InvalidOperationException("AWS AccessKey not configured");
            var secretKey = _configuration["AWS:SecretKey"] ?? throw new InvalidOperationException("AWS SecretKey not configured");

            _sesClient = new AmazonSimpleEmailServiceClient(
                accessKey,
                secretKey,
                RegionEndpoint.GetBySystemName(awsRegion)
            );
        }

        public async Task SendEmail(string to, string subject, string body)
        {
            try
            {
                var sendRequest = new SendEmailRequest
                {
                    Source = _configuration["AWS:FromEmail"],
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { to }
                    },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Text = new Content
                            {
                                Charset = "UTF-8",
                                Data = body
                            }
                        }
                    }
                };

                var response = await _sesClient.SendEmailAsync(sendRequest);
                Console.WriteLine($"Email sent successfully. MessageId: {response.MessageId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                throw;
            }
        }
    }
} 