/// <summary>
/// Server side representation of a Client
/// </summary>
public class Spectator
{
    public string UserId;
    public string UserDisplayName;
    public LiteNetLib.NetPeer peer;

    public Spectator(LiteNetLib.NetPeer peer)
    {
        this.peer = peer;
    }
}