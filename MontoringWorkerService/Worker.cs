using MimeKit;
using MimeKit.Text;
using Publisher.DTO;

namespace MontoringWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ServerDTO[] _servers;
    private readonly RecipientsDTO[] _recipients;
    
    private const string fromMailAddress = "support@varcode.com";
    private const string toMailAddress = "alonbarel221@gmail.com";
    private const string SMTP_SERVER = "3.121.250.121";
    private const string SMTP_USER = "varcodemonitor@varcodealerts.com";
    private const string SMTP_PASSWORD = "Password1!";

    private DateTime? lastEmailSent = null;
    
    public Worker(ILogger<Worker> logger)
    {
        _servers = new ServerDTO[]
        {
            new ServerDTO {Url = "https://apps.varcode.com:3443", Route = "/api", ServerName = "CAM-CODE API"}, //camcode - api
            new ServerDTO { Url = "https://apps.varcode.com", Route = "/" , ServerName = "CAM-CODE CLIENT"}, // camcode ui
            new ServerDTO { Url = "https://apps.varcode.com:4431", Route = "/", ServerName = "CAM-CODE MANAGER" }, //manager
            new ServerDTO {Url = "http://apps.varcode.com:4432", Route = "/", ServerName = "QR-CODE"}, //qr code
            new ServerDTO {Url = "https://apps.varcode.com:3002", Route = "/", ServerName = "STMS"}, //stms
        };

        _recipients = new RecipientsDTO[]
        {
            new RecipientsDTO {Address = "alonbarel221@gmail.com", Name = "Alon"},
            new RecipientsDTO {Address = "mharari@varcode.com", Name = "Michael Harari"}
        };
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation($"Started health checking servers - {DateTimeOffset.Now}");
            var results = await CheckServerHealthAsync();
            var errorMessages = new List<MessageDTO>();
            try
            {
                var errorMessagesObj = results.Where(result => result.Type == MessageTypes.Error);
                if (errorMessagesObj.ToList().Count == 0)
                {
                    if (lastEmailSent is not null)
                    {
                        lastEmailSent = null;
                        SendMailToWarn("SUCCESS - ALL SERVICES ARE FIXED!", "all systems are up and running on apps.varcode.com");
                    }
                    _logger.LogInformation("Done health check, ALL O.K");
                    lastEmailSent = null;
                }
                else
                {
                    if (lastEmailSent is null)
                    {
                        string message = errorMessagesObj
                            .Select(result => result.Message)
                            .Aggregate((i, j) => i + '\n' + j );
                        SendMailToWarn("WARNING - SERVICE CRASHED!", message);
                        _logger.LogError("Done health check, server didn't responded in time! check apps.varcode.com");
                        lastEmailSent = DateTime.Now;
                        continue;
                    }
                    
                    var now = DateTime.Now;
                    if (lastEmailSent.HasValue && lastEmailSent.Value.AddMinutes(19) > now)
                    {
                        lastEmailSent = null;
                    }
                    else
                    {
                        await Task.Delay(1000 * 60 * 5, stoppingToken);
                        continue;
                    }
                   
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e, "error acquired in Linq");
            }
            
            await Task.Delay(1000 * 60 * 5, stoppingToken);
        }
    }

    private void SendMailToWarn(string subject, string message)
    {
        try
        {
            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress("Varcode-MonitoringService", fromMailAddress));
           
            foreach (var recipient in _recipients)
            {
                mailMessage.To.Add(new MailboxAddress(recipient.Name, recipient.Address));
            }

            mailMessage.Subject = subject;
            mailMessage.Body = new TextPart(TextFormat.Plain)
            {
                Text = message
            };

            using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
            smtpClient.Connect(SMTP_SERVER, 2525, MailKit.Security.SecureSocketOptions.None);
            smtpClient.Authenticate(SMTP_USER, SMTP_PASSWORD);

            smtpClient.Send(mailMessage);
            smtpClient.Disconnect(true);
            _logger.LogInformation("DONE SENDING MAIL");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EMAIL FAILED to send");
        }
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
                if (response.IsSuccessStatusCode)
                {
                    serverStatus.Add(new MessageDTO {Message = $"name: {server.ServerName}, url: {server.Url}  is healthy!", Type = MessageTypes.Success});
                }
                else
                {
                   
                    serverStatus.Add(new MessageDTO {Message = $"|name: {server.ServerName}, url: {server.Url} | is NOT healthy! Check Immoderately", Type = MessageTypes.Error});
                }
            }
            catch (HttpRequestException e)
            {
                serverStatus.Add(new MessageDTO {Message = $"An error occurred while checking | name: {server.ServerName}, url:{server.Url} |: {e.Message}", Type = MessageTypes.Error});
            }
        }

        return serverStatus;
    }
}