using System.Threading.Tasks;
using Confluent.Kafka;

namespace A6k.Messaging
{
    public interface IMessageHandler<TKey, TValue>
    {
        Task HandleAsync(Message<TKey, TValue> message);
    }
}
