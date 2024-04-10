using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;


[Route("[controller]")]
[ApiController]
public class ApiController : ControllerBase
{
    private readonly Configs _configs;
    private readonly IStateService _stateService;
    private readonly Communications _communications;

    public ApiController(IStateService stateService, IOptions<Configs> options, Communications communications)
    {
        _configs = options.Value;
        _stateService = stateService;
        _communications = communications;
    }

    [HttpGet("Ping")]
    public ActionResult Heartbeat()
    {
        return Ok();
    }

    [HttpGet("Status/{pin}")]
    public ActionResult<State> GetStatus(string pin)
    {
        if (pin == _configs.Pin)
            return _stateService.State;
        else
            return Forbid();
    }

    [HttpGet("Discovery")]
    public ActionResult<DtoDiscoveryResponse> GetDiscovery()
    {
        return new DtoDiscoveryResponse
        {
            HostName = _configs.PublicFacingUrl.Host,
            PortUDP = _configs.PortUDP,
            ServerDisplayName = _configs.ServerDisplayName,
            LevelAndShip = _stateService.State.LevelAndShip,
            PlayerCount = _stateService.State.PlayerCount
        };
    }

    [HttpGet("LevelAndShip")]
    public ActionResult<DtoExpandedLevelAndShip> GetLevelAndShip()
    {
        return _stateService.State.LevelAndShip;
    }

    [HttpPost("LevelAndShip")]
    public async Task<ActionResult<DtoExpandedLevelAndShip>> SetLevelAndShip([FromBody] DtoLevelAndShip levelAndShip)
    {
        var expandedLevelAndShip = await _communications.ExpandLevelAndShipAsync(levelAndShip);
        _stateService.State.LevelAndShip = expandedLevelAndShip;
        _stateService.Save();
        return _stateService.State.LevelAndShip;
    }
}