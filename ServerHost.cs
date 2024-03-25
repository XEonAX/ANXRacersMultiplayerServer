using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ServerHost : IHostedService
{
    private readonly ILogger<ServerHost> logger;
    private Timer _timerReceive;
    private long _oldSendTimestamp;
    private Timer _timerSend;
    private MultiplayerServer _server;

    public ServerHost(ILogger<ServerHost> logger, MultiplayerServer multiplayerServer)
    {
        this.logger = logger;
        _server = multiplayerServer;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Timed Hosted Service running.");
        await Task.Delay(500);
        _server.StartServer();
        _timerReceive = new Timer(ReceiveWorker, null, TimeSpan.Zero,
            TimeSpan.FromMilliseconds(15));
        _oldSendTimestamp = Stopwatch.GetTimestamp();
        _timerSend = new Timer(SendWorker, null, TimeSpan.Zero,
            TimeSpan.FromMilliseconds(45));
    }

    private void ReceiveWorker(object? state)
    {
        _server.ReceiveUpdates();
    }

    private void SendWorker(object? state)
    {
        var newSendTimestamp = Stopwatch.GetTimestamp();
        var deltatimeInMillisecs = (uint)((newSendTimestamp - _oldSendTimestamp) / TimeSpan.TicksPerMillisecond);
        _oldSendTimestamp = newSendTimestamp;
        _server.SendUpdates(deltatimeInMillisecs);
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Timed Hosted Service is stopping.");
        _timerReceive?.Change(Timeout.Infinite, 0);
        _timerSend?.Change(Timeout.Infinite, 0);
        _server.StopServer();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timerReceive?.Dispose();
        _timerSend?.Dispose();
    }
}