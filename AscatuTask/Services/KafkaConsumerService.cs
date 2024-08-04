using AscatuTask;
using AscatuTask.Data;
using AscatuTask.Services;
using CloudNative.CloudEvents;
using Confluent.Kafka;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

public class KafkaConsumerService : BackgroundService
{
    public string[] topics = new[] { "personevents-changed", "personevents-deleted" };
    public readonly ILoggerService _logger;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IOrderDbContext _orderDbContext;
    public bool cancelled { get; set; } = false;

    public KafkaConsumerService(ILoggerService logger, IKafkaProducer kafkaProducer, IOrderDbContext orderDbContext)
    {
        _logger = logger;
        _kafkaProducer = kafkaProducer;
        _orderDbContext = orderDbContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "172.21.0.4:9092",
            GroupId = "personevents-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
        {
            await _kafkaProducer.ProduceAsync("personevents-changed", new Order { Id = "OrderServiceConnectedToPersonevents-changedTopic" }, "changed");
            await _kafkaProducer.ProduceAsync("personevents-deleted", new Order { Id = "OrderServiceConnectedToPersonevents-deletedTopic" }, "deleted");

            consumer.Subscribe(topics);

            while (!cancelled && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(TimeSpan.FromSeconds(5));

                    if (consumeResult != null)
                    {
                        _logger.Write("Consumed message: " + consumeResult.Message.Value + " on topic " + consumeResult.Topic + " at partition " + consumeResult.Partition + " with offset " + consumeResult.Offset);

                        if (consumeResult.Topic == "personevents-changed")
                        {
                            _logger.Write("Person updated event received.");

                            var personId = JsonConvert.DeserializeObject<message>(consumeResult.Message.Value)?.Data.personId;
                            if (personId != null)
                            {
                                var orders = _orderDbContext.Orders.Where(o => o.PersonId == personId).ToList();

                                foreach (var order in orders)
                                {
                                    order.Product = order.Product+" - Updated With Kafka";
                                    _orderDbContext.Orders.Update(order);
                                    await _orderDbContext.SaveChangesAsync();
                                    await _kafkaProducer.ProduceAsync("orderevents-updated", order, "updated");
                                }
                            }
                            
                        }
                        else if (consumeResult.Topic == "personevents-deleted")
                        {
                            _logger.Write("Person deleted event received.");

                            var personId = JsonConvert.DeserializeObject<message>(consumeResult.Message.Value)?.Data.personId;
                            if (personId != null)
                            {
                                var orders = _orderDbContext.Orders.Where(o => o.PersonId == personId).ToList();

                                foreach (var order in orders)
                                {
                                    order.Product = order.Product + " - Deleted With Kafka";
                                    _orderDbContext.Orders.Update(order);
                                    await _orderDbContext.SaveChangesAsync();
                                    await _kafkaProducer.ProduceAsync("orderevents-deleted", order, "deleted");
                                }
                            }
                        }
                    }
                    else
                    {
                        
                    }
                }
                catch (Exception e)
                {
                    _logger.Write("Consume error: " + e.Message);
                }
            }

            consumer.Close();
        }
    }
    private class message
    {
        public string SpecVersion { get; set; }
        public string Id { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public string DataContentType { get; set; }
        public data Data { get; set; }
        public DateTimeOffset Time { get; set; }

    }
    private class data
    {
        public string orderId { get; set; }
        public string personId { get; set; }
    }

}
