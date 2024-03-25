using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// This hosted service sets up timers that very frequently poll for receiving updates,
/// less frequently polls to send aggregated updates.
/// </summary>
public class ServerHost : IHostedService
{
    private readonly ILogger<ServerHost> logger;
    private Timer _timerReceive;
    private long _oldSendTimestamp;
    private Timer _timerSend;
    private IMultiplayerServer _server;

    public ServerHost(ILogger<ServerHost> logger, IMultiplayerServer multiplayerServer)
    {
        this.logger = logger;
        _server = multiplayerServer;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Server Host Starting.");
        await Task.Delay(500);
        _server.StartServer();
        _timerReceive = new Timer(ReceiveWorker, null, TimeSpan.Zero,
            TimeSpan.FromMilliseconds(15));
        _oldSendTimestamp = Stopwatch.GetTimestamp();
        _timerSend = new Timer(SendWorker, null, TimeSpan.Zero,
            TimeSpan.FromMilliseconds(45));
        logger.LogInformation("Server Host Running.");
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
        logger.LogInformation("Server Host is stopping.");
        _timerReceive?.Change(Timeout.Infinite, 0);
        _timerSend?.Change(Timeout.Infinite, 0);
        _server.StopServer();
        logger.LogInformation("Server Host is stopped.");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timerReceive?.Dispose();
        _timerSend?.Dispose();
    }
}