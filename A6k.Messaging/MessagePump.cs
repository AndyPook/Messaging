using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using System.Text;

namespace A6k.Messaging
{
    public class MessagePump<TKey, TValue> : BackgroundService
    {
        private readonly IConsumer<TKey, TValue> consumer;
        private readonly IMessageHandler<TKey, TValue> handler;
        private readonly string handlerName;
        private readonly ILogger<MessagePump<TKey, TValue>> logger;

        public MessagePump(IConsumer<TKey, TValue> consumer, IMessageHandler<TKey, TValue> handler, ILogger<MessagePump<TKey, TValue>> logger)
        {
            this.consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            handlerName = handler.GetType().Name;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("MessagePump started for {Handler}", handlerName);

            // yielding here allows the BackgroundService.Start to capture this as a Task
            // some impl of Consumer never release the context, so we just get stuck in the while loop
            await Task.Yield();

            // main message loop
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume();
                    if (consumeResult == null)
                        continue;

                    var activity = new Activity("handle: " + GetCorrelationId(consumeResult.Message.Headers));
                    activity.Start();

                    try
                    {
                        await handler.HandleAsync(consumeResult.Message);
                        consumer.StoreOffset(consumeResult);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("MessagePump Handler error {Handler}", handlerName, Activity.Current, ex);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError("MessagePump Consumer error {Handler}", handlerName, Activity.Current, ex);
                }
            }

            logger.LogInformation("MessagePump stopped for {Handler}", handlerName);
        }

        private string GetCorrelationId(Headers headers)
        {
            if (headers.TryGetLastBytes("correlationid", out var c))
                return Encoding.ASCII.GetString(c);
            return string.Empty;
        }
    }
}
