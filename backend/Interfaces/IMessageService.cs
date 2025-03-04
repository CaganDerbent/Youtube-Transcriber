namespace backend.Interfaces
{
    public interface IMessageService
    {
        void PublishMessage<T>(T message, string queueName);
    }
} 