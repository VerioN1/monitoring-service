using System.Text;
using Confluent.Kafka;
using Kafka.Public;
using Kafka.Public.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Program{
    public static void Main(string[] args)
    {
        Subscriber.Init();
    }
}


public class Subscriber : IHostedService
{
    private ClusterClient _cluster;
    private readonly ILogger<Subscriber> _logger;

    public Subscriber(ILogger<Subscriber> logger)
    {
        var KAFKA_SERVER =  Environment.GetEnvironmentVariable("KAFKA_SERVER");
        Console.WriteLine(KAFKA_SERVER);
        _logger = logger;
        _cluster = new ClusterClient(new Configuration
        {
            Seeds = KAFKA_SERVER
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