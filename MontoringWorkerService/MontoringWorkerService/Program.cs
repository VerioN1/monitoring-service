using MontoringWorkerService;
using Serilog;
using Serilog.Core;
using Serilog.Events;

try
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.File(@"C:\temp\monitoring\logs-.txt", retainedFileCountLimit:10, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 1048576 * 10)
        .CreateLogger();

    Log.Information("Starting Up!");

    IHost host = Host.CreateDefaultBuilder(args)
        .UseWindowsService()
        .ConfigureServices(services => { services.AddHostedService<Worker>(); })
        .UseSerilog()
        .Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SERVICE CRASHED");
}
finally
{
    Log.CloseAndFlush();
}

