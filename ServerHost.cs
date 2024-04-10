using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// This hosted service sets up timers that very frequently poll for receiving updates,
/// less frequently polls to send aggregated updates.
/// </summary>
public class ServerHost : IHostedService
{
    private readonly ILogger<ServerHost> _logger;
    private Timer _timerReceive;
    private long _oldSendTimestamp;
    private Timer _timerSend;
    private IMultiplayerServer _server;
    private readonly IStateService _stateService;
    private readonly Configs _configs;

    public ServerHost(ILogger<ServerHost> logger, IMultiplayerServer multiplayerServer, IOptions<Configs> options, IStateService stateService)
    {
        _logger = logger;
        _server = multiplayerServer;
        _configs = options.Value;
        _stateService = stateService;
        _stateService.Load();
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Server Host Starting.");
        await Task.Delay(500, cancellationToken);
        if (_configs.AutoStart)
            _server.StartServer();
        _timerReceive = new Timer(ReceiveWorker, null, TimeSpan.Zero,
            TimeSpan.FromMilliseconds(15));
        _oldSendTimestamp = Stopwatch.GetTimestamp();
        _timerSend = new Timer(SendWorker, null, TimeSpan.Zero,
            TimeSpan.FromMilliseconds(45));
        _logger.LogInformation("Server Host Running.");
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
        _logger.LogInformation("Server Host is stopping.");
        _timerReceive?.Change(Timeout.Infinite, 0);
        _timerSend?.Change(Timeout.Infinite, 0);
        _server.StopServer();
        _logger.LogInformation("Server Host is stopped.");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timerReceive?.Dispose();
        _timerSend?.Dispose();
    }
}