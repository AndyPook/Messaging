using System;
using A6k.Messaging;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureKafkaOptions(this IServiceCollection services, IConfiguration configuration, string name)
        {
            return services
                .Configure<KafkaOptions>(name, configuration.GetSection(name))
                .AddKafkaOptionsPostConfigure()
                .AddKafkaOptionsValidation(name);
        }

        /// <summary>
        /// Register for validation of KafkaOptions.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name">Name of config to validate</param>
        /// <returns></returns>
        public static IServiceCollection AddKafkaOptionsValidation(this IServiceCollection services, string name)
        {
            return services.AddSingleton<IValidateOptions<KafkaOptions>>(new KafkaOptionsValidation(name));
        }

        private static IServiceCollection AddKafkaOptionsPostConfigure(this IServiceCollection services)
        {
            services.TryAddSingleton((Func<IServiceProvider, IPostConfigureOptions<KafkaOptions>>)(sp =>
            {
                var appName = sp.GetRequiredService<IHostingEnvironment>().ApplicationName;

                return new PostConfigureOptions<KafkaOptions>(null, o =>
                {
                    // ensure "group.id" is set. Default to name of the current app
                    if (!o.Configuration.TryGetValue("group.id", out var groupid))
                        o.Configuration["group.id"] = appName;
                });
            }));
            return services;
        }

        /// <summary>
        /// Add a Message Pump (ie a Consumer)
        /// </summary>
        /// <typeparam name="TKey">Key type of the message</typeparam>
        /// <typeparam name="TValue">Value type of the message</typeparam>
        /// <param name="services"></param>
        /// <param name="configName">Name of the configuration/></param>
        /// <param name="featureConfig">Features to add to this Consumer</param>
        /// <returns></returns>
        public static IServiceCollection AddMessagePump<TKey, TValue, THandler>(this IServiceCollection services, string configName)
            where THandler : IMessageHandler<TKey, TValue>
        {
            return services.AddTransient<IHostedService>(sp =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<KafkaOptions>>().Get(configName);
                var consumerConfig = new ConsumerConfig(options.Configuration);
                var consumer = new ConsumerBuilder<TKey, TValue>(consumerConfig).Build();

                var handler = ActivatorUtilities.CreateInstance<THandler>(sp);
                var pump = ActivatorUtilities.CreateInstance<MessagePump<TKey, TValue>>(sp, consumer, handler);

                return pump;
            });
        }
    }
}
