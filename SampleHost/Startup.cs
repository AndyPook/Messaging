using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SampleHost
{
    public class Startup
    {
        private readonly IConfiguration config;

        public Startup(IConfiguration config)
        {
            this.config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureKafkaOptions(config, "Kafka");

            services.AddMessagePump<string, string, MyHandler>("Kafka");
        }

        public void Configure() { }
    }
}
