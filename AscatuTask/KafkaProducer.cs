using Confluent.Kafka;
using CloudNative.CloudEvents;
using System.Net.Mime;
using Newtonsoft.Json;

namespace AscatuTask
{
    public interface IKafkaProducer
    {
        Task ProduceAsync(string topic, Order order, string eventType);
        Task InitializeAsync(string topic);
    }
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<Null, string> _producer;

        public KafkaProducer(IConfiguration configuration)
        {
            var config = new ProducerConfig { BootstrapServers = "172.21.0.4:9092", AllowAutoCreateTopics = true };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task InitializeAsync(string topic)
        {
            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = "Order Service Connected." });
        }

        public async Task ProduceAsync(string topic, Order order, string eventType)
        {
            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Source = new Uri("http://localhost:8081/v1/api/order"),
                Type = $"order.{eventType}",
                DataContentType = MediaTypeNames.Application.Json,
                Data = new { orderId=order.Id },
                Time = DateTimeOffset.UtcNow
            };

            var jsonEvent = new
            {
                SpecVersion = cloudEvent.SpecVersion.VersionId,
                cloudEvent.Id,
                cloudEvent.Source,
                cloudEvent.Type,
                cloudEvent.DataContentType,
                cloudEvent.Data,
                cloudEvent.Time
            };

            var cloudEventJson = JsonConvert.SerializeObject(jsonEvent, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            });
            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = cloudEventJson });
        }
    }
}
