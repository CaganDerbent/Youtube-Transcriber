using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using backend.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class RabbitMQService : IMessageService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
  
        private bool _isConnected;
        private readonly int _retryCount = 5;

        public RabbitMQService(IConfiguration configuration)
        {

            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"] ?? "localhost",
                UserName = configuration["RabbitMQ:Username"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest",
                RequestedHeartbeat = TimeSpan.FromSeconds(60)
            };

            for (int retry = 1; retry <= _retryCount; retry++)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _isConnected = true;
                    Console.WriteLine("Producer successfully connected to RabbitMQ");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RabbitMQ producer connection attempt {retry} of {_retryCount} failed: {ex.Message}");
                    if (retry == _retryCount)
                    {
                        Console.WriteLine($"Could not connect to RabbitMQ-producer after {_retryCount} attempts");
                        throw;
                    }
                    Thread.Sleep(5000);
                }
            }
        }

        public void PublishMessage<T>(T message, string queueName)
        {
            if (!_isConnected)
            {
                throw new InvalidOperationException("RabbitMQ producer connection failed.");
            }

            try
            {
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: null,
                    body: body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing message: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
} 