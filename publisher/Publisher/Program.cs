// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Hosting;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using Publisher.DTO;

KafkaProducer.Init();

class KafkaProducer : IHostedService
{
    private readonly ILogger<KafkaProducer> _logger;
    private readonly IProducer<Null, string> _producer;
    private readonly ServerDTO[] _servers;

    private const string TOPIC = "example-topic";
    private const string BOOTSTRAP_SERVER = "apps.varcode.com:29092";
    
    public static void Init()
    {
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, collection) => { collection.AddHostedService<KafkaProducer>(); }).Build()
            .Run();
    }


    public KafkaProducer(ILogger<KafkaProducer> logger)
    {
        _servers = new ServerDTO[]
        {
            new ServerDTO {Url = "https://apps.varcode.com:3443", Route = "/api", ServerName = "CAM-CODE API"}, //camcode - api
            new ServerDTO { Url = "https://apps.varcode.com", Route = "/" , ServerName = "CAM-CODE CLIENT"}, // camcode ui
            new ServerDTO { Url = "https://apps.varcode.com:4431", Route = "/", ServerName = "CAM-CODE MANAGER" }, //manager
            new ServerDTO {Url = "http://apps.varcode.com:4432", Route = "/", ServerName = "QR-CODE"}, //qr code
            new ServerDTO {Url = "https://apps.varcode.com:3002", Route = "/", ServerName = "STMS"}, //stms
        };
        _logger = logger;
        var config = new ProducerConfig()
        {
            BootstrapServers = BOOTSTRAP_SERVER
        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var results = await CheckServerHealthAsync();
            foreach (var result in results)
            {
                Console.WriteLine(result.Message);

                if (result.Type != MessageTypes.Error) continue;

                const string fromMailAddress = "support@varcode.com";
                const string toMailAddress = "alonbarel221@gmail.com";
                
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress("Varcode-Alert",fromMailAddress));
                mailMessage.To.Add(new MailboxAddress("Alon", toMailAddress));
                mailMessage.Subject = "SendMail_MailKit_WithDomain";
                mailMessage.Body = new TextPart(TextFormat.Plain)
                {
                    Text = "Hello"
                };

                using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
                smtpClient.Connect("3.121.250.121:2525", 25, MailKit.Security.SecureSocketOptions.None);
                smtpClient.Send(mailMessage);
                smtpClient.Disconnect(true);
            }
            System.Threading.Thread.Sleep(10000 * 6);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _producer.Dispose();
        return Task.CompletedTask;
    }

    private async Task<List<MessageDTO>> CheckServerHealthAsync()
    {
        var serverStatus = new List<MessageDTO>();
        using var client = new HttpClient();
        foreach (var server in _servers)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(server.Url + server.Route);
                if (!response.IsSuccessStatusCode)
                {
                    serverStatus.Add(new MessageDTO {Message = $"name: {server.ServerName}, url:{server.Url}  is healthy!", Type = MessageTypes.Success});
                }
                else
                {
                   
                    serverStatus.Add(new MessageDTO {Message = $"|name: {server.ServerName}, url:{server.Url}| is NOT healthy! Check Immoderately", Type = MessageTypes.Error});
                }
            }
            catch (HttpRequestException e)
            {
               
                serverStatus.Add(new MessageDTO {Message = $"An error occurred while checking |name: {server.ServerName}, url:{server.Url} |: {e.Message}", Type = MessageTypes.Error});
            }
        }

        return serverStatus;
    }
}