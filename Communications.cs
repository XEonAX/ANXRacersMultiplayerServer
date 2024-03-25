using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class Communications : IHostedService
{
    private readonly ILogger<Communications> _logger;
    private readonly Configs _configs;

    public Communications(ILogger<Communications> logger, IOptions<Configs> options)
    {
        _logger = logger;
        _configs = options.Value;
    }

    internal void RegisterServer()
    {
        // TODO actual registration to central server
        _logger.LogInformation("New Server Registered!\r\nName:{ServerDisplayName}\r\nIsPublic:{IsPublic}", _configs.ServerDisplayName, _configs.IsPublic);
    }

    internal void SetActivePlayerCount(int count)
    {
        // TODO actual updating player count on central server
        _logger.LogInformation("Name:{ServerDisplayName}\r\nActivePlayers:{count}", _configs.ServerDisplayName, count);
    }

    internal void PlayerJoined(string userId, string userDisplayName)
    {
        _logger.LogInformation("Name:{ServerDisplayName}\r\nPlayerJoined:{userDisplayName}", _configs.ServerDisplayName, userDisplayName);
    }

    internal void ChatMessage(string userId, string userDisplayName, string message)
    {
        _logger.LogInformation("Name:{ServerDisplayName}\r\nChat from:{userDisplayName} message:{message}", _configs.ServerDisplayName, userDisplayName, message);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Communications running.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Communications stopped.");
        return Task.CompletedTask;
    }
}