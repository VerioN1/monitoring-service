using System.Text;
using Confluent.Kafka;
using Kafka.Public;
using Kafka.Public.Loggers;
using Microsoft.Extensions.Logging;

namespace monitoring_service.Services;

public class Subscriber : IHostedService
{
    private ClusterClient _cluster;
    private readonly ILogger<Subscriber> _logger;

    public Subscriber(ILogger<Subscriber> logger)
    {
        _logger = logger;
        _cluster = new ClusterClient(new Configuration
        {
            Seeds = "apps.varcode.com:29092"
        }, new ConsoleLogger());
    }

    public static void Init()
    {
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, collection) =>
            {
                collection.AddHostedService<Subscriber>();
            }).Build().Run();
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cluster.ConsumeFromLatest("example-topic");
        _cluster.MessageReceived += record =>
        {
            _logger.LogInformation($"Received : {Encoding.UTF8.GetString(record.Value as byte[])}");
        };
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cluster.Dispose();
        return Task.CompletedTask;
    }
}