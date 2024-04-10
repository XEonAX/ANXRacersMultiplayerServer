public class State
{
    public DtoExpandedLevelAndShip LevelAndShip { get; set; }
    public Session Session { get; set; }
    public int PlayerCount { get; set; }
    public bool IsServerStarted { get; internal set; }
    public DtoGlobalDiscoveryResponse Discovery { get; internal set; }
}