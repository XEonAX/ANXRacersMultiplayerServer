public class DtoDiscoveryResponse
{
    public int Version { get { return Constants.Version; } }
    public string ServerDisplayName { get; set; }
    public string HostName { get; set; }
    public int PortUDP { get; set; }
    public DtoExpandedLevelAndShip LevelAndShip { get; set; }
    public int PlayerCount { get; set; }

}