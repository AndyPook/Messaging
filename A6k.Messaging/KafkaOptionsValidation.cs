using Microsoft.Extensions.Options;

namespace A6k.Messaging
{
    public class KafkaOptionsValidation : IValidateOptions<KafkaOptions>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"></param>
        public KafkaOptionsValidation(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The options name.
        /// </summary>
        public string Name { get; }

        public ValidateOptionsResult Validate(string name, KafkaOptions options)
        {
            if (options.Configuration.Count == 0)
                return ValidateOptionsResult.Fail("Kafka Configuration MUST be specified");
            if (string.IsNullOrWhiteSpace(options.Topic))
                return ValidateOptionsResult.Fail("Kafka Topic MUST be specified");

            return ValidateOptionsResult.Success;
        }
    }
}
