using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using backend.Models;

namespace backend.Services
{
    public class EmailConsumerService : BackgroundService // .NET'in BackgroundService sýnýfýný kullanarak oluþturulan bir hizmet, uygulama baþlatýldýðýnda otomatik olarak çalýþtýrýlýr ve uygulama kapanana kadar arka planda çalýþmaya devam eder. 
    {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IModel _channel;
        private bool _isConnected;
        private readonly int _retryCount = 5;
        private readonly AmazonSESService _sesService;

        public EmailConsumerService(
            IConfiguration configuration, 
            AmazonSESService sesService)
        {
            _configuration = configuration;
            _sesService = sesService;
            ConnectToRabbitMQ();
        }

        private void ConnectToRabbitMQ()
        {
            if (_isConnected) return;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                UserName = _configuration["RabbitMQ:Username"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest",
                RequestedHeartbeat = TimeSpan.FromSeconds(60)
            };

            for (int retry = 1; retry <= _retryCount; retry++)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _channel.QueueDeclare(
                        queue: "email_notifications",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    _isConnected = true;
                    Console.WriteLine("Consumer successfully connected to RabbitMQ");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RabbitMQ consumer connection attempt {retry} of {_retryCount} failed: {ex.Message}");
                    if (retry == _retryCount)
                    {
                        Console.WriteLine($"Could not connect to RabbitMQ-consumer after {_retryCount} attempts");
                        throw;
                    }
                    Thread.Sleep(5000);
                }
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) // ack-nack
        {
            var consumer = new EventingBasicConsumer(_channel); 
            
            consumer.Received += async (model, ea) => // Mesaj geldiði zaman çalýþýr.
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<EmailMessage>(
                    Encoding.UTF8.GetString(body));

                try
                {
                    if (message != null)
                    {
                        await _sesService.SendEmail(
                            message.To,
                            message.Subject,
                            message.Body
                        );
                        _channel.BasicAck(ea.DeliveryTag, false);
                        Console.WriteLine($"Email sent successfully to {message.To}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email via SES: {ex.Message}");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: "email_notifications",
                autoAck: false,
                consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
} 