using ANXRacersGalaxy.Serializables;


/// <summary>
/// Server side representation of a Player.
/// Player is Spectator but with a Skin
/// </summary>
public class Player : Spectator
{
    public uint MPId;
    public string ShipSkinId;
    public PShipUpdate state;

    public Player(uint mpId, LiteNetLib.NetPeer peer, PPlayerJoin packet) : base(peer)
    {
        MPId = mpId;
        base.UserDisplayName = packet.UserDisplayName;
        base.UserId = packet.UserId;
        this.ShipSkinId = packet.SkinId;
        state = new PShipUpdate
        {
            MPId = mpId
        };
    }
}