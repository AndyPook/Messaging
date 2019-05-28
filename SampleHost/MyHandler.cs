using System;
using System.Threading.Tasks;
using A6k.Messaging;
using Confluent.Kafka;

namespace SampleHost
{
    public class MyHandler : IMessageHandler<string, string>
    {
        public Task HandleAsync(Message<string, string> message)
        {
            Console.WriteLine($"message: key={message.Key} - value={message.Value}");
            return Task.CompletedTask;
        }
    }
}
