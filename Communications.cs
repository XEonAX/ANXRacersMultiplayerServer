using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class Communications : IHostedService
{
    private readonly ILogger<Communications> _logger;
    private readonly IStateService _stateService;
    private readonly HttpClient _httpClient;
    private readonly Configs _configs;


    public Communications(ILogger<Communications> logger, IOptions<Configs> options, IStateService stateService, HttpClient httpClient)
    {
        _logger = logger;
        _stateService = stateService;
        _httpClient = httpClient;
        _configs = options.Value;
    }

    internal async void RegisterServer()
    {
        // TODO actual registration to central server
        try
        {
            var status = await _httpClient.GetFromJsonAsync<DtoGlobalDiscoveryResponse>(_configs.DiscoveryUrl);
            if (status != default)
            {
                _stateService.State.Discovery = status;
                var registrationResponse = await _httpClient.PostAsJsonAsync(status.SingularityService + "servers", new DtoRegistrationRequest
                {
                    ServerDisplayName = _configs.ServerDisplayName,
                    PublicFacingUrl = _configs.PublicFacingUrl,
                    PortUDP = _configs.PortUDP,
                    IsPublic = _configs.IsPublic
                });
                _logger.LogInformation("New Server Registered!\r\nName:{ServerDisplayName}\r\nIsPublic:{IsPublic}", _configs.ServerDisplayName, _configs.IsPublic);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register server");
        }

    }

    internal void SetActivePlayerCount(int count)
    {
        _stateService.State.PlayerCount = count;
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

    internal void NewScoreAndRank(string userId, string userDisplayName, float time, uint rank)
    {
        _logger.LogInformation("Name:{ServerDisplayName}\r\nNew Score\r\nPlayer:{userDisplayName} Time:{time} Rank:{rank}", _configs.ServerDisplayName, userDisplayName, time, rank);
    }

    public async Task<DtoExpandedLevelAndShip> ExpandLevelAndShipAsync(DtoLevelAndShip levelAndShip)
    {
        if (_stateService.State.Discovery != default)
        {
            var levelTask = _httpClient.GetFromJsonAsync<DtoLevelResponse>(_stateService.State.Discovery.SingularityService + $"Level/{levelAndShip.LevelId}");
            var shipTask = _httpClient.GetFromJsonAsync<DtoShipResponse>(_stateService.State.Discovery.SingularityService + $"Ship/{levelAndShip.ShipId}");
            try
            {
                await Task.WhenAll(levelTask, shipTask);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch {levelId} and/or {shipId}", levelAndShip.LevelId, levelAndShip.ShipId);
                throw new Exception($"Failed to fetch Level:{levelAndShip.LevelId} and/or Ship:{levelAndShip.ShipId} due to {ex.Message}", ex);
            }
            if (levelTask.IsCompletedSuccessfully && shipTask.IsCompletedSuccessfully)
            {
                return new DtoExpandedLevelAndShip
                {
                    Level = levelTask.Result,
                    Ship = shipTask.Result
                };
            }
            else
            {
                throw levelTask.Exception ?? shipTask.Exception;
            }
        }
        else
        {
            return new DtoExpandedLevelAndShip
            {
                Level = new DtoLevelResponse
                {
                    LevelId = levelAndShip.LevelId
                },
                Ship = new DtoShipResponse
                {
                    ShipId = levelAndShip.ShipId
                }
            };
        }
    }
}